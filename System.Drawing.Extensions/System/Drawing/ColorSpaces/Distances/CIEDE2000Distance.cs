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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.ColorSpaces.Distances;

/// <summary>
/// Calculates the CIEDE2000 delta E between two colors.
/// This is the most accurate perceptual color difference formula.
/// </summary>
/// <remarks>
/// <para>
/// CIEDE2000 (also known as ΔE00) is the current CIE recommendation for
/// calculating color differences. It includes corrections for:
/// - Lightness (L')
/// - Chroma (C')
/// - Hue (H')
/// - Interaction between chroma and hue (rotation term)
/// </para>
/// </remarks>
public readonly struct CIEDE2000Distance : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(CIEDE2000DistanceSquared._Calculate(color1, color2));
}

/// <summary>
/// Calculates the squared CIEDE2000 delta E between two colors.
/// Faster than <see cref="CIEDE2000Distance"/> when only comparing distances.
/// </summary>
public readonly struct CIEDE2000DistanceSquared : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(Color color1, Color color2) {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1);
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2);
    // Parametric weighting factors (typically 1:1:1)
    const double kL = 1.0;
    const double kC = 1.0;
    const double kH = 1.0;

    var l1 = (double)lab1.L;
    var a1 = (double)lab1.A;
    var b1 = (double)lab1.B;
    var l2 = (double)lab2.L;
    var a2 = (double)lab2.A;
    var b2 = (double)lab2.B;

    // Calculate C* (chroma) values
    var c1 = Math.Sqrt(a1 * a1 + b1 * b1);
    var c2 = Math.Sqrt(a2 * a2 + b2 * b2);
    var cMean = (c1 + c2) / 2.0;

    // Calculate G (a' adjustment factor)
    var cMean7 = Math.Pow(cMean, 7);
    var g = 0.5 * (1.0 - Math.Sqrt(cMean7 / (cMean7 + 6103515625.0))); // 25^7 = 6103515625

    // Calculate a' (adjusted a values)
    var a1Prime = a1 * (1.0 + g);
    var a2Prime = a2 * (1.0 + g);

    // Calculate C' (adjusted chroma)
    var c1Prime = Math.Sqrt(a1Prime * a1Prime + b1 * b1);
    var c2Prime = Math.Sqrt(a2Prime * a2Prime + b2 * b2);

    // Calculate h' (adjusted hue)
    var h1Prime = _Atan2Degrees(b1, a1Prime);
    var h2Prime = _Atan2Degrees(b2, a2Prime);

    // Calculate delta values
    var deltaLPrime = l2 - l1;
    var deltaCPrime = c2Prime - c1Prime;

    // Calculate delta h'
    double deltaHPrime;
    var cProduct = c1Prime * c2Prime;
    if (cProduct == 0)
      deltaHPrime = 0;
    else {
      var dhPrime = h2Prime - h1Prime;
      if (dhPrime > 180)
        dhPrime -= 360;
      else if (dhPrime < -180)
        dhPrime += 360;
      deltaHPrime = dhPrime;
    }

    // Calculate delta H'
    var deltaHPrimeBig = 2.0 * Math.Sqrt(cProduct) * Math.Sin(_DegreesToRadians(deltaHPrime / 2.0));

    // Calculate mean values
    var lMeanPrime = (l1 + l2) / 2.0;
    var cMeanPrime = (c1Prime + c2Prime) / 2.0;

    // Calculate h' mean
    double hMeanPrime;
    if (cProduct == 0)
      hMeanPrime = h1Prime + h2Prime;
    else {
      var hSum = h1Prime + h2Prime;
      var hDiff = Math.Abs(h1Prime - h2Prime);
      if (hDiff <= 180)
        hMeanPrime = hSum / 2.0;
      else if (hSum < 360)
        hMeanPrime = (hSum + 360) / 2.0;
      else
        hMeanPrime = (hSum - 360) / 2.0;
    }

    // Calculate T
    var hMeanPrimeRad = _DegreesToRadians(hMeanPrime);
    var t = 1.0
      - 0.17 * Math.Cos(hMeanPrimeRad - _DegreesToRadians(30))
      + 0.24 * Math.Cos(2.0 * hMeanPrimeRad)
      + 0.32 * Math.Cos(3.0 * hMeanPrimeRad + _DegreesToRadians(6))
      - 0.20 * Math.Cos(4.0 * hMeanPrimeRad - _DegreesToRadians(63));

    // Calculate SL, SC, SH
    var lMeanPrimeMinus50Sq = (lMeanPrime - 50) * (lMeanPrime - 50);
    var sL = 1.0 + (0.015 * lMeanPrimeMinus50Sq) / Math.Sqrt(20.0 + lMeanPrimeMinus50Sq);
    var sC = 1.0 + 0.045 * cMeanPrime;
    var sH = 1.0 + 0.015 * cMeanPrime * t;

    // Calculate RT (rotation term)
    var deltaTheta = 30.0 * Math.Exp(-Math.Pow((hMeanPrime - 275.0) / 25.0, 2));
    var cMeanPrime7 = Math.Pow(cMeanPrime, 7);
    var rC = 2.0 * Math.Sqrt(cMeanPrime7 / (cMeanPrime7 + 6103515625.0));
    var rT = -rC * Math.Sin(_DegreesToRadians(2.0 * deltaTheta));

    // Calculate final delta E (squared)
    var lightness = deltaLPrime / (kL * sL);
    var chroma = deltaCPrime / (kC * sC);
    var hue = deltaHPrimeBig / (kH * sH);

    return lightness * lightness
      + chroma * chroma
      + hue * hue
      + rT * chroma * hue;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _Atan2Degrees(double y, double x) {
    var angle = Math.Atan2(y, x) * (180.0 / Math.PI);
    return angle < 0 ? angle + 360.0 : angle;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);
}
