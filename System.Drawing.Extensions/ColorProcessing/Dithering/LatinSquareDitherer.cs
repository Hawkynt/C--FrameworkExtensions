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
/// Ordered dithering using an 8×8 Latin-square threshold matrix — each row and
/// each column contains every value 0..7 exactly once, cycled by a non-trivial
/// offset to break diagonal regularity.
/// </summary>
/// <remarks>
/// <para>
/// A Latin square of side N is an N×N matrix in which every symbol 0..N-1
/// appears exactly once in every row and column (Wikipedia: Latin square). The
/// 8×8 variant used here is constructed as <c>L[y, x] = (a·x + b·y) mod 8</c>
/// with a = 1 and b = 3 — coprime to 8 on both axes, so every row is a
/// permutation of 0..7 (guaranteed by <c>gcd(1, 8) = 1</c>) and every column is
/// a permutation of 0..7 (guaranteed by <c>gcd(3, 8) = 1</c>). The resulting
/// threshold matrix is then expanded to 64 distinct levels by offsetting each
/// cell with <c>y·8</c> so no two cells share the same threshold, the same way
/// a magic square is expanded for ordered dithering.
/// </para>
/// <para>
/// Artefact profile: the spectral fingerprint sits between Bayer 8×8 (diagonal
/// cross-hatch) and the Magic-Square-8×8 ditherer (axis-balanced). Latin
/// squares guarantee per-row and per-column balance but place no constraint on
/// the diagonals, so a faint diagonal drift remains — distinct from both. Use
/// where a slightly more "organic" looking 8×8 screen is desired, still with
/// 64 unique thresholds and full parallelism.
/// </para>
/// <para>
/// References:
/// <a href="https://en.wikipedia.org/wiki/Latin_square">Latin square
/// (Wikipedia)</a>. Use of Latin squares as dither screens is discussed in
/// D. Knuth 1987, "Digital halftones by dot diffusion", ACM TOG vol. 6,
/// pp. 245-273, alongside related balanced-incomplete-block designs; the
/// combinatorial family is extensively catalogued in J. Dénes and A. D.
/// Keedwell, <i>Latin Squares and their Applications</i>, Academic Press
/// 1974.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Latin Square 8x8", Description = "Ordered dithering using an 8x8 Latin-square threshold matrix", Type = DitheringType.Ordered)]
public readonly struct LatinSquareDitherer : IDitherer {

  private const int _SIZE = 8;

  private static readonly float[,] _Matrix = _BuildLatinSquare();

  private static readonly OrderedDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static LatinSquareDitherer Instance { get; } = new();

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

  // Latin square L[y, x] = (x + 3·y) mod 8. Since gcd(1, 8) = gcd(3, 8) = 1
  // every row and column is a permutation of 0..7. Expanding each cell by
  // y·8 yields 64 unique thresholds in [0, 63] while preserving per-row /
  // per-column balance modulo 8.
  private static float[,] _BuildLatinSquare() {
    var result = new float[_SIZE, _SIZE];
    for (var y = 0; y < _SIZE; ++y)
    for (var x = 0; x < _SIZE; ++x) {
      var latin = (x + 3 * y) & (_SIZE - 1);
      result[y, x] = latin + y * _SIZE;
    }
    return result;
  }
}
