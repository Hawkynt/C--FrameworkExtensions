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
/// Kohonen Self-Organising Map color quantizer — 1-D lattice with fixed-topology neighbourhood.
/// </summary>
/// <remarks>
/// <para>
/// Places <c>k</c> neurons on a 1-D ring and trains them with winner-take-all plus a grid-topology
/// neighbourhood: for each sample the Best-Matching Unit (BMU) is pulled toward the colour and its
/// lattice neighbours are pulled proportionally to a Gaussian on <b>grid distance</b>. Both the
/// learning rate and the neighbourhood radius decay exponentially across training.
/// </para>
/// <para>
/// Although conceptually close to Neural Gas, SOM differs in an important way: the neighbourhood is
/// defined on a fixed index grid, <b>not</b> on data-space ranks. This produces a palette that is
/// smoothly ordered on the ring — useful for gradient-friendly indexed output and for applications
/// that want palette indices to correspond to perceptual distance.
/// </para>
/// <para>
/// Distinct from <see cref="OctreeSomQuantizer"/> (which preloads prototypes from an octree), this
/// quantizer trains from scratch with weighted PCA-style initial spread along the principal axis of
/// the input histogram, so it is useful as a pure-SOM reference.
/// </para>
/// <para>Reference: Kohonen (1982) — "Self-Organized Formation of Topologically Correct Feature Maps", Biological Cybernetics 43(1).</para>
/// </remarks>
[Quantizer(QuantizationType.Neural, DisplayName = "SOM", Author = "T. Kohonen", Year = 1982, QualityRating = 8)]
public struct SomQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the number of training epochs.
  /// </summary>
  public int Epochs { get; set; } = 30;

  /// <summary>
  /// Gets or sets the initial learning rate.
  /// </summary>
  public float InitialLearningRate { get; set; } = 0.4f;

  /// <summary>
  /// Gets or sets the final learning rate at the end of training.
  /// </summary>
  public float FinalLearningRate { get; set; } = 0.01f;

  /// <summary>
  /// Gets or sets the initial neighbourhood radius on the lattice, as a fraction of <c>k</c>.
  /// </summary>
  public float InitialRadiusFactor { get; set; } = 0.5f;

  /// <summary>
  /// Gets or sets the final neighbourhood radius on the lattice (absolute, lattice cells).
  /// </summary>
  public float FinalRadius { get; set; } = 0.5f;

  /// <summary>
  /// Gets or sets the maximum sample size for training.
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>
  /// Gets or sets the deterministic random seed.
  /// </summary>
  public int Seed { get; set; } = 42;

  public SomQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Epochs,
    this.InitialLearningRate,
    this.FinalLearningRate,
    this.InitialRadiusFactor,
    this.FinalRadius,
    this.MaxSampleSize,
    this.Seed);

  internal sealed class Kernel<TWork>(
    int epochs,
    float initialLearningRate,
    float finalLearningRate,
    float initialRadiusFactor,
    float finalRadius,
    int maxSampleSize,
    int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= k)
        return colors.Select(c => c.color);

      var sampled = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);
      var n = sampled.Length;

      var sx1 = new double[n];
      var sx2 = new double[n];
      var sx3 = new double[n];
      var sxa = new double[n];
      var sw = new uint[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, a) = sampled[i].color.ToNormalized();
        sx1[i] = c1.ToFloat();
        sx2[i] = c2.ToFloat();
        sx3[i] = c3.ToFloat();
        sxa[i] = a.ToFloat();
        sw[i] = sampled[i].count;
      }

      var random = new Random(seed);
      var pca = QuantizerHelper.InitializePaletteWithPCA(sampled, k, seed);
      var p1 = new double[k];
      var p2 = new double[k];
      var p3 = new double[k];
      var pa = new double[k];
      for (var j = 0; j < k; ++j) {
        var (n1, n2, n3, na) = pca[j].ToNormalized();
        p1[j] = n1.ToFloat();
        p2[j] = n2.ToFloat();
        p3[j] = n3.ToFloat();
        pa[j] = na.ToFloat();
      }

      var totalWeight = 0.0;
      for (var i = 0; i < n; ++i)
        totalWeight += sw[i];

      var initRadius = Math.Max(1.0, (double)initialRadiusFactor * k);
      var finRadius = Math.Max(0.25, (double)finalRadius);
      var initEps = (double)initialLearningRate;
      var finEps = Math.Max(1e-4, (double)finalLearningRate);
      var totalSteps = (double)Math.Max(1, epochs * n);
      var step = 0;

      for (var epoch = 0; epoch < epochs; ++epoch) {
        // Shuffle presentation order — deterministic via seeded Random.
        var order = new int[n];
        for (var i = 0; i < n; ++i)
          order[i] = i;

        for (var i = n - 1; i > 0; --i) {
          var j = random.Next(i + 1);
          (order[i], order[j]) = (order[j], order[i]);
        }

        for (var oi = 0; oi < n; ++oi) {
          var si = order[oi];
          var x1 = sx1[si];
          var x2 = sx2[si];
          var x3 = sx3[si];
          var xa = sxa[si];

          // Find BMU.
          var bmu = 0;
          var bmuDist = double.MaxValue;
          for (var j = 0; j < k; ++j) {
            var d1 = p1[j] - x1;
            var d2 = p2[j] - x2;
            var d3 = p3[j] - x3;
            var d4 = pa[j] - xa;
            var dist = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
            if (dist >= bmuDist)
              continue;

            bmuDist = dist;
            bmu = j;
          }

          var t = step / totalSteps;
          var eps = initEps * Math.Pow(finEps / initEps, t);
          var radius = initRadius * Math.Pow(finRadius / initRadius, t);
          var twoSigma2 = 2 * radius * radius;
          var sampleWeight = Math.Sqrt(Math.Max(1, sw[si]));

          // Pull BMU and its grid-topology neighbours. Radius cap at 3·σ.
          var rCap = (int)Math.Ceiling(3 * radius);
          for (var off = -rCap; off <= rCap; ++off) {
            var gd = Math.Abs(off);
            var h = Math.Exp(-(gd * gd) / twoSigma2);
            if (h < 1e-4)
              continue;

            var j = bmu + off;
            if (j < 0 || j >= k)
              continue;

            var update = eps * h * sampleWeight;
            p1[j] += update * (x1 - p1[j]);
            p2[j] += update * (x2 - p2[j]);
            p3[j] += update * (x3 - p3[j]);
            pa[j] += update * (xa - pa[j]);
          }

          ++step;
        }
      }

      var palette = new TWork[k];
      for (var j = 0; j < k; ++j)
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, p1[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, p2[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, p3[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, pa[j])))
        );

      return palette;
    }

  }
}
