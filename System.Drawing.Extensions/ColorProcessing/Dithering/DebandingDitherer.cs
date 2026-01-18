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
/// Debanding ditherer that targets and reduces gradient banding artifacts.
/// </summary>
/// <remarks>
/// <para>
/// This ditherer detects smooth gradient regions by analyzing local color variance
/// and applies stronger dithering in those areas to break up visible banding.
/// </para>
/// <para>
/// In regions with low variance (smooth gradients), the quantization error becomes
/// more visible as distinct bands. By increasing the dithering strength in these
/// regions, the banding is broken up into a less noticeable noise pattern.
/// </para>
/// <para>
/// Areas with high variance (detailed regions, edges) receive normal or reduced
/// dithering to preserve detail.
/// </para>
/// </remarks>
[Ditherer("Debanding", Description = "Targets and reduces gradient banding artifacts", Type = DitheringType.ErrorDiffusion)]
public readonly struct DebandingDitherer : IDitherer {

  private readonly float _gradientThreshold;
  private readonly float _ditherStrength;
  private readonly int _kernelSize;

  /// <summary>Pre-configured instance with default settings.</summary>
  public static DebandingDitherer Default { get; } = new(0.08f, 1.0f);

  /// <summary>Pre-configured instance with strong debanding.</summary>
  public static DebandingDitherer Strong { get; } = new(0.12f, 1.5f);

  /// <summary>Pre-configured instance with gentle debanding.</summary>
  public static DebandingDitherer Gentle { get; } = new(0.06f, 0.7f);

  /// <summary>
  /// Creates a debanding ditherer with the specified settings.
  /// </summary>
  /// <param name="gradientThreshold">Variance threshold for gradient detection (normalized).</param>
  /// <param name="ditherStrength">Strength of error diffusion in gradient areas.</param>
  /// <param name="kernelSize">Size of the local variance kernel.</param>
  public DebandingDitherer(float gradientThreshold = 0.08f, float ditherStrength = 1.0f, int kernelSize = 3) {
    this._gradientThreshold = gradientThreshold;
    this._ditherStrength = ditherStrength;
    this._kernelSize = kernelSize;
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

    // Store source pixels in normalized form for variance calculation
    var sourceColors = new (float c1, float c2, float c3, float a)[height * width];
    for (var y = startY; y < endY; ++y)
    for (var x = 0; x < width; ++x) {
      var pixel = decoder.Decode(source[y * sourceStride + x]);
      var (c1, c2, c3, alpha) = pixel.ToNormalized();
      sourceColors[(y - startY) * width + x] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), alpha.ToFloat());
    }

    // Precompute palette colors in normalized form
    var paletteColors = new (float c1, float c2, float c3, float a)[palette.Length];
    for (var i = 0; i < palette.Length; ++i) {
      var (c1, c2, c3, a) = palette[i].ToNormalized();
      paletteColors[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat());
    }

    // Create error buffer for Floyd-Steinberg-style error diffusion
    var errorBuffer = new float[width + 2, 3];

    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;

      // Swap error buffers (next row becomes current row)
      var currentRow = new float[width + 2, 3];
      for (var x = 0; x <= width + 1; ++x)
      for (var c = 0; c < 3; ++c)
        currentRow[x, c] = errorBuffer[x, c];

      // Clear next row errors
      for (var x = 0; x <= width + 1; ++x)
      for (var c = 0; c < 3; ++c)
        errorBuffer[x, c] = 0;

      for (var x = 0; x < width; ++x) {
        var sourceIdx = localY * width + x;
        var originalColor = sourceColors[sourceIdx];

        // Calculate local variance to detect gradient regions
        var variance = _CalculateLocalVariance(sourceColors, x, localY, width, height, this._kernelSize);

        // Determine if we're in a gradient region (low variance)
        var isGradient = variance < this._gradientThreshold;

        // Calculate dither strength based on gradient detection
        var strength = isGradient ? this._ditherStrength : 0.5f;

        // Apply accumulated error from previous pixels
        var c1 = Math.Max(0f, Math.Min(1f, originalColor.c1 + currentRow[x + 1, 0] * strength));
        var c2 = Math.Max(0f, Math.Min(1f, originalColor.c2 + currentRow[x + 1, 1] * strength));
        var c3 = Math.Max(0f, Math.Min(1f, originalColor.c3 + currentRow[x + 1, 2] * strength));

        var ditheredColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(c1),
          UNorm32.FromFloatClamped(c2),
          UNorm32.FromFloatClamped(c3),
          UNorm32.FromFloatClamped(originalColor.a)
        );

        var paletteIndex = lookup.FindNearest(ditheredColor);
        indices[y * targetStride + x] = (byte)paletteIndex;

        // Calculate quantization error
        var paletteColor = paletteColors[paletteIndex];
        var errorC1 = (c1 - paletteColor.c1) * strength;
        var errorC2 = (c2 - paletteColor.c2) * strength;
        var errorC3 = (c3 - paletteColor.c3) * strength;

        // Distribute error using Floyd-Steinberg pattern
        if (x + 1 < width) {
          currentRow[x + 2, 0] += errorC1 * 7.0f / 16.0f;
          currentRow[x + 2, 1] += errorC2 * 7.0f / 16.0f;
          currentRow[x + 2, 2] += errorC3 * 7.0f / 16.0f;
        }

        if (localY + 1 < height) {
          if (x > 0) {
            errorBuffer[x, 0] += errorC1 * 3.0f / 16.0f;
            errorBuffer[x, 1] += errorC2 * 3.0f / 16.0f;
            errorBuffer[x, 2] += errorC3 * 3.0f / 16.0f;
          }

          errorBuffer[x + 1, 0] += errorC1 * 5.0f / 16.0f;
          errorBuffer[x + 1, 1] += errorC2 * 5.0f / 16.0f;
          errorBuffer[x + 1, 2] += errorC3 * 5.0f / 16.0f;

          if (x + 1 < width) {
            errorBuffer[x + 2, 0] += errorC1 * 1.0f / 16.0f;
            errorBuffer[x + 2, 1] += errorC2 * 1.0f / 16.0f;
            errorBuffer[x + 2, 2] += errorC3 * 1.0f / 16.0f;
          }
        }
      }
    }
  }

  private static float _CalculateLocalVariance(
    (float c1, float c2, float c3, float a)[] sourceColors,
    int px, int py,
    int width, int height,
    int kernelSize) {

    var halfKernel = kernelSize / 2;
    var count = 0;
    var sumC1 = 0.0;
    var sumC2 = 0.0;
    var sumC3 = 0.0;

    // Calculate mean
    for (var dy = -halfKernel; dy <= halfKernel; ++dy)
    for (var dx = -halfKernel; dx <= halfKernel; ++dx) {
      var nx = px + dx;
      var ny = py + dy;

      if (nx < 0 || nx >= width || ny < 0 || ny >= height)
        continue;

      var pixel = sourceColors[ny * width + nx];
      sumC1 += pixel.c1;
      sumC2 += pixel.c2;
      sumC3 += pixel.c3;
      ++count;
    }

    if (count == 0)
      return 0;

    var meanC1 = sumC1 / count;
    var meanC2 = sumC2 / count;
    var meanC3 = sumC3 / count;

    // Calculate variance
    var varianceC1 = 0.0;
    var varianceC2 = 0.0;
    var varianceC3 = 0.0;

    for (var dy = -halfKernel; dy <= halfKernel; ++dy)
    for (var dx = -halfKernel; dx <= halfKernel; ++dx) {
      var nx = px + dx;
      var ny = py + dy;

      if (nx < 0 || nx >= width || ny < 0 || ny >= height)
        continue;

      var pixel = sourceColors[ny * width + nx];
      var diffC1 = pixel.c1 - meanC1;
      var diffC2 = pixel.c2 - meanC2;
      var diffC3 = pixel.c3 - meanC3;

      varianceC1 += diffC1 * diffC1;
      varianceC2 += diffC2 * diffC2;
      varianceC3 += diffC3 * diffC3;
    }

    // Return combined variance (average of RGB variances)
    return (float)((varianceC1 + varianceC2 + varianceC3) / (3.0 * count));
  }
}
