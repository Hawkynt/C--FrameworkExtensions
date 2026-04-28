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
using Hawkynt.ColorProcessing.Resizing;
using Hawkynt.ColorProcessing.Resizing.Resamplers;
using Hawkynt.Drawing.Lockers;

namespace Hawkynt.ColorProcessing.Geometry;

/// <summary>
/// Internal helper: samples a source bitmap at an arbitrary
/// floating-point <c>(x, y)</c> using an upstream
/// <see cref="IKernelResampler"/> kernel for the weighting function.
/// </summary>
/// <remarks>
/// <para>
/// All four geometric transforms (<see cref="Rotate"/>, <see cref="Shear"/>,
/// <see cref="AffineTransform"/>, <see cref="PerspectiveTransform"/>) reduce
/// to "for every destination pixel, inverse-map to a source coordinate and
/// sample." The sampling step is shared here so each transform contains only
/// the matrix algebra specific to it.
/// </para>
/// <para>
/// The sampler picks the kernel by <see cref="GeometricInterpolation"/> and
/// queries <see cref="IKernelResampler.EvaluateWeight"/> at the offset of every
/// source pixel inside the kernel footprint. Weights are summed with the
/// per-channel pixel values and the running weight, then divided once at the
/// end — a textbook separable convolution evaluated in 2-D.
/// </para>
/// </remarks>
internal static class GeometricSampler {

  /// <summary>Samples <paramref name="src"/> at <c>(srcX, srcY)</c> using <paramref name="mode"/>.</summary>
  /// <param name="src">The source bitmap locker (read-only).</param>
  /// <param name="srcX">Floating-point source X coordinate.</param>
  /// <param name="srcY">Floating-point source Y coordinate.</param>
  /// <param name="mode">Interpolation kernel selector.</param>
  /// <param name="fill">Boundary policy for samples outside the source image.</param>
  /// <returns>The interpolated colour, suitable for writing to the destination.</returns>
  /// <remarks>
  /// <para>
  /// For <see cref="GeometricInterpolation.NearestNeighbor"/> the sampler
  /// short-circuits to a single pixel fetch (no kernel evaluation).
  /// </para>
  /// <para>
  /// For all other modes the sampler computes the kernel weight at every
  /// integer source position within radius and accumulates colour channels
  /// in linear-space arithmetic on bytes — this matches GDI+'s legacy bicubic
  /// closely enough for visual consistency while remaining bit-exact across
  /// architectures.
  /// </para>
  /// </remarks>
  public static Color Sample(IBitmapLocker src, double srcX, double srcY, GeometricInterpolation mode, FillSpec fill) {
    var w = src.Width;
    var h = src.Height;

    if (mode == GeometricInterpolation.NearestNeighbor) {
      var ix = (int)Math.Floor(srcX + 0.5);
      var iy = (int)Math.Floor(srcY + 0.5);
      return _Fetch(src, ix, iy, w, h, fill);
    }

    // Kernel-based separable convolution.
    int radius;
    Func<float, float> weight;
    switch (mode) {
      case GeometricInterpolation.Bilinear: {
        var k = Bilinear.Default;
        radius = k.Radius;
        weight = k.EvaluateWeight;
        break;
      }
      case GeometricInterpolation.Bicubic: {
        var k = Bicubic.Default;
        radius = k.Radius;
        weight = k.EvaluateWeight;
        break;
      }
      case GeometricInterpolation.Lanczos3: {
        var k = Lanczos3.Default;
        radius = k.Radius;
        weight = k.EvaluateWeight;
        break;
      }
      default:
        throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown interpolation mode.");
    }

    var x0 = (int)Math.Floor(srcX);
    var y0 = (int)Math.Floor(srcY);
    var fx = (float)(srcX - x0);
    var fy = (float)(srcY - y0);

    // The kernel samples from -(radius-1) to +radius; precompute weights along each axis.
    var span = 2 * radius;
    Span<float> wx = stackalloc float[8]; // max radius supported = 4 ⇒ span 8
    Span<float> wy = stackalloc float[8];
    if (span > 8)
      throw new InvalidOperationException("Kernel radius exceeds sampler stack budget.");

    var sumWx = 0f;
    var sumWy = 0f;
    for (var i = 0; i < span; ++i) {
      var ox = i - (radius - 1);
      wx[i] = weight(ox - fx);
      wy[i] = weight(ox - fy);
      sumWx += wx[i];
      sumWy += wy[i];
    }
    // Guard against pathological zero-sum (shouldn't happen for shipped kernels).
    if (sumWx == 0f) sumWx = 1f;
    if (sumWy == 0f) sumWy = 1f;
    for (var i = 0; i < span; ++i) {
      wx[i] /= sumWx;
      wy[i] /= sumWy;
    }

    float aA = 0f, aR = 0f, aG = 0f, aB = 0f;
    for (var j = 0; j < span; ++j) {
      var sy = y0 + j - (radius - 1);
      var rowW = wy[j];
      for (var i = 0; i < span; ++i) {
        var sx = x0 + i - (radius - 1);
        var c = _Fetch(src, sx, sy, w, h, fill);
        var ww = rowW * wx[i];
        aA += c.A * ww;
        aR += c.R * ww;
        aG += c.G * ww;
        aB += c.B * ww;
      }
    }

    return Color.FromArgb(
      _Clamp(aA),
      _Clamp(aR),
      _Clamp(aG),
      _Clamp(aB));
  }

  private static int _Clamp(float v) {
    var i = (int)(v + 0.5f);
    if (i < 0) return 0;
    if (i > 255) return 255;
    return i;
  }

  private static Color _Fetch(IBitmapLocker src, int x, int y, int w, int h, FillSpec fill) {
    if ((uint)x < (uint)w && (uint)y < (uint)h)
      return src[x, y];

    switch (fill.Mode) {
      case GeometricFillMode.Clamp:
        if (x < 0) x = 0;
        else if (x >= w) x = w - 1;
        if (y < 0) y = 0;
        else if (y >= h) y = h - 1;
        return src[x, y];
      case GeometricFillMode.Wrap:
        x = _Mod(x, w);
        y = _Mod(y, h);
        return src[x, y];
      case GeometricFillMode.Mirror:
        x = _Mirror(x, w);
        y = _Mirror(y, h);
        return src[x, y];
      case GeometricFillMode.Constant:
      default:
        return fill.Color;
    }
  }

  private static int _Mod(int v, int n) {
    var r = v % n;
    return r < 0 ? r + n : r;
  }

  private static int _Mirror(int v, int n) {
    if (n <= 1) return 0;
    var period = 2 * n - 2;
    var r = _Mod(v, period);
    return r >= n ? period - r : r;
  }
}
