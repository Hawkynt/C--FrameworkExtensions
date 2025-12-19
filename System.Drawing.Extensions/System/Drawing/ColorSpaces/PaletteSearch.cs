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
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Provides methods for finding the most similar color in a palette using various distance metrics.
/// </summary>
/// <remarks>
/// <para>
/// This class uses generic struct constraints to enable zero-cost abstraction.
/// The JIT compiler can fully inline the distance calculations, resulting in
/// performance equivalent to hand-written specialized code.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// // Euclidean distance in YUV color space
/// var index = PaletteSearch.GetMostSimilarColorIndex&lt;EuclideanDistance&lt;Yuv&gt;&gt;(palette, targetColor);
///
/// // CIEDE2000 perceptual distance
/// var index = PaletteSearch.GetMostSimilarColorIndex&lt;CIEDE2000Distance&gt;(palette, targetColor);
/// </code>
/// </para>
/// </remarks>
public static class PaletteSearch {

  /// <summary>
  /// Finds the index of the most similar color in the palette to the target color.
  /// </summary>
  /// <typeparam name="TDistanceCalculator">
  /// The type of distance calculator to use. Must be a struct implementing <see cref="IColorDistanceCalculator"/>.
  /// </typeparam>
  /// <param name="palette">The palette of colors to search.</param>
  /// <param name="targetColor">The color to find the closest match for.</param>
  /// <returns>The index of the most similar color in the palette.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">Thrown when <paramref name="palette"/> is empty.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GetMostSimilarColorIndex<TDistanceCalculator>(Color[] palette, Color targetColor)
    where TDistanceCalculator : struct, IColorDistanceCalculator {
    ArgumentNullException.ThrowIfNull(palette);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(palette.Length);
    
    var calculator = default(TDistanceCalculator);
    var bestIndex = 0;
    var bestDistance = calculator.Calculate(palette[0], targetColor);

    for (var i = 1; i < palette.Length; ++i) {
      var distance = calculator.Calculate(palette[i], targetColor);
      if (!(distance < bestDistance))
        continue;

      if (distance == 0)
        return i;

      bestDistance = distance;
      bestIndex = i;
    }

    return bestIndex;
  }

  /// <summary>
  /// Finds the most similar color in the palette to the target color.
  /// </summary>
  /// <typeparam name="TDistanceCalculator">
  /// The type of distance calculator to use. Must be a struct implementing <see cref="IColorDistanceCalculator"/>.
  /// </typeparam>
  /// <param name="palette">The palette of colors to search.</param>
  /// <param name="targetColor">The color to find the closest match for.</param>
  /// <returns>The most similar color in the palette.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">Thrown when <paramref name="palette"/> is empty.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color GetMostSimilarColor<TDistanceCalculator>(Color[] palette, Color targetColor)
    where TDistanceCalculator : struct, IColorDistanceCalculator
    => palette[GetMostSimilarColorIndex<TDistanceCalculator>(palette, targetColor)];

  /// <summary>
  /// Finds the index of the most similar color in the palette to the target color,
  /// also returning the distance value.
  /// </summary>
  /// <typeparam name="TDistanceCalculator">
  /// The type of distance calculator to use. Must be a struct implementing <see cref="IColorDistanceCalculator"/>.
  /// </typeparam>
  /// <param name="palette">The palette of colors to search.</param>
  /// <param name="targetColor">The color to find the closest match for.</param>
  /// <returns>A tuple containing the index of the most similar color and its distance from the target.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">Thrown when <paramref name="palette"/> is empty.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (int Index, double Distance) GetMostSimilarColorWithDistance<TDistanceCalculator>(Color[] palette, Color targetColor)
    where TDistanceCalculator : struct, IColorDistanceCalculator {
    ArgumentNullException.ThrowIfNull(palette);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(palette.Length);
    
    var calculator = default(TDistanceCalculator);
    var bestIndex = 0;
    var bestDistance = calculator.Calculate(palette[0], targetColor);

    for (var i = 1; i < palette.Length; ++i) {
      var distance = calculator.Calculate(palette[i], targetColor);
      if (!(distance < bestDistance))
        continue;

      if(distance == 0)
        return (i, 0);

      bestDistance = distance;
      bestIndex = i;
    }

    return (bestIndex, bestDistance);
  }

  /// <summary>
  /// Finds multiple closest colors in the palette to the target color, sorted by distance.
  /// </summary>
  /// <typeparam name="TDistanceCalculator">
  /// The type of distance calculator to use. Must be a struct implementing <see cref="IColorDistanceCalculator"/>.
  /// </typeparam>
  /// <param name="palette">The palette of colors to search.</param>
  /// <param name="targetColor">The color to find matches for.</param>
  /// <param name="count">The number of closest colors to return.</param>
  /// <returns>An array of tuples containing indices and distances, sorted by distance ascending.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">Thrown when <paramref name="palette"/> is empty.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1.</exception>
  public static (int Index, double Distance)[] GetClosestColors<TDistanceCalculator>(Color[] palette, Color targetColor, int count)
    where TDistanceCalculator : struct, IColorDistanceCalculator {
    ArgumentNullException.ThrowIfNull(palette);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(palette.Length);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

    var calculator = default(TDistanceCalculator);
    var actualCount = Math.Min(count, palette.Length);
    var results = new (int Index, double Distance)[actualCount];

    // Initialize with first 'count' elements
    for (var i = 0; i < actualCount; ++i)
      results[i] = (i, calculator.Calculate(palette[i], targetColor));

    // Sort initial results by distance
    Array.Sort(results, (a, b) => a.Distance.CompareTo(b.Distance));

    // Check remaining elements
    for (var i = actualCount; i < palette.Length; ++i) {
      var distance = calculator.Calculate(palette[i], targetColor);

      // If this distance is smaller than the worst in our results, insert it
      if (!(distance < results[actualCount - 1].Distance))
        continue;

      // Find insertion position
      var insertPos = actualCount - 1;
      while (insertPos > 0 && results[insertPos - 1].Distance > distance)
        --insertPos;

      // Shift elements and insert
      for (var j = actualCount - 1; j > insertPos; --j)
        results[j] = results[j - 1];

      results[insertPos] = (i, distance);
    }

    return results;
  }

  /// <summary>
  /// Calculates the distance between two colors using the specified distance calculator.
  /// </summary>
  /// <typeparam name="TDistanceCalculator">
  /// The type of distance calculator to use. Must be a struct implementing <see cref="IColorDistanceCalculator"/>.
  /// </typeparam>
  /// <param name="color1">The first color.</param>
  /// <param name="color2">The second color.</param>
  /// <returns>The distance between the two colors according to the specified metric.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CalculateDistance<TDistanceCalculator>(Color color1, Color color2)
    where TDistanceCalculator : struct, IColorDistanceCalculator
    => default(TDistanceCalculator).Calculate(color1, color2);

}
