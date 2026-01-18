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
  /// Builds a histogram by deduplicating in Bgra8888 space, then decoding to TWork.
  /// Uses parallel processing with thread-local histograms for performance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe (TWork color, uint count)[] _BuildHistogram<TWork, TDecode>(
    Bitmap source,
    TDecode decoder)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TDecode : struct, IDecode<Bgra8888, TWork> {

    var bgra8888Histogram = new Dictionary<uint, uint>();
    var histogramLock = new object();

    using (var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly)) {
      var srcFrame = srcLocker.AsFrame();
      var width = srcFrame.Width;
      var height = srcFrame.Height;
      var stride = srcFrame.Stride;

      fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels) {
        var srcPtrValue = (IntPtr)srcPtr;

        // Build thread-local histograms in parallel, then merge
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
            // Merge thread-local histogram into global histogram
            lock (histogramLock)
              foreach (var kvp in localHistogram) {
                bgra8888Histogram.TryGetValue(kvp.Key, out var existingCount);
                bgra8888Histogram[kvp.Key] = existingCount + kvp.Value;
              }
          }
        );
      }
    }

    // Convert each unique Bgra8888 to TWork (single decode per unique color)
    var histogram = new (TWork, uint)[bgra8888Histogram.Count];
    var i = 0;
    foreach (var kvp in bgra8888Histogram) {
      var bgra = new Bgra8888(kvp.Key);
      histogram[i] = (decoder.Decode(bgra), kvp.Value);
      ++i;
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
