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
/// Adaptive 2-D chromaticity mesh quantizer — relaxation-based 2-D mesh equalising input
/// histogram density.
/// </summary>
/// <remarks>
/// <para>
/// Fits a deformable <c>m × m</c> lattice of palette anchors to the chromaticity plane (the
/// perceptual a/b axes of OkLab or the c2/c3 axes of the working space), relaxed so that each
/// lattice vertex migrates toward the histogram-density-weighted centroid of its Voronoi cell
/// while preserving neighbour-smoothness via a Laplacian regulariser.
/// </para>
/// <para>
/// Similar in spirit to adaptive-grid mesh refinement in PDE solvers: the mesh is denser where
/// the histogram is dense, coarser where it is sparse. The final lattice vertices become the
/// palette — each is the weighted centroid of nearby input colours with luminance carried
/// separately (averaged across the contributing samples).
/// </para>
/// <para>
/// <b>Distinct from 1-D greedy mesh:</b> a 1-D variant (widely cited but seldom preferred) only
/// relaxes on luminance, losing chroma structure. The 2-D chromaticity-plane variant implemented
/// here preserves the full colour distribution and is the practical member of the family.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Adaptive Mesh 2D", QualityRating = 7)]
public struct AdaptiveMeshQuantizer : IQuantizer {

  /// <summary>Gets or sets the number of relaxation iterations.</summary>
  public int Iterations { get; set; } = 12;

  /// <summary>Gets or sets the Laplacian-regularisation weight (0 = pure Voronoi, 1 = pure mesh).</summary>
  public float LaplacianWeight { get; set; } = 0.2f;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public AdaptiveMeshQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Iterations, this.LaplacianWeight, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int iterations, float laplacianWeight, int maxSampleSize, int seed) : IQuantizer<TWork>
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

      // Choose mesh grid size m×m so that m² ≥ k. For non-square k we trim to exactly k at the end.
      var m = (int)Math.Ceiling(Math.Sqrt(k));
      var total = m * m;
      // Initialise mesh on a uniform grid over the chroma-plane bounding box (axes c2/c3).
      double c2Min = double.MaxValue, c2Max = double.MinValue;
      double c3Min = double.MaxValue, c3Max = double.MinValue;
      for (var i = 0; i < n; ++i) {
        if (y[i] < c2Min) c2Min = y[i]; if (y[i] > c2Max) c2Max = y[i];
        if (z[i] < c3Min) c3Min = z[i]; if (z[i] > c3Max) c3Max = z[i];
      }
      if (c2Max - c2Min < 1e-6) c2Max = c2Min + 1e-6;
      if (c3Max - c3Min < 1e-6) c3Max = c3Min + 1e-6;

      var nodeC2 = new double[total]; var nodeC3 = new double[total];
      for (var i = 0; i < m; ++i) for (var j = 0; j < m; ++j) {
        var gi = i * m + j;
        nodeC2[gi] = c2Min + (c2Max - c2Min) * (m > 1 ? (double)i / (m - 1) : 0.5);
        nodeC3[gi] = c3Min + (c3Max - c3Min) * (m > 1 ? (double)j / (m - 1) : 0.5);
      }

      var assign = new int[n];
      var lw = (double)laplacianWeight;

      for (var iter = 0; iter < iterations; ++iter) {
        // Assign each sample to nearest mesh node in (c2, c3) plane.
        for (var p = 0; p < n; ++p) {
          var best = 0;
          var bestD = double.MaxValue;
          for (var g = 0; g < total; ++g) {
            var d1 = y[p] - nodeC2[g]; var d2 = z[p] - nodeC3[g];
            var d = d1 * d1 + d2 * d2;
            if (d < bestD) { bestD = d; best = g; }
          }
          assign[p] = best;
        }
        // Voronoi centroids (weighted).
        var vc2 = new double[total]; var vc3 = new double[total]; var vW = new double[total];
        for (var p = 0; p < n; ++p) {
          var g = assign[p];
          vc2[g] += y[p] * w[p]; vc3[g] += z[p] * w[p]; vW[g] += w[p];
        }
        // Apply combined update: new_node = (1-lw)·voronoi + lw·laplacian_of_neighbours.
        var newC2 = new double[total]; var newC3 = new double[total];
        for (var i = 0; i < m; ++i) for (var j = 0; j < m; ++j) {
          var g = i * m + j;
          double vx = vW[g] > 0 ? vc2[g] / vW[g] : nodeC2[g];
          double vy = vW[g] > 0 ? vc3[g] / vW[g] : nodeC3[g];
          double lx = 0, ly = 0; var count = 0;
          if (i > 0) { lx += nodeC2[(i - 1) * m + j]; ly += nodeC3[(i - 1) * m + j]; ++count; }
          if (i < m - 1) { lx += nodeC2[(i + 1) * m + j]; ly += nodeC3[(i + 1) * m + j]; ++count; }
          if (j > 0) { lx += nodeC2[i * m + j - 1]; ly += nodeC3[i * m + j - 1]; ++count; }
          if (j < m - 1) { lx += nodeC2[i * m + j + 1]; ly += nodeC3[i * m + j + 1]; ++count; }
          if (count > 0) { lx /= count; ly /= count; } else { lx = vx; ly = vy; }
          newC2[g] = (1 - lw) * vx + lw * lx;
          newC3[g] = (1 - lw) * vy + lw * ly;
        }
        Array.Copy(newC2, nodeC2, total);
        Array.Copy(newC3, nodeC3, total);
      }

      // Final palette: for each non-empty mesh cell emit the (c1, c2, c3, a) weighted centroid
      // of the assigned samples.
      var clusters = new (double c1, double c2, double c3, double a, double w)[total];
      for (var p = 0; p < n; ++p) {
        var g = assign[p];
        clusters[g].c1 += x[p] * w[p];
        clusters[g].c2 += y[p] * w[p];
        clusters[g].c3 += z[p] * w[p];
        clusters[g].a += a[p] * w[p];
        clusters[g].w += w[p];
      }
      var palette = new List<(TWork c, double w)>();
      for (var g = 0; g < total; ++g) {
        if (clusters[g].w <= 0) continue;
        palette.Add((ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, clusters[g].c1 / clusters[g].w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, clusters[g].c2 / clusters[g].w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, clusters[g].c3 / clusters[g].w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, clusters[g].a / clusters[g].w)))
        ), clusters[g].w));
      }
      var top = palette.OrderByDescending(p => p.w).Take(k).Select(p => p.c).ToList();
      if (top.Count < k) {
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        top.AddRange(fallback.GeneratePalette(colors, k - top.Count));
      }
      return top.Take(k);
    }
  }
}
