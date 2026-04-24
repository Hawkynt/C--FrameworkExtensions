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
/// Locality-Sensitive-Hashing (LSH) approximate bucketing quantizer — Indyk &amp; Motwani (1998).
/// </summary>
/// <remarks>
/// <para>
/// Projects each colour through <see cref="NumProjections"/> random Gaussian hyperplanes, quantises
/// the projection with bucket width <see cref="BucketWidth"/>, and concatenates the integer bucket
/// indices into an LSH hash. Colours that collide in the hash are assigned to the same cluster;
/// palette entries are the weighted centroids of the top-<c>k</c> most-populated buckets.
/// </para>
/// <para>
/// <b>Speed vs quality trade-off:</b> LSH quantization runs in O(n · p) (one projection per
/// colour, constant bucket lookup) — strictly linear in input size, much faster than any
/// clustering-based quantizer. The trade-off is quality: LSH clusters are axis-aligned in hash
/// space, not Voronoi in colour space, so the resulting palette is approximate. Excellent for
/// interactive tools where &lt; 10 ms palette generation matters more than optimal quality.
/// </para>
/// <para>Reference: Indyk &amp; Motwani (1998) — "Approximate nearest neighbors: towards removing
/// the curse of dimensionality", STOC 1998; random-projection LSH variant for L2 (E2LSH, 2004).</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "LSH", Author = "Indyk & Motwani", Year = 1998, QualityRating = 5)]
public struct LshQuantizer : IQuantizer {

  /// <summary>Gets or sets the number of random hyperplane projections.</summary>
  public int NumProjections { get; set; } = 6;

  /// <summary>Gets or sets the width of each hash bucket (normalized space units).</summary>
  public float BucketWidth { get; set; } = 0.1f;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public LshQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.NumProjections, this.BucketWidth, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int numProjections, float bucketWidth, int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0) return [];
      if (colors.Length <= k) return colors.Select(c => c.color);

      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);

      var rng = new Random(seed);
      // Sample random Gaussian projection vectors (rows) and uniform offsets (b_i ∈ [0, W)).
      var p = numProjections;
      var proj = new double[p, 3];
      var off = new double[p];
      for (var i = 0; i < p; ++i) {
        proj[i, 0] = _Gauss(rng);
        proj[i, 1] = _Gauss(rng);
        proj[i, 2] = _Gauss(rng);
        off[i] = rng.NextDouble() * bucketWidth;
      }

      // Hash each colour.
      var buckets = new Dictionary<long, (double c1, double c2, double c3, double a, double w)>();
      foreach (var (col, cnt) in colors) {
        var (c1, c2, c3, a) = col.ToNormalized();
        double f1 = c1.ToFloat(), f2 = c2.ToFloat(), f3 = c3.ToFloat(), fa = a.ToFloat();
        long hash = 0;
        for (var i = 0; i < p; ++i) {
          var v = proj[i, 0] * f1 + proj[i, 1] * f2 + proj[i, 2] * f3 + off[i];
          var bucket = (int)Math.Floor(v / bucketWidth);
          hash = unchecked(hash * 2654435761L + bucket);
        }
        if (!buckets.TryGetValue(hash, out var entry)) entry = (0, 0, 0, 0, 0);
        buckets[hash] = (entry.c1 + f1 * cnt, entry.c2 + f2 * cnt, entry.c3 + f3 * cnt, entry.a + fa * cnt, entry.w + cnt);
      }

      // Top-k by bucket weight.
      var top = buckets.Values.OrderByDescending(b => b.w).Take(k)
        .Select(b => ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, b.c1 / b.w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, b.c2 / b.w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, b.c3 / b.w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, b.a / b.w))))).ToList();

      if (top.Count < k) {
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        top.AddRange(fallback.GeneratePalette(colors, k - top.Count));
      }
      return top.Take(k);
    }

    private static double _Gauss(Random r) {
      double u1 = 1.0 - r.NextDouble();
      double u2 = 1.0 - r.NextDouble();
      return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
  }
}
