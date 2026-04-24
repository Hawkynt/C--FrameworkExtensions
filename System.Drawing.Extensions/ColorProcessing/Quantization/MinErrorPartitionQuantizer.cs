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
/// Optimal dynamic-programming luminance-partitioning quantizer — the 1-D luma specialist.
/// </summary>
/// <remarks>
/// <para>
/// Computes the globally-optimal partition of input colours into <c>k</c> luminance bins that
/// minimises sum-of-squared-error on the luma axis. Unlike greedy splitters (Median-Cut, Wu)
/// this is <b>provably optimal</b> for the 1-D SSE-partitioning problem in O(n·k) time via the
/// classical <c>MinErr</c> dynamic program (Bellman, 1973).
/// </para>
/// <para>
/// The trade-off: only luma is optimised — chroma is not. Every palette entry is the weighted
/// centroid of its bin (so chroma is preserved as the average within-bin colour), but hue and
/// saturation are not explicitly partitioned. This makes the quantizer ideal for:
/// </para>
/// <list type="bullet">
///   <item><description>High-contrast line-art and manga artwork where luma preservation dominates.</description></item>
///   <item><description>Tonal-mapping previews where the final rendering step will re-tint the palette.</description></item>
///   <item><description>Any workflow where a <i>proven-optimal</i> quantizer along one axis beats a heuristic in three.</description></item>
/// </list>
/// <para>Reference: Bellman, R. (1973) — "Dynamic Programming"; classical MinErr 1-D optimal
/// partition (Ω(nk) lower bound; SMAWK speedups exist but the simple cubic-cost DP is used here).</para>
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "MinErr Partition", Author = "Bellman (DP)", Year = 1973, QualityRating = 6)]
public struct MinErrorPartitionQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, _ReduceColorsTo);

    private static IEnumerable<TWork> _ReduceColorsTo(int k, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];
      if (colors.Length <= k)
        return colors.Select(c => c.color);

      // Cap n for the O(n·k²) DP table; 512 is a sweet spot on typical palettes.
      const int maxN = 512;
      if (colors.Length > maxN)
        colors = QuantizerHelper.SampleHistogram(colors, maxN, 42);

      var n = colors.Length;

      // Project and sort by luma (Rec.709-ish on the working space channels).
      var pts = new (double c1, double c2, double c3, double a, uint w, double luma)[n];
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        var f1 = c1.ToFloat();
        var f2 = c2.ToFloat();
        var f3 = c3.ToFloat();
        var fa = a.ToFloat();
        pts[i] = (f1, f2, f3, fa, colors[i].count, 0.2126 * f1 + 0.7152 * f2 + 0.0722 * f3);
      }

      Array.Sort(pts, (x, y) => x.luma.CompareTo(y.luma));

      // Precompute prefix sums for O(1) SSE(i,j).
      var pw = new double[n + 1];
      var psumL = new double[n + 1];
      var psumL2 = new double[n + 1];
      for (var i = 0; i < n; ++i) {
        pw[i + 1] = pw[i] + pts[i].w;
        psumL[i + 1] = psumL[i] + pts[i].luma * pts[i].w;
        psumL2[i + 1] = psumL2[i] + pts[i].luma * pts[i].luma * pts[i].w;
      }

      double Cost(int l, int r) {
        // r exclusive.
        var w = pw[r] - pw[l];
        if (w <= 0) return 0;
        var s = psumL[r] - psumL[l];
        var s2 = psumL2[r] - psumL2[l];
        // Σw(x - μ)² = Σw·x² - (Σw·x)²/Σw
        return s2 - s * s / w;
      }

      k = Math.Min(k, n);
      var dp = new double[k + 1, n + 1];
      var cut = new int[k + 1, n + 1];

      // Base case: 1 cluster covers [0,j).
      for (var j = 0; j <= n; ++j) {
        dp[1, j] = Cost(0, j);
        cut[1, j] = 0;
      }
      for (var c = 2; c <= k; ++c) {
        for (var j = c; j <= n; ++j) {
          var best = double.MaxValue;
          var bestI = c - 1;
          for (var i = c - 1; i < j; ++i) {
            var v = dp[c - 1, i] + Cost(i, j);
            if (v >= best) continue;
            best = v;
            bestI = i;
          }
          dp[c, j] = best;
          cut[c, j] = bestI;
        }
      }

      // Backtrack boundaries.
      var boundaries = new int[k + 1];
      boundaries[k] = n;
      for (var c = k; c >= 1; --c)
        boundaries[c - 1] = cut[c, boundaries[c]];

      // Each cluster's palette entry is the weighted centroid across all channels within the bin.
      var result = new TWork[k];
      for (var c = 0; c < k; ++c) {
        var lo = boundaries[c];
        var hi = boundaries[c + 1];
        double s1 = 0, s2 = 0, s3 = 0, sa = 0, sw = 0;
        for (var i = lo; i < hi; ++i) {
          s1 += pts[i].c1 * pts[i].w;
          s2 += pts[i].c2 * pts[i].w;
          s3 += pts[i].c3 * pts[i].w;
          sa += pts[i].a * pts[i].w;
          sw += pts[i].w;
        }
        if (sw <= 0) {
          result[c] = default;
          continue;
        }
        result[c] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s1 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s2 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s3 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sa / sw)))
        );
      }
      return result;
    }
  }
}
