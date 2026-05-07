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
/// Multi-Scale Retinex tone mapping — Jobson, Rahman &amp; Woodell 1997.
/// </summary>
/// <remarks>
/// <para>Retinex models human vision's lightness-perception by subtracting the surround
/// (low-pass-filtered version of the image) from each pixel in log-space — what's left is
/// the local <i>reflectance</i>, free of illumination gradients. The "multi-scale" form
/// combines several Gaussian surrounds at different sigmas, capturing dynamic-range
/// compression at fine detail and shading correction at coarse scales simultaneously.</para>
/// <para>Distinct from the existing <c>Reinhard</c> / <c>Drago</c> / <c>Mantiuk</c> tone
/// mappers in this library: those are global or local-but-single-scale operators; MSR
/// operates simultaneously across multiple scales using the
/// <see cref="GaussianPyramid"/> infrastructure shipped alongside this class.</para>
/// <para>Uses the pyramid infrastructure: at each chosen pyramid level, the Gaussian-
/// blurred surround is computed by walking down then back up the pyramid (so the σ values
/// are dyadic: 2, 4, 8, ...). Three default scales (levels 1, 3, 5) approximate the
/// classical 15 / 80 / 250-pixel σ values of the Jobson paper.</para>
/// <para>Reference: Jobson, Rahman &amp; Woodell 1997, "A multiscale retinex for bridging
/// the gap between color images and the human observation of scenes", IEEE Trans. Image
/// Processing 6(7):965–976.</para>
/// </remarks>
public static class MultiScaleRetinex {

  /// <summary>Default gain applied to the log-difference output before sRGB clamping.</summary>
  public const float DefaultGain = 30f;

  /// <summary>Default offset added after gain (centres output around mid-gray).</summary>
  public const float DefaultOffset = 128f;

  /// <summary>
  /// Applies Multi-Scale Retinex to the source bitmap and returns a new bitmap.
  /// </summary>
  /// <param name="source">Non-null input. Must be at least 32×32 to support 5 pyramid levels.</param>
  /// <param name="gain">Output gain on the (centred) log-difference signal. Higher = stronger
  /// local-contrast enhancement.</param>
  /// <param name="offset">Output offset added after gain (in 0..255 byte space).</param>
  public static Bitmap Apply(Bitmap source, float gain = DefaultGain, float offset = DefaultOffset) {
    ArgumentNullException.ThrowIfNull(source);

    var pyramid = GaussianPyramid.Build(source, 6);
    try {
      var w = source.Width;
      var h = source.Height;

      // Walk the chosen levels back up to source resolution. Levels 1, 3, 5 give σ ≈ 2,
      // 8, 32 in source-pixel terms — captures fine, mid, and coarse surround.
      using var s1 = _ExpandToSource(pyramid[1], w, h);
      using var s3 = _ExpandToSource(pyramid[Math.Min(3, pyramid.Length - 1)], w, h);
      using var s5 = _ExpandToSource(pyramid[Math.Min(5, pyramid.Length - 1)], w, h);

      // For each pixel, MSR(x) = (1/3) · Σ_k (log(I + 1) − log(S_k + 1)) , then gain + offset.
      return _Combine(source, s1, s3, s5, gain, offset);
    } finally {
      foreach (var bmp in pyramid)
        bmp.Dispose();
    }
  }

  // Walk a coarse-pyramid level back up to source resolution by repeated 2× expand.
  private static Bitmap _ExpandToSource(Bitmap level, int targetW, int targetH) {
    Bitmap current = (Bitmap)level.Clone();
    while (current.Width < targetW || current.Height < targetH) {
      var nextW = Math.Min(targetW, current.Width * 2);
      var nextH = Math.Min(targetH, current.Height * 2);
      var expanded = GaussianPyramid.ExpandOnce(current, nextW, nextH);
      current.Dispose();
      current = expanded;
    }
    return current;
  }

  private static unsafe Bitmap _Combine(Bitmap source, Bitmap s1, Bitmap s3, Bitmap s5, float gain, float offset) {
    var w = source.Width;
    var h = source.Height;
    var dst = new Bitmap(w, h, PixelFormat.Format32bppArgb);

    var srcD = source.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    var s1D = s1.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    var s3D = s3.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    var s5D = s5.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    var dstD = dst.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try {
      for (var y = 0; y < h; ++y) {
        var sRow = (byte*)srcD.Scan0 + y * srcD.Stride;
        var s1R = (byte*)s1D.Scan0 + y * s1D.Stride;
        var s3R = (byte*)s3D.Scan0 + y * s3D.Stride;
        var s5R = (byte*)s5D.Scan0 + y * s5D.Stride;
        var dRow = (byte*)dstD.Scan0 + y * dstD.Stride;
        for (var x = 0; x < w; ++x) {
          var b = sRow[x * 4 + 0];
          var g = sRow[x * 4 + 1];
          var r = sRow[x * 4 + 2];
          var a = sRow[x * 4 + 3];

          // Per-channel log-difference at three scales, averaged.
          var rMsr = _Msr(r, s1R[x * 4 + 2], s3R[x * 4 + 2], s5R[x * 4 + 2]);
          var gMsr = _Msr(g, s1R[x * 4 + 1], s3R[x * 4 + 1], s5R[x * 4 + 1]);
          var bMsr = _Msr(b, s1R[x * 4 + 0], s3R[x * 4 + 0], s5R[x * 4 + 0]);

          dRow[x * 4 + 0] = _Clamp(bMsr * gain + offset);
          dRow[x * 4 + 1] = _Clamp(gMsr * gain + offset);
          dRow[x * 4 + 2] = _Clamp(rMsr * gain + offset);
          dRow[x * 4 + 3] = a;
        }
      }
    } finally {
      source.UnlockBits(srcD);
      s1.UnlockBits(s1D);
      s3.UnlockBits(s3D);
      s5.UnlockBits(s5D);
      dst.UnlockBits(dstD);
    }
    return dst;
  }

  // Jobson 1997 eq. (2): R_i(x,y) = Σ_n W_n · [log I_i(x,y) − log(F_n * I_i)(x,y)].
  // Operates on NORMALISED intensity I ∈ (0, 1] so the log is unitless and bit-depth
  // independent. `eps = 1/255` is the unit-byte floor preventing log(0). Equal-weight
  // averaging W_n = 1/3 across scales is the simplification used in the public code
  // distributions of MSR (e.g. NASA's MSRCR Photoshop plugin); the paper allows
  // unequal W_n for tuning per-scale contribution but most implementations use 1/N.
  private static float _Msr(byte i, byte s1, byte s3, byte s5) {
    const float Eps = 1f / 255f;
    var lI = MathF.Log(i / 255f + Eps);
    return (1f / 3f) * (
      (lI - MathF.Log(s1 / 255f + Eps)) +
      (lI - MathF.Log(s3 / 255f + Eps)) +
      (lI - MathF.Log(s5 / 255f + Eps))
    );
  }

  private static byte _Clamp(float v) {
    if (v < 0f) return 0;
    if (v > 255f) return 255;
    return (byte)(v + 0.5f);
  }
}
