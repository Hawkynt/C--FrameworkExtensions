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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering;

/// <summary>
/// Provides RGB↔HSL conversion utilities for per-pixel filter operations.
/// </summary>
/// <remarks>
/// <para>All values are normalized to [0,1]. Hue wraps at 1.0 (represents 360°).</para>
/// </remarks>
internal static class HslMath {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (float H, float S, float L) RgbToHsl(float r, float g, float b) {
    var max = Math.Max(r, Math.Max(g, b));
    var min = Math.Min(r, Math.Min(g, b));
    var l = (max + min) * 0.5f;

    if (max == min)
      return (0f, 0f, l);

    var d = max - min;
    var s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

    float h;
    if (max == r)
      h = (g - b) / d + (g < b ? 6f : 0f);
    else if (max == g)
      h = (b - r) / d + 2f;
    else
      h = (r - g) / d + 4f;

    h /= 6f;
    return (h, s, l);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (float R, float G, float B) HslToRgb(float h, float s, float l) {
    if (s == 0f)
      return (l, l, l);

    var q = l < 0.5f ? l * (1f + s) : l + s - l * s;
    var p = 2f * l - q;
    return (
      HueToRgb(p, q, h + 1f / 3f),
      HueToRgb(p, q, h),
      HueToRgb(p, q, h - 1f / 3f)
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float HueToRgb(float p, float q, float t) {
    if (t < 0f) ++t;
    if (t > 1f) --t;
    if (t < 1f / 6f) return p + (q - p) * 6f * t;
    if (t < 0.5f) return q;
    if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
    return p;
  }
}
