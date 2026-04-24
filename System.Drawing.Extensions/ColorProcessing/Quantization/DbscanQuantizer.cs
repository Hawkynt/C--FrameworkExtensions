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
/// DBSCAN (Density-Based Spatial Clustering of Applications with Noise) color quantizer.
/// </summary>
/// <remarks>
/// <para>
/// Groups colors by density in the active <typeparamref name="TWork"/> normalized space; when the
/// pipeline operates in <c>OklabaF</c> this clusters perceptually-coherent regions of OkLab.
/// Unlike K-Means or K-Medoids DBSCAN discovers the cluster count from the data and naturally handles
/// non-convex distributions, at the cost of two tuning knobs: <see cref="Epsilon"/> (neighbourhood
/// radius) and <see cref="MinPoints"/> (density threshold).
/// </para>
/// <para>
/// Cluster centroids (weighted by histogram counts) are emitted as palette entries. If the density
/// structure yields fewer clusters than the requested palette size, the palette is padded via a
/// fallback quantizer applied to unassigned / noise points; if it yields more, only the largest
/// clusters (by cumulative histogram weight) are kept.
/// </para>
/// <para>Reference: Ester, Kriegel, Sander, Xu (1996) — "A density-based algorithm for discovering clusters in large spatial databases with noise", KDD'96.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "DBSCAN", Author = "Ester et al.", Year = 1996, QualityRating = 7)]
public struct DbscanQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the neighbourhood radius (Euclidean distance in normalized TWork space).
  /// </summary>
  /// <remarks>Reasonable defaults for OkLab work around 0.03 (L/a/b are bounded roughly to [0,1]).</remarks>
  public float Epsilon { get; set; } = 0.03f;

  /// <summary>
  /// Gets or sets the minimum number of neighbouring points required to form a dense region.
  /// </summary>
  public int MinPoints { get; set; } = 4;

  /// <summary>
  /// Gets or sets the maximum sample size for processing (DBSCAN is O(n²) without an index).
  /// </summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>
  /// Gets or sets the deterministic random seed used for sampling and tie-breaking.
  /// </summary>
  public int Seed { get; set; } = 42;

  public DbscanQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Epsilon,
    this.MinPoints,
    this.MaxSampleSize,
    this.Seed);

  internal sealed class Kernel<TWork>(
    float epsilon,
    int minPoints,
    int maxSampleSize,
    int seed) : IQuantizer<TWork>
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

      // Project to normalized float arrays once.
      var n = colors.Length;
      var c1 = new double[n];
      var c2 = new double[n];
      var c3 = new double[n];
      var ca = new double[n];
      var weight = new uint[n];

      for (var i = 0; i < n; ++i) {
        var (n1, n2, n3, na) = colors[i].color.ToNormalized();
        c1[i] = n1.ToFloat();
        c2[i] = n2.ToFloat();
        c3[i] = n3.ToFloat();
        ca[i] = na.ToFloat();
        weight[i] = colors[i].count;
      }

      var eps2 = (double)epsilon * epsilon;
      var labels = new int[n]; // 0 = unvisited, -1 = noise, >=1 = cluster id
      var clusterId = 0;

      var neighborsBuffer = new List<int>(64);
      var seedQueue = new Queue<int>();

      for (var i = 0; i < n; ++i) {
        if (labels[i] != 0)
          continue;

        // Compute eps-neighborhood of point i.
        _RegionQuery(i, c1, c2, c3, ca, eps2, neighborsBuffer);
        if (neighborsBuffer.Count < minPoints) {
          labels[i] = -1; // tentative noise — may be reclassified as border later
          continue;
        }

        ++clusterId;
        labels[i] = clusterId;

        // Seed queue with all neighbours except i itself.
        seedQueue.Clear();
        for (var j = 0; j < neighborsBuffer.Count; ++j)
          if (neighborsBuffer[j] != i)
            seedQueue.Enqueue(neighborsBuffer[j]);

        while (seedQueue.Count > 0) {
          var q = seedQueue.Dequeue();
          if (labels[q] == -1)
            labels[q] = clusterId; // was noise, upgrade to border of this cluster

          if (labels[q] != 0)
            continue;

          labels[q] = clusterId;

          _RegionQuery(q, c1, c2, c3, ca, eps2, neighborsBuffer);
          if (neighborsBuffer.Count < minPoints)
            continue;

          // q is also a core point — expand the frontier.
          for (var j = 0; j < neighborsBuffer.Count; ++j) {
            var nb = neighborsBuffer[j];
            if (labels[nb] == 0 || labels[nb] == -1)
              seedQueue.Enqueue(nb);
          }
        }
      }

      // Collect cluster centroids weighted by histogram counts, along with cluster weight.
      var sums = new (double c1, double c2, double c3, double a, double w)[clusterId];
      for (var i = 0; i < n; ++i) {
        var id = labels[i];
        if (id <= 0)
          continue;

        var w = weight[i];
        sums[id - 1].c1 += c1[i] * w;
        sums[id - 1].c2 += c2[i] * w;
        sums[id - 1].c3 += c3[i] * w;
        sums[id - 1].a += ca[i] * w;
        sums[id - 1].w += w;
      }

      var centroids = new List<(TWork color, double weight)>(clusterId);
      for (var cl = 0; cl < clusterId; ++cl) {
        if (sums[cl].w <= 0)
          continue;

        centroids.Add((ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sums[cl].c1 / sums[cl].w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sums[cl].c2 / sums[cl].w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sums[cl].c3 / sums[cl].w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sums[cl].a / sums[cl].w)))
        ), sums[cl].w));
      }

      // Too many clusters? Keep the k most-populated ones.
      if (centroids.Count >= k)
        return centroids
          .OrderByDescending(c => c.weight)
          .Take(k)
          .Select(c => c.color);

      // Too few clusters? Pad using a secondary quantizer on the noise / residual points.
      var noiseColors = new List<(TWork color, uint count)>();
      for (var i = 0; i < n; ++i)
        if (labels[i] <= 0)
          noiseColors.Add((colors[i].color, weight[i]));

      var remaining = k - centroids.Count;
      var padded = centroids.Select(c => c.color).ToList();

      if (noiseColors.Count > 0 && remaining > 0) {
        // Wu is a fast, high-quality splitting-based quantizer — a sensible pad choice.
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        var extra = fallback.GeneratePalette(noiseColors, remaining);

        // Guard: quantizer may return fewer than requested if noise has < remaining unique colors.
        padded.AddRange(extra);
      }

      // If still short (very degenerate case) pad with the most-frequent input colors not yet chosen.
      if (padded.Count < k) {
        foreach (var (color, _) in colors.OrderByDescending(c => c.count)) {
          if (padded.Count >= k)
            break;

          var normalized = color.ToNormalized();
          var duplicate = false;
          foreach (var existing in padded) {
            if (!existing.ToNormalized().Equals(normalized))
              continue;

            duplicate = true;
            break;
          }

          if (duplicate)
            continue;

          padded.Add(color);
        }
      }

      return padded;
    }

    private static void _RegionQuery(
      int idx,
      double[] c1,
      double[] c2,
      double[] c3,
      double[] ca,
      double eps2,
      List<int> output) {
      output.Clear();
      var x1 = c1[idx];
      var x2 = c2[idx];
      var x3 = c3[idx];
      var xa = ca[idx];

      for (var i = 0; i < c1.Length; ++i) {
        var d1 = c1[i] - x1;
        var d2 = c2[i] - x2;
        var d3 = c3[i] - x3;
        var d4 = ca[i] - xa;
        var dist2 = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
        if (dist2 <= eps2)
          output.Add(i);
      }
    }

  }
}
