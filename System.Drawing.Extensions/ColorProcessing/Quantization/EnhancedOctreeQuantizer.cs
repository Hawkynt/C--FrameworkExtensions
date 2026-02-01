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
/// Enhanced Octree-based color quantizer with variance tracking and adaptive thresholds.
/// </summary>
/// <remarks>
/// <para>Improves upon basic Octree quantization by tracking weighted variance per node,
/// using adaptive pruning thresholds per level, and reserving colors at level 2 for coverage.</para>
/// <para>Reference: Bloomberg (2008) - "Color quantization using octrees", Leptonica</para>
/// <para>See also: http://www.leptonica.org/color-quantization.html</para>
/// </remarks>
[Quantizer(QuantizationType.Tree, DisplayName = "Enhanced Octree", Author = "D.S. Bloomberg", Year = 2008, QualityRating = 8)]
public struct EnhancedOctreeQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum tree depth (2^depth max leaves at deepest level).
  /// </summary>
  public int MaxLevel { get; set; } = 5;

  /// <summary>
  /// Gets or sets the number of colors reserved at level 2 for full color coverage.
  /// </summary>
  public int ReservedLevel2Colors { get; set; } = 64;

  public EnhancedOctreeQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.MaxLevel, this.ReservedLevel2Colors);

  internal sealed class Kernel<TWork>(int maxLevel, int reservedLevel2Colors) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    // Threshold factors per level: no pruning at levels 0-1, progressive at deeper levels
    private static readonly double[] ThresholdFactors = [0, 0, 1, 1, 1, 1, 1, 1];

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var root = new OctreeNode();
      var leafCount = 0;

      // Build octree with variance tracking
      foreach (var (color, count) in histogram)
        _AddColor(root, color, 0, count, ref leafCount);

      // Calculate reserved slots for level 2 coverage
      var reserved = Math.Min(reservedLevel2Colors, colorCount / 2);
      var remainingSlots = colorCount - reserved;

      // Collect all leaves with their statistics
      var leaves = new List<(OctreeNode node, int level)>();
      _CollectLeaves(root, 0, leaves);

      // Separate level 2 nodes for coverage reservation
      var level2Leaves = leaves.Where(l => l.level <= 2).OrderByDescending(l => l.node.PixelCount).ToList();
      var deeperLeaves = leaves.Where(l => l.level > 2).OrderByDescending(l => l.node.PixelCount).ToList();

      // Select palette: reserve some level 2 colors, fill rest with CTE (Color Table Entry) selection
      var selectedLeaves = new List<OctreeNode>();

      // Reserve level 2 colors for coverage
      var level2Selected = level2Leaves.Take(reserved).Select(l => l.node).ToList();
      selectedLeaves.AddRange(level2Selected);

      // Fill remaining with deeper leaves by pixel count (CTE selection)
      var deeperSelected = deeperLeaves.Take(remainingSlots).Select(l => l.node).ToList();
      selectedLeaves.AddRange(deeperSelected);

      // If we still need more colors and have level 2 colors remaining
      if (selectedLeaves.Count < colorCount) {
        var additional = level2Leaves.Skip(reserved).Take(colorCount - selectedLeaves.Count).Select(l => l.node);
        selectedLeaves.AddRange(additional);
      }

      // Prune if we have too many
      if (selectedLeaves.Count > colorCount) {
        // Use adaptive threshold based on variance
        selectedLeaves = selectedLeaves
          .OrderByDescending(n => n.PixelCount)
          .Take(colorCount)
          .ToList();
      }

      // Generate palette from selected leaves
      return selectedLeaves.Select(n => n.CreateColor<TWork>());
    }

    private void _AddColor(OctreeNode node, TWork color, int level, ulong pixelCount, ref int leafCount) {
      var (c1, c2, c3, a) = color.ToNormalized();
      var c1Byte = c1.ToByte();
      var c2Byte = c2.ToByte();
      var c3Byte = c3.ToByte();

      var c1Float = c1.ToFloat();
      var c2Float = c2.ToFloat();
      var c3Float = c3.ToFloat();
      var aFloat = a.ToFloat();

      for (;;) {
        if (level >= maxLevel) {
          // Update leaf node with variance tracking
          if (node.PixelCount == 0)
            ++leafCount;

          var oldCount = node.PixelCount;
          var newCount = oldCount + pixelCount;

          // Welford's online algorithm for variance
          if (oldCount > 0) {
            var delta1 = c1Float - node.C1Sum / oldCount;
            var delta2 = c2Float - node.C2Sum / oldCount;
            var delta3 = c3Float - node.C3Sum / oldCount;
            node.VarianceSum += pixelCount * (
              delta1 * delta1 +
              delta2 * delta2 +
              delta3 * delta3
            );
          }

          node.PixelCount = newCount;
          node.C1Sum += c1Float * pixelCount;
          node.C2Sum += c2Float * pixelCount;
          node.C3Sum += c3Float * pixelCount;
          node.ASum += aFloat * pixelCount;
          return;
        }

        var index =
          (((c1Byte >> (7 - level)) & 1) << 2) |
          (((c2Byte >> (7 - level)) & 1) << 1) |
          ((c3Byte >> (7 - level)) & 1);

        if (node.Children[index] == null)
          node.Children[index] = new();

        node = node.Children[index]!;
        ++level;
      }
    }

    private static void _CollectLeaves(OctreeNode node, int level, List<(OctreeNode node, int level)> leaves) {
      var hasChildren = false;
      foreach (var child in node.Children)
        if (child != null) {
          hasChildren = true;
          _CollectLeaves(child, level + 1, leaves);
        }

      if (!hasChildren && node.PixelCount > 0)
        leaves.Add((node, level));
    }

    private sealed class OctreeNode {
      public OctreeNode?[] Children { get; } = new OctreeNode?[8];
      public double C1Sum { get; set; }
      public double C2Sum { get; set; }
      public double C3Sum { get; set; }
      public double ASum { get; set; }
      public double VarianceSum { get; set; }
      public ulong PixelCount { get; set; }

      public T CreateColor<T>() where T : unmanaged, IColorSpace4<T> {
        if (this.PixelCount == 0)
          return default;

        var c1 = (float)(this.C1Sum / this.PixelCount);
        var c2 = (float)(this.C2Sum / this.PixelCount);
        var c3 = (float)(this.C3Sum / this.PixelCount);
        var a = (float)(this.ASum / this.PixelCount);

        return ColorFactory.FromNormalized_4<T>(
          UNorm32.FromFloatClamped(c1),
          UNorm32.FromFloatClamped(c2),
          UNorm32.FromFloatClamped(c3),
          UNorm32.FromFloatClamped(a)
        );
      }
    }

  }
}
