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
  public TResult InvokeKernel<TWork, TPixel, TDecode, TEncode, TMetric, TResult>(
    IDithererCallback<TWork, TPixel, TDecode, TEncode, TMetric, TResult> callback,
    int width,
    int height)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TMetric : struct, IColorMetric<TWork>
    => callback.Invoke(new OrderedDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric>(
      width, height, this._thresholds, this.MatrixSize, this.Strength));

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

/// <summary>
/// Ordered ditherer kernel that applies threshold-based dithering.
/// </summary>
file readonly struct OrderedDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric>(
  int width, int height,
  float[] thresholds, int matrixSize, float strength)
  : IDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TPixel : unmanaged, IStorageSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TMetric : struct, IColorMetric<TWork> {

  public int Width => width;
  public int Height => height;
  public bool RequiresSequentialProcessing => false;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void ProcessOrdered(
    TPixel* source,
    int sourceStride,
    int x, int y,
    TPixel* dest,
    int destStride,
    in TDecode decoder,
    in TEncode encoder,
    in TMetric metric,
    TWork[] palette) {

    // Decode source pixel
    var sourceIdx = y * sourceStride + x;
    var color = decoder.Decode(source[sourceIdx]);

    // Get threshold from matrix
    var mx = x % matrixSize;
    var my = y % matrixSize;
    var threshold = thresholds[my * matrixSize + mx] * strength;

    // Apply threshold to color
    var adjustedColor = ColorFactory.Create4F<TWork>(
      color.C1 + threshold,
      color.C2 + threshold,
      color.C3 + threshold,
      color.A
    );

    // Find nearest palette color
    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var nearestIdx = lookup.FindNearest(adjustedColor);

    // Write result
    var destIdx = y * destStride + x;
    dest[destIdx] = encoder.Encode(lookup[nearestIdx]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void ProcessErrorDiffusion(
    TPixel* source,
    int sourceStride,
    TPixel* dest,
    int destStride,
    in TDecode decoder,
    in TEncode encoder,
    in TMetric metric,
    TWork[] palette) {
    // Ordered dithering doesn't use error diffusion - process all pixels independently
    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);

    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var sourceIdx = y * sourceStride + x;
      var color = decoder.Decode(source[sourceIdx]);

      var mx = x % matrixSize;
      var my = y % matrixSize;
      var threshold = thresholds[my * matrixSize + mx] * strength;

      var adjustedColor = ColorFactory.Create4F<TWork>(
        color.C1 + threshold,
        color.C2 + threshold,
        color.C3 + threshold,
        color.A
      );

      var nearestIdx = lookup.FindNearest(adjustedColor);
      var destIdx = y * destStride + x;
      dest[destIdx] = encoder.Encode(lookup[nearestIdx]);
    }
  }

}
