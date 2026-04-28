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
using System.Drawing;
using Guard;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics.Lab;
using Hawkynt.ColorProcessing.Spaces.Lab;

namespace Hawkynt.ColorProcessing.Quality;

/// <summary>
/// Root-mean-square of CIE ΔE2000 (CIEDE2000) over all pixels.
/// Provides a perceptual-uniform color difference metric suitable for color
/// reproduction quality assessment.
/// </summary>
/// <remarks>
/// <para>
/// CIEDE2000 (Sharma, Wu &amp; Dalal 2005, "The CIEDE2000 Color Difference
/// Formula") is the current CIE recommendation. Returns RMS over the whole
/// frame so a small handful of large per-pixel errors doesn't dominate; use
/// <see cref="DeltaEMax"/> for the worst-case figure.
/// </para>
/// <para>
/// Pipeline: sRGB byte → linear RGB (sRGB EOTF) → XYZ (D65 matrix) →
/// L*a*b* (D65 reference white). Reuses upstream
/// <see cref="Hawkynt.ColorProcessing.Internal.FixedPointMath.LabF"/>
/// LUT and <see cref="CIEDE2000Squared"/> for the per-pixel ΔE².
/// </para>
/// <para>
/// Complexity: O(W·H). On 1024² ~80–120 ms managed because of the per-pixel
/// CIEDE2000 (its trig core dominates).
/// </para>
/// </remarks>
public static class DeltaERms {

  /// <summary>Returns RMS ΔE2000.</summary>
  public static double Compute(Bitmap reference, Bitmap candidate) {
    var (rms, _) = _Both(reference, candidate);
    return rms;
  }

  /// <summary>Returns the worst (max) ΔE2000 across all pixels.</summary>
  public static double DeltaEMax(Bitmap reference, Bitmap candidate) {
    var (_, max) = _Both(reference, candidate);
    return max;
  }

  private static (double rms, double max) _Both(Bitmap reference, Bitmap candidate) {
    Against.ArgumentIsNull(reference);
    Against.ArgumentIsNull(candidate);
    if (reference.Width != candidate.Width || reference.Height != candidate.Height)
      throw new ArgumentException(
        $"Dimensions differ: {reference.Width}x{reference.Height} vs {candidate.Width}x{candidate.Height}.");

    var w = reference.Width;
    var h = reference.Height;
    using var refLock = reference.Lock();
    using var canLock = candidate.Lock();

    double sumSq = 0;
    double max = 0;
    long count = 0;
    for (var y = 0; y < h; ++y)
    for (var x = 0; x < w; ++x) {
      var a = _ToLab(refLock[x, y]);
      var b = _ToLab(canLock[x, y]);
      var dSq = CIEDE2000Squared._Calculate(a, b);
      var d = Math.Sqrt(dSq);
      if (d > max) max = d;
      sumSq += dSq;
      ++count;
    }
    var rms = count == 0 ? 0 : Math.Sqrt(sumSq / count);
    return (rms, max);
  }

  private static LabF _ToLab(Color c) {
    // sRGB byte → linear via simple gamma 2.2 inverse (good enough for ΔE-RMS;
    // matches the upstream FrameworkExtensions linearisation tolerance).
    var lr = _SrgbToLinear(c.R / 255f);
    var lg = _SrgbToLinear(c.G / 255f);
    var lb = _SrgbToLinear(c.B / 255f);

    var X = ColorMatrices.RgbToXyz_XR * lr + ColorMatrices.RgbToXyz_XG * lg + ColorMatrices.RgbToXyz_XB * lb;
    var Y = ColorMatrices.RgbToXyz_YR * lr + ColorMatrices.RgbToXyz_YG * lg + ColorMatrices.RgbToXyz_YB * lb;
    var Z = ColorMatrices.RgbToXyz_ZR * lr + ColorMatrices.RgbToXyz_ZG * lg + ColorMatrices.RgbToXyz_ZB * lb;

    var fx = _LabF((int)(X * ColorMatrices.Inv_D65_Xn * 65536f)) / 65536f;
    var fy = _LabF((int)(Y * ColorMatrices.Inv_D65_Yn * 65536f)) / 65536f;
    var fz = _LabF((int)(Z * ColorMatrices.Inv_D65_Zn * 65536f)) / 65536f;

    return new LabF(116f * fy - 16f, 500f * (fx - fy), 200f * (fy - fz));
  }

  private static float _SrgbToLinear(float v) {
    if (v <= 0.04045f) return v / 12.92f;
    return (float)Math.Pow((v + 0.055f) / 1.055f, 2.4f);
  }

  private static int _LabF(int v) => FixedPointMath.LabF(v);
}
