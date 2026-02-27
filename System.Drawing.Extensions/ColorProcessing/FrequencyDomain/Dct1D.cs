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
/// 1D Discrete Cosine Transform (Type-II forward, Type-III inverse).
/// </summary>
public static class Dct1D {

  /// <summary>
  /// Computes the forward DCT-II in-place.
  /// </summary>
  /// <param name="data">The float data array to transform in-place.</param>
  public static void Forward(float[] data) {
    var n = data.Length;
    if (n <= 1)
      return;

    var result = new float[n];
    var scale = (float)Math.Sqrt(2.0 / n);

    for (var k = 0; k < n; ++k) {
      var sum = 0.0;
      for (var i = 0; i < n; ++i)
        sum += data[i] * Math.Cos(Math.PI * k * (2 * i + 1) / (2.0 * n));

      result[k] = (float)(sum * scale);
    }

    result[0] *= (float)(1.0 / Math.Sqrt(2.0));
    Array.Copy(result, data, n);
  }

  /// <summary>
  /// Computes the inverse DCT (Type-III) in-place.
  /// </summary>
  /// <param name="data">The float data array to transform in-place.</param>
  public static void Inverse(float[] data) {
    var n = data.Length;
    if (n <= 1)
      return;

    var result = new float[n];
    var scale = (float)Math.Sqrt(2.0 / n);
    var dc = data[0] * (float)Math.Sqrt(2.0);

    for (var i = 0; i < n; ++i) {
      var sum = dc * 0.5;
      for (var k = 1; k < n; ++k)
        sum += data[k] * Math.Cos(Math.PI * k * (2 * i + 1) / (2.0 * n));

      result[i] = (float)(sum * scale);
    }

    Array.Copy(result, data, n);
  }
}
