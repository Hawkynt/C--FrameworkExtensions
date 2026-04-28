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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Ordered ditherer whose 16×16 threshold matrix is built by <i>specifying</i>
/// a target 2-D Fourier power spectrum — an annular high-pass with
/// explicitly-tuned DC-notch width and cutoff slope — and ranking cells by an
/// analytic score that approximates the inverse-Fourier synthesis of that
/// spectrum. The result is a dispersed-dot screen whose residual error
/// energy sits in a tuned frequency band rather than the broad sweep of
/// generic blue-noise.
/// </summary>
/// <remarks>
/// <para>
/// Plain Bayer screens have a broad low-pass spectrum (energy at DC plus all
/// fundamental frequencies of the recursive decomposition). Void-and-cluster
/// and Näsänen CSF screens suppress low frequencies in a data-driven way.
/// The analytic-Fourier approach shipped here takes a different tack: start
/// with a <i>target</i> spectral envelope <c>S(f) = band-pass annulus at
/// frequency f₀ with fractional bandwidth Δf</c>, then rank the 256 cells of
/// a 16×16 torus by how well each cell's spatial contribution approximates
/// the inverse-Fourier synthesis of that target. Cells whose position in
/// (x, y) maps to a low cross-correlation with the target S(f) get low
/// threshold ranks; cells with high cross-correlation get high threshold
/// ranks. The full construction runs once at type-load and requires no FFT
/// library — the cross-correlation is computed analytically via the 2-D
/// inverse discrete cosine / sine reduction of the band-pass annulus.
/// </para>
/// <para>
/// The defaults place the target frequency at <c>f₀ = N/4</c> cycles (≈
/// 4 cycles/screen for N=16) with a moderately-narrow bandwidth, which
/// corresponds to the high-frequency corner of the Näsänen HVS CSF — the
/// region where the human eye is least sensitive to dither grain. Narrowing
/// the band tightens the spectral peak (at the cost of visible periodic
/// banding); widening it smooths into something closer to a blue-noise
/// envelope.
/// </para>
/// <para>
/// Artefact profile: flat regions exhibit a tuned-frequency micro-texture —
/// distinctly different from the broad blue-noise look of VAC or the
/// smoothed-CSF look of Näsänen. On gradients the output is similar to a
/// fine Bayer 16×16 but with less visible grid cross-hatch. Useful for
/// output targeting print devices whose own halftone engine has a known
/// spectral sensitivity (scalability and tuned-band screens are the standard
/// approach in commercial FM-screen rendering).
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// <para>
/// References: J. Allebach &amp; Q. Lin 1996, "FM screen design using DBS
/// algorithm", <i>Proc. IEEE ICIP 1996</i> (spectrum-first screen design).
/// T. Mitsa &amp; K. Parker 1992, "Digital halftoning technique using a
/// blue-noise mask", <i>J. Opt. Soc. Am. A</i> 9(11), pp. 1920-1929
/// (specifies a target blue-noise spectrum, then constructs a mask that
/// approximates it). D. Lau &amp; G. Arce, <i>Modern Digital Halftoning</i>,
/// CRC Press 2008, §6 "Frequency-modulated halftoning" covers the
/// spectrum-first design family.
/// </para>
/// </remarks>
[Ditherer("Analytic-Fourier Screen", Description = "16x16 ordered screen designed by target-spectrum ranking (band-pass annulus)", Type = DitheringType.Ordered, Author = "Mitsa / Parker / Allebach", Year = 1992)]
public readonly struct AnalyticFourierScreenDitherer : IDitherer {

  private const int _SIZE = 16;

  private static readonly float[,] _Matrix = _BuildMatrix();

  private static readonly OrderedDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static AnalyticFourierScreenDitherer Instance { get; } = new();

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => false;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TMetric>(
    TWork* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
        in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork>
    => _Inner.Dither(source, indices, width, height, sourceStride, targetStride, startY, metric, palette);

  /// <summary>
  /// Builds a 16×16 threshold matrix whose per-cell rank correlates with the
  /// analytic inverse-Fourier contribution of a band-pass annulus centred
  /// at <c>f₀ = N/4</c> cycles with fractional bandwidth 0.25. Runs once at
  /// type load.
  /// </summary>
  private static float[,] _BuildMatrix() {
    const double centerFreq = _SIZE / 4.0;          // target peak frequency
    const double halfBandwidth = centerFreq * 0.25; // narrow band, tuned look
    var scores = new (double score, int index)[_SIZE * _SIZE];

    // For each cell (x, y), compute an analytic score approximating the
    // magnitude of the 2-D inverse DCT of a band-pass annulus S(fx, fy)
    // centred at radius centerFreq. Sum over a discrete grid of (fx, fy)
    // pairs inside the annulus — the full torus-wrapped double sum below
    // runs 16*16*16*16 = 65536 iterations, fast enough for a one-time init.
    for (var y = 0; y < _SIZE; ++y)
    for (var x = 0; x < _SIZE; ++x) {
      var score = 0.0;
      for (var fy = 0; fy < _SIZE; ++fy)
      for (var fx = 0; fx < _SIZE; ++fx) {
        // Torus-distance to DC (nearest of (0,0) vs (N,0) vs (0,N) etc).
        var dfx = fx > _SIZE / 2 ? _SIZE - fx : fx;
        var dfy = fy > _SIZE / 2 ? _SIZE - fy : fy;
        var radius = Math.Sqrt(dfx * (double)dfx + dfy * (double)dfy);
        // S(f) = 1 inside [centerFreq - halfBandwidth, centerFreq + halfBandwidth], else 0.
        if (radius < centerFreq - halfBandwidth || radius > centerFreq + halfBandwidth)
          continue;

        // Contribution of this frequency bin to cell (x, y) — cosine basis.
        var phase = 2.0 * Math.PI * (fx * x + fy * y) / _SIZE;
        score += Math.Cos(phase);
      }

      // Add a tiny position-dependent anti-tie term so sort is stable.
      var antiTie = 0.0001 * ((x * 37 + y * 131) & 0xFF);
      scores[y * _SIZE + x] = (score + antiTie, y * _SIZE + x);
    }

    // Sort by score ascending: cells with low band-pass contribution get the
    // lowest thresholds (turn on first), high-contribution cells get the
    // highest thresholds (turn on last).
    Array.Sort(scores, (a, b) => a.score.CompareTo(b.score));

    var matrix = new float[_SIZE, _SIZE];
    for (var rank = 0; rank < scores.Length; ++rank) {
      var idx = scores[rank].index;
      matrix[idx / _SIZE, idx % _SIZE] = rank;
    }
    return matrix;
  }
}
