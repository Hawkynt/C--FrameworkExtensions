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

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Provides methods for calculating the distance between colors in various color spaces.
/// </summary>
public static class ColorDistance {

  #region Euclidean RGB

  /// <summary>
  /// Calculates the Euclidean distance between two colors in RGB space.
  /// </summary>
  /// <typeparam name="TColor1">The type of the first color.</typeparam>
  /// <typeparam name="TColor2">The type of the second color.</typeparam>
  /// <param name="color1">The first color.</param>
  /// <param name="color2">The second color.</param>
  /// <returns>The Euclidean distance (0-441.67 for fully opaque colors).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double EuclideanRgb<TColor1, TColor2>(TColor1 color1, TColor2 color2)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var c1 = new Rgba32(color1.ToColor());
    var c2 = new Rgba32(color2.ToColor());
    return _EuclideanRgb(c1, c2);
  }

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double EuclideanRgb(Color color1, Color color2) {
    var c1 = new Rgba32(color1);
    var c2 = new Rgba32(color2);
    return _EuclideanRgb(c1, c2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _EuclideanRgb(Rgba32 color1, Rgba32 color2) {
    var dr = color1.R - color2.R;
    var dg = color1.G - color2.G;
    var db = color1.B - color2.B;
    return Math.Sqrt(dr * dr + dg * dg + db * db);
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors in RGB space.
  /// Faster than <see cref="EuclideanRgb{TColor1,TColor2}"/> when only comparing distances.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int EuclideanRgbSquared<TColor1, TColor2>(TColor1 color1, TColor2 color2)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var c1 = new Rgba32(color1.ToColor());
    var c2 = new Rgba32(color2.ToColor());
    return _EuclideanRgbSquared(c1, c2);
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int EuclideanRgbSquared(Color color1, Color color2) {
    var c1 = new Rgba32(color1);
    var c2 = new Rgba32(color2);
    return _EuclideanRgbSquared(c1, c2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _EuclideanRgbSquared(Rgba32 color1, Rgba32 color2) {
    var dr = color1.R - color2.R;
    var dg = color1.G - color2.G;
    var db = color1.B - color2.B;
    return dr * dr + dg * dg + db * db;
  }

  #endregion

  #region Weighted Euclidean RGB

  /// <summary>
  /// Calculates the weighted Euclidean distance between two colors in RGB space.
  /// Uses human perception weights (red: 0.30, green: 0.59, blue: 0.11).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double WeightedEuclideanRgb<TColor1, TColor2>(TColor1 color1, TColor2 color2)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var c1 = new Rgba32(color1.ToColor());
    var c2 = new Rgba32(color2.ToColor());
    return _WeightedEuclideanRgb(c1, c2);
  }

  /// <summary>
  /// Calculates the weighted Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double WeightedEuclideanRgb(Color color1, Color color2) {
    var c1 = new Rgba32(color1);
    var c2 = new Rgba32(color2);
    return _WeightedEuclideanRgb(c1, c2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _WeightedEuclideanRgb(Rgba32 color1, Rgba32 color2) {
    var dr = color1.R - color2.R;
    var dg = color1.G - color2.G;
    var db = color1.B - color2.B;
    // Weights based on human perception
    return Math.Sqrt(0.30 * dr * dr + 0.59 * dg * dg + 0.11 * db * db);
  }

  #endregion

  #region CIE76 (Lab Delta E)

  /// <summary>
  /// Calculates the CIE76 delta E between two colors using the Lab color space.
  /// This is a perceptually uniform distance metric.
  /// </summary>
  /// <typeparam name="TColor1">The type of the first color.</typeparam>
  /// <typeparam name="TColor2">The type of the second color.</typeparam>
  /// <param name="color1">The first color.</param>
  /// <param name="color2">The second color.</param>
  /// <returns>The CIE76 delta E value (0 = identical, 1 = just noticeable difference).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CIE76<TColor1, TColor2>(TColor1 color1, TColor2 color2)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1.ToColor());
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2.ToColor());
    return CIE76(lab1, lab2);
  }

  /// <summary>
  /// Calculates the CIE76 delta E between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CIE76(Color color1, Color color2) {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1);
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2);
    return CIE76(lab1, lab2);
  }

  /// <summary>
  /// Calculates the CIE76 delta E between two Lab colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CIE76(LabNormalized lab1, LabNormalized lab2) {
    var dL = lab1.L - lab2.L;
    var da = lab1.A - lab2.A;
    var db = lab1.B - lab2.B;
    return Math.Sqrt(dL * dL + da * da + db * db);
  }

  #endregion

  #region CIEDE2000

  /// <summary>
  /// Calculates the CIEDE2000 delta E between two colors.
  /// This is the most accurate perceptual color difference formula.
  /// </summary>
  /// <typeparam name="TColor1">The type of the first color.</typeparam>
  /// <typeparam name="TColor2">The type of the second color.</typeparam>
  /// <param name="color1">The first color.</param>
  /// <param name="color2">The second color.</param>
  /// <returns>The CIEDE2000 delta E value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CIEDE2000<TColor1, TColor2>(TColor1 color1, TColor2 color2)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1.ToColor());
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2.ToColor());
    return CIEDE2000(lab1, lab2);
  }

  /// <summary>
  /// Calculates the CIEDE2000 delta E between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CIEDE2000(Color color1, Color color2) {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1);
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2);
    return CIEDE2000(lab1, lab2);
  }

  /// <summary>
  /// Calculates the CIEDE2000 delta E between two Lab colors.
  /// Implementation follows the CIE technical report.
  /// </summary>
  public static double CIEDE2000(LabNormalized lab1, LabNormalized lab2) {
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

    // Calculate final delta E
    var lightness = deltaLPrime / (kL * sL);
    var chroma = deltaCPrime / (kC * sC);
    var hue = deltaHPrimeBig / (kH * sH);

    return Math.Sqrt(
      lightness * lightness
      + chroma * chroma
      + hue * hue
      + rT * chroma * hue
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _Atan2Degrees(double y, double x) {
    var angle = Math.Atan2(y, x) * (180.0 / Math.PI);
    return angle < 0 ? angle + 360.0 : angle;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);

  #endregion

  #region Manhattan Distance

  /// <summary>
  /// Calculates the Manhattan (taxicab) distance between two colors in RGB space.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ManhattanRgb<TColor1, TColor2>(TColor1 color1, TColor2 color2)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var c1 = new Rgba32(color1.ToColor());
    var c2 = new Rgba32(color2.ToColor());
    return _ManhattanRgb(c1, c2);
  }

  /// <summary>
  /// Calculates the Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ManhattanRgb(Color color1, Color color2) {
    var c1 = new Rgba32(color1);
    var c2 = new Rgba32(color2);
    return _ManhattanRgb(c1, c2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _ManhattanRgb(Rgba32 color1, Rgba32 color2) {
    var dr = Math.Abs(color1.R - color2.R);
    var dg = Math.Abs(color1.G - color2.G);
    var db = Math.Abs(color1.B - color2.B);
    return dr + dg + db;
  }

  #endregion

  #region Chebyshev Distance

  /// <summary>
  /// Calculates the Chebyshev (maximum) distance between two colors in RGB space.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ChebyshevRgb<TColor1, TColor2>(TColor1 color1, TColor2 color2)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var c1 = new Rgba32(color1.ToColor());
    var c2 = new Rgba32(color2.ToColor());
    return _ChebyshevRgb(c1, c2);
  }

  /// <summary>
  /// Calculates the Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ChebyshevRgb(Color color1, Color color2) {
    var c1 = new Rgba32(color1);
    var c2 = new Rgba32(color2);
    return _ChebyshevRgb(c1, c2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _ChebyshevRgb(Rgba32 color1, Rgba32 color2) {
    var dr = Math.Abs(color1.R - color2.R);
    var dg = Math.Abs(color1.G - color2.G);
    var db = Math.Abs(color1.B - color2.B);
    return dr > dg ? (dr > db ? dr : db) : (dg > db ? dg : db);
  }

  #endregion

  #region Compuserve (Redmean)

  /// <summary>
  /// Calculates the Compuserve color distance (also known as "redmean").
  /// This formula adjusts RGB weights based on the average red value,
  /// providing better perceptual accuracy than simple Euclidean distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Redmean<TColor1, TColor2>(TColor1 color1, TColor2 color2)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var c1 = new Rgba32(color1.ToColor());
    var c2 = new Rgba32(color2.ToColor());
    return _Redmean(c1, c2);
  }

  /// <summary>
  /// Calculates the Compuserve color distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Redmean(Color color1, Color color2) {
    var c1 = new Rgba32(color1);
    var c2 = new Rgba32(color2);
    return _Redmean(c1, c2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Redmean(Rgba32 color1, Rgba32 color2) {
    var rMean = (color1.R + color2.R) / 2.0;
    var dr = color1.R - color2.R;
    var dg = color1.G - color2.G;
    var db = color1.B - color2.B;

    // Weights vary based on red mean value
    var rWeight = 2.0 + rMean / 256.0;
    var gWeight = 4.0;
    var bWeight = 2.0 + (255.0 - rMean) / 256.0;

    return Math.Sqrt(rWeight * dr * dr + gWeight * dg * dg + bWeight * db * db);
  }

  #endregion

  #region CIE94

  /// <summary>
  /// Calculates the CIE94 delta E between two colors.
  /// An improvement over CIE76 that accounts for perceptual non-uniformities.
  /// </summary>
  /// <param name="color1">The first color.</param>
  /// <param name="color2">The second color.</param>
  /// <param name="textiles">If true, uses textile industry weights; otherwise uses graphic arts weights.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CIE94<TColor1, TColor2>(TColor1 color1, TColor2 color2, bool textiles = false)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1.ToColor());
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2.ToColor());
    return CIE94(lab1, lab2, textiles);
  }

  /// <summary>
  /// Calculates the CIE94 delta E between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CIE94(Color color1, Color color2, bool textiles = false) {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1);
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2);
    return CIE94(lab1, lab2, textiles);
  }

  /// <summary>
  /// Calculates the CIE94 delta E between two Lab colors.
  /// </summary>
  public static double CIE94(LabNormalized lab1, LabNormalized lab2, bool textiles = false) {
    // Weighting factors
    double kL, k1, k2;
    if (textiles) {
      kL = 2.0;
      k1 = 0.048;
      k2 = 0.014;
    } else {
      // Graphic arts
      kL = 1.0;
      k1 = 0.045;
      k2 = 0.015;
    }

    var dL = lab1.L - lab2.L;
    var da = lab1.A - lab2.A;
    var db = lab1.B - lab2.B;

    var c1 = Math.Sqrt(lab1.A * lab1.A + lab1.B * lab1.B);
    var c2 = Math.Sqrt(lab2.A * lab2.A + lab2.B * lab2.B);
    var dC = c1 - c2;

    var dH2 = da * da + db * db - dC * dC;
    var dH = dH2 > 0 ? Math.Sqrt(dH2) : 0;

    var sL = 1.0;
    var sC = 1.0 + k1 * c1;
    var sH = 1.0 + k2 * c1;

    var lTerm = dL / (kL * sL);
    var cTerm = dC / sC;
    var hTerm = dH / sH;

    return Math.Sqrt(lTerm * lTerm + cTerm * cTerm + hTerm * hTerm);
  }

  #endregion

  #region CMC l:c

  /// <summary>
  /// Calculates the CMC l:c color difference.
  /// Developed for the textile industry and based on CIE Lab.
  /// </summary>
  /// <param name="color1">The first color (reference).</param>
  /// <param name="color2">The second color (sample).</param>
  /// <param name="l">Lightness weighting factor (typically 1 for perceptibility, 2 for acceptability).</param>
  /// <param name="c">Chroma weighting factor (typically 1).</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CMC<TColor1, TColor2>(TColor1 color1, TColor2 color2, double l = 1.0, double c = 1.0)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1.ToColor());
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2.ToColor());
    return CMC(lab1, lab2, l, c);
  }

  /// <summary>
  /// Calculates the CMC l:c color difference between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CMC(Color color1, Color color2, double l = 1.0, double c = 1.0) {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1);
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2);
    return CMC(lab1, lab2, l, c);
  }

  /// <summary>
  /// Calculates the CMC l:c color difference between two Lab colors.
  /// </summary>
  public static double CMC(LabNormalized lab1, LabNormalized lab2, double l = 1.0, double c = 1.0) {
    var l1 = (double)lab1.L;
    var a1 = (double)lab1.A;
    var b1 = (double)lab1.B;
    var l2 = (double)lab2.L;
    var a2 = (double)lab2.A;
    var b2 = (double)lab2.B;

    var c1 = Math.Sqrt(a1 * a1 + b1 * b1);
    var c2 = Math.Sqrt(a2 * a2 + b2 * b2);

    var dL = l1 - l2;
    var dC = c1 - c2;
    var da = a1 - a2;
    var db = b1 - b2;
    var dH2 = da * da + db * db - dC * dC;
    var dH = dH2 > 0 ? Math.Sqrt(dH2) : 0;

    // Calculate SL
    var sL = l1 < 16 ? 0.511 : (0.040975 * l1) / (1.0 + 0.01765 * l1);

    // Calculate SC
    var sC = (0.0638 * c1) / (1.0 + 0.0131 * c1) + 0.638;

    // Calculate SH
    var h1 = Math.Atan2(b1, a1) * (180.0 / Math.PI);
    if (h1 < 0)
      h1 += 360.0;

    double t;
    if (h1 >= 164 && h1 <= 345)
      t = 0.56 + Math.Abs(0.2 * Math.Cos((h1 + 168.0) * (Math.PI / 180.0)));
    else
      t = 0.36 + Math.Abs(0.4 * Math.Cos((h1 + 35.0) * (Math.PI / 180.0)));

    var c1_4 = c1 * c1 * c1 * c1;
    var f = Math.Sqrt(c1_4 / (c1_4 + 1900.0));
    var sH = sC * (f * t + 1.0 - f);

    var lTerm = dL / (l * sL);
    var cTerm = dC / (c * sC);
    var hTerm = dH / sH;

    return Math.Sqrt(lTerm * lTerm + cTerm * cTerm + hTerm * hTerm);
  }

  #endregion

  #region DIN99

  /// <summary>
  /// Calculates the DIN99 color difference.
  /// German standard (DIN 6176) optimized for small color differences.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double DIN99<TColor1, TColor2>(TColor1 color1, TColor2 color2)
    where TColor1 : IColorSpace
    where TColor2 : IColorSpace {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1.ToColor());
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2.ToColor());
    return DIN99(lab1, lab2);
  }

  /// <summary>
  /// Calculates the DIN99 color difference between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double DIN99(Color color1, Color color2) {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1);
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2);
    return DIN99(lab1, lab2);
  }

  /// <summary>
  /// Calculates the DIN99 color difference between two Lab colors.
  /// </summary>
  public static double DIN99(LabNormalized lab1, LabNormalized lab2) {
    // Convert Lab to DIN99 space
    var (l99_1, a99_1, b99_1) = _LabToDin99(lab1.L, lab1.A, lab1.B);
    var (l99_2, a99_2, b99_2) = _LabToDin99(lab2.L, lab2.A, lab2.B);

    var dL = l99_1 - l99_2;
    var da = a99_1 - a99_2;
    var db = b99_1 - b99_2;

    return Math.Sqrt(dL * dL + da * da + db * db);
  }

  private static (double L99, double a99, double b99) _LabToDin99(double l, double a, double b) {
    const double kE = 1.0;   // Reference white adjustment
    const double kCH = 1.0;  // Chroma/hue adjustment

    // DIN99 transformation constants
    const double cos16 = 0.9612616959383189; // cos(16°)
    const double sin16 = 0.27563735581699916; // sin(16°)

    var l99 = 105.509 * Math.Log(1.0 + 0.0158 * l * kE);

    // Rotate a,b by 16 degrees
    var e = a * cos16 + b * sin16;
    var f = 0.7 * (-a * sin16 + b * cos16);

    var g = Math.Sqrt(e * e + f * f);
    var c99 = g > 0 ? Math.Log(1.0 + 0.045 * g * kCH * kE) / 0.045 : 0;

    var h99Ef = Math.Atan2(f, e);
    var a99 = c99 * Math.Cos(h99Ef);
    var b99 = c99 * Math.Sin(h99Ef);

    return (l99, a99, b99);
  }

  #endregion

}
