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
/// Epanechnikov-kernel Mean-Shift colour quantizer.
/// </summary>
/// <remarks>
/// <para>
/// Same mode-seeking gradient-ascent dynamics as <see cref="MeanShiftQuantizer"/>, but uses the
/// Epanechnikov kernel <c>K(x) = (1 - ‖x/h‖²)·1[‖x‖≤h]</c> instead of a Gaussian. The
/// Epanechnikov kernel is the asymptotically-optimal kernel in the MSE sense (Parzen-Rosenblatt
/// density estimation) and has <i>compact support</i> — outside the bandwidth, the kernel is
/// strictly zero, making the inner loop cheaper than the open-tailed Gaussian.
/// </para>
/// <para>
/// <b>Practical difference from Gaussian Mean-Shift:</b> Epanechnikov produces slightly sharper
/// mode detection with less "blending" of nearby modes — preferred on sharp-edged colour
/// distributions like cartoons and logos. Gaussian Mean-Shift produces smoother, more
/// photo-appropriate modes.
/// </para>
/// <para>Reference: Epanechnikov (1969) — "Non-parametric estimation of a multivariate
/// probability density"; applied to Mean-Shift in Comaniciu &amp; Meer (2002).</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Mean-Shift (Epanechnikov)", Author = "Epanechnikov; Comaniciu & Meer", Year = 2002, QualityRating = 7)]
public struct EpanechnikovMeanShiftQuantizer : IQuantizer {

  /// <summary>Gets or sets the Epanechnikov kernel bandwidth in normalized colour space.</summary>
  public float Bandwidth { get; set; } = 0.06f;

  /// <summary>Gets or sets the maximum number of gradient-ascent steps per sample.</summary>
  public int MaxIterations { get; set; } = 25;

  /// <summary>Gets or sets the convergence threshold (squared) for mode-seeking.</summary>
  public float ConvergenceThreshold { get; set; } = 1e-5f;

  /// <summary>Gets or sets the merge threshold (squared) for fusing nearby modes.</summary>
  public float MergeThreshold { get; set; } = 1e-3f;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = 1024;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public EpanechnikovMeanShiftQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.Bandwidth, this.MaxIterations, this.ConvergenceThreshold, this.MergeThreshold,
    this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    float bandwidth, int maxIterations, float convergenceThreshold, float mergeThreshold,
    int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0) return [];
      if (colors.Length <= k) return colors.Select(c => c.color);

      colors = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);
      var n = colors.Length;
      var x1 = new double[n]; var x2 = new double[n]; var x3 = new double[n]; var xa = new double[n];
      var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        x1[i] = c1.ToFloat(); x2[i] = c2.ToFloat(); x3[i] = c3.ToFloat(); xa[i] = a.ToFloat();
        w[i] = Math.Max(1, colors[i].count);
      }

      var h2 = (double)bandwidth * bandwidth;
      var convergeEps2 = (double)convergenceThreshold;

      var m1 = new double[n]; var m2 = new double[n]; var m3 = new double[n]; var ma = new double[n];
      for (var i = 0; i < n; ++i) {
        double y1 = x1[i], y2 = x2[i], y3 = x3[i], ya = xa[i];
        for (var iter = 0; iter < maxIterations; ++iter) {
          double sumW = 0, sy1 = 0, sy2 = 0, sy3 = 0, sya = 0;
          for (var j = 0; j < n; ++j) {
            var d1 = y1 - x1[j]; var d2 = y2 - x2[j]; var d3 = y3 - x3[j]; var d4 = ya - xa[j];
            var dist2 = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
            if (dist2 >= h2) continue; // compact support
            var gEp = (1 - dist2 / h2) * w[j];
            sumW += gEp;
            sy1 += x1[j] * gEp; sy2 += x2[j] * gEp; sy3 += x3[j] * gEp; sya += xa[j] * gEp;
          }
          if (sumW <= 0) break;
          var ny1 = sy1 / sumW; var ny2 = sy2 / sumW; var ny3 = sy3 / sumW; var nya = sya / sumW;
          var step = (ny1 - y1) * (ny1 - y1) + (ny2 - y2) * (ny2 - y2) + (ny3 - y3) * (ny3 - y3) + (nya - ya) * (nya - ya);
          y1 = ny1; y2 = ny2; y3 = ny3; ya = nya;
          if (step < convergeEps2) break;
        }
        m1[i] = y1; m2[i] = y2; m3[i] = y3; ma[i] = ya;
      }

      // Merge modes.
      var merge2 = (double)mergeThreshold;
      var modeC1 = new List<double>(); var modeC2 = new List<double>();
      var modeC3 = new List<double>(); var modeCa = new List<double>();
      var modeW = new List<double>();
      for (var i = 0; i < n; ++i) {
        var best = -1;
        var bestD = double.MaxValue;
        for (var c = 0; c < modeC1.Count; ++c) {
          var d1 = modeC1[c] - m1[i]; var d2 = modeC2[c] - m2[i]; var d3 = modeC3[c] - m3[i]; var d4 = modeCa[c] - ma[i];
          var dd = d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4;
          if (dd >= bestD || dd > merge2) continue;
          bestD = dd; best = c;
        }
        if (best < 0) {
          modeC1.Add(m1[i]); modeC2.Add(m2[i]); modeC3.Add(m3[i]); modeCa.Add(ma[i]); modeW.Add(w[i]);
          continue;
        }
        var total = modeW[best] + w[i];
        modeC1[best] = (modeC1[best] * modeW[best] + m1[i] * w[i]) / total;
        modeC2[best] = (modeC2[best] * modeW[best] + m2[i] * w[i]) / total;
        modeC3[best] = (modeC3[best] * modeW[best] + m3[i] * w[i]) / total;
        modeCa[best] = (modeCa[best] * modeW[best] + ma[i] * w[i]) / total;
        modeW[best] = total;
      }

      var palette = new List<(TWork color, double w)>();
      for (var c = 0; c < modeC1.Count; ++c)
        palette.Add((ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeC1[c]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeC2[c]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeC3[c]))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, modeCa[c])))), modeW[c]));

      var top = palette.OrderByDescending(p => p.w).Take(k).Select(p => p.color).ToList();
      if (top.Count < k) {
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        top.AddRange(fallback.GeneratePalette(colors, k - top.Count));
      }
      return top.Take(k);
    }
  }
}
