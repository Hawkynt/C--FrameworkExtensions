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
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Implements the NeuQuant neural network-based color quantization algorithm.
/// Uses a self-organizing map with neurons that adapt to the input colors
/// through competitive learning with a learning rate decay and neighbor function.
/// </summary>
/// <remarks>
/// <para>Reference: Anthony Dekker 1994 "Kohonen Neural Networks for Optimal Colour Quantization"</para>
/// <para>Network: Computing, 73, pp. 351-367</para>
/// <para>See also: https://scientificgems.wordpress.com/stuff/neuquant-fast-high-quality-image-quantization/</para>
/// </remarks>
[Quantizer(QuantizationType.Neural, DisplayName = "NeuQuant", Author = "Anthony Dekker", Year = 1994, QualityRating = 9)]
public struct NeuquantQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets whether to fill unused palette entries with generated colors.
  /// </summary>
  public bool AllowFillingColors { get; set; } = true;

  /// <summary>
  /// Gets or sets the maximum number of training iterations.
  /// </summary>
  public int MaxIterations { get; set; } = 100;

  /// <summary>
  /// Gets or sets the initial learning rate.
  /// </summary>
  public float InitialAlpha { get; set; } = 0.1f;

  public NeuquantQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.AllowFillingColors, this.MaxIterations, this.InitialAlpha);

  internal sealed class Kernel<TWork>(bool allowFillingColors, int maxIterations, float initialAlpha) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private const int _NETWORK_SIZE = 256;

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      var result = QuantizerHelper.TryHandleSimpleCases(histogram, colorCount, allowFillingColors, out var used);
      if (result != null)
        return result;

      var reduced = this._ReduceColorsTo(colorCount, used);
      return PaletteFiller.GenerateFinalPalette(reduced, colorCount, allowFillingColors);
    }

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var colorList = histogram.ToList();
      if (colorList.Count == 0)
        return [];

      var networkSize = Math.Min(_NETWORK_SIZE, colorCount * 4);
      var initialRadius = networkSize / 4.0; // Larger initial radius for better coverage

      // Prepare sample data (expand histogram into individual color samples)
      var samples = new List<TWork>();
      foreach (var (color, count) in colorList) {
        var sampleCount = Math.Max(1, (int)Math.Sqrt(count)); // Reduce memory but maintain distribution
        for (var i = 0; i < sampleCount; ++i)
          samples.Add(color);
      }

      var totalSamples = samples.Count;

      // Initialize network from input colors (sampled evenly across the histogram)
      // This is crucial - starting from greyscale prevents proper color learning
      var random = Random.Shared;
      var network = new double[networkSize][];
      for (var i = 0; i < networkSize; ++i) {
        var sampleIdx = (i * totalSamples / networkSize) % totalSamples;
        var initColor = samples[sampleIdx];
        var (c1N, c2N, c3N, aN) = initColor.ToNormalized();
        // Add small random perturbation to avoid identical neurons
        network[i] = [
          c1N.ToFloat() + (random.NextDouble() - 0.5) * 0.01,
          c2N.ToFloat() + (random.NextDouble() - 0.5) * 0.01,
          c3N.ToFloat() + (random.NextDouble() - 0.5) * 0.01,
          aN.ToFloat()
        ];
      }

      // Training phase
      var radius = initialRadius;
      var alpha = (double)initialAlpha;
      var radiusDec = radius / maxIterations;
      var alphaDec = alpha / maxIterations;

      for (var iteration = 0; iteration < maxIterations; ++iteration) {
        // Sample colors using prime number stepping for better distribution
        var step = Math.Max(1, totalSamples / 499);
        var sampleIndex = 0;

        for (var i = 0; i < totalSamples; i += step) {
          var color = samples[sampleIndex % totalSamples];
          sampleIndex += step;

          // Find best matching unit
          var bestIndex = _FindClosestNeuron(network, color);

          // Update neurons in neighborhood
          _UpdateNeighborhood(network, bestIndex, color, alpha, radius);
        }

        // Decay learning parameters
        alpha -= alphaDec;
        radius -= radiusDec;
      }

      // Extract final palette, limited to requested color count
      return network
        .Take(colorCount)
        .Select(neuron => ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)neuron[0]),
          UNorm32.FromFloatClamped((float)neuron[1]),
          UNorm32.FromFloatClamped((float)neuron[2]),
          UNorm32.FromFloatClamped((float)neuron[3])
        ));
    }

    private static int _FindClosestNeuron(double[][] network, TWork color) {
      var bestDistance = double.MaxValue;
      var bestIndex = 0;

      var (c1N, c2N, c3N, aN) = color.ToNormalized();
      var c1 = (double)c1N.ToFloat();
      var c2 = (double)c2N.ToFloat();
      var c3 = (double)c3N.ToFloat();
      var a = (double)aN.ToFloat();

      for (var i = 0; i < network.Length; ++i) {
        var neuron = network[i];
        var dc1 = neuron[0] - c1;
        var dc2 = neuron[1] - c2;
        var dc3 = neuron[2] - c3;
        var da = neuron[3] - a;
        var distance = dc1 * dc1 + dc2 * dc2 + dc3 * dc3 + da * da;

        if (!(distance < bestDistance))
          continue;

        bestDistance = distance;
        bestIndex = i;
      }

      return bestIndex;
    }

    private static void _UpdateNeighborhood(double[][] network, int bestIndex, TWork color, double alpha, double radius) {
      var radiusSquared = radius * radius;

      var (c1N, c2N, c3N, aN) = color.ToNormalized();
      var c1 = (double)c1N.ToFloat();
      var c2 = (double)c2N.ToFloat();
      var c3 = (double)c3N.ToFloat();
      var a = (double)aN.ToFloat();

      for (var i = 0; i < network.Length; ++i) {
        var distance = Math.Abs(i - bestIndex);
        if (distance > radius)
          continue;

        // Gaussian neighborhood function
        var influence = alpha * Math.Exp(-distance * distance / (2.0 * radiusSquared));

        var neuron = network[i];
        neuron[0] += influence * (c1 - neuron[0]);
        neuron[1] += influence * (c2 - neuron[1]);
        neuron[2] += influence * (c3 - neuron[2]);
        neuron[3] += influence * (a - neuron[3]);
      }
    }

  }
}
