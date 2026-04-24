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
/// K-Medoids (PAM - Partitioning Around Medoids) color quantizer.
/// </summary>
/// <remarks>
/// <para>
/// Unlike K-Means which emits synthetic centroids, K-Medoids picks actual input colors as palette anchors.
/// This produces crisper, more pixel-art-friendly palettes since every palette entry is guaranteed to be
/// an observed color.
/// </para>
/// <para>
/// The classical PAM algorithm seeds <c>k</c> medoids, then for each non-medoid checks whether swapping
/// it with any existing medoid reduces the total assignment cost; the best improving swap is taken each
/// round, and iteration stops when no swap improves cost.
/// </para>
/// <para>
/// For large inputs (more than <see cref="ClaraSampleThreshold"/> unique colors) a CLARA-style sampling
/// loop is used: PAM is run on several small random samples and the best medoid set wins. This keeps
/// runtime bounded regardless of palette diversity.
/// </para>
/// <para>
/// Distance is Euclidean in the active <typeparamref name="TWork"/> normalized space; when the pipeline
/// operates in <c>OklabaF</c> this corresponds to ΔE in OkLab (perceptual).
/// </para>
/// <para>Reference: Kaufman &amp; Rousseeuw (1990) - "Finding Groups in Data: An Introduction to Cluster Analysis".</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "K-Medoids", Author = "Kaufman & Rousseeuw", Year = 1990, QualityRating = 8)]
public struct KMedoidsQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum number of PAM swap iterations before stopping.
  /// </summary>
  public int MaxIterations { get; set; } = 50;

  /// <summary>
  /// Gets or sets the CLARA sample size used when the input exceeds <see cref="ClaraSampleThreshold"/>.
  /// </summary>
  public int ClaraSampleSize { get; set; } = 1024;

  /// <summary>
  /// Gets or sets the number of CLARA resampling rounds (the best medoid set is kept).
  /// </summary>
  public int ClaraRounds { get; set; } = 5;

  /// <summary>
  /// Gets or sets the unique-color count above which CLARA-style sampling is used instead of full PAM.
  /// </summary>
  public int ClaraSampleThreshold { get; set; } = 2048;

  /// <summary>
  /// Gets or sets the deterministic random seed used for medoid initialization and CLARA sampling.
  /// </summary>
  public int Seed { get; set; } = 42;

  public KMedoidsQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MaxIterations,
    this.ClaraSampleSize,
    this.ClaraRounds,
    this.ClaraSampleThreshold,
    this.Seed);

  internal sealed class Kernel<TWork>(
    int maxIterations,
    int claraSampleSize,
    int claraRounds,
    int claraSampleThreshold,
    int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= k)
        return colors.Select(c => c.color);

      // Project to normalized float vectors once for speed.
      var points = _Project(colors);

      // Decide full PAM vs CLARA based on dataset size.
      if (points.Length <= claraSampleThreshold)
        return _RunPam(points, k, new Random(seed));

      return _RunClara(points, k, new Random(seed));
    }

    private static (double c1, double c2, double c3, double a, uint count, int originalIndex)[] _Project(
      (TWork color, uint count)[] colors) {
      var arr = new (double, double, double, double, uint, int)[colors.Length];
      for (var i = 0; i < colors.Length; ++i) {
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        arr[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat(), colors[i].count, i);
      }

      return arr;
    }

    private TWork[] _RunPam(
      (double c1, double c2, double c3, double a, uint count, int originalIndex)[] points,
      int k,
      Random random) {
      var medoidIdx = _InitializeMedoids(points, k, random);
      var (cost, nearest) = _AssignToMedoids(points, medoidIdx);

      for (var iter = 0; iter < maxIterations; ++iter) {
        var bestGain = 0.0;
        var bestMedoidSlot = -1;
        var bestCandidate = -1;

        // Try swapping each medoid with each non-medoid; keep the best cost-reducing swap.
        for (var m = 0; m < k; ++m) {
          var currentMedoid = medoidIdx[m];

          for (var i = 0; i < points.Length; ++i) {
            if (i == currentMedoid)
              continue;

            // Avoid evaluating candidates that are already medoids.
            var isMedoid = false;
            for (var mm = 0; mm < k; ++mm) {
              if (medoidIdx[mm] != i)
                continue;

              isMedoid = true;
              break;
            }

            if (isMedoid)
              continue;

            medoidIdx[m] = i;
            var (newCost, _) = _AssignToMedoids(points, medoidIdx);
            var gain = cost - newCost;
            medoidIdx[m] = currentMedoid;

            if (gain <= bestGain)
              continue;

            bestGain = gain;
            bestMedoidSlot = m;
            bestCandidate = i;
          }
        }

        if (bestMedoidSlot < 0)
          break; // converged — no swap improves cost

        medoidIdx[bestMedoidSlot] = bestCandidate;
        (cost, nearest) = _AssignToMedoids(points, medoidIdx);
      }

      return _MedoidsToPalette(points, medoidIdx);
    }

    private TWork[] _RunClara(
      (double c1, double c2, double c3, double a, uint count, int originalIndex)[] points,
      int k,
      Random random) {
      var sampleSize = Math.Max(k * 2, Math.Min(claraSampleSize, points.Length));
      var bestMedoids = Array.Empty<int>();
      var bestCost = double.MaxValue;

      for (var round = 0; round < claraRounds; ++round) {
        var sample = _WeightedSample(points, sampleSize, random);
        var roundRandom = new Random(seed + round + 1);
        var medoidIdx = _InitializeMedoids(sample, k, roundRandom);
        var (cost, _) = _AssignToMedoids(sample, medoidIdx);

        // Mini PAM loop on the sample.
        for (var iter = 0; iter < maxIterations; ++iter) {
          var bestGain = 0.0;
          var bestMedoidSlot = -1;
          var bestCandidate = -1;

          for (var m = 0; m < k; ++m) {
            var currentMedoid = medoidIdx[m];
            for (var i = 0; i < sample.Length; ++i) {
              if (i == currentMedoid)
                continue;

              var isMedoid = false;
              for (var mm = 0; mm < k; ++mm) {
                if (medoidIdx[mm] != i)
                  continue;

                isMedoid = true;
                break;
              }

              if (isMedoid)
                continue;

              medoidIdx[m] = i;
              var (newCost, _) = _AssignToMedoids(sample, medoidIdx);
              var gain = cost - newCost;
              medoidIdx[m] = currentMedoid;

              if (gain <= bestGain)
                continue;

              bestGain = gain;
              bestMedoidSlot = m;
              bestCandidate = i;
            }
          }

          if (bestMedoidSlot < 0)
            break;

          medoidIdx[bestMedoidSlot] = bestCandidate;
          (cost, _) = _AssignToMedoids(sample, medoidIdx);
        }

        // Evaluate the candidate medoids on the full dataset using their original-index mapping.
        var fullMedoids = medoidIdx.Select(i => sample[i].originalIndex).ToArray();
        var (fullCost, _) = _AssignToMedoids(points, fullMedoids);

        if (fullCost >= bestCost)
          continue;

        bestCost = fullCost;
        bestMedoids = fullMedoids;
      }

      return _MedoidsToPalette(points, bestMedoids);
    }

    private static int[] _InitializeMedoids(
      (double c1, double c2, double c3, double a, uint count, int originalIndex)[] points,
      int k,
      Random random) {
      // K-Means++ style weighted seeding — good quality, deterministic given the seed.
      var chosen = new int[k];
      var distances = new double[points.Length];

      var totalWeight = points.Sum(p => (double)p.count);
      var target = random.NextDouble() * totalWeight;
      var cumulative = 0.0;
      chosen[0] = 0;
      for (var i = 0; i < points.Length; ++i) {
        cumulative += points[i].count;
        if (cumulative < target)
          continue;

        chosen[0] = i;
        break;
      }

      for (var i = 0; i < points.Length; ++i)
        distances[i] = _SquaredDistance(points[i], points[chosen[0]]);

      for (var c = 1; c < k; ++c) {
        var totalDist = 0.0;
        for (var i = 0; i < points.Length; ++i)
          totalDist += distances[i] * points[i].count;

        if (totalDist <= 0) {
          // All remaining points collapse onto existing medoids — pick the next unused index.
          var fallback = 0;
          for (var i = 0; i < points.Length; ++i) {
            var taken = false;
            for (var mm = 0; mm < c; ++mm) {
              if (chosen[mm] != i)
                continue;

              taken = true;
              break;
            }

            if (taken)
              continue;

            fallback = i;
            break;
          }

          chosen[c] = fallback;
          continue;
        }

        target = random.NextDouble() * totalDist;
        cumulative = 0.0;
        var selected = 0;
        for (var i = 0; i < points.Length; ++i) {
          cumulative += distances[i] * points[i].count;
          if (cumulative < target)
            continue;

          selected = i;
          break;
        }

        chosen[c] = selected;

        // Update distances to nearest existing medoid.
        for (var i = 0; i < points.Length; ++i) {
          var d = _SquaredDistance(points[i], points[selected]);
          if (d < distances[i])
            distances[i] = d;
        }
      }

      return chosen;
    }

    private static (double cost, int[] nearest) _AssignToMedoids(
      (double c1, double c2, double c3, double a, uint count, int originalIndex)[] points,
      int[] medoidIdx) {
      var nearest = new int[points.Length];
      var totalCost = 0.0;

      for (var i = 0; i < points.Length; ++i) {
        var best = 0;
        var bestDist = _SquaredDistance(points[i], points[medoidIdx[0]]);
        for (var m = 1; m < medoidIdx.Length; ++m) {
          var d = _SquaredDistance(points[i], points[medoidIdx[m]]);
          if (!(d < bestDist))
            continue;

          bestDist = d;
          best = m;
        }

        nearest[i] = best;
        totalCost += bestDist * points[i].count;
      }

      return (totalCost, nearest);
    }

    private static (double c1, double c2, double c3, double a, uint count, int originalIndex)[] _WeightedSample(
      (double c1, double c2, double c3, double a, uint count, int originalIndex)[] points,
      int sampleSize,
      Random random) {
      if (sampleSize >= points.Length)
        return points;

      // Weighted reservoir (A-Res) — stable with RNG seed.
      var reservoir = new (double c1, double c2, double c3, double a, uint count, int originalIndex, double key)[sampleSize];
      for (var i = 0; i < sampleSize; ++i) {
        var w = Math.Max(1, points[i].count);
        var key = Math.Pow(random.NextDouble(), 1.0 / w);
        reservoir[i] = (points[i].c1, points[i].c2, points[i].c3, points[i].a, points[i].count, points[i].originalIndex, key);
      }

      var minIdx = 0;
      for (var i = 1; i < sampleSize; ++i)
        if (reservoir[i].key < reservoir[minIdx].key)
          minIdx = i;

      for (var i = sampleSize; i < points.Length; ++i) {
        var w = Math.Max(1, points[i].count);
        var key = Math.Pow(random.NextDouble(), 1.0 / w);
        if (!(key > reservoir[minIdx].key))
          continue;

        reservoir[minIdx] = (points[i].c1, points[i].c2, points[i].c3, points[i].a, points[i].count, points[i].originalIndex, key);
        minIdx = 0;
        for (var j = 1; j < sampleSize; ++j)
          if (reservoir[j].key < reservoir[minIdx].key)
            minIdx = j;
      }

      var result = new (double, double, double, double, uint, int)[sampleSize];
      for (var i = 0; i < sampleSize; ++i)
        result[i] = (reservoir[i].c1, reservoir[i].c2, reservoir[i].c3, reservoir[i].a, reservoir[i].count, reservoir[i].originalIndex);

      return result;
    }

    private static TWork[] _MedoidsToPalette(
      (double c1, double c2, double c3, double a, uint count, int originalIndex)[] points,
      int[] medoidIdx) {
      var palette = new TWork[medoidIdx.Length];
      for (var i = 0; i < medoidIdx.Length; ++i) {
        var p = points[medoidIdx[i]];
        palette[i] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, p.c1))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, p.c2))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, p.c3))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, p.a)))
        );
      }

      return palette;
    }

    private static double _SquaredDistance(
      (double c1, double c2, double c3, double a, uint count, int originalIndex) x,
      (double c1, double c2, double c3, double a, uint count, int originalIndex) y) {
      var d1 = x.c1 - y.c1;
      var d2 = x.c2 - y.c2;
      var d3 = x.c3 - y.c3;
      var d4 = x.a - y.a;
      return d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
    }

  }
}
