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
/// Growing Neural Gas colour quantizer — Fritzke (1995).
/// </summary>
/// <remarks>
/// <para>
/// Starts with two prototypes and <i>grows</i> the network dynamically, inserting
/// a new unit every <see cref="InsertionInterval"/> presentations between the
/// highest-accumulated-error unit and its worst-error neighbour, until <c>k</c>
/// prototypes exist. Each step also updates the winner and its topological
/// neighbours, ages edges, and prunes old edges / orphaned units.
/// </para>
/// <para>
/// Distinct from the existing fixed-size <see cref="NeuralGasQuantizer"/> (Martinetz
/// &amp; Schulten 1991, all prototypes exist from the start with rank-based soft
/// updates) — GNG grows the network where the data demands it, yielding more
/// faithful density tracking on strongly-skewed colour distributions at the
/// cost of a slightly more complex implementation.
/// </para>
/// <para>
/// Reference: Bernd Fritzke (1995) — "A Growing Neural Gas Network Learns
/// Topologies", Advances in Neural Information Processing Systems 7:625-632.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Neural, DisplayName = "Growing Neural Gas", Author = "B. Fritzke", Year = 1995, QualityRating = 8)]
public struct GrowingNeuralGasQuantizer : IQuantizer {

  /// <summary>Gets or sets the total number of sample presentations.</summary>
  public int MaxSteps { get; set; } = 20000;

  /// <summary>Gets or sets the insertion interval (steps between new unit insertions).</summary>
  public int InsertionInterval { get; set; } = 200;

  /// <summary>Gets or sets the winner learning rate ε_b.</summary>
  public float WinnerLearningRate { get; set; } = 0.2f;

  /// <summary>Gets or sets the neighbour learning rate ε_n.</summary>
  public float NeighbourLearningRate { get; set; } = 0.006f;

  /// <summary>Gets or sets the error reduction factor α applied at insertion.</summary>
  public float ErrorReductionAlpha { get; set; } = 0.5f;

  /// <summary>Gets or sets the global error decay factor d per step.</summary>
  public float ErrorDecay { get; set; } = 0.995f;

  /// <summary>Gets or sets the maximum edge age before pruning.</summary>
  public int MaxEdgeAge { get; set; } = 50;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public GrowingNeuralGasQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MaxSteps, this.InsertionInterval, this.WinnerLearningRate, this.NeighbourLearningRate,
    this.ErrorReductionAlpha, this.ErrorDecay, this.MaxEdgeAge, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int maxSteps, int insertionInterval, float winnerLearningRate, float neighbourLearningRate,
    float errorReductionAlpha, float errorDecay, int maxEdgeAge, int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0) return [];
      if (colors.Length <= k) return colors.Select(c => c.color);

      var sampled = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);
      var n = sampled.Length;
      var x = new double[n]; var y = new double[n]; var z = new double[n]; var a = new double[n]; var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, al) = sampled[i].color.ToNormalized();
        x[i] = c1.ToFloat(); y[i] = c2.ToFloat(); z[i] = c3.ToFloat(); a[i] = al.ToFloat();
        w[i] = Math.Max(1, sampled[i].count);
      }

      var random = new Random(seed);
      // Seed with two random units.
      var ux = new List<double>(); var uy = new List<double>(); var uz = new List<double>(); var ua = new List<double>();
      var err = new List<double>();
      // Adjacency matrix as age dictionary (i,j) -> age, i<j.
      var age = new Dictionary<long, int>();
      var i1 = _WeightedPick(w, n, random);
      ux.Add(x[i1]); uy.Add(y[i1]); uz.Add(z[i1]); ua.Add(a[i1]); err.Add(0);
      var i2 = _WeightedPick(w, n, random);
      ux.Add(x[i2]); uy.Add(y[i2]); uz.Add(z[i2]); ua.Add(a[i2]); err.Add(0);

      long EdgeKey(int i, int j) => i < j ? (long)i * 100000 + j : (long)j * 100000 + i;

      for (var step = 1; step <= maxSteps; ++step) {
        // Weighted sample.
        var pi = _WeightedPick(w, n, random);
        var px = x[pi]; var py = y[pi]; var pz = z[pi]; var pa = a[pi];

        // Find two nearest units.
        int b1 = 0, b2 = 1; var d1 = double.MaxValue; var d2 = double.MaxValue;
        for (var u = 0; u < ux.Count; ++u) {
          var dx = ux[u] - px; var dy = uy[u] - py; var dz = uz[u] - pz; var da = ua[u] - pa;
          var d = dx * dx + dy * dy + dz * dz + da * da;
          if (d < d1) { d2 = d1; b2 = b1; d1 = d; b1 = u; }
          else if (d < d2) { d2 = d; b2 = u; }
        }

        // Age all edges incident to b1.
        var touched = new List<long>();
        foreach (var key in age.Keys) {
          var i = (int)(key / 100000); var j = (int)(key % 100000);
          if (i == b1 || j == b1) touched.Add(key);
        }
        foreach (var key in touched) age[key]++;

        // Accumulate squared error on b1.
        err[b1] += d1;

        // Move winner and its topological neighbours.
        ux[b1] += winnerLearningRate * (px - ux[b1]);
        uy[b1] += winnerLearningRate * (py - uy[b1]);
        uz[b1] += winnerLearningRate * (pz - uz[b1]);
        ua[b1] += winnerLearningRate * (pa - ua[b1]);
        foreach (var key in age.Keys.ToList()) {
          var i = (int)(key / 100000); var j = (int)(key % 100000);
          int nb = -1;
          if (i == b1) nb = j; else if (j == b1) nb = i;
          if (nb < 0) continue;
          ux[nb] += neighbourLearningRate * (px - ux[nb]);
          uy[nb] += neighbourLearningRate * (py - uy[nb]);
          uz[nb] += neighbourLearningRate * (pz - uz[nb]);
          ua[nb] += neighbourLearningRate * (pa - ua[nb]);
        }

        // Create / refresh edge (b1,b2).
        age[EdgeKey(b1, b2)] = 0;

        // Remove edges older than MaxEdgeAge; prune orphaned units.
        var old = age.Where(kv => kv.Value > maxEdgeAge).Select(kv => kv.Key).ToList();
        foreach (var key in old) age.Remove(key);
        if (old.Count > 0) {
          var connected = new HashSet<int>();
          foreach (var key in age.Keys) {
            connected.Add((int)(key / 100000)); connected.Add((int)(key % 100000));
          }
          // Remove orphaned units, but keep at least 2 total.
          for (var u = ux.Count - 1; u >= 0 && ux.Count > 2; --u) {
            if (connected.Contains(u)) continue;
            _RemoveUnit(u, ux, uy, uz, ua, err, age);
          }
        }

        // Insert new unit periodically.
        if (step % insertionInterval == 0 && ux.Count < k) {
          // q = arg max err.
          var q = 0; var qv = err[0];
          for (var u = 1; u < ux.Count; ++u) if (err[u] > qv) { qv = err[u]; q = u; }
          // f = neighbour of q with max error.
          var f = -1; var fv = double.NegativeInfinity;
          foreach (var key in age.Keys) {
            var i = (int)(key / 100000); var j = (int)(key % 100000);
            int nb = -1;
            if (i == q) nb = j; else if (j == q) nb = i;
            if (nb < 0) continue;
            if (err[nb] > fv) { fv = err[nb]; f = nb; }
          }
          if (f >= 0) {
            ux.Add(0.5 * (ux[q] + ux[f]));
            uy.Add(0.5 * (uy[q] + uy[f]));
            uz.Add(0.5 * (uz[q] + uz[f]));
            ua.Add(0.5 * (ua[q] + ua[f]));
            err.Add(0.5 * (err[q] + err[f]));
            var r = ux.Count - 1;
            // Replace (q,f) with (q,r) and (r,f).
            age.Remove(EdgeKey(q, f));
            age[EdgeKey(q, r)] = 0;
            age[EdgeKey(r, f)] = 0;
            err[q] *= errorReductionAlpha;
            err[f] *= errorReductionAlpha;
          }
        }

        // Global error decay.
        for (var u = 0; u < err.Count; ++u) err[u] *= errorDecay;
      }

      // If grown fewer than k prototypes, pad by splitting most populated.
      while (ux.Count < k) {
        var q = 0; var qv = err[0];
        for (var u = 1; u < ux.Count; ++u) if (err[u] > qv) { qv = err[u]; q = u; }
        ux.Add(ux[q] + 1e-4); uy.Add(uy[q] + 1e-4); uz.Add(uz[q] + 1e-4); ua.Add(ua[q]);
        err.Add(err[q] * 0.5); err[q] *= 0.5;
      }
      // If grown more than k, merge the closest pair repeatedly.
      while (ux.Count > k) {
        var bi = 0; var bj = 1; var bd = double.MaxValue;
        for (var i = 0; i < ux.Count; ++i)
          for (var j = i + 1; j < ux.Count; ++j) {
            var dx = ux[i] - ux[j]; var dy = uy[i] - uy[j]; var dz = uz[i] - uz[j];
            var d = dx * dx + dy * dy + dz * dz;
            if (d < bd) { bd = d; bi = i; bj = j; }
          }
        ux[bi] = 0.5 * (ux[bi] + ux[bj]); uy[bi] = 0.5 * (uy[bi] + uy[bj]);
        uz[bi] = 0.5 * (uz[bi] + uz[bj]); ua[bi] = 0.5 * (ua[bi] + ua[bj]);
        _RemoveUnit(bj, ux, uy, uz, ua, err, age);
      }

      var palette = new TWork[k];
      for (var j = 0; j < k; ++j)
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ux[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, uy[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, uz[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ua[j]))));
      return palette;
    }

    private static int _WeightedPick(double[] w, int n, Random r) {
      var tot = 0.0;
      for (var i = 0; i < n; ++i) tot += w[i];
      var t = r.NextDouble() * tot;
      var c = 0.0;
      for (var i = 0; i < n; ++i) {
        c += w[i];
        if (c >= t) return i;
      }
      return n - 1;
    }

    private static void _RemoveUnit(
      int u, List<double> ux, List<double> uy, List<double> uz, List<double> ua, List<double> err,
      Dictionary<long, int> age) {
      ux.RemoveAt(u); uy.RemoveAt(u); uz.RemoveAt(u); ua.RemoveAt(u); err.RemoveAt(u);
      // Reindex edge keys.
      var copy = new Dictionary<long, int>();
      foreach (var kv in age) {
        var i = (int)(kv.Key / 100000); var j = (int)(kv.Key % 100000);
        if (i == u || j == u) continue;
        var ni = i > u ? i - 1 : i;
        var nj = j > u ? j - 1 : j;
        var nk = ni < nj ? (long)ni * 100000 + nj : (long)nj * 100000 + ni;
        copy[nk] = kv.Value;
      }
      age.Clear();
      foreach (var kv in copy) age[kv.Key] = kv.Value;
    }

  }
}
