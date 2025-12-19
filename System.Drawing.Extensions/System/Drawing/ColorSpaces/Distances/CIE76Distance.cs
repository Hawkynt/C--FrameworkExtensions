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
/// Calculates the CIE76 delta E between two colors using the Lab color space.
/// </summary>
/// <remarks>
/// <para>
/// CIE76 (also known as ΔE*ab or CIE 1976) is a simple Euclidean distance in Lab space.
/// It was the first standardized color difference formula.
/// </para>
/// <para>
/// Typical interpretation:
/// - 0-1: Not perceptible by human eyes
/// - 1-2: Perceptible through close observation
/// - 2-10: Perceptible at a glance
/// - 11-49: Colors are more similar than opposite
/// - 100+: Colors are exact opposite
/// </para>
/// </remarks>
public readonly struct CIE76Distance : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(CIE76DistanceSquared._Calculate(color1, color2));
}

public readonly struct CIE76DistanceSquared : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2);

  internal static double _Calculate(Color color1, Color color2) {
    var lab1 = (LabNormalized)LabNormalized.FromColor(color1);
    var lab2 = (LabNormalized)LabNormalized.FromColor(color2);

    var dL = lab1.L - lab2.L;
    var da = lab1.A - lab2.A;
    var db = lab1.B - lab2.B;

    return dL * dL + da * da + db * db;
  }
}
