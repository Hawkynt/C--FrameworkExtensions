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
/// Recursive PCA-split color quantizer — splits boxes along the principal axis of their weighted covariance.
/// </summary>
/// <remarks>
/// <para>
/// Classical Median-Cut splits at the midpoint of the longest RGB axis; this quantizer instead splits
/// along the <b>principal component</b> of the cluster's weighted covariance, which is the direction
/// of maximum variance. Points are projected onto that axis and split at the weighted median
/// projection. The cluster with the largest total variance is always selected for the next split,
/// giving a globally variance-reducing recursion.
/// </para>
/// <para>
/// Compared to the included <see cref="PcaQuantizerWrapper"/> (which PCA-transforms the colour space
/// <i>before</i> a downstream quantizer), this is a <b>standalone splitting quantizer</b>: it
/// produces palette entries directly as weighted centroids of its PCA-bisected cells. The result is
/// typically somewhere between Wu (variance-optimal but 3·1-D) and Ward (fully greedy O(m²)) —
/// higher quality than classical Median-Cut, far cheaper than Ward, and deterministic.
/// </para>
/// <para>Reference: Orchard &amp; Bouman (1991) — "Color Quantization of Images", IEEE TSP; extends the
/// principal-axis idea of Wu (1991) and Heckbert (1982) with directed variance splitting.</para>
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "PCA Split", Author = "Orchard & Bouman", Year = 1991, QualityRating = 8)]
public struct PcaSplitQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the number of power-iteration steps per covariance eigendecomposition.
  /// </summary>
  public int PowerIterations { get; set; } = 24;

  /// <summary>
  /// Gets or sets the maximum sample size. The algorithm is O(n·k·log k) after projection.
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>
  /// Gets or sets the deterministic random seed used for sampling and power-iteration seeding.
  /// </summary>
  public int Seed { get; set; } = 42;

  public PcaSplitQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.PowerIterations,
    this.MaxSampleSize,
    this.Seed);

  internal sealed class Kernel<TWork>(
    int powerIterations,
    int maxSampleSize,
    int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private sealed class Cluster {
      public int[] Indices = null!;
      public double Weight;
      public double TotalVariance;
      public double[] Centroid = new double[4];
    }

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
      var x = new double[n, 4];
      var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        x[i, 0] = c1.ToFloat();
        x[i, 1] = c2.ToFloat();
        x[i, 2] = c3.ToFloat();
        x[i, 3] = a.ToFloat();
        w[i] = Math.Max(1, colors[i].count);
      }

      var all = new int[n];
      for (var i = 0; i < n; ++i)
        all[i] = i;

      var root = _MakeCluster(all, x, w);
      var clusters = new List<Cluster> { root };

      // Priority splits: always split the largest-variance cluster next.
      var random = new Random(seed);
      while (clusters.Count < k) {
        // Find highest-variance splittable cluster.
        var best = -1;
        var bestVar = -1.0;
        for (var c = 0; c < clusters.Count; ++c) {
          if (clusters[c].Indices.Length < 2)
            continue;

          if (clusters[c].TotalVariance <= bestVar)
            continue;

          bestVar = clusters[c].TotalVariance;
          best = c;
        }

        if (best < 0)
          break; // no further splits possible

        var split = _SplitOnPrincipalAxis(clusters[best], x, w, random);
        if (split == null)
          break;

        clusters[best] = split.Value.left;
        clusters.Add(split.Value.right);
      }

      var palette = new TWork[clusters.Count];
      for (var c = 0; c < clusters.Count; ++c) {
        var ct = clusters[c];
        palette[c] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ct.Centroid[0]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ct.Centroid[1]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ct.Centroid[2]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ct.Centroid[3])))
        );
      }

      return palette;
    }

    private static Cluster _MakeCluster(int[] indices, double[,] x, double[] w) {
      var c = new Cluster { Indices = indices };
      _RecomputeStats(c, x, w);
      return c;
    }

    private static void _RecomputeStats(Cluster c, double[,] x, double[] w) {
      var total = 0.0;
      var m = new double[4];
      foreach (var i in c.Indices) {
        var wi = w[i];
        total += wi;
        m[0] += x[i, 0] * wi;
        m[1] += x[i, 1] * wi;
        m[2] += x[i, 2] * wi;
        m[3] += x[i, 3] * wi;
      }

      if (total <= 0) {
        c.Weight = 0;
        c.TotalVariance = 0;
        return;
      }

      for (var j = 0; j < 4; ++j)
        m[j] /= total;

      var variance = 0.0;
      foreach (var i in c.Indices) {
        var wi = w[i];
        var d0 = x[i, 0] - m[0];
        var d1 = x[i, 1] - m[1];
        var d2 = x[i, 2] - m[2];
        var d3 = x[i, 3] - m[3];
        variance += wi * (d0 * d0 + d1 * d1 + d2 * d2 + d3 * d3);
      }

      c.Centroid = m;
      c.Weight = total;
      c.TotalVariance = variance;
    }

    private (Cluster left, Cluster right)? _SplitOnPrincipalAxis(Cluster parent, double[,] x, double[] w, Random random) {
      if (parent.Indices.Length < 2)
        return null;

      // Weighted covariance of parent cluster.
      var cov = new double[4, 4];
      foreach (var i in parent.Indices) {
        var wi = w[i];
        var d0 = x[i, 0] - parent.Centroid[0];
        var d1 = x[i, 1] - parent.Centroid[1];
        var d2 = x[i, 2] - parent.Centroid[2];
        var d3 = x[i, 3] - parent.Centroid[3];
        cov[0, 0] += wi * d0 * d0;
        cov[1, 1] += wi * d1 * d1;
        cov[2, 2] += wi * d2 * d2;
        cov[3, 3] += wi * d3 * d3;
        cov[0, 1] += wi * d0 * d1; cov[0, 2] += wi * d0 * d2; cov[0, 3] += wi * d0 * d3;
        cov[1, 2] += wi * d1 * d2; cov[1, 3] += wi * d1 * d3;
        cov[2, 3] += wi * d2 * d3;
      }

      cov[1, 0] = cov[0, 1]; cov[2, 0] = cov[0, 2]; cov[3, 0] = cov[0, 3];
      cov[2, 1] = cov[1, 2]; cov[3, 1] = cov[1, 3];
      cov[3, 2] = cov[2, 3];

      if (parent.Weight > 0)
        for (var i = 0; i < 4; ++i)
          for (var j = 0; j < 4; ++j)
            cov[i, j] /= parent.Weight;

      // Power iteration from a deterministic seed-derived start vector.
      var v = new double[4];
      for (var j = 0; j < 4; ++j)
        v[j] = random.NextDouble() - 0.5;

      _Normalize(v);
      for (var it = 0; it < powerIterations; ++it) {
        var nv = new double[4];
        for (var i = 0; i < 4; ++i)
          for (var j = 0; j < 4; ++j)
            nv[i] += cov[i, j] * v[j];

        if (!_Normalize(nv)) {
          // Degenerate cluster — fall back to widest axis split.
          var widest = 0;
          var widestRange = 0.0;
          for (var ax = 0; ax < 4; ++ax) {
            var lo = double.MaxValue;
            var hi = double.MinValue;
            foreach (var p in parent.Indices) {
              if (x[p, ax] < lo) lo = x[p, ax];
              if (x[p, ax] > hi) hi = x[p, ax];
            }

            var range = hi - lo;
            if (range <= widestRange)
              continue;

            widestRange = range;
            widest = ax;
          }

          v = new double[4];
          v[widest] = 1;
          break;
        }

        v = nv;
      }

      // Project and weighted-median split.
      var proj = new (double p, int idx)[parent.Indices.Length];
      for (var k = 0; k < parent.Indices.Length; ++k) {
        var idx = parent.Indices[k];
        proj[k] = (
          x[idx, 0] * v[0] + x[idx, 1] * v[1] + x[idx, 2] * v[2] + x[idx, 3] * v[3],
          idx);
      }

      Array.Sort(proj, (lhs, rhs) => lhs.p.CompareTo(rhs.p));

      var totalW = 0.0;
      foreach (var p in proj)
        totalW += w[p.idx];

      var halfW = totalW / 2;
      var running = 0.0;
      var splitAt = 0;
      for (var k = 0; k < proj.Length; ++k) {
        running += w[proj[k].idx];
        if (running < halfW)
          continue;

        splitAt = Math.Max(1, Math.Min(proj.Length - 1, k));
        break;
      }

      var leftIdx = new int[splitAt];
      var rightIdx = new int[proj.Length - splitAt];
      for (var k = 0; k < splitAt; ++k)
        leftIdx[k] = proj[k].idx;

      for (var k = splitAt; k < proj.Length; ++k)
        rightIdx[k - splitAt] = proj[k].idx;

      if (leftIdx.Length == 0 || rightIdx.Length == 0)
        return null;

      return (_MakeCluster(leftIdx, x, w), _MakeCluster(rightIdx, x, w));
    }

    private static bool _Normalize(double[] v) {
      var s = 0.0;
      for (var i = 0; i < v.Length; ++i)
        s += v[i] * v[i];

      if (s <= 1e-20)
        return false;

      var inv = 1.0 / Math.Sqrt(s);
      for (var i = 0; i < v.Length; ++i)
        v[i] *= inv;

      return true;
    }

  }
}
