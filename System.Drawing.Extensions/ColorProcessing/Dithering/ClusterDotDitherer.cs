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
using Hawkynt.ColorProcessing.Codecs;
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
    var endY = startY + height;
    var matrixSize = this.MatrixSize;
    var thresholds = this._thresholds;
    var strength = this.Strength;

    for (var y = startY; y < endY; ++y) {
      var matrixY = y % matrixSize;

      for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
        var originalColor = decoder.Decode(source[sourceIdx]);
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
  /// Clustered-dot 8x8 dithering pattern for larger dot clusters with finest tonal gradations.
  /// </summary>
  public static ClusterDotDitherer ClusterDot8x8 { get; } = new(new byte[,] {
    { 24, 47, 49, 37, 12, 35, 33, 21 },
    { 46, 58, 60, 50, 10, 22, 20, 8 },
    { 48, 59, 63, 52, 11, 23, 19, 7 },
    { 36, 56, 54, 38, 25, 41, 17, 9 },
    { 13, 11, 23, 25, 48, 36, 58, 60 },
    { 1, 9, 21, 27, 46, 38, 56, 62 },
    { 3, 7, 19, 29, 44, 40, 54, 61 },
    { 15, 5, 17, 31, 32, 42, 52, 39 }
  });

  /// <summary>Default cluster dot ditherer (4x4 pattern).</summary>
  public static ClusterDotDitherer Default => ClusterDot4x4;

  #endregion
}
