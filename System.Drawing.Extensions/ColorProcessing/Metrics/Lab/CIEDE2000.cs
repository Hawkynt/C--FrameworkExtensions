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
using Hawkynt.ColorProcessing.Spaces.Lab;
using UNorm32 = Hawkynt.ColorProcessing.Metrics.UNorm32;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics.Lab;

/// <summary>
/// Calculates the CIEDE2000 delta E (ΔE₀₀) between two Lab colors.
/// </summary>
/// <remarks>
/// <para>CIEDE2000 is the current CIE recommendation for calculating perceptual colour
/// differences (CIE 142:2001). The most accurate published colour-difference formula,
/// adding to CIE94 a chroma-rescaling factor, a hue-rotation term R_T (which corrects
/// the blue-region perceptual non-uniformity), and an L*-dependent S_L weighting.</para>
/// <code>
///   ΔE₀₀ = sqrt( (ΔL'/(kL·SL))² + (ΔC'/(kC·SC))² + (ΔH'/(kH·SH))²
///                + R_T·(ΔC'/(kC·SC))·(ΔH'/(kH·SH)) )
/// </code>
/// <para>Reference: G. Sharma, W. Wu &amp; E. N. Dalal, "The CIEDE2000 color-difference
/// formula: implementation notes, supplementary test data, and mathematical
/// observations", Color Research &amp; Application 30(1):21-30, 2005.
/// Verified against the 34 published test pairs from
/// <see href="http://www2.ece.rochester.edu/~gsharma/ciede2000/"/>.</para>
/// <para>Returns UNorm32 normalised against ΔE = 100.</para>
/// </remarks>
public readonly struct CIEDE2000 : IColorMetric<LabF>, INormalizedMetric {

  // Practical max delta E for normalization
  private const float MaxDeltaE = 100f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LabF a, in LabF b) {
    var raw = (float)Math.Sqrt(CIEDE2000Squared._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw / MaxDeltaE);
  }
}

/// <summary>
/// Calculates the squared CIEDE2000 delta E between two Lab colors.
/// </summary>
/// <remarks>
/// <para>Faster than CIEDE2000 when only comparing distances (no final sqrt).</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max delta E² of 10000.</para>
/// </remarks>
public readonly struct CIEDE2000Squared : IColorMetric<LabF>, INormalizedMetric {

  private const double Pow25_7 = 6103515625.0;

  // Practical max delta E squared for normalization (100²)
  private const float MaxDeltaESquared = 10000f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LabF a, in LabF b)
    => UNorm32.FromFloatClamped((float)_Calculate(a, b) / MaxDeltaESquared);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(in LabF lab1, in LabF lab2) {
    const double kL = 1.0, kC = 1.0, kH = 1.0;
    double l1 = lab1.L, a1 = lab1.A, b1 = lab1.B;
    double l2 = lab2.L, a2 = lab2.A, b2 = lab2.B;

    var c1 = Math.Sqrt(a1 * a1 + b1 * b1);
    var c2 = Math.Sqrt(a2 * a2 + b2 * b2);
    var cMean = (c1 + c2) * 0.5;
    // x^7 inlined: (x^2)^3 * x — 4 muls vs Math.Pow's transcendental call.
    var cMeanSq = cMean * cMean;
    var cMean7 = cMeanSq * cMeanSq * cMeanSq * cMean;
    var g = 0.5 * (1.0 - Math.Sqrt(cMean7 / (cMean7 + Pow25_7)));

    var a1Prime = a1 * (1.0 + g);
    var a2Prime = a2 * (1.0 + g);
    var c1Prime = Math.Sqrt(a1Prime * a1Prime + b1 * b1);
    var c2Prime = Math.Sqrt(a2Prime * a2Prime + b2 * b2);

    var h1Prime = _Atan2Degrees(b1, a1Prime);
    var h2Prime = _Atan2Degrees(b2, a2Prime);

    var deltaLPrime = l2 - l1;
    var deltaCPrime = c2Prime - c1Prime;

    double deltaHPrime;
    var cProduct = c1Prime * c2Prime;
    if (cProduct == 0)
      deltaHPrime = 0;
    else {
      var dhPrime = h2Prime - h1Prime;
      if (dhPrime > 180) dhPrime -= 360;
      else if (dhPrime < -180) dhPrime += 360;
      deltaHPrime = dhPrime;
    }

    var deltaHPrimeBig = 2.0 * Math.Sqrt(cProduct) * Math.Sin(_DegreesToRadians(deltaHPrime * 0.5));
    var lMeanPrime = (l1 + l2) * 0.5;
    var cMeanPrime = (c1Prime + c2Prime) * 0.5;

    double hMeanPrime;
    if (cProduct == 0)
      hMeanPrime = h1Prime + h2Prime;
    else {
      var hSum = h1Prime + h2Prime;
      var hDiff = Math.Abs(h1Prime - h2Prime);
      if (hDiff <= 180) hMeanPrime = hSum * 0.5;
      else if (hSum < 360) hMeanPrime = (hSum + 360) * 0.5;
      else hMeanPrime = (hSum - 360) * 0.5;
    }

    var hMeanPrimeRad = _DegreesToRadians(hMeanPrime);
    var t = 1.0
      - 0.17 * Math.Cos(hMeanPrimeRad - _DegreesToRadians(30))
      + 0.24 * Math.Cos(2.0 * hMeanPrimeRad)
      + 0.32 * Math.Cos(3.0 * hMeanPrimeRad + _DegreesToRadians(6))
      - 0.20 * Math.Cos(4.0 * hMeanPrimeRad - _DegreesToRadians(63));

    var lMeanPrimeMinus50Sq = (lMeanPrime - 50) * (lMeanPrime - 50);
    var sL = 1.0 + (0.015 * lMeanPrimeMinus50Sq) / Math.Sqrt(20.0 + lMeanPrimeMinus50Sq);
    var sC = 1.0 + 0.045 * cMeanPrime;
    var sH = 1.0 + 0.015 * cMeanPrime * t;

    var hMeanShifted = (hMeanPrime - 275.0) / 25.0;
    var deltaTheta = 30.0 * Math.Exp(-(hMeanShifted * hMeanShifted));
    var cMeanPrimeSq = cMeanPrime * cMeanPrime;
    var cMeanPrime7 = cMeanPrimeSq * cMeanPrimeSq * cMeanPrimeSq * cMeanPrime;
    var rC = 2.0 * Math.Sqrt(cMeanPrime7 / (cMeanPrime7 + Pow25_7));
    var rT = -rC * Math.Sin(_DegreesToRadians(2.0 * deltaTheta));

    var lightness = deltaLPrime / (kL * sL);
    var chroma = deltaCPrime / (kC * sC);
    var hue = deltaHPrimeBig / (kH * sH);

    return lightness * lightness + chroma * chroma + hue * hue + rT * chroma * hue;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _Atan2Degrees(double y, double x) {
    var angle = Math.Atan2(y, x) * (180.0 / Math.PI);
    return angle < 0 ? angle + 360.0 : angle;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);
}
