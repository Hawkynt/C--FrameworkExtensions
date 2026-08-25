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
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Types of space-filling and locality-oriented curves for image traversal.
/// </summary>
/// <remarks>
/// Riemersma's error history benefits from spatial locality. Continuous curves such as Hilbert,
/// Moore, Gilbert and the Peano family keep consecutive samples adjacent on their native domains;
/// Morton and diagonal scans are included as useful comparison traversals with weaker continuity.
/// </remarks>
public enum SpaceFillingCurve {
  /// <summary>
  /// Hilbert curve - subdivides space into 4 quadrants recursively.
  /// Order range: 1-7 when explicitly specified. Order n covers 2^n × 2^n.
  /// </summary>
  Hilbert = 0,

  /// <summary>
  /// Peano curve - subdivides space into a recursively transformed 3×3 grid.
  /// Order range: 1-5. Order n covers 3^n × 3^n.
  /// </summary>
  Peano = 1,

  /// <summary>
  /// Simple serpentine (boustrophedon) scan - alternating left-to-right and right-to-left rows.
  /// </summary>
  Linear = 2,

  /// <summary>
  /// Moore curve - closed-loop Hilbert relative. Complete 2^n × 2^n domains are 4-connected
  /// and the final point is adjacent to the first one.
  /// </summary>
  Moore = 3,

  /// <summary>
  /// Generalized Hilbert (Gilbert) traversal for arbitrary rectangular dimensions.
  /// The curve order parameter is ignored.
  /// </summary>
  Gilbert = 4,

  /// <summary>
  /// Coil curve - continuous 3×3 Peano-family traversal that swaps axes in every recursive subcell.
  /// </summary>
  Coil = 5,

  /// <summary>
  /// Half-coil curve - continuous 3×3 Peano-family traversal alternating Peano and Coil orientations.
  /// </summary>
  HalfCoil = 6,

  /// <summary>
  /// Meurthe curve - continuous 3×3 Peano-family traversal with neutral recursive orientation.
  /// </summary>
  Meurthe = 7,

  /// <summary>
  /// Morton/Z-order traversal. Preserves hierarchical locality but may jump between consecutive pixels.
  /// </summary>
  Morton = 8,

  /// <summary>
  /// Clockwise inward rectangular spiral. Supports arbitrary dimensions and remains 4-connected.
  /// </summary>
  Spiral = 9,

  /// <summary>
  /// Zig-zag traversal over successive x+y diagonals. Supports arbitrary dimensions and includes diagonal steps.
  /// </summary>
  DiagonalSerpentine = 10
}

/// <summary>
/// Riemersma dithering using configurable space-filling or locality-oriented traversals.
/// </summary>
/// <remarks>
/// <para>Reference: T. Riemersma 1998 "A Balanced Dithering Technique" C/C++ Users Journal.</para>
/// <para>See also: https://www.compuphase.com/riemer.htm</para>
/// <para>Uses exponential decay weights with a history buffer along the selected traversal.</para>
/// </remarks>
[Ditherer("Riemersma", Description = "Space-filling curve dithering with exponential decay history", Type = DitheringType.Custom, Author = "Thiadmer Riemersma", Year = 1998)]
public readonly struct RiemersmaDitherer : IDitherer {

  private const int _DEFAULT_HISTORY_SIZE = 16;
  private readonly int _historySize;
  private readonly SpaceFillingCurve _curveType;
  private readonly int? _curveOrder;

  /// <summary>Maximum explicitly requested Hilbert order.</summary>
  public const int MaxHilbertOrder = SpaceFillingCurves.MaxHilbertOrder;

  /// <summary>Maximum explicitly requested Moore order.</summary>
  public const int MaxMooreOrder = SpaceFillingCurves.MaxMooreOrder;

  /// <summary>Maximum Peano-family order.</summary>
  public const int MaxPeanoOrder = SpaceFillingCurves.MaxPeanoOrder;

  /// <summary>Maximum explicitly requested Morton order.</summary>
  public const int MaxMortonOrder = SpaceFillingCurves.MaxMortonOrder;

  /// <summary>Pre-configured instance with 16-entry history and Hilbert curve (auto order).</summary>
  public static RiemersmaDitherer Default { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Hilbert);

  /// <summary>Pre-configured instance with 8-entry history (faster, lower quality).</summary>
  public static RiemersmaDitherer Small { get; } = new(8, SpaceFillingCurve.Hilbert);

  /// <summary>Pre-configured instance with 32-entry history (slower, higher quality).</summary>
  public static RiemersmaDitherer Large { get; } = new(32, SpaceFillingCurve.Hilbert);

  /// <summary>Pre-configured instance with linear serpentine traversal.</summary>
  public static RiemersmaDitherer LinearScan { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Linear);

  /// <summary>Pre-configured instance with Peano traversal.</summary>
  public static RiemersmaDitherer Peano { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Peano);

  /// <summary>Pre-configured instance with Moore traversal.</summary>
  public static RiemersmaDitherer Moore { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Moore);

  /// <summary>Pre-configured instance with generalized Hilbert traversal for arbitrary rectangles.</summary>
  public static RiemersmaDitherer Gilbert { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Gilbert);

  /// <summary>Pre-configured instance with Coil traversal.</summary>
  public static RiemersmaDitherer Coil { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Coil);

  /// <summary>Pre-configured instance with Half-coil traversal.</summary>
  public static RiemersmaDitherer HalfCoil { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.HalfCoil);

  /// <summary>Pre-configured instance with Meurthe traversal.</summary>
  public static RiemersmaDitherer Meurthe { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Meurthe);

  /// <summary>Pre-configured instance with Morton/Z-order traversal.</summary>
  public static RiemersmaDitherer Morton { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Morton);

  /// <summary>Pre-configured instance with clockwise inward spiral traversal.</summary>
  public static RiemersmaDitherer SpiralScan { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.Spiral);

  /// <summary>Pre-configured instance with diagonal serpentine traversal.</summary>
  public static RiemersmaDitherer DiagonalScan { get; } = new(_DEFAULT_HISTORY_SIZE, SpaceFillingCurve.DiagonalSerpentine);

  /// <summary>
  /// Creates a Riemersma ditherer with the specified traversal.
  /// </summary>
  /// <param name="historySize">Size of the error history buffer (typically 8, 16, or 32).</param>
  /// <param name="curveType">Traversal used to order pixels.</param>
  /// <param name="curveOrder">
  /// Optional recursive order. Used by Hilbert, Moore, Peano, Coil, Half-coil, Meurthe and Morton;
  /// ignored by Gilbert, Spiral, DiagonalSerpentine and Linear. <see langword="null"/> selects
  /// an order automatically where applicable.
  /// </param>
  public RiemersmaDitherer(
    int historySize = _DEFAULT_HISTORY_SIZE,
    SpaceFillingCurve curveType = SpaceFillingCurve.Hilbert,
    int? curveOrder = null) {
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
  public unsafe void Dither<TWork, TMetric>(
    TWork* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);

    // Handle default struct initialization (historySize = 0)
    var historySize = this._historySize > 0 ? this._historySize : _DEFAULT_HISTORY_SIZE;

    if (palette.Length == 0)
      return;

    // Error history buffer for exponential decay
    var errorHistory = new (double c1, double c2, double c3)[historySize];
    var historyIndex = 0;

    var traversalOrder = this._curveType switch {
      SpaceFillingCurve.Hilbert => SpaceFillingCurves.Hilbert(width, height, startY, this._curveOrder),
      SpaceFillingCurve.Peano => SpaceFillingCurves.Peano(width, height, startY, this._curveOrder),
      SpaceFillingCurve.Linear => SpaceFillingCurves.LinearSerpentine(width, height, startY),
      SpaceFillingCurve.Moore => SpaceFillingCurves.Moore(width, height, startY, this._curveOrder),
      SpaceFillingCurve.Gilbert => SpaceFillingCurves.Gilbert(width, height, startY),
      SpaceFillingCurve.Coil => SpaceFillingCurves.Coil(width, height, startY, this._curveOrder),
      SpaceFillingCurve.HalfCoil => SpaceFillingCurves.HalfCoil(width, height, startY, this._curveOrder),
      SpaceFillingCurve.Meurthe => SpaceFillingCurves.Meurthe(width, height, startY, this._curveOrder),
      SpaceFillingCurve.Morton => SpaceFillingCurves.Morton(width, height, startY, this._curveOrder),
      SpaceFillingCurve.Spiral => SpaceFillingCurves.Spiral(width, height, startY),
      SpaceFillingCurve.DiagonalSerpentine => SpaceFillingCurves.DiagonalSerpentine(width, height, startY),
      _ => SpaceFillingCurves.Hilbert(width, height, startY, this._curveOrder)
    };

    foreach (var (x, y) in traversalOrder) {
      var sourceIdx = y * sourceStride + x;

      // Decode source pixel
      var pixel = source[sourceIdx];
      var (c1, c2, c3, alpha) = pixel.ToNormalized();
      var originalC1 = c1.ToFloat();
      var originalC2 = c2.ToFloat();
      var originalC3 = c3.ToFloat();
      var originalA = alpha.ToFloat();

      // Riemersma 1998 weight schedule (https://www.compuphase.com/riemer.htm):
      //   weight[i] = (1/r) * pow(b, j)  where j = (q-1-i) so the most recent (i=0)
      //   gets weight 1 and the oldest (i=q-1) gets weight 1/r.
      // Equivalently: weight[i] = exp(-i * log(r) / (q-1)). r = 16 is the published
      // newest-to-oldest ratio; q = history size.
      var totalErrorC1 = 0.0;
      var totalErrorC2 = 0.0;
      var totalErrorC3 = 0.0;
      const double Ratio = 16.0;
      var logR = Math.Log(Ratio);
      var qm1 = Math.Max(1, historySize - 1);

      for (var i = 0; i < historySize; ++i) {
        var index = (historyIndex - i - 1 + historySize) % historySize;
        var weight = Math.Exp(-i * logR / qm1);
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
