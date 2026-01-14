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

using System.Collections.Generic;
using Hawkynt.ColorProcessing.Storage;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Defines the interface for color quantization algorithms.
/// </summary>
/// <remarks>
/// <para>
/// Color quantization reduces the number of distinct colors in an image while
/// preserving visual quality. Common use cases include:
/// </para>
/// <list type="bullet">
/// <item><description>GIF/PNG8 palette generation</description></item>
/// <item><description>Retro game palette reduction</description></item>
/// <item><description>Color analysis and clustering</description></item>
/// </list>
/// </remarks>
public interface IQuantizer {

  /// <summary>
  /// Generates a color palette of the specified size from a collection of colors.
  /// </summary>
  /// <param name="colors">The colors to analyze.</param>
  /// <param name="colorCount">The desired number of colors in the palette (1-256).</param>
  /// <returns>An array of colors representing the optimized palette.</returns>
  /// <remarks>
  /// Each color in the collection is treated as having equal weight (count of 1).
  /// For weighted quantization, use <see cref="GeneratePalette(IEnumerable{ValueTuple{Bgra8888, uint}}, int)"/>.
  /// </remarks>
  Bgra8888[] GeneratePalette(IEnumerable<Bgra8888> colors, int colorCount);

  /// <summary>
  /// Generates a color palette of the specified size from a color histogram.
  /// </summary>
  /// <param name="histogram">The color histogram where each entry contains a color and its occurrence count.</param>
  /// <param name="colorCount">The desired number of colors in the palette (1-256).</param>
  /// <returns>An array of colors representing the optimized palette.</returns>
  /// <remarks>
  /// The histogram provides weighted importance for each color based on its frequency
  /// in the original image. Colors with higher counts will have more influence on
  /// the resulting palette.
  /// </remarks>
  Bgra8888[] GeneratePalette(IEnumerable<(Bgra8888 color, uint count)> histogram, int colorCount);

}
