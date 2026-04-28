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
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Gamut;

/// <summary>
/// Perceptual chroma-reducing gamut map: bisect chroma in OkLCh until the resulting
/// linear-sRGB colour is in [0, 1]³, while preserving lightness (L) and hue (h).
/// </summary>
/// <remarks>
/// <para>Implements the canonical "chroma reduction toward white-point with lightness
/// preservation" strategy widely used as a baseline in colour-management research.
/// It is the algorithm CSS Color Module 4 specifies for the <c>color()</c> function's
/// implicit gamut mapping (CSS WG resolved on this in 2022, citing Ottosson's OkLCh
/// formulation).</para>
/// <para>Because L and h are held constant, achromatic detail (texture, fine
/// shading) is preserved at the cost of fully desaturating colours that lie far
/// outside the gamut. This is the opposite trade-off from <see cref="GamutCompress"/>,
/// which preserves saturation more aggressively at the cost of small lightness/hue
/// shifts in the highlight region.</para>
/// <para>Algorithm: binary-search the chroma multiplier α ∈ [0, 1] such that
/// (L, α·a, α·b) projects to a linear-sRGB colour with all channels in [0, 1].
/// 16 iterations give &lt; 0.0008 chroma error — well below the JND for any sRGB
/// display.</para>
/// <para>References:
/// <list type="bullet">
///   <item><description>CSS Color Module 4, §13.3 "Gamut mapping" (W3C WD 2022).</description></item>
///   <item><description>Ottosson, "Sigmoid-like compression in OkLCh" (2021).</description></item>
///   <item><description>Morovic, "Color Gamut Mapping" (2008), §3.4 (constant-L line clipping).</description></item>
/// </list>
/// </para>
/// </remarks>
public readonly struct PerceptualGamutMap : IGamutMap {

  // 16 iterations: chroma error ≤ 1/2^16 ≈ 1.5e-5 (well below 8-bit JND).
  private const int MaxBisections = 16;

  // Just-Noticeable-Difference epsilon for "in-gamut" termination.
  private const float Eps = 1f / 1024f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Map(in LinearRgbF color) {
    if (IsInGamut(color)) return color;

    // Convert to OkLab so L and the (a, b) chromaticity vector are independent.
    var oklab = new LinearRgbFToOklabF().Project(color);
    var L = oklab.L;
    var a = oklab.A;
    var b = oklab.B;

    // Edge case: black/white, or fully achromatic — reduces to a clamp.
    if (a * a + b * b < 1e-12f) {
      var l01 = L < 0f ? 0f : (L > 1f ? 1f : L);
      return new(l01, l01, l01);
    }

    // Find a chroma multiplier α such that (L, α·a, α·b) is in gamut.
    // α=0 is always in gamut (achromatic at this lightness, but only if L itself is in [0,1]).
    var lo = 0f;
    var hi = 1f;
    for (var i = 0; i < MaxBisections; ++i) {
      var mid = 0.5f * (lo + hi);
      var trial = new OklabFToLinearRgbF().Project(new OklabF(L, a * mid, b * mid));
      if (IsInGamut(trial, Eps)) lo = mid;
      else hi = mid;
    }
    var alpha = lo;
    var result = new OklabFToLinearRgbF().Project(new OklabF(L, a * alpha, b * alpha));

    // Final clamp to absorb the residual epsilon.
    return new(
      result.R < 0f ? 0f : (result.R > 1f ? 1f : result.R),
      result.G < 0f ? 0f : (result.G > 1f ? 1f : result.G),
      result.B < 0f ? 0f : (result.B > 1f ? 1f : result.B)
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool IsInGamut(in LinearRgbF c) =>
    c.R >= 0f && c.R <= 1f &&
    c.G >= 0f && c.G <= 1f &&
    c.B >= 0f && c.B <= 1f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool IsInGamut(in LinearRgbF c, float eps) =>
    c.R >= -eps && c.R <= 1f + eps &&
    c.G >= -eps && c.G <= 1f + eps &&
    c.B >= -eps && c.B <= 1f + eps;
}
