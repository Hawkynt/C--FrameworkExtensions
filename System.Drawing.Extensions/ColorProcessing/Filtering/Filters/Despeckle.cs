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
/// Hybrid median filter for speckle noise removal.
/// Computes medians along horizontal, vertical, and both diagonal axes,
/// then takes the per-channel median of those four sub-medians.
/// </summary>
[FilterInfo("Despeckle",
  Description = "Hybrid median filter for speckle noise removal", Category = FilterCategory.Noise)]
public readonly struct Despeckle(int radius) : IPixelFilter, IFrameFilter {
  private readonly int _radius = Math.Max(1, radius);

  public Despeckle() : this(1) { }

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
    => callback.Invoke(new DespecklePassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new DespeckleFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._radius, sourceWidth, sourceHeight));

  public static Despeckle Default => new();
}

file readonly struct DespecklePassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct DespeckleFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int radius, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, radius);
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Luminance(float r, float g, float b)
    => ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _SortByLuminance(float[] lr, float[] lg, float[] lb, float[] lum, int count) {
    for (var i = 1; i < count; ++i) {
      var keyL = lum[i];
      var keyR = lr[i];
      var keyG = lg[i];
      var keyB = lb[i];
      var j = i - 1;
      while (j >= 0 && lum[j] > keyL) {
        lum[j + 1] = lum[j];
        lr[j + 1] = lr[j];
        lg[j + 1] = lg[j];
        lb[j + 1] = lb[j];
        --j;
      }

      lum[j + 1] = keyL;
      lr[j + 1] = keyR;
      lg[j + 1] = keyG;
      lb[j + 1] = keyB;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _MedianOf4(float a, float b, float c, float d) {
    if (a > b) (a, b) = (b, a);
    if (c > d) (c, d) = (d, c);
    if (a > c) (a, c) = (c, a);
    if (b > d) (b, d) = (d, b);
    return (b + c) * 0.5f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var len = 2 * radius + 1;

    var hR = new float[len];
    var hG = new float[len];
    var hB = new float[len];
    var hL = new float[len];

    var vR = new float[len];
    var vG = new float[len];
    var vB = new float[len];
    var vL = new float[len];

    var d1R = new float[len];
    var d1G = new float[len];
    var d1B = new float[len];
    var d1L = new float[len];

    var d2R = new float[len];
    var d2G = new float[len];
    var d2B = new float[len];
    var d2L = new float[len];

    for (var i = -radius; i <= radius; ++i) {
      var idx = i + radius;

      var hPx = frame[destX + i, destY].Work;
      var (hr, hg, hb, _) = ColorConverter.GetNormalizedRgba(in hPx);
      hR[idx] = hr;
      hG[idx] = hg;
      hB[idx] = hb;
      hL[idx] = _Luminance(hr, hg, hb);

      var vPx = frame[destX, destY + i].Work;
      var (vr, vg, vb, _2) = ColorConverter.GetNormalizedRgba(in vPx);
      vR[idx] = vr;
      vG[idx] = vg;
      vB[idx] = vb;
      vL[idx] = _Luminance(vr, vg, vb);

      var d1Px = frame[destX + i, destY + i].Work;
      var (d1r, d1g, d1b, _3) = ColorConverter.GetNormalizedRgba(in d1Px);
      d1R[idx] = d1r;
      d1G[idx] = d1g;
      d1B[idx] = d1b;
      d1L[idx] = _Luminance(d1r, d1g, d1b);

      var d2Px = frame[destX + i, destY - i].Work;
      var (d2r, d2g, d2b, _4) = ColorConverter.GetNormalizedRgba(in d2Px);
      d2R[idx] = d2r;
      d2G[idx] = d2g;
      d2B[idx] = d2b;
      d2L[idx] = _Luminance(d2r, d2g, d2b);
    }

    _SortByLuminance(hR, hG, hB, hL, len);
    _SortByLuminance(vR, vG, vB, vL, len);
    _SortByLuminance(d1R, d1G, d1B, d1L, len);
    _SortByLuminance(d2R, d2G, d2B, d2L, len);

    var mid = len / 2;
    var or2 = _MedianOf4(hR[mid], vR[mid], d1R[mid], d2R[mid]);
    var og = _MedianOf4(hG[mid], vG[mid], d1G[mid], d2G[mid]);
    var ob = _MedianOf4(hB[mid], vB[mid], d1B[mid], d2B[mid]);

    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or2, og, ob, ca));
  }
}
