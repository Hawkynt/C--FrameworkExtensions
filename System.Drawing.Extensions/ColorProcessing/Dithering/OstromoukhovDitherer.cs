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
/// Ostromoukhov variable-coefficient error diffusion dithering.
/// </summary>
/// <remarks>
/// <para>Reference: V. Ostromoukhov 2001 "A Simple and Efficient Error-Diffusion Algorithm"</para>
/// <para>SIGGRAPH 2001, pp. 567-572</para>
/// <para>See also: https://perso.liris.cnrs.fr/victor.ostromoukhov/publications/pdf/SIGGRAPH01_varcoeffED.pdf</para>
/// <para>Uses intensity-dependent coefficients interpolated between key levels for blue-noise properties.</para>
/// </remarks>
[Ditherer("Ostromoukhov", Description = "Variable-coefficient error diffusion with blue-noise properties", Type = DitheringType.ErrorDiffusion, Author = "Victor Ostromoukhov", Year = 2001)]
public readonly struct OstromoukhovDitherer : IDitherer {

  // Static coefficient table shared by all instances - avoids null issues with default struct initialization
  private static readonly (int a, int b, int c)[] _Coefficients = _CreateCoefficients();
  private readonly bool _useSerpentine;

  /// <summary>Pre-configured instance with serpentine scanning (recommended).</summary>
  public static OstromoukhovDitherer Instance { get; } = new(true);

  /// <summary>Pre-configured instance without serpentine scanning.</summary>
  public static OstromoukhovDitherer Linear { get; } = new(false);

  /// <summary>Returns this ditherer with serpentine scan enabled.</summary>
  public OstromoukhovDitherer Serpentine => new(true);

  /// <summary>
  /// Creates an Ostromoukhov ditherer.
  /// </summary>
  /// <param name="useSerpentine">If true, alternates scan direction per row for reduced artifacts.</param>
  public OstromoukhovDitherer(bool useSerpentine = true) => this._useSerpentine = useSerpentine;

  private static (int a, int b, int c)[] _CreateCoefficients() {
    var coefficients = new (int, int, int)[256];
    _InitializeCoefficients(coefficients);
    return coefficients;
  }

  private static void _InitializeCoefficients((int a, int b, int c)[] coefficients) {
    // Key coefficients from the paper at specific intensity levels
    // (value, a=right, b=bottom-left/right depending on direction, c=bottom)
    var baseCoefficients = new (int value, int a, int b, int c)[] {
      (0, 13, 0, 5), (25, 13, 0, 5), (51, 21, 0, 10), (76, 7, 0, 4),
      (102, 8, 0, 5), (127, 47, 3, 28), (153, 23, 3, 13), (178, 15, 3, 8),
      (204, 22, 6, 11), (229, 16, 5, 7), (255, 9, 4, 4)
    };

    for (var i = 0; i < 256; ++i) {
      int lower = 0, upper = baseCoefficients.Length - 1;
      for (var j = 0; j < baseCoefficients.Length - 1; ++j) {
        if (i > baseCoefficients[j + 1].value)
          continue;

        lower = j;
        upper = j + 1;
        break;
      }

      var lowerCoeff = baseCoefficients[lower];
      var upperCoeff = baseCoefficients[upper];

      if (lowerCoeff.value == upperCoeff.value)
        coefficients[i] = (lowerCoeff.a, lowerCoeff.b, lowerCoeff.c);
      else {
        var t = (float)(i - lowerCoeff.value) / (upperCoeff.value - lowerCoeff.value);
        var a = (int)(lowerCoeff.a + t * (upperCoeff.a - lowerCoeff.a));
        var b = (int)(lowerCoeff.b + t * (upperCoeff.b - lowerCoeff.b));
        var c = (int)(lowerCoeff.c + t * (upperCoeff.c - lowerCoeff.c));
        coefficients[i] = (a, b, c);
      }
    }
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
    var coefficients = _Coefficients;
    var useSerpentine = this._useSerpentine;
    var endY = startY + height;

    // Error buffers for each channel - using 2D arrays for simplicity
    // Ring buffer optimization could be done but not critical for this algorithm
    var errorC1 = new float[width, height];
    var errorC2 = new float[width, height];
    var errorC3 = new float[width, height];

    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;
      var rightToLeft = useSerpentine && (y & 1) == 1;

      for (var i = 0; i < width; ++i) {
        var x = rightToLeft ? width - 1 - i : i;
        var sourceIdx = y * sourceStride + x;

        // Decode source pixel
        var pixel = decoder.Decode(source[sourceIdx]);
        var (c1, c2, c3, alpha) = pixel.ToNormalized();
        var pixelC1 = c1.ToFloat();
        var pixelC2 = c2.ToFloat();
        var pixelC3 = c3.ToFloat();
        var pixelA = alpha.ToFloat();

        // Apply accumulated error
        var newC1 = Math.Max(0f, Math.Min(1f, pixelC1 + errorC1[x, localY]));
        var newC2 = Math.Max(0f, Math.Min(1f, pixelC2 + errorC2[x, localY]));
        var newC3 = Math.Max(0f, Math.Min(1f, pixelC3 + errorC3[x, localY]));

        // Create adjusted color
        var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(newC1),
          UNorm32.FromFloatClamped(newC2),
          UNorm32.FromFloatClamped(newC3),
          UNorm32.FromFloatClamped(pixelA)
        );

        // Find nearest palette color
        var closestIndex = lookup.FindNearest(adjustedColor, out var closestColor);
        indices[y * targetStride + x] = (byte)closestIndex;

        // Calculate error
        var (cc1, cc2, cc3, _) = closestColor.ToNormalized();
        var errC1 = newC1 - cc1.ToFloat();
        var errC2 = newC2 - cc2.ToFloat();
        var errC3 = newC3 - cc3.ToFloat();

        // Skip diffusion if no error
        if (errC1 == 0 && errC2 == 0 && errC3 == 0)
          continue;

        // Calculate luminance for coefficient lookup (scale to 0-255)
        var luminance = (int)((0.299f * newC1 + 0.587f * newC2 + 0.114f * newC3) * 255);
        luminance = Math.Max(0, Math.Min(255, luminance));
        var (a, b, c) = coefficients[luminance];
        var sum = (float)(a + b + c);

        if (sum == 0)
          continue;

        // Direction-dependent neighbor offsets
        var dx1 = rightToLeft ? -1 : 1;  // right/left neighbor
        var dx2 = rightToLeft ? 1 : -1;   // bottom-left/right neighbor

        // Distribute error to right/left neighbor
        if (x + dx1 >= 0 && x + dx1 < width) {
          errorC1[x + dx1, localY] += errC1 * a / sum;
          errorC2[x + dx1, localY] += errC2 * a / sum;
          errorC3[x + dx1, localY] += errC3 * a / sum;
        }

        // Distribute error to bottom-left/right neighbor
        if (localY + 1 < height && x + dx2 >= 0 && x + dx2 < width) {
          errorC1[x + dx2, localY + 1] += errC1 * b / sum;
          errorC2[x + dx2, localY + 1] += errC2 * b / sum;
          errorC3[x + dx2, localY + 1] += errC3 * b / sum;
        }

        // Distribute error to bottom neighbor
        if (localY + 1 < height) {
          errorC1[x, localY + 1] += errC1 * c / sum;
          errorC2[x, localY + 1] += errC2 * c / sum;
          errorC3[x, localY + 1] += errC3 * c / sum;
        }
      }
    }
  }
}
