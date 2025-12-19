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

#if !SUPPORTS_INT64_STATICMETHODS

using System.Numerics;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class Int64Polyfills {
  extension(long) {

    /// <summary>
    /// Computes the absolute value of a value.
    /// </summary>
    /// <param name="value">The value for which to get its absolute value.</param>
    /// <returns>The absolute value of <paramref name="value"/>.</returns>
    /// <exception cref="OverflowException"><paramref name="value"/> is <see cref="long.MinValue"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Abs(long value)
      => value == long.MinValue ? throw new OverflowException("Negating the minimum value of a twos complement number is invalid.") : value < 0 ? -value : value;

    /// <summary>
    /// Computes the quotient and remainder of two values.
    /// </summary>
    /// <param name="left">The value which <paramref name="right"/> divides.</param>
    /// <param name="right">The value which divides <paramref name="left"/>.</param>
    /// <returns>The quotient and remainder of <paramref name="left"/> divided-by <paramref name="right"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (long Quotient, long Remainder) DivRem(long left, long right)
      => (left / right, left % right);

    /// <summary>
    /// Determines if a value represents an even integral number.
    /// </summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is an even integer; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEvenInteger(long value)
      => (value & 1) == 0;

    /// <summary>
    /// Determines if a value is negative.
    /// </summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is negative; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegative(long value)
      => value < 0;

    /// <summary>
    /// Determines if a value represents an odd integral number.
    /// </summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is an odd integer; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOddInteger(long value)
      => (value & 1) != 0;

    /// <summary>
    /// Determines if a value is positive.
    /// </summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is positive; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositive(long value)
      => value >= 0;

    /// <summary>
    /// Computes the number of leading zeros in a value.
    /// </summary>
    /// <param name="value">The value whose leading zeroes are to be counted.</param>
    /// <returns>The number of leading zeros in <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LeadingZeroCount(long value)
      => BitOperations.LeadingZeroCount((ulong)value);

    /// <summary>
    /// Computes the log2 of a value.
    /// </summary>
    /// <param name="value">The value whose log2 is to be computed.</param>
    /// <returns>The log2 of <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Log2(long value)
      => BitOperations.Log2((ulong)value);

    /// <summary>
    /// Compares two values to compute which is greater.
    /// </summary>
    /// <param name="x">The value to compare with <paramref name="y"/>.</param>
    /// <param name="y">The value to compare with <paramref name="x"/>.</param>
    /// <returns><paramref name="x"/> if it is greater than <paramref name="y"/>; otherwise, <paramref name="y"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Max(long x, long y)
      => x > y ? x : y;

    /// <summary>
    /// Compares two values to compute which has the greater magnitude.
    /// </summary>
    /// <param name="x">The value to compare with <paramref name="y"/>.</param>
    /// <param name="y">The value to compare with <paramref name="x"/>.</param>
    /// <returns><paramref name="x"/> if it has a greater magnitude than <paramref name="y"/>; otherwise, <paramref name="y"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long MaxMagnitude(long x, long y) {
      var absX = x < 0 ? (x == long.MinValue ? long.MaxValue : -x) : x;
      var absY = y < 0 ? (y == long.MinValue ? long.MaxValue : -y) : y;
      return absX > absY ? x : absX < absY ? y : x > y ? x : y;
    }

    /// <summary>
    /// Compares two values to compute which is lesser.
    /// </summary>
    /// <param name="x">The value to compare with <paramref name="y"/>.</param>
    /// <param name="y">The value to compare with <paramref name="x"/>.</param>
    /// <returns><paramref name="x"/> if it is less than <paramref name="y"/>; otherwise, <paramref name="y"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Min(long x, long y)
      => x < y ? x : y;

    /// <summary>
    /// Compares two values to compute which has the lesser magnitude.
    /// </summary>
    /// <param name="x">The value to compare with <paramref name="y"/>.</param>
    /// <param name="y">The value to compare with <paramref name="x"/>.</param>
    /// <returns><paramref name="x"/> if it has a lesser magnitude than <paramref name="y"/>; otherwise, <paramref name="y"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long MinMagnitude(long x, long y) {
      var absX = x < 0 ? (x == long.MinValue ? long.MaxValue : -x) : x;
      var absY = y < 0 ? (y == long.MinValue ? long.MaxValue : -y) : y;
      return absX < absY ? x : absX > absY ? y : x < y ? x : y;
    }

    /// <summary>
    /// Computes the number of bits that are set in a value.
    /// </summary>
    /// <param name="value">The value whose set bits are to be counted.</param>
    /// <returns>The number of set bits in <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PopCount(long value)
      => BitOperations.PopCount((ulong)value);

    /// <summary>
    /// Rotates a value left by a given amount.
    /// </summary>
    /// <param name="value">The value which is rotated left by <paramref name="rotateAmount"/>.</param>
    /// <param name="rotateAmount">The amount by which <paramref name="value"/> is rotated left.</param>
    /// <returns>The result of rotating <paramref name="value"/> left by <paramref name="rotateAmount"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RotateLeft(long value, int rotateAmount)
      => (long)BitOperations.RotateLeft((ulong)value, rotateAmount);

    /// <summary>
    /// Rotates a value right by a given amount.
    /// </summary>
    /// <param name="value">The value which is rotated right by <paramref name="rotateAmount"/>.</param>
    /// <param name="rotateAmount">The amount by which <paramref name="value"/> is rotated right.</param>
    /// <returns>The result of rotating <paramref name="value"/> right by <paramref name="rotateAmount"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RotateRight(long value, int rotateAmount)
      => (long)BitOperations.RotateRight((ulong)value, rotateAmount);

    /// <summary>
    /// Computes the sign of a value.
    /// </summary>
    /// <param name="value">The value whose sign is to be computed.</param>
    /// <returns>A positive value if <paramref name="value"/> is positive, 0 if <paramref name="value"/> is zero, and a negative value if <paramref name="value"/> is negative.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Sign(long value)
      => value < 0 ? -1 : value > 0 ? 1 : 0;

    /// <summary>
    /// Computes the number of trailing zeros in a value.
    /// </summary>
    /// <param name="value">The value whose trailing zeroes are to be counted.</param>
    /// <returns>The number of trailing zeros in <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int TrailingZeroCount(long value)
      => BitOperations.TrailingZeroCount(value);

    /// <summary>
    /// Clamps a value to an inclusive minimum and maximum value.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The inclusive minimum to which <paramref name="value"/> should clamp.</param>
    /// <param name="max">The inclusive maximum to which <paramref name="value"/> should clamp.</param>
    /// <returns>The result of clamping <paramref name="value"/> to the inclusive range of <paramref name="min"/> and <paramref name="max"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Clamp(long value, long min, long max)
      => value < min ? min : value > max ? max : value;

    /// <summary>
    /// Copies the sign of a value to the sign of another value.
    /// </summary>
    /// <param name="value">The value whose magnitude is used in the result.</param>
    /// <param name="sign">The value whose sign is used in the result.</param>
    /// <returns>A value with the magnitude of <paramref name="value"/> and the sign of <paramref name="sign"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CopySign(long value, long sign) {
      var absValue = value < 0 ? (value == long.MinValue ? throw new OverflowException() : -value) : value;
      return sign < 0 ? -absValue : absValue;
    }

  }
}

#endif
