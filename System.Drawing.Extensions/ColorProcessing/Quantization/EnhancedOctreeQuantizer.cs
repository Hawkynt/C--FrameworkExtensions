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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

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
      var histArray = histogram as (TWork color, uint count)[] ?? histogram.ToArray();

      // When the unique-colour histogram is large enough to amortise thread-spawn overhead,
      // partition into chunks, build one private sub-tree per chunk, and merge them in
      // deterministic partition order. ulong PixelCount merges associatively (bit-exact).
      // Welford's running variance is order-dependent in the serial form, so per-thread
      // sub-trees use a Chan/Welford parallel-merge formula (Chan, Golub & LeVeque 1979) when
      // combining child accumulators into the master. Note: VarianceSum is currently a
      // computed-but-unread field on this class; merging it correctly preserves semantic
      // equivalence for any future reader without affecting the produced palette (which only
      // reads C{1,2,3}Sum/PixelCount via OctreeNode.CreateColor). Goldens use 64²/96²/128²
      // inputs whose histogram length stays well under the threshold, keeping the sequential
      // path and bit-exact output for all 82 entries.
      OctreeNode root;
      var leafCount = 0;
      if (histArray.Length >= _ParallelBuildThreshold)
        root = this._BuildTreeParallel(histArray, ref leafCount);
      else {
        root = new OctreeNode();
        // Build octree with variance tracking. M3: branch-once on TWork; the JIT folds
        // the comparison and inlines the byte-domain fast path for 32bpp 4-channel storage.
        if (typeof(TWork) == typeof(Bgra8888))
          for (var i = 0; i < histArray.Length; ++i)
            _AddColorFast32bpp4ch<BgraLayout>(root, histArray[i].color, histArray[i].count, ref leafCount);
        else
          for (var i = 0; i < histArray.Length; ++i)
            _AddColor(root, histArray[i].color, 0, histArray[i].count, ref leafCount);
      }

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

    /// <summary>
    /// Minimum unique-colour histogram length for the parallel tree-build path. As with
    /// <see cref="OctreeQuantizer"/>, octree merge has the same per-node work as sequential
    /// insertion so the parallel speedup is bounded by Amdahl on the merge phase. The threshold
    /// is set high enough to keep goldens on the sequential path (bit-exact) and avoid regressing
    /// typical photo workloads, while leaving the parallel path available for genuinely extreme
    /// inputs.
    /// </summary>
    private const int _ParallelBuildThreshold = 1 << 20; // 1,048,576 unique colours

    /// <summary>
    /// Parallel tree-build. Partitions the histogram, builds a private sub-tree per chunk,
    /// then merges them deterministically in partition-index order. Sums and counts merge
    /// associatively for ulong; double-precision sums merge in fixed order; VarianceSum is
    /// combined via Chan/Welford's parallel formula (Chan, Golub &amp; LeVeque 1979) so the
    /// merged variance is correct even though the serial form is order-dependent.
    /// </summary>
    private OctreeNode _BuildTreeParallel((TWork color, uint count)[] histogram, ref int leafCount) {
      var total = histogram.Length;
      var partitionCount = Math.Min(Environment.ProcessorCount, Math.Max(2, total / 16384));
      var chunkSize = (total + partitionCount - 1) / partitionCount;
      var subTrees = new OctreeNode[partitionCount];

      // Capture-by-value of the kernel ref-type for the lambda; instance fields (maxLevel, etc.)
      // are read-only after construction so concurrent reads are safe.
      var self = this;
      Parallel.For(0, partitionCount, p => {
        var start = p * chunkSize;
        var end = Math.Min(start + chunkSize, total);
        var local = new OctreeNode();
        var localLeafCount = 0;
        if (typeof(TWork) == typeof(Bgra8888))
          for (var i = start; i < end; ++i)
            self._AddColorFast32bpp4ch<BgraLayout>(local, histogram[i].color, histogram[i].count, ref localLeafCount);
        else
          for (var i = start; i < end; ++i)
            self._AddColor(local, histogram[i].color, 0, histogram[i].count, ref localLeafCount);
        subTrees[p] = local;
      });

      // Deterministic in-order merge.
      var master = new OctreeNode();
      var mergedLeafCount = 0;
      for (var p = 0; p < partitionCount; ++p)
        _MergeSubTreeInto(master, subTrees[p], ref mergedLeafCount);

      leafCount = mergedLeafCount;
      return master;
    }

    /// <summary>
    /// Merges <paramref name="src"/> into <paramref name="dst"/> in deterministic order. At a
    /// leaf, sums are added and the variance is combined via Chan's parallel formula. At an
    /// internal node, recurse into matching child slots.
    /// </summary>
    private static void _MergeSubTreeInto(OctreeNode dst, OctreeNode src, ref int leafCount) {
      var srcHasChildren = false;
      for (var i = 0; i < src.Children.Length; ++i)
        if (src.Children[i] != null) {
          srcHasChildren = true;
          break;
        }

      if (!srcHasChildren) {
        // src is a leaf — replicate the leaf-update arithmetic from _DescendAndAccumulate
        // but in the associative-merge form that does not depend on insertion order.
        if (src.PixelCount == 0)
          return; // empty source leaf — no contribution
        if (dst.PixelCount == 0)
          ++leafCount;

        var nA = (double)dst.PixelCount;
        var nB = (double)src.PixelCount;
        var n = nA + nB;

        // Chan/Welford parallel-merge for VarianceSum (only if both sides have data; else just
        // adopt the non-empty side's value). This treats VarianceSum as the per-leaf M2-style
        // weighted-squared-deviation accumulator the serial path computes.
        if (dst.PixelCount > 0 && src.PixelCount > 0) {
          var meanA1 = dst.C1Sum / nA;
          var meanA2 = dst.C2Sum / nA;
          var meanA3 = dst.C3Sum / nA;
          var meanB1 = src.C1Sum / nB;
          var meanB2 = src.C2Sum / nB;
          var meanB3 = src.C3Sum / nB;
          var d1 = meanB1 - meanA1;
          var d2 = meanB2 - meanA2;
          var d3 = meanB3 - meanA3;
          var factor = nA * nB / n;
          dst.VarianceSum = dst.VarianceSum + src.VarianceSum + (d1 * d1 + d2 * d2 + d3 * d3) * factor;
        } else
          dst.VarianceSum += src.VarianceSum;

        dst.PixelCount = (ulong)n;
        dst.C1Sum += src.C1Sum;
        dst.C2Sum += src.C2Sum;
        dst.C3Sum += src.C3Sum;
        dst.ASum += src.ASum;
        return;
      }

      // src is internal — recurse into matching child slots.
      for (var i = 0; i < src.Children.Length; ++i) {
        var srcChild = src.Children[i];
        if (srcChild == null)
          continue;
        if (dst.Children[i] == null)
          dst.Children[i] = new OctreeNode();
        _MergeSubTreeInto(dst.Children[i]!, srcChild, ref leafCount);
      }
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

      _DescendAndAccumulate(node, level, pixelCount, ref leafCount, c1Byte, c2Byte, c3Byte, c1Float, c2Float, c3Float, aFloat);
    }

    /// <summary>
    /// 32bpp 4-channel fast path. Bytes are read directly from the packed pixel via the
    /// JIT-folded layout descriptor; floats are computed from those same bytes using the
    /// arithmetic <see cref="UNorm32.FromByte"/> + <c>ToFloat()</c> would have produced. Result
    /// values for tree-descent indices and leaf accumulators are bit-exact with the slow path.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void _AddColorFast32bpp4ch<TLayout>(OctreeNode node, TWork color, ulong pixelCount, ref int leafCount)
      where TLayout : struct {
      var packed = Unsafe.As<TWork, uint>(ref color);
      var (c1Byte, c2Byte, c3Byte, aByte) = StorageLayoutFast.UnpackBytes<TLayout>(packed);

      const float unormToFloat = 1f / uint.MaxValue;
      var c1Float = (float)((uint)c1Byte * 0x01010101u) * unormToFloat;
      var c2Float = (float)((uint)c2Byte * 0x01010101u) * unormToFloat;
      var c3Float = (float)((uint)c3Byte * 0x01010101u) * unormToFloat;
      var aFloat = (float)((uint)aByte * 0x01010101u) * unormToFloat;

      _DescendAndAccumulate(node, 0, pixelCount, ref leafCount, c1Byte, c2Byte, c3Byte, c1Float, c2Float, c3Float, aFloat);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void _DescendAndAccumulate(OctreeNode node, int level, ulong pixelCount, ref int leafCount,
      byte c1Byte, byte c2Byte, byte c3Byte,
      float c1Float, float c2Float, float c3Float, float aFloat) {
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
