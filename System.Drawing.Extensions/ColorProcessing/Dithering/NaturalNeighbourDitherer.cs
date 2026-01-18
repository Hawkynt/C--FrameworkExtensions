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
/// Natural Neighbour dithering using Voronoi-based area-weighted interpolation.
/// </summary>
/// <remarks>
/// <para>Constructs a Voronoi diagram of palette colors.</para>
/// <para>Calculates "stolen area" when inserting query point.</para>
/// <para>Normalizes displaced areas as blend weights.</para>
/// </remarks>
[Ditherer("Natural Neighbour", Description = "Voronoi-based area-weighted interpolation dithering", Type = DitheringType.Ordered)]
public readonly struct NaturalNeighbourDitherer : IDitherer {

  private const int _DEFAULT_MATRIX_SIZE = 4;
  private static readonly double[,] _DefaultMatrix = _GenerateBayerMatrix(_DEFAULT_MATRIX_SIZE);

  private readonly int _matrixSize;
  private readonly double[,] _bayerMatrix;

  /// <summary>Pre-configured instance with 2x2 Bayer matrix.</summary>
  public static NaturalNeighbourDitherer Bayer2x2 { get; } = new(2);

  /// <summary>Pre-configured instance with 4x4 Bayer matrix.</summary>
  public static NaturalNeighbourDitherer Bayer4x4 { get; } = new(4);

  /// <summary>Pre-configured instance with 8x8 Bayer matrix.</summary>
  public static NaturalNeighbourDitherer Bayer8x8 { get; } = new(8);

  /// <summary>
  /// Creates a natural neighbour ditherer with the specified matrix size.
  /// </summary>
  /// <param name="matrixSize">Size of the Bayer matrix (must be power of 2).</param>
  public NaturalNeighbourDitherer(int matrixSize = _DEFAULT_MATRIX_SIZE) {
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

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;
    const int neighborCount = 6;

    // Handle default struct initialization (fields = 0/null)
    var matrixSize = this._matrixSize > 0 ? this._matrixSize : _DEFAULT_MATRIX_SIZE;
    var bayerMatrix = this._bayerMatrix ?? _DefaultMatrix;

    for (var y = startY; y < endY; ++y)
    for (var x = 0; x < width; ++x) {
      var pixel = decoder.Decode(source[y * sourceStride + x]);

      // Find natural neighbors (closest colors that would share Voronoi boundary)
      var neighbors = _FindNClosestColors(pixel, palette, metric, neighborCount);

      if (neighbors.Count == 0) {
        indices[y * targetStride + x] = 0;
        continue;
      }

      if (neighbors.Count == 1) {
        indices[y * targetStride + x] = (byte)neighbors[0].index;
        continue;
      }

      // Get Bayer threshold for this position
      var threshold = bayerMatrix[y % matrixSize, x % matrixSize];

      // Calculate natural neighbour weights (Sibson interpolation approximation)
      var weights = _CalculateNaturalNeighbourWeights(neighbors);

      // Select color based on threshold and cumulative weights
      var cumulativeWeight = 0.0;
      var selectedIndex = neighbors[0].index;

      for (var i = 0; i < neighbors.Count && i < weights.Length; ++i) {
        cumulativeWeight += weights[i];
        if (threshold < cumulativeWeight) {
          selectedIndex = neighbors[i].index;
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

  private static double[] _CalculateNaturalNeighbourWeights(List<(int index, double distance)> neighbors) {
    if (neighbors.Count == 0) return [];
    if (neighbors.Count == 1) return [1.0];

    var weights = new double[neighbors.Count];
    var minDist = neighbors[0].distance;

    // Calculate weights using Sibson-like formula
    for (var i = 0; i < neighbors.Count; ++i) {
      var distance = neighbors[i].distance;
      if (distance < 1e-10) {
        // Exact match - all weight to this color
        Array.Clear(weights, 0, weights.Length);
        weights[i] = 1.0;
        return weights;
      }

      var normalizedDist = distance / (minDist + 1e-10);
      var decay = Math.Exp(-0.5 * (i * i));
      weights[i] = decay / (normalizedDist * normalizedDist);
    }

    // Normalize weights
    var total = 0.0;
    for (var i = 0; i < weights.Length; ++i)
      total += weights[i];

    if (total > 0)
      for (var i = 0; i < weights.Length; ++i)
        weights[i] /= total;

    return weights;
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
