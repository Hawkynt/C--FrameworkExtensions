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
using Hawkynt.ColorProcessing.Storage;

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
public class NeuquantQuantizer : QuantizerBase {

  private const int _NETWORK_SIZE = 256;
  private const int _MAX_ITERATIONS = 100;
  private const double _INITIAL_ALPHA = 0.1;

  /// <inheritdoc />
  protected override Bgra8888[] _ReduceColorsTo(int colorCount, IEnumerable<(Bgra8888 color, uint count)> histogram) {
    var colorList = histogram.ToList();
    if (colorList.Count == 0)
      return [];

    var networkSize = Math.Min(_NETWORK_SIZE, colorCount * 4);
    var initialRadius = networkSize / 8.0;

    // Initialize network with evenly distributed colors
    var network = new double[networkSize][];
    for (var i = 0; i < networkSize; ++i)
      network[i] = [i * 255.0 / networkSize, i * 255.0 / networkSize, i * 255.0 / networkSize];

    // Prepare sample data (expand histogram into individual color samples)
    var samples = new List<Bgra8888>();
    foreach (var (color, count) in colorList) {
      var sampleCount = Math.Max(1, (int)Math.Sqrt(count)); // Reduce memory but maintain distribution
      for (var i = 0; i < sampleCount; ++i)
        samples.Add(color);
    }

    var totalSamples = samples.Count;

    // Training phase
    var radius = initialRadius;
    var alpha = _INITIAL_ALPHA;
    var radiusDec = radius / _MAX_ITERATIONS;
    var alphaDec = alpha / _MAX_ITERATIONS;

    for (var iteration = 0; iteration < _MAX_ITERATIONS; ++iteration) {
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
      .Select(neuron => Bgra8888.Create(
        Bgra8888.ClampToByte((float)neuron[0]),
        Bgra8888.ClampToByte((float)neuron[1]),
        Bgra8888.ClampToByte((float)neuron[2]),
        255
      ))
      .ToArray();
  }

  private static int _FindClosestNeuron(double[][] network, Bgra8888 color) {
    var bestDistance = double.MaxValue;
    var bestIndex = 0;

    for (var i = 0; i < network.Length; ++i) {
      var neuron = network[i];
      var dc1 = neuron[0] - color.C1;
      var dc2 = neuron[1] - color.C2;
      var dc3 = neuron[2] - color.C3;
      var distance = dc1 * dc1 + dc2 * dc2 + dc3 * dc3;

      if (!(distance < bestDistance))
        continue;

      bestDistance = distance;
      bestIndex = i;
    }

    return bestIndex;
  }

  private static void _UpdateNeighborhood(double[][] network, int bestIndex, Bgra8888 color, double alpha, double radius) {
    var radiusSquared = radius * radius;

    for (var i = 0; i < network.Length; ++i) {
      var distance = Math.Abs(i - bestIndex);
      if (distance > radius)
        continue;

      // Gaussian neighborhood function
      var influence = alpha * Math.Exp(-distance * distance / (2.0 * radiusSquared));

      var neuron = network[i];
      neuron[0] += influence * (color.C1 - neuron[0]);
      neuron[1] += influence * (color.C2 - neuron[1]);
      neuron[2] += influence * (color.C3 - neuron[2]);
    }
  }

}
