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
/// 3D relief effect using directional derivative of luminance with configurable light angle and depth.
/// </summary>
[FilterInfo("Relief",
  Description = "3D relief effect with directional light source", Category = FilterCategory.Artistic)]
public readonly struct Relief : IPixelFilter {
  private readonly float _dx;
  private readonly float _dy;
  private readonly float _depth;

  public Relief() : this(315f, 1f) { }

  public Relief(float angle = 315f, float depth = 1f) {
    var rad = angle * (float)(Math.PI / 180.0);
    this._dx = (float)Math.Cos(rad);
    this._dy = (float)Math.Sin(rad);
    this._depth = Math.Max(0f, Math.Min(5f, depth));
  }

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
    => callback.Invoke(new ReliefKernel<TWork, TKey, TPixel, TEncode>(this._dx, this._dy, this._depth));

  public static Relief Default => new();
}

file readonly struct ReliefKernel<TWork, TKey, TPixel, TEncode>(float dx, float dy, float depth)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum(in NeighborPixel<TWork, TKey> p) {
    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Compute gradient using Sobel-like approach
    var gx = -_Lum(window.M1M1) + _Lum(window.M1P1)
           - 2f * _Lum(window.P0M1) + 2f * _Lum(window.P0P1)
           - _Lum(window.P1M1) + _Lum(window.P1P1);
    var gy = -_Lum(window.M1M1) - 2f * _Lum(window.M1P0) - _Lum(window.M1P1)
           + _Lum(window.P1M1) + 2f * _Lum(window.P1P0) + _Lum(window.P1P1);

    var dot = gx * dx + gy * dy;
    var v = Math.Max(0f, Math.Min(1f, 0.5f + dot * depth));

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(v, v, v, ca));
  }
}
