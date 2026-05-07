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
/// Integer-only directional motion blur. For axis-aligned angles
/// (0°, 90°, 180°, 270°) uses the sliding-window int box-blur primitive
/// (O(1) per pixel regardless of length). For arbitrary angles, a direct
/// per-pixel directional walk in pure int arithmetic.
/// </summary>
public static class BitmapMotionBlurIntExtensions {

  /// <param name="this">The source bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Applies an integer-only directional motion blur. The blur kernel is a
    /// 1D line of length <paramref name="length"/> rotated to the given angle.
    /// </summary>
    /// <param name="length">Number of pixels along the blur direction (≥ 1).</param>
    /// <param name="angleDegrees">Direction in degrees. 0° = horizontal, 90° = vertical.
    /// Negative or > 360° values are normalised modulo 360°.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap MotionBlurInt(int length, double angleDegrees) {
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
      if (length == 1)
        return (Bitmap)@this.Clone();

      // Normalise the angle.
      var angle = angleDegrees % 360.0;
      if (angle < 0) angle += 360.0;

      // Axis-aligned shortcut: 0° or 180° is a horizontal 1D box, 90°/270° is vertical.
      // Use the sliding-window box pipeline directly for O(1)-per-pixel cost.
      if (_IsAxisAligned(angle)) {
        var radius = (length - 1) / 2;
        return _AxisAlignedMotionBlur(@this, radius, angle);
      }

      // Arbitrary angle: direct per-pixel directional walk.
      var radians = angle * Math.PI / 180.0;
      var dx = Math.Cos(radians);
      var dy = Math.Sin(radians);
      return MotionBlurIntPipeline.Run(@this, length, (float)dx, (float)dy);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool _IsAxisAligned(double angle) {
    const double Eps = 1e-3;
    return Math.Abs(angle) < Eps
        || Math.Abs(angle - 90.0) < Eps
        || Math.Abs(angle - 180.0) < Eps
        || Math.Abs(angle - 270.0) < Eps
        || Math.Abs(angle - 360.0) < Eps;
  }

  private static unsafe Bitmap _AxisAlignedMotionBlur(Bitmap source, int radius, double angle) {
    // 0°/180° = horizontal 1D box; 90°/270° = vertical 1D box.
    var horizontal = Math.Abs(angle) < 1e-3 || Math.Abs(angle - 180.0) < 1e-3 || Math.Abs(angle - 360.0) < 1e-3;
    return MotionBlurIntPipeline.RunAxisAligned(source, radius, horizontal);
  }
}

/// <summary>
/// Internal motion-blur pipeline. Two paths: sliding-window 1D box for axis-aligned,
/// direct per-pixel walk for arbitrary angles.
/// </summary>
internal static class MotionBlurIntPipeline {

  public static unsafe Bitmap RunAxisAligned(Bitmap source, int radius, bool horizontal) {
    var w = source.Width;
    var h = source.Height;
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

    var output = new byte[h * stride];
    fixed (byte* src = pixels)
    fixed (byte* dst = output) {
      if (horizontal)
        _BlurHorizontal(src, dst, w, h, stride, radius);
      else
        _BlurVertical(src, dst, w, h, stride, radius);
    }

    var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    var dstData = result.LockBits(srcRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try {
      var p = (byte*)dstData.Scan0;
      for (var y = 0; y < h; ++y)
        System.Runtime.InteropServices.Marshal.Copy(output, y * stride, (IntPtr)(p + y * dstData.Stride), stride);
    } finally {
      result.UnlockBits(dstData);
    }
    return result;
  }

  public static unsafe Bitmap Run(Bitmap source, int length, float dx, float dy) {
    var w = source.Width;
    var h = source.Height;
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

    var output = new byte[h * stride];
    var halfLen = length / 2;
    fixed (byte* src = pixels)
    fixed (byte* dst = output) {
      // Walk a centered line of `length` taps for each output pixel; clamp samples to image bounds.
      for (var y = 0; y < h; ++y) {
        for (var x = 0; x < w; ++x) {
          int sumB = 0, sumG = 0, sumR = 0, sumA = 0;
          for (var k = -halfLen; k < length - halfLen; ++k) {
            var sx = (int)(x + k * dx + 0.5f);
            var sy = (int)(y + k * dy + 0.5f);
            if (sx < 0) sx = 0; else if (sx >= w) sx = w - 1;
            if (sy < 0) sy = 0; else if (sy >= h) sy = h - 1;
            var off = sy * stride + sx * 4;
            sumB += src[off + 0];
            sumG += src[off + 1];
            sumR += src[off + 2];
            sumA += src[off + 3];
          }
          var dOff = y * stride + x * 4;
          var half = length / 2;
          dst[dOff + 0] = (byte)((sumB + half) / length);
          dst[dOff + 1] = (byte)((sumG + half) / length);
          dst[dOff + 2] = (byte)((sumR + half) / length);
          dst[dOff + 3] = (byte)((sumA + half) / length);
        }
      }
    }

    var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    var dstData = result.LockBits(srcRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try {
      var p = (byte*)dstData.Scan0;
      for (var y = 0; y < h; ++y)
        System.Runtime.InteropServices.Marshal.Copy(output, y * stride, (IntPtr)(p + y * dstData.Stride), stride);
    } finally {
      result.UnlockBits(dstData);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _BlurHorizontal(byte* src, byte* dst, int w, int h, int stride, int radius) {
    var kernelSize = 2 * radius + 1;
    var halfK = kernelSize >> 1;
    for (var y = 0; y < h; ++y) {
      var rowOff = y * stride;
      var sumB = 0; var sumG = 0; var sumR = 0; var sumA = 0;
      for (var k = -radius; k <= radius; ++k) {
        var sx = k < 0 ? 0 : k >= w ? w - 1 : k;
        var off = rowOff + sx * 4;
        sumB += src[off + 0];
        sumG += src[off + 1];
        sumR += src[off + 2];
        sumA += src[off + 3];
      }
      for (var x = 0; x < w; ++x) {
        var dOff = rowOff + x * 4;
        dst[dOff + 0] = (byte)((sumB + halfK) / kernelSize);
        dst[dOff + 1] = (byte)((sumG + halfK) / kernelSize);
        dst[dOff + 2] = (byte)((sumR + halfK) / kernelSize);
        dst[dOff + 3] = (byte)((sumA + halfK) / kernelSize);
        var leaveX = x - radius;
        var enterX = x + radius + 1;
        var lSx = leaveX < 0 ? 0 : leaveX >= w ? w - 1 : leaveX;
        var eSx = enterX < 0 ? 0 : enterX >= w ? w - 1 : enterX;
        var lOff = rowOff + lSx * 4;
        var eOff = rowOff + eSx * 4;
        sumB += src[eOff + 0] - src[lOff + 0];
        sumG += src[eOff + 1] - src[lOff + 1];
        sumR += src[eOff + 2] - src[lOff + 2];
        sumA += src[eOff + 3] - src[lOff + 3];
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _BlurVertical(byte* src, byte* dst, int w, int h, int stride, int radius) {
    var kernelSize = 2 * radius + 1;
    var halfK = kernelSize >> 1;
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
      for (var y = 0; y < h; ++y) {
        var dOff = y * stride + colOff;
        dst[dOff + 0] = (byte)((sumB + halfK) / kernelSize);
        dst[dOff + 1] = (byte)((sumG + halfK) / kernelSize);
        dst[dOff + 2] = (byte)((sumR + halfK) / kernelSize);
        dst[dOff + 3] = (byte)((sumA + halfK) / kernelSize);
        var leaveY = y - radius;
        var enterY = y + radius + 1;
        var lSy = leaveY < 0 ? 0 : leaveY >= h ? h - 1 : leaveY;
        var eSy = enterY < 0 ? 0 : enterY >= h ? h - 1 : enterY;
        var lOff = lSy * stride + colOff;
        var eOff = eSy * stride + colOff;
        sumB += src[eOff + 0] - src[lOff + 0];
        sumG += src[eOff + 1] - src[lOff + 1];
        sumR += src[eOff + 2] - src[lOff + 2];
        sumA += src[eOff + 3] - src[lOff + 3];
      }
    }
  }
}
