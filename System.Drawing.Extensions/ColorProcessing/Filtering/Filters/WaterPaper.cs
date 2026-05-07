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
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Water paper texture effect combining horizontal directional blur with fiber grain.
/// Produces a grayscale output with adjustable brightness and contrast.
/// </summary>
[FilterInfo("WaterPaper",
  Description = "Water paper texture with directional blur and fibers", Category = FilterCategory.Artistic)]
public readonly struct WaterPaper(int fiberLength = 15, float brightness = 60f, float contrast = 80f, int seed = 0) : IPixelFilter, IFrameFilter {

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
    => throw new NotSupportedException("WaterPaper requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new WaterPaperFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      fiberLength, brightness, contrast, seed, sourceWidth, sourceHeight));

  public static WaterPaper Default => new(15, 60f, 80f, 0);
}

file readonly struct WaterPaperFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int fiberLength, float brightness, float contrast, int seed, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, fiberLength / 2);
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Hash(int x, int y, int s) {
    var h = (uint)(x * 374761393 + y * 668265263 + s * 1274126177);
    h = (h ^ (h >> 13)) * 1274126177;
    h ^= h >> 16;
    return (h & 0xFFFF) / 32768f - 1f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var center = frame[destX, destY].Work;
    var (_, _, _, a) = ColorConverter.GetNormalizedRgba(in center);

    // Horizontal directional blur
    var half = fiberLength / 2;
    var sumLum = 0f;
    var count = 0;

    for (var i = -half; i <= half; ++i) {
      var px = frame[destX + i, destY].Work;
      var (nr, ng, nb, _) = ColorConverter.GetNormalizedRgba(in px);
      sumLum += ColorConverter.LuminanceFromRgb(nr, ng, nb);
      ++count;
    }

    var avgLum = sumLum / count;

    // Fiber grain texture
    var fiber = _Hash(destX, destY, seed) * 0.1f;
    var v = avgLum + fiber;

    // Apply brightness/contrast — pivot on perceptual mid-grey (see Contrast.cs).
    var pivot = typeof(TWork) == typeof(Bgra8888) ? 0.5f : 0.21404114f;
    v = (v - pivot) * (contrast / 50f) + pivot + (brightness - 50f) / 100f;
    v = ColorConverter.Saturate(v);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(v, v, v, a));
  }
}
