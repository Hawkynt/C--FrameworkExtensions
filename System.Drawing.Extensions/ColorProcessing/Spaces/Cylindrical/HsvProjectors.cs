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
/// Projects LinearRgbF to HsvF.
/// </summary>
public readonly struct LinearRgbFToHsvF : IProject<LinearRgbF, HsvF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HsvF Project(in LinearRgbF work) {
    var r = work.R;
    var g = work.G;
    var b = work.B;

    var max = r > g ? (r > b ? r : b) : (g > b ? g : b);
    var min = r < g ? (r < b ? r : b) : (g < b ? g : b);
    var delta = max - min;
    var v = max;

    if (max < 1e-6f)
      return new(0f, 0f, v);

    var s = delta / max;

    if (delta < 1e-6f)
      return new(0f, s, v);

    float h;
    if (max == r)
      h = ((g - b) / delta + (g < b ? 6f : 0f)) * ColorMatrices.Inv6;
    else if (max == g)
      h = ((b - r) / delta + 2f) * ColorMatrices.Inv6;
    else
      h = ((r - g) / delta + 4f) * ColorMatrices.Inv6;

    return new(h, s, v);
  }
}

/// <summary>
/// Projects LinearRgbaF to HsvF.
/// </summary>
public readonly struct LinearRgbaFToHsvF : IProject<LinearRgbaF, HsvF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HsvF Project(in LinearRgbaF work) {
    var r = work.R;
    var g = work.G;
    var b = work.B;

    var max = r > g ? (r > b ? r : b) : (g > b ? g : b);
    var min = r < g ? (r < b ? r : b) : (g < b ? g : b);
    var delta = max - min;
    var v = max;

    if (max < 1e-6f)
      return new(0f, 0f, v);

    var s = delta / max;

    if (delta < 1e-6f)
      return new(0f, s, v);

    float h;
    if (max == r)
      h = ((g - b) / delta + (g < b ? 6f : 0f)) * ColorMatrices.Inv6;
    else if (max == g)
      h = ((b - r) / delta + 2f) * ColorMatrices.Inv6;
    else
      h = ((r - g) / delta + 4f) * ColorMatrices.Inv6;

    return new(h, s, v);
  }
}

/// <summary>
/// Projects HsvF back to LinearRgbF.
/// </summary>
public readonly struct HsvFToLinearRgbF : IProject<HsvF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in HsvF hsv) {
    var h = hsv.H;
    var s = hsv.S;
    var v = hsv.V;

    if (s < 1e-6f)
      return new(v, v, v);

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
