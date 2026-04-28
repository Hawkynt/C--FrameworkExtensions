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
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

// ---------------------------------------------------------------------------
// Magic-square ordered dithering family.
//
// The N×N doubly-even magic square (Dürer 4×4 and its 8×8 "X-method" extension)
// produces thresholds whose row, column and diagonal sums all match, so every
// axis is equally weighted. Compared to Bayer's recursive bit-interleave this
// suppresses the characteristic 45° cross-hatch grain and yields a grain that
// looks closer to checker-micro-texture. Each size is a thin wrapper that
// delegates to the shared <see cref="OrderedDitherer"/> kernel with a frozen
// matrix; no per-instance state, fully parallel-friendly, deterministic.
//
// Consolidated file (previously one .cs per size): all magic-square size
// variants live here so the matrices and documentation stay side-by-side.
// Each size remains an independent public struct so the source generator
// still emits a distinct registry entry per size — no user-facing name is
// changed by the merge.
// ---------------------------------------------------------------------------

/// <summary>
/// Ordered dithering with a 4×4 doubly-even magic-square threshold matrix —
/// an alternative dispersed-dot pattern to Bayer 4×4 with equalised row, column
/// and diagonal sums.
/// </summary>
/// <remarks>
/// <para>
/// Uses the classical 4×4 "Dürer" doubly-even magic square (famously depicted
/// in Albrecht Dürer's 1514 engraving <i>Melencolia I</i>), shifted to the
/// 0-based range [0, 15]. Every row, every column and both main diagonals sum
/// to 30 (= 6·(4²-1)/2), which guarantees a balanced threshold distribution
/// along every axis and suppresses the diagonal striping that Bayer's pure
/// bit-interleave pattern produces on certain gradients.
/// </para>
/// <para>
/// Typical artefact profile: uniform grain with no dominant diagonal, very
/// close to Bayer 4×4 in tonal resolution (16 levels) but with a different
/// spectral fingerprint that can look cleaner on near-horizontal or near-
/// vertical gradients. Commonly used in retro 8-bit / 16-bit rendering where
/// Bayer's diagonal grain is undesirable.
/// </para>
/// <para>
/// References:
/// <a href="https://en.wikipedia.org/wiki/Magic_square">Magic square
/// (Wikipedia)</a>,
/// <a href="https://en.wikipedia.org/wiki/Melencolia_I">Melencolia I</a>.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Magic Square 4x4", Description = "Ordered dithering using Dürer's 4x4 doubly-even magic square", Type = DitheringType.Ordered, Author = "Albrecht Dürer", Year = 1514)]
public readonly struct MagicSquare4x4Ditherer : IDitherer {

  // Dürer's 16/3/2/13 / 5/10/11/8 / 9/6/7/12 / 4/15/14/1 magic square,
  // shifted by -1 so values sit in [0,15]. Every row, column and main
  // diagonal sums to 30. OrderedDitherer normalises to [-0.5, 0.5].
  private static readonly float[,] _Matrix = {
    { 15,  2,  1, 12 },
    {  4,  9, 10,  7 },
    {  8,  5,  6, 11 },
    {  3, 14, 13,  0 },
  };

  private static readonly OrderedDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static MagicSquare4x4Ditherer Instance { get; } = new();

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
}

/// <summary>
/// Ordered dithering with a classical 8×8 doubly-even magic-square threshold
/// matrix — an extended (64-level) alternative to <see cref="MagicSquare4x4Ditherer"/>.
/// </summary>
/// <remarks>
/// <para>
/// Built from the canonical 8×8 doubly-even magic square (every row, every
/// column and both main diagonals sum to 260 = 8·(8²+1)/2). Each natural number
/// in 1..64 is used exactly once, so — unlike Bayer 8×8 which exhibits its
/// characteristic diagonal bit-interleave grain — the thresholds are balanced
/// along every axis and both diagonals. Shifted to the 0-based range [0, 63] so
/// it maps through <see cref="OrderedDitherer"/>'s [-0.5, 0.5] normaliser.
/// </para>
/// <para>
/// Artefact profile: a noticeably different spectral fingerprint from Bayer
/// 8×8. Because every axis is balanced the pattern tends to break into rough
/// checker-like micro-texture rather than Bayer's ordered cross-hatch; on
/// near-horizontal or near-vertical gradients the magic-square's equalised
/// diagonals also suppress the 45° striping that Bayer's recursive construction
/// produces. Still a dispersed-dot pattern — not a halftone — and fully
/// parallelisable.
/// </para>
/// <para>
/// Tonal resolution: 64 unique thresholds versus Magic-Square-4×4's 16. Good
/// middle ground between Bayer 4×4 and Bayer 8×8 for retro 8/16-bit output
/// where Bayer's diagonal grain is undesirable but more than 16 levels are
/// needed on smooth gradients.
/// </para>
/// <para>
/// References:
/// <a href="https://en.wikipedia.org/wiki/Magic_square">Magic square
/// (Wikipedia)</a>,
/// <a href="https://en.wikipedia.org/wiki/Doubly_even">Doubly-even integer</a>.
/// The specific 8×8 layout is the standard "X-method" doubly-even construction
/// popularised by Dürer's 4×4 and extended by the same diagonal-complement
/// rule; see e.g. W. W. Rouse Ball, <i>Mathematical Recreations and Essays</i>,
/// Macmillan 1939, ch. V.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Magic Square 8x8", Description = "Ordered dithering using an 8x8 doubly-even magic square (64 levels)", Type = DitheringType.Ordered)]
public readonly struct MagicSquare8x8Ditherer : IDitherer {

  // Classical 8x8 doubly-even magic square (row/col/diagonal sums = 260),
  // shifted by -1 so values sit in [0, 63]. The "X-method" construction:
  // fill 1..64 row-major, then complement any cell lying on the two main
  // diagonals of each of the four 4x4 quadrants (cells marked with an X in
  // Dürer's classical 4x4 template).
  private static readonly float[,] _Matrix = {
    { 63,  1,  2, 60, 59,  5,  6, 56 },
    {  8, 54, 53, 11, 12, 50, 49, 15 },
    { 16, 46, 45, 19, 20, 42, 41, 23 },
    { 39, 25, 26, 36, 35, 29, 30, 32 },
    { 31, 33, 34, 28, 27, 37, 38, 24 },
    { 40, 22, 21, 43, 44, 18, 17, 47 },
    { 48, 14, 13, 51, 52, 10,  9, 55 },
    {  7, 57, 58,  4,  3, 61, 62,  0 },
  };

  private static readonly OrderedDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static MagicSquare8x8Ditherer Instance { get; } = new();

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
}
