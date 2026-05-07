#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System;
using System.Collections.Generic;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Super-xBR 2× edge-directed scaler — port of Hyllian's 3-pass GLSL shader.
/// </summary>
/// <remarks>
/// <para>Reference: Hyllian's Super-xBR shader,
/// https://github.com/libretro/glsl-shaders/tree/master/xbr/shaders/super-xbr.
/// All three passes share a structurally identical kernel: the published
/// <c>d_wd</c> weighted-distance and <c>hv_wd</c> horizontal/vertical edgeness
/// functions over a 16-tap stencil drive a smoothstep blend between diagonal
/// (<c>c1, c2</c>) and h/v (<c>c3, c4</c>) 4-tap directional filters, followed
/// by a local <c>{E, F, H, I}</c> min/max anti-ringing clamp. Each pass uses its
/// own coefficient set (mode 0 defaults).</para>
/// <para><b>Pass 0 — diagonal output (e11)</b>: full port. Stencil samples a
/// 4×4 source-pixel block. Coefficients <c>wp = (2, 1, −1, 4, −1, 1)</c>,
/// <c>weight1 ≈ 0.1296</c>, <c>weight2 ≈ 0.0875</c>.</para>
/// <para><b>Pass 1 — h/v outputs (e01, e10)</b>: full port within the available
/// 5×5 source window. The 16-tap stencil rotated to the H/V output position has
/// 8 integer-source-pixel positions (read directly), 4 pass-0 e11 positions
/// (computed on-the-fly using the pass-0 kernel inside the window), and 4 pass-0
/// e11 positions whose own 4×4 source stencils reach one column/row outside the
/// 5×5 window — those are substituted by the bilinear midpoint of the cell's
/// four source corners (the limiting value of any unbiased pass-0 reconstruction
/// when its stencil is unavailable). Coefficients
/// <c>wp = (8, 0, 0, 0, 0, 0)</c>, <c>weight1 ≈ 0.1751</c>,
/// <c>weight2 ≈ 0.0648</c>.</para>
/// <para><b>Pass 2 — deringing</b>: in the reference shader, pass 2 runs the same
/// kernel a second time at every output position with stencil reads from
/// already-filled pass-1 outputs. A single-pass per-source-pixel kernel cannot
/// reach those output-grid pixels, so the dominant deringing effect — the
/// 3×3 source-pixel min/max clamp on the filtered result — is applied to e01
/// and e10 as well as e11 (pass 0 already clamps e11). This matches pass 2's
/// anti-ringing intent on each output channel without fabricating a second
/// filter pass against unavailable inputs.</para>
/// </remarks>
[ScalerInfo("Super-xBR", Author = "Hyllian", Year = 2015,
  Url = "https://github.com/libretro/glsl-shaders/tree/master/xbr/shaders/super-xbr",
  Description = "Super-xBR 2× edge-directed scaler (pass 0/1 ported from Hyllian's GLSL; pass 2 deringing applied uniformly)",
  Category = ScalerCategory.Rescaler)]
public readonly struct SuperXbr : IRescaler {

  private readonly bool _fast;

  internal const float SmallNumber = 0.0001f;

  /// <summary>
  /// Creates a new SuperXbr instance.
  /// </summary>
  /// <param name="fast">Use fast single-pass mode (simpler, faster but lower quality).</param>
  public SuperXbr(bool fast = false) => this._fast = fast;

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 2);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => this._fast
      ? callback.Invoke(new SuperXbrFastKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp))
      : callback.Invoke(new SuperXbrKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp));

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Creates a fast variant (single-pass).
  /// </summary>
  public SuperXbr AsFast() => new(true);

  /// <summary>
  /// Gets a standard 2x scale instance.
  /// </summary>
  public static SuperXbr Scale2x => new();

  /// <summary>
  /// Gets a fast 2x scale instance.
  /// </summary>
  public static SuperXbr Scale2xFast => new(true);

  /// <summary>
  /// Gets the default configuration (2x).
  /// </summary>
  public static SuperXbr Default => Scale2x;

  /// <summary>
  /// Calculates perceptual color difference using luminance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float ColorDiff<TWork>(in TWork c1, in TWork c2) where TWork : unmanaged, IColorSpace
    => MathF.Abs(ColorConverter.GetLuminance(c1) - ColorConverter.GetLuminance(c2));

  /// <summary>
  /// Smoothstep function for smooth blending transitions.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float Smoothstep(float x) {
    x = MathF.Max(0.0f, MathF.Min(1.0f, x));
    return x * x * (3.0f - 2.0f * x);
  }
}

file readonly struct SuperXbrKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Get 5x5 neighborhood for full Super-xBR
    var p00 = window.M2M2.Work;
    var p01 = window.M2M1.Work;
    var p02 = window.M2P0.Work;
    var p03 = window.M2P1.Work;
    var p04 = window.M2P2.Work;

    var p10 = window.M1M2.Work;
    var p11 = window.M1M1.Work;
    var p12 = window.M1P0.Work;
    var p13 = window.M1P1.Work;
    var p14 = window.M1P2.Work;

    var p20 = window.P0M2.Work;
    var p21 = window.P0M1.Work;
    var p22 = window.P0P0.Work;
    var p23 = window.P0P1.Work;
    var p24 = window.P0P2.Work;

    var p30 = window.P1M2.Work;
    var p31 = window.P1M1.Work;
    var p32 = window.P1P0.Work;
    var p33 = window.P1P1.Work;
    var p34 = window.P1P2.Work;

    var p40 = window.P2M2.Work;
    var p41 = window.P2M1.Work;
    var p42 = window.P2P0.Work;
    var p43 = window.P2P1.Work;
    var p44 = window.P2P2.Work;

    // Output positions per Hyllian's Super-xBR pipeline:
    //   e00 = source pixel (aligns 2× output grid with integer source positions).
    //   e11 = pass 0 diagonal output between source (i,j), (i+1,j), (i,j+1), (i+1,j+1).
    //   e01 = pass 1 H output (right of source pixel).
    //   e10 = pass 1 V output (below source pixel).
    var e00 = p22;

    // ----- Pass 0: e11 between (p22, p23, p32, p33). 4×4 source-pixel stencil. -----
    var e11 = _SuperXbrPass(
      p11, p12, p13, p14,        // P0, B,  C,  P1
      p21, p22, p23, p24,        // D,  E,  F,  F4
      p31, p32, p33, p34,        // G,  H,  I,  I4
      p41, p42, p43, p44,        // P2, H5, I5, P3
      Pass0Wp1, Pass0Wp2, Pass0Wp3, Pass0Wp4, Pass0Wp5, Pass0Wp6,
      Pass0Weight1, Pass0Weight2);

    // ----- Pass-0 outputs needed by pass 1's rotated stencil. -----
    // Each is the e11 of a different source-pixel cell whose 4×4 stencil fits in the 5×5 window.
    // Using shader naming: B=NW, D=SW, F=N, H=S of the e01 stencil.
    // (these double as F=W, H=E of the e10 stencil; B,D match e10's NW/SW unchanged.)
    var pass0_NW = _SuperXbrPass(   // e11 of source pixel (i-1, j-1)
      p00, p01, p02, p03, p10, p11, p12, p13,
      p20, p21, p22, p23, p30, p31, p32, p33,
      Pass0Wp1, Pass0Wp2, Pass0Wp3, Pass0Wp4, Pass0Wp5, Pass0Wp6,
      Pass0Weight1, Pass0Weight2);
    var pass0_SW = _SuperXbrPass(   // e11 of source pixel (i-1, j)
      p10, p11, p12, p13, p20, p21, p22, p23,
      p30, p31, p32, p33, p40, p41, p42, p43,
      Pass0Wp1, Pass0Wp2, Pass0Wp3, Pass0Wp4, Pass0Wp5, Pass0Wp6,
      Pass0Weight1, Pass0Weight2);
    var pass0_N = _SuperXbrPass(    // e11 of source pixel (i, j-1)
      p01, p02, p03, p04, p11, p12, p13, p14,
      p21, p22, p23, p24, p31, p32, p33, p34,
      Pass0Wp1, Pass0Wp2, Pass0Wp3, Pass0Wp4, Pass0Wp5, Pass0Wp6,
      Pass0Weight1, Pass0Weight2);
    var pass0_S = e11;              // e11 of source pixel (i, j) — the pass-0 result we just computed

    // 4-corner cell-average substitutes for the 4 pass-0 outputs whose own 4×4 stencils
    // reach one column/row past the 5×5 window. Each is the bilinear midpoint of the
    // four source pixels surrounding that pass-0 sample position — the unbiased
    // limiting value when no edge-direction information is available.
    // Naming: avg_<row><col> = pass-0 e11 of source pixel at that direction from (i, j).
    var avg_N2 = _Avg4(p02, p03, p12, p13);  // pass-0 e11 of (i, j-2) — used as e01's P1
    var avg_S1 = _Avg4(p32, p33, p42, p43);  // pass-0 e11 of (i, j+1) — used as e01's P2 / e10's I5
    var avg_E1high = _Avg4(p13, p14, p23, p24); // pass-0 e11 of (i+1, j-1) — used as e01's I4
    var avg_E1low  = _Avg4(p23, p24, p33, p34); // pass-0 e11 of (i+1, j)   — used as e01's I5 / e10's P2
    var avg_W2 = _Avg4(p20, p21, p30, p31);  // pass-0 e11 of (i-2, j)   — used as e10's P1
    var avg_W1S = _Avg4(p31, p32, p41, p42); // pass-0 e11 of (i-1, j+1) — used as e10's I4

    // ----- Pass 1: e01 (H position, right of source pixel (i, j)) -----
    // Stencil mapping (shader notation → our 5×5 window):
    //   E = p22 (i, j)            I = p23 (i+1, j)
    //   F = pass0_N  (e11 N)      H = pass0_S (e11 S, = main e11 above)
    //   B = pass0_NW              D = pass0_SW
    //   C = p12 (i, j-1)          G = p32 (i, j+1)
    //   F4 = p13 (i+1, j-1)       H5 = p33 (i+1, j+1)
    //   I4 = avg(p13,p14,p23,p24) I5 = avg(p23,p24,p33,p34)
    //   P0 = p21 (i-1, j)         P3 = p24 (i+2, j)
    //   P1 = avg(p02,p03,p12,p13) P2 = avg(p32,p33,p42,p43)
    var e01 = _SuperXbrPass(
      p21,        pass0_NW,   p12,        avg_N2,        // P0, B,  C,  P1
      pass0_SW,   p22,        pass0_N,    p13,           // D,  E,  F,  F4
      p32,        pass0_S,    p23,        avg_E1high,    // G,  H,  I,  I4
      avg_S1,     p33,        avg_E1low,  p24,           // P2, H5, I5, P3
      Pass1Wp1, Pass1Wp2, Pass1Wp3, Pass1Wp4, Pass1Wp5, Pass1Wp6,
      Pass1Weight1, Pass1Weight2);

    // ----- Pass 1: e10 (V position, below source pixel (i, j)) -----
    // Stencil rotated 90° from e01. fp.x<0.5 case in the shader: g1 in y, g2 in x, so:
    //   E = p22 (i, j)            I = p32 (i, j+1)
    //   F = pass0_W (e11 W = NW-S diagonal of column i-1) — same as pass0_SW for e01 stencil
    //                              H = pass0_E (e11 E) — same as pass0_S
    //   B = pass0_NW              D = pass0_NE-of-column-i = pass0_N of e01 stencil
    //   C = p21 (i-1, j)          G = p23 (i+1, j)
    //   F4 = p31 (i-1, j+1)       H5 = p33 (i+1, j+1)
    //   I4 = avg(p31,p32,p41,p42) I5 = avg(p32,p33,p42,p43)
    //   P0 = p12 (i, j-1)         P3 = p42 (i, j+2)
    //   P1 = avg(p20,p21,p30,p31) P2 = avg(p23,p24,p33,p34)
    var e10 = _SuperXbrPass(
      p12,        pass0_NW,   p21,        avg_W2,        // P0, B,  C,  P1
      pass0_N,    p22,        pass0_SW,   p31,           // D,  E,  F,  F4
      p23,        pass0_S,    p32,        avg_W1S,       // G,  H,  I,  I4
      avg_E1low,  p33,        avg_S1,     p42,           // P2, H5, I5, P3
      Pass1Wp1, Pass1Wp2, Pass1Wp3, Pass1Wp4, Pass1Wp5, Pass1Wp6,
      Pass1Weight1, Pass1Weight2);

    // ----- Pass 2: anti-ringing on all three filtered outputs. -----
    // The reference pass 2 runs the full kernel again at every output position reading
    // pass-1 outputs; we cannot reach those in a single-pass kernel. The dominant
    // deringing component is the local source-pixel min/max clamp — apply it to the
    // pass-0 e11 (continues existing behaviour) and to the pass-1 e01/e10.
    e11 = _ApplyAntiRinging(e11, p11, p12, p13, p21, p22, p23, p31, p32, p33);
    e01 = _ApplyAntiRinging(e01, p11, p12, p13, p21, p22, p23, p31, p32, p33);
    e10 = _ApplyAntiRinging(e10, p11, p12, p13, p21, p22, p23, p31, p32, p33);

    // Write 2x2 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(e11);
  }

  // ===========================================================================
  // Hyllian Super-xBR pass kernel. Faithful port of the libretro/glsl-shaders
  // super-xbr-pass{0,1,2}.glsl shaders (default MODE=0). All three passes run
  // the same algorithm with different coefficient sets.
  // Reference: https://github.com/libretro/glsl-shaders/tree/master/xbr/shaders/super-xbr
  // ===========================================================================

  // Pass-0 mode 0: edge detector and 4-tap directional filter weights for the
  // diagonal output. Drives the first pass that interpolates the e11 output
  // between four source pixels.
  private const float Pass0Wp1 = 2f, Pass0Wp2 = 1f, Pass0Wp3 = -1f, Pass0Wp4 = 4f, Pass0Wp5 = -1f, Pass0Wp6 = 1f;
  private const float Pass0Weight1 = 1.29633f / 10f;
  private const float Pass0Weight2 = 1.75068f / 10f / 2f;

  // Pass-1 mode 0: only wp1 nonzero — d_wd reduces to 8·(inner cross sum) and
  // hv_wd to 8·(inner-vs-outer sum). Filter weights swap weight1/weight2 magnitudes
  // relative to pass 0 (the H/V output sits along the cardinal axes).
  private const float Pass1Wp1 = 8f, Pass1Wp2 = 0f, Pass1Wp3 = 0f, Pass1Wp4 = 0f, Pass1Wp5 = 0f, Pass1Wp6 = 0f;
  private const float Pass1Weight1 = 1.75068f / 10f;
  private const float Pass1Weight2 = 1.29633f / 10f / 2f;

  // XBR_EDGE_STR (smoothstep limit on |d_edge|).
  private const float XbrEdgeStr = 0.6f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum(in TWork c) => ColorConverter.GetLuminance(in c);

  /// <summary>
  /// 4-pixel arithmetic mean (bilinear midpoint of four corner samples).
  /// Used as the unbiased substitute for a pass-0 reconstruction whose own 4×4
  /// stencil reaches outside the available 5×5 window.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork _Avg4(in TWork a, in TWork b, in TWork c, in TWork d) {
    var (ar, ag, ab) = ColorConverter.GetNormalizedRgb(a);
    var (br, bg, bb) = ColorConverter.GetNormalizedRgb(b);
    var (cr, cg, cb) = ColorConverter.GetNormalizedRgb(c);
    var (dr, dg, db) = ColorConverter.GetNormalizedRgb(d);
    var aA = ColorConverter.GetAlpha(a);
    return ColorConverter.FromNormalizedRgba<TWork>(
      0.25f * (ar + br + cr + dr),
      0.25f * (ag + bg + cg + dg),
      0.25f * (ab + bb + cb + db),
      aA);
  }

  /// <summary>
  /// Hyllian's d_wd weighted-distance function. Computes the "diagonal edgeness" via
  /// a weighted sum of luminance differences over the 16-tap stencil. Coefficients
  /// wp1..wp6 are pass-specific; the formula is identical across all three passes.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Dwd(
      float wp1, float wp2, float wp3, float wp4, float wp5, float wp6,
      float b0, float b1,
      float c0, float c1, float c2,
      float d0, float d1, float d2, float d3,
      float e1, float e2, float e3,
      float f2, float f3) {
    static float Df(float a, float b) => MathF.Abs(a - b);
    return wp1 * (Df(c1, c2) + Df(c1, c0) + Df(e2, e1) + Df(e2, e3))
         + wp2 * (Df(d2, d3) + Df(d0, d1))
         + wp3 * (Df(d1, d3) + Df(d0, d2))
         + wp4 * Df(d1, d2)
         + wp5 * (Df(c0, c2) + Df(e1, e3))
         + wp6 * (Df(b0, b1) + Df(f2, f3));
  }

  /// <summary>
  /// Hyllian's hv_wd: horizontal/vertical "edgeness" from a 2×4 stencil (4 inner taps
  /// i1..i4, 4 outer taps e1..e4). Coefficients wp1..wp6 are pass-specific.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Hvwd(
      float wp1, float wp3, float wp4,
      float i1, float i2, float i3, float i4,
      float e1, float e2, float e3, float e4) {
    static float Df(float a, float b) => MathF.Abs(a - b);
    return wp4 * (Df(i1, i2) + Df(i3, i4))
         + wp1 * (Df(i1, e1) + Df(i2, e2) + Df(i3, e3) + Df(i4, e4))
         + wp3 * (Df(i1, e2) + Df(i3, e4) + Df(e1, i2) + Df(e3, i4));
  }

  /// <summary>
  /// 4-tap directional filter c = w·(a, b, c, d) using the published filter weights
  /// w1 = (-w, w+0.5, w+0.5, -w). Negative side-lobes give the characteristic xBR-style
  /// sharpening; sums to 1 by construction (-w + w+0.5 + w+0.5 - w = 1).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork _Filter4Tap(in TWork a, in TWork b, in TWork c, in TWork d, float w) {
    // Use signed accumulation: weights are negative for the outer taps, so the
    // ILerp-based blending used elsewhere doesn't work directly. Decompose to RGB.
    var (ar, ag, ab) = ColorConverter.GetNormalizedRgb(a);
    var (br, bg, bb) = ColorConverter.GetNormalizedRgb(b);
    var (cr, cg, cb) = ColorConverter.GetNormalizedRgb(c);
    var (dr, dg, db) = ColorConverter.GetNormalizedRgb(d);
    var aA = ColorConverter.GetAlpha(a);
    var w_outer = -w;
    var w_inner = w + 0.5f;
    var or = ar * w_outer + br * w_inner + cr * w_inner + dr * w_outer;
    var og = ag * w_outer + bg * w_inner + cg * w_inner + dg * w_outer;
    var ob = ab * w_outer + bb * w_inner + cb * w_inner + db * w_outer;
    or = MathF.Max(0f, MathF.Min(1f, or));
    og = MathF.Max(0f, MathF.Min(1f, og));
    ob = MathF.Max(0f, MathF.Min(1f, ob));
    return ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, aA);
  }

  /// <summary>
  /// Generic Super-xBR pass kernel. Same algorithm for all three reference passes,
  /// parameterised by the wp1..wp6 weighted-distance coefficients and the weight1/weight2
  /// directional-filter strengths. Stencil layout (output sits between H and F):
  /// <code>
  ///   |P0|B |C |P1|
  ///   |D |E |F |F4|
  ///   |G |H |I |I4|
  ///   |P2|H5|I5|P3|
  /// </code>
  /// Smoothly blends between two diagonal-direction filters (c1: P2-H-F-P1, c2: P0-E-I-P3)
  /// and two h/v-direction filters (c3, c4) based on the smoothstepped |d_edge| value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork _SuperXbrPass(
      in TWork P0, in TWork B,  in TWork C,  in TWork P1,
      in TWork D,  in TWork E,  in TWork F,  in TWork F4,
      in TWork G,  in TWork H,  in TWork I,  in TWork I4,
      in TWork P2, in TWork H5, in TWork I5, in TWork P3,
      float wp1, float wp2, float wp3, float wp4, float wp5, float wp6,
      float weight1, float weight2) {

    var p0 = _Lum(P0); var b = _Lum(B); var c = _Lum(C); var p1 = _Lum(P1);
    var d = _Lum(D); var e = _Lum(E); var f = _Lum(F); var f4 = _Lum(F4);
    var g = _Lum(G); var h = _Lum(H); var i = _Lum(I); var i4 = _Lum(I4);
    var p2 = _Lum(P2); var h5 = _Lum(H5); var i5 = _Lum(I5); var p3 = _Lum(P3);

    // d_edge: signed; positive favours one diagonal, negative the other.
    var dEdge = _Dwd(wp1, wp2, wp3, wp4, wp5, wp6, d, b, g, e, c, p2, h, f, p1, h5, i, f4, i5, i4)
              - _Dwd(wp1, wp2, wp3, wp4, wp5, wp6, c, f4, b, f, i4, p0, e, i, p3, d, h, i5, g, h5);
    var hvEdge = _Hvwd(wp1, wp3, wp4, f, i, e, h, c, i5, b, h5)
               - _Hvwd(wp1, wp3, wp4, e, f, h, i, d, f4, g, i4);

    var edgeStrength = MathF.Min(1f, MathF.Abs(dEdge) / (XbrEdgeStr + 1e-6f));
    edgeStrength = SuperXbr.Smoothstep(edgeStrength);

    // Diagonal candidates: c1 along P2-H-F-P1; c2 along P0-E-I-P3.
    var c1 = _Filter4Tap(P2, H, F, P1, weight1);
    var c2 = _Filter4Tap(P0, E, I, P3, weight1);
    // H/V candidates: c3 vertical pairs averaged, c4 horizontal pairs.
    var c3 = _Filter4TapPair(D, G, E, H, F, I, F4, I4, weight2);
    var c4 = _Filter4TapPair(C, B, F, E, I, H, I5, H5, weight2);

    // Pick diag side by sign(d_edge), hv side by sign(hv_edge), blend by edge_strength.
    var diagPick = dEdge >= 0 ? c2 : c1;
    var hvPick = hvEdge >= 0 ? c4 : c3;

    // Final mix: (1-edge_strength)·hvPick + edge_strength·diagPick.
    var w2i = (int)(edgeStrength * 256f);
    return lerp.Lerp(hvPick, diagPick, 256 - w2i, w2i);
  }

  /// <summary>
  /// Filter c3/c4 form: 4-tap on PAIRS of pixels (a+b, c+d, e+f, g+h), divided by 3
  /// since each tap is a sum of 2 pixels. Implements `mul(w2, mat4x3(a+b, c+d, e+f, g+h))/3`
  /// from the shader.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork _Filter4TapPair(
      in TWork a, in TWork b, in TWork c, in TWork d,
      in TWork e, in TWork f, in TWork g, in TWork h,
      float w) {
    var (ar, ag, ab) = ColorConverter.GetNormalizedRgb(a);
    var (br, bg, bb) = ColorConverter.GetNormalizedRgb(b);
    var (cr, cg, cb) = ColorConverter.GetNormalizedRgb(c);
    var (dr, dg, db) = ColorConverter.GetNormalizedRgb(d);
    var (er, eg, eb) = ColorConverter.GetNormalizedRgb(e);
    var (fr, fg, fb) = ColorConverter.GetNormalizedRgb(f);
    var (gr, gg, gb) = ColorConverter.GetNormalizedRgb(g);
    var (hr, hg, hb) = ColorConverter.GetNormalizedRgb(h);
    var aA = ColorConverter.GetAlpha(a);
    var w_outer = -w;
    var w_inner = w + 0.25f;
    var or = (ar + br) * w_outer + (cr + dr) * w_inner + (er + fr) * w_inner + (gr + hr) * w_outer;
    var og = (ag + bg) * w_outer + (cg + dg) * w_inner + (eg + fg) * w_inner + (gg + hg) * w_outer;
    var ob = (ab + bb) * w_outer + (cb + db) * w_inner + (eb + fb) * w_inner + (gb + hb) * w_outer;
    or *= 1f / 3f; og *= 1f / 3f; ob *= 1f / 3f;
    or = MathF.Max(0f, MathF.Min(1f, or));
    og = MathF.Max(0f, MathF.Min(1f, og));
    ob = MathF.Max(0f, MathF.Min(1f, ob));
    return ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, aA);
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork _ApplyAntiRinging(
    in TWork result,
    in TWork c00, in TWork c01, in TWork c02,
    in TWork c10, in TWork c11, in TWork c12,
    in TWork c20, in TWork c21, in TWork c22) {

    // Get RGB components using safe ColorConverter access
    var (rR, gR, bR) = ColorConverter.GetNormalizedRgb(result);
    var aR = ColorConverter.GetAlpha(result);

    var (r00, g00, b00) = ColorConverter.GetNormalizedRgb(c00);
    var (r01, g01, b01) = ColorConverter.GetNormalizedRgb(c01);
    var (r02, g02, b02) = ColorConverter.GetNormalizedRgb(c02);
    var (r10, g10, b10) = ColorConverter.GetNormalizedRgb(c10);
    var (r11, g11, b11) = ColorConverter.GetNormalizedRgb(c11);
    var (r12, g12, b12) = ColorConverter.GetNormalizedRgb(c12);
    var (r20, g20, b20) = ColorConverter.GetNormalizedRgb(c20);
    var (r21, g21, b21) = ColorConverter.GetNormalizedRgb(c21);
    var (r22, g22, b22) = ColorConverter.GetNormalizedRgb(c22);

    // Clamp each component to min/max of neighborhood
    var minR = MathF.Min(r00, MathF.Min(r01, MathF.Min(r02,
               MathF.Min(r10, MathF.Min(r11, MathF.Min(r12,
               MathF.Min(r20, MathF.Min(r21, r22))))))));
    var maxR = MathF.Max(r00, MathF.Max(r01, MathF.Max(r02,
               MathF.Max(r10, MathF.Max(r11, MathF.Max(r12,
               MathF.Max(r20, MathF.Max(r21, r22))))))));

    var minG = MathF.Min(g00, MathF.Min(g01, MathF.Min(g02,
               MathF.Min(g10, MathF.Min(g11, MathF.Min(g12,
               MathF.Min(g20, MathF.Min(g21, g22))))))));
    var maxG = MathF.Max(g00, MathF.Max(g01, MathF.Max(g02,
               MathF.Max(g10, MathF.Max(g11, MathF.Max(g12,
               MathF.Max(g20, MathF.Max(g21, g22))))))));

    var minB = MathF.Min(b00, MathF.Min(b01, MathF.Min(b02,
               MathF.Min(b10, MathF.Min(b11, MathF.Min(b12,
               MathF.Min(b20, MathF.Min(b21, b22))))))));
    var maxB = MathF.Max(b00, MathF.Max(b01, MathF.Max(b02,
               MathF.Max(b10, MathF.Max(b11, MathF.Max(b12,
               MathF.Max(b20, MathF.Max(b21, b22))))))));

    var clampedR = MathF.Max(minR, MathF.Min(maxR, rR));
    var clampedG = MathF.Max(minG, MathF.Min(maxG, gR));
    var clampedB = MathF.Max(minB, MathF.Min(maxB, bR));

    return ColorConverter.FromNormalizedRgba<TWork>(clampedR, clampedG, clampedB, aR);
  }
}

file readonly struct SuperXbrFastKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Get 3x3 neighborhood for fast Super-xBR
    var p11 = window.M1M1.Work;
    var p12 = window.M1P0.Work;
    var p13 = window.M1P1.Work;

    var p21 = window.P0M1.Work;
    var p22 = window.P0P0.Work;
    var p23 = window.P0P1.Work;

    var p31 = window.P1M1.Work;
    var p32 = window.P1P0.Work;
    var p33 = window.P1P1.Work;

    // Simple edge-directed interpolation without second pass
    var e00 = this._InterpolateFast(p22, p12, p21, p11);
    var e01 = this._InterpolateFast(p22, p12, p23, p13);
    var e10 = this._InterpolateFast(p22, p21, p32, p31);
    var e11 = this._InterpolateFast(p22, p23, p32, p33);

    // Write 2x2 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(e11);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork _InterpolateFast(in TWork center, in TWork a, in TWork b, in TWork diag) {
    // Simple edge detection
    var diffA = SuperXbr.ColorDiff(center, a);
    var diffB = SuperXbr.ColorDiff(center, b);
    var diffDiag = SuperXbr.ColorDiff(a, b);

    if (diffDiag < diffA + diffB - SuperXbr.SmallNumber) {
      // Diagonal edge detected
      var blend = diffA / (diffA + diffB + SuperXbr.SmallNumber);
      var w2 = (int)(blend * 256f);
      return lerp.Lerp(a, b, 256 - w2, w2);
    }

    // No clear edge, use center
    return center;
  }
}
