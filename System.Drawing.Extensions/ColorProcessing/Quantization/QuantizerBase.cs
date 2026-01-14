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

using System;
using System.Collections.Generic;
using System.Linq;
using Hawkynt.ColorProcessing.Storage;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Abstract base class for color quantization algorithms.
/// Provides common functionality for palette generation and color filling.
/// </summary>
public abstract class QuantizerBase : IQuantizer {

  /// <summary>
  /// Gets or sets whether to fill unused palette entries with generated colors.
  /// When true, generates additional colors to fill the requested palette size.
  /// When false, fills with transparent colors only.
  /// </summary>
  public bool AllowFillingColors { get; set; } = true;

  /// <inheritdoc />
  public Bgra8888[] GeneratePalette(IEnumerable<Bgra8888> colors, int colorCount) {
    switch (colorCount) {
      case <= 0:
        return [];
      case 1:
        return [colors.FirstOrDefault()];
    }

    var used = colors.Distinct().ToArray();
    return this._GenerateFinalPalette(
      used.Length < colorCount
        ? used
        : this._ReduceColorsTo(colorCount, used.Select(c => (c, 1u)))
      , colorCount
    );
  }

  /// <inheritdoc />
  public Bgra8888[] GeneratePalette(IEnumerable<(Bgra8888 color, uint count)> histogram, int colorCount) {
    switch (colorCount) {
      case <= 0:
        return [];
      case 1:
        return [histogram.FirstOrDefault().color];
    }

    var used = histogram
      .GroupBy(h => h.color.Packed)
      .Select(g => (color: new Bgra8888(g.Key), count: (uint)g.Sum(h => h.count)))
      .ToArray();

    return this._GenerateFinalPalette(
      used.Length < colorCount
        ? used.Select(h => h.color)
        : this._ReduceColorsTo(colorCount, used)
      , colorCount
    );
  }

  /// <summary>
  /// Generates the final palette by filling any remaining slots with appropriate colors.
  /// </summary>
  private Bgra8888[] _GenerateFinalPalette(IEnumerable<Bgra8888> proposedPalette, int colorCount) {
    var distinctColors = proposedPalette.Distinct().ToArray();
    var result = new Bgra8888[colorCount];
    var index = 0;

    var colorsToTake = Math.Min(distinctColors.Length, colorCount);
    for (; index < colorsToTake; ++index)
      result[index] = distinctColors[index];

    if (!this.AllowFillingColors) {
      while (index < colorCount)
        result[index++] = Bgra8888.Transparent;
      return result;
    }

    // Add basic colors if still space left
    if (index < colorCount) {
      Bgra8888[] basicColors = [Bgra8888.Black, Bgra8888.White, Bgra8888.Transparent];
      foreach (var color in basicColors) {
        if (index >= colorCount)
          break;

        if (result.Take(index).All(c => c.Packed != color.Packed))
          result[index++] = color;
      }
    }

    // Add primary colors with varying shades
    if (index < colorCount) {
      Bgra8888[] primaryColors = [
        Bgra8888.FromRgb(255, 0, 0),   // Red
        Bgra8888.FromRgb(0, 255, 0),   // Lime
        Bgra8888.FromRgb(0, 0, 255),   // Blue
        Bgra8888.FromRgb(0, 255, 255), // Cyan
        Bgra8888.FromRgb(255, 255, 0), // Yellow
        Bgra8888.FromRgb(255, 0, 255), // Magenta
        Bgra8888.FromRgb(128, 128, 128) // Gray
      ];
      double[] shadeFactors = [1.0, 0.75, 0.5, 0.25, 0.1];

      foreach (var shadeFactor in shadeFactors) {
        foreach (var baseColor in primaryColors) {
          var shadedColor = Bgra8888.Create(
            (byte)(baseColor.C1 * shadeFactor),
            (byte)(baseColor.C2 * shadeFactor),
            (byte)(baseColor.C3 * shadeFactor),
            255
          );

          if (result.Take(index).Any(c => c.Packed == shadedColor.Packed))
            continue;

          result[index++] = shadedColor;
          if (index >= colorCount)
            return result;
        }
      }
    }

    // Fill remaining with pseudo-random colors
    for (; index < colorCount; ++index)
      result[index] = Bgra8888.FromRgb(
        (byte)((index * 37) % 256),
        (byte)((index * 73) % 256),
        (byte)((index * 109) % 256)
      );

    return result;
  }

  /// <summary>
  /// Reduces colors to the specified count. Override to implement specific quantization algorithm.
  /// </summary>
  protected virtual Bgra8888[] _ReduceColorsTo(int colorCount, IEnumerable<Bgra8888> colors)
    => this._ReduceColorsTo(colorCount, colors.Select(c => (c, 1u)));

  /// <summary>
  /// Reduces colors to the specified count using histogram data. Override to implement specific quantization algorithm.
  /// </summary>
  protected abstract Bgra8888[] _ReduceColorsTo(int colorCount, IEnumerable<(Bgra8888 color, uint count)> histogram);

}
