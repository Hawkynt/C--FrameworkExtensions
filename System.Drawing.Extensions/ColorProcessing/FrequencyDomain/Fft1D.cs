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
/// 1D Fast Fourier Transform using Cooley-Tukey radix-2 algorithm.
/// </summary>
public static class Fft1D {

  /// <summary>
  /// Computes the forward FFT in-place. Input array length must be a power of 2 (zero-padded if needed).
  /// </summary>
  /// <param name="data">The complex data array to transform in-place.</param>
  public static void Forward(Complex[] data) {
    _ValidateAndPad(ref data);
    _Transform(data, false);
  }

  /// <summary>
  /// Computes the inverse FFT in-place.
  /// </summary>
  /// <param name="data">The complex data array to transform in-place.</param>
  public static void Inverse(Complex[] data) {
    _ValidateAndPad(ref data);
    _Transform(data, true);
    var n = data.Length;
    var invN = 1f / n;
    for (var i = 0; i < n; ++i)
      data[i] = data[i] * invN;
  }

  /// <summary>
  /// Returns the next power of 2 greater than or equal to n.
  /// </summary>
  internal static int NextPowerOf2(int n) {
    if (n <= 1) return 1;
    var p = 1;
    while (p < n)
      p <<= 1;
    return p;
  }

  /// <summary>
  /// Zero-pads data to the next power of 2 if not already a power of 2.
  /// </summary>
  internal static void _ValidateAndPad(ref Complex[] data) {
    var n = data.Length;
    var np2 = NextPowerOf2(n);
    if (np2 == n)
      return;

    var padded = new Complex[np2];
    Array.Copy(data, padded, n);
    data = padded;
  }

  private static void _Transform(Complex[] data, bool inverse) {
    var n = data.Length;
    if (n <= 1)
      return;

    // Bit-reversal permutation
    var j = 0;
    for (var i = 0; i < n - 1; ++i) {
      if (i < j)
        (data[i], data[j]) = (data[j], data[i]);

      var m = n >> 1;
      while (m >= 1 && j >= m) {
        j -= m;
        m >>= 1;
      }

      j += m;
    }

    // Cooley-Tukey iterative FFT
    for (var len = 2; len <= n; len <<= 1) {
      var halfLen = len >> 1;
      var angle = (float)(2.0 * Math.PI / len) * (inverse ? -1f : 1f);
      var wBase = Complex.FromPolar(1f, angle);

      for (var i = 0; i < n; i += len) {
        var w = Complex.One;
        for (var k = 0; k < halfLen; ++k) {
          var t = w * data[i + k + halfLen];
          var u = data[i + k];
          data[i + k] = u + t;
          data[i + k + halfLen] = u - t;
          w = w * wBase;
        }
      }
    }
  }
}
