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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// N-Closest dithering algorithm that finds the N closest colors and selects from them.
/// </summary>
/// <remarks>
/// <para>Supports multiple selection strategies: Random, WeightedRandom, RoundRobin, Luminance, and BlueNoise.</para>
/// </remarks>
[Ditherer("N-Closest", Description = "Selection from N closest palette colors", Type = DitheringType.Random)]
public readonly struct NClosestDitherer : IDitherer {

  /// <summary>
  /// Strategy for selecting among N closest colors.
  /// </summary>
  public enum SelectionStrategy {
    /// <summary>Uniform random selection.</summary>
    Random,
    /// <summary>Random selection weighted by inverse distance.</summary>
    WeightedRandom,
    /// <summary>Cycle through colors based on position.</summary>
    RoundRobin,
    /// <summary>Select color with closest luminance.</summary>
    Luminance,
    /// <summary>Select using blue noise texture.</summary>
    BlueNoise
  }

  private readonly int _n;
  private readonly SelectionStrategy _strategy;
  private readonly int _seed;

  /// <summary>Pre-configured instance with random selection from 3 closest.</summary>
  public static NClosestDitherer Default { get; } = new(3, SelectionStrategy.Random);

  /// <summary>Pre-configured instance with weighted random selection from 5 closest.</summary>
  public static NClosestDitherer WeightedRandom5 { get; } = new(5, SelectionStrategy.WeightedRandom);

  /// <summary>Pre-configured instance with round robin selection from 4 closest.</summary>
  public static NClosestDitherer RoundRobin4 { get; } = new(4, SelectionStrategy.RoundRobin);

  /// <summary>Pre-configured instance with luminance-based selection from 6 closest.</summary>
  public static NClosestDitherer Luminance6 { get; } = new(6, SelectionStrategy.Luminance);

  /// <summary>Pre-configured instance with blue noise selection from 4 closest.</summary>
  public static NClosestDitherer BlueNoise4 { get; } = new(4, SelectionStrategy.BlueNoise);

  /// <summary>
  /// Creates an N-Closest ditherer with the specified settings.
  /// </summary>
  /// <param name="n">Number of closest colors to consider.</param>
  /// <param name="strategy">Selection strategy for choosing among closest colors.</param>
  /// <param name="seed">Random seed for reproducibility.</param>
  public NClosestDitherer(int n = 3, SelectionStrategy strategy = SelectionStrategy.Random, int seed = 42) {
    this._n = Math.Max(1, n);
    this._strategy = strategy;
    this._seed = seed;
  }

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => this._strategy is SelectionStrategy.Random or SelectionStrategy.WeightedRandom;

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

    var random = new Random(this._seed + startY);
    var endY = startY + height;

    // Pre-generate blue noise texture if needed
    byte[,]? blueNoiseTexture = null;
    if (this._strategy == SelectionStrategy.BlueNoise)
      blueNoiseTexture = _GenerateBlueNoiseTexture();

    // Precompute palette colors in normalized form for luminance calculations
    var paletteColors = new (float c1, float c2, float c3, float a)[palette.Length];
    for (var i = 0; i < palette.Length; ++i) {
      var (c1, c2, c3, a) = palette[i].ToNormalized();
      paletteColors[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat());
    }

    for (var y = startY; y < endY; ++y)
    for (var x = 0; x < width; ++x) {
      var pixel = decoder.Decode(source[y * sourceStride + x]);
      var closestColors = _FindNClosestColors(pixel, palette, metric, this._n);

      if (closestColors.Count == 0) {
        indices[y * targetStride + x] = 0;
        continue;
      }

      if (closestColors.Count == 1) {
        indices[y * targetStride + x] = (byte)closestColors[0].index;
        continue;
      }

      var (c1, c2, c3, alpha) = pixel.ToNormalized();
      var pixelNormalized = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), alpha.ToFloat());

      var selectedIndex = this._SelectColor(closestColors, pixelNormalized, paletteColors, x, y, random, blueNoiseTexture);
      indices[y * targetStride + x] = (byte)selectedIndex;
    }
  }

  private static List<(int index, double distance)> _FindNClosestColors<TWork, TMetric>(
    TWork color,
    TWork[] palette,
    in TMetric metric,
    int n)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    if (palette.Length == 0) return [];

    var distances = new List<(int index, double distance)>(palette.Length);

    for (var i = 0; i < palette.Length; ++i) {
      var distance = (double)metric.Distance(color, palette[i]).ToFloat();
      distances.Add((i, distance));
    }

    distances.Sort((a, b) => a.distance.CompareTo(b.distance));

    var result = new List<(int index, double distance)>(Math.Min(n, distances.Count));
    for (var i = 0; i < n && i < distances.Count; ++i)
      result.Add(distances[i]);

    return result;
  }

  private int _SelectColor(
    List<(int index, double distance)> closestColors,
    (float c1, float c2, float c3, float a) pixelNormalized,
    (float c1, float c2, float c3, float a)[] paletteColors,
    int x, int y,
    Random random,
    byte[,]? blueNoiseTexture) {

    if (closestColors.Count == 1)
      return closestColors[0].index;

    return this._strategy switch {
      SelectionStrategy.Random => _SelectRandom(closestColors, random),
      SelectionStrategy.WeightedRandom => _SelectWeightedRandom(closestColors, random),
      SelectionStrategy.RoundRobin => _SelectRoundRobin(closestColors, x, y),
      SelectionStrategy.Luminance => _SelectByLuminance(closestColors, pixelNormalized, paletteColors),
      SelectionStrategy.BlueNoise => _SelectByBlueNoise(closestColors, x, y, blueNoiseTexture!),
      _ => closestColors[0].index
    };
  }

  private static int _SelectRandom(List<(int index, double distance)> closestColors, Random random) {
    var randomIndex = random.Next(closestColors.Count);
    return closestColors[randomIndex].index;
  }

  private static int _SelectWeightedRandom(List<(int index, double distance)> closestColors, Random random) {
    var maxDistance = 0.0;
    foreach (var c in closestColors)
      if (c.distance > maxDistance)
        maxDistance = c.distance;

    var weights = new double[closestColors.Count];
    var totalWeight = 0.0;
    for (var i = 0; i < closestColors.Count; ++i) {
      weights[i] = maxDistance - closestColors[i].distance + 1;
      totalWeight += weights[i];
    }

    if (totalWeight == 0)
      return closestColors[0].index;

    var randomValue = random.NextDouble() * totalWeight;
    var cumulativeWeight = 0.0;

    for (var i = 0; i < weights.Length; ++i) {
      cumulativeWeight += weights[i];
      if (randomValue < cumulativeWeight)
        return closestColors[i].index;
    }

    return closestColors[^1].index;
  }

  private static int _SelectRoundRobin(List<(int index, double distance)> closestColors, int x, int y) {
    var position = (x + y * 37) % closestColors.Count;
    return closestColors[position].index;
  }

  private static int _SelectByLuminance(
    List<(int index, double distance)> closestColors,
    (float c1, float c2, float c3, float a) pixel,
    (float c1, float c2, float c3, float a)[] paletteColors) {

    var originalLuminance = 0.299f * pixel.c1 + 0.587f * pixel.c2 + 0.114f * pixel.c3;

    var bestIndex = 0;
    var bestLuminanceDiff = float.MaxValue;

    for (var i = 0; i < closestColors.Count; ++i) {
      var color = paletteColors[closestColors[i].index];
      var luminance = 0.299f * color.c1 + 0.587f * color.c2 + 0.114f * color.c3;
      var luminanceDiff = Math.Abs(originalLuminance - luminance);

      if (luminanceDiff < bestLuminanceDiff) {
        bestLuminanceDiff = luminanceDiff;
        bestIndex = i;
      }
    }

    return closestColors[bestIndex].index;
  }

  private static int _SelectByBlueNoise(List<(int index, double distance)> closestColors, int x, int y, byte[,] blueNoiseTexture) {
    var noiseWidth = blueNoiseTexture.GetLength(1);
    var noiseHeight = blueNoiseTexture.GetLength(0);
    var noiseValue = blueNoiseTexture[y % noiseHeight, x % noiseWidth];

    var normalizedNoise = noiseValue / 255.0;
    var colorIndex = (int)(normalizedNoise * closestColors.Count);
    colorIndex = Math.Min(colorIndex, closestColors.Count - 1);

    return closestColors[colorIndex].index;
  }

  private static byte[,] _GenerateBlueNoiseTexture() {
    const int textureSize = 64;
    var texture = new byte[textureSize, textureSize];
    var random = new Random(12345);

    for (var y = 0; y < textureSize; ++y)
    for (var x = 0; x < textureSize; ++x)
      texture[y, x] = (byte)random.Next(256);

    var filtered = new byte[textureSize, textureSize];
    for (var y = 0; y < textureSize; ++y)
    for (var x = 0; x < textureSize; ++x) {
      var sum = 0;
      var count = 0;

      for (var dy = -1; dy <= 1; ++dy)
      for (var dx = -1; dx <= 1; ++dx) {
        var ny = (y + dy + textureSize) % textureSize;
        var nx = (x + dx + textureSize) % textureSize;
        sum += texture[ny, nx];
        ++count;
      }

      var average = sum / count;
      var highPass = texture[y, x] - average + 128;
      filtered[y, x] = (byte)Math.Max(0, Math.Min(255, highPass));
    }

    return filtered;
  }
}
