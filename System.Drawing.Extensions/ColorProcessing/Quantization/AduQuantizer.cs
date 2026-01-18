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
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Implements Adaptive Distributing Units (ADU) quantization.
/// </summary>
/// <remarks>
/// <para>
/// Uses competitive learning to iteratively refine palette colors,
/// updating the winning unit and its neighbors based on input colors.
/// </para>
/// <para>
/// ADU is related to self-organizing maps but uses a simpler topology-free
/// approach suitable for color quantization.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "ADU", QualityRating = 6)]
public struct AduQuantizer : IQuantizer {

  private const double InitialLearningRate = 0.01;
  private const double MinLearningRate = 0.001;

  /// <summary>
  /// Gets or sets the number of training iterations.
  /// </summary>
  public int IterationCount { get; set; } = 10;

  public AduQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.IterationCount);

  internal sealed class Kernel<TWork>(int iterationCount) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var colorsWithCounts = histogram.ToArray();

      // Initialize units (palette colors) with most frequent colors
      var palette = new List<(float c1, float c2, float c3, float a)>();
      var sortedColors = colorsWithCounts.OrderByDescending(c => c.count).ToArray();

      // Initialize with most frequent colors
      for (var i = 0; i < colorCount && i < sortedColors.Length; ++i) {
        var (c1, c2, c3, a) = sortedColors[i].color.ToNormalized();
        palette.Add((c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat()));
      }

      // Fill remaining slots with evenly spaced colors if needed
      while (palette.Count < colorCount && sortedColors.Length > 0) {
        var step = Math.Max(1, sortedColors.Length / (colorCount - palette.Count));
        for (var i = palette.Count; i < sortedColors.Length && palette.Count < colorCount; i += step) {
          var (c1, c2, c3, a) = sortedColors[i].color.ToNormalized();
          var newColor = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat());
          if (!palette.Contains(newColor))
            palette.Add(newColor);
        }
        break;
      }

      // Convert histogram to float arrays for faster processing
      var inputColors = colorsWithCounts
        .Select(c => {
          var (c1, c2, c3, a) = c.color.ToNormalized();
          return (c1: c1.ToFloat(), c2: c2.ToFloat(), c3: c3.ToFloat(), a: a.ToFloat(), count: c.count);
        })
        .ToArray();

      // Competitive learning iterations
      var random = new Random(42); // Fixed seed for reproducibility
      for (var iteration = 0; iteration < iterationCount; ++iteration) {
        // Exponential decay learning rate
        var learningRate = Math.Max(MinLearningRate,
          InitialLearningRate * Math.Exp(-3.0 * iteration / iterationCount));

        // Shuffle input colors each iteration
        _Shuffle(inputColors, random);

        foreach (var input in inputColors) {
          // Apply count-based weighting
          var weightedLearningRate = learningRate * Math.Min(1.0, Math.Log(input.count + 1) / 10.0);

          // Find winning unit (closest palette color)
          var winningUnitIndex = 0;
          var minDistance = _DistanceSquared(input.c1, input.c2, input.c3, palette[0].c1, palette[0].c2, palette[0].c3);
          for (var i = 1; i < palette.Count; ++i) {
            var current = _DistanceSquared(input.c1, input.c2, input.c3, palette[i].c1, palette[i].c2, palette[i].c3);
            if (current >= minDistance)
              continue;

            minDistance = current;
            winningUnitIndex = i;
          }

          // Update winning unit
          var winner = palette[winningUnitIndex];
          var deltaC1 = input.c1 - winner.c1;
          var deltaC2 = input.c2 - winner.c2;
          var deltaC3 = input.c3 - winner.c3;

          palette[winningUnitIndex] = (
            Math.Clamp((float)(winner.c1 + weightedLearningRate * deltaC1), 0f, 1f),
            Math.Clamp((float)(winner.c2 + weightedLearningRate * deltaC2), 0f, 1f),
            Math.Clamp((float)(winner.c3 + weightedLearningRate * deltaC3), 0f, 1f),
            winner.a
          );

          // Update neighboring units with reduced learning rate
          var neighborLearningRate = weightedLearningRate * 0.1;
          for (var i = 0; i < palette.Count; ++i) {
            if (i == winningUnitIndex)
              continue;

            var neighbor = palette[i];
            var neighborDistance = _DistanceSquared(winner.c1, winner.c2, winner.c3, neighbor.c1, neighbor.c2, neighbor.c3);

            if (neighborDistance >= minDistance * 2)
              continue;

            // Only update close neighbors
            var neighborInfluence = neighborLearningRate * Math.Exp(-neighborDistance * 1000.0);

            palette[i] = (
              Math.Clamp((float)(neighbor.c1 + neighborInfluence * deltaC1), 0f, 1f),
              Math.Clamp((float)(neighbor.c2 + neighborInfluence * deltaC2), 0f, 1f),
              Math.Clamp((float)(neighbor.c3 + neighborInfluence * deltaC3), 0f, 1f),
              neighbor.a
            );
          }
        }
      }

      // Convert palette back to TWork colors
      return palette.Select(p => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(p.c1),
        UNorm32.FromFloatClamped(p.c2),
        UNorm32.FromFloatClamped(p.c3),
        UNorm32.FromFloatClamped(p.a)
      ));
    }

    private static double _DistanceSquared(float c1a, float c2a, float c3a, float c1b, float c2b, float c3b) {
      var d1 = c1a - c1b;
      var d2 = c2a - c2b;
      var d3 = c3a - c3b;
      return d1 * d1 + d2 * d2 + d3 * d3;
    }

    private static void _Shuffle<T>(T[] array, Random random) {
      for (var i = array.Length - 1; i > 0; --i) {
        var j = random.Next(i + 1);
        (array[i], array[j]) = (array[j], array[i]);
      }
    }
  }
}
