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
/// Direct Binary Search (DBS) dithering using iterative HVS-based optimization.
/// </summary>
/// <remarks>
/// <para>Reference: Lieberman, J. and Alreja, N., "Optimization of halftone images", 1997</para>
/// <para>Starts with initial halftone (threshold or error diffusion).</para>
/// <para>Iteratively toggles/swaps pixels to minimize HVS-weighted error.</para>
/// <para>Uses Gaussian-weighted error metric for human visual perception.</para>
/// </remarks>
[Ditherer("Direct Binary Search", Description = "Iterative HVS-based optimization dithering", Type = DitheringType.Custom)]
public readonly struct DbsDitherer : IDitherer {

  private const double Sigma = 1.5;
  private const int KernelRadius = 3;
  private const int _DEFAULT_ITERATIONS = 1; // Fast default for quick initialization

  // Static default kernel shared by all instances - avoids null issues with default struct initialization
  private static readonly double[,] _DefaultKernel = _GenerateGaussianKernel(KernelRadius, Sigma);

  private readonly int _iterations;
  private readonly double[,] _gaussianKernel;

  /// <summary>Pre-configured instance with 1 iteration (fast).</summary>
  public static DbsDitherer Fast { get; } = new(_DEFAULT_ITERATIONS);

  /// <summary>Pre-configured instance with 3 iterations (balanced).</summary>
  public static DbsDitherer Balanced { get; } = new(3);

  /// <summary>Pre-configured instance with 5 iterations (quality).</summary>
  public static DbsDitherer Quality { get; } = new(5);

  /// <summary>Pre-configured instance with 10 iterations (best quality, slow).</summary>
  public static DbsDitherer Best { get; } = new(10);

  /// <summary>
  /// Creates a DBS ditherer with the specified number of iterations.
  /// </summary>
  /// <param name="iterations">Number of optimization iterations.</param>
  public DbsDitherer(int iterations = _DEFAULT_ITERATIONS) {
    this._iterations = iterations;
    this._gaussianKernel = _DefaultKernel; // Always use static kernel since parameters are constant
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

    // Store original colors in normalized form
    var originalColors = new (float c1, float c2, float c3, float a)[height, width];
    var halftone = new byte[height, width];

    // Phase 1: Initial halftone using nearest color
    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;
      for (var x = 0; x < width; ++x) {
        var pixel = decoder.Decode(source[y * sourceStride + x]);
        var (c1, c2, c3, alpha) = pixel.ToNormalized();
        originalColors[localY, x] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), alpha.ToFloat());
        halftone[localY, x] = (byte)lookup.FindNearest(pixel);
      }
    }

    // Precompute palette colors in normalized form
    var paletteColors = new (float c1, float c2, float c3, float a)[palette.Length];
    for (var i = 0; i < palette.Length; ++i) {
      var (c1, c2, c3, a) = palette[i].ToNormalized();
      paletteColors[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat());
    }

    // Phase 2: Iterative DBS optimization
    var random = new Random(42 + startY);

    // Handle default struct initialization (iterations = 0)
    var iterations = this._iterations > 0 ? this._iterations : _DEFAULT_ITERATIONS;

    for (var iteration = 0; iteration < iterations; ++iteration) {
      var improved = false;

      // Scan in random order to avoid patterns
      var pixels = new (int x, int y)[width * height];
      var idx = 0;
      for (var y = 0; y < height; ++y)
      for (var x = 0; x < width; ++x)
        pixels[idx++] = (x, y);

      // Shuffle
      for (var i = pixels.Length - 1; i > 0; --i) {
        var j = random.Next(i + 1);
        (pixels[i], pixels[j]) = (pixels[j], pixels[i]);
      }

      foreach (var (px, py) in pixels) {
        var currentIndex = halftone[py, px];
        var currentError = this._CalculateLocalError(originalColors, halftone, paletteColors, px, py, width, height);

        // Try toggling to each other palette color
        var bestIndex = currentIndex;
        var bestError = currentError;

        for (var newIndex = 0; newIndex < palette.Length; ++newIndex) {
          if (newIndex == currentIndex)
            continue;

          halftone[py, px] = (byte)newIndex;
          var newError = this._CalculateLocalError(originalColors, halftone, paletteColors, px, py, width, height);

          if (newError < bestError - 1e-10) {
            bestError = newError;
            bestIndex = (byte)newIndex;
            improved = true;
          }
        }

        halftone[py, px] = (byte)bestIndex;

        // Try swapping with neighbors
        for (var dy = -1; dy <= 1; ++dy)
        for (var dx = -1; dx <= 1; ++dx) {
          if (dx == 0 && dy == 0) continue;

          var nx = px + dx;
          var ny = py + dy;

          if (nx < 0 || nx >= width || ny < 0 || ny >= height)
            continue;

          if (halftone[py, px] == halftone[ny, nx])
            continue;

          // Calculate error before swap
          var errorBefore = this._CalculateLocalError(originalColors, halftone, paletteColors, px, py, width, height) + this._CalculateLocalError(originalColors, halftone, paletteColors, nx, ny, width, height);

          // Swap
          (halftone[py, px], halftone[ny, nx]) = (halftone[ny, nx], halftone[py, px]);

          // Calculate error after swap
          var errorAfter = this._CalculateLocalError(originalColors, halftone, paletteColors, px, py, width, height) + this._CalculateLocalError(originalColors, halftone, paletteColors, nx, ny, width, height);

          if (errorAfter < errorBefore - 1e-10)
            improved = true;
          else
            // Swap back
            (halftone[py, px], halftone[ny, nx]) = (halftone[ny, nx], halftone[py, px]);
        }
      }

      if (!improved)
        break;  // Converged
    }

    // Copy result to output
    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;
      for (var x = 0; x < width; ++x)
        indices[y * targetStride + x] = halftone[localY, x];
    }
  }

  private double _CalculateLocalError(
    (float c1, float c2, float c3, float a)[,] original,
    byte[,] halftone,
    (float c1, float c2, float c3, float a)[] paletteColors,
    int cx, int cy,
    int width, int height) {

    // Handle default struct initialization (gaussianKernel = null)
    var gaussianKernel = this._gaussianKernel ?? _DefaultKernel;

    var error = 0.0;
    var kernelSize = 2 * KernelRadius + 1;

    for (var ky = 0; ky < kernelSize; ++ky)
    for (var kx = 0; kx < kernelSize; ++kx) {
      var px = cx + kx - KernelRadius;
      var py = cy + ky - KernelRadius;

      if (px < 0 || px >= width || py < 0 || py >= height)
        continue;

      var orig = original[py, px];
      var halftoneColor = paletteColors[halftone[py, px]];

      // HVS-weighted error (luminance-weighted)
      var dc1 = orig.c1 - halftoneColor.c1;
      var dc2 = orig.c2 - halftoneColor.c2;
      var dc3 = orig.c3 - halftoneColor.c3;

      // Luminance-weighted color difference
      var colorError = 0.299 * dc1 * dc1 + 0.587 * dc2 * dc2 + 0.114 * dc3 * dc3;

      // Apply Gaussian weight
      var weight = gaussianKernel[ky, kx];
      error += colorError * weight;
    }

    return error;
  }

  private static double[,] _GenerateGaussianKernel(int radius, double sigma) {
    var size = 2 * radius + 1;
    var kernel = new double[size, size];
    var sigma2 = 2 * sigma * sigma;
    var sum = 0.0;

    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x) {
      var dx = x - radius;
      var dy = y - radius;
      var value = Math.Exp(-(dx * dx + dy * dy) / sigma2);
      kernel[y, x] = value;
      sum += value;
    }

    // Normalize
    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x)
      kernel[y, x] /= sum;

    return kernel;
  }
}
