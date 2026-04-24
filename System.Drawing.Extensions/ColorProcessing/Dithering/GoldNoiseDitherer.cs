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
/// Noise-based ditherer that uses a 2-D golden-ratio hash — a very cheap
/// low-discrepancy sequence with a near-blue-noise spectrum — to compute
/// the per-pixel threshold.
/// </summary>
/// <remarks>
/// <para>
/// Variant of the "Gold Noise" formula popularised by Dominik "dla dooo" /
/// I. Quilez: the fractional part of <c>dot((x, y), φ·k) · ψ</c>, where φ is
/// the reciprocal of the golden ratio and ψ is a second irrational constant.
/// The result is a deterministic, position-only noise field whose Fourier
/// spectrum approximates blue noise (very little low-frequency energy) but
/// whose cost is a handful of multiply-add / frac operations — no lookup
/// table, no state, no RNG.
/// </para>
/// <para>
/// Compared to the other noise ditherers shipped here:
/// <list type="bullet">
///   <item><description><c>WhiteNoise</c> — cheap hash but visible clumping on
///     flat regions.</description></item>
///   <item><description><c>BlueNoise</c> — 64×64 precomputed blue-noise table;
///     good spectrum, fixed tile.</description></item>
///   <item><description><c>InterleavedGradientNoise</c> — low-discrepancy
///     pseudo-random (Jimenez), designed for temporal AA.</description></item>
///   <item><description><c>GoldNoise</c> — this ditherer; low-discrepancy
///     golden-ratio formula, no table, no repeating tile, very cheap.</description></item>
/// </list>
/// For dithering purposes GoldNoise sits between IGN (very cheap, slight
/// banding on ramps) and true blue noise (nicer spectrum, requires table) —
/// it has a longer effective period than a 64×64 blue-noise tile and very
/// little visible structure even on full-frame gradients.
/// </para>
/// <para>
/// References:
/// <a href="https://www.shadertoy.com/view/ltB3zD">"Gold Noise" (Shadertoy)</a>
/// — the original 2013 shader by "dla dooo". See also:
/// <a href="https://www.iquilezles.org/www/articles/functions/functions.htm">
/// Inigo Quilez, "Useful little functions"</a> (golden-ratio low-discrepancy
/// sequences). Theory: R. L. Cook 1986, "Stochastic sampling in computer
/// graphics", ACM TOG 5, especially §3 on low-discrepancy sequences; J. H.
/// Halton 1960, "On the efficiency of certain quasi-random sequences".
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Gold Noise", Description = "Golden-ratio low-discrepancy noise dithering — 2D analog of Quilez's Gold Noise", Type = DitheringType.Noise)]
public readonly struct GoldNoiseDitherer : IDitherer {

  // (sqrt(5) - 1) / 2 — reciprocal of the golden ratio φ.
  private const double _PHI_INV = 0.6180339887498948;
  // e^π - π — Quilez's second irrational amplifier; keeps the noise
  // essentially uncorrelated to the φ-based projection direction.
  private const double _PI_AMP = 3.1415926535897932;
  private const double _SCALE = 43758.5453;

  private readonly float _strength;
  private readonly int _seed;

  /// <summary>Default instance (strength 1.0, seed 42).</summary>
  public static GoldNoiseDitherer Instance { get; } = new();

  /// <summary>
  /// Creates a gold-noise ditherer.
  /// </summary>
  /// <param name="strength">Dither strength in [0, 1]. Default 1.</param>
  /// <param name="seed">
  /// Integer seed — applied as an offset to (x, y) so distinct seeds sample a
  /// different slice of the same low-discrepancy field. Default 42.
  /// </param>
  public GoldNoiseDitherer(float strength = 1f, int seed = 42) {
    this._strength = Math.Max(0f, Math.Min(1f, strength));
    this._seed = seed;
  }

  /// <summary>Returns this ditherer with specified strength.</summary>
  public GoldNoiseDitherer WithStrength(float strength) => new(strength, this._seed);

  /// <summary>Returns this ditherer with specified seed.</summary>
  public GoldNoiseDitherer WithSeed(int seed) => new(this._strength, seed);

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

    for (var y = startY; y < endY; ++y)
    for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
      var color = decoder.Decode(source[sourceIdx]);
      var (c1, c2, c3, alpha) = color.ToNormalized();

      // Gold-noise formula: fract( sin( dot((x,y)+seed, (φ⁻¹, π)) ) · 43758.5453 )
      // Produces a deterministic value in [0,1) with near-blue-noise spectrum.
      var n = _GoldNoise(x + seed, y, seed);
      var noise = (n - 0.5f) * strength;

      var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(c1.ToFloat() + noise),
        UNorm32.FromFloatClamped(c2.ToFloat() + noise),
        UNorm32.FromFloatClamped(c3.ToFloat() + noise),
        alpha
      );

      indices[targetIdx] = (byte)lookup.FindNearest(adjustedColor);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _GoldNoise(int x, int y, int seed) {
    // Project onto two irrational directions, then apply a sin-based mixer
    // to get a low-discrepancy deterministic noise value in [0, 1).
    var dx = x * _PHI_INV;
    var dy = y * _PI_AMP;
    // Offset by seed as well so distinct seeds select different slices.
    var sum = dx + dy + seed * _PHI_INV;
    var mixed = Math.Sin(sum) * _SCALE;
    var frac = mixed - Math.Floor(mixed);
    return (float)frac;
  }
}
