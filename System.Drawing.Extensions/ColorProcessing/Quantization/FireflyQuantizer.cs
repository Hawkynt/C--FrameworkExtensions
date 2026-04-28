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
/// Firefly Algorithm colour quantizer — Yang (2008).
/// </summary>
/// <remarks>
/// <para>
/// Fireflies are candidate palettes. Each firefly has a "brightness" <c>I</c>
/// proportional to its fitness (negative MSE). At every iteration, firefly
/// <c>i</c> moves toward every brighter firefly <c>j</c> with attractiveness
/// <c>β(r) = β₀·exp(−γ·r²)</c> where <c>r</c> is the Euclidean distance between
/// them in <c>4·k</c>-dim palette space. A small random walk term <c>α·(rand−0.5)</c>
/// is added for exploration; <c>α</c> decays geometrically across iterations.
/// </para>
/// <para>
/// Distinct from <see cref="ParticleSwarmQuantizer"/> (single global best attracts
/// all particles) and <see cref="DifferentialEvolutionQuantizer"/> (vector-difference
/// mutation + greedy selection). The firefly rule is <i>pairwise</i>: brightness
/// comparisons are made against every other firefly individually, which tends to
/// preserve population diversity longer and thus handle multi-modal landscapes well.
/// </para>
/// <para>
/// Reference: Xin-She Yang (2008) — "Nature-Inspired Metaheuristic Algorithms",
/// Luniver Press, ch. 8; Yang (2009) "Firefly algorithms for multimodal optimization",
/// Stochastic Algorithms: Foundations and Applications, LNCS 5792:169-178.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Firefly", Author = "X.-S. Yang", Year = 2008, QualityRating = 7)]
public struct FireflyQuantizer : IQuantizer {

  /// <summary>Gets or sets the number of fireflies.</summary>
  public int PopulationSize { get; set; } = 16;

  /// <summary>Gets or sets the number of iterations.</summary>
  public int Iterations { get; set; } = 30;

  /// <summary>Gets or sets the initial randomness factor α.</summary>
  public float InitialAlpha { get; set; } = 0.2f;

  /// <summary>Gets or sets the α decay factor per iteration.</summary>
  public float AlphaDecay { get; set; } = 0.97f;

  /// <summary>Gets or sets the base attractiveness β₀.</summary>
  public float BaseAttractiveness { get; set; } = 1.0f;

  /// <summary>Gets or sets the absorption coefficient γ.</summary>
  public float Absorption { get; set; } = 1.0f;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public FireflyQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.PopulationSize, this.Iterations, this.InitialAlpha, this.AlphaDecay,
    this.BaseAttractiveness, this.Absorption, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int populationSize, int iterations, float initialAlpha, float alphaDecay,
    float baseAttractiveness, float absorption, int maxSampleSize, int seed) : IQuantizer<TWork>
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
      var light = new double[populationSize];
      for (var p = 0; p < populationSize; ++p) {
        pop[p] = _KmPlusPlus(x, y, z, a, w, n, k, random);
        light[p] = -_Mse(x, y, z, a, w, n, pop[p], k);
      }

      var alpha = (double)initialAlpha;
      for (var it = 0; it < iterations; ++it) {
        // Evaluate all brightness values (cached in light[]).
        for (var i = 0; i < populationSize; ++i) {
          for (var j = 0; j < populationSize; ++j) {
            if (light[j] <= light[i]) continue; // only move toward brighter
            var r2 = 0.0;
            for (var d = 0; d < dim; ++d) {
              var diff = pop[i][d] - pop[j][d];
              r2 += diff * diff;
            }
            var beta = baseAttractiveness * Math.Exp(-absorption * r2);
            for (var d = 0; d < dim; ++d) {
              var v = pop[i][d] + beta * (pop[j][d] - pop[i][d]) + alpha * (random.NextDouble() - 0.5);
              if (v < 0) v = 0;
              else if (v > 1) v = 1;
              pop[i][d] = v;
            }
            light[i] = -_Mse(x, y, z, a, w, n, pop[i], k);
          }
        }
        alpha *= alphaDecay;
      }

      var bestIdx = 0;
      for (var p = 1; p < populationSize; ++p) if (light[p] > light[bestIdx]) bestIdx = p;

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
