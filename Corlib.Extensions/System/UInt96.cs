#nullable enable

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
/// Represents a 96-bit unsigned integer.
/// </summary>
public readonly struct UInt96 : IComparable, IComparable<UInt96>, IEquatable<UInt96>, IFormattable, IParsable<UInt96> {
  /// <summary>
  /// Gets the lower 64 bits of the 96-bit value.
  /// </summary>
  internal ulong Lower { get; }

  /// <summary>
  /// Gets the upper 32 bits of the 96-bit value.
  /// </summary>
  internal uint Upper { get; }

  /// <summary>
  /// Initializes a new instance of UInt96 with the specified upper and lower values.
  /// </summary>
  public UInt96(uint upper, ulong lower) {
    this.Upper = upper;
    this.Lower = lower;
  }

  /// <summary>
  /// Gets the value 0 as a UInt96.
  /// </summary>
  public static UInt96 Zero => new(0, 0);

  /// <summary>
  /// Gets the value 1 as a UInt96.
  /// </summary>
  public static UInt96 One => new(0, 1);

  /// <summary>
  /// Gets the maximum value of UInt96 (2^96 - 1).
  /// </summary>
  public static UInt96 MaxValue => new(uint.MaxValue, ulong.MaxValue);

  /// <summary>
  /// Gets the minimum value of UInt96 (0).
  /// </summary>
  public static UInt96 MinValue => new(0, 0);

  /// <summary>
  /// Determines whether the specified value is an even integer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsEvenInteger(UInt96 value) => (value.Lower & 1) == 0;

  /// <summary>
  /// Determines whether the specified value is an odd integer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsOddInteger(UInt96 value) => (value.Lower & 1) != 0;

  /// <summary>
  /// Determines whether the specified value is a power of two.
  /// </summary>
  public static bool IsPow2(UInt96 value) {
    var popCount = _PopCount(value.Upper) + _PopCount(value.Lower);
    return popCount == 1;
  }

  /// <summary>
  /// Clamps a value to the specified range.
  /// </summary>
  public static UInt96 Clamp(UInt96 value, UInt96 min, UInt96 max) {
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns the larger of two values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UInt96 Max(UInt96 x, UInt96 y) => x >= y ? x : y;

  /// <summary>
  /// Returns the smaller of two values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UInt96 Min(UInt96 x, UInt96 y) => x <= y ? x : y;

  /// <summary>
  /// Computes the quotient and remainder of two values.
  /// </summary>
  public static (UInt96 Quotient, UInt96 Remainder) DivRem(UInt96 left, UInt96 right) {
    var quotient = left / right;
    var remainder = left - (quotient * right);
    return (quotient, remainder);
  }

  /// <summary>
  /// Returns the number of leading zeros.
  /// </summary>
  public static int LeadingZeroCount(UInt96 value) {
    if (value.Upper != 0)
      return _LeadingZeroCount(value.Upper);
    return 32 + _LeadingZeroCount(value.Lower);
  }

  /// <summary>
  /// Returns the number of trailing zeros.
  /// </summary>
  public static int TrailingZeroCount(UInt96 value) {
    if (value.Lower != 0)
      return _TrailingZeroCount(value.Lower);
    return 64 + _TrailingZeroCount(value.Upper);
  }

  /// <summary>
  /// Returns the population count (number of bits set).
  /// </summary>
  public static int PopCount(UInt96 value) => _PopCount(value.Upper) + _PopCount(value.Lower);

  /// <summary>
  /// Returns the base-2 logarithm of a value.
  /// </summary>
  public static int Log2(UInt96 value) {
    if (value == Zero)
      throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
    return 95 - LeadingZeroCount(value);
  }

  /// <summary>
  /// Rotates a value left by the specified amount.
  /// </summary>
  public static UInt96 RotateLeft(UInt96 value, int rotateAmount) {
    rotateAmount &= 95;
    if (rotateAmount == 0)
      return value;
    return (value << rotateAmount) | (value >> (96 - rotateAmount));
  }

  /// <summary>
  /// Rotates a value right by the specified amount.
  /// </summary>
  public static UInt96 RotateRight(UInt96 value, int rotateAmount) {
    rotateAmount &= 95;
    if (rotateAmount == 0)
      return value;
    return (value >> rotateAmount) | (value << (96 - rotateAmount));
  }

  // Comparison methods
  public int CompareTo(UInt96 other) {
    var upperCmp = this.Upper.CompareTo(other.Upper);
    return upperCmp != 0 ? upperCmp : this.Lower.CompareTo(other.Lower);
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;

    if (obj is not UInt96 other)
      throw new ArgumentException("Object must be of type UInt96.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(UInt96 other) => this.Upper == other.Upper && this.Lower == other.Lower;

  public override bool Equals(object? obj) => obj is UInt96 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => unchecked((int)(this.Upper ^ this.Lower ^ (this.Lower >> 32)));

  // ToString methods
  public override string ToString() => _ToDecimalString(this);

  public string ToString(IFormatProvider? provider) => _ToDecimalString(this);

  public string ToString(string? format) => _ToDecimalString(this);

  public string ToString(string? format, IFormatProvider? provider) => _ToDecimalString(this);

  private static string _ToDecimalString(UInt96 value) {
    if (value == Zero)
      return "0";

    var chars = new char[29]; // 96 bits can represent up to 29 decimal digits
    var pos = chars.Length;

    while (value != Zero) {
      var (quotient, remainder) = DivRem(value, 10);
      chars[--pos] = (char)('0' + (int)remainder.Lower);
      value = quotient;
    }

    return new string(chars, pos, chars.Length - pos);
  }

  // Parse methods
  public static UInt96 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static UInt96 Parse(string s, NumberStyles style) => Parse(s, style, null);

  public static UInt96 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static UInt96 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    if (!TryParse(s, style, provider, out var result))
      throw new FormatException("Input string was not in a correct format.");
    return result;
  }

  public static bool TryParse(string? s, out UInt96 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out UInt96 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out UInt96 result) {
    result = Zero;
    if (string.IsNullOrWhiteSpace(s))
      return false;

    s = s!.Trim();
    var startIndex = 0;

    if (s[0] == '+')
      startIndex = 1;
    else if (s[0] == '-')
      return false; // Negative values not allowed for unsigned

    var value = Zero;
    for (var i = startIndex; i < s.Length; ++i) {
      var c = s[i];
      if (c < '0' || c > '9')
        return false;

      value = value * 10 + (uint)(c - '0');
    }

    result = value;
    return true;
  }

  // Operators
  public static bool operator ==(UInt96 left, UInt96 right) => left.Equals(right);
  public static bool operator !=(UInt96 left, UInt96 right) => !left.Equals(right);

  public static bool operator <(UInt96 left, UInt96 right) => left.CompareTo(right) < 0;
  public static bool operator >(UInt96 left, UInt96 right) => left.CompareTo(right) > 0;
  public static bool operator <=(UInt96 left, UInt96 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(UInt96 left, UInt96 right) => left.CompareTo(right) >= 0;

  public static UInt96 operator +(UInt96 value) => value;

  public static UInt96 operator ++(UInt96 value) => value + One;
  public static UInt96 operator --(UInt96 value) => value - One;

  public static UInt96 operator +(UInt96 left, UInt96 right) {
    var lower = left.Lower + right.Lower;
    var carry = lower < left.Lower ? 1U : 0U;
    var upper = left.Upper + right.Upper + carry;
    return new(upper, lower);
  }

  public static UInt96 operator -(UInt96 left, UInt96 right) {
    var lower = left.Lower - right.Lower;
    var borrow = lower > left.Lower ? 1U : 0U;
    var upper = left.Upper - right.Upper - borrow;
    return new(upper, lower);
  }

  public static UInt96 operator *(UInt96 left, UInt96 right) {
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

  public static UInt96 operator /(UInt96 left, UInt96 right) {
    if (right == Zero)
      throw new DivideByZeroException();

    var (quotient, _) = _DivRem(left, right);
    return quotient;
  }

  public static UInt96 operator %(UInt96 left, UInt96 right) {
    if (right == Zero)
      throw new DivideByZeroException();

    var (_, remainder) = _DivRem(left, right);
    return remainder;
  }

  private static (UInt96 Quotient, UInt96 Remainder) _DivRem(UInt96 left, UInt96 right) {
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

  private static bool _GetBit(UInt96 value, int bit) {
    if (bit < 64)
      return (value.Lower & (1UL << bit)) != 0;
    return (value.Upper & (1U << (bit - 64))) != 0;
  }

  private static UInt96 _SetBit(UInt96 value, int bit) {
    if (bit < 64)
      return new(value.Upper, value.Lower | (1UL << bit));
    return new(value.Upper | (1U << (bit - 64)), value.Lower);
  }

  // Bitwise operators
  public static UInt96 operator &(UInt96 left, UInt96 right) => new(left.Upper & right.Upper, left.Lower & right.Lower);
  public static UInt96 operator |(UInt96 left, UInt96 right) => new(left.Upper | right.Upper, left.Lower | right.Lower);
  public static UInt96 operator ^(UInt96 left, UInt96 right) => new(left.Upper ^ right.Upper, left.Lower ^ right.Lower);
  public static UInt96 operator ~(UInt96 value) => new(~value.Upper, ~value.Lower);

  public static UInt96 operator <<(UInt96 value, int shiftAmount) {
    shiftAmount &= 95;
    if (shiftAmount == 0)
      return value;
    if (shiftAmount >= 64)
      return new((uint)(value.Lower << (shiftAmount - 64)), 0);
    return new((value.Upper << shiftAmount) | (uint)(value.Lower >> (64 - shiftAmount)), value.Lower << shiftAmount);
  }

  public static UInt96 operator >>(UInt96 value, int shiftAmount) {
    shiftAmount &= 95;
    if (shiftAmount == 0)
      return value;
    if (shiftAmount >= 64)
      return new(0, (ulong)value.Upper >> (shiftAmount - 64));
    return new(value.Upper >> shiftAmount, (value.Lower >> shiftAmount) | ((ulong)value.Upper << (64 - shiftAmount)));
  }

  public static UInt96 operator >>>(UInt96 value, int shiftAmount) => value >> shiftAmount;

  // Implicit conversions from smaller types
  public static implicit operator UInt96(byte value) => new(0, value);
  public static implicit operator UInt96(ushort value) => new(0, value);
  public static implicit operator UInt96(char value) => new(0, value);
  public static implicit operator UInt96(uint value) => new(0, value);
  public static implicit operator UInt96(ulong value) => new(0, value);

  // Explicit conversions from signed types (could be negative)
  public static explicit operator UInt96(sbyte value) => value < 0 ? throw new OverflowException() : new(0, (ulong)value);
  public static explicit operator UInt96(short value) => value < 0 ? throw new OverflowException() : new(0, (ulong)value);
  public static explicit operator UInt96(int value) => value < 0 ? throw new OverflowException() : new(0, (ulong)value);
  public static explicit operator UInt96(long value) => value < 0 ? throw new OverflowException() : new(0, (ulong)value);

  // Explicit conversions to smaller types
  public static explicit operator byte(UInt96 value) => (byte)value.Lower;
  public static explicit operator sbyte(UInt96 value) => (sbyte)value.Lower;
  public static explicit operator short(UInt96 value) => (short)value.Lower;
  public static explicit operator ushort(UInt96 value) => (ushort)value.Lower;
  public static explicit operator char(UInt96 value) => (char)value.Lower;
  public static explicit operator int(UInt96 value) => (int)value.Lower;
  public static explicit operator uint(UInt96 value) => (uint)value.Lower;
  public static explicit operator long(UInt96 value) => (long)value.Lower;
  public static explicit operator ulong(UInt96 value) => value.Lower;

  public static explicit operator float(UInt96 value) => (float)(double)value;
  public static explicit operator Half(UInt96 value) => (Half)(double)value;
  public static explicit operator Quarter(UInt96 value) => Quarter.FromDouble((double)value);

  public static explicit operator double(UInt96 value) => value.Upper * 18446744073709551616.0 + value.Lower;

  public static explicit operator decimal(UInt96 value) {
    var lo = (int)value.Lower;
    var mid = (int)(value.Lower >> 32);
    var hi = (int)value.Upper;

    return new(lo, mid, hi, false, 0);
  }

  public static explicit operator UInt96(float value) => (UInt96)(double)value;
  public static explicit operator UInt96(Half value) => (UInt96)(double)value;
  public static explicit operator UInt96(Quarter value) => (UInt96)(double)value.ToDouble();

  public static explicit operator UInt96(double value) {
    if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
      throw new OverflowException();

    var upper = (uint)(value / 18446744073709551616.0);
    var lower = (ulong)(value - upper * 18446744073709551616.0);

    return new(upper, lower);
  }

  public static explicit operator UInt96(decimal value) {
    if (value < 0)
      throw new OverflowException();

    var bits = decimal.GetBits(value);
    var lo = (uint)bits[0];
    var mid = (uint)bits[1];
    var hi = (uint)bits[2];

    var lower = ((ulong)mid << 32) | lo;
    var upper = hi;

    return new(upper, lower);
  }

  // Conversion from/to Int96
  public static explicit operator UInt96(Int96 value) => new(value.Upper, value.Lower);
  public static explicit operator Int96(UInt96 value) => new(value.Upper, value.Lower);

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
