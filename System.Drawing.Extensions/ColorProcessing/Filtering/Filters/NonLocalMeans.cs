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
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Non-Local Means denoising — Buades, Coll &amp; Morel 2005.
/// </summary>
/// <remarks>
/// <para>For each output pixel, weights every other pixel in a search window by the
/// similarity of <i>patches</i> centred on each — so a pixel sitting on a particular
/// texture pattern gets contributions only from other pixels sitting on the <i>same</i>
/// pattern, regardless of geometric distance. The result is a remarkable preservation of
/// structured detail (lines, repeated textures) under aggressive smoothing — strictly
/// stronger denoising than the local-only filters in this namespace (bilateral, Perona-
/// Malik, etc.) on noisy images with repeated structure.</para>
/// <para>Distinct from the Round 3 <c>GlasnerSelfSimilarity</c> resampler (which picks the
/// single best-matching patch and uses it as a high-resolution candidate); NLM here is the
/// canonical <i>weighted average</i> form used for denoising, not super-resolution.</para>
/// <para>Default search radius 5 (11×11 window) and patch radius 1 (3×3 patches) — total
/// reach 6 source pixels around each output. Higher search radius improves quality but
/// increases per-pixel cost quadratically.</para>
/// <para>Reference: Buades, Coll &amp; Morel 2005, "A Non-Local Algorithm for Image
/// Denoising", CVPR.</para>
/// </remarks>
[FilterInfo("NonLocalMeans",
  Description = "Patch-similarity-weighted non-local denoising", Category = FilterCategory.Enhancement,
  Author = "Buades, Coll & Morel", Year = 2005)]
public readonly struct NonLocalMeans : IPixelFilter, IFrameFilter {

  private readonly int _searchRadius;
  private readonly int _patchRadius;
  private readonly float _h;

  /// <summary>Default: search radius 5 (11×11), patch radius 1 (3×3), filtering h = 0.05.</summary>
  public NonLocalMeans() : this(5, 1, 0.05f) { }

  /// <summary>Creates an NLM filter with custom parameters.</summary>
  /// <param name="searchRadius">Search-window half-width (3..10 typical; window is 2·r+1 wide).</param>
  /// <param name="patchRadius">Patch half-width (1..3 typical; patch is 2·p+1 wide).</param>
  /// <param name="h">Filtering parameter h ∈ (0, 0.5] — proportional to the noise standard
  /// deviation in normalised luminance. Smaller h = stronger detail preservation, weaker
  /// denoising; larger h = stronger denoising, more blurring.</param>
  public NonLocalMeans(int searchRadius, int patchRadius, float h) {
    this._searchRadius = Math.Max(1, Math.Min(10, searchRadius));
    this._patchRadius = Math.Max(0, Math.Min(3, patchRadius));
    this._h = Math.Max(1e-3f, Math.Min(0.5f, h));
  }

  /// <inheritdoc />
  public bool UsesFrameAccess => true;

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
    => callback.Invoke(new NlmPassThroughKernel<TWork, TKey, TPixel, TEncode>());

  /// <inheritdoc />
  public TResult InvokeFrameKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth, int sourceHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new NlmFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._searchRadius, this._patchRadius, this._h, sourceWidth, sourceHeight));

  /// <summary>Default configuration.</summary>
  public static NonLocalMeans Default => new();
}

file readonly struct NlmPassThroughKernel<TWork, TKey, TPixel, TEncode>
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder)
    => dest[0] = encoder.Encode(window.P0P0.Work);
}

file readonly struct NlmFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int searchRadius, int patchRadius, float h, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private readonly float _h2 = h * h;

  public int Radius => searchRadius + patchRadius;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(frame[destX, destY].Work);

    // Patch normalisation: number of pixels in a (2·p+1)² patch.
    var patchSize = 2 * patchRadius + 1;
    var patchCount = patchSize * patchSize;
    var invPatchCount = 1f / patchCount;

    float ar = cr, ag = cg, ab = cb;
    var totalW = 1f; // self-weight = 1 (perfect patch match against itself).

    for (var dy = -searchRadius; dy <= searchRadius; ++dy)
    for (var dx = -searchRadius; dx <= searchRadius; ++dx) {
      if (dx == 0 && dy == 0) continue;
      var qx = destX + dx;
      var qy = destY + dy;

      // Patch SAD against the centred patch around (destX, destY).
      var patchSad = 0f;
      for (var py = -patchRadius; py <= patchRadius; ++py)
      for (var px = -patchRadius; px <= patchRadius; ++px) {
        var (pr, pg, pb, _) = ColorConverter.GetNormalizedRgba(frame[destX + px, destY + py].Work);
        var (qr, qg, qb, _) = ColorConverter.GetNormalizedRgba(frame[qx + px, qy + py].Work);
        var dR = pr - qr;
        var dG = pg - qg;
        var dB = pb - qb;
        patchSad += dR * dR + dG * dG + dB * dB;
      }
      var patchDist2 = patchSad * invPatchCount; // normalised mean-squared patch distance

      var weight = MathF.Exp(-patchDist2 / _h2);
      var (qcr, qcg, qcb, _) = ColorConverter.GetNormalizedRgba(frame[qx, qy].Work);
      ar += qcr * weight;
      ag += qcg * weight;
      ab += qcb * weight;
      totalW += weight;
    }

    var inv = 1f / totalW;
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      ar * inv, ag * inv, ab * inv, ca));
  }
}
