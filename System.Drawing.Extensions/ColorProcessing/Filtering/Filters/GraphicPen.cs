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
/// Graphic pen stroke effect with directional edges.
/// Computes Sobel gradients and projects them along a configurable angle
/// to produce directional pen strokes thresholded to black and white.
/// </summary>
[FilterInfo("GraphicPen",
  Description = "Graphic pen stroke effect with directional edges", Category = FilterCategory.Artistic)]
public readonly struct GraphicPen : IPixelFilter, IFrameFilter {
  private readonly float _strokeWidth;
  private readonly float _lightDarkBalance;
  private readonly float _cos;
  private readonly float _sin;

  public GraphicPen() : this(1f, 0.5f, 45f) { }

  public GraphicPen(float strokeWidth = 1f, float lightDarkBalance = 0.5f, float angle = 45f) {
    this._strokeWidth = strokeWidth;
    this._lightDarkBalance = lightDarkBalance;
    var rad = angle * (float)(Math.PI / 180.0);
    this._cos = (float)Math.Cos(rad);
    this._sin = (float)Math.Sin(rad);
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
    => callback.Invoke(new GraphicPenPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new GraphicPenFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._strokeWidth, this._lightDarkBalance, this._cos, this._sin, sourceWidth, sourceHeight));

  public static GraphicPen Default => new(1f, 0.5f, 45f);
}

file readonly struct GraphicPenPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct GraphicPenFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float strokeWidth, float lightDarkBalance, float cos, float sin, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 1;
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
    var px = frame[destX, destY].Work;
    var (_, _, _, a) = ColorConverter.GetNormalizedRgba(in px);

    var tl = _Lum(frame, destX - 1, destY - 1);
    var t  = _Lum(frame, destX,     destY - 1);
    var tr = _Lum(frame, destX + 1, destY - 1);
    var l  = _Lum(frame, destX - 1, destY);
    var ri = _Lum(frame, destX + 1, destY);
    var bl = _Lum(frame, destX - 1, destY + 1);
    var bo = _Lum(frame, destX,     destY + 1);
    var br = _Lum(frame, destX + 1, destY + 1);

    var gx = -tl + tr - 2f * l + 2f * ri - bl + br;
    var gy = -tl - 2f * t - tr + bl + 2f * bo + br;

    var grad = gx * cos + gy * sin;
    var mag = Math.Min(1f, Math.Abs(grad) * strokeWidth);
    var bw = mag > lightDarkBalance ? 0f : 1f;

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(bw, bw, bw, a));
  }
}
