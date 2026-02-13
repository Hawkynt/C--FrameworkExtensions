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
/// Morphological dilation with configurable radius.
/// Outputs the brightest pixel (by luminance) in the neighborhood.
/// For radii 0-2, uses the efficient 5x5 NeighborWindow.
/// For larger radii, uses frame-level random access.
/// </summary>
[FilterInfo("Dilate",
  Description = "Morphological dilation (max luminance) with configurable radius", Category = FilterCategory.Enhancement)]
public readonly struct Dilate(int radius) : IPixelFilter, IFrameFilter {
  private readonly int _radius = Math.Max(0, radius);

  public Dilate() : this(1) { }

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
    => callback.Invoke(new DilateKernel<TWork, TKey, TPixel, TEncode>(this._radius));

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
    => callback.Invoke(new DilateFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._radius, sourceWidth, sourceHeight));

  public static Dilate Default => new();
}

file readonly struct DilateKernel<TWork, TKey, TPixel, TEncode>(int radius)
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
  private static void _Check(ref float maxLum, ref TWork best, in NeighborPixel<TWork, TKey> p) {
    var lum = _Lum(p);
    if (lum <= maxLum)
      return;

    maxLum = lum;
    best = p.Work;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _CheckRow(
    ref float maxLum, ref TWork best,
    in NeighborPixel<TWork, TKey> pm2,
    in NeighborPixel<TWork, TKey> pm1,
    in NeighborPixel<TWork, TKey> p0,
    in NeighborPixel<TWork, TKey> pp1,
    in NeighborPixel<TWork, TKey> pp2) {
    if (radius >= 2)
      _Check(ref maxLum, ref best, pm2);
    if (radius >= 1)
      _Check(ref maxLum, ref best, pm1);
    _Check(ref maxLum, ref best, p0);
    if (radius >= 1)
      _Check(ref maxLum, ref best, pp1);
    if (radius >= 2)
      _Check(ref maxLum, ref best, pp2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var maxLum = float.MinValue;
    var best = window.P0P0.Work;

    if (radius >= 2)
      _CheckRow(ref maxLum, ref best, window.M2M2, window.M2M1, window.M2P0, window.M2P1, window.M2P2);
    if (radius >= 1)
      _CheckRow(ref maxLum, ref best, window.M1M2, window.M1M1, window.M1P0, window.M1P1, window.M1P2);
    _CheckRow(ref maxLum, ref best, window.P0M2, window.P0M1, window.P0P0, window.P0P1, window.P0P2);
    if (radius >= 1)
      _CheckRow(ref maxLum, ref best, window.P1M2, window.P1M1, window.P1P0, window.P1P1, window.P1P2);
    if (radius >= 2)
      _CheckRow(ref maxLum, ref best, window.P2M2, window.P2M1, window.P2P0, window.P2P1, window.P2P2);

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    var (mr, mg, mb, _) = ColorConverter.GetNormalizedRgba(in best);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(mr, mg, mb, ca));
  }
}

file readonly struct DilateFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
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
    var maxLum = float.MinValue;
    var best = frame[destX, destY].Work;

    for (var dy = -radius; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      var lum = ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
      if (lum <= maxLum)
        continue;

      maxLum = lum;
      best = px;
    }

    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    var (mr, mg, mb, _2) = ColorConverter.GetNormalizedRgba(in best);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(mr, mg, mb, ca));
  }
}
