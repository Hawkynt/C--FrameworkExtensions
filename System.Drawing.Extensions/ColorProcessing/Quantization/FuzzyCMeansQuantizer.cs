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
/// Implements Fuzzy C-Means clustering-based color quantization.
/// </summary>
/// <remarks>
/// <para>Reference: J.C. Bezdek 1981 "Pattern Recognition with Fuzzy Objective Function Algorithms"</para>
/// <para>Plenum Press, New York</para>
/// <para>See also: https://en.wikipedia.org/wiki/Fuzzy_clustering</para>
/// <para>
/// Unlike hard clustering (K-Means), Fuzzy C-Means allows each data point to belong to multiple
/// clusters with a degree of membership. This often produces smoother color transitions in the
/// resulting palette.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Fuzzy C-Means", QualityRating = 8)]
public struct FuzzyCMeansQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum number of iterations for the algorithm.
  /// </summary>
  public int MaxIterations { get; set; } = 100;

  /// <summary>
  /// Gets or sets the convergence threshold.
  /// </summary>
  public double ConvergenceThreshold { get; set; } = 0.001;

  /// <summary>
  /// Gets or sets the fuzziness parameter (m). Values typically range from 1.5 to 2.5.
  /// Higher values produce fuzzier clusters. A value of 1.0 reduces to hard K-Means.
  /// </summary>
  public double Fuzziness { get; set; } = 2.0;

  /// <summary>
  /// Gets or sets the maximum histogram sample size for large images.
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  public FuzzyCMeansQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MaxIterations,
    this.ConvergenceThreshold,
    this.Fuzziness,
    this.MaxSampleSize
  );

  internal sealed class Kernel<TWork>(int maxIterations, double convergenceThreshold, double fuzziness, int maxSampleSize) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, (TWork color, uint count)[] histogram) {
      // Sample histogram if too large for iterative processing
      var sampled = QuantizerHelper.SampleHistogram(histogram, maxSampleSize);
      var colorList = sampled.Select(h => {
        var (c1, c2, c3, a) = h.color.ToNormalized();
        return (c1: c1.ToFloat(), c2: c2.ToFloat(), c3: c3.ToFloat(), a: a.ToFloat(), count: h.count);
      }).ToList();

      if (colorList.Count == 0)
        return [];

      // Initialize centroids using k-means++ for better initial placement
      var centroids = _InitializeCentroidsKMeansPlusPlus(colorList, colorCount);

      // Initialize membership matrix
      var membershipMatrix = new double[colorList.Count, colorCount];
      var previousCentroids = new double[colorCount][];

      // Iterative refinement
      for (var iteration = 0; iteration < maxIterations; ++iteration) {
        // Store previous centroids for convergence check
        for (var i = 0; i < colorCount; ++i)
          previousCentroids[i] = centroids[i].ToArray();

        // Update membership matrix
        _UpdateMembershipMatrix(colorList, centroids, membershipMatrix, fuzziness);

        // Update centroids based on fuzzy memberships
        _UpdateCentroids(colorList, centroids, membershipMatrix, fuzziness);

        // Check for convergence
        if (_HasConverged(centroids, previousCentroids, convergenceThreshold))
          break;
      }

      // Convert centroids to TWork colors
      return centroids.Select(centroid => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)centroid[0]),
        UNorm32.FromFloatClamped((float)centroid[1]),
        UNorm32.FromFloatClamped((float)centroid[2]),
        UNorm32.FromFloatClamped((float)centroid[3])
      ));
    }

    private static double[][] _InitializeCentroidsKMeansPlusPlus(
      List<(float c1, float c2, float c3, float a, uint count)> colors,
      int k
    ) {
      var centroids = new double[k][];
      var random = new Random(42); // Fixed seed for reproducibility

      // Choose first centroid randomly
      var firstIndex = random.Next(colors.Count);
      centroids[0] = [colors[firstIndex].c1, colors[firstIndex].c2, colors[firstIndex].c3, colors[firstIndex].a];

      // Choose remaining centroids using k-means++ algorithm
      for (var i = 1; i < k; ++i) {
        var distances = new double[colors.Count];
        var totalDistance = 0.0;

        // Calculate minimum squared distance to existing centroids
        for (var j = 0; j < colors.Count; ++j) {
          var color = colors[j];
          var minDistance = double.MaxValue;

          for (var c = 0; c < i; ++c) {
            var centroid = centroids[c];
            var d1 = color.c1 - centroid[0];
            var d2 = color.c2 - centroid[1];
            var d3 = color.c3 - centroid[2];
            var distance = d1 * d1 + d2 * d2 + d3 * d3;

            if (distance < minDistance)
              minDistance = distance;
          }

          distances[j] = minDistance;
          totalDistance += minDistance;
        }

        // Choose next centroid with probability proportional to distance squared
        var threshold = random.NextDouble() * totalDistance;
        var cumulative = 0.0;
        var selectedIndex = 0;

        for (var j = 0; j < colors.Count; ++j) {
          cumulative += distances[j];
          if (cumulative >= threshold) {
            selectedIndex = j;
            break;
          }
        }

        centroids[i] = [colors[selectedIndex].c1, colors[selectedIndex].c2, colors[selectedIndex].c3, colors[selectedIndex].a];
      }

      return centroids;
    }

    private static void _UpdateMembershipMatrix(
      List<(float c1, float c2, float c3, float a, uint count)> colors,
      double[][] centroids,
      double[,] membershipMatrix,
      double fuzziness
    ) {
      var exponent = 2.0 / (fuzziness - 1.0);

      // Reusable arrays to avoid allocations
      var invDistPow = new double[centroids.Length];

      for (var i = 0; i < colors.Count; ++i) {
        var color = colors[i];

        // Calculate 1/(distance^exponent) for all centroids and sum
        var sumInvDistPow = 0.0;
        for (var j = 0; j < centroids.Length; ++j) {
          var centroid = centroids[j];
          var d1 = color.c1 - centroid[0];
          var d2 = color.c2 - centroid[1];
          var d3 = color.c3 - centroid[2];
          var dist = Math.Sqrt(d1 * d1 + d2 * d2 + d3 * d3);

          // Avoid division by zero
          if (dist < 1e-10)
            dist = 1e-10;

          // Precompute 1/(dist^exponent)
          invDistPow[j] = 1.0 / Math.Pow(dist, exponent);
          sumInvDistPow += invDistPow[j];
        }

        // Membership is (1/dist_j^p) / sum_k(1/dist_k^p) - O(k) instead of O(k²)
        for (var j = 0; j < centroids.Length; ++j)
          membershipMatrix[i, j] = invDistPow[j] / sumInvDistPow;
      }
    }

    private static void _UpdateCentroids(
      List<(float c1, float c2, float c3, float a, uint count)> colors,
      double[][] centroids,
      double[,] membershipMatrix,
      double fuzziness
    ) {
      for (var j = 0; j < centroids.Length; ++j) {
        var sumC1 = 0.0;
        var sumC2 = 0.0;
        var sumC3 = 0.0;
        var sumA = 0.0;
        var sumMembership = 0.0;

        for (var i = 0; i < colors.Count; ++i) {
          var color = colors[i];
          var membership = Math.Pow(membershipMatrix[i, j], fuzziness);
          var weightedMembership = membership * color.count;

          sumC1 += color.c1 * weightedMembership;
          sumC2 += color.c2 * weightedMembership;
          sumC3 += color.c3 * weightedMembership;
          sumA += color.a * weightedMembership;
          sumMembership += weightedMembership;
        }

        if (sumMembership > 0) {
          centroids[j][0] = sumC1 / sumMembership;
          centroids[j][1] = sumC2 / sumMembership;
          centroids[j][2] = sumC3 / sumMembership;
          centroids[j][3] = sumA / sumMembership;
        }
      }
    }

    private static bool _HasConverged(double[][] centroids, double[][] previousCentroids, double threshold) {
      var totalChange = 0.0;

      for (var i = 0; i < centroids.Length; ++i) {
        var d1 = centroids[i][0] - previousCentroids[i][0];
        var d2 = centroids[i][1] - previousCentroids[i][1];
        var d3 = centroids[i][2] - previousCentroids[i][2];
        totalChange += Math.Sqrt(d1 * d1 + d2 * d2 + d3 * d3);
      }

      return totalChange / centroids.Length < threshold;
    }
  }
}
