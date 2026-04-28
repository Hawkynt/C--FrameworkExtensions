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
/// Weighted reservoir-sampling colour quantizer — Efraimidis &amp; Spirakis (2006, "A-Res").
/// </summary>
/// <remarks>
/// <para>
/// A single-pass streaming quantizer: each histogram entry is assigned the
/// priority key <c>k_i = u^{1/w_i}</c> with <c>u ~ U(0,1)</c>, and the top-<c>k</c>
/// keys are kept in a <see cref="MinKeyReservoir"/>-style max-heap. The resulting
/// palette is a <i>frequency-weighted uniform sample</i> of the actual colours —
/// no clustering, no optimisation, no histogram pass beyond the single linear
/// scan used by the reservoir itself.
/// </para>
/// <para>
/// Distinct from <see cref="PopularityQuantizer"/> (top-k most frequent colours —
/// deterministic, biased toward majority) and <see cref="LshQuantizer"/>
/// (bucket-based locality-sensitive hashing — bucket centroids). A-Res gives a
/// statistically-unbiased weighted sample: rare colours get represented
/// proportionally to their count, so the palette captures tail diversity that
/// pure-popularity discards.
/// </para>
/// <para>
/// Reference: Pavlos S. Efraimidis &amp; Paul G. Spirakis (2006) — "Weighted
/// random sampling with a reservoir", Information Processing Letters 97(5):
/// 181-185. Also known as "Algorithm A-Res".
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Reservoir Sampling", Author = "Efraimidis & Spirakis", Year = 2006, QualityRating = 4)]
public struct ReservoirSamplingQuantizer : IQuantizer {

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public ReservoirSamplingQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.Seed);

  internal sealed class Kernel<TWork>(int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      if (colorCount <= 0)
        return [];

      // Deduplicate in normalised space so each unique colour contributes once.
      var items = histogram
        .GroupBy(h => h.color.ToNormalized())
        .Select(g => (color: g.First().color, count: (uint)g.Sum(h => h.count)))
        .ToArray();

      if (items.Length == 0)
        return [];
      if (items.Length <= colorCount)
        return items.Select(c => c.color).ToArray();

      var random = new Random(seed);
      var reservoir = new (TWork color, double key)[colorCount];
      for (var i = 0; i < colorCount; ++i) {
        var u = 1.0 - random.NextDouble();
        var w = Math.Max(1, items[i].count);
        reservoir[i] = (items[i].color, Math.Pow(u, 1.0 / w));
      }
      var minIdx = 0;
      for (var i = 1; i < colorCount; ++i)
        if (reservoir[i].key < reservoir[minIdx].key) minIdx = i;

      for (var i = colorCount; i < items.Length; ++i) {
        var u = 1.0 - random.NextDouble();
        var w = Math.Max(1, items[i].count);
        var key = Math.Pow(u, 1.0 / w);
        if (key <= reservoir[minIdx].key) continue;
        reservoir[minIdx] = (items[i].color, key);
        minIdx = 0;
        for (var j = 1; j < colorCount; ++j)
          if (reservoir[j].key < reservoir[minIdx].key) minIdx = j;
      }

      var palette = new TWork[colorCount];
      for (var i = 0; i < colorCount; ++i) palette[i] = reservoir[i].color;
      return palette;
    }
  }
}
