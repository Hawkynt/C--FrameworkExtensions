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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Color-distance metric used by <see cref="AduQuantizer"/> when selecting winning
/// and neighbor palette units during competitive learning.
/// </summary>
public enum AduMetric {
  /// <summary>Classic 3-channel Euclidean squared distance (default).</summary>
  Euclidean = 0,
  /// <summary>Sum of absolute per-channel differences.</summary>
  Manhattan,
  /// <summary>Maximum of absolute per-channel differences.</summary>
  Chebyshev,
}

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
/// <para>
/// The <see cref="Metric"/> property selects the color-distance function driving
/// winner selection. Default is <see cref="AduMetric.Euclidean"/> for backward compat.
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

  /// <summary>
  /// Gets or sets the color-distance metric used for winner/neighbor selection.
  /// </summary>
  public AduMetric Metric { get; set; } = AduMetric.Euclidean;

  public AduQuantizer() { }

  public AduQuantizer(int iterationCount, AduMetric metric = AduMetric.Euclidean) {
    this.IterationCount = iterationCount;
    this.Metric = metric;
  }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => this.Metric switch {
    AduMetric.Manhattan => new Kernel<TWork, ManhattanAduDistance>(this.IterationCount),
    AduMetric.Chebyshev => new Kernel<TWork, ChebyshevAduDistance>(this.IterationCount),
    _ => new Kernel<TWork, EuclideanAduDistance>(this.IterationCount),
  };

  internal sealed class Kernel<TWork, TDistance>(int iterationCount) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork>
    where TDistance : struct, IAduDistance {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      // TDistance is a struct parameter — the JIT specialises this method per
      // concrete struct, so the Distance call below inlines to the underlying
      // per-metric formula with zero dispatch overhead.
      var distance = default(TDistance);
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
          var minDistance = distance.Distance(input.c1, input.c2, input.c3, palette[0].c1, palette[0].c2, palette[0].c3);
          for (var i = 1; i < palette.Count; ++i) {
            var current = distance.Distance(input.c1, input.c2, input.c3, palette[i].c1, palette[i].c2, palette[i].c3);
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
            var neighborDistance = distance.Distance(winner.c1, winner.c2, winner.c3, neighbor.c1, neighbor.c2, neighbor.c3);

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

    private static void _Shuffle<T>(T[] array, Random random) {
      for (var i = array.Length - 1; i > 0; --i) {
        var j = random.Next(i + 1);
        (array[i], array[j]) = (array[j], array[i]);
      }
    }
  }
}

/// <summary>
/// Zero-cost distance dispatch for <see cref="AduQuantizer.Kernel{TWork, TDistance}"/>.
/// Implementors are <see langword="struct"/>s with <c>[MethodImpl(AggressiveInlining)]</c>
/// so the JIT inlines the formula at every call site; no virtual dispatch, no enum switch.
/// </summary>
internal interface IAduDistance {
  double Distance(float c1a, float c2a, float c3a, float c1b, float c2b, float c3b);
}

/// <summary>Squared Euclidean distance in RGB (or any 3-channel) space.</summary>
internal readonly struct EuclideanAduDistance : IAduDistance {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Distance(float c1a, float c2a, float c3a, float c1b, float c2b, float c3b) {
    var d1 = c1a - c1b;
    var d2 = c2a - c2b;
    var d3 = c3a - c3b;
    return d1 * d1 + d2 * d2 + d3 * d3;
  }
}

/// <summary>Manhattan (L1) distance in RGB (or any 3-channel) space.</summary>
internal readonly struct ManhattanAduDistance : IAduDistance {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Distance(float c1a, float c2a, float c3a, float c1b, float c2b, float c3b)
    => Math.Abs(c1a - c1b) + Math.Abs(c2a - c2b) + Math.Abs(c3a - c3b);
}

/// <summary>Chebyshev (L∞) distance in RGB (or any 3-channel) space.</summary>
internal readonly struct ChebyshevAduDistance : IAduDistance {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Distance(float c1a, float c2a, float c3a, float c1b, float c2b, float c3b) {
    var a1 = Math.Abs(c1a - c1b);
    var a2 = Math.Abs(c2a - c2b);
    var a3 = Math.Abs(c3a - c3b);
    return Math.Max(a1, Math.Max(a2, a3));
  }
}
