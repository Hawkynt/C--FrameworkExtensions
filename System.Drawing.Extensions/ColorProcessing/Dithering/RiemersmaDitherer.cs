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
/// Riemersma dithering using space-filling curves (Hilbert curve).
/// </summary>
/// <remarks>
/// <para>Reference: T. Riemersma 1998 "A Balanced Dithering Technique" C/C++ Users Journal</para>
/// <para>See also: https://www.compuphase.com/riemer.htm</para>
/// <para>Uses exponential decay weights with history buffer along Hilbert curve traversal.</para>
/// </remarks>
[Ditherer("Riemersma", Description = "Space-filling curve dithering with exponential decay history", Type = DitheringType.Custom, Author = "Thiadmer Riemersma", Year = 1998)]
public readonly struct RiemersmaDitherer : IDitherer {

  private const int _DEFAULT_HISTORY_SIZE = 16;
  private readonly int _historySize;
  private readonly bool _useHilbertCurve;

  /// <summary>Pre-configured instance with 16-entry history and Hilbert curve.</summary>
  public static RiemersmaDitherer Default { get; } = new(_DEFAULT_HISTORY_SIZE, true);

  /// <summary>Pre-configured instance with 8-entry history (faster, lower quality).</summary>
  public static RiemersmaDitherer Small { get; } = new(8, true);

  /// <summary>Pre-configured instance with 32-entry history (slower, higher quality).</summary>
  public static RiemersmaDitherer Large { get; } = new(32, true);

  /// <summary>Pre-configured instance with linear (serpentine) traversal instead of Hilbert curve.</summary>
  public static RiemersmaDitherer Linear { get; } = new(_DEFAULT_HISTORY_SIZE, false);

  /// <summary>
  /// Creates a Riemersma ditherer.
  /// </summary>
  /// <param name="historySize">Size of the error history buffer (typically 8, 16, or 32).</param>
  /// <param name="useHilbertCurve">If true, uses Hilbert curve traversal; otherwise uses serpentine scan.</param>
  public RiemersmaDitherer(int historySize = _DEFAULT_HISTORY_SIZE, bool useHilbertCurve = true) {
    this._historySize = historySize;
    this._useHilbertCurve = useHilbertCurve;
  }

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

    // Generate traversal order for the specified region
    var traversalOrder = this._useHilbertCurve
      ? _GenerateHilbertCurveOrder(width, height, startY)
      : _GenerateLinearOrder(width, height, startY);

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

  private static List<(int x, int y)> _GenerateHilbertCurveOrder(int width, int height, int startY = 0) {
    var result = new List<(int, int)>(width * height);
    var endY = startY + height;

    if (width <= 0 || height <= 0)
      return result;

    // Find next power of 2 that covers both dimensions
    var n = 1;
    while (n < Math.Max(width, endY))
      n *= 2;

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
      (x, y) = _Rot(s, x, y, rx, ry);
      x += s * rx;
      y += s * ry;
      t /= 4;
      s *= 2;
    }

    return (x, y);
  }

  private static (int x, int y) _Rot(int n, int x, int y, int rx, int ry) {
    if (ry == 0) {
      if (rx == 1) {
        x = n - 1 - x;
        y = n - 1 - y;
      }
      (x, y) = (y, x);
    }
    return (x, y);
  }

  private static List<(int x, int y)> _GenerateLinearOrder(int width, int height, int startY = 0) {
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
}
