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

      return _GenerateUniformPalette(colorCount).ToArray();
    }

    private static List<TWork> _GenerateUniformPalette(int colorCount) {
      // Special case: 2 colors should be black and white for grayscale compatibility
      if (colorCount == 2) {
        return [
          ColorFactory.FromNormalized_4<TWork>(UNorm32.Zero, UNorm32.Zero, UNorm32.Zero, UNorm32.One),
          ColorFactory.FromNormalized_4<TWork>(UNorm32.One, UNorm32.One, UNorm32.One, UNorm32.One)
        ];
      }

      var levelsPerChannel = (int)Math.Ceiling(Math.Pow(colorCount, 1.0 / 3.0));
      levelsPerChannel = Math.Max(2, Math.Min(levelsPerChannel, 8));

      var step = 1.0f / (levelsPerChannel - 1);
      var result = new List<TWork>(levelsPerChannel * levelsPerChannel * levelsPerChannel);

      for (var c1 = 0; c1 < levelsPerChannel; ++c1)
      for (var c2 = 0; c2 < levelsPerChannel; ++c2)
      for (var c3 = 0; c3 < levelsPerChannel; ++c3) {
        result.Add(ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(c1 * step),
          UNorm32.FromFloatClamped(c2 * step),
          UNorm32.FromFloatClamped(c3 * step),
          UNorm32.One
        ));

        if (result.Count >= colorCount)
          return result;
      }

      return result;
    }

  }
}
