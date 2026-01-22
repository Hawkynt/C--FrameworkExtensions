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
/// Implements Spatial Color Quantization using deterministic annealing optimization.
/// </summary>
/// <remarks>
/// <para><b>Reference:</b> J. Puzicha, M. Held, J. Ketterer, J.M. Buhmann, D. Fellner</para>
/// <para>University of Bonn - "On Spatial Quantization of Color Images" (IEEE Trans. Image Processing, 2000)</para>
/// <para/>
/// <para><b>Algorithm Overview:</b></para>
/// <para>The original Puzicha algorithm simultaneously optimizes palette colors AND pixel assignments</para>
/// <para>using a perception model (Gaussian filter) that simulates how humans perceive spatially averaged colors.</para>
/// <para/>
/// <para><b>Implementation Notes:</b></para>
/// <para>This implementation uses deterministic annealing optimization on the color histogram.</para>
/// <para>Since the quantizer interface works with histograms (not 2D pixel positions), this version</para>
/// <para>approximates spatial relationships using color-space proximity.</para>
/// <para/>
/// <para>Features implemented from the paper:</para>
/// <list type="bullet">
///   <item><description>Deterministic annealing with temperature schedule</description></item>
///   <item><description>Soft assignment probabilities during optimization</description></item>
///   <item><description>Color-space neighbor consideration for spatial approximation</description></item>
///   <item><description>Iterative refinement with convergence detection</description></item>
/// </list>
/// <para/>
/// <para>For full 2D spatial perception modeling, see rscolorq or apply dithering after quantization.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Spatial Color", QualityRating = 9)]
public struct SpatialColorQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum number of annealing iterations.
  /// </summary>
  public int MaxIterations { get; set; } = 30;

  /// <summary>
  /// Gets or sets the convergence threshold for palette updates.
  /// </summary>
  public double ConvergenceThreshold { get; set; } = 0.001;

  /// <summary>
  /// Gets or sets the spatial weighting factor (higher values = more spatial influence).
  /// </summary>
  public double SpatialWeight { get; set; } = 1.0;

  /// <summary>
  /// Gets or sets the neighborhood radius for color-space spatial approximation.
  /// </summary>
  public int NeighborhoodRadius { get; set; } = 3;

  /// <summary>
  /// Gets or sets the initial temperature for deterministic annealing.
  /// </summary>
  public double InitialTemperature { get; set; } = 2.0;

  /// <summary>
  /// Gets or sets the final temperature for deterministic annealing.
  /// </summary>
  public double FinalTemperature { get; set; } = 0.01;

  /// <summary>
  /// Gets or sets the cooling rate for temperature schedule (geometric cooling).
  /// </summary>
  public double CoolingRate { get; set; } = 0.9;

  public SpatialColorQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MaxIterations,
    this.ConvergenceThreshold,
    this.SpatialWeight,
    this.NeighborhoodRadius,
    this.InitialTemperature,
    this.FinalTemperature,
    this.CoolingRate
  );

  internal sealed class Kernel<TWork>(
    int maxIterations,
    double convergenceThreshold,
    double spatialWeight,
    int neighborhoodRadius,
    double initialTemperature,
    double finalTemperature,
    double coolingRate
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

      // Sort colors by color space proximity (approximates spatial locality)
      colorList = _SortByColorProximity(colorList);

      // Build neighbor lookup based on sorted order and color distance
      var neighbors = _BuildNeighborGraph(colorList, neighborhoodRadius);

      // Initialize palette using k-means++
      var palette = _InitializePaletteKMeansPlusPlus(colorList, colorCount);
      var previousPalette = new (float c1, float c2, float c3, float a)[colorCount];

      // Soft assignment probabilities for deterministic annealing
      var probabilities = new double[colorList.Count, colorCount];

      // Deterministic annealing optimization
      var temperature = initialTemperature;

      for (var iteration = 0; iteration < maxIterations && temperature > finalTemperature; ++iteration) {
        Array.Copy(palette, previousPalette, colorCount);

        // E-step: Calculate soft assignment probabilities (Gibbs distribution)
        this._CalculateSoftAssignments(colorList, neighbors, palette, probabilities, temperature);

        // M-step: Update palette based on soft assignments
        _UpdatePaletteWithSoftAssignments(colorList, probabilities, palette);

        // Check for convergence
        if (this._HasPaletteConverged(palette, previousPalette))
          break;

        // Cool down temperature
        temperature *= coolingRate;
      }

      // Final refinement with hard assignments at low temperature
      for (var refinement = 0; refinement < 5; ++refinement) {
        Array.Copy(palette, previousPalette, colorCount);

        var assignments = new int[colorList.Count];
        for (var i = 0; i < colorList.Count; ++i)
          assignments[i] = this._FindBestPaletteIndex(colorList[i], palette, colorList, neighbors[i]);

        _UpdatePaletteWithHardAssignments(colorList, assignments, palette);

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

    private static List<(float c1, float c2, float c3, float a, uint count)> _SortByColorProximity(
      List<(float c1, float c2, float c3, float a, uint count)> colors
    ) {
      // Sort using a space-filling curve approximation (Morton code / Z-order)
      return colors.OrderBy(c => {
        // Simple approximation: interleave bits of quantized color components
        var r = (int)(c.c1 * 255);
        var g = (int)(c.c2 * 255);
        var b = (int)(c.c3 * 255);
        return _Morton3D(r, g, b);
      }).ToList();
    }

    private static ulong _Morton3D(int x, int y, int z) {
      // Interleave bits for Z-order curve
      return _SplitBy3((uint)x) | (_SplitBy3((uint)y) << 1) | (_SplitBy3((uint)z) << 2);
    }

    private static ulong _SplitBy3(uint a) {
      ulong x = a & 0x1fffff;
      x = (x | x << 32) & 0x1f00000000ffff;
      x = (x | x << 16) & 0x1f0000ff0000ff;
      x = (x | x << 8) & 0x100f00f00f00f00f;
      x = (x | x << 4) & 0x10c30c30c30c30c3;
      x = (x | x << 2) & 0x1249249249249249;
      return x;
    }

    private static List<int>[] _BuildNeighborGraph(
      List<(float c1, float c2, float c3, float a, uint count)> colors,
      int radius
    ) {
      var neighbors = new List<int>[colors.Count];

      for (var i = 0; i < colors.Count; ++i) {
        neighbors[i] = [];

        // Add neighbors from sorted order (approximates spatial proximity)
        for (var offset = -radius; offset <= radius; ++offset) {
          if (offset == 0) continue;
          var neighborIdx = i + offset;
          if (neighborIdx >= 0 && neighborIdx < colors.Count)
            neighbors[i].Add(neighborIdx);
        }

        // Also add color-space nearest neighbors
        var current = colors[i];
        var nearestByColor = colors
          .Select((c, idx) => (idx, dist: _ColorDistanceSquared(current.c1, current.c2, current.c3, c.c1, c.c2, c.c3)))
          .Where(x => x.idx != i)
          .OrderBy(x => x.dist)
          .Take(radius * 2)
          .Select(x => x.idx);

        foreach (var idx in nearestByColor)
          if (!neighbors[i].Contains(idx))
            neighbors[i].Add(idx);
      }

      return neighbors;
    }

    private void _CalculateSoftAssignments(
      List<(float c1, float c2, float c3, float a, uint count)> colors,
      List<int>[] neighbors,
      (float c1, float c2, float c3, float a)[] palette,
      double[,] probabilities,
      double temperature
    ) {
      for (var i = 0; i < colors.Count; ++i) {
        var color = colors[i];
        var sumExp = 0.0;
        var energies = new double[palette.Length];

        // Calculate energy for each palette color
        for (var k = 0; k < palette.Length; ++k) {
          var energy = this._CalculateEnergy(color, palette[k], colors, neighbors[i], palette, k);
          energies[k] = -energy / temperature;
        }

        // Numerical stability: subtract max before exp
        var maxEnergy = energies.Max();
        for (var k = 0; k < palette.Length; ++k) {
          energies[k] = Math.Exp(energies[k] - maxEnergy);
          sumExp += energies[k];
        }

        // Normalize to get probabilities
        for (var k = 0; k < palette.Length; ++k)
          probabilities[i, k] = sumExp > 0 ? energies[k] / sumExp : 1.0 / palette.Length;
      }
    }

    private double _CalculateEnergy(
      (float c1, float c2, float c3, float a, uint count) color,
      (float c1, float c2, float c3, float a) paletteColor,
      List<(float c1, float c2, float c3, float a, uint count)> allColors,
      List<int> neighborIndices,
      (float c1, float c2, float c3, float a)[] palette,
      int paletteIndex
    ) {
      // Direct color distance cost
      var colorCost = _ColorDistanceSquared(color.c1, color.c2, color.c3, paletteColor.c1, paletteColor.c2, paletteColor.c3);

      // Spatial coherence cost: neighbors should have similar palette assignments
      var spatialCost = 0.0;
      if (neighborIndices.Count > 0) {
        foreach (var neighborIdx in neighborIndices) {
          var neighbor = allColors[neighborIdx];

          // Cost based on how well the palette color matches neighbors
          // This encourages smooth transitions
          var neighborDist = _ColorDistanceSquared(neighbor.c1, neighbor.c2, neighbor.c3, paletteColor.c1, paletteColor.c2, paletteColor.c3);

          // Weight by color similarity to current pixel (closer pixels matter more)
          var similarity = Math.Exp(-_ColorDistanceSquared(color.c1, color.c2, color.c3, neighbor.c1, neighbor.c2, neighbor.c3) * 4);
          spatialCost += neighborDist * similarity;
        }
        spatialCost /= neighborIndices.Count;
      }

      return colorCost + spatialWeight * spatialCost;
    }

    private static void _UpdatePaletteWithSoftAssignments(
      List<(float c1, float c2, float c3, float a, uint count)> colors,
      double[,] probabilities,
      (float c1, float c2, float c3, float a)[] palette
    ) {
      var paletteC1 = new double[palette.Length];
      var paletteC2 = new double[palette.Length];
      var paletteC3 = new double[palette.Length];
      var paletteA = new double[palette.Length];
      var paletteCounts = new double[palette.Length];

      for (var i = 0; i < colors.Count; ++i) {
        var color = colors[i];

        for (var k = 0; k < palette.Length; ++k) {
          var weight = probabilities[i, k] * color.count;
          paletteC1[k] += color.c1 * weight;
          paletteC2[k] += color.c2 * weight;
          paletteC3[k] += color.c3 * weight;
          paletteA[k] += color.a * weight;
          paletteCounts[k] += weight;
        }
      }

      for (var i = 0; i < palette.Length; ++i) {
        if (paletteCounts[i] > 1e-10) {
          palette[i] = (
            (float)(paletteC1[i] / paletteCounts[i]),
            (float)(paletteC2[i] / paletteCounts[i]),
            (float)(paletteC3[i] / paletteCounts[i]),
            (float)(paletteA[i] / paletteCounts[i])
          );
        }
      }
    }

    private static (float c1, float c2, float c3, float a)[] _InitializePaletteKMeansPlusPlus(
      List<(float c1, float c2, float c3, float a, uint count)> colors,
      int k
    ) {
      var palette = new (float c1, float c2, float c3, float a)[k];
      var random = new Random(42);

      if (colors.Count == 0)
        return palette;

      // Choose first color weighted by count
      var totalCount = colors.Sum(c => (double)c.count);
      var threshold = random.NextDouble() * totalCount;
      var cumulative = 0.0;
      var firstIndex = 0;

      for (var i = 0; i < colors.Count; ++i) {
        cumulative += colors[i].count;
        if (cumulative >= threshold) {
          firstIndex = i;
          break;
        }
      }

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
          threshold = random.NextDouble() * totalDistance;
          cumulative = 0.0;

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
      List<int> neighborIndices
    ) {
      var minCost = double.MaxValue;
      var bestIndex = 0;

      for (var i = 0; i < palette.Length; ++i) {
        var cost = this._CalculateEnergy(color, palette[i], allColors, neighborIndices, palette, i);
        if (cost < minCost) {
          minCost = cost;
          bestIndex = i;
        }
      }

      return bestIndex;
    }

    private static void _UpdatePaletteWithHardAssignments(
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
