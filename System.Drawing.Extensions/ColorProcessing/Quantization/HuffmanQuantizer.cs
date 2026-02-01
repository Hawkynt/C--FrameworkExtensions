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
/// Huffman-inspired color quantizer using bottom-up hierarchical merging.
/// </summary>
/// <remarks>
/// <para>Similar to Huffman coding, this algorithm builds a tree by progressively merging the least weighted nodes.</para>
/// <para>Unlike traditional quantizers, it preserves dominant colors exactly, making it suitable for logos and graphics.</para>
/// <para>The merge decision balances frequency (weight) with color similarity.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "Huffman", QualityRating = 7)]
public struct HuffmanQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets how many top candidates to examine when selecting pairs to merge.
  /// </summary>
  public int CandidatesToExamine { get; set; } = 20;

  /// <summary>
  /// Gets or sets the weight balancing frequency importance vs. color similarity.
  /// Higher values favor merging similar colors regardless of frequency.
  /// </summary>
  public float SimilarityWeight { get; set; } = 10000.0f;

  public HuffmanQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.CandidatesToExamine, this.SimilarityWeight);

  internal sealed class Kernel<TWork>(int candidatesToExamine, float similarityWeight) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= colorCount)
        return colors.Select(c => c.color);

      // Create leaf nodes for each unique color
      var nodes = colors.Select(c => {
        var (c1, c2, c3, a) = c.color.ToNormalized();
        return new MergeNode(c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat(), c.count);
      }).ToList();

      // Sort by weight ascending (lowest weight first for efficient candidate selection)
      nodes.Sort((a, b) => a.Weight.CompareTo(b.Weight));

      // Merge until we reach desired color count
      while (nodes.Count > colorCount) {
        // Examine top candidates (lowest weights)
        var examineCount = Math.Min(candidatesToExamine, nodes.Count);

        var bestI = 0;
        var bestJ = 1;
        var bestScore = double.MaxValue;

        // Find best pair to merge among candidates
        for (var i = 0; i < examineCount; ++i)
          for (var j = i + 1; j < examineCount; ++j) {
            var colorDist = _ColorDistance(nodes[i], nodes[j]);
            var combinedWeight = nodes[i].Weight + nodes[j].Weight;

            // Score: lower is better (prefer low weight pairs with similar colors)
            var score = combinedWeight * (1.0 + colorDist * similarityWeight);

            if (!(score < bestScore))
              continue;

            bestScore = score;
            bestI = i;
            bestJ = j;
          }

        // Merge the best pair
        var node1 = nodes[bestI];
        var node2 = nodes[bestJ];
        var mergedWeight = node1.Weight + node2.Weight;

        // Weighted average color
        var merged = new MergeNode(
          (node1.C1 * node1.Weight + node2.C1 * node2.Weight) / mergedWeight,
          (node1.C2 * node1.Weight + node2.C2 * node2.Weight) / mergedWeight,
          (node1.C3 * node1.Weight + node2.C3 * node2.Weight) / mergedWeight,
          (node1.A * node1.Weight + node2.A * node2.Weight) / mergedWeight,
          mergedWeight
        );

        // Remove original nodes (remove higher index first to preserve lower index)
        nodes.RemoveAt(bestJ);
        nodes.RemoveAt(bestI);

        // Insert merged node maintaining sorted order
        var insertPos = nodes.FindIndex(n => n.Weight > merged.Weight);
        if (insertPos < 0)
          insertPos = nodes.Count;

        nodes.Insert(insertPos, merged);
      }

      // Convert remaining nodes to palette colors
      return nodes.Select(n => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, n.C1))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, n.C2))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, n.C3))),
        UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, n.A)))
      ));
    }

    private static double _ColorDistance(MergeNode a, MergeNode b) {
      var d1 = a.C1 - b.C1;
      var d2 = a.C2 - b.C2;
      var d3 = a.C3 - b.C3;
      var d4 = a.A - b.A;
      return Math.Sqrt(d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4);
    }

    private sealed class MergeNode(double c1, double c2, double c3, double a, double weight) {
      public double C1 { get; } = c1;
      public double C2 { get; } = c2;
      public double C3 { get; } = c3;
      public double A { get; } = a;
      public double Weight { get; } = weight;
    }

  }
}
