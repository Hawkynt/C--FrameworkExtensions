#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.

#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Integer-only Median filter with constant per-pixel cost regardless of kernel
/// radius, using the Perreault–Hébert 2007 sliding-histogram algorithm
/// ("Median Filtering in Constant Time", IEEE TIP 16(9), 2007).
/// </summary>
/// <remarks>
/// <para>The algorithm maintains, per output column, a 256-bin histogram of the
/// pixel values in the vertical window <c>[y − r, y + r]</c>. As we advance one
/// row at a time, each column histogram has the row leaving the window
/// subtracted and the row entering it added — O(1) per column per row.</para>
/// <para>The output for each row is produced by sliding a window-of-columns
/// histogram horizontally: starting at column 0 (sum of the first 2r+1 column
/// histograms) and incrementally subtracting the left-leaving column and adding
/// the right-entering column at each step. Median lookup uses a two-level
/// (coarse-16 / fine-256) histogram so each lookup costs O(32) bin comparisons
/// instead of O(256).</para>
/// <para>This is gamma-naive (sRGB byte values, no linearisation). For
/// gamma-correct float median, use <c>ApplyFilter(MedianFilter)</c>. Boundary:
/// clamp-to-edge.</para>
/// </remarks>
public static class BitmapMedianFilterIntExtensions {

  /// <param name="this">The source bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Applies a constant-time integer Median filter over a square
    /// (2r+1)×(2r+1) kernel. Per-channel (B, G, R, A) median.
    /// </summary>
    /// <param name="radius">Half-width of the kernel (radius=1 → 3×3 window).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap MedianFilterInt(int radius) {
      ArgumentOutOfRangeException.ThrowIfNegative(radius);
      if (radius == 0)
        return (Bitmap)@this.Clone();
      return MedianFilterIntPipeline.Run(@this, radius);
    }
  }
}

/// <summary>
/// Internal int-only median pipeline. Implements Perreault–Hébert 2007 with
/// per-channel two-level (coarse/fine) histograms.
/// </summary>
internal static class MedianFilterIntPipeline {

  // Histogram layout: per channel, 16 coarse bins + 256 fine bins. Each pixel
  // value v contributes +1 at coarse[v >> 4] and +1 at fine[v]. The coarse bin
  // is the running sum of its 16 fine bins, which lets median search descend
  // 16-coarse → 16-fine for O(32) per channel.

  private const int Channels = 4;        // BGRA
  private const int CoarseBins = 16;
  private const int FineBins = 256;

  public static unsafe Bitmap Run(Bitmap source, int radius) {
    var w = source.Width;
    var h = source.Height;
    if (w == 0 || h == 0)
      return new(Math.Max(1, w), Math.Max(1, h), PixelFormat.Format32bppArgb);

    var rect = new Rectangle(0, 0, w, h);
    var srcData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    var srcStride = srcData.Stride;
    var stride = w * 4;
    var pixels = new byte[h * stride];
    try {
      var p = (byte*)srcData.Scan0;
      for (var y = 0; y < h; ++y)
        System.Runtime.InteropServices.Marshal.Copy((IntPtr)(p + y * srcStride), pixels, y * stride, stride);
    } finally {
      source.UnlockBits(srcData);
    }

    var output = new byte[h * stride];

    fixed (byte* src = pixels)
    fixed (byte* dst = output) {
      _ProcessImage(src, dst, w, h, stride, radius);
    }

    var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    var dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try {
      var p = (byte*)dstData.Scan0;
      for (var y = 0; y < h; ++y)
        System.Runtime.InteropServices.Marshal.Copy(output, y * stride, (IntPtr)(p + y * dstData.Stride), stride);
    } finally {
      result.UnlockBits(dstData);
    }
    return result;
  }

  /// <summary>
  /// Core 2D sliding-histogram median. For each output row y, advances every
  /// per-column histogram by ±1 row (O(w) work per row), then walks the row
  /// left-to-right maintaining a window-of-columns histogram (O(w) work per
  /// row, O(32) per pixel for median lookup). Total: O(w·h) regardless of r.
  /// </summary>
  private static unsafe void _ProcessImage(byte* src, byte* dst, int w, int h, int stride, int radius) {
    // Per-column histograms: one (coarse + fine) pair per channel per column.
    // Layout per column: [coarse 16 × Channels][fine 256 × Channels]. Stored
    // contiguously in flat int[] arrays so the inner loops index by simple
    // multiplication (no jagged-array bounds checks per access).
    var colCoarseStride = CoarseBins * Channels;       // ints per column in colCoarse
    var colFineStride = FineBins * Channels;         // ints per column in colFine
    var colCoarse = new int[w * colCoarseStride];
    var colFine = new int[w * colFineStride];

    // Initialise per-column histograms with rows [-radius .. radius] (top-clamped).
    // Equivalent to "the window before output row 0".
    for (var ky = -radius; ky <= radius; ++ky) {
      var sy = ky < 0 ? 0 : ky >= h ? h - 1 : ky;
      var rowOff = sy * stride;
      for (var x = 0; x < w; ++x) {
        var pxOff = rowOff + x * 4;
        _AddColumnPixel(colCoarse, colFine, x, colCoarseStride, colFineStride, src + pxOff);
      }
    }

    // Window-of-columns histogram. We rebuild this from scratch at the start
    // of each output row (cheap: 2r+1 column histograms × 4 channels × 256
    // ints summed, bypassed by coarse-bin maintenance below). To avoid even
    // that cost, we rebuild incrementally per row by initialising the window
    // for column 0 only once per row, then sliding.
    var winCoarse = new int[CoarseBins * Channels];
    var winFine = new int[FineBins * Channels];
    var halfWin = (2 * radius + 1) * (2 * radius + 1) / 2;

    for (var y = 0; y < h; ++y) {
      var rowOff = y * stride;

      // Build the window histogram for x=0 by summing columns in [-radius..radius]
      // (left-clamped). Reset first.
      Array.Clear(winCoarse, 0, winCoarse.Length);
      Array.Clear(winFine, 0, winFine.Length);
      for (var kx = -radius; kx <= radius; ++kx) {
        var sx = kx < 0 ? 0 : kx >= w ? w - 1 : kx;
        _AddColumnHistogramToWindow(colCoarse, colFine, sx, colCoarseStride, colFineStride, winCoarse, winFine);
      }

      // Emit row.
      for (var x = 0; x < w; ++x) {
        var dstOff = rowOff + x * 4;
        // Per-channel median lookup (coarse → fine, O(32) compares).
        dst[dstOff + 0] = _FindMedian(winCoarse, winFine, channel: 0, halfWin);
        dst[dstOff + 1] = _FindMedian(winCoarse, winFine, channel: 1, halfWin);
        dst[dstOff + 2] = _FindMedian(winCoarse, winFine, channel: 2, halfWin);
        dst[dstOff + 3] = _FindMedian(winCoarse, winFine, channel: 3, halfWin);

        // Slide window right: remove column at x-radius, add column at x+radius+1.
        // Skip on the last iteration since we won't emit again on this row.
        if (x == w - 1) break;
        var leaveX = x - radius;
        var enterX = x + radius + 1;
        var leaveSx = leaveX < 0 ? 0 : leaveX >= w ? w - 1 : leaveX;
        var enterSx = enterX < 0 ? 0 : enterX >= w ? w - 1 : enterX;
        _SwapColumnHistogramsInWindow(colCoarse, colFine, leaveSx, enterSx, colCoarseStride, colFineStride, winCoarse, winFine);
      }

      // Advance per-column histograms by one row: remove (y - radius)-th row,
      // add (y + radius + 1)-th row. Only needed if there is a next row.
      if (y == h - 1) break;
      var leaveY = y - radius;
      var enterY = y + radius + 1;
      var leaveSy = leaveY < 0 ? 0 : leaveY >= h ? h - 1 : leaveY;
      var enterSy = enterY < 0 ? 0 : enterY >= h ? h - 1 : enterY;
      var leaveRow = leaveSy * stride;
      var enterRow = enterSy * stride;
      for (var x = 0; x < w; ++x) {
        _RemoveColumnPixel(colCoarse, colFine, x, colCoarseStride, colFineStride, src + leaveRow + x * 4);
        _AddColumnPixel(colCoarse, colFine, x, colCoarseStride, colFineStride, src + enterRow + x * 4);
      }
    }
  }

  /// <summary>
  /// Increments per-column histograms (all 4 channels) for one pixel.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _AddColumnPixel(
    int[] colCoarse, int[] colFine, int x, int coarseStride, int fineStride, byte* px) {
    var c0 = x * coarseStride;
    var f0 = x * fineStride;
    var b = px[0]; var g = px[1]; var r = px[2]; var a = px[3];
    // Coarse bin index = v >> 4. Fine bin index = v. Per-channel storage
    // interleaves channels within a bin: [bin0_B, bin0_G, bin0_R, bin0_A, bin1_B, ...]
    ++colCoarse[c0 + ((b >> 4) << 2) + 0];
    ++colCoarse[c0 + ((g >> 4) << 2) + 1];
    ++colCoarse[c0 + ((r >> 4) << 2) + 2];
    ++colCoarse[c0 + ((a >> 4) << 2) + 3];
    ++colFine[f0 + (b << 2) + 0];
    ++colFine[f0 + (g << 2) + 1];
    ++colFine[f0 + (r << 2) + 2];
    ++colFine[f0 + (a << 2) + 3];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _RemoveColumnPixel(
    int[] colCoarse, int[] colFine, int x, int coarseStride, int fineStride, byte* px) {
    var c0 = x * coarseStride;
    var f0 = x * fineStride;
    var b = px[0]; var g = px[1]; var r = px[2]; var a = px[3];
    --colCoarse[c0 + ((b >> 4) << 2) + 0];
    --colCoarse[c0 + ((g >> 4) << 2) + 1];
    --colCoarse[c0 + ((r >> 4) << 2) + 2];
    --colCoarse[c0 + ((a >> 4) << 2) + 3];
    --colFine[f0 + (b << 2) + 0];
    --colFine[f0 + (g << 2) + 1];
    --colFine[f0 + (r << 2) + 2];
    --colFine[f0 + (a << 2) + 3];
  }

  /// <summary>
  /// Adds an entire column histogram (all 4 channels, coarse + fine) to the
  /// window aggregate. Cost: 16·4 + 256·4 = 1088 int adds per column.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _AddColumnHistogramToWindow(
    int[] colCoarse, int[] colFine, int x, int coarseStride, int fineStride,
    int[] winCoarse, int[] winFine) {
    var c0 = x * coarseStride;
    var f0 = x * fineStride;
    for (var i = 0; i < CoarseBins * Channels; ++i) winCoarse[i] += colCoarse[c0 + i];
    for (var i = 0; i < FineBins * Channels; ++i) winFine[i] += colFine[f0 + i];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _SubColumnHistogramFromWindow(
    int[] colCoarse, int[] colFine, int x, int coarseStride, int fineStride,
    int[] winCoarse, int[] winFine) {
    var c0 = x * coarseStride;
    var f0 = x * fineStride;
    for (var i = 0; i < CoarseBins * Channels; ++i) winCoarse[i] -= colCoarse[c0 + i];
    for (var i = 0; i < FineBins * Channels; ++i) winFine[i] -= colFine[f0 + i];
  }

  /// <summary>
  /// Combined slide step: removes the leaving column and adds the entering
  /// column in a single fused pass. Same arithmetic as Sub+Add but issues one
  /// loop instead of two, halving the number of windowed-array writes.
  /// When leave == enter (window pinned at an image edge), this is a no-op.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _SwapColumnHistogramsInWindow(
    int[] colCoarse, int[] colFine, int leaveX, int enterX, int coarseStride, int fineStride,
    int[] winCoarse, int[] winFine) {
    if (leaveX == enterX) return;
    var lc = leaveX * coarseStride;
    var lf = leaveX * fineStride;
    var ec = enterX * coarseStride;
    var ef = enterX * fineStride;
    for (var i = 0; i < CoarseBins * Channels; ++i)
      winCoarse[i] += colCoarse[ec + i] - colCoarse[lc + i];
    for (var i = 0; i < FineBins * Channels; ++i)
      winFine[i] += colFine[ef + i] - colFine[lf + i];
  }

  /// <summary>
  /// Two-level median search: walk 16 coarse bins until cumulative count
  /// crosses <paramref name="halfWin"/>; then walk that bin's 16 fine sub-bins.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _FindMedian(int[] winCoarse, int[] winFine, int channel, int halfWin) {
    var acc = 0;
    var coarseIdx = 0;
    for (; coarseIdx < CoarseBins; ++coarseIdx) {
      acc += winCoarse[(coarseIdx << 2) + channel];
      if (acc > halfWin) {
        acc -= winCoarse[(coarseIdx << 2) + channel];
        break;
      }
    }
    // Descend into fine bins of `coarseIdx`. Fine indices are [coarseIdx*16 .. coarseIdx*16+15].
    var fineBase = coarseIdx << 4;
    for (var i = 0; i < CoarseBins; ++i) {
      acc += winFine[((fineBase + i) << 2) + channel];
      if (acc > halfWin)
        return (byte)(fineBase + i);
    }
    // Saturate (only reached if histogram is empty, which shouldn't happen).
    return 255;
  }
}
