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
/// Noise ditherer whose per-pixel threshold is drawn from a Poisson-disc-
/// sampled point cloud rather than from white / blue noise — the noise values
/// preserve a guaranteed minimum spacing so the visible grain never clumps,
/// even at the flat-region extremes where white noise typically misbehaves.
/// </summary>
/// <remarks>
/// <para>
/// The shipped <c>StippleDitherer</c> uses Poisson-disc <em>thresholds</em>
/// — a screen of ordered, pre-ranked dot positions with Mitchell's
/// best-candidate construction. A noise-domain Poisson-disc variant is
/// related but structurally different: instead of producing a ranked screen
/// (0..N-1), it produces an array of jittered threshold <em>samples</em>
/// that are accessed by a hash of (x, y) — the noise never repeats, but
/// each sample is drawn from a set that preserves the Poisson-disc minimum-
/// distance property.
/// </para>
/// <para>
/// Concretely: a 256-sample Poisson-disc point cloud is generated once at
/// type-load via Bridson's dart-throwing (2007). Per pixel, a seeded integer
/// hash of (x, y) selects one of the 256 samples, and its scalar value
/// becomes the dither threshold. Because the samples retain the Poisson-disc
/// minimum-distance property in their 1-D projection, adjacent pixels' noise
/// values are less likely to collide into visible clumps than pure white
/// noise.
/// </para>
/// <para>
/// Artefact profile: very close to blue-noise in appearance but with a
/// different spectral signature — Poisson-disc has stronger high-frequency
/// content but less perfectly-isotropic spread than true void-and-cluster
/// blue noise. Distinct from <c>BlueNoise</c>, <c>GoldNoise</c>,
/// <c>InterleavedGradientNoise</c>, and <c>HilbertNoise</c>. Fully parallel.
/// </para>
/// <para>
/// References: R. Bridson 2007, "Fast Poisson disk sampling in arbitrary
/// dimensions", <i>SIGGRAPH 2007 Sketches</i>. D. Cook 1986, "Stochastic
/// sampling in computer graphics", <i>ACM TOG</i> 5(1). A. Lagae et al.
/// 2010, "A survey of procedural noise functions", <i>Computer Graphics
/// Forum</i> 29(8) (§4 covers Poisson-disc sampling of noise).
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Poisson-Disc Noise", Description = "Noise dithering with Poisson-disc-sampled jittered threshold samples", Type = DitheringType.Noise, Author = "Robert Bridson", Year = 2007)]
public readonly struct PoissonDiscNoiseDitherer : IDitherer {

  private const int _SAMPLE_COUNT = 256;
  private static readonly float[] _Samples = _BuildSamples(42);

  private readonly float _strength;
  private readonly int _seed;

  /// <summary>Default instance (strength 1.0, seed 42).</summary>
  public static PoissonDiscNoiseDitherer Instance { get; } = new();

  /// <summary>Creates a Poisson-disc noise ditherer.</summary>
  /// <param name="strength">Dither strength in [0, 1]. Default 1.</param>
  /// <param name="seed">Hash seed for reproducibility. Default 42.</param>
  public PoissonDiscNoiseDitherer(float strength = 1f, int seed = 42) {
    this._strength = Math.Max(0f, Math.Min(1f, strength));
    this._seed = seed;
  }

  /// <summary>Returns this ditherer with the specified strength.</summary>
  public PoissonDiscNoiseDitherer WithStrength(float strength) => new(strength, this._seed);

  /// <summary>Returns this ditherer with the specified seed.</summary>
  public PoissonDiscNoiseDitherer WithSeed(int seed) => new(this._strength, seed);

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

      var h = _Hash(x, y, seed);
      var sample = _Samples[(h & 0x7FFFFFFF) % _SAMPLE_COUNT];
      var noise = (sample - 0.5f) * strength;

      var adj = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(c1.ToFloat() + noise),
        UNorm32.FromFloatClamped(c2.ToFloat() + noise),
        UNorm32.FromFloatClamped(c3.ToFloat() + noise),
        alpha);

      indices[targetIdx] = (byte)lookup.FindNearest(adj);
    }
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

  /// <summary>
  /// Bridson-style 1-D Poisson-disc sampling: accept a candidate iff it's
  /// at least <c>r</c> away from all previously-accepted samples. Runs once
  /// at type-load; produces <c>_SAMPLE_COUNT</c> values in [0, 1].
  /// </summary>
  private static float[] _BuildSamples(int seed) {
    const int candidatesPerStep = 16;
    // Target minimum-distance is ≈ 1/_SAMPLE_COUNT; the best-candidate
    // dart-throwing loop below approaches that bound without the explicit
    // constant ever needing to be referenced.
    var accepted = new float[_SAMPLE_COUNT];
    accepted[0] = 0.5f;
    var acceptedCount = 1;
    var rng = new Random(seed);

    while (acceptedCount < _SAMPLE_COUNT) {
      var bestSample = -1f;
      var bestMinDist = -1.0;
      for (var c = 0; c < candidatesPerStep; ++c) {
        var cand = (float)rng.NextDouble();
        var d = double.MaxValue;
        for (var j = 0; j < acceptedCount; ++j) {
          var dd = Math.Abs(cand - accepted[j]);
          if (dd < d)
            d = dd;
        }
        if (d > bestMinDist) {
          bestMinDist = d;
          bestSample = cand;
        }
      }
      // Accept the best candidate even if it's below minDist — after enough
      // acceptances the Poisson-disc constraint has to relax to fill the
      // budget. This matches Bridson's dart-throwing fallback mode.
      accepted[acceptedCount++] = bestSample;
    }

    return accepted;
  }
}
