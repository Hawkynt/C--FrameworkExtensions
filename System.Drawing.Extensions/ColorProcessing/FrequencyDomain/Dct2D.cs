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

namespace Hawkynt.ColorProcessing.FrequencyDomain;

/// <summary>
/// 2D Discrete Cosine Transform by applying 1D DCT row-wise then column-wise.
/// </summary>
public static class Dct2D {

  /// <summary>
  /// Computes the forward 2D DCT in-place.
  /// </summary>
  /// <param name="data">The 2D float data array to transform.</param>
  public static void Forward(float[,] data) {
    var rows = data.GetLength(0);
    var cols = data.GetLength(1);

    // Transform rows
    var rowBuf = new float[cols];
    for (var r = 0; r < rows; ++r) {
      for (var c = 0; c < cols; ++c)
        rowBuf[c] = data[r, c];

      Dct1D.Forward(rowBuf);
      for (var c = 0; c < cols; ++c)
        data[r, c] = rowBuf[c];
    }

    // Transform columns
    var colBuf = new float[rows];
    for (var c = 0; c < cols; ++c) {
      for (var r = 0; r < rows; ++r)
        colBuf[r] = data[r, c];

      Dct1D.Forward(colBuf);
      for (var r = 0; r < rows; ++r)
        data[r, c] = colBuf[r];
    }
  }

  /// <summary>
  /// Computes the inverse 2D DCT in-place.
  /// </summary>
  /// <param name="data">The 2D float data array to transform.</param>
  public static void Inverse(float[,] data) {
    var rows = data.GetLength(0);
    var cols = data.GetLength(1);

    // Inverse columns first
    var colBuf = new float[rows];
    for (var c = 0; c < cols; ++c) {
      for (var r = 0; r < rows; ++r)
        colBuf[r] = data[r, c];

      Dct1D.Inverse(colBuf);
      for (var r = 0; r < rows; ++r)
        data[r, c] = colBuf[r];
    }

    // Inverse rows
    var rowBuf = new float[cols];
    for (var r = 0; r < rows; ++r) {
      for (var c = 0; c < cols; ++c)
        rowBuf[c] = data[r, c];

      Dct1D.Inverse(rowBuf);
      for (var c = 0; c < cols; ++c)
        data[r, c] = rowBuf[c];
    }
  }
}
