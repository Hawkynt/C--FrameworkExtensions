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

    // Generate traversal order for the specified region. Delegates to the public
    // SpaceFillingCurves utility so other tools can reuse the same curves.
    var traversalOrder = this._curveType switch {
      SpaceFillingCurve.Hilbert => SpaceFillingCurves.Hilbert(width, height, startY, this._curveOrder),
      SpaceFillingCurve.Peano => SpaceFillingCurves.Peano(width, height, startY, this._curveOrder),
      SpaceFillingCurve.Linear => SpaceFillingCurves.LinearSerpentine(width, height, startY),
      _ => SpaceFillingCurves.Hilbert(width, height, startY, this._curveOrder)
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


}
