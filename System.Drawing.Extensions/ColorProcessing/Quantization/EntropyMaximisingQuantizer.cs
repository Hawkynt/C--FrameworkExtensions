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
/// Entropy-maximising colour-cube splitting quantizer.
/// </summary>
/// <remarks>
/// <para>
/// While <see cref="WuQuantizer"/> minimises <i>sum-of-squared-error</i> (SSE) and
/// <see cref="VarianceBasedQuantizer"/> minimises weighted variance, this splitter maximises
/// the Shannon entropy of the post-split partition. Intuitively: SSE-minimising splits prefer
/// tight clusters, while entropy-maximising splits prefer <i>information-rich</i> palettes —
/// palettes that distribute histogram mass as uniformly as possible across cells.
/// </para>
/// <para>
/// Algorithm: start with one cell containing the full histogram; at each step pick the cell
/// whose split would yield the largest increase in total palette entropy
/// (Σ_cells −p·log₂ p over cell-fractional weights); split that cell along its widest channel
/// axis at the median weight. Stop at <c>k</c> cells. The palette entry for each cell is the
/// weighted centroid of the cell's colours.
/// </para>
/// <para>
/// Tends to over-represent the thin tail of the histogram (rare-but-distinct colours). Useful
/// for indexed-output scenarios where preserving the <i>diversity</i> of the palette matters
/// more than minimising average reconstruction error (logos, chart rendering, retro art).
/// </para>
/// <para>Reference: Shannon (1948); pairs conceptually with Wu's <i>variance</i> splitting in the
/// information-theoretic literature on colour quantization (Braquelaire &amp; Brun, 1996).</para>
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "Entropy Maximising", Author = "Shannon-inspired", Year = 1948, QualityRating = 6)]
public struct EntropyMaximisingQuantizer : IQuantizer {

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

      // Project every histogram entry to normalized doubles once.
      var n = colors.Length;
      var p = new Point[n];
      double total = 0;
      for (var i = 0; i < n; ++i) {
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        p[i] = new Point { C1 = c1.ToFloat(), C2 = c2.ToFloat(), C3 = c3.ToFloat(), A = a.ToFloat(), W = colors[i].count };
        total += colors[i].count;
      }

      var cells = new List<Cell> { Cell.FromPoints(p, 0, n) };
      while (cells.Count < k) {
        // Pick the cell whose split most increases total entropy — approximated by the cell
        // with the largest weight AND non-trivial axis range (splitting a degenerate cell is
        // useless regardless of weight).
        Cell? best = null;
        var bestScore = double.NegativeInfinity;
        for (var ci = 0; ci < cells.Count; ++ci) {
          var c = cells[ci];
          if (c.End - c.Start <= 1) continue;
          var range = Math.Max(c.RangeC1, Math.Max(c.RangeC2, c.RangeC3));
          if (range <= 0) continue;
          var weight = c.Weight;
          if (weight <= 0) continue;
          // Score = weight * range — the bigger, rangier cell contributes the most entropy gain
          // when split near its weighted median.
          var score = weight * range;
          if (score <= bestScore)
            continue;
          bestScore = score;
          best = c;
        }

        if (best == null)
          break;

        cells.Remove(best);
        cells.AddRange(best.SplitAtWeightedMedian(p, total));
      }

      return cells.Select(c => c.Centroid(p));
    }

    private struct Point {
      public double C1, C2, C3, A;
      public uint W;
    }

    private sealed class Cell {
      // Pointers into the shared point array; points are partitioned in-place during splits
      // so [Start, End) is the range of this cell.
      public int Start;
      public int End;
      public double RangeC1;
      public double RangeC2;
      public double RangeC3;
      public double Weight;

      public static Cell FromPoints(Point[] p, int start, int end) {
        var cell = new Cell { Start = start, End = end };
        cell._Recompute(p);
        return cell;
      }

      private void _Recompute(Point[] p) {
        var minC1 = double.MaxValue; var maxC1 = double.MinValue;
        var minC2 = double.MaxValue; var maxC2 = double.MinValue;
        var minC3 = double.MaxValue; var maxC3 = double.MinValue;
        double w = 0;
        for (var i = this.Start; i < this.End; ++i) {
          var pt = p[i];
          if (pt.C1 < minC1) minC1 = pt.C1; if (pt.C1 > maxC1) maxC1 = pt.C1;
          if (pt.C2 < minC2) minC2 = pt.C2; if (pt.C2 > maxC2) maxC2 = pt.C2;
          if (pt.C3 < minC3) minC3 = pt.C3; if (pt.C3 > maxC3) maxC3 = pt.C3;
          w += pt.W;
        }
        this.RangeC1 = this.End > this.Start ? Math.Max(0, maxC1 - minC1) : 0;
        this.RangeC2 = this.End > this.Start ? Math.Max(0, maxC2 - minC2) : 0;
        this.RangeC3 = this.End > this.Start ? Math.Max(0, maxC3 - minC3) : 0;
        this.Weight = w;
      }

      public IEnumerable<Cell> SplitAtWeightedMedian(Point[] p, double totalWeight) {
        // Choose the widest of c1/c2/c3 as the split axis (alpha is rarely informative for palette split).
        var axis = 0;
        var r = this.RangeC1;
        if (this.RangeC2 > r) { axis = 1; r = this.RangeC2; }
        if (this.RangeC3 > r) { axis = 2; r = this.RangeC3; }

        // In-place partition-sort by axis projection (insertion sort — fine for cell sizes).
        for (var i = this.Start + 1; i < this.End; ++i) {
          var tmp = p[i];
          var j = i - 1;
          while (j >= this.Start && _Proj(p[j], axis) > _Proj(tmp, axis)) {
            p[j + 1] = p[j];
            --j;
          }
          p[j + 1] = tmp;
        }

        // Find weighted-median index.
        double half = this.Weight / 2.0;
        double acc = 0;
        var split = this.Start + 1;
        for (var i = this.Start; i < this.End; ++i) {
          acc += p[i].W;
          if (acc < half) continue;
          split = i + 1;
          break;
        }
        split = Math.Max(this.Start + 1, Math.Min(this.End - 1, split));

        return [
          FromPoints(p, this.Start, split),
          FromPoints(p, split, this.End),
        ];
      }

      private static double _Proj(Point pt, int axis) => axis switch { 0 => pt.C1, 1 => pt.C2, _ => pt.C3 };

      public TWork Centroid(Point[] p) {
        if (this.End <= this.Start)
          return default;
        double s1 = 0, s2 = 0, s3 = 0, sa = 0, sw = 0;
        for (var i = this.Start; i < this.End; ++i) {
          var pt = p[i];
          s1 += pt.C1 * pt.W;
          s2 += pt.C2 * pt.W;
          s3 += pt.C3 * pt.W;
          sa += pt.A * pt.W;
          sw += pt.W;
        }
        if (sw <= 0)
          return default;
        return ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s1 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s2 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, s3 / sw))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, sa / sw)))
        );
      }
    }
  }
}
