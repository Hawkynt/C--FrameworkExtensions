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
/// Provides distance calculation between two colors in key space.
/// </summary>
/// <typeparam name="TKey">The key color type for distance measurement.</typeparam>
/// <remarks>
/// <para>Returns distance as <see cref="UNorm32"/>:</para>
/// <list type="bullet">
///   <item><c>Zero</c> = identical colors (0.0)</item>
///   <item><c>One</c> = maximum distance (1.0) for normalized metrics</item>
/// </list>
/// <para>To convert to float: <c>(float)distance</c> (explicit cast).</para>
/// <para>For comparisons, use <see cref="UNorm32"/> operators directly.</para>
/// <para>
/// Different metrics provide different perceptual accuracy:
/// <list type="bullet">
///   <item><description>Euclidean in RGB: Fast but perceptually inaccurate</description></item>
///   <item><description>Euclidean in YUV: Good perceptual balance</description></item>
///   <item><description>Euclidean in Lab: Best perceptual uniformity</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IColorMetric<TKey> where TKey : unmanaged {

  /// <summary>
  /// Calculates the distance between two colors.
  /// </summary>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <returns>The normalized distance (Zero = identical, One = maximum).</returns>
  UNorm32 Distance(in TKey a, in TKey b);
}
