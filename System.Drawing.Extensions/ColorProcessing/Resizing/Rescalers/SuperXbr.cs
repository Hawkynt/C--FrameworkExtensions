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
/// Super-xBR (Super-Scale2x Refinement) scaler by Hyllian (2x only).
/// </summary>
/// <remarks>
/// <para>2-pass edge-directed scaling algorithm that improves upon XBR.</para>
/// <para>
/// Uses diagonal edge detection with weighted color differences and
/// anti-ringing filter to reduce artifacts.
/// </para>
/// <para>Algorithm by Hyllian, 2015.</para>
/// </remarks>
[ScalerInfo("Super-xBR", Author = "Hyllian", Year = 2015,
  Description = "Super-Scale2x Refinement edge-directed scaler", Category = ScalerCategory.PixelArt)]
public readonly struct SuperXbr : IPixelScaler {

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

    // Apply edge-directed interpolation for each output pixel
    var e00 = _InterpolateDiagonal(p22, p12, p21, p11, p13, p31, p23, p33, p02, p20, p24, p42);
    var e01 = _InterpolateDiagonal(p22, p13, p23, p12, p14, p32, p24, p34, p03, p21, p33, p43);
    var e10 = _InterpolateDiagonal(p22, p21, p32, p11, p31, p13, p33, p23, p20, p02, p42, p24);
    var e11 = _InterpolateDiagonal(p22, p23, p33, p12, p32, p14, p34, p24, p21, p03, p43, p31);

    // Apply anti-ringing using 3x3 neighborhood around center
    e00 = _ApplyAntiRinging(e00, p11, p12, p13, p21, p22, p23, p31, p32, p33);
    e01 = _ApplyAntiRinging(e01, p12, p13, p14, p22, p23, p24, p32, p33, p34);
    e10 = _ApplyAntiRinging(e10, p21, p22, p23, p31, p32, p33, p41, p42, p43);
    e11 = _ApplyAntiRinging(e11, p22, p23, p24, p32, p33, p34, p42, p43, p44);

    // Write 2x2 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(e11);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork _InterpolateDiagonal(
    in TWork center, in TWork n, in TWork e, in TWork nw, in TWork ne,
    in TWork w, in TWork se, in TWork sw, in TWork nn, in TWork ww,
    in TWork ee, in TWork ss) {

    // Calculate edge weights using color differences
    var d_edge = SuperXbr.ColorDiff(n, e)
               + SuperXbr.ColorDiff(w, se)
               + SuperXbr.ColorDiff(nw, sw)
               + SuperXbr.ColorDiff(center, nn)
               + SuperXbr.ColorDiff(center, ww)
               + SuperXbr.SmallNumber;

    var d_diag = SuperXbr.ColorDiff(nw, center)
               + SuperXbr.ColorDiff(center, se)
               + SuperXbr.ColorDiff(n, w)
               + SuperXbr.ColorDiff(e, sw)
               + SuperXbr.ColorDiff(ne, center)
               + SuperXbr.SmallNumber;

    var edgeStrength = d_diag / (d_edge + d_diag);

    // Use smoothstep for smoother blending
    edgeStrength = SuperXbr.Smoothstep(edgeStrength);

    // Blend between edge and diagonal interpolation
    var edgeColor = lerp.Lerp(n, e);
    var diagColor = lerp.Lerp(nw, se);

    return lerp.Lerp(edgeColor, diagColor, edgeStrength);
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
    var e00 = _InterpolateFast(p22, p12, p21, p11);
    var e01 = _InterpolateFast(p22, p12, p23, p13);
    var e10 = _InterpolateFast(p22, p21, p32, p31);
    var e11 = _InterpolateFast(p22, p23, p32, p33);

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
      return lerp.Lerp(a, b, blend);
    }

    // No clear edge, use center
    return center;
  }
}
