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
/// Density-Peaks color quantizer — non-iterative cluster-centre detection by ρ × δ ranking.
/// </summary>
/// <remarks>
/// <para>For each sample two scalars are computed:</para>
/// <list type="bullet">
/// <item><b>ρ</b> — local density: count (or weight) of neighbours within a cutoff distance.</item>
/// <item><b>δ</b> — separation: the minimum distance to any point with higher density (the
///   maximum pairwise distance is used for the single global-max-density point).</item>
/// </list>
/// <para>Cluster centres are then the points that score high on <c>γ = ρ · δ</c> — they sit
/// in dense regions <i>and</i> are far from anywhere even denser, which is the geometric
/// definition of an isolated mode. The top <c>k</c> γ scores become the palette anchors.</para>
/// <para>Distinct from Mean-Shift (no iterative gradient ascent), DBSCAN/HDBSCAN (no ε-graph
/// connectivity), and KMeans (no Lloyd iteration). Single-pass, deterministic.</para>
/// <para>Reference: Rodriguez &amp; Laio 2014, "Clustering by fast search and find of density
/// peaks", <i>Science</i> 344(6191):1492–1496.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Density Peaks", Author = "Rodriguez & Laio", Year = 2014, QualityRating = 7)]
public struct DensityPeaksQuantizer : IQuantizer {

  /// <summary>
  /// Cutoff distance defining the local-density window in normalized TWork space.
  /// </summary>
  /// <remarks>Smaller values give sharper density peaks; in OkLab ~0.05–0.10 works well.</remarks>
  public float CutoffDistance { get; set; } = 0.08f;

  /// <summary>Maximum sample size — the algorithm is O(n²) in pairwise distances.</summary>
  public int MaxSampleSize { get; set; } = 1024;

  /// <summary>Deterministic seed for the histogram subsampling.</summary>
  public int Seed { get; set; } = 42;

  public DensityPeaksQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.CutoffDistance,
    this.MaxSampleSize,
    this.Seed);

  internal sealed class Kernel<TWork>(
    float cutoffDistance,
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
      var x1 = new double[n];
      var x2 = new double[n];
      var x3 = new double[n];
      var xa = new double[n];
      var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        x1[i] = c1.ToFloat();
        x2[i] = c2.ToFloat();
        x3[i] = c3.ToFloat();
        xa[i] = a.ToFloat();
        w[i] = Math.Max(1, colors[i].count);
      }

      // ρ_i: weighted count of neighbours within cutoff distance.
      var dc2 = (double)cutoffDistance * cutoffDistance;
      var rho = new double[n];
      for (var i = 0; i < n; ++i) {
        var sum = w[i]; // self counts; ensures ρ_i > 0 for isolated points
        for (var j = 0; j < n; ++j) {
          if (j == i) continue;
          var d1 = x1[i] - x1[j];
          var d2 = x2[i] - x2[j];
          var d3 = x3[i] - x3[j];
          var d4 = xa[i] - xa[j];
          var dist2 = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
          if (dist2 <= dc2)
            sum += w[j];
        }
        rho[i] = sum;
      }

      // δ_i: minimum distance to any point with strictly higher density. For the global max,
      // use the maximum pairwise distance (paper's convention — these become the most
      // isolated anchors).
      var delta = new double[n];
      var globalMaxDist2 = 0.0;
      for (var i = 0; i < n; ++i) {
        var minHigher2 = double.MaxValue;
        var maxAny2 = 0.0;
        for (var j = 0; j < n; ++j) {
          if (j == i) continue;
          var d1 = x1[i] - x1[j];
          var d2 = x2[i] - x2[j];
          var d3 = x3[i] - x3[j];
          var d4 = xa[i] - xa[j];
          var dist2 = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
          if (dist2 > maxAny2)
            maxAny2 = dist2;
          if (rho[j] > rho[i] && dist2 < minHigher2)
            minHigher2 = dist2;
        }
        if (minHigher2 == double.MaxValue) {
          // Global density max — gets the maximum-pairwise-distance δ in the paper.
          delta[i] = -1; // sentinel; resolved after the loop with globalMaxDist2.
          if (maxAny2 > globalMaxDist2)
            globalMaxDist2 = maxAny2;
        } else {
          delta[i] = Math.Sqrt(minHigher2);
        }
      }
      var globalMaxDist = Math.Sqrt(globalMaxDist2);
      for (var i = 0; i < n; ++i)
        if (delta[i] < 0)
          delta[i] = globalMaxDist;

      // γ = ρ · δ — the cluster-centre score. Top-k go into the palette.
      var gamma = new (int idx, double score)[n];
      for (var i = 0; i < n; ++i)
        gamma[i] = (i, rho[i] * delta[i]);
      Array.Sort(gamma, (a, b) => b.score.CompareTo(a.score));

      var picked = new List<TWork>(k);
      var taken = Math.Min(k, n);
      for (var i = 0; i < taken; ++i)
        picked.Add(colors[gamma[i].idx].color);

      if (picked.Count >= k)
        return picked;

      // Fallback: pad via Wu on the unpicked colours.
      var pickedIds = new HashSet<int>();
      for (var i = 0; i < taken; ++i)
        pickedIds.Add(gamma[i].idx);
      var residual = new List<(TWork color, uint count)>();
      for (var i = 0; i < n; ++i)
        if (!pickedIds.Contains(i))
          residual.Add((colors[i].color, colors[i].count));
      var remaining = k - picked.Count;
      if (remaining > 0 && residual.Count > 0) {
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        picked.AddRange(fallback.GeneratePalette(residual, remaining));
      }
      return picked;
    }
  }
}
