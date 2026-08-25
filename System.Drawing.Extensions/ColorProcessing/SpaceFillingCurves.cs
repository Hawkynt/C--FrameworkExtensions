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

  /// <summary>Largest explicitly requested Hilbert order (2^7 = 128 pixels per side).</summary>
  public const int MaxHilbertOrder = 7;

  /// <summary>Largest explicitly requested Moore order (2^7 = 128 pixels per side).</summary>
  public const int MaxMooreOrder = 7;

  /// <summary>Largest Peano-family order we accept (3^5 = 243 pixels per side).</summary>
  public const int MaxPeanoOrder = 5;

  /// <summary>Largest Coil order we accept.</summary>
  public const int MaxCoilOrder = MaxPeanoOrder;

  /// <summary>Largest Half-coil order we accept.</summary>
  public const int MaxHalfCoilOrder = MaxPeanoOrder;

  /// <summary>Largest Meurthe order we accept.</summary>
  public const int MaxMeurtheOrder = MaxPeanoOrder;

  /// <summary>Largest explicitly requested Morton order (2^7 = 128 pixels per side).</summary>
  public const int MaxMortonOrder = 7;

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
  /// Generates a Moore-curve traversal. Moore is the closed-loop relative of Hilbert:
  /// complete <c>2ⁿ × 2ⁿ</c> domains have unit Manhattan steps and the final point is
  /// adjacent to the first one. Rectangles are covered by clipping the enclosing Moore square.
  /// </summary>
  /// <param name="order">Optional order (1..<see cref="MaxMooreOrder"/>). Each order covers <c>2ⁿ × 2ⁿ</c>.</param>
  public static List<(int x, int y)> Moore(int width, int height, int startY = 0, int? order = null) {
    var result = new List<(int, int)>(Math.Max(0, width * height));
    var endY = startY + height;
    if (width <= 0 || height <= 0)
      return result;

    int curveOrder;
    if (order.HasValue)
      curveOrder = Math.Max(1, Math.Min(MaxMooreOrder, order.Value));
    else {
      curveOrder = 1;
      var side = 2;
      while (side < Math.Max(width, endY)) {
        side *= 2;
        ++curveOrder;
      }
    }

    // Moore's L-system is traced twice. The first pass determines the translation
    // needed to normalize the turtle coordinates without allocating a second full
    // curve-sized list; the second pass emits only pixels inside the requested region.
    int minX = 0, minY = 0;
    _TraceMoore(curveOrder, null, 0, 0, 0, 0, ref minX, ref minY);
    _TraceMoore(curveOrder, result, -minX, -minY, width, endY, ref minX, ref minY, startY);
    return result;
  }

  /// <summary>
  /// Generates a generalized Hilbert ("Gilbert") traversal that directly fills an arbitrary
  /// rectangular raster instead of generating a square and clipping it.
  /// </summary>
  /// <remarks>
  /// Consecutive points are normally orthogonal neighbors. Some odd/even dimension combinations
  /// require a single diagonal transition; this is inherent to Hamiltonian paths on those rectangles.
  /// Based on Jakub Červený's generalized Hilbert construction (BSD-2-Clause).
  /// </remarks>
  public static List<(int x, int y)> Gilbert(int width, int height, int startY = 0) {
    var result = new List<(int, int)>(Math.Max(0, width * height));
    if (width <= 0 || height <= 0)
      return result;

    if (width >= height)
      _GilbertRecursive(result, 0, startY, width, 0, 0, height);
    else
      _GilbertRecursive(result, 0, startY, 0, height, width, 0);

    return result;
  }

  /// <summary>
  /// Generates the classical Peano traversal using recursive 3×3 subdivision.
  /// </summary>
  /// <param name="order">Optional order (1..<see cref="MaxPeanoOrder"/>). Each order covers <c>3ⁿ × 3ⁿ</c>.</param>
  public static List<(int x, int y)> Peano(int width, int height, int startY = 0, int? order = null)
    => _TernaryCurve(width, height, startY, order, _TernaryCurveType.Peano)
    ;

  /// <summary>
  /// Generates Haverkort's Coil traversal, a continuous 3×3 Peano-family curve
  /// that swaps the coordinate axes in every recursive subcell.
  /// </summary>
  /// <param name="order">Optional order (1..<see cref="MaxCoilOrder"/>). Each order covers <c>3ⁿ × 3ⁿ</c>.</param>
  public static List<(int x, int y)> Coil(int width, int height, int startY = 0, int? order = null)
    => _TernaryCurve(width, height, startY, order, _TernaryCurveType.Coil)
    ;

  /// <summary>
  /// Generates Haverkort's Half-coil traversal, alternating between Peano-like
  /// and Coil-like recursive orientation according to the subcell rank.
  /// </summary>
  /// <param name="order">Optional order (1..<see cref="MaxHalfCoilOrder"/>). Each order covers <c>3ⁿ × 3ⁿ</c>.</param>
  public static List<(int x, int y)> HalfCoil(int width, int height, int startY = 0, int? order = null)
    => _TernaryCurve(width, height, startY, order, _TernaryCurveType.HalfCoil)
    ;

  /// <summary>
  /// Generates Haverkort's Meurthe traversal, a continuous 3×3 curve with
  /// neutral orientation over recursive pieces.
  /// </summary>
  /// <param name="order">Optional order (1..<see cref="MaxMeurtheOrder"/>). Each order covers <c>3ⁿ × 3ⁿ</c>.</param>
  public static List<(int x, int y)> Meurthe(int width, int height, int startY = 0, int? order = null)
    => _TernaryCurve(width, height, startY, order, _TernaryCurveType.Meurthe)
    ;

  /// <summary>
  /// Generates Morton/Z-order by recursively visiting quadtree cells in bit-interleaving order.
  /// Hierarchical locality is preserved, but consecutive points are not guaranteed to be neighbors.
  /// </summary>
  /// <param name="order">Optional order (1..<see cref="MaxMortonOrder"/>). Each order covers <c>2ⁿ × 2ⁿ</c>.</param>
  public static List<(int x, int y)> Morton(int width, int height, int startY = 0, int? order = null) {
    var result = new List<(int, int)>(Math.Max(0, width * height));
    var endY = startY + height;
    if (width <= 0 || height <= 0)
      return result;

    int side;
    if (order.HasValue) {
      var clampedOrder = Math.Max(1, Math.Min(MaxMortonOrder, order.Value));
      side = 1 << clampedOrder;
    } else {
      side = 1;
      while (side < Math.Max(width, endY))
        side *= 2;
    }

    _MortonRecursive(result, 0, 0, side, width, startY, endY);
    return result;
  }

  /// <summary>
  /// Generates a clockwise inward spiral over an arbitrary rectangle.
  /// Every consecutive pixel is 4-connected.
  /// </summary>
  public static List<(int x, int y)> Spiral(int width, int height, int startY = 0) {
    var result = new List<(int, int)>(Math.Max(0, width * height));
    if (width <= 0 || height <= 0)
      return result;

    var left = 0;
    var right = width - 1;
    var top = startY;
    var bottom = startY + height - 1;

    while (left <= right && top <= bottom) {
      for (var x = left; x <= right; ++x)
        result.Add((x, top));
      ++top;

      for (var y = top; y <= bottom; ++y)
        result.Add((right, y));
      --right;

      if (top <= bottom) {
        for (var x = right; x >= left; --x)
          result.Add((x, bottom));
        --bottom;
      }

      if (left <= right) {
        for (var y = bottom; y >= top; --y)
          result.Add((left, y));
        ++left;
      }
    }

    return result;
  }

  /// <summary>
  /// Generates a zig-zag traversal over successive <c>x + y</c> diagonals.
  /// This reduces horizontal scan bias but uses diagonal steps within each diagonal.
  /// </summary>
  public static List<(int x, int y)> DiagonalSerpentine(int width, int height, int startY = 0) {
    var result = new List<(int, int)>(Math.Max(0, width * height));
    if (width <= 0 || height <= 0)
      return result;

    var lastSum = width + height - 2;
    for (var sum = 0; sum <= lastSum; ++sum) {
      var minX = Math.Max(0, sum - (height - 1));
      var maxX = Math.Min(width - 1, sum);

      if ((sum & 1) == 0)
        for (var x = maxX; x >= minX; --x)
          result.Add((x, startY + sum - x));
      else
        for (var x = minX; x <= maxX; ++x)
          result.Add((x, startY + sum - x));
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

  #region Moore internals

  private struct _MooreState {
    public List<(int, int)>? Result;
    public int X;
    public int Y;
    public int Direction;
    public int OffsetX;
    public int OffsetY;
    public int MaxWidth;
    public int MinYFilter;
    public int MaxY;
    public int MinX;
    public int MinY;
  }

  private static void _TraceMoore(
    int order,
    List<(int, int)>? result,
    int offsetX,
    int offsetY,
    int maxWidth,
    int maxY,
    ref int minX,
    ref int minY,
    int minYFilter = int.MinValue) {

    var state = new _MooreState {
      Result = result,
      OffsetX = offsetX,
      OffsetY = offsetY,
      MaxWidth = maxWidth,
      MinYFilter = minYFilter,
      MaxY = maxY,
      MinX = minX,
      MinY = minY
    };

    if (result != null)
      _AddMoorePoint(ref state);

    var depth = order - 1;
    _MooreL(depth, ref state);
    _MooreForward(ref state);
    _MooreL(depth, ref state);
    _MooreTurn(ref state, 1);
    _MooreForward(ref state);
    _MooreTurn(ref state, 1);
    _MooreL(depth, ref state);
    _MooreForward(ref state);
    _MooreL(depth, ref state);

    minX = state.MinX;
    minY = state.MinY;
  }

  private static void _MooreL(int depth, ref _MooreState state) {
    if (depth <= 0)
      return;

    _MooreTurn(ref state, -1);
    _MooreR(depth - 1, ref state);
    _MooreForward(ref state);
    _MooreTurn(ref state, 1);
    _MooreL(depth - 1, ref state);
    _MooreForward(ref state);
    _MooreL(depth - 1, ref state);
    _MooreTurn(ref state, 1);
    _MooreForward(ref state);
    _MooreR(depth - 1, ref state);
    _MooreTurn(ref state, -1);
  }

  private static void _MooreR(int depth, ref _MooreState state) {
    if (depth <= 0)
      return;

    _MooreTurn(ref state, 1);
    _MooreL(depth - 1, ref state);
    _MooreForward(ref state);
    _MooreTurn(ref state, -1);
    _MooreR(depth - 1, ref state);
    _MooreForward(ref state);
    _MooreR(depth - 1, ref state);
    _MooreTurn(ref state, -1);
    _MooreForward(ref state);
    _MooreL(depth - 1, ref state);
    _MooreTurn(ref state, 1);
  }

  private static void _MooreForward(ref _MooreState state) {
    switch (state.Direction & 3) {
      case 0: ++state.X; break;
      case 1: --state.Y; break;
      case 2: --state.X; break;
      default: ++state.Y; break;
    }

    if (state.Result == null) {
      state.MinX = Math.Min(state.MinX, state.X);
      state.MinY = Math.Min(state.MinY, state.Y);
      return;
    }

    _AddMoorePoint(ref state);
  }

  private static void _AddMoorePoint(ref _MooreState state) {
    var x = state.X + state.OffsetX;
    var y = state.Y + state.OffsetY;
    if (x >= 0 && x < state.MaxWidth && y >= state.MinYFilter && y < state.MaxY)
      state.Result!.Add((x, y));
  }

  private static void _MooreTurn(ref _MooreState state, int quarterTurns)
    => state.Direction = (state.Direction + quarterTurns) & 3;

  #endregion

  #region Gilbert internals

  private static void _GilbertRecursive(
    List<(int, int)> result,
    int x,
    int y,
    int ax,
    int ay,
    int bx,
    int by) {

    var w = Math.Abs(ax + ay);
    var h = Math.Abs(bx + by);

    var dax = Math.Sign(ax);
    var day = Math.Sign(ay);
    var dbx = Math.Sign(bx);
    var dby = Math.Sign(by);

    if (h == 1) {
      for (var i = 0; i < w; ++i) {
        result.Add((x, y));
        x += dax;
        y += day;
      }
      return;
    }

    if (w == 1) {
      for (var i = 0; i < h; ++i) {
        result.Add((x, y));
        x += dbx;
        y += dby;
      }
      return;
    }

    // Arithmetic right shift is intentional: unlike integer division, it rounds
    // negative odd vectors toward -infinity, matching the reference construction.
    var ax2 = ax >> 1;
    var ay2 = ay >> 1;
    var bx2 = bx >> 1;
    var by2 = by >> 1;
    var w2 = Math.Abs(ax2 + ay2);
    var h2 = Math.Abs(bx2 + by2);

    if (2 * w > 3 * h) {
      if ((w2 & 1) != 0 && w > 2) {
        ax2 += dax;
        ay2 += day;
      }

      _GilbertRecursive(result, x, y, ax2, ay2, bx, by);
      _GilbertRecursive(result, x + ax2, y + ay2, ax - ax2, ay - ay2, bx, by);
      return;
    }

    if ((h2 & 1) != 0 && h > 2) {
      bx2 += dbx;
      by2 += dby;
    }

    _GilbertRecursive(result, x, y, bx2, by2, ax2, ay2);
    _GilbertRecursive(result, x + bx2, y + by2, ax, ay, bx - bx2, by - by2);
    _GilbertRecursive(
      result,
      x + (ax - dax) + (bx2 - dbx),
      y + (ay - day) + (by2 - dby),
      -bx2,
      -by2,
      -(ax - ax2),
      -(ay - ay2)
    );
  }

  #endregion

  #region Peano-family internals

  private enum _TernaryCurveType {
    Peano,
    Coil,
    HalfCoil,
    Meurthe
  }

  private static List<(int x, int y)> _TernaryCurve(
    int width,
    int height,
    int startY,
    int? order,
    _TernaryCurveType curveType) {

    var result = new List<(int, int)>(Math.Max(0, width * height));
    var endY = startY + height;
    if (width <= 0 || height <= 0)
      return result;

    int curveOrder;
    int side;
    if (order.HasValue) {
      curveOrder = Math.Max(1, Math.Min(MaxPeanoOrder, order.Value));
      side = 1;
      for (var k = 0; k < curveOrder; ++k)
        side *= 3;
    } else {
      curveOrder = 1;
      side = 3;
      while (side < Math.Max(width, endY) && curveOrder < MaxPeanoOrder) {
        side *= 3;
        ++curveOrder;
      }
    }

    var totalPoints = side * side;
    for (var i = 0; i < totalPoints; ++i) {
      var (x, y) = _TernaryIndexToXY(i, curveOrder, curveType);
      if (x < width && y >= startY && y < endY)
        result.Add((x, y));
    }

    return result;
  }

  /// <summary>
  /// Maps a linear index to a 2D 3-regular mono-Wunderlich traversal.
  /// </summary>
  /// <remarks>
  /// This is the two-dimensional specialization of Haverkort's common framework
  /// for Peano, Coil, Half-coil and Meurthe curves. All four use the same ternary
  /// reflected-Gray-code subcell order and reflection state; only the recursive
  /// axis permutation differs. Keeping that distinction as data/state rather than
  /// four bespoke recursions makes the continuity rules explicit and testable.
  /// </remarks>
  private static (int x, int y) _TernaryIndexToXY(int index, int order, _TernaryCurveType curveType) {
    int x = 0, y = 0;
    int axis0 = 0, axis1 = 1;
    var reflectedX = false;
    var reflectedY = false;
    var forward = true;

    for (var level = order - 1; level >= 0; --level) {
      var subSize = 1;
      for (var k = 0; k < level; ++k)
        subSize *= 9;

      var digit = (index / subSize) % 9;
      var rank0 = digit / 3;
      var rank1 = digit % 3;
      var directionBeforeCell = forward;
      int xDigit = 0, yDigit = 0;

      _DecodeTernaryDigit(
        rank0,
        axis0,
        ref reflectedX,
        ref reflectedY,
        ref forward,
        ref xDigit,
        ref yDigit
      );
      _DecodeTernaryDigit(
        rank1,
        axis1,
        ref reflectedX,
        ref reflectedY,
        ref forward,
        ref xDigit,
        ref yDigit
      );

      x = x * 3 + xDigit;
      y = y * 3 + yDigit;

      var swapAxes = false;
      switch (curveType) {
        case _TernaryCurveType.Coil:
          swapAxes = true;
          break;
        case _TernaryCurveType.HalfCoil:
          swapAxes = forward == directionBeforeCell;
          break;
        case _TernaryCurveType.Meurthe:
          // In two dimensions Haverkort's Meurthe inverse-permutation rule
          // reduces to swapping axes exactly when the second rank digit is 0 or 1.
          swapAxes = rank1 != 2;
          break;
      }

      if (swapAxes)
        (axis0, axis1) = (axis1, axis0);
    }

    return (x, y);
  }

  private static void _DecodeTernaryDigit(
    int rank,
    int axis,
    ref bool reflectedX,
    ref bool reflectedY,
    ref bool forward,
    ref int xDigit,
    ref int yDigit) {

    var reflected = axis == 0 ? reflectedX : reflectedY;
    var spatialDigit = reflected == forward ? 2 - rank : rank;
    if (axis == 0)
      xDigit = spatialDigit;
    else
      yDigit = spatialDigit;

    if ((rank & 1) == 0)
      return;

    forward = !forward;
    if (axis == 0)
      reflectedX = !reflectedX;
    else
      reflectedY = !reflectedY;
  }

  #endregion

  #region Morton internals

  private static void _MortonRecursive(
    List<(int, int)> result,
    int x,
    int y,
    int size,
    int maxWidth,
    int minY,
    int maxY) {

    if (x >= maxWidth || y >= maxY || x + size <= 0 || y + size <= minY)
      return;

    if (size == 1) {
      if (x >= 0 && y >= minY)
        result.Add((x, y));
      return;
    }

    var half = size >> 1;
    _MortonRecursive(result, x, y, half, maxWidth, minY, maxY);
    _MortonRecursive(result, x + half, y, half, maxWidth, minY, maxY);
    _MortonRecursive(result, x, y + half, half, maxWidth, minY, maxY);
    _MortonRecursive(result, x + half, y + half, half, maxWidth, minY, maxY);
  }

  #endregion
}
