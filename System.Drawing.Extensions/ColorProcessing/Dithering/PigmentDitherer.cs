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
/// Pigment dithering — error-diffusion variant that simulates watercolour
/// bleed by distributing the quantization error to a randomised four-
/// neighbour footprint weighted by a "viscosity" factor, producing
/// soft-edged, organic colour boundaries reminiscent of wet-on-wet paper.
/// </summary>
/// <remarks>
/// <para>
/// Classical error diffusion distributes error along a fixed matrix
/// (Floyd-Steinberg, Stucki, etc.) — sharp, engineered, directional.
/// Watercolour and ink-wash media behave differently: the fluid carrier
/// (water for watercolour, thinner for ink) flows slowly outward from each
/// pigment deposition, and the pigment's "viscosity" determines how far the
/// error diffuses before the medium dries. Simulating this as a ditherer
/// requires stochastic, locally-varying diffusion: the direction depends on
/// a per-pixel random flow vector, the magnitude on the local pigment
/// viscosity.
/// </para>
/// <para>
/// The algorithm:
/// </para>
/// <list type="number">
/// <item><description>Compute the per-pixel quantization error as usual.</description></item>
/// <item><description>Choose a random 2-D flow vector from a hash of
/// (x, y, seed), biased toward the four cardinal and four diagonal
/// directions — mimicking paper-fiber capillary paths.</description></item>
/// <item><description>Split the error into a "deposited" fraction (kept on
/// the current pixel's neighbourhood) and a "flowing" fraction (spread
/// further along the flow vector), with the ratio controlled by the
/// viscosity parameter.</description></item>
/// <item><description>Distribute the flowing error over the 3×3 footprint
/// centred on the flow-vector target.</description></item>
/// </list>
/// <para>
/// Artefact profile: visibly "painted" — edges bleed softly, flat regions
/// develop subtle tonal variation, and transitions between adjacent palette
/// entries no longer form sharp diagonals. Best at high palette counts
/// (&gt; 64) where the subtle colour transitions have somewhere to go. At
/// 8-16 colours the bleed becomes exaggerated and the dither pattern
/// dominates.
/// </para>
/// <para>
/// References: P. Haeberli 1990, "Paint by numbers: abstract image
/// representations", <i>SIGGRAPH '90</i> (brush-stroke simulation). A. Hertzmann
/// 2002, "A survey of stroke-based rendering", <i>IEEE CG&amp;A</i> 23(4).
/// T. Van Laerhoven, F. Van Reeth 2005, "Real-time simulation of watercolour
/// on paper", <i>Computer Animation and Virtual Worlds</i> 16 (the viscosity
/// / capillary-flow model used here).
/// </para>
/// <para>Sequential (error-diffusion variant). Deterministic given a seed.</para>
/// </remarks>
[Ditherer("Pigment", Description = "Watercolour-style error diffusion with viscosity-controlled stochastic flow", Type = DitheringType.ErrorDiffusion)]
public readonly struct PigmentDitherer : IDitherer {

  private readonly float _viscosity;
  private readonly int _seed;

  /// <summary>Default instance (viscosity 0.5, seed 42).</summary>
  public static PigmentDitherer Instance { get; } = new();

  /// <summary>Creates a pigment ditherer.</summary>
  /// <param name="viscosity">
  /// Viscosity factor in [0, 1]. 0 = fully fluid (all error flows to the
  /// single flow-vector target); 1 = rigid (most error stays on the current
  /// 3×3 footprint). Default 0.5.
  /// </param>
  /// <param name="seed">RNG seed for reproducible flow-vector selection.</param>
  public PigmentDitherer(float viscosity = 0.5f, int seed = 42) {
    this._viscosity = Math.Max(0f, Math.Min(1f, viscosity));
    this._seed = seed;
  }

  /// <summary>Returns this ditherer with the specified viscosity.</summary>
  public PigmentDitherer WithViscosity(float viscosity) => new(viscosity, this._seed);

  /// <summary>Returns this ditherer with the specified seed.</summary>
  public PigmentDitherer WithSeed(int seed) => new(this._viscosity, seed);

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => true;

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
    var endY = startY + height;
    var viscosity = this._viscosity > 0 ? this._viscosity : 0.5f;
    var seed = this._seed;

    // Float error buffer over the working rows.
    var errR = new float[width, height];
    var errG = new float[width, height];
    var errB = new float[width, height];

    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;
      for (var x = 0; x < width; ++x) {
        var color = decoder.Decode(source[y * sourceStride + x]);
        var (c1, c2, c3, alpha) = color.ToNormalized();
        var pr = Math.Max(0f, Math.Min(1f, c1.ToFloat() + errR[x, localY]));
        var pg = Math.Max(0f, Math.Min(1f, c2.ToFloat() + errG[x, localY]));
        var pb = Math.Max(0f, Math.Min(1f, c3.ToFloat() + errB[x, localY]));

        var adj = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(pr),
          UNorm32.FromFloatClamped(pg),
          UNorm32.FromFloatClamped(pb),
          alpha);

        var idx = (byte)lookup.FindNearest(adj, out var nearest);
        indices[y * targetStride + x] = idx;

        var (n1, n2, n3, _) = nearest.ToNormalized();
        var er = pr - n1.ToFloat();
        var eg = pg - n2.ToFloat();
        var eb = pb - n3.ToFloat();

        // Hash a flow vector in 8 possible directions (cardinal + diagonal).
        var hash = _Hash(x, y, seed) & 7;
        int fdx = hash switch {
          0 => 1, 1 => 1, 2 => 0, 3 => -1, 4 => -1, 5 => -1, 6 => 0, _ => 1,
        };
        int fdy = hash switch {
          0 => 0, 1 => 1, 2 => 1, 3 => 1, 4 => 0, 5 => -1, 6 => -1, _ => -1,
        };
        // Flow direction always has a forward component (dy ≥ 0) so errors
        // don't leak "backward" and break raster-scan causality.
        if (fdy < 0) fdy = 1;

        // Split into deposited (viscous) and flowing (fluid) fractions.
        var flowFrac = 1f - viscosity;
        var deposited = viscosity;

        // Deposit: 4-neighbour FS-like spread within current / next row.
        _Deposit(errR, errG, errB, x + 1, localY, er, eg, eb, 0.4375f * deposited, width, height);
        if (localY + 1 < height) {
          _Deposit(errR, errG, errB, x - 1, localY + 1, er, eg, eb, 0.1875f * deposited, width, height);
          _Deposit(errR, errG, errB, x, localY + 1, er, eg, eb, 0.3125f * deposited, width, height);
          _Deposit(errR, errG, errB, x + 1, localY + 1, er, eg, eb, 0.0625f * deposited, width, height);
        }

        // Flow: deliver the remaining error to the flow-vector target and
        // its 4 cardinal neighbours (blur-like spread).
        var tx = x + fdx;
        var ty = localY + fdy;
        _Deposit(errR, errG, errB, tx, ty, er, eg, eb, 0.5f * flowFrac, width, height);
        _Deposit(errR, errG, errB, tx + 1, ty, er, eg, eb, 0.125f * flowFrac, width, height);
        _Deposit(errR, errG, errB, tx - 1, ty, er, eg, eb, 0.125f * flowFrac, width, height);
        _Deposit(errR, errG, errB, tx, ty + 1, er, eg, eb, 0.125f * flowFrac, width, height);
        _Deposit(errR, errG, errB, tx, ty - 1, er, eg, eb, 0.125f * flowFrac, width, height);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _Deposit(float[,] errR, float[,] errG, float[,] errB, int x, int y, float er, float eg, float eb, float w, int width, int height) {
    if (x < 0 || x >= width || y < 0 || y >= height) return;
    errR[x, y] += er * w;
    errG[x, y] += eg * w;
    errB[x, y] += eb * w;
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
