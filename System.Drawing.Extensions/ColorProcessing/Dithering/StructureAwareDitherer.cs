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
/// Structure-aware error diffusion dithering with circular error distribution.
/// </summary>
/// <remarks>
/// <para>Analyzes local gradients using Sobel operator.</para>
/// <para>Distributes error using a circular kernel with gradient-adjusted weights.</para>
/// <para>Optionally uses priority ordering for error distribution.</para>
/// </remarks>
[Ditherer("Structure-Aware", Description = "Edge-aware error diffusion with circular kernel", Type = DitheringType.ErrorDiffusion)]
public readonly struct StructureAwareDitherer : IDitherer {

  private readonly int _radius;
  private readonly bool _usePriorityOrder;

  /// <summary>Pre-configured instance with default settings.</summary>
  public static StructureAwareDitherer Default { get; } = new(2, false);

  /// <summary>Pre-configured instance with priority-based error distribution.</summary>
  public static StructureAwareDitherer Priority { get; } = new(3, true);

  /// <summary>Pre-configured instance with larger kernel radius.</summary>
  public static StructureAwareDitherer Large { get; } = new(4, false);

  /// <summary>
  /// Creates a structure-aware ditherer with the specified settings.
  /// </summary>
  /// <param name="radius">Radius of the circular error distribution kernel.</param>
  /// <param name="usePriorityOrder">Whether to use priority-based error distribution.</param>
  public StructureAwareDitherer(int radius = 2, bool usePriorityOrder = false) {
    this._radius = Math.Max(1, Math.Min(6, radius));
    this._usePriorityOrder = usePriorityOrder;
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

    // Calculate gradients using Sobel operator
    var gradients = _CalculateGradients(sourceColors, width, height);

    // Error buffers
    var errorC1 = new float[width, height];
    var errorC2 = new float[width, height];
    var errorC3 = new float[width, height];

    // Generate circular kernel
    var errorKernel = _GenerateCircularKernel(this._radius);

    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;
      for (var x = 0; x < width; ++x) {
        var pixel = sourceColors[localY, x];

        var newC1 = Math.Max(0f, Math.Min(1f, pixel.c1 + errorC1[x, localY]));
        var newC2 = Math.Max(0f, Math.Min(1f, pixel.c2 + errorC2[x, localY]));
        var newC3 = Math.Max(0f, Math.Min(1f, pixel.c3 + errorC3[x, localY]));

        var newColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(newC1),
          UNorm32.FromFloatClamped(newC2),
          UNorm32.FromFloatClamped(newC3),
          UNorm32.FromFloatClamped(pixel.a)
        );

        var gradient = gradients[x, localY];

        // For high gradient areas, consider luminance preservation
        int closestIndex;
        if (gradient > 0.3f) {
          closestIndex = _FindStructureAwareColor(newColor, palette, paletteColors, gradient, lookup, metric);
        } else {
          closestIndex = lookup.FindNearest(newColor);
        }

        indices[y * targetStride + x] = (byte)closestIndex;

        var closestColor = paletteColors[closestIndex];
        var errC1 = newC1 - closestColor.c1;
        var errC2 = newC2 - closestColor.c2;
        var errC3 = newC3 - closestColor.c3;

        if (Math.Abs(errC1) < 1e-6f && Math.Abs(errC2) < 1e-6f && Math.Abs(errC3) < 1e-6f)
          continue;

        _DistributeError(x, localY, errC1, errC2, errC3, width, height, errorC1, errorC2, errorC3, gradients, errorKernel, this._usePriorityOrder);
      }
    }
  }

  private static float[,] _CalculateGradients((float c1, float c2, float c3, float a)[,] sourceColors, int width, int height) {
    var gradients = new float[width, height];

    for (var y = 1; y < height - 1; ++y)
    for (var x = 1; x < width - 1; ++x) {
      // Sobel operator for gradient magnitude
      var pixels = new float[9];
      for (var dy = -1; dy <= 1; ++dy)
      for (var dx = -1; dx <= 1; ++dx) {
        var c = sourceColors[y + dy, x + dx];
        pixels[(dy + 1) * 3 + (dx + 1)] = 0.299f * c.c1 + 0.587f * c.c2 + 0.114f * c.c3;
      }

      var gx = -pixels[0] + pixels[2] +
               -2 * pixels[3] + 2 * pixels[5] +
               -pixels[6] + pixels[8];

      var gy = -pixels[0] - 2 * pixels[1] - pixels[2] +
               pixels[6] + 2 * pixels[7] + pixels[8];

      gradients[x, y] = (float)Math.Sqrt(gx * gx + gy * gy);
    }

    return gradients;
  }

  private static int _FindStructureAwareColor<TWork, TMetric>(
    TWork target,
    TWork[] palette,
    (float c1, float c2, float c3, float a)[] paletteColors,
    float gradient,
    PaletteLookup<TWork, TMetric> lookup,
    TMetric metric)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    // Get 3 closest colors
    var distances = new List<(int index, float distance)>(palette.Length);
    for (var i = 0; i < palette.Length; ++i) {
      var distance = metric.Distance(target, palette[i]).ToFloat();
      distances.Add((i, distance));
    }
    distances.Sort((a, b) => a.distance.CompareTo(b.distance));

    if (distances.Count < 3)
      return distances.Count > 0 ? distances[0].index : 0;

    // Get target luminance
    var (tc1, tc2, tc3, _) = target.ToNormalized();
    var targetLuminance = 0.299f * tc1.ToFloat() + 0.587f * tc2.ToFloat() + 0.114f * tc3.ToFloat();

    // Select candidate with best luminance contrast preservation
    var bestIndex = distances[0].index;
    var bestLumDiff = float.MinValue;

    for (var i = 0; i < Math.Min(3, distances.Count); ++i) {
      var color = paletteColors[distances[i].index];
      var luminance = 0.299f * color.c1 + 0.587f * color.c2 + 0.114f * color.c3;
      var lumDiff = Math.Abs(luminance - targetLuminance);

      if (lumDiff > bestLumDiff) {
        bestLumDiff = lumDiff;
        bestIndex = distances[i].index;
      }
    }

    return bestIndex;
  }

  private static List<(int dx, int dy, float weight)> _GenerateCircularKernel(int radius) {
    var kernel = new List<(int dx, int dy, float weight)>();
    var totalWeight = 0f;

    for (var dy = 0; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      if (dx == 0 && dy == 0) continue;

      var distance = (float)Math.Sqrt(dx * dx + dy * dy);
      if (!(distance <= radius))
        continue;

      var weight = (float)Math.Exp(-distance * 2.0);

      if (dy == 0) weight *= 1.2f;
      if (dx == 0) weight *= 1.1f;

      kernel.Add((dx, dy, weight));
      totalWeight += weight;
    }

    for (var i = 0; i < kernel.Count; ++i) {
      var (dx, dy, weight) = kernel[i];
      kernel[i] = (dx, dy, weight / totalWeight);
    }

    return kernel;
  }

  private static void _DistributeError(
    int x, int y,
    float errC1, float errC2, float errC3,
    int width, int height,
    float[,] errorC1, float[,] errorC2, float[,] errorC3,
    float[,] gradients,
    List<(int dx, int dy, float weight)> errorKernel,
    bool usePriorityOrder) {

    var currentGradient = gradients[x, y];
    var candidates = new List<(int tx, int ty, float weight, float priority)>();

    foreach (var (dx, dy, weight) in errorKernel) {
      var tx = x + dx;
      var ty = y + dy;

      if (tx >= 0 && tx < width && ty >= 0 && ty < height) {
        var priority = usePriorityOrder ? _CalculatePriority(x, y, tx, ty, currentGradient, gradients) : weight;
        candidates.Add((tx, ty, weight, priority));
      }
    }

    if (usePriorityOrder)
      candidates.Sort((a, b) => b.priority.CompareTo(a.priority));

    foreach (var (tx, ty, weight, _) in candidates) {
      var adjustedWeight = weight;

      var targetGradient = gradients[tx, ty];
      var gradientSimilarity = 1.0f - Math.Abs(currentGradient - targetGradient);
      adjustedWeight *= 0.7f + 0.3f * gradientSimilarity;

      errorC1[tx, ty] += errC1 * adjustedWeight;
      errorC2[tx, ty] += errC2 * adjustedWeight;
      errorC3[tx, ty] += errC3 * adjustedWeight;
    }
  }

  private static float _CalculatePriority(int x, int y, int tx, int ty, float currentGradient, float[,] gradients) {
    var distance = (float)Math.Sqrt((tx - x) * (tx - x) + (ty - y) * (ty - y));
    var targetGradient = gradients[tx, ty];
    var gradientSimilarity = 1.0f - Math.Abs(currentGradient - targetGradient);
    return gradientSimilarity / (1.0f + distance);
  }
}
