#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.

#endregion

using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Integer-only Gaussian blur via three iterated <see cref="BitmapBoxBlurIntExtensions.BoxBlurInt"/>
/// passes. Three box passes approximate a true Gaussian to within ~3% RMS error
/// (Wells 1986; Kovesi 2010 "Fast Almost-Gaussian Filtering"). Combines the 50×+
/// speedup of the int sliding-window box with a near-Gaussian frequency response.
/// </summary>
/// <remarks>
/// <para>For a target σ (Gaussian standard deviation in pixels), the optimal three-box
/// radii satisfy <c>3·variance(box(r)) = σ²</c>. The standard rounded-integer
/// approximation distributes the radii as evenly as possible across the three
/// passes (Kovesi's <c>boxesForGauss</c> recipe).</para>
/// <para>Tradeoff: gamma-naive (operates on sRGB byte values directly), matching the
/// lib's existing <c>GaussianBlur</c> Fast quality preset. For gamma-correct
/// rendering use <c>ApplyFilter(GaussianBlur, ScalerQuality.HighQuality)</c>.</para>
/// </remarks>
public static class BitmapGaussianBlurIntExtensions {

  /// <param name="this">The source bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Applies an integer-only Gaussian blur with the given σ (standard deviation
    /// in pixels). σ ≤ 0 returns a clone.
    /// </summary>
    /// <param name="sigma">Gaussian standard deviation in pixels (e.g., σ = 1.5).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap GaussianBlurInt(double sigma) {
      if (sigma <= 0)
        return (Bitmap)@this.Clone();

      // Kovesi's three-box recipe: distribute radii so total variance = σ².
      var (r1, r2, r3) = _BoxRadiiForGaussian(sigma);

      // Iterate three box-blur passes. Reuse intermediate bitmaps to limit GC pressure.
      var pass1 = @this.BoxBlurInt(r1);
      try {
        var pass2 = pass1.BoxBlurInt(r2);
        try {
          return pass2.BoxBlurInt(r3);
        } finally {
          pass2.Dispose();
        }
      } finally {
        pass1.Dispose();
      }
    }
  }

  /// <summary>
  /// Computes the three integer box-blur radii whose combined variance approximates
  /// a Gaussian with the given σ. Distributes the rounded radii so that the first
  /// <paramref name="m"/> passes use radius <c>(wl-1)/2</c> and the remaining use
  /// <c>(wu-1)/2</c>, matching Wells's optimal-rounding formula.
  /// </summary>
  /// <remarks>
  /// Reference: Peter Kovesi, "Fast Almost-Gaussian Filtering", Proc. DICTA 2010.
  /// <c>https://www.peterkovesi.com/papers/FastGaussianSmoothing.pdf</c>
  /// </remarks>
  private static (int r1, int r2, int r3) _BoxRadiiForGaussian(double sigma) {
    const int N = 3;
    // Ideal box width (real-valued): wIdeal = sqrt((12σ²/N) + 1).
    var wIdeal = Math.Sqrt(12.0 * sigma * sigma / N + 1.0);

    // Lower odd integer ≤ wIdeal.
    var wl = (int)Math.Floor(wIdeal);
    if ((wl & 1) == 0) wl -= 1;
    if (wl < 1) wl = 1;
    var wu = wl + 2;

    // m = how many passes use wl (the rest use wu) so that total variance matches σ²
    // as closely as possible. Wells's formula:
    //   m = round((12σ² − N·wl² − 4·N·wl − 3·N) / (−4·wl − 4))
    var mIdeal = (12.0 * sigma * sigma - N * (double)wl * wl - 4.0 * N * wl - 3.0 * N)
               / (-4.0 * wl - 4.0);
    var m = (int)Math.Round(mIdeal);
    if (m < 0) m = 0;
    if (m > N) m = N;

    var r_low = (wl - 1) / 2;
    var r_high = (wu - 1) / 2;
    return (
      r1: 0 < m ? r_low : r_high,
      r2: 1 < m ? r_low : r_high,
      r3: 2 < m ? r_low : r_high
    );
  }
}
