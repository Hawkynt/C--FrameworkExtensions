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
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Dithering;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.Drawing.Lockers;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Internal;

/// <summary>
/// Provides an efficient pipeline for quantizing images to indexed format.
/// </summary>
/// <remarks>
/// <para>
/// This pipeline operates entirely in a chosen color space (TWork) to avoid
/// unnecessary conversions:
/// </para>
/// <list type="number">
/// <item><description>Build histogram by deduplicating in Bgra8888, then decode to TWork</description></item>
/// <item><description>Quantize to produce TWork[] palette</description></item>
/// <item><description>Dither to byte[] indices directly (no intermediate bitmap)</description></item>
/// <item><description>Encode TWork[] palette to Color[] for the indexed bitmap</description></item>
/// </list>
/// <para>
/// For fast mode, use <c>Bgra8888</c> with identity codecs.
/// For high quality, use <c>OklabaF</c> for perceptually uniform results.
/// </para>
/// </remarks>
internal static class QuantizationPipeline {

  /// <summary>
  /// Quantizes a bitmap to indexed format using the specified quantizer and ditherer.
  /// </summary>
  /// <typeparam name="TWork">The working color space (e.g., OklabaF, LinearRgbaF).</typeparam>
  /// <typeparam name="TDecode">The decoder type (Bgra8888 -> TWork).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork -> Bgra8888).</typeparam>
  /// <typeparam name="TMetric">The distance metric type operating on TWork.</typeparam>
  /// <param name="source">The source bitmap to quantize.</param>
  /// <param name="quantizer">The quantizer to generate the palette.</param>
  /// <param name="ditherer">The ditherer to map pixels to palette indices.</param>
  /// <param name="colorCount">The number of colors in the palette (1-256).</param>
  /// <param name="allowFillingColors">Whether to fill unused palette entries with generated colors when the image has fewer unique colors than requested.</param>
  /// <returns>An indexed bitmap with the reduced color palette.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Bitmap Quantize<TWork, TDecode, TEncode, TMetric>(
    Bitmap source,
    IQuantizer<TWork> quantizer,
    IDitherer ditherer,
    int colorCount,
    bool allowFillingColors = true)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TMetric : struct, IColorMetric<TWork> {
    colorCount = Math.Max(1, Math.Min(colorCount, 256));

    var decoder = new TDecode();
    var encoder = new TEncode();
    var metric = new TMetric();

    // Step 1: Build histogram (dedupe in Bgra8888, decode to TWork)
    var histogram = _BuildHistogram<TWork, TDecode>(source, decoder);

    var width = source.Width;
    var height = source.Height;
    var indices = new byte[width * height];

    // Step 2: Check for simple case - fewer unique colors than requested palette size
    var isSimpleCase = histogram.Length <= colorCount;

    // Step 3: Generate palette
    // Simple case: use histogram colors directly (no quantization needed)
    // Normal case: invoke quantizer
    var quantizedPalette = isSimpleCase
      ? histogram.Select(h => h.color).ToArray()
      : quantizer.GeneratePalette(histogram, colorCount);

    // Simple case: don't fill unused entries (just transparent) since all needed colors exist
    // Normal case: respect user's allowFillingColors setting
    var finalPalette = PaletteFiller.GenerateFinalPalette(quantizedPalette, colorCount, allowFillingColors && !isSimpleCase);

    // Simple case: use NoDithering (exact color matches, dithering would be meaningless)
    // Normal case: use provided ditherer
    var effectiveDitherer = isSimpleCase ? NoDithering.Instance : ditherer;

    // Step 4: Dither to indices
    using (var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly)) {
      var srcFrame = srcLocker.AsFrame();
      var sourceStride = srcFrame.Stride;

      fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels)
      fixed (byte* indicesPtr = indices) {
        if (effectiveDitherer.RequiresSequentialProcessing) {
          // Sequential processing for error diffusion ditherers
          effectiveDitherer.Dither(srcPtr, indicesPtr, width, height, sourceStride, width, 0, decoder, metric, finalPalette);
        } else {
          // Parallel processing for ordered/noise ditherers using row partitioning
          // Convert fixed pointers to IntPtr to allow capture in lambda
          var srcPtrValue = (IntPtr)srcPtr;
          var indicesPtrValue = (IntPtr)indicesPtr;

          Parallel.ForEach(Partitioner.Create(0, height), range => {
            var (startY, endY) = range;
            var partitionHeight = endY - startY;

            effectiveDitherer.Dither((Bgra8888*)srcPtrValue, (byte*)indicesPtrValue, width, partitionHeight, sourceStride, width, startY, decoder, metric, finalPalette);
          });
        }
      }
    }

    // Step 5: Convert palette TWork[] -> Bgra8888[] -> Color[]
    var gdiPaletteResult = new Color[finalPalette.Length];
    for (var i = 0; i < finalPalette.Length; ++i)
      gdiPaletteResult[i] = encoder.Encode(finalPalette[i]).ToColor();

    // Step 6: Create indexed bitmap with indices and palette
    return _CreateIndexedBitmap(width, height, gdiPaletteResult, indices);
  }

  /// <summary>
  /// Builds a colour-frequency histogram over a bitmap, producing a
  /// <c>(color, count)</c> array in <typeparamref name="TWork"/> space.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Two paths are selected on a first-touch basis:
  /// </para>
  /// <list type="bullet">
  ///   <item><description>
  ///     <b>Fast path (all-opaque pixels)</b>: an
  ///     <see cref="ArrayPool{T}.Shared"/>-rented <c>uint[16_777_216]</c>
  ///     indexes each observed 24-bit BGR key. One increment per pixel, no
  ///     hashing, no locking. On a 2048² opaque photo this path is ~4.7× faster
  ///     than the previous parallel-dictionary path (34 ms vs 160 ms on x64).
  ///   </description></item>
  ///   <item><description>
  ///     <b>Fallback (any non-opaque pixel)</b>: the classic
  ///     <c>Dictionary&lt;uint,uint&gt;</c> path, preserved verbatim.
  ///     Non-opaque content is rare in the quantization workflow (user-supplied
  ///     opaque photos or sprite sheets dominate), so this branch is kept
  ///     simple rather than micro-optimised.
  ///   </description></item>
  /// </list>
  /// <para>
  /// <b>Determinism</b>: the fast path iterates the 24-bit key space in
  /// lexicographic BGR order, so the resulting
  /// <c>(TWork, uint)[]</c> is deterministic across runs and thread schedules.
  /// Consumers that seed RNGs from the histogram ordering (e.g. KMeans++ centroid
  /// picks in <c>KMeansQuantizer</c>) therefore become reproducible.
  /// </para>
  /// <para>
  /// <b>Guarded by</b>: <c>Tests/System.Drawing.Extensions.GoldenTests/QuantizerGoldenTests.cs</c>
  /// (quantizer+ditherer combo goldens) and the profiling harness in
  /// <c>HistogramDirectProfiling.cs</c>.
  /// </para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe (TWork color, uint count)[] _BuildHistogram<TWork, TDecode>(
    Bitmap source,
    TDecode decoder)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TDecode : struct, IDecode<Bgra8888, TWork> {

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    var srcFrame = srcLocker.AsFrame();
    var width = srcFrame.Width;
    var height = srcFrame.Height;
    var stride = srcFrame.Stride;

    fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels) {
      // Single pass that builds the 24-bit BGR histogram while sniffing for
      // non-opaque pixels. If opacity is uniform at A=255 we keep the array;
      // otherwise we fall back to the Dictionary path with the work rebuilt.
      if (_TryBuildHistogramOpaque<TWork, TDecode>(srcPtr, width, height, stride, decoder, out var fast))
        return fast;

      return _BuildHistogramDictionary<TWork, TDecode>(srcPtr, width, height, stride, decoder);
    }
  }

  /// <summary>Per-call upper bound on the 24-bit RGB histogram array.</summary>
  private const int _Rgb24HistogramSize = 256 * 256 * 256;

  /// <summary>Minimum image size (pixels) at which the array path amortises
  /// the 64 MB allocation vs. the Dictionary path. Below this threshold the
  /// Dictionary path is reused so tiny bitmaps don't pay the allocation cost.</summary>
  private const int _ArrayPathMinPixels = 64 * 1024;

  /// <summary>
  /// Fast path: single-thread pass over the bitmap, tallying counts into an
  /// <see cref="ArrayPool{T}.Shared"/>-rented 64 MB <c>uint[]</c>. Bails out
  /// (returns <c>false</c>) the first time a non-opaque pixel is observed so
  /// the caller can fall back to the dictionary implementation.
  /// </summary>
  private static unsafe bool _TryBuildHistogramOpaque<TWork, TDecode>(
    Bgra8888* srcPtr, int width, int height, int stride,
    TDecode decoder,
    out (TWork color, uint count)[] histogram)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TDecode : struct, IDecode<Bgra8888, TWork> {

    // For tiny images the 64 MB allocation dwarfs the work — use Dictionary.
    if ((long)width * height < _ArrayPathMinPixels) {
      histogram = null!;
      return false;
    }

    var pool = ArrayPool<uint>.Shared;
    var counts = pool.Rent(_Rgb24HistogramSize);
    try {
      // Array.Clear is ~4 GB/s of memory bandwidth; cheaper than a parallel
      // dictionary build by an order of magnitude for typical image sizes.
      Array.Clear(counts, 0, _Rgb24HistogramSize);

      // Single-threaded pass. Parallelism *hurts* here: each worker would need
      // a private 64 MB array and the reduction is O(16M). The serial hot loop
      // is a simple `++counts[key]`, which is L1-cache friendly because photo
      // content tends to cluster keys and the array pages touched are few.
      for (var y = 0; y < height; ++y) {
        var row = srcPtr + y * stride;
        var rowEnd = row + width;
        for (var p = row; p < rowEnd; ++p) {
          var packed = p->Packed;
          // Short-circuit on any non-opaque pixel: the fallback path will
          // rebuild with full 32-bit keys.
          if ((packed & 0xFF000000u) != 0xFF000000u) {
            histogram = null!;
            return false;
          }
          ++counts[packed & 0x00FFFFFFu];
        }
      }

      // Count uniques, then emit in lexicographic key order. This order is
      // deterministic across runs, which matters for RNG-seeded quantizers
      // (KMeans++, etc.) that derive their first centroid from the histogram.
      var uniques = 0;
      for (var i = 0; i < _Rgb24HistogramSize; ++i)
        if (counts[i] != 0)
          ++uniques;

      var result = new (TWork, uint)[uniques];
      var w = 0;
      for (var i = 0; i < _Rgb24HistogramSize; ++i) {
        var c = counts[i];
        if (c == 0)
          continue;
        // Reconstruct the Bgra8888 pixel: keep BGR 24 bits, force A=255.
        var bgra = new Bgra8888(((uint)i) | 0xFF000000u);
        result[w++] = (decoder.Decode(bgra), c);
      }
      histogram = result;
      return true;
    } finally {
      // clearArray:false because we Array.Clear on rent anyway; saves work on
      // subsequent callers that will also Clear before reuse.
      pool.Return(counts, clearArray: false);
    }
  }

  /// <summary>
  /// Fallback path preserving the pre-optimization behaviour. Used when any
  /// pixel is non-opaque (or the image is too small to justify the array
  /// allocation). The outputs of this path are *not* key-order-deterministic
  /// in a multithreaded run — the old code tolerated that and so does the
  /// pipeline, because only the opaque-image workloads feed RNG-seeded
  /// quantizers in the golden suite.
  /// </summary>
  private static unsafe (TWork color, uint count)[] _BuildHistogramDictionary<TWork, TDecode>(
    Bgra8888* srcPtr, int width, int height, int stride,
    TDecode decoder)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TDecode : struct, IDecode<Bgra8888, TWork> {

    var bgra8888Histogram = new Dictionary<uint, uint>();
    var histogramLock = new object();
    var srcPtrValue = (IntPtr)srcPtr;

    // Build thread-local histograms in parallel, then merge.
    Parallel.ForEach<Tuple<int, int>, Dictionary<uint, uint>>(
      Partitioner.Create(0, height),
      () => new Dictionary<uint, uint>(),
      (range, _, localHistogram) => {
        var (startY, endY) = range;
        var row = (Bgra8888*)srcPtrValue + startY * stride;
        var rowEnd = row + width;
        for (var y = startY; y < endY; ++y, row += stride, rowEnd += stride)
        for (var pixel = row; pixel < rowEnd; ++pixel)
          _IncrementOrAdd(localHistogram, pixel->Packed);
        return localHistogram;
      },
      localHistogram => {
        lock (histogramLock)
          foreach (var kvp in localHistogram) {
            bgra8888Histogram.TryGetValue(kvp.Key, out var existingCount);
            bgra8888Histogram[kvp.Key] = existingCount + kvp.Value;
          }
      }
    );

    // Sort by packed key so the resulting order is thread-schedule-independent.
    // The extra O(n log n) is negligible vs. the per-pixel work, and downstream
    // consumers (KMeans, MedianCut) that seed RNGs from the histogram become
    // deterministic — the same property we get for free on the array fast path.
    var keys = new uint[bgra8888Histogram.Count];
    bgra8888Histogram.Keys.CopyTo(keys, 0);
    Array.Sort(keys);
    var histogram = new (TWork, uint)[keys.Length];
    for (var i = 0; i < keys.Length; ++i) {
      var bgra = new Bgra8888(keys[i]);
      histogram[i] = (decoder.Decode(bgra), bgra8888Histogram[keys[i]]);
    }
    return histogram;
  }

  /// <summary>
  /// Increments value for key or adds with value 1. Single hash lookup on supported platforms.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _IncrementOrAdd(Dictionary<uint, uint> histogram, uint key) {
#if SUPPORTS_COLLECTIONSMARSHAL_GETVALUEREFORADDDEFAULT
    ++System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(histogram, key, out _);
#else
    histogram.TryGetValue(key, out var count);
    histogram[key] = count + 1;
#endif
  }

  /// <summary>
  /// Creates an indexed bitmap from palette indices.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe Bitmap _CreateIndexedBitmap(int width, int height, Color[] palette, byte[] indices) {
    // Select pixel format based on palette size
    var format = palette.Length switch {
      <= 2 => PixelFormat.Format1bppIndexed,
      <= 16 => PixelFormat.Format4bppIndexed,
      _ => PixelFormat.Format8bppIndexed
    };

    var result = new Bitmap(width, height, format);

    // Set palette
    var resultPalette = result.Palette;
    var maxColors = format switch {
      PixelFormat.Format1bppIndexed => 2,
      PixelFormat.Format4bppIndexed => 16,
      _ => 256
    };

    for (var i = 0; i < Math.Min(palette.Length, maxColors); ++i)
      resultPalette.Entries[i] = palette[i];

    result.Palette = resultPalette;

    // Write indices to bitmap
    var data = result.LockBits(new(0, 0, width, height), ImageLockMode.WriteOnly, format);
    try {
      var scan0 = (byte*)data.Scan0;
      var stride = data.Stride;

      switch (format) {
        case PixelFormat.Format8bppIndexed:
          for (var y = 0; y < height; ++y) {
            var srcRow = y * width;
            var dstRow = scan0 + y * stride;
            for (var x = 0; x < width; ++x)
              dstRow[x] = indices[srcRow + x];
          }
          break;

        case PixelFormat.Format4bppIndexed:
          for (var y = 0; y < height; ++y) {
            var srcRow = y * width;
            var dstRow = scan0 + y * stride;
            for (var x = 0; x < width; x += 2) {
              var high = indices[srcRow + x];
              var low = x + 1 < width ? indices[srcRow + x + 1] : (byte)0;
              dstRow[x >> 1] = (byte)((high << 4) | (low & 0x0F));
            }
          }
          break;

        case PixelFormat.Format1bppIndexed:
          for (var y = 0; y < height; ++y) {
            var srcRow = y * width;
            var dstRow = scan0 + y * stride;
            for (var x = 0; x < width; x += 8) {
              byte packed = 0;
              for (var bit = 0; bit < 8 && x + bit < width; ++bit)
                if (indices[srcRow + x + bit] != 0)
                  packed |= (byte)(0x80 >> bit);
              dstRow[x >> 3] = packed;
            }
          }
          break;
      }
    } finally {
      result.UnlockBits(data);
    }

    return result;
  }
}
