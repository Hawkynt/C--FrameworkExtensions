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
/// ISODATA (Iterative Self-Organizing Data Analysis Technique Algorithm) — Ball &amp; Hall (1965).
/// </summary>
/// <remarks>
/// <para>
/// ISODATA augments the basic K-Means loop with two heuristics applied each iteration:
/// </para>
/// <list type="bullet">
///   <item><description><b>Split</b>: any cluster whose standard deviation along any axis
///     exceeds <see cref="SplitThreshold"/> is bisected.</description></item>
///   <item><description><b>Merge</b>: any pair of cluster centroids closer than
///     <see cref="MergeThreshold"/> is fused into one.</description></item>
/// </list>
/// <para>
/// The result is a cluster-count-free variant of K-Means: the requested <c>k</c> is treated as
/// an upper bound; the actual number of clusters emerges from the split-merge dynamics. Predates
/// DBSCAN and Mean-Shift by decades and is still used in remote-sensing image classification.
/// </para>
/// <para>
/// Historically significant: ISODATA is the original "unsupervised K-Means with automatic k"
/// algorithm, published in 1965 — thirty-five years before X-Means.
/// </para>
/// <para>Reference: Ball &amp; Hall (1965) — "ISODATA, A Novel Method of Data Analysis and Pattern
/// Classification", Stanford Research Institute.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "ISODATA", Author = "Ball & Hall", Year = 1965, QualityRating = 7)]
public struct IsodataQuantizer : IQuantizer {

  /// <summary>Gets or sets the stddev threshold above which a cluster is split.</summary>
  public float SplitThreshold { get; set; } = 0.08f;

  /// <summary>Gets or sets the inter-centroid distance below which two clusters are merged.</summary>
  public float MergeThreshold { get; set; } = 0.03f;

  /// <summary>Gets or sets the maximum number of ISODATA outer iterations.</summary>
  public int MaxIterations { get; set; } = 15;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public IsodataQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.SplitThreshold, this.MergeThreshold, this.MaxIterations, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    float splitThreshold, float mergeThreshold, int maxIterations, int maxSampleSize, int seed) : IQuantizer<TWork>
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
      // Seed with k/2 centroids via K-Means++ then let split grow to k.
      var start = Math.Max(2, k / 2);
      var centroids = _KmppSeed(x, y, z, a, w, n, start, rng);

      for (var iter = 0; iter < maxIterations; ++iter) {
        // Assign.
        var numC = centroids.Count;
        var buckets = new List<int>[numC];
        for (var i = 0; i < numC; ++i) buckets[i] = new List<int>();
        for (var i = 0; i < n; ++i) {
          var best = 0;
          var bestD = double.MaxValue;
          for (var j = 0; j < numC; ++j) {
            var dx = x[i] - centroids[j].x; var dy = y[i] - centroids[j].y; var dz = z[i] - centroids[j].z; var da = a[i] - centroids[j].a;
            var d = dx * dx + dy * dy + dz * dz + da * da;
            if (d < bestD) { bestD = d; best = j; }
          }
          buckets[best].Add(i);
        }

        // Update centroids + compute per-cluster stddev.
        var newC = new List<(double x, double y, double z, double a, double sd)>();
        for (var c = 0; c < numC; ++c) {
          var m = buckets[c];
          if (m.Count == 0) continue;
          double s1 = 0, s2 = 0, s3 = 0, sa = 0, sw = 0;
          foreach (var i in m) { s1 += x[i] * w[i]; s2 += y[i] * w[i]; s3 += z[i] * w[i]; sa += a[i] * w[i]; sw += w[i]; }
          if (sw <= 0) continue;
          var cx = s1 / sw; var cy = s2 / sw; var cz = s3 / sw; var ca2 = sa / sw;
          double varMax = 0;
          double v1 = 0, v2 = 0, v3 = 0;
          foreach (var i in m) {
            var dx = x[i] - cx; var dy = y[i] - cy; var dz = z[i] - cz;
            v1 += dx * dx * w[i]; v2 += dy * dy * w[i]; v3 += dz * dz * w[i];
          }
          v1 /= sw; v2 /= sw; v3 /= sw;
          varMax = Math.Max(v1, Math.Max(v2, v3));
          newC.Add((cx, cy, cz, ca2, Math.Sqrt(varMax)));
        }
        centroids = newC.Select(p => (p.x, p.y, p.z, p.a)).ToList();
        // Merge: if any two centroids are within mergeThreshold, fuse.
        var merge2 = (double)mergeThreshold * mergeThreshold;
        var merged = true;
        while (merged && centroids.Count > 1) {
          merged = false;
          for (var i = 0; i < centroids.Count && !merged; ++i)
            for (var j = i + 1; j < centroids.Count; ++j) {
              var dx = centroids[i].x - centroids[j].x;
              var dy = centroids[i].y - centroids[j].y;
              var dz = centroids[i].z - centroids[j].z;
              if (dx * dx + dy * dy + dz * dz > merge2) continue;
              centroids[i] = ((centroids[i].x + centroids[j].x) / 2,
                              (centroids[i].y + centroids[j].y) / 2,
                              (centroids[i].z + centroids[j].z) / 2,
                              (centroids[i].a + centroids[j].a) / 2);
              centroids.RemoveAt(j);
              merged = true;
              break;
            }
        }
        // Split: if any cluster's stddev exceeds threshold AND we have room, split by perturbation.
        for (var c = 0; c < newC.Count && centroids.Count < k; ++c) {
          if (newC[c].sd < splitThreshold) continue;
          var p = centroids[c];
          var j = perturbIndex(rng);
          var delta = newC[c].sd * 0.5;
          var p2 = (p.x, p.y, p.z, p.a);
          switch (j) {
            case 0: p = (p.x - delta, p.y, p.z, p.a); p2 = (p2.x + delta, p2.y, p2.z, p2.a); break;
            case 1: p = (p.x, p.y - delta, p.z, p.a); p2 = (p2.x, p2.y + delta, p2.z, p2.a); break;
            default: p = (p.x, p.y, p.z - delta, p.a); p2 = (p2.x, p2.y, p2.z + delta, p2.a); break;
          }
          centroids[c] = p;
          centroids.Add(p2);
        }
      }

      return centroids.Select(c => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.x))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.y))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.z))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.a)))));

      int perturbIndex(Random r) => r.Next(3);
    }

    private static List<(double x, double y, double z, double a)> _KmppSeed(
      double[] x, double[] y, double[] z, double[] a, double[] w, int n, int k, Random rng) {
      var chosen = new List<(double x, double y, double z, double a)>();
      var tot = 0.0;
      for (var i = 0; i < n; ++i) tot += w[i];
      var t = rng.NextDouble() * tot;
      var c = 0.0; var idx = 0;
      for (var i = 0; i < n; ++i) { c += w[i]; if (c < t) continue; idx = i; break; }
      chosen.Add((x[idx], y[idx], z[idx], a[idx]));
      var d2 = new double[n];
      for (var i = 0; i < n; ++i) {
        var dx = x[i] - x[idx]; var dy = y[i] - y[idx]; var dz = z[i] - z[idx]; var da = a[i] - a[idx];
        d2[i] = dx * dx + dy * dy + dz * dz + da * da;
      }
      for (var j = 1; j < k; ++j) {
        tot = 0;
        for (var i = 0; i < n; ++i) tot += d2[i] * w[i];
        t = rng.NextDouble() * tot;
        c = 0; idx = 0;
        for (var i = 0; i < n; ++i) { c += d2[i] * w[i]; if (c < t) continue; idx = i; break; }
        chosen.Add((x[idx], y[idx], z[idx], a[idx]));
        for (var i = 0; i < n; ++i) {
          var dx = x[i] - x[idx]; var dy = y[i] - y[idx]; var dz = z[i] - z[idx]; var da = a[i] - a[idx];
          var dd = dx * dx + dy * dy + dz * dz + da * da;
          if (dd < d2[i]) d2[i] = dd;
        }
      }
      return chosen;
    }
  }
}
