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
/// Pure-histogram simulated-annealing colour quantizer — Kirkpatrick, Gelatt &amp; Vecchi (1983).
/// </summary>
/// <remarks>
/// <para>
/// Minimises the weighted sum-of-squared-error between input histogram and palette by randomly
/// perturbing palette entries and accepting uphill moves with the classical Metropolis probability
/// <c>exp(−ΔE/T)</c>, where <c>T</c> follows a geometric cooling schedule.
/// </para>
/// <para>
/// <b>Difference from <see cref="SpatialColorQuantizer"/>:</b> that quantizer uses deterministic
/// annealing with a spatial-coherence term (pixel-locality penalties on a 2-D image grid). This
/// quantizer operates purely on the colour histogram — no spatial term, no grid — and uses
/// stochastic (Metropolis-Hastings) annealing instead of deterministic. Different dynamics,
/// different assumptions.
/// </para>
/// <para>
/// The algorithm is perturbation-based: small Gaussian jitters on a randomly-chosen palette entry
/// are proposed each step; the cost delta is evaluated incrementally. Well-suited to escape the
/// local optima that K-Means falls into on multimodal histograms, at modestly higher CPU cost.
/// </para>
/// <para>Reference: Kirkpatrick, Gelatt &amp; Vecchi (1983) — "Optimization by Simulated Annealing",
/// Science 220(4598).</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Simulated Annealing", Author = "Kirkpatrick et al.", Year = 1983, QualityRating = 7)]
public struct SimulatedAnnealingQuantizer : IQuantizer {

  /// <summary>Gets or sets the starting temperature of the annealing schedule.</summary>
  public float InitialTemperature { get; set; } = 0.1f;

  /// <summary>Gets or sets the geometric cooling rate applied every iteration.</summary>
  public float CoolingRate { get; set; } = 0.995f;

  /// <summary>Gets or sets the number of Metropolis iterations.</summary>
  public int Iterations { get; set; } = 2000;

  /// <summary>Gets or sets the Gaussian stddev (in normalized colour space) for palette jitter.</summary>
  public float PerturbationSigma { get; set; } = 0.03f;

  /// <summary>Gets or sets the maximum sample size (the cost function scales with n).</summary>
  public int MaxSampleSize { get; set; } = 2048;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public SimulatedAnnealingQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.InitialTemperature, this.CoolingRate, this.Iterations,
    this.PerturbationSigma, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    float initialTemperature,
    float coolingRate,
    int iterations,
    float perturbationSigma,
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
      var px = new double[n];
      var py = new double[n];
      var pz = new double[n];
      var pa = new double[n];
      var pw = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        px[i] = c1.ToFloat();
        py[i] = c2.ToFloat();
        pz[i] = c3.ToFloat();
        pa[i] = a.ToFloat();
        pw[i] = Math.Max(1, colors[i].count);
      }

      // Seed palette via K-Means++ style weighted sampling (same approach used elsewhere).
      var random = new Random(seed);
      var palette = _Seed(px, py, pz, pa, pw, n, k, random);
      var currentCost = _Cost(px, py, pz, pa, pw, n, palette, k);
      var bestCost = currentCost;
      var best = (double[,])palette.Clone();

      var t = (double)initialTemperature;
      for (var step = 0; step < iterations && t > 1e-6; ++step) {
        // Perturb a randomly chosen palette entry by a small gaussian jitter.
        var slot = random.Next(k);
        var dx = _Gauss(random) * perturbationSigma;
        var dy = _Gauss(random) * perturbationSigma;
        var dz = _Gauss(random) * perturbationSigma;
        var saved0 = palette[slot, 0];
        var saved1 = palette[slot, 1];
        var saved2 = palette[slot, 2];
        palette[slot, 0] = Math.Max(0, Math.Min(1, saved0 + dx));
        palette[slot, 1] = Math.Max(0, Math.Min(1, saved1 + dy));
        palette[slot, 2] = Math.Max(0, Math.Min(1, saved2 + dz));

        var newCost = _Cost(px, py, pz, pa, pw, n, palette, k);
        var delta = newCost - currentCost;
        var accept = delta < 0 || random.NextDouble() < Math.Exp(-delta / t);
        if (accept) {
          currentCost = newCost;
          if (currentCost < bestCost) {
            bestCost = currentCost;
            best = (double[,])palette.Clone();
          }
        } else {
          palette[slot, 0] = saved0;
          palette[slot, 1] = saved1;
          palette[slot, 2] = saved2;
        }

        t *= coolingRate;
      }

      var result = new TWork[k];
      for (var j = 0; j < k; ++j)
        result[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)best[j, 0]),
          UNorm32.FromFloatClamped((float)best[j, 1]),
          UNorm32.FromFloatClamped((float)best[j, 2]),
          UNorm32.FromFloatClamped((float)best[j, 3]));
      return result;
    }

    private static double[,] _Seed(double[] x, double[] y, double[] z, double[] a, double[] w, int n, int k, Random r) {
      var pal = new double[k, 4];
      // First centroid chosen by weighted random.
      var tot = 0.0;
      for (var i = 0; i < n; ++i) tot += w[i];
      var t = r.NextDouble() * tot;
      var c = 0.0; var idx = 0;
      for (var i = 0; i < n; ++i) {
        c += w[i];
        if (c < t) continue;
        idx = i; break;
      }
      pal[0, 0] = x[idx]; pal[0, 1] = y[idx]; pal[0, 2] = z[idx]; pal[0, 3] = a[idx];

      var d2 = new double[n];
      for (var i = 0; i < n; ++i) {
        var dx = x[i] - pal[0, 0];
        var dy = y[i] - pal[0, 1];
        var dz = z[i] - pal[0, 2];
        d2[i] = dx * dx + dy * dy + dz * dz;
      }
      for (var j = 1; j < k; ++j) {
        tot = 0.0;
        for (var i = 0; i < n; ++i) tot += d2[i] * w[i];
        t = r.NextDouble() * tot;
        c = 0.0; idx = 0;
        for (var i = 0; i < n; ++i) {
          c += d2[i] * w[i];
          if (c < t) continue;
          idx = i; break;
        }
        pal[j, 0] = x[idx]; pal[j, 1] = y[idx]; pal[j, 2] = z[idx]; pal[j, 3] = a[idx];
        for (var i = 0; i < n; ++i) {
          var dx = x[i] - pal[j, 0];
          var dy = y[i] - pal[j, 1];
          var dz = z[i] - pal[j, 2];
          var dd = dx * dx + dy * dy + dz * dz;
          if (dd < d2[i]) d2[i] = dd;
        }
      }
      return pal;
    }

    private static double _Cost(double[] x, double[] y, double[] z, double[] a, double[] w, int n, double[,] pal, int k) {
      var total = 0.0;
      for (var i = 0; i < n; ++i) {
        var best = double.MaxValue;
        for (var j = 0; j < k; ++j) {
          var dx = x[i] - pal[j, 0];
          var dy = y[i] - pal[j, 1];
          var dz = z[i] - pal[j, 2];
          var d = dx * dx + dy * dy + dz * dz;
          if (d < best) best = d;
        }
        total += best * w[i];
      }
      return total;
    }

    private static double _Gauss(Random r) {
      // Box-Muller.
      double u1 = 1.0 - r.NextDouble();
      double u2 = 1.0 - r.NextDouble();
      return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
  }
}
