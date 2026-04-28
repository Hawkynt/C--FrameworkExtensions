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
/// Noise-based ditherer using Steven Worley's 1996 cellular / Voronoi noise
/// as the per-pixel threshold field. Distance to the nearest Voronoi feature
/// point drives the threshold — produces characteristic "cell" / "stone" /
/// "scale" grain patterns.
/// </summary>
/// <remarks>
/// <para>
/// Worley noise is evaluated by placing one random feature point inside each
/// unit cell of a coarse lattice, then at each sample the distance to the
/// nearest feature point (<c>F1</c>) is returned. Variants return <c>F2−F1</c>
/// for a more cellular look, <c>F2</c> for thicker borders, and so on. This
/// ditherer uses the classic <c>F1</c> formulation with a 3×3 neighbour-cell
/// search, producing a Voronoi-like stippled grain.
/// </para>
/// <para>
/// Compared to the other noise ditherers shipped here:
/// <list type="bullet">
///   <item><description><c>PerlinNoise</c> / <c>SimplexNoise</c> — smooth lattice
///     gradient; looks like clouds.</description></item>
///   <item><description><c>WorleyNoise</c> (this ditherer) — distance-to-feature-point;
///     looks like pebbles / stones / reptile skin depending on lattice
///     scale.</description></item>
/// </list>
/// </para>
/// <para>
/// Artefact profile: highly distinctive compared to every other noise ditherer
/// in the library — the output reveals the Voronoi structure as the
/// threshold field, producing visible "cell walls" on flat mid-tone regions.
/// At small lattice scales (≤4 px) the effect is subtle stippled grain; at
/// medium scales (8–16 px) the cellular structure becomes visually prominent
/// and gives a stone / scale / cracked-mud aesthetic. Useful for stylised
/// halftoning, NPR (non-photo-realistic rendering) pipelines, and map-making
/// where an organic irregular grain is desired.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// <para>
/// References: S. Worley 1996, "A cellular texture basis function",
/// <i>SIGGRAPH 1996</i>, pp. 291-294. A. Lagae et al. 2010, "A survey of
/// procedural noise functions", <i>Computer Graphics Forum</i> 29(8)
/// (§3.5 covers Worley / cellular noise). The Voronoi-distance viewpoint
/// traces back to G. Voronoi 1908, "Nouvelles applications des paramètres
/// continus à la théorie des formes quadratiques", <i>J. reine u. angew.
/// Mathematik</i> 133.
/// </para>
/// </remarks>
[Ditherer("Worley Noise", Description = "Worley 1996 cellular / Voronoi-distance threshold field — organic stippled grain", Type = DitheringType.Noise, Author = "Steven Worley", Year = 1996)]
public readonly struct WorleyNoiseDitherer : IDitherer {

  private const int _LATTICE_SCALE_DEFAULT = 8;

  private readonly float _strength;
  private readonly int _seed;
  private readonly int _latticeScale;

  /// <summary>Default instance (strength 1.0, seed 42, 8-pixel lattice).</summary>
  public static WorleyNoiseDitherer Instance { get; } = new();

  /// <summary>Creates a Worley-noise ditherer.</summary>
  /// <param name="strength">Dither strength in [0, 1]. Default 1.</param>
  /// <param name="seed">Integer seed for feature-point placement. Default 42.</param>
  /// <param name="latticeScale">Voronoi cell size in pixels. Default 8.</param>
  public WorleyNoiseDitherer(float strength = 1f, int seed = 42, int latticeScale = _LATTICE_SCALE_DEFAULT) {
    this._strength = Math.Max(0f, Math.Min(1f, strength));
    this._seed = seed;
    this._latticeScale = Math.Max(1, latticeScale);
  }

  /// <summary>Returns this ditherer with the specified strength.</summary>
  public WorleyNoiseDitherer WithStrength(float strength) => new(strength, this._seed, this._latticeScale);

  /// <summary>Returns this ditherer with the specified seed.</summary>
  public WorleyNoiseDitherer WithSeed(int seed) => new(this._strength, seed, this._latticeScale);

  /// <summary>Returns this ditherer with the specified lattice scale.</summary>
  public WorleyNoiseDitherer WithLatticeScale(int latticeScale) => new(this._strength, this._seed, latticeScale);

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
    var latticeScale = this._latticeScale > 0 ? this._latticeScale : _LATTICE_SCALE_DEFAULT;
    var invScale = 1f / latticeScale;
    var endY = startY + height;

    // F1 distance is in [0, sqrt(2)] on the unit cell; expected mean ≈ 0.33.
    // Pre-scale by ~1/0.5 to map roughly into [-0.5, 0.5] after centring.
    const float _NORM_SCALE = 2f;
    const float _NORM_BIAS = 0.5f;

    for (var y = startY; y < endY; ++y)
    for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
      var color = source[sourceIdx];
      var (c1, c2, c3, alpha) = color.ToNormalized();

      var f1 = _WorleyF1(x * invScale, y * invScale, seed);
      // f1 is in [0, ~1.2]; clamp-scale into [-0.5, 0.5] range roughly.
      var centred = Math.Min(1f, f1 * _NORM_SCALE) - _NORM_BIAS;
      var noise = centred * strength;

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
  /// Worley F1 (distance to nearest feature point) on a 3×3 neighbour-cell
  /// search. Each lattice cell owns exactly one jittered feature point whose
  /// offset is hashed from the cell coordinate + seed.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _WorleyF1(float x, float y, int seed) {
    var cx = (int)Math.Floor(x);
    var cy = (int)Math.Floor(y);

    var f1 = float.MaxValue;

    for (var dy = -1; dy <= 1; ++dy)
    for (var dx = -1; dx <= 1; ++dx) {
      var ncx = cx + dx;
      var ncy = cy + dy;

      // Two independent hashes for (fx, fy) jitter offsets in [0, 1).
      var h1 = _Hash(ncx, ncy, seed) & 0xFFFF;
      var h2 = _Hash(ncx, ncy, seed ^ 0x5555AAAA) & 0xFFFF;
      var fx = h1 / 65536f;
      var fy = h2 / 65536f;

      var px = ncx + fx - x;
      var py = ncy + fy - y;
      var d = px * px + py * py;
      if (d < f1)
        f1 = d;
    }

    // Return Euclidean distance (sqrt) so the histogram of F1 values is
    // better-spread than squared distance.
    return (float)Math.Sqrt(f1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _Hash(int x, int y, int seed) {
    var h = seed;
    h ^= x * 374761393;
    h ^= y * 668265263;
    h = (h ^ (h >> 15)) * 1103515245;
    h ^= h >> 13;
    return h;
  }
}
