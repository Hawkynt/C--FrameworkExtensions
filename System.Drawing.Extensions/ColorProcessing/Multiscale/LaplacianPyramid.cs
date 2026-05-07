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
/// Burt-Adelson Laplacian pyramid: the band-pass decomposition of an image into a sequence
/// of difference-of-Gaussians plus the coarsest Gaussian residual.
/// </summary>
/// <remarks>
/// <para>For an N-level pyramid:</para>
/// <list type="bullet">
/// <item>Levels 0..N-2 store the band-pass residual <c>L_k = G_k - expand(G_{k+1})</c>
///   where G is the matching Gaussian-pyramid level.</item>
/// <item>Level N-1 stores the coarsest Gaussian <c>G_{N-1}</c> directly (the DC residual).</item>
/// </list>
/// <para>Reconstruction is lossless under exact arithmetic: walk from coarsest to finest
/// level, expanding and adding the band-pass residual at each step. Round-trip
/// accuracy is bounded by the rounding done when packing residuals into 8-bit BGRA storage
/// — this implementation stores residuals in <see cref="float"/> arrays so the round-trip
/// is exact in floating point.</para>
/// <para>Reference: Burt &amp; Adelson 1983, "The Laplacian Pyramid as a Compact Image Code",
/// IEEE Trans. Communications.</para>
/// </remarks>
public sealed class LaplacianPyramid {

  /// <summary>
  /// Per-level band-pass residual storage. Indexed [w*4 + h*4*w + c]; channels are BGRA.
  /// Residual values are in float (signed) — typically [-128, 127] but unbounded; the
  /// last level holds the coarsest Gaussian (unsigned, in [0, 255]).
  /// </summary>
  private readonly float[][] _levels;
  private readonly int[] _widths;
  private readonly int[] _heights;

  /// <summary>Total number of pyramid levels (including the coarsest Gaussian residual).</summary>
  public int LevelCount => this._levels.Length;

  /// <summary>Width of pyramid level <paramref name="level"/>.</summary>
  public int WidthAt(int level) {
    ArgumentOutOfRangeException.ThrowIfNegative(level);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(level, this._levels.Length);
    return this._widths[level];
  }

  /// <summary>Height of pyramid level <paramref name="level"/>.</summary>
  public int HeightAt(int level) {
    ArgumentOutOfRangeException.ThrowIfNegative(level);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(level, this._levels.Length);
    return this._heights[level];
  }

  private LaplacianPyramid(float[][] levels, int[] widths, int[] heights) {
    this._levels = levels;
    this._widths = widths;
    this._heights = heights;
  }

  /// <summary>
  /// Builds a Laplacian pyramid from <paramref name="source"/>.
  /// </summary>
  /// <param name="source">The source bitmap. Must be non-null and at least 2×2.</param>
  /// <param name="levels">Number of pyramid levels (≥ 1). Auto-truncated if a level shrinks
  /// below 1 pixel. With <c>levels = 1</c> the result holds only the source as a "residual"
  /// (degenerate case).</param>
  public static LaplacianPyramid Build(Bitmap source, int levels) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfLessThan(levels, 1);

    var gaussian = GaussianPyramid.Build(source, levels);
    try {
      var actual = gaussian.Length;
      var resLevels = new float[actual][];
      var widths = new int[actual];
      var heights = new int[actual];

      for (var i = 0; i < actual - 1; ++i) {
        // expand(G_{i+1}) at G_i's resolution, then subtract for the band-pass residual.
        var coarse = gaussian[i + 1];
        var fineW = gaussian[i].Width;
        var fineH = gaussian[i].Height;
        using var expanded = GaussianPyramid.ExpandOnce(coarse, fineW, fineH);
        resLevels[i] = _SubtractToFloat(gaussian[i], expanded);
        widths[i] = fineW;
        heights[i] = fineH;
      }
      // Last level: keep the coarsest Gaussian as the DC residual, unsigned.
      var last = actual - 1;
      resLevels[last] = _ReadFloat(gaussian[last]);
      widths[last] = gaussian[last].Width;
      heights[last] = gaussian[last].Height;

      return new LaplacianPyramid(resLevels, widths, heights);
    } finally {
      foreach (var bmp in gaussian)
        bmp.Dispose();
    }
  }

  /// <summary>
  /// Reconstructs the source bitmap from this pyramid. Lossless in floating-point
  /// arithmetic; final encoding into 8-bit BGRA clamps to [0, 255] with rounding.
  /// </summary>
  /// <remarks>
  /// Uses the float-domain <see cref="GaussianPyramid.ExpandOnceFloat"/> to avoid an
  /// intermediate byte-clamping round-trip — the running reconstruction `current` may
  /// exceed [0, 255] at intermediate levels (high-amplitude residuals), and clamping it
  /// before the next expand would destroy the lossless property the class promises.
  /// </remarks>
  public Bitmap Reconstruct() {
    var actual = this._levels.Length;
    if (actual == 0)
      throw new InvalidOperationException("Empty pyramid.");

    // Walk from coarsest to finest: expand current (in float), add band-pass residual.
    var current = (float[])this._levels[actual - 1].Clone();
    var curW = this._widths[actual - 1];
    var curH = this._heights[actual - 1];

    for (var i = actual - 2; i >= 0; --i) {
      var fineW = this._widths[i];
      var fineH = this._heights[i];
      var expandedFloat = GaussianPyramid.ExpandOnceFloat(current, curW, curH, fineW, fineH);
      var residual = this._levels[i];
      var combined = new float[fineW * fineH * 4];
      for (var k = 0; k < combined.Length; ++k)
        combined[k] = expandedFloat[k] + residual[k];
      current = combined;
      curW = fineW;
      curH = fineH;
    }

    // Final write: clamp + encode to 8-bit BGRA. This is the ONLY clamp in the
    // reconstruction pipeline, applied once at the end.
    return _WriteFloat(current, curW, curH);
  }

  /// <summary>
  /// Round-trip helper for tests / external diagnostics: build, then reconstruct, returning
  /// the difference image (max abs ΔRGBA per pixel).
  /// </summary>
  public static int RoundTripMaxDelta(Bitmap source, int levels) {
    using var pyramid = (Bitmap)null!;
    var p = Build(source, levels);
    using var reconstructed = p.Reconstruct();
    var maxDelta = 0;
    var srcFloat = _ReadFloat(source);
    var rcFloat = _ReadFloat(reconstructed);
    for (var i = 0; i < srcFloat.Length; ++i) {
      var d = (int)Math.Abs(Math.Round(srcFloat[i]) - Math.Round(rcFloat[i]));
      if (d > maxDelta) maxDelta = d;
    }
    return maxDelta;
  }

  // ---- internals -------------------------------------------------------------

  private static unsafe float[] _ReadFloat(Bitmap source) {
    var w = source.Width;
    var h = source.Height;
    var bgra = source.PixelFormat == PixelFormat.Format32bppArgb ? source : _CloneToBgra(source);
    try {
      var data = bgra.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      try {
        var dst = new float[w * h * 4];
        for (var y = 0; y < h; ++y) {
          var row = (byte*)data.Scan0 + y * data.Stride;
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
        bgra.UnlockBits(data);
      }
    } finally {
      if (!ReferenceEquals(bgra, source))
        bgra.Dispose();
    }
  }

  private static Bitmap _CloneToBgra(Bitmap source) {
    var clone = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
    using var g = Graphics.FromImage(clone);
    g.DrawImage(source, 0, 0, source.Width, source.Height);
    return clone;
  }

  private static unsafe Bitmap _WriteFloat(float[] src, int w, int h) {
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

  private static float[] _SubtractToFloat(Bitmap fine, Bitmap expanded) {
    var fineF = _ReadFloat(fine);
    var expF = _ReadFloat(expanded);
    var dst = new float[fineF.Length];
    for (var i = 0; i < fineF.Length; ++i)
      dst[i] = fineF[i] - expF[i];
    return dst;
  }
}
