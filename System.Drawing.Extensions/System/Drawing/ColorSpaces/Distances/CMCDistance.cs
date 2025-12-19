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
/// Calculates the CMC l:c color difference (perceptibility variant, l=1, c=1).
/// Developed for the textile industry and based on CIE Lab.
/// </summary>
/// <remarks>
/// <para>
/// CMC (Color Measurement Committee) l:c is widely used in the textile industry.
/// The l and c parameters control the relative importance of lightness and chroma.
/// </para>
/// <para>
/// l=1, c=1 is used for perceptibility (detecting if colors differ).
/// For acceptability (determining if differences are acceptable), use <see cref="CMCAcceptabilityDistance"/>.
/// </para>
/// </remarks>
public readonly struct CMCDistance : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(CMCDistanceSquared._Calculate(color1, color2, l: 1.0, c: 1.0));
}

/// <summary>
/// Calculates the squared CMC l:c color difference (perceptibility variant, l=1, c=1).
/// Faster than <see cref="CMCDistance"/> when only comparing distances.
/// </summary>
public readonly struct CMCDistanceSquared : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2, l: 1.0, c: 1.0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(Color color1, Color color2, double l, double c) {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1);
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2);
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
    if (h1 is >= 164 and <= 345)
      t = 0.56 + Math.Abs(0.2 * Math.Cos((h1 + 168.0) * (Math.PI / 180.0)));
    else
      t = 0.36 + Math.Abs(0.4 * Math.Cos((h1 + 35.0) * (Math.PI / 180.0)));

    var c1_4 = c1 * c1 * c1 * c1;
    var f = Math.Sqrt(c1_4 / (c1_4 + 1900.0));
    var sH = sC * (f * t + 1.0 - f);

    var lTerm = dL / (l * sL);
    var cTerm = dC / (c * sC);
    var hTerm = dH / sH;

    return lTerm * lTerm + cTerm * cTerm + hTerm * hTerm;
  }
}

/// <summary>
/// Calculates the CMC l:c color difference (acceptability variant, l=2, c=1).
/// </summary>
/// <remarks>
/// l=2, c=1 is used for acceptability (determining if color differences are acceptable in production).
/// </remarks>
public readonly struct CMCAcceptabilityDistance : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(CMCDistanceSquared._Calculate(color1, color2, l: 2.0, c: 1.0));
}

/// <summary>
/// Calculates the squared CMC l:c color difference (acceptability variant, l=2, c=1).
/// Faster than <see cref="CMCAcceptabilityDistance"/> when only comparing distances.
/// </summary>
public readonly struct CMCAcceptabilityDistanceSquared : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => CMCDistanceSquared._Calculate(color1, color2, l: 2.0, c: 1.0);
}
