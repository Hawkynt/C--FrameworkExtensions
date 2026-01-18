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
/// Color Quantization Network (CQN) using competitive learning with conscience mechanism.
/// </summary>
/// <remarks>
/// <para>Uses Learning Vector Quantization (LVQ) with frequency-sensitive competitive learning.</para>
/// <para>The conscience mechanism prevents dead neurons and ensures all palette entries are utilized.</para>
/// <para>Produces well-distributed palette colors even for images with highly skewed color distributions.</para>
/// </remarks>
[Quantizer(QuantizationType.Neural, DisplayName = "Color Quantization Network", Author = "Various", Year = 1992, QualityRating = 8)]
public struct ColorQuantizationNetworkQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum number of training epochs.
  /// </summary>
  public int MaxEpochs { get; set; } = 100;

  /// <summary>
  /// Gets or sets the initial learning rate.
  /// </summary>
  public float InitialLearningRate { get; set; } = 0.3f;

  /// <summary>
  /// Gets or sets the conscience factor (higher values enforce more balanced neuron usage).
  /// </summary>
  public float ConscienceFactor { get; set; } = 0.1f;

  /// <summary>
  /// Gets or sets whether to use frequency-sensitive competitive learning.
  /// </summary>
  public bool UseFrequencySensitive { get; set; } = true;

  /// <summary>
  /// Gets or sets the maximum histogram sample size for large images.
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  public ColorQuantizationNetworkQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MaxEpochs,
    this.InitialLearningRate,
    this.ConscienceFactor,
    this.UseFrequencySensitive,
    this.MaxSampleSize
  );

  internal sealed class Kernel<TWork>(
    int maxEpochs,
    float initialLearningRate,
    float conscienceFactor,
    bool useFrequencySensitive,
    int maxSampleSize
  ) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= k)
        return colors.Select(c => c.color);

      // Sample histogram if too large for iterative processing
      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize);

      // Initialize neurons using K-Means++ style seeding
      var neurons = _InitializeNeurons(colors, k);
      var winCounts = new double[k];
      var biases = new double[k];
      var totalWeight = colors.Sum(c => (long)c.count);

      // Initialize conscience mechanism - all neurons start with equal bias
      for (var i = 0; i < k; ++i) {
        biases[i] = 0.0;
        winCounts[i] = 0.0;
      }

      var random = new Random(42);
      var learningRate = (double)initialLearningRate;
      var learningRateDecay = learningRate / maxEpochs;

      // Training loop
      for (var epoch = 0; epoch < maxEpochs; ++epoch) {
        // Present each color to the network
        foreach (var (color, count) in colors) {
          var (c1, c2, c3, a) = color.ToNormalized();
          var input = new double[] { c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat() };

          // Find winning neuron (with conscience if enabled)
          var winner = _FindWinner(neurons, input, biases, useFrequencySensitive);

          // Update winning neuron
          _UpdateNeuron(neurons[winner], input, learningRate);

          // Update conscience mechanism
          if (useFrequencySensitive) {
            winCounts[winner] += count;
            _UpdateBiases(biases, winCounts, totalWeight, conscienceFactor);
          }
        }

        // Decay learning rate
        learningRate -= learningRateDecay;
        if (learningRate < 0.001)
          learningRate = 0.001;
      }

      // Convert neurons to palette colors
      return neurons.Select(neuron => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)neuron[0]),
        UNorm32.FromFloatClamped((float)neuron[1]),
        UNorm32.FromFloatClamped((float)neuron[2]),
        UNorm32.FromFloatClamped((float)neuron[3])
      ));
    }

    private static double[][] _InitializeNeurons((TWork color, uint count)[] colors, int k) {
      var random = new Random(42);
      var neurons = new double[k][];
      var distances = new double[colors.Length];

      // K-Means++ initialization
      var totalWeight = colors.Sum(c => (long)c.count);
      var target = random.NextDouble() * totalWeight;
      long cumulative = 0;
      var firstIndex = 0;

      for (var i = 0; i < colors.Length; ++i) {
        cumulative += colors[i].count;
        if (cumulative >= target) {
          firstIndex = i;
          break;
        }
      }

      var (c1, c2, c3, a) = colors[firstIndex].color.ToNormalized();
      neurons[0] = [c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat()];

      // Select remaining initial neurons
      for (var n = 1; n < k; ++n) {
        var totalDistance = 0.0;

        for (var i = 0; i < colors.Length; ++i) {
          var (pc1, pc2, pc3, pa) = colors[i].color.ToNormalized();
          var point = new double[] { pc1.ToFloat(), pc2.ToFloat(), pc3.ToFloat(), pa.ToFloat() };

          var minDist = double.MaxValue;
          for (var j = 0; j < n; ++j) {
            var dist = _SquaredDistance(point, neurons[j]);
            if (dist < minDist)
              minDist = dist;
          }
          distances[i] = minDist * colors[i].count;
          totalDistance += distances[i];
        }

        // Weighted random selection
        target = random.NextDouble() * totalDistance;
        cumulative = 0;
        var selectedIndex = 0;

        for (var i = 0; i < colors.Length; ++i) {
          cumulative += (long)distances[i];
          if (cumulative >= target) {
            selectedIndex = i;
            break;
          }
        }

        var (sc1, sc2, sc3, sa) = colors[selectedIndex].color.ToNormalized();
        neurons[n] = [sc1.ToFloat(), sc2.ToFloat(), sc3.ToFloat(), sa.ToFloat()];
      }

      return neurons;
    }

    private static int _FindWinner(double[][] neurons, double[] input, double[] biases, bool useConscience) {
      var minDistance = double.MaxValue;
      var winner = 0;

      for (var i = 0; i < neurons.Length; ++i) {
        var dist = _SquaredDistance(input, neurons[i]);

        // Apply conscience bias if enabled
        if (useConscience)
          dist += biases[i];

        if (dist < minDistance) {
          minDistance = dist;
          winner = i;
        }
      }

      return winner;
    }

    private static double _SquaredDistance(double[] a, double[] b) {
      var sum = 0.0;
      for (var i = 0; i < a.Length; ++i) {
        var diff = a[i] - b[i];
        sum += diff * diff;
      }
      return sum;
    }

    private static void _UpdateNeuron(double[] neuron, double[] input, double learningRate) {
      for (var i = 0; i < neuron.Length; ++i)
        neuron[i] += learningRate * (input[i] - neuron[i]);
    }

    private static void _UpdateBiases(double[] biases, double[] winCounts, long totalWeight, float conscienceFactor) {
      var expectedWinRate = 1.0 / biases.Length;
      var totalWins = winCounts.Sum();

      if (totalWins < 1.0)
        return;

      for (var i = 0; i < biases.Length; ++i) {
        var actualWinRate = winCounts[i] / totalWins;
        // Increase bias for neurons that win too often, decrease for those that win less
        biases[i] = conscienceFactor * (actualWinRate - expectedWinRate);
      }
    }
  }
}
