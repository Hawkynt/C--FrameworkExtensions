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
/// K-Means clustering color quantizer with configurable parameters.
/// </summary>
/// <remarks>
/// Uses K-means++ initialization for better convergence.
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "K-Means", Author = "J. MacQueen", Year = 1967, QualityRating = 7)]
public struct KMeansQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum number of iterations before stopping.
  /// </summary>
  public int MaxIterations { get; set; } = 100;

  /// <summary>
  /// Gets or sets the convergence threshold (normalized 0-1).
  /// </summary>
  public float ConvergenceThreshold { get; set; } = 0.001f;

  public KMeansQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.MaxIterations, this.ConvergenceThreshold);

  internal sealed class Kernel<TWork>(int maxIterations, float convergenceThreshold) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= colorCount)
        return colors.Select(c => c.color);

      var metric = new Euclidean4N<TWork>();
      var centroids = _InitializeCentroidsKMeansPlusPlus(colors, colorCount, metric);
      var assignments = new int[colors.Length];
      var threshold = UNorm32.FromFloatClamped(convergenceThreshold);

      for (var iteration = 0; iteration < maxIterations; ++iteration) {
        // Assign each color to nearest centroid
        for (var i = 0; i < colors.Length; ++i)
          assignments[i] = _FindNearestCentroid(colors[i].color, centroids, metric);

        // Calculate new centroids
        var newCentroids = _CalculateCentroids(colors, assignments, colorCount);

        // Check for convergence
        var maxMovement = UNorm32.Zero;
        for (var i = 0; i < colorCount; ++i) {
          var movement = metric.Distance(centroids[i], newCentroids[i]);
          if (movement > maxMovement)
            maxMovement = movement;
        }

        centroids = newCentroids;

        if (maxMovement < threshold)
          break;
      }

      return centroids;
    }

    private static TWork[] _InitializeCentroidsKMeansPlusPlus((TWork color, uint count)[] colors, int k, Euclidean4N<TWork> metric) {
      var random = new Random(42);
      var centroids = new TWork[k];
      var distances = new UNorm32[colors.Length];

      // Choose first centroid randomly (weighted by count)
      var totalWeight = colors.Sum(c => (long)c.count);
      var target = random.NextDouble() * totalWeight;
      long cumulative = 0;
      for (var i = 0; i < colors.Length; ++i) {
        cumulative += colors[i].count;
        if (!(cumulative >= target))
          continue;

        centroids[0] = colors[i].color;
        break;
      }

      // Initialize distances
      for (var i = 0; i < colors.Length; ++i)
        distances[i] = metric.Distance(colors[i].color, centroids[0]);

      // Choose remaining centroids
      for (var c = 1; c < k; ++c) {
        var totalDist = 0.0;
        for (var i = 0; i < colors.Length; ++i)
          totalDist += (float)distances[i] * colors[i].count;

        if (totalDist <= 0) {
          centroids[c] = colors[random.Next(colors.Length)].color;
          continue;
        }

        target = random.NextDouble() * totalDist;
        double cumulativeDist = 0;
        var selectedIndex = 0;
        for (var i = 0; i < colors.Length; ++i) {
          cumulativeDist += (float)distances[i] * colors[i].count;
          if (cumulativeDist >= target) {
            selectedIndex = i;
            break;
          }
        }

        centroids[c] = colors[selectedIndex].color;

        // Update distances (keep minimum)
        for (var i = 0; i < colors.Length; ++i) {
          var newDist = metric.Distance(colors[i].color, centroids[c]);
          if (newDist < distances[i])
            distances[i] = newDist;
        }
      }

      return centroids;
    }

    private static int _FindNearestCentroid(TWork color, TWork[] centroids, Euclidean4N<TWork> metric) {
      var nearest = 0;
      var minDist = metric.Distance(color, centroids[0]);

      for (var i = 1; i < centroids.Length; ++i) {
        var dist = metric.Distance(color, centroids[i]);
        if (!(dist < minDist))
          continue;

        minDist = dist;
        nearest = i;
      }

      return nearest;
    }

    private static TWork[] _CalculateCentroids((TWork color, uint count)[] colors, int[] assignments, int k) {
      // Accumulate in normalized float space
      var c1Sums = new double[k];
      var c2Sums = new double[k];
      var c3Sums = new double[k];
      var aSums = new double[k];
      var weights = new double[k];

      for (var i = 0; i < colors.Length; ++i) {
        var cluster = assignments[i];
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        var weight = colors[i].count;

        c1Sums[cluster] += c1.ToFloat() * weight;
        c2Sums[cluster] += c2.ToFloat() * weight;
        c3Sums[cluster] += c3.ToFloat() * weight;
        aSums[cluster] += a.ToFloat() * weight;
        weights[cluster] += weight;
      }

      var centroids = new TWork[k];
      for (var i = 0; i < k; ++i) {
        if (weights[i] > 0)
          centroids[i] = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped((float)(c1Sums[i] / weights[i])),
            UNorm32.FromFloatClamped((float)(c2Sums[i] / weights[i])),
            UNorm32.FromFloatClamped((float)(c3Sums[i] / weights[i])),
            UNorm32.FromFloatClamped((float)(aSums[i] / weights[i]))
          );
      }

      return centroids;
    }

  }
}

/// <summary>
/// Euclidean distance metric for IColorSpace4 using normalized values.
/// </summary>
internal readonly struct Euclidean4N<TWork> : IColorMetric<TWork>
  where TWork : unmanaged, IColorSpace4<TWork> {

  public UNorm32 Distance(in TWork a, in TWork b) {
    var (a1, a2, a3, aa) = a.ToNormalized();
    var (b1, b2, b3, ba) = b.ToNormalized();

    var d1 = a1.ToFloat() - b1.ToFloat();
    var d2 = a2.ToFloat() - b2.ToFloat();
    var d3 = a3.ToFloat() - b3.ToFloat();
    var da = aa.ToFloat() - ba.ToFloat();

    var distSquared = d1 * d1 + d2 * d2 + d3 * d3 + da * da;
    return UNorm32.FromFloatClamped((float)Math.Sqrt(distSquared) * 0.5f);
  }
}
