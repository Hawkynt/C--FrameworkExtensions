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
/// Soft glow effect using screen blend of blurred highlights.
/// Computes a local average, extracts bright areas, and screen-blends them back.
/// </summary>
[FilterInfo("SoftGlow",
  Description = "Soft glow effect using screen blend of blurred highlights", Category = FilterCategory.Artistic)]
public readonly struct SoftGlow(float glowRadius = 3f, float glowBrightness = 0.5f, float sharpness = 0.5f) : IPixelFilter, IFrameFilter {

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
    => throw new NotSupportedException("SoftGlow requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new SoftGlowFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      glowRadius, glowBrightness, sharpness, sourceWidth, sourceHeight));

  public static SoftGlow Default => new(3f, 0.5f, 0.5f);
}

file readonly struct SoftGlowFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float glowRadius, float glowBrightness, float sharpness, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, (int)Math.Ceiling(glowRadius));
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
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);

    // Compute local average (blur) within glowRadius
    var rad = Math.Max(1, (int)Math.Ceiling(glowRadius));
    var sumR = 0f;
    var sumG = 0f;
    var sumB = 0f;
    var count = 0;

    for (var dy = -rad; dy <= rad; ++dy)
      for (var dx = -rad; dx <= rad; ++dx) {
        var px = frame[destX + dx, destY + dy].Work;
        var (nr, ng, nb, _) = ColorConverter.GetNormalizedRgba(in px);
        sumR += nr;
        sumG += ng;
        sumB += nb;
        ++count;
      }

    var avgR = sumR / count;
    var avgG = sumG / count;
    var avgB = sumB / count;

    // Luminance of average
    var blurLum = ColorConverter.LuminanceFromRgb(avgR, avgG, avgB);

    // Screen blend only if blur is bright enough
    float outR, outG, outB;
    if (blurLum > 0.5f) {
      var glowR = avgR * glowBrightness;
      var glowG = avgG * glowBrightness;
      var glowB = avgB * glowBrightness;

      // Screen blend: 1 - (1 - c) * (1 - glow)
      outR = 1f - (1f - cr) * (1f - glowR);
      outG = 1f - (1f - cg) * (1f - glowG);
      outB = 1f - (1f - cb) * (1f - glowB);
    } else {
      outR = cr;
      outG = cg;
      outB = cb;
    }

    // Blend with original by sharpness
    outR = outR * (1f - sharpness) + cr * sharpness;
    outG = outG * (1f - sharpness) + cg * sharpness;
    outB = outB * (1f - sharpness) + cb * sharpness;

    outR = ColorConverter.Saturate(outR);
    outG = ColorConverter.Saturate(outG);
    outB = ColorConverter.Saturate(outB);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, ca));
  }
}
