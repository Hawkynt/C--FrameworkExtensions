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
/// X-Means quantizer — K-Means with automatic k-selection via Bayesian Information Criterion
/// (Pelleg &amp; Moore, 2000).
/// </summary>
/// <remarks>
/// <para>
/// Starts with <c>k=2</c> and repeatedly splits each cluster into two sub-clusters. For every
/// candidate split the BIC score before and after splitting is compared; the split is accepted
/// only if it improves BIC. The algorithm halts when no cluster benefits from splitting
/// <i>or</i> the target palette size <c>k</c> is reached (whichever first).
/// </para>
/// <para>
/// <b>Why it's distinct:</b> regular K-Means takes <c>k</c> as input. X-Means uses BIC as a
/// principled Occam's-razor criterion to <i>grow</i> the cluster count data-adaptively. On
/// palettes where the requested <c>k</c> far exceeds the natural modality of the histogram,
/// X-Means will stop early and return a smaller palette — preventing artificial over-splitting.
/// </para>
/// <para>Reference: Pelleg &amp; Moore (2000) — "X-means: Extending K-means with Efficient Estimation
/// of the Number of Clusters", ICML 2000.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "X-Means", Author = "Pelleg & Moore", Year = 2000, QualityRating = 7)]
public struct XMeansQuantizer : IQuantizer {

  /// <summary>Gets or sets the maximum K-Means iterations per inner fit.</summary>
  public int MaxIterations { get; set; } = 20;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public XMeansQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MaxIterations, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(int maxIterations, int maxSampleSize, int seed) : IQuantizer<TWork>
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
      // Assignment of each point to a cluster id.
      var assign = new int[n];
      // Start with 1 cluster covering everything.
      var numClusters = 1;
      // Run K-Means to initialise; here only the centroid is needed, it's the overall mean.
      var clusters = new List<Cluster> { Cluster.FromPoints(x, y, z, a, w, Enumerable.Range(0, n).ToArray()) };

      var queue = new Queue<int>();
      queue.Enqueue(0);

      while (queue.Count > 0 && numClusters < k) {
        var ci = queue.Dequeue();
        var cluster = clusters[ci];
        if (cluster.Members.Length < 4) continue;
        // Split into 2 via K-Means with K=2.
        var (childA, childB) = _Split2(x, y, z, a, w, cluster.Members, rng, maxIterations);
        if (childA.Members.Length == 0 || childB.Members.Length == 0) continue;

        var bicBefore = _Bic(cluster, 1);
        var bicAfter = _Bic(childA, 2) + _Bic(childB, 2);
        // Reject the split if BIC doesn't improve.
        if (bicAfter <= bicBefore) continue;
        // Accept split.
        clusters[ci] = childA;
        clusters.Add(childB);
        queue.Enqueue(ci);
        queue.Enqueue(clusters.Count - 1);
        ++numClusters;
      }

      // If we didn't reach k clusters, return whatever we have — short-palette padding happens
      // at the pipeline level.
      return clusters.Select(c => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)c.Cx),
        UNorm32.FromFloatClamped((float)c.Cy),
        UNorm32.FromFloatClamped((float)c.Cz),
        UNorm32.FromFloatClamped((float)c.Ca)));
    }

    private static (Cluster a, Cluster b) _Split2(double[] x, double[] y, double[] z, double[] a, double[] w, int[] members, Random rng, int maxIter) {
      if (members.Length < 2) {
        var one = Cluster.FromPoints(x, y, z, a, w, members);
        return (one, Cluster.FromPoints(x, y, z, a, w, []));
      }
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
        for (var k = 0; k < members.Length; ++k) {
          var idx = members[k];
          var dxA = x[idx] - mx1; var dyA = y[idx] - my1; var dzA = z[idx] - mz1; var daA = a[idx] - ma1;
          var dxB = x[idx] - mx2; var dyB = y[idx] - my2; var dzB = z[idx] - mz2; var daB = a[idx] - ma2;
          var dA = dxA * dxA + dyA * dyA + dzA * dzA + daA * daA;
          var dB = dxB * dxB + dyB * dyB + dzB * dzB + daB * daB;
          if (dA < dB) bucketA.Add(idx); else bucketB.Add(idx);
        }
        if (bucketA.Count == 0 || bucketB.Count == 0) break;
        var (nx1, ny1, nz1, na1) = _Centroid(x, y, z, a, w, bucketA);
        var (nx2, ny2, nz2, na2) = _Centroid(x, y, z, a, w, bucketB);
        var move = Math.Abs(nx1 - mx1) + Math.Abs(ny1 - my1) + Math.Abs(nz1 - mz1) +
                   Math.Abs(nx2 - mx2) + Math.Abs(ny2 - my2) + Math.Abs(nz2 - mz2);
        mx1 = nx1; my1 = ny1; mz1 = nz1; ma1 = na1;
        mx2 = nx2; my2 = ny2; mz2 = nz2; ma2 = na2;
        if (move < 1e-6) break;
      }
      return (Cluster.FromPoints(x, y, z, a, w, bucketA.ToArray()),
              Cluster.FromPoints(x, y, z, a, w, bucketB.ToArray()));
    }

    private static (double, double, double, double) _Centroid(double[] x, double[] y, double[] z, double[] a, double[] w, List<int> members) {
      double s1 = 0, s2 = 0, s3 = 0, sa = 0, sw = 0;
      foreach (var i in members) {
        s1 += x[i] * w[i]; s2 += y[i] * w[i]; s3 += z[i] * w[i]; sa += a[i] * w[i]; sw += w[i];
      }
      if (sw <= 0) return (0, 0, 0, 0);
      return (s1 / sw, s2 / sw, s3 / sw, sa / sw);
    }

    private static double _Bic(Cluster c, int totalClusters) {
      // Spherical Gaussian approximation, BIC = log-likelihood - (p/2) log n.
      var n = c.TotalWeight;
      if (n <= 1 || c.Variance <= 0) return double.NegativeInfinity;
      const int dim = 3;
      var ll = -n / 2.0 * (Math.Log(2 * Math.PI) + dim * Math.Log(c.Variance) + dim);
      // Free parameters per cluster: 1 (weight) + dim (mean) + 1 (variance).
      var p = (totalClusters - 1) + dim * totalClusters + 1;
      return ll - p / 2.0 * Math.Log(n);
    }

    private sealed class Cluster {
      public int[] Members { get; private set; } = [];
      public double Cx { get; private set; }
      public double Cy { get; private set; }
      public double Cz { get; private set; }
      public double Ca { get; private set; }
      public double TotalWeight { get; private set; }
      public double Variance { get; private set; }

      public static Cluster FromPoints(double[] x, double[] y, double[] z, double[] a, double[] w, int[] members) {
        var c = new Cluster { Members = members };
        if (members.Length == 0) return c;
        double s1 = 0, s2 = 0, s3 = 0, sa = 0, sw = 0;
        foreach (var i in members) { s1 += x[i] * w[i]; s2 += y[i] * w[i]; s3 += z[i] * w[i]; sa += a[i] * w[i]; sw += w[i]; }
        if (sw <= 0) return c;
        c.Cx = s1 / sw; c.Cy = s2 / sw; c.Cz = s3 / sw; c.Ca = sa / sw;
        c.TotalWeight = sw;
        double varSum = 0;
        foreach (var i in members) {
          var dx = x[i] - c.Cx; var dy = y[i] - c.Cy; var dz = z[i] - c.Cz;
          varSum += (dx * dx + dy * dy + dz * dz) * w[i];
        }
        c.Variance = Math.Max(varSum / (3.0 * sw), 1e-12);
        return c;
      }
    }
  }
}
