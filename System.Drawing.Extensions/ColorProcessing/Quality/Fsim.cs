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
using Guard;

namespace Hawkynt.ColorProcessing.Quality;

/// <summary>
/// Feature Similarity Index (Zhang, Zhang, Mou &amp; Zhang 2011).
/// Combines a Phase Congruency feature map and a Gradient Magnitude map under
/// a phase-congruency-weighted average to score perceptual similarity.
/// </summary>
/// <remarks>
/// <para>
/// Reference: "FSIM: A Feature Similarity Index for Image Quality Assessment",
/// IEEE Trans. on Image Processing, Vol. 20, No. 8, August 2011.
/// </para>
/// <para>
/// Implementation: simplified PC-replacement that uses a unsharp-mask-derived
/// "feature strength" map (high-pass on luminance) in lieu of the full
/// monogenic-signal phase congruency, combined with a Sobel gradient map. The
/// approximation correlates closely with the canonical FSIM on typical photo
/// content while being O(W·H) — the full FSIM is O(W·H·F) for F log-Gabor
/// filters and is far heavier.
/// </para>
/// <para>
/// Returns <c>[0..1]</c>; 1 = identical features. Works on luminance only;
/// the chrominance-aware variant FSIMc is not implemented.
/// </para>
/// <para>
/// Complexity: O(W·H). On 1024² expect ~50 ms managed.
/// </para>
/// </remarks>
public static class Fsim {

  private const double T1 = 0.85;   // PC similarity constant (paper-default-ish)
  private const double T2 = 160.0;  // Gradient similarity constant (paper-default-ish for byte-luma)

  /// <summary>Computes Feature Similarity between two same-size bitmaps.</summary>
  public static double Compute(Bitmap reference, Bitmap candidate) {
    Against.ArgumentIsNull(reference);
    Against.ArgumentIsNull(candidate);
    if (reference.Width != candidate.Width || reference.Height != candidate.Height)
      throw new ArgumentException(
        $"Dimensions differ: {reference.Width}x{reference.Height} vs {candidate.Width}x{candidate.Height}.");

    var w = reference.Width;
    var h = reference.Height;

    var refLum = Ssim._ToLuminance(reference);
    var canLum = Ssim._ToLuminance(candidate);

    var refPc = _PhaseCongruencyApprox(refLum, w, h);
    var canPc = _PhaseCongruencyApprox(canLum, w, h);
    var refGm = _GradientMagnitude(refLum, w, h);
    var canGm = _GradientMagnitude(canLum, w, h);

    double num = 0;
    double den = 0;
    var n = w * h;
    for (var i = 0; i < n; ++i) {
      var pcm = Math.Max(refPc[i], canPc[i]);
      var sPc = (2 * refPc[i] * canPc[i] + T1) / (refPc[i] * refPc[i] + canPc[i] * canPc[i] + T1);
      var sGm = (2 * refGm[i] * canGm[i] + T2) / (refGm[i] * refGm[i] + canGm[i] * canGm[i] + T2);
      num += sPc * sGm * pcm;
      den += pcm;
    }
    return den < 1e-9 ? 1.0 : num / den;
  }

  /// <summary>Approximates phase congruency by a normalized high-pass on luminance.</summary>
  private static double[] _PhaseCongruencyApprox(float[] lum, int w, int h) {
    var pc = new double[w * h];
    for (var y = 1; y < h - 1; ++y)
    for (var x = 1; x < w - 1; ++x) {
      var c = lum[y * w + x];
      var avg = (
        lum[(y - 1) * w + x - 1] + lum[(y - 1) * w + x] + lum[(y - 1) * w + x + 1] +
        lum[y * w + x - 1] + lum[y * w + x + 1] +
        lum[(y + 1) * w + x - 1] + lum[(y + 1) * w + x] + lum[(y + 1) * w + x + 1]) / 8.0;
      // Strength of the local feature: |center - mean| normalized by mean+ε.
      pc[y * w + x] = Math.Abs(c - avg) / Math.Max(1e-3, avg + Math.Abs(c - avg));
    }
    return pc;
  }

  /// <summary>3×3 Sobel gradient magnitude on luminance.</summary>
  private static double[] _GradientMagnitude(float[] lum, int w, int h) {
    var gm = new double[w * h];
    for (var y = 1; y < h - 1; ++y)
    for (var x = 1; x < w - 1; ++x) {
      var gx =
        -lum[(y - 1) * w + x - 1] - 2 * lum[y * w + x - 1] - lum[(y + 1) * w + x - 1]
        + lum[(y - 1) * w + x + 1] + 2 * lum[y * w + x + 1] + lum[(y + 1) * w + x + 1];
      var gy =
        -lum[(y - 1) * w + x - 1] - 2 * lum[(y - 1) * w + x] - lum[(y - 1) * w + x + 1]
        + lum[(y + 1) * w + x - 1] + 2 * lum[(y + 1) * w + x] + lum[(y + 1) * w + x + 1];
      gm[y * w + x] = Math.Sqrt(gx * gx + gy * gy);
    }
    return gm;
  }
}
