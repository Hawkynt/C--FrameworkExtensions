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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Types of space-filling curves for image traversal.
/// </summary>
/// <remarks>
/// Space-filling curves visit every point in a 2D region while maintaining locality -
/// nearby points in the curve tend to be nearby in space. This property makes them
/// ideal for error diffusion dithering where error propagation benefits from spatial coherence.
/// </remarks>
public enum SpaceFillingCurve {
  /// <summary>
  /// Hilbert curve - subdivides space into 4 quadrants recursively.
  /// Order range: 1-7. Each order doubles the resolution (order n covers 2^n × 2^n).
  /// </summary>
  /// <remarks>
  /// Reference: D. Hilbert 1891 "Über die stetige Abbildung einer Linie auf ein Flächenstück"
  /// See: https://en.wikipedia.org/wiki/Hilbert_curve
  /// </remarks>
  Hilbert,

  /// <summary>
  /// Peano curve - subdivides space into 9 parts recursively (3×3 grid).
  /// Order range: 1-5. Each order triples the resolution (order n covers 3^n × 3^n).
  /// </summary>
  /// <remarks>
  /// Reference: G. Peano 1890 "Sur une courbe, qui remplit toute une aire plane"
  /// See: https://en.wikipedia.org/wiki/Peano_curve
  /// </remarks>
  Peano,

  /// <summary>
  /// Simple serpentine (boustrophedon) scan - alternating left-to-right and right-to-left rows.
  /// No order parameter needed.
  /// </summary>
  Linear
}

/// <summary>
/// Riemersma dithering using space-filling curves (Hilbert, Peano, or linear).
/// </summary>
/// <remarks>
/// <para>Reference: T. Riemersma 1998 "A Balanced Dithering Technique" C/C++ Users Journal</para>
/// <para>See also: https://www.compuphase.com/riemer.htm</para>
/// <para>Uses exponential decay weights with history buffer along space-filling curve traversal.</para>
/// <para>
/// Space-filling curves provide better error diffusion than simple row scanning by maintaining
/// spatial locality - pixels that are nearby in the traversal order are also nearby in the image.
/// </para>
/// </remarks>
[Ditherer("Riemersma", Description = "Space-filling curve dithering with exponential decay history", Type = DitheringType.Custom, Author = "Thiadmer Riemersma", Year = 1998)]
public readonly struct RiemersmaDitherer : IDitherer {

  private const int _DEFAULT_HISTORY_SIZE = 16;
  private readonly int _historySize;
  private readonly SpaceFillingCurve _curveType;
  private readonly int? _curveOrder;

  /// <summary>Maximum order for Hilbert curve (2^7 = 128 pixels per side).</summary>
  public const int MaxHilbertOrder = 7;

  /// <summary>Maximum order for Peano curve (3^5 = 243 pixels per side).</summary>
  public const int MaxPeanoOrder = 5;

  /// <summary>Pre-configured instance with 16-entry history and Hilbert curve (auto order).</summary>
  public static RiemersmaDitherer Default { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Hilbert);

  /// <summary>Pre-configured instance with 8-entry history (faster, lower quality).</summary>
  public static RiemersmaDitherer Small { get; } = new(8, SpaceFillingCurve.Hilbert);

  /// <summary>Pre-configured instance with 32-entry history (slower, higher quality).</summary>
  public static RiemersmaDitherer Large { get; } = new(32, SpaceFillingCurve.Hilbert);

  /// <summary>Pre-configured instance with linear (serpentine) traversal instead of space-filling curve.</summary>
  public static RiemersmaDitherer LinearScan { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Linear);

  /// <summary>Pre-configured instance with Peano curve traversal (3×3 subdivision).</summary>
  public static RiemersmaDitherer Peano { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Peano);

  /// <summary>
  /// Creates a Riemersma ditherer with specified curve type.
  /// </summary>
  /// <param name="historySize">Size of the error history buffer (typically 8, 16, or 32).</param>
  /// <param name="curveType">Type of space-filling curve to use for traversal.</param>
  /// <param name="curveOrder">
  /// Order/level of the curve (null = auto-calculate based on image size).
  /// For Hilbert: 1-7 (covers 2^n × 2^n pixels). For Peano: 1-5 (covers 3^n × 3^n pixels).
  /// </param>
  public RiemersmaDitherer(int historySize = _DEFAULT_HISTORY_SIZE, SpaceFillingCurve curveType = SpaceFillingCurve.Hilbert, int? curveOrder = null) {
    this._historySize = historySize;
    this._curveType = curveType;
    this._curveOrder = curveOrder;
  }

  /// <summary>
  /// Creates a Riemersma ditherer (legacy constructor for backwards compatibility).
  /// </summary>
  /// <param name="historySize">Size of the error history buffer.</param>
  /// <param name="useHilbertCurve">If true, uses Hilbert curve; otherwise uses linear scan.</param>
  [Obsolete("Use the constructor with SpaceFillingCurve parameter instead.")]
  public RiemersmaDitherer(int historySize, bool useHilbertCurve)
    : this(historySize, useHilbertCurve ? SpaceFillingCurve.Hilbert : SpaceFillingCurve.Linear) { }

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => true;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);

    // Handle default struct initialization (historySize = 0)
    var historySize = this._historySize > 0 ? this._historySize : _DEFAULT_HISTORY_SIZE;

    if (palette.Length == 0)
      return;

    // Error history buffer for exponential decay
    var errorHistory = new (double c1, double c2, double c3)[historySize];
    var historyIndex = 0;

    // Generate traversal order for the specified region based on curve type
    var traversalOrder = this._curveType switch {
      SpaceFillingCurve.Hilbert => _GenerateHilbertCurveOrder(width, height, startY, this._curveOrder),
      SpaceFillingCurve.Peano => _GeneratePeanoCurveOrder(width, height, startY, this._curveOrder),
      SpaceFillingCurve.Linear => _GenerateLinearOrder(width, height, startY),
      _ => _GenerateHilbertCurveOrder(width, height, startY, this._curveOrder)
    };

    foreach (var (x, y) in traversalOrder) {
      var sourceIdx = y * sourceStride + x;

      // Decode source pixel
      var pixel = decoder.Decode(source[sourceIdx]);
      var (c1, c2, c3, alpha) = pixel.ToNormalized();
      var originalC1 = c1.ToFloat();
      var originalC2 = c2.ToFloat();
      var originalC3 = c3.ToFloat();
      var originalA = alpha.ToFloat();

      // Calculate weighted error from history using exponential decay
      var totalErrorC1 = 0.0;
      var totalErrorC2 = 0.0;
      var totalErrorC3 = 0.0;

      for (var i = 0; i < historySize; ++i) {
        var index = (historyIndex - i - 1 + historySize) % historySize;
        var weight = Math.Exp(-i * 0.1);
        totalErrorC1 += errorHistory[index].c1 * weight;
        totalErrorC2 += errorHistory[index].c2 * weight;
        totalErrorC3 += errorHistory[index].c3 * weight;
      }

      // Apply damping factor
      const double dampingFactor = 0.5;
      totalErrorC1 *= dampingFactor;
      totalErrorC2 *= dampingFactor;
      totalErrorC3 *= dampingFactor;

      // Create adjusted color
      var adjustedC1 = (float)Math.Max(0, Math.Min(1, originalC1 + totalErrorC1));
      var adjustedC2 = (float)Math.Max(0, Math.Min(1, originalC2 + totalErrorC2));
      var adjustedC3 = (float)Math.Max(0, Math.Min(1, originalC3 + totalErrorC3));

      var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(adjustedC1),
        UNorm32.FromFloatClamped(adjustedC2),
        UNorm32.FromFloatClamped(adjustedC3),
        UNorm32.FromFloatClamped(originalA)
      );

      // Find nearest palette color
      var closestIndex = lookup.FindNearest(adjustedColor, out var closestColor);
      indices[y * targetStride + x] = (byte)closestIndex;

      // Calculate error (from original, not adjusted)
      var (cc1, cc2, cc3, _) = closestColor.ToNormalized();
      var errorC1 = originalC1 - cc1.ToFloat();
      var errorC2 = originalC2 - cc2.ToFloat();
      var errorC3 = originalC3 - cc3.ToFloat();

      // Store in history buffer
      errorHistory[historyIndex] = (errorC1, errorC2, errorC3);
      historyIndex = (historyIndex + 1) % historySize;
    }
  }

  #region Hilbert Curve

  private static List<(int x, int y)> _GenerateHilbertCurveOrder(int width, int height, int startY, int? order) {
    var result = new List<(int, int)>(width * height);
    var endY = startY + height;

    if (width <= 0 || height <= 0)
      return result;

    // Calculate order if not specified (find minimum order that covers the image)
    int n;
    if (order.HasValue) {
      var clampedOrder = Math.Max(1, Math.Min(MaxHilbertOrder, order.Value));
      n = 1 << clampedOrder; // 2^order
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

  #region Peano Curve

  private static List<(int x, int y)> _GeneratePeanoCurveOrder(int width, int height, int startY, int? order) {
    var result = new List<(int, int)>(width * height);
    var endY = startY + height;

    if (width <= 0 || height <= 0)
      return result;

    // Calculate order if not specified (find minimum order that covers the image)
    int n;
    if (order.HasValue) {
      var clampedOrder = Math.Max(1, Math.Min(MaxPeanoOrder, order.Value));
      n = (int)Math.Pow(3, clampedOrder); // 3^order
    } else {
      n = 1;
      while (n < Math.Max(width, endY))
        n *= 3;
    }

    // Generate Peano curve points
    // Initial vectors: a=(n,0) points right, b=(0,n) points down
    _GeneratePeanoRecursive(result, 0, 0, n, 0, 0, n, width, startY, endY);

    return result;
  }

  /// <summary>
  /// Recursively generates Peano curve coordinates.
  /// </summary>
  /// <param name="result">List to add coordinates to.</param>
  /// <param name="x">Current X position.</param>
  /// <param name="y">Current Y position.</param>
  /// <param name="ax">X component of the a vector.</param>
  /// <param name="ay">Y component of the a vector.</param>
  /// <param name="bx">X component of the b vector.</param>
  /// <param name="by">Y component of the b vector.</param>
  /// <param name="maxWidth">Maximum X coordinate (exclusive).</param>
  /// <param name="minY">Minimum Y coordinate (inclusive).</param>
  /// <param name="maxY">Maximum Y coordinate (exclusive).</param>
  private static void _GeneratePeanoRecursive(
    List<(int, int)> result,
    int x, int y,
    int ax, int ay,
    int bx, int by,
    int maxWidth, int minY, int maxY) {

    var w = Math.Abs(ax + ay);
    var h = Math.Abs(bx + by);

    // Determine direction signs
    var dax = ax > 0 ? 1 : ax < 0 ? -1 : 0;
    var day = ay > 0 ? 1 : ay < 0 ? -1 : 0;
    var dbx = bx > 0 ? 1 : bx < 0 ? -1 : 0;
    var dby = by > 0 ? 1 : by < 0 ? -1 : 0;

    // Base case: single pixel
    if (w == 1 && h == 1) {
      if (x >= 0 && x < maxWidth && y >= minY && y < maxY)
        result.Add((x, y));
      return;
    }

    // Divide into 3x3 grid
    var ax2 = ax / 3;
    var ay2 = ay / 3;
    var bx2 = bx / 3;
    var by2 = by / 3;

    // Width or height = 2 edge cases
    if (w == 2) {
      // Handle 2xN case with simple serpentine
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
      // Handle Nx2 case with simple serpentine
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

    // Standard 3x3 Peano subdivision
    // The Peano curve visits 9 sub-squares in a specific pattern with alternating directions
    // Pattern: traverse columns, alternating direction each column

    // Column 0: bottom to top
    _GeneratePeanoRecursive(result, x, y, ax2, ay2, bx2, by2, maxWidth, minY, maxY);
    _GeneratePeanoRecursive(result, x + bx2, y + by2, ax2, ay2, bx2, by2, maxWidth, minY, maxY);
    _GeneratePeanoRecursive(result, x + 2 * bx2, y + 2 * by2, ax2, ay2, bx2, by2, maxWidth, minY, maxY);

    // Column 1: top to bottom (reversed b direction)
    _GeneratePeanoRecursive(result, x + ax2 + 2 * bx2, y + ay2 + 2 * by2, ax2, ay2, -bx2, -by2, maxWidth, minY, maxY);
    _GeneratePeanoRecursive(result, x + ax2 + bx2, y + ay2 + by2, ax2, ay2, -bx2, -by2, maxWidth, minY, maxY);
    _GeneratePeanoRecursive(result, x + ax2, y + ay2, ax2, ay2, -bx2, -by2, maxWidth, minY, maxY);

    // Column 2: bottom to top
    _GeneratePeanoRecursive(result, x + 2 * ax2, y + 2 * ay2, ax2, ay2, bx2, by2, maxWidth, minY, maxY);
    _GeneratePeanoRecursive(result, x + 2 * ax2 + bx2, y + 2 * ay2 + by2, ax2, ay2, bx2, by2, maxWidth, minY, maxY);
    _GeneratePeanoRecursive(result, x + 2 * ax2 + 2 * bx2, y + 2 * ay2 + 2 * by2, ax2, ay2, bx2, by2, maxWidth, minY, maxY);
  }

  #endregion

  #region Linear Scan

  private static List<(int x, int y)> _GenerateLinearOrder(int width, int height, int startY) {
    var result = new List<(int, int)>(width * height);
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

  #endregion

}
