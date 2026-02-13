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
/// 2D Fast Fourier Transform by applying 1D FFT row-wise then column-wise.
/// </summary>
public static class Fft2D {

  /// <summary>
  /// Computes the forward 2D FFT in-place.
  /// </summary>
  /// <param name="data">The 2D complex data array to transform.</param>
  public static void Forward(Complex[,] data) => _Transform(data, false);

  /// <summary>
  /// Computes the inverse 2D FFT in-place.
  /// </summary>
  /// <param name="data">The 2D complex data array to transform.</param>
  public static void Inverse(Complex[,] data) => _Transform(data, true);

  private static void _Transform(Complex[,] data, bool inverse) {
    var rows = data.GetLength(0);
    var cols = data.GetLength(1);

    // Transform rows
    var rowBuf = new Complex[cols];
    for (var r = 0; r < rows; ++r) {
      for (var c = 0; c < cols; ++c)
        rowBuf[c] = data[r, c];

      if (inverse)
        Fft1D.Inverse(rowBuf);
      else
        Fft1D.Forward(rowBuf);

      var usedCols = Math.Min(cols, rowBuf.Length);
      for (var c = 0; c < usedCols; ++c)
        data[r, c] = rowBuf[c];
    }

    // Transform columns
    var colBuf = new Complex[rows];
    for (var c = 0; c < cols; ++c) {
      for (var r = 0; r < rows; ++r)
        colBuf[r] = data[r, c];

      if (inverse)
        Fft1D.Inverse(colBuf);
      else
        Fft1D.Forward(colBuf);

      var usedRows = Math.Min(rows, colBuf.Length);
      for (var r = 0; r < usedRows; ++r)
        data[r, c] = colBuf[r];
    }
  }
}
