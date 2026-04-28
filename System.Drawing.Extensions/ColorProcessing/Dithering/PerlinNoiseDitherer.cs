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
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Noise-based ditherer using Ken Perlin's 1985 classical gradient noise as
/// the per-pixel threshold field. Produces a smooth, organic "cloud" grain
/// that is band-limited around a single dominant spatial frequency.
/// </summary>
/// <remarks>
/// <para>
/// Perlin gradient noise is evaluated on an integer lattice of random unit
/// gradient vectors: at each lattice corner the gradient is dotted with the
/// offset to the sample point, and the four dot products are interpolated
/// with Perlin's 5th-order Hermite smoothstep (<c>6t⁵ − 15t⁴ + 10t⁶</c>). The
/// resulting field is <i>C²</i>-continuous and its power spectrum is strongly
/// peaked around <c>1/latticeScale</c> — distinct from both the broad-band
/// spectrum of white noise and the shifted blue-noise spectrum of ordered
/// dither screens.
/// </para>
/// <para>
/// Compared to the other noise ditherers in this library:
/// <list type="bullet">
///   <item><description><c>WhiteNoise</c> — flat spectrum; visible clumping.</description></item>
///   <item><description><c>BlueNoise</c> — void-and-cluster tile; low-freq suppressed.</description></item>
///   <item><description><c>InterleavedGradientNoise</c> — shader-style, cheap per-pixel hash.</description></item>
///   <item><description><c>GoldNoise</c> — low-discrepancy golden-ratio formula, table-free.</description></item>
///   <item><description><c>PoissonDiscNoise</c> — blue-noise-like without a repeating tile.</description></item>
///   <item><description><c>PerlinNoise</c> (this ditherer) — band-limited "cloud" grain; the
///     characteristic Perlin look, useful for stylised /
///     poster-like output.</description></item>
/// </list>
/// </para>
/// <para>
/// Artefact profile: mid-scale blotchy grain with visible low-frequency
/// undulation — quite different from blue/white noise. Use when a
/// "hand-drawn" or "cloud-shaded" look is desired. The lattice size controls
/// grain coarseness: small lattice (≤4 px) gives fine texture with visible
/// Perlin sinusoidal bias on large flat regions; larger lattice (16–32 px)
/// produces recognisable Perlin cloud shapes.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// <para>
/// References: K. Perlin 1985, "An image synthesizer", <i>SIGGRAPH 1985</i>,
/// pp. 287-296. K. Perlin 2002, "Improving noise", <i>SIGGRAPH 2002</i>,
/// pp. 681-682 (the improved Hermite smoothstep used here). A. Lagae et al.
/// 2010, "A survey of procedural noise functions", <i>Computer Graphics
/// Forum</i> 29(8) (§3.2 covers Perlin gradient noise).
/// </para>
/// </remarks>
[Ditherer("Perlin Noise", Description = "Classical Perlin gradient-noise threshold field — band-limited cloud grain", Type = DitheringType.Noise, Author = "Ken Perlin", Year = 1985)]
public readonly struct PerlinNoiseDitherer : IDitherer {

  private const int _PERMUTATION_SIZE = 256;
  private const int _LATTICE_SCALE_DEFAULT = 8;

  // Static permutation table — built once, shared by all seed=42 instances.
  private static readonly byte[] _DefaultPermutation = _BuildPermutation(42);

  private readonly float _strength;
  private readonly int _seed;
  private readonly int _latticeScale;
  private readonly byte[] _permutation;

  /// <summary>Default instance (strength 1.0, seed 42, 8-pixel lattice).</summary>
  public static PerlinNoiseDitherer Instance { get; } = new();

  /// <summary>Creates a Perlin-noise ditherer.</summary>
  /// <param name="strength">Dither strength in [0, 1]. Default 1.</param>
  /// <param name="seed">Permutation seed for reproducibility. Default 42.</param>
  /// <param name="latticeScale">Lattice period in pixels — controls grain
  /// coarseness. Default 8.</param>
  public PerlinNoiseDitherer(float strength = 1f, int seed = 42, int latticeScale = _LATTICE_SCALE_DEFAULT) {
    this._strength = Math.Max(0f, Math.Min(1f, strength));
    this._seed = seed;
    this._latticeScale = Math.Max(1, latticeScale);
    this._permutation = seed == 42 ? _DefaultPermutation : _BuildPermutation(seed);
  }

  /// <summary>Returns this ditherer with the specified strength.</summary>
  public PerlinNoiseDitherer WithStrength(float strength) => new(strength, this._seed, this._latticeScale);

  /// <summary>Returns this ditherer with the specified seed.</summary>
  public PerlinNoiseDitherer WithSeed(int seed) => new(this._strength, seed, this._latticeScale);

  /// <summary>Returns this ditherer with the specified lattice scale.</summary>
  public PerlinNoiseDitherer WithLatticeScale(int latticeScale) => new(this._strength, this._seed, latticeScale);

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
    var permutation = this._permutation ?? _DefaultPermutation;
    var latticeScale = this._latticeScale > 0 ? this._latticeScale : _LATTICE_SCALE_DEFAULT;
    var invScale = 1f / latticeScale;
    var endY = startY + height;

    for (var y = startY; y < endY; ++y)
    for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
      var color = source[sourceIdx];
      var (c1, c2, c3, alpha) = color.ToNormalized();

      // Perlin noise in [-1, 1]; re-centre to [-0.5, 0.5].
      var raw = _Perlin2D(x * invScale, y * invScale, permutation);
      var noise = raw * 0.5f * strength;

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
  /// Classical 2-D Perlin gradient noise with Perlin's 2002 improved Hermite
  /// smoothstep. Output is approximately in [-1, 1].
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Perlin2D(float x, float y, byte[] perm) {
    var xi = (int)Math.Floor(x) & 255;
    var yi = (int)Math.Floor(y) & 255;
    var xf = x - (float)Math.Floor(x);
    var yf = y - (float)Math.Floor(y);

    var u = _Fade(xf);
    var v = _Fade(yf);

    var aa = perm[(perm[xi] + yi) & 255];
    var ab = perm[(perm[xi] + yi + 1) & 255];
    var ba = perm[(perm[(xi + 1) & 255] + yi) & 255];
    var bb = perm[(perm[(xi + 1) & 255] + yi + 1) & 255];

    var x1 = _Lerp(u, _Grad(aa, xf, yf), _Grad(ba, xf - 1, yf));
    var x2 = _Lerp(u, _Grad(ab, xf, yf - 1), _Grad(bb, xf - 1, yf - 1));
    return _Lerp(v, x1, x2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lerp(float t, float a, float b) => a + t * (b - a);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Grad(byte hash, float x, float y) {
    // Perlin's 8-direction gradient table for 2-D noise.
    switch (hash & 7) {
      case 0: return x + y;
      case 1: return -x + y;
      case 2: return x - y;
      case 3: return -x - y;
      case 4: return x;
      case 5: return -x;
      case 6: return y;
      default: return -y;
    }
  }

  /// <summary>
  /// Builds a Fisher-Yates-shuffled permutation table of
  /// <see cref="_PERMUTATION_SIZE"/> entries, seeded for reproducibility.
  /// </summary>
  private static byte[] _BuildPermutation(int seed) {
    var p = new byte[_PERMUTATION_SIZE];
    for (var i = 0; i < _PERMUTATION_SIZE; ++i)
      p[i] = (byte)i;

    var rng = new Random(seed);
    for (var i = _PERMUTATION_SIZE - 1; i > 0; --i) {
      var j = rng.Next(i + 1);
      (p[i], p[j]) = (p[j], p[i]);
    }
    return p;
  }
}
