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
/// Smudge stick painting with highlight brightening.
/// Averages neighborhood pixels and brightens areas where the center luminance exceeds the highlight threshold.
/// Always uses frame-level random access.
/// </summary>
[FilterInfo("SmudgeStick",
  Description = "Smudge stick painting with highlight brightening", Category = FilterCategory.Artistic)]
public readonly struct SmudgeStick(int strokeLength = 2, float highlightArea = 0.5f, float intensity = 0.5f) : IPixelFilter, IFrameFilter {
  private readonly int _strokeLength = Math.Max(1, strokeLength);
  private readonly float _highlightArea = Math.Max(0f, Math.Min(1f, highlightArea));
  private readonly float _intensity = Math.Max(0f, Math.Min(1f, intensity));

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
    => callback.Invoke(new SmudgeStickPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new SmudgeStickFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._strokeLength, this._highlightArea, this._intensity, sourceWidth, sourceHeight));

  public static SmudgeStick Default => new(2, 0.5f, 0.5f);
}

file readonly struct SmudgeStickPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct SmudgeStickFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int strokeLength, float highlightArea, float intensity, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, strokeLength);
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
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, a) = ColorConverter.GetNormalizedRgba(in center);
    var lum = ColorMatrices.BT601_R * cr + ColorMatrices.BT601_G * cg + ColorMatrices.BT601_B * cb;

    var sumR = 0f;
    var sumG = 0f;
    var sumB = 0f;
    var count = 0;

    for (var dy = -strokeLength; dy <= strokeLength; ++dy)
    for (var dx = -strokeLength; dx <= strokeLength; ++dx) {
      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      sumR += r;
      sumG += g;
      sumB += b;
      ++count;
    }

    var inv = 1f / count;
    var avgR = sumR * inv;
    var avgG = sumG * inv;
    var avgB = sumB * inv;

    float outR, outG, outB;
    if (lum > highlightArea) {
      var factor = intensity * (lum - highlightArea) / (1f - highlightArea + 0.001f);
      outR = avgR + (1f - avgR) * factor;
      outG = avgG + (1f - avgG) * factor;
      outB = avgB + (1f - avgB) * factor;
    } else {
      outR = avgR;
      outG = avgG;
      outB = avgB;
    }

    outR = Math.Max(0f, Math.Min(1f, outR));
    outG = Math.Max(0f, Math.Min(1f, outG));
    outB = Math.Max(0f, Math.Min(1f, outB));

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, a));
  }
}
