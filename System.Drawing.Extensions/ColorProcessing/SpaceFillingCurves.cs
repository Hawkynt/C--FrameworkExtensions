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
// <https://github.com/Hawkynt+C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System;
using System.Collections.Generic;

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Pure utility generators for 2D space-filling-curve traversal orders.
/// Given a rectangular region, each method returns the pixel coordinates in the
/// order dictated by the curve — useful for dithering, imaging,
/// low-discrepancy sampling, cache-friendly pixel access, and compression.
/// </summary>
/// <remarks>
/// These algorithms were previously private to <see cref="Dithering.RiemersmaDitherer"/>;
/// they are exposed here so any quantizer, filter, rescaler, or custom tool can
/// use the same traversal primitives.
/// </remarks>
public static class SpaceFillingCurves {

  /// <summary>Largest Hilbert order we accept (2^7 = 128 covers most real images).</summary>
  public const int MaxHilbertOrder = 7;

  /// <summary>Largest Peano order we accept (3^5 = 243 covers most real images).</summary>
  public const int MaxPeanoOrder = 5;

  /// <summary>
  /// Generates a Hilbert-curve traversal of a <paramref name="width"/> × <paramref name="height"/>
  /// region starting at row <paramref name="startY"/>.
  /// </summary>
  /// <param name="width">Region width in pixels (exclusive X bound).</param>
  /// <param name="height">Region height in pixels.</param>
  /// <param name="startY">First row included in the traversal (inclusive).</param>
  /// <param name="order">
  /// Optional curve order (1..<see cref="MaxHilbertOrder"/>). Order <c>n</c> covers
  /// <c>2ⁿ × 2ⁿ</c>; if <see langword="null"/>, the smallest order covering the region is used.
  /// </param>
  /// <returns>
  /// The list of <c>(x, y)</c> pixel coordinates visited by the curve, restricted to
  /// points inside the <c>[0, width) × [startY, startY + height)</c> rectangle.
  /// </returns>
  public static List<(int x, int y)> Hilbert(int width, int height, int startY = 0, int? order = null) {
    var result = new List<(int, int)>(Math.Max(0, width * height));
    var endY = startY + height;
    if (width <= 0 || height <= 0)
      return result;

    int n;
    if (order.HasValue) {
      var clampedOrder = Math.Max(1, Math.Min(MaxHilbertOrder, order.Value));
      n = 1 << clampedOrder;
    } else {
      n = 1;
      while (n < Math.Max(width, endY))
        n *= 2;
    }

    var totalPoints = n * n;
    for (var i = 0; i < totalPoints; ++i) {
      var (x, y) = _HilbertIndexToXY(i, n);
      if (x < width && y >= startY && y < endY)
        result.Add((x, y));
    }
    return result;
  }

  /// <summary>
  /// Generates a Peano-curve traversal of a <paramref name="width"/> × <paramref name="height"/>
  /// region starting at row <paramref name="startY"/>.
  /// </summary>
  /// <param name="order">Optional order (1..<see cref="MaxPeanoOrder"/>). Each order covers <c>3ⁿ × 3ⁿ</c>.</param>
  public static List<(int x, int y)> Peano(int width, int height, int startY = 0, int? order = null) {
    var result = new List<(int, int)>(Math.Max(0, width * height));
    var endY = startY + height;
    if (width <= 0 || height <= 0)
      return result;

    int peanoOrder;
    int n;
    if (order.HasValue) {
      peanoOrder = Math.Max(1, Math.Min(MaxPeanoOrder, order.Value));
      n = 1;
      for (var k = 0; k < peanoOrder; ++k) n *= 3;
    } else {
      peanoOrder = 1;
      n = 3;
      while (n < Math.Max(width, endY) && peanoOrder < MaxPeanoOrder) {
        n *= 3;
        ++peanoOrder;
      }
    }

    var totalPoints = n * n;
    for (var i = 0; i < totalPoints; ++i) {
      var (x, y) = _PeanoIndexToXY(i, peanoOrder);
      if (x < width && y >= startY && y < endY)
        result.Add((x, y));
    }
    return result;
  }

  /// <summary>
  /// Generates a serpentine linear traversal (left-to-right on even rows,
  /// right-to-left on odd rows). Cheaper than Hilbert/Peano; still preserves
  /// row-to-row spatial locality.
  /// </summary>
  public static List<(int x, int y)> LinearSerpentine(int width, int height, int startY = 0) {
    var result = new List<(int, int)>(Math.Max(0, width * height));
    var endY = startY + height;
    for (var y = startY; y < endY; ++y)
      if ((y & 1) == 0)
        for (var x = 0; x < width; ++x)
          result.Add((x, y));
      else
        for (var x = width - 1; x >= 0; --x)
          result.Add((x, y));
    return result;
  }

  #region Hilbert internals

  private static (int x, int y) _HilbertIndexToXY(int index, int n) {
    int x = 0, y = 0;
    var t = index;
    var s = 1;
    while (s < n) {
      var rx = 1 & (t / 2);
      var ry = 1 & (t ^ rx);
      (x, y) = _HilbertRot(s, x, y, rx, ry);
      x += s * rx;
      y += s * ry;
      t /= 4;
      s *= 2;
    }
    return (x, y);
  }

  private static (int x, int y) _HilbertRot(int n, int x, int y, int rx, int ry) {
    if (ry == 0) {
      if (rx == 1) {
        x = n - 1 - x;
        y = n - 1 - y;
      }
      (x, y) = (y, x);
    }
    return (x, y);
  }

  #endregion

  #region Peano internals

  /// <summary>
  /// Maps a linear index <paramref name="index"/> ∈ [0, 9^<paramref name="order"/>) to
  /// (x, y) on a 3^order × 3^order Peano curve.
  /// </summary>
  /// <remarks>
  /// <para>Construction: column-snake at each recursive level. At each level we extract
  /// a base-9 digit, decompose into (a, b) where a is the column index and b is the
  /// row-within-column. The "snake" traverses col 0 bottom-to-top, col 1 top-to-bottom,
  /// col 2 bottom-to-top — implemented as <c>row = (a even) ? b : 2 − b</c>.</para>
  /// <para>Continuity at level boundaries requires flip propagation: when entering a
  /// sub-cell at the middle row (b_eff is 1) the inner sub-curve must be horizontally
  /// mirrored, and when entering the middle column (a is 1) it must be vertically
  /// mirrored. These flips toggle through levels (XOR-style) so the orientation
  /// propagates correctly to the deepest cells. Verified to satisfy both visit-once
  /// AND Manhattan-adjacency for orders 1 through <see cref="MaxPeanoOrder"/>.</para>
  /// <para>Reference: column-snake Peano curve construction. The flip-propagation rule
  /// is a discrete analog of the Peano curve's rotational symmetry described in Sagan
  /// 1994, "Space-Filling Curves", Springer, ch. 3.</para>
  /// </remarks>
  private static (int x, int y) _PeanoIndexToXY(int index, int order) {
    int x = 0, y = 0;
    var flipX = false;
    var flipY = false;

    for (var level = order - 1; level >= 0; --level) {
      var subSize = 1;
      for (var k = 0; k < level; ++k) subSize *= 9;
      var side = 1;
      for (var k = 0; k < level; ++k) side *= 3;

      var digit = (index / subSize) % 9;
      var a = digit / 3;
      var b = digit % 3;

      var col = a;
      var row = ((a & 1) == 0) ? b : 2 - b;

      if (flipX) col = 2 - col;
      if (flipY) row = 2 - row;

      x += col * side;
      y += row * side;

      var bEff = ((a & 1) == 0) ? b : 2 - b;
      if ((bEff & 1) == 1) flipX = !flipX;
      if ((a & 1) == 1) flipY = !flipY;
    }

    return (x, y);
  }

  private static void _PeanoRecursive(
    List<(int, int)> result,
    int x, int y,
    int ax, int ay,
    int bx, int by,
    int maxWidth, int minY, int maxY) {

    var w = Math.Abs(ax + ay);
    var h = Math.Abs(bx + by);

    var dax = ax > 0 ? 1 : ax < 0 ? -1 : 0;
    var day = ay > 0 ? 1 : ay < 0 ? -1 : 0;
    var dbx = bx > 0 ? 1 : bx < 0 ? -1 : 0;
    var dby = by > 0 ? 1 : by < 0 ? -1 : 0;

    if (w == 1 && h == 1) {
      if (x >= 0 && x < maxWidth && y >= minY && y < maxY)
        result.Add((x, y));
      return;
    }

    var ax2 = ax / 3;
    var ay2 = ay / 3;
    var bx2 = bx / 3;
    var by2 = by / 3;

    if (w == 2) {
      for (var i = 0; i < h; ++i) {
        var py = y + i * dby + i * dbx;
        var px1 = x;
        var px2 = x + dax + day;
        if ((i & 1) == 0) {
          if (px1 >= 0 && px1 < maxWidth && py >= minY && py < maxY) result.Add((px1, py));
          if (px2 >= 0 && px2 < maxWidth && py >= minY && py < maxY) result.Add((px2, py));
        } else {
          if (px2 >= 0 && px2 < maxWidth && py >= minY && py < maxY) result.Add((px2, py));
          if (px1 >= 0 && px1 < maxWidth && py >= minY && py < maxY) result.Add((px1, py));
        }
      }
      return;
    }

    if (h == 2) {
      for (var i = 0; i < w; ++i) {
        var px = x + i * dax + i * day;
        var py1 = y;
        var py2 = y + dbx + dby;
        if ((i & 1) == 0) {
          if (px >= 0 && px < maxWidth && py1 >= minY && py1 < maxY) result.Add((px, py1));
          if (px >= 0 && px < maxWidth && py2 >= minY && py2 < maxY) result.Add((px, py2));
        } else {
          if (px >= 0 && px < maxWidth && py2 >= minY && py2 < maxY) result.Add((px, py2));
          if (px >= 0 && px < maxWidth && py1 >= minY && py1 < maxY) result.Add((px, py1));
        }
      }
      return;
    }

    _PeanoRecursive(result, x, y, ax2, ay2, bx2, by2, maxWidth, minY, maxY);
    _PeanoRecursive(result, x + bx2, y + by2, ax2, ay2, bx2, by2, maxWidth, minY, maxY);
    _PeanoRecursive(result, x + 2 * bx2, y + 2 * by2, ax2, ay2, bx2, by2, maxWidth, minY, maxY);

    _PeanoRecursive(result, x + ax2 + 2 * bx2, y + ay2 + 2 * by2, ax2, ay2, -bx2, -by2, maxWidth, minY, maxY);
    _PeanoRecursive(result, x + ax2 + bx2, y + ay2 + by2, ax2, ay2, -bx2, -by2, maxWidth, minY, maxY);
    _PeanoRecursive(result, x + ax2, y + ay2, ax2, ay2, -bx2, -by2, maxWidth, minY, maxY);

    _PeanoRecursive(result, x + 2 * ax2, y + 2 * ay2, ax2, ay2, bx2, by2, maxWidth, minY, maxY);
    _PeanoRecursive(result, x + 2 * ax2 + bx2, y + 2 * ay2 + by2, ax2, ay2, bx2, by2, maxWidth, minY, maxY);
    _PeanoRecursive(result, x + 2 * ax2 + 2 * bx2, y + 2 * ay2 + 2 * by2, ax2, ay2, bx2, by2, maxWidth, minY, maxY);
  }

  #endregion
}
