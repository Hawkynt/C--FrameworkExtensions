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
/// Integer-only Box blur — bypasses every float conversion that the standard
/// <c>BoxBlur</c> filter performs (sRGB→linear decode, normalised float
/// accumulation, gamma re-encode). For sRGB-to-sRGB box smoothing without
/// gamma awareness, this delivers a fully-integer hot path that is
/// SIMD-friendly and bit-exact across TFMs.
/// </summary>
/// <remarks>
/// <para>Tradeoff: this is gamma-naive (operates on sRGB byte values directly),
/// matching the lib's existing per-pixel <c>BoxBlur</c> Fast quality preset.
/// Use <c>ApplyFilter(BoxBlur, ScalerQuality.HighQuality)</c> for the
/// gamma-correct linear-space variant.</para>
/// </remarks>
public static class BitmapBoxBlurIntExtensions {

  /// <param name="this">The source bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Applies an integer-only box blur with the given radius. Output dimensions
    /// match input. Boundary handling: clamp-to-edge.
    /// </summary>
    /// <param name="radius">Half-width of the kernel (radius=1 → 3×3 box).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap BoxBlurInt(int radius) {
      ArgumentOutOfRangeException.ThrowIfNegative(radius);
      if (radius == 0)
        return (Bitmap)@this.Clone();
      return BoxBlurIntPipeline.Run(@this, radius);
    }
  }
}

/// <summary>
/// Internal int-only box blur pipeline. Two-pass separable (horizontal then
/// vertical) using a sliding window for O(1) per-pixel cost regardless of
/// radius. Pure <c>int</c>/<c>long</c> arithmetic; no float conversion at any
/// stage.
/// </summary>
internal static class BoxBlurIntPipeline {

  public static unsafe Bitmap Run(Bitmap source, int radius) {
    var w = source.Width;
    var h = source.Height;
    if (w == 0 || h == 0)
      return new(Math.Max(1, w), Math.Max(1, h), PixelFormat.Format32bppArgb);

    // Stage 1: copy source into a flat byte buffer for fast row/column access.
    // We could avoid this with two LockBits passes, but the copy is cheap
    // compared to the blur work and lets us keep the inner loop tight.
    var srcRect = new Rectangle(0, 0, w, h);
    var srcData = source.LockBits(srcRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
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

    var temp = new byte[h * stride];

    // Stage 2: horizontal box blur using sliding window.
    // The window covers [x - radius, x + radius] in source columns; OOB clamps to 0 / w-1.
    fixed (byte* src = pixels)
    fixed (byte* dst = temp) {
      _BlurHorizontal(src, dst, w, h, stride, radius);
    }

    // Stage 3: vertical box blur using sliding window.
    fixed (byte* src = temp)
    fixed (byte* dst = pixels) {
      _BlurVertical(src, dst, w, h, stride, radius);
    }

    // Stage 4: write result to a fresh bitmap.
    var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    var dstData = result.LockBits(srcRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try {
      var p = (byte*)dstData.Scan0;
      for (var y = 0; y < h; ++y)
        System.Runtime.InteropServices.Marshal.Copy(pixels, y * stride, (IntPtr)(p + y * dstData.Stride), stride);
    } finally {
      result.UnlockBits(dstData);
    }
    return result;
  }

  /// <summary>
  /// One-pass horizontal box blur with a sliding window. Per-pixel cost is O(1)
  /// regardless of kernel radius; the kernel sum is maintained incrementally
  /// (subtract leaving pixel, add entering pixel each step).
  /// </summary>
  /// <remarks>
  /// Pure scalar implementation. We tried <see cref="Vector128{Int32}"/> for the
  /// running sum (Round 26 SIMD experiment) but it ran ~20% SLOWER than scalar
  /// on the AVX2 reference machine. Two reasons:
  /// (1) Vector128/int lane-wise integer division has no native x86 instruction —
  /// the JIT expands `vec/scalar` into 4 scalar IDIVs per pixel anyway, killing
  /// the SIMD win at the divide-and-store step.
  /// (2) The scalar add/sub of 4 byte channels is already cache-tight and the JIT
  /// auto-vectorises it where profitable; explicit Vector128 packing/unpacking
  /// adds overhead without saving instructions.
  /// SIMD pays off for arithmetic-heavy kernels (multiplies); for cumulative
  /// add/sub of bytes, scalar wins.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _BlurHorizontal(byte* src, byte* dst, int w, int h, int stride, int radius) {
    var kernelSize = 2 * radius + 1;
    for (var y = 0; y < h; ++y) {
      var rowOff = y * stride;
      // Initialise window: sum of pixels in [-radius, radius] with left clamp.
      var sumB = 0; var sumG = 0; var sumR = 0; var sumA = 0;
      for (var k = -radius; k <= radius; ++k) {
        var sx = k < 0 ? 0 : k >= w ? w - 1 : k;
        var off = rowOff + sx * 4;
        sumB += src[off + 0];
        sumG += src[off + 1];
        sumR += src[off + 2];
        sumA += src[off + 3];
      }
      // Emit x=0 and slide window.
      var halfK = kernelSize >> 1;
      for (var x = 0; x < w; ++x) {
        var dstOff = rowOff + x * 4;
        dst[dstOff + 0] = (byte)((sumB + halfK) / kernelSize);
        dst[dstOff + 1] = (byte)((sumG + halfK) / kernelSize);
        dst[dstOff + 2] = (byte)((sumR + halfK) / kernelSize);
        dst[dstOff + 3] = (byte)((sumA + halfK) / kernelSize);
        // Slide: remove pixel at (x - radius), add pixel at (x + radius + 1).
        var leaveX = x - radius;
        var enterX = x + radius + 1;
        var leaveSx = leaveX < 0 ? 0 : leaveX >= w ? w - 1 : leaveX;
        var enterSx = enterX < 0 ? 0 : enterX >= w ? w - 1 : enterX;
        var leaveOff = rowOff + leaveSx * 4;
        var enterOff = rowOff + enterSx * 4;
        sumB += src[enterOff + 0] - src[leaveOff + 0];
        sumG += src[enterOff + 1] - src[leaveOff + 1];
        sumR += src[enterOff + 2] - src[leaveOff + 2];
        sumA += src[enterOff + 3] - src[leaveOff + 3];
      }
    }
  }

  /// <summary>
  /// One-pass vertical box blur with a sliding window. Same structure as the
  /// horizontal pass with stride substituted for column-offset.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _BlurVertical(byte* src, byte* dst, int w, int h, int stride, int radius) {
    var kernelSize = 2 * radius + 1;
    for (var x = 0; x < w; ++x) {
      var colOff = x * 4;
      var sumB = 0; var sumG = 0; var sumR = 0; var sumA = 0;
      for (var k = -radius; k <= radius; ++k) {
        var sy = k < 0 ? 0 : k >= h ? h - 1 : k;
        var off = sy * stride + colOff;
        sumB += src[off + 0];
        sumG += src[off + 1];
        sumR += src[off + 2];
        sumA += src[off + 3];
      }
      var halfK = kernelSize >> 1;
      for (var y = 0; y < h; ++y) {
        var dstOff = y * stride + colOff;
        dst[dstOff + 0] = (byte)((sumB + halfK) / kernelSize);
        dst[dstOff + 1] = (byte)((sumG + halfK) / kernelSize);
        dst[dstOff + 2] = (byte)((sumR + halfK) / kernelSize);
        dst[dstOff + 3] = (byte)((sumA + halfK) / kernelSize);
        var leaveY = y - radius;
        var enterY = y + radius + 1;
        var leaveSy = leaveY < 0 ? 0 : leaveY >= h ? h - 1 : leaveY;
        var enterSy = enterY < 0 ? 0 : enterY >= h ? h - 1 : enterY;
        var leaveOff = leaveSy * stride + colOff;
        var enterOff = enterSy * stride + colOff;
        sumB += src[enterOff + 0] - src[leaveOff + 0];
        sumG += src[enterOff + 1] - src[leaveOff + 1];
        sumR += src[enterOff + 2] - src[leaveOff + 2];
        sumA += src[enterOff + 3] - src[leaveOff + 3];
      }
    }
  }
}
