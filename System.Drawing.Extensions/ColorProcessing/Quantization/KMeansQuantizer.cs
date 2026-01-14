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
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Implements the K-Means clustering quantization algorithm with K-means++ initialization.
/// Uses squared Euclidean distance for fast comparison.
/// </summary>
/// <remarks>
/// <para>Reference: J. MacQueen 1967 "Some Methods for Classification and Analysis of Multivariate Observations"</para>
/// <para>K-means++ initialization: Arthur &amp; Vassilvitskii 2007 "k-means++: The Advantages of Careful Seeding"</para>
/// <para>See also: https://en.wikipedia.org/wiki/K-means_clustering</para>
/// <para>For alternative distance metrics, use <see cref="KMeansQuantizer{TMetric}"/>.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "K-Means", Author = "J. MacQueen", Year = 1967, QualityRating = 7)]
public class KMeansQuantizer : KMeansQuantizer<EuclideanSquared4B<Bgra8888>> {
  /// <summary>
  /// Creates a K-Means quantizer with default squared Euclidean distance.
  /// </summary>
  public KMeansQuantizer() : base(default) { }
}

/// <summary>
/// Implements the K-Means clustering quantization algorithm with K-means++ initialization
/// and a configurable distance metric.
/// </summary>
/// <typeparam name="TMetric">The color metric type implementing <see cref="IColorMetric{Bgra8888}"/>.</typeparam>
/// <remarks>
/// <para>Reference: J. MacQueen 1967 "Some Methods for Classification and Analysis of Multivariate Observations"</para>
/// <para>K-means++ initialization: Arthur &amp; Vassilvitskii 2007 "k-means++: The Advantages of Careful Seeding"</para>
/// <para>See also: https://en.wikipedia.org/wiki/K-means_clustering</para>
/// </remarks>
public class KMeansQuantizer<TMetric> : QuantizerBase
  where TMetric : struct, IColorMetric<Bgra8888> {

  private readonly TMetric _metric;

  public KMeansQuantizer() : this(default) { }

  /// <summary>
  /// Creates a K-Means quantizer with the specified color metric.
  /// </summary>
  /// <param name="metric">The color metric to use for clustering.</param>
  public KMeansQuantizer(TMetric metric) => this._metric = metric;

  /// <summary>
  /// Gets or sets the maximum number of iterations before stopping.
  /// </summary>
  public int MaxIterations { get; set; } = 100;

  /// <summary>
  /// Gets or sets the convergence threshold (normalized 0-1). Iterations stop when centroid movement falls below this value.
  /// </summary>
  public float ConvergenceThreshold { get; set; } = 0.001f;

  /// <inheritdoc />
  protected override Bgra8888[] _ReduceColorsTo(int colorCount, IEnumerable<(Bgra8888 color, uint count)> histogram) {
    var colors = histogram.ToArray();
    if (colors.Length == 0)
      return [];

    if (colors.Length <= colorCount)
      return colors.Select(c => c.color).ToArray();

    var centroids = this._InitializeCentroidsKMeansPlusPlus(colors, colorCount);
    var assignments = new int[colors.Length];
    var threshold = UNorm32.FromFloatClamped(this.ConvergenceThreshold);

    for (var iteration = 0; iteration < this.MaxIterations; ++iteration) {
      // Assign each color to nearest centroid
      for (var i = 0; i < colors.Length; ++i)
        assignments[i] = this._FindNearestCentroid(colors[i].color, centroids);

      // Calculate new centroids
      var newCentroids = _CalculateCentroids(colors, assignments, colorCount);

      // Check for convergence
      var maxMovement = UNorm32.Zero;
      for (var i = 0; i < colorCount; ++i) {
        var movement = this._metric.Distance(centroids[i], newCentroids[i]);
        if (movement > maxMovement)
          maxMovement = movement;
      }

      centroids = newCentroids;

      if (maxMovement < threshold)
        break;
    }

    return centroids;
  }

  private Bgra8888[] _InitializeCentroidsKMeansPlusPlus((Bgra8888 color, uint count)[] colors, int k) {
    var random = new Random(42); // Fixed seed for reproducibility
    var centroids = new Bgra8888[k];
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
      distances[i] = this._metric.Distance(colors[i].color, centroids[0]);

    // Choose remaining centroids
    for (var c = 1; c < k; ++c) {
      // Calculate weighted probability based on distance
      var totalDist = 0.0;
      for (var i = 0; i < colors.Length; ++i)
        totalDist += (float)distances[i] * colors[i].count;

      if (totalDist <= 0) {
        // All remaining points are identical to existing centroids
        centroids[c] = colors[random.Next(colors.Length)].color;
        continue;
      }

      // Choose next centroid with probability proportional to D(x)
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
        var newDist = this._metric.Distance(colors[i].color, centroids[c]);
        if (newDist < distances[i])
          distances[i] = newDist;
      }
    }

    return centroids;
  }

  private int _FindNearestCentroid(Bgra8888 color, Bgra8888[] centroids) {
    var nearest = 0;
    var minDist = this._metric.Distance(color, centroids[0]);

    for (var i = 1; i < centroids.Length; ++i) {
      var dist = this._metric.Distance(color, centroids[i]);
      if (!(dist < minDist))
        continue;

      minDist = dist;
      nearest = i;
    }

    return nearest;
  }

  private static Bgra8888[] _CalculateCentroids((Bgra8888 color, uint count)[] colors, int[] assignments, int k) {
    var sums = new long[k, 3];
    var counts = new long[k];

    for (var i = 0; i < colors.Length; ++i) {
      var cluster = assignments[i];
      var weight = colors[i].count;
      sums[cluster, 0] += colors[i].color.C1 * weight;
      sums[cluster, 1] += colors[i].color.C2 * weight;
      sums[cluster, 2] += colors[i].color.C3 * weight;
      counts[cluster] += weight;
    }

    var centroids = new Bgra8888[k];
    for (var i = 0; i < k; ++i) {
      if (counts[i] > 0) {
        centroids[i] = Bgra8888.Create(
          (byte)(sums[i, 0] / counts[i]),
          (byte)(sums[i, 1] / counts[i]),
          (byte)(sums[i, 2] / counts[i]),
          255
        );
      } else {
        centroids[i] = Bgra8888.Black;
      }
    }

    return centroids;
  }

}
