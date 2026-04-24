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
/// Agglomerative hierarchical color quantizer using Ward linkage (minimum variance criterion).
/// </summary>
/// <remarks>
/// <para>
/// Starts with each unique color as its own cluster and iteratively merges the pair whose fusion
/// yields the smallest increase in total within-cluster variance (Ward linkage). Stops when exactly
/// <c>k</c> clusters remain; emits weighted centroids as palette entries.
/// </para>
/// <para>
/// Ward linkage optimises the sum-of-squared-errors criterion globally (subject to the greedy merge
/// order), making it a useful reference quantizer — it tends to produce compact, similarly-sized
/// clusters and is deterministic given a fixed input ordering.
/// </para>
/// <para>
/// Memory is O(m²) where <c>m</c> is the number of starting clusters after sampling, so we clamp
/// input via <see cref="MaxSampleSize"/> (default 512 — matrices beyond this become uncomfortably
/// large and slow with the O(m³) naive implementation).
/// </para>
/// <para>Reference: Ward (1963) — "Hierarchical Grouping to Optimize an Objective Function", Journal of the American Statistical Association.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Agglomerative Ward", Author = "J.H. Ward", Year = 1963, QualityRating = 8)]
public struct AgglomerativeWardQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum number of initial clusters (unique colors). Inputs beyond this are
  /// reduced via weighted reservoir sampling.
  /// </summary>
  public int MaxSampleSize { get; set; } = 512;

  /// <summary>
  /// Gets or sets the deterministic random seed used for sampling.
  /// </summary>
  public int Seed { get; set; } = 42;

  public AgglomerativeWardQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= k)
        return colors.Select(c => c.color);

      // Clamp input — Ward linkage with a naive distance matrix is O(m²) memory, O(m³) time.
      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);

      var m = colors.Length;
      if (m <= k)
        return colors.Select(c => c.color);

      // Initialize each sample as its own singleton cluster.
      var c1 = new double[m];
      var c2 = new double[m];
      var c3 = new double[m];
      var ca = new double[m];
      var weight = new double[m];
      var active = new bool[m];

      for (var i = 0; i < m; ++i) {
        var (n1, n2, n3, na) = colors[i].color.ToNormalized();
        c1[i] = n1.ToFloat();
        c2[i] = n2.ToFloat();
        c3[i] = n3.ToFloat();
        ca[i] = na.ToFloat();
        weight[i] = Math.Max(1, colors[i].count);
        active[i] = true;
      }

      // Precompute Ward-linkage pairwise costs: Δ = (wA·wB / (wA + wB)) * ||cA − cB||²
      var cost = new double[m, m];
      for (var i = 0; i < m; ++i) {
        for (var j = i + 1; j < m; ++j) {
          var d1 = c1[i] - c1[j];
          var d2 = c2[i] - c2[j];
          var d3 = c3[i] - c3[j];
          var d4 = ca[i] - ca[j];
          var sq = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
          var linkage = (weight[i] * weight[j]) / (weight[i] + weight[j]) * sq;
          cost[i, j] = linkage;
          cost[j, i] = linkage;
        }
      }

      var remaining = m;
      while (remaining > k) {
        // Find the cheapest Ward merge among all active pairs.
        var bestI = -1;
        var bestJ = -1;
        var bestCost = double.MaxValue;

        for (var i = 0; i < m; ++i) {
          if (!active[i])
            continue;

          for (var j = i + 1; j < m; ++j) {
            if (!active[j])
              continue;

            var c = cost[i, j];
            if (!(c < bestCost))
              continue;

            bestCost = c;
            bestI = i;
            bestJ = j;
          }
        }

        if (bestI < 0)
          break;

        // Merge j into i (weighted centroid), deactivate j.
        var wi = weight[bestI];
        var wj = weight[bestJ];
        var newWeight = wi + wj;
        c1[bestI] = (c1[bestI] * wi + c1[bestJ] * wj) / newWeight;
        c2[bestI] = (c2[bestI] * wi + c2[bestJ] * wj) / newWeight;
        c3[bestI] = (c3[bestI] * wi + c3[bestJ] * wj) / newWeight;
        ca[bestI] = (ca[bestI] * wi + ca[bestJ] * wj) / newWeight;
        weight[bestI] = newWeight;
        active[bestJ] = false;

        // Recompute row/column bestI against all other active clusters.
        for (var p = 0; p < m; ++p) {
          if (p == bestI || !active[p])
            continue;

          var d1 = c1[bestI] - c1[p];
          var d2 = c2[bestI] - c2[p];
          var d3 = c3[bestI] - c3[p];
          var d4 = ca[bestI] - ca[p];
          var sq = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
          var linkage = (weight[bestI] * weight[p]) / (weight[bestI] + weight[p]) * sq;
          cost[bestI, p] = linkage;
          cost[p, bestI] = linkage;
        }

        --remaining;
      }

      var palette = new List<TWork>(k);
      for (var i = 0; i < m; ++i) {
        if (!active[i])
          continue;

        palette.Add(ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c1[i]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c2[i]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c3[i]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ca[i])))
        ));
      }

      return palette;
    }

  }
}
