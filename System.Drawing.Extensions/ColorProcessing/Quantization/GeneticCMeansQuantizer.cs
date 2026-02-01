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
/// Genetic C-Means color quantizer combining genetic algorithms with C-Means clustering.
/// </summary>
/// <remarks>
/// <para>Uses a genetic algorithm for global optimization to escape local optima, combined with
/// C-Means refinement for local convergence. Each offspring is refined with a few C-Means iterations.</para>
/// <para>While computationally expensive, this approach can find better solutions for complex color distributions.</para>
/// <para>Reference: Scheunders (1997) - "A comparison of clustering algorithms applied to color image quantization"</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Genetic C-Means", Author = "P. Scheunders", Year = 1997, QualityRating = 10)]
public struct GeneticCMeansQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the population size (number of candidate palettes).
  /// </summary>
  public int PopulationSize { get; set; } = 20;

  /// <summary>
  /// Gets or sets the number of generations to evolve.
  /// </summary>
  public int Generations { get; set; } = 50;

  /// <summary>
  /// Gets or sets the tournament size for parent selection.
  /// </summary>
  public int TournamentSize { get; set; } = 3;

  /// <summary>
  /// Gets or sets the standard deviation for Gaussian mutation.
  /// </summary>
  public float MutationSigma { get; set; } = 10.0f;

  /// <summary>
  /// Gets or sets the number of elite individuals to preserve each generation.
  /// </summary>
  public int EliteCount { get; set; } = 2;

  /// <summary>
  /// Gets or sets the number of C-Means iterations to refine each offspring.
  /// </summary>
  public int CMeansIterationsPerOffspring { get; set; } = 3;

  /// <summary>
  /// Gets or sets the maximum sample size for processing.
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  public GeneticCMeansQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.PopulationSize,
    this.Generations,
    this.TournamentSize,
    this.MutationSigma,
    this.EliteCount,
    this.CMeansIterationsPerOffspring,
    this.MaxSampleSize
  );

  internal sealed class Kernel<TWork>(
    int populationSize,
    int generations,
    int tournamentSize,
    float mutationSigma,
    int eliteCount,
    int cMeansIterationsPerOffspring,
    int maxSampleSize
  ) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= colorCount)
        return colors.Select(c => c.color);

      // Sample histogram for GA
      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize, 42);

      // Convert to float arrays
      var colorData = colors.Select(c => {
        var (c1, c2, c3, a) = c.color.ToNormalized();
        return (c1: (double)c1.ToFloat(), c2: (double)c2.ToFloat(), c3: (double)c3.ToFloat(), a: (double)a.ToFloat(), count: c.count);
      }).ToArray();

      var random = new Random(42);
      var normalizedSigma = mutationSigma / 255.0; // Normalize mutation sigma to [0,1] range

      // Initialize population using K-means++ style initialization
      var population = new double[populationSize][][];
      var fitness = new double[populationSize];

      for (var i = 0; i < populationSize; ++i) {
        population[i] = _InitializeRandomPalette(colorData, colorCount, random);
        fitness[i] = _EvaluateFitness(colorData, population[i]);
      }

      // Evolve for specified generations
      for (var gen = 0; gen < generations; ++gen) {
        // Sort by fitness (descending, higher is better)
        var sorted = Enumerable.Range(0, populationSize)
          .OrderByDescending(i => fitness[i])
          .ToArray();

        var newPopulation = new double[populationSize][][];
        var newFitness = new double[populationSize];

        // Preserve elites
        for (var i = 0; i < eliteCount; ++i) {
          newPopulation[i] = population[sorted[i]];
          newFitness[i] = fitness[sorted[i]];
        }

        // Create offspring for remaining slots
        for (var i = eliteCount; i < populationSize; ++i) {
          // Tournament selection for parents
          var parent1 = _TournamentSelect(population, fitness, tournamentSize, random);
          var parent2 = _TournamentSelect(population, fitness, tournamentSize, random);

          // Uniform crossover
          var offspring = _UniformCrossover(parent1, parent2, random);

          // Gaussian mutation
          _GaussianMutate(offspring, normalizedSigma, random);

          // Refine with C-Means iterations
          _CMeansRefine(offspring, colorData, cMeansIterationsPerOffspring);

          newPopulation[i] = offspring;
          newFitness[i] = _EvaluateFitness(colorData, offspring);
        }

        population = newPopulation;
        fitness = newFitness;
      }

      // Return best individual
      var bestIdx = 0;
      for (var i = 1; i < populationSize; ++i)
        if (fitness[i] > fitness[bestIdx])
          bestIdx = i;

      return population[bestIdx].Select(c => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c[0]))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c[1]))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c[2]))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c[3])))
      ));
    }

    private static double[][] _InitializeRandomPalette(
      (double c1, double c2, double c3, double a, uint count)[] colorData,
      int colorCount,
      Random random
    ) {
      // K-means++ style initialization
      var palette = new double[colorCount][];
      var totalWeight = colorData.Sum(c => (long)c.count);

      // First center: random weighted selection
      var target = random.NextDouble() * totalWeight;
      long cumulative = 0;
      var firstIdx = 0;
      for (var i = 0; i < colorData.Length; ++i) {
        cumulative += colorData[i].count;
        if (cumulative >= target) {
          firstIdx = i;
          break;
        }
      }

      palette[0] = [colorData[firstIdx].c1, colorData[firstIdx].c2, colorData[firstIdx].c3, colorData[firstIdx].a];

      // Remaining centers: proportional to squared distance
      var distances = new double[colorData.Length];
      for (var i = 0; i < colorData.Length; ++i)
        distances[i] = _SquaredDistance(colorData[i], palette[0]);

      for (var c = 1; c < colorCount; ++c) {
        var totalDist = 0.0;
        for (var i = 0; i < colorData.Length; ++i)
          totalDist += distances[i] * colorData[i].count;

        if (totalDist <= 0) {
          // Fallback to random selection
          var idx = random.Next(colorData.Length);
          palette[c] = [colorData[idx].c1, colorData[idx].c2, colorData[idx].c3, colorData[idx].a];
        } else {
          target = random.NextDouble() * totalDist;
          double cumulativeDist = 0;
          var selectedIdx = 0;

          for (var i = 0; i < colorData.Length; ++i) {
            cumulativeDist += distances[i] * colorData[i].count;
            if (cumulativeDist >= target) {
              selectedIdx = i;
              break;
            }
          }

          palette[c] = [colorData[selectedIdx].c1, colorData[selectedIdx].c2, colorData[selectedIdx].c3, colorData[selectedIdx].a];
        }

        // Update distances
        for (var i = 0; i < colorData.Length; ++i) {
          var newDist = _SquaredDistance(colorData[i], palette[c]);
          if (newDist < distances[i])
            distances[i] = newDist;
        }
      }

      return palette;
    }

    private static double _EvaluateFitness(
      (double c1, double c2, double c3, double a, uint count)[] colorData,
      double[][] palette
    ) {
      // Fitness = negative MSE (higher is better)
      var totalError = 0.0;
      var totalWeight = 0.0;

      foreach (var (c1, c2, c3, a, count) in colorData) {
        var minDist = double.MaxValue;
        foreach (var center in palette) {
          var d1 = c1 - center[0];
          var d2 = c2 - center[1];
          var d3 = c3 - center[2];
          var d4 = a - center[3];
          var dist = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
          if (dist < minDist)
            minDist = dist;
        }

        totalError += minDist * count;
        totalWeight += count;
      }

      return -totalError / totalWeight;
    }

    private static double[][] _TournamentSelect(
      double[][][] population,
      double[] fitness,
      int tournamentSize,
      Random random
    ) {
      var bestIdx = random.Next(population.Length);
      var bestFitness = fitness[bestIdx];

      for (var i = 1; i < tournamentSize; ++i) {
        var idx = random.Next(population.Length);
        if (fitness[idx] <= bestFitness)
          continue;

        bestFitness = fitness[idx];
        bestIdx = idx;
      }

      return population[bestIdx];
    }

    private static double[][] _UniformCrossover(double[][] parent1, double[][] parent2, Random random) {
      var offspring = new double[parent1.Length][];
      for (var i = 0; i < parent1.Length; ++i)
        offspring[i] = random.Next(2) == 0
          ? [..parent1[i]]
          : [..parent2[i]];

      return offspring;
    }

    private static void _GaussianMutate(double[][] palette, double sigma, Random random) {
      foreach (var color in palette)
        for (var i = 0; i < 4; ++i) {
          // Box-Muller transform for Gaussian
          var u1 = 1.0 - random.NextDouble();
          var u2 = 1.0 - random.NextDouble();
          var gaussian = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

          color[i] = Math.Max(0, Math.Min(1, color[i] + gaussian * sigma));
        }
    }

    private static void _CMeansRefine(
      double[][] palette,
      (double c1, double c2, double c3, double a, uint count)[] colorData,
      int iterations
    ) {
      var k = palette.Length;
      var assignments = new int[colorData.Length];

      for (var iter = 0; iter < iterations; ++iter) {
        // Assign colors to nearest centroid
        for (var i = 0; i < colorData.Length; ++i) {
          var nearest = 0;
          var minDist = _SquaredDistance(colorData[i], palette[0]);

          for (var j = 1; j < k; ++j) {
            var dist = _SquaredDistance(colorData[i], palette[j]);
            if (!(dist < minDist))
              continue;

            minDist = dist;
            nearest = j;
          }

          assignments[i] = nearest;
        }

        // Recompute centroids
        var sums = new double[k][];
        var weights = new double[k];

        for (var j = 0; j < k; ++j)
          sums[j] = [0, 0, 0, 0];

        for (var i = 0; i < colorData.Length; ++i) {
          var cluster = assignments[i];
          sums[cluster][0] += colorData[i].c1 * colorData[i].count;
          sums[cluster][1] += colorData[i].c2 * colorData[i].count;
          sums[cluster][2] += colorData[i].c3 * colorData[i].count;
          sums[cluster][3] += colorData[i].a * colorData[i].count;
          weights[cluster] += colorData[i].count;
        }

        for (var j = 0; j < k; ++j)
          if (weights[j] > 0) {
            palette[j][0] = sums[j][0] / weights[j];
            palette[j][1] = sums[j][1] / weights[j];
            palette[j][2] = sums[j][2] / weights[j];
            palette[j][3] = sums[j][3] / weights[j];
          }
      }
    }

    private static double _SquaredDistance(
      (double c1, double c2, double c3, double a, uint count) color,
      double[] center
    ) {
      var d1 = color.c1 - center[0];
      var d2 = color.c2 - center[1];
      var d3 = color.c3 - center[2];
      var d4 = color.a - center[3];
      return d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
    }

  }
}
