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
/// Spectral Clustering colour quantizer — graph-Laplacian-eigenmap clustering (Shi &amp; Malik, 2000;
/// Ng, Jordan &amp; Weiss, 2002).
/// </summary>
/// <remarks>
/// <para>
/// Builds a similarity graph over histogram colours using an RBF kernel, constructs the symmetric
/// normalised graph Laplacian <c>L_sym = I - D^(-1/2) W D^(-1/2)</c>, computes its <c>k</c>
/// smallest non-trivial eigenvectors via power iteration with deflation, and runs K-Means in the
/// resulting spectral embedding. Final cluster assignments are mapped back to histogram colours
/// and weighted centroids are emitted as palette entries.
/// </para>
/// <para>
/// <b>Distinct from K-Means / K-Medoids / DBSCAN:</b> spectral clustering partitions on the
/// <i>connectivity structure</i> of the colour graph, not on Euclidean distance. Works well on
/// non-convex clusters (ring-shaped colour distributions, double-arcs) that defeat centroid
/// methods.
/// </para>
/// <para>Reference: Shi &amp; Malik (2000) — "Normalized Cuts and Image Segmentation", IEEE TPAMI;
/// Ng, Jordan &amp; Weiss (2002) — "On Spectral Clustering: Analysis and an algorithm", NIPS 2002.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Spectral", Author = "Shi, Malik; Ng, Jordan, Weiss", Year = 2000, QualityRating = 7)]
public struct SpectralClusteringQuantizer : IQuantizer {

  /// <summary>Gets or sets the RBF kernel bandwidth σ in normalized colour space.</summary>
  public float Sigma { get; set; } = 0.15f;

  /// <summary>Gets or sets the maximum K-Means iterations in the spectral embedding.</summary>
  public int MaxIterations { get; set; } = 30;

  /// <summary>Gets or sets the maximum sample size (O(n³) eigen-decomposition).</summary>
  public int MaxSampleSize { get; set; } = 256;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public SpectralClusteringQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Sigma, this.MaxIterations, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    float sigma, int maxIterations, int maxSampleSize, int seed) : IQuantizer<TWork>
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

      // Build similarity matrix W (dense). For n≤256 this is 256²×8 = 512 KB — acceptable.
      var invTwoSigma2 = 1.0 / (2 * sigma * sigma);
      var W = new double[n, n];
      var deg = new double[n];
      for (var i = 0; i < n; ++i) {
        for (var j = i + 1; j < n; ++j) {
          var dx = x[i] - x[j]; var dy = y[i] - y[j]; var dz = z[i] - z[j]; var da = a[i] - a[j];
          var d2 = dx * dx + dy * dy + dz * dz + da * da;
          var sim = Math.Exp(-d2 * invTwoSigma2);
          W[i, j] = sim; W[j, i] = sim;
        }
      }
      for (var i = 0; i < n; ++i) {
        double s = 0;
        for (var j = 0; j < n; ++j) s += W[i, j];
        deg[i] = Math.Max(1e-12, s);
      }
      var invSqrtDeg = new double[n];
      for (var i = 0; i < n; ++i) invSqrtDeg[i] = 1.0 / Math.Sqrt(deg[i]);

      // L_sym = I - D^(-1/2) W D^(-1/2). Shift by I (spectral eigenvectors of L_sym = eigvecs
      // of (I - L_sym) = D^(-1/2) W D^(-1/2) with mapped eigenvalues).
      var M = new double[n, n];
      for (var i = 0; i < n; ++i)
        for (var j = 0; j < n; ++j)
          M[i, j] = invSqrtDeg[i] * W[i, j] * invSqrtDeg[j];

      // Power iteration with deflation to extract top-k eigenvectors.
      var embedding = new double[n, k];
      var v = new double[n];
      var rng = new Random(seed);
      for (var c = 0; c < k; ++c) {
        for (var i = 0; i < n; ++i) v[i] = rng.NextDouble() - 0.5;
        for (var iter = 0; iter < 50; ++iter) {
          var next = new double[n];
          for (var i = 0; i < n; ++i)
            for (var j = 0; j < n; ++j)
              next[i] += M[i, j] * v[j];
          // Orthogonalise against previously-extracted eigenvectors.
          for (var p = 0; p < c; ++p) {
            double dot = 0;
            for (var i = 0; i < n; ++i) dot += next[i] * embedding[i, p];
            for (var i = 0; i < n; ++i) next[i] -= dot * embedding[i, p];
          }
          var norm = 0.0;
          for (var i = 0; i < n; ++i) norm += next[i] * next[i];
          norm = Math.Sqrt(Math.Max(1e-18, norm));
          for (var i = 0; i < n; ++i) v[i] = next[i] / norm;
        }
        for (var i = 0; i < n; ++i) embedding[i, c] = v[i];
      }

      // Row-normalise embedding (Ng-Jordan-Weiss step).
      for (var i = 0; i < n; ++i) {
        double norm = 0;
        for (var c = 0; c < k; ++c) norm += embedding[i, c] * embedding[i, c];
        norm = Math.Sqrt(Math.Max(1e-18, norm));
        for (var c = 0; c < k; ++c) embedding[i, c] /= norm;
      }

      // K-Means in the spectral embedding.
      var assign = _KMeansOnEmbedding(embedding, n, k, maxIterations, rng);

      // Map back to original colours — weighted centroids per cluster.
      var sums = new (double c1, double c2, double c3, double a, double w)[k];
      for (var i = 0; i < n; ++i) {
        var c = assign[i];
        sums[c].c1 += x[i] * w[i];
        sums[c].c2 += y[i] * w[i];
        sums[c].c3 += z[i] * w[i];
        sums[c].a += a[i] * w[i];
        sums[c].w += w[i];
      }
      var palette = new List<TWork>();
      for (var c = 0; c < k; ++c) {
        if (sums[c].w <= 0) continue;
        palette.Add(ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sums[c].c1 / sums[c].w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sums[c].c2 / sums[c].w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sums[c].c3 / sums[c].w))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sums[c].a / sums[c].w)))));
      }
      if (palette.Count < k) {
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        palette.AddRange(fallback.GeneratePalette(colors, k - palette.Count));
      }
      return palette.Take(k);
    }

    private static int[] _KMeansOnEmbedding(double[,] emb, int n, int k, int maxIter, Random rng) {
      var assign = new int[n];
      // Random initialisation.
      var centers = new double[k, emb.GetLength(1)];
      var pickedIdx = new HashSet<int>();
      for (var c = 0; c < k; ++c) {
        int idx;
        do { idx = rng.Next(n); } while (pickedIdx.Contains(idx) && pickedIdx.Count < n);
        pickedIdx.Add(idx);
        for (var d = 0; d < emb.GetLength(1); ++d) centers[c, d] = emb[idx, d];
      }
      for (var iter = 0; iter < maxIter; ++iter) {
        var changed = false;
        // Assign.
        for (var i = 0; i < n; ++i) {
          var best = 0;
          var bestD = double.MaxValue;
          for (var c = 0; c < k; ++c) {
            double d = 0;
            for (var dd = 0; dd < emb.GetLength(1); ++dd) {
              var diff = emb[i, dd] - centers[c, dd];
              d += diff * diff;
            }
            if (d < bestD) { bestD = d; best = c; }
          }
          if (assign[i] != best) { assign[i] = best; changed = true; }
        }
        if (!changed) break;
        // Update.
        var counts = new int[k];
        var newCtr = new double[k, emb.GetLength(1)];
        for (var i = 0; i < n; ++i) {
          ++counts[assign[i]];
          for (var dd = 0; dd < emb.GetLength(1); ++dd) newCtr[assign[i], dd] += emb[i, dd];
        }
        for (var c = 0; c < k; ++c) {
          if (counts[c] == 0) continue;
          for (var dd = 0; dd < emb.GetLength(1); ++dd) centers[c, dd] = newCtr[c, dd] / counts[c];
        }
      }
      return assign;
    }
  }
}
