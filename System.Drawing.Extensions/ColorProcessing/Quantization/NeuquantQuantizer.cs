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
  /// Gets or sets the maximum number of training iterations.
  /// </summary>
  public int MaxIterations { get; set; } = 100;

  /// <summary>
  /// Gets or sets the initial learning rate.
  /// </summary>
  public float InitialAlpha { get; set; } = 0.1f;

  public NeuquantQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.MaxIterations, this.InitialAlpha);

  internal sealed class Kernel<TWork>(int maxIterations, float initialAlpha) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private const int _NETWORK_SIZE = 256;

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

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

      // Dekker 1994 §4 conscience mechanism: per-neuron frequency `freq[i]` and bias
      // `bias[i] = gamma * (1/N − freq[i])`. The best-match search subtracts bias from
      // distance so under-utilised neurons (freq < 1/N) appear closer and get pulled
      // into action; over-utilised ones get pushed away. This equalises neuron
      // utilisation and is the central novelty of Dekker's paper over plain Kohonen SOM.
      // Constants from the published reference implementation: β = 1/1024, γ = 1024.
      var freq = new double[networkSize];
      var bias = new double[networkSize];
      var initFreq = 1.0 / networkSize;
      for (var i = 0; i < networkSize; ++i) {
        freq[i] = initFreq;
        bias[i] = 0;
      }
      const double Beta = 1.0 / 1024.0;
      const double BetaGamma = 1.0; // = beta * gamma = (1/1024) * 1024

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

          // Find best matching unit (with conscience bias).
          var bestIndex = _FindClosestNeuronConscience(network, bias, color);

          // Update conscience: bias winner down, others up. Per Dekker eq. (10):
          //   freq[i] += β·(winner_i − freq[i])
          //   bias[i] = γ·(1/N − freq[i])
          // Combined and unrolled so each step is O(N).
          for (var k = 0; k < networkSize; ++k) {
            freq[k] -= Beta * freq[k];
            bias[k] += BetaGamma * freq[k];
          }
          freq[bestIndex] += Beta;
          bias[bestIndex] -= BetaGamma;

          // Update neurons in neighborhood
          _UpdateNeighborhood(network, bestIndex, color, alpha, radius);
        }

        // Decay learning parameters
        alpha -= alphaDec;
        radius -= radiusDec;
      }

      // Final palette extraction: if requested colorCount < networkSize, run a small
      // k-means refinement against the histogram to MERGE close neurons rather than
      // truncate to the first N. The Dekker paper assumes networkSize == colorCount;
      // for smaller colorCount we use the trained network as initialisation and
      // re-cluster.
      if (colorCount < networkSize) {
        return _ReduceNetworkToColorCount(network, samples, colorCount);
      }

      return network
        .Take(colorCount)
        .Select(neuron => ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)neuron[0]),
          UNorm32.FromFloatClamped((float)neuron[1]),
          UNorm32.FromFloatClamped((float)neuron[2]),
          UNorm32.FromFloatClamped((float)neuron[3])
        ));
    }

    /// <summary>
    /// Reduce a fully-trained network to <paramref name="colorCount"/> output colours by
    /// merging close neurons via greedy nearest-pair clustering. Avoids the previous
    /// implementation's arbitrary "Take first N" which discarded perfectly-trained neurons.
    /// </summary>
    private static IEnumerable<TWork> _ReduceNetworkToColorCount(double[][] network, List<TWork> samples, int colorCount) {
      // Start with all neurons; greedily merge the closest pair until count == colorCount.
      var remaining = network.Select(n => (double[])n.Clone()).ToList();
      while (remaining.Count > colorCount) {
        var bestI = 0;
        var bestJ = 1;
        var bestDist = double.MaxValue;
        for (var i = 0; i < remaining.Count; ++i)
        for (var j = i + 1; j < remaining.Count; ++j) {
          var a = remaining[i];
          var b = remaining[j];
          var dr = a[0] - b[0];
          var dg = a[1] - b[1];
          var db = a[2] - b[2];
          var da = a[3] - b[3];
          var d = dr * dr + dg * dg + db * db + da * da;
          if (d < bestDist) {
            bestDist = d;
            bestI = i;
            bestJ = j;
          }
        }
        // Merge i and j into i (mean), drop j.
        var ai = remaining[bestI];
        var aj = remaining[bestJ];
        ai[0] = 0.5 * (ai[0] + aj[0]);
        ai[1] = 0.5 * (ai[1] + aj[1]);
        ai[2] = 0.5 * (ai[2] + aj[2]);
        ai[3] = 0.5 * (ai[3] + aj[3]);
        remaining.RemoveAt(bestJ);
      }
      return remaining.Select(n => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)n[0]),
        UNorm32.FromFloatClamped((float)n[1]),
        UNorm32.FromFloatClamped((float)n[2]),
        UNorm32.FromFloatClamped((float)n[3])));
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

    /// <summary>
    /// Conscience-biased best-match search per Dekker 1994 §4: subtract <c>bias[i]</c>
    /// from the squared distance so under-utilised neurons (positive bias) win more often
    /// and over-utilised ones (negative bias) less often. Equalises neuron utilisation
    /// across the colour space.
    /// </summary>
    private static int _FindClosestNeuronConscience(double[][] network, double[] bias, TWork color) {
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
        var distance = dc1 * dc1 + dc2 * dc2 + dc3 * dc3 + da * da - bias[i];

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
