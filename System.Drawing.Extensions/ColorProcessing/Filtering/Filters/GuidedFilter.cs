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
/// Guided filter (He, Sun &amp; Tang 2010) — edge-preserving smoothing that uses the
/// input image as its own guidance signal. For each pixel a local linear model
/// <c>q = a·I + b</c> is fit on a <c>(2r+1)×(2r+1)</c> window, with the variance term
/// regularised by <paramref name="epsilon"/>; smoothing strength is governed by
/// <paramref name="epsilon"/> and the window radius.
/// </summary>
/// <remarks>
/// <para>
/// Compared to the bilateral filter, the guided filter has O(N) cost in the radius and
/// preserves linear gradients perfectly inside flat or weakly textured regions.
/// </para>
/// <para>
/// This implementation operates per-channel (R/G/B) using self-guidance and computes
/// box-mean / box-variance / a-b box-means directly per output pixel using random frame
/// access. Default radius = 4, epsilon = 0.01 (corresponds to roughly 0.1² in
/// normalized intensity, i.e. mild detail-preserving smoothing).
/// </para>
/// <para>
/// Reference: K. He, J. Sun &amp; X. Tang, "Guided Image Filtering", ECCV 2010.
/// </para>
/// </remarks>
[FilterInfo("GuidedFilter",
  Author = "He, Sun & Tang", Year = 2010,
  Url = "http://kaiminghe.com/eccv10/",
  Description = "Edge-preserving guided filter with self-guidance (He et al. 2010)",
  Category = FilterCategory.Enhancement)]
public readonly struct GuidedFilter : IPixelFilter, IFrameFilter {
  private readonly int _radius;
  private readonly float _epsilon;

  public GuidedFilter() : this(4, 0.01f) { }

  public GuidedFilter(int radius, float epsilon) {
    this._radius = Math.Max(1, Math.Min(16, radius));
    this._epsilon = Math.Max(1e-6f, Math.Min(1f, epsilon));
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
    => callback.Invoke(new GuidedPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new GuidedFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._radius, this._epsilon, sourceWidth, sourceHeight));

  public static GuidedFilter Default => new();
}

file readonly struct GuidedPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct GuidedFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int radius, float epsilon, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 2 * radius;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (float r, float g, float b, float a) _Get(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame, int x, int y) {
    var px = frame[x, y].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);
    return (r, g, b, a);
  }

  /// <summary>
  /// Computes a per-channel guided-filter output for one pixel using a single (2r+1)²
  /// window centred at the destination. Self-guidance: q = a·I + b with
  /// a = var/(var+ε) and b = (1-a)·mean.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private (float r, float g, float b) _Process(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame, int destX, int destY) {
    float sumR = 0, sumG = 0, sumB = 0;
    float sumR2 = 0, sumG2 = 0, sumB2 = 0;
    var count = 0;
    for (var dy = -radius; dy <= radius; ++dy) {
      var y = destY + dy;
      for (var dx = -radius; dx <= radius; ++dx) {
        var x = destX + dx;
        var (r, g, b, _) = _Get(frame, x, y);
        sumR += r; sumG += g; sumB += b;
        sumR2 += r * r; sumG2 += g * g; sumB2 += b * b;
        ++count;
      }
    }
    var inv = 1f / count;
    var meanR = sumR * inv;
    var meanG = sumG * inv;
    var meanB = sumB * inv;
    var varR = sumR2 * inv - meanR * meanR;
    var varG = sumG2 * inv - meanG * meanG;
    var varB = sumB2 * inv - meanB * meanB;
    if (varR < 0f) varR = 0f;
    if (varG < 0f) varG = 0f;
    if (varB < 0f) varB = 0f;

    var (cr, cg, cb, _) = _Get(frame, destX, destY);
    var aR = varR / (varR + epsilon);
    var aG = varG / (varG + epsilon);
    var aB = varB / (varB + epsilon);
    var bR = meanR - aR * meanR;
    var bG = meanG - aG * meanG;
    var bB = meanB - aB * meanB;

    var qR = aR * cr + bR;
    var qG = aG * cg + bG;
    var qB = aB * cb + bB;

    if (qR < 0f) qR = 0f; else if (qR > 1f) qR = 1f;
    if (qG < 0f) qG = 0f; else if (qG > 1f) qG = 1f;
    if (qB < 0f) qB = 0f; else if (qB > 1f) qB = 1f;
    return (qR, qG, qB);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var (r, g, b) = this._Process(frame, destX, destY);
    var (_, _, _, a) = _Get(frame, destX, destY);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(r, g, b, a));
  }
}
