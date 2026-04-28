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
#if SUPPORTS_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
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
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var strength = this._strength > 0 ? this._strength : 1f;
    var seed = this._seed;
    var endY = startY + height;

#if SUPPORTS_INTRINSICS
    // float-domain SIMD across 4 pixels of B/G/R add+clamp; scalar quantize.
    // Eligibility check is loop-invariant — hoisted out of the y-loop so the JIT sees
    // two distinct hot loops, and so legacy TFMs (net35/40/45/48) don't pay a per-row
    // Sse2.IsSupported field load.
    var simdEligible = Sse2.IsSupported && typeof(TWork) == typeof(Bgra8888) && width >= 4;
    if (simdEligible) {
      var simdEnd = width & ~3;
      var bChannels = stackalloc float[4];
      var gChannels = stackalloc float[4];
      var rChannels = stackalloc float[4];
      var alphaBytes = stackalloc byte[4];
      var thresholds4 = stackalloc float[4];

      for (var y = startY; y < endY; ++y) {
        var rowSource = source + y * sourceStride;
        var x = 0;
        var targetIdx = y * targetStride;

        var srcBase = (byte*)rowSource;
        for (; x < simdEnd; x += 4) {
          ThresholdDithererSimd.DecodeBgra4Pixels(srcBase + x * 4, bChannels, gChannels, rChannels, alphaBytes);

          // Compute 4 per-pixel gold-noise thresholds (sin/floor in double, scalar).
          for (var lane = 0; lane < 4; ++lane) {
            var n = _GoldNoise(x + lane + seed, y, seed);
            thresholds4[lane] = (n - 0.5f) * strength;
          }

          ThresholdDithererSimd.AddThresholdAndClamp_4Pixels(bChannels, gChannels, rChannels, thresholds4);

          for (var lane = 0; lane < 4; ++lane) {
            // Bgra8888 component convention: (C1, C2, C3, A) = (R, G, B, A).
            var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
              UNorm32.FromFloatClamped(rChannels[lane]),
              UNorm32.FromFloatClamped(gChannels[lane]),
              UNorm32.FromFloatClamped(bChannels[lane]),
              UNorm32.FromByte(alphaBytes[lane])
            );
            indices[targetIdx + x + lane] = (byte)lookup.FindNearest(adjustedColor);
          }
        }
        targetIdx += x;

        // Tail: width-mod-4 leftover lanes.
        for (; x < width; ++x, ++targetIdx) {
          var color = rowSource[x];
          var (c1, c2, c3, alpha) = color.ToNormalized();

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
    } else
#endif
    {
      for (var y = startY; y < endY; ++y) {
        var rowSource = source + y * sourceStride;
        var targetIdx = y * targetStride;

        for (var x = 0; x < width; ++x, ++targetIdx) {
          var color = rowSource[x];
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
