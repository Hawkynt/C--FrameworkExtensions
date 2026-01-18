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
/// Interleaved Gradient Noise (IGN) dithering algorithm.
/// </summary>
/// <remarks>
/// <para>
/// IGN is a pseudo-random noise function commonly used in modern graphics pipelines,
/// particularly for temporal anti-aliasing and screen-space effects. The algorithm
/// generates a deterministic noise pattern based on pixel coordinates using the formula:
/// IGN(x,y) = frac(52.9829189 * frac(0.06711056*x + 0.00583715*y))
/// </para>
/// <para>
/// This dithering technique is especially useful for:
/// - Real-time graphics and temporal anti-aliasing
/// - Screen-space dithering with good spatial distribution
/// - Reducing banding in gradients with minimal pattern visibility
/// </para>
/// </remarks>
[Ditherer("Interleaved Gradient Noise", Description = "Deterministic noise for smooth dithering", Type = DitheringType.Noise)]
public readonly struct InterleavedGradientNoiseDitherer : IDitherer {

  private readonly float _intensity;

  /// <summary>Pre-configured instance with standard intensity (0.5).</summary>
  public static InterleavedGradientNoiseDitherer Instance { get; } = new(0.5f);

  /// <summary>Pre-configured instance with light intensity (0.3).</summary>
  public static InterleavedGradientNoiseDitherer Light { get; } = new(0.3f);

  /// <summary>Pre-configured instance with strong intensity (0.7).</summary>
  public static InterleavedGradientNoiseDitherer Strong { get; } = new(0.7f);

  /// <summary>
  /// Creates an IGN ditherer with the specified intensity.
  /// </summary>
  /// <param name="intensity">Noise intensity (0-1).</param>
  public InterleavedGradientNoiseDitherer(float intensity = 0.5f) {
    this._intensity = Math.Max(0f, Math.Min(1f, intensity));
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

        // Compute IGN value (-1 to 1) and scale by intensity
        var noiseValue = (float)_ComputeIGN(x, y);
        var threshold = noiseValue * intensity;

        // Apply noise to each channel
        var adjustedC1 = Math.Max(0f, Math.Min(1f, pixelC1 + threshold));
        var adjustedC2 = Math.Max(0f, Math.Min(1f, pixelC2 + threshold));
        var adjustedC3 = Math.Max(0f, Math.Min(1f, pixelC3 + threshold));

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

  /// <summary>
  /// Computes the Interleaved Gradient Noise value for the given coordinates.
  /// Formula: IGN(x,y) = frac(52.9829189 * frac(0.06711056*x + 0.00583715*y))
  /// </summary>
  /// <param name="x">The x coordinate.</param>
  /// <param name="y">The y coordinate.</param>
  /// <returns>A noise value in the range [-1, 1].</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _ComputeIGN(int x, int y) {
    var frac1 = (0.06711056 * x + 0.00583715 * y) % 1.0;
    var frac2 = (52.9829189 * frac1) % 1.0;
    return frac2 * 2.0 - 1.0;
  }
}
