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

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Interface for calculating distance between two colors.
/// Implementations must be structs to enable JIT inlining and zero-cost abstraction.
/// </summary>
/// <remarks>
/// <para>
/// When using struct implementations with generic type constraints, the JIT compiler
/// will fully devirtualize and inline the Calculate method, eliminating all abstraction overhead.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var index = PaletteSearch.GetMostSimilarColorIndex&lt;EuclideanDistance&lt;Yuv&gt;&gt;(palette, color);
/// </code>
/// </para>
/// </remarks>
public interface IColorDistanceCalculator {
  /// <summary>
  /// Calculates the distance between two colors.
  /// </summary>
  /// <param name="color1">The first color.</param>
  /// <param name="color2">The second color.</param>
  /// <returns>
  /// The distance value. Lower values indicate more similar colors.
  /// The scale depends on the implementation (e.g., 0-441 for RGB Euclidean, 0-100+ for Lab).
  /// </returns>
  double Calculate(Color color1, Color color2);
}
