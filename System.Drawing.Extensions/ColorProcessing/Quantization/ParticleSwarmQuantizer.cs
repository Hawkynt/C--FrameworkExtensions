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
/// Particle Swarm Optimisation colour quantizer — Kennedy &amp; Eberhart (1995).
/// </summary>
/// <remarks>
/// <para>
/// Each particle in the swarm encodes a full <c>k</c>-entry palette as a point in
/// <c>4·k</c>-dimensional space. Particles move through the search space under the
/// classical velocity update
/// <c>v ← w·v + c₁·r₁·(pBest − x) + c₂·r₂·(gBest − x)</c>, where <c>pBest</c> is the
/// particle's best-known position and <c>gBest</c> is the globally-best position.
/// Fitness is negative MSE between palette and weighted histogram.
/// </para>
/// <para>
/// Compared to the existing <see cref="GeneticCMeansQuantizer"/> (population-level
/// crossover + mutation + C-Means refinement), PSO is a pure-velocity metaheuristic:
/// no crossover, no Lamarckian refinement — just swarm dynamics. It tends to converge
/// faster than GA on smooth fitness landscapes but is more prone to premature
/// convergence on multimodal ones, which is why <see cref="InertiaWeight"/> decays
/// linearly to balance exploration/exploitation.
/// </para>
/// <para>
/// Reference: Kennedy &amp; Eberhart (1995) — "Particle Swarm Optimization",
/// Proc. IEEE International Conference on Neural Networks IV, 1942-1948;
/// Omran, Engelbrecht &amp; Salman (2005) "Dynamic clustering using PSO with application
/// in image segmentation" for the colour-quantization specialisation.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Particle Swarm", Author = "Kennedy & Eberhart", Year = 1995, QualityRating = 8)]
public struct ParticleSwarmQuantizer : IQuantizer {

  /// <summary>Gets or sets the number of particles in the swarm.</summary>
  public int SwarmSize { get; set; } = 24;

  /// <summary>Gets or sets the number of iterations.</summary>
  public int Iterations { get; set; } = 60;

  /// <summary>Gets or sets the initial inertia weight <c>w</c>.</summary>
  public float InitialInertia { get; set; } = 0.9f;

  /// <summary>Gets or sets the final inertia weight <c>w</c>.</summary>
  public float FinalInertia { get; set; } = 0.4f;

  /// <summary>Gets or sets the cognitive coefficient <c>c₁</c> (attraction to pBest).</summary>
  public float CognitiveCoefficient { get; set; } = 1.49f;

  /// <summary>Gets or sets the social coefficient <c>c₂</c> (attraction to gBest).</summary>
  public float SocialCoefficient { get; set; } = 1.49f;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public ParticleSwarmQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.SwarmSize, this.Iterations, this.InitialInertia, this.FinalInertia,
    this.CognitiveCoefficient, this.SocialCoefficient, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int swarmSize, int iterations, float initialInertia, float finalInertia,
    float cognitiveCoefficient, float socialCoefficient, int maxSampleSize, int seed) : IQuantizer<TWork>
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
      var position = new double[swarmSize][];
      var velocity = new double[swarmSize][];
      var pBest = new double[swarmSize][];
      var pBestFit = new double[swarmSize];
      var gBest = new double[dim];
      var gBestFit = double.NegativeInfinity;

      // Initialise swarm: first particle PCA-seeded, the rest K-Means++-seeded.
      var pcaSeed = QuantizerHelper.InitializePaletteWithPCA(sampled, k, seed);
      position[0] = _PaletteToVector(pcaSeed, k);
      velocity[0] = new double[dim];
      for (var p = 1; p < swarmSize; ++p) {
        position[p] = _KmPlusPlus(x, y, z, a, w, n, k, random);
        velocity[p] = new double[dim];
        for (var d = 0; d < dim; ++d) velocity[p][d] = (random.NextDouble() - 0.5) * 0.1;
      }

      for (var p = 0; p < swarmSize; ++p) {
        pBest[p] = (double[])position[p].Clone();
        pBestFit[p] = -_Mse(x, y, z, a, w, n, position[p], k);
        if (pBestFit[p] <= gBestFit) continue;
        gBestFit = pBestFit[p];
        gBest = (double[])position[p].Clone();
      }

      for (var it = 0; it < iterations; ++it) {
        var t = iterations <= 1 ? 1.0 : (double)it / (iterations - 1);
        var inertia = initialInertia + (finalInertia - initialInertia) * t;
        for (var p = 0; p < swarmSize; ++p) {
          for (var d = 0; d < dim; ++d) {
            var r1 = random.NextDouble();
            var r2 = random.NextDouble();
            velocity[p][d] = inertia * velocity[p][d]
              + cognitiveCoefficient * r1 * (pBest[p][d] - position[p][d])
              + socialCoefficient * r2 * (gBest[d] - position[p][d]);
            // Velocity clamp to [-0.2, 0.2] in normalised units for stability.
            if (velocity[p][d] > 0.2) velocity[p][d] = 0.2;
            else if (velocity[p][d] < -0.2) velocity[p][d] = -0.2;
            position[p][d] += velocity[p][d];
            if (position[p][d] < 0) position[p][d] = 0;
            else if (position[p][d] > 1) position[p][d] = 1;
          }
          var fit = -_Mse(x, y, z, a, w, n, position[p], k);
          if (fit <= pBestFit[p]) continue;
          pBestFit[p] = fit;
          pBest[p] = (double[])position[p].Clone();
          if (fit <= gBestFit) continue;
          gBestFit = fit;
          gBest = (double[])position[p].Clone();
        }
      }

      var palette = new TWork[k];
      for (var j = 0; j < k; ++j)
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)gBest[j * 4 + 0]),
          UNorm32.FromFloatClamped((float)gBest[j * 4 + 1]),
          UNorm32.FromFloatClamped((float)gBest[j * 4 + 2]),
          UNorm32.FromFloatClamped((float)gBest[j * 4 + 3]));
      return palette;
    }

    private static double[] _PaletteToVector(TWork[] palette, int k) {
      var v = new double[k * 4];
      for (var j = 0; j < k; ++j) {
        var (c1, c2, c3, a) = palette[j].ToNormalized();
        v[j * 4 + 0] = c1.ToFloat();
        v[j * 4 + 1] = c2.ToFloat();
        v[j * 4 + 2] = c3.ToFloat();
        v[j * 4 + 3] = a.ToFloat();
      }
      return v;
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
