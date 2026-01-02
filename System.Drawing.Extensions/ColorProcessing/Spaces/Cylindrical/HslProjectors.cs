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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Cylindrical;

/// <summary>
/// Projects LinearRgbF to HslF.
/// </summary>
public readonly struct LinearRgbFToHslF : IProject<LinearRgbF, HslF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HslF Project(in LinearRgbF work) {
    var r = work.R;
    var g = work.G;
    var b = work.B;

    var max = r > g ? (r > b ? r : b) : (g > b ? g : b);
    var min = r < g ? (r < b ? r : b) : (g < b ? g : b);
    var delta = max - min;
    var l = (max + min) * 0.5f;

    if (delta < 1e-6f)
      return new(0f, 0f, l);

    var s = l > 0.5f ? delta / (2f - max - min) : delta / (max + min);

    float h;
    if (max == r)
      h = ((g - b) / delta + (g < b ? 6f : 0f)) * ColorMatrices.Inv6;
    else if (max == g)
      h = ((b - r) / delta + 2f) * ColorMatrices.Inv6;
    else
      h = ((r - g) / delta + 4f) * ColorMatrices.Inv6;

    return new(h, s, l);
  }
}

/// <summary>
/// Projects LinearRgbaF to HslF.
/// </summary>
public readonly struct LinearRgbaFToHslF : IProject<LinearRgbaF, HslF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HslF Project(in LinearRgbaF work) {
    var r = work.R;
    var g = work.G;
    var b = work.B;

    var max = r > g ? (r > b ? r : b) : (g > b ? g : b);
    var min = r < g ? (r < b ? r : b) : (g < b ? g : b);
    var delta = max - min;
    var l = (max + min) * 0.5f;

    if (delta < 1e-6f)
      return new(0f, 0f, l);

    var s = l > 0.5f ? delta / (2f - max - min) : delta / (max + min);

    float h;
    if (max == r)
      h = ((g - b) / delta + (g < b ? 6f : 0f)) * ColorMatrices.Inv6;
    else if (max == g)
      h = ((b - r) / delta + 2f) * ColorMatrices.Inv6;
    else
      h = ((r - g) / delta + 4f) * ColorMatrices.Inv6;

    return new(h, s, l);
  }
}

/// <summary>
/// Projects HslF back to LinearRgbF.
/// </summary>
public readonly struct HslFToLinearRgbF : IProject<HslF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in HslF hsl) {
    var h = hsl.H;
    var s = hsl.S;
    var l = hsl.L;

    if (s < 1e-6f)
      return new(l, l, l);

    var q = l < 0.5f ? l * (1f + s) : l + s - l * s;
    var p = 2f * l - q;

    return new(
      _HueToRgb(p, q, h + 1f / 3f),
      _HueToRgb(p, q, h),
      _HueToRgb(p, q, h - 1f / 3f)
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _HueToRgb(float p, float q, float t) {
    if (t < 0f) t += 1f;
    if (t > 1f) t -= 1f;
    if (t < 1f / 6f) return p + (q - p) * 6f * t;
    if (t < 0.5f) return q;
    if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
    return p;
  }
}
