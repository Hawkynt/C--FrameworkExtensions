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
/// Internal helper for common quantizer operations.
/// </summary>
internal static class QuantizerHelper {

  /// <summary>
  /// Handles common edge cases and prepares histogram for quantization.
  /// </summary>
  /// <typeparam name="TWork">The color space type.</typeparam>
  /// <param name="histogram">The input histogram of colors and counts.</param>
  /// <param name="colorCount">The requested number of palette colors.</param>
  /// <param name="allowFillingColors">Whether to fill unused palette entries.</param>
  /// <param name="normalizedHistogram">
  /// Output: The deduplicated and normalized histogram for quantization.
  /// Only valid when return value is null.
  /// </param>
  /// <returns>
  /// The final palette if no quantization is needed (0/1 colors or fewer unique colors than requested),
  /// or null if quantization is required.
  /// </returns>
  public static TWork[]? TryHandleSimpleCases<TWork>(
    IEnumerable<(TWork color, uint count)> histogram,
    int colorCount,
    bool allowFillingColors,
    out (TWork color, uint count)[] normalizedHistogram)
    where TWork : unmanaged, IColorSpace4<TWork> {

    // Handle trivial cases
    switch (colorCount) {
      case <= 0:
        normalizedHistogram = [];
        return [];
      case 1: {
        // For single color, find most common
        var mostCommon = histogram
          .GroupBy(h => h.color.ToNormalized())
          .Select(g => (color: g.First().color, count: (uint)g.Sum(h => h.count)))
          .OrderByDescending(h => h.count)
          .FirstOrDefault();
        normalizedHistogram = mostCommon.count > 0 ? [mostCommon] : [];
        return [mostCommon.color];
      }
    }

    // Deduplicate and aggregate by normalized color
    var used = histogram
      .GroupBy(h => h.color.ToNormalized())
      .Select(g => (color: g.First().color, count: (uint)g.Sum(h => h.count)))
      .ToArray();

    // If we have fewer unique colors than requested, no quantization needed
    if (used.Length <= colorCount) {
      normalizedHistogram = used;
      return PaletteFiller.GenerateFinalPalette(used.Select(h => h.color), colorCount, allowFillingColors);
    }

    // Quantization is required
    normalizedHistogram = used;
    return null;
  }

}
