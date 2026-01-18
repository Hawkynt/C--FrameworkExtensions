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
/// Ordered ditherer using threshold matrices (Bayer, halftone, cluster dot).
/// </summary>
/// <remarks>
/// <para>Ordered dithering adds a position-dependent threshold before quantization.</para>
/// <para>Unlike error diffusion, pixels can be processed independently (parallelizable).</para>
/// <para>Reference: B. Bayer 1973 "An optimum method for two-level rendition of continuous-tone pictures"</para>
/// <para>IEEE Int. Conf. on Communications, vol. 1, pp. 26-11 to 26-15</para>
/// </remarks>
[Ditherer("Ordered Dithering", Description = "Threshold matrix dithering with Bayer and other patterns", Type = DitheringType.Ordered, Author = "Bryce Bayer", Year = 1973)]
public readonly struct OrderedDitherer : IDitherer {

  #region fields

  private readonly float[] _thresholds;

  #endregion

  #region properties

  /// <summary>Dithering strength (0-1). Higher values produce more visible patterns.</summary>
  public float Strength { get; }

  /// <summary>Size of the threshold matrix.</summary>
  public int MatrixSize { get; }

  #endregion

  #region fluent API

  /// <summary>Returns this ditherer with specified strength.</summary>
  public OrderedDitherer WithStrength(float strength) => new(this._thresholds, this.MatrixSize, strength);

  #endregion

  #region constructors

  /// <summary>
  /// Creates an ordered ditherer from a threshold matrix.
  /// </summary>
  /// <param name="matrix">The threshold matrix (values will be normalized to [-0.5, 0.5]).</param>
  /// <param name="strength">Dithering strength (0-1). Default is 1.</param>
  public OrderedDitherer(float[,] matrix, float strength = 1f) {
    var rows = matrix.GetLength(0);
    var cols = matrix.GetLength(1);

    if (rows != cols)
      cols = rows = rows > cols ? rows : cols;

    this.MatrixSize = rows;
    this._thresholds = new float[rows * cols];
    this.Strength = strength;

    // Find min/max for normalization
    var min = float.MaxValue;
    var max = float.MinValue;
    for (var r = 0; r < rows; ++r)
    for (var c = 0; c < cols; ++c) {
      var v = matrix[r, c];
      if (v < min) min = v;
      if (v > max) max = v;
    }

    // Normalize to [-0.5, 0.5]
    var range = max - min;
    if (!(range > 0))
      return;

    for (var r = 0; r < rows; ++r)
    for (var c = 0; c < cols; ++c)
      this._thresholds[r * cols + c] = (matrix[r, c] - min) / range - 0.5f;
  }

  private OrderedDitherer(float[] thresholds, int size, float strength) {
    this._thresholds = thresholds;
    this.MatrixSize = size;
    this.Strength = strength;
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
    var matrixSize = this.MatrixSize;
    var strength = this.Strength;
    var thresholds = this._thresholds;
    var endY = startY + height;

    for (var y = startY; y < endY; ++y) {
      // Pre-calculate row-invariant values
      var thresholdRowOffset = (y % matrixSize) * matrixSize;

      for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride, mx = 0; x < width; ++x, ++sourceIdx, ++targetIdx, mx = ++mx < matrixSize ? mx : 0) {
        // Decode source pixel
        var color = decoder.Decode(source[sourceIdx]);

        // First check if this color is very close to a palette entry (exact or near-exact match)
        // In that case, skip threshold adjustment to preserve exact matches
        var nearestIdx = lookup.FindNearest(color, out var nearestColor);
        var (c1, c2, c3, a) = color.ToNormalized();
        var (n1, n2, n3, na) = nearestColor.ToNormalized();
        var distToNearest = Math.Abs(c1.ToFloat() - n1.ToFloat()) +
                            Math.Abs(c2.ToFloat() - n2.ToFloat()) +
                            Math.Abs(c3.ToFloat() - n3.ToFloat()) +
                            Math.Abs(a.ToFloat() - na.ToFloat());

        // If the color is very close to the nearest palette entry, use it directly
        // Threshold of 0.02 allows for minor rounding/quantization differences
        if (distToNearest < 0.02f) {
          indices[targetIdx] = (byte)nearestIdx;
          continue;
        }

        // Get threshold from matrix (mx wraps via increment logic above)
        var threshold = thresholds[thresholdRowOffset + mx] * strength;

        // Apply threshold to color components
        var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(c1.ToFloat() + threshold),
          UNorm32.FromFloatClamped(c2.ToFloat() + threshold),
          UNorm32.FromFloatClamped(c3.ToFloat() + threshold),
          a
        );

        // Find nearest palette color and store index
        nearestIdx = lookup.FindNearest(adjustedColor);
        indices[targetIdx] = (byte)nearestIdx;
      }
    }
  }

  #endregion

  #region Bayer matrices

  /// <summary>Bayer 2x2: Smallest ordered dither pattern.</summary>
  public static OrderedDitherer Bayer2x2 { get; } = new(GenerateBayer(2));

  /// <summary>Bayer 4x4: Standard ordered dither pattern.</summary>
  public static OrderedDitherer Bayer4x4 { get; } = new(GenerateBayer(4));

  /// <summary>Bayer 8x8: Larger pattern with more gradations.</summary>
  public static OrderedDitherer Bayer8x8 { get; } = new(GenerateBayer(8));

  /// <summary>Bayer 16x16: Very large pattern for high quality.</summary>
  public static OrderedDitherer Bayer16x16 { get; } = new(GenerateBayer(16));

  /// <summary>
  /// Generates a Bayer threshold matrix of the specified size.
  /// </summary>
  /// <param name="size">Matrix size (must be power of 2).</param>
  public static float[,] GenerateBayer(int size) {
    var matrix = new float[size, size];

    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x)
      matrix[y, x] = BayerValue(x, y, size);

    return matrix;
  }

  /// <summary>
  /// Calculates the Bayer matrix value at position (x, y) for a given matrix size.
  /// Uses the recursive definition of the Bayer matrix.
  /// </summary>
  private static float BayerValue(int x, int y, int size) {
    var value = 0;
    var mask = size >> 1;

    while (mask > 0) {
      value <<= 2;
      var xBit = (x & mask) != 0 ? 1 : 0;
      var yBit = (y & mask) != 0 ? 1 : 0;
      value |= (xBit ^ yBit) | (yBit << 1);
      mask >>= 1;
    }

    return value;
  }

  #endregion

  #region Halftone patterns

  /// <summary>Halftone 4x4: Simulates halftone printing pattern.</summary>
  public static OrderedDitherer Halftone4x4 { get; } = new(new float[,] {
    {  7, 13, 11,  4 },
    { 12, 16, 14,  8 },
    { 10, 15,  6,  2 },
    {  5,  9,  3,  1 }
  });

  /// <summary>Halftone 8x8: Larger halftone pattern.</summary>
  public static OrderedDitherer Halftone8x8 { get; } = new(new float[,] {
    { 24, 10, 12, 26, 35, 47, 49, 37 },
    {  8,  0,  2, 14, 45, 59, 61, 51 },
    { 22,  6,  4, 16, 43, 57, 63, 53 },
    { 30, 20, 18, 28, 33, 41, 55, 39 },
    { 34, 46, 48, 36, 25, 11, 13, 27 },
    { 44, 58, 60, 50,  9,  1,  3, 15 },
    { 42, 56, 62, 52, 23,  7,  5, 17 },
    { 32, 40, 54, 38, 31, 21, 19, 29 }
  });

  #endregion

  #region Cluster dot patterns

  /// <summary>Cluster Dot 4x4: Clustered dot pattern for smoother appearance.</summary>
  public static OrderedDitherer ClusterDot4x4 { get; } = new(new float[,] {
    { 12,  5,  6, 13 },
    {  4,  0,  1,  7 },
    { 11,  3,  2,  8 },
    { 15, 10,  9, 14 }
  });

  /// <summary>Cluster Dot 8x8: Larger cluster dot pattern.</summary>
  public static OrderedDitherer ClusterDot8x8 { get; } = new(new float[,] {
    { 24, 10, 12, 26, 35, 47, 49, 37 },
    {  8,  0,  2, 14, 45, 59, 61, 51 },
    { 22,  6,  4, 16, 43, 57, 63, 53 },
    { 30, 20, 18, 28, 33, 41, 55, 39 },
    { 34, 46, 48, 36, 25, 11, 13, 27 },
    { 44, 58, 60, 50,  9,  1,  3, 15 },
    { 42, 56, 62, 52, 23,  7,  5, 17 },
    { 32, 40, 54, 38, 31, 21, 19, 29 }
  });

  #endregion

  #region Diagonal patterns

  /// <summary>Diagonal 4x4: Diagonal line pattern.</summary>
  public static OrderedDitherer Diagonal4x4 { get; } = new(new float[,] {
    {  0,  8,  2, 10 },
    { 12,  4, 14,  6 },
    {  3, 11,  1,  9 },
    { 15,  7, 13,  5 }
  });

  #endregion

}
