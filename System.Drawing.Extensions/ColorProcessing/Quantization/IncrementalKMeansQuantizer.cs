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
/// Implements Incremental (Online) K-Means clustering-based color quantization.
/// </summary>
/// <remarks>
/// <para>
/// This algorithm is deterministic and requires no random initialization. It starts with the first k
/// unique colors as initial centers and updates them incrementally as new colors are processed.
/// </para>
/// <para>
/// The incremental update formula for each cluster center is: c_new = c_old + (x - c_old) / n
/// where x is the new color assigned to the cluster and n is the number of colors assigned to that cluster.
/// </para>
/// <para>
/// This approach is particularly useful for streaming data or when deterministic results are required,
/// as it produces the same output for the same input order every time.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Incremental K-Means", QualityRating = 7)]
public struct IncrementalKMeansQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the number of refinement passes.
  /// After the initial incremental pass, the algorithm can make additional passes
  /// to further refine the cluster centers.
  /// </summary>
  public int RefinementPasses { get; set; } = 3;

  public IncrementalKMeansQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.RefinementPasses);

  internal sealed class Kernel<TWork>(int refinementPasses) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var colorList = histogram.Select(h => {
        var (c1, c2, c3, a) = h.color.ToNormalized();
        return (c1: c1.ToFloat(), c2: c2.ToFloat(), c3: c3.ToFloat(), a: a.ToFloat(), count: h.count);
      }).ToList();

      if (colorList.Count == 0)
        return [];

      // Initialize centroids with first k unique colors (deterministic)
      var centroids = _InitializeCentroidsDeterministic(colorList, colorCount);
      var clusterCounts = new double[colorCount];

      // Initialize cluster counts to 1 to avoid division by zero
      for (var i = 0; i < colorCount; ++i)
        clusterCounts[i] = 1;

      // Initial incremental pass: process all colors in order
      foreach (var color in colorList) {
        // Find nearest centroid
        var nearestIndex = _FindNearestCentroid(color.c1, color.c2, color.c3, centroids);

        // Update centroid incrementally: c = c + (x - c) / n
        var weight = color.count;
        for (var pass = 0; pass < weight; ++pass) {
          var n = clusterCounts[nearestIndex];
          centroids[nearestIndex][0] += (color.c1 - centroids[nearestIndex][0]) / n;
          centroids[nearestIndex][1] += (color.c2 - centroids[nearestIndex][1]) / n;
          centroids[nearestIndex][2] += (color.c3 - centroids[nearestIndex][2]) / n;
          centroids[nearestIndex][3] += (color.a - centroids[nearestIndex][3]) / n;
          clusterCounts[nearestIndex] += 1.0;
        }
      }

      // Refinement passes: reassign colors and recalculate centroids
      for (var pass = 0; pass < refinementPasses; ++pass) {
        var clusterSums = new double[colorCount][];
        var newClusterCounts = new double[colorCount];

        for (var i = 0; i < colorCount; ++i)
          clusterSums[i] = new double[4];

        // Assignment step: assign each color to nearest centroid
        foreach (var color in colorList) {
          var nearestIndex = _FindNearestCentroid(color.c1, color.c2, color.c3, centroids);
          clusterSums[nearestIndex][0] += color.c1 * color.count;
          clusterSums[nearestIndex][1] += color.c2 * color.count;
          clusterSums[nearestIndex][2] += color.c3 * color.count;
          clusterSums[nearestIndex][3] += color.a * color.count;
          newClusterCounts[nearestIndex] += color.count;
        }

        // Update step: recalculate centroids
        for (var i = 0; i < colorCount; ++i) {
          if (newClusterCounts[i] > 0) {
            centroids[i][0] = clusterSums[i][0] / newClusterCounts[i];
            centroids[i][1] = clusterSums[i][1] / newClusterCounts[i];
            centroids[i][2] = clusterSums[i][2] / newClusterCounts[i];
            centroids[i][3] = clusterSums[i][3] / newClusterCounts[i];
            clusterCounts[i] = newClusterCounts[i];
          }
          // If cluster is empty, keep the previous centroid position
        }
      }

      // Convert centroids to TWork colors
      return centroids.Select(centroid => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)centroid[0]),
        UNorm32.FromFloatClamped((float)centroid[1]),
        UNorm32.FromFloatClamped((float)centroid[2]),
        UNorm32.FromFloatClamped((float)centroid[3])
      ));
    }

    private static double[][] _InitializeCentroidsDeterministic(
      List<(float c1, float c2, float c3, float a, uint count)> colors,
      int k
    ) {
      var initialCount = Math.Min(k, colors.Count);
      var centroids = new double[initialCount][];

      // Use first k unique colors as initial centroids
      for (var i = 0; i < initialCount; ++i) {
        var color = colors[i];
        centroids[i] = [color.c1, color.c2, color.c3, color.a];
      }

      // If we need more centroids than we have colors, distribute evenly in color space
      if (initialCount < k) {
        var result = new double[k][];
        for (var i = 0; i < initialCount; ++i)
          result[i] = centroids[i];

        // Fill remaining centroids by interpolating between existing ones
        for (var i = initialCount; i < k; ++i) {
          var index1 = (i - initialCount) % initialCount;
          var index2 = (i - initialCount + 1) % initialCount;
          result[i] = [
            (centroids[index1][0] + centroids[index2][0]) / 2.0,
            (centroids[index1][1] + centroids[index2][1]) / 2.0,
            (centroids[index1][2] + centroids[index2][2]) / 2.0,
            (centroids[index1][3] + centroids[index2][3]) / 2.0
          ];
        }

        return result;
      }

      return centroids;
    }

    private static int _FindNearestCentroid(float c1, float c2, float c3, double[][] centroids) {
      var minDistance = double.MaxValue;
      var nearestIndex = 0;

      for (var i = 0; i < centroids.Length; ++i) {
        var centroid = centroids[i];
        var d1 = c1 - centroid[0];
        var d2 = c2 - centroid[1];
        var d3 = c3 - centroid[2];
        var distance = d1 * d1 + d2 * d2 + d3 * d3;

        if (distance < minDistance) {
          minDistance = distance;
          nearestIndex = i;
        }
      }

      return nearestIndex;
    }
  }
}
