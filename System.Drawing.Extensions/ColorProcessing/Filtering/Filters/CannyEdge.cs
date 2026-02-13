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
/// Canny edge detection with hysteresis thresholding.
/// Computes Sobel gradient magnitude and direction, applies simplified non-maximum suppression,
/// and uses dual thresholds (low/high) for hysteresis: strong edges are white, weak edges are gray,
/// and non-edges are black.
/// </summary>
[FilterInfo("CannyEdge",
  Description = "Canny edge detection with hysteresis thresholding", Category = FilterCategory.Analysis)]
public readonly struct CannyEdge(float lowThreshold = 0.1f, float highThreshold = 0.3f) : IPixelFilter, IFrameFilter {
  private readonly float _lowThreshold = lowThreshold;
  private readonly float _highThreshold = highThreshold;

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
    => callback.Invoke(new CannyEdgePassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new CannyEdgeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._lowThreshold, this._highThreshold, sourceWidth, sourceHeight));

  public static CannyEdge Default => new(0.1f, 0.3f);
}

file readonly struct CannyEdgePassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct CannyEdgeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float lowThreshold, float highThreshold, int sourceWidth, int sourceHeight)
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

    // Sobel gradient
    var tl = _Lum(frame, destX - 1, destY - 1);
    var t = _Lum(frame, destX, destY - 1);
    var tr = _Lum(frame, destX + 1, destY - 1);
    var l = _Lum(frame, destX - 1, destY);
    var ri = _Lum(frame, destX + 1, destY);
    var bl = _Lum(frame, destX - 1, destY + 1);
    var bo = _Lum(frame, destX, destY + 1);
    var br = _Lum(frame, destX + 1, destY + 1);

    var gx = -tl + tr - 2f * l + 2f * ri - bl + br;
    var gy = -tl - 2f * t - tr + bl + 2f * bo + br;
    var edgeMag = (float)Math.Sqrt(gx * gx + gy * gy);

    // Non-maximum suppression (simplified)
    var dir = (float)Math.Atan2(gy, gx);
    var absDir = dir < 0 ? dir + (float)Math.PI : dir;
    float n1, n2;
    if (absDir < Math.PI / 8 || absDir >= 7 * Math.PI / 8) {
      n1 = _Lum(frame, destX - 1, destY);
      n2 = _Lum(frame, destX + 1, destY);
    } else if (absDir < 3 * Math.PI / 8) {
      n1 = _Lum(frame, destX + 1, destY - 1);
      n2 = _Lum(frame, destX - 1, destY + 1);
    } else if (absDir < 5 * Math.PI / 8) {
      n1 = _Lum(frame, destX, destY - 1);
      n2 = _Lum(frame, destX, destY + 1);
    } else {
      n1 = _Lum(frame, destX - 1, destY - 1);
      n2 = _Lum(frame, destX + 1, destY + 1);
    }

    // Suppress if not a local maximum along gradient direction
    var centerLum = _Lum(frame, destX, destY);
    var diff1 = Math.Abs(centerLum - n1);
    var diff2 = Math.Abs(centerLum - n2);
    if (edgeMag < diff1 || edgeMag < diff2)
      edgeMag = 0f;

    // Hysteresis thresholding
    var v = edgeMag >= highThreshold ? 1f : edgeMag >= lowThreshold ? 0.5f : 0f;

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(v, v, v, a));
  }
}
