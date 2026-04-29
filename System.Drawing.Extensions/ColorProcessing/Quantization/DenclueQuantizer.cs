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
/// DENCLUE color quantizer — DENsity-based CLUstering with triangular kernel and noise
/// rejection.
/// </summary>
/// <remarks>
/// <para>For each sample, hill-climb on a kernel-density estimate built from the histogram
/// until convergence; sample points that converge to the same local maximum form a cluster.
/// Unlike Mean-Shift (Gaussian) and the existing Epanechnikov Mean-Shift (quadratic), DENCLUE
/// here uses a <b>triangular influence function</b> with <b>noise-mode rejection</b>:
/// attractors whose local density falls below ξ (a fraction of the global maximum density)
/// are discarded as outlier clusters before the top-k selection. This filters salt-and-pepper
/// colour outliers that the existing density methods would otherwise treat as legitimate
/// modes.</para>
/// <para>Triangular weight: <c>w(d) = max(0, 1 − d/σ)</c>. Steeper decay than Gaussian, less
/// outlier-tolerant than Epanechnikov; produces sharper, more discrete attractors typical of
/// the DENCLUE family.</para>
/// <para>Reference: Hinneburg &amp; Keim 1998, "An Efficient Approach to Clustering in Large
/// Multimedia Databases with Noise", KDD'98. Refinement: Hinneburg &amp; Gabriel 2007, "DENCLUE
/// 2.0: Fast Clustering Based on Kernel Density Estimation", IDA.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "DENCLUE", Author = "Hinneburg & Keim", Year = 1998, QualityRating = 7)]
public struct DenclueQuantizer : IQuantizer {

  /// <summary>Influence-function bandwidth σ in normalized TWork space.</summary>
  /// <remarks>OkLab-friendly default. Smaller values produce more attractors.</remarks>
  public float Bandwidth { get; set; } = 0.07f;

  /// <summary>
  /// Noise-rejection threshold ξ — attractors with density below ξ × max-density are dropped.
  /// </summary>
  /// <remarks>Range [0, 1). 0 keeps every attractor (= triangular Mean-Shift); typical
  /// DENCLUE use is 0.05–0.20 to filter outlier modes.</remarks>
  public float NoiseFraction { get; set; } = 0.1f;

  /// <summary>Maximum hill-climb iterations per sample.</summary>
  public int MaxIterations { get; set; } = 25;

  /// <summary>Squared convergence threshold for the hill-climb.</summary>
  public float ConvergenceThreshold { get; set; } = 1e-5f;

  /// <summary>Squared distance below which two attractors are fused into one mode.</summary>
  public float MergeThreshold { get; set; } = 1e-3f;

  /// <summary>Maximum sample size — algorithm is O(n²·iterations) in pairwise weights.</summary>
  public int MaxSampleSize { get; set; } = 1024;

  /// <summary>Deterministic seed for the histogram subsampling.</summary>
  public int Seed { get; set; } = 42;

  public DenclueQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Bandwidth,
    this.NoiseFraction,
    this.MaxIterations,
    this.ConvergenceThreshold,
    this.MergeThreshold,
    this.MaxSampleSize,
    this.Seed);

  internal sealed class Kernel<TWork>(
    float bandwidth,
    float noiseFraction,
    int maxIterations,
    float convergenceThreshold,
    float mergeThreshold,
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

      var n = colors.Length;
      var x1 = new double[n];
      var x2 = new double[n];
      var x3 = new double[n];
      var xa = new double[n];
      var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        x1[i] = c1.ToFloat();
        x2[i] = c2.ToFloat();
        x3[i] = c3.ToFloat();
        xa[i] = a.ToFloat();
        w[i] = Math.Max(1, colors[i].count);
      }

      var sigma = (double)bandwidth;
      var convergeEps2 = (double)convergenceThreshold;

      // Hill-climb every sample to its DENCLUE attractor using triangular kernel:
      //   w_j(y) = max(0, 1 − ‖y − x_j‖ / σ)
      //   y_new  = Σ w_j(y) · w_j · x_j / Σ w_j(y) · w_j
      var m1 = new double[n];
      var m2 = new double[n];
      var m3 = new double[n];
      var ma = new double[n];
      var density = new double[n];
      for (var i = 0; i < n; ++i) {
        double y1 = x1[i], y2 = x2[i], y3 = x3[i], ya = xa[i];
        var lastDensity = 0.0;
        for (var iter = 0; iter < maxIterations; ++iter) {
          double sumW = 0, sy1 = 0, sy2 = 0, sy3 = 0, sya = 0;
          for (var j = 0; j < n; ++j) {
            var d1 = y1 - x1[j];
            var d2 = y2 - x2[j];
            var d3 = y3 - x3[j];
            var d4 = ya - xa[j];
            var dist = Math.Sqrt(d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4);
            if (dist >= sigma)
              continue;
            var tri = (1.0 - dist / sigma) * w[j];
            sumW += tri;
            sy1 += x1[j] * tri;
            sy2 += x2[j] * tri;
            sy3 += x3[j] * tri;
            sya += xa[j] * tri;
          }
          if (sumW <= 0) break;

          var ny1 = sy1 / sumW;
          var ny2 = sy2 / sumW;
          var ny3 = sy3 / sumW;
          var nya = sya / sumW;

          var step = (ny1 - y1) * (ny1 - y1) + (ny2 - y2) * (ny2 - y2) + (ny3 - y3) * (ny3 - y3) + (nya - ya) * (nya - ya);
          y1 = ny1;
          y2 = ny2;
          y3 = ny3;
          ya = nya;
          lastDensity = sumW;
          if (step < convergeEps2)
            break;
        }
        m1[i] = y1;
        m2[i] = y2;
        m3[i] = y3;
        ma[i] = ya;
        density[i] = lastDensity;
      }

      // Greedy bucketing of attractors by mergeThreshold. Each bucket carries running
      // weighted centroid + total weight + max density (used by the noise filter).
      var merge2 = (double)mergeThreshold;
      var modeC1 = new List<double>();
      var modeC2 = new List<double>();
      var modeC3 = new List<double>();
      var modeCa = new List<double>();
      var modeWeight = new List<double>();
      var modeDensity = new List<double>();
      for (var i = 0; i < n; ++i) {
        var best = -1;
        var bestDist = double.MaxValue;
        for (var c = 0; c < modeC1.Count; ++c) {
          var d1 = modeC1[c] - m1[i];
          var d2 = modeC2[c] - m2[i];
          var d3 = modeC3[c] - m3[i];
          var d4 = modeCa[c] - ma[i];
          var dist2 = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
          if (dist2 >= bestDist || dist2 > merge2) continue;
          bestDist = dist2;
          best = c;
        }

        if (best < 0) {
          modeC1.Add(m1[i]);
          modeC2.Add(m2[i]);
          modeC3.Add(m3[i]);
          modeCa.Add(ma[i]);
          modeWeight.Add(w[i]);
          modeDensity.Add(density[i]);
          continue;
        }

        var wi = w[i];
        var totalW = modeWeight[best] + wi;
        modeC1[best] = (modeC1[best] * modeWeight[best] + m1[i] * wi) / totalW;
        modeC2[best] = (modeC2[best] * modeWeight[best] + m2[i] * wi) / totalW;
        modeC3[best] = (modeC3[best] * modeWeight[best] + m3[i] * wi) / totalW;
        modeCa[best] = (modeCa[best] * modeWeight[best] + ma[i] * wi) / totalW;
        modeWeight[best] = totalW;
        if (density[i] > modeDensity[best])
          modeDensity[best] = density[i];
      }

      // Noise filter: drop attractors whose density < ξ · max-density. Distinguishes DENCLUE
      // from a pure triangular-kernel mean-shift.
      var maxDensity = 0.0;
      for (var c = 0; c < modeDensity.Count; ++c)
        if (modeDensity[c] > maxDensity)
          maxDensity = modeDensity[c];
      var floor = noiseFraction * maxDensity;

      var modes = new List<(TWork color, double weight)>(modeC1.Count);
      for (var c = 0; c < modeC1.Count; ++c) {
        if (modeDensity[c] < floor) continue;
        modes.Add((ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeC1[c]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeC2[c]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeC3[c]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeCa[c])))
        ), modeWeight[c]));
      }

      if (modes.Count >= k)
        return modes.OrderByDescending(m => m.weight).Take(k).Select(m => m.color);

      var palette = modes.OrderByDescending(m => m.weight).Select(m => m.color).ToList();
      var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
      palette.AddRange(fallback.GeneratePalette(colors, k - palette.Count));
      return palette;
    }
  }
}
