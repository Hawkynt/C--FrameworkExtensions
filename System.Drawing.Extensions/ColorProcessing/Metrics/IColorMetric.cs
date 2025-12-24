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
/// Used in quantization for finding the nearest palette color.
/// Different metrics provide different perceptual accuracy:
/// <list type="bullet">
///   <item><description>Euclidean in RGB: Fast but perceptually inaccurate</description></item>
///   <item><description>Euclidean in YUV: Good perceptual balance</description></item>
///   <item><description>Euclidean in Lab: Best perceptual uniformity</description></item>
/// </list>
/// </remarks>
public interface IColorMetric<TKey> where TKey : unmanaged {

  /// <summary>
  /// Calculates the distance between two colors.
  /// </summary>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <returns>The distance (lower = more similar).</returns>
  float Distance(in TKey a, in TKey b);
}
