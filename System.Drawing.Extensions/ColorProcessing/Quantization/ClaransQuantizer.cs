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
/// CLARANS (Clustering Large Applications based on RANdomized Search) colour quantizer —
/// Ng &amp; Han (1994 / 2002).
/// </summary>
/// <remarks>
/// <para>
/// Randomised-neighbour K-Medoids variant. Where <see cref="KMedoidsQuantizer"/> uses PAM
/// (checks <i>all</i> swaps) or CLARA (runs PAM on several random samples), CLARANS walks a
/// randomised subset of the neighbour graph: at each step it picks a random medoid slot and a
/// random non-medoid candidate and accepts the swap if it improves the objective. After
/// <c>MaxNeighbors</c> consecutive non-improving trials the current medoid set is treated as a
/// local optimum and recorded; the process restarts from a fresh random seed for
/// <c>NumLocal</c> restarts, and the best local optimum is returned.
/// </para>
/// <para>
/// <b>Distinct from PAM:</b> PAM is deterministic and explores every pair (O(n²) per iteration).
/// CLARANS samples the swap space, making it tractable on very large data where PAM's
/// all-pairs sweep is infeasible.
/// </para>
/// <para>
/// <b>Distinct from CLARA:</b> CLARA reduces <i>input</i> by sampling; CLARANS reduces the
/// <i>search space</i> by sampling. CLARA can miss good medoids that weren't in any sample;
/// CLARANS cannot because it operates on the full dataset.
/// </para>
/// <para>Reference: Ng &amp; Han (1994) — "Efficient and Effective Clustering Methods for Spatial
/// Data Mining", VLDB 1994.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "CLARANS", Author = "Ng & Han", Year = 1994, QualityRating = 8)]
public struct ClaransQuantizer : IQuantizer {

  /// <summary>Gets or sets the number of randomized local-search restarts.</summary>
  public int NumLocal { get; set; } = 3;

  /// <summary>Gets or sets the maximum number of consecutive non-improving neighbour trials
  /// before concluding a local optimum has been reached.</summary>
  public int MaxNeighbors { get; set; } = 25;

  /// <summary>Gets or sets the maximum sample size for processing.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public ClaransQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.NumLocal, this.MaxNeighbors, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int numLocal, int maxNeighbors, int maxSampleSize, int seed) : IQuantizer<TWork>
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
      var x = new double[n]; var y = new double[n]; var z = new double[n]; var a = new double[n];
      var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, ca) = colors[i].color.ToNormalized();
        x[i] = c1.ToFloat(); y[i] = c2.ToFloat(); z[i] = c3.ToFloat(); a[i] = ca.ToFloat();
        w[i] = Math.Max(1, colors[i].count);
      }

      int[]? bestMedoids = null;
      var bestCost = double.MaxValue;

      for (var restart = 0; restart < numLocal; ++restart) {
        var rng = new Random(seed + restart * 31);
        var medoids = _InitMedoids(x, y, z, a, w, n, k, rng);
        var cost = _Cost(x, y, z, a, w, n, medoids, k);
        var nonImprov = 0;
        while (nonImprov < maxNeighbors) {
          var slot = rng.Next(k);
          var cand = rng.Next(n);
          if (Array.IndexOf(medoids, cand) >= 0) { ++nonImprov; continue; }
          var saved = medoids[slot];
          medoids[slot] = cand;
          var newCost = _Cost(x, y, z, a, w, n, medoids, k);
          if (newCost < cost) {
            cost = newCost;
            nonImprov = 0;
          } else {
            medoids[slot] = saved;
            ++nonImprov;
          }
        }
        if (cost >= bestCost) continue;
        bestCost = cost;
        bestMedoids = (int[])medoids.Clone();
      }

      bestMedoids ??= Enumerable.Range(0, k).ToArray();
      return bestMedoids.Select(i => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)x[i]),
        UNorm32.FromFloatClamped((float)y[i]),
        UNorm32.FromFloatClamped((float)z[i]),
        UNorm32.FromFloatClamped((float)a[i])));
    }

    private static int[] _InitMedoids(double[] x, double[] y, double[] z, double[] a, double[] w, int n, int k, Random r) {
      var pick = new int[k];
      var taken = new HashSet<int>();
      // Weighted K-Means++ style.
      var tot = 0.0;
      for (var i = 0; i < n; ++i) tot += w[i];
      var t = r.NextDouble() * tot;
      var c = 0.0; var idx = 0;
      for (var i = 0; i < n; ++i) { c += w[i]; if (c < t) continue; idx = i; break; }
      pick[0] = idx; taken.Add(idx);
      var d2 = new double[n];
      for (var i = 0; i < n; ++i) {
        var dx = x[i] - x[idx]; var dy = y[i] - y[idx]; var dz = z[i] - z[idx]; var da = a[i] - a[idx];
        d2[i] = dx * dx + dy * dy + dz * dz + da * da;
      }
      for (var j = 1; j < k; ++j) {
        tot = 0.0;
        for (var i = 0; i < n; ++i) tot += d2[i] * w[i];
        t = r.NextDouble() * tot;
        c = 0.0; idx = 0;
        for (var i = 0; i < n; ++i) { c += d2[i] * w[i]; if (c < t) continue; idx = i; break; }
        // Ensure distinct.
        var tries = 0;
        while (taken.Contains(idx) && tries < n) { idx = (idx + 1) % n; ++tries; }
        pick[j] = idx; taken.Add(idx);
        for (var i = 0; i < n; ++i) {
          var dx = x[i] - x[idx]; var dy = y[i] - y[idx]; var dz = z[i] - z[idx]; var da = a[i] - a[idx];
          var dd = dx * dx + dy * dy + dz * dz + da * da;
          if (dd < d2[i]) d2[i] = dd;
        }
      }
      return pick;
    }

    private static double _Cost(double[] x, double[] y, double[] z, double[] a, double[] w, int n, int[] medoids, int k) {
      var total = 0.0;
      for (var i = 0; i < n; ++i) {
        var best = double.MaxValue;
        for (var j = 0; j < k; ++j) {
          var m = medoids[j];
          var dx = x[i] - x[m]; var dy = y[i] - y[m]; var dz = z[i] - z[m]; var da = a[i] - a[m];
          var d = dx * dx + dy * dy + dz * dz + da * da;
          if (d < best) best = d;
        }
        total += best * w[i];
      }
      return total;
    }
  }
}
