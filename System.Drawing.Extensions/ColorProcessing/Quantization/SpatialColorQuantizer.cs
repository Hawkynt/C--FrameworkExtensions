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
using System.Linq;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Implements Spatial Color Quantization that combines dithering with palette generation.
/// </summary>
/// <remarks>
/// <para>Reference: J. Puzicha, M. Held, J. Ketterer, J.M. Buhmann, D. Fellner</para>
/// <para>University of Bonn - "On Spatial Quantization of Color Images"</para>
/// <para>This algorithm considers spatial context (neighboring pixels) when assigning colors,</para>
/// <para>resulting in superior visual quality by integrating dithering with palette optimization.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Spatial Color", QualityRating = 9)]
public struct SpatialColorQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum number of iterations for spatial optimization.
  /// </summary>
  public int MaxIterations { get; set; } = 10;

  /// <summary>
  /// Gets or sets the convergence threshold for palette updates.
  /// </summary>
  public double ConvergenceThreshold { get; set; } = 0.01;

  /// <summary>
  /// Gets or sets the spatial weighting factor (higher values = more spatial influence).
  /// </summary>
  public double SpatialWeight { get; set; } = 1.0;

  /// <summary>
  /// Gets or sets the neighborhood radius for spatial context.
  /// </summary>
  public int NeighborhoodRadius { get; set; } = 1;

  public SpatialColorQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MaxIterations,
    this.ConvergenceThreshold,
    this.SpatialWeight,
    this.NeighborhoodRadius
  );

  internal sealed class Kernel<TWork>(
    int maxIterations,
    double convergenceThreshold,
    double spatialWeight,
    int neighborhoodRadius
  ) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var colorList = histogram.Select(h => {
        var (c1, c2, c3, a) = h.color.ToNormalized();
        return (c1: c1.ToFloat(), c2: c2.ToFloat(), c3: c3.ToFloat(), a: a.ToFloat(), count: h.count);
      }).ToList();

      if (colorList.Count == 0)
        return [];

      // Initialize palette using k-means++
      var palette = _InitializePaletteKMeansPlusPlus(colorList, colorCount);
      var previousPalette = new (float c1, float c2, float c3, float a)[colorCount];

      // Iterative refinement with spatial awareness
      for (var iteration = 0; iteration < maxIterations; ++iteration) {
        Array.Copy(palette, previousPalette, colorCount);

        // Assignment step
        var assignments = new int[colorList.Count];
        for (var i = 0; i < colorList.Count; ++i)
          assignments[i] = this._FindBestPaletteIndex(colorList[i], palette, colorList, i);

        // Update step
        _UpdatePaletteWithSpatialWeighting(colorList, assignments, palette);

        // Check for convergence
        if (this._HasPaletteConverged(palette, previousPalette))
          break;
      }

      // Convert to TWork
      return palette.Select(p => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(p.c1),
        UNorm32.FromFloatClamped(p.c2),
        UNorm32.FromFloatClamped(p.c3),
        UNorm32.FromFloatClamped(p.a)
      ));
    }

    private static (float c1, float c2, float c3, float a)[] _InitializePaletteKMeansPlusPlus(
      List<(float c1, float c2, float c3, float a, uint count)> colors,
      int k
    ) {
      var palette = new (float c1, float c2, float c3, float a)[k];
      var random = new Random(42);

      if (colors.Count == 0)
        return palette;

      // Choose first color randomly
      var firstIndex = random.Next(colors.Count);
      palette[0] = (colors[firstIndex].c1, colors[firstIndex].c2, colors[firstIndex].c3, colors[firstIndex].a);

      // Choose remaining colors using k-means++
      for (var i = 1; i < k; ++i) {
        var distances = new double[colors.Count];
        var totalDistance = 0.0;

        for (var j = 0; j < colors.Count; ++j) {
          var color = colors[j];
          var minDistance = double.MaxValue;

          for (var c = 0; c < i; ++c) {
            var paletteColor = palette[c];
            var distance = _ColorDistanceSquared(color.c1, color.c2, color.c3, paletteColor.c1, paletteColor.c2, paletteColor.c3);
            if (distance < minDistance)
              minDistance = distance;
          }

          distances[j] = minDistance * color.count;
          totalDistance += distances[j];
        }

        if (totalDistance > 0) {
          var threshold = random.NextDouble() * totalDistance;
          var cumulative = 0.0;

          for (var j = 0; j < colors.Count; ++j) {
            cumulative += distances[j];
            if (cumulative >= threshold) {
              palette[i] = (colors[j].c1, colors[j].c2, colors[j].c3, colors[j].a);
              break;
            }
          }
        } else {
          var idx = random.Next(colors.Count);
          palette[i] = (colors[idx].c1, colors[idx].c2, colors[idx].c3, colors[idx].a);
        }
      }

      return palette;
    }

    private int _FindBestPaletteIndex(
      (float c1, float c2, float c3, float a, uint count) color,
      (float c1, float c2, float c3, float a)[] palette,
      List<(float c1, float c2, float c3, float a, uint count)> allColors,
      int currentIndex
    ) {
      var minCost = double.MaxValue;
      var bestIndex = 0;

      for (var i = 0; i < palette.Length; ++i) {
        var cost = this._CalculateSpatialCost(color, palette[i], allColors, currentIndex);
        if (cost < minCost) {
          minCost = cost;
          bestIndex = i;
        }
      }

      return bestIndex;
    }

    private double _CalculateSpatialCost(
      (float c1, float c2, float c3, float a, uint count) color,
      (float c1, float c2, float c3, float a) paletteColor,
      List<(float c1, float c2, float c3, float a, uint count)> allColors,
      int index
    ) {
      // Color distance cost
      var colorCost = _ColorDistanceSquared(color.c1, color.c2, color.c3, paletteColor.c1, paletteColor.c2, paletteColor.c3);

      // Spatial cost: consider neighboring colors in the histogram
      var spatialCost = 0.0;
      var neighborCount = 0;

      for (var offset = -neighborhoodRadius; offset <= neighborhoodRadius; ++offset) {
        if (offset == 0)
          continue;

        var neighborIndex = index + offset;
        if (neighborIndex < 0 || neighborIndex >= allColors.Count)
          continue;

        var neighbor = allColors[neighborIndex];
        var neighborDistance = _ColorDistanceSquared(neighbor.c1, neighbor.c2, neighbor.c3, paletteColor.c1, paletteColor.c2, paletteColor.c3);
        spatialCost += neighborDistance;
        ++neighborCount;
      }

      if (neighborCount > 0)
        spatialCost /= neighborCount;

      return colorCost + spatialWeight * spatialCost;
    }

    private static void _UpdatePaletteWithSpatialWeighting(
      List<(float c1, float c2, float c3, float a, uint count)> colors,
      int[] assignments,
      (float c1, float c2, float c3, float a)[] palette
    ) {
      var paletteC1 = new double[palette.Length];
      var paletteC2 = new double[palette.Length];
      var paletteC3 = new double[palette.Length];
      var paletteA = new double[palette.Length];
      var paletteCounts = new double[palette.Length];

      for (var i = 0; i < colors.Count; ++i) {
        var color = colors[i];
        var paletteIndex = assignments[i];
        var weight = (double)color.count;

        paletteC1[paletteIndex] += color.c1 * weight;
        paletteC2[paletteIndex] += color.c2 * weight;
        paletteC3[paletteIndex] += color.c3 * weight;
        paletteA[paletteIndex] += color.a * weight;
        paletteCounts[paletteIndex] += weight;
      }

      for (var i = 0; i < palette.Length; ++i) {
        if (paletteCounts[i] > 0) {
          palette[i] = (
            (float)(paletteC1[i] / paletteCounts[i]),
            (float)(paletteC2[i] / paletteCounts[i]),
            (float)(paletteC3[i] / paletteCounts[i]),
            (float)(paletteA[i] / paletteCounts[i])
          );
        }
      }
    }

    private bool _HasPaletteConverged(
      (float c1, float c2, float c3, float a)[] current,
      (float c1, float c2, float c3, float a)[] previous
    ) {
      var totalChange = 0.0;

      for (var i = 0; i < current.Length; ++i) {
        var d1 = current[i].c1 - previous[i].c1;
        var d2 = current[i].c2 - previous[i].c2;
        var d3 = current[i].c3 - previous[i].c3;
        totalChange += Math.Sqrt(d1 * d1 + d2 * d2 + d3 * d3);
      }

      return totalChange / current.Length < convergenceThreshold;
    }

    private static double _ColorDistanceSquared(float c1a, float c2a, float c3a, float c1b, float c2b, float c3b) {
      var d1 = c1a - c1b;
      var d2 = c2a - c2b;
      var d3 = c3a - c3b;
      return d1 * d1 + d2 * d2 + d3 * d3;
    }
  }
}
