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
using System.Drawing;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Implements octree-based color quantization.
/// Builds a tree where each node represents a region of RGB color space,
/// then merges leaves with lowest reference counts to reduce palette size.
/// </summary>
/// <remarks>
/// <para>Reference: M. Gervautz, W. Purgathofer 1988 "A Simple Method for Color Quantization: Octree Quantization"</para>
/// <para>Graphics Gems, Springer, Berlin, Heidelberg, pp. 219-231</para>
/// <para>See also: https://www.cubic.org/docs/octree.htm</para>
/// </remarks>
[Quantizer(QuantizationType.Tree, DisplayName = "Octree", Year = 1988, QualityRating = 7)]
public class OctreeQuantizer : QuantizerBase {

  private const int _MAX_DEPTH = 7;

  /// <inheritdoc />
  protected override Color[] _ReduceColorsTo(int colorCount, IEnumerable<(Color color, uint count)> histogram) {
    var root = new Node();
    var colorsCount = 0;

    foreach (var (color, count) in histogram)
      _AddColor(root, color, 0, count, ref colorsCount);

    return _MergeAndGeneratePalette(root, (uint)colorCount, ref colorsCount);
  }

  private static void _AddColor(Node node, Color color, int level, ulong pixelCount, ref int colorsCount) {
    for (;;) {
      if (level >= _MAX_DEPTH) {
        _SetupColor(node, color, pixelCount, ref colorsCount);
        return;
      }

      var index =
        (((color.R >> (7 - level)) & 1) << 2) |
        (((color.G >> (7 - level)) & 1) << 1) |
        ((color.B >> (7 - level)) & 1);

      ++node.ReferencesCount;

      if (node.Children[index] == null) {
        node.Children[index] = new Node();
        ++node.ChildrenCount;
      }

      node = node.Children[index]!;
      ++level;
    }
  }

  private static void _SetupColor(Node node, Color color, ulong pixelCount, ref int colorsCount) {
    colorsCount += node.ReferencesCount == 0 ? 1 : 0;
    ++node.ReferencesCount;
    ++node.PixelCount;
    node.RSum += color.R;
    node.GSum += color.G;
    node.BSum += color.B;
  }

  private static Color[] _MergeAndGeneratePalette(Node root, uint desiredColors, ref int colorsCount) {
    var minimumReferenceCount = uint.MaxValue;
    _GetMinimumReferenceCount(root, ref minimumReferenceCount);
    var least = minimumReferenceCount;

    while (colorsCount > desiredColors) {
      _MergeLeast(root, least, desiredColors, ref colorsCount);
      least += minimumReferenceCount;
    }

    var result = new Color[colorsCount];
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
      currentNode.RSum += childNode.RSum;
      currentNode.GSum += childNode.GSum;
      currentNode.BSum += childNode.BSum;

      currentNode.Children[i] = null;
      --currentNode.ChildrenCount;
      --colorsCount;

      if (colorsCount >= maxColors)
        continue;

      currentNode.Children[i] = new() {
        RSum = currentNode.RSum,
        GSum = currentNode.GSum,
        BSum = currentNode.BSum,
        PixelCount = currentNode.PixelCount
      };

      ++currentNode.ChildrenCount;
      ++colorsCount;
      break;
    }

    if (currentNode.ChildrenCount == 0)
      ++colorsCount;
  }

  private static void _FillPalette(Node currentNode, Color[] palette, ref int index) {
    if (currentNode is { ChildrenCount: 0, PixelCount: > 0 })
      palette[index++] = currentNode.CreateColor();

    foreach (var childNode in currentNode.Children)
      if (childNode != null)
        _FillPalette(childNode, palette, ref index);
  }

  private sealed class Node {
    public Node?[] Children { get; } = new Node?[8];
    public int ChildrenCount { get; set; }
    public ulong RSum { get; set; }
    public ulong GSum { get; set; }
    public ulong BSum { get; set; }
    public ulong ReferencesCount { get; set; }
    public ulong PixelCount { get; set; }

    public Color CreateColor() {
      var r = (double)this.RSum / this.PixelCount;
      var g = (double)this.GSum / this.PixelCount;
      var b = (double)this.BSum / this.PixelCount;
      return Color.FromArgb((byte)Math.Round(r), (byte)Math.Round(g), (byte)Math.Round(b));
    }
  }

}
