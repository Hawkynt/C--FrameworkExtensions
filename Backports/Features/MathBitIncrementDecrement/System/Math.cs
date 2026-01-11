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

// Math.BitIncrement and Math.BitDecrement were added in .NET Core 3.0
#if !SUPPORTS_MATH_COPYSIGN

namespace System;

public static partial class MathPolyfills {

  extension(Math) {

    /// <summary>
    /// Returns the smallest value that compares greater than a specified value.
    /// </summary>
    /// <param name="x">The value to increment.</param>
    /// <returns>The smallest value that compares greater than <paramref name="x"/>, or <see cref="double.PositiveInfinity"/> if <paramref name="x"/> equals <see cref="double.PositiveInfinity"/>, or <see cref="double.NaN"/> if <paramref name="x"/> equals <see cref="double.NaN"/>.</returns>
    public static double BitIncrement(double x) {
      var bits = BitConverter.DoubleToInt64Bits(x);

      // Handle NaN and positive infinity - return as is
      if ((bits & 0x7FF0000000000000) >= 0x7FF0000000000000)
        // If negative infinity, return double.MinValue
        return bits == unchecked((long)0xFFF0000000000000) ? double.MinValue : x;

      // Handle -0.0, return smallest positive (double.Epsilon)
      if (bits == unchecked((long)0x8000000000000000))
        return double.Epsilon;

      // For negative numbers, decrement the bits (which increases the value towards zero)
      // For positive numbers, increment the bits (which increases the value away from zero)
      bits += bits < 0 ? -1 : +1;
      return BitConverter.Int64BitsToDouble(bits);
    }

    /// <summary>
    /// Returns the largest value that compares less than a specified value.
    /// </summary>
    /// <param name="x">The value to decrement.</param>
    /// <returns>The largest value that compares less than <paramref name="x"/>, or <see cref="double.NegativeInfinity"/> if <paramref name="x"/> equals <see cref="double.NegativeInfinity"/>, or <see cref="double.NaN"/> if <paramref name="x"/> equals <see cref="double.NaN"/>.</returns>
    public static double BitDecrement(double x) {
      var bits = BitConverter.DoubleToInt64Bits(x);

      // Handle NaN and positive infinity
      if ((bits & 0x7FF0000000000000) >= 0x7FF0000000000000)
        // If positive infinity, return double.MaxValue
        return bits == 0x7FF0000000000000 ? double.MaxValue : x;

      // Handle +0.0, return smallest negative (-double.Epsilon)
      if (bits == 0x0000000000000000)
        return -double.Epsilon;

      // For negative numbers, increment the bits (which decreases the value away from zero)
      // For positive numbers, decrement the bits (which decreases the value towards zero)
      bits += bits < 0 ? +1 : -1;
      return BitConverter.Int64BitsToDouble(bits);
    }

  }

}

#endif
