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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Dithering;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Quantization;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using Hawkynt.Drawing.Lockers;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Provides extension methods for color quantization and dithering of Bitmaps.
/// </summary>
public static class BitmapQuantizationExtensions {

  /// <param name="this">Source bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Reduces the colors in a bitmap using quantization and dithering.
    /// </summary>
    /// <typeparam name="TQuantizer">The quantizer type (e.g., OctreeQuantizer, MedianCutQuantizer).</typeparam>
    /// <typeparam name="TDitherer">The ditherer type (must implement IDitherer).</typeparam>
    /// <param name="quantizer">The quantizer instance for palette generation.</param>
    /// <param name="ditherer">The ditherer instance for error diffusion.</param>
    /// <param name="colorCount">The target number of colors (1-256).</param>
    /// <param name="isHighQuality">
    /// When <see langword="true"/>, uses OkLab perceptual color space for matching (slower, better gradients).
    /// When <see langword="false"/>, uses linear RGB with Euclidean distance (faster, physically accurate).
    /// </param>
    /// <returns>
    /// A new indexed bitmap with pixel format based on color count:
    /// 2 colors → 1bpp, ≤16 colors → 4bpp, ≤256 colors → 8bpp.
    /// </returns>
    /// <example>
    /// <code>
    /// using var original = new Bitmap("photo.png");
    /// using var indexed = original.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 16, isHighQuality: true);
    /// indexed.Save("indexed.gif");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe Bitmap ReduceColors<TQuantizer, TDitherer>(
      TQuantizer quantizer,
      TDitherer ditherer,
      byte colorCount,
      bool isHighQuality = false)
      where TQuantizer : class, IQuantizer
      where TDitherer : struct, IDitherer {

      // Build histogram from source image
      var histogram = new Dictionary<uint, uint>();

      using (var srcLocker = new Argb8888BitmapLocker(@this, ImageLockMode.ReadOnly)) {
        var srcFrame = srcLocker.AsFrame();

        fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels) {
          var width = srcFrame.Width;
          var height = srcFrame.Height;
          var stride = srcFrame.Stride;

          for (var y = 0; y < height; ++y)
          for (var x = 0; x < width; ++x) {
            var pixel = srcPtr[y * stride + x];

            // Skip fully transparent pixels
            if (pixel.A == 0)
              continue;

            var packed = pixel.Packed;
            if (histogram.TryGetValue(packed, out var count))
              histogram[packed] = count + 1;
            else
              histogram[packed] = 1;
          }
        }
      }

      // Generate palette from histogram
      var histogramList = new List<(Bgra8888 color, uint count)>(histogram.Count);
      foreach (var kvp in histogram)
        histogramList.Add((new Bgra8888(kvp.Key), kvp.Value));

      var palette = quantizer.GeneratePalette(histogramList, Math.Clamp((int)colorCount, 1, 256));

      return isHighQuality
        ? _DitherToIndexedHighQuality(@this, ditherer, palette)
        : _DitherToIndexedFast(@this, ditherer, palette);
    }
  }

  /// <summary>
  /// Fast dithering using linear RGB color space with Euclidean distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static Bitmap _DitherToIndexedFast<TDitherer>(
    Bitmap source,
    TDitherer ditherer,
    Bgra8888[] palette)
    where TDitherer : struct, IDitherer {
    var dithered = _DitherTo<
      TDitherer, LinearRgbaF,
      Srgb32ToLinearRgbaF, LinearRgbaFToSrgb32,
      EuclideanSquared4F<LinearRgbaF>>(source, ditherer, palette);

    try {
      return _ConvertToIndexed(dithered, palette);
    } finally {
      dithered.Dispose();
    }
  }

  /// <summary>
  /// High quality dithering using OkLab perceptual color space.
  /// </summary>
  /// <remarks>
  /// Works directly in OkLab space for perceptually uniform error diffusion.
  /// Uses simple Euclidean distance since OkLab is already perceptually uniform.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static Bitmap _DitherToIndexedHighQuality<TDitherer>(
    Bitmap source,
    TDitherer ditherer,
    Bgra8888[] palette)
    where TDitherer : struct, IDitherer {
    var dithered = _DitherTo<
      TDitherer, OklabaF,
      Srgb32ToOklabaF, OklabaFToSrgb32,
      EuclideanSquared4F<OklabaF>>(source, ditherer, palette);

    try {
      return _ConvertToIndexed(dithered, palette);
    } finally {
      dithered.Dispose();
    }
  }

  /// <summary>
  /// Applies dithering with configurable color spaces.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe Bitmap _DitherTo<TDitherer, TWork, TDecode, TEncode, TMetric>(
    Bitmap source,
    TDitherer ditherer,
    Bgra8888[] palette)
    where TDitherer : struct, IDitherer
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TMetric : struct, IColorMetric<TWork> {
    var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);

    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    // Pre-convert palette to TWork space
    TDecode decoder = default;
    var paletteWork = new TWork[palette.Length];

    for (var i = 0; i < palette.Length; ++i)
      paletteWork[i] = decoder.Decode(palette[i]);

    fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels)
      ditherer.InvokeKernel<TWork, Bgra8888, TDecode, TEncode, TMetric, Bgra8888>(
        new DitherCallback<TWork, TDecode, TEncode, TMetric>(
          srcPtr, srcFrame.Stride, dstPtr, dstFrame.Stride, paletteWork),
        srcFrame.Width, srcFrame.Height);

    return result;
  }

  /// <summary>
  /// Converts a 32bpp dithered bitmap to indexed format.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe Bitmap _ConvertToIndexed(Bitmap dithered, Bgra8888[] palette) {
    var width = dithered.Width;
    var height = dithered.Height;

    // Build color to index lookup (dithered pixels are exact palette matches)
    var colorToIndex = new Dictionary<uint, byte>(palette.Length);
    for (var i = 0; i < palette.Length; ++i)
      colorToIndex.TryAdd(palette[i].Packed, (byte)i);

    // Allocate index buffer
    var indices = new byte[width * height];

    using (var srcLocker = new Argb8888BitmapLocker(dithered, ImageLockMode.ReadOnly)) {
      var srcFrame = srcLocker.AsFrame();

      fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels) {
        var stride = srcFrame.Stride;
        for (var y = 0; y < height; ++y)
        for (var x = 0; x < width; ++x) {
          var pixel = srcPtr[y * stride + x];

          // After dithering, all pixels must be exact palette colors
          indices[y * width + x] = colorToIndex.TryGetValue(pixel.Packed, out var idx) ? idx : (byte)0;
        }
      }
    }

    return _CreateIndexedBitmap(width, height, palette, indices);
  }

  /// <summary>
  /// Creates an indexed bitmap from palette indices.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe Bitmap _CreateIndexedBitmap(int width, int height, Bgra8888[] palette, byte[] indices) {
    // Select pixel format based on palette size
    var format = palette.Length switch {
      <= 2 => PixelFormat.Format1bppIndexed,
      <= 16 => PixelFormat.Format4bppIndexed,
      _ => PixelFormat.Format8bppIndexed
    };

    var result = new Bitmap(width, height, format);

    // Set palette (convert Bgra8888 to Color for GDI)
    var resultPalette = result.Palette;
    var maxColors = format switch {
      PixelFormat.Format1bppIndexed => 2,
      PixelFormat.Format4bppIndexed => 16,
      _ => 256
    };

    for (var i = 0; i < Math.Min(palette.Length, maxColors); ++i)
      resultPalette.Entries[i] = palette[i].ToColor();
    result.Palette = resultPalette;

    // Write indices to bitmap
    var data = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, format);
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

  /// <summary>
  /// Callback for dithering with configurable color spaces.
  /// </summary>
  private readonly unsafe struct DitherCallback<TWork, TDecode, TEncode, TMetric>(
    Bgra8888* source,
    int sourceStride,
    Bgra8888* dest,
    int destStride,
    TWork[] palette)
    : IDithererCallback<TWork, Bgra8888, TDecode, TEncode, TMetric, Bgra8888>
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TMetric : struct, IColorMetric<TWork> {

    public Bgra8888 Invoke<TKernel>(TKernel kernel)
      where TKernel : struct, IDithererKernel<TWork, Bgra8888, TDecode, TEncode, TMetric> {
      TDecode decoder = default;
      TEncode encoder = default;
      TMetric metric = default;

      if (kernel.RequiresSequentialProcessing)
        kernel.ProcessErrorDiffusion(
          source, sourceStride, dest, destStride,
          decoder, encoder, metric,
          palette);
      else {
        var width = kernel.Width;
        var height = kernel.Height;
        for (var y = 0; y < height; ++y)
        for (var x = 0; x < width; ++x)
          kernel.ProcessOrdered(
            source, sourceStride, x, y, dest, destStride,
            decoder, encoder, metric,
            palette);
      }

      return default;
    }
  }

}
