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
/// Wu's color quantizer with configurable parameters.
/// </summary>
/// <remarks>
/// Minimizes the weighted variance of color distribution.
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
        // Find the largest cube that actually has data
        var largestCube = cubes.Where(c => c.HasData).OrderByDescending(c => c.Volume).FirstOrDefault();
        if (largestCube == null || largestCube.Volume <= 0)
          break;

        cubes.Remove(largestCube);
        var splitCubes = largestCube.Split();
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
        var c1 = _FloatToIndex(c1N.ToFloat());
        var c2 = _FloatToIndex(c2N.ToFloat());
        var c3 = _FloatToIndex(c3N.ToFloat());

        ref var entry = ref smallHistogram[c1, c2, c3];
        entry.Count += count;
        entry.C1Sum += c1N.ToFloat() * count;
        entry.C2Sum += c2N.ToFloat() * count;
        entry.C3Sum += c3N.ToFloat() * count;
        entry.ASum += aN.ToFloat() * count;
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
      public uint Count;
      public double C1Sum;
      public double C2Sum;
      public double C3Sum;
      public double ASum;
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

      public bool HasData => this._GetPixelCount() > 0;

      public TWork AverageColor => this._GetAverageColor();

      private ulong _GetPixelCount() {
        ulong count = 0;
        for (var c1 = this._c1Min; c1 <= this._c1Max; ++c1)
        for (var c2 = this._c2Min; c2 <= this._c2Max; ++c2)
        for (var c3 = this._c3Min; c3 <= this._c3Max; ++c3)
          count += this._histogram[c1, c2, c3].Count;
        return count;
      }

      private TWork _GetAverageColor() {
        double c1Sum = 0, c2Sum = 0, c3Sum = 0, aSum = 0;
        ulong count = 0;

        for (var c1 = this._c1Min; c1 <= this._c1Max; ++c1)
        for (var c2 = this._c2Min; c2 <= this._c2Max; ++c2)
        for (var c3 = this._c3Min; c3 <= this._c3Max; ++c3) {
          ref var entry = ref this._histogram[c1, c2, c3];
          c1Sum += entry.C1Sum;
          c2Sum += entry.C2Sum;
          c3Sum += entry.C3Sum;
          aSum += entry.ASum;
          count += entry.Count;
        }

        return count == 0
          ? default
          : ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped((float)(c1Sum / count)),
            UNorm32.FromFloatClamped((float)(c2Sum / count)),
            UNorm32.FromFloatClamped((float)(c3Sum / count)),
            UNorm32.FromFloatClamped((float)(aSum / count))
          );
      }

      public IEnumerable<ColorCube> Split() {
        var c1Range = this._c1Max - this._c1Min;
        var c2Range = this._c2Max - this._c2Min;
        var c3Range = this._c3Max - this._c3Min;

        // Branchless argmax preserving the original tie-break semantics:
        //   original: c1 wins ties vs c2 and vs c3; c2 wins ties vs c3.
        //   axis 0 unless c2 strictly beats c1, then axis 1; then axis 2 only if c3 strictly beats current.
        var axis = c2Range > c1Range ? 1 : 0;
        var currentMax = axis == 0 ? c1Range : c2Range;
        if (c3Range > currentMax)
          axis = 2;

        int mid;
        switch (axis) {
          case 0:
            mid = this._c1Min + (c1Range >> 1);
            return [
              new(this._histogram, this._c1Min, mid, this._c2Min, this._c2Max, this._c3Min, this._c3Max),
              new(this._histogram, mid + 1, this._c1Max, this._c2Min, this._c2Max, this._c3Min, this._c3Max)
            ];
          case 1:
            mid = this._c2Min + (c2Range >> 1);
            return [
              new(this._histogram, this._c1Min, this._c1Max, this._c2Min, mid, this._c3Min, this._c3Max),
              new(this._histogram, this._c1Min, this._c1Max, mid + 1, this._c2Max, this._c3Min, this._c3Max)
            ];
          default:
            mid = this._c3Min + (c3Range >> 1);
            return [
              new(this._histogram, this._c1Min, this._c1Max, this._c2Min, this._c2Max, this._c3Min, mid),
              new(this._histogram, this._c1Min, this._c1Max, this._c2Min, this._c2Max, mid + 1, this._c3Max)
            ];
        }
      }
    }
  }
}
