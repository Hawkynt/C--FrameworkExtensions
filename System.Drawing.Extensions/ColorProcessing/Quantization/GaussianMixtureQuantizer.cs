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
/// Gaussian Mixture Model (GMM) color quantizer using EM algorithm.
/// </summary>
/// <remarks>
/// <para>Uses Expectation-Maximization to fit Gaussian distributions to color data.</para>
/// <para>Provides soft clustering where colors can belong to multiple clusters probabilistically.</para>
/// <para>Good for natural images with smooth color transitions.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Gaussian Mixture", Author = "Various", Year = 1977, QualityRating = 8)]
public struct GaussianMixtureQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum number of EM iterations.
  /// </summary>
  public int MaxIterations { get; set; } = 50;

  /// <summary>
  /// Gets or sets the convergence threshold for log-likelihood change.
  /// </summary>
  public float ConvergenceThreshold { get; set; } = 0.0001f;

  /// <summary>
  /// Gets or sets the minimum variance to prevent singular covariance matrices.
  /// </summary>
  public float MinVariance { get; set; } = 0.0001f;

  /// <summary>
  /// Gets or sets the maximum histogram sample size for large images.
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  public GaussianMixtureQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.MaxIterations, this.ConvergenceThreshold, this.MinVariance, this.MaxSampleSize);

  internal sealed class Kernel<TWork>(int maxIterations, float convergenceThreshold, float minVariance, int maxSampleSize) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= k)
        return colors.Select(c => c.color);

      // Sample histogram if too large for iterative processing
      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize);

      // Convert to normalized form for GMM calculations
      var points = new (float c1, float c2, float c3, float a, uint count)[colors.Length];
      for (var i = 0; i < colors.Length; ++i) {
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        points[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat(), colors[i].count);
      }

      // Initialize using K-Means++ for better starting positions
      var means = _InitializeMeans(points, k);
      var variances = new float[k];
      var weights = new float[k];

      for (var i = 0; i < k; ++i) {
        variances[i] = 0.1f; // Initial variance
        weights[i] = 1.0f / k; // Equal initial weights
      }

      var responsibilities = new float[colors.Length, k];
      var prevLogLikelihood = float.MinValue;

      for (var iteration = 0; iteration < maxIterations; ++iteration) {
        // E-step: Calculate responsibilities
        this._ExpectationStep(points, means, variances, weights, responsibilities);

        // M-step: Update parameters
        this._MaximizationStep(points, responsibilities, means, variances, weights);

        // Check convergence
        var logLikelihood = this._CalculateLogLikelihood(points, means, variances, weights);
        if (Math.Abs(logLikelihood - prevLogLikelihood) < convergenceThreshold)
          break;

        prevLogLikelihood = logLikelihood;
      }

      // Convert means back to TWork
      var palette = new TWork[k];
      for (var i = 0; i < k; ++i) {
        palette[i] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(means[i].c1),
          UNorm32.FromFloatClamped(means[i].c2),
          UNorm32.FromFloatClamped(means[i].c3),
          UNorm32.FromFloatClamped(means[i].a)
        );
      }

      return palette;
    }

    private static (float c1, float c2, float c3, float a)[] _InitializeMeans((float c1, float c2, float c3, float a, uint count)[] points, int k) {
      var random = new Random(42);
      var means = new (float c1, float c2, float c3, float a)[k];
      var distances = new float[points.Length];

      // K-Means++ initialization
      var totalWeight = points.Sum(p => (long)p.count);
      var target = random.NextDouble() * totalWeight;
      long cumulative = 0;

      for (var i = 0; i < points.Length; ++i) {
        cumulative += points[i].count;
        if (cumulative >= target) {
          means[0] = (points[i].c1, points[i].c2, points[i].c3, points[i].a);
          break;
        }
      }

      for (var c = 1; c < k; ++c) {
        var totalDistance = 0.0;

        for (var i = 0; i < points.Length; ++i) {
          var minDist = float.MaxValue;
          for (var j = 0; j < c; ++j) {
            var dist = _SquaredDistance(points[i], means[j]);
            if (dist < minDist)
              minDist = dist;
          }
          distances[i] = minDist * points[i].count;
          totalDistance += distances[i];
        }

        target = random.NextDouble() * totalDistance;
        cumulative = 0;

        for (var i = 0; i < points.Length; ++i) {
          cumulative += (long)distances[i];
          if (cumulative >= target) {
            means[c] = (points[i].c1, points[i].c2, points[i].c3, points[i].a);
            break;
          }
        }
      }

      return means;
    }

    private static float _SquaredDistance((float c1, float c2, float c3, float a, uint count) point, (float c1, float c2, float c3, float a) mean) {
      var d1 = point.c1 - mean.c1;
      var d2 = point.c2 - mean.c2;
      var d3 = point.c3 - mean.c3;
      var d4 = point.a - mean.a;
      return d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
    }

    private void _ExpectationStep(
      (float c1, float c2, float c3, float a, uint count)[] points,
      (float c1, float c2, float c3, float a)[] means,
      float[] variances,
      float[] weights,
      float[,] responsibilities) {

      var k = means.Length;

      for (var i = 0; i < points.Length; ++i) {
        var totalProb = 0.0f;
        var probs = new float[k];

        for (var j = 0; j < k; ++j) {
          var sqDist = _SquaredDistance(points[i], means[j]);
          var variance = Math.Max(variances[j], minVariance);

          // Gaussian probability (simplified - using spherical covariance)
          var exponent = -sqDist / (2.0f * variance);
          var prob = weights[j] * (float)Math.Exp(exponent) / (float)Math.Pow(2 * Math.PI * variance, 2);
          probs[j] = prob;
          totalProb += prob;
        }

        if (totalProb > 1e-10f) {
          for (var j = 0; j < k; ++j)
            responsibilities[i, j] = probs[j] / totalProb;
        } else {
          // Equal responsibilities if all probabilities are too small
          for (var j = 0; j < k; ++j)
            responsibilities[i, j] = 1.0f / k;
        }
      }
    }

    private void _MaximizationStep(
      (float c1, float c2, float c3, float a, uint count)[] points,
      float[,] responsibilities,
      (float c1, float c2, float c3, float a)[] means,
      float[] variances,
      float[] weights) {

      var k = means.Length;
      var totalPoints = 0.0f;

      for (var i = 0; i < points.Length; ++i)
        totalPoints += points[i].count;

      for (var j = 0; j < k; ++j) {
        var nk = 0.0f; // Effective number of points in cluster j
        var sumC1 = 0.0f;
        var sumC2 = 0.0f;
        var sumC3 = 0.0f;
        var sumA = 0.0f;

        for (var i = 0; i < points.Length; ++i) {
          var weight = responsibilities[i, j] * points[i].count;
          nk += weight;
          sumC1 += weight * points[i].c1;
          sumC2 += weight * points[i].c2;
          sumC3 += weight * points[i].c3;
          sumA += weight * points[i].a;
        }

        if (nk > 1e-10f) {
          means[j] = (sumC1 / nk, sumC2 / nk, sumC3 / nk, sumA / nk);

          // Update variance (spherical covariance)
          var sumVariance = 0.0f;
          for (var i = 0; i < points.Length; ++i) {
            var weight = responsibilities[i, j] * points[i].count;
            sumVariance += weight * _SquaredDistance(points[i], means[j]);
          }
          variances[j] = Math.Max(sumVariance / (nk * 4), minVariance); // 4 dimensions

          weights[j] = nk / totalPoints;
        } else {
          variances[j] = minVariance;
          weights[j] = 1e-10f;
        }
      }

      // Normalize weights
      var totalWeight = weights.Sum();
      if (totalWeight > 1e-10f)
        for (var j = 0; j < k; ++j)
          weights[j] /= totalWeight;
    }

    private float _CalculateLogLikelihood(
      (float c1, float c2, float c3, float a, uint count)[] points,
      (float c1, float c2, float c3, float a)[] means,
      float[] variances,
      float[] weights) {

      var k = means.Length;
      var logLikelihood = 0.0f;

      for (var i = 0; i < points.Length; ++i) {
        var prob = 0.0f;

        for (var j = 0; j < k; ++j) {
          var sqDist = _SquaredDistance(points[i], means[j]);
          var variance = Math.Max(variances[j], minVariance);
          var exponent = -sqDist / (2.0f * variance);
          prob += weights[j] * (float)Math.Exp(exponent) / (float)Math.Pow(2 * Math.PI * variance, 2);
        }

        if (prob > 1e-10f)
          logLikelihood += points[i].count * (float)Math.Log(prob);
      }

      return logLikelihood;
    }
  }
}
