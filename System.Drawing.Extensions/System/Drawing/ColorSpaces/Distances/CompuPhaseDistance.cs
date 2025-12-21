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
/// Calculates color distance using the CompuPhase low-cost approximation algorithm.
/// </summary>
/// <remarks>
/// <para>
/// This algorithm provides a fast approximation of perceptual color difference
/// by weighting RGB components based on the mean red value. It's particularly
/// efficient as it uses only integer arithmetic and bit shifts.
/// </para>
/// <para>
/// The algorithm adjusts red and blue weights based on average red intensity:
/// - Higher red values increase red weight and decrease blue weight
/// - Lower red values decrease red weight and increase blue weight
/// - Green is weighted consistently at 4x
/// </para>
/// <para>
/// Reference: https://www.compuphase.com/cmetric.htm
/// </para>
/// </remarks>
public readonly struct CompuPhaseDistance : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(CompuPhaseDistanceSquared._Calculate(color1, color2));

}

/// <summary>
/// Calculates squared color distance using the CompuPhase low-cost approximation algorithm.
/// Faster than <see cref="CompuPhaseDistance"/> when only comparing distances.
/// </summary>
/// <remarks>
/// <para>
/// This algorithm provides a fast approximation of perceptual color difference
/// by weighting RGB components based on the mean red value. It's particularly
/// efficient as it uses only integer arithmetic and bit shifts.
/// </para>
/// <para>
/// Reference: https://www.compuphase.com/cmetric.htm
/// </para>
/// </remarks>
public readonly struct CompuPhaseDistanceSquared : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _Calculate(Color color1, Color color2) {
    var c1 = new Rgba32(color1);
    var c2 = new Rgba32(color2);

    var r1 = c1.R;
    var g1 = c1.G;
    var b1 = c1.B;
    var a1 = c1.A;
    var r2 = c2.R;
    var g2 = c2.G;
    var b2 = c2.B;
    var a2 = c2.A;

    var rMean = r1 + r2;
    var r = r1 - r2;
    var g = g1 - g2;
    var b = b1 - b2;
    var a = a1 - a2;
    rMean >>= 1;
    r *= r;
    g *= g;
    b *= b;
    a *= a;
    var rb = 512 + rMean;
    var bb = 767 - rMean;
    g <<= 2;
    rb *= r;
    bb *= b;
    rb >>= 8;
    bb >>= 8;

    return rb + g + bb + a;
  }

}
