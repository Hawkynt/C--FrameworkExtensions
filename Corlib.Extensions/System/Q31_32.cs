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
/// Represents a signed Q31.32 fixed-point number (64-bit: 1 sign + 31 integer + 32 fractional bits).
/// Range: approximately -2^31 to 2^31-1 with resolution of 1/4294967296.
/// </summary>
public readonly struct Q31_32 : IComparable, IComparable<Q31_32>, IEquatable<Q31_32>, IFormattable, IParsable<Q31_32> {

  private const int FractionalBits = 32;
  private const long Scale = 1L << FractionalBits; // 4294967296

  /// <summary>
  /// Gets the raw underlying fixed-point representation.
  /// </summary>
  public long RawValue { get; }

  private Q31_32(long raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a Q31_32 from the raw fixed-point representation.
  /// </summary>
  public static Q31_32 FromRaw(long raw) => new(raw);

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static Q31_32 Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static Q31_32 One => new(Scale);

  /// <summary>
  /// Gets the smallest positive value (1/4294967296).
  /// </summary>
  public static Q31_32 Epsilon => new(1);

  /// <summary>
  /// Gets the maximum value.
  /// </summary>
  public static Q31_32 MaxValue => new(long.MaxValue);

  /// <summary>
  /// Gets the minimum value.
  /// </summary>
  public static Q31_32 MinValue => new(long.MinValue);

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(Q31_32 other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Q31_32 other)
      throw new ArgumentException("Object must be of type Q31_32.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Q31_32 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is Q31_32 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDouble().ToString(CultureInfo.InvariantCulture);

  public string ToString(IFormatProvider? provider) => this.ToDouble().ToString(provider);

  public string ToString(string? format) => this.ToDouble().ToString(format, CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? provider) => this.ToDouble().ToString(format, provider);

  // Conversion to floating point
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ToSingle() => (float)((double)this.RawValue / Scale);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double ToDouble() => (double)this.RawValue / Scale;

  // Factory methods from floating point
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 FromSingle(float value) => new((long)(value * Scale));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 FromDouble(double value) => new((long)(value * Scale));

  // Factory method from integer
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 FromInt32(int value) => new((long)value << FractionalBits);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 FromInt64(long value) => new(value << FractionalBits);

  // Integer part extraction
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int ToInt32() => (int)(this.RawValue >> FractionalBits);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public long ToInt64() => this.RawValue >> FractionalBits;

  // Comparison operators
  public static bool operator ==(Q31_32 left, Q31_32 right) => left.Equals(right);
  public static bool operator !=(Q31_32 left, Q31_32 right) => !left.Equals(right);
  public static bool operator <(Q31_32 left, Q31_32 right) => left.RawValue < right.RawValue;
  public static bool operator >(Q31_32 left, Q31_32 right) => left.RawValue > right.RawValue;
  public static bool operator <=(Q31_32 left, Q31_32 right) => left.RawValue <= right.RawValue;
  public static bool operator >=(Q31_32 left, Q31_32 right) => left.RawValue >= right.RawValue;

  // Arithmetic operators
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator +(Q31_32 left, Q31_32 right) => new(left.RawValue + right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator -(Q31_32 left, Q31_32 right) => new(left.RawValue - right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator -(Q31_32 value) => new(-value.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator *(Q31_32 left, Q31_32 right) {
    // Use 128-bit multiplication to avoid overflow
    // Split into high and low 32-bit parts
    var aLo = (uint)left.RawValue;
    var aHi = (uint)(left.RawValue >> 32);
    var bLo = (uint)right.RawValue;
    var bHi = (uint)(right.RawValue >> 32);
    var aSign = left.RawValue < 0;
    var bSign = right.RawValue < 0;

    // Work with absolute values for the multiplication
    ulong aAbs = aSign ? (ulong)(-left.RawValue) : (ulong)left.RawValue;
    ulong bAbs = bSign ? (ulong)(-right.RawValue) : (ulong)right.RawValue;

    var aLoU = (uint)aAbs;
    var aHiU = (uint)(aAbs >> 32);
    var bLoU = (uint)bAbs;
    var bHiU = (uint)(bAbs >> 32);

    // Compute partial products
    ulong loLo = (ulong)aLoU * bLoU;
    ulong loHi = (ulong)aLoU * bHiU;
    ulong hiLo = (ulong)aHiU * bLoU;
    ulong hiHi = (ulong)aHiU * bHiU;

    // Combine (we need bits 32-95 of the 128-bit result, i.e., shift right by 32)
    ulong mid = (loLo >> 32) + (uint)loHi + (uint)hiLo;
    ulong high = hiHi + (loHi >> 32) + (hiLo >> 32) + (mid >> 32);
    ulong result = (high << 32) | (mid & 0xFFFFFFFF);

    // Apply sign
    if (aSign != bSign)
      return new(-(long)result);
    return new((long)result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator /(Q31_32 left, Q31_32 right) {
    // For division, we need to shift left by 32 bits before dividing
    // This requires 128-bit arithmetic or approximation
    // Use double as intermediate for simplicity (some precision loss)
    var result = (left.RawValue / (double)right.RawValue) * Scale;
    return new((long)result);
  }

  // Unary plus
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator +(Q31_32 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator %(Q31_32 left, Q31_32 right) => new(left.RawValue % right.RawValue);

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator ++(Q31_32 value) => new(value.RawValue + 1);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator --(Q31_32 value) => new(value.RawValue - 1);

  // Mixed-type arithmetic with integers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator *(Q31_32 left, long right) => new(left.RawValue * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator *(long left, Q31_32 right) => new(left * right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator /(Q31_32 left, long right) => new(left.RawValue / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator +(Q31_32 left, long right) => new(left.RawValue + (right << FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator +(long left, Q31_32 right) => new((left << FractionalBits) + right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator -(Q31_32 left, long right) => new(left.RawValue - (right << FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 operator -(long left, Q31_32 right) => new((left << FractionalBits) - right.RawValue);

  // Conversions from integers (implicit - safe within range)
  public static implicit operator Q31_32(sbyte value) => FromInt32(value);
  public static implicit operator Q31_32(short value) => FromInt32(value);
  public static implicit operator Q31_32(int value) => FromInt32(value);

  // Conversions to integers (explicit - truncation)
  public static explicit operator sbyte(Q31_32 value) => (sbyte)value.ToInt32();
  public static explicit operator short(Q31_32 value) => (short)value.ToInt32();
  public static explicit operator int(Q31_32 value) => value.ToInt32();
  public static explicit operator long(Q31_32 value) => value.ToInt64();

  // Conversions from floating point (explicit - precision loss possible)
  public static explicit operator Q31_32(float value) => FromSingle(value);
  public static explicit operator Q31_32(double value) => FromDouble(value);

  // Conversions to floating point (explicit - representation change)
  public static explicit operator float(Q31_32 value) => value.ToSingle();
  public static explicit operator double(Q31_32 value) => value.ToDouble();

  // Raw value conversion (explicit)
  public static explicit operator Q31_32(long raw) => FromRaw(raw);

  // Narrowing to smaller fixed-point (explicit - precision loss)
  public static explicit operator Q7_8(Q31_32 value) => Q7_8.FromRaw((short)(value.RawValue >> 24));
  public static explicit operator Q15_16(Q31_32 value) => Q15_16.FromRaw((int)(value.RawValue >> 16));

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 Abs(Q31_32 value) => value.RawValue >= 0 ? value : new(-value.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 Min(Q31_32 left, Q31_32 right) => left.RawValue <= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 Max(Q31_32 left, Q31_32 right) => left.RawValue >= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q31_32 Clamp(Q31_32 value, Q31_32 min, Q31_32 max) =>
    value.RawValue < min.RawValue ? min : value.RawValue > max.RawValue ? max : value;

  // Parsing
  public static Q31_32 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static Q31_32 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static Q31_32 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    return FromDouble(value);
  }

  public static bool TryParse(string? s, out Q31_32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Q31_32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Q31_32 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
