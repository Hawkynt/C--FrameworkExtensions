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
/// Ordered dithering with an 8×8 Bayer threshold matrix evaluated along the
/// rotated 45° axes (u = (x+y), v = (x−y)), producing a diamond/anti-diagonal
/// grain instead of Bayer's classic 45°-diagonal cross-hatch.
/// </summary>
/// <remarks>
/// <para>
/// A standard Bayer matrix of side N has its dominant spatial frequency aligned
/// with the 45° diagonals — the characteristic visible "cross-hatch" pattern
/// every pixel-art Bayer screenshot shows. Sampling the same matrix through a
/// 45°-rotated coordinate system (u = (x+y) mod N, v = (x−y) mod N) aligns the
/// dominant spatial frequency with the horizontal / vertical axes instead,
/// yielding a rotated grain with different visual affordance while retaining
/// all of Bayer's algorithmic properties (iterated halving dispersion,
/// maximum-minimum-difference optimality, cheap per-pixel formula).
/// </para>
/// <para>
/// The rotation is performed at table-build time: an 8×8 lookup table is
/// materialised once by evaluating <see cref="BayerMatrix.Generate(int)"/>
/// at the rotated sample sites, then fed through <see cref="OrderedDitherer"/>'s
/// [-0.5, 0.5] normaliser. At runtime the ditherer is identical in cost to a
/// plain 8×8 Bayer.
/// </para>
/// <para>
/// Artefact profile: visible axis-aligned micro-stripes replace Bayer's
/// diagonal cross-hatch. Particularly useful when the input image contains
/// near-45° edges that would beat against a standard Bayer grid and produce
/// moiré; the rotated pattern moves the beat frequency to the horizontal /
/// vertical instead. Still a dispersed-dot pattern — not a halftone.
/// </para>
/// <para>
/// References: rotation of threshold screens is a classical technique in print
/// halftoning (see M. A. Riskin 1993, "Rotation of ordered halftone screens",
/// Xerox Technical Report) and is widely used in stochastic screening where
/// axis-aligned beat frequencies are preferred to diagonal ones. Underlying
/// Bayer construction: B. Bayer 1973 "An optimum method for two-level rendition
/// of continuous-tone pictures", IEEE ICC, vol. 1, pp. 26-11 to 26-15.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Rotated Bayer 8x8", Description = "45°-rotated 8x8 Bayer screen (axis-aligned grain replaces diagonal cross-hatch)", Type = DitheringType.Ordered)]
public readonly struct RotatedBayer8x8Ditherer : IDitherer {

  private const int _SIZE = 8;

  // Build an 8x8 table by evaluating the Bayer function at the 45°-rotated
  // sample sites (u = (x+y) & 7, v = (x-y) & 7). The resulting matrix still
  // contains a permutation of 0..63 and still has the iterated-halving
  // dispersion property of plain Bayer, but its dominant spatial frequency
  // is axis-aligned rather than diagonal.
  private static readonly float[,] _Matrix = _BuildRotatedBayer();

  private static readonly OrderedDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static RotatedBayer8x8Ditherer Instance { get; } = new();

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

  private static float[,] _BuildRotatedBayer() {
    var result = new float[_SIZE, _SIZE];
    for (var y = 0; y < _SIZE; ++y)
    for (var x = 0; x < _SIZE; ++x) {
      var u = (x + y) & (_SIZE - 1);
      var v = (x - y + _SIZE) & (_SIZE - 1);
      result[y, x] = BayerMatrix._ComputeValue(u, v, _SIZE);
    }
    return result;
  }
}
