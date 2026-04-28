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
// <https://github.com/Hawkynt@C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Differential Evolution colour quantizer — Storn &amp; Price (1997).
/// </summary>
/// <remarks>
/// <para>
/// Implements the classical <c>DE/rand/1/bin</c> scheme. For each target vector
/// <c>x_i</c> a donor is constructed as <c>v = x_{r1} + F·(x_{r2} − x_{r3})</c>
/// with three distinct random population indices; a trial vector <c>u</c> is
/// produced via binomial crossover between <c>v</c> and <c>x_i</c> at rate
/// <c>Cr</c>; finally <c>u</c> replaces <c>x_i</c> iff its fitness is better
/// (greedy selection). Each individual is a <c>4·k</c>-dim vector encoding a
/// full palette. Fitness is negative MSE vs. the weighted histogram.
/// </para>
/// <para>
/// Complements the existing <see cref="GeneticCMeansQuantizer"/> (standard GA with
/// Gaussian mutation + uniform crossover + C-Means refinement) and the existing
/// <see cref="ParticleSwarmQuantizer"/> (swarm-dynamics metaheuristic). DE uses
/// <i>vector differences</i> as the mutation operator, which adaptively scales
/// step-size to the current population spread — in practice this gives it a
/// strong advantage on highly-correlated fitness landscapes that defeat PSO.
/// </para>
/// <para>
/// Reference: Storn &amp; Price (1997) — "Differential Evolution – A Simple and
/// Efficient Heuristic for Global Optimization over Continuous Spaces",
/// J. Global Optimization 11(4):341-359.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Differential Evolution", Author = "Storn & Price", Year = 1997, QualityRating = 8)]
public struct DifferentialEvolutionQuantizer : IQuantizer {

  /// <summary>Gets or sets the population size <c>NP</c>.</summary>
  public int PopulationSize { get; set; } = 30;

  /// <summary>Gets or sets the number of generations.</summary>
  public int Generations { get; set; } = 60;

  /// <summary>Gets or sets the differential weight <c>F</c> (mutation scale).</summary>
  public float DifferentialWeight { get; set; } = 0.7f;

  /// <summary>Gets or sets the crossover rate <c>Cr</c>.</summary>
  public float CrossoverRate { get; set; } = 0.9f;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public DifferentialEvolutionQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.PopulationSize, this.Generations, this.DifferentialWeight, this.CrossoverRate,
    this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int populationSize, int generations, float differentialWeight, float crossoverRate,
    int maxSampleSize, int seed) : IQuantizer<TWork>
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
      var dim = k * 4;
      var pop = new double[populationSize][];
      var fit = new double[populationSize];
      var np = Math.Max(4, populationSize);

      for (var p = 0; p < np; ++p) {
        pop[p] = _KmPlusPlus(x, y, z, a, w, n, k, random);
        fit[p] = -_Mse(x, y, z, a, w, n, pop[p], k);
      }

      var bestIdx = 0;
      for (var p = 1; p < np; ++p) if (fit[p] > fit[bestIdx]) bestIdx = p;

      var trial = new double[dim];
      for (var gen = 0; gen < generations; ++gen) {
        for (var i = 0; i < np; ++i) {
          // Pick three distinct indices r1,r2,r3 != i.
          int r1, r2, r3;
          do { r1 = random.Next(np); } while (r1 == i);
          do { r2 = random.Next(np); } while (r2 == i || r2 == r1);
          do { r3 = random.Next(np); } while (r3 == i || r3 == r1 || r3 == r2);

          var forced = random.Next(dim);
          for (var d = 0; d < dim; ++d) {
            if (d == forced || random.NextDouble() < crossoverRate) {
              var donor = pop[r1][d] + differentialWeight * (pop[r2][d] - pop[r3][d]);
              if (donor < 0) donor = 0;
              else if (donor > 1) donor = 1;
              trial[d] = donor;
            } else
              trial[d] = pop[i][d];
          }

          var trialFit = -_Mse(x, y, z, a, w, n, trial, k);
          if (trialFit <= fit[i]) continue;
          Array.Copy(trial, pop[i], dim);
          fit[i] = trialFit;
          if (trialFit > fit[bestIdx]) bestIdx = i;
        }
      }

      var palette = new TWork[k];
      for (var j = 0; j < k; ++j)
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)pop[bestIdx][j * 4 + 0]),
          UNorm32.FromFloatClamped((float)pop[bestIdx][j * 4 + 1]),
          UNorm32.FromFloatClamped((float)pop[bestIdx][j * 4 + 2]),
          UNorm32.FromFloatClamped((float)pop[bestIdx][j * 4 + 3]));
      return palette;
    }

    private static double[] _KmPlusPlus(double[] x, double[] y, double[] z, double[] a, double[] w, int n, int k, Random r) {
      var v = new double[k * 4];
      var totalW = 0.0;
      for (var i = 0; i < n; ++i) totalW += w[i];
      var target = r.NextDouble() * totalW;
      var cum = 0.0; var idx = 0;
      for (var i = 0; i < n; ++i) { cum += w[i]; if (cum < target) continue; idx = i; break; }
      v[0] = x[idx]; v[1] = y[idx]; v[2] = z[idx]; v[3] = a[idx];
      var d2 = new double[n];
      for (var i = 0; i < n; ++i) {
        var dx = x[i] - v[0]; var dy = y[i] - v[1]; var dz = z[i] - v[2];
        d2[i] = dx * dx + dy * dy + dz * dz;
      }
      for (var j = 1; j < k; ++j) {
        var tot = 0.0;
        for (var i = 0; i < n; ++i) tot += d2[i] * w[i];
        if (tot <= 0) { idx = r.Next(n); }
        else {
          target = r.NextDouble() * tot; cum = 0.0; idx = 0;
          for (var i = 0; i < n; ++i) { cum += d2[i] * w[i]; if (cum < target) continue; idx = i; break; }
        }
        v[j * 4 + 0] = x[idx]; v[j * 4 + 1] = y[idx]; v[j * 4 + 2] = z[idx]; v[j * 4 + 3] = a[idx];
        for (var i = 0; i < n; ++i) {
          var dx = x[i] - v[j * 4 + 0]; var dy = y[i] - v[j * 4 + 1]; var dz = z[i] - v[j * 4 + 2];
          var dd = dx * dx + dy * dy + dz * dz;
          if (dd < d2[i]) d2[i] = dd;
        }
      }
      return v;
    }

    private static double _Mse(double[] x, double[] y, double[] z, double[] a, double[] w, int n, double[] pal, int k) {
      var total = 0.0; var tw = 0.0;
      for (var i = 0; i < n; ++i) {
        var best = double.MaxValue;
        for (var j = 0; j < k; ++j) {
          var dx = x[i] - pal[j * 4 + 0]; var dy = y[i] - pal[j * 4 + 1];
          var dz = z[i] - pal[j * 4 + 2]; var da = a[i] - pal[j * 4 + 3];
          var d = dx * dx + dy * dy + dz * dz + da * da;
          if (d < best) best = d;
        }
        total += best * w[i]; tw += w[i];
      }
      return tw > 0 ? total / tw : total;
    }

  }
}
