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
/// Peak Signal-to-Noise Ratio (PSNR) — measures the ratio between the maximum
/// possible pixel value (255 for 8-bit) and the RMS reconstruction error,
/// expressed in decibels.
/// </summary>
/// <remarks>
/// <para>
/// Formula: <c>PSNR = 10 · log10(MAX² / MSE)</c> where MSE is the mean squared
/// error across all R/G/B channels of all pixels.
/// </para>
/// <para>
/// Use case: standard objective quality metric for lossy compression and image
/// reconstruction. Higher is better; identical images give <see cref="double.PositiveInfinity"/>.
/// Typical "imperceptible" threshold is ~40 dB.
/// </para>
/// <para>
/// Complexity: O(W·H). On a 1024² image this is a single linear pass over
/// ~3M channel samples — sub-millisecond on modern hardware.
/// </para>
/// </remarks>
public static class Psnr {

  /// <summary>
  /// Computes PSNR between two same-size bitmaps over R, G, B channels.
  /// </summary>
  /// <param name="reference">The ground-truth bitmap.</param>
  /// <param name="candidate">The reconstructed / candidate bitmap.</param>
  /// <returns>PSNR in decibels. <see cref="double.PositiveInfinity"/> when the images are byte-identical.</returns>
  public static double Compute(Bitmap reference, Bitmap candidate) {
    Against.ArgumentIsNull(reference);
    Against.ArgumentIsNull(candidate);
    if (reference.Width != candidate.Width || reference.Height != candidate.Height)
      throw new ArgumentException(
        $"Dimensions differ: {reference.Width}x{reference.Height} vs {candidate.Width}x{candidate.Height}.");

    var w = reference.Width;
    var h = reference.Height;
    using var refLock = reference.Lock();
    using var canLock = candidate.Lock();

    double sumSq = 0;
    long n = 0;
    for (var y = 0; y < h; ++y)
    for (var x = 0; x < w; ++x) {
      var a = refLock[x, y];
      var b = canLock[x, y];
      var dr = a.R - b.R;
      var dg = a.G - b.G;
      var db = a.B - b.B;
      sumSq += dr * dr + dg * dg + db * db;
      n += 3;
    }

    if (n == 0)
      return double.PositiveInfinity;
    var mse = sumSq / n;
    return mse <= 0
      ? double.PositiveInfinity
      : 10.0 * Math.Log10(255.0 * 255.0 / mse);
  }
}
