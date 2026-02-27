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
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Spin blur (rotational blur) around a configurable center point.
/// Samples along a circular arc at the same radius from center and averages their colors.
/// Always uses frame-level random access for angular sampling.
/// </summary>
[FilterInfo("SpinBlur",
  Description = "Rotational spin blur around a configurable center point", Category = FilterCategory.Enhancement)]
public readonly struct SpinBlur(float angleDegrees, float centerX, float centerY) : IPixelFilter, IFrameFilter {
  private readonly float _angleDegrees = Math.Max(0f, angleDegrees);
  private readonly float _centerX = centerX;
  private readonly float _centerY = centerY;

  public SpinBlur() : this(5f, 0.5f, 0.5f) { }

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
    => callback.Invoke(new SpinBlurPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new SpinBlurFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._angleDegrees, this._centerX, this._centerY, sourceWidth, sourceHeight));

  public static SpinBlur Default => new();
}

file readonly struct SpinBlurPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct SpinBlurFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float angleDegrees, float centerX, float centerY, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private const int Samples = 10;
  private static readonly float _Deg2Rad = (float)(Math.PI / 180.0);

  public int Radius => (int)Math.Ceiling(angleDegrees) + 1;
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
    var cx = centerX * sourceWidth;
    var cy = centerY * sourceHeight;
    var dx = destX - cx;
    var dy = destY - cy;
    var pixelRadius = (float)Math.Sqrt(dx * dx + dy * dy);

    if (pixelRadius < 0.001f) {
      var px = frame[destX, destY].Work;
      var (pr, pg, pb, pa) = ColorConverter.GetNormalizedRgba(in px);
      dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(pr, pg, pb, pa));
      return;
    }

    var baseAngle = (float)Math.Atan2(dy, dx);
    var halfSpan = angleDegrees * 0.5f * _Deg2Rad;
    var inv = 1f / Samples;

    float ar = 0, ag = 0, ab = 0;

    for (var i = 0; i < Samples; ++i) {
      var t = (i - Samples * 0.5f) / Samples;
      var sampleAngle = baseAngle + t * 2f * halfSpan;
      var sx = (int)Math.Round(cx + pixelRadius * Math.Cos(sampleAngle));
      var sy = (int)Math.Round(cy + pixelRadius * Math.Sin(sampleAngle));
      var px = frame[sx, sy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      ar += r;
      ag += g;
      ab += b;
    }

    ar *= inv;
    ag *= inv;
    ab *= inv;

    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(ar, ag, ab, ca));
  }
}
