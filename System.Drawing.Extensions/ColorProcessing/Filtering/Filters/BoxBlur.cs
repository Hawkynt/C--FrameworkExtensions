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
/// Box blur with configurable radii per axis.
/// Supports both square (k x k) and rectangular (k x m) kernels of any size.
/// For radii 0-2, uses the efficient 5x5 NeighborWindow.
/// For larger radii, uses frame-level random access for single-pass computation.
/// </summary>
[FilterInfo("BoxBlur",
  Description = "Uniform box blur with configurable kernel size", Category = FilterCategory.Artistic)]
public readonly struct BoxBlur : IPixelFilter, IFrameFilter {
  private readonly int _rx, _ry;
  private readonly float _xw0, _xw1, _xw2;
  private readonly float _yw0, _yw1, _yw2;

  public BoxBlur() : this(1, 1) { }

  public BoxBlur(int radiusX, int radiusY) {
    this._rx = Math.Max(0, radiusX);
    this._ry = Math.Max(0, radiusY);
    if (this._rx <= 2 && this._ry <= 2) {
      (this._xw0, this._xw1, this._xw2) = _ComputeWeights(this._rx);
      (this._yw0, this._yw1, this._yw2) = _ComputeWeights(this._ry);
    } else {
      (this._xw0, this._xw1, this._xw2) = (0f, 0f, 0f);
      (this._yw0, this._yw1, this._yw2) = (0f, 0f, 0f);
    }
  }

  /// <inheritdoc />
  public bool UsesFrameAccess => this._rx > 2 || this._ry > 2;

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
    => callback.Invoke(new BoxBlurKernel<TWork, TKey, TPixel, TEncode>(
      this._xw0, this._xw1, this._xw2,
      this._yw0, this._yw1, this._yw2));

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
    => callback.Invoke(new BoxBlurFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._rx, this._ry, sourceWidth, sourceHeight));

  private static (float w0, float w1, float w2) _ComputeWeights(int radius) {
    if (radius <= 0)
      return (1f, 0f, 0f);

    var size = 2 * radius + 1;
    var w = 1f / size;
    return radius switch {
      1 => (w, w, 0f),
      _ => (w, w, w),
    };
  }

  public static BoxBlur Default => new();
}

file readonly struct BoxBlurKernel<TWork, TKey, TPixel, TEncode>(
  float xw0, float xw1, float xw2,
  float yw0, float yw1, float yw2)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _Accum(ref float ar, ref float ag, ref float ab, in NeighborPixel<TWork, TKey> p, float w) {
    if (w == 0f)
      return;

    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    ar += r * w;
    ag += g * w;
    ab += b * w;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AccumRow(
    ref float ar, ref float ag, ref float ab,
    in NeighborPixel<TWork, TKey> pm2,
    in NeighborPixel<TWork, TKey> pm1,
    in NeighborPixel<TWork, TKey> p0,
    in NeighborPixel<TWork, TKey> pp1,
    in NeighborPixel<TWork, TKey> pp2,
    float rowWeight) {
    if (rowWeight == 0f)
      return;

    _Accum(ref ar, ref ag, ref ab, pm2, xw2 * rowWeight);
    _Accum(ref ar, ref ag, ref ab, pm1, xw1 * rowWeight);
    _Accum(ref ar, ref ag, ref ab, p0, xw0 * rowWeight);
    _Accum(ref ar, ref ag, ref ab, pp1, xw1 * rowWeight);
    _Accum(ref ar, ref ag, ref ab, pp2, xw2 * rowWeight);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    float ar = 0, ag = 0, ab = 0;

    _AccumRow(ref ar, ref ag, ref ab, window.M2M2, window.M2M1, window.M2P0, window.M2P1, window.M2P2, yw2);
    _AccumRow(ref ar, ref ag, ref ab, window.M1M2, window.M1M1, window.M1P0, window.M1P1, window.M1P2, yw1);
    _AccumRow(ref ar, ref ag, ref ab, window.P0M2, window.P0M1, window.P0P0, window.P0P1, window.P0P2, yw0);
    _AccumRow(ref ar, ref ag, ref ab, window.P1M2, window.P1M1, window.P1P0, window.P1P1, window.P1P2, yw1);
    _AccumRow(ref ar, ref ag, ref ab, window.P2M2, window.P2M1, window.P2P0, window.P2P1, window.P2P2, yw2);

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(ar, ag, ab, ca));
  }
}

file readonly struct BoxBlurFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int rx, int ry, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(rx, ry);
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
    var inv = 1f / ((2 * rx + 1) * (2 * ry + 1));
    float ar = 0, ag = 0, ab = 0;
    for (var dy = -ry; dy <= ry; ++dy)
    for (var dx = -rx; dx <= rx; ++dx) {
      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      ar += r;
      ag += g;
      ab += b;
    }

    ar *= inv;
    ag *= inv;
    ab *= inv;

    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(ar, ag, ab, ca));
  }
}
