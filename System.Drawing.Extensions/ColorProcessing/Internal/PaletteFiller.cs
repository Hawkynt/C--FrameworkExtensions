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
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Internal;

/// <summary>
/// Internal helper for filling palette entries when quantizers produce fewer colors than requested.
/// </summary>
internal static class PaletteFiller {

  /// <summary>
  /// Creates a final palette array from proposed colors, filling remaining slots if allowed.
  /// </summary>
  public static TWork[] GenerateFinalPalette<TWork>(IEnumerable<TWork> proposedPalette, int colorCount, bool allowFillingColors)
    where TWork : unmanaged, IColorSpace4<TWork> {
    var distinctColors = proposedPalette.Distinct().ToArray();
    var result = new TWork[colorCount];
    var index = 0;

    var colorsToTake = Math.Min(distinctColors.Length, colorCount);
    for (; index < colorsToTake; ++index)
      result[index] = distinctColors[index];

    if (!allowFillingColors) {
      var transparent = ColorFactory.FromNormalized_4<TWork>(UNorm32.Zero, UNorm32.Zero, UNorm32.Zero, UNorm32.Zero);
      while (index < colorCount)
        result[index++] = transparent;
      return result;
    }

    // Add basic colors if still space left
    if (index < colorCount) {
      (UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a)[] basicColors = [
        (UNorm32.Zero, UNorm32.Zero, UNorm32.Zero, UNorm32.One),
        (UNorm32.One, UNorm32.One, UNorm32.One, UNorm32.One),
        (UNorm32.Zero, UNorm32.Zero, UNorm32.Zero, UNorm32.Zero)
      ];

      foreach (var (c1, c2, c3, a) in basicColors) {
        if (index >= colorCount)
          break;

        var normalized = (c1, c2, c3, a);
        var exists = false;
        for (var i = 0; i < index; ++i)
          if (result[i].ToNormalized().Equals(normalized)) {
            exists = true;
            break;
          }

        if (!exists)
          result[index++] = ColorFactory.FromNormalized_4<TWork>(c1, c2, c3, a);
      }
    }

    // Add primary colors with varying shades
    if (index < colorCount) {
      (float r, float g, float b)[] primaryColors = [
        (1f, 0f, 0f), (0f, 1f, 0f), (0f, 0f, 1f),
        (0f, 1f, 1f), (1f, 1f, 0f), (1f, 0f, 1f), (0.5f, 0.5f, 0.5f)
      ];
      float[] shadeFactors = [1.0f, 0.75f, 0.5f, 0.25f, 0.1f];

      foreach (var shadeFactor in shadeFactors)
      foreach (var (r, g, b) in primaryColors) {
        var c1 = UNorm32.FromFloatClamped(r * shadeFactor);
        var c2 = UNorm32.FromFloatClamped(g * shadeFactor);
        var c3 = UNorm32.FromFloatClamped(b * shadeFactor);
        var a = UNorm32.One;

        var normalized = (c1, c2, c3, a);
        var exists = false;
        for (var i = 0; i < index; ++i)
          if (result[i].ToNormalized().Equals(normalized)) {
            exists = true;
            break;
          }

        if (exists)
          continue;

        result[index++] = ColorFactory.FromNormalized_4<TWork>(c1, c2, c3, a);
        if (index >= colorCount)
          return result;
      }
    }

    // Fill remaining with pseudo-random colors
    for (; index < colorCount; ++index)
      result[index] = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((index * 37 % 256) / 255f),
        UNorm32.FromFloatClamped((index * 73 % 256) / 255f),
        UNorm32.FromFloatClamped((index * 109 % 256) / 255f),
        UNorm32.One
      );

    return result;
  }
}
