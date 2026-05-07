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

namespace Hawkynt.ColorProcessing.Multiscale;

/// <summary>
/// Burt-Adelson Gaussian pyramid: a sequence of progressively half-sized
/// Gaussian-blurred copies of the source image.
/// </summary>
/// <remarks>
/// <para>Each level is a 2× downsample of a 5-tap-Gaussian (1, 4, 6, 4, 1)/16 blurred copy
/// of the previous level. The blur is the standard Burt-Adelson kernel; the downsample is a
/// strict drop-every-other-pixel after the blur (equivalent to convolution + decimation).</para>
/// <para>Reference: Burt &amp; Adelson 1983, "The Laplacian Pyramid as a Compact Image Code",
/// IEEE Trans. Communications.</para>
/// </remarks>
public static class GaussianPyramid {

  /// <summary>
  /// Builds a Gaussian pyramid from <paramref name="source"/> with <paramref name="levels"/>
  /// total levels (level 0 = original size, level k = source / 2^k).
  /// </summary>
  /// <param name="source">The source bitmap. Must be non-null and at least 2×2.</param>
  /// <param name="levels">Total number of levels (≥ 1). Truncated automatically if any
  /// level would shrink below 1 pixel.</param>
  /// <returns>An array of <c>levels</c> bitmaps, owned by the caller — dispose each.</returns>
  public static Bitmap[] Build(Bitmap source, int levels) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfLessThan(levels, 1);

    var result = new Bitmap[levels];
    result[0] = _CloneToBgra32(source);

    for (var i = 1; i < levels; ++i) {
      var prev = result[i - 1];
      var w = prev.Width / 2;
      var h = prev.Height / 2;
      if (w < 1 || h < 1) {
        // Truncate the array to what we actually built.
        Array.Resize(ref result, i);
        break;
      }
      result[i] = ReduceOnce(prev);
    }
    return result;
  }

  /// <summary>
  /// Performs one Gaussian-blur + 2× downsample step. Public so callers can build
  /// custom-shaped pyramids if needed (e.g., wavelet-style steerable pyramids).
  /// </summary>
  public static Bitmap ReduceOnce(Bitmap source) {
    ArgumentNullException.ThrowIfNull(source);

    var w = source.Width;
    var h = source.Height;
    var dstW = Math.Max(1, w / 2);
    var dstH = Math.Max(1, h / 2);

    var src = _Read(source);
    var blurred = _BlurSeparable(src, w, h);
    var dst = new float[dstW * dstH * 4];

    // Decimate: pick (2x, 2y) sample after the blur. This is the canonical Burt-Adelson
    // step and matches the closed-form analysis of the Laplacian pyramid construction.
    for (var y = 0; y < dstH; ++y) {
      var sy = Math.Min(h - 1, y * 2);
      for (var x = 0; x < dstW; ++x) {
        var sx = Math.Min(w - 1, x * 2);
        var srcIdx = (sy * w + sx) * 4;
        var dstIdx = (y * dstW + x) * 4;
        dst[dstIdx + 0] = blurred[srcIdx + 0];
        dst[dstIdx + 1] = blurred[srcIdx + 1];
        dst[dstIdx + 2] = blurred[srcIdx + 2];
        dst[dstIdx + 3] = blurred[srcIdx + 3];
      }
    }

    return _Write(dst, dstW, dstH);
  }

  /// <summary>
  /// Performs one 2× upsample + Gaussian smoothing step (the "expand" operation). Used by
  /// <see cref="LaplacianPyramid"/> reconstruction; exposed so callers can build their own
  /// pyramid-walking traversals.
  /// </summary>
  /// <param name="source">The lower-resolution image to expand.</param>
  /// <param name="targetWidth">Target width (typically 2× source width, but supports the
  /// odd-dimensioned case where the target is 2·src ± 1).</param>
  /// <param name="targetHeight">Target height.</param>
  public static Bitmap ExpandOnce(Bitmap source, int targetWidth, int targetHeight) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var w = source.Width;
    var h = source.Height;
    var src = _Read(source);
    var blurred = ExpandOnceFloat(src, w, h, targetWidth, targetHeight);
    return _Write(blurred, targetWidth, targetHeight);
  }

  /// <summary>
  /// Float-domain expand step for use in Laplacian-pyramid reconstruction. Avoids the
  /// byte-clamping round-trip through Bitmap that destroys the float-exact lossless
  /// property when intermediate reconstruction values exceed [0, 255].
  /// </summary>
  /// <remarks>
  /// <para>Burt &amp; Adelson 1983 §II.B "expand" form: insert zeros (upsample 2×), then convolve
  /// with the 5-tap binomial kernel scaled by 4 (one factor of 2 per axis to compensate
  /// for the zero-insertion energy loss). Source <c>src</c> is a 4-channel BGRA float buffer
  /// of size <c>w × h</c>; output is <c>targetWidth × targetHeight</c> 4-channel float.</para>
  /// <para>Concretely: this implementation does (a) zero-insert, (b) multiply non-zero
  /// samples by 4, then (c) apply <see cref="_BlurSeparable"/> which uses the
  /// <em>normalised</em> kernel <c>[1, 4, 6, 4, 1] / 16</c> per axis. Trace on a constant
  /// input <c>c</c>: zero-insert produces a checkerboard with values {c, 0}; the H-pass
  /// blur on a row "c, 0, c, 0, c" gives <c>(0+4c+0+4c+0)/16 = c/2</c>; the V-pass on
  /// a column of {c/2, 0, c/2, 0, c/2} likewise gives <c>c/4</c>. The explicit ×4 prefactor
  /// restores <c>c</c>, matching the canonical Burt-Adelson EXPAND specification
  /// <c>EXPAND(g_l)(i,j) = 4 · Σ w(m,n) · g_l((i−m)/2, (j−n)/2)</c>.</para>
  /// </remarks>
  internal static float[] ExpandOnceFloat(float[] src, int w, int h, int targetWidth, int targetHeight) {
    var upsampled = new float[targetWidth * targetHeight * 4];
    for (var y = 0; y < h; ++y) {
      var ty = y * 2;
      if (ty >= targetHeight) break;
      for (var x = 0; x < w; ++x) {
        var tx = x * 2;
        if (tx >= targetWidth) break;
        var s = (y * w + x) * 4;
        var d = (ty * targetWidth + tx) * 4;
        upsampled[d + 0] = src[s + 0] * 4f;
        upsampled[d + 1] = src[s + 1] * 4f;
        upsampled[d + 2] = src[s + 2] * 4f;
        upsampled[d + 3] = src[s + 3] * 4f;
      }
    }
    return _BlurSeparable(upsampled, targetWidth, targetHeight);
  }

  // ---- internals -------------------------------------------------------------

  private static Bitmap _CloneToBgra32(Bitmap source) {
    var clone = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
    using var g = Graphics.FromImage(clone);
    g.DrawImage(source, 0, 0, source.Width, source.Height);
    return clone;
  }

  private static unsafe float[] _Read(Bitmap source) {
    var w = source.Width;
    var h = source.Height;
    using var bgra = (source.PixelFormat == PixelFormat.Format32bppArgb)
      ? null
      : _CloneToBgra32(source);
    var bmp = bgra ?? source;

    var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    try {
      var dst = new float[w * h * 4];
      for (var y = 0; y < h; ++y) {
        var row = (byte*)data.Scan0 + y * data.Stride;
        for (var x = 0; x < w; ++x) {
          var i = (y * w + x) * 4;
          dst[i + 0] = row[x * 4 + 0]; // B
          dst[i + 1] = row[x * 4 + 1]; // G
          dst[i + 2] = row[x * 4 + 2]; // R
          dst[i + 3] = row[x * 4 + 3]; // A
        }
      }
      return dst;
    } finally {
      bmp.UnlockBits(data);
    }
  }

  private static unsafe Bitmap _Write(float[] src, int w, int h) {
    var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try {
      for (var y = 0; y < h; ++y) {
        var row = (byte*)data.Scan0 + y * data.Stride;
        for (var x = 0; x < w; ++x) {
          var i = (y * w + x) * 4;
          row[x * 4 + 0] = _ClampByte(src[i + 0]);
          row[x * 4 + 1] = _ClampByte(src[i + 1]);
          row[x * 4 + 2] = _ClampByte(src[i + 2]);
          row[x * 4 + 3] = _ClampByte(src[i + 3]);
        }
      }
    } finally {
      bmp.UnlockBits(data);
    }
    return bmp;
  }

  private static byte _ClampByte(float v) {
    if (v < 0f) return 0;
    if (v > 255f) return 255;
    return (byte)(v + 0.5f);
  }

  // 5-tap Gaussian (1, 4, 6, 4, 1) / 16, separable. Reflective edge handling.
  private static float[] _BlurSeparable(float[] src, int w, int h) {
    var tmp = new float[w * h * 4];
    var dst = new float[w * h * 4];
    const float k0 = 6f / 16f;
    const float k1 = 4f / 16f;
    const float k2 = 1f / 16f;

    // Horizontal pass.
    for (var y = 0; y < h; ++y) {
      for (var x = 0; x < w; ++x) {
        var i = (y * w + x) * 4;
        var im2 = (y * w + _Mirror(x - 2, w)) * 4;
        var im1 = (y * w + _Mirror(x - 1, w)) * 4;
        var ip1 = (y * w + _Mirror(x + 1, w)) * 4;
        var ip2 = (y * w + _Mirror(x + 2, w)) * 4;
        tmp[i + 0] = k2 * src[im2 + 0] + k1 * src[im1 + 0] + k0 * src[i + 0] + k1 * src[ip1 + 0] + k2 * src[ip2 + 0];
        tmp[i + 1] = k2 * src[im2 + 1] + k1 * src[im1 + 1] + k0 * src[i + 1] + k1 * src[ip1 + 1] + k2 * src[ip2 + 1];
        tmp[i + 2] = k2 * src[im2 + 2] + k1 * src[im1 + 2] + k0 * src[i + 2] + k1 * src[ip1 + 2] + k2 * src[ip2 + 2];
        tmp[i + 3] = k2 * src[im2 + 3] + k1 * src[im1 + 3] + k0 * src[i + 3] + k1 * src[ip1 + 3] + k2 * src[ip2 + 3];
      }
    }

    // Vertical pass.
    for (var y = 0; y < h; ++y) {
      var ym2 = _Mirror(y - 2, h);
      var ym1 = _Mirror(y - 1, h);
      var yp1 = _Mirror(y + 1, h);
      var yp2 = _Mirror(y + 2, h);
      for (var x = 0; x < w; ++x) {
        var i = (y * w + x) * 4;
        var jm2 = (ym2 * w + x) * 4;
        var jm1 = (ym1 * w + x) * 4;
        var jp1 = (yp1 * w + x) * 4;
        var jp2 = (yp2 * w + x) * 4;
        dst[i + 0] = k2 * tmp[jm2 + 0] + k1 * tmp[jm1 + 0] + k0 * tmp[i + 0] + k1 * tmp[jp1 + 0] + k2 * tmp[jp2 + 0];
        dst[i + 1] = k2 * tmp[jm2 + 1] + k1 * tmp[jm1 + 1] + k0 * tmp[i + 1] + k1 * tmp[jp1 + 1] + k2 * tmp[jp2 + 1];
        dst[i + 2] = k2 * tmp[jm2 + 2] + k1 * tmp[jm1 + 2] + k0 * tmp[i + 2] + k1 * tmp[jp1 + 2] + k2 * tmp[jp2 + 2];
        dst[i + 3] = k2 * tmp[jm2 + 3] + k1 * tmp[jm1 + 3] + k0 * tmp[i + 3] + k1 * tmp[jp1 + 3] + k2 * tmp[jp2 + 3];
      }
    }
    return dst;
  }

  // Mirror-edge sample index — common boundary policy for image pyramids; avoids the dark
  // border halo that zero-extension produces and the brightness drift that clamp produces.
  private static int _Mirror(int i, int n) {
    if (n == 1) return 0;
    if (i < 0) i = -i;
    if (i >= n) i = 2 * (n - 1) - i;
    if (i < 0) i = 0;
    if (i >= n) i = n - 1;
    return i;
  }
}
