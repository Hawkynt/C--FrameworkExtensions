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

#if !SUPPORTS_UINT128

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Represents a 128-bit unsigned integer.
/// </summary>
public readonly struct UInt128 : IComparable, IComparable<UInt128>, IEquatable<UInt128>, IFormattable {

  private readonly ulong _lower;
  private readonly ulong _upper;

  /// <summary>
  /// Gets the lower 64 bits of the 128-bit value.
  /// </summary>
  internal ulong Lower => _lower;

  /// <summary>
  /// Gets the upper 64 bits of the 128-bit value.
  /// </summary>
  internal ulong Upper => _upper;

  /// <summary>
  /// Initializes a new instance of UInt128 with the specified upper and lower 64-bit values.
  /// </summary>
  public UInt128(ulong upper, ulong lower) {
    _upper = upper;
    _lower = lower;
  }

  /// <summary>
  /// Gets the value 0 as a UInt128.
  /// </summary>
  public static UInt128 Zero => new(0, 0);

  /// <summary>
  /// Gets the value 1 as a UInt128.
  /// </summary>
  public static UInt128 One => new(0, 1);

  /// <summary>
  /// Gets the maximum value of UInt128.
  /// </summary>
  public static UInt128 MaxValue => new(ulong.MaxValue, ulong.MaxValue);

  /// <summary>
  /// Gets the minimum value of UInt128 (which is 0).
  /// </summary>
  public static UInt128 MinValue => Zero;

  /// <summary>
  /// Determines whether the specified value is an even integer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsEvenInteger(UInt128 value) => (value._lower & 1) == 0;

  /// <summary>
  /// Determines whether the specified value is an odd integer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsOddInteger(UInt128 value) => (value._lower & 1) != 0;

  /// <summary>
  /// Determines whether the specified value is a power of two.
  /// </summary>
  public static bool IsPow2(UInt128 value) {
    var popCount = _PopCount(value._upper) + _PopCount(value._lower);
    return popCount == 1;
  }

  /// <summary>
  /// Clamps a value to the specified range.
  /// </summary>
  public static UInt128 Clamp(UInt128 value, UInt128 min, UInt128 max) {
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
  public static UInt128 Max(UInt128 x, UInt128 y) => x >= y ? x : y;

  /// <summary>
  /// Returns the smaller of two values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UInt128 Min(UInt128 x, UInt128 y) => x <= y ? x : y;

  /// <summary>
  /// Returns the sign of a value (0 for zero, 1 for positive).
  /// </summary>
  public static int Sign(UInt128 value) => value == Zero ? 0 : 1;

  /// <summary>
  /// Computes the quotient and remainder of two values.
  /// </summary>
  public static (UInt128 Quotient, UInt128 Remainder) DivRem(UInt128 left, UInt128 right) {
    var quotient = left / right;
    var remainder = left - (quotient * right);
    return (quotient, remainder);
  }

  /// <summary>
  /// Returns the number of leading zeros.
  /// </summary>
  public static int LeadingZeroCount(UInt128 value) {
    if (value._upper != 0)
      return _LeadingZeroCount(value._upper);
    return 64 + _LeadingZeroCount(value._lower);
  }

  /// <summary>
  /// Returns the number of trailing zeros.
  /// </summary>
  public static int TrailingZeroCount(UInt128 value) {
    if (value._lower != 0)
      return _TrailingZeroCount(value._lower);
    return 64 + _TrailingZeroCount(value._upper);
  }

  /// <summary>
  /// Returns the population count (number of bits set).
  /// </summary>
  public static int PopCount(UInt128 value) => _PopCount(value._upper) + _PopCount(value._lower);

  /// <summary>
  /// Returns the base-2 logarithm of a value.
  /// </summary>
  public static int Log2(UInt128 value) {
    if (value == Zero)
      throw new ArgumentOutOfRangeException(nameof(value));
    return 127 - LeadingZeroCount(value);
  }

  /// <summary>
  /// Rotates a value left by the specified amount.
  /// </summary>
  public static UInt128 RotateLeft(UInt128 value, int rotateAmount) {
    rotateAmount &= 127;
    if (rotateAmount == 0)
      return value;
    return (value << rotateAmount) | (value >> (128 - rotateAmount));
  }

  /// <summary>
  /// Rotates a value right by the specified amount.
  /// </summary>
  public static UInt128 RotateRight(UInt128 value, int rotateAmount) {
    rotateAmount &= 127;
    if (rotateAmount == 0)
      return value;
    return (value >> rotateAmount) | (value << (128 - rotateAmount));
  }

  // Comparison methods
  public int CompareTo(UInt128 other) {
    var upperCmp = _upper.CompareTo(other._upper);
    return upperCmp != 0 ? upperCmp : _lower.CompareTo(other._lower);
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not UInt128 other)
      throw new ArgumentException("Object must be of type UInt128.");
    return CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(UInt128 other) => _upper == other._upper && _lower == other._lower;

  public override bool Equals(object? obj) => obj is UInt128 other && Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => unchecked((int)(_upper ^ _lower ^ (_upper >> 32) ^ (_lower >> 32)));

  // ToString methods
  public override string ToString() => _ToDecimalString(this);

  public string ToString(IFormatProvider? provider) => _ToDecimalString(this);

  public string ToString(string? format) => _ToDecimalString(this);

  public string ToString(string? format, IFormatProvider? provider) => _ToDecimalString(this);

  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) {
    var str = _ToDecimalString(this);
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }
    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  private static string _ToDecimalString(UInt128 value) {
    if (value == Zero)
      return "0";

    var chars = new char[40];
    var pos = chars.Length;

    while (value != Zero) {
      var (quotient, remainder) = _DivRem(value, 10);
      chars[--pos] = (char)('0' + (int)remainder._lower);
      value = quotient;
    }

    return new string(chars, pos, chars.Length - pos);
  }

  // Parse methods
  public static UInt128 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static UInt128 Parse(string s, NumberStyles style) => Parse(s, style, null);

  public static UInt128 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static UInt128 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    if (!TryParse(s, style, provider, out var result))
      throw new FormatException("Input string was not in a correct format.");
    return result;
  }

  public static UInt128 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static UInt128 Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null) {
    if (!TryParse(s, style, provider, out var result))
      throw new FormatException("Input string was not in a correct format.");
    return result;
  }

  public static bool TryParse(string? s, out UInt128 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out UInt128 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out UInt128 result) {
    result = Zero;
    if (string.IsNullOrWhiteSpace(s))
      return false;

    s = s.Trim();
    var startIndex = 0;

    if (s[0] == '+')
      startIndex = 1;

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

  public static bool TryParse(ReadOnlySpan<char> s, out UInt128 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out UInt128 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out UInt128 result) {
    result = Zero;
    s = s.Trim();
    if (s.IsEmpty)
      return false;

    var startIndex = 0;

    if (s[0] == '+')
      startIndex = 1;

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
  public static bool operator ==(UInt128 left, UInt128 right) => left.Equals(right);
  public static bool operator !=(UInt128 left, UInt128 right) => !left.Equals(right);

  public static bool operator <(UInt128 left, UInt128 right) => left.CompareTo(right) < 0;
  public static bool operator >(UInt128 left, UInt128 right) => left.CompareTo(right) > 0;
  public static bool operator <=(UInt128 left, UInt128 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(UInt128 left, UInt128 right) => left.CompareTo(right) >= 0;

  public static UInt128 operator +(UInt128 value) => value;

  public static UInt128 operator -(UInt128 value) {
    var lower = ~value._lower + 1;
    var upper = ~value._upper;
    if (lower == 0)
      ++upper;
    return new(upper, lower);
  }

  public static UInt128 operator ++(UInt128 value) => value + One;
  public static UInt128 operator --(UInt128 value) => value - One;

  public static UInt128 operator +(UInt128 left, UInt128 right) {
    var lower = left._lower + right._lower;
    var carry = lower < left._lower ? 1UL : 0UL;
    var upper = left._upper + right._upper + carry;
    return new(upper, lower);
  }

  public static UInt128 operator -(UInt128 left, UInt128 right) => left + (-right);

  public static UInt128 operator *(UInt128 left, UInt128 right) {
    // Use grade-school multiplication
    var a0 = (uint)left._lower;
    var a1 = (uint)(left._lower >> 32);
    var a2 = (uint)left._upper;
    var a3 = (uint)(left._upper >> 32);

    var b0 = (uint)right._lower;
    var b1 = (uint)(right._lower >> 32);
    var b2 = (uint)right._upper;
    var b3 = (uint)(right._upper >> 32);

    var r0 = (ulong)a0 * b0;
    var r1 = (ulong)a0 * b1 + (ulong)a1 * b0;
    var r2 = (ulong)a0 * b2 + (ulong)a1 * b1 + (ulong)a2 * b0;
    var r3 = (ulong)a0 * b3 + (ulong)a1 * b2 + (ulong)a2 * b1 + (ulong)a3 * b0;

    var c0 = (uint)r0;
    r1 += r0 >> 32;
    var c1 = (uint)r1;
    r2 += r1 >> 32;
    var c2 = (uint)r2;
    r3 += r2 >> 32;
    var c3 = (uint)r3;

    return new(((ulong)c3 << 32) | c2, ((ulong)c1 << 32) | c0);
  }

  public static UInt128 operator /(UInt128 left, UInt128 right) {
    if (right == Zero)
      throw new DivideByZeroException();

    var (quotient, _) = _DivRem(left, right);
    return quotient;
  }

  public static UInt128 operator %(UInt128 left, UInt128 right) {
    if (right == Zero)
      throw new DivideByZeroException();

    var (_, remainder) = _DivRem(left, right);
    return remainder;
  }

  private static (UInt128 Quotient, UInt128 Remainder) _DivRem(UInt128 left, UInt128 right) {
    if (right == Zero)
      throw new DivideByZeroException();
    if (left == Zero)
      return (Zero, Zero);
    if (left < right)
      return (Zero, left);

    var quotient = Zero;
    var remainder = Zero;

    for (var i = 127; i >= 0; --i) {
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

  private static bool _GetBit(UInt128 value, int bit) {
    if (bit < 64)
      return (value._lower & (1UL << bit)) != 0;
    return (value._upper & (1UL << (bit - 64))) != 0;
  }

  private static UInt128 _SetBit(UInt128 value, int bit) {
    if (bit < 64)
      return new(value._upper, value._lower | (1UL << bit));
    return new(value._upper | (1UL << (bit - 64)), value._lower);
  }

  // Bitwise operators
  public static UInt128 operator &(UInt128 left, UInt128 right) => new(left._upper & right._upper, left._lower & right._lower);
  public static UInt128 operator |(UInt128 left, UInt128 right) => new(left._upper | right._upper, left._lower | right._lower);
  public static UInt128 operator ^(UInt128 left, UInt128 right) => new(left._upper ^ right._upper, left._lower ^ right._lower);
  public static UInt128 operator ~(UInt128 value) => new(~value._upper, ~value._lower);

  public static UInt128 operator <<(UInt128 value, int shiftAmount) {
    shiftAmount &= 127;
    if (shiftAmount == 0)
      return value;
    if (shiftAmount >= 64)
      return new(value._lower << (shiftAmount - 64), 0);
    return new((value._upper << shiftAmount) | (value._lower >> (64 - shiftAmount)), value._lower << shiftAmount);
  }

  public static UInt128 operator >>(UInt128 value, int shiftAmount) {
    shiftAmount &= 127;
    if (shiftAmount == 0)
      return value;
    if (shiftAmount >= 64)
      return new(0, value._upper >> (shiftAmount - 64));
    return new(value._upper >> shiftAmount, (value._lower >> shiftAmount) | (value._upper << (64 - shiftAmount)));
  }

  // Implicit conversions from smaller types
  public static implicit operator UInt128(byte value) => new(0, value);
  public static implicit operator UInt128(ushort value) => new(0, value);
  public static implicit operator UInt128(char value) => new(0, value);
  public static implicit operator UInt128(uint value) => new(0, value);
  public static implicit operator UInt128(ulong value) => new(0, value);

  // Explicit conversions from signed types (may throw for negative values)
  public static explicit operator UInt128(sbyte value) {
    if (value < 0)
      throw new OverflowException();
    return new(0, (ulong)value);
  }

  public static explicit operator UInt128(short value) {
    if (value < 0)
      throw new OverflowException();
    return new(0, (ulong)value);
  }

  public static explicit operator UInt128(int value) {
    if (value < 0)
      throw new OverflowException();
    return new(0, (ulong)value);
  }

  public static explicit operator UInt128(long value) {
    if (value < 0)
      throw new OverflowException();
    return new(0, (ulong)value);
  }

  // Explicit conversions to smaller types
  public static explicit operator byte(UInt128 value) => (byte)value._lower;
  public static explicit operator sbyte(UInt128 value) => (sbyte)value._lower;
  public static explicit operator short(UInt128 value) => (short)value._lower;
  public static explicit operator ushort(UInt128 value) => (ushort)value._lower;
  public static explicit operator char(UInt128 value) => (char)value._lower;
  public static explicit operator int(UInt128 value) => (int)value._lower;
  public static explicit operator uint(UInt128 value) => (uint)value._lower;
  public static explicit operator long(UInt128 value) => (long)value._lower;
  public static explicit operator ulong(UInt128 value) => value._lower;

  public static explicit operator float(UInt128 value) => (float)(double)value;

  public static explicit operator double(UInt128 value) => value._upper * 18446744073709551616.0 + value._lower;

  public static explicit operator decimal(UInt128 value) {
    var lo = (int)value._lower;
    var mid = (int)(value._lower >> 32);
    var hi = (int)value._upper;

    return new(lo, mid, hi, false, 0);
  }

  public static explicit operator UInt128(float value) => (UInt128)(double)value;

  public static explicit operator UInt128(double value) {
    if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
      throw new OverflowException();

    var upper = (ulong)(value / 18446744073709551616.0);
    var lower = (ulong)(value - upper * 18446744073709551616.0);

    return new(upper, lower);
  }

  public static explicit operator UInt128(decimal value) {
    if (value < 0)
      throw new OverflowException();

    var bits = decimal.GetBits(value);
    var lo = (uint)bits[0];
    var mid = (uint)bits[1];
    var hi = (uint)bits[2];

    var lower = ((ulong)mid << 32) | lo;
    var upper = (ulong)hi;

    return new(upper, lower);
  }

  // Conversion between Int128 and UInt128
  public static explicit operator UInt128(Int128 value) => new(value.Upper, value.Lower);
  public static explicit operator Int128(UInt128 value) => new(value._upper, value._lower);

  // Helper methods for bit operations
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _PopCount(ulong value) {
    value -= (value >> 1) & 0x5555555555555555UL;
    value = (value & 0x3333333333333333UL) + ((value >> 2) & 0x3333333333333333UL);
    value = (value + (value >> 4)) & 0x0F0F0F0F0F0F0F0FUL;
    return (int)((value * 0x0101010101010101UL) >> 56);
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

#endif
