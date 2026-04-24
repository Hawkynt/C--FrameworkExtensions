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
/// Neural Gas color quantizer — soft competitive learning from Martinetz &amp; Schulten (1991).
/// </summary>
/// <remarks>
/// <para>
/// For each presented sample all prototypes are ranked by distance and updated with a strength that
/// decays exponentially with rank (neighbourhood size <c>λ</c>). This soft update avoids the dead-unit
/// problem of winner-take-all schemes and typically reaches lower quantization error than K-Means at
/// comparable iteration counts, especially on skewed distributions.
/// </para>
/// <para>
/// Learning rate and neighbourhood width decay geometrically from their initial to final values over
/// the course of training: <c>ε(t) = ε_i (ε_f/ε_i)^(t/T)</c> and analogously for <c>λ</c>.
/// </para>
/// <para>Reference: Martinetz &amp; Schulten (1991) — "A 'Neural-Gas' Network Learns Topologies".</para>
/// </remarks>
[Quantizer(QuantizationType.Neural, DisplayName = "Neural Gas", Author = "Martinetz & Schulten", Year = 1991, QualityRating = 8)]
public struct NeuralGasQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the number of training epochs (presentations of the full sample set).
  /// </summary>
  public int Epochs { get; set; } = 40;

  /// <summary>
  /// Gets or sets the initial learning rate.
  /// </summary>
  public float InitialLearningRate { get; set; } = 0.5f;

  /// <summary>
  /// Gets or sets the final learning rate at the end of training.
  /// </summary>
  public float FinalLearningRate { get; set; } = 0.005f;

  /// <summary>
  /// Gets or sets the initial neighbourhood size relative to <c>k</c> (multiplied internally, typical 0.5-1.0).
  /// </summary>
  public float InitialNeighborhoodFactor { get; set; } = 0.5f;

  /// <summary>
  /// Gets or sets the final neighbourhood size (absolute, typical ~0.01).
  /// </summary>
  public float FinalNeighborhood { get; set; } = 0.01f;

  /// <summary>
  /// Gets or sets the maximum sample size for training.
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>
  /// Gets or sets the deterministic random seed.
  /// </summary>
  public int Seed { get; set; } = 42;

  public NeuralGasQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Epochs,
    this.InitialLearningRate,
    this.FinalLearningRate,
    this.InitialNeighborhoodFactor,
    this.FinalNeighborhood,
    this.MaxSampleSize,
    this.Seed);

  internal sealed class Kernel<TWork>(
    int epochs,
    float initialLearningRate,
    float finalLearningRate,
    float initialNeighborhoodFactor,
    float finalNeighborhood,
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

      // Project to normalized float arrays for fast updates.
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

      // Initialize prototypes from weighted random samples (deterministic given seed).
      var random = new Random(seed);
      var px1 = new double[k];
      var px2 = new double[k];
      var px3 = new double[k];
      var pxa = new double[k];

      var totalWeight = 0.0;
      for (var i = 0; i < n; ++i)
        totalWeight += sw[i];

      for (var j = 0; j < k; ++j) {
        var target = random.NextDouble() * totalWeight;
        var cumulative = 0.0;
        var picked = 0;
        for (var i = 0; i < n; ++i) {
          cumulative += sw[i];
          if (cumulative < target)
            continue;

          picked = i;
          break;
        }

        px1[j] = sx1[picked];
        px2[j] = sx2[picked];
        px3[j] = sx3[picked];
        pxa[j] = sxa[picked];
      }

      // Schedule parameters.
      var initialLambda = Math.Max(1e-3, (double)initialNeighborhoodFactor * k);
      var finalLambda = Math.Max(1e-3, (double)finalNeighborhood);
      var initEps = (double)initialLearningRate;
      var finEps = Math.Max(1e-4, (double)finalLearningRate);
      var totalSteps = (double)Math.Max(1, epochs * n);
      var rankIndex = new int[k];
      var dist = new double[k];

      var step = 0;
      for (var epoch = 0; epoch < epochs; ++epoch) {
        // Shuffle a copy of the sample-order indices for this epoch (deterministic).
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

          // Compute distances to all prototypes.
          for (var j = 0; j < k; ++j) {
            var d1 = px1[j] - x1;
            var d2 = px2[j] - x2;
            var d3 = px3[j] - x3;
            var d4 = pxa[j] - xa;
            dist[j] = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
            rankIndex[j] = j;
          }

          // Sort rankIndex ascending by distance (selection is fine here since k is small).
          Array.Sort(rankIndex, (a, b) => dist[a].CompareTo(dist[b]));

          // Schedule values at this step.
          var t = step / totalSteps;
          var eps = initEps * Math.Pow(finEps / initEps, t);
          var lambda = initialLambda * Math.Pow(finalLambda / initialLambda, t);

          // Weighted sample effect — scale update by sample count so frequent colors get more influence.
          var sampleWeight = Math.Sqrt(Math.Max(1, sw[si]));

          for (var rank = 0; rank < k; ++rank) {
            var proto = rankIndex[rank];
            var h = Math.Exp(-rank / lambda);
            if (h < 1e-6)
              break; // negligible update for remaining ranks

            var update = eps * h * sampleWeight;
            px1[proto] += update * (x1 - px1[proto]);
            px2[proto] += update * (x2 - px2[proto]);
            px3[proto] += update * (x3 - px3[proto]);
            pxa[proto] += update * (xa - pxa[proto]);
          }

          ++step;
        }
      }

      var palette = new TWork[k];
      for (var j = 0; j < k; ++j)
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, px1[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, px2[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, px3[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, pxa[j])))
        );

      return palette;
    }

  }
}
