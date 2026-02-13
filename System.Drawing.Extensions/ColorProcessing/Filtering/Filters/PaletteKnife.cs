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
/// Palette knife painting effect using directional smoothing along edges.
/// Computes Sobel gradients on luminance to find edge direction, then averages
/// pixels along the edge (perpendicular to gradient) with Gaussian weighting
/// to simulate paint strokes made by a palette knife.
/// Always uses frame-level random access for directional sampling.
/// </summary>
[FilterInfo("PaletteKnife",
  Description = "Palette knife painting with directional edge-following smoothing", Category = FilterCategory.Artistic)]
public readonly struct PaletteKnife(int strokeSize, float softness = 0.5f) : IPixelFilter, IFrameFilter {
  private readonly int _strokeSize = Math.Max(1, strokeSize);
  private readonly float _softness = Math.Max(0.01f, Math.Min(2f, softness));

  public PaletteKnife() : this(5, 0.5f) { }

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
    => callback.Invoke(new PaletteKnifePassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new PaletteKnifeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._strokeSize, this._softness, sourceWidth, sourceHeight));

  public static PaletteKnife Default => new();
}

file readonly struct PaletteKnifePassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct PaletteKnifeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int strokeSize, float softness, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, strokeSize);
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
    // Sobel gradient on luminance to find edge direction
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

    // Edge direction is perpendicular to gradient: (-gy, gx)
    var mag = (float)Math.Sqrt(gx * gx + gy * gy);
    float edgeDx, edgeDy;
    if (mag > 1e-6f) {
      var invMag = 1f / mag;
      edgeDx = -gy * invMag;
      edgeDy = gx * invMag;
    } else {
      edgeDx = 1f;
      edgeDy = 0f;
    }

    // Sample along edge direction with Gaussian weighting
    var sigma = softness * strokeSize;
    var twoSigmaSq = 2f * sigma * sigma;
    float sumR = 0, sumG = 0, sumB = 0, sumA = 0;
    var totalWeight = 0f;

    for (var i = -strokeSize; i <= strokeSize; ++i) {
      var sx = destX + (int)Math.Round(i * edgeDx);
      var sy = destY + (int)Math.Round(i * edgeDy);

      var weight = (float)Math.Exp(-(i * i) / twoSigmaSq);

      var px = frame[sx, sy].Work;
      var (pr, pg, pb, pa) = ColorConverter.GetNormalizedRgba(in px);
      sumR += pr * weight;
      sumG += pg * weight;
      sumB += pb * weight;
      sumA += pa * weight;
      totalWeight += weight;
    }

    var inv = 1f / totalWeight;
    var outR = Math.Max(0f, Math.Min(1f, sumR * inv));
    var outG = Math.Max(0f, Math.Min(1f, sumG * inv));
    var outB = Math.Max(0f, Math.Min(1f, sumB * inv));
    var outA = Math.Max(0f, Math.Min(1f, sumA * inv));

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, outA));
  }
}
