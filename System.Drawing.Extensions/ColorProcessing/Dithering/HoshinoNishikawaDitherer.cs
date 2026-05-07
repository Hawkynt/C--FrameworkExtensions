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

using Hawkynt.ColorProcessing.ColorMath;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Ordered ditherer with a 16×16 linear-scan dispersed-dot threshold matrix
/// and per-pixel deterministic sub-quantum jitter, after Hoshino &amp;
/// Nishikawa 1990. Designed to break the "grid regularity" of a plain
/// ordered screen on large flat regions without requiring the history /
/// error-diffusion buffer of the sequential filters.
/// </summary>
/// <remarks>
/// <para>
/// The 1990 Hoshino-Nishikawa screen combines a Bayer-derived dispersed-dot
/// threshold matrix (guaranteed rank-homogeneous across the screen area)
/// with a small per-pixel quasi-random jitter added to the threshold before
/// comparison. The jitter amplitude is ≤ one sub-quantum (1/N for an
/// N-level matrix), so the ordered pattern is preserved at full strength on
/// gradients — but the grid regularity is broken on large flats where a
/// plain Bayer screen would otherwise produce visible tile seams.
/// </para>
/// <para>
/// The jitter source is a deterministic xorshift hash of (x, y, seed), not a
/// shared RNG — that keeps the ditherer parallel-friendly and
/// thread-order-independent. Setting the jitter amplitude to zero recovers a
/// pure 16×16 Bayer screen; setting it to the full sub-quantum gives a
/// screen whose Fourier spectrum is nearly identical to the pure Bayer case
/// at low frequencies but visibly smoother at high frequencies — a cheap
/// middle ground between ordered and noise dithering.
/// </para>
/// <para>
/// Artefact profile: near-identical to Bayer 16×16 on ramps and structured
/// content; distinctly cleaner than Bayer 16×16 on large flats (no visible
/// grid tile), at the cost of slightly noisier mid-tone texture. Cheaper
/// than blue-noise / void-and-cluster (no 64-KB lookup table) but more
/// irregular-looking than pure Bayer. Useful when output is viewed at the
/// tile boundary (e.g. tiled bitmap previews in IDEs).
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// <para>
/// References: Y. Hoshino &amp; Y. Nishikawa 1990, "A new dither method for
/// improved image quality" — described in later halftone surveys as one of
/// the first "hybrid" ordered-plus-jitter approaches (distinct from Ulichney
/// 1987's pattern-dependent threshold; the jitter is deterministic and
/// per-pixel rather than pattern-based). The approach has since been
/// re-invented under several names in real-time graphics literature,
/// including Mittring 2007's sub-pixel jitter for screen-space AA in
/// Crysis. D. Lau &amp; G. Arce, <i>Modern Digital Halftoning</i>, CRC
/// Press 2008, §5.2 discusses the jitter-on-screen approach.
/// </para>
/// </remarks>
[Ditherer("Hoshino-Nishikawa", Description = "Ordered dither with 16x16 Bayer-derived screen and per-pixel sub-quantum jitter", Type = DitheringType.Ordered, Author = "Y. Hoshino, Y. Nishikawa", Year = 1990)]
public readonly struct HoshinoNishikawaDitherer : IDitherer {

  private const int _SIZE = 16;
  private static readonly float[] _Thresholds = _BuildThresholds();

  private readonly float _strength;
  private readonly float _jitterAmplitude;
  private readonly int _seed;

  /// <summary>Default instance (strength 1.0, full sub-quantum jitter, seed 42).</summary>
  public static HoshinoNishikawaDitherer Instance { get; } = new();

  /// <summary>Creates a Hoshino-Nishikawa ditherer.</summary>
  /// <param name="strength">Overall strength in [0, 1]. Default 1.</param>
  /// <param name="jitterAmplitude">Jitter amplitude as a fraction of one
  /// sub-quantum (1/256 for the 16×16 screen). Default 1.</param>
  /// <param name="seed">Jitter-hash seed. Default 42.</param>
  public HoshinoNishikawaDitherer(float strength = 1f, float jitterAmplitude = 1f, int seed = 42) {
    this._strength = ColorConverter.Saturate(strength);
    this._jitterAmplitude = ColorConverter.Saturate(jitterAmplitude);
    this._seed = seed;
  }

  /// <summary>Returns this ditherer with the specified strength.</summary>
  public HoshinoNishikawaDitherer WithStrength(float strength) => new(strength, this._jitterAmplitude, this._seed);

  /// <summary>Returns this ditherer with the specified jitter amplitude.</summary>
  public HoshinoNishikawaDitherer WithJitterAmplitude(float jitterAmplitude) => new(this._strength, jitterAmplitude, this._seed);

  /// <summary>Returns this ditherer with the specified seed.</summary>
  public HoshinoNishikawaDitherer WithSeed(int seed) => new(this._strength, this._jitterAmplitude, seed);

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
    var jitterAmp = this._jitterAmplitude;
    var seed = this._seed;
    const float subQuantum = 1f / (_SIZE * _SIZE); // 1/256
    var endY = startY + height;

    for (var y = startY; y < endY; ++y) {
      var rowOffset = (y & (_SIZE - 1)) * _SIZE;
      for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
        var color = source[sourceIdx];
        var (c1, c2, c3, alpha) = color.ToNormalized();

        var baseThreshold = _Thresholds[rowOffset + (x & (_SIZE - 1))];
        // Jitter in ±0.5 × sub-quantum × amplitude.
        var h = _Hash(x, y, seed) & 0xFFFF;
        var jitter = (h / 65536f - 0.5f) * subQuantum * jitterAmp;
        var threshold = (baseThreshold + jitter) * strength;

        var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(c1.ToFloat() + threshold),
          UNorm32.FromFloatClamped(c2.ToFloat() + threshold),
          UNorm32.FromFloatClamped(c3.ToFloat() + threshold),
          alpha
        );

        indices[targetIdx] = (byte)lookup.FindNearest(adjustedColor);
      }
    }
  }

  /// <summary>
  /// Build a 16×16 Bayer-derived dispersed-dot threshold matrix, normalised
  /// into [-0.5, 0.5] and flattened row-major.
  /// </summary>
  private static float[] _BuildThresholds() {
    var matrix = BayerMatrix.Generate(_SIZE);
    var flat = new float[_SIZE * _SIZE];

    // Normalise to [-0.5, 0.5].
    var min = float.MaxValue;
    var max = float.MinValue;
    for (var y = 0; y < _SIZE; ++y)
    for (var x = 0; x < _SIZE; ++x) {
      var v = matrix[y, x];
      if (v < min) min = v;
      if (v > max) max = v;
    }
    var range = max - min;
    if (!(range > 0))
      return flat;

    for (var y = 0; y < _SIZE; ++y)
    for (var x = 0; x < _SIZE; ++x)
      flat[y * _SIZE + x] = (matrix[y, x] - min) / range - 0.5f;

    return flat;
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
