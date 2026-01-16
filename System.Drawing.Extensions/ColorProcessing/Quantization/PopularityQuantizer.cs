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
using System.Linq;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Popularity-based color quantizer with configurable parameters.
/// </summary>
/// <remarks>
/// Selects the most frequently occurring colors.
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Popularity", QualityRating = 3)]
public struct PopularityQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets whether to fill unused palette entries with generated colors.
  /// </summary>
  public bool AllowFillingColors { get; set; } = true;

  public PopularityQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.AllowFillingColors);

  internal sealed class Kernel<TWork>(bool allowFillingColors) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      var result = QuantizerHelper.TryHandleSimpleCases(histogram, colorCount, allowFillingColors, out var used);
      if (result != null)
        return result;

      // Select most popular colors
      var reduced = used.OrderByDescending(h => h.count).Take(colorCount).Select(h => h.color);
      return PaletteFiller.GenerateFinalPalette(reduced, colorCount, allowFillingColors);
    }

  }
}
