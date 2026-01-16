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
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;

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

  /// <summary>
  /// Gets or sets whether to fill unused palette entries with generated colors.
  /// </summary>
  public bool AllowFillingColors { get; set; } = true;

  public OctreeQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.AllowFillingColors);

  internal sealed class Kernel<TWork>(bool allowFillingColors) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private const int _MAX_DEPTH = 7;

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      var result = QuantizerHelper.TryHandleSimpleCases(histogram, colorCount, allowFillingColors, out var used);
      if (result != null)
        return result;

      var reduced = this._ReduceColorsTo(colorCount, used);
      return PaletteFiller.GenerateFinalPalette(reduced, colorCount, allowFillingColors);
    }

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var root = new Node();
      var colorsCount = 0;

      foreach (var (color, count) in histogram)
        _AddColor(root, color, 0, count, ref colorsCount);

      return _MergeAndGeneratePalette(root, (uint)colorCount, ref colorsCount);
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

    private static void _SetupColor(Node node, float c1, float c2, float c3, float a, ulong pixelCount, ref int colorsCount) {
      colorsCount += node.ReferencesCount == 0 ? 1 : 0;
      ++node.ReferencesCount;
      ++node.PixelCount;
      node.C1Sum += c1;
      node.C2Sum += c2;
      node.C3Sum += c3;
      node.ASum += a;
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
