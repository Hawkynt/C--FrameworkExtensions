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
/// Wrapper that refines any quantizer's output using Ant Colony Optimization.
/// </summary>
/// <remarks>
/// <para>
/// Uses a bio-inspired optimization technique based on ant foraging behavior
/// to find optimal color-to-palette assignments. Ants probabilistically explore
/// color-palette mappings, depositing "pheromones" on good solutions.
/// </para>
/// <para>
/// This is more computationally expensive than simple K-means refinement but
/// may produce better results for complex color distributions by escaping
/// local optima.
/// </para>
/// </remarks>
/// <typeparam name="TInner">The type of the wrapped quantizer.</typeparam>
[Quantizer(QuantizationType.Postprocessing, DisplayName = "ACO Refinement", QualityRating = 0)]
public readonly struct AcoRefinementWrapper<TInner> : IQuantizer
  where TInner : struct, IQuantizer {

  private readonly TInner _inner;
  private readonly int _antCount;
  private readonly int _iterations;
  private readonly double _evaporationRate;
  private readonly int? _seed;

  /// <summary>
  /// Creates an ACO refinement wrapper around the specified quantizer.
  /// </summary>
  /// <param name="inner">The quantizer to wrap.</param>
  /// <param name="antCount">Number of ants. Default is 20.</param>
  /// <param name="iterations">Number of iterations. Default is 50.</param>
  /// <param name="evaporationRate">Pheromone evaporation rate (0-1). Default is 0.1.</param>
  /// <param name="seed">Random seed for reproducibility. Null for non-deterministic behavior.</param>
  public AcoRefinementWrapper(
    TInner inner,
    int antCount = QuantizerHelper.DefaultAntCount,
    int iterations = QuantizerHelper.DefaultAcoIterations,
    double evaporationRate = 0.1,
    int? seed = null) {
    this._inner = inner;
    this._antCount = Math.Max(1, antCount);
    this._iterations = Math.Max(1, iterations);
    this._evaporationRate = Math.Max(0, Math.Min(1, evaporationRate));
    this._seed = seed;
  }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>()
    => new Kernel<TWork>(((IQuantizer)this._inner).CreateKernel<TWork>(),
      this._antCount, this._iterations, this._evaporationRate, this._seed);

  private sealed class Kernel<TWork>(
    IQuantizer<TWork> innerKernel,
    int antCount,
    int iterations,
    double evaporationRate,
    int? seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      var histArray = histogram.ToArray();
      if (histArray.Length == 0)
        return [];

      // Get initial palette from wrapped quantizer
      var initialPalette = innerKernel.GeneratePalette(histArray, colorCount);
      if (initialPalette.Length == 0)
        return initialPalette;

      // Optimize using ACO
      return QuantizerHelper.OptimizePaletteWithACO(
        histArray, initialPalette,
        antCount, iterations, evaporationRate, seed: seed);
    }
  }
}
