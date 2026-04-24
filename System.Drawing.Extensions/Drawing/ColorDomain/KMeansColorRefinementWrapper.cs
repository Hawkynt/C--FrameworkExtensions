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
// <https://github.com/Hawkynt+C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Hawkynt.Drawing.ColorDomain;

/// <summary>
/// Wraps any <see cref="IColorQuantizer"/> with iterative k-means cluster reassignment
/// over the produced palette. Each iteration assigns every histogram color to its nearest
/// palette entry under the supplied metric, then recomputes each palette entry as the
/// weighted centroid of its cluster. Improves perceptual quality at the cost of
/// O(iterations × histogramSize × paletteSize) extra work.
/// </summary>
/// <remarks>
/// This is the runtime/Color-domain analogue of upstream's typed
/// <see cref="Hawkynt.ColorProcessing.Quantization.KMeansRefinementWrapper{TInner}"/> —
/// trades the compile-time generic specialisation for the ability to pick the inner
/// quantizer and metric at runtime (e.g. from a CLI flag).
/// </remarks>
public sealed class KMeansColorRefinementWrapper : IColorQuantizer {

  private readonly IColorQuantizer _inner;
  private readonly int _iterations;
  private readonly Func<Color, Color, int> _metric;

  public KMeansColorRefinementWrapper(IColorQuantizer inner, int iterations, Func<Color, Color, int> metric) {
    this._inner = inner ?? throw new ArgumentNullException(nameof(inner));
    if (iterations < 0) throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations must be non-negative");
    this._iterations = iterations;
    this._metric = metric ?? throw new ArgumentNullException(nameof(metric));
  }

  public Color[] ReduceColorsTo(ushort numberOfColors, IEnumerable<Color> usedColors)
    => this.ReduceColorsTo(numberOfColors, usedColors.Select(c => (c, 1u)));

  public Color[] ReduceColorsTo(ushort numberOfColors, IEnumerable<(Color color, uint count)> histogram) {
    var snapshot = histogram.ToList();
    var palette = this._inner.ReduceColorsTo(numberOfColors, snapshot);

    if (palette.Length == 0 || this._iterations <= 0)
      return palette;

    var clusters = new Dictionary<Color, List<(Color color, uint count)>>();
    var nextPalette = new List<Color>(palette.Length);
    var metric = this._metric;

    for (var iter = 0; iter < this._iterations; ++iter) {
      clusters.Clear();
      foreach (var c in palette)
        clusters[c] = [];

      foreach (var (originalColor, count) in snapshot) {
        var closest = palette[0];
        var bestDistance = metric(originalColor, closest);
        for (var i = 1; i < palette.Length; ++i) {
          var d = metric(originalColor, palette[i]);
          if (d >= bestDistance)
            continue;
          bestDistance = d;
          closest = palette[i];
        }
        clusters[closest].Add((originalColor, count));
      }

      nextPalette.Clear();
      foreach (var paletteColor in palette) {
        var cluster = clusters[paletteColor];
        if (cluster.Count == 0) {
          nextPalette.Add(paletteColor);
          continue;
        }

        long sumR = 0, sumG = 0, sumB = 0, totalCount = 0;
        foreach (var (color, count) in cluster) {
          sumR += color.R * count;
          sumG += color.G * count;
          sumB += color.B * count;
          totalCount += count;
        }

        nextPalette.Add(Color.FromArgb(
          (int)Math.Round((double)sumR / totalCount),
          (int)Math.Round((double)sumG / totalCount),
          (int)Math.Round((double)sumB / totalCount)));
      }

      palette = nextPalette.ToArray();
    }

    return palette;
  }
}
