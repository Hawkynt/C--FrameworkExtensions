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
/// Random dithering algorithm that uses random threshold values for quantization.
/// </summary>
/// <remarks>
/// <para>
/// This is one of the simplest dithering techniques, applying random noise to each pixel
/// before quantization. While it effectively breaks up color banding, it can introduce
/// visible grain or speckle patterns in the output image.
/// </para>
/// <para>
/// The algorithm generates a random threshold value for each pixel and adds it to the
/// color components before finding the nearest palette color. This randomization helps
/// distribute quantization errors across the image.
/// </para>
/// <para>
/// Reference: https://www.graphicsacademy.com/what_ditherr.php
/// </para>
/// </remarks>
[Ditherer("Random", Description = "Simple random noise dithering", Type = DitheringType.Random)]
public readonly struct RandomDitherer : IDitherer {

  private readonly float _intensity;
  private readonly int _seed;

  /// <summary>Pre-configured instance with standard intensity (0.5).</summary>
  public static RandomDitherer Instance { get; } = new(0.5f);

  /// <summary>Pre-configured instance with light intensity (0.3).</summary>
  public static RandomDitherer Light { get; } = new(0.3f);

  /// <summary>Pre-configured instance with strong intensity (0.7).</summary>
  public static RandomDitherer Strong { get; } = new(0.7f);

  /// <summary>
  /// Creates a random ditherer with the specified intensity.
  /// </summary>
  /// <param name="intensity">Noise intensity (0-1).</param>
  /// <param name="seed">Optional random seed for reproducibility.</param>
  public RandomDitherer(float intensity = 0.5f, int seed = 42) {
    this._intensity = Math.Max(0f, Math.Min(1f, intensity));
    this._seed = seed;
  }

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
    var intensity = this._intensity;
    var random = new Random(this._seed + startY);
    var endY = startY + height;

    for (var y = startY; y < endY; ++y) {
      for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
        // Decode source pixel
        var pixel = decoder.Decode(source[sourceIdx]);
        var (c1, c2, c3, alpha) = pixel.ToNormalized();
        var pixelC1 = c1.ToFloat();
        var pixelC2 = c2.ToFloat();
        var pixelC3 = c3.ToFloat();
        var pixelA = alpha.ToFloat();

        // Generate random noise value (-1 to 1) and scale by intensity
        var noiseValue = (float)(random.NextDouble() * 2.0 - 1.0) * intensity;

        // Apply noise to each channel
        var adjustedC1 = Math.Max(0f, Math.Min(1f, pixelC1 + noiseValue));
        var adjustedC2 = Math.Max(0f, Math.Min(1f, pixelC2 + noiseValue));
        var adjustedC3 = Math.Max(0f, Math.Min(1f, pixelC3 + noiseValue));

        // Create adjusted color
        var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(adjustedC1),
          UNorm32.FromFloatClamped(adjustedC2),
          UNorm32.FromFloatClamped(adjustedC3),
          UNorm32.FromFloatClamped(pixelA)
        );

        // Find nearest palette color
        indices[targetIdx] = (byte)lookup.FindNearest(adjustedColor);
      }
    }
  }
}
