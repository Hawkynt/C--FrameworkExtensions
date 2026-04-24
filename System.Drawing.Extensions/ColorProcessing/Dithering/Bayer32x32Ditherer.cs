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
/// Bayer ordered dithering with a 32×32 threshold matrix (1024 gradations) —
/// the highest-resolution Bayer preset shipped by the library.
/// </summary>
/// <remarks>
/// <para>
/// Standard Bayer ordered dithering (<see cref="OrderedDitherer.Bayer16x16"/>)
/// tops out at 256 unique threshold levels, which can still show visible pattern
/// tiling on very smooth gradients in high-resolution output. A 32×32 matrix
/// supplies 1024 levels — enough to keep the pattern imperceptible on 1080p and
/// larger gradients while remaining fully parallel-friendly.
/// </para>
/// <para>
/// Larger matrices trade slightly higher memory footprint (4 KB vs. 1 KB for
/// 16×16) for smoother appearance on large, near-flat colour fields. The typical
/// artefact profile is Bayer's characteristic cross-hatch, still visible on
/// zoomed-in crops but essentially invisible at native resolution.
/// </para>
/// <para>
/// Reference: B. Bayer 1973 "An optimum method for two-level rendition of
/// continuous-tone pictures" IEEE Int. Conf. on Communications, vol. 1,
/// pp. 26-11 to 26-15. Matrix generated recursively by
/// <see cref="BayerMatrix.Generate(int)"/>.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Bayer 32x32", Description = "Bayer ordered dithering with 32x32 threshold matrix (1024 levels)", Type = DitheringType.Ordered, Author = "Bryce Bayer", Year = 1973)]
public readonly struct Bayer32x32Ditherer : IDitherer {

  // Materialise the inner OrderedDitherer once and share across all uses.
  private static readonly OrderedDitherer _Inner = new(BayerMatrix.Generate(32));

  /// <summary>Default instance.</summary>
  public static Bayer32x32Ditherer Instance { get; } = new();

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
