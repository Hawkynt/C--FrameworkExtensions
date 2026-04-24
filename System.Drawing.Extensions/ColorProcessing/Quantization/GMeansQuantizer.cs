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
/// G-Means quantizer — K-Means with Anderson-Darling-test-based automatic k-selection
/// (Hamerly &amp; Elkan, 2003).
/// </summary>
/// <remarks>
/// <para>
/// Related to <see cref="XMeansQuantizer"/> but uses a different split criterion: each candidate
/// cluster is projected onto its principal axis and the projection is tested for normality via
/// the Anderson-Darling statistic. Non-Gaussian clusters are split into two; Gaussian clusters
/// are kept.
/// </para>
/// <para>
/// <b>X-Means vs G-Means:</b> X-Means uses information-criterion-based model selection (BIC),
/// which favours parsimony. G-Means uses a null-hypothesis statistical test, which favours
/// <i>fit</i> to the Gaussian mixture assumption. On visually-multimodal colour histograms
/// (sharp edges, logos, pixel art) G-Means typically discovers more clusters than X-Means.
/// </para>
/// <para>Reference: Hamerly &amp; Elkan (2003) — "Learning the k in k-means", NIPS 2003.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "G-Means", Author = "Hamerly & Elkan", Year = 2003, QualityRating = 7)]
public struct GMeansQuantizer : IQuantizer {

  /// <summary>Gets or sets the Anderson-Darling critical value (α = 0.0001 → ≈ 1.8692).</summary>
  public float Alpha { get; set; } = 1.8692f;

  /// <summary>Gets or sets the maximum K-Means iterations per inner fit.</summary>
  public int MaxIterations { get; set; } = 20;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public GMeansQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Alpha, this.MaxIterations, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(float alpha, int maxIterations, int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0) return [];
      if (colors.Length <= k) return colors.Select(c => c.color);

      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);
      var n = colors.Length;
      var x = new double[n]; var y = new double[n]; var z = new double[n]; var a = new double[n];
      var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, ca) = colors[i].color.ToNormalized();
        x[i] = c1.ToFloat(); y[i] = c2.ToFloat(); z[i] = c3.ToFloat(); a[i] = ca.ToFloat();
        w[i] = Math.Max(1, colors[i].count);
      }

      var rng = new Random(seed);
      var clusters = new List<int[]> { Enumerable.Range(0, n).ToArray() };
      var queue = new Queue<int>();
      queue.Enqueue(0);

      while (queue.Count > 0 && clusters.Count < k) {
        var ci = queue.Dequeue();
        var members = clusters[ci];
        if (members.Length < 8) continue;
        // Anderson-Darling test on principal-axis projection.
        var adStatistic = _AdStatistic(x, y, z, members);
        if (adStatistic <= alpha) continue; // Gaussian-enough → don't split.

        var (bucketA, bucketB) = _Split2(x, y, z, a, w, members, rng, maxIterations);
        if (bucketA.Length == 0 || bucketB.Length == 0) continue;
        clusters[ci] = bucketA;
        clusters.Add(bucketB);
        queue.Enqueue(ci);
        queue.Enqueue(clusters.Count - 1);
      }

      return clusters.Select(m => {
        double s1 = 0, s2 = 0, s3 = 0, sa = 0, sw = 0;
        foreach (var i in m) { s1 += x[i] * w[i]; s2 += y[i] * w[i]; s3 += z[i] * w[i]; sa += a[i] * w[i]; sw += w[i]; }
        if (sw <= 0) return default(TWork);
        return ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s1 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s2 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s3 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sa / sw))));
      }).Where(c => !c.Equals(default(TWork)));
    }

    private static double _AdStatistic(double[] x, double[] y, double[] z, int[] members) {
      // Project on first principal axis, normalise to N(0,1), compute Anderson-Darling.
      var n = members.Length;
      // Principal axis via power-iteration on 3×3 covariance.
      double mx = 0, my = 0, mz = 0;
      foreach (var i in members) { mx += x[i]; my += y[i]; mz += z[i]; }
      mx /= n; my /= n; mz /= n;
      double cxx = 0, cxy = 0, cxz = 0, cyy = 0, cyz = 0, czz = 0;
      foreach (var i in members) {
        var dx = x[i] - mx; var dy = y[i] - my; var dz = z[i] - mz;
        cxx += dx * dx; cxy += dx * dy; cxz += dx * dz;
        cyy += dy * dy; cyz += dy * dz; czz += dz * dz;
      }
      // Power iteration.
      double vx = 1, vy = 0, vz = 0;
      for (var it = 0; it < 30; ++it) {
        var nx = cxx * vx + cxy * vy + cxz * vz;
        var ny = cxy * vx + cyy * vy + cyz * vz;
        var nz = cxz * vx + cyz * vy + czz * vz;
        var norm = Math.Sqrt(nx * nx + ny * ny + nz * nz);
        if (norm < 1e-12) break;
        vx = nx / norm; vy = ny / norm; vz = nz / norm;
      }
      var proj = new double[n];
      for (var i = 0; i < n; ++i) {
        var idx = members[i];
        proj[i] = (x[idx] - mx) * vx + (y[idx] - my) * vy + (z[idx] - mz) * vz;
      }
      Array.Sort(proj);
      double mean = 0, var = 0;
      for (var i = 0; i < n; ++i) mean += proj[i];
      mean /= n;
      for (var i = 0; i < n; ++i) { var dd = proj[i] - mean; var += dd * dd; }
      var = Math.Max(var / Math.Max(1, n - 1), 1e-12);
      var sd = Math.Sqrt(var);
      // Anderson-Darling for N(0,1).
      var A2 = 0.0;
      for (var i = 0; i < n; ++i) {
        var u = _NormCdf((proj[i] - mean) / sd);
        u = Math.Max(1e-12, Math.Min(1 - 1e-12, u));
        A2 += (2 * i + 1) * Math.Log(u) + (2 * (n - i) - 1) * Math.Log(1 - u);
      }
      A2 = -n - A2 / n;
      // Small-sample correction.
      A2 *= (1.0 + 0.75 / n + 2.25 / (n * n));
      return A2;
    }

    private static double _NormCdf(double x) {
      // Abramowitz-Stegun approximation.
      var t = 1.0 / (1.0 + 0.2316419 * Math.Abs(x));
      var d = 0.3989422804 * Math.Exp(-x * x / 2);
      var p = d * t * ((((1.330274429 * t - 1.821255978) * t + 1.781477937) * t - 0.356563782) * t + 0.319381530);
      return x >= 0 ? 1.0 - p : p;
    }

    private static (int[] a, int[] b) _Split2(double[] x, double[] y, double[] z, double[] a, double[] w, int[] members, Random rng, int maxIter) {
      if (members.Length < 2) return (members, []);
      var i1 = members[rng.Next(members.Length)];
      var i2 = members[rng.Next(members.Length)];
      var tries = 0;
      while (i1 == i2 && tries < 10) { i2 = members[rng.Next(members.Length)]; ++tries; }
      double mx1 = x[i1], my1 = y[i1], mz1 = z[i1], ma1 = a[i1];
      double mx2 = x[i2], my2 = y[i2], mz2 = z[i2], ma2 = a[i2];
      var bucketA = new List<int>();
      var bucketB = new List<int>();
      for (var iter = 0; iter < maxIter; ++iter) {
        bucketA.Clear(); bucketB.Clear();
        foreach (var idx in members) {
          var dxA = x[idx] - mx1; var dyA = y[idx] - my1; var dzA = z[idx] - mz1; var daA = a[idx] - ma1;
          var dxB = x[idx] - mx2; var dyB = y[idx] - my2; var dzB = z[idx] - mz2; var daB = a[idx] - ma2;
          var dA = dxA * dxA + dyA * dyA + dzA * dzA + daA * daA;
          var dB = dxB * dxB + dyB * dyB + dzB * dzB + daB * daB;
          if (dA < dB) bucketA.Add(idx); else bucketB.Add(idx);
        }
        if (bucketA.Count == 0 || bucketB.Count == 0) break;
        double sx1 = 0, sy1 = 0, sz1 = 0, sa1 = 0, sw1 = 0;
        foreach (var i in bucketA) { sx1 += x[i] * w[i]; sy1 += y[i] * w[i]; sz1 += z[i] * w[i]; sa1 += a[i] * w[i]; sw1 += w[i]; }
        double sx2 = 0, sy2 = 0, sz2 = 0, sa2 = 0, sw2 = 0;
        foreach (var i in bucketB) { sx2 += x[i] * w[i]; sy2 += y[i] * w[i]; sz2 += z[i] * w[i]; sa2 += a[i] * w[i]; sw2 += w[i]; }
        if (sw1 <= 0 || sw2 <= 0) break;
        var nx1 = sx1 / sw1; var ny1 = sy1 / sw1; var nz1 = sz1 / sw1; var na1 = sa1 / sw1;
        var nx2 = sx2 / sw2; var ny2 = sy2 / sw2; var nz2 = sz2 / sw2; var na2 = sa2 / sw2;
        var move = Math.Abs(nx1 - mx1) + Math.Abs(ny1 - my1) + Math.Abs(nz1 - mz1) +
                   Math.Abs(nx2 - mx2) + Math.Abs(ny2 - my2) + Math.Abs(nz2 - mz2);
        mx1 = nx1; my1 = ny1; mz1 = nz1; ma1 = na1;
        mx2 = nx2; my2 = ny2; mz2 = nz2; ma2 = na2;
        if (move < 1e-6) break;
      }
      return (bucketA.ToArray(), bucketB.ToArray());
    }
  }
}
