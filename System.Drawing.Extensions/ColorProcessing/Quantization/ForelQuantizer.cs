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
/// Forel (FORmal ELement) clustering — Zagoruiko, Novosibirsk school (1968).
/// </summary>
/// <remarks>
/// <para>
/// Russian-school sphere-based clustering: at each step, a random unlabelled point is chosen;
/// the centroid of all points within a <see cref="Radius"/> sphere around it is computed; the
/// sphere is recentred on the centroid and the procedure repeated until the sphere stabilises.
/// All points inside the final sphere form one cluster. Repeat until all points are clustered.
/// </para>
/// <para>
/// <b>Distinct from K-Means:</b> Forel is agglomerative in the sense that each cluster is grown
/// from a seed point rather than partitioning the space around centroids globally.
/// </para>
/// <para>
/// <b>Distinct from Mean-Shift:</b> Forel uses a hard-cut sphere with re-centering; Mean-Shift
/// uses a smooth kernel density estimate. Forel clusters are disjoint from first construction;
/// Mean-Shift clusters can overlap (multiple starting points can converge to the same mode).
/// </para>
/// <para>Reference: Zagoruiko, N.G. (1968) — "Empirical Forecasting: Problems and Methods",
/// Nauka Publishers (Russian); widely documented in Soviet-era pattern-recognition literature.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "FOREL", Author = "Zagoruiko", Year = 1968, QualityRating = 6)]
public struct ForelQuantizer : IQuantizer {

  /// <summary>Gets or sets the sphere radius in normalized colour space.</summary>
  public float Radius { get; set; } = 0.08f;

  /// <summary>Gets or sets the maximum recentring iterations per cluster.</summary>
  public int MaxIterationsPerCluster { get; set; } = 20;

  /// <summary>Gets or sets the maximum sample size (O(n²) inner loop).</summary>
  public int MaxSampleSize { get; set; } = 1024;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public ForelQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Radius, this.MaxIterationsPerCluster, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    float radius, int maxIterationsPerCluster, int maxSampleSize, int seed) : IQuantizer<TWork>
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
      var rad2 = (double)radius * radius;
      var assigned = new bool[n];
      var clusters = new List<(double x, double y, double z, double a, double w)>();

      var remaining = n;
      while (remaining > 0) {
        // Pick a random unassigned seed (deterministic by seeded RNG).
        int seedIdx = -1;
        var startFrom = rng.Next(n);
        for (var step = 0; step < n; ++step) {
          var i = (startFrom + step) % n;
          if (!assigned[i]) { seedIdx = i; break; }
        }
        if (seedIdx < 0) break;

        double cx = x[seedIdx], cy = y[seedIdx], cz = z[seedIdx], ca = a[seedIdx];
        var members = new List<int>();

        for (var iter = 0; iter < maxIterationsPerCluster; ++iter) {
          members.Clear();
          for (var i = 0; i < n; ++i) {
            if (assigned[i]) continue;
            var dx = x[i] - cx; var dy = y[i] - cy; var dz = z[i] - cz; var da = a[i] - ca;
            var d2 = dx * dx + dy * dy + dz * dz + da * da;
            if (d2 <= rad2) members.Add(i);
          }
          if (members.Count == 0) { members.Add(seedIdx); break; }
          double s1 = 0, s2 = 0, s3 = 0, sa = 0, sw = 0;
          foreach (var i in members) {
            s1 += x[i] * w[i]; s2 += y[i] * w[i]; s3 += z[i] * w[i]; sa += a[i] * w[i]; sw += w[i];
          }
          if (sw <= 0) break;
          var nx = s1 / sw; var ny = s2 / sw; var nz = s3 / sw; var na = sa / sw;
          var move = (nx - cx) * (nx - cx) + (ny - cy) * (ny - cy) + (nz - cz) * (nz - cz);
          cx = nx; cy = ny; cz = nz; ca = na;
          if (move < 1e-8) break;
        }

        double cw = 0;
        foreach (var i in members) { cw += w[i]; assigned[i] = true; --remaining; }
        clusters.Add((cx, cy, cz, ca, cw));
      }

      // Top-k by weight.
      var top = clusters.OrderByDescending(c => c.w).Take(k)
        .Select(c => ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.x))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.y))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.z))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, c.a))))).ToList();
      if (top.Count < k) {
        // Pad with Wu.
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        top.AddRange(fallback.GeneratePalette(colors, k - top.Count));
      }
      return top.Take(k);
    }
  }
}
