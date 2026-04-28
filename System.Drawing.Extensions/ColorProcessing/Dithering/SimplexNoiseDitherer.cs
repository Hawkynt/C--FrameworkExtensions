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
/// Noise-based ditherer using Ken Perlin's 2001 "Simplex" noise, which
/// evaluates gradient noise on a triangular (simplex) lattice instead of
/// Perlin 1985's square lattice — cheaper per-sample cost, fewer directional
/// artefacts, and an almost-isotropic grain.
/// </summary>
/// <remarks>
/// <para>
/// Simplex noise subdivides 2-D space into equilateral triangles via a skew /
/// unskew transform; each triangle has three corners, so the per-sample cost
/// is <c>O(3)</c> gradient dot-products plus a smoothstep, vs.
/// <c>O(4)</c> for Perlin 1985 on a square lattice. The power spectrum is
/// more rotation-invariant, so the grain has less of the "axis-aligned"
/// bias visible in classical Perlin noise at small lattice scales.
/// </para>
/// <para>
/// Compared to the other noise ditherers shipped here:
/// <list type="bullet">
///   <item><description><c>PerlinNoise</c> — square lattice; faster in pure FPU
///     terms but shows axis-aligned grain at small scale.</description></item>
///   <item><description><c>SimplexNoise</c> (this ditherer) — triangular lattice;
///     nearly isotropic grain, same band-limited spectrum shape as Perlin.</description></item>
///   <item><description><c>BlueNoise</c> — table-driven, unrelated spectrum.</description></item>
/// </list>
/// </para>
/// <para>
/// Artefact profile: looks almost identical to Perlin noise at large lattice
/// scales (≥32 px) but with subtly reduced directional streak. At small
/// scales (≤8 px) the difference is visible — Simplex is rounder and less
/// "knitted". Useful wherever Perlin noise is useful but the client image
/// has a lot of near-horizontal / near-vertical edges that pick up Perlin's
/// axis bias.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// <para>
/// References: K. Perlin 2001, "Noise hardware", <i>Real-Time Shading,
/// SIGGRAPH Course Notes</i>. S. Gustavson 2005, "Simplex noise demystified"
/// (<a href="https://weber.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf">
/// PDF</a>) — the reference implementation of 2-D / 3-D / 4-D Simplex noise
/// used as the template here. A. Lagae et al. 2010, "A survey of procedural
/// noise functions", <i>Computer Graphics Forum</i> 29(8) (§3.3 covers
/// Simplex noise).
/// </para>
/// </remarks>
[Ditherer("Simplex Noise", Description = "Perlin 2001 Simplex noise threshold field — triangular-lattice gradient noise", Type = DitheringType.Noise, Author = "Ken Perlin", Year = 2001)]
public readonly struct SimplexNoiseDitherer : IDitherer {

  private const int _PERMUTATION_SIZE = 256;
  private const int _LATTICE_SCALE_DEFAULT = 8;

  // Skew / unskew constants for 2-D Simplex noise (Gustavson 2005, §4).
  private const float _F2 = 0.3660254037844386f;  // (sqrt(3) - 1) / 2
  private const float _G2 = 0.21132486540518713f; // (3 - sqrt(3)) / 6

  private static readonly byte[] _DefaultPermutation = _BuildPermutation(42);

  private readonly float _strength;
  private readonly int _seed;
  private readonly int _latticeScale;
  private readonly byte[] _permutation;

  /// <summary>Default instance (strength 1.0, seed 42, 8-pixel lattice).</summary>
  public static SimplexNoiseDitherer Instance { get; } = new();

  /// <summary>Creates a Simplex-noise ditherer.</summary>
  /// <param name="strength">Dither strength in [0, 1]. Default 1.</param>
  /// <param name="seed">Permutation seed for reproducibility. Default 42.</param>
  /// <param name="latticeScale">Lattice period in pixels — controls grain
  /// coarseness. Default 8.</param>
  public SimplexNoiseDitherer(float strength = 1f, int seed = 42, int latticeScale = _LATTICE_SCALE_DEFAULT) {
    this._strength = Math.Max(0f, Math.Min(1f, strength));
    this._seed = seed;
    this._latticeScale = Math.Max(1, latticeScale);
    this._permutation = seed == 42 ? _DefaultPermutation : _BuildPermutation(seed);
  }

  /// <summary>Returns this ditherer with the specified strength.</summary>
  public SimplexNoiseDitherer WithStrength(float strength) => new(strength, this._seed, this._latticeScale);

  /// <summary>Returns this ditherer with the specified seed.</summary>
  public SimplexNoiseDitherer WithSeed(int seed) => new(this._strength, seed, this._latticeScale);

  /// <summary>Returns this ditherer with the specified lattice scale.</summary>
  public SimplexNoiseDitherer WithLatticeScale(int latticeScale) => new(this._strength, this._seed, latticeScale);

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

      // Simplex noise is approximately in [-1, 1]; re-centre to [-0.5, 0.5].
      var raw = _Simplex2D(x * invScale, y * invScale, permutation);
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
  /// 2-D Simplex noise per Gustavson 2005 "Simplex noise demystified".
  /// Output is approximately in [-1, 1].
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Simplex2D(float x, float y, byte[] perm) {
    // Skew input space to determine which simplex cell contains the point.
    var s = (x + y) * _F2;
    var i = (int)Math.Floor(x + s);
    var j = (int)Math.Floor(y + s);

    var t = (i + j) * _G2;
    var x0 = x - (i - t);
    var y0 = y - (j - t);

    // Offsets for second (middle) corner of simplex — pick which triangle
    // we are in based on which of (x0 - y0), (y0 - x0) is positive.
    int i1, j1;
    if (x0 > y0) {
      i1 = 1;
      j1 = 0;
    } else {
      i1 = 0;
      j1 = 1;
    }

    var x1 = x0 - i1 + _G2;
    var y1 = y0 - j1 + _G2;
    var x2 = x0 - 1f + 2f * _G2;
    var y2 = y0 - 1f + 2f * _G2;

    var ii = i & 255;
    var jj = j & 255;

    var gi0 = perm[(ii + perm[jj]) & 255] & 7;
    var gi1 = perm[(ii + i1 + perm[(jj + j1) & 255]) & 255] & 7;
    var gi2 = perm[(ii + 1 + perm[(jj + 1) & 255]) & 255] & 7;

    var t0 = 0.5f - x0 * x0 - y0 * y0;
    var n0 = t0 < 0 ? 0f : t0 * t0 * t0 * t0 * _Grad(gi0, x0, y0);

    var t1 = 0.5f - x1 * x1 - y1 * y1;
    var n1 = t1 < 0 ? 0f : t1 * t1 * t1 * t1 * _Grad(gi1, x1, y1);

    var t2 = 0.5f - x2 * x2 - y2 * y2;
    var n2 = t2 < 0 ? 0f : t2 * t2 * t2 * t2 * _Grad(gi2, x2, y2);

    // Gustavson's empirical scale factor bringing output into ~[-1, 1].
    return 70f * (n0 + n1 + n2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Grad(int hash, float x, float y) {
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
