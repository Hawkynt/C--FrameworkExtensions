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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

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

        // Byte-domain fast path for the dominant histogram-build phase (the weighted-sum sweep).
        // The variance/split helpers below still go through ToNormalized() because they index over
        // axis-projected values per call; M3 narrowly addresses the linear sum.
        // Parallelise the root-cube sweep when large enough to amortise spawn overhead.
        double sumC1, sumC2, sumC3, sumA;
        long totalCount;
        if (typeof(TWork) == typeof(Bgra8888)) {
          if (colors.Count >= _ParallelHistogramThreshold)
            _SumWeightedFast32bpp4chParallel<BgraLayout>(colors, out sumC1, out sumC2, out sumC3, out sumA, out totalCount);
          else
            _SumWeightedFast32bpp4ch<BgraLayout>(colors, out sumC1, out sumC2, out sumC3, out sumA, out totalCount);
        } else {
          _SumWeightedSlow(colors, out sumC1, out sumC2, out sumC3, out sumA, out totalCount);
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

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private static void _SumWeightedSlow(List<(TWork color, uint count)> colors, out double sumC1, out double sumC2, out double sumC3, out double sumA, out long totalCount) {
        sumC1 = 0; sumC2 = 0; sumC3 = 0; sumA = 0; totalCount = 0;
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
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private static void _SumWeightedFast32bpp4ch<TLayout>(List<(TWork color, uint count)> colors, out double sumC1, out double sumC2, out double sumC3, out double sumA, out long totalCount)
        where TLayout : struct {
        sumC1 = 0; sumC2 = 0; sumC3 = 0; sumA = 0; totalCount = 0;
        foreach (var entry in colors) {
          var color = entry.color;
          var packed = Unsafe.As<TWork, uint>(ref color);
          var (c1, c2, c3, a) = StorageLayoutFast.UnpackFloats<TLayout>(packed);
          var count = entry.count;
          sumC1 += c1 * count;
          sumC2 += c2 * count;
          sumC3 += c3 * count;
          sumA += a * count;
          totalCount += count;
        }
      }

      /// <summary>Parallel partition-then-merge of <see cref="_SumWeightedFast32bpp4ch{TLayout}"/>.</summary>
      private static void _SumWeightedFast32bpp4chParallel<TLayout>(List<(TWork color, uint count)> colors, out double sumC1, out double sumC2, out double sumC3, out double sumA, out long totalCount)
        where TLayout : struct {
        var total = colors.Count;
        var partitionCount = Math.Min(Environment.ProcessorCount, Math.Max(2, total / 16384));
        var chunkSize = (total + partitionCount - 1) / partitionCount;
        var partials = new (double c1, double c2, double c3, double a, long n)[partitionCount];

        Parallel.For(0, partitionCount, p => {
          var start = p * chunkSize;
          var end = Math.Min(start + chunkSize, total);
          double s1 = 0, s2 = 0, s3 = 0, sA = 0; long n = 0;
          for (var i = start; i < end; ++i) {
            var entry = colors[i];
            var color = entry.color;
            var packed = Unsafe.As<TWork, uint>(ref color);
            var (c1, c2, c3, a) = StorageLayoutFast.UnpackFloats<TLayout>(packed);
            var count = entry.count;
            s1 += c1 * count;
            s2 += c2 * count;
            s3 += c3 * count;
            sA += a * count;
            n += count;
          }
          partials[p] = (s1, s2, s3, sA, n);
        });

        sumC1 = sumC2 = sumC3 = sumA = 0; totalCount = 0;
        for (var p = 0; p < partitionCount; ++p) {
          sumC1 += partials[p].c1;
          sumC2 += partials[p].c2;
          sumC3 += partials[p].c3;
          sumA += partials[p].a;
          totalCount += partials[p].n;
        }
      }

      /// <summary>Same threshold as Wu — see WuQuantizer for rationale.</summary>
      private const int _ParallelHistogramThreshold = 65536;

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
          var weightedSum = dist1.Sum(kv => { var d = kv.Key - mean1; return d * d * kv.Value; }) +
                           dist2.Sum(kv => { var d = kv.Key - mean2; return d * d * kv.Value; });

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
