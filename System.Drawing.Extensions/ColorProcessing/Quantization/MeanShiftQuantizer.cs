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
/// Mean-Shift color quantizer — Gaussian kernel mode-seeking in the active colour space.
/// </summary>
/// <remarks>
/// <para>
/// For each sample a gradient ascent on the kernel density estimate is performed: the point is
/// iteratively replaced by the weighted mean of its neighbours inside a <see cref="Bandwidth"/>-sized
/// window until convergence. Points that converge to the same mode are merged; each mode becomes a
/// palette anchor. The number of modes is data-driven — <see cref="Bandwidth"/> is the one knob.
/// </para>
/// <para>
/// Unlike DBSCAN (ε-neighbourhood connectivity) Mean-Shift is strictly density-based mode-seeking:
/// it follows the kernel-density gradient and does not require a minimum-points threshold. On highly
/// multimodal colour distributions (e.g. cartoons, logos) this tends to recover cleaner palette
/// anchors than DBSCAN; on smooth photographic content the two agree roughly.
/// </para>
/// <para>
/// Too many modes? Only the <c>k</c> heaviest (by cumulative histogram weight) are kept. Too few?
/// A Wu fallback fills the remaining slots from the residual points; if that still falls short the
/// most-frequent uncovered input colours are appended.
/// </para>
/// <para>Reference: Fukunaga &amp; Hostetler (1975); Comaniciu &amp; Meer (2002) — "Mean Shift: A Robust Approach Toward Feature Space Analysis", IEEE TPAMI.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Mean-Shift", Author = "Comaniciu & Meer", Year = 2002, QualityRating = 7)]
public struct MeanShiftQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the Gaussian kernel bandwidth (Euclidean radius in normalized TWork space).
  /// </summary>
  /// <remarks>Reasonable values for OkLab lie around 0.04-0.08; smaller = more modes.</remarks>
  public float Bandwidth { get; set; } = 0.06f;

  /// <summary>
  /// Gets or sets the maximum number of gradient-ascent steps per sample.
  /// </summary>
  public int MaxIterations { get; set; } = 25;

  /// <summary>
  /// Gets or sets the convergence threshold (squared) for the mode-seeking loop.
  /// </summary>
  public float ConvergenceThreshold { get; set; } = 1e-5f;

  /// <summary>
  /// Gets or sets the distance (squared) below which two modes are fused.
  /// </summary>
  public float MergeThreshold { get; set; } = 1e-3f;

  /// <summary>
  /// Gets or sets the maximum sample size for processing (per-sample mode-seeking is O(n²)).
  /// </summary>
  public int MaxSampleSize { get; set; } = 1024;

  /// <summary>
  /// Gets or sets the deterministic random seed used for sampling.
  /// </summary>
  public int Seed { get; set; } = 42;

  public MeanShiftQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Bandwidth,
    this.MaxIterations,
    this.ConvergenceThreshold,
    this.MergeThreshold,
    this.MaxSampleSize,
    this.Seed);

  internal sealed class Kernel<TWork>(
    float bandwidth,
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

      var h2 = (double)bandwidth * bandwidth;
      // Use -1/(2h²) in Gaussian weight; radius cap at 3·h to bound the inner loop.
      var radius2 = 9.0 * h2;
      var invTwoH2 = 1.0 / (2.0 * h2);
      var convergeEps2 = (double)convergenceThreshold;

      // Shift every sample to its mode.
      var m1 = new double[n];
      var m2 = new double[n];
      var m3 = new double[n];
      var ma = new double[n];
      for (var i = 0; i < n; ++i) {
        double y1 = x1[i], y2 = x2[i], y3 = x3[i], ya = xa[i];
        for (var iter = 0; iter < maxIterations; ++iter) {
          double sumW = 0, sy1 = 0, sy2 = 0, sy3 = 0, sya = 0;
          for (var j = 0; j < n; ++j) {
            var d1 = y1 - x1[j];
            var d2 = y2 - x2[j];
            var d3 = y3 - x3[j];
            var d4 = ya - xa[j];
            var dist2 = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
            if (dist2 > radius2)
              continue;

            var g = Math.Exp(-dist2 * invTwoH2) * w[j];
            sumW += g;
            sy1 += x1[j] * g;
            sy2 += x2[j] * g;
            sy3 += x3[j] * g;
            sya += xa[j] * g;
          }

          if (sumW <= 0)
            break;

          var ny1 = sy1 / sumW;
          var ny2 = sy2 / sumW;
          var ny3 = sy3 / sumW;
          var nya = sya / sumW;

          var step = (ny1 - y1) * (ny1 - y1) + (ny2 - y2) * (ny2 - y2) + (ny3 - y3) * (ny3 - y3) + (nya - ya) * (nya - ya);
          y1 = ny1;
          y2 = ny2;
          y3 = ny3;
          ya = nya;
          if (step < convergeEps2)
            break;
        }

        m1[i] = y1;
        m2[i] = y2;
        m3[i] = y3;
        ma[i] = ya;
      }

      // Merge modes whose pairwise distance is below mergeThreshold (simple greedy bucketing).
      var merge2 = (double)mergeThreshold;
      var assigned = new int[n];
      for (var i = 0; i < n; ++i)
        assigned[i] = -1;

      var modeC1 = new List<double>();
      var modeC2 = new List<double>();
      var modeC3 = new List<double>();
      var modeCa = new List<double>();
      var modeWeight = new List<double>();

      for (var i = 0; i < n; ++i) {
        var best = -1;
        var bestDist = double.MaxValue;
        for (var c = 0; c < modeC1.Count; ++c) {
          var d1 = modeC1[c] - m1[i];
          var d2 = modeC2[c] - m2[i];
          var d3 = modeC3[c] - m3[i];
          var d4 = modeCa[c] - ma[i];
          var dist2 = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
          if (dist2 >= bestDist || dist2 > merge2)
            continue;

          bestDist = dist2;
          best = c;
        }

        if (best < 0) {
          modeC1.Add(m1[i]);
          modeC2.Add(m2[i]);
          modeC3.Add(m3[i]);
          modeCa.Add(ma[i]);
          modeWeight.Add(w[i]);
          assigned[i] = modeC1.Count - 1;
          continue;
        }

        // Weighted running average so the mode tracks its members.
        var wi = w[i];
        var totalW = modeWeight[best] + wi;
        modeC1[best] = (modeC1[best] * modeWeight[best] + m1[i] * wi) / totalW;
        modeC2[best] = (modeC2[best] * modeWeight[best] + m2[i] * wi) / totalW;
        modeC3[best] = (modeC3[best] * modeWeight[best] + m3[i] * wi) / totalW;
        modeCa[best] = (modeCa[best] * modeWeight[best] + ma[i] * wi) / totalW;
        modeWeight[best] = totalW;
        assigned[i] = best;
      }

      var modes = new List<(TWork color, double weight, int id)>(modeC1.Count);
      for (var c = 0; c < modeC1.Count; ++c)
        modes.Add((ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeC1[c]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeC2[c]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeC3[c]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeCa[c])))
        ), modeWeight[c], c));

      if (modes.Count >= k)
        return modes.OrderByDescending(m => m.weight).Take(k).Select(m => m.color);

      // Pad via Wu on the points that fell into the smallest (tail) modes.
      var keepIds = new HashSet<int>(modes.OrderByDescending(m => m.weight).Select(m => m.id));
      var padded = modes.OrderByDescending(m => m.weight).Select(m => m.color).ToList();

      var residual = new List<(TWork color, uint count)>();
      for (var i = 0; i < n; ++i)
        if (!keepIds.Contains(assigned[i]))
          residual.Add((colors[i].color, colors[i].count));

      var remaining = k - padded.Count;
      if (remaining > 0 && residual.Count > 0) {
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        padded.AddRange(fallback.GeneratePalette(residual, remaining));
      }

      if (padded.Count < k)
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

      return padded;
    }

  }
}
