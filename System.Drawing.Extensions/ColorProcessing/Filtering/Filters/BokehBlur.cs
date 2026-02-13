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
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Disc-shaped blur that emphasizes bright spots in the image.
/// Pixels within a circular neighborhood are weighted by luminance,
/// creating the characteristic bokeh look of out-of-focus highlights.
/// </summary>
[FilterInfo("BokehBlur",
  Description = "Disc-shaped blur with bright spot emphasis", Category = FilterCategory.Enhancement)]
public readonly struct BokehBlur(int radius = 5, float threshold = 0.5f, int bladeCount = 6) : IPixelFilter, IFrameFilter {
  private readonly int _radius = Math.Max(1, radius);
  private readonly float _threshold = threshold;
  private readonly int _bladeCount = bladeCount;

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
    => callback.Invoke(new BokehBlurPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new BokehBlurFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._radius, this._threshold, this._bladeCount, sourceWidth, sourceHeight));

  public static BokehBlur Default => new(5, 0.5f, 6);
}

file readonly struct BokehBlurPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct BokehBlurFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int radius, float threshold, int bladeCount, int sourceWidth, int sourceHeight)
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
  private static float _Lum(NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame, int x, int y) {
    var px = frame[x, y].Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var bc = Math.Max(3, bladeCount);
    var angleStep = (float)(2.0 * Math.PI / bc);
    float ar = 0, ag = 0, ab = 0, wSum = 0;

    for (var dy = -radius; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      var dist = (float)Math.Sqrt(dx * dx + dy * dy);
      if (dist > radius)
        continue;

      var angle = (float)Math.Atan2(dy, dx);
      if (angle < 0) angle += (float)(2.0 * Math.PI);
      var sector = angle / angleStep;
      var sectorAngle = (sector - (float)Math.Floor(sector)) * angleStep;
      var polyRadius = radius * (float)Math.Cos(angleStep * 0.5f) / (float)Math.Cos(sectorAngle - angleStep * 0.5f);
      if (dist > polyRadius)
        continue;

      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      var lum = ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
      var w = lum > threshold ? 1f + (lum - threshold) * 3f : 1f;
      ar += r * w;
      ag += g * w;
      ab += b * w;
      wSum += w;
    }

    if (wSum > 0f) {
      ar /= wSum;
      ag /= wSum;
      ab /= wSum;
    }

    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(ar, ag, ab, ca));
  }
}
