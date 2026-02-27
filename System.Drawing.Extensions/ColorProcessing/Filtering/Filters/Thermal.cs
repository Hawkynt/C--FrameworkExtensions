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
/// False-color thermal/infrared mapping. Maps luminance to a cold-to-hot color ramp
/// (black → blue → red → yellow → white).
/// </summary>
[FilterInfo("Thermal",
  Description = "False-color thermal imaging with cold-to-hot color ramp", Category = FilterCategory.Artistic)]
public readonly struct Thermal(float intensity = 1f) : IPixelFilter {
  private readonly float _intensity = Math.Max(0f, Math.Min(1f, intensity));

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
    => callback.Invoke(new ThermalKernel<TWork, TKey, TPixel, TEncode>(this._intensity));

  public static Thermal Default => new();
}

file readonly struct ThermalKernel<TWork, TKey, TPixel, TEncode>(float intensity)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (float r, float g, float b) _MapThermal(float t) {
    // 5-stop ramp: black(0) → blue(0.25) → red(0.5) → yellow(0.75) → white(1.0)
    if (t < 0.25f) {
      var f = t * 4f;
      return (0f, 0f, f);
    }

    if (t < 0.5f) {
      var f = (t - 0.25f) * 4f;
      return (f, 0f, 1f - f);
    }

    if (t < 0.75f) {
      var f = (t - 0.5f) * 4f;
      return (1f, f, 0f);
    }

    {
      var f = (t - 0.75f) * 4f;
      return (1f, 1f, f);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in pixel);
    var lum = ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;

    var (tr, tg, tb) = _MapThermal(lum);
    var or = r + (tr - r) * intensity;
    var og = g + (tg - g) * intensity;
    var ob = b + (tb - b) * intensity;

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a));
  }
}
