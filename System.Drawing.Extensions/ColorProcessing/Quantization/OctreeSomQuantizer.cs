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
/// Octree-initialized Self-Organizing Map color quantizer.
/// </summary>
/// <remarks>
/// <para>Combines fast Octree initialization with SOM refinement using two separate learning rates.</para>
/// <para>The two-rate approach reduces "color loss" that occurs when neighbor updates pull colors away from their targets.</para>
/// <para>Reference: Park, Kim, Cha (2015) - "An Effective Color Quantization Method Using Octree-Based Self-Organizing Maps"</para>
/// </remarks>
[Quantizer(QuantizationType.Neural, DisplayName = "Octree-SOM", Author = "Park, Kim, Cha", Year = 2015, QualityRating = 9)]
public struct OctreeSomQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum number of SOM training epochs.
  /// </summary>
  public int MaxEpochs { get; set; } = 50;

  /// <summary>
  /// Gets or sets the learning rate for the Best Matching Unit (winner).
  /// </summary>
  public float WinnerLearningRate { get; set; } = 0.1f;

  /// <summary>
  /// Gets or sets the learning rate for neighbor neurons (typically 1% of winner rate).
  /// </summary>
  public float NeighborLearningRate { get; set; } = 0.001f;

  /// <summary>
  /// Gets or sets the maximum sample size for processing.
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  public OctreeSomQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.MaxEpochs, this.WinnerLearningRate, this.NeighborLearningRate, this.MaxSampleSize);

  internal sealed class Kernel<TWork>(int maxEpochs, float winnerLearningRate, float neighborLearningRate, int maxSampleSize) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= colorCount)
        return colors.Select(c => c.color);

      // Sample histogram for SOM training
      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize, 42);

      // Initialize palette using basic Octree quantizer (fast, provides diverse initial coverage)
      var octreeQuantizer = new OctreeQuantizer();
      var kernel = (IQuantizer)octreeQuantizer;
      var octreeKernel = kernel.CreateKernel<TWork>();
      var initialPalette = octreeKernel.GeneratePalette(colors, colorCount);

      // Convert to neuron weights (normalized floats)
      var gridSize = (int)Math.Ceiling(Math.Sqrt(colorCount));
      var neurons = new double[colorCount][];

      for (var i = 0; i < colorCount; ++i) {
        var (c1, c2, c3, a) = initialPalette[i].ToNormalized();
        neurons[i] = [c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat()];
      }

      // Convert colors to float arrays for faster processing
      var colorData = colors.Select(c => {
        var (c1, c2, c3, a) = c.color.ToNormalized();
        return (c1: (double)c1.ToFloat(), c2: (double)c2.ToFloat(), c3: (double)c3.ToFloat(), a: (double)a.ToFloat(), count: c.count);
      }).ToArray();

      // SOM training with two learning rates
      var winnerRate = (double)winnerLearningRate;
      var neighborRate = (double)neighborLearningRate;
      var winnerDecay = winnerRate / maxEpochs;
      var neighborDecay = neighborRate / maxEpochs;

      var random = new Random(42);

      for (var epoch = 0; epoch < maxEpochs; ++epoch) {
        // Shuffle training order each epoch
        var indices = Enumerable.Range(0, colorData.Length).OrderBy(_ => random.Next()).ToArray();

        foreach (var idx in indices) {
          var (c1, c2, c3, a, count) = colorData[idx];

          // Find Best Matching Unit (BMU)
          var bmu = _FindBMU(neurons, c1, c2, c3, a);
          var bmuRow = bmu / gridSize;
          var bmuCol = bmu % gridSize;

          // Update BMU with winner learning rate
          var n = neurons[bmu];
          n[0] += winnerRate * (c1 - n[0]);
          n[1] += winnerRate * (c2 - n[1]);
          n[2] += winnerRate * (c3 - n[2]);
          n[3] += winnerRate * (a - n[3]);

          // Update Von Neumann neighbors (4-connected) with neighbor learning rate
          _UpdateNeighbor(neurons, gridSize, colorCount, bmuRow - 1, bmuCol, c1, c2, c3, a, neighborRate);
          _UpdateNeighbor(neurons, gridSize, colorCount, bmuRow + 1, bmuCol, c1, c2, c3, a, neighborRate);
          _UpdateNeighbor(neurons, gridSize, colorCount, bmuRow, bmuCol - 1, c1, c2, c3, a, neighborRate);
          _UpdateNeighbor(neurons, gridSize, colorCount, bmuRow, bmuCol + 1, c1, c2, c3, a, neighborRate);
        }

        // Linear decay of learning rates
        winnerRate -= winnerDecay;
        neighborRate -= neighborDecay;
      }

      // Convert neurons back to palette colors
      return neurons.Select(n => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, n[0]))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, n[1]))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, n[2]))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, n[3])))
      ));
    }

    private static int _FindBMU(double[][] neurons, double c1, double c2, double c3, double a) {
      var bestIndex = 0;
      var bestDist = double.MaxValue;

      for (var i = 0; i < neurons.Length; ++i) {
        var n = neurons[i];
        var d1 = n[0] - c1;
        var d2 = n[1] - c2;
        var d3 = n[2] - c3;
        var d4 = n[3] - a;
        var dist = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;

        if (!(dist < bestDist))
          continue;

        bestDist = dist;
        bestIndex = i;
      }

      return bestIndex;
    }

    private static void _UpdateNeighbor(double[][] neurons, int gridSize, int colorCount, int row, int col, double c1, double c2, double c3, double a, double rate) {
      if (row < 0 || col < 0 || row >= gridSize || col >= gridSize)
        return;

      var idx = row * gridSize + col;
      if (idx >= colorCount)
        return;

      var n = neurons[idx];
      n[0] += rate * (c1 - n[0]);
      n[1] += rate * (c2 - n[1]);
      n[2] += rate * (c3 - n[2]);
      n[3] += rate * (a - n[3]);
    }

  }
}
