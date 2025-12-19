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
/// Calculates the CIE94 delta E for graphic arts applications.
/// </summary>
/// <remarks>
/// <para>
/// CIE94 is an improvement over CIE76 that accounts for perceptual non-uniformities
/// in the Lab color space. It uses different weighting for lightness, chroma, and hue.
/// </para>
/// <para>
/// Uses graphic arts weighting factors (kL=1, k1=0.045, k2=0.015).
/// For textile applications, use <see cref="CIE94TextilesDistance"/>.
/// </para>
/// </remarks>
public readonly struct CIE94Distance : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(CIE94DistanceSquared._Calculate(color1, color2, kL: 1.0, k1: 0.045, k2: 0.015));
}

/// <summary>
/// Calculates the squared CIE94 delta E for graphic arts applications.
/// Faster than <see cref="CIE94Distance"/> when only comparing distances.
/// </summary>
public readonly struct CIE94DistanceSquared : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2, kL: 1.0, k1: 0.045, k2: 0.015);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(Color color1, Color color2, double kL, double k1, double k2) {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1);
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2);

    var dL = lab1.L - lab2.L;
    var da = lab1.A - lab2.A;
    var db = lab1.B - lab2.B;

    var c1 = Math.Sqrt(lab1.A * lab1.A + lab1.B * lab1.B);
    var c2 = Math.Sqrt(lab2.A * lab2.A + lab2.B * lab2.B);
    var dC = c1 - c2;

    var dH2 = da * da + db * db - dC * dC;
    var dH = dH2 > 0 ? Math.Sqrt(dH2) : 0;

    const double sL = 1.0;
    var sC = 1.0 + k1 * c1;
    var sH = 1.0 + k2 * c1;

    var lTerm = dL / (kL * sL);
    var cTerm = dC / sC;
    var hTerm = dH / sH;

    return lTerm * lTerm + cTerm * cTerm + hTerm * hTerm;
  }
}

/// <summary>
/// Calculates the CIE94 delta E for textile applications.
/// </summary>
/// <remarks>
/// Uses textile industry weighting factors (kL=2, k1=0.048, k2=0.014).
/// </remarks>
public readonly struct CIE94TextilesDistance : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(CIE94DistanceSquared._Calculate(color1, color2, kL: 2.0, k1: 0.048, k2: 0.014));
}

/// <summary>
/// Calculates the squared CIE94 delta E for textile applications.
/// Faster than <see cref="CIE94TextilesDistance"/> when only comparing distances.
/// </summary>
public readonly struct CIE94TextilesDistanceSquared : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => CIE94DistanceSquared._Calculate(color1, color2, kL: 2.0, k1: 0.048, k2: 0.014);
}
