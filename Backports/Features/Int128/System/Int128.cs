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

#if !SUPPORTS_INT128

using System.Globalization;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Represents a 128-bit signed integer.
/// </summary>
public readonly struct Int128 : IComparable, IComparable<Int128>, IEquatable<Int128>, IFormattable {

  private readonly ulong _lower;
  private readonly ulong _upper;

  /// <summary>
  /// Gets the lower 64 bits of the 128-bit value.
  /// </summary>
  internal ulong Lower => this._lower;

  /// <summary>
  /// Gets the upper 64 bits of the 128-bit value.
  /// </summary>
  internal ulong Upper => this._upper;

  /// <summary>
  /// Initializes a new instance of Int128 with the specified upper and lower 64-bit values.
  /// </summary>
  public Int128(ulong upper, ulong lower) {
    this._upper = upper;
    this._lower = lower;
  }

  /// <summary>
  /// Gets the value 0 as an Int128.
  /// </summary>
  public static Int128 Zero => new(0, 0);

  /// <summary>
  /// Gets the value 1 as an Int128.
  /// </summary>
  public static Int128 One => new(0, 1);

  /// <summary>
  /// Gets the value -1 as an Int128.
  /// </summary>
  public static Int128 NegativeOne => new(ulong.MaxValue, ulong.MaxValue);

  /// <summary>
  /// Gets the maximum value of Int128.
  /// </summary>
  public static Int128 MaxValue => new(0x7FFFFFFFFFFFFFFF, ulong.MaxValue);

  /// <summary>
  /// Gets the minimum value of Int128.
  /// </summary>
  public static Int128 MinValue => new(0x8000000000000000, 0);

  /// <summary>
  /// Determines whether the specified value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(Int128 value) => (long)value._upper < 0;

  /// <summary>
  /// Determines whether the specified value is positive.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPositive(Int128 value) => (long)value._upper >= 0;

  /// <summary>
  /// Determines whether the specified value is an even integer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsEvenInteger(Int128 value) => (value._lower & 1) == 0;

  /// <summary>
  /// Determines whether the specified value is an odd integer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsOddInteger(Int128 value) => (value._lower & 1) != 0;

  /// <summary>
  /// Determines whether the specified value is a power of two.
  /// </summary>
  public static bool IsPow2(Int128 value) {
    if (IsNegative(value))
      return false;
    var popCount = _PopCount(value._upper) + _PopCount(value._lower);
    return popCount == 1;
  }

  /// <summary>
  /// Returns the absolute value.
  /// </summary>
  public static Int128 Abs(Int128 value) => IsNegative(value) ? -value : value;

  /// <summary>
  /// Clamps a value to the specified range.
  /// </summary>
  public static Int128 Clamp(Int128 value, Int128 min, Int128 max) {
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns a value with the same magnitude as value and the sign of sign.
  /// </summary>
  public static Int128 CopySign(Int128 value, Int128 sign) {
    var absValue = Abs(value);
    return IsNegative(sign) ? -absValue : absValue;
  }

  /// <summary>
  /// Returns the larger of two values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Int128 Max(Int128 x, Int128 y) => x >= y ? x : y;

  /// <summary>
  /// Returns the smaller of two values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Int128 Min(Int128 x, Int128 y) => x <= y ? x : y;

  /// <summary>
  /// Returns the value with the larger magnitude.
  /// </summary>
  public static Int128 MaxMagnitude(Int128 x, Int128 y) {
    var absX = Abs(x);
    var absY = Abs(y);
    return absX >= absY ? x : y;
  }

  /// <summary>
  /// Returns the value with the smaller magnitude.
  /// </summary>
  public static Int128 MinMagnitude(Int128 x, Int128 y) {
    var absX = Abs(x);
    var absY = Abs(y);
    return absX <= absY ? x : y;
  }

  /// <summary>
  /// Returns an indication of the sign of a value.
  /// </summary>
  public static int Sign(Int128 value) {
    if (IsNegative(value))
      return -1;
    if (value == Zero)
      return 0;
    return 1;
  }

  /// <summary>
  /// Computes the quotient and remainder of two values.
  /// </summary>
  public static (Int128 Quotient, Int128 Remainder) DivRem(Int128 left, Int128 right) {
    var quotient = left / right;
    var remainder = left - (quotient * right);
    return (quotient, remainder);
  }

  /// <summary>
  /// Returns the number of leading zeros.
  /// </summary>
  public static int LeadingZeroCount(Int128 value) {
    if (value._upper != 0)
      return _LeadingZeroCount(value._upper);
    return 64 + _LeadingZeroCount(value._lower);
  }

  /// <summary>
  /// Returns the number of trailing zeros.
  /// </summary>
  public static int TrailingZeroCount(Int128 value) {
    if (value._lower != 0)
      return _TrailingZeroCount(value._lower);
    return 64 + _TrailingZeroCount(value._upper);
  }

  /// <summary>
  /// Returns the population count (number of bits set).
  /// </summary>
  public static int PopCount(Int128 value) => _PopCount(value._upper) + _PopCount(value._lower);

  /// <summary>
  /// Returns the base-2 logarithm of a value.
  /// </summary>
  public static int Log2(Int128 value) {
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, Zero);
    return 127 - LeadingZeroCount(value);
  }

  /// <summary>
  /// Rotates a value left by the specified amount.
  /// </summary>
  public static Int128 RotateLeft(Int128 value, int rotateAmount) {
    rotateAmount &= 127;
    if (rotateAmount == 0)
      return value;
    return (value << rotateAmount) | (value >>> (128 - rotateAmount));
  }

  /// <summary>
  /// Rotates a value right by the specified amount.
  /// </summary>
  public static Int128 RotateRight(Int128 value, int rotateAmount) {
    rotateAmount &= 127;
    if (rotateAmount == 0)
      return value;
    return (value >>> rotateAmount) | (value << (128 - rotateAmount));
  }

  // Comparison methods
  public int CompareTo(Int128 other) {
    if (IsNegative(this) && !IsNegative(other))
      return -1;
    if (!IsNegative(this) && IsNegative(other))
      return 1;

    var upperCmp = this._upper.CompareTo(other._upper);
    return upperCmp != 0 ? upperCmp : this._lower.CompareTo(other._lower);
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    
    Against.ArgumentIsNotOfType<Int128>(obj);
    return this.CompareTo((Int128)obj);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Int128 other) => this._upper == other._upper && this._lower == other._lower;

  public override bool Equals(object? obj) => obj is Int128 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => unchecked((int)(this._upper ^ this._lower ^ (this._upper >> 32) ^ (this._lower >> 32)));

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

  private static string _ToDecimalString(Int128 value) {
    if (value == Zero)
      return "0";

    var isNegative = IsNegative(value);
    if (isNegative)
      value = -value;

    var chars = new char[40];
    var pos = chars.Length;

    while (value != Zero) {
      var (quotient, remainder) = _DivRemUnsigned(value, 10);
      chars[--pos] = (char)('0' + (int)remainder._lower);
      value = quotient;
    }

    if (isNegative)
      chars[--pos] = '-';

    return new string(chars, pos, chars.Length - pos);
  }

  // Parse methods
  public static Int128 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static Int128 Parse(string s, NumberStyles style) => Parse(s, style, null);

  public static Int128 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static Int128 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    if (!TryParse(s, style, provider, out var result))
      throw new FormatException("Input string was not in a correct format.");
    return result;
  }
  public static Int128 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static Int128 Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null) {
    if (!TryParse(s, style, provider, out var result))
      throw new FormatException("Input string was not in a correct format.");
    return result;
  }

  public static bool TryParse(string? s, out Int128 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Int128 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Int128 result) {
    result = Zero;
    if (string.IsNullOrWhiteSpace(s))
      return false;

    s = s!.Trim();
    var isNegative = false;
    var startIndex = 0;

    if (s[0] == '-') {
      isNegative = true;
      startIndex = 1;
    } else if (s[0] == '+') {
      startIndex = 1;
    }

    var value = Zero;
    for (var i = startIndex; i < s.Length; ++i) {
      var c = s[i];
      if (c < '0' || c > '9')
        return false;

      value = value * 10 + (c - '0');
    }

    result = isNegative ? -value : value;
    return true;
  }

  public static bool TryParse(ReadOnlySpan<char> s, out Int128 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Int128 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Int128 result) {
    result = Zero;
    s = s.Trim();
    if (s.IsEmpty)
      return false;

    var isNegative = false;
    var startIndex = 0;

    if (s[0] == '-') {
      isNegative = true;
      startIndex = 1;
    } else if (s[0] == '+') {
      startIndex = 1;
    }

    var value = Zero;
    for (var i = startIndex; i < s.Length; ++i) {
      var c = s[i];
      if (c < '0' || c > '9')
        return false;

      value = value * 10 + (c - '0');
    }

    result = isNegative ? -value : value;
    return true;
  }

  // Operators
  public static bool operator ==(Int128 left, Int128 right) => left.Equals(right);
  public static bool operator !=(Int128 left, Int128 right) => !left.Equals(right);

  public static bool operator <(Int128 left, Int128 right) => left.CompareTo(right) < 0;
  public static bool operator >(Int128 left, Int128 right) => left.CompareTo(right) > 0;
  public static bool operator <=(Int128 left, Int128 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(Int128 left, Int128 right) => left.CompareTo(right) >= 0;

  public static Int128 operator +(Int128 value) => value;

  public static Int128 operator -(Int128 value) {
    var lower = ~value._lower + 1;
    var upper = ~value._upper;
    if (lower == 0)
      ++upper;
    return new(upper, lower);
  }

  public static Int128 operator ++(Int128 value) => value + One;
  public static Int128 operator --(Int128 value) => value - One;

  public static Int128 operator +(Int128 left, Int128 right) {
    var lower = left._lower + right._lower;
    var carry = lower < left._lower ? 1UL : 0UL;
    var upper = left._upper + right._upper + carry;
    return new(upper, lower);
  }

  public static Int128 operator -(Int128 left, Int128 right) => left + (-right);

  public static Int128 operator *(Int128 left, Int128 right) {
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

  public static Int128 operator /(Int128 left, Int128 right) {
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

  public static Int128 operator %(Int128 left, Int128 right) {
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

  private static (Int128 Quotient, Int128 Remainder) _DivRemUnsigned(Int128 left, Int128 right) {
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

  private static bool _GetBit(Int128 value, int bit) {
    if (bit < 64)
      return (value._lower & (1UL << bit)) != 0;
    return (value._upper & (1UL << (bit - 64))) != 0;
  }

  private static Int128 _SetBit(Int128 value, int bit) {
    if (bit < 64)
      return new(value._upper, value._lower | (1UL << bit));
    return new(value._upper | (1UL << (bit - 64)), value._lower);
  }

  // Bitwise operators
  public static Int128 operator &(Int128 left, Int128 right) => new(left._upper & right._upper, left._lower & right._lower);
  public static Int128 operator |(Int128 left, Int128 right) => new(left._upper | right._upper, left._lower | right._lower);
  public static Int128 operator ^(Int128 left, Int128 right) => new(left._upper ^ right._upper, left._lower ^ right._lower);
  public static Int128 operator ~(Int128 value) => new(~value._upper, ~value._lower);

  public static Int128 operator <<(Int128 value, int shiftAmount) {
    shiftAmount &= 127;
    if (shiftAmount == 0)
      return value;
    if (shiftAmount >= 64)
      return new(value._lower << (shiftAmount - 64), 0);
    return new((value._upper << shiftAmount) | (value._lower >> (64 - shiftAmount)), value._lower << shiftAmount);
  }

  public static Int128 operator >>(Int128 value, int shiftAmount) {
    shiftAmount &= 127;
    if (shiftAmount == 0)
      return value;
    if (shiftAmount >= 64)
      return new((ulong)((long)value._upper >> 63), (ulong)((long)value._upper >> (shiftAmount - 64)));
    return new((ulong)((long)value._upper >> shiftAmount), (value._lower >> shiftAmount) | (value._upper << (64 - shiftAmount)));
  }

  public static Int128 operator >>>(Int128 value, int shiftAmount) {
    shiftAmount &= 127;
    if (shiftAmount == 0)
      return value;
    if (shiftAmount >= 64)
      return new(0, value._upper >> (shiftAmount - 64));
    return new(value._upper >> shiftAmount, (value._lower >> shiftAmount) | (value._upper << (64 - shiftAmount)));
  }

  // Implicit conversions from smaller types
  public static implicit operator Int128(byte value) => new(0, value);
  public static implicit operator Int128(sbyte value) => value < 0 ? new(ulong.MaxValue, (ulong)value) : new(0, (ulong)value);
  public static implicit operator Int128(short value) => value < 0 ? new(ulong.MaxValue, (ulong)value) : new(0, (ulong)value);
  public static implicit operator Int128(ushort value) => new(0, value);
  public static implicit operator Int128(char value) => new(0, value);
  public static implicit operator Int128(int value) => value < 0 ? new(ulong.MaxValue, (ulong)value) : new(0, (ulong)value);
  public static implicit operator Int128(uint value) => new(0, value);
  public static implicit operator Int128(long value) => value < 0 ? new(ulong.MaxValue, (ulong)value) : new(0, (ulong)value);
  public static implicit operator Int128(ulong value) => new(0, value);

  // Explicit conversions to smaller types
  public static explicit operator byte(Int128 value) => (byte)value._lower;
  public static explicit operator sbyte(Int128 value) => (sbyte)value._lower;
  public static explicit operator short(Int128 value) => (short)value._lower;
  public static explicit operator ushort(Int128 value) => (ushort)value._lower;
  public static explicit operator char(Int128 value) => (char)value._lower;
  public static explicit operator int(Int128 value) => (int)value._lower;
  public static explicit operator uint(Int128 value) => (uint)value._lower;
  public static explicit operator long(Int128 value) => (long)value._lower;
  public static explicit operator ulong(Int128 value) => value._lower;

  public static explicit operator float(Int128 value) => (float)(double)value;

  public static explicit operator double(Int128 value) {
    var isNegative = IsNegative(value);
    if (isNegative)
      value = -value;

    var result = value._upper * 18446744073709551616.0 + value._lower;
    return isNegative ? -result : result;
  }

  public static explicit operator decimal(Int128 value) {
    var isNegative = IsNegative(value);
    if (isNegative)
      value = -value;

    var lo = (int)value._lower;
    var mid = (int)(value._lower >> 32);
    var hi = (int)value._upper;

    var result = new decimal(lo, mid, hi, isNegative, 0);
    return result;
  }

  public static explicit operator Int128(float value) => (Int128)(double)value;

  public static explicit operator Int128(double value) {
    if (double.IsNaN(value) || double.IsInfinity(value))
      throw new OverflowException();

    var isNegative = value < 0;
    if (isNegative)
      value = -value;

    var upper = (ulong)(value / 18446744073709551616.0);
    var lower = (ulong)(value - upper * 18446744073709551616.0);

    var result = new Int128(upper, lower);
    return isNegative ? -result : result;
  }

  public static explicit operator Int128(decimal value) {
    var bits = decimal.GetBits(value);
    var lo = (uint)bits[0];
    var mid = (uint)bits[1];
    var hi = (uint)bits[2];
    var isNegative = (bits[3] & 0x80000000) != 0;

    var lower = ((ulong)mid << 32) | lo;
    var upper = (ulong)hi;

    var result = new Int128(upper, lower);
    return isNegative ? -result : result;
  }

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
