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

#if !SUPPORTS_MATH_SCALEB

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MathPolyfills {

  extension(Math) {

    /// <summary>
    /// Returns x * 2^n computed efficiently.
    /// </summary>
    /// <param name="x">A double-precision floating-point number that specifies the base value.</param>
    /// <param name="n">A 32-bit integer that specifies the power.</param>
    /// <returns>x * 2^n computed efficiently.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ScaleB(double x, int n) {
      // Handle special cases
      if (double.IsNaN(x) || double.IsInfinity(x) || x == 0.0)
        return x;

      if (n == 0)
        return x;

      // For large n, use iterative multiplication to avoid overflow in intermediate steps
      if (n > 0) {
        while (n > 1023) {
          x *= 8.98846567431158e+307; // 2^1023
          n -= 1023;
        }
        return x * _GetPowerOf2(n);
      }

      // n < 0
      while (n < -1022) {
        x *= 2.2250738585072014e-308; // 2^-1022
        n += 1022;
      }
      return x * _GetPowerOf2(n);
    }

    private static double _GetPowerOf2(int n) {
      // Build 2^n using bit manipulation
      if (n is < -1022 or > 1023)
        return Math.Pow(2, n);
      var bits = (long)(n + 1023) << 52;
      return BitConverter.Int64BitsToDouble(bits);
    }

  }

}

#endif
