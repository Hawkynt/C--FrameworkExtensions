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
/// Hierarchical Competitive Learning color quantizer.
/// </summary>
/// <remarks>
/// <para>This algorithm progressively splits clusters and refines them using competitive learning.</para>
/// <para>Unlike K-Means, it uses sequential winner-take-all updates and is initialization-independent.</para>
/// <para>Reference: Scheunders (1997) - "A comparison of clustering algorithms applied to color image quantization"</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Hierarchical CL", Author = "P. Scheunders", Year = 1997, QualityRating = 8)]
public struct HierarchicalCompetitiveLearningQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the number of competitive learning epochs after each split.
  /// </summary>
  public int EpochsPerSplit { get; set; } = 5;

  /// <summary>
  /// Gets or sets the initial learning rate for competitive learning.
  /// </summary>
  public float InitialLearningRate { get; set; } = 0.1f;

  /// <summary>
  /// Gets or sets the maximum sample size for processing.
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  public HierarchicalCompetitiveLearningQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.EpochsPerSplit, this.InitialLearningRate, this.MaxSampleSize);

  internal sealed class Kernel<TWork>(int epochsPerSplit, float initialLearningRate, int maxSampleSize) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= colorCount)
        return colors.Select(c => c.color);

      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize, 42);

      // Convert to normalized float arrays for faster computation
      var colorData = colors.Select(c => {
        var (c1, c2, c3, a) = c.color.ToNormalized();
        return (c1: (double)c1.ToFloat(), c2: (double)c2.ToFloat(), c3: (double)c3.ToFloat(), a: (double)a.ToFloat(), count: c.count);
      }).ToArray();

      var totalWeight = colorData.Sum(c => (long)c.count);

      // Initialize with single cluster (weighted mean of all colors)
      var clusters = new List<double[]> {
        new[] {
          colorData.Sum(c => c.c1 * c.count) / totalWeight,
          colorData.Sum(c => c.c2 * c.count) / totalWeight,
          colorData.Sum(c => c.c3 * c.count) / totalWeight,
          colorData.Sum(c => c.a * c.count) / totalWeight
        }
      };

      var assignments = new int[colorData.Length];
      var random = new Random(42);

      // Progressive splitting until we have enough clusters
      while (clusters.Count < colorCount) {
        // Find cluster with highest quantization error
        var clusterErrors = new double[clusters.Count];
        var clusterWeights = new double[clusters.Count];

        for (var i = 0; i < colorData.Length; ++i) {
          var cluster = assignments[i];
          var centroid = clusters[cluster];
          var dist = _SquaredDistance(colorData[i], centroid);
          clusterErrors[cluster] += dist * colorData[i].count;
          clusterWeights[cluster] += colorData[i].count;
        }

        var worstCluster = 0;
        var maxError = clusterErrors[0];
        for (var i = 1; i < clusters.Count; ++i)
          if (clusterErrors[i] > maxError) {
            maxError = clusterErrors[i];
            worstCluster = i;
          }

        // Split worst cluster by perturbing its center along a random direction
        var perturbation = 0.05 + random.NextDouble() * 0.05;
        var original = clusters[worstCluster];

        // Random direction for perturbation
        var dir1 = random.NextDouble() * 2 - 1;
        var dir2 = random.NextDouble() * 2 - 1;
        var dir3 = random.NextDouble() * 2 - 1;
        var dirLen = Math.Sqrt(dir1 * dir1 + dir2 * dir2 + dir3 * dir3);
        if (dirLen > 0) {
          dir1 /= dirLen;
          dir2 /= dirLen;
          dir3 /= dirLen;
        }

        var split1 = new[] { original[0] + perturbation * dir1, original[1] + perturbation * dir2, original[2] + perturbation * dir3, original[3] };
        var split2 = new[] { original[0] - perturbation * dir1, original[1] - perturbation * dir2, original[2] - perturbation * dir3, original[3] };

        clusters[worstCluster] = split1;
        clusters.Add(split2);

        // Immediately reassign after split
        for (var i = 0; i < colorData.Length; ++i)
          assignments[i] = _FindNearestCluster(colorData[i], clusters);

        // Apply competitive learning for several epochs
        var learningRate = (double)initialLearningRate;
        var minLearningRate = learningRate * 0.01; // Don't decay to zero
        var decayFactor = Math.Pow(minLearningRate / learningRate, 1.0 / epochsPerSplit);

        for (var epoch = 0; epoch < epochsPerSplit; ++epoch) {
          // Accumulate weighted updates per cluster
          var clusterSums = new double[clusters.Count][];
          var epochWeights = new double[clusters.Count];
          for (var j = 0; j < clusters.Count; ++j)
            clusterSums[j] = new double[4];

          // Winner-take-all: find winner for each color, accumulate weighted contribution
          foreach (var (c1, c2, c3, a, count) in colorData) {
            var winner = _FindNearestCluster((c1, c2, c3, a, count), clusters);
            clusterSums[winner][0] += c1 * count;
            clusterSums[winner][1] += c2 * count;
            clusterSums[winner][2] += c3 * count;
            clusterSums[winner][3] += a * count;
            epochWeights[winner] += count;
          }

          // Update each cluster towards its weighted centroid
          for (var j = 0; j < clusters.Count; ++j) {
            if (epochWeights[j] <= 0)
              continue;

            var target0 = clusterSums[j][0] / epochWeights[j];
            var target1 = clusterSums[j][1] / epochWeights[j];
            var target2 = clusterSums[j][2] / epochWeights[j];
            var target3 = clusterSums[j][3] / epochWeights[j];

            var centroid = clusters[j];
            centroid[0] += learningRate * (target0 - centroid[0]);
            centroid[1] += learningRate * (target1 - centroid[1]);
            centroid[2] += learningRate * (target2 - centroid[2]);
            centroid[3] += learningRate * (target3 - centroid[3]);
          }

          // Update assignments for next epoch
          for (var i = 0; i < colorData.Length; ++i)
            assignments[i] = _FindNearestCluster(colorData[i], clusters);

          learningRate *= decayFactor;
        }

        // Reassign after learning
        for (var i = 0; i < colorData.Length; ++i)
          assignments[i] = _FindNearestCluster(colorData[i], clusters);
      }

      // Convert clusters back to TWork colors
      return clusters.Select(c => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c[0]))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c[1]))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c[2]))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c[3])))
      ));
    }

    private static double _SquaredDistance((double c1, double c2, double c3, double a, uint count) color, double[] centroid) {
      var d1 = color.c1 - centroid[0];
      var d2 = color.c2 - centroid[1];
      var d3 = color.c3 - centroid[2];
      var d4 = color.a - centroid[3];
      return d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
    }

    private static int _FindNearestCluster((double c1, double c2, double c3, double a, uint count) color, List<double[]> clusters) {
      var nearest = 0;
      var minDist = _SquaredDistance(color, clusters[0]);

      for (var i = 1; i < clusters.Count; ++i) {
        var dist = _SquaredDistance(color, clusters[i]);
        if (!(dist < minDist))
          continue;

        minDist = dist;
        nearest = i;
      }

      return nearest;
    }

  }
}
