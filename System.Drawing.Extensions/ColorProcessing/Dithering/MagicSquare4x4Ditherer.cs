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
