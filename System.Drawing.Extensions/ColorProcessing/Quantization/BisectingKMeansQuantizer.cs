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
/// Bisecting K-Means color quantizer that recursively splits clusters.
/// </summary>
/// <remarks>
/// <para>Starts with all colors in one cluster, then recursively bisects the cluster with highest SSE.</para>
/// <para>Can produce more balanced clusters than standard K-Means for some color distributions.</para>
/// <para>Deterministic splitting ensures reproducible results.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Bisecting K-Means", Author = "M. Steinbach et al.", Year = 2000, QualityRating = 7)]
public struct BisectingKMeansQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum iterations per bisection.
  /// </summary>
  public int MaxIterationsPerSplit { get; set; } = 10;

  /// <summary>
  /// Gets or sets how many bisection trials to run, keeping the best result.
  /// </summary>
  public int BisectionTrials { get; set; } = 3;

  /// <summary>
  /// Gets or sets the convergence threshold (normalized 0-1).
  /// </summary>
  public float ConvergenceThreshold { get; set; } = 0.001f;

  public BisectingKMeansQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.MaxIterationsPerSplit, this.BisectionTrials, this.ConvergenceThreshold);

  internal sealed class Kernel<TWork>(int maxIterationsPerSplit, int bisectionTrials, float convergenceThreshold) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private sealed class Cluster {
      public List<(TWork color, uint count)> Points { get; } = [];
      public TWork Centroid { get; set; }
      public double Sse { get; set; }
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= k)
        return colors.Select(c => c.color);

      var metric = new Euclidean4N<TWork>();

      // Start with one cluster containing all points
      var initialCluster = new Cluster();
      foreach (var color in colors)
        initialCluster.Points.Add(color);

      initialCluster.Centroid = _CalculateCentroid(initialCluster.Points);
      initialCluster.Sse = _CalculateSse(initialCluster.Points, initialCluster.Centroid, metric);

      var clusters = new List<Cluster> { initialCluster };

      // Bisect until we have k clusters
      while (clusters.Count < k) {
        // Find cluster with highest SSE
        var maxSse = double.MinValue;
        var clusterToSplit = 0;

        for (var i = 0; i < clusters.Count; ++i)
          if (clusters[i].Sse > maxSse && clusters[i].Points.Count > 1) {
            maxSse = clusters[i].Sse;
            clusterToSplit = i;
          }

        // If no cluster can be split, break
        if (clusters[clusterToSplit].Points.Count <= 1)
          break;

        // Bisect the cluster
        var (cluster1, cluster2) = this._BisectCluster(clusters[clusterToSplit], metric);

        // Replace the original cluster with the two new ones
        clusters.RemoveAt(clusterToSplit);
        clusters.Add(cluster1);
        clusters.Add(cluster2);
      }

      return clusters.Select(c => c.Centroid);
    }

    private (Cluster, Cluster) _BisectCluster(Cluster cluster, Euclidean4N<TWork> metric) {
      var bestCluster1 = new Cluster();
      var bestCluster2 = new Cluster();
      var bestSse = double.MaxValue;

      var random = new Random(42 + cluster.Points.Count);

      for (var trial = 0; trial < bisectionTrials; ++trial) {
        // Initialize two centroids using K-Means++ style
        var idx1 = random.Next(cluster.Points.Count);
        var centroid1 = cluster.Points[idx1].color;

        // Find point farthest from centroid1
        var maxDist = UNorm32.Zero;
        var idx2 = 0;
        for (var i = 0; i < cluster.Points.Count; ++i) {
          var dist = metric.Distance(cluster.Points[i].color, centroid1);
          if (dist > maxDist) {
            maxDist = dist;
            idx2 = i;
          }
        }
        var centroid2 = cluster.Points[idx2].color;

        var threshold = UNorm32.FromFloatClamped(convergenceThreshold);

        // Run K-Means with k=2
        for (var iteration = 0; iteration < maxIterationsPerSplit; ++iteration) {
          var points1 = new List<(TWork color, uint count)>();
          var points2 = new List<(TWork color, uint count)>();

          // Assign points to nearest centroid
          foreach (var point in cluster.Points) {
            var dist1 = metric.Distance(point.color, centroid1);
            var dist2 = metric.Distance(point.color, centroid2);
            if (dist1 <= dist2)
              points1.Add(point);
            else
              points2.Add(point);
          }

          // Handle empty clusters
          if (points1.Count == 0 || points2.Count == 0)
            break;

          // Update centroids
          var newCentroid1 = _CalculateCentroid(points1);
          var newCentroid2 = _CalculateCentroid(points2);

          // Check convergence
          var movement = metric.Distance(centroid1, newCentroid1);
          var movement2 = metric.Distance(centroid2, newCentroid2);
          if (movement2 > movement)
            movement = movement2;

          centroid1 = newCentroid1;
          centroid2 = newCentroid2;

          if (movement < threshold)
            break;
        }

        // Calculate SSE for this trial
        var cluster1 = new Cluster { Centroid = centroid1 };
        var cluster2 = new Cluster { Centroid = centroid2 };

        foreach (var point in cluster.Points) {
          var dist1 = metric.Distance(point.color, centroid1);
          var dist2 = metric.Distance(point.color, centroid2);
          if (dist1 <= dist2)
            cluster1.Points.Add(point);
          else
            cluster2.Points.Add(point);
        }

        cluster1.Sse = _CalculateSse(cluster1.Points, centroid1, metric);
        cluster2.Sse = _CalculateSse(cluster2.Points, centroid2, metric);
        var totalSse = cluster1.Sse + cluster2.Sse;

        if (totalSse < bestSse && cluster1.Points.Count > 0 && cluster2.Points.Count > 0) {
          bestSse = totalSse;
          bestCluster1 = cluster1;
          bestCluster2 = cluster2;
        }
      }

      // If bisection failed, return original cluster and empty one
      if (bestCluster1.Points.Count == 0 || bestCluster2.Points.Count == 0) {
        bestCluster1 = cluster;
        bestCluster2 = new Cluster { Centroid = cluster.Centroid };
      }

      return (bestCluster1, bestCluster2);
    }

    private static TWork _CalculateCentroid(List<(TWork color, uint count)> points) {
      if (points.Count == 0)
        return default;

      var sumC1 = 0.0;
      var sumC2 = 0.0;
      var sumC3 = 0.0;
      var sumA = 0.0;
      var totalWeight = 0L;

      foreach (var (color, count) in points) {
        var (c1, c2, c3, a) = color.ToNormalized();
        sumC1 += c1.ToFloat() * count;
        sumC2 += c2.ToFloat() * count;
        sumC3 += c3.ToFloat() * count;
        sumA += a.ToFloat() * count;
        totalWeight += count;
      }

      if (totalWeight == 0)
        return default;

      return ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)(sumC1 / totalWeight)),
        UNorm32.FromFloatClamped((float)(sumC2 / totalWeight)),
        UNorm32.FromFloatClamped((float)(sumC3 / totalWeight)),
        UNorm32.FromFloatClamped((float)(sumA / totalWeight))
      );
    }

    private static double _CalculateSse(List<(TWork color, uint count)> points, TWork centroid, Euclidean4N<TWork> metric) {
      var sse = 0.0;
      foreach (var (color, count) in points) {
        var dist = metric.Distance(color, centroid).ToFloat();
        sse += dist * dist * count;
      }
      return sse;
    }
  }
}
