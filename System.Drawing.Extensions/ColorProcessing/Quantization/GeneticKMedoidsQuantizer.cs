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
/// Genetic K-Medoids colour quantizer — Lucasius, Dane &amp; Kateman (1993).
/// </summary>
/// <remarks>
/// <para>
/// A genetic algorithm whose genome is a <i>medoid-index vector</i> — an array
/// of <c>k</c> distinct indices into the histogram. This is the discrete /
/// combinatorial complement to the existing <see cref="GeneticCMeansQuantizer"/>
/// (continuous centroid encoding, Gaussian mutation, uniform crossover).
/// </para>
/// <para>
/// Each generation performs tournament selection, one-point crossover on the
/// index vectors, and point-mutation (replace one index with a random
/// histogram entry). Fitness is the negative weighted sum of nearest-medoid
/// distances; the best individual is tracked as elite. The resulting palette
/// always consists of <i>actual input colours</i> (like
/// <see cref="KMedoidsQuantizer"/>), but GA escapes local optima that trap
/// PAM's greedy swap loop on multimodal histograms.
/// </para>
/// <para>
/// Reference: C.B. Lucasius, A.D. Dane &amp; G. Kateman (1993) — "On k-medoid
/// clustering of large data sets with the aid of a genetic algorithm",
/// Analytica Chimica Acta 282(3):647-669.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Genetic K-Medoids", Author = "Lucasius, Dane & Kateman", Year = 1993, QualityRating = 8)]
public struct GeneticKMedoidsQuantizer : IQuantizer {

  /// <summary>Gets or sets the population size.</summary>
  public int PopulationSize { get; set; } = 24;

  /// <summary>Gets or sets the number of generations.</summary>
  public int Generations { get; set; } = 50;

  /// <summary>Gets or sets the tournament size.</summary>
  public int TournamentSize { get; set; } = 3;

  /// <summary>Gets or sets the per-gene mutation probability.</summary>
  public float MutationRate { get; set; } = 0.05f;

  /// <summary>Gets or sets the number of elite individuals preserved each generation.</summary>
  public int EliteCount { get; set; } = 2;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = 2048;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public GeneticKMedoidsQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.PopulationSize, this.Generations, this.TournamentSize, this.MutationRate,
    this.EliteCount, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int populationSize, int generations, int tournamentSize, float mutationRate,
    int eliteCount, int maxSampleSize, int seed) : IQuantizer<TWork>
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
      var pop = new int[populationSize][];
      var fit = new double[populationSize];

      for (var p = 0; p < populationSize; ++p) {
        pop[p] = _RandomGenome(n, k, random);
        fit[p] = -_Cost(x, y, z, a, w, n, pop[p]);
      }

      for (var gen = 0; gen < generations; ++gen) {
        var sorted = Enumerable.Range(0, populationSize).OrderByDescending(i => fit[i]).ToArray();
        var newPop = new int[populationSize][];
        var newFit = new double[populationSize];
        // Elites.
        for (var i = 0; i < eliteCount && i < populationSize; ++i) {
          newPop[i] = (int[])pop[sorted[i]].Clone();
          newFit[i] = fit[sorted[i]];
        }
        for (var i = eliteCount; i < populationSize; ++i) {
          var p1 = _Tournament(pop, fit, tournamentSize, random);
          var p2 = _Tournament(pop, fit, tournamentSize, random);
          var child = _Crossover(p1, p2, k, n, random);
          _Mutate(child, n, mutationRate, random);
          newPop[i] = child;
          newFit[i] = -_Cost(x, y, z, a, w, n, child);
        }
        pop = newPop;
        fit = newFit;
      }

      var bestIdx = 0;
      for (var p = 1; p < populationSize; ++p) if (fit[p] > fit[bestIdx]) bestIdx = p;

      var palette = new TWork[k];
      for (var j = 0; j < k; ++j) {
        var idx = pop[bestIdx][j];
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)x[idx]),
          UNorm32.FromFloatClamped((float)y[idx]),
          UNorm32.FromFloatClamped((float)z[idx]),
          UNorm32.FromFloatClamped((float)a[idx]));
      }
      return palette;
    }

    private static int[] _RandomGenome(int n, int k, Random r) {
      // Reservoir sample k distinct indices in [0, n).
      var g = new int[k];
      var set = new HashSet<int>();
      while (set.Count < k) set.Add(r.Next(n));
      var j = 0;
      foreach (var idx in set) g[j++] = idx;
      return g;
    }

    private static int[] _Tournament(int[][] pop, double[] fit, int ts, Random r) {
      var bi = r.Next(pop.Length); var bf = fit[bi];
      for (var t = 1; t < ts; ++t) {
        var i = r.Next(pop.Length);
        if (fit[i] <= bf) continue;
        bi = i; bf = fit[i];
      }
      return pop[bi];
    }

    private static int[] _Crossover(int[] p1, int[] p2, int k, int n, Random r) {
      // Point crossover preserving distinctness.
      var cut = r.Next(1, k);
      var child = new int[k];
      var used = new HashSet<int>();
      for (var i = 0; i < cut; ++i) { child[i] = p1[i]; used.Add(p1[i]); }
      var j = cut;
      for (var i = 0; i < k && j < k; ++i) {
        if (used.Contains(p2[i])) continue;
        child[j++] = p2[i]; used.Add(p2[i]);
      }
      // Fill any remaining slots with random distinct indices.
      while (j < k) {
        var idx = r.Next(n);
        if (used.Add(idx)) child[j++] = idx;
      }
      return child;
    }

    private static void _Mutate(int[] g, int n, float rate, Random r) {
      var used = new HashSet<int>(g);
      for (var i = 0; i < g.Length; ++i) {
        if (r.NextDouble() >= rate) continue;
        used.Remove(g[i]);
        int idx;
        do { idx = r.Next(n); } while (!used.Add(idx));
        g[i] = idx;
      }
    }

    private static double _Cost(double[] x, double[] y, double[] z, double[] a, double[] w, int n, int[] medoids) {
      var total = 0.0;
      for (var i = 0; i < n; ++i) {
        var best = double.MaxValue;
        foreach (var m in medoids) {
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
