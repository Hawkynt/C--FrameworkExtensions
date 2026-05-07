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
/// Integer-only morphological operations: <see cref="ErodeInt"/> (per-channel min over
/// a square kernel) and <see cref="DilateInt"/> (per-channel max). Uses Lemire's
/// monotonic-deque sliding-window algorithm — O(1) amortised per pixel regardless
/// of radius, far faster than per-pixel kernel scanning. Pure int byte arithmetic.
/// </summary>
/// <remarks>
/// Reference: Daniel Lemire, "Streaming maximum-minimum filter using no more than
/// three comparisons per element", Nordic Journal of Computing 13 (4), 2006.
/// </remarks>
public static class BitmapMorphologyIntExtensions {

  /// <param name="this">The source bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Erode (per-channel minimum) over a square radius-<paramref name="radius"/>
    /// kernel. Separable: horizontal pass then vertical pass.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ErodeInt(int radius) {
      ArgumentOutOfRangeException.ThrowIfNegative(radius);
      if (radius == 0)
        return (Bitmap)@this.Clone();
      return MorphologyIntPipeline.Run(@this, radius, dilate: false);
    }

    /// <summary>
    /// Dilate (per-channel maximum) over a square radius-<paramref name="radius"/>
    /// kernel. Separable: horizontal pass then vertical pass.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap DilateInt(int radius) {
      ArgumentOutOfRangeException.ThrowIfNegative(radius);
      if (radius == 0)
        return (Bitmap)@this.Clone();
      return MorphologyIntPipeline.Run(@this, radius, dilate: true);
    }
  }
}

internal static class MorphologyIntPipeline {

  public static unsafe Bitmap Run(Bitmap source, int radius, bool dilate) {
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

    var temp = new byte[h * stride];

    // Horizontal pass: each row → independent monotonic deque per channel.
    fixed (byte* src = pixels)
    fixed (byte* dst = temp) {
      _MonotonicHorizontal(src, dst, w, h, stride, radius, dilate);
    }

    // Vertical pass: each column → independent monotonic deque per channel.
    fixed (byte* src = temp)
    fixed (byte* dst = pixels) {
      _MonotonicVertical(src, dst, w, h, stride, radius, dilate);
    }

    var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    var dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
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
  /// Lemire monotonic-deque sliding-window min/max over each row, per channel.
  /// Two deques (one for min/max as needed) maintain candidate indices; head index
  /// is the answer for the current window position.
  /// </summary>
  private static unsafe void _MonotonicHorizontal(byte* src, byte* dst, int w, int h, int stride, int radius, bool dilate) {
    // Deque capacity must accommodate cumulative back-push count without wrap-around
    // (we use linear indices, not a ring buffer). Worst case: every pixel pushed
    // once = n elements. Reuse buffers across rows.
    var dB = new int[w]; var dG = new int[w]; var dR = new int[w]; var dA = new int[w];

    for (var y = 0; y < h; ++y) {
      var rowOff = y * stride;
      _MonotonicScan1D(
        src, dst, rowOff, w, 4, // pixel-stride = 4 bytes (BGRA)
        radius, dilate, dB, dG, dR, dA);
    }
  }

  private static unsafe void _MonotonicVertical(byte* src, byte* dst, int w, int h, int stride, int radius, bool dilate) {
    var dB = new int[h]; var dG = new int[h]; var dR = new int[h]; var dA = new int[h];

    for (var x = 0; x < w; ++x) {
      var colOff = x * 4;
      _MonotonicScan1D(
        src, dst, colOff, h, stride, // axis-stride = source stride (skip a whole row each step)
        radius, dilate, dB, dG, dR, dA);
    }
  }

  /// <summary>
  /// One-dimensional monotonic-deque sliding-window scan. <paramref name="baseOff"/>
  /// + <paramref name="step"/> · i is the byte offset of pixel i along the chosen
  /// axis (4 for horizontal, full row stride for vertical). Writes to the same
  /// position in <paramref name="dst"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _MonotonicScan1D(
    byte* src, byte* dst, int baseOff, int n, int step,
    int radius, bool dilate,
    int[] dB, int[] dG, int[] dR, int[] dA) {
    // Each deque stores indices into the 1D axis. Head at `front`, tail at `back`.
    int frontB = 0, backB = 0; int frontG = 0, backG = 0;
    int frontR = 0, backR = 0; int frontA = 0, backA = 0;

    for (var i = 0; i < n; ++i) {
      var iOff = baseOff + i * step;
      var b = src[iOff + 0];
      var g = src[iOff + 1];
      var r = src[iOff + 2];
      var a = src[iOff + 3];

      // Push i, popping any tail elements that violate the monotonic invariant.
      // dilate=true → maintain decreasing deque (head = max).
      // dilate=false → maintain increasing deque (head = min).
      if (dilate) {
        while (backB > frontB && src[baseOff + dB[backB - 1] * step + 0] <= b) --backB;
        dB[backB++] = i;
        while (backG > frontG && src[baseOff + dG[backG - 1] * step + 1] <= g) --backG;
        dG[backG++] = i;
        while (backR > frontR && src[baseOff + dR[backR - 1] * step + 2] <= r) --backR;
        dR[backR++] = i;
        while (backA > frontA && src[baseOff + dA[backA - 1] * step + 3] <= a) --backA;
        dA[backA++] = i;
      } else {
        while (backB > frontB && src[baseOff + dB[backB - 1] * step + 0] >= b) --backB;
        dB[backB++] = i;
        while (backG > frontG && src[baseOff + dG[backG - 1] * step + 1] >= g) --backG;
        dG[backG++] = i;
        while (backR > frontR && src[baseOff + dR[backR - 1] * step + 2] >= r) --backR;
        dR[backR++] = i;
        while (backA > frontA && src[baseOff + dA[backA - 1] * step + 3] >= a) --backA;
        dA[backA++] = i;
      }

      // Pop heads that have left the window [i - 2*radius, i].
      var windowStart = i - 2 * radius;
      while (frontB < backB && dB[frontB] < windowStart) ++frontB;
      while (frontG < backG && dG[frontG] < windowStart) ++frontG;
      while (frontR < backR && dR[frontR] < windowStart) ++frontR;
      while (frontA < backA && dA[frontA] < windowStart) ++frontA;

      // Write output for the centre of the window: position (i - radius), but we
      // only emit once enough lookahead has been processed. Centred output
      // position is `outI = i - radius`; emit if outI ≥ 0.
      var outI = i - radius;
      if (outI < 0) continue;
      var outOff = baseOff + outI * step;
      dst[outOff + 0] = src[baseOff + dB[frontB] * step + 0];
      dst[outOff + 1] = src[baseOff + dG[frontG] * step + 1];
      dst[outOff + 2] = src[baseOff + dR[frontR] * step + 2];
      dst[outOff + 3] = src[baseOff + dA[frontA] * step + 3];
    }

    // Drain tail: positions in [n - radius, n - 1] still need output. Their
    // window extends past the source end; clamp to last index.
    for (var outI = n - radius; outI < n; ++outI) {
      // Pop heads that have left the right-clamped window [outI - radius, n - 1].
      var windowStart = outI - radius;
      while (frontB < backB && dB[frontB] < windowStart) ++frontB;
      while (frontG < backG && dG[frontG] < windowStart) ++frontG;
      while (frontR < backR && dR[frontR] < windowStart) ++frontR;
      while (frontA < backA && dA[frontA] < windowStart) ++frontA;
      if (outI < 0) continue;
      var outOff = baseOff + outI * step;
      dst[outOff + 0] = src[baseOff + dB[frontB] * step + 0];
      dst[outOff + 1] = src[baseOff + dG[frontG] * step + 1];
      dst[outOff + 2] = src[baseOff + dR[frontR] * step + 2];
      dst[outOff + 3] = src[baseOff + dA[frontA] * step + 3];
    }
  }
}
