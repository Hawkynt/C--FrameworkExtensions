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
/// Barycentric dithering algorithm using triangle-based color interpolation.
/// </summary>
/// <remarks>
/// <para>Projects each pixel color into a triangle formed by the 3 closest palette colors.</para>
/// <para>Uses barycentric coordinates (sub-triangle area ratios) as blend weights.</para>
/// <para>Combines with ordered dithering patterns for smooth color transitions.</para>
/// </remarks>
[Ditherer("Barycentric", Description = "Triangle-based interpolation with Bayer pattern", Type = DitheringType.Ordered)]
public readonly struct BarycentricDitherer : IDitherer {

  private const int _DEFAULT_MATRIX_SIZE = 4;
  private static readonly double[,] _DefaultMatrix = _GenerateBayerMatrix(_DEFAULT_MATRIX_SIZE);

  private readonly int _matrixSize;
  private readonly double[,] _bayerMatrix;

  /// <summary>Pre-configured instance with 2x2 Bayer matrix.</summary>
  public static BarycentricDitherer Bayer2x2 { get; } = new(2);

  /// <summary>Pre-configured instance with 4x4 Bayer matrix.</summary>
  public static BarycentricDitherer Bayer4x4 { get; } = new(4);

  /// <summary>Pre-configured instance with 8x8 Bayer matrix.</summary>
  public static BarycentricDitherer Bayer8x8 { get; } = new(8);

  /// <summary>
  /// Creates a barycentric ditherer with the specified matrix size.
  /// </summary>
  /// <param name="matrixSize">Size of the Bayer matrix (must be power of 2).</param>
  public BarycentricDitherer(int matrixSize = _DEFAULT_MATRIX_SIZE) {
    this._matrixSize = matrixSize;
    this._bayerMatrix = matrixSize == _DEFAULT_MATRIX_SIZE ? _DefaultMatrix : _GenerateBayerMatrix(matrixSize);
  }

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

    var endY = startY + height;

    // Handle default struct initialization (fields = 0/null)
    var matrixSize = this._matrixSize > 0 ? this._matrixSize : _DEFAULT_MATRIX_SIZE;
    var bayerMatrix = this._bayerMatrix ?? _DefaultMatrix;

    // Precompute palette colors in normalized form
    var paletteColors = new (float c1, float c2, float c3, float a)[palette.Length];
    for (var i = 0; i < palette.Length; ++i) {
      var (c1, c2, c3, a) = palette[i].ToNormalized();
      paletteColors[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat());
    }

    for (var y = startY; y < endY; ++y)
    for (var x = 0; x < width; ++x) {
      var pixel = decoder.Decode(source[y * sourceStride + x]);
      var (c1, c2, c3, alpha) = pixel.ToNormalized();
      var pixelNormalized = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), alpha.ToFloat());

      // Find 3 closest colors
      var closest = _FindNClosestColors(pixel, palette, metric, 3);

      if (closest.Count == 0) {
        indices[y * targetStride + x] = 0;
        continue;
      }

      if (closest.Count == 1) {
        indices[y * targetStride + x] = (byte)closest[0].index;
        continue;
      }

      // Get Bayer threshold for this position
      var threshold = bayerMatrix[y % matrixSize, x % matrixSize];

      // Calculate barycentric coordinates
      var (w0, w1, w2) = _CalculateBarycentricWeights(
        pixelNormalized,
        paletteColors[closest[0].index],
        closest.Count > 1 ? paletteColors[closest[1].index] : paletteColors[closest[0].index],
        closest.Count > 2 ? paletteColors[closest[2].index] : paletteColors[closest[0].index]
      );

      // Select color based on threshold and weights
      int selectedIndex;
      if (threshold < w0)
        selectedIndex = closest[0].index;
      else if (threshold < w0 + w1 && closest.Count > 1)
        selectedIndex = closest[1].index;
      else if (closest.Count > 2)
        selectedIndex = closest[2].index;
      else
        selectedIndex = closest[0].index;

      indices[y * targetStride + x] = (byte)selectedIndex;
    }
  }

  private static List<(int index, double distance)> _FindNClosestColors<TWork, TMetric>(
    TWork color,
    TWork[] palette,
    in TMetric metric,
    int n)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    if (palette.Length == 0) return [];

    var distances = new List<(int index, double distance)>(palette.Length);

    for (var i = 0; i < palette.Length; ++i) {
      var distance = (double)metric.Distance(color, palette[i]).ToFloat();
      distances.Add((i, distance));
    }

    distances.Sort((a, b) => a.distance.CompareTo(b.distance));

    var result = new List<(int index, double distance)>(Math.Min(n, distances.Count));
    for (var i = 0; i < n && i < distances.Count; ++i)
      result.Add(distances[i]);

    return result;
  }

  private static (double w0, double w1, double w2) _CalculateBarycentricWeights(
    (float c1, float c2, float c3, float a) p,
    (float c1, float c2, float c3, float a) c0,
    (float c1, float c2, float c3, float a) c1,
    (float c1, float c2, float c3, float a) c2) {

    // Convert to 3D vectors (color space)
    double px = p.c1, py = p.c2, pz = p.c3;
    double v0x = c0.c1, v0y = c0.c2, v0z = c0.c3;
    double v1x = c1.c1, v1y = c1.c2, v1z = c1.c3;
    double v2x = c2.c1, v2y = c2.c2, v2z = c2.c3;

    // Edge vectors
    var e1x = v1x - v0x;
    var e1y = v1y - v0y;
    var e1z = v1z - v0z;
    var e2x = v2x - v0x;
    var e2y = v2y - v0y;
    var e2z = v2z - v0z;
    var epx = px - v0x;
    var epy = py - v0y;
    var epz = pz - v0z;

    // Dot products
    var d11 = e1x * e1x + e1y * e1y + e1z * e1z;
    var d12 = e1x * e2x + e1y * e2y + e1z * e2z;
    var d22 = e2x * e2x + e2y * e2y + e2z * e2z;
    var d1p = e1x * epx + e1y * epy + e1z * epz;
    var d2p = e2x * epx + e2y * epy + e2z * epz;

    var denom = d11 * d22 - d12 * d12;

    if (Math.Abs(denom) < 1e-10)
      return (1.0, 0.0, 0.0);

    var invDenom = 1.0 / denom;
    var w1 = (d22 * d1p - d12 * d2p) * invDenom;
    var w2 = (d11 * d2p - d12 * d1p) * invDenom;
    var w0 = 1.0 - w1 - w2;

    // Clamp and normalize
    w0 = Math.Max(0, Math.Min(1, w0));
    w1 = Math.Max(0, Math.Min(1, w1));
    w2 = Math.Max(0, Math.Min(1, w2));

    var sum = w0 + w1 + w2;
    if (sum > 0) {
      w0 /= sum;
      w1 /= sum;
      w2 /= sum;
    }

    return (w0, w1, w2);
  }

  private static double[,] _GenerateBayerMatrix(int size) {
    var matrix = new double[size, size];
    var n = (int)Math.Log(size, 2);

    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x) {
      var value = 0;
      var bit = 0;

      for (var i = n - 1; i >= 0; --i) {
        var xBit = (x >> i) & 1;
        var yBit = (y >> i) & 1;
        value |= (xBit ^ yBit) << bit++;
        value |= yBit << bit++;
      }

      matrix[y, x] = (value + 0.5) / (size * size);
    }

    return matrix;
  }
}
