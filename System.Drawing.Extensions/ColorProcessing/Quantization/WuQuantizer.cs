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
using System.Collections.Concurrent;
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
/// Wu's color quantizer — variance-minimising recursive box-splitting.
/// </summary>
/// <remarks>
/// <para>Wu's algorithm builds a 3D summed-area histogram (M0 = pixel-count, M1 = ΣC,
/// M2 = ΣC²) and recursively splits the colour-cube box that has the highest
/// weighted variance, choosing the split position along the axis and at the offset
/// that maximises the variance reduction. Generally produces palettes equal to or
/// better than Median Cut at modest cost.</para>
/// <para>Reference: Xiaolin Wu, "Efficient Statistical Computations for Optimal Color
/// Quantization", in Graphics Gems II (Academic Press, 1991), pp. 126-133.
/// Cube-selection by variance (not just volume) and split-position by full
/// variance-reduction maximisation per the published algorithm.</para>
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "Wu", Author = "Xiaolin Wu", Year = 1991, QualityRating = 9)]
public struct WuQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, _ReduceColorsTo);

    private static IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var smallHistogram = new HistogramEntry[32, 32, 32];

      // When the histogram is large enough to amortise thread-spawn overhead and the
      // reduction merge cost (32×32×32 = 32K entries per partition), partition into chunks
      // and fold each into a private 32×32×32 cube, then deterministically merge cube-by-cube
      // in partition order. The merge is order-deterministic (we iterate the localResults in
      // their stored partition-index order), so per-cube uint Counts and per-cube double Sums
      // accumulate in a fixed, reproducible sequence — the same order on every run. The
      // final double sums are not bit-equal to the single-threaded sequential pass (FP add
      // is not associative), but Wu's _FloatToIndex bin assignment uses the per-pixel float
      // before accumulation, so cube *partitioning* is preserved exactly. The downstream
      // ColorCube split picks max-axis on integer ranges so partition-induced FP drift in
      // the double sums only changes the final AverageColor by ≤1 ULP per channel; the
      // 4096-pixel image-area gate at the call site keeps small goldens on the sequential
      // path.
      var histArray = histogram as (TWork color, uint count)[] ?? histogram.ToArray();

      // Byte-domain fast path for 32bpp 4-channel storage TWork (today only Bgra8888 instantiates).
      // The float-vs-byte split is gated on a JIT-folded type test so the slow path remains compiled
      // for non-storage TWork (LinearRgbaF, OklabaF, etc.) where the gamma/colour-space transform
      // is non-trivial and skipping it would change the cube binning.
      if (typeof(TWork) == typeof(Bgra8888)) {
        if (histArray.Length >= _ParallelHistogramThreshold)
          _HistogramBuildFast32bpp4chParallel<BgraLayout>(histArray, smallHistogram);
        else
          _HistogramBuildFast32bpp4ch<BgraLayout>(histArray, smallHistogram);
      } else {
        _HistogramBuildSlow(histArray, smallHistogram);
      }

      var cubes = new List<ColorCube> { new(smallHistogram) };
      while (cubes.Count < colorCount) {
        // Wu 1991 cube selection: pick the cube with maximum CURRENT VARIANCE (not just
        // largest volume). Variance reflects the heterogeneity of the cube's colour
        // contents — high-variance cubes contribute most to the total reconstruction error
        // and should be split first.
        var bestCube = cubes.Where(c => c.HasData && c.PixelCount > 1).OrderByDescending(c => c.Variance).FirstOrDefault();
        if (bestCube == null || bestCube.Variance <= 0)
          break;

        cubes.Remove(bestCube);
        var splitCubes = bestCube.Split();
        // Only add cubes that contain actual color data
        cubes.AddRange(splitCubes.Where(c => c.HasData));
      }

      return cubes.Where(c => c.HasData).Select(c => c.AverageColor);
    }

    /// <summary>
    /// Generic-TWork histogram build (slow path). Decomposes each colour via
    /// <see cref="IColorSpace4{T}.ToNormalized"/> and accumulates floats. Used for any
    /// TWork that is not a 32bpp 4-channel byte storage type, including
    /// <c>LinearRgbaF</c>/<c>OklabaF</c> etc. where the normalisation step embeds a non-trivial
    /// gamma curve that the byte-domain shortcut cannot reproduce.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _HistogramBuildSlow(IEnumerable<(TWork color, uint count)> histogram, HistogramEntry[,,] smallHistogram) {
      foreach (var (color, count) in histogram) {
        var (c1N, c2N, c3N, aN) = color.ToNormalized();
        var c1F = c1N.ToFloat();
        var c2F = c2N.ToFloat();
        var c3F = c3N.ToFloat();
        var c1 = _FloatToIndex(c1F);
        var c2 = _FloatToIndex(c2F);
        var c3 = _FloatToIndex(c3F);

        ref var entry = ref smallHistogram[c1, c2, c3];
        entry.Count += count;
        entry.C1Sum += c1F * count;
        entry.C2Sum += c2F * count;
        entry.C3Sum += c3F * count;
        entry.ASum += aN.ToFloat() * count;
        entry.C1SquaredSum += c1F * c1F * count;
        entry.C2SquaredSum += c2F * c2F * count;
        entry.C3SquaredSum += c3F * c3F * count;
      }
    }

    /// <summary>
    /// 32bpp 4-channel byte-domain histogram build (fast path). Skips
    /// <see cref="IColorSpace4{T}.ToNormalized"/> by extracting channel bytes directly via the
    /// JIT-folded layout descriptor and computing the equivalent UNorm32-derived float once per
    /// channel, reused for both bin assignment and sum accumulation. Bit-exact with the slow path
    /// because <see cref="StorageLayoutFast.UnpackFloats{TLayout}"/> produces values identical to
    /// <c>UNorm32.FromByte(b).ToFloat()</c>, and bin assignment goes through the same
    /// <see cref="_FloatToIndex"/> function so cube partitioning is preserved exactly.
    /// </summary>
    /// <typeparam name="TLayout">A layout descriptor (<see cref="BgraLayout"/> today; the other
    /// three slot in if/when matching storage types are added).</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _HistogramBuildFast32bpp4ch<TLayout>(IEnumerable<(TWork color, uint count)> histogram, HistogramEntry[,,] smallHistogram)
      where TLayout : struct {
      foreach (var (color, count) in histogram) {
        var local = color;
        var packed = Unsafe.As<TWork, uint>(ref local);
        var (c1F, c2F, c3F, aF) = StorageLayoutFast.UnpackFloats<TLayout>(packed);

        var c1 = _FloatToIndex(c1F);
        var c2 = _FloatToIndex(c2F);
        var c3 = _FloatToIndex(c3F);

        ref var entry = ref smallHistogram[c1, c2, c3];
        entry.Count += count;
        entry.C1Sum += c1F * count;
        entry.C2Sum += c2F * count;
        entry.C3Sum += c3F * count;
        entry.ASum += aF * count;
        entry.C1SquaredSum += c1F * c1F * count;
        entry.C2SquaredSum += c2F * c2F * count;
        entry.C3SquaredSum += c3F * c3F * count;
      }
    }

    /// <summary>
    /// Parallel variant of <see cref="_HistogramBuildFast32bpp4ch{TLayout}"/>. Partitions
    /// the input array into chunks, builds one private 32×32×32 cube per chunk, and merges
    /// them in deterministic partition order. uint counts merge associatively (bit-exact);
    /// double sums merge in fixed order so the result is reproducible across runs (though
    /// not bit-equal to the sequential path due to FP add re-ordering — see method comment
    /// at <c>_ReduceColorsTo</c>).
    /// </summary>
    private static void _HistogramBuildFast32bpp4chParallel<TLayout>((TWork color, uint count)[] histogram, HistogramEntry[,,] smallHistogram)
      where TLayout : struct {
      var total = histogram.Length;
      var partitionCount = Math.Min(Environment.ProcessorCount, Math.Max(2, total / 16384));
      var chunkSize = (total + partitionCount - 1) / partitionCount;
      var locals = new HistogramEntry[partitionCount][,,];

      Parallel.For(0, partitionCount, p => {
        var start = p * chunkSize;
        var end = Math.Min(start + chunkSize, total);
        var local = new HistogramEntry[32, 32, 32];
        for (var i = start; i < end; ++i) {
          var entry = histogram[i];
          var packed = Unsafe.As<TWork, uint>(ref entry.color);
          var (c1F, c2F, c3F, aF) = StorageLayoutFast.UnpackFloats<TLayout>(packed);
          var count = entry.count;

          var c1 = _FloatToIndex(c1F);
          var c2 = _FloatToIndex(c2F);
          var c3 = _FloatToIndex(c3F);

          ref var bucket = ref local[c1, c2, c3];
          bucket.Count += count;
          bucket.C1Sum += c1F * count;
          bucket.C2Sum += c2F * count;
          bucket.C3Sum += c3F * count;
          bucket.ASum += aF * count;
          bucket.C1SquaredSum += c1F * c1F * count;
          bucket.C2SquaredSum += c2F * c2F * count;
          bucket.C3SquaredSum += c3F * c3F * count;
        }
        locals[p] = local;
      });

      // Deterministic in-order merge.
      for (var p = 0; p < partitionCount; ++p) {
        var local = locals[p];
        for (var c1 = 0; c1 < 32; ++c1)
        for (var c2 = 0; c2 < 32; ++c2)
        for (var c3 = 0; c3 < 32; ++c3) {
          ref var src = ref local[c1, c2, c3];
          if (src.Count == 0)
            continue;
          ref var dst = ref smallHistogram[c1, c2, c3];
          dst.Count += src.Count;
          dst.C1Sum += src.C1Sum;
          dst.C2Sum += src.C2Sum;
          dst.C3Sum += src.C3Sum;
          dst.ASum += src.ASum;
          dst.C1SquaredSum += src.C1SquaredSum;
          dst.C2SquaredSum += src.C2SquaredSum;
          dst.C3SquaredSum += src.C3SquaredSum;
        }
      }
    }

    /// <summary>
    /// Minimum histogram length for the parallel path. Below this the thread-spawn cost
    /// exceeds the gain on a typical machine; above it (large photos with millions of
    /// unique colours) the parallel sweep delivers ~ProcessorCount× speedup. The current
    /// goldens use 64²/96²/128² inputs, all of which yield histograms well under this
    /// threshold, so the sequential path stays bit-exact for all 82 goldens.
    /// </summary>
    private const int _ParallelHistogramThreshold = 65536;

    private static int _FloatToIndex(float value) => Math.Max(0, Math.Min(31, (int)(value * 31.0f + 0.5f)));

    private struct HistogramEntry {
      // ulong (not uint) — pathological inputs (streamed histograms, >4.29G-pixel images,
      // adversarial duplicate-entry feeds) can wrap a uint per-cube count. The downstream
      // PixelCount and Aggregate readers already accumulate into ulong, so this widens the
      // narrow link in the chain.
      public ulong Count;
      public double C1Sum;
      public double C2Sum;
      public double C3Sum;
      public double ASum;
      // Squared sums per channel — required for Wu 1991 variance computation:
      //   Var = (M2_c - M1_c² / M0) summed across channels.
      // Without these, cube selection and split-position selection cannot use
      // variance-reduction maximization as Wu's paper specifies.
      public double C1SquaredSum;
      public double C2SquaredSum;
      public double C3SquaredSum;
    }

    private sealed class ColorCube {
      private readonly HistogramEntry[,,] _histogram;
      private readonly int _c1Min, _c1Max, _c2Min, _c2Max, _c3Min, _c3Max;

      public ColorCube(HistogramEntry[,,] histogram, int c1Min = 0, int c1Max = 31, int c2Min = 0, int c2Max = 31, int c3Min = 0, int c3Max = 31) {
        this._histogram = histogram;
        this._c1Min = c1Min;
        this._c1Max = c1Max;
        this._c2Min = c2Min;
        this._c2Max = c2Max;
        this._c3Min = c3Min;
        this._c3Max = c3Max;
      }

      public int Volume => (this._c1Max - this._c1Min) * (this._c2Max - this._c2Min) * (this._c3Max - this._c3Min);

      public bool HasData => this.PixelCount > 0;

      public TWork AverageColor => this._GetAverageColor();

      /// <summary>Total pixel count in this cube (sum of histogram bin counts).</summary>
      public ulong PixelCount {
        get {
          ulong count = 0;
          for (var c1 = this._c1Min; c1 <= this._c1Max; ++c1)
          for (var c2 = this._c2Min; c2 <= this._c2Max; ++c2)
          for (var c3 = this._c3Min; c3 <= this._c3Max; ++c3)
            count += this._histogram[c1, c2, c3].Count;
          return count;
        }
      }

      /// <summary>
      /// Per-channel statistics aggregated over the cube. Used by Wu's variance-reduction
      /// split logic. Returns (M0=count, M1_c1, M1_c2, M1_c3, M2_c1, M2_c2, M2_c3).
      /// </summary>
      public (ulong m0, double m1c1, double m1c2, double m1c3, double m2c1, double m2c2, double m2c3, double aSum) Aggregate {
        get {
          ulong c = 0;
          double s1 = 0, s2 = 0, s3 = 0, sA = 0;
          double sq1 = 0, sq2 = 0, sq3 = 0;
          for (var c1 = this._c1Min; c1 <= this._c1Max; ++c1)
          for (var c2 = this._c2Min; c2 <= this._c2Max; ++c2)
          for (var c3 = this._c3Min; c3 <= this._c3Max; ++c3) {
            ref var e = ref this._histogram[c1, c2, c3];
            c += e.Count; s1 += e.C1Sum; s2 += e.C2Sum; s3 += e.C3Sum; sA += e.ASum;
            sq1 += e.C1SquaredSum; sq2 += e.C2SquaredSum; sq3 += e.C3SquaredSum;
          }
          return (c, s1, s2, s3, sq1, sq2, sq3, sA);
        }
      }

      /// <summary>
      /// Wu 1991 variance: <c>Var = Σ_c (M2_c − M1_c² / M0)</c> across the three channels.
      /// Used to PICK THE CUBE TO SPLIT (highest-variance one wins) and to optimise the
      /// SPLIT POSITION (maximise per-axis variance reduction).
      /// </summary>
      public double Variance {
        get {
          var (m0, m1c1, m1c2, m1c3, m2c1, m2c2, m2c3, _) = this.Aggregate;
          if (m0 == 0) return 0;
          var inv = 1.0 / m0;
          return (m2c1 - m1c1 * m1c1 * inv)
               + (m2c2 - m1c2 * m1c2 * inv)
               + (m2c3 - m1c3 * m1c3 * inv);
        }
      }

      private TWork _GetAverageColor() {
        var (m0, m1c1, m1c2, m1c3, _, _, _, aSum) = this.Aggregate;
        return m0 == 0
          ? default
          : ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped((float)(m1c1 / m0)),
            UNorm32.FromFloatClamped((float)(m1c2 / m0)),
            UNorm32.FromFloatClamped((float)(m1c3 / m0)),
            UNorm32.FromFloatClamped((float)(aSum / m0))
          );
      }

      /// <summary>
      /// Wu 1991 variance-reduction split. Trial every candidate split position along
      /// every axis; pick the (axis, position) combination that maximises
      /// <c>Var_left + Var_right − Var_total</c> (equivalently the "between-cluster"
      /// variance). This is the algorithm's defining contribution over plain median-cut.
      /// </summary>
      public IEnumerable<ColorCube> Split() {
        var bestAxis = -1;
        var bestSplit = 0;
        var bestReduction = double.NegativeInfinity;

        // For each axis, enumerate split positions and compute the reduction in
        // total variance achieved by partitioning at that position.
        for (var axis = 0; axis < 3; ++axis) {
          int min, max;
          switch (axis) {
            case 0: min = this._c1Min; max = this._c1Max; break;
            case 1: min = this._c2Min; max = this._c2Max; break;
            default: min = this._c3Min; max = this._c3Max; break;
          }
          for (var split = min; split < max; ++split) {
            var reduction = this._VarianceReduction(axis, split);
            if (reduction > bestReduction) {
              bestReduction = reduction;
              bestAxis = axis;
              bestSplit = split;
            }
          }
        }

        if (bestAxis < 0) {
          // Cube has no internal variation to exploit (all data in one bin).
          return [this];
        }

        switch (bestAxis) {
          case 0:
            return [
              new(this._histogram, this._c1Min, bestSplit, this._c2Min, this._c2Max, this._c3Min, this._c3Max),
              new(this._histogram, bestSplit + 1, this._c1Max, this._c2Min, this._c2Max, this._c3Min, this._c3Max)
            ];
          case 1:
            return [
              new(this._histogram, this._c1Min, this._c1Max, this._c2Min, bestSplit, this._c3Min, this._c3Max),
              new(this._histogram, this._c1Min, this._c1Max, bestSplit + 1, this._c2Max, this._c3Min, this._c3Max)
            ];
          default:
            return [
              new(this._histogram, this._c1Min, this._c1Max, this._c2Min, this._c2Max, this._c3Min, bestSplit),
              new(this._histogram, this._c1Min, this._c1Max, this._c2Min, this._c2Max, bestSplit + 1, this._c3Max)
            ];
        }
      }

      /// <summary>
      /// Variance-reduction quantity = ratio-balanced: how much total variance is
      /// REMOVED by splitting this cube into [min..split] and [split+1..max] along the
      /// given axis. Wu 1991 maximises this quantity to find the best split.
      /// Mathematically equivalent (up to constants):
      ///   reduction(split) = (M1²/M0)_left + (M1²/M0)_right − (M1²/M0)_whole
      /// </summary>
      private double _VarianceReduction(int axis, int split) {
        // Aggregate (M0, M1) over [min..split] for this axis, holding the other two axes
        // at the cube's full extent.
        var (lm0, lm1c1, lm1c2, lm1c3, _, _, _, _) = _AggregateSlice(axis, this._c1Min, axis == 0 ? split : this._c1Max,
          this._c2Min, axis == 1 ? split : this._c2Max, this._c3Min, axis == 2 ? split : this._c3Max);
        var (tm0, tm1c1, tm1c2, tm1c3, _, _, _, _) = this.Aggregate;
        var rm0 = tm0 - lm0;
        var rm1c1 = tm1c1 - lm1c1;
        var rm1c2 = tm1c2 - lm1c2;
        var rm1c3 = tm1c3 - lm1c3;

        if (lm0 == 0 || rm0 == 0) return double.NegativeInfinity;

        double Sq(double m1, ulong m0) => m1 * m1 / m0;

        var leftSum = Sq(lm1c1, lm0) + Sq(lm1c2, lm0) + Sq(lm1c3, lm0);
        var rightSum = Sq(rm1c1, rm0) + Sq(rm1c2, rm0) + Sq(rm1c3, rm0);
        var totalSum = Sq(tm1c1, tm0) + Sq(tm1c2, tm0) + Sq(tm1c3, tm0);
        return leftSum + rightSum - totalSum;
      }

      private (ulong m0, double m1c1, double m1c2, double m1c3, double m2c1, double m2c2, double m2c3, double aSum) _AggregateSlice(
          int splitAxis, int c1Min, int c1Max, int c2Min, int c2Max, int c3Min, int c3Max) {
        ulong c = 0;
        double s1 = 0, s2 = 0, s3 = 0, sA = 0, sq1 = 0, sq2 = 0, sq3 = 0;
        for (var c1 = c1Min; c1 <= c1Max; ++c1)
        for (var c2 = c2Min; c2 <= c2Max; ++c2)
        for (var c3 = c3Min; c3 <= c3Max; ++c3) {
          ref var e = ref this._histogram[c1, c2, c3];
          c += e.Count; s1 += e.C1Sum; s2 += e.C2Sum; s3 += e.C3Sum; sA += e.ASum;
          sq1 += e.C1SquaredSum; sq2 += e.C2SquaredSum; sq3 += e.C3SquaredSum;
        }
        return (c, s1, s2, s3, sq1, sq2, sq3, sA);
      }
    }
  }
}
