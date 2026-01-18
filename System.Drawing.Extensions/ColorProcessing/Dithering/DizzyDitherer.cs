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
/// Dizzy Dithering algorithm with spiral-based error distribution.
/// </summary>
/// <remarks>
/// <para>
/// This ditherer uses a unique spiral pattern for error diffusion, creating
/// a distinctive visual effect with reduced directional artifacts.
/// </para>
/// <para>
/// The spiral pattern rotates based on position, creating a more chaotic but
/// visually pleasing distribution of quantization errors.
/// </para>
/// </remarks>
[Ditherer("Dizzy", Description = "Spiral-based error distribution dithering", Type = DitheringType.ErrorDiffusion)]
public readonly struct DizzyDitherer : IDitherer {

  private readonly float _randomness;
  private readonly int _spiralRadius;
  private readonly int _seed;

  /// <summary>Pre-configured instance with default settings.</summary>
  public static DizzyDitherer Default { get; } = new(0.15f, 3);

  /// <summary>Pre-configured instance with high quality settings.</summary>
  public static DizzyDitherer HighQuality { get; } = new(0.1f, 4);

  /// <summary>Pre-configured instance with fast settings.</summary>
  public static DizzyDitherer Fast { get; } = new(0.2f, 2);

  /// <summary>
  /// Creates a dizzy ditherer with the specified settings.
  /// </summary>
  /// <param name="randomness">Amount of randomness in spiral pattern (0-1).</param>
  /// <param name="spiralRadius">Radius of the spiral pattern.</param>
  /// <param name="seed">Random seed for reproducibility.</param>
  public DizzyDitherer(float randomness = 0.15f, int spiralRadius = 3, int seed = 42) {
    this._randomness = Math.Max(0f, Math.Min(1f, randomness));
    this._spiralRadius = Math.Max(1, Math.Min(6, spiralRadius));
    this._seed = seed;
  }

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

    // Precompute palette colors in normalized form
    var paletteColors = new (float c1, float c2, float c3, float a)[palette.Length];
    for (var i = 0; i < palette.Length; ++i) {
      var (c1, c2, c3, a) = palette[i].ToNormalized();
      paletteColors[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat());
    }

    var errorC1 = new float[width, height];
    var errorC2 = new float[width, height];
    var errorC3 = new float[width, height];

    var random = new Random(this._seed + startY);
    var spiralPattern = this._GenerateSpiralPattern(random);

    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;
      for (var x = 0; x < width; ++x) {
        var pixel = decoder.Decode(source[y * sourceStride + x]);
        var (c1, c2, c3, alpha) = pixel.ToNormalized();

        var newC1 = Math.Max(0f, Math.Min(1f, c1.ToFloat() + errorC1[x, localY]));
        var newC2 = Math.Max(0f, Math.Min(1f, c2.ToFloat() + errorC2[x, localY]));
        var newC3 = Math.Max(0f, Math.Min(1f, c3.ToFloat() + errorC3[x, localY]));

        var newColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(newC1),
          UNorm32.FromFloatClamped(newC2),
          UNorm32.FromFloatClamped(newC3),
          alpha
        );

        var closestIndex = lookup.FindNearest(newColor);
        indices[y * targetStride + x] = (byte)closestIndex;

        var closestColor = paletteColors[closestIndex];
        var errC1 = newC1 - closestColor.c1;
        var errC2 = newC2 - closestColor.c2;
        var errC3 = newC3 - closestColor.c3;

        if (Math.Abs(errC1) < 1e-6f && Math.Abs(errC2) < 1e-6f && Math.Abs(errC3) < 1e-6f)
          continue;

        _DistributeErrorDizzy(x, localY, errC1, errC2, errC3, width, height, errorC1, errorC2, errorC3, spiralPattern, random);
      }
    }
  }

  private (int dx, int dy, float weight)[] _GenerateSpiralPattern(Random random) {
    var pattern = new List<(int dx, int dy, float weight)>();
    var totalWeight = 0f;

    for (var radius = 1; radius <= this._spiralRadius; ++radius) {
      var angleStep = (float)(2 * Math.PI / (radius * 6));

      for (var angle = 0f; angle < 2 * Math.PI; angle += angleStep) {
        var randomAngle = angle + ((float)random.NextDouble() - 0.5f) * this._randomness;
        var randomRadius = radius + ((float)random.NextDouble() - 0.5f) * this._randomness * 0.5f;

        var dx = (int)Math.Round(randomRadius * Math.Cos(randomAngle));
        var dy = (int)Math.Round(randomRadius * Math.Sin(randomAngle));

        if (dx == 0 && dy == 0) continue;
        if (pattern.Exists(p => p.dx == dx && p.dy == dy)) continue;

        var distance = (float)Math.Sqrt(dx * dx + dy * dy);
        var weight = this._CalculateBlueNoiseWeight(distance, dx, dy);

        pattern.Add((dx, dy, weight));
        totalWeight += weight;
      }
    }

    for (var i = 0; i < pattern.Count; ++i) {
      var (dx, dy, weight) = pattern[i];
      pattern[i] = (dx, dy, weight / totalWeight);
    }

    return pattern.ToArray();
  }

  private float _CalculateBlueNoiseWeight(float distance, int dx, int dy) {
    var baseWeight = 1.0f / (1.0f + distance * distance * 0.5f);
    var noiseComponent = (float)(0.5 + 0.3 * Math.Sin(dx * 2.1 + dy * 3.7) + 0.2 * Math.Cos(dx * 1.3 - dy * 2.9));
    var frequency = (float)Math.Sqrt(dx * dx + dy * dy);
    var frequencyBoost = Math.Min(1.0f, frequency / this._spiralRadius);
    return baseWeight * noiseComponent * (0.7f + 0.3f * frequencyBoost);
  }

  private static void _DistributeErrorDizzy(
    int x, int y,
    float errC1, float errC2, float errC3,
    int width, int height,
    float[,] errorC1, float[,] errorC2, float[,] errorC3,
    (int dx, int dy, float weight)[] spiralPattern,
    Random random) {

    var timePhase = (x * 7 + y * 11) % 100 / 100.0f;

    foreach (var (dx, dy, weight) in spiralPattern) {
      var rotationAngle = timePhase * (float)Math.PI * 0.25f + (x + y) * 0.1f;
      var cos = (float)Math.Cos(rotationAngle);
      var sin = (float)Math.Sin(rotationAngle);

      var rotatedDx = (int)Math.Round(dx * cos - dy * sin);
      var rotatedDy = (int)Math.Round(dx * sin + dy * cos);

      var targetX = x + rotatedDx;
      var targetY = y + rotatedDy;

      if (targetX < 0 || targetX >= width || targetY < 0 || targetY >= height)
        continue;

      var randomWeight = weight * (0.8f + 0.4f * (float)random.NextDouble());

      errorC1[targetX, targetY] += errC1 * randomWeight;
      errorC2[targetX, targetY] += errC2 * randomWeight;
      errorC3[targetX, targetY] += errC3 * randomWeight;
    }
  }
}
