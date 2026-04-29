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
/// Agglomerative Information-Bottleneck color quantizer.
/// </summary>
/// <remarks>
/// <para>Each input colour starts as its own cluster; the algorithm greedily merges the pair
/// of clusters whose merge causes the smallest loss of information about the original
/// colour distribution, repeating until <c>k</c> clusters remain. The merge cost combines
/// (a) the entropy increase of the cluster-weight distribution and (b) the geometric
/// distance between cluster centroids:</para>
/// <code>
///   ΔI(i,j) = (w_i + w_j) · ln((w_i + w_j) / w_total)
///           − w_i · ln(w_i / w_total) − w_j · ln(w_j / w_total)
///           + λ · w_i · w_j / (w_i + w_j) · ‖μ_i − μ_j‖²
/// </code>
/// <para>The entropy term is the agglomerative-IB criterion (Slonim &amp; Tishby), which is
/// independent of geometric distance — purely about how the cluster mass redistributes.
/// The geometric term anchors merges to perceptually-close colours. λ trades the two off.</para>
/// <para>Distinct from <see cref="AgglomerativeWardQuantizer"/> (which uses pure variance and
/// no information term) and from <see cref="EntropyMaximisingQuantizer"/> (which optimises a
/// global entropy objective via a different schedule).</para>
/// <para>Reference: Slonim &amp; Tishby 2000, "Agglomerative Information Bottleneck",
/// NIPS 12 (extends Tishby, Pereira &amp; Bialek 1999, "The Information Bottleneck Method").</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Information Bottleneck", Author = "Slonim & Tishby", Year = 2000, QualityRating = 7)]
public struct InformationBottleneckQuantizer : IQuantizer {

  /// <summary>
  /// Trade-off λ between the information-theoretic entropy term and the geometric distance term.
  /// </summary>
  /// <remarks>0 = pure information bottleneck (entropy only); larger values prefer merging
  /// geometrically-close colours. Default 1 is balanced.</remarks>
  public float Lambda { get; set; } = 1f;

  /// <summary>Maximum sample size — algorithm is O(n³) in the agglomerative loop.</summary>
  public int MaxSampleSize { get; set; } = 512;

  /// <summary>Deterministic seed for the histogram subsampling.</summary>
  public int Seed { get; set; } = 42;

  public InformationBottleneckQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Lambda,
    this.MaxSampleSize,
    this.Seed);

  internal sealed class Kernel<TWork>(
    float lambda,
    int maxSampleSize,
    int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];
      if (colors.Length <= k)
        return colors.Select(c => c.color);

      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);

      var n = colors.Length;
      var c1 = new double[n];
      var c2 = new double[n];
      var c3 = new double[n];
      var ca = new double[n];
      var w = new double[n];
      var alive = new bool[n];
      var totalW = 0.0;
      for (var i = 0; i < n; ++i) {
        var (n1, n2, n3, na) = colors[i].color.ToNormalized();
        c1[i] = n1.ToFloat();
        c2[i] = n2.ToFloat();
        c3[i] = n3.ToFloat();
        ca[i] = na.ToFloat();
        w[i] = Math.Max(1, colors[i].count);
        alive[i] = true;
        totalW += w[i];
      }
      var invTotalW = 1.0 / totalW;

      // Initial pairwise merge-cost matrix (upper triangle). For n = 512 this is 512²/2 =
      // 131 K entries — comfortably fits in working memory.
      var cost = new double[n, n];
      for (var i = 0; i < n; ++i) {
        cost[i, i] = double.MaxValue;
        for (var j = i + 1; j < n; ++j) {
          var c = MergeCost(c1, c2, c3, ca, w, i, j, totalW, invTotalW, lambda);
          cost[i, j] = c;
          cost[j, i] = c;
        }
      }

      var clusters = n;
      while (clusters > k) {
        // Find the cheapest live pair.
        var bestI = -1;
        var bestJ = -1;
        var bestCost = double.MaxValue;
        for (var i = 0; i < n; ++i) {
          if (!alive[i]) continue;
          for (var j = i + 1; j < n; ++j) {
            if (!alive[j]) continue;
            if (cost[i, j] >= bestCost) continue;
            bestCost = cost[i, j];
            bestI = i;
            bestJ = j;
          }
        }
        if (bestI < 0) break;

        // Merge j into i: weighted-mean centroid, sum weights, kill j.
        var wi = w[bestI];
        var wj = w[bestJ];
        var wij = wi + wj;
        c1[bestI] = (c1[bestI] * wi + c1[bestJ] * wj) / wij;
        c2[bestI] = (c2[bestI] * wi + c2[bestJ] * wj) / wij;
        c3[bestI] = (c3[bestI] * wi + c3[bestJ] * wj) / wij;
        ca[bestI] = (ca[bestI] * wi + ca[bestJ] * wj) / wij;
        w[bestI] = wij;
        alive[bestJ] = false;
        --clusters;

        // Recompute costs from i to all live clusters; j is dead.
        for (var t = 0; t < n; ++t) {
          if (t == bestI || !alive[t]) continue;
          var c = MergeCost(c1, c2, c3, ca, w, bestI, t, totalW, invTotalW, lambda);
          cost[bestI, t] = c;
          cost[t, bestI] = c;
        }
      }

      var palette = new List<TWork>(clusters);
      for (var i = 0; i < n; ++i) {
        if (!alive[i]) continue;
        palette.Add(ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c1[i]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c2[i]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c3[i]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ca[i])))));
      }

      if (palette.Count >= k)
        return palette;

      // Pad via Wu on uncovered colours.
      var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
      palette.AddRange(fallback.GeneratePalette(colors, k - palette.Count));
      return palette;
    }

    private static double MergeCost(
      double[] c1, double[] c2, double[] c3, double[] ca, double[] w,
      int i, int j, double totalW, double invTotalW, float lambda) {
      var wi = w[i];
      var wj = w[j];
      var wij = wi + wj;
      // Information term: ΔI = w_ij·ln(w_ij/W) - w_i·ln(w_i/W) - w_j·ln(w_j/W).
      // This is exactly the cluster-mass entropy increase from the agglomerative-IB paper.
      var info = wij * Math.Log(wij * invTotalW)
               - wi * Math.Log(wi * invTotalW)
               - wj * Math.Log(wj * invTotalW);
      // Geometric term: weighted squared centroid distance — Ward-style penalty pulling the
      // merge toward perceptually-close colours so the IB doesn't collapse equally-weighted
      // distant colours.
      var d1 = c1[i] - c1[j];
      var d2 = c2[i] - c2[j];
      var d3 = c3[i] - c3[j];
      var d4 = ca[i] - ca[j];
      var geom = (wi * wj / wij) * (d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4);
      return info + lambda * geom;
    }
  }
}
