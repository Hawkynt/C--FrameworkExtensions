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
/// Mitchell's best-candidate sampling colour quantizer — Mitchell (1991).
/// </summary>
/// <remarks>
/// <para>
/// Mitchell's algorithm approximates a blue-noise (Poisson-disc-like) distribution
/// of points: for each new palette entry, <see cref="CandidatesPerSample"/> random
/// candidates are drawn (weighted by histogram count), and the one whose minimum
/// distance to the already-selected palette is largest is kept. This produces a
/// palette of <i>maximally-spread</i> actual input colours — every entry is far
/// from every other entry in the input's colour distribution.
/// </para>
/// <para>
/// Distinct from <see cref="GoldenRatioPaletteQuantizer"/> (image-independent φ⁻¹
/// Lab hue sweep — deterministic quasi-random on the hue ring) and
/// <see cref="KMedoidsQuantizer"/> (PAM — selects actual colours but minimises
/// assignment cost, not mutual distance). Mitchell best-candidate is
/// <i>image-adaptive</i> (candidates come from the actual histogram) but
/// optimises for <i>spread</i> rather than reconstruction error — useful when
/// pixel-art authors want a visibly diverse palette rather than a centroid-hugging
/// one.
/// </para>
/// <para>
/// Reference: Don P. Mitchell (1991) — "Spectrally Optimal Sampling for Distribution
/// Ray Tracing", Proc. SIGGRAPH '91 25(4):157-164; described as a cheap
/// approximation to Poisson-disc sampling.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Mitchell Best-Candidate", Author = "D.P. Mitchell", Year = 1991, QualityRating = 6)]
public struct MitchellBestCandidateQuantizer : IQuantizer {

  /// <summary>Gets or sets the number of candidates drawn per palette entry (classical value: k+1, typically 10-30).</summary>
  public int CandidatesPerSample { get; set; } = 20;

  /// <summary>Gets or sets the maximum sample size.</summary>
  public int MaxSampleSize { get; set; } = QuantizerHelper.DefaultMaxSampleSize;

  /// <summary>Gets or sets the deterministic random seed.</summary>
  public int Seed { get; set; } = 42;

  public MitchellBestCandidateQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.CandidatesPerSample, this.MaxSampleSize, this.Seed);

  internal sealed class Kernel<TWork>(
    int candidatesPerSample, int maxSampleSize, int seed) : IQuantizer<TWork>
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
      var tot = 0.0;
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, al) = sampled[i].color.ToNormalized();
        x[i] = c1.ToFloat(); y[i] = c2.ToFloat(); z[i] = c3.ToFloat(); a[i] = al.ToFloat();
        w[i] = Math.Max(1, sampled[i].count); tot += w[i];
      }

      var random = new Random(seed);
      var chosen = new int[k];

      // First entry: weighted sample.
      chosen[0] = _WeightedPick(w, n, tot, random);

      for (var j = 1; j < k; ++j) {
        var bestIdx = -1;
        var bestMinDist = -1.0;
        var trials = Math.Max(1, candidatesPerSample);
        for (var t = 0; t < trials; ++t) {
          var cand = _WeightedPick(w, n, tot, random);
          // Reject if already chosen; but don't loop forever.
          var duplicate = false;
          for (var p = 0; p < j; ++p) if (chosen[p] == cand) { duplicate = true; break; }
          if (duplicate) continue;
          // Compute min distance to existing palette.
          var minD = double.MaxValue;
          for (var p = 0; p < j; ++p) {
            var q = chosen[p];
            var dx = x[cand] - x[q]; var dy = y[cand] - y[q]; var dz = z[cand] - z[q]; var da = a[cand] - a[q];
            var d = dx * dx + dy * dy + dz * dz + da * da;
            if (d < minD) minD = d;
          }
          if (minD <= bestMinDist) continue;
          bestMinDist = minD; bestIdx = cand;
        }
        if (bestIdx < 0) {
          // All trials were duplicates — pick any not-yet-used.
          for (var i = 0; i < n; ++i) {
            var taken = false;
            for (var p = 0; p < j; ++p) if (chosen[p] == i) { taken = true; break; }
            if (!taken) { bestIdx = i; break; }
          }
          if (bestIdx < 0) bestIdx = chosen[0];
        }
        chosen[j] = bestIdx;
      }

      var palette = new TWork[k];
      for (var j = 0; j < k; ++j) {
        var idx = chosen[j];
        palette[j] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)x[idx]),
          UNorm32.FromFloatClamped((float)y[idx]),
          UNorm32.FromFloatClamped((float)z[idx]),
          UNorm32.FromFloatClamped((float)a[idx]));
      }
      return palette;
    }

    private static int _WeightedPick(double[] w, int n, double tot, Random r) {
      var t = r.NextDouble() * tot;
      var c = 0.0;
      for (var i = 0; i < n; ++i) {
        c += w[i];
        if (c >= t) return i;
      }
      return n - 1;
    }

  }
}
