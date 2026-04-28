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
/// Standalone Ant Colony Optimisation colour quantizer — Dorigo, Maniezzo &amp; Colorni (1996).
/// </summary>
/// <remarks>
/// <para>
/// Each ant builds an assignment from every histogram colour to one of <c>k</c>
/// palette slots. Colour-to-slot preferences are biased by pheromone trail
/// <c>τ_{ij}</c> and heuristic desirability <c>η_{ij} = 1/d(colour_i, slot_j)</c>
/// via the classical transition rule
/// <c>p_{ij} ∝ τ_{ij}^α · η_{ij}^β</c>. After each iteration, pheromones
/// evaporate uniformly and successful ants deposit proportional to solution
/// quality. Palette slots are initialised to K-Means++ seeds and re-fit
/// as weighted centroids of the best ant's assignment at each round.
/// </para>
/// <para>
/// Distinct from the existing <see cref="AcoRefinementWrapper{TInner}"/> (post-
/// processing wrapper that refines another quantizer's output using the shared
/// ACO helper). This quantizer is a <i>standalone</i> ACO palette-builder:
/// it seeds itself and runs the full ACO search without any inner quantizer.
/// </para>
/// <para>
/// Reference: M. Dorigo, V. Maniezzo &amp; A. Colorni (1996) — "Ant System:
/// Optimization by a colony of cooperating agents", IEEE Transactions on
/// Systems, Man, and Cybernetics 26(1):29-41. The colour-quantization
/// specialisation follows Omran (2005).
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Ant Colony", Author = "Dorigo et al.", Year = 1996, QualityRating = 7)]
public struct AntColonyQuantizer : IQuantizer {

  /// <summary>Gets or sets the number of ants.</summary>
  public int AntCount { get; set; } = 20;

  /// <summary>Gets or sets the number of ACO iterations.</summary>
  public int Iterations { get; set; } = 40;

  /// <summary>Gets or sets the pheromone evaporation rate (0-1).</summary>
  public double EvaporationRate { get; set; } = 0.1;

  /// <summary>Gets or sets the pheromone influence factor α.</summary>
  public double Alpha { get; set; } = 1.0;

  /// <summary>Gets or sets the heuristic influence factor β.</summary>
  public double Beta { get; set; } = 2.0;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = 1024;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public AntColonyQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.AntCount, this.Iterations, this.EvaporationRate, this.Alpha, this.Beta,
    this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int antCount, int iterations, double evaporationRate, double alpha, double beta,
    int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0) return [];
      if (colors.Length <= k) return colors.Select(c => c.color);

      var sampled = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);

      // Seed palette via K-Means++ using helper.
      var initPalette = QuantizerHelper.InitializePaletteWithPCA(sampled, k, seed);

      return QuantizerHelper.OptimizePaletteWithACO(
        sampled, initPalette,
        antCount, iterations, evaporationRate, alpha, beta, seed);
    }
  }
}
