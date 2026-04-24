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

using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Ordered dithering using a 4×4 screen constructed by tiling a 2×2 Bayer
/// matrix across a checkerboard phase — even cells use the base 2×2 Bayer
/// sequence, odd cells use its two-step phase rotation, producing an 8-level
/// screen with a visible "woven" structure in flat regions.
/// </summary>
/// <remarks>
/// <para>
/// Don P. Mitchell's 1991 paper "Generating Antialiased Images at Low Sampling
/// Densities" (<i>SIGGRAPH '91</i>) discusses several small-matrix dither
/// constructions for low-bit-depth rasters. A characteristic 4×4 construction
/// tiles a 2×2 Bayer pattern onto a checkerboard: the four odd-parity cells
/// use the base <c>{0, 2, 3, 1}</c> sequence; the four even-parity cells use
/// a phase-rotated <c>{2, 0, 1, 3}</c> sequence. The result is a 4×4 screen
/// with exactly 8 distinct threshold levels (instead of Bayer-4's 16),
/// producing a more pronounced woven / textile look.
/// </para>
/// <para>
/// Laid out explicitly:
/// </para>
/// <code>
///      0   6   1   7
///      4   2   5   3
///      1   7   0   6
///      5   3   4   2
/// </code>
/// <para>
/// Artefact profile: the two interleaved 2×2 phases create a visible woven /
/// basket-weave texture in mid-tones that sits halfway between Bayer-2 and
/// Bayer-4. Particularly useful for retro pixel-art styling where an overtly
/// "dithery" look is wanted. 8 unique threshold levels.
/// </para>
/// <para>
/// References: D. P. Mitchell 1991, "Generating antialiased images at low
/// sampling densities", <i>SIGGRAPH '91 Proceedings</i>. Underlying Bayer
/// construction: B. Bayer 1973, "An optimum method for two-level rendition of
/// continuous-tone pictures", IEEE ICC vol. 1.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Mitchell Checkerboard", Description = "4x4 checkerboard-phase tiled 2x2 Bayer screen (8 levels)", Type = DitheringType.Ordered, Author = "Don P. Mitchell", Year = 1991)]
public readonly struct MitchellCheckerboardDitherer : IDitherer {

  // Two 2x2 Bayer phases interleaved on a checkerboard. Even-parity cells
  // ((x^y)&1 == 0) use the base sequence {0,2,3,1} mapped to even levels;
  // odd-parity cells use its phase rotation {2,0,1,3} mapped to odd levels.
  private static readonly float[,] _Matrix = {
    { 0, 6, 1, 7 },
    { 4, 2, 5, 3 },
    { 1, 7, 0, 6 },
    { 5, 3, 4, 2 },
  };

  private static readonly OrderedDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static MitchellCheckerboardDitherer Instance { get; } = new();

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
}
