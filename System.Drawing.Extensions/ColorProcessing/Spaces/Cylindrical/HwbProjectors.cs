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
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Cylindrical;

/// <summary>
/// Projects LinearRgbF to HwbF.
/// </summary>
public readonly struct LinearRgbFToHwbF : IProject<LinearRgbF, HwbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HwbF Project(in LinearRgbF work) {
    var r = work.R;
    var g = work.G;
    var b = work.B;

    var max = r > g ? (r > b ? r : b) : (g > b ? g : b);
    var min = r < g ? (r < b ? r : b) : (g < b ? g : b);
    var delta = max - min;

    var w = min;
    var bl = 1f - max;

    if (delta < 1e-6f)
      return new(0f, w, bl);

    float h;
    if (max == r)
      h = ((g - b) / delta + (g < b ? 6f : 0f)) / 6f;
    else if (max == g)
      h = ((b - r) / delta + 2f) / 6f;
    else
      h = ((r - g) / delta + 4f) / 6f;

    return new(h, w, bl);
  }
}

/// <summary>
/// Projects LinearRgbaF to HwbF.
/// </summary>
public readonly struct LinearRgbaFToHwbF : IProject<LinearRgbaF, HwbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HwbF Project(in LinearRgbaF work) {
    var r = work.R;
    var g = work.G;
    var b = work.B;

    var max = r > g ? (r > b ? r : b) : (g > b ? g : b);
    var min = r < g ? (r < b ? r : b) : (g < b ? g : b);
    var delta = max - min;

    var w = min;
    var bl = 1f - max;

    if (delta < 1e-6f)
      return new(0f, w, bl);

    float h;
    if (max == r)
      h = ((g - b) / delta + (g < b ? 6f : 0f)) / 6f;
    else if (max == g)
      h = ((b - r) / delta + 2f) / 6f;
    else
      h = ((r - g) / delta + 4f) / 6f;

    return new(h, w, bl);
  }
}

/// <summary>
/// Projects HwbF back to LinearRgbF.
/// </summary>
public readonly struct HwbFToLinearRgbF : IProject<HwbF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in HwbF hwb) {
    var h = hwb.H;
    var w = hwb.W;
    var b = hwb.B;

    var sum = w + b;
    if (sum >= 1f) {
      var gray = w / sum;
      return new(gray, gray, gray);
    }

    // HWB to HSV: V = 1 - B, S = 1 - W/V
    var v = 1f - b;
    var s = 1f - w / v;

    // HSV to RGB
    var h6 = h * 6f;
    var sector = (int)h6;
    var f = h6 - sector;

    var p = v * (1f - s);
    var q = v * (1f - s * f);
    var t = v * (1f - s * (1f - f));

    return (sector % 6) switch {
      0 => new(v, t, p),
      1 => new(q, v, p),
      2 => new(p, v, t),
      3 => new(p, q, v),
      4 => new(t, p, v),
      _ => new(v, p, q)
    };
  }
}
