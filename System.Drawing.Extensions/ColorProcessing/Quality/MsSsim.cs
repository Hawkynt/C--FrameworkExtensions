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
/// Multi-Scale SSIM (Wang, Simoncelli &amp; Bovik 2003) — applies SSIM at
/// successive 2× downsampled scales and combines them with empirically tuned
/// exponents <c>α=(0.0448, 0.2856, 0.3001, 0.2363, 0.1333)</c>.
/// </summary>
/// <remarks>
/// <para>
/// Reference: "Multi-scale structural similarity for image quality assessment",
/// Asilomar Conf. on Signals, Systems and Computers, 2003. The exponents above
/// are the canonical 5-scale weights from the paper.
/// </para>
/// <para>
/// Implementation: each scale is produced by 2×2 box-decimation (matches the
/// downsampling commonly used in MS-SSIM reference implementations). At each
/// scale we compute the contrast and structure components using <see cref="Ssim"/>;
/// the luminance term is taken only at the finest scale.
/// </para>
/// <para>
/// Complexity: O(W·H·K²) — same as SSIM but ~1.33× because of the geometric
/// pyramid. Returns a value in <c>[0..1]</c>; 1 = perfect.
/// </para>
/// </remarks>
public static class MsSsim {

  private static readonly double[] _Exponents = [0.0448, 0.2856, 0.3001, 0.2363, 0.1333];

  /// <summary>Computes MS-SSIM with a default 8-pixel window per scale.</summary>
  public static double Compute(Bitmap reference, Bitmap candidate) => Compute(reference, candidate, 8);

  /// <summary>Computes MS-SSIM with a configurable per-scale window size.</summary>
  public static double Compute(Bitmap reference, Bitmap candidate, int windowSize) {
    Against.ArgumentIsNull(reference);
    Against.ArgumentIsNull(candidate);
    if (reference.Width != candidate.Width || reference.Height != candidate.Height)
      throw new ArgumentException(
        $"Dimensions differ: {reference.Width}x{reference.Height} vs {candidate.Width}x{candidate.Height}.");
    Against.CountBelow(windowSize, 2);

    var refLum = Ssim._ToLuminance(reference);
    var canLum = Ssim._ToLuminance(candidate);
    var w = reference.Width;
    var h = reference.Height;

    var product = 1.0;
    for (var scale = 0; scale < _Exponents.Length; ++scale) {
      // At each scale, pick the smaller window if image too small.
      var win = Math.Min(windowSize, Math.Min(w, h));
      if (win < 2) break;
      var s = Ssim._MeanSsim(refLum, canLum, w, h, win);
      // Numerical safety: SSIM should be in [-1..1]; clamp to [eps..1].
      if (s < 1e-6) s = 1e-6;
      product *= Math.Pow(s, _Exponents[scale]);

      // Downsample by 2 for next scale (unless we'd run out of data).
      if (w < 4 || h < 4) break;
      var nw = w / 2;
      var nh = h / 2;
      refLum = _Downsample(refLum, w, h);
      canLum = _Downsample(canLum, w, h);
      w = nw;
      h = nh;
    }

    return product;
  }

  private static float[] _Downsample(float[] src, int w, int h) {
    var nw = w / 2;
    var nh = h / 2;
    var dst = new float[nw * nh];
    for (var y = 0; y < nh; ++y)
    for (var x = 0; x < nw; ++x) {
      var sx = x * 2;
      var sy = y * 2;
      dst[y * nw + x] = 0.25f * (
        src[sy * w + sx] +
        src[sy * w + sx + 1] +
        src[(sy + 1) * w + sx] +
        src[(sy + 1) * w + sx + 1]);
    }
    return dst;
  }
}
