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

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// Provides integer distance calculation between two colors in key space.
/// </summary>
/// <typeparam name="TKey">The key color type for distance measurement.</typeparam>
/// <remarks>
/// <para>
/// Integer metrics are optimized for byte-based color spaces where the distance
/// can be computed entirely in integer arithmetic, avoiding float conversion overhead.
/// </para>
/// <para>
/// For byte colors (0-255 per channel), maximum distances are bounded:
/// <list type="bullet">
///   <item><description>Manhattan 4-channel: 4 × 255 = 1,020</description></item>
///   <item><description>Chebyshev: 255</description></item>
///   <item><description>Euclidean Squared 4-channel: 4 × 255² = 260,100</description></item>
/// </list>
/// All fit comfortably in a 32-bit integer.
/// </para>
/// <para>
/// Use integer metrics when performing relative comparisons (finding minimum distance)
/// rather than when absolute distance values are needed.
/// </para>
/// </remarks>
public interface IColorMetricInt<TKey> where TKey : unmanaged {

  /// <summary>
  /// Calculates the integer distance between two colors.
  /// </summary>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <returns>The distance as an integer (lower = more similar).</returns>
  int Distance(in TKey a, in TKey b);
}
