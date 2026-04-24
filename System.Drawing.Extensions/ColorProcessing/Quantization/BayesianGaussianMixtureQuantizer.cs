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
/// Variational-Bayesian Gaussian Mixture Model quantizer — Bishop (2006), Corduneanu &amp;
/// Bishop (2001).
/// </summary>
/// <remarks>
/// <para>
/// Fits a Dirichlet-prior-regularised Gaussian mixture via variational inference. Unlike the
/// plain EM-based <see cref="GaussianMixtureQuantizer"/>, the variational variant places a
/// conjugate Dirichlet prior on the mixing weights; during inference, components whose posterior
/// weight falls below <see cref="WeightPruneThreshold"/> are <i>pruned</i> — the model
/// regularises itself down to the effective number of components actually supported by the data.
/// </para>
/// <para>
/// <b>Why distinct from GaussianMixtureQuantizer:</b> the standard EM-GMM always produces
/// exactly <c>k</c> components, even if the data supports fewer natural modes. Variational-Bayes
/// prunes superfluous components automatically, producing a self-regularising cluster count.
/// </para>
/// <para>Reference: Bishop (2006) — "Pattern Recognition and Machine Learning" §10.2; Corduneanu
/// &amp; Bishop (2001) — "Variational Bayesian Model Selection for Mixture Distributions", AISTATS 2001.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Bayesian GMM", Author = "Bishop", Year = 2006, QualityRating = 8)]
public struct BayesianGaussianMixtureQuantizer : IQuantizer {

  /// <summary>Gets or sets the maximum number of variational-EM iterations.</summary>
  public int MaxIterations { get; set; } = 40;

  /// <summary>Gets or sets the Dirichlet concentration prior α₀ (small ⇒ more pruning).</summary>
  public float DirichletConcentration { get; set; } = 0.01f;

  /// <summary>Gets or sets the posterior-weight threshold below which a component is pruned.</summary>
  public float WeightPruneThreshold { get; set; } = 0.005f;

  /// <summary>Gets or sets the minimum variance (prevents singular covariance matrices).</summary>
  public float MinVariance { get; set; } = 0.0005f;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public BayesianGaussianMixtureQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MaxIterations, this.DirichletConcentration, this.WeightPruneThreshold,
    this.MinVariance, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int maxIterations, float dirichletConcentration, float weightPruneThreshold,
    float minVariance, int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0) return [];
      if (colors.Length <= k) return colors.Select(c => c.color);

      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);
      var n = colors.Length;
      var x = new double[n]; var y = new double[n]; var z = new double[n]; var a = new double[n];
      var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, ca) = colors[i].color.ToNormalized();
        x[i] = c1.ToFloat(); y[i] = c2.ToFloat(); z[i] = c3.ToFloat(); a[i] = ca.ToFloat();
        w[i] = Math.Max(1, colors[i].count);
      }
      var totalW = w.Sum();

      // Start with more components than k to let variational pruning reduce them.
      var numComp = Math.Min(n, k * 2);
      var rng = new Random(seed);
      var means = new (double x, double y, double z, double a)[numComp];
      var variances = new double[numComp];
      var weights = new double[numComp];
      // K-Means++ seeding (truncated).
      means[0] = (x[rng.Next(n)], y[rng.Next(n)], z[rng.Next(n)], a[rng.Next(n)]);
      var d2 = new double[n];
      for (var i = 0; i < n; ++i) {
        var dx = x[i] - means[0].x; var dy = y[i] - means[0].y; var dz = z[i] - means[0].z; var da = a[i] - means[0].a;
        d2[i] = dx * dx + dy * dy + dz * dz + da * da;
      }
      for (var c = 1; c < numComp; ++c) {
        var tot = 0.0;
        for (var i = 0; i < n; ++i) tot += d2[i] * w[i];
        var t = rng.NextDouble() * tot;
        var cu = 0.0; var idx = 0;
        for (var i = 0; i < n; ++i) { cu += d2[i] * w[i]; if (cu < t) continue; idx = i; break; }
        means[c] = (x[idx], y[idx], z[idx], a[idx]);
        for (var i = 0; i < n; ++i) {
          var dx = x[i] - means[c].x; var dy = y[i] - means[c].y; var dz = z[i] - means[c].z; var da = a[i] - means[c].a;
          var dd = dx * dx + dy * dy + dz * dz + da * da;
          if (dd < d2[i]) d2[i] = dd;
        }
      }
      for (var c = 0; c < numComp; ++c) { variances[c] = 0.05; weights[c] = 1.0 / numComp; }

      var resp = new double[n, numComp];

      for (var iter = 0; iter < maxIterations; ++iter) {
        var alpha0 = (double)dirichletConcentration;
        // E-step with Dirichlet-prior responsibility computation.
        for (var i = 0; i < n; ++i) {
          var sum = 0.0;
          var probs = new double[numComp];
          for (var c = 0; c < numComp; ++c) {
            var dx = x[i] - means[c].x; var dy = y[i] - means[c].y; var dz = z[i] - means[c].z;
            var sd2 = dx * dx + dy * dy + dz * dz;
            var vr = Math.Max(variances[c], minVariance);
            var pc = weights[c] * Math.Exp(-sd2 / (2 * vr)) / Math.Pow(2 * Math.PI * vr, 1.5);
            probs[c] = pc;
            sum += pc;
          }
          if (sum > 1e-20)
            for (var c = 0; c < numComp; ++c) resp[i, c] = probs[c] / sum;
          else
            for (var c = 0; c < numComp; ++c) resp[i, c] = 1.0 / numComp;
        }

        // M-step with Dirichlet prior on weights: w_c = (N_c + α₀ - 1) / (N + numComp·(α₀ - 1)).
        for (var c = 0; c < numComp; ++c) {
          double nk = 0, s1 = 0, s2 = 0, s3 = 0, sa = 0;
          for (var i = 0; i < n; ++i) {
            var r = resp[i, c] * w[i];
            nk += r; s1 += x[i] * r; s2 += y[i] * r; s3 += z[i] * r; sa += a[i] * r;
          }
          var effective = Math.Max(0, nk + alpha0 - 1);
          if (nk <= 1e-8) { weights[c] = 0; continue; }
          means[c] = (s1 / nk, s2 / nk, s3 / nk, sa / nk);
          double sVar = 0;
          for (var i = 0; i < n; ++i) {
            var dx = x[i] - means[c].x; var dy = y[i] - means[c].y; var dz = z[i] - means[c].z;
            sVar += (dx * dx + dy * dy + dz * dz) * resp[i, c] * w[i];
          }
          variances[c] = Math.Max(minVariance, sVar / (3 * nk));
          weights[c] = effective / Math.Max(1e-8, totalW + numComp * (alpha0 - 1));
          if (weights[c] < weightPruneThreshold) weights[c] = 0;
        }
        // Normalise.
        var sumW = weights.Sum();
        if (sumW > 1e-12)
          for (var c = 0; c < numComp; ++c) weights[c] /= sumW;
      }

      // Emit the top-k components by posterior weight.
      var final = new List<(TWork color, double w)>();
      for (var c = 0; c < numComp; ++c) {
        if (weights[c] <= 0) continue;
        final.Add((ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, means[c].x))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, means[c].y))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, means[c].z))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, means[c].a)))
        ), weights[c]));
      }
      var top = final.OrderByDescending(p => p.w).Take(k).Select(p => p.color).ToList();
      if (top.Count < k) {
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        top.AddRange(fallback.GeneratePalette(colors, k - top.Count));
      }
      return top.Take(k);
    }
  }
}
