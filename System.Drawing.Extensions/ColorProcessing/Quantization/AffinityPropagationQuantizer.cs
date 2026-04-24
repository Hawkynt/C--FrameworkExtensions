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
/// Affinity Propagation color quantizer — message-passing exemplar selection (Frey &amp; Dueck 2007).
/// </summary>
/// <remarks>
/// <para>
/// Every sample is a candidate "exemplar". Two real-valued messages are passed between points until
/// convergence:
/// </para>
/// <list type="bullet">
///   <item><description><b>Responsibility</b> <c>r(i,k)</c> — how well-suited point <c>k</c> is as
///     exemplar for point <c>i</c>, relative to all other candidates.</description></item>
///   <item><description><b>Availability</b> <c>a(i,k)</c> — how appropriate it is for <c>i</c> to
///     pick <c>k</c>, given how well <c>k</c> suits other points.</description></item>
/// </list>
/// <para>
/// At convergence the points with <c>r(k,k) + a(k,k) &gt; 0</c> are the chosen exemplars; every other
/// point is assigned to its best exemplar. The cluster count is <b>not</b> a parameter — it emerges
/// from the "preference" (self-similarity) <c>s(k,k)</c>, which is pre-set to the median of the
/// off-diagonal similarities (the conventional heuristic).
/// </para>
/// <para>
/// Unlike every existing quantizer in this registry AP does not take <c>k</c> as an input; instead
/// the preference is binary-searched so that the number of returned exemplars matches the requested
/// palette size. Exemplars are actual sample colours (pixel-art-friendly), so this quantizer shares
/// the "anchor = real colour" property of <see cref="KMedoidsQuantizer"/> but with fundamentally
/// different dynamics.
/// </para>
/// <para>Reference: Frey &amp; Dueck (2007) — "Clustering by Passing Messages Between Data Points", Science 315(5814).</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Affinity Propagation", Author = "Frey & Dueck", Year = 2007, QualityRating = 8)]
public struct AffinityPropagationQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum message-passing iterations per inner run.
  /// </summary>
  public int MaxIterations { get; set; } = 100;

  /// <summary>
  /// Gets or sets the damping factor applied to message updates (0.5-0.9 typical).
  /// </summary>
  public float Damping { get; set; } = 0.85f;

  /// <summary>
  /// Gets or sets the number of binary-search rounds over the "preference" parameter to hit the
  /// requested exemplar count.
  /// </summary>
  public int PreferenceSearchRounds { get; set; } = 8;

  /// <summary>
  /// Gets or sets the maximum sample size. AP stores an O(m²) responsibility/availability matrix pair.
  /// </summary>
  public int MaxSampleSize { get; set; } = 384;

  /// <summary>
  /// Gets or sets the deterministic random seed used for sampling and message-tie breaking.
  /// </summary>
  public int Seed { get; set; } = 42;

  public AffinityPropagationQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(
    this.MaxIterations,
    this.Damping,
    this.PreferenceSearchRounds,
    this.MaxSampleSize,
    this.Seed);

  internal sealed class Kernel<TWork>(
    int maxIterations,
    float damping,
    int preferenceSearchRounds,
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
      var m = colors.Length;
      if (m <= k)
        return colors.Select(c => c.color);

      // Project once; similarities are -squared distance (so higher = more similar).
      var c1 = new double[m];
      var c2 = new double[m];
      var c3 = new double[m];
      var ca = new double[m];
      for (var i = 0; i < m; ++i) {
        var (n1, n2, n3, na) = colors[i].color.ToNormalized();
        c1[i] = n1.ToFloat();
        c2[i] = n2.ToFloat();
        c3[i] = n3.ToFloat();
        ca[i] = na.ToFloat();
      }

      var s = new double[m, m];
      var offDiag = new List<double>(m * (m - 1) / 2);
      for (var i = 0; i < m; ++i) {
        for (var j = 0; j < m; ++j) {
          if (i == j) {
            s[i, j] = 0; // filled in below via preference
            continue;
          }

          var d1 = c1[i] - c1[j];
          var d2 = c2[i] - c2[j];
          var d3 = c3[i] - c3[j];
          var d4 = ca[i] - ca[j];
          var neg = -(d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4);
          s[i, j] = neg;
          if (i < j)
            offDiag.Add(neg);
        }
      }

      // Deterministic tiny noise breaks ties in message-passing. Seeded Random, small scale.
      var random = new Random(seed);
      var avgAbs = offDiag.Count == 0 ? 1.0 : offDiag.Select(Math.Abs).Average();
      var noiseScale = avgAbs * 1e-12;
      for (var i = 0; i < m; ++i)
        for (var j = 0; j < m; ++j)
          if (i != j)
            s[i, j] += (random.NextDouble() - 0.5) * noiseScale;

      if (offDiag.Count == 0)
        return colors.Take(k).Select(c => c.color);

      offDiag.Sort();
      var median = offDiag[offDiag.Count / 2];
      var minVal = offDiag[0];

      // Binary-search preference so exemplar count tracks the requested palette size.
      var low = minVal * 4;
      var high = median;
      var bestExemplars = Array.Empty<int>();
      var bestDiff = int.MaxValue;

      for (var round = 0; round < preferenceSearchRounds; ++round) {
        var pref = (low + high) / 2;
        var exemplars = _RunAp(s, m, pref);
        var diff = Math.Abs(exemplars.Length - k);
        if (diff < bestDiff || (diff == bestDiff && bestExemplars.Length == 0)) {
          bestDiff = diff;
          bestExemplars = exemplars;
          if (diff == 0)
            break;
        }

        if (exemplars.Length < k)
          low = pref; // more clusters want higher preference
        else
          high = pref;
      }

      if (bestExemplars.Length == 0)
        bestExemplars = _RunAp(s, m, median);

      if (bestExemplars.Length >= k) {
        // Too many — keep k largest clusters by assignment count.
        return _PickHeaviest(s, m, bestExemplars, k, colors);
      }

      // Pad short palettes via Wu over the non-exemplar residual (same strategy as DBSCAN).
      var palette = bestExemplars.Select(idx => colors[idx].color).ToList();
      var exemplarSet = new HashSet<int>(bestExemplars);
      var residual = new List<(TWork color, uint count)>();
      for (var i = 0; i < m; ++i)
        if (!exemplarSet.Contains(i))
          residual.Add((colors[i].color, colors[i].count));

      var remaining = k - palette.Count;
      if (remaining > 0 && residual.Count > 0) {
        var fallback = ((IQuantizer)new WuQuantizer()).CreateKernel<TWork>();
        palette.AddRange(fallback.GeneratePalette(residual, remaining));
      }

      if (palette.Count < k)
        foreach (var (color, _) in colors.OrderByDescending(c => c.count)) {
          if (palette.Count >= k)
            break;

          var normalized = color.ToNormalized();
          var duplicate = false;
          foreach (var existing in palette) {
            if (!existing.ToNormalized().Equals(normalized))
              continue;

            duplicate = true;
            break;
          }

          if (duplicate)
            continue;

          palette.Add(color);
        }

      return palette;
    }

    private int[] _RunAp(double[,] s, int m, double preference) {
      for (var i = 0; i < m; ++i)
        s[i, i] = preference;

      var r = new double[m, m];
      var a = new double[m, m];
      var lam = (double)damping;
      var invLam = 1 - lam;

      for (var iter = 0; iter < maxIterations; ++iter) {
        // Responsibility update: r(i,k) ← s(i,k) − max_{k'≠k}(a(i,k') + s(i,k'))
        for (var i = 0; i < m; ++i) {
          var max1 = double.NegativeInfinity;
          var max2 = double.NegativeInfinity;
          var max1Idx = -1;
          for (var kk = 0; kk < m; ++kk) {
            var v = a[i, kk] + s[i, kk];
            if (v > max1) {
              max2 = max1;
              max1 = v;
              max1Idx = kk;
            } else if (v > max2) {
              max2 = v;
            }
          }

          for (var kk = 0; kk < m; ++kk) {
            var newR = s[i, kk] - (kk == max1Idx ? max2 : max1);
            r[i, kk] = lam * r[i, kk] + invLam * newR;
          }
        }

        // Availability update: a(i,k) = min(0, r(k,k) + Σ_{i'≠i,k} max(0, r(i',k)))
        //                      a(k,k) = Σ_{i'≠k} max(0, r(i',k))
        for (var kk = 0; kk < m; ++kk) {
          var sumPos = 0.0;
          for (var ii = 0; ii < m; ++ii)
            if (ii != kk)
              sumPos += Math.Max(0, r[ii, kk]);

          var newAkk = sumPos;
          a[kk, kk] = lam * a[kk, kk] + invLam * newAkk;

          for (var ii = 0; ii < m; ++ii) {
            if (ii == kk)
              continue;

            var term = r[kk, kk] + sumPos - Math.Max(0, r[ii, kk]);
            var newA = Math.Min(0, term);
            a[ii, kk] = lam * a[ii, kk] + invLam * newA;
          }
        }
      }

      // Exemplars: points where r(k,k) + a(k,k) > 0.
      var exemplars = new List<int>();
      for (var kk = 0; kk < m; ++kk)
        if (r[kk, kk] + a[kk, kk] > 0)
          exemplars.Add(kk);

      if (exemplars.Count == 0) {
        // Fallback: pick the single best self-exemplar to avoid returning empty.
        var best = 0;
        var bestScore = r[0, 0] + a[0, 0];
        for (var kk = 1; kk < m; ++kk) {
          var v = r[kk, kk] + a[kk, kk];
          if (!(v > bestScore))
            continue;

          bestScore = v;
          best = kk;
        }

        exemplars.Add(best);
      }

      return exemplars.ToArray();
    }

    private static IEnumerable<TWork> _PickHeaviest(
      double[,] s,
      int m,
      int[] exemplars,
      int k,
      (TWork color, uint count)[] colors) {
      var counts = new long[exemplars.Length];
      for (var i = 0; i < m; ++i) {
        var best = 0;
        var bestS = double.NegativeInfinity;
        for (var e = 0; e < exemplars.Length; ++e) {
          var v = s[i, exemplars[e]];
          if (!(v > bestS))
            continue;

          bestS = v;
          best = e;
        }

        counts[best] += colors[i].count;
      }

      return exemplars
        .Select((idx, e) => (idx, weight: counts[e]))
        .OrderByDescending(t => t.weight)
        .Take(k)
        .Select(t => colors[t.idx].color);
    }

  }
}
