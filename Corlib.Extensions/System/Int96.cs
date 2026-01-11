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
//

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Represents a 96-bit signed integer.
/// </summary>
public readonly struct Int96 : IComparable, IComparable<Int96>, IEquatable<Int96>, IFormattable, IParsable<Int96> {
  /// <summary>
  /// Gets the lower 64 bits of the 96-bit value.
  /// </summary>
  internal ulong Lower { get; }

  /// <summary>
  /// Gets the upper 32 bits of the 96-bit value.
  /// </summary>
  internal uint Upper { get; }

  /// <summary>
  /// Initializes a new instance of Int96 with the specified upper and lower values.
  /// </summary>
  public Int96(uint upper, ulong lower) {
    this.Upper = upper;
    this.Lower = lower;
  }

  /// <summary>
  /// Gets the value 0 as an Int96.
  /// </summary>
  public static Int96 Zero => new(0, 0);

  /// <summary>
  /// Gets the value 1 as an Int96.
  /// </summary>
  public static Int96 One => new(0, 1);

  /// <summary>
  /// Gets the value -1 as an Int96.
  /// </summary>
  public static Int96 NegativeOne => new(uint.MaxValue, ulong.MaxValue);

  /// <summary>
  /// Gets the maximum value of Int96 (2^95 - 1).
  /// </summary>
  public static Int96 MaxValue => new(0x7FFFFFFF, ulong.MaxValue);

  /// <summary>
  /// Gets the minimum value of Int96 (-2^95).
  /// </summary>
  public static Int96 MinValue => new(0x80000000, 0);

  /// <summary>
  /// Determines whether the specified value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(Int96 value) => (int)value.Upper < 0;

  /// <summary>
  /// Determines whether the specified value is positive.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPositive(Int96 value) => (int)value.Upper >= 0;

  /// <summary>
  /// Determines whether the specified value is an even integer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsEvenInteger(Int96 value) => (value.Lower & 1) == 0;

  /// <summary>
  /// Determines whether the specified value is an odd integer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsOddInteger(Int96 value) => (value.Lower & 1) != 0;

  /// <summary>
  /// Determines whether the specified value is a power of two.
  /// </summary>
  public static bool IsPow2(Int96 value) {
    if (IsNegative(value))
      return false;
    var popCount = _PopCount(value.Upper) + _PopCount(value.Lower);
    return popCount == 1;
  }

  /// <summary>
  /// Returns the absolute value.
  /// </summary>
  public static Int96 Abs(Int96 value) => IsNegative(value) ? -value : value;

  /// <summary>
  /// Clamps a value to the specified range.
  /// </summary>
  public static Int96 Clamp(Int96 value, Int96 min, Int96 max) {
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns a value with the same magnitude as value and the sign of sign.
  /// </summary>
  public static Int96 CopySign(Int96 value, Int96 sign) {
    var absValue = Abs(value);
    return IsNegative(sign) ? -absValue : absValue;
  }

  /// <summary>
  /// Returns the larger of two values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Int96 Max(Int96 x, Int96 y) => x >= y ? x : y;

  /// <summary>
  /// Returns the smaller of two values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Int96 Min(Int96 x, Int96 y) => x <= y ? x : y;

  /// <summary>
  /// Returns the value with the larger magnitude.
  /// </summary>
  public static Int96 MaxMagnitude(Int96 x, Int96 y) {
    var absX = Abs(x);
    var absY = Abs(y);
    return absX >= absY ? x : y;
  }

  /// <summary>
  /// Returns the value with the smaller magnitude.
  /// </summary>
  public static Int96 MinMagnitude(Int96 x, Int96 y) {
    var absX = Abs(x);
    var absY = Abs(y);
    return absX <= absY ? x : y;
  }

  /// <summary>
  /// Returns an indication of the sign of a value.
  /// </summary>
  public static int Sign(Int96 value) {
    if (IsNegative(value))
      return -1;
    if (value == Zero)
      return 0;
    return 1;
  }

  /// <summary>
  /// Computes the quotient and remainder of two values.
  /// </summary>
  public static (Int96 Quotient, Int96 Remainder) DivRem(Int96 left, Int96 right) {
    var quotient = left / right;
    var remainder = left - (quotient * right);
    return (quotient, remainder);
  }

  /// <summary>
  /// Returns the number of leading zeros.
  /// </summary>
  public static int LeadingZeroCount(Int96 value) {
    if (value.Upper != 0)
      return _LeadingZeroCount(value.Upper);
    return 32 + _LeadingZeroCount(value.Lower);
  }

  /// <summary>
  /// Returns the number of trailing zeros.
  /// </summary>
  public static int TrailingZeroCount(Int96 value) {
    if (value.Lower != 0)
      return _TrailingZeroCount(value.Lower);
    return 64 + _TrailingZeroCount(value.Upper);
  }

  /// <summary>
  /// Returns the population count (number of bits set).
  /// </summary>
  public static int PopCount(Int96 value) => _PopCount(value.Upper) + _PopCount(value.Lower);

  /// <summary>
  /// Returns the base-2 logarithm of a value.
  /// </summary>
  public static int Log2(Int96 value) {
    if (value <= Zero)
      throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
    return 95 - LeadingZeroCount(value);
  }

  /// <summary>
  /// Rotates a value left by the specified amount.
  /// </summary>
  public static Int96 RotateLeft(Int96 value, int rotateAmount) {
    rotateAmount &= 95;
    if (rotateAmount == 0)
      return value;
    return (value << rotateAmount) | (value >>> (96 - rotateAmount));
  }

  /// <summary>
  /// Rotates a value right by the specified amount.
  /// </summary>
  public static Int96 RotateRight(Int96 value, int rotateAmount) {
    rotateAmount &= 95;
    if (rotateAmount == 0)
      return value;
    return (value >>> rotateAmount) | (value << (96 - rotateAmount));
  }

  // Comparison methods
  public int CompareTo(Int96 other) {
    if (IsNegative(this) && !IsNegative(other))
      return -1;
    if (!IsNegative(this) && IsNegative(other))
      return 1;

    var upperCmp = this.Upper.CompareTo(other.Upper);
    return upperCmp != 0 ? upperCmp : this.Lower.CompareTo(other.Lower);
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;

    if (obj is not Int96 other)
      throw new ArgumentException("Object must be of type Int96.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Int96 other) => this.Upper == other.Upper && this.Lower == other.Lower;

  public override bool Equals(object? obj) => obj is Int96 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => unchecked((int)(this.Upper ^ this.Lower ^ (this.Lower >> 32)));

  // ToString methods
  public override string ToString() => _ToDecimalString(this);

  public string ToString(IFormatProvider? provider) => _ToDecimalString(this);

  public string ToString(string? format) => _ToDecimalString(this);

  public string ToString(string? format, IFormatProvider? provider) => _ToDecimalString(this);

  private static string _ToDecimalString(Int96 value) {
    if (value == Zero)
      return "0";

    var isNegative = IsNegative(value);
    if (isNegative)
      value = -value;

    var chars = new char[30]; // 96 bits can represent up to 29 decimal digits + sign
    var pos = chars.Length;

    while (value != Zero) {
      var (quotient, remainder) = _DivRemUnsigned(value, 10);
      chars[--pos] = (char)('0' + (int)remainder.Lower);
      value = quotient;
    }

    if (isNegative)
      chars[--pos] = '-';

    return new string(chars, pos, chars.Length - pos);
  }

  // Parse methods
  public static Int96 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static Int96 Parse(string s, NumberStyles style) => Parse(s, style, null);

  public static Int96 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static Int96 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    if (!TryParse(s, style, provider, out var result))
      throw new FormatException("Input string was not in a correct format.");
    return result;
  }

  public static bool TryParse(string? s, out Int96 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Int96 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Int96 result) {
    result = Zero;
    if (string.IsNullOrWhiteSpace(s))
      return false;

    s = s!.Trim();
    var isNegative = false;
    var startIndex = 0;

    if (s[0] == '-') {
      isNegative = true;
      startIndex = 1;
    } else if (s[0] == '+')
      startIndex = 1;

    var value = Zero;
    for (var i = startIndex; i < s.Length; ++i) {
      var c = s[i];
      if (c < '0' || c > '9')
        return false;

      value = value * 10 + (uint)(c - '0');
    }

    result = isNegative ? -value : value;
    return true;
  }

  // Operators
  public static bool operator ==(Int96 left, Int96 right) => left.Equals(right);
  public static bool operator !=(Int96 left, Int96 right) => !left.Equals(right);

  public static bool operator <(Int96 left, Int96 right) => left.CompareTo(right) < 0;
  public static bool operator >(Int96 left, Int96 right) => left.CompareTo(right) > 0;
  public static bool operator <=(Int96 left, Int96 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(Int96 left, Int96 right) => left.CompareTo(right) >= 0;

  public static Int96 operator +(Int96 value) => value;

  public static Int96 operator -(Int96 value) {
    var lower = ~value.Lower + 1;
    var upper = ~value.Upper;
    if (lower == 0)
      ++upper;
    return new(upper, lower);
  }

  public static Int96 operator ++(Int96 value) => value + One;
  public static Int96 operator --(Int96 value) => value - One;

  public static Int96 operator +(Int96 left, Int96 right) {
    var lower = left.Lower + right.Lower;
    var carry = lower < left.Lower ? 1U : 0U;
    var upper = left.Upper + right.Upper + carry;
    return new(upper, lower);
  }

  public static Int96 operator -(Int96 left, Int96 right) => left + (-right);

  public static Int96 operator *(Int96 left, Int96 right) {
    // Use grade-school multiplication with 32-bit words
    var a0 = (uint)left.Lower;
    var a1 = (uint)(left.Lower >> 32);
    var a2 = left.Upper;

    var b0 = (uint)right.Lower;
    var b1 = (uint)(right.Lower >> 32);
    var b2 = right.Upper;

    var r0 = (ulong)a0 * b0;
    var r1 = (ulong)a0 * b1 + (ulong)a1 * b0;
    var r2 = (ulong)a0 * b2 + (ulong)a1 * b1 + (ulong)a2 * b0;

    var c0 = (uint)r0;
    r1 += r0 >> 32;
    var c1 = (uint)r1;
    r2 += r1 >> 32;
    var c2 = (uint)r2;

    return new(c2, ((ulong)c1 << 32) | c0);
  }

  public static Int96 operator /(Int96 left, Int96 right) {
    if (right == Zero)
      throw new DivideByZeroException();

    var leftNeg = IsNegative(left);
    var rightNeg = IsNegative(right);

    if (leftNeg)
      left = -left;
    if (rightNeg)
      right = -right;

    var (quotient, _) = _DivRemUnsigned(left, right);

    return leftNeg != rightNeg ? -quotient : quotient;
  }

  public static Int96 operator %(Int96 left, Int96 right) {
    if (right == Zero)
      throw new DivideByZeroException();

    var leftNeg = IsNegative(left);
    var rightNeg = IsNegative(right);

    if (leftNeg)
      left = -left;
    if (rightNeg)
      right = -right;

    var (_, remainder) = _DivRemUnsigned(left, right);

    return leftNeg ? -remainder : remainder;
  }

  private static (Int96 Quotient, Int96 Remainder) _DivRemUnsigned(Int96 left, Int96 right) {
    if (right == Zero)
      throw new DivideByZeroException();
    if (left == Zero)
      return (Zero, Zero);
    if (left < right)
      return (Zero, left);

    var quotient = Zero;
    var remainder = Zero;

    for (var i = 95; i >= 0; --i) {
      remainder <<= 1;
      if (_GetBit(left, i))
        remainder += One;

      if (remainder >= right) {
        remainder -= right;
        quotient = _SetBit(quotient, i);
      }
    }

    return (quotient, remainder);
  }

  private static bool _GetBit(Int96 value, int bit) {
    if (bit < 64)
      return (value.Lower & (1UL << bit)) != 0;
    return (value.Upper & (1U << (bit - 64))) != 0;
  }

  private static Int96 _SetBit(Int96 value, int bit) {
    if (bit < 64)
      return new(value.Upper, value.Lower | (1UL << bit));
    return new(value.Upper | (1U << (bit - 64)), value.Lower);
  }

  // Bitwise operators
  public static Int96 operator &(Int96 left, Int96 right) => new(left.Upper & right.Upper, left.Lower & right.Lower);
  public static Int96 operator |(Int96 left, Int96 right) => new(left.Upper | right.Upper, left.Lower | right.Lower);
  public static Int96 operator ^(Int96 left, Int96 right) => new(left.Upper ^ right.Upper, left.Lower ^ right.Lower);
  public static Int96 operator ~(Int96 value) => new(~value.Upper, ~value.Lower);

  public static Int96 operator <<(Int96 value, int shiftAmount) {
    shiftAmount &= 95;
    if (shiftAmount == 0)
      return value;
    if (shiftAmount >= 64)
      return new((uint)(value.Lower << (shiftAmount - 64)), 0);
    return new((value.Upper << shiftAmount) | (uint)(value.Lower >> (64 - shiftAmount)), value.Lower << shiftAmount);
  }

  public static Int96 operator >>(Int96 value, int shiftAmount) {
    shiftAmount &= 95;
    if (shiftAmount == 0)
      return value;
    if (shiftAmount >= 64)
      return new((uint)((int)value.Upper >> 31), (ulong)((int)value.Upper >> (shiftAmount - 64)));
    return new((uint)((int)value.Upper >> shiftAmount), (value.Lower >> shiftAmount) | ((ulong)value.Upper << (64 - shiftAmount)));
  }

  public static Int96 operator >>>(Int96 value, int shiftAmount) {
    shiftAmount &= 95;
    if (shiftAmount == 0)
      return value;
    if (shiftAmount >= 64)
      return new(0, (ulong)value.Upper >> (shiftAmount - 64));
    return new(value.Upper >> shiftAmount, (value.Lower >> shiftAmount) | ((ulong)value.Upper << (64 - shiftAmount)));
  }

  // Implicit conversions from smaller types
  public static implicit operator Int96(byte value) => new(0, value);
  public static implicit operator Int96(sbyte value) => value < 0 ? new(uint.MaxValue, (ulong)(long)value) : new(0, (ulong)value);
  public static implicit operator Int96(short value) => value < 0 ? new(uint.MaxValue, (ulong)(long)value) : new(0, (ulong)value);
  public static implicit operator Int96(ushort value) => new(0, value);
  public static implicit operator Int96(char value) => new(0, value);
  public static implicit operator Int96(int value) => value < 0 ? new(uint.MaxValue, (ulong)(long)value) : new(0, (ulong)value);
  public static implicit operator Int96(uint value) => new(0, value);
  public static implicit operator Int96(long value) => value < 0 ? new(uint.MaxValue, (ulong)value) : new(0, (ulong)value);
  public static implicit operator Int96(ulong value) => new(0, value);

  // Explicit conversions to smaller types
  public static explicit operator byte(Int96 value) => (byte)value.Lower;
  public static explicit operator sbyte(Int96 value) => (sbyte)value.Lower;
  public static explicit operator short(Int96 value) => (short)value.Lower;
  public static explicit operator ushort(Int96 value) => (ushort)value.Lower;
  public static explicit operator char(Int96 value) => (char)value.Lower;
  public static explicit operator int(Int96 value) => (int)value.Lower;
  public static explicit operator uint(Int96 value) => (uint)value.Lower;
  public static explicit operator long(Int96 value) => (long)value.Lower;
  public static explicit operator ulong(Int96 value) => value.Lower;

  public static explicit operator float(Int96 value) => (float)(double)value;

  public static explicit operator double(Int96 value) {
    var isNegative = IsNegative(value);
    if (isNegative)
      value = -value;

    var result = value.Upper * 18446744073709551616.0 + value.Lower;
    return isNegative ? -result : result;
  }

  public static explicit operator decimal(Int96 value) {
    var isNegative = IsNegative(value);
    if (isNegative)
      value = -value;

    var lo = (int)value.Lower;
    var mid = (int)(value.Lower >> 32);
    var hi = (int)value.Upper;

    return new(lo, mid, hi, isNegative, 0);
  }

  public static explicit operator Int96(float value) => (Int96)(double)value;

  public static explicit operator Int96(double value) {
    if (double.IsNaN(value) || double.IsInfinity(value))
      throw new OverflowException();

    var isNegative = value < 0;
    if (isNegative)
      value = -value;

    var upper = (uint)(value / 18446744073709551616.0);
    var lower = (ulong)(value - upper * 18446744073709551616.0);

    var result = new Int96(upper, lower);
    return isNegative ? -result : result;
  }

  public static explicit operator Int96(decimal value) {
    var bits = decimal.GetBits(value);
    var lo = (uint)bits[0];
    var mid = (uint)bits[1];
    var hi = (uint)bits[2];
    var isNegative = (bits[3] & 0x80000000) != 0;

    var lower = ((ulong)mid << 32) | lo;
    var upper = hi;

    var result = new Int96(upper, lower);
    return isNegative ? -result : result;
  }

  // Conversion from/to UInt96
  public static explicit operator Int96(UInt96 value) => new(value.Upper, value.Lower);
  public static explicit operator UInt96(Int96 value) => new(value.Upper, value.Lower);

  // Helper methods for bit operations
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _PopCount(ulong value) {
    value -= (value >> 1) & 0x5555555555555555UL;
    value = (value & 0x3333333333333333UL) + ((value >> 2) & 0x3333333333333333UL);
    value = (value + (value >> 4)) & 0x0F0F0F0F0F0F0F0FUL;
    return (int)((value * 0x0101010101010101UL) >> 56);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _PopCount(uint value) {
    value -= (value >> 1) & 0x55555555U;
    value = (value & 0x33333333U) + ((value >> 2) & 0x33333333U);
    value = (value + (value >> 4)) & 0x0F0F0F0FU;
    return (int)((value * 0x01010101U) >> 24);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _LeadingZeroCount(uint value) {
    if (value == 0)
      return 32;
    var n = 0;
    if ((value & 0xFFFF0000U) == 0) { n += 16; value <<= 16; }
    if ((value & 0xFF000000U) == 0) { n += 8; value <<= 8; }
    if ((value & 0xF0000000U) == 0) { n += 4; value <<= 4; }
    if ((value & 0xC0000000U) == 0) { n += 2; value <<= 2; }
    if ((value & 0x80000000U) == 0) { ++n; }
    return n;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _LeadingZeroCount(ulong value) {
    if (value == 0)
      return 64;
    var n = 0;
    if ((value & 0xFFFFFFFF00000000UL) == 0) { n += 32; value <<= 32; }
    if ((value & 0xFFFF000000000000UL) == 0) { n += 16; value <<= 16; }
    if ((value & 0xFF00000000000000UL) == 0) { n += 8; value <<= 8; }
    if ((value & 0xF000000000000000UL) == 0) { n += 4; value <<= 4; }
    if ((value & 0xC000000000000000UL) == 0) { n += 2; value <<= 2; }
    if ((value & 0x8000000000000000UL) == 0) { ++n; }
    return n;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _TrailingZeroCount(uint value) {
    if (value == 0)
      return 32;
    var n = 0;
    if ((value & 0x0000FFFFU) == 0) { n += 16; value >>= 16; }
    if ((value & 0x000000FFU) == 0) { n += 8; value >>= 8; }
    if ((value & 0x0000000FU) == 0) { n += 4; value >>= 4; }
    if ((value & 0x00000003U) == 0) { n += 2; value >>= 2; }
    if ((value & 0x00000001U) == 0) { ++n; }
    return n;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _TrailingZeroCount(ulong value) {
    if (value == 0)
      return 64;
    var n = 0;
    if ((value & 0x00000000FFFFFFFFUL) == 0) { n += 32; value >>= 32; }
    if ((value & 0x000000000000FFFFUL) == 0) { n += 16; value >>= 16; }
    if ((value & 0x00000000000000FFUL) == 0) { n += 8; value >>= 8; }
    if ((value & 0x000000000000000FUL) == 0) { n += 4; value >>= 4; }
    if ((value & 0x0000000000000003UL) == 0) { n += 2; value >>= 2; }
    if ((value & 0x0000000000000001UL) == 0) { ++n; }
    return n;
  }

}
