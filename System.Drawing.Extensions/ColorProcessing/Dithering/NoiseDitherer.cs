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
/// Selects how the noise value drives the per-pixel palette decision.
/// </summary>
public enum NoiseMode {
  /// <summary>
  /// Noise picks between nearest and second-nearest palette colors based on a threshold
  /// derived from how far the source sits between them. Default; avoids clamping-at-edges
  /// and preserves palette fidelity. (Classic blue-noise-style dithering.)
  /// </summary>
  ThresholdSelection = 0,
  /// <summary>
  /// Noise is added to the source color before nearest-neighbor lookup, producing
  /// a more "random grain" look similar to analog film noise. Simpler algorithm, often
  /// preferred for posterization effects and retro aesthetics.
  /// </summary>
  AdditivePerturb,
}

/// <summary>
/// Defines the type of noise used for dithering.
/// </summary>
public enum NoiseType {
  /// <summary>White noise: uniform random distribution with equal energy at all frequencies.</summary>
  White,
  /// <summary>Blue noise: high-frequency emphasis, reduced low-frequency content. Produces even spatial distribution.</summary>
  Blue,
  /// <summary>Pink noise (1/f): equal energy per octave, reduced high-frequency content. More natural-looking than white noise.</summary>
  Pink,
  /// <summary>Brown/Red noise (1/f²): strong low-frequency emphasis (Brownian motion). Smooth, organic appearance.</summary>
  Brown,
  /// <summary>Violet noise (f): high-frequency emphasis, opposite of pink. Sharp, textured appearance.</summary>
  Violet,
  /// <summary>Grey noise: perceptually uniform noise adjusted for human vision response.</summary>
  Grey
}

/// <summary>
/// Noise-based ditherer using random or blue noise patterns.
/// </summary>
/// <remarks>
/// <para>Noise dithering adds random threshold values before quantization.</para>
/// <para>White noise is simple but may show clumping. Blue noise produces more even distribution.</para>
/// <para>Unlike error diffusion, pixels can be processed independently (parallelizable).</para>
/// </remarks>
[Ditherer("Noise Dithering", Description = "Random noise-based dithering", Type = DitheringType.Random)]
public readonly struct NoiseDitherer : IDitherer {

  #region properties

  /// <summary>The type of noise used.</summary>
  public NoiseType NoiseType { get; }

  /// <summary>Dithering strength (0-1). Higher values produce more visible noise.</summary>
  public float Strength { get; }

  /// <summary>Random seed for reproducible results.</summary>
  public int Seed { get; }

  /// <summary>How the noise value is applied to palette selection.</summary>
  public NoiseMode Mode { get; }

  #endregion

  #region fluent API

  /// <summary>Returns this ditherer with specified strength.</summary>
  public NoiseDitherer WithStrength(float strength) => new(this.NoiseType, strength, this.Seed, this.Mode);

  /// <summary>Returns this ditherer with specified seed for reproducible results.</summary>
  public NoiseDitherer WithSeed(int seed) => new(this.NoiseType, this.Strength, seed, this.Mode);

  /// <summary>Returns this ditherer with the specified noise-application mode.</summary>
  public NoiseDitherer WithMode(NoiseMode mode) => new(this.NoiseType, this.Strength, this.Seed, mode);

  #endregion

  #region constructors

  /// <summary>
  /// Creates a noise ditherer.
  /// </summary>
  /// <param name="noiseType">Type of noise pattern.</param>
  /// <param name="strength">Dithering strength (0-1). Default is 1.</param>
  /// <param name="seed">Random seed. Default is 42 for reproducibility.</param>
  /// <param name="mode">How noise drives the palette decision. Default is threshold-based selection.</param>
  public NoiseDitherer(NoiseType noiseType = NoiseType.White, float strength = 1f, int seed = 42, NoiseMode mode = NoiseMode.ThresholdSelection) {
    this.NoiseType = noiseType;
    this.Strength = Math.Max(0, Math.Min(1, strength));
    this.Seed = seed;
    this.Mode = mode;
  }

  #endregion

  #region IDitherer

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
    // Switch happens ONCE here, not per-pixel - each noise type uses specialized code path
    if (this.Mode == NoiseMode.AdditivePerturb) {
      switch (this.NoiseType) {
        case NoiseType.Blue:
          _DitherAdditive<TWork, TPixel, TDecode, TMetric, BlueNoiseGenerator>(
            source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(), this.Strength);
          return;
        case NoiseType.Pink:
          _DitherAdditive<TWork, TPixel, TDecode, TMetric, PinkNoiseGenerator>(
            source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
          return;
        case NoiseType.Brown:
          _DitherAdditive<TWork, TPixel, TDecode, TMetric, BrownNoiseGenerator>(
            source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
          return;
        case NoiseType.Violet:
          _DitherAdditive<TWork, TPixel, TDecode, TMetric, VioletNoiseGenerator>(
            source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
          return;
        case NoiseType.Grey:
          _DitherAdditive<TWork, TPixel, TDecode, TMetric, GreyNoiseGenerator>(
            source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
          return;
        default:
          _DitherAdditive<TWork, TPixel, TDecode, TMetric, WhiteNoiseGenerator>(
            source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
          return;
      }
    }

    switch (this.NoiseType) {
      case NoiseType.White:
        _DitherWithNoise<TWork, TPixel, TDecode, TMetric, WhiteNoiseGenerator>(
          source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
        break;
      case NoiseType.Blue:
        _DitherWithNoise<TWork, TPixel, TDecode, TMetric, BlueNoiseGenerator>(
          source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(), this.Strength);
        break;
      case NoiseType.Pink:
        _DitherWithNoise<TWork, TPixel, TDecode, TMetric, PinkNoiseGenerator>(
          source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
        break;
      case NoiseType.Brown:
        _DitherWithNoise<TWork, TPixel, TDecode, TMetric, BrownNoiseGenerator>(
          source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
        break;
      case NoiseType.Violet:
        _DitherWithNoise<TWork, TPixel, TDecode, TMetric, VioletNoiseGenerator>(
          source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
        break;
      case NoiseType.Grey:
        _DitherWithNoise<TWork, TPixel, TDecode, TMetric, GreyNoiseGenerator>(
          source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
        break;
      default:
        _DitherWithNoise<TWork, TPixel, TDecode, TMetric, WhiteNoiseGenerator>(
          source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette, new(this.Seed), this.Strength);
        break;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _DitherAdditive<TWork, TPixel, TDecode, TMetric, TNoiseGen>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette,
    TNoiseGen noiseGen,
    float strength)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork>
    where TNoiseGen : struct, INoiseGenerator {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;

    for (var y = startY; y < endY; ++y)
    for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
      var color = decoder.Decode(source[sourceIdx]);
      var (c1, c2, c3, a) = color.ToNormalized();

      // Perturb each channel by (noise * strength). Noise generator returns [-0.5, 0.5].
      var noise = noiseGen.GetThreshold(x, y) * strength;
      var n1 = UNorm32.FromFloatClamped(c1.ToFloat() + noise);
      var n2 = UNorm32.FromFloatClamped(c2.ToFloat() + noise);
      var n3 = UNorm32.FromFloatClamped(c3.ToFloat() + noise);

      var perturbed = ColorFactory.FromNormalized_4<TWork>(n1, n2, n3, a);
      indices[targetIdx] = (byte)lookup.FindNearest(perturbed, out _);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _DitherWithNoise<TWork, TPixel, TDecode, TMetric, TNoiseGen>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette,
    TNoiseGen noiseGen,
    float strength)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork>
    where TNoiseGen : struct, INoiseGenerator {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;

    for (var y = startY; y < endY; ++y)
    for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
      var color = decoder.Decode(source[sourceIdx]);

      // Find the two closest palette colors
      var nearestIdx = lookup.FindNearest(color, out var nearestColor);
      var secondNearestIdx = _FindSecondNearest(color, palette, nearestIdx, lookup);

      // Calculate how far the original is between the nearest and second-nearest colors
      var (c1, c2, c3, _) = color.ToNormalized();
      var (n1, n2, n3, _) = nearestColor.ToNormalized();
      var (s1, s2, s3, _) = palette[secondNearestIdx].ToNormalized();

      var distToNearest = Math.Abs(c1.ToFloat() - n1.ToFloat()) +
                          Math.Abs(c2.ToFloat() - n2.ToFloat()) +
                          Math.Abs(c3.ToFloat() - n3.ToFloat());

      var distToSecond = Math.Abs(c1.ToFloat() - s1.ToFloat()) +
                         Math.Abs(c2.ToFloat() - s2.ToFloat()) +
                         Math.Abs(c3.ToFloat() - s3.ToFloat());

      var totalDist = distToNearest + distToSecond;

      // Direct call to struct method - devirtualized by JIT
      // Get noise value and clamp to [0, 1] range
      var noiseValue = noiseGen.GetThreshold(x, y);
      var threshold = Math.Max(0f, Math.Min(1f, (noiseValue + 0.5f) * strength));

      // Use threshold to decide between nearest and second-nearest based on relative distances
      // ratio = 0 means we're exactly on nearest, ratio = 1 means we're exactly on second-nearest
      var ratio = totalDist > 0.001f ? distToNearest / totalDist : 0f;
      // Only select second-nearest if we're meaningfully between colors AND threshold exceeds our position
      var selectedIdx = threshold > 1f - ratio ? secondNearestIdx : nearestIdx;

      indices[targetIdx] = (byte)selectedIdx;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _FindSecondNearest<TWork, TMetric>(
    TWork color,
    TWork[] palette,
    int excludeIndex,
    in PaletteLookup<TWork, TMetric> lookup)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    if (palette.Length <= 1)
      return excludeIndex;

    var (c1, c2, c3, _) = color.ToNormalized();
    var bestIdx = excludeIndex == 0 ? 1 : 0;
    var bestDist = float.MaxValue;

    for (var i = 0; i < palette.Length; ++i) {
      if (i == excludeIndex)
        continue;

      var (p1, p2, p3, _) = palette[i].ToNormalized();
      var dist = Math.Abs(c1.ToFloat() - p1.ToFloat()) +
                 Math.Abs(c2.ToFloat() - p2.ToFloat()) +
                 Math.Abs(c3.ToFloat() - p3.ToFloat());

      if (dist < bestDist) {
        bestDist = dist;
        bestIdx = i;
      }
    }

    return bestIdx;
  }

  #endregion

  #region pre-configured instances

  /// <summary>White noise dithering: uniform random threshold with equal energy at all frequencies.</summary>
  public static NoiseDitherer WhiteNoise { get; } = new(NoiseType.White);

  /// <summary>Blue noise dithering: spatially-filtered noise for more even distribution.</summary>
  public static NoiseDitherer BlueNoise { get; } = new(NoiseType.Blue);

  /// <summary>Pink noise dithering (1/f): equal energy per octave, more natural-looking than white noise.</summary>
  public static NoiseDitherer PinkNoise { get; } = new(NoiseType.Pink);

  /// <summary>Brown/Red noise dithering (1/f²): strong low-frequency emphasis, smooth organic appearance.</summary>
  public static NoiseDitherer BrownNoise { get; } = new(NoiseType.Brown);

  /// <summary>Violet noise dithering (f): high-frequency emphasis, sharp textured appearance.</summary>
  public static NoiseDitherer VioletNoise { get; } = new(NoiseType.Violet);

  /// <summary>Grey noise dithering: perceptually uniform noise adjusted for human vision.</summary>
  public static NoiseDitherer GreyNoise { get; } = new(NoiseType.Grey);

  /// <summary>White-noise additive dithering: source color perturbed then nearest-neighbor lookup.</summary>
  public static NoiseDitherer WhiteNoiseAdditive { get; } = new(NoiseType.White, 0.5f, 42, NoiseMode.AdditivePerturb);

  /// <summary>Blue-noise additive dithering: source color perturbed then nearest-neighbor lookup.</summary>
  public static NoiseDitherer BlueNoiseAdditive { get; } = new(NoiseType.Blue, 0.5f, 42, NoiseMode.AdditivePerturb);

  /// <summary>Pink-noise additive dithering: source color perturbed then nearest-neighbor lookup.</summary>
  public static NoiseDitherer PinkNoiseAdditive { get; } = new(NoiseType.Pink, 0.5f, 42, NoiseMode.AdditivePerturb);

  /// <summary>Brown-noise additive dithering: source color perturbed then nearest-neighbor lookup.</summary>
  public static NoiseDitherer BrownNoiseAdditive { get; } = new(NoiseType.Brown, 0.5f, 42, NoiseMode.AdditivePerturb);

  /// <summary>Violet-noise additive dithering: source color perturbed then nearest-neighbor lookup.</summary>
  public static NoiseDitherer VioletNoiseAdditive { get; } = new(NoiseType.Violet, 0.5f, 42, NoiseMode.AdditivePerturb);

  /// <summary>Grey-noise additive dithering: source color perturbed then nearest-neighbor lookup.</summary>
  public static NoiseDitherer GreyNoiseAdditive { get; } = new(NoiseType.Grey, 0.5f, 42, NoiseMode.AdditivePerturb);

  #endregion

}

#region Noise Generator Interface

/// <summary>
/// Interface for noise generators - zero-cost abstraction for devirtualization.
/// </summary>
internal interface INoiseGenerator {
  /// <summary>Gets the noise threshold value at the specified position.</summary>
  float GetThreshold(int x, int y);
}

#endregion

#region Noise Generator Implementations

/// <summary>
/// White noise generator: uniform random distribution.
/// </summary>
internal readonly struct WhiteNoiseGenerator(int seed) : INoiseGenerator {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float GetThreshold(int x, int y) {
    var hash = HashPosition(x, y, seed);
    return (hash & 0xFFFF) / 65536f - 0.5f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int HashPosition(int x, int y, int seed) {
    var h = seed;
    h ^= x * 374761393;
    h ^= y * 668265263;
    h = (h ^ (h >> 15)) * 1103515245;
    h = h ^ (h >> 13);
    return h;
  }
}

/// <summary>
/// Blue noise generator: spatially-filtered noise pattern.
/// </summary>
internal readonly struct BlueNoiseGenerator : INoiseGenerator {
  private static readonly float[] _blueNoise64 = GenerateBlueNoise64();
  private static readonly double _GOLDEN = (Math.Sqrt(5) - 1) / 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float GetThreshold(int x, int y)
    => _blueNoise64[(y & 63) * 64 + (x & 63)];

  private static float[] GenerateBlueNoise64() {
    const int size = 64;
    var result = new float[size * size];

    for (var i = 0; i < result.Length; ++i) {
      var x = i % size;
      var y = i / size;
      var r2x = (0.5f + _GOLDEN * x) % 1.0;
      var r2y = (0.5f + _GOLDEN * _GOLDEN * y) % 1.0;
      var noise = (float)((r2x + r2y + Math.Sin(x * 0.7) * 0.1 + Math.Cos(y * 0.7) * 0.1) % 1.0);
      result[i] = noise - 0.5f;
    }

    return result;
  }
}

/// <summary>
/// Pink noise generator: 1/f noise using octave summation.
/// </summary>
internal readonly struct PinkNoiseGenerator(int seed) : INoiseGenerator {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float GetThreshold(int x, int y) {
    var noise = 0f;
    var amplitude = 0.5f;
    var freq = 1;

    for (var octave = 0; octave < 4; ++octave) {
      var hash = HashPosition(x * freq, y * freq, seed + octave);
      noise += ((hash & 0xFFFF) / 65536f - 0.5f) * amplitude;
      amplitude *= 0.5f;
      freq *= 2;
    }

    return noise * 1.067f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int HashPosition(int x, int y, int seed) {
    var h = seed;
    h ^= x * 374761393;
    h ^= y * 668265263;
    h = (h ^ (h >> 15)) * 1103515245;
    h = h ^ (h >> 13);
    return h;
  }
}

/// <summary>
/// Brown/Red noise generator: 1/f² noise with spatial smoothing.
/// </summary>
internal readonly struct BrownNoiseGenerator(int seed) : INoiseGenerator {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float GetThreshold(int x, int y) {
    var sum = 0f;
    const int range = 3;

    for (var dy = -range; dy <= range; ++dy)
    for (var dx = -range; dx <= range; ++dx) {
      var weight = 1f / (1 + Math.Abs(dx) + Math.Abs(dy));
      var hash = HashPosition(x + dx, y + dy, seed);
      sum += ((hash & 0xFFFF) / 65536f - 0.5f) * weight;
    }

    return sum / 4.2f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int HashPosition(int x, int y, int seed) {
    var h = seed;
    h ^= x * 374761393;
    h ^= y * 668265263;
    h = (h ^ (h >> 15)) * 1103515245;
    h = h ^ (h >> 13);
    return h;
  }
}

/// <summary>
/// Violet noise generator: f noise using differentiation.
/// </summary>
internal readonly struct VioletNoiseGenerator(int seed) : INoiseGenerator {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float GetThreshold(int x, int y) {
    var center = HashPosition(x, y, seed) & 0xFFFF;
    var right = HashPosition(x + 1, y, seed) & 0xFFFF;
    var down = HashPosition(x, y + 1, seed) & 0xFFFF;

    var dx = (right - center) / 65536f;
    var dy = (down - center) / 65536f;

    return (dx + dy) * 0.707f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int HashPosition(int x, int y, int seed) {
    var h = seed;
    h ^= x * 374761393;
    h ^= y * 668265263;
    h = (h ^ (h >> 15)) * 1103515245;
    h = h ^ (h >> 13);
    return h;
  }
}

/// <summary>
/// Grey noise generator: perceptually uniform blended noise.
/// </summary>
internal readonly struct GreyNoiseGenerator(int seed) : INoiseGenerator {
  private static readonly float[] _blueNoise64 = GenerateBlueNoise64();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float GetThreshold(int x, int y) {
    var white = this.GetWhiteNoise(x, y);
    var blue = _blueNoise64[(y & 63) * 64 + (x & 63)];
    return white * 0.4f + blue * 0.6f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float GetWhiteNoise(int x, int y) {
    var hash = HashPosition(x, y, seed);
    return (hash & 0xFFFF) / 65536f - 0.5f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int HashPosition(int x, int y, int seed) {
    var h = seed;
    h ^= x * 374761393;
    h ^= y * 668265263;
    h = (h ^ (h >> 15)) * 1103515245;
    h = h ^ (h >> 13);
    return h;
  }

  private static float[] GenerateBlueNoise64() {
    const int size = 64;
    var result = new float[size * size];
    var golden = (Math.Sqrt(5) - 1) / 2;

    for (var i = 0; i < result.Length; ++i) {
      var x = i % size;
      var y = i / size;
      var r2x = (0.5f + golden * x) % 1.0;
      var r2y = (0.5f + golden * golden * y) % 1.0;
      var noise = (float)((r2x + r2y + Math.Sin(x * 0.7) * 0.1 + Math.Cos(y * 0.7) * 0.1) % 1.0);
      result[i] = noise - 0.5f;
    }

    return result;
  }
}

#endregion
