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
using System.Drawing;

namespace Hawkynt.Drawing.ColorDomain;

/// <summary>
/// Color-domain quantizer contract. Reduces a histogram (or color set) to a fixed-size
/// palette of <see cref="Color"/>s. Designed for tools that work in
/// <see cref="System.Drawing.Color"/> space (CLI converters, palette designers, GIF writers)
/// and don't want to bind to the generic <c>TWork</c> machinery of
/// <see cref="Hawkynt.ColorProcessing.IQuantizer"/>.
/// </summary>
/// <remarks>
/// Get instances from <see cref="ColorQuantizerRegistry"/>, or wrap an extension quantizer
/// directly via <see cref="ColorQuantizerAdapter"/>.
/// </remarks>
public interface IColorQuantizer {

  /// <summary>
  /// Reduces <paramref name="usedColors"/> down to at most <paramref name="numberOfColors"/>
  /// representative colors. Each input color is treated as having weight 1.
  /// </summary>
  Color[] ReduceColorsTo(byte numberOfColors, IEnumerable<Color> usedColors);

  /// <summary>
  /// Reduces a color histogram to at most <paramref name="numberOfColors"/> representative
  /// colors. Higher-count entries influence the result more.
  /// </summary>
  Color[] ReduceColorsTo(byte numberOfColors, IEnumerable<(Color color, uint count)> histogram);
}
