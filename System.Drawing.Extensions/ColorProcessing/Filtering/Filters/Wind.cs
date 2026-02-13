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
/// Wind distortion — creates directional pixel streaks at bright edges simulating wind blur.
/// </summary>
[FilterInfo("Wind",
  Description = "Directional pixel streaks at bright edges simulating wind blur", Category = FilterCategory.Distortion)]
public readonly struct Wind(int strength, float angle = 0f) : IPixelFilter, IFrameFilter {
  private readonly int _strength = Math.Max(1, strength);
  private readonly float _angle = angle;

  public Wind() : this(10, 0f) { }

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
    => callback.Invoke(new WindPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new WindFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._strength, this._angle, sourceWidth, sourceHeight));

  public static Wind Default => new();
}

file readonly struct WindPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct WindFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int strength, float angle, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => strength;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum(float r, float g, float b)
    => ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var rad = angle * (float)(Math.PI / 180.0);
    var windDx = (float)Math.Cos(rad);
    var windDy = (float)Math.Sin(rad);

    // Current pixel
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);
    var centerLum = _Lum(cr, cg, cb);

    // Check luminance gradient in wind direction
    var upwind = frame[destX - (int)windDx, destY - (int)windDy].Work;
    var (ur, ug, ub, _) = ColorConverter.GetNormalizedRgba(in upwind);
    var upwindLum = _Lum(ur, ug, ub);
    var gradient = centerLum - upwindLum;

    // Only streak if there is a bright edge (gradient above threshold)
    if (gradient > 0.1f) {
      // Average along wind direction for streak
      var ar = cr;
      var ag = cg;
      var ab = cb;
      var count = 1;
      var streakLen = (int)(gradient * strength);

      for (var i = 1; i <= streakLen; ++i) {
        var sx = (int)(destX - windDx * i);
        var sy = (int)(destY - windDy * i);
        var px = frame[sx, sy].Work;
        var (pr, pg, pb, _) = ColorConverter.GetNormalizedRgba(in px);
        ar += pr;
        ag += pg;
        ab += pb;
        ++count;
      }

      var inv = 1f / count;
      cr = ar * inv;
      cg = ag * inv;
      cb = ab * inv;
    }

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(cr, cg, cb, ca));
  }
}
