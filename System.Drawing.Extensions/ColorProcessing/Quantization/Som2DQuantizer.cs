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
/// 2-D Kohonen Self-Organising Map colour quantizer — Kohonen (1982) on a 2-D grid.
/// </summary>
/// <remarks>
/// <para>
/// Neurons are arranged on a √k × √k toroidal 2-D grid (rather than the 1-D ring
/// used by <see cref="SomQuantizer"/>). For each sample the Best-Matching Unit
/// (BMU) is pulled toward the input and its toroidal 2-D grid neighbours are
/// pulled proportionally to a Gaussian on <i>2-D grid distance</i>. Both the
/// learning rate and the neighbourhood radius decay exponentially across
/// training.
/// </para>
/// <para>
/// Distinct from <see cref="SomQuantizer"/> (1-D ring — palette index order is
/// a single line through colour space) and <see cref="OctreeSomQuantizer"/>
/// (octree-preloaded prototypes). A 2-D grid yields a palette whose indices
/// form a <i>plane</i> through colour space — useful for applications that want
/// to treat the palette as an image-space texture atlas or hue × lightness
/// lookup table.
/// </para>
/// <para>
/// Reference: T. Kohonen (1982) — "Self-Organized Formation of Topologically
/// Correct Feature Maps", Biological Cybernetics 43(1):59-69. The 2-D grid
/// variant is the default topology in most SOM software (MATLAB Neural Network
/// Toolbox, Python <c>minisom</c>).
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Neural, DisplayName = "SOM 2D", Author = "T. Kohonen", Year = 1982, QualityRating = 8)]
public struct Som2DQuantizer : IQuantizer {

  /// <summary>Gets or sets the number of training epochs.</summary>
  public int Epochs { get; set; } = 25;

  /// <summary>Gets or sets the initial learning rate.</summary>
  public float InitialLearningRate { get; set; } = 0.4f;

  /// <summary>Gets or sets the final learning rate.</summary>
  public float FinalLearningRate { get; set; } = 0.01f;

  /// <summary>Gets or sets the initial neighbourhood radius as a fraction of √k.</summary>
  public float InitialRadiusFactor { get; set; } = 0.5f;

  /// <summary>Gets or sets the final neighbourhood radius (grid cells).</summary>
  public float FinalRadius { get; set; } = 0.5f;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public Som2DQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Epochs, this.InitialLearningRate, this.FinalLearningRate,
    this.InitialRadiusFactor, this.FinalRadius, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int epochs, float initialLearningRate, float finalLearningRate,
    float initialRadiusFactor, float finalRadius, int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0) return [];
      if (colors.Length <= k) return colors.Select(c => c.color);

      var sampled = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);
      var n = sampled.Length;
      var sx1 = new double[n]; var sx2 = new double[n]; var sx3 = new double[n]; var sxa = new double[n]; var sw = new uint[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, al) = sampled[i].color.ToNormalized();
        sx1[i] = c1.ToFloat(); sx2[i] = c2.ToFloat(); sx3[i] = c3.ToFloat(); sxa[i] = al.ToFloat();
        sw[i] = sampled[i].count;
      }

      // Pick a nearly-square grid whose product is >= k; then truncate to first k cells.
      var gw = (int)Math.Ceiling(Math.Sqrt(k));
      var gh = (int)Math.Ceiling((double)k / gw);
      var total = gw * gh;
      var p1 = new double[total]; var p2 = new double[total]; var p3 = new double[total]; var pa = new double[total];

      // PCA-spread initialisation of the first k prototypes; extra grid slots cloned from prototype 0.
      var pcaInit = QuantizerHelper.InitializePaletteWithPCA(sampled, k, seed);
      for (var j = 0; j < k; ++j) {
        var (n1, n2, n3, na) = pcaInit[j].ToNormalized();
        p1[j] = n1.ToFloat(); p2[j] = n2.ToFloat(); p3[j] = n3.ToFloat(); pa[j] = na.ToFloat();
      }
      for (var j = k; j < total; ++j) { p1[j] = p1[j - k]; p2[j] = p2[j - k]; p3[j] = p3[j - k]; pa[j] = pa[j - k]; }

      var random = new Random(seed);
      var initRadius = Math.Max(1.0, (double)initialRadiusFactor * Math.Max(gw, gh));
      var finRadius = Math.Max(0.25, (double)finalRadius);
      var initEps = (double)initialLearningRate;
      var finEps = Math.Max(1e-4, (double)finalLearningRate);
      var totalSteps = (double)Math.Max(1, epochs * n);
      var step = 0;

      for (var epoch = 0; epoch < epochs; ++epoch) {
        // Seeded Fisher-Yates shuffle of presentation order.
        var order = new int[n];
        for (var i = 0; i < n; ++i) order[i] = i;
        for (var i = n - 1; i > 0; --i) {
          var j = random.Next(i + 1);
          (order[i], order[j]) = (order[j], order[i]);
        }

        for (var oi = 0; oi < n; ++oi) {
          var si = order[oi];
          var x1 = sx1[si]; var x2 = sx2[si]; var x3 = sx3[si]; var xa = sxa[si];

          // Find BMU across all grid cells.
          var bmu = 0;
          var bmuDist = double.MaxValue;
          for (var j = 0; j < total; ++j) {
            var d1 = p1[j] - x1; var d2 = p2[j] - x2; var d3 = p3[j] - x3; var d4 = pa[j] - xa;
            var dist = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
            if (dist >= bmuDist) continue;
            bmuDist = dist; bmu = j;
          }
          var br = bmu / gw; var bc = bmu % gw;

          var t = step / totalSteps;
          var eps = initEps * Math.Pow(finEps / initEps, t);
          var radius = initRadius * Math.Pow(finRadius / initRadius, t);
          var twoSigma2 = 2 * radius * radius;
          var sampleWeight = Math.Sqrt(Math.Max(1, sw[si]));
          var rCap = (int)Math.Ceiling(3 * radius);

          for (var dr = -rCap; dr <= rCap; ++dr)
            for (var dc = -rCap; dc <= rCap; ++dc) {
              var gd2 = (double)dr * dr + (double)dc * dc;
              var h = Math.Exp(-gd2 / twoSigma2);
              if (h < 1e-4) continue;
              var r = br + dr; var c = bc + dc;
              if (r < 0 || r >= gh || c < 0 || c >= gw) continue;
              var j = r * gw + c;
              var update = eps * h * sampleWeight;
              p1[j] += update * (x1 - p1[j]);
              p2[j] += update * (x2 - p2[j]);
              p3[j] += update * (x3 - p3[j]);
              pa[j] += update * (xa - pa[j]);
            }
          ++step;
        }
      }

      // Emit the first k prototypes (row-major grid traversal).
      var palette = new TWork[k];
      for (var j = 0; j < k; ++j)
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, p1[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, p2[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, p3[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, pa[j]))));
      return palette;
    }
  }
}
