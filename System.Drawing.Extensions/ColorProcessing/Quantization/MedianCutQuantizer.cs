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
/// Median Cut color quantizer — Heckbert 1980/1982 population-weighted variant.
/// </summary>
/// <remarks>
/// <para>Recursively subdivides the colour cube: at each step pick the box with the
/// longest axis (max − min) and split it at the population-weighted MEDIAN along
/// that axis, so each half holds half the pixel population. After K-1 splits, the
/// K resulting boxes' centroids form the palette.</para>
/// <para>Reference: P. Heckbert, "Color Image Quantization for Frame Buffer Display",
/// SIGGRAPH '82 Proceedings, ACM Computer Graphics 16(3):297-307, July 1982.
/// Splits at cumulative-pixel-count median (not unique-color median) per §3.2 of
/// the paper.</para>
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "Median Cut", Author = "Paul Heckbert", Year = 1982, QualityRating = 6)]
public struct MedianCutQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, _ReduceColorsTo);

    private static IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var hist = histogram.ToList();
      var cubes = new List<ColorCube> { new(hist.Select(h => h.color), hist.Select(h => h.count).ToList()) };

      while (cubes.Count < colorCount) {
        var largestCube = cubes.OrderByDescending(c => c.Volume).FirstOrDefault();
        if (largestCube is not { ColorCount: > 1 })
          break;

        cubes.Remove(largestCube);
        var splitCubes = largestCube.Split();
        cubes.AddRange(splitCubes);
      }

      return cubes.Select(c => c.AverageColor);
    }

    private sealed class ColorCube {
      private readonly List<TWork> _colors;
      // Per-colour pixel counts. Used to compute the population-weighted median during
      // Split(), per Heckbert 1980/82 — splits the cube at the population-weighted median
      // index so each child has roughly equal pixel coverage. Without this, splits favour
      // sparse outliers in unique-color space and waste palette budget.
      private readonly List<uint> _counts;
      private readonly float _c1Min, _c1Max, _c2Min, _c2Max, _c3Min, _c3Max;
      private readonly double _c1Sum, _c2Sum, _c3Sum, _aSum;

      public ColorCube(IEnumerable<TWork> colors) : this(colors, null) { }

      public ColorCube(IEnumerable<TWork> colors, List<uint>? counts) {
        this._colors = colors.ToList();
        this._counts = counts ?? Enumerable.Repeat(1u, this._colors.Count).ToList();
        if (this._colors.Count == 0) {
          this._c1Min = this._c1Max = this._c2Min = this._c2Max = this._c3Min = this._c3Max = 0;
          this._c1Sum = this._c2Sum = this._c3Sum = this._aSum = 0;
          return;
        }

        // Byte-domain fast path for 32bpp 4-channel storage TWork (today only Bgra8888).
        // The branch is folded by the JIT in monomorphic instantiations.
        // When this cube is large enough to amortise the parallel-spawn overhead
        // (typically only the *root* cube during the very first split — sub-cubes
        // are smaller and stay sequential), partition the sweep across cores and
        // merge in deterministic order.
        float c1Min, c1Max, c2Min, c2Max, c3Min, c3Max;
        double c1Sum, c2Sum, c3Sum, aSum;
        if (typeof(TWork) == typeof(Bgra8888)) {
          if (this._colors.Count >= _ParallelHistogramThreshold)
            _ComputeRangesAndSumsFast32bpp4chParallel<BgraLayout>(this._colors, out c1Min, out c1Max, out c2Min, out c2Max, out c3Min, out c3Max, out c1Sum, out c2Sum, out c3Sum, out aSum);
          else
            _ComputeRangesAndSumsFast32bpp4ch<BgraLayout>(this._colors, out c1Min, out c1Max, out c2Min, out c2Max, out c3Min, out c3Max, out c1Sum, out c2Sum, out c3Sum, out aSum);
        } else {
          _ComputeRangesAndSumsSlow(this._colors, out c1Min, out c1Max, out c2Min, out c2Max, out c3Min, out c3Max, out c1Sum, out c2Sum, out c3Sum, out aSum);
        }

        this._c1Min = c1Min; this._c1Max = c1Max;
        this._c2Min = c2Min; this._c2Max = c2Max;
        this._c3Min = c3Min; this._c3Max = c3Max;
        this._c1Sum = c1Sum; this._c2Sum = c2Sum; this._c3Sum = c3Sum;
        this._aSum = aSum;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private static void _ComputeRangesAndSumsSlow(
        List<TWork> colors,
        out float c1Min, out float c1Max,
        out float c2Min, out float c2Max,
        out float c3Min, out float c3Max,
        out double c1Sum, out double c2Sum, out double c3Sum, out double aSum) {
        var (firstC1, firstC2, firstC3, firstA) = colors[0].ToNormalized();
        c1Min = firstC1.ToFloat(); c1Max = c1Min;
        c2Min = firstC2.ToFloat(); c2Max = c2Min;
        c3Min = firstC3.ToFloat(); c3Max = c3Min;
        c1Sum = c1Min; c2Sum = c2Min; c3Sum = c3Min;
        aSum = firstA.ToFloat();

        for (var i = 1; i < colors.Count; ++i) {
          var (c1N, c2N, c3N, aN) = colors[i].ToNormalized();
          var c1 = c1N.ToFloat();
          var c2 = c2N.ToFloat();
          var c3 = c3N.ToFloat();
          var a = aN.ToFloat();

          if (c1 < c1Min) c1Min = c1; else if (c1 > c1Max) c1Max = c1;
          if (c2 < c2Min) c2Min = c2; else if (c2 > c2Max) c2Max = c2;
          if (c3 < c3Min) c3Min = c3; else if (c3 > c3Max) c3Max = c3;

          c1Sum += c1;
          c2Sum += c2;
          c3Sum += c3;
          aSum += a;
        }
      }

      /// <summary>
      /// 32bpp 4-channel byte-domain range/sum sweep. Skips per-element
      /// <see cref="IColorSpace4{T}.ToNormalized"/> and reads channel bytes directly via the
      /// JIT-folded layout descriptor. Float values are computed once per channel and reused for
      /// both range tracking and sum accumulation — bit-exact with the slow path because the
      /// arithmetic chain (uint &gt; float &gt; min/max/sum) is identical.
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private static void _ComputeRangesAndSumsFast32bpp4ch<TLayout>(
        List<TWork> colors,
        out float c1Min, out float c1Max,
        out float c2Min, out float c2Max,
        out float c3Min, out float c3Max,
        out double c1Sum, out double c2Sum, out double c3Sum, out double aSum)
        where TLayout : struct {
        var first = colors[0];
        var firstPacked = Unsafe.As<TWork, uint>(ref first);
        var (firstC1F, firstC2F, firstC3F, firstAF) = StorageLayoutFast.UnpackFloats<TLayout>(firstPacked);
        c1Min = firstC1F; c1Max = firstC1F;
        c2Min = firstC2F; c2Max = firstC2F;
        c3Min = firstC3F; c3Max = firstC3F;
        c1Sum = firstC1F; c2Sum = firstC2F; c3Sum = firstC3F;
        aSum = firstAF;

        for (var i = 1; i < colors.Count; ++i) {
          var color = colors[i];
          var packed = Unsafe.As<TWork, uint>(ref color);
          var (c1, c2, c3, a) = StorageLayoutFast.UnpackFloats<TLayout>(packed);

          if (c1 < c1Min) c1Min = c1; else if (c1 > c1Max) c1Max = c1;
          if (c2 < c2Min) c2Min = c2; else if (c2 > c2Max) c2Max = c2;
          if (c3 < c3Min) c3Min = c3; else if (c3 > c3Max) c3Max = c3;

          c1Sum += c1;
          c2Sum += c2;
          c3Sum += c3;
          aSum += a;
        }
      }

      /// <summary>
      /// Parallel sweep over the colour list, partitioning into chunks and computing
      /// per-chunk min/max/sum, then deterministically reducing in partition order.
      /// Min/max merge associatively across chunks (bit-exact). Double sums merge in
      /// fixed partition order, so the result is reproducible across runs.
      /// </summary>
      private static void _ComputeRangesAndSumsFast32bpp4chParallel<TLayout>(
        List<TWork> colors,
        out float c1Min, out float c1Max,
        out float c2Min, out float c2Max,
        out float c3Min, out float c3Max,
        out double c1Sum, out double c2Sum, out double c3Sum, out double aSum)
        where TLayout : struct {
        var total = colors.Count;
        var partitionCount = Math.Min(Environment.ProcessorCount, Math.Max(2, total / 16384));
        var chunkSize = (total + partitionCount - 1) / partitionCount;

        var partials = new (float c1Min, float c1Max, float c2Min, float c2Max, float c3Min, float c3Max,
          double c1Sum, double c2Sum, double c3Sum, double aSum)[partitionCount];

        Parallel.For(0, partitionCount, p => {
          var start = p * chunkSize;
          var end = Math.Min(start + chunkSize, total);
          if (start >= end) {
            partials[p] = (float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, 0, 0, 0, 0);
            return;
          }

          var first = colors[start];
          var firstPacked = Unsafe.As<TWork, uint>(ref first);
          var (firstC1F, firstC2F, firstC3F, firstAF) = StorageLayoutFast.UnpackFloats<TLayout>(firstPacked);
          var lc1Min = firstC1F; var lc1Max = firstC1F;
          var lc2Min = firstC2F; var lc2Max = firstC2F;
          var lc3Min = firstC3F; var lc3Max = firstC3F;
          double lC1Sum = firstC1F, lC2Sum = firstC2F, lC3Sum = firstC3F, lASum = firstAF;

          for (var i = start + 1; i < end; ++i) {
            var color = colors[i];
            var packed = Unsafe.As<TWork, uint>(ref color);
            var (c1, c2, c3, a) = StorageLayoutFast.UnpackFloats<TLayout>(packed);

            if (c1 < lc1Min) lc1Min = c1; else if (c1 > lc1Max) lc1Max = c1;
            if (c2 < lc2Min) lc2Min = c2; else if (c2 > lc2Max) lc2Max = c2;
            if (c3 < lc3Min) lc3Min = c3; else if (c3 > lc3Max) lc3Max = c3;

            lC1Sum += c1;
            lC2Sum += c2;
            lC3Sum += c3;
            lASum += a;
          }

          partials[p] = (lc1Min, lc1Max, lc2Min, lc2Max, lc3Min, lc3Max, lC1Sum, lC2Sum, lC3Sum, lASum);
        });

        // Deterministic in-order merge.
        var firstValid = -1;
        for (var p = 0; p < partitionCount; ++p) {
          if (partials[p].c1Min == float.MaxValue && partials[p].c1Max == float.MinValue)
            continue;
          firstValid = p;
          break;
        }
        if (firstValid < 0) {
          c1Min = c2Min = c3Min = 0; c1Max = c2Max = c3Max = 0;
          c1Sum = c2Sum = c3Sum = aSum = 0;
          return;
        }
        var seed = partials[firstValid];
        c1Min = seed.c1Min; c1Max = seed.c1Max;
        c2Min = seed.c2Min; c2Max = seed.c2Max;
        c3Min = seed.c3Min; c3Max = seed.c3Max;
        c1Sum = seed.c1Sum; c2Sum = seed.c2Sum; c3Sum = seed.c3Sum; aSum = seed.aSum;
        for (var p = firstValid + 1; p < partitionCount; ++p) {
          var pp = partials[p];
          if (pp.c1Min == float.MaxValue && pp.c1Max == float.MinValue)
            continue;
          if (pp.c1Min < c1Min) c1Min = pp.c1Min;
          if (pp.c1Max > c1Max) c1Max = pp.c1Max;
          if (pp.c2Min < c2Min) c2Min = pp.c2Min;
          if (pp.c2Max > c2Max) c2Max = pp.c2Max;
          if (pp.c3Min < c3Min) c3Min = pp.c3Min;
          if (pp.c3Max > c3Max) c3Max = pp.c3Max;
          c1Sum += pp.c1Sum;
          c2Sum += pp.c2Sum;
          c3Sum += pp.c3Sum;
          aSum += pp.aSum;
        }
      }

      /// <summary>Same threshold as Wu — see WuQuantizer for rationale.</summary>
      private const int _ParallelHistogramThreshold = 65536;

      public float Volume => (this._c1Max - this._c1Min) * (this._c2Max - this._c2Min) * (this._c3Max - this._c3Min);
      public int ColorCount => this._colors.Count;

      public TWork AverageColor => this._colors.Count == 0
        ? default
        : ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)(this._c1Sum / this._colors.Count)),
          UNorm32.FromFloatClamped((float)(this._c2Sum / this._colors.Count)),
          UNorm32.FromFloatClamped((float)(this._c3Sum / this._colors.Count)),
          UNorm32.FromFloatClamped((float)(this._aSum / this._colors.Count))
        );

      public IEnumerable<ColorCube> Split() {
        if (this._colors.Count <= 1)
          return [this];

        var c1Range = this._c1Max - this._c1Min;
        var c2Range = this._c2Max - this._c2Min;
        var c3Range = this._c3Max - this._c3Min;

        // Branchless argmax preserving original tie-break semantics:
        //   c1 wins ties vs c2 and vs c3; c2 wins ties vs c3.
        var axis = c2Range > c1Range ? 1 : 0;
        var currentMax = axis == 0 ? c1Range : c2Range;
        if (c3Range > currentMax)
          axis = 2;

        // Sort colors and counts together in lockstep (zip-sort).
        var pairs = new List<(TWork color, uint count)>(this._colors.Count);
        for (var i = 0; i < this._colors.Count; ++i)
          pairs.Add((this._colors[i], this._counts[i]));

        switch (axis) {
          case 0:
            pairs.Sort((a, b) => a.color.ToNormalized().C1.CompareTo(b.color.ToNormalized().C1));
            break;
          case 1:
            pairs.Sort((a, b) => a.color.ToNormalized().C2.CompareTo(b.color.ToNormalized().C2));
            break;
          default:
            pairs.Sort((a, b) => a.color.ToNormalized().C3.CompareTo(b.color.ToNormalized().C3));
            break;
        }

        // Population-weighted median per Heckbert 1980/82: split where cumulative pixel
        // count crosses half the total population. Each child has ≈ equal pixel coverage,
        // so subsequent splits target where the actual image content is dense.
        ulong totalPop = 0;
        foreach (var (_, c) in pairs) totalPop += c;
        var halfPop = totalPop / 2;
        var splitIndex = 0;
        ulong cumPop = 0;
        for (var i = 0; i < pairs.Count; ++i) {
          cumPop += pairs[i].count;
          if (cumPop >= halfPop) {
            splitIndex = i + 1;
            break;
          }
        }
        if (splitIndex < 1) splitIndex = 1;
        if (splitIndex >= pairs.Count) splitIndex = pairs.Count - 1;

        var leftColors = new List<TWork>(splitIndex);
        var leftCounts = new List<uint>(splitIndex);
        var rightColors = new List<TWork>(pairs.Count - splitIndex);
        var rightCounts = new List<uint>(pairs.Count - splitIndex);
        for (var i = 0; i < splitIndex; ++i) {
          leftColors.Add(pairs[i].color);
          leftCounts.Add(pairs[i].count);
        }
        for (var i = splitIndex; i < pairs.Count; ++i) {
          rightColors.Add(pairs[i].color);
          rightCounts.Add(pairs[i].count);
        }

        return [
          new(leftColors, leftCounts),
          new(rightColors, rightCounts)
        ];
      }
    }
  }
}
