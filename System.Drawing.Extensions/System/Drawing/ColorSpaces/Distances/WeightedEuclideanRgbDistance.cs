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
/// Calculates weighted Euclidean distance in RGB space using human perception weights.
/// </summary>
/// <remarks>
/// <para>
/// Uses weights based on human color perception:
/// Red: 0.30, Green: 0.59, Blue: 0.11
/// </para>
/// <para>
/// This approximates how humans perceive color differences, giving more weight
/// to green (most sensitive) and less to blue (least sensitive).
/// </para>
/// </remarks>
public readonly struct WeightedEuclideanRgbDistance : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(WeightedEuclideanRgbDistanceSquared._Calculate(color1, color2));
}

/// <summary>
/// Calculates squared weighted Euclidean distance in RGB space using human perception weights.
/// Faster than <see cref="WeightedEuclideanRgbDistance"/> when only comparing distances.
/// </summary>
public readonly struct WeightedEuclideanRgbDistanceSquared : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(Color color1, Color color2) {
    var c1 = new Rgba32(color1);
    var c2 = new Rgba32(color2);

    var dr = c1.R - c2.R;
    var dg = c1.G - c2.G;
    var db = c1.B - c2.B;

    // Weights based on human perception
    return 0.30 * dr * dr + 0.59 * dg * dg + 0.11 * db * db;
  }
}
