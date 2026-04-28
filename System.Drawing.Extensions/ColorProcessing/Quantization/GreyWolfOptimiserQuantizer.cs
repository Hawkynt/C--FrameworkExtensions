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
/// Grey Wolf Optimiser colour quantizer — Mirjalili, Mirjalili &amp; Lewis (2014).
/// </summary>
/// <remarks>
/// <para>
/// The swarm of candidate palettes is ordered by fitness each iteration; the
/// top three are labelled <c>α</c>, <c>β</c> and <c>δ</c> (the pack's leaders).
/// Each remaining wolf updates its position toward the <i>average</i> of three
/// candidate positions <c>X₁ = Xα − A·|C·Xα − X|</c> (and analogously for β, δ),
/// where <c>A = 2a·r − a</c> and <c>C = 2r</c> are the classical GWO coefficients
/// and <c>a</c> linearly decreases from 2 to 0 across iterations. As <c>a</c>
/// shrinks the swarm transitions from exploration (|A|&gt;1) to exploitation
/// (|A|&lt;1).
/// </para>
/// <para>
/// Distinct from <see cref="ParticleSwarmQuantizer"/> (single gBest leader) and
/// <see cref="FireflyQuantizer"/> (pairwise brightness attraction): GWO uses a
/// three-leader hierarchy plus smoothly-shrinking exploration radius. In practice
/// this yields competitive results with fewer parameters to tune.
/// </para>
/// <para>
/// Reference: S. Mirjalili, S.M. Mirjalili &amp; A. Lewis (2014) — "Grey Wolf
/// Optimizer", Advances in Engineering Software 69:46-61.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Grey Wolf Optimiser", Author = "Mirjalili et al.", Year = 2014, QualityRating = 8)]
public struct GreyWolfOptimiserQuantizer : IQuantizer {

  /// <summary>Gets or sets the pack size.</summary>
  public int PackSize { get; set; } = 20;

  /// <summary>Gets or sets the number of iterations.</summary>
  public int Iterations { get; set; } = 50;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public GreyWolfOptimiserQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.PackSize, this.Iterations, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int packSize, int iterations, int maxSampleSize, int seed) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0) return [];
      if (colors.Length <= k) return colors.Select(c => c.color);

      var sampled = QuantizerHelper.SampleHistogram(colors, maxSampleSize, seed);
      var n = sampled.Length;
      var x = new double[n]; var y = new double[n]; var z = new double[n]; var a = new double[n]; var w = new double[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, al) = sampled[i].color.ToNormalized();
        x[i] = c1.ToFloat(); y[i] = c2.ToFloat(); z[i] = c3.ToFloat(); a[i] = al.ToFloat();
        w[i] = Math.Max(1, sampled[i].count);
      }

      var random = new Random(seed);
      var dim = k * 4;
      var pack = new double[packSize][];
      var fit = new double[packSize];
      for (var p = 0; p < packSize; ++p) {
        pack[p] = _KmPlusPlus(x, y, z, a, w, n, k, random);
        fit[p] = -_Mse(x, y, z, a, w, n, pack[p], k);
      }

      for (var it = 0; it < iterations; ++it) {
        // Identify alpha, beta, delta wolves.
        int alphaIdx = 0, betaIdx = 1, deltaIdx = 2;
        if (fit[betaIdx] > fit[alphaIdx]) (alphaIdx, betaIdx) = (betaIdx, alphaIdx);
        if (fit[deltaIdx] > fit[alphaIdx]) (alphaIdx, deltaIdx) = (deltaIdx, alphaIdx);
        if (fit[deltaIdx] > fit[betaIdx]) (betaIdx, deltaIdx) = (deltaIdx, betaIdx);
        for (var p = 3; p < packSize; ++p) {
          if (fit[p] > fit[alphaIdx]) { deltaIdx = betaIdx; betaIdx = alphaIdx; alphaIdx = p; }
          else if (fit[p] > fit[betaIdx]) { deltaIdx = betaIdx; betaIdx = p; }
          else if (fit[p] > fit[deltaIdx]) { deltaIdx = p; }
        }

        // a decreases linearly from 2 to 0.
        var a2 = iterations <= 1 ? 0.0 : 2.0 * (1.0 - (double)it / (iterations - 1));

        for (var p = 0; p < packSize; ++p) {
          if (p == alphaIdx || p == betaIdx || p == deltaIdx) continue;
          for (var d = 0; d < dim; ++d) {
            var X1 = _Candidate(pack[alphaIdx][d], pack[p][d], a2, random);
            var X2 = _Candidate(pack[betaIdx][d], pack[p][d], a2, random);
            var X3 = _Candidate(pack[deltaIdx][d], pack[p][d], a2, random);
            var v = (X1 + X2 + X3) / 3.0;
            if (v < 0) v = 0;
            else if (v > 1) v = 1;
            pack[p][d] = v;
          }
          fit[p] = -_Mse(x, y, z, a, w, n, pack[p], k);
        }
      }

      var bestIdx = 0;
      for (var p = 1; p < packSize; ++p) if (fit[p] > fit[bestIdx]) bestIdx = p;

      var palette = new TWork[k];
      for (var j = 0; j < k; ++j)
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)pack[bestIdx][j * 4 + 0]),
          UNorm32.FromFloatClamped((float)pack[bestIdx][j * 4 + 1]),
          UNorm32.FromFloatClamped((float)pack[bestIdx][j * 4 + 2]),
          UNorm32.FromFloatClamped((float)pack[bestIdx][j * 4 + 3]));
      return palette;
    }

    private static double _Candidate(double leader, double self, double a, Random r) {
      var A = 2 * a * r.NextDouble() - a;
      var C = 2 * r.NextDouble();
      var D = Math.Abs(C * leader - self);
      return leader - A * D;
    }

    private static double[] _KmPlusPlus(double[] x, double[] y, double[] z, double[] a, double[] w, int n, int k, Random r) {
      var v = new double[k * 4];
      var totalW = 0.0;
      for (var i = 0; i < n; ++i) totalW += w[i];
      var target = r.NextDouble() * totalW;
      var cum = 0.0; var idx = 0;
      for (var i = 0; i < n; ++i) { cum += w[i]; if (cum < target) continue; idx = i; break; }
      v[0] = x[idx]; v[1] = y[idx]; v[2] = z[idx]; v[3] = a[idx];
      var d2 = new double[n];
      for (var i = 0; i < n; ++i) {
        var dx = x[i] - v[0]; var dy = y[i] - v[1]; var dz = z[i] - v[2];
        d2[i] = dx * dx + dy * dy + dz * dz;
      }
      for (var j = 1; j < k; ++j) {
        var tot = 0.0;
        for (var i = 0; i < n; ++i) tot += d2[i] * w[i];
        if (tot <= 0) { idx = r.Next(n); }
        else {
          target = r.NextDouble() * tot; cum = 0.0; idx = 0;
          for (var i = 0; i < n; ++i) { cum += d2[i] * w[i]; if (cum < target) continue; idx = i; break; }
        }
        v[j * 4 + 0] = x[idx]; v[j * 4 + 1] = y[idx]; v[j * 4 + 2] = z[idx]; v[j * 4 + 3] = a[idx];
        for (var i = 0; i < n; ++i) {
          var dx = x[i] - v[j * 4 + 0]; var dy = y[i] - v[j * 4 + 1]; var dz = z[i] - v[j * 4 + 2];
          var dd = dx * dx + dy * dy + dz * dz;
          if (dd < d2[i]) d2[i] = dd;
        }
      }
      return v;
    }

    private static double _Mse(double[] x, double[] y, double[] z, double[] a, double[] w, int n, double[] pal, int k) {
      var total = 0.0; var tw = 0.0;
      for (var i = 0; i < n; ++i) {
        var best = double.MaxValue;
        for (var j = 0; j < k; ++j) {
          var dx = x[i] - pal[j * 4 + 0]; var dy = y[i] - pal[j * 4 + 1];
          var dz = z[i] - pal[j * 4 + 2]; var da = a[i] - pal[j * 4 + 3];
          var d = dx * dx + dy * dy + dz * dz + da * da;
          if (d < best) best = d;
        }
        total += best * w[i]; tw += w[i];
      }
      return tw > 0 ? total / tw : total;
    }

  }
}
