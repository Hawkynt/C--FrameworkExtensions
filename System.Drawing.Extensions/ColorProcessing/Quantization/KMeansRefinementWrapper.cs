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
/// Wrapper that refines any quantizer's output using iterative K-means-style clustering.
/// </summary>
/// <remarks>
/// <para>
/// After initial palette generation by the wrapped quantizer, this wrapper repeatedly
/// reassigns colors to closest palette entries and recalculates palette colors as
/// weighted cluster centroids.
/// </para>
/// <para>
/// This refinement process typically improves palette quality by optimizing the
/// color-to-palette mapping, especially for quantizers that may produce suboptimal
/// initial results.
/// </para>
/// </remarks>
/// <typeparam name="TInner">The type of the wrapped quantizer.</typeparam>
[Quantizer(QuantizationType.Postprocessing, DisplayName = "K-Means Refinement", QualityRating = 0)]
public readonly struct KMeansRefinementWrapper<TInner> : IQuantizer
  where TInner : struct, IQuantizer {

  private readonly TInner _inner;
  private readonly int _iterations;

  /// <summary>Default number of refinement iterations.</summary>
  public const int DefaultIterations = 10;

  /// <summary>
  /// Creates a refinement wrapper around the specified quantizer.
  /// </summary>
  /// <param name="inner">The quantizer to wrap.</param>
  /// <param name="iterations">Number of refinement iterations. Default is 10.</param>
  public KMeansRefinementWrapper(TInner inner, int iterations = DefaultIterations) {
    this._inner = inner;
    this._iterations = Math.Max(1, iterations);
  }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>()
    => new Kernel<TWork>(((IQuantizer)this._inner).CreateKernel<TWork>(), this._iterations);

  private sealed class Kernel<TWork>(IQuantizer<TWork> innerKernel, int iterations) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      var histArray = histogram.ToArray();
      if (histArray.Length == 0)
        return [];

      // Get initial palette from wrapped quantizer
      var palette = innerKernel.GeneratePalette(histArray, colorCount);
      if (palette.Length == 0)
        return palette;

      // Convert histogram to float arrays for faster computation
      var colors = histArray.Select(h => {
        var (c1, c2, c3, a) = h.color.ToNormalized();
        return (c1: c1.ToFloat(), c2: c2.ToFloat(), c3: c3.ToFloat(), a: a.ToFloat(), count: h.count);
      }).ToArray();

      // Iterative refinement
      for (var iter = 0; iter < iterations; ++iter) {
        // Convert palette to float for distance calculation
        var paletteFloats = palette.Select(p => {
          var (c1, c2, c3, a) = p.ToNormalized();
          return (c1: c1.ToFloat(), c2: c2.ToFloat(), c3: c3.ToFloat(), a: a.ToFloat());
        }).ToArray();

        // Assign each color to nearest palette entry
        var assignments = new int[colors.Length];
        for (var i = 0; i < colors.Length; ++i) {
          var color = colors[i];
          var minDist = float.MaxValue;
          var minIdx = 0;

          for (var j = 0; j < paletteFloats.Length; ++j) {
            var pc = paletteFloats[j];
            var dist = (color.c1 - pc.c1) * (color.c1 - pc.c1) +
                       (color.c2 - pc.c2) * (color.c2 - pc.c2) +
                       (color.c3 - pc.c3) * (color.c3 - pc.c3) +
                       (color.a - pc.a) * (color.a - pc.a);

            if (dist >= minDist)
              continue;

            minDist = dist;
            minIdx = j;
          }

          assignments[i] = minIdx;
        }

        // Compute new palette as weighted centroids
        var sums = new (double c1, double c2, double c3, double a, double weight)[palette.Length];
        for (var i = 0; i < colors.Length; ++i) {
          var slot = assignments[i];
          sums[slot].c1 += colors[i].c1 * colors[i].count;
          sums[slot].c2 += colors[i].c2 * colors[i].count;
          sums[slot].c3 += colors[i].c3 * colors[i].count;
          sums[slot].a += colors[i].a * colors[i].count;
          sums[slot].weight += colors[i].count;
        }

        var newPalette = new TWork[palette.Length];
        for (var j = 0; j < palette.Length; ++j)
          newPalette[j] = sums[j].weight > 0
            ? ColorFactory.FromNormalized_4<TWork>(
              UNorm32.FromFloatClamped((float)(sums[j].c1 / sums[j].weight)),
              UNorm32.FromFloatClamped((float)(sums[j].c2 / sums[j].weight)),
              UNorm32.FromFloatClamped((float)(sums[j].c3 / sums[j].weight)),
              UNorm32.FromFloatClamped((float)(sums[j].a / sums[j].weight))
            )
            : palette[j]; // Keep old color if no assignments

        palette = newPalette;
      }

      return palette;
    }
  }
}
