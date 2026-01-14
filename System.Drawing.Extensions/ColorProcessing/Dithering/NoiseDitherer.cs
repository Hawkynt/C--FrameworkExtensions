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

  #endregion

  #region fluent API

  /// <summary>Returns this ditherer with specified strength.</summary>
  public NoiseDitherer WithStrength(float strength) => new(this.NoiseType, strength, this.Seed);

  /// <summary>Returns this ditherer with specified seed for reproducible results.</summary>
  public NoiseDitherer WithSeed(int seed) => new(this.NoiseType, this.Strength, seed);

  #endregion

  #region constructors

  /// <summary>
  /// Creates a noise ditherer.
  /// </summary>
  /// <param name="noiseType">Type of noise pattern.</param>
  /// <param name="strength">Dithering strength (0-1). Default is 1.</param>
  /// <param name="seed">Random seed. Default is 42 for reproducibility.</param>
  public NoiseDitherer(NoiseType noiseType = NoiseType.White, float strength = 1f, int seed = 42) {
    this.NoiseType = noiseType;
    this.Strength = Math.Max(0, Math.Min(1, strength));
    this.Seed = seed;
  }

  #endregion

  #region IDitherer

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => false;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TPixel, TDecode, TEncode, TMetric, TResult>(
    IDithererCallback<TWork, TPixel, TDecode, TEncode, TMetric, TResult> callback,
    int width,
    int height)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TMetric : struct, IColorMetric<TWork>
    // Switch happens ONCE here, not per-pixel - each noise type uses a specialized kernel
    => this.NoiseType switch {
      NoiseType.White => callback.Invoke(new NoiseDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric, WhiteNoiseGenerator>(
        width, height, new WhiteNoiseGenerator(this.Seed), this.Strength)),
      NoiseType.Blue => callback.Invoke(new NoiseDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric, BlueNoiseGenerator>(
        width, height, new BlueNoiseGenerator(), this.Strength)),
      NoiseType.Pink => callback.Invoke(new NoiseDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric, PinkNoiseGenerator>(
        width, height, new PinkNoiseGenerator(this.Seed), this.Strength)),
      NoiseType.Brown => callback.Invoke(new NoiseDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric, BrownNoiseGenerator>(
        width, height, new BrownNoiseGenerator(this.Seed), this.Strength)),
      NoiseType.Violet => callback.Invoke(new NoiseDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric, VioletNoiseGenerator>(
        width, height, new VioletNoiseGenerator(this.Seed), this.Strength)),
      NoiseType.Grey => callback.Invoke(new NoiseDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric, GreyNoiseGenerator>(
        width, height, new GreyNoiseGenerator(this.Seed), this.Strength)),
      _ => callback.Invoke(new NoiseDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric, WhiteNoiseGenerator>(
        width, height, new WhiteNoiseGenerator(this.Seed), this.Strength)),
    };

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

  #endregion

}

#region Noise Generator Interface

/// <summary>
/// Interface for noise generators - zero-cost abstraction for devirtualization.
/// </summary>
file interface INoiseGenerator {
  /// <summary>Gets the noise threshold value at the specified position.</summary>
  float GetThreshold(int x, int y);
}

#endregion

#region Noise Generator Implementations

/// <summary>
/// White noise generator: uniform random distribution.
/// </summary>
file readonly struct WhiteNoiseGenerator(int seed) : INoiseGenerator {
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
file readonly struct BlueNoiseGenerator : INoiseGenerator {
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
file readonly struct PinkNoiseGenerator(int seed) : INoiseGenerator {
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
file readonly struct BrownNoiseGenerator(int seed) : INoiseGenerator {
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
file readonly struct VioletNoiseGenerator(int seed) : INoiseGenerator {
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
file readonly struct GreyNoiseGenerator(int seed) : INoiseGenerator {
  private static readonly float[] _blueNoise64 = GenerateBlueNoise64();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float GetThreshold(int x, int y) {
    var white = GetWhiteNoise(x, y);
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

#region Generic Noise Ditherer Kernel

/// <summary>
/// Generic noise ditherer kernel using zero-cost noise generator abstraction.
/// </summary>
file readonly struct NoiseDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric, TNoiseGen>(
  int width, int height,
  TNoiseGen noiseGenerator, float strength)
  : IDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TPixel : unmanaged, IStorageSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TMetric : struct, IColorMetric<TWork>
  where TNoiseGen : struct, INoiseGenerator {

  public int Width => width;
  public int Height => height;
  public bool RequiresSequentialProcessing => false;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void ProcessOrdered(
    TPixel* source,
    int sourceStride,
    int x, int y,
    TPixel* dest,
    int destStride,
    in TDecode decoder,
    in TEncode encoder,
    in TMetric metric,
    TWork[] palette) {

    var sourceIdx = y * sourceStride + x;
    var color = decoder.Decode(source[sourceIdx]);

    // Direct call to struct method - devirtualized by JIT
    var threshold = noiseGenerator.GetThreshold(x, y) * strength;

    var adjustedColor = ColorFactory.Create4F<TWork>(
      color.C1 + threshold,
      color.C2 + threshold,
      color.C3 + threshold,
      color.A
    );

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var nearestIdx = lookup.FindNearest(adjustedColor);

    var destIdx = y * destStride + x;
    dest[destIdx] = encoder.Encode(lookup[nearestIdx]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void ProcessErrorDiffusion(
    TPixel* source,
    int sourceStride,
    TPixel* dest,
    int destStride,
    in TDecode decoder,
    in TEncode encoder,
    in TMetric metric,
    TWork[] palette) {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);

    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var sourceIdx = y * sourceStride + x;
      var color = decoder.Decode(source[sourceIdx]);

      // Direct call to struct method - devirtualized by JIT
      var threshold = noiseGenerator.GetThreshold(x, y) * strength;

      var adjustedColor = ColorFactory.Create4F<TWork>(
        color.C1 + threshold,
        color.C2 + threshold,
        color.C3 + threshold,
        color.A
      );

      var nearestIdx = lookup.FindNearest(adjustedColor);
      var destIdx = y * destStride + x;
      dest[destIdx] = encoder.Encode(lookup[nearestIdx]);
    }
  }
}

#endregion
