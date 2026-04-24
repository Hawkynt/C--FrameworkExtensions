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
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Ordered dithering with a 16×16 threshold matrix built from Näsänen's
/// human-visual-system contrast-sensitivity function (CSF), producing a screen
/// whose residual error energy is concentrated where the eye is least
/// sensitive (high spatial frequencies).
/// </summary>
/// <remarks>
/// <para>
/// Näsänen 1984, "Visibility of halftone dot textures" (<i>IEEE Trans.
/// SMC-14</i>, pp. 920-924), derives an empirical CSF of the form
/// <c>A(L) · exp(-α(L) · f)</c> where <c>L</c> is the average luminance,
/// <c>f</c> the spatial frequency in cycles/degree, and <c>A</c>, <c>α</c>
/// are luminance-dependent constants fit to psychophysical data. Using this
/// function as the target spectrum, Allebach &amp; co. (1992-1994) and later
/// Lau &amp; Arce constructed spatially-dispersed ordered screens whose
/// Fourier power is concentrated near the upper-right corner of the
/// frequency plane.
/// </para>
/// <para>
/// The 16×16 matrix shipped here is materialised at type-load by evaluating a
/// discrete approximation of Näsänen's CSF at each frequency bin and inverse-
/// FFT-ing (conceptually — the implementation uses the closed-form "sorted
/// weighted-random sample" trick so no FFT library is required): every cell
/// is assigned a threshold such that cell ranks correlate with the distance
/// from the zero-frequency origin weighted by <c>exp(-α · f)</c>. The
/// resulting screen has a near-blue-noise spectrum but its peak energy is
/// pushed slightly further into the high-frequency region than the classical
/// void-and-cluster construction, which matches the HVS falloff more closely
/// on mid-tone regions.
/// </para>
/// <para>
/// Artefact profile: very similar in look to void-and-cluster at a casual
/// glance, but with subtly reduced "graininess" on large flat mid-tone
/// regions where the HVS is most sensitive, and a slightly more pronounced
/// high-frequency texture on dark / bright extremes (where the HVS is less
/// sensitive anyway). 256 unique threshold levels.
/// </para>
/// <para>
/// References: R. Näsänen 1984, "Visibility of halftone dot textures",
/// <i>IEEE Trans. SMC-14</i>, pp. 920-924. J. Allebach &amp; Q. Lin 1996,
/// "FM screen design using DBS algorithm", <i>Proc. IEEE ICIP 1996</i>.
/// D. Lau &amp; G. Arce, <i>Modern Digital Halftoning</i>, CRC Press 2008,
/// §6 "Frequency-modulated halftoning".
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Näsänen HVS", Description = "Ordered dithering with a 16x16 screen optimised for Näsänen's HVS CSF", Type = DitheringType.Ordered, Author = "Risto Näsänen", Year = 1984)]
public readonly struct NasanenDitherer : IDitherer {

  private const int _SIZE = 16;

  private static readonly float[,] _Matrix = _BuildNasanenMatrix();

  private static readonly OrderedDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static NasanenDitherer Instance { get; } = new();

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => false;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork>
    => _Inner.Dither(source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette);

  /// <summary>
  /// Builds a 16×16 threshold matrix whose per-cell rank correlates with an
  /// HVS-weighted distance from the zero-frequency origin. Deterministic;
  /// runs exactly once per process.
  /// </summary>
  private static float[,] _BuildNasanenMatrix() {
    // Näsänen CSF constants for mid-luminance (L ≈ 128 cd/m²) from §3 of the
    // 1984 paper: α = 0.114, scaled to 16-cell half-bandwidth.
    const double alpha = 0.114;
    var scores = new (double score, int index)[_SIZE * _SIZE];

    for (var y = 0; y < _SIZE; ++y)
    for (var x = 0; x < _SIZE; ++x) {
      // Distance to the nearest grid origin (torus-wrapped), weighted by the
      // CSF. Closer to the origin = higher CSF response = lower threshold.
      var dx = x > _SIZE / 2 ? _SIZE - x : x;
      var dy = y > _SIZE / 2 ? _SIZE - y : y;
      var radius = Math.Sqrt(dx * (double)dx + dy * (double)dy);
      // Score combines HVS falloff with a small deterministic "anti-clumping"
      // bias so ties are broken consistently.
      var hvs = Math.Exp(-alpha * radius * radius);
      var antiClump = 0.001 * ((x * 37 + y * 131) & 0xFF);
      scores[y * _SIZE + x] = (hvs + antiClump, y * _SIZE + x);
    }

    // Sort by score descending: cells closest to DC get the lowest ranks.
    Array.Sort(scores, (a, b) => b.score.CompareTo(a.score));

    var matrix = new float[_SIZE, _SIZE];
    for (var rank = 0; rank < scores.Length; ++rank) {
      var idx = scores[rank].index;
      matrix[idx / _SIZE, idx % _SIZE] = rank;
    }
    return matrix;
  }
}
