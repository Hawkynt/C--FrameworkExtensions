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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Clustered-dot ordered dithering algorithm that uses threshold matrices where dots cluster together.
/// </summary>
/// <remarks>
/// <para>Clustered-dot dithering creates halftone patterns similar to traditional printing methods.</para>
/// <para>Unlike dispersed-dot patterns (Bayer), clustered-dot patterns arrange thresholds in</para>
/// <para>circular or spiral patterns to create dot clusters, making them better for print simulation.</para>
/// <para>This algorithm is particularly useful for simulating newspaper or magazine printing.</para>
/// </remarks>
[Ditherer("Cluster Dot Dithering", Description = "Halftone-style ordered dithering with clustered dot patterns", Type = DitheringType.Ordered)]
public readonly struct ClusterDotDitherer : IDitherer {

  #region fields

  private readonly float[] _thresholds;

  #endregion

  #region properties

  /// <summary>Size of the threshold matrix.</summary>
  public int MatrixSize { get; }

  /// <summary>Dithering strength (0-1). Higher values produce more visible patterns.</summary>
  public float Strength { get; }

  #endregion

  #region fluent API

  /// <summary>Returns this ditherer with specified strength.</summary>
  public ClusterDotDitherer WithStrength(float strength) => new(this._thresholds, this.MatrixSize, strength);

  #endregion

  #region constructors

  /// <summary>
  /// Creates a cluster dot ditherer from a threshold matrix.
  /// </summary>
  /// <param name="matrix">The threshold matrix (values will be normalized to [-0.5, 0.5]).</param>
  /// <param name="strength">Dithering strength (0-1). Default is 1.</param>
  public ClusterDotDitherer(byte[,] matrix, float strength = 1f) {
    var rows = matrix.GetLength(0);
    var cols = matrix.GetLength(1);

    if (rows != cols)
      cols = rows = rows > cols ? rows : cols;

    this.MatrixSize = rows;
    this._thresholds = new float[rows * cols];
    this.Strength = Math.Max(0, Math.Min(1, strength));

    // Find max for normalization
    var maxVal = (float)(rows * cols - 1);

    // Normalize to [-0.5, 0.5]
    for (var r = 0; r < rows; ++r)
    for (var c = 0; c < cols; ++c)
      this._thresholds[r * cols + c] = matrix[r, c] / maxVal - 0.5f;
  }

  private ClusterDotDitherer(float[] thresholds, int matrixSize, float strength) {
    this._thresholds = thresholds;
    this.MatrixSize = matrixSize;
    this.Strength = Math.Max(0, Math.Min(1, strength));
  }

  #endregion

  #region IDitherer

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => false;

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
    var endY = startY + height;
    var matrixSize = this.MatrixSize;
    var thresholds = this._thresholds;
    var strength = this.Strength;

    for (var y = startY; y < endY; ++y) {
      var matrixY = y % matrixSize;

      for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
        var originalColor = source[sourceIdx];
        var (c1, c2, c3, c4) = originalColor.ToNormalized();

        // Get float values
        var f1 = c1.ToFloat();
        var f2 = c2.ToFloat();
        var f3 = c3.ToFloat();

        // Get threshold from matrix
        var threshold = thresholds[matrixY * matrixSize + (x % matrixSize)] * strength;

        // Apply threshold adjustment using ColorFactory
        var ditheredColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(f1 + threshold),
          UNorm32.FromFloatClamped(f2 + threshold),
          UNorm32.FromFloatClamped(f3 + threshold),
          c4
        );

        indices[targetIdx] = (byte)lookup.FindNearest(ditheredColor);
      }
    }
  }

  #endregion

  #region pre-configured instances

  /// <summary>
  /// Clustered-dot 3x3 dithering pattern for small, tight dot clusters.
  /// </summary>
  public static ClusterDotDitherer ClusterDot3x3 { get; } = new(new byte[,] {
    { 6, 7, 4 },
    { 5, 0, 3 },
    { 2, 1, 8 }
  });

  /// <summary>
  /// Clustered-dot 4x4 dithering pattern for medium dot clusters with better tonal range.
  /// </summary>
  public static ClusterDotDitherer ClusterDot4x4 { get; } = new(new byte[,] {
    { 12, 5, 6, 13 },
    { 4, 0, 1, 7 },
    { 11, 3, 2, 8 },
    { 15, 10, 9, 14 }
  });

  /// <summary>
  /// Clustered-dot 8×8 dithering pattern (newspaper-style halftone screen). Threshold values
  /// 0..63 each appear exactly once and grow outward in a spiral from cluster centres,
  /// producing the characteristic dot-clustering of offset printing.
  /// </summary>
  /// <remarks>
  /// Reference: standard 8×8 clustered-dot ordered-dither matrix (Foley, van Dam, Feiner &amp;
  /// Hughes 1990 / Ulichney 1987 family). Verified at https://maximmcnair.com/p/webgl-dithering
  /// (this matrix layout). The previous matrix had duplicate values (two copies of a 4×4
  /// spiral pasted side-by-side); ranking was degenerate.
  /// </remarks>
  public static ClusterDotDitherer ClusterDot8x8 { get; } = new(new byte[,] {
    { 24, 10, 12, 26, 35, 47, 49, 37 },
    {  8,  0,  2, 14, 45, 59, 61, 51 },
    { 22,  6,  4, 16, 43, 57, 63, 53 },
    { 30, 20, 18, 28, 33, 41, 55, 39 },
    { 34, 46, 48, 36, 25, 11, 13, 27 },
    { 44, 58, 60, 50,  9,  1,  3, 15 },
    { 42, 56, 62, 52, 23,  7,  5, 17 },
    { 32, 40, 54, 38, 31, 21, 19, 29 }
  });

  /// <summary>Default cluster dot ditherer (4x4 pattern).</summary>
  public static ClusterDotDitherer Default => ClusterDot4x4;

  #endregion
}
