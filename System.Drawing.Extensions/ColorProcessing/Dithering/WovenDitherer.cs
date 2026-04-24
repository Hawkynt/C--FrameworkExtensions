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
/// Woven dithering — ordered ditherer whose threshold screen alternates
/// horizontal and vertical stripe phases in a plain-weave pattern, producing
/// a textile / basketwork appearance distinct from both Bayer and halftone
/// screens.
/// </summary>
/// <remarks>
/// <para>
/// Plain-weave textiles (fabric, canvas, wicker) are built from two
/// orthogonal yarn families — the warp runs vertically, the weft
/// horizontally, alternately passing over and under each other. The visible
/// surface therefore shows vertical grain on half the cells and horizontal
/// grain on the other half, arranged in a checkerboard. This ditherer
/// emulates the same structure by alternating two 4×4 Bayer-style screens —
/// one biased to horizontal gradient, one to vertical — on a 2×2 cell
/// checker.
/// </para>
/// <para>
/// The 8×8 composite matrix used below. Even-parity 4×4 blocks use a
/// horizontally-progressing threshold sequence; odd-parity blocks use a
/// vertically-progressing sequence. When applied to a mid-tone region the
/// result shows the characteristic plain-weave texture at 4-pixel cell
/// granularity. 64 unique thresholds.
/// </para>
/// <para>
/// Artefact profile: unmistakably "woven" — micro horizontal / vertical
/// stripes organised in a checker. Useful for NPR / textile / stylised
/// retro-pixel-art output where an organic, hand-drafted look is wanted.
/// Poor choice for photographic content — the weave pattern dominates the
/// image.
/// </para>
/// <para>
/// References: textile-simulation dither screens are a classical NPR topic;
/// see P. Brandhofer, B. Wuensche 2015, "Real-time rendering of woven
/// fabrics", <i>Image and Vision Computing NZ</i>; A. Hertzmann 2002,
/// "A survey of stroke-based rendering", <i>IEEE CG&amp;A</i> 23(4).
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Woven", Description = "Plain-weave alternating horizontal/vertical stripe dither screen", Type = DitheringType.Ordered)]
public readonly struct WovenDitherer : IDitherer {

  // 8×8 weave: top-left & bottom-right 4x4 blocks use a horizontally-
  // progressing sequence; top-right & bottom-left use a vertically-
  // progressing one. Each block spans its own 16-level subrange so the
  // composite matrix has 64 unique thresholds.
  private static readonly float[,] _Matrix = {
    {  0,  2,  4,  6, 32, 40, 33, 41 },
    {  1,  3,  5,  7, 48, 56, 49, 57 },
    {  8, 10, 12, 14, 34, 42, 35, 43 },
    {  9, 11, 13, 15, 50, 58, 51, 59 },
    { 36, 44, 37, 45, 16, 18, 20, 22 },
    { 52, 60, 53, 61, 17, 19, 21, 23 },
    { 38, 46, 39, 47, 24, 26, 28, 30 },
    { 54, 62, 55, 63, 25, 27, 29, 31 },
  };

  private static readonly OrderedDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static WovenDitherer Instance { get; } = new();

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
