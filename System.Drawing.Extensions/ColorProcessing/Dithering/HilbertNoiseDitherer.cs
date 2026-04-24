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
/// Noise-based ditherer whose per-pixel threshold is a deterministic hash of
/// the pixel's 1-D position along a Hilbert space-filling curve, rather than
/// its 2-D (x, y) coordinates.
/// </summary>
/// <remarks>
/// <para>
/// Standard noise ditherers (white, blue, Interleaved-Gradient-Noise) hash the
/// pixel's Cartesian coordinates directly. Because the Hilbert curve visits
/// every point in a 2-D region while preserving spatial locality, hashing the
/// 1-D Hilbert index instead yields noise that is locally smoother — two
/// neighbouring pixels usually sit close on the curve, so their noise values
/// are close too. The result is a grain with clustered "ink-blot" micro-shapes
/// instead of the isotropic speckle of white/blue noise, useful for organic /
/// hand-drawn-looking dithered output.
/// </para>
/// <para>
/// The Hilbert coordinate is computed per-pixel in O(log N) with the classic
/// iterative rot / interleave formula (see the WP citation below). The order
/// of the curve is derived from image size at dither time — no table, no
/// sequential state, fully parallel-friendly. Hashing uses the same
/// xorshift-mul integer hash as <c>NoiseDitherer.White</c> so outputs stay
/// deterministic and thread-order-independent.
/// </para>
/// <para>
/// Artefact profile: visibly different from plain white/blue noise — the
/// characteristic Hilbert "L-shaped" micro-clusters show through on smooth
/// gradients as gently twisting grain. Less isotropic than blue noise, more
/// organic-looking than IGN. Behaves well on text / line-art where
/// the smooth-along-curve property hides the noise inside the strokes.
/// </para>
/// <para>
/// References: D. Hilbert 1891, "Über die stetige Abbildung einer Linie auf
/// ein Flächenstück", Math. Ann. 38, 459-460. Hilbert-index encoding:
/// <a href="https://en.wikipedia.org/wiki/Hilbert_curve#Applications_and_mapping_algorithms">
/// WP: Hilbert curve — mapping algorithms</a>. Noise-along-curve as a visual
/// aesthetic: T. Riemersma 1998 (Riemersma dither) exploits the same
/// spatial-locality property for error diffusion; this ditherer repurposes it
/// as an ordered noise source.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Hilbert Noise", Description = "Noise dithering whose threshold is hashed from the pixel's Hilbert-curve index", Type = DitheringType.Noise)]
public readonly struct HilbertNoiseDitherer : IDitherer {

  private readonly float _strength;
  private readonly int _seed;

  /// <summary>Default instance (strength 1.0, seed 42).</summary>
  public static HilbertNoiseDitherer Instance { get; } = new();

  /// <summary>
  /// Creates a Hilbert-curve noise ditherer.
  /// </summary>
  /// <param name="strength">Dither strength in [0, 1]. Default 1.</param>
  /// <param name="seed">Hash seed for reproducibility. Default 42.</param>
  public HilbertNoiseDitherer(float strength = 1f, int seed = 42) {
    this._strength = Math.Max(0f, Math.Min(1f, strength));
    this._seed = seed;
  }

  /// <summary>Returns this ditherer with specified strength.</summary>
  public HilbertNoiseDitherer WithStrength(float strength) => new(strength, this._seed);

  /// <summary>Returns this ditherer with specified seed.</summary>
  public HilbertNoiseDitherer WithSeed(int seed) => new(this._strength, seed);

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
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var strength = this._strength > 0 ? this._strength : 1f;
    var seed = this._seed;
    var endY = startY + height;

    // Snap the Hilbert order up to the next power of two that covers both
    // width and height. 2^14 = 16384 pixels per side is plenty for any
    // reasonable image; the encoder cost scales with log2(sidePow2).
    var side = Math.Max(width, height);
    var sidePow2 = 1;
    while (sidePow2 < side)
      sidePow2 <<= 1;

    for (var y = startY; y < endY; ++y)
    for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
      var color = decoder.Decode(source[sourceIdx]);
      var (c1, c2, c3, alpha) = color.ToNormalized();

      // Compute Hilbert-curve 1-D index for this pixel.
      var d = _HilbertXYToD(sidePow2, x, y);

      // Hash the 1-D index to a centred noise value in [-0.5, 0.5].
      var hash = _Hash(d, seed);
      var noise = ((hash & 0xFFFF) / 65536f - 0.5f) * strength;

      var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(c1.ToFloat() + noise),
        UNorm32.FromFloatClamped(c2.ToFloat() + noise),
        UNorm32.FromFloatClamped(c3.ToFloat() + noise),
        alpha
      );

      indices[targetIdx] = (byte)lookup.FindNearest(adjustedColor);
    }
  }

  /// <summary>
  /// Converts (x, y) into a 1-D Hilbert curve index on a sidePow2 × sidePow2
  /// grid. sidePow2 must be a power of two ≥ 1. See WP: Hilbert curve —
  /// iterative xy2d reference.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _HilbertXYToD(int sidePow2, int x, int y) {
    var d = 0;
    for (var s = sidePow2 >> 1; s > 0; s >>= 1) {
      var rx = (x & s) > 0 ? 1 : 0;
      var ry = (y & s) > 0 ? 1 : 0;
      d += s * s * ((3 * rx) ^ ry);

      // Rotate quadrant appropriately.
      if (ry == 0) {
        if (rx == 1) {
          x = s - 1 - x;
          y = s - 1 - y;
        }
        (x, y) = (y, x);
      }
    }
    return d;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _Hash(int d, int seed) {
    var h = seed ^ d * 374761393;
    h = (h ^ (h >> 15)) * 1103515245;
    h ^= h >> 13;
    return h;
  }
}
