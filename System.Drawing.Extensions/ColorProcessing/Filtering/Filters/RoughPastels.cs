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
/// Rough pastel strokes with canvas texture — averages pixels along a
/// horizontal stroke direction, adds procedural canvas texture noise,
/// and applies a detail contrast factor for a hand-drawn pastel appearance.
/// </summary>
[FilterInfo("RoughPastels",
  Description = "Rough pastel strokes with canvas texture", Category = FilterCategory.Artistic)]
public readonly struct RoughPastels(int strokeLength = 6, float detail = 4f, float textureAmount = 0.5f, int seed = 0) : IPixelFilter, IFrameFilter {

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
    => throw new NotSupportedException("RoughPastels requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new RoughPastelsFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      strokeLength, detail, textureAmount, seed, sourceWidth, sourceHeight));

  public static RoughPastels Default => new(6, 4f, 0.5f, 0);
}

file readonly struct RoughPastelsFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int strokeLength, float detail, float textureAmount, int seed, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, strokeLength / 2);
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
    var half = strokeLength / 2;
    float ar = 0f, ag = 0f, ab = 0f, aa = 0f;
    var count = 0;

    for (var i = -half; i <= half; ++i) {
      var px = frame[destX + i, destY].Work;
      var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);
      ar += r;
      ag += g;
      ab += b;
      aa += a;
      ++count;
    }

    var inv = 1f / count;
    var avgR = ar * inv;
    var avgG = ag * inv;
    var avgB = ab * inv;
    var avgA = aa * inv;

    // Canvas texture
    var tex = _Hash(destX, destY, seed) * textureAmount * 0.15f;
    avgR += tex;
    avgG += tex;
    avgB += tex;

    // Detail contrast factor — pivot on perceptual mid-grey (see Contrast.cs).
    var detailFactor = detail / 4f;
    var pivot = typeof(TWork) == typeof(Bgra8888) ? 0.5f : 0.21404114f;
    var outR = ColorConverter.Saturate(pivot + (avgR - pivot) * detailFactor);
    var outG = ColorConverter.Saturate(pivot + (avgG - pivot) * detailFactor);
    var outB = ColorConverter.Saturate(pivot + (avgB - pivot) * detailFactor);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, avgA));
  }
}
