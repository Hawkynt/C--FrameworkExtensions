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
/// Calculates the CMC l:c color difference (perceptibility variant, l=1, c=1).
/// </summary>
/// <remarks>
/// CMC (Color Measurement Committee) l:c is widely used in the textile industry.
/// l=1, c=1 is used for perceptibility (detecting if colors differ).
/// </remarks>
public readonly struct CMC : IColorMetric<LabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in LabF a, in LabF b) => (float)SysMath.Sqrt(_Calculate(a, b, 1.0, 1.0));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(in LabF lab1, in LabF lab2, double l, double c) {
    double l1 = lab1.L, a1 = lab1.A, b1 = lab1.B;
    double l2 = lab2.L, a2 = lab2.A, b2 = lab2.B;

    var c1 = SysMath.Sqrt(a1 * a1 + b1 * b1);
    var c2 = SysMath.Sqrt(a2 * a2 + b2 * b2);

    var dL = l1 - l2;
    var dC = c1 - c2;
    var da = a1 - a2;
    var db = b1 - b2;
    var dH2 = da * da + db * db - dC * dC;
    var dH = dH2 > 0 ? SysMath.Sqrt(dH2) : 0;

    // Calculate SL
    var sL = l1 < 16 ? 0.511 : (0.040975 * l1) / (1.0 + 0.01765 * l1);

    // Calculate SC
    var sC = (0.0638 * c1) / (1.0 + 0.0131 * c1) + 0.638;

    // Calculate SH
    var h1 = SysMath.Atan2(b1, a1) * (180.0 / SysMath.PI);
    if (h1 < 0)
      h1 += 360.0;

    double t;
    if (h1 is >= 164 and <= 345)
      t = 0.56 + SysMath.Abs(0.2 * SysMath.Cos((h1 + 168.0) * (SysMath.PI / 180.0)));
    else
      t = 0.36 + SysMath.Abs(0.4 * SysMath.Cos((h1 + 35.0) * (SysMath.PI / 180.0)));

    var c1_4 = c1 * c1 * c1 * c1;
    var f = SysMath.Sqrt(c1_4 / (c1_4 + 1900.0));
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
public readonly struct CMCAcceptability : IColorMetric<LabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in LabF a, in LabF b) => (float)SysMath.Sqrt(CMC._Calculate(a, b, 2.0, 1.0));
}

/// <summary>
/// Calculates the squared CMC l:c color difference (perceptibility variant).
/// </summary>
/// <remarks>
/// Faster than CMC when only comparing distances (no sqrt).
/// </remarks>
public readonly struct CMCSquared : IColorMetric<LabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in LabF a, in LabF b) => (float)CMC._Calculate(a, b, 1.0, 1.0);
}
