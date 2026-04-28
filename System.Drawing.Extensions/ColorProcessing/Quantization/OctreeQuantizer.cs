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
/// Octree-based color quantizer with configurable parameters.
/// </summary>
/// <remarks>
/// Builds a tree where each node represents a region of color space,
/// then merges leaves with lowest reference counts to reduce palette size.
/// </remarks>
[Quantizer(QuantizationType.Tree, DisplayName = "Octree", Year = 1988, QualityRating = 7)]
public struct OctreeQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private const int _MAX_DEPTH = 7;

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var histArray = histogram as (TWork color, uint count)[] ?? histogram.ToArray();

      // When the unique-colour histogram is large enough to amortise thread-spawn overhead,
      // partition into chunks, build one private sub-tree per chunk, and merge them in
      // deterministic partition order. ulong PixelCount and ReferencesCount merge associatively
      // (bit-exact); double C{1,2,3}Sum/ASum merge in fixed partition order so the result is
      // reproducible across runs. The sequential path stays bit-exact for all goldens because the
      // 64²/96²/128² golden inputs all yield histograms well under the threshold.
      Node root;
      var colorsCount = 0;
      if (histArray.Length >= _ParallelBuildThreshold)
        root = _BuildTreeParallel(histArray, ref colorsCount);
      else {
        root = new Node();
        // Branch on TWork once outside the inner loop. The JIT folds the typeof comparison.
        if (typeof(TWork) == typeof(Bgra8888))
          for (var i = 0; i < histArray.Length; ++i)
            _AddColorFast32bpp4ch<BgraLayout>(root, histArray[i].color, histArray[i].count, ref colorsCount);
        else
          for (var i = 0; i < histArray.Length; ++i)
            _AddColor(root, histArray[i].color, 0, histArray[i].count, ref colorsCount);
      }

      return _MergeAndGeneratePalette(root, (uint)colorCount, ref colorsCount);
    }

    /// <summary>
    /// Minimum unique-colour histogram length for the parallel tree-build path. The opt-in
    /// threshold is intentionally high: octree merge has the same per-node work as the sequential
    /// insertion (one walk per internal node in each sub-tree, then again across all sub-trees in
    /// the merge), so the parallel speedup is bounded by Amdahl on the sequential merge phase and
    /// in measurements on dense photo histograms (~200k-500k unique colours) the parallel path
    /// regresses end-to-end. Above ~1M unique colours the per-thread float-conversion cost in
    /// <c>_AddColor</c> begins to outweigh the merge overhead and the parallel path catches up.
    /// The threshold keeps all goldens (max histogram ~16k unique colours from 64²/96²/128² inputs)
    /// on the sequential path so they remain bit-exact, and avoids regressing typical photo
    /// workloads while leaving the parallel path available for genuinely extreme inputs.
    /// </summary>
    private const int _ParallelBuildThreshold = 1 << 20; // 1,048,576 unique colours

    /// <summary>
    /// Parallel tree-build. Partitions the histogram into chunks, builds a private sub-tree
    /// per chunk, then merges them deterministically into a master tree in partition-index order.
    /// Sub-tree merge walks both trees together adding accumulators; ulong sums are associatively
    /// associative (bit-exact) and double sums merge in fixed partition order so the result is
    /// reproducible across runs. The downstream <see cref="_MergeAndGeneratePalette"/> reduction
    /// then runs once on the merged master.
    /// </summary>
    private static Node _BuildTreeParallel((TWork color, uint count)[] histogram, ref int colorsCount) {
      var total = histogram.Length;
      var partitionCount = Math.Min(Environment.ProcessorCount, Math.Max(2, total / 16384));
      var chunkSize = (total + partitionCount - 1) / partitionCount;
      var subTrees = new Node[partitionCount];

      // Per-thread leaf counts are recomputed during the merge (the merge replays the
      // _SetupColor first-touch accounting onto the master), so the local count from each
      // sub-tree build can be discarded. Tracking it explicitly would require summing
      // overlapping leaf positions twice.
      Parallel.For(0, partitionCount, p => {
        var start = p * chunkSize;
        var end = Math.Min(start + chunkSize, total);
        var local = new Node();
        var localColorsCount = 0;
        if (typeof(TWork) == typeof(Bgra8888))
          for (var i = start; i < end; ++i)
            _AddColorFast32bpp4ch<BgraLayout>(local, histogram[i].color, histogram[i].count, ref localColorsCount);
        else
          for (var i = start; i < end; ++i)
            _AddColor(local, histogram[i].color, 0, histogram[i].count, ref localColorsCount);
        subTrees[p] = local;
      });

      // Deterministic in-order merge.
      var master = new Node();
      var mergedColorsCount = 0;
      for (var p = 0; p < partitionCount; ++p)
        _MergeSubTreeInto(master, subTrees[p], ref mergedColorsCount);

      colorsCount = mergedColorsCount;
      return master;
    }

    /// <summary>
    /// Merges <paramref name="src"/> into <paramref name="dst"/> by walking matching positions
    /// and adding accumulators. <paramref name="colorsCount"/> tracks live leaves on the merged
    /// tree using the same accounting rule as <see cref="_SetupColor"/>: a position counts as a
    /// new leaf the first time it accumulates non-zero pixels in the master.
    /// </summary>
    private static void _MergeSubTreeInto(Node dst, Node src, ref int colorsCount) {
      // Internal-node accumulators (ReferencesCount, ChildrenCount) and the descent path are
      // recreated on the master from the sub-tree's structure. Leaf detection mirrors
      // _SetupColor: a leaf in the source tree contributes a new master leaf only when it lands
      // on a master node that had zero references prior to this merge step.
      // We distinguish leaf vs internal by ChildrenCount==0 in the source — at a leaf, the source
      // has accumulated PixelCount and the float sums; at an internal node, ChildrenCount > 0.
      var srcIsLeaf = src.ChildrenCount == 0 && src.PixelCount > 0;
      if (srcIsLeaf) {
        // Leaf — replicate _SetupColor onto the master node.
        if (dst.ReferencesCount == 0)
          ++colorsCount;

        dst.ReferencesCount += src.ReferencesCount;
        dst.PixelCount += src.PixelCount;
        dst.C1Sum += src.C1Sum;
        dst.C2Sum += src.C2Sum;
        dst.C3Sum += src.C3Sum;
        dst.ASum += src.ASum;
        return;
      }

      // Internal node — accumulate ReferencesCount on the master and recurse into children.
      dst.ReferencesCount += src.ReferencesCount;
      for (var i = 0; i < src.Children.Length; ++i) {
        var srcChild = src.Children[i];
        if (srcChild == null)
          continue;

        if (dst.Children[i] == null) {
          dst.Children[i] = new Node();
          ++dst.ChildrenCount;
        }
        _MergeSubTreeInto(dst.Children[i]!, srcChild, ref colorsCount);
      }
    }

    private static void _AddColor(Node node, TWork color, int level, ulong pixelCount, ref int colorsCount) {
      var (c1, c2, c3, a) = color.ToNormalized();
      var c1Byte = c1.ToByte();
      var c2Byte = c2.ToByte();
      var c3Byte = c3.ToByte();

      for (;;) {
        if (level >= _MAX_DEPTH) {
          _SetupColor(node, c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat(), pixelCount, ref colorsCount);
          return;
        }

        var index =
          (((c1Byte >> (7 - level)) & 1) << 2) |
          (((c2Byte >> (7 - level)) & 1) << 1) |
          ((c3Byte >> (7 - level)) & 1);

        ++node.ReferencesCount;

        if (node.Children[index] == null) {
          node.Children[index] = new Node();
          ++node.ChildrenCount;
        }

        node = node.Children[index]!;
        ++level;
      }
    }

    /// <summary>
    /// 32bpp 4-channel fast path. The tree-descent already operates on bytes so the
    /// only float-conversion saving is at the leaf <see cref="_SetupColor"/> call. We read
    /// channel bytes directly via the layout descriptor and compute leaf floats from those bytes
    /// using the same UNorm32-derived arithmetic as <c>UNorm32.FromByte(b).ToFloat()</c>, so leaf
    /// accumulation values are bit-exact with the slow path.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _AddColorFast32bpp4ch<TLayout>(Node root, TWork color, ulong pixelCount, ref int colorsCount)
      where TLayout : struct {
      var packed = Unsafe.As<TWork, uint>(ref color);
      var (c1Byte, c2Byte, c3Byte, aByte) = StorageLayoutFast.UnpackBytes<TLayout>(packed);

      // Leaf-level floats — only computed if/when we descend to MaxDepth.
      // Float arithmetic identical to UNorm32.FromByte(b).ToFloat().
      const float unormToFloat = 1f / uint.MaxValue;

      var node = root;
      var level = 0;
      for (;;) {
        if (level >= _MAX_DEPTH) {
          var c1F = (float)((uint)c1Byte * 0x01010101u) * unormToFloat;
          var c2F = (float)((uint)c2Byte * 0x01010101u) * unormToFloat;
          var c3F = (float)((uint)c3Byte * 0x01010101u) * unormToFloat;
          var aF = (float)((uint)aByte * 0x01010101u) * unormToFloat;
          _SetupColor(node, c1F, c2F, c3F, aF, pixelCount, ref colorsCount);
          return;
        }

        var index =
          (((c1Byte >> (7 - level)) & 1) << 2) |
          (((c2Byte >> (7 - level)) & 1) << 1) |
          ((c3Byte >> (7 - level)) & 1);

        ++node.ReferencesCount;

        if (node.Children[index] == null) {
          node.Children[index] = new Node();
          ++node.ChildrenCount;
        }

        node = node.Children[index]!;
        ++level;
      }
    }

    private static void _SetupColor(Node node, float c1, float c2, float c3, float a, ulong pixelCount, ref int colorsCount) {
      colorsCount += node.ReferencesCount == 0 ? 1 : 0;
      ++node.ReferencesCount;
      node.PixelCount += pixelCount;
      node.C1Sum += c1 * pixelCount;
      node.C2Sum += c2 * pixelCount;
      node.C3Sum += c3 * pixelCount;
      node.ASum += a * pixelCount;
    }

    private static TWork[] _MergeAndGeneratePalette(Node root, uint desiredColors, ref int colorsCount) {
      var minimumReferenceCount = uint.MaxValue;
      _GetMinimumReferenceCount(root, ref minimumReferenceCount);
      var least = minimumReferenceCount;

      while (colorsCount > desiredColors) {
        _MergeLeast(root, least, desiredColors, ref colorsCount);
        least += minimumReferenceCount;
      }

      var result = new TWork[colorsCount];
      var index = 0;
      _FillPalette(root, result, ref index);

      return result;
    }

    private static void _GetMinimumReferenceCount(Node currentNode, ref uint minimumReferenceCount) {
      if (currentNode.ReferencesCount < minimumReferenceCount)
        minimumReferenceCount = (uint)currentNode.ReferencesCount;

      foreach (var childNode in currentNode.Children)
        if (childNode != null)
          _GetMinimumReferenceCount(childNode, ref minimumReferenceCount);
    }

    private static void _MergeLeast(Node currentNode, uint minCount, uint maxColors, ref int colorsCount) {
      if (currentNode.ReferencesCount > minCount || colorsCount <= maxColors)
        return;

      for (var i = 0; i < currentNode.Children.Length; ++i) {
        var childNode = currentNode.Children[i];
        switch (childNode) {
          case null:
            continue;
          case { ChildrenCount: > 0 }:
            _MergeLeast(childNode, minCount, maxColors, ref colorsCount);
            continue;
        }

        if (currentNode.ReferencesCount > minCount)
          continue;

        currentNode.PixelCount += childNode.PixelCount;
        currentNode.C1Sum += childNode.C1Sum;
        currentNode.C2Sum += childNode.C2Sum;
        currentNode.C3Sum += childNode.C3Sum;
        currentNode.ASum += childNode.ASum;

        currentNode.Children[i] = null;
        --currentNode.ChildrenCount;
        --colorsCount;

        if (colorsCount >= maxColors)
          continue;

        currentNode.Children[i] = new() {
          C1Sum = currentNode.C1Sum,
          C2Sum = currentNode.C2Sum,
          C3Sum = currentNode.C3Sum,
          ASum = currentNode.ASum,
          PixelCount = currentNode.PixelCount
        };

        ++currentNode.ChildrenCount;
        ++colorsCount;
        break;
      }

      if (currentNode.ChildrenCount == 0)
        ++colorsCount;
    }

    private static void _FillPalette(Node currentNode, TWork[] palette, ref int index) {
      if (currentNode is { ChildrenCount: 0, PixelCount: > 0 })
        palette[index++] = currentNode.CreateColor<TWork>();

      foreach (var childNode in currentNode.Children)
        if (childNode != null)
          _FillPalette(childNode, palette, ref index);
    }

    private sealed class Node {
      public Node?[] Children { get; } = new Node?[8];
      public int ChildrenCount { get; set; }
      public double C1Sum { get; set; }
      public double C2Sum { get; set; }
      public double C3Sum { get; set; }
      public double ASum { get; set; }
      public ulong ReferencesCount { get; set; }
      public ulong PixelCount { get; set; }

      public T CreateColor<T>() where T : unmanaged, IColorSpace4<T> {
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
