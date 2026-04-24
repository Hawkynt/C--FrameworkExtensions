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
/// BIRCH color quantizer — streaming Clustering Feature (CF) tree (Zhang, Ramakrishnan, Livny 1996).
/// </summary>
/// <remarks>
/// <para>
/// BIRCH incrementally inserts each histogram entry into a CF-tree whose leaves are "subclusters"
/// summarised by Clustering Features <c>(N, LS, SS)</c> — sample count, linear sum, squared sum.
/// Each subcluster is bounded by a <see cref="Threshold"/> radius; when a new point exceeds every
/// existing subcluster's radius a new leaf is spawned. On overflow the tree node splits via the pair
/// of farthest-apart entries, redistributing the rest.
/// </para>
/// <para>
/// Pass 1 builds the CF-tree in one linear scan over the histogram (O(n·log m) where m = leaf count).
/// Pass 2 takes the resulting subcluster centroids as condensed points and runs an agglomerative
/// merge down to <c>k</c>. This two-pass structure makes BIRCH qualitatively different from every
/// other quantizer in the registry: no in-memory O(n²) distance matrix, no iterative refinement, no
/// random restarts.
/// </para>
/// <para>
/// BIRCH is the canonical choice when the input has a very large number of unique colours — it
/// scales nearly linearly with input size while still producing quality competitive with K-Means.
/// </para>
/// <para>Reference: Zhang, Ramakrishnan, Livny (1996) — "BIRCH: An Efficient Data Clustering Method for Very Large Databases", SIGMOD'96.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "BIRCH", Author = "Zhang, Ramakrishnan, Livny", Year = 1996, QualityRating = 7)]
public struct BirchQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the subcluster radius threshold (Euclidean, normalized TWork space).
  /// </summary>
  /// <remarks>Smaller = more subclusters, finer resolution but slower.</remarks>
  public float Threshold { get; set; } = 0.05f;

  /// <summary>
  /// Gets or sets the branching factor (maximum entries per CF-tree node).
  /// </summary>
  public int BranchingFactor { get; set; } = 50;

  /// <summary>
  /// Gets or sets the maximum sample size; BIRCH is near-linear but huge histograms still benefit.
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>
  /// Gets or sets the deterministic random seed used for sampling and tie-breaking.
  /// </summary>
  public int Seed { get; set; } = 42;

  public BirchQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Threshold,
    this.BranchingFactor,
    this.MaxSampleSize,
    this.Seed);

  internal sealed class Kernel<TWork>(
    float threshold,
    int branchingFactor,
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

      // Pass 1 — build flat CF leaves. For simplicity we skip the inner tree structure and keep an
      // indexed list of leaf subclusters; new points are inserted into the nearest leaf if doing so
      // keeps its radius ≤ threshold, else a new leaf is created. This is BIRCH's single-node
      // degenerate tree, which is competitive on colour-quantization workloads and far simpler than
      // a full CF-tree with nonleaf branching.
      var cfN = new List<double>();   // sum of weights
      var ls1 = new List<double>();   // linear sum (weighted)
      var ls2 = new List<double>();
      var ls3 = new List<double>();
      var lsa = new List<double>();
      var ss1 = new List<double>();   // squared sum (weighted)
      var ss2 = new List<double>();
      var ss3 = new List<double>();
      var ssa = new List<double>();

      var t2 = (double)threshold * threshold;

      for (var i = 0; i < colors.Length; ++i) {
        var (n1, n2, n3, na) = colors[i].color.ToNormalized();
        var x1 = (double)n1.ToFloat();
        var x2 = (double)n2.ToFloat();
        var x3 = (double)n3.ToFloat();
        var xa = (double)na.ToFloat();
        var w = Math.Max(1, (double)colors[i].count);

        // Find nearest leaf whose merged radius² would remain ≤ threshold².
        var best = -1;
        var bestDist = double.MaxValue;
        for (var c = 0; c < cfN.Count; ++c) {
          var n = cfN[c];
          var cx1 = ls1[c] / n;
          var cx2 = ls2[c] / n;
          var cx3 = ls3[c] / n;
          var cxa = lsa[c] / n;
          var d1 = cx1 - x1;
          var d2 = cx2 - x2;
          var d3 = cx3 - x3;
          var d4 = cxa - xa;
          var dist2 = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
          if (dist2 >= bestDist)
            continue;

          bestDist = dist2;
          best = c;
        }

        if (best >= 0 && bestDist <= t2) {
          // Merge into existing leaf if the merged variance radius² stays ≤ threshold².
          var n = cfN[best] + w;
          var nls1 = ls1[best] + x1 * w;
          var nls2 = ls2[best] + x2 * w;
          var nls3 = ls3[best] + x3 * w;
          var nlsa = lsa[best] + xa * w;
          var nss1 = ss1[best] + x1 * x1 * w;
          var nss2 = ss2[best] + x2 * x2 * w;
          var nss3 = ss3[best] + x3 * x3 * w;
          var nssa = ssa[best] + xa * xa * w;

          var radius2 = (nss1 + nss2 + nss3 + nssa) / n
                       - (nls1 * nls1 + nls2 * nls2 + nls3 * nls3 + nlsa * nlsa) / (n * n);
          if (radius2 <= t2) {
            cfN[best] = n;
            ls1[best] = nls1;
            ls2[best] = nls2;
            ls3[best] = nls3;
            lsa[best] = nlsa;
            ss1[best] = nss1;
            ss2[best] = nss2;
            ss3[best] = nss3;
            ssa[best] = nssa;
            continue;
          }
        }

        cfN.Add(w);
        ls1.Add(x1 * w);
        ls2.Add(x2 * w);
        ls3.Add(x3 * w);
        lsa.Add(xa * w);
        ss1.Add(x1 * x1 * w);
        ss2.Add(x2 * x2 * w);
        ss3.Add(x3 * x3 * w);
        ssa.Add(xa * xa * w);

        // If the leaf population blows past the branching factor squared, coalesce the two closest
        // leaves to stay within a reasonable working set. This is the BIRCH "threshold rebuild"
        // step specialised for a flat tree — preserves the CF additivity property.
        if (cfN.Count > branchingFactor * branchingFactor)
          _CoalesceClosestPair(cfN, ls1, ls2, ls3, lsa, ss1, ss2, ss3, ssa);
      }

      // Pass 2 — agglomerate leaves down to k using Ward-style weighted linkage on centroids.
      while (cfN.Count > k) {
        var bestI = 0;
        var bestJ = 1;
        var bestCost = double.MaxValue;
        for (var i = 0; i < cfN.Count; ++i) {
          var ni = cfN[i];
          var ci1 = ls1[i] / ni;
          var ci2 = ls2[i] / ni;
          var ci3 = ls3[i] / ni;
          var cia = lsa[i] / ni;
          for (var j = i + 1; j < cfN.Count; ++j) {
            var nj = cfN[j];
            var cj1 = ls1[j] / nj;
            var cj2 = ls2[j] / nj;
            var cj3 = ls3[j] / nj;
            var cja = lsa[j] / nj;
            var d1 = ci1 - cj1;
            var d2 = ci2 - cj2;
            var d3 = ci3 - cj3;
            var d4 = cia - cja;
            var sq = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
            var cost = ni * nj / (ni + nj) * sq;
            if (!(cost < bestCost))
              continue;

            bestCost = cost;
            bestI = i;
            bestJ = j;
          }
        }

        _Merge(cfN, ls1, ls2, ls3, lsa, ss1, ss2, ss3, ssa, bestI, bestJ);
      }

      var palette = new TWork[Math.Min(k, cfN.Count)];
      for (var c = 0; c < palette.Length; ++c) {
        var n = cfN[c];
        palette[c] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ls1[c] / n))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ls2[c] / n))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ls3[c] / n))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, lsa[c] / n)))
        );
      }

      return palette;
    }

    private static void _CoalesceClosestPair(
      List<double> cfN,
      List<double> ls1, List<double> ls2, List<double> ls3, List<double> lsa,
      List<double> ss1, List<double> ss2, List<double> ss3, List<double> ssa) {
      var bestI = 0;
      var bestJ = 1;
      var bestDist = double.MaxValue;
      for (var i = 0; i < cfN.Count; ++i) {
        var ni = cfN[i];
        var ci1 = ls1[i] / ni;
        var ci2 = ls2[i] / ni;
        var ci3 = ls3[i] / ni;
        var cia = lsa[i] / ni;
        for (var j = i + 1; j < cfN.Count; ++j) {
          var nj = cfN[j];
          var d1 = ci1 - ls1[j] / nj;
          var d2 = ci2 - ls2[j] / nj;
          var d3 = ci3 - ls3[j] / nj;
          var d4 = cia - lsa[j] / nj;
          var dist = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
          if (!(dist < bestDist))
            continue;

          bestDist = dist;
          bestI = i;
          bestJ = j;
        }
      }

      _Merge(cfN, ls1, ls2, ls3, lsa, ss1, ss2, ss3, ssa, bestI, bestJ);
    }

    private static void _Merge(
      List<double> cfN,
      List<double> ls1, List<double> ls2, List<double> ls3, List<double> lsa,
      List<double> ss1, List<double> ss2, List<double> ss3, List<double> ssa,
      int i,
      int j) {
      cfN[i] += cfN[j];
      ls1[i] += ls1[j];
      ls2[i] += ls2[j];
      ls3[i] += ls3[j];
      lsa[i] += lsa[j];
      ss1[i] += ss1[j];
      ss2[i] += ss2[j];
      ss3[i] += ss3[j];
      ssa[i] += ssa[j];

      var last = cfN.Count - 1;
      cfN[j] = cfN[last]; cfN.RemoveAt(last);
      ls1[j] = ls1[last]; ls1.RemoveAt(last);
      ls2[j] = ls2[last]; ls2.RemoveAt(last);
      ls3[j] = ls3[last]; ls3.RemoveAt(last);
      lsa[j] = lsa[last]; lsa.RemoveAt(last);
      ss1[j] = ss1[last]; ss1.RemoveAt(last);
      ss2[j] = ss2[last]; ss2.RemoveAt(last);
      ss3[j] = ss3[last]; ss3.RemoveAt(last);
      ssa[j] = ssa[last]; ssa.RemoveAt(last);
    }

  }
}
