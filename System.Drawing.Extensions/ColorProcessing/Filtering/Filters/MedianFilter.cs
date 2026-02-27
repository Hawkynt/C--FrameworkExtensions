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
/// Median filter with configurable radius.
/// Selects the pixel with the median luminance from the neighborhood.
/// For radius 1, uses an optimized sorting network on the 3x3 window.
/// For radius 2, uses the 5x5 window with sorting.
/// For larger radii, uses frame-level random access.
/// </summary>
[FilterInfo("MedianFilter",
  Description = "Median filter for noise reduction with configurable radius", Category = FilterCategory.Enhancement)]
public readonly struct MedianFilter(int radius) : IPixelFilter, IFrameFilter {
  private readonly int _radius = Math.Max(0, radius);

  public MedianFilter() : this(1) { }

  /// <inheritdoc />
  public bool UsesFrameAccess => this._radius > 2;

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
    => callback.Invoke(new MedianKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new MedianFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._radius, sourceWidth, sourceHeight));

  public static MedianFilter Default => new();
}

file readonly struct MedianKernel<TWork, TKey, TPixel, TEncode>
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum(in NeighborPixel<TWork, TKey> p) {
    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _Sort2(ref float a, ref float b, ref int ia, ref int ib) {
    if (a <= b) return;
    (a, b) = (b, a);
    (ia, ib) = (ib, ia);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var l0 = _Lum(window.M1M1);
    var l1 = _Lum(window.M1P0);
    var l2 = _Lum(window.M1P1);
    var l3 = _Lum(window.P0M1);
    var l4 = _Lum(window.P0P0);
    var l5 = _Lum(window.P0P1);
    var l6 = _Lum(window.P1M1);
    var l7 = _Lum(window.P1P0);
    var l8 = _Lum(window.P1P1);

    int i0 = 0, i1 = 1, i2 = 2, i3 = 3, i4 = 4, i5 = 5, i6 = 6, i7 = 7, i8 = 8;

    _Sort2(ref l1, ref l2, ref i1, ref i2);
    _Sort2(ref l4, ref l5, ref i4, ref i5);
    _Sort2(ref l7, ref l8, ref i7, ref i8);
    _Sort2(ref l0, ref l1, ref i0, ref i1);
    _Sort2(ref l3, ref l4, ref i3, ref i4);
    _Sort2(ref l6, ref l7, ref i6, ref i7);
    _Sort2(ref l1, ref l2, ref i1, ref i2);
    _Sort2(ref l4, ref l5, ref i4, ref i5);
    _Sort2(ref l7, ref l8, ref i7, ref i8);
    _Sort2(ref l0, ref l3, ref i0, ref i3);
    _Sort2(ref l5, ref l8, ref i5, ref i8);
    _Sort2(ref l4, ref l7, ref i4, ref i7);
    _Sort2(ref l3, ref l6, ref i3, ref i6);
    _Sort2(ref l1, ref l4, ref i1, ref i4);
    _Sort2(ref l2, ref l5, ref i2, ref i5);
    _Sort2(ref l4, ref l7, ref i4, ref i7);
    _Sort2(ref l4, ref l2, ref i4, ref i2);
    _Sort2(ref l6, ref l4, ref i6, ref i4);
    _Sort2(ref l4, ref l2, ref i4, ref i2);

    var median = i4 switch {
      0 => window.M1M1.Work,
      1 => window.M1P0.Work,
      2 => window.M1P1.Work,
      3 => window.P0M1.Work,
      4 => window.P0P0.Work,
      5 => window.P0P1.Work,
      6 => window.P1M1.Work,
      7 => window.P1P0.Work,
      _ => window.P1P1.Work,
    };

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    var (mr, mg, mb, _) = ColorConverter.GetNormalizedRgba(in median);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(mr, mg, mb, ca));
  }
}

file readonly struct MedianFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
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
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var size = (2 * radius + 1) * (2 * radius + 1);
    var lums = new float[size];
    var pixels = new TWork[size];
    var idx = 0;

    for (var dy = -radius; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      lums[idx] = ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
      pixels[idx] = px;
      ++idx;
    }

    Array.Sort(lums, pixels);
    var median = pixels[size / 2];

    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    var (mr, mg, mb, _2) = ColorConverter.GetNormalizedRgba(in median);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(mr, mg, mb, ca));
  }
}
