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

#if !SUPPORTS_MATH_BIGMUL_INT64

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MathPolyfills {

  extension(Math) {

    /// <summary>
    /// Produces the full product of two 64-bit signed integers.
    /// </summary>
    /// <param name="a">The first number to multiply.</param>
    /// <param name="b">The second number to multiply.</param>
    /// <returns>The full product of the specified numbers as an <see cref="Int128"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int128 BigMul(long a, long b) {
      var high = _BigMulUnsigned((ulong)a, (ulong)b, out var low);

      // Handle sign
      if (a < 0)
        high -= (ulong)b;
      if (b < 0)
        high -= (ulong)a;

      return new((ulong)high, low);
    }

    /// <summary>
    /// Produces the full product of two 64-bit unsigned integers.
    /// </summary>
    /// <param name="a">The first number to multiply.</param>
    /// <param name="b">The second number to multiply.</param>
    /// <returns>The full product of the specified numbers as a <see cref="UInt128"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 BigMul(ulong a, ulong b) {
      var high = _BigMulUnsigned(a, b, out var low);
      return new(high, low);
    }

    /// <summary>
    /// Produces the full product of two 64-bit signed integers, returning the high 64 bits and the low 64 bits separately.
    /// </summary>
    /// <param name="a">The first number to multiply.</param>
    /// <param name="b">The second number to multiply.</param>
    /// <param name="low">The low 64 bits of the product.</param>
    /// <returns>The high 64 bits of the product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long BigMul(long a, long b, out long low) {
      var highUnsigned = _BigMulUnsigned((ulong)a, (ulong)b, out var lowUnsigned);

      // Handle sign
      if (a < 0)
        highUnsigned -= (ulong)b;
      if (b < 0)
        highUnsigned -= (ulong)a;

      low = (long)lowUnsigned;
      return (long)highUnsigned;
    }

    /// <summary>
    /// Produces the full product of two 64-bit unsigned integers, returning the high 64 bits and the low 64 bits separately.
    /// </summary>
    /// <param name="a">The first number to multiply.</param>
    /// <param name="b">The second number to multiply.</param>
    /// <param name="low">The low 64 bits of the product.</param>
    /// <returns>The high 64 bits of the product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong BigMul(ulong a, ulong b, out ulong low)
      => _BigMulUnsigned(a, b, out low);

  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _BigMulUnsigned(ulong a, ulong b, out ulong low) {
    // Split into 32-bit parts
    var aLow = (uint)a;
    var aHigh = (uint)(a >> 32);
    var bLow = (uint)b;
    var bHigh = (uint)(b >> 32);

    // Compute partial products
    var lowLow = (ulong)aLow * bLow;
    var lowHigh = (ulong)aLow * bHigh;
    var highLow = (ulong)aHigh * bLow;
    var highHigh = (ulong)aHigh * bHigh;

    // Sum partial products
    var middle = lowHigh + (lowLow >> 32);
    middle += highLow;

    // Handle carry
    if (middle < highLow)
      highHigh += 1UL << 32;

    low = (middle << 32) | (uint)lowLow;
    return highHigh + (middle >> 32);
  }

}

#endif
