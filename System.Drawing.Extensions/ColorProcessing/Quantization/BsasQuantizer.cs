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
/// Basic Sequential Algorithmic Scheme (BSAS) colour quantizer — Theodoridis &amp; Koutroumbas (2003).
/// </summary>
/// <remarks>
/// <para>
/// A single-pass greedy clusterer: for each input colour, assign it to the
/// nearest cluster if the distance is below the threshold <see cref="Threshold"/>;
/// otherwise create a new cluster (up to the target palette size <c>k</c>).
/// Cluster centres are updated incrementally with weighted means. No
/// iteration — O(n·k) in total, one input pass.
/// </para>
/// <para>
/// Distinct from the existing <see cref="ForelQuantizer"/> (Zagoruiko 1968
/// sphere-based clustering — iterates on the full dataset re-centring spheres
/// until convergence). BSAS is strictly single-pass, making it the fastest
/// adaptive quantizer in the registry after LSH; the threshold <c>Θ</c> auto-
/// adjusts to yield at most <c>k</c> clusters via an outer bisection loop.
/// </para>
/// <para>
/// Reference: S. Theodoridis &amp; K. Koutroumbas (2003) — "Pattern Recognition"
/// (2nd ed.) §14.1, Academic Press. The scheme is attributed to the 1960s
/// ISODATA lineage but BSAS-proper is the textbook single-pass specialisation.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "BSAS", Author = "Theodoridis & Koutroumbas", Year = 2003, QualityRating = 5)]
public struct BsasQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the distance threshold Θ (normalised units). When 0 (default),
  /// an outer bisection picks a threshold that yields exactly k clusters.
  /// </summary>
  public float Threshold { get; set; } = 0f;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed (used for presentation ordering).</summary>
  public int Seed { get; set; } = 42;

  public BsasQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Threshold, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    float threshold, int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0) return [];
      if (colors.Length <= k) return colors.Select(c => c.color);

      var sampled = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);
      var n = sampled.Length;
      var x = new double[n]; var y = new double[n]; var z = new double[n]; var a = new double[n]; var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, al) = sampled[i].color.ToNormalized();
        x[i] = c1.ToFloat(); y[i] = c2.ToFloat(); z[i] = c3.ToFloat(); a[i] = al.ToFloat();
        w[i] = Math.Max(1, sampled[i].count);
      }

      // Deterministic presentation order: weighted-descending so heavy colours seed early.
      var order = Enumerable.Range(0, n).OrderByDescending(i => w[i]).ToArray();

      // If explicit threshold given, use it; else bisection for at most k clusters.
      double thr = threshold > 0 ? threshold : _Bisect(x, y, z, a, w, order, k);
      var (cx, cy, cz, ca, cw) = _Run(x, y, z, a, w, order, k, thr);
      var actual = cx.Length;

      // If fewer than k clusters emerged, repeatedly split the heaviest cluster to pad to k.
      var lx = cx.ToList(); var ly = cy.ToList(); var lz = cz.ToList(); var la = ca.ToList(); var lw = cw.ToList();
      while (lx.Count < k) {
        // Find heaviest cluster, jitter-split.
        var hi = 0;
        for (var i = 1; i < lx.Count; ++i) if (lw[i] > lw[hi]) hi = i;
        var jitter = 1e-3;
        lx.Add(lx[hi] + jitter); ly.Add(ly[hi] - jitter); lz.Add(lz[hi] + jitter); la.Add(la[hi]); lw.Add(lw[hi] * 0.5);
        lw[hi] *= 0.5;
      }

      var palette = new TWork[k];
      for (var j = 0; j < k; ++j)
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, lx[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, ly[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, lz[j]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, la[j]))));
      return palette;
    }

    private static double _Bisect(double[] x, double[] y, double[] z, double[] a, double[] w, int[] order, int k) {
      // Find a threshold that produces exactly ≤ k clusters but close to k.
      // Bisection: very small Θ → many clusters; very large Θ → 1 cluster.
      double lo = 1e-4, hi = 2.0; // diameter of 4-D unit cube ≈ 2
      double chosen = lo;
      for (var it = 0; it < 30; ++it) {
        var mid = 0.5 * (lo + hi);
        var (cx, _, _, _, _) = _Run(x, y, z, a, w, order, k + 1, mid);
        if (cx.Length > k) lo = mid; else { chosen = mid; hi = mid; }
      }
      return chosen;
    }

    private static (double[] cx, double[] cy, double[] cz, double[] ca, double[] cw) _Run(
      double[] x, double[] y, double[] z, double[] a, double[] w, int[] order, int kmax, double thr) {
      var cx = new List<double>(); var cy = new List<double>(); var cz = new List<double>();
      var ca = new List<double>(); var cw = new List<double>();
      var t2 = thr * thr;
      foreach (var i in order) {
        // Nearest cluster.
        var best = -1; var bestD = double.MaxValue;
        for (var c = 0; c < cx.Count; ++c) {
          var dx = x[i] - cx[c]; var dy = y[i] - cy[c]; var dz = z[i] - cz[c]; var da = a[i] - ca[c];
          var d = dx * dx + dy * dy + dz * dz + da * da;
          if (d < bestD) { bestD = d; best = c; }
        }
        if (best < 0 || (bestD > t2 && cx.Count < kmax)) {
          cx.Add(x[i]); cy.Add(y[i]); cz.Add(z[i]); ca.Add(a[i]); cw.Add(w[i]);
        } else {
          // Weighted incremental mean.
          var nw = cw[best] + w[i];
          cx[best] = (cx[best] * cw[best] + x[i] * w[i]) / nw;
          cy[best] = (cy[best] * cw[best] + y[i] * w[i]) / nw;
          cz[best] = (cz[best] * cw[best] + z[i] * w[i]) / nw;
          ca[best] = (ca[best] * cw[best] + a[i] * w[i]) / nw;
          cw[best] = nw;
        }
      }
      return (cx.ToArray(), cy.ToArray(), cz.ToArray(), ca.ToArray(), cw.ToArray());
    }

  }
}
