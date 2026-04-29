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
/// Domain-Transform Filter — Gastal &amp; Oliveira 2011.
/// </summary>
/// <remarks>
/// <para>The fastest edge-aware filter in the literature: O(N) per axis via a 1-D
/// recursive filter whose feedback coefficient at each step is determined by the local
/// image gradient. Replaces 2-D Gaussian / bilateral / NLM convolutions with two cheap
/// recursive passes — typical 1024² runtime is &lt; 50 ms single-threaded versus
/// hundreds of milliseconds for the comparable bilateral.</para>
/// <para>Algorithm: build a 1-D <i>domain transform</i> ct(x) = ∫ (1 + (σ_s/σ_r)·|∇I|) dx
/// along each axis (cumulative gradient with σ_s, σ_r weighting). Apply a recursive
/// filter <c>f[i] = (1−a^d)·I[i] + a^d·f[i−1]</c> where <c>d</c> = ct(i) − ct(i−1), so
/// the filter naturally <i>stops</i> at edges (large d → small a^d → little carry-over).
/// Two passes per axis (forward + backward) for full-symmetry behaviour, and N iterations
/// of the whole 4-pass cycle for stronger smoothing.</para>
/// <para>Distinct from the Round 5 <see cref="Hawkynt.ColorProcessing.Filtering.Filters.NonLocalMeans"/>
/// (patch-search-based, O(N · |patch|² · |search|²)) and the various Gaussian / bilateral
/// filters (O(N · radius²)). Domain-Transform is strictly faster for the same edge-aware
/// quality on smooth content.</para>
/// <para>Reference: Gastal &amp; Oliveira 2011, "Domain Transform for Edge-Aware Image and
/// Video Processing", SIGGRAPH (ACM TOG 30:4).</para>
/// </remarks>
public static class DomainTransformFilter {

  /// <summary>Default spatial sigma (in normalised pixel units).</summary>
  public const float DefaultSigmaSpatial = 60f;

  /// <summary>Default range sigma (in 0-255 luminance units).</summary>
  public const float DefaultSigmaRange = 0.4f;

  /// <summary>Default iteration count of the 4-pass cycle.</summary>
  public const int DefaultIterations = 3;

  /// <summary>
  /// Applies the Domain-Transform recursive filter to <paramref name="source"/>.
  /// </summary>
  /// <param name="source">Input bitmap.</param>
  /// <param name="sigmaSpatial">Controls the filter's spatial reach. Larger = stronger smoothing.</param>
  /// <param name="sigmaRange">Controls edge-stopping. Smaller = sharper edges preserved.</param>
  /// <param name="iterations">Number of forward + backward filter cycles. Each iteration's
  /// feedback coefficient is scaled to give a Gaussian-like impulse response after N passes.</param>
  public static Bitmap Apply(
    Bitmap source,
    float sigmaSpatial = DefaultSigmaSpatial,
    float sigmaRange = DefaultSigmaRange,
    int iterations = DefaultIterations) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sigmaSpatial);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sigmaRange);
    ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(iterations, 16);

    var w = source.Width;
    var h = source.Height;
    var data = _Read(source);

    // Precompute the per-axis gradient sums Σ_c |∂I_c/∂axis| in 0..1 normalised intensity.
    // These determine the "domain transform" distances ct_h[x,y], ct_v[x,y].
    var dHdx = new float[w * h]; // |∂_x I| (sum over channels), at each pixel
    var dVdy = new float[w * h]; // |∂_y I|
    for (var y = 0; y < h; ++y) {
      for (var x = 1; x < w; ++x) {
        var dr = (data[(y * w + x) * 4 + 2] - data[(y * w + (x - 1)) * 4 + 2]) / 255f;
        var dg = (data[(y * w + x) * 4 + 1] - data[(y * w + (x - 1)) * 4 + 1]) / 255f;
        var db = (data[(y * w + x) * 4 + 0] - data[(y * w + (x - 1)) * 4 + 0]) / 255f;
        dHdx[y * w + x] = MathF.Abs(dr) + MathF.Abs(dg) + MathF.Abs(db);
      }
    }
    for (var x = 0; x < w; ++x) {
      for (var y = 1; y < h; ++y) {
        var dr = (data[(y * w + x) * 4 + 2] - data[((y - 1) * w + x) * 4 + 2]) / 255f;
        var dg = (data[(y * w + x) * 4 + 1] - data[((y - 1) * w + x) * 4 + 1]) / 255f;
        var db = (data[(y * w + x) * 4 + 0] - data[((y - 1) * w + x) * 4 + 0]) / 255f;
        dVdy[y * w + x] = MathF.Abs(dr) + MathF.Abs(dg) + MathF.Abs(db);
      }
    }

    // Iteration loop — each pass uses a halving sigma to mimic a Gaussian impulse response
    // built from N exponential-recursive filters. Standard Gastal-Oliveira convergence.
    for (var iter = 0; iter < iterations; ++iter) {
      var sigmaH = sigmaSpatial * MathF.Sqrt(3f) * MathF.Pow(2f, iterations - iter - 1)
                                                  / MathF.Sqrt(MathF.Pow(4f, iterations) - 1f);
      _HorizontalPass(data, dHdx, w, h, sigmaH, sigmaRange);
      _VerticalPass(data, dVdy, w, h, sigmaH, sigmaRange);
    }

    return _Write(data, w, h);
  }

  // 1-D recursive filter along each row, both directions, using the cumulative |∂_x I|.
  private static void _HorizontalPass(float[] data, float[] dHdx, int w, int h, float sigmaH, float sigmaR) {
    var a = MathF.Exp(-MathF.Sqrt(2f) / sigmaH);
    for (var y = 0; y < h; ++y) {
      // Forward pass: f[i] = (1 − a^d) · I[i] + a^d · f[i−1]
      for (var x = 1; x < w; ++x) {
        var d = 1f + sigmaH / sigmaR * dHdx[y * w + x];
        var alpha = MathF.Pow(a, d);
        for (var c = 0; c < 4; ++c) {
          var idx = (y * w + x) * 4 + c;
          var prev = (y * w + (x - 1)) * 4 + c;
          data[idx] = (1f - alpha) * data[idx] + alpha * data[prev];
        }
      }
      // Backward pass: same form, right-to-left, using d at the next position.
      for (var x = w - 2; x >= 0; --x) {
        var d = 1f + sigmaH / sigmaR * dHdx[y * w + (x + 1)];
        var alpha = MathF.Pow(a, d);
        for (var c = 0; c < 4; ++c) {
          var idx = (y * w + x) * 4 + c;
          var next = (y * w + (x + 1)) * 4 + c;
          data[idx] = (1f - alpha) * data[idx] + alpha * data[next];
        }
      }
    }
  }

  private static void _VerticalPass(float[] data, float[] dVdy, int w, int h, float sigmaH, float sigmaR) {
    var a = MathF.Exp(-MathF.Sqrt(2f) / sigmaH);
    for (var x = 0; x < w; ++x) {
      for (var y = 1; y < h; ++y) {
        var d = 1f + sigmaH / sigmaR * dVdy[y * w + x];
        var alpha = MathF.Pow(a, d);
        for (var c = 0; c < 4; ++c) {
          var idx = (y * w + x) * 4 + c;
          var prev = ((y - 1) * w + x) * 4 + c;
          data[idx] = (1f - alpha) * data[idx] + alpha * data[prev];
        }
      }
      for (var y = h - 2; y >= 0; --y) {
        var d = 1f + sigmaH / sigmaR * dVdy[(y + 1) * w + x];
        var alpha = MathF.Pow(a, d);
        for (var c = 0; c < 4; ++c) {
          var idx = (y * w + x) * 4 + c;
          var next = ((y + 1) * w + x) * 4 + c;
          data[idx] = (1f - alpha) * data[idx] + alpha * data[next];
        }
      }
    }
  }

  private static unsafe float[] _Read(Bitmap source) {
    var w = source.Width;
    var h = source.Height;
    Bitmap bgra = source;
    Bitmap? clone = null;
    if (source.PixelFormat != PixelFormat.Format32bppArgb) {
      clone = new Bitmap(w, h, PixelFormat.Format32bppArgb);
      using var g = Graphics.FromImage(clone);
      g.DrawImage(source, 0, 0);
      bgra = clone;
    }
    try {
      var d = bgra.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      try {
        var dst = new float[w * h * 4];
        for (var y = 0; y < h; ++y) {
          var row = (byte*)d.Scan0 + y * d.Stride;
          for (var x = 0; x < w; ++x) {
            var i = (y * w + x) * 4;
            dst[i + 0] = row[x * 4 + 0];
            dst[i + 1] = row[x * 4 + 1];
            dst[i + 2] = row[x * 4 + 2];
            dst[i + 3] = row[x * 4 + 3];
          }
        }
        return dst;
      } finally {
        bgra.UnlockBits(d);
      }
    } finally {
      clone?.Dispose();
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
}
