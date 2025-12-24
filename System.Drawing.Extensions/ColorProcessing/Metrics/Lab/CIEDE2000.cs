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

using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Spaces.Lab;
using SysMath = System.Math;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics.Lab;

/// <summary>
/// Calculates the CIEDE2000 delta E (ΔE00) between two Lab colors.
/// </summary>
/// <remarks>
/// CIEDE2000 is the current CIE recommendation for calculating color differences.
/// It's the most accurate perceptual color difference formula, accounting for:
/// - Lightness (L')
/// - Chroma (C')
/// - Hue (H')
/// - Interaction between chroma and hue (rotation term)
/// </remarks>
public readonly struct CIEDE2000 : IColorMetric<LabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in LabF a, in LabF b) => (float)SysMath.Sqrt(CIEDE2000Squared._Calculate(a, b));
}

/// <summary>
/// Calculates the squared CIEDE2000 delta E between two Lab colors.
/// </summary>
/// <remarks>
/// Faster than CIEDE2000 when only comparing distances (no final sqrt).
/// </remarks>
public readonly struct CIEDE2000Squared : IColorMetric<LabF> {

  private const double Pow25_7 = 6103515625.0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in LabF a, in LabF b) => (float)_Calculate(a, b);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(in LabF lab1, in LabF lab2) {
    const double kL = 1.0, kC = 1.0, kH = 1.0;
    double l1 = lab1.L, a1 = lab1.A, b1 = lab1.B;
    double l2 = lab2.L, a2 = lab2.A, b2 = lab2.B;

    var c1 = SysMath.Sqrt(a1 * a1 + b1 * b1);
    var c2 = SysMath.Sqrt(a2 * a2 + b2 * b2);
    var cMean = (c1 + c2) / 2.0;
    var cMean7 = SysMath.Pow(cMean, 7);
    var g = 0.5 * (1.0 - SysMath.Sqrt(cMean7 / (cMean7 + Pow25_7)));

    var a1Prime = a1 * (1.0 + g);
    var a2Prime = a2 * (1.0 + g);
    var c1Prime = SysMath.Sqrt(a1Prime * a1Prime + b1 * b1);
    var c2Prime = SysMath.Sqrt(a2Prime * a2Prime + b2 * b2);

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

    var deltaHPrimeBig = 2.0 * SysMath.Sqrt(cProduct) * SysMath.Sin(_DegreesToRadians(deltaHPrime / 2.0));
    var lMeanPrime = (l1 + l2) / 2.0;
    var cMeanPrime = (c1Prime + c2Prime) / 2.0;

    double hMeanPrime;
    if (cProduct == 0)
      hMeanPrime = h1Prime + h2Prime;
    else {
      var hSum = h1Prime + h2Prime;
      var hDiff = SysMath.Abs(h1Prime - h2Prime);
      if (hDiff <= 180) hMeanPrime = hSum / 2.0;
      else if (hSum < 360) hMeanPrime = (hSum + 360) / 2.0;
      else hMeanPrime = (hSum - 360) / 2.0;
    }

    var hMeanPrimeRad = _DegreesToRadians(hMeanPrime);
    var t = 1.0
      - 0.17 * SysMath.Cos(hMeanPrimeRad - _DegreesToRadians(30))
      + 0.24 * SysMath.Cos(2.0 * hMeanPrimeRad)
      + 0.32 * SysMath.Cos(3.0 * hMeanPrimeRad + _DegreesToRadians(6))
      - 0.20 * SysMath.Cos(4.0 * hMeanPrimeRad - _DegreesToRadians(63));

    var lMeanPrimeMinus50Sq = (lMeanPrime - 50) * (lMeanPrime - 50);
    var sL = 1.0 + (0.015 * lMeanPrimeMinus50Sq) / SysMath.Sqrt(20.0 + lMeanPrimeMinus50Sq);
    var sC = 1.0 + 0.045 * cMeanPrime;
    var sH = 1.0 + 0.015 * cMeanPrime * t;

    var deltaTheta = 30.0 * SysMath.Exp(-SysMath.Pow((hMeanPrime - 275.0) / 25.0, 2));
    var cMeanPrime7 = SysMath.Pow(cMeanPrime, 7);
    var rC = 2.0 * SysMath.Sqrt(cMeanPrime7 / (cMeanPrime7 + Pow25_7));
    var rT = -rC * SysMath.Sin(_DegreesToRadians(2.0 * deltaTheta));

    var lightness = deltaLPrime / (kL * sL);
    var chroma = deltaCPrime / (kC * sC);
    var hue = deltaHPrimeBig / (kH * sH);

    return lightness * lightness + chroma * chroma + hue * hue + rT * chroma * hue;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _Atan2Degrees(double y, double x) {
    var angle = SysMath.Atan2(y, x) * (180.0 / SysMath.PI);
    return angle < 0 ? angle + 360.0 : angle;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _DegreesToRadians(double degrees) => degrees * (SysMath.PI / 180.0);
}
