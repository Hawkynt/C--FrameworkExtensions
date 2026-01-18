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
/// Gradient-aware dithering that adapts error diffusion based on local image gradients.
/// </summary>
/// <remarks>
/// <para>This ditherer analyzes local gradients to reduce dithering artifacts in smooth gradient areas</para>
/// <para>while maintaining detail in edges and textured regions.</para>
/// </remarks>
[Ditherer("Gradient Aware", Description = "Adapts error diffusion based on local gradients", Type = DitheringType.ErrorDiffusion)]
public readonly struct GradientAwareDitherer : IDitherer {

  private readonly float _edgeThreshold;
  private readonly float _gradientStrength;

  /// <summary>Pre-configured instance with default settings.</summary>
  public static GradientAwareDitherer Default { get; } = new(0.1f, 1.0f);

  /// <summary>Pre-configured instance with soft settings.</summary>
  public static GradientAwareDitherer Soft { get; } = new(0.05f, 0.5f);

  /// <summary>Pre-configured instance with strong settings.</summary>
  public static GradientAwareDitherer Strong { get; } = new(0.2f, 1.5f);

  /// <summary>
  /// Creates a gradient-aware ditherer with the specified settings.
  /// </summary>
  /// <param name="edgeThreshold">Threshold for edge detection (0-1).</param>
  /// <param name="gradientStrength">Strength of gradient-based modulation.</param>
  public GradientAwareDitherer(float edgeThreshold = 0.1f, float gradientStrength = 1.0f) {
    this._edgeThreshold = edgeThreshold;
    this._gradientStrength = gradientStrength;
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

    // Store source pixels in normalized form
    var sourceColors = new (float c1, float c2, float c3, float a)[height, width];
    for (var y = startY; y < endY; ++y)
    for (var x = 0; x < width; ++x) {
      var pixel = decoder.Decode(source[y * sourceStride + x]);
      var (c1, c2, c3, alpha) = pixel.ToNormalized();
      sourceColors[y - startY, x] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), alpha.ToFloat());
    }

    // Precompute palette colors in normalized form
    var paletteColors = new (float c1, float c2, float c3, float a)[palette.Length];
    for (var i = 0; i < palette.Length; ++i) {
      var (c1, c2, c3, a) = palette[i].ToNormalized();
      paletteColors[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat());
    }

    // Pre-calculate gradient magnitudes
    var gradients = _CalculateGradients(sourceColors, width, height);

    // Error buffers for error diffusion
    var errorC1 = new float[width + 2, 2];
    var errorC2 = new float[width + 2, 2];
    var errorC3 = new float[width + 2, 2];

    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;
      var serpentine = (y & 1) == 1;
      var xStart = serpentine ? width - 1 : 0;
      var xEnd = serpentine ? -1 : width;
      var xStep = serpentine ? -1 : 1;

      // Clear next row's error
      for (var i = 0; i < width + 2; ++i) {
        errorC1[i, 1] = 0;
        errorC2[i, 1] = 0;
        errorC3[i, 1] = 0;
      }

      for (var x = xStart; x != xEnd; x += xStep) {
        var originalColor = sourceColors[localY, x];

        // Get accumulated error
        var xi = x + 1;
        var eC1 = errorC1[xi, 0];
        var eC2 = errorC2[xi, 0];
        var eC3 = errorC3[xi, 0];

        // Get gradient magnitude at this pixel
        var gradient = gradients[x, localY];

        // Adjust error diffusion strength based on gradient
        var diffusionStrength = gradient < this._edgeThreshold
          ? gradient / this._edgeThreshold * this._gradientStrength
          : this._gradientStrength;

        // Apply error to color
        var c1 = Math.Max(0f, Math.Min(1f, originalColor.c1 + eC1 * diffusionStrength));
        var c2 = Math.Max(0f, Math.Min(1f, originalColor.c2 + eC2 * diffusionStrength));
        var c3 = Math.Max(0f, Math.Min(1f, originalColor.c3 + eC3 * diffusionStrength));

        var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(c1),
          UNorm32.FromFloatClamped(c2),
          UNorm32.FromFloatClamped(c3),
          UNorm32.FromFloatClamped(originalColor.a)
        );

        var closestColorIndex = lookup.FindNearest(adjustedColor);
        indices[y * targetStride + x] = (byte)closestColorIndex;

        var closestColor = paletteColors[closestColorIndex];

        // Calculate quantization error
        var newEC1 = c1 - closestColor.c1;
        var newEC2 = c2 - closestColor.c2;
        var newEC3 = c3 - closestColor.c3;

        // Distribute error (Floyd-Steinberg pattern)
        if (serpentine) {
          errorC1[xi - 1, 0] += newEC1 * 7 / 16;
          errorC2[xi - 1, 0] += newEC2 * 7 / 16;
          errorC3[xi - 1, 0] += newEC3 * 7 / 16;

          errorC1[xi + 1, 1] += newEC1 * 3 / 16;
          errorC2[xi + 1, 1] += newEC2 * 3 / 16;
          errorC3[xi + 1, 1] += newEC3 * 3 / 16;

          errorC1[xi, 1] += newEC1 * 5 / 16;
          errorC2[xi, 1] += newEC2 * 5 / 16;
          errorC3[xi, 1] += newEC3 * 5 / 16;

          errorC1[xi - 1, 1] += newEC1 * 1 / 16;
          errorC2[xi - 1, 1] += newEC2 * 1 / 16;
          errorC3[xi - 1, 1] += newEC3 * 1 / 16;
        } else {
          errorC1[xi + 1, 0] += newEC1 * 7 / 16;
          errorC2[xi + 1, 0] += newEC2 * 7 / 16;
          errorC3[xi + 1, 0] += newEC3 * 7 / 16;

          errorC1[xi - 1, 1] += newEC1 * 3 / 16;
          errorC2[xi - 1, 1] += newEC2 * 3 / 16;
          errorC3[xi - 1, 1] += newEC3 * 3 / 16;

          errorC1[xi, 1] += newEC1 * 5 / 16;
          errorC2[xi, 1] += newEC2 * 5 / 16;
          errorC3[xi, 1] += newEC3 * 5 / 16;

          errorC1[xi + 1, 1] += newEC1 * 1 / 16;
          errorC2[xi + 1, 1] += newEC2 * 1 / 16;
          errorC3[xi + 1, 1] += newEC3 * 1 / 16;
        }
      }

      // Swap error buffers
      for (var i = 0; i < width + 2; ++i) {
        errorC1[i, 0] = errorC1[i, 1];
        errorC2[i, 0] = errorC2[i, 1];
        errorC3[i, 0] = errorC3[i, 1];
      }
    }
  }

  private static float[,] _CalculateGradients(
    (float c1, float c2, float c3, float a)[,] sourceColors,
    int width, int height) {

    var gradients = new float[width, height];

    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var c = sourceColors[y, x];

      // Get neighbor colors
      var left = x > 0 ? sourceColors[y, x - 1] : c;
      var right = x < width - 1 ? sourceColors[y, x + 1] : c;
      var top = y > 0 ? sourceColors[y - 1, x] : c;
      var bottom = y < height - 1 ? sourceColors[y + 1, x] : c;

      // Calculate gradient using luminance
      var lumCenter = _Luminance(c);
      var lumLeft = _Luminance(left);
      var lumRight = _Luminance(right);
      var lumTop = _Luminance(top);
      var lumBottom = _Luminance(bottom);

      var gx = lumRight - lumLeft;
      var gy = lumBottom - lumTop;

      gradients[x, y] = (float)Math.Sqrt(gx * gx + gy * gy);
    }

    // Normalize gradients
    var maxGradient = 0f;
    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x)
      if (gradients[x, y] > maxGradient)
        maxGradient = gradients[x, y];

    if (maxGradient > 0)
      for (var y = 0; y < height; ++y)
      for (var x = 0; x < width; ++x)
        gradients[x, y] /= maxGradient;

    return gradients;
  }

  private static float _Luminance((float c1, float c2, float c3, float a) c)
    => 0.299f * c.c1 + 0.587f * c.c2 + 0.114f * c.c3;
}
