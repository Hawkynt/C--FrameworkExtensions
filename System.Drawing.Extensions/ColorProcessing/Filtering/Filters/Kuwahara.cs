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
/// Kuwahara filter — edge-preserving painterly smoothing (Kuwahara et al. 1976).
/// </summary>
/// <remarks>
/// <para>Divides a square neighbourhood into four overlapping quadrants. For each, the
/// mean and variance of luminance are computed; the output pixel takes the colour
/// mean of the quadrant with the LOWEST variance. Because edges and texture
/// boundaries lie inside high-variance quadrants, the filter selectively averages
/// only within smoothly-varying regions, producing a painterly look that preserves
/// sharp edges far better than mean / median filtering.</para>
/// <para>Reference: M. Kuwahara, K. Hachimura, S. Eiho &amp; M. Kinoshita, "Processing
/// of RI-Angiocardiographic Images", in Digital Processing of Biomedical Images
/// (Plenum Press 1976), pp. 187-202. Modern variants (Papari et al. 2007) use
/// circular weighting kernels for better rotation invariance.</para>
/// </remarks>
[FilterInfo("Kuwahara",
  Description = "Painterly Kuwahara filter using quadrant variance analysis", Category = FilterCategory.Artistic)]
public readonly struct Kuwahara(int radius) : IPixelFilter, IFrameFilter {
  private readonly int _radius = Math.Max(1, radius);

  public Kuwahara() : this(3) { }

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
    => throw new NotSupportedException("Kuwahara requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new KuwaharaFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._radius, sourceWidth, sourceHeight));

  public static Kuwahara Default => new();
}

file readonly struct KuwaharaFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int radius, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => radius;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ComputeQuadrant(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int cx, int cy,
    int x0, int y0, int x1, int y1,
    out float mr, out float mg, out float mb, out float variance) {
    float sr = 0, sg = 0, sb = 0;
    float sr2 = 0, sg2 = 0, sb2 = 0;
    var count = 0;

    for (var y = y0; y <= y1; ++y)
    for (var x = x0; x <= x1; ++x) {
      var px = frame[cx + x, cy + y].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      sr += r;
      sg += g;
      sb += b;
      sr2 += r * r;
      sg2 += g * g;
      sb2 += b * b;
      ++count;
    }

    var inv = 1f / count;
    mr = sr * inv;
    mg = sg * inv;
    mb = sb * inv;
    variance = (sr2 * inv - mr * mr) + (sg2 * inv - mg * mg) + (sb2 * inv - mb * mb);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var r = radius;

    _ComputeQuadrant(frame, destX, destY, -r, -r, 0, 0, out var mr0, out var mg0, out var mb0, out var v0);
    _ComputeQuadrant(frame, destX, destY, 0, -r, r, 0, out var mr1, out var mg1, out var mb1, out var v1);
    _ComputeQuadrant(frame, destX, destY, -r, 0, 0, r, out var mr2, out var mg2, out var mb2, out var v2);
    _ComputeQuadrant(frame, destX, destY, 0, 0, r, r, out var mr3, out var mg3, out var mb3, out var v3);

    float bestR, bestG, bestB;
    var minVar = v0;
    (bestR, bestG, bestB) = (mr0, mg0, mb0);

    if (v1 < minVar) {
      minVar = v1;
      (bestR, bestG, bestB) = (mr1, mg1, mb1);
    }

    if (v2 < minVar) {
      minVar = v2;
      (bestR, bestG, bestB) = (mr2, mg2, mb2);
    }

    if (v3 < minVar)
      (bestR, bestG, bestB) = (mr3, mg3, mb3);

    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(bestR, bestG, bestB, ca));
  }
}
