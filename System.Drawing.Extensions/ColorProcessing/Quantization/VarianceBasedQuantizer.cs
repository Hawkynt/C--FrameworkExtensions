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
/// Advanced variance-based color quantizer using weighted variance optimization.
/// </summary>
/// <remarks>
/// <para>
/// Uses weighted variance optimization to find optimal split points,
/// minimizing the sum of projected variances after each split.
/// </para>
/// <para>
/// This produces higher quality results than simple variance cut by
/// considering the optimal split threshold for each axis.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Variance, DisplayName = "Variance Based", QualityRating = 8)]
public struct VarianceBasedQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, _ReduceColorsTo);

    private static IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var cubes = new List<ColorCube> { new(histogram.ToList()) };

      while (cubes.Count < colorCount) {
        // Find cube with highest weighted variance
        var largestCube = cubes.OrderByDescending(c => c.WeightedVariance).FirstOrDefault();
        if (largestCube is not { ColorCount: > 1 })
          break;

        cubes.Remove(largestCube);
        var splitCubes = largestCube.Split();
        cubes.AddRange(splitCubes);
      }

      return cubes.Select(c => c.AverageColor);
    }

    private sealed class ColorCube {
      private readonly List<(TWork color, uint count)> _colors;
      private readonly float _avgC1, _avgC2, _avgC3, _avgA;
      private readonly double _varC1, _varC2, _varC3;
      private readonly double _weightedVariance;
      private readonly long _totalCount;

      public ColorCube(List<(TWork color, uint count)> colors) {
        this._colors = colors;

        if (colors.Count == 0) {
          this._avgC1 = this._avgC2 = this._avgC3 = this._avgA = 0;
          this._varC1 = this._varC2 = this._varC3 = 0;
          this._weightedVariance = 0;
          this._totalCount = 0;
          return;
        }

        // Calculate weighted sums for average
        double sumC1 = 0, sumC2 = 0, sumC3 = 0, sumA = 0;
        long totalCount = 0;

        foreach (var (color, count) in colors) {
          var (c1N, c2N, c3N, aN) = color.ToNormalized();
          var c1 = c1N.ToFloat();
          var c2 = c2N.ToFloat();
          var c3 = c3N.ToFloat();
          var a = aN.ToFloat();

          sumC1 += c1 * count;
          sumC2 += c2 * count;
          sumC3 += c3 * count;
          sumA += a * count;
          totalCount += count;
        }

        this._totalCount = totalCount;
        if (totalCount == 0) {
          this._avgC1 = this._avgC2 = this._avgC3 = this._avgA = 0;
          this._varC1 = this._varC2 = this._varC3 = 0;
          this._weightedVariance = 0;
          return;
        }

        this._avgC1 = (float)(sumC1 / totalCount);
        this._avgC2 = (float)(sumC2 / totalCount);
        this._avgC3 = (float)(sumC3 / totalCount);
        this._avgA = (float)(sumA / totalCount);

        // Calculate projected variances
        this._varC1 = _CalculateProjectedVariance(colors, c => c.color.ToNormalized().C1.ToFloat(), this._avgC1, totalCount);
        this._varC2 = _CalculateProjectedVariance(colors, c => c.color.ToNormalized().C2.ToFloat(), this._avgC2, totalCount);
        this._varC3 = _CalculateProjectedVariance(colors, c => c.color.ToNormalized().C3.ToFloat(), this._avgC3, totalCount);

        // Weighted Variance = totalCount * (sigma_c1^2 + sigma_c2^2 + sigma_c3^2)
        this._weightedVariance = (this._varC1 + this._varC2 + this._varC3) * totalCount;
      }

      private static double _CalculateProjectedVariance(
        List<(TWork color, uint count)> colors,
        Func<(TWork color, uint count), float> selector,
        float mean,
        long totalCount
      ) {
        double variance = 0;
        foreach (var item in colors) {
          var diff = selector(item) - mean;
          variance += diff * diff * item.count;
        }
        return variance / totalCount;
      }

      public double WeightedVariance => this._weightedVariance;
      public int ColorCount => this._colors.Count;

      public TWork AverageColor => this._colors.Count == 0
        ? default
        : ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(this._avgC1),
          UNorm32.FromFloatClamped(this._avgC2),
          UNorm32.FromFloatClamped(this._avgC3),
          UNorm32.FromFloatClamped(this._avgA)
        );

      public IEnumerable<ColorCube> Split() {
        if (this._colors.Count <= 1)
          return [this];

        // Find optimal split for each axis
        var c1Dist = this._GetProjectedDistribution(item => item.color.ToNormalized().C1.ToFloat());
        var c2Dist = this._GetProjectedDistribution(item => item.color.ToNormalized().C2.ToFloat());
        var c3Dist = this._GetProjectedDistribution(item => item.color.ToNormalized().C3.ToFloat());

        var c1Split = _FindOptimalSplit(c1Dist);
        var c2Split = _FindOptimalSplit(c2Dist);
        var c3Split = _FindOptimalSplit(c3Dist);

        // Choose axis with smallest weighted sum of projected variances
        Func<(TWork color, uint count), float> splitComponent;
        float optimalThreshold;
        var minWeightedSum = double.MaxValue;

        splitComponent = item => item.color.ToNormalized().C1.ToFloat();
        optimalThreshold = c1Split.OptimalThreshold;

        if (c1Split.WeightedSum < minWeightedSum) {
          minWeightedSum = c1Split.WeightedSum;
          splitComponent = item => item.color.ToNormalized().C1.ToFloat();
          optimalThreshold = c1Split.OptimalThreshold;
        }

        if (c2Split.WeightedSum < minWeightedSum) {
          minWeightedSum = c2Split.WeightedSum;
          splitComponent = item => item.color.ToNormalized().C2.ToFloat();
          optimalThreshold = c2Split.OptimalThreshold;
        }

        if (c3Split.WeightedSum < minWeightedSum) {
          minWeightedSum = c3Split.WeightedSum;
          splitComponent = item => item.color.ToNormalized().C3.ToFloat();
          optimalThreshold = c3Split.OptimalThreshold;
        }

        // If no valid split found, return original cube
        if (minWeightedSum == double.MaxValue)
          return [this];

        // Split colors at optimal threshold
        var firstHalf = new List<(TWork color, uint count)>();
        var secondHalf = new List<(TWork color, uint count)>();

        foreach (var item in this._colors) {
          if (splitComponent(item) < optimalThreshold)
            firstHalf.Add(item);
          else
            secondHalf.Add(item);
        }

        // Ensure both halves are non-empty
        if (firstHalf.Count == 0 || secondHalf.Count == 0)
          return [this];

        return [new ColorCube(firstHalf), new ColorCube(secondHalf)];
      }

      private Dictionary<float, uint> _GetProjectedDistribution(Func<(TWork color, uint count), float> selector) {
        var distribution = new Dictionary<float, uint>();
        foreach (var item in this._colors) {
          var value = selector(item);
          if (!distribution.TryAdd(value, item.count))
            distribution[value] += item.count;
        }
        return distribution;
      }

      private static (float OptimalThreshold, double WeightedSum) _FindOptimalSplit(Dictionary<float, uint> distribution) {
        var minWeightedSum = double.MaxValue;
        var optimalThreshold = 0f;

        var sortedValues = distribution.Keys.OrderBy(v => v).ToList();
        foreach (var threshold in sortedValues) {
          // Split distribution into two parts
          var dist1 = distribution.Where(kv => kv.Key < threshold).ToDictionary(kv => kv.Key, kv => kv.Value);
          var dist2 = distribution.Where(kv => kv.Key >= threshold).ToDictionary(kv => kv.Key, kv => kv.Value);

          if (dist1.Count == 0 || dist2.Count == 0)
            continue;

          // Calculate mean for each part
          var sum1 = dist1.Sum(kv => (double)kv.Key * kv.Value);
          var count1 = dist1.Sum(kv => (long)kv.Value);
          var mean1 = count1 > 0 ? sum1 / count1 : 0;

          var sum2 = dist2.Sum(kv => (double)kv.Key * kv.Value);
          var count2 = dist2.Sum(kv => (long)kv.Value);
          var mean2 = count2 > 0 ? sum2 / count2 : 0;

          // Calculate weighted sum of projected variances
          var weightedSum = dist1.Sum(kv => Math.Pow(kv.Key - mean1, 2) * kv.Value) +
                           dist2.Sum(kv => Math.Pow(kv.Key - mean2, 2) * kv.Value);

          if (weightedSum < minWeightedSum) {
            minWeightedSum = weightedSum;
            optimalThreshold = threshold;
          }
        }

        return (optimalThreshold, minWeightedSum);
      }
    }
  }
}
