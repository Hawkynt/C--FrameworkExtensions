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
#if SUPPORTS_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Ordered ditherer using threshold matrices (Bayer, halftone, cluster dot).
/// </summary>
/// <remarks>
/// <para>Ordered dithering adds a position-dependent threshold before quantization.</para>
/// <para>Unlike error diffusion, pixels can be processed independently (parallelizable).</para>
/// <para>Reference: B. E. Bayer, <i>An optimum method for two-level rendition of continuous-tone
/// pictures</i>, IEEE Int. Conf. on Communications, vol. 1, pp. 26-11 to 26-15, 1973.</para>
/// <code>output(x, y) = quantise( source(x, y) + (M[x mod N, y mod N] / N² − 0.5) · scale )</code>
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
    var matrixSize = this.MatrixSize;
    var strength = this.Strength;
    var thresholds = this._thresholds;
    var endY = startY + height;

#if SUPPORTS_INTRINSICS
    // float-domain SIMD across 4 pixels of B/G/R add+clamp; scalar UNorm32.FromFloatClamped
    // quantize per channel preserves bit-exact output (post-FindNearest goldens stay byte-exact).
    // Stackalloc'd outside the y/x loops to avoid CA2014.
    // Eligibility check is loop-invariant — hoisted out of the y-loop so the JIT sees
    // two distinct hot loops, and so legacy TFMs (net35/40/45/48) don't pay a per-row
    // Sse2.IsSupported field load.
    var simdEligible = Sse2.IsSupported && typeof(TWork) == typeof(Bgra8888) && width >= 4;
    if (simdEligible) {
      var simdEnd = width & ~3;
      var bChannels = stackalloc float[4];
      var gChannels = stackalloc float[4];
      var rChannels = stackalloc float[4];
      var alphaBytes = stackalloc byte[4];
      var thresholds4 = stackalloc float[4];

      // 4-pixel SIMD batch: SIMD add+clamp on B/G/R, scalar quantize and lookup per lane.
      // Bit-exact with the scalar path because SSE2 single-precision arithmetic is IEEE-754
      // 32-bit identical to the .NET float path used by UNorm32.ToFloat.
      for (var y = startY; y < endY; ++y) {
        var thresholdRowOffset = (y % matrixSize) * matrixSize;
        var rowSource = source + y * sourceStride;
        var x = 0;
        var targetIdx = y * targetStride;
        var mx = 0;

        var srcBase = (byte*)rowSource;
        for (; x < simdEnd; x += 4) {
          // Decode 4 pixels' BGR channels into separate float[4] arrays + 4 alpha bytes.
          ThresholdDithererSimd.DecodeBgra4Pixels(srcBase + x * 4, bChannels, gChannels, rChannels, alphaBytes);

          // Compute 4 per-pixel thresholds (scalar — they're cheap and the SSE2 broadcast
          // would cost more in setup than it saves).
          for (var lane = 0; lane < 4; ++lane) {
            thresholds4[lane] = thresholds[thresholdRowOffset + mx] * strength;
            mx = ++mx < matrixSize ? mx : 0;
          }

          // SIMD add+clamp on all 3 channels in parallel (4 pixels per channel).
          ThresholdDithererSimd.AddThresholdAndClamp_4Pixels(bChannels, gChannels, rChannels, thresholds4);

          // Per-lane: early-out check against original colour, else lookup adjusted.
          for (var lane = 0; lane < 4; ++lane) {
            var color = rowSource[x + lane];
            var nearestIdx = lookup.FindNearest(color, out var nearestColor);
            var (c1, c2, c3, a) = color.ToNormalized();
            var (n1, n2, n3, na) = nearestColor.ToNormalized();
            var distToNearest = Math.Abs(c1.ToFloat() - n1.ToFloat()) +
                                Math.Abs(c2.ToFloat() - n2.ToFloat()) +
                                Math.Abs(c3.ToFloat() - n3.ToFloat()) +
                                Math.Abs(a.ToFloat() - na.ToFloat());
            if (distToNearest < 0.02f) {
              indices[targetIdx + x + lane] = (byte)nearestIdx;
              continue;
            }

            // Bgra8888 component convention: (C1, C2, C3, A) = (R, G, B, A).
            var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
              UNorm32.FromFloatClamped(rChannels[lane]),
              UNorm32.FromFloatClamped(gChannels[lane]),
              UNorm32.FromFloatClamped(bChannels[lane]),
              UNorm32.FromByte(alphaBytes[lane])
            );
            indices[targetIdx + x + lane] = (byte)lookup.FindNearest(adjustedColor);
          }
        }
        targetIdx += x;

        // Tail: width-mod-4 leftover lanes.
        for (; x < width; ++x, ++targetIdx, mx = ++mx < matrixSize ? mx : 0) {
          var color = rowSource[x];
          var nearestIdx = lookup.FindNearest(color, out var nearestColor);
          var (c1, c2, c3, a) = color.ToNormalized();
          var (n1, n2, n3, na) = nearestColor.ToNormalized();
          var distToNearest = Math.Abs(c1.ToFloat() - n1.ToFloat()) +
                              Math.Abs(c2.ToFloat() - n2.ToFloat()) +
                              Math.Abs(c3.ToFloat() - n3.ToFloat()) +
                              Math.Abs(a.ToFloat() - na.ToFloat());

          if (distToNearest < 0.02f) {
            indices[targetIdx] = (byte)nearestIdx;
            continue;
          }

          var threshold = thresholds[thresholdRowOffset + mx] * strength;
          var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped(c1.ToFloat() + threshold),
            UNorm32.FromFloatClamped(c2.ToFloat() + threshold),
            UNorm32.FromFloatClamped(c3.ToFloat() + threshold),
            a
          );

          nearestIdx = lookup.FindNearest(adjustedColor);
          indices[targetIdx] = (byte)nearestIdx;
        }
      }
    } else
#endif
    {
      for (var y = startY; y < endY; ++y) {
        // Pre-calculate row-invariant values
        var thresholdRowOffset = (y % matrixSize) * matrixSize;
        var rowSource = source + y * sourceStride;
        var targetIdx = y * targetStride;
        var mx = 0;

        for (var x = 0; x < width; ++x, ++targetIdx, mx = ++mx < matrixSize ? mx : 0) {
          // Read pre-decoded source pixel from the working buffer.
          var color = rowSource[x];

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
  /// <param name="size">Matrix size (must be a positive power of two).</param>
  /// <remarks>Delegates to <see cref="BayerMatrix.Generate"/>; kept for API continuity.</remarks>
  public static float[,] GenerateBayer(int size) => BayerMatrix.Generate(size);

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
