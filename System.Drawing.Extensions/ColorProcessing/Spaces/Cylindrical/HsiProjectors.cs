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
/// Projects LinearRgbF to HsiF (Gonzalez &amp; Woods, "Digital Image Processing", 1992).
/// </summary>
/// <remarks>
/// <para>I = (R+G+B)/3,  S = 1 − min(R,G,B)/I,  H from the standard hexagon hue formula
/// using arccosine of the chromaticity numerator/denominator (computed in the YUV-style
/// dot-product form to avoid the explicit acos branch).</para>
/// </remarks>
public readonly struct LinearRgbFToHsiF : IProject<LinearRgbF, HsiF> {

  private const float TwoPi = 6.2831853071795864769f;
  private const float InvTwoPi = 1f / TwoPi;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HsiF Project(in LinearRgbF work) => Compute(work.R, work.G, work.B);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static HsiF Compute(float r, float g, float b) {
    var i = (r + g + b) * (1f / 3f);
    if (i < 1e-6f) return new(0f, 0f, 0f);

    var min = r < g ? (r < b ? r : b) : (g < b ? g : b);
    var s = 1f - min / i;

    // Standard arccos hue formula.
    var num = 0.5f * ((r - g) + (r - b));
    var denom = MathF.Sqrt((r - g) * (r - g) + (r - b) * (g - b));
    if (denom < 1e-9f) return new(0f, s, i);

    var ratio = num / denom;
    if (ratio > 1f) ratio = 1f;
    else if (ratio < -1f) ratio = -1f;
    var theta = MathF.Acos(ratio);
    var hRad = b > g ? TwoPi - theta : theta;
    return new(hRad * InvTwoPi, s, i);
  }
}

/// <summary>
/// Projects LinearRgbaF to HsiF.
/// </summary>
public readonly struct LinearRgbaFToHsiF : IProject<LinearRgbaF, HsiF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HsiF Project(in LinearRgbaF work) => LinearRgbFToHsiF.Compute(work.R, work.G, work.B);
}

/// <summary>
/// Projects HsiF back to LinearRgbF.
/// </summary>
public readonly struct HsiFToLinearRgbF : IProject<HsiF, LinearRgbF> {

  private const float TwoPi = 6.2831853071795864769f;
  private const float ThirdPi = TwoPi / 6f;
  private const float TwoThirdsPi = 2f * TwoPi / 6f;
  private const float FourThirdsPi = 4f * TwoPi / 6f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in HsiF hsi) {
    var h = hsi.H * TwoPi;
    var s = hsi.S;
    var i = hsi.I;
    if (s < 1e-6f) return new(i, i, i);

    float r, g, b;
    if (h < TwoThirdsPi) {
      b = i * (1f - s);
      r = i * (1f + s * MathF.Cos(h) / MathF.Cos(ThirdPi - h));
      g = 3f * i - (r + b);
    } else if (h < FourThirdsPi) {
      var hh = h - TwoThirdsPi;
      r = i * (1f - s);
      g = i * (1f + s * MathF.Cos(hh) / MathF.Cos(ThirdPi - hh));
      b = 3f * i - (r + g);
    } else {
      var hh = h - FourThirdsPi;
      g = i * (1f - s);
      b = i * (1f + s * MathF.Cos(hh) / MathF.Cos(ThirdPi - hh));
      r = 3f * i - (g + b);
    }
    return new(r, g, b);
  }
}
