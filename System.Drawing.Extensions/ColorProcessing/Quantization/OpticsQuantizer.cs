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
/// OPTICS (Ordering Points To Identify Clustering Structure) density-based quantizer.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="DbscanQuantizer"/>, OPTICS does not require a single <c>ε</c> parameter.
/// Instead it computes a <i>reachability plot</i> — a 1-D ordered sequence of reachability
/// distances that captures the clustering structure at all density levels simultaneously. Cluster
/// boundaries are then auto-selected from the valleys of the reachability plot via the classical
/// <c>ξ</c>-extraction (steep-point detection).
/// </para>
/// <para>
/// <b>Why it's distinct from DBSCAN:</b>
/// </para>
/// <list type="bullet">
///   <item><description>OPTICS discovers hierarchical density structure — nested clusters inside
///     broader ones — which a single-ε DBSCAN run cannot.</description></item>
///   <item><description>OPTICS is robust to density variation — regions of wildly different density
///     can yield meaningful clusters simultaneously, while DBSCAN picks one scale.</description></item>
///   <item><description>The <c>ε</c>-like parameter (<see cref="MaxEpsilon"/>) is only an upper bound
///     for performance, not a detection threshold — the algorithm self-selects density levels.</description></item>
/// </list>
/// <para>Reference: Ankerst, Breunig, Kriegel, Sander (1999) — "OPTICS: Ordering Points To Identify the
/// Clustering Structure", SIGMOD'99.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "OPTICS", Author = "Ankerst et al.", Year = 1999, QualityRating = 7)]
public struct OpticsQuantizer : IQuantizer {

  /// <summary>Gets or sets the hard upper bound on neighbourhood radius (performance cap only).</summary>
  public float MaxEpsilon { get; set; } = 0.15f;

  /// <summary>Gets or sets the minimum-points density threshold (affects core-distance).</summary>
  public int MinPoints { get; set; } = 4;

  /// <summary>Gets or sets the reachability steepness threshold ξ for cluster extraction (0-1, typical 0.05).</summary>
  public float Xi { get; set; } = 0.05f;

  /// <summary>Gets or sets the maximum sample size (O(n²) reachability scan).</summary>
  public int MaxSampleSize { get; set; } = 1024;

  /// <summary>Gets or sets the deterministic random seed used for sampling and tie-breaking.</summary>
  public int Seed { get; set; } = 42;

  public OpticsQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MaxEpsilon, this.MinPoints, this.Xi, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    float maxEpsilon, int minPoints, float xi, int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];
      if (colors.Length <= k)
        return colors.Select(c => c.color);

      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);
      var n = colors.Length;

      var c1 = new double[n]; var c2 = new double[n]; var c3 = new double[n]; var c4 = new double[n];
      var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (n1, n2, n3, na) = colors[i].color.ToNormalized();
        c1[i] = n1.ToFloat(); c2[i] = n2.ToFloat(); c3[i] = n3.ToFloat(); c4[i] = na.ToFloat();
        w[i] = Math.Max(1, colors[i].count);
      }

      var eps2 = (double)maxEpsilon * maxEpsilon;
      var reach = new double[n];
      var coreDist = new double[n];
      var processed = new bool[n];
      var order = new List<int>(n);

      for (var i = 0; i < n; ++i) reach[i] = double.PositiveInfinity;
      for (var i = 0; i < n; ++i) coreDist[i] = double.PositiveInfinity;

      // Precompute core distances.
      var kthBuffer = new double[n];
      for (var i = 0; i < n; ++i) {
        var count = 0;
        for (var j = 0; j < n; ++j) {
          var d2 = _Dist2(c1, c2, c3, c4, i, j);
          if (d2 > eps2) continue;
          kthBuffer[count++] = d2;
        }
        if (count < minPoints) continue;
        Array.Sort(kthBuffer, 0, count);
        coreDist[i] = Math.Sqrt(kthBuffer[minPoints - 1]);
      }

      // OPTICS main loop.
      for (var seedPt = 0; seedPt < n; ++seedPt) {
        if (processed[seedPt]) continue;
        processed[seedPt] = true;
        order.Add(seedPt);
        if (double.IsPositiveInfinity(coreDist[seedPt])) continue;

        // Priority queue ordered by reachability (min-heap) — we use a list since n ≤ 1024.
        var seeds = new List<int>();
        _Update(seedPt, c1, c2, c3, c4, coreDist, reach, processed, eps2, seeds, n);
        while (seeds.Count > 0) {
          // Extract min-reach seed.
          var minIdx = 0;
          for (var s = 1; s < seeds.Count; ++s)
            if (reach[seeds[s]] < reach[seeds[minIdx]]) minIdx = s;
          var q = seeds[minIdx];
          seeds.RemoveAt(minIdx);
          if (processed[q]) continue;
          processed[q] = true;
          order.Add(q);
          if (!double.IsPositiveInfinity(coreDist[q]))
            _Update(q, c1, c2, c3, c4, coreDist, reach, processed, eps2, seeds, n);
        }
      }

      // Reachability-plot cluster extraction: any descent of >=(1-ξ) marks a valley entrance,
      // symmetrical ascent marks exit. We emit cluster centroids for the discovered valleys.
      var clusters = new List<(double c1, double c2, double c3, double a, double w)>();
      double Threshold(int i) => reach[order[i]];

      var valleyStart = -1;
      double startVal = double.PositiveInfinity;
      for (var i = 1; i < order.Count; ++i) {
        var prev = Threshold(i - 1);
        var cur = Threshold(i);
        if (double.IsPositiveInfinity(prev) || double.IsPositiveInfinity(cur)) {
          if (valleyStart >= 0) {
            _Emit(order, valleyStart, i - 1, c1, c2, c3, c4, w, clusters);
            valleyStart = -1;
          }
          continue;
        }
        if (cur < prev * (1 - xi)) {
          if (valleyStart < 0) {
            valleyStart = i - 1;
            startVal = prev;
          }
        } else if (cur > startVal * (1 - xi) && valleyStart >= 0) {
          _Emit(order, valleyStart, i - 1, c1, c2, c3, c4, w, clusters);
          valleyStart = -1;
        }
      }
      if (valleyStart >= 0)
        _Emit(order, valleyStart, order.Count - 1, c1, c2, c3, c4, w, clusters);

      // If no clusters extracted, fall back to the heaviest k histogram modes.
      if (clusters.Count == 0) {
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        return fallback.GeneratePalette(colors, k);
      }

      var palette = clusters
        .OrderByDescending(c => c.w)
        .Take(k)
        .Select(c => ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.c1 / c.w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.c2 / c.w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.c3 / c.w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.a / c.w)))
        )).ToList();

      if (palette.Count < k) {
        // Pad via Wu on whole input.
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        palette.AddRange(fallback.GeneratePalette(colors, k - palette.Count));
      }
      return palette.Take(k);
    }

    private static void _Emit(List<int> order, int lo, int hi,
      double[] c1, double[] c2, double[] c3, double[] c4, double[] w,
      List<(double c1, double c2, double c3, double a, double w)> into) {
      double s1 = 0, s2 = 0, s3 = 0, sa = 0, sw = 0;
      for (var i = lo; i <= hi; ++i) {
        var idx = order[i];
        s1 += c1[idx] * w[idx];
        s2 += c2[idx] * w[idx];
        s3 += c3[idx] * w[idx];
        sa += c4[idx] * w[idx];
        sw += w[idx];
      }
      if (sw <= 0) return;
      into.Add((s1, s2, s3, sa, sw));
    }

    private static double _Dist2(double[] x, double[] y, double[] z, double[] a, int i, int j) {
      var dx = x[i] - x[j];
      var dy = y[i] - y[j];
      var dz = z[i] - z[j];
      var da = a[i] - a[j];
      return dx * dx + dy * dy + dz * dz + da * da;
    }

    private static void _Update(int p,
      double[] c1, double[] c2, double[] c3, double[] c4,
      double[] coreDist, double[] reach, bool[] processed,
      double eps2, List<int> seeds, int n) {
      var cd = coreDist[p];
      for (var o = 0; o < n; ++o) {
        if (processed[o]) continue;
        var d2 = _Dist2(c1, c2, c3, c4, p, o);
        if (d2 > eps2) continue;
        var newReach = Math.Max(cd, Math.Sqrt(d2));
        if (newReach < reach[o]) {
          reach[o] = newReach;
          if (!seeds.Contains(o)) seeds.Add(o);
        }
      }
    }
  }
}
