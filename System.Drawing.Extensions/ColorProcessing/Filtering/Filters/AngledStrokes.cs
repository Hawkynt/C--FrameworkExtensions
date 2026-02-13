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
/// Directional brush strokes at a configurable angle.
/// Averages pixels along a line through each pixel at the given angle, then blends with the original by sharpness.
/// Always uses frame-level random access.
/// </summary>
[FilterInfo("AngledStrokes",
  Description = "Directional brush strokes at configurable angle", Category = FilterCategory.Artistic)]
public readonly struct AngledStrokes(float angle = 45f, int strokeLength = 15, float sharpness = 0.5f) : IPixelFilter, IFrameFilter {
  private readonly float _angle = angle;
  private readonly int _strokeLength = Math.Max(1, strokeLength);
  private readonly float _sharpness = Math.Max(0f, Math.Min(1f, sharpness));

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
    => callback.Invoke(new AngledStrokesPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new AngledStrokesFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._angle, this._strokeLength, this._sharpness, sourceWidth, sourceHeight));

  public static AngledStrokes Default => new(45f, 15, 0.5f);
}

file readonly struct AngledStrokesPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct AngledStrokesFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float angle, int strokeLength, float sharpness, int sourceWidth, int sourceHeight)
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
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var px = frame[destX, destY].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);

    var rad = angle * (float)(Math.PI / 180.0);
    var cos = (float)Math.Cos(rad);
    var sin = (float)Math.Sin(rad);

    var half = strokeLength / 2;
    var sumR = 0f;
    var sumG = 0f;
    var sumB = 0f;
    var count = 0;

    for (var i = -half; i <= half; ++i) {
      var sx = (int)(destX + i * cos);
      var sy = (int)(destY + i * sin);
      var sp = frame[sx, sy].Work;
      var (sr, sg, sb, _) = ColorConverter.GetNormalizedRgba(in sp);
      sumR += sr;
      sumG += sg;
      sumB += sb;
      ++count;
    }

    var inv = 1f / count;
    var avgR = sumR * inv;
    var avgG = sumG * inv;
    var avgB = sumB * inv;

    var outR = avgR * (1f - sharpness) + r * sharpness;
    var outG = avgG * (1f - sharpness) + g * sharpness;
    var outB = avgB * (1f - sharpness) + b * sharpness;

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, a));
  }
}
