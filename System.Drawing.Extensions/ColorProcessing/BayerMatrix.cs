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

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Generates Bayer threshold matrices for ordered dithering and other
/// spatial threshold applications. A Bayer matrix of size <c>N</c> contains
/// the values <c>0..N*N-1</c> arranged so that iterated halving of the space
/// maximises dispersion — the defining property that makes it useful for
/// dithering, anti-aliasing patterns, stochastic sampling and similar tasks.
/// </summary>
/// <remarks>
/// Reference: B. Bayer, "An optimum method for two-level rendition of continuous-tone
/// pictures", IEEE Int. Conf. on Communications, vol. 1, 1973, pp. 26-11 to 26-15.
/// </remarks>
public static class BayerMatrix {

  /// <summary>
  /// Generates a Bayer threshold matrix of the specified size.
  /// </summary>
  /// <param name="size">Matrix side length. Must be a positive power of two.</param>
  /// <returns>A <c>size × size</c> float matrix whose cells contain the Bayer
  /// indices <c>0..size*size-1</c>, row-major.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="size"/>
  /// is not a positive power of two.</exception>
  public static float[,] Generate(int size) {
    if (!IsValidSize(size))
      throw new ArgumentOutOfRangeException(nameof(size), size, "Bayer matrix size must be a positive power of two (1, 2, 4, 8, 16, …).");

    var matrix = new float[size, size];
    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x)
      matrix[y, x] = _ComputeValue(x, y, size);
    return matrix;
  }

  /// <summary>
  /// Tests whether <paramref name="size"/> is an acceptable Bayer matrix size
  /// (positive power of two).
  /// </summary>
  public static bool IsValidSize(int size) => size > 0 && (size & (size - 1)) == 0;

  /// <summary>
  /// Computes a single Bayer matrix cell value without materialising the full matrix.
  /// Uses the classic recursive definition via bit-interleaving.
  /// </summary>
  internal static float _ComputeValue(int x, int y, int size) {
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
}
