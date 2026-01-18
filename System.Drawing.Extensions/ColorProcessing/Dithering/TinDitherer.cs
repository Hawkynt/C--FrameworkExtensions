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
/// Triangulated Irregular Network (TIN) dithering using Delaunay-like tessellation.
/// </summary>
/// <remarks>
/// <para>Finds 4 closest palette colors to form a tetrahedron in color space.</para>
/// <para>Uses barycentric/inverse distance weights for color selection.</para>
/// <para>Combines with Bayer ordered dithering for smooth transitions.</para>
/// </remarks>
[Ditherer("TIN", Description = "Tetrahedral interpolation with Bayer pattern", Type = DitheringType.Ordered)]
public readonly struct TinDitherer : IDitherer {

  private const int _DEFAULT_MATRIX_SIZE = 4;
  private static readonly double[,] _DefaultMatrix = _GenerateBayerMatrix(_DEFAULT_MATRIX_SIZE);

  private readonly int _matrixSize;
  private readonly double[,] _bayerMatrix;

  /// <summary>Pre-configured instance with 2x2 Bayer matrix.</summary>
  public static TinDitherer Bayer2x2 { get; } = new(2);

  /// <summary>Pre-configured instance with 4x4 Bayer matrix.</summary>
  public static TinDitherer Bayer4x4 { get; } = new(4);

  /// <summary>Pre-configured instance with 8x8 Bayer matrix.</summary>
  public static TinDitherer Bayer8x8 { get; } = new(8);

  /// <summary>
  /// Creates a TIN ditherer with the specified matrix size.
  /// </summary>
  /// <param name="matrixSize">Size of the Bayer matrix (must be power of 2).</param>
  public TinDitherer(int matrixSize = _DEFAULT_MATRIX_SIZE) {
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
      var (pc1, pc2, pc3, palpha) = pixel.ToNormalized();
      var pixelNormalized = (pc1.ToFloat(), pc2.ToFloat(), pc3.ToFloat(), palpha.ToFloat());

      // Find 4 closest colors for tetrahedron
      var closest = _FindNClosestColors(pixel, palette, metric, 4);

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

      // Calculate tetrahedral barycentric weights
      var closestColorsList = new List<(float c1, float c2, float c3, float a)>(closest.Count);
      foreach (var c in closest)
        closestColorsList.Add(paletteColors[c.index]);

      var weights = _CalculateTetrahedralWeights(pixelNormalized, closestColorsList);

      // Select color based on threshold and cumulative weights
      var cumulativeWeight = 0.0;
      var selectedIndex = closest[0].index;

      for (var i = 0; i < closest.Count && i < weights.Length; ++i) {
        cumulativeWeight += weights[i];
        if (threshold < cumulativeWeight) {
          selectedIndex = closest[i].index;
          break;
        }
      }

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

  private static double[] _CalculateTetrahedralWeights(
    (float c1, float c2, float c3, float a) p,
    List<(float c1, float c2, float c3, float a)> vertices) {

    if (vertices.Count < 2)
      return vertices.Count == 1 ? [1.0] : [];

    if (vertices.Count == 2) {
      // Linear interpolation between 2 colors
      var d0 = _ColorDistance(p, vertices[0]);
      var d1 = _ColorDistance(p, vertices[1]);
      var total = d0 + d1;
      if (total < 1e-10) return [0.5, 0.5];
      return [d1 / total, d0 / total];
    }

    if (vertices.Count == 3)
      return _CalculateTriangularWeights(p, vertices[0], vertices[1], vertices[2]);

    // Full tetrahedral weights for 4 vertices using inverse distance
    return _CalculateTetrahedralWeightsInternal(p, vertices[0], vertices[1], vertices[2], vertices[3]);
  }

  private static double[] _CalculateTriangularWeights(
    (float c1, float c2, float c3, float a) p,
    (float c1, float c2, float c3, float a) v0,
    (float c1, float c2, float c3, float a) v1,
    (float c1, float c2, float c3, float a) v2) {

    var d0 = _ColorDistance(p, v0);
    var d1 = _ColorDistance(p, v1);
    var d2 = _ColorDistance(p, v2);

    // Inverse distance weighting
    var invD0 = d0 < 1e-10 ? 1e10 : 1.0 / d0;
    var invD1 = d1 < 1e-10 ? 1e10 : 1.0 / d1;
    var invD2 = d2 < 1e-10 ? 1e10 : 1.0 / d2;

    var total = invD0 + invD1 + invD2;
    return [invD0 / total, invD1 / total, invD2 / total];
  }

  private static double[] _CalculateTetrahedralWeightsInternal(
    (float c1, float c2, float c3, float a) p,
    (float c1, float c2, float c3, float a) v0,
    (float c1, float c2, float c3, float a) v1,
    (float c1, float c2, float c3, float a) v2,
    (float c1, float c2, float c3, float a) v3) {

    var d0 = _ColorDistance(p, v0);
    var d1 = _ColorDistance(p, v1);
    var d2 = _ColorDistance(p, v2);
    var d3 = _ColorDistance(p, v3);

    var invD0 = d0 < 1e-10 ? 1e10 : 1.0 / d0;
    var invD1 = d1 < 1e-10 ? 1e10 : 1.0 / d1;
    var invD2 = d2 < 1e-10 ? 1e10 : 1.0 / d2;
    var invD3 = d3 < 1e-10 ? 1e10 : 1.0 / d3;

    var total = invD0 + invD1 + invD2 + invD3;
    return [invD0 / total, invD1 / total, invD2 / total, invD3 / total];
  }

  private static double _ColorDistance(
    (float c1, float c2, float c3, float a) a,
    (float c1, float c2, float c3, float a) b) {
    var dc1 = a.c1 - b.c1;
    var dc2 = a.c2 - b.c2;
    var dc3 = a.c3 - b.c3;
    return Math.Sqrt(dc1 * dc1 + dc2 * dc2 + dc3 * dc3);
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
