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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.Drawing.Lockers;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Fast integer-only resampling kernels for the Bgra8888 fast path.
/// </summary>
public enum FastResampleMode {
  /// <summary>Nearest-neighbor sampling. No interpolation; preserves hard edges.</summary>
  NearestNeighbor,

  /// <summary>Bilinear interpolation over a 2x2 source neighborhood. Q16 fractional weights.</summary>
  Bilinear,

  /// <summary>Area-average (box). Integrates fractional source pixel coverage with Q24 weights.</summary>
  Box,
}

/// <summary>
/// Fast int-only Bgra8888 resampling. Bypasses the kernel-template pipeline entirely:
/// no codec round-trip, no float accumulator, no decode cache. Use when speed matters
/// more than colour-space-correct (linear-light, gamma-aware) resampling.
/// </summary>
/// <remarks>
/// Pixels are interpolated directly in sRGB byte space — gamma-naive but extremely
/// cache-friendly. For colour-correct (linear-light) resampling, use the
/// <see cref="BitmapScalerExtensions"/> Resample API which routes through
/// LinearRgbaF + sRGB EOTF/OETF codecs.
/// </remarks>
public static class BitmapResampleFastExtensions {

  /// <param name="this">Source bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Fast int-only resample to target dimensions. Operates directly on Bgra8888
    /// with integer Q16/Q24 fixed-point weights — no float arithmetic, no codec
    /// round-trip.
    /// </summary>
    /// <param name="targetWidth">Target width in pixels.</param>
    /// <param name="targetHeight">Target height in pixels.</param>
    /// <param name="mode">Sampling mode. <see cref="FastResampleMode.Bilinear"/> is the default.</param>
    /// <returns>A new 32bppArgb bitmap at the target size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ResampleFast(int targetWidth, int targetHeight, FastResampleMode mode = FastResampleMode.Bilinear) {
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

      return mode switch {
        FastResampleMode.NearestNeighbor => BitmapResampleFastExtensions._NearestNeighbor(@this, targetWidth, targetHeight),
        FastResampleMode.Bilinear => BitmapResampleFastExtensions._Bilinear(@this, targetWidth, targetHeight),
        FastResampleMode.Box => BitmapResampleFastExtensions._Box(@this, targetWidth, targetHeight),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown fast resample mode.")
      };
    }

    /// <summary>
    /// Convenience overload taking a <see cref="Size"/> for target dimensions.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ResampleFast(Size targetSize, FastResampleMode mode = FastResampleMode.Bilinear)
      => @this.ResampleFast(targetSize.Width, targetSize.Height, mode);
  }

  private const long _Q16 = 1L << 16;
  private const long _Q16_HALF = 1L << 15;

  private static unsafe Bitmap _NearestNeighbor(Bitmap source, int targetWidth, int targetHeight) {
    var srcWidth = source.Width;
    var srcHeight = source.Height;
    var result = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);
    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    // Centered-grid mapping in Q16: srcX_q16 = (destX + 0.5) * srcWidth / destWidth - 0.5
    // We compute integer source index = srcX_q16 >> 16 with rounding via +0.5.
    var stepX_q16 = ((long)srcWidth << 16) / targetWidth;
    var stepY_q16 = ((long)srcHeight << 16) / targetHeight;

    fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels) {
      var srcStride = srcFrame.Stride;
      var dstStride = dstFrame.Stride;
      var srcMaxX = srcWidth - 1;
      var srcMaxY = srcHeight - 1;
      var srcCapture = srcPtr;
      var dstCapture = dstPtr;

      _RunRows(targetHeight, targetWidth, (destY, destX) => {
        var srcY_q16 = stepY_q16 * destY + (stepY_q16 >> 1);
        var srcY = (int)(srcY_q16 >> 16);
        if (srcY < 0) srcY = 0; else if (srcY > srcMaxY) srcY = srcMaxY;
        var srcRow = srcCapture + srcY * srcStride;
        var dstRow = dstCapture + destY * dstStride;

        var srcX_q16 = stepX_q16 * destX + (stepX_q16 >> 1);
        var srcX = (int)(srcX_q16 >> 16);
        if (srcX < 0) srcX = 0; else if (srcX > srcMaxX) srcX = srcMaxX;
        dstRow[destX] = srcRow[srcX];
      });
    }

    return result;
  }

  private static unsafe Bitmap _Bilinear(Bitmap source, int targetWidth, int targetHeight) {
    var srcWidth = source.Width;
    var srcHeight = source.Height;
    var result = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);
    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    // Q16 step: source pixel coordinate per destination pixel.
    // Centered grid: srcX(destX) = (destX + 0.5) * (srcWidth/targetWidth) - 0.5
    // In Q16: srcX_q16 = ((destX << 1 | 1) * scaleX_q16 - _Q16) >> 1
    //                  = destX * scaleX_q16 + (scaleX_q16 - _Q16) / 2
    // We accumulate per-row instead of recomputing per-pixel.
    var scaleX_q16 = ((long)srcWidth << 16) / targetWidth;
    var scaleY_q16 = ((long)srcHeight << 16) / targetHeight;
    var startX_q16 = (scaleX_q16 - _Q16) >> 1;
    var startY_q16 = (scaleY_q16 - _Q16) >> 1;

    fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels) {
      var srcStride = srcFrame.Stride;
      var dstStride = dstFrame.Stride;
      var srcMaxX = srcWidth - 1;
      var srcMaxY = srcHeight - 1;
      var srcCapture = srcPtr;
      var dstCapture = dstPtr;

      _RunRowsBatched(targetHeight, destY => {
        var srcY_q16 = startY_q16 + scaleY_q16 * destY;
        var sy0 = (int)(srcY_q16 >> 16);
        var fracY = (int)(srcY_q16 & 0xFFFF);
        if (srcY_q16 < 0) { sy0 = 0; fracY = 0; }
        var sy1 = sy0 + 1;
        if (sy0 < 0) sy0 = 0;
        if (sy0 > srcMaxY) sy0 = srcMaxY;
        if (sy1 > srcMaxY) sy1 = srcMaxY;
        var invFracY = (int)_Q16 - fracY;

        var row0 = srcCapture + sy0 * srcStride;
        var row1 = srcCapture + sy1 * srcStride;
        var dstRow = dstCapture + destY * dstStride;

        for (var destX = 0; destX < targetWidth; ++destX) {
          var srcX_q16 = startX_q16 + scaleX_q16 * destX;
          var sx0 = (int)(srcX_q16 >> 16);
          var fracX = (int)(srcX_q16 & 0xFFFF);
          if (srcX_q16 < 0) { sx0 = 0; fracX = 0; }
          var sx1 = sx0 + 1;
          if (sx0 < 0) sx0 = 0;
          if (sx0 > srcMaxX) sx0 = srcMaxX;
          if (sx1 > srcMaxX) sx1 = srcMaxX;
          var invFracX = (int)_Q16 - fracX;

          var p00 = row0[sx0];
          var p01 = row0[sx1];
          var p10 = row1[sx0];
          var p11 = row1[sx1];

          // Bilinear weights in Q32 (each w_ij = Q16 * Q16).
          // sum of weights = 2^32, so dividing by 2^32 means right-shift 32.
          // Use long for products to avoid intermediate overflow (max 65535^2 = ~2^32).
          var w00 = (long)invFracX * invFracY;
          var w01 = (long)fracX * invFracY;
          var w10 = (long)invFracX * fracY;
          var w11 = (long)fracX * fracY;

          // Round-to-nearest: add 2^31 before shift-32.
          const long _ROUND = 1L << 31;

          var b = (byte)(((long)p00.B * w00 + (long)p01.B * w01 + (long)p10.B * w10 + (long)p11.B * w11 + _ROUND) >> 32);
          var g = (byte)(((long)p00.G * w00 + (long)p01.G * w01 + (long)p10.G * w10 + (long)p11.G * w11 + _ROUND) >> 32);
          var r = (byte)(((long)p00.R * w00 + (long)p01.R * w01 + (long)p10.R * w10 + (long)p11.R * w11 + _ROUND) >> 32);
          var a = (byte)(((long)p00.A * w00 + (long)p01.A * w01 + (long)p10.A * w10 + (long)p11.A * w11 + _ROUND) >> 32);

          dstRow[destX] = new Bgra8888(r, g, b, a);
        }
      });
    }

    return result;
  }

  private static unsafe Bitmap _Box(Bitmap source, int targetWidth, int targetHeight) {
    var srcWidth = source.Width;
    var srcHeight = source.Height;

    // Box is an area-average resampler. For upscale (target > source), it's effectively
    // nearest-neighbor since each destination pixel sub-samples a source pixel — fall
    // back to nearest-neighbor in that case (faster).
    if (targetWidth >= srcWidth && targetHeight >= srcHeight)
      return _NearestNeighbor(source, targetWidth, targetHeight);

    var result = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);
    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    // Q16 source interval per destination pixel.
    var stepX_q16 = ((long)srcWidth << 16) / targetWidth;
    var stepY_q16 = ((long)srcHeight << 16) / targetHeight;

    // Total weight per destination pixel = stepX_q16 * stepY_q16 (Q32).
    // Guard against pathological-ratio long-overflow: stepX/stepY individually fit in 47 bits
    // (16-bit fractional × max-int dimension) but their product can hit 94 bits. With Bitmap
    // dimensions capped at ~2^31 (int) the realistic upper bound is 2^62, well within long —
    // but a defensive negative-check catches future input-validation regressions cheaply.
    var totalWeight = stepX_q16 * stepY_q16;
    if (totalWeight <= 0)
      throw new ArgumentOutOfRangeException(nameof(source), "Source dimensions × ratio overflow long Q32 weight; reduce source size or target ratio.");
    // Round-to-nearest constant for the final divide.
    var roundDivide = totalWeight >> 1;

    fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels) {
      var srcStride = srcFrame.Stride;
      var dstStride = dstFrame.Stride;
      var srcMaxX = srcWidth - 1;
      var srcMaxY = srcHeight - 1;
      var srcCapture = srcPtr;
      var dstCapture = dstPtr;

      _RunRowsBatched(targetHeight, destY => {
        var y0_q16 = stepY_q16 * destY;
        var y1_q16 = y0_q16 + stepY_q16;
        var dstRow = dstCapture + destY * dstStride;

        for (var destX = 0; destX < targetWidth; ++destX) {
          var x0_q16 = stepX_q16 * destX;
          var x1_q16 = x0_q16 + stepX_q16;

          long sumB = 0, sumG = 0, sumR = 0, sumA = 0;

          var sy0 = (int)(y0_q16 >> 16);
          var sy1 = (int)((y1_q16 - 1) >> 16);
          if (sy1 > srcMaxY) sy1 = srcMaxY;
          var sx0 = (int)(x0_q16 >> 16);
          var sx1 = (int)((x1_q16 - 1) >> 16);
          if (sx1 > srcMaxX) sx1 = srcMaxX;

          for (var sy = sy0; sy <= sy1; ++sy) {
            // Vertical coverage of pixel sy: overlap between [y0_q16, y1_q16] and [sy<<16, (sy+1)<<16].
            var pyTop = (long)sy << 16;
            var pyBot = pyTop + _Q16;
            var top = y0_q16 > pyTop ? y0_q16 : pyTop;
            var bot = y1_q16 < pyBot ? y1_q16 : pyBot;
            var wy = bot - top;
            if (wy <= 0) continue;

            var srcRow = srcCapture + sy * srcStride;
            for (var sx = sx0; sx <= sx1; ++sx) {
              var pxLeft = (long)sx << 16;
              var pxRight = pxLeft + _Q16;
              var left = x0_q16 > pxLeft ? x0_q16 : pxLeft;
              var right = x1_q16 < pxRight ? x1_q16 : pxRight;
              var wx = right - left;
              if (wx <= 0) continue;

              // Per-pixel weight in Q32. Cap source overlap area at Q16*Q16 = 2^32, fits in long.
              var w = wx * wy >> 16; // Reduce Q64 → Q32 to keep accumulation in long range.
              var p = srcRow[sx];
              sumB += p.B * w;
              sumG += p.G * w;
              sumR += p.R * w;
              sumA += p.A * w;
            }
          }

          // totalWeight is Q32; we divided each per-pixel weight by 2^16 above, so accumulator
          // represents (channel * total) at Q16 precision per channel-value. Divide by totalWeight>>16.
          var divisor = totalWeight >> 16;
          var halfDiv = divisor >> 1;
          var b = (byte)((sumB + halfDiv) / divisor);
          var g = (byte)((sumG + halfDiv) / divisor);
          var r = (byte)((sumR + halfDiv) / divisor);
          var a = (byte)((sumA + halfDiv) / divisor);

          dstRow[destX] = new Bgra8888(r, g, b, a);
        }
      });
    }

    return result;
  }

  /// <summary>
  /// Parallel-or-sequential dispatcher that calls <paramref name="rowAction"/> once per
  /// destination row. Falls through to a sequential loop for small images.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _RunRowsBatched(int destHeight, Action<int> rowAction) {
    var minRowsForParallel = Math.Max(100, Environment.ProcessorCount * 5);
    if (destHeight < minRowsForParallel) {
      for (var destY = 0; destY < destHeight; ++destY)
        rowAction(destY);
      return;
    }

    Parallel.For(0, destHeight, rowAction);
  }

  /// <summary>
  /// Per-pixel parallel dispatcher (NearestNeighbor's hot loop has no per-row state, so
  /// per-pixel parallelism is fine — we batch by row inside Parallel.For for cache locality).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _RunRows(int destHeight, int destWidth, Action<int, int> pixelAction) {
    var minRowsForParallel = Math.Max(100, Environment.ProcessorCount * 5);
    if (destHeight < minRowsForParallel) {
      for (var destY = 0; destY < destHeight; ++destY)
      for (var destX = 0; destX < destWidth; ++destX)
        pixelAction(destY, destX);
      return;
    }

    Parallel.For(0, destHeight, destY => {
      for (var destX = 0; destX < destWidth; ++destX)
        pixelAction(destY, destX);
    });
  }
}
