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
/// Poisson seamless image cloning — Pérez, Gangnet &amp; Blake 2003.
/// </summary>
/// <remarks>
/// <para>Pastes a region of a <i>source</i> image into a <i>destination</i> image with
/// imperceptible boundaries by reconstructing the cloned region's pixels to satisfy
/// <c>∇²f = ∇²I_source</c> inside the masked area while matching the destination's pixel
/// values exactly on the boundary. The result is that the source's local detail (edges,
/// texture, contrast) transfers cleanly while overall colour and lighting smoothly blend
/// into the destination — the canonical solution for compositing without visible seams.</para>
/// <para>This is a genuinely new capability category — none of the library's 170+ filters
/// or 40+ resamplers solves the gradient-domain image-editing problem.</para>
/// <para>Solver: 200 Gauss-Seidel iterations on the dense Poisson equation (sufficient for
/// patches up to ~64×64 with default damping; larger patches benefit from the multigrid
/// extension which is out of scope here). Deterministic; no RNG.</para>
/// <para>Reference: Pérez, Gangnet &amp; Blake 2003, "Poisson Image Editing", SIGGRAPH.</para>
/// </remarks>
public static class PoissonSeamlessCloning {

  /// <summary>Default Gauss-Seidel iteration count.</summary>
  public const int DefaultIterations = 200;

  /// <summary>
  /// Clones a rectangular region of <paramref name="source"/> into <paramref name="destination"/>
  /// at offset <paramref name="offset"/>, using Poisson seamless blending.
  /// </summary>
  /// <param name="source">The source image to clone <i>from</i>.</param>
  /// <param name="destination">The destination image to clone <i>into</i>. Returned bitmap
  /// is a new clone of this image with the patched region edited; the destination input is
  /// left unchanged.</param>
  /// <param name="sourceRect">Region of <paramref name="source"/> to copy. Must fit inside source.</param>
  /// <param name="offset">Top-left corner in <paramref name="destination"/> where the patch
  /// should land. Must be ≥ 1 and (offset + sourceRect.Size) must fit ≤ destination − 1 to
  /// guarantee a 1-pixel boundary on every side for the Dirichlet condition.</param>
  /// <param name="iterations">Gauss-Seidel iteration count (default 200).</param>
  public static Bitmap Clone(
    Bitmap source, Bitmap destination, Rectangle sourceRect, Point offset,
    int iterations = DefaultIterations) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(destination);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(iterations);

    var rw = sourceRect.Width;
    var rh = sourceRect.Height;
    if (sourceRect.Left < 0 || sourceRect.Top < 0
        || sourceRect.Right > source.Width || sourceRect.Bottom > source.Height
        || rw < 1 || rh < 1)
      throw new ArgumentOutOfRangeException(nameof(sourceRect), "sourceRect must be in source bounds.");
    if (offset.X < 1 || offset.Y < 1
        || offset.X + rw > destination.Width - 1
        || offset.Y + rh > destination.Height - 1)
      throw new ArgumentOutOfRangeException(nameof(offset),
        "offset + sourceRect.Size must leave at least 1 pixel of margin in destination.");

    var result = (Bitmap)destination.Clone();
    if (result.PixelFormat != PixelFormat.Format32bppArgb) {
      var rgba = new Bitmap(result.Width, result.Height, PixelFormat.Format32bppArgb);
      using (var g = Graphics.FromImage(rgba)) g.DrawImage(result, 0, 0);
      result.Dispose();
      result = rgba;
    }

    _CloneChannels(source, result, sourceRect, offset, iterations);
    return result;
  }

  private static unsafe void _CloneChannels(
    Bitmap source, Bitmap result, Rectangle sourceRect, Point offset, int iterations) {
    var rw = sourceRect.Width;
    var rh = sourceRect.Height;
    var srcF = _Read(source);
    var dstF = _Read(result);
    var srcW = source.Width;
    var dstW = result.Width;

    // Solve the Poisson equation per channel (B, G, R; alpha is copied through).
    // f(x, y) = ¼ · (f(x−1) + f(x+1) + f(y−1) + f(y+1) + b(x, y))
    // where b is the source's discrete Laplacian (sampled at the source position).
    for (var ch = 0; ch < 3; ++ch) {
      // Compute b = ∇² of source channel at every pixel inside the patch.
      var b = new float[rw * rh];
      for (var y = 0; y < rh; ++y) {
        for (var x = 0; x < rw; ++x) {
          var sx = sourceRect.Left + x;
          var sy = sourceRect.Top + y;
          var c = srcF[(sy * srcW + sx) * 4 + ch];
          // Reflective edge handling at the source-rect boundary.
          var sxm = Math.Max(0, sx - 1);
          var sxp = Math.Min(source.Width - 1, sx + 1);
          var sym = Math.Max(0, sy - 1);
          var syp = Math.Min(source.Height - 1, sy + 1);
          var lap = srcF[(sy * srcW + sxm) * 4 + ch] + srcF[(sy * srcW + sxp) * 4 + ch]
                  + srcF[(sym * srcW + sx) * 4 + ch] + srcF[(syp * srcW + sx) * 4 + ch]
                  - 4f * c;
          b[y * rw + x] = lap;
        }
      }

      // Initialise interior to source value; boundary in dstF is already destination.
      var f = new float[rw * rh];
      for (var y = 0; y < rh; ++y)
      for (var x = 0; x < rw; ++x)
        f[y * rw + x] = srcF[((sourceRect.Top + y) * srcW + (sourceRect.Left + x)) * 4 + ch];

      // Gauss-Seidel iteration. Boundary samples come from dstF at offset±0.
      for (var iter = 0; iter < iterations; ++iter) {
        for (var y = 0; y < rh; ++y) {
          var ty = offset.Y + y;
          for (var x = 0; x < rw; ++x) {
            var tx = offset.X + x;
            float fxm, fxp, fym, fyp;
            // West neighbour: in-mask if x > 0 else destination boundary at (tx − 1, ty).
            fxm = x > 0 ? f[y * rw + (x - 1)] : dstF[(ty * dstW + (tx - 1)) * 4 + ch];
            fxp = x < rw - 1 ? f[y * rw + (x + 1)] : dstF[(ty * dstW + (tx + 1)) * 4 + ch];
            fym = y > 0 ? f[(y - 1) * rw + x] : dstF[((ty - 1) * dstW + tx) * 4 + ch];
            fyp = y < rh - 1 ? f[(y + 1) * rw + x] : dstF[((ty + 1) * dstW + tx) * 4 + ch];
            f[y * rw + x] = 0.25f * (fxm + fxp + fym + fyp - b[y * rw + x]);
          }
        }
      }

      // Write solved values back into dstF.
      for (var y = 0; y < rh; ++y)
      for (var x = 0; x < rw; ++x)
        dstF[((offset.Y + y) * dstW + (offset.X + x)) * 4 + ch] = f[y * rw + x];
    }

    // Repack into the result bitmap.
    var data = result.LockBits(new Rectangle(0, 0, result.Width, result.Height),
      ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try {
      for (var y = 0; y < result.Height; ++y) {
        var row = (byte*)data.Scan0 + y * data.Stride;
        for (var x = 0; x < result.Width; ++x) {
          var i = (y * result.Width + x) * 4;
          row[x * 4 + 0] = _Clamp(dstF[i + 0]);
          row[x * 4 + 1] = _Clamp(dstF[i + 1]);
          row[x * 4 + 2] = _Clamp(dstF[i + 2]);
          row[x * 4 + 3] = _Clamp(dstF[i + 3]);
        }
      }
    } finally {
      result.UnlockBits(data);
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

  private static byte _Clamp(float v) {
    if (v < 0f) return 0;
    if (v > 255f) return 255;
    return (byte)(v + 0.5f);
  }
}
