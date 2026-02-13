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
/// Accented edges filter -- Sobel-based edge detection with brightness and smoothness modulation.
/// Computes Sobel gradient magnitude, scales by edge width, applies smoothness blending
/// with an averaged neighborhood, and modulates the result by brightness.
/// The original image is blended with the detected edge color based on edge magnitude.
/// </summary>
[FilterInfo("AccentedEdges",
  Description = "Sobel edges with brightness and smoothness modulation", Category = FilterCategory.Analysis)]
public readonly struct AccentedEdges : IPixelFilter, IFrameFilter {
  private readonly float _edgeWidth;
  private readonly float _brightness;
  private readonly float _smoothness;

  public AccentedEdges() : this(1f, 0.5f, 0.5f) { }

  public AccentedEdges(float edgeWidth = 1f, float brightness = 0.5f, float smoothness = 0.5f) {
    this._edgeWidth = Math.Max(0.1f, edgeWidth);
    this._brightness = Math.Max(0f, Math.Min(1f, brightness));
    this._smoothness = Math.Max(0f, Math.Min(1f, smoothness));
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
    => callback.Invoke(new AccentedEdgesPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new AccentedEdgesFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._edgeWidth, this._brightness, this._smoothness, sourceWidth, sourceHeight));

  public static AccentedEdges Default => new();
}

file readonly struct AccentedEdgesPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct AccentedEdgesFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float edgeWidth, float brightness, float smoothness, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => (int)Math.Ceiling(edgeWidth) + 1;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum(in NeighborPixel<TWork, TKey> p) {
    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);

    // Sobel edge detection using immediate neighbors
    var tl = _Lum(frame[destX - 1, destY - 1]);
    var t = _Lum(frame[destX, destY - 1]);
    var tr = _Lum(frame[destX + 1, destY - 1]);
    var l = _Lum(frame[destX - 1, destY]);
    var r = _Lum(frame[destX + 1, destY]);
    var bl = _Lum(frame[destX - 1, destY + 1]);
    var b = _Lum(frame[destX, destY + 1]);
    var br = _Lum(frame[destX + 1, destY + 1]);

    // Sobel X: [-1,0,1; -2,0,2; -1,0,1]
    var gx = -tl + tr - 2f * l + 2f * r - bl + br;
    // Sobel Y: [-1,-2,-1; 0,0,0; 1,2,1]
    var gy = -tl - 2f * t - tr + bl + 2f * b + br;

    var rawMag = (float)Math.Sqrt(gx * gx + gy * gy);

    // Scale edge magnitude by edgeWidth
    var scaledMag = rawMag * edgeWidth;

    // Compute smoothed version: average center + 4-connected neighbors weighted by smoothness
    var avgR = cr;
    var avgG = cg;
    var avgB = cb;
    if (smoothness > 0f) {
      var tPx = frame[destX, destY - 1].Work;
      var (tR, tG, tB, _) = ColorConverter.GetNormalizedRgba(in tPx);
      var bPx = frame[destX, destY + 1].Work;
      var (bR, bG, bB, _) = ColorConverter.GetNormalizedRgba(in bPx);
      var lPx = frame[destX - 1, destY].Work;
      var (lR, lG, lB, _) = ColorConverter.GetNormalizedRgba(in lPx);
      var rPx = frame[destX + 1, destY].Work;
      var (rR, rG, rB, _) = ColorConverter.GetNormalizedRgba(in rPx);

      var smoothR = (cr + tR + bR + lR + rR) * 0.2f;
      var smoothG = (cg + tG + bG + lG + rG) * 0.2f;
      var smoothB = (cb + tB + bB + lB + rB) * 0.2f;

      avgR = cr * (1f - smoothness) + smoothR * smoothness;
      avgG = cg * (1f - smoothness) + smoothG * smoothness;
      avgB = cb * (1f - smoothness) + smoothB * smoothness;
    }

    // Apply smoothness to edge magnitude: blend raw edge with smoothed edge
    var smoothedMag = scaledMag * (1f - smoothness * 0.5f);

    // Modulate by brightness
    var edgeMag = Math.Min(1f, smoothedMag * brightness * 2f);

    // Edge color is brightened version of the averaged original
    var edgeR = Math.Min(1f, avgR + edgeMag);
    var edgeG = Math.Min(1f, avgG + edgeMag);
    var edgeB = Math.Min(1f, avgB + edgeMag);

    // Blend: original * (1 - edgeMag) + edgeColor * edgeMag
    var outR = Math.Max(0f, Math.Min(1f, avgR * (1f - edgeMag) + edgeR * edgeMag));
    var outG = Math.Max(0f, Math.Min(1f, avgG * (1f - edgeMag) + edgeG * edgeMag));
    var outB = Math.Max(0f, Math.Min(1f, avgB * (1f - edgeMag) + edgeB * edgeMag));

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, ca));
  }
}
