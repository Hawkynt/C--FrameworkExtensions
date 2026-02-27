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
/// Kuwahara filter — painterly effect.
/// Divides neighborhood into 4 quadrants, computes mean and variance for each,
/// outputs the mean of the quadrant with lowest variance.
/// Always uses frame-level random access.
/// </summary>
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
    => callback.Invoke(new KuwaharaPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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

file readonly struct KuwaharaPassThroughKernel<TWork, TKey, TPixel, TEncode>
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
