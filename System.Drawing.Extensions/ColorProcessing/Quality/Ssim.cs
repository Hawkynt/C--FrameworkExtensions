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
/// Structural Similarity Index (Wang, Bovik, Sheikh &amp; Simoncelli 2004).
/// Combines luminance, contrast and structure terms over local windows; values
/// in <c>[-1..1]</c> with 1 = perfect similarity, 0 = no structural correlation.
/// </summary>
/// <remarks>
/// <para>
/// Reference: "Image Quality Assessment: From Error Visibility to Structural
/// Similarity", IEEE Trans. on Image Processing, Vol. 13, No. 4, April 2004.
/// </para>
/// <para>
/// Implementation: standard 8×8 sliding-window mean SSIM (the original
/// Wang-Bovik formulation used a Gaussian; the box-window variant gives a
/// nearly-identical score and is the textbook simplification). Operates on
/// luminance computed via Rec. 601.
/// </para>
/// <para>Constants: <c>K1=0.01, K2=0.03, L=255</c> giving
/// <c>C1=(K1·L)², C2=(K2·L)²</c>.</para>
/// <para>
/// Complexity: O(W·H·K²) for window-size K (default 8). On 1024² with K=8
/// expect ~50–80 ms managed.
/// </para>
/// </remarks>
public static class Ssim {

  /// <summary>Computes the mean SSIM over <paramref name="reference"/> vs <paramref name="candidate"/> on luminance.</summary>
  public static double Compute(Bitmap reference, Bitmap candidate) => Compute(reference, candidate, 8);

  /// <summary>Computes mean SSIM with a configurable window size (must be ≥ 2).</summary>
  public static double Compute(Bitmap reference, Bitmap candidate, int windowSize) {
    Against.ArgumentIsNull(reference);
    Against.ArgumentIsNull(candidate);
    if (reference.Width != candidate.Width || reference.Height != candidate.Height)
      throw new ArgumentException(
        $"Dimensions differ: {reference.Width}x{reference.Height} vs {candidate.Width}x{candidate.Height}.");
    Against.CountBelow(windowSize, 2);

    var w = reference.Width;
    var h = reference.Height;
    if (w < windowSize || h < windowSize)
      windowSize = Math.Min(w, h);

    var refLum = _ToLuminance(reference);
    var canLum = _ToLuminance(candidate);

    return _MeanSsim(refLum, canLum, w, h, windowSize);
  }

  internal static float[] _ToLuminance(Bitmap bmp) {
    var w = bmp.Width;
    var h = bmp.Height;
    var lum = new float[w * h];
    using var lk = bmp.Lock();
    for (var y = 0; y < h; ++y)
    for (var x = 0; x < w; ++x) {
      var c = lk[x, y];
      lum[y * w + x] = 0.299f * c.R + 0.587f * c.G + 0.114f * c.B;
    }
    return lum;
  }

  internal static double _MeanSsim(float[] refLum, float[] canLum, int w, int h, int windowSize) {
    const double K1 = 0.01;
    const double K2 = 0.03;
    const double L = 255.0;
    const double C1 = (K1 * L) * (K1 * L);
    const double C2 = (K2 * L) * (K2 * L);

    double total = 0;
    long count = 0;
    var n = windowSize * windowSize;

    for (var y = 0; y + windowSize <= h; ++y)
    for (var x = 0; x + windowSize <= w; ++x) {
      double meanR = 0, meanC = 0;
      for (var dy = 0; dy < windowSize; ++dy)
      for (var dx = 0; dx < windowSize; ++dx) {
        var idx = (y + dy) * w + x + dx;
        meanR += refLum[idx];
        meanC += canLum[idx];
      }
      meanR /= n;
      meanC /= n;

      double varR = 0, varC = 0, cov = 0;
      for (var dy = 0; dy < windowSize; ++dy)
      for (var dx = 0; dx < windowSize; ++dx) {
        var idx = (y + dy) * w + x + dx;
        var rr = refLum[idx] - meanR;
        var cc = canLum[idx] - meanC;
        varR += rr * rr;
        varC += cc * cc;
        cov += rr * cc;
      }
      varR /= n - 1;
      varC /= n - 1;
      cov /= n - 1;

      var num = (2 * meanR * meanC + C1) * (2 * cov + C2);
      var den = (meanR * meanR + meanC * meanC + C1) * (varR + varC + C2);
      total += num / den;
      ++count;
    }

    return count == 0 ? 1.0 : total / count;
  }
}
