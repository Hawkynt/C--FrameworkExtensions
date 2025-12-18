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

#if !SUPPORTS_MATH_ILOGB

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MathPolyfills {

  extension(Math) {

    /// <summary>
    /// Returns the base 2 integer logarithm of a specified number.
    /// </summary>
    /// <param name="x">The number whose logarithm is to be found.</param>
    /// <returns>
    /// The base 2 integer logarithm of <paramref name="x"/>.
    /// Returns <see cref="int.MaxValue"/> if <paramref name="x"/> is positive infinity.
    /// Returns <see cref="int.MinValue"/> if <paramref name="x"/> is zero or negative infinity.
    /// Returns <see cref="int.MinValue"/> if <paramref name="x"/> is NaN.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ILogB(double x) {
      var bits = BitConverter.DoubleToInt64Bits(x);

      // Extract exponent (bits 52-62)
      var exponent = (int)((bits >> 52) & 0x7FF);

      if (exponent == 0) {
        // Zero or subnormal
        if ((bits & 0x7FFFFFFFFFFFFFFF) == 0)
          return int.MinValue; // Zero

        // Subnormal - count leading zeros in mantissa
        var mantissa = bits & 0xFFFFFFFFFFFFF;
        var shift = 0;
        while ((mantissa & 0x10000000000000) == 0) {
          mantissa <<= 1;
          ++shift;
        }
        return -1022 - shift;
      }

      if (exponent == 0x7FF) {
        // Infinity or NaN
        if ((bits & 0xFFFFFFFFFFFFF) == 0)
          return (bits & unchecked((long)0x8000000000000000)) != 0 ? int.MinValue : int.MaxValue; // Infinity
        return int.MinValue; // NaN
      }

      // Normal number
      return exponent - 1023;
    }

  }

}

#endif
