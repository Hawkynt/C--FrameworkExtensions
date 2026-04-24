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
/// HDBSCAN (Hierarchical Density-Based Clustering) colour quantizer — Campello, Moulavi &amp; Sander (2013).
/// </summary>
/// <remarks>
/// <para>
/// Builds the full single-linkage hierarchy over the mutual-reachability graph and extracts clusters by
/// maximising <i>cluster stability</i> across persistence levels — no fixed <c>ε</c> or level is required.
/// Each cluster's "lifetime" in the hierarchy (the range of density levels over which it exists as a
/// cohesive unit) is the stability score; HDBSCAN picks the set of non-overlapping clusters that
/// maximises total stability.
/// </para>
/// <para>
/// <b>Distinct from OPTICS:</b> OPTICS produces a linear reachability plot and extracts clusters via
/// <c>ξ</c>-steepness heuristics; HDBSCAN produces a condensed tree and extracts clusters via a global
/// stability-maximisation optimisation over the entire hierarchy. In practice, HDBSCAN tends to produce
/// more semantically-coherent clusters on noisy data.
/// </para>
/// <para>
/// <b>Distinct from DBSCAN:</b> HDBSCAN replaces the two-knob (ε, minPts) heuristic with a single knob
/// (minPts), then automatically selects the best density threshold per cluster. This is the modern
/// "successor" algorithm to DBSCAN.
/// </para>
/// <para>Reference: Campello, Moulavi, Sander (2013) — "Density-Based Clustering Based on Hierarchical
/// Density Estimates", PAKDD 2013.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "HDBSCAN", Author = "Campello et al.", Year = 2013, QualityRating = 8)]
public struct HdbscanQuantizer : IQuantizer {

  /// <summary>Gets or sets the minimum-samples density parameter (analogous to DBSCAN's MinPoints).</summary>
  public int MinClusterSize { get; set; } = 5;

  /// <summary>Gets or sets the maximum sample size (O(n²) mutual-reachability).</summary>
  public int MaxSampleSize { get; set; } = 768;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public HdbscanQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MinClusterSize, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int minClusterSize, int maxSampleSize, int seed) : IQuantizer<TWork>
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

      var x = new double[n]; var y = new double[n]; var z = new double[n]; var a = new double[n];
      var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, ca) = colors[i].color.ToNormalized();
        x[i] = c1.ToFloat(); y[i] = c2.ToFloat(); z[i] = c3.ToFloat(); a[i] = ca.ToFloat();
        w[i] = Math.Max(1, colors[i].count);
      }

      // Step 1: core distances (kNN where k = minClusterSize).
      var coreDist = new double[n];
      var tmp = new double[n];
      var kSel = Math.Min(minClusterSize, n) - 1;
      for (var i = 0; i < n; ++i) {
        for (var j = 0; j < n; ++j) tmp[j] = _Dist(x, y, z, a, i, j);
        Array.Sort(tmp, 0, n);
        coreDist[i] = tmp[Math.Max(0, kSel)];
      }

      // Step 2: mutual-reachability distance = max(core(a), core(b), dist(a,b)).
      // Step 3: MST over mutual-reachability graph (Prim's, O(n²)).
      var inTree = new bool[n];
      var minEdge = new double[n];
      var parent = new int[n];
      for (var i = 0; i < n; ++i) { minEdge[i] = double.PositiveInfinity; parent[i] = -1; }
      minEdge[0] = 0;
      var mst = new List<(int u, int v, double d)>(n - 1);
      for (var step = 0; step < n; ++step) {
        var bestI = -1;
        var bestV = double.PositiveInfinity;
        for (var i = 0; i < n; ++i) {
          if (inTree[i]) continue;
          if (minEdge[i] < bestV) { bestV = minEdge[i]; bestI = i; }
        }
        if (bestI < 0) break;
        inTree[bestI] = true;
        if (parent[bestI] >= 0) mst.Add((parent[bestI], bestI, minEdge[bestI]));
        for (var i = 0; i < n; ++i) {
          if (inTree[i]) continue;
          var d = Math.Max(coreDist[bestI], Math.Max(coreDist[i], _Dist(x, y, z, a, bestI, i)));
          if (d < minEdge[i]) { minEdge[i] = d; parent[i] = bestI; }
        }
      }

      // Step 4: single-linkage dendrogram — build by sorting MST edges ascending.
      mst.Sort((p, q) => p.d.CompareTo(q.d));
      // Union-find with size tracking.
      var uf = new int[n * 2];
      var size = new double[n * 2];
      var birth = new double[n * 2];
      for (var i = 0; i < n; ++i) { uf[i] = i; size[i] = w[i]; birth[i] = 0; }
      var nextId = n;
      var children = new List<(int a, int b, double d, int parent)>(n - 1);
      int Find(int u) { while (uf[u] != u) u = uf[u] = uf[uf[u]]; return u; }
      foreach (var (u, v, d) in mst) {
        var ru = Find(u); var rv = Find(v);
        if (ru == rv) continue;
        uf[ru] = nextId; uf[rv] = nextId; uf[nextId] = nextId;
        size[nextId] = size[ru] + size[rv];
        birth[nextId] = d;
        children.Add((ru, rv, d, nextId));
        ++nextId;
      }

      // Step 5: stability-based cluster extraction on condensed tree.
      // Stability(c) = Σ_{leaf point p in c} (λ_p - λ_c), where λ = 1/d; we approximate by using
      // each point's departure density (child's birth d) vs. the parent's birth d.
      var nodeStability = new double[nextId];
      var totalMass = 0.0;
      foreach (var row in children) {
        var parentLambda = row.d > 0 ? 1.0 / row.d : double.MaxValue;
        // Each side contributes: child's mass × (child_lambda - parent_lambda).
        double ChildLambda(int ch) {
          if (ch < n) return double.MaxValue; // a leaf — λ = +∞
          var cb = birth[ch]; return cb > 0 ? 1.0 / cb : double.MaxValue;
        }
        var la = ChildLambda(row.a); var lb = ChildLambda(row.b);
        var dlambdaA = Math.Max(0, la - parentLambda); var dlambdaB = Math.Max(0, lb - parentLambda);
        nodeStability[row.parent] += size[row.a] * dlambdaA + size[row.b] * dlambdaB;
        totalMass += nodeStability[row.parent];
      }

      // Select top-k internal nodes by stability as clusters.
      var ranked = new List<(int id, double stability)>();
      for (var i = n; i < nextId; ++i) {
        if (size[i] < minClusterSize) continue;
        ranked.Add((i, nodeStability[i]));
      }
      ranked.Sort((p, q) => q.stability.CompareTo(p.stability));

      // Collect cluster leaves (points) per selected cluster.
      var selected = new List<int>();
      var used = new bool[nextId];
      foreach (var r in ranked) {
        if (used[r.id]) continue;
        selected.Add(r.id);
        // Mark all descendants as used.
        var stack = new Stack<int>();
        stack.Push(r.id);
        while (stack.Count > 0) {
          var u = stack.Pop();
          if (used[u]) continue;
          used[u] = true;
          // Find this node among children records.
          foreach (var row in children) {
            if (row.parent != u) continue;
            stack.Push(row.a); stack.Push(row.b);
          }
        }
        if (selected.Count >= k) break;
      }

      // Compute each selected cluster's weighted centroid over its leaf points.
      var palette = new List<(TWork color, double weight)>();
      foreach (var clusterId in selected) {
        double s1 = 0, s2 = 0, s3 = 0, sa = 0, sw = 0;
        var stack = new Stack<int>();
        stack.Push(clusterId);
        while (stack.Count > 0) {
          var u = stack.Pop();
          if (u < n) {
            s1 += x[u] * w[u]; s2 += y[u] * w[u]; s3 += z[u] * w[u]; sa += a[u] * w[u]; sw += w[u];
            continue;
          }
          foreach (var row in children) {
            if (row.parent != u) continue;
            stack.Push(row.a); stack.Push(row.b);
          }
        }
        if (sw <= 0) continue;
        palette.Add((ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s1 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s2 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s3 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sa / sw)))
        ), sw));
      }

      if (palette.Count >= k)
        return palette.OrderByDescending(p => p.weight).Take(k).Select(p => p.color);

      // Pad via Wu on whole input.
      var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
      var padded = palette.OrderByDescending(p => p.weight).Select(p => p.color).ToList();
      padded.AddRange(fallback.GeneratePalette(colors, k - padded.Count));
      return padded.Take(k);
    }

    private static double _Dist(double[] x, double[] y, double[] z, double[] a, int i, int j) {
      var dx = x[i] - x[j];
      var dy = y[i] - y[j];
      var dz = z[i] - z[j];
      var da = a[i] - a[j];
      return Math.Sqrt(dx * dx + dy * dy + dz * dz + da * da);
    }
  }
}
