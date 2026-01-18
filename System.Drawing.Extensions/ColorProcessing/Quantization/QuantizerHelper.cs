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

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Internal helper for common quantizer operations.
/// </summary>
internal static class QuantizerHelper {

  /// <summary>
  /// Handles common edge cases and prepares histogram for quantization.
  /// </summary>
  /// <typeparam name="TWork">The color space type.</typeparam>
  /// <param name="histogram">The input histogram of colors and counts.</param>
  /// <param name="colorCount">The requested number of palette colors.</param>
  /// <param name="allowFillingColors">Whether to fill unused palette entries.</param>
  /// <param name="normalizedHistogram">
  /// Output: The deduplicated and normalized histogram for quantization.
  /// Only valid when return value is null.
  /// </param>
  /// <returns>
  /// The final palette if no quantization is needed (0/1 colors or fewer unique colors than requested),
  /// or null if quantization is required.
  /// </returns>
  public static TWork[]? TryHandleSimpleCases<TWork>(
    IEnumerable<(TWork color, uint count)> histogram,
    int colorCount,
    bool allowFillingColors,
    out (TWork color, uint count)[] normalizedHistogram)
    where TWork : unmanaged, IColorSpace4<TWork> {

    // Handle trivial cases
    switch (colorCount) {
      case <= 0:
        normalizedHistogram = [];
        return [];
      case 1: {
        // For single color, find most common
        var mostCommon = histogram
          .GroupBy(h => h.color.ToNormalized())
          .Select(g => (color: g.First().color, count: (uint)g.Sum(h => h.count)))
          .OrderByDescending(h => h.count)
          .FirstOrDefault();
        normalizedHistogram = mostCommon.count > 0 ? [mostCommon] : [];
        return [mostCommon.color];
      }
    }

    // Deduplicate and aggregate by normalized color
    var used = histogram
      .GroupBy(h => h.color.ToNormalized())
      .Select(g => (color: g.First().color, count: (uint)g.Sum(h => h.count)))
      .ToArray();

    // If we have fewer unique colors than requested, no quantization needed
    if (used.Length <= colorCount) {
      normalizedHistogram = used;
      return PaletteFiller.GenerateFinalPalette(used.Select(h => h.color), colorCount, allowFillingColors);
    }

    // Quantization is required
    normalizedHistogram = used;
    return null;
  }

  /// <summary>
  /// Default maximum sample size for iterative quantizers.
  /// </summary>
  public const int DefaultMaxSampleSize = 8192;

  /// <summary>
  /// Reduces a large histogram to a smaller sample for iterative algorithms.
  /// Uses weighted reservoir sampling to preserve color distribution.
  /// </summary>
  /// <typeparam name="TWork">The color space type.</typeparam>
  /// <param name="histogram">The input histogram.</param>
  /// <param name="maxSampleSize">Maximum number of colors in the sample.</param>
  /// <param name="seed">Random seed for reproducibility.</param>
  /// <returns>A reduced histogram preserving relative color importance.</returns>
  public static (TWork color, uint count)[] SampleHistogram<TWork>(
    (TWork color, uint count)[] histogram,
    int maxSampleSize = DefaultMaxSampleSize,
    int? seed = null)
    where TWork : unmanaged, IColorSpace4<TWork> {

    if (histogram.Length <= maxSampleSize)
      return histogram;

    // Use weighted reservoir sampling (Algorithm A-Res)
    var random = seed == null ? Random.Shared : new Random(seed.Value);
    var reservoir = new (TWork color, uint count, double key)[maxSampleSize];
    var totalWeight = histogram.Sum(h => (long)h.count);

    // Initialize reservoir with first maxSampleSize items
    for (var i = 0; i < maxSampleSize; ++i) {
      var weight = (double)histogram[i].count / totalWeight;
      var key = Math.Pow(random.NextDouble(), 1.0 / weight);
      reservoir[i] = (histogram[i].color, histogram[i].count, key);
    }

    // Find minimum key in reservoir
    var minKeyIndex = 0;
    for (var i = 1; i < maxSampleSize; ++i)
      if (reservoir[i].key < reservoir[minKeyIndex].key)
        minKeyIndex = i;

    // Process remaining items
    for (var i = maxSampleSize; i < histogram.Length; ++i) {
      var weight = (double)histogram[i].count / totalWeight;
      var key = Math.Pow(random.NextDouble(), 1.0 / weight);

      if (!(key > reservoir[minKeyIndex].key))
        continue;

      reservoir[minKeyIndex] = (histogram[i].color, histogram[i].count, key);

      // Find new minimum
      minKeyIndex = 0;
      for (var j = 1; j < maxSampleSize; ++j)
        if (reservoir[j].key < reservoir[minKeyIndex].key)
          minKeyIndex = j;
    }

    // Return sampled histogram, scaling counts to preserve total weight approximation
    var sampleTotal = reservoir.Sum(r => (long)r.count);
    var scale = (double)totalWeight / sampleTotal;

    return reservoir
      .Select(r => (r.color, count: (uint)Math.Max(1, r.count * scale / maxSampleSize * histogram.Length)))
      .ToArray();
  }

  #region PCA Initialization

  /// <summary>
  /// Initializes palette colors using Principal Component Analysis.
  /// Distributes colors along the principal axes of the color distribution.
  /// </summary>
  /// <typeparam name="TWork">The color space type.</typeparam>
  /// <param name="histogram">The input histogram.</param>
  /// <param name="colorCount">Number of palette colors to generate.</param>
  /// <param name="seed">Random seed for reproducibility.</param>
  /// <returns>Initial palette colors distributed along principal axes.</returns>
  public static TWork[] InitializePaletteWithPCA<TWork>(
    (TWork color, uint count)[] histogram,
    int colorCount,
    int? seed = null)
    where TWork : unmanaged, IColorSpace4<TWork> {

    if (histogram.Length == 0)
      return [];

    // Convert to normalized float arrays
    var points = new (float c1, float c2, float c3, float a, uint count)[histogram.Length];
    for (var i = 0; i < histogram.Length; ++i) {
      var (c1, c2, c3, a) = histogram[i].color.ToNormalized();
      points[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat(), histogram[i].count);
    }

    // Compute weighted centroid
    var totalWeight = points.Sum(p => (double)p.count);
    var centroid = (
      c1: points.Sum(p => p.c1 * p.count) / totalWeight,
      c2: points.Sum(p => p.c2 * p.count) / totalWeight,
      c3: points.Sum(p => p.c3 * p.count) / totalWeight,
      a: points.Sum(p => p.a * p.count) / totalWeight
    );

    // Compute covariance matrix (4x4 for RGBA)
    var cov = new double[4, 4];
    foreach (var (c1, c2, c3, a, count) in points) {
      var d = new[] { c1 - centroid.c1, c2 - centroid.c2, c3 - centroid.c3, a - centroid.a };
      for (var i = 0; i < 4; ++i)
        for (var j = 0; j < 4; ++j)
          cov[i, j] += d[i] * d[j] * count;
    }

    for (var i = 0; i < 4; ++i)
      for (var j = 0; j < 4; ++j)
        cov[i, j] /= totalWeight;

    // Find principal eigenvector using power iteration
    var eigenvectors = _ComputePrincipalComponents(cov, Math.Min(3, colorCount));

    // Find min/max projections along first principal component
    var projections = points.Select(p => {
      var proj = (p.c1 - centroid.c1) * eigenvectors[0][0] +
                 (p.c2 - centroid.c2) * eigenvectors[0][1] +
                 (p.c3 - centroid.c3) * eigenvectors[0][2] +
                 (p.a - centroid.a) * eigenvectors[0][3];
      return (proj, p);
    }).ToArray();

    var minProj = projections.Min(p => p.proj);
    var maxProj = projections.Max(p => p.proj);

    // Distribute palette colors along principal axis
    var palette = new TWork[colorCount];
    var random = seed == null ? Random.Shared : new Random(seed.Value);

    for (var i = 0; i < colorCount; ++i) {
      double t;
      if (colorCount == 1)
        t = 0.5;
      else
        t = (double)i / (colorCount - 1);

      var proj = minProj + t * (maxProj - minProj);

      // Add small random offset using secondary components if available
      var offset1 = eigenvectors.Length > 1 ? (random.NextDouble() - 0.5) * 0.1 : 0;
      var offset2 = eigenvectors.Length > 2 ? (random.NextDouble() - 0.5) * 0.1 : 0;

      var c1 = (float)(centroid.c1 + proj * eigenvectors[0][0] +
                       (eigenvectors.Length > 1 ? offset1 * eigenvectors[1][0] : 0) +
                       (eigenvectors.Length > 2 ? offset2 * eigenvectors[2][0] : 0));
      var c2 = (float)(centroid.c2 + proj * eigenvectors[0][1] +
                       (eigenvectors.Length > 1 ? offset1 * eigenvectors[1][1] : 0) +
                       (eigenvectors.Length > 2 ? offset2 * eigenvectors[2][1] : 0));
      var c3 = (float)(centroid.c3 + proj * eigenvectors[0][2] +
                       (eigenvectors.Length > 1 ? offset1 * eigenvectors[1][2] : 0) +
                       (eigenvectors.Length > 2 ? offset2 * eigenvectors[2][2] : 0));
      var a = (float)(centroid.a + proj * eigenvectors[0][3] +
                      (eigenvectors.Length > 1 ? offset1 * eigenvectors[1][3] : 0) +
                      (eigenvectors.Length > 2 ? offset2 * eigenvectors[2][3] : 0));

      // Clamp to valid range
      c1 = Math.Max(0, Math.Min(1, c1));
      c2 = Math.Max(0, Math.Min(1, c2));
      c3 = Math.Max(0, Math.Min(1, c3));
      a = Math.Max(0, Math.Min(1, a));

      palette[i] = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(c1),
        UNorm32.FromFloatClamped(c2),
        UNorm32.FromFloatClamped(c3),
        UNorm32.FromFloatClamped(a)
      );
    }

    return palette;
  }

  /// <summary>
  /// Computes principal components using power iteration with deflation.
  /// </summary>
  private static double[][] _ComputePrincipalComponents(double[,] covariance, int numComponents) {
    const int maxIterations = 100;
    const double tolerance = 1e-10;
    var n = covariance.GetLength(0);
    var result = new double[numComponents][];
    var deflatedCov = (double[,])covariance.Clone();

    for (var comp = 0; comp < numComponents; ++comp) {
      // Power iteration to find dominant eigenvector
      var v = new double[n];
      for (var i = 0; i < n; ++i)
        v[i] = 1.0 / Math.Sqrt(n);

      for (var iter = 0; iter < maxIterations; ++iter) {
        // Multiply: w = Av
        var w = new double[n];
        for (var i = 0; i < n; ++i)
          for (var j = 0; j < n; ++j)
            w[i] += deflatedCov[i, j] * v[j];

        // Normalize
        var norm = Math.Sqrt(w.Sum(x => x * x));
        if (norm < tolerance) {
          // Degenerate case - use unit vector
          for (var i = 0; i < n; ++i)
            w[i] = i == comp ? 1.0 : 0.0;
          norm = 1.0;
        }

        var newV = w.Select(x => x / norm).ToArray();

        // Check convergence
        var diff = 0.0;
        for (var i = 0; i < n; ++i)
          diff += (newV[i] - v[i]) * (newV[i] - v[i]);

        v = newV;
        if (diff < tolerance)
          break;
      }

      result[comp] = v;

      // Deflate: remove this component from covariance matrix
      // C' = C - λ * v * v^T (approximate λ using Rayleigh quotient)
      var eigenvalue = 0.0;
      for (var i = 0; i < n; ++i)
        for (var j = 0; j < n; ++j)
          eigenvalue += v[i] * deflatedCov[i, j] * v[j];

      for (var i = 0; i < n; ++i)
        for (var j = 0; j < n; ++j)
          deflatedCov[i, j] -= eigenvalue * v[i] * v[j];
    }

    return result;
  }

  #endregion

  #region Ant Colony Optimization

  /// <summary>
  /// Default number of ants for ACO.
  /// </summary>
  public const int DefaultAntCount = 20;

  /// <summary>
  /// Default number of ACO iterations.
  /// </summary>
  public const int DefaultAcoIterations = 50;

  /// <summary>
  /// Optimizes an existing palette using Ant Colony Optimization.
  /// </summary>
  /// <typeparam name="TWork">The color space type.</typeparam>
  /// <param name="histogram">The input histogram.</param>
  /// <param name="initialPalette">Initial palette to optimize.</param>
  /// <param name="antCount">Number of ants.</param>
  /// <param name="iterations">Number of iterations.</param>
  /// <param name="evaporationRate">Pheromone evaporation rate (0-1).</param>
  /// <param name="alpha">Pheromone influence factor.</param>
  /// <param name="beta">Heuristic influence factor.</param>
  /// <param name="seed">Random seed for reproducibility.</param>
  /// <returns>Optimized palette.</returns>
  public static TWork[] OptimizePaletteWithACO<TWork>(
    (TWork color, uint count)[] histogram,
    TWork[] initialPalette,
    int antCount = DefaultAntCount,
    int iterations = DefaultAcoIterations,
    double evaporationRate = 0.1,
    double alpha = 1.0,
    double beta = 2.0,
    int? seed = null)
    where TWork : unmanaged, IColorSpace4<TWork> {

    if (histogram.Length == 0 || initialPalette.Length == 0)
      return initialPalette;

    var random = seed == null ? Random.Shared : new Random(seed.Value);
    var k = initialPalette.Length;

    // Convert histogram to float arrays for faster computation
    var colors = histogram.Select(h => {
      var (c1, c2, c3, a) = h.color.ToNormalized();
      return (c1: c1.ToFloat(), c2: c2.ToFloat(), c3: c3.ToFloat(), a: a.ToFloat(), count: h.count);
    }).ToArray();

    // Initialize pheromone matrix (histogram colors × palette slots)
    var pheromone = new double[colors.Length, k];
    for (var i = 0; i < colors.Length; ++i)
      for (var j = 0; j < k; ++j)
        pheromone[i, j] = 1.0;

    // Precompute heuristic information (inverse distance to initial palette)
    var heuristic = new double[colors.Length, k];
    for (var i = 0; i < colors.Length; ++i) {
      var (ic1, ic2, ic3, ia) = initialPalette.Select(p => p.ToNormalized()).ToArray()[0];
      for (var j = 0; j < k; ++j) {
        var (pc1, pc2, pc3, pa) = initialPalette[j].ToNormalized();
        
        // TODO: thats euclidean distance. we have code for that, maybe we can re-use
        var dist = Math.Sqrt(
          Math.Pow(colors[i].c1 - pc1.ToFloat(), 2) +
          Math.Pow(colors[i].c2 - pc2.ToFloat(), 2) +
          Math.Pow(colors[i].c3 - pc3.ToFloat(), 2) +
          Math.Pow(colors[i].a - pa.ToFloat(), 2)
        );
        heuristic[i, j] = 1.0 / (dist + 0.001);
      }
    }

    var bestPalette = (TWork[])initialPalette.Clone();
    var bestQuality = _EvaluatePaletteQuality<TWork>(colors, bestPalette);

    // ACO main loop
    for (var iter = 0; iter < iterations; ++iter) {
      var antSolutions = new (int[] assignments, double quality)[antCount];

      // Each ant constructs a solution
      for (var ant = 0; ant < antCount; ++ant) {
        var assignments = new int[colors.Length];

        // Assign each color to a palette slot based on pheromone and heuristic
        for (var i = 0; i < colors.Length; ++i) {
          var probabilities = new double[k];
          var total = 0.0;

          for (var j = 0; j < k; ++j) {
            probabilities[j] = Math.Pow(pheromone[i, j], alpha) * Math.Pow(heuristic[i, j], beta);
            total += probabilities[j];
          }

          // Roulette wheel selection
          var threshold = random.NextDouble() * total;
          var cumulative = 0.0;
          var selected = 0;

          for (var j = 0; j < k; ++j) {
            cumulative += probabilities[j];
            if (cumulative < threshold)
              continue;

            selected = j;
            break;
          }

          assignments[i] = selected;
        }

        // Compute palette from assignments (weighted centroids)
        var newPalette = _ComputePaletteFromAssignments<TWork>(colors, assignments, k);
        var quality = _EvaluatePaletteQuality<TWork>(colors, newPalette);
        antSolutions[ant] = (assignments, quality);
        if (quality <= bestQuality)
          continue;

        bestQuality = quality;
        bestPalette = newPalette;
      }

      // Update pheromones
      // Evaporation
      for (var i = 0; i < colors.Length; ++i)
        for (var j = 0; j < k; ++j)
          pheromone[i, j] *= (1 - evaporationRate);

      // Deposit pheromones based on solution quality
      foreach (var (assignments, quality) in antSolutions) {
        var deposit = quality;
        for (var i = 0; i < colors.Length; ++i)
          pheromone[i, assignments[i]] += deposit;
      }
    }

    return bestPalette;
  }

  /// <summary>
  /// Computes palette colors as weighted centroids from color-to-slot assignments.
  /// </summary>
  private static TWork[] _ComputePaletteFromAssignments<TWork>(
    (float c1, float c2, float c3, float a, uint count)[] colors,
    int[] assignments,
    int k)
    where TWork : unmanaged, IColorSpace4<TWork> {

    var palette = new TWork[k];
    var sums = new (double c1, double c2, double c3, double a, double weight)[k];

    for (var i = 0; i < colors.Length; ++i) {
      var slot = assignments[i];
      sums[slot].c1 += colors[i].c1 * colors[i].count;
      sums[slot].c2 += colors[i].c2 * colors[i].count;
      sums[slot].c3 += colors[i].c3 * colors[i].count;
      sums[slot].a += colors[i].a * colors[i].count;
      sums[slot].weight += colors[i].count;
    }

    for (var j = 0; j < k; ++j) {
      if (sums[j].weight > 0) {
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)(sums[j].c1 / sums[j].weight)),
          UNorm32.FromFloatClamped((float)(sums[j].c2 / sums[j].weight)),
          UNorm32.FromFloatClamped((float)(sums[j].c3 / sums[j].weight)),
          UNorm32.FromFloatClamped((float)(sums[j].a / sums[j].weight))
        );
      }
    }

    return palette;
  }

  /// <summary>
  /// Evaluates palette quality as negative mean squared error (higher is better).
  /// </summary>
  private static double _EvaluatePaletteQuality<TWork>(
    (float c1, float c2, float c3, float a, uint count)[] colors,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork> {

    var paletteFloats = palette.Select(p => {
      var (c1, c2, c3, a) = p.ToNormalized();
      return (c1: c1.ToFloat(), c2: c2.ToFloat(), c3: c3.ToFloat(), a: a.ToFloat());
    }).ToArray();

    var totalError = 0.0;
    var totalWeight = 0.0;

    foreach (var (c1, c2, c3, a, count) in colors) {
      var minDist = double.MaxValue;
      foreach (var (pc1, pc2, pc3, pa) in paletteFloats) {
        var dist = (c1 - pc1) * (c1 - pc1) +
                   (c2 - pc2) * (c2 - pc2) +
                   (c3 - pc3) * (c3 - pc3) +
                   (a - pa) * (a - pa);
        if (dist < minDist)
          minDist = dist;
      }

      totalError += minDist * count;
      totalWeight += count;
    }

    // Return negative MSE so higher is better
    return -totalError / totalWeight;
  }

  #endregion

}
