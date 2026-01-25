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

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Uniform grid color quantizer with configurable parameters.
/// </summary>
/// <remarks>
/// Divides color space into uniform cells and averages colors in each cell.
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Uniform", QualityRating = 2)]
public struct UniformQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      switch (colorCount) {
        case <= 0:
          return [];
        case 1:
          return [histogram.FirstOrDefault().color];
      }

      return _GenerateUniformPalette(colorCount);
    }

    private static TWork[] _GenerateUniformPalette(int colorCount) {
      // Special case: 2 colors should be black and white for grayscale compatibility
      if (colorCount == 2)
        return [
          ColorFactory.FromNormalized_4<TWork>(UNorm32.Zero, UNorm32.Zero, UNorm32.Zero, UNorm32.One),
          ColorFactory.FromNormalized_4<TWork>(UNorm32.One, UNorm32.One, UNorm32.One, UNorm32.One)
        ];

      // Calculate optimal levels per channel to get close to colorCount colors
      // Try to find r×g×b that is as close to colorCount as possible without exceeding it
      var baseLevels = (int)Math.Floor(Math.Pow(colorCount, 1.0 / 3.0));
      baseLevels = Math.Max(2, Math.Min(baseLevels, 8));

      // Find the best combination of levels that gives us <= colorCount colors
      int rLevels = baseLevels, gLevels = baseLevels, bLevels = baseLevels;
      var product = rLevels * gLevels * bLevels;

      // Try to increase levels one at a time to get closer to colorCount
      while (true) {
        var tryR = (rLevels + 1) * gLevels * bLevels;
        var tryG = rLevels * (gLevels + 1) * bLevels;
        var tryB = rLevels * gLevels * (bLevels + 1);

        if (tryR <= colorCount && rLevels < 8 && tryR > product) {
          ++rLevels;
          product = tryR;
        } else if (tryG <= colorCount && gLevels < 8 && tryG > product) {
          ++gLevels;
          product = tryG;
        } else if (tryB <= colorCount && bLevels < 8 && tryB > product) {
          ++bLevels;
          product = tryB;
        } else
          break;
      }

      // Generate the cube with calculated levels
      var result = new List<TWork>(product);
      var rStep = rLevels > 1 ? 1.0f / (rLevels - 1) : 0f;
      var gStep = gLevels > 1 ? 1.0f / (gLevels - 1) : 0f;
      var bStep = bLevels > 1 ? 1.0f / (bLevels - 1) : 0f;

      for (var r = 0; r < rLevels; ++r)
      for (var g = 0; g < gLevels; ++g)
      for (var b = 0; b < bLevels; ++b)
        result.Add(ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(r * rStep),
          UNorm32.FromFloatClamped(g * gStep),
          UNorm32.FromFloatClamped(b * bStep),
          UNorm32.One
        ));

      return result.ToArray();
    }

  }
}
