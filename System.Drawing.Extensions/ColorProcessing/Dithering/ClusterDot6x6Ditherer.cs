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
/// Clustered-dot halftone ordered dithering with a 6×6 threshold matrix (36 gradations).
/// </summary>
/// <remarks>
/// <para>
/// A 6×6 clustered-dot screen sits between the existing 4×4 (16 levels) and 8×8
/// (64 levels) halftone presets. The matrix below is the classical spiral
/// clustered-dot pattern — thresholds grow outward from the centre so that
/// darker input tones paint a compact central dot which expands isotropically
/// as the tone grows.
/// </para>
/// <para>
/// 6×6 is a common newspaper / coarse-print screen size (roughly 85 LPI at
/// 150 DPI scan), producing the visible but orderly dot texture associated
/// with comic-book and coarse magazine reproduction. Artefact profile: regular
/// circular dots, strong 6-pixel periodicity, no diagonal striping.
/// </para>
/// <para>
/// Reference: D. Knuth 1987 "Digital Halftones by Dot Diffusion"
/// ACM Trans. Graph. 6(4), 245-273; classic clustered-dot ordered dithering
/// as described in R. Ulichney 1987 "Digital Halftoning" MIT Press.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Cluster Dot 6x6", Description = "Clustered-dot halftone ordered dithering with 6x6 threshold matrix", Type = DitheringType.Ordered)]
public readonly struct ClusterDot6x6Ditherer : IDitherer {

  // Classical 6x6 spiral clustered-dot matrix. Values 0..35 grow outward from
  // the centre in a spiral so darker tones produce a compact centred dot that
  // expands isotropically as brightness decreases. Fed through
  // ClusterDotDitherer's (rows * cols - 1) normaliser this maps to [-0.5, 0.5].
  private static readonly byte[,] _Matrix = {
    { 34, 29, 17, 21, 30, 35 },
    { 28, 14,  9, 16, 20, 31 },
    { 13,  8,  4,  5, 15, 19 },
    { 12,  3,  0,  1, 10, 23 },
    { 27,  7,  2,  6, 11, 24 },
    { 33, 26, 22, 18, 25, 32 },
  };

  private static readonly ClusterDotDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static ClusterDot6x6Ditherer Instance { get; } = new();

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
