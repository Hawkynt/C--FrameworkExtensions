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
/// Binary splitting color quantizer using variance-based principal axis splitting.
/// </summary>
/// <remarks>
/// <para>
/// Splits color cubes along the principal axis (direction of maximum variance).
/// This is a simplified implementation that uses the component with maximum variance
/// as an approximation of the principal eigenvector.
/// </para>
/// <para>
/// Full PCA-based binary splitting requires eigenvalue decomposition support.
/// This simplified approach provides good results for most use cases.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "Binary Splitting", QualityRating = 6)]
public struct BinarySplittingQuantizer : IQuantizer {

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
        // Find cube with highest total variance
        var largestCube = cubes.OrderByDescending(c => c.TotalVariance).FirstOrDefault();
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
      private readonly double _totalVariance;
      private readonly long _totalCount;

      public ColorCube(List<(TWork color, uint count)> colors) {
        this._colors = colors;

        if (colors.Count == 0) {
          this._avgC1 = this._avgC2 = this._avgC3 = this._avgA = 0;
          this._varC1 = this._varC2 = this._varC3 = 0;
          this._totalVariance = 0;
          this._totalCount = 0;
          return;
        }

        double sumC1, sumC2, sumC3, sumA;
        long totalCount;

        // byte-domain fast path for 32bpp 4-channel storage TWork (today only Bgra8888).
        // parallelise large root-cube sweeps; sub-cubes naturally fall under the threshold.
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
          this._totalVariance = 0;
          return;
        }

        this._avgC1 = (float)(sumC1 / totalCount);
        this._avgC2 = (float)(sumC2 / totalCount);
        this._avgC3 = (float)(sumC3 / totalCount);
        this._avgA = (float)(sumA / totalCount);

        // Calculate variance for each component
        double varC1, varC2, varC3;
        if (typeof(TWork) == typeof(Bgra8888)) {
          if (colors.Count >= _ParallelHistogramThreshold)
            _VarianceFast32bpp4chParallel<BgraLayout>(colors, this._avgC1, this._avgC2, this._avgC3, out varC1, out varC2, out varC3);
          else
            _VarianceFast32bpp4ch<BgraLayout>(colors, this._avgC1, this._avgC2, this._avgC3, out varC1, out varC2, out varC3);
        } else {
          _VarianceSlow(colors, this._avgC1, this._avgC2, this._avgC3, out varC1, out varC2, out varC3);
        }

        this._varC1 = varC1 / totalCount;
        this._varC2 = varC2 / totalCount;
        this._varC3 = varC3 / totalCount;
        this._totalVariance = this._varC1 + this._varC2 + this._varC3;
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

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private static void _VarianceSlow(List<(TWork color, uint count)> colors, float avgC1, float avgC2, float avgC3, out double varC1, out double varC2, out double varC3) {
        varC1 = 0; varC2 = 0; varC3 = 0;
        foreach (var (color, count) in colors) {
          var (c1N, c2N, c3N, _) = color.ToNormalized();
          var c1 = c1N.ToFloat();
          var c2 = c2N.ToFloat();
          var c3 = c3N.ToFloat();
          var d1 = c1 - avgC1;
          var d2 = c2 - avgC2;
          var d3 = c3 - avgC3;
          varC1 += d1 * d1 * count;
          varC2 += d2 * d2 * count;
          varC3 += d3 * d3 * count;
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private static void _VarianceFast32bpp4ch<TLayout>(List<(TWork color, uint count)> colors, float avgC1, float avgC2, float avgC3, out double varC1, out double varC2, out double varC3)
        where TLayout : struct {
        varC1 = 0; varC2 = 0; varC3 = 0;
        foreach (var entry in colors) {
          var color = entry.color;
          var packed = Unsafe.As<TWork, uint>(ref color);
          var (c1, c2, c3, _) = StorageLayoutFast.UnpackFloats<TLayout>(packed);
          var count = entry.count;
          var d1 = c1 - avgC1;
          var d2 = c2 - avgC2;
          var d3 = c3 - avgC3;
          varC1 += d1 * d1 * count;
          varC2 += d2 * d2 * count;
          varC3 += d3 * d3 * count;
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

      /// <summary>Parallel partition-then-merge of <see cref="_VarianceFast32bpp4ch{TLayout}"/>.</summary>
      private static void _VarianceFast32bpp4chParallel<TLayout>(List<(TWork color, uint count)> colors, float avgC1, float avgC2, float avgC3, out double varC1, out double varC2, out double varC3)
        where TLayout : struct {
        var total = colors.Count;
        var partitionCount = Math.Min(Environment.ProcessorCount, Math.Max(2, total / 16384));
        var chunkSize = (total + partitionCount - 1) / partitionCount;
        var partials = new (double v1, double v2, double v3)[partitionCount];

        Parallel.For(0, partitionCount, p => {
          var start = p * chunkSize;
          var end = Math.Min(start + chunkSize, total);
          double v1 = 0, v2 = 0, v3 = 0;
          for (var i = start; i < end; ++i) {
            var entry = colors[i];
            var color = entry.color;
            var packed = Unsafe.As<TWork, uint>(ref color);
            var (c1, c2, c3, _) = StorageLayoutFast.UnpackFloats<TLayout>(packed);
            var count = entry.count;
            var d1 = c1 - avgC1;
            var d2 = c2 - avgC2;
            var d3 = c3 - avgC3;
            v1 += d1 * d1 * count;
            v2 += d2 * d2 * count;
            v3 += d3 * d3 * count;
          }
          partials[p] = (v1, v2, v3);
        });

        varC1 = varC2 = varC3 = 0;
        for (var p = 0; p < partitionCount; ++p) {
          varC1 += partials[p].v1;
          varC2 += partials[p].v2;
          varC3 += partials[p].v3;
        }
      }

      /// <summary>Same threshold as Wu — see WuQuantizer for rationale.</summary>
      private const int _ParallelHistogramThreshold = 65536;

      public double TotalVariance => this._totalVariance;
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

        // Find axis with maximum variance (approximates principal eigenvector)
        Func<(TWork color, uint count), float> getComponent;
        float meanValue;

        if (this._varC1 >= this._varC2 && this._varC1 >= this._varC3) {
          getComponent = item => item.color.ToNormalized().C1.ToFloat();
          meanValue = this._avgC1;
        } else if (this._varC2 >= this._varC1 && this._varC2 >= this._varC3) {
          getComponent = item => item.color.ToNormalized().C2.ToFloat();
          meanValue = this._avgC2;
        } else {
          getComponent = item => item.color.ToNormalized().C3.ToFloat();
          meanValue = this._avgC3;
        }

        // Split colors at the mean value along the principal axis
        var firstHalf = new List<(TWork color, uint count)>();
        var secondHalf = new List<(TWork color, uint count)>();

        foreach (var item in this._colors) {
          if (getComponent(item) < meanValue)
            firstHalf.Add(item);
          else
            secondHalf.Add(item);
        }

        // Ensure both halves are non-empty
        if (firstHalf.Count == 0 || secondHalf.Count == 0)
          return [this];

        return [new ColorCube(firstHalf), new ColorCube(secondHalf)];
      }
    }
  }
}
