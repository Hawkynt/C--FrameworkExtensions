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
/// Count-Min Sketch colour quantizer — Cormode &amp; Muthukrishnan (2005).
/// </summary>
/// <remarks>
/// <para>
/// Projects the input histogram through <see cref="SketchDepth"/> independent
/// hash functions into a <c>d × SketchWidth</c> counter matrix; the estimated
/// frequency of any colour is <c>min(row_i[h_i(colour)] for all i)</c>. Palette
/// entries are then the top-<c>k</c> colours by estimated count — a
/// <i>popularity quantizer with bounded memory</i>.
/// </para>
/// <para>
/// Distinct from the existing <see cref="PopularityQuantizer"/> (stores exact
/// counts in a full dictionary — memory scales with unique colour count) and
/// <see cref="LshQuantizer"/> (bucket centroids, not popularity). CMS is useful
/// when the colour distribution has a very long tail and exact counts are
/// prohibitively expensive; ε·N over-estimation is guaranteed with probability
/// ≥ 1 − δ for <c>width = ⌈e/ε⌉</c>, <c>depth = ⌈ln(1/δ)⌉</c>.
/// </para>
/// <para>
/// Reference: Graham Cormode &amp; S. Muthukrishnan (2005) — "An improved data
/// stream summary: the count-min sketch and its applications",
/// Journal of Algorithms 55(1):58-75.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Count-Min Sketch", Author = "Cormode & Muthukrishnan", Year = 2005, QualityRating = 3)]
public struct CountMinSketchQuantizer : IQuantizer {

  /// <summary>Gets or sets the sketch width (per-row counter array length).</summary>
  public int SketchWidth { get; set; } = 1024;

  /// <summary>Gets or sets the sketch depth (number of hash rows).</summary>
  public int SketchDepth { get; set; } = 4;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public CountMinSketchQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.SketchWidth, this.SketchDepth, this.Seed);

  internal sealed class Kernel<TWork>(
    int sketchWidth, int sketchDepth, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      if (colorCount <= 0) return [];

      // Aggregate distinct colours.
      var items = histogram
        .GroupBy(h => h.color.ToNormalized())
        .Select(g => (color: g.First().color, count: (uint)g.Sum(h => h.count)))
        .ToArray();

      if (items.Length == 0) return [];
      if (items.Length <= colorCount) return items.Select(c => c.color).ToArray();

      var width = Math.Max(16, sketchWidth);
      var depth = Math.Max(1, sketchDepth);
      var table = new ulong[depth, width];
      var random = new Random(seed);
      var a = new ulong[depth];
      var b = new ulong[depth];
      for (var d = 0; d < depth; ++d) {
        a[d] = ((ulong)random.Next() << 32 | (uint)random.Next()) | 1UL; // ensure non-zero
        b[d] = (ulong)random.Next() << 32 | (uint)random.Next();
      }

      // Feed histogram into sketch.
      foreach (var (color, count) in items) {
        var key = _ColorKey(color);
        for (var d = 0; d < depth; ++d) {
          var h = (uint)((a[d] * key + b[d]) % (ulong)width);
          table[d, h] += count;
        }
      }

      // Estimate count for each distinct colour and keep top-k.
      var estimates = new (TWork color, ulong est)[items.Length];
      for (var i = 0; i < items.Length; ++i) {
        var key = _ColorKey(items[i].color);
        var minEst = ulong.MaxValue;
        for (var d = 0; d < depth; ++d) {
          var h = (uint)((a[d] * key + b[d]) % (ulong)width);
          var v = table[d, h];
          if (v < minEst) minEst = v;
        }
        estimates[i] = (items[i].color, minEst);
      }

      return estimates
        .OrderByDescending(e => e.est)
        .Take(colorCount)
        .Select(e => e.color)
        .ToArray();
    }

    private static ulong _ColorKey(TWork color) {
      var (c1, c2, c3, a) = color.ToNormalized();
      // Pack all four 32-bit raw norm values into a 64-bit hash seed via FNV-like mix.
      var h = 1469598103934665603UL;
      h = (h ^ c1.RawValue) * 1099511628211UL;
      h = (h ^ c2.RawValue) * 1099511628211UL;
      h = (h ^ c3.RawValue) * 1099511628211UL;
      h = (h ^ a.RawValue) * 1099511628211UL;
      return h;
    }
  }
}
