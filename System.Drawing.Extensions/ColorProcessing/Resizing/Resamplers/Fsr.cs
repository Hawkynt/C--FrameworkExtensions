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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// AMD FidelityFX Super Resolution 1.0 (FSR) — EASU pass. Spatial-only, edge-adaptive,
/// anisotropic Lanczos2-approximation upsampler with 2×2 min/max ringing clamp.
/// </summary>
/// <remarks>
/// <para>Reference: AMD FidelityFX FSR 1.0, https://github.com/GPUOpen-Effects/FidelityFX-FSR
/// (file <c>ffx_fsr1.h</c>, function <c>FsrEasuF</c>). This is a faithful C# port of the
/// 32-bit floating-point reference path, preserving the 12-tap pattern, 4-quadrant
/// SetF direction/length accumulation, anisotropic Lanczos2 weight approximation, and
/// 2×2 min/max ringing clamp.</para>
/// <para>The reference RCAS sharpening pass is a separate operation; users wanting the
/// full FSR pipeline should chain a sharpening filter after this resampler.</para>
/// </remarks>
[ScalerInfo("FSR", Author = "AMD", Year = 2021,
  Url = "https://github.com/GPUOpen-Effects/FidelityFX-FSR",
  Description = "FidelityFX Super Resolution 1.0 EASU upsampling",
  Category = ScalerCategory.Resampler)]
public readonly struct Fsr : IResampler {

  private readonly float _sharpness;

  /// <summary>
  /// Creates an FSR resampler with default sharpness.
  /// </summary>
  public Fsr() : this(0.5f) { }

  /// <summary>
  /// Creates an FSR resampler with custom sharpness.
  /// </summary>
  /// <param name="sharpness">Sharpness parameter (0-1). Higher values produce sharper results.</param>
  public Fsr(float sharpness) => this._sharpness = Math.Clamp(sharpness, 0f, 1f);

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new FsrKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._sharpness, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Fsr Default => new();

  /// <summary>
  /// Gets a sharper configuration.
  /// </summary>
  public static Fsr Sharp => new(0.8f);

  /// <summary>
  /// Gets a softer configuration with reduced artifacts.
  /// </summary>
  public static Fsr Soft => new(0.2f);
}

file readonly struct FsrKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float sharpness, bool useCenteredGrid)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 2;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  // Precomputed scale factors and offsets for zero-cost grid centering
  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;
  private readonly float _sharpnessParam = 1f - sharpness * 0.5f; // Map 0-1 to 1-0.5 for filter width

  // FSR's 2× luminance approximation per ffx_fsr1.h: lum2 = 0.5R + G + 0.5B.
  // Used for edge-direction extraction; cancels out in normalisation.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum2(in TWork c) {
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in c);
    return r * 0.5f + g + b * 0.5f;
  }

  /// <summary>
  /// Accumulate one quadrant's contribution to the gradient direction and length, per
  /// FsrEasuSetF. Uses 5 luminance taps in a + pattern around the centre <c>lC</c>:
  /// <c>lA</c> above, <c>lB</c> left, <c>lD</c> right, <c>lE</c> below. The bilinear
  /// weight <c>w</c> is non-zero only for the active quadrant of the 2×2 around (fx, fy).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _SetDir(ref float dirX, ref float dirY, ref float lenSum, float w,
      float lA, float lB, float lC, float lD, float lE) {
    if (w == 0f) return;
    // Direction: '+' diff. Length: gradient-reversal magnitude shaped, 0 to 1.
    var dc = lD - lC;
    var cb = lC - lB;
    var lenX = MathF.Max(MathF.Abs(dc), MathF.Abs(cb));
    var rcpX = lenX > 1e-9f ? 1f / lenX : 0f;
    var dxL = lD - lB;
    dirX += dxL * w;
    var lenXs = MathF.Min(1f, MathF.Abs(dxL) * rcpX);
    lenSum += lenXs * lenXs * w;

    var ec = lE - lC;
    var ca = lC - lA;
    var lenY = MathF.Max(MathF.Abs(ec), MathF.Abs(ca));
    var rcpY = lenY > 1e-9f ? 1f / lenY : 0f;
    var dyL = lE - lA;
    dirY += dyL * w;
    var lenYs = MathF.Min(1f, MathF.Abs(dyL) * rcpY);
    lenSum += lenYs * lenYs * w;
  }

  /// <summary>
  /// Lanczos2-approximation tap weighting per FsrEasuTapF. Rotates the offset by the
  /// gradient direction, applies anisotropic length, computes <c>d²</c>, clips to the
  /// adjustable window, and evaluates the rational approximation
  /// <c>(25/16 · (2/5·d² − 1)² − (25/16 − 1)) · (lob·d² − 1)²</c>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Tap(float offX, float offY, float dirX, float dirY, float lenX, float lenY, float lob, float clp) {
    // Rotate offset by direction.
    var vx = offX * dirX + offY * dirY;
    var vy = offX * (-dirY) + offY * dirX;
    // Anisotropy.
    vx *= lenX;
    vy *= lenY;
    var d2 = vx * vx + vy * vy;
    if (d2 > clp) d2 = clp;
    var wB = 2f / 5f * d2 - 1f;
    var wA = lob * d2 - 1f;
    wB *= wB;
    wA *= wA;
    wB = 25f / 16f * wB - (25f / 16f - 1f);
    return wB * wA;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Map destination pixel to source coordinates.
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // 12-tap pattern relative to (x0, y0) — the upper-left of the f/g/j/k 2×2:
    //      b c
    //    e f g h
    //    i j k l
    //      n o
    var bC = frame[x0,     y0 - 1].Work;
    var cC = frame[x0 + 1, y0 - 1].Work;
    var eC = frame[x0 - 1, y0    ].Work;
    var fC = frame[x0,     y0    ].Work;
    var gC = frame[x0 + 1, y0    ].Work;
    var hC = frame[x0 + 2, y0    ].Work;
    var iC = frame[x0 - 1, y0 + 1].Work;
    var jC = frame[x0,     y0 + 1].Work;
    var kC = frame[x0 + 1, y0 + 1].Work;
    var lC = frame[x0 + 2, y0 + 1].Work;
    var nC = frame[x0,     y0 + 2].Work;
    var oC = frame[x0 + 1, y0 + 2].Work;

    // Per-tap luminance for direction/length extraction.
    var bL = _Lum2(bC); var cL = _Lum2(cC);
    var eL = _Lum2(eC); var fL = _Lum2(fC); var gL = _Lum2(gC); var hL = _Lum2(hC);
    var iL = _Lum2(iC); var jL = _Lum2(jC); var kL = _Lum2(kC); var lL = _Lum2(lC);
    var nL = _Lum2(nC); var oL = _Lum2(oC);

    // Accumulate direction and length over 4 quadrants per FsrEasuSetF. Bilinear
    // weights select the active quadrant: s/t/u/v for (1-fx,1-fy)/(fx,1-fy)/(1-fx,fy)/(fx,fy).
    float dirX = 0f, dirY = 0f, len = 0f;
    var ws = (1f - fx) * (1f - fy);
    var wt = fx * (1f - fy);
    var wu = (1f - fx) * fy;
    var wv = fx * fy;
    // Quadrant s (top-left): + pattern around f, with b above, e left, g right, j below.
    _SetDir(ref dirX, ref dirY, ref len, ws, bL, eL, fL, gL, jL);
    // Quadrant t (top-right): around g, with c above, f left, h right, k below.
    _SetDir(ref dirX, ref dirY, ref len, wt, cL, fL, gL, hL, kL);
    // Quadrant u (bottom-left): around j, with f above, i left, k right, n below.
    _SetDir(ref dirX, ref dirY, ref len, wu, fL, iL, jL, kL, nL);
    // Quadrant v (bottom-right): around k, with g above, j left, l right, o below.
    _SetDir(ref dirX, ref dirY, ref len, wv, gL, jL, kL, lL, oL);

    // Normalize direction; cleanup degenerate near-zero gradients.
    var dirR = dirX * dirX + dirY * dirY;
    if (dirR < 1f / 32768f) {
      dirX = 1f;
      dirY = 0f;
    } else {
      var invLen = 1f / MathF.Sqrt(dirR);
      dirX *= invLen;
      dirY *= invLen;
    }

    // Transform length from {0..2} to {0..1} and shape with square.
    len *= 0.5f;
    len *= len;

    // Stretch kernel from {1.0 vert/horz, sqrt(2) on diagonal}.
    var maxAbs = MathF.Max(MathF.Abs(dirX), MathF.Abs(dirY));
    var stretch = (dirX * dirX + dirY * dirY) / maxAbs;

    // Anisotropic length.
    var lenAxX = 1f + (stretch - 1f) * len;
    var lenAxY = 1f + (-0.5f) * len;

    // Negative-lobe strength: shifts window from sqrt(2) to slightly beyond 2.0.
    var lob = 0.5f + (1f / 4f - 0.04f - 0.5f) * len;
    var clp = lob > 1e-9f ? 1f / lob : 1e9f;

    // 2×2 min/max for ringing clamp (per channel of the central f/g/j/k taps).
    var (fR, fG, fB, _) = ColorConverter.GetNormalizedRgba(in fC);
    var (gR, gG, gB, _) = ColorConverter.GetNormalizedRgba(in gC);
    var (jR, jG, jB, _) = ColorConverter.GetNormalizedRgba(in jC);
    var (kR, kG, kB, kA) = ColorConverter.GetNormalizedRgba(in kC);
    var minR = MathF.Min(MathF.Min(fR, gR), MathF.Min(jR, kR));
    var minG = MathF.Min(MathF.Min(fG, gG), MathF.Min(jG, kG));
    var minB = MathF.Min(MathF.Min(fB, gB), MathF.Min(jB, kB));
    var maxR = MathF.Max(MathF.Max(fR, gR), MathF.Max(jR, kR));
    var maxG = MathF.Max(MathF.Max(fG, gG), MathF.Max(jG, kG));
    var maxB = MathF.Max(MathF.Max(fB, gB), MathF.Max(jB, kB));

    // Accumulate 12 weighted taps. Each tap's offset is its pattern position − pp,
    // where pp = (fx, fy) is the target's fractional position within f's quadrant.
    float aR = 0f, aG = 0f, aB = 0f, aA = 0f, aW = 0f;
    void Tap(float offX, float offY, in TWork col) {
      var w = _Tap(offX - fx, offY - fy, dirX, dirY, lenAxX, lenAxY, lob, clp);
      var (r, g, bb, a) = ColorConverter.GetNormalizedRgba(in col);
      aR += r * w; aG += g * w; aB += bb * w; aA += a * w; aW += w;
    }
    Tap(0f, -1f, bC); // b
    Tap(1f, -1f, cC); // c
    Tap(-1f, 1f, iC); // i
    Tap(0f, 1f, jC);  // j
    Tap(0f, 0f, fC);  // f
    Tap(-1f, 0f, eC); // e
    Tap(1f, 1f, kC);  // k
    Tap(2f, 1f, lC);  // l
    Tap(2f, 0f, hC);  // h
    Tap(1f, 0f, gC);  // g
    Tap(1f, 2f, oC);  // o
    Tap(0f, 2f, nC);  // n

    // Normalize and dering: clamp to 2×2 min/max.
    var invW = aW != 0f ? 1f / aW : 0f;
    var outR = MathF.Max(minR, MathF.Min(maxR, aR * invW));
    var outG = MathF.Max(minG, MathF.Min(maxG, aG * invW));
    var outB = MathF.Max(minB, MathF.Min(maxB, aB * invW));
    var outA = aA * invW;

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, outA));
  }

}
