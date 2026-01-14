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
/// Represents an unsigned UQ32.32 fixed-point number (64-bit: 32 integer + 32 fractional bits).
/// Range: 0 to approximately 4294967295.99999999977 with resolution of 1/4294967296.
/// </summary>
public readonly struct UQ32_32 : IComparable, IComparable<UQ32_32>, IEquatable<UQ32_32>, IFormattable, IParsable<UQ32_32> {

  private const int FractionalBits = 32;
  private const ulong Scale = 1UL << FractionalBits; // 4294967296

  /// <summary>
  /// Gets the raw underlying fixed-point representation.
  /// </summary>
  public ulong RawValue { get; }

  private UQ32_32(ulong raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a UQ32_32 from the raw fixed-point representation.
  /// </summary>
  public static UQ32_32 FromRaw(ulong raw) => new(raw);

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static UQ32_32 Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static UQ32_32 One => new(Scale);

  /// <summary>
  /// Gets the smallest positive value (1/4294967296).
  /// </summary>
  public static UQ32_32 Epsilon => new(1);

  /// <summary>
  /// Gets the maximum value.
  /// </summary>
  public static UQ32_32 MaxValue => new(ulong.MaxValue);

  /// <summary>
  /// Gets the minimum value (0).
  /// </summary>
  public static UQ32_32 MinValue => new(0);

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(UQ32_32 other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not UQ32_32 other)
      throw new ArgumentException("Object must be of type UQ32_32.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(UQ32_32 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is UQ32_32 other && this.Equals(other);

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
  public static UQ32_32 FromSingle(float value) => new((ulong)(value * Scale));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 FromDouble(double value) => new((ulong)(value * Scale));

  // Factory method from integer
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 FromUInt32(uint value) => new((ulong)value << FractionalBits);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 FromUInt64(ulong value) => new(value << FractionalBits);

  // Integer part extraction
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public uint ToUInt32() => (uint)(this.RawValue >> FractionalBits);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ulong ToUInt64() => this.RawValue >> FractionalBits;

  // Comparison operators
  public static bool operator ==(UQ32_32 left, UQ32_32 right) => left.Equals(right);
  public static bool operator !=(UQ32_32 left, UQ32_32 right) => !left.Equals(right);
  public static bool operator <(UQ32_32 left, UQ32_32 right) => left.RawValue < right.RawValue;
  public static bool operator >(UQ32_32 left, UQ32_32 right) => left.RawValue > right.RawValue;
  public static bool operator <=(UQ32_32 left, UQ32_32 right) => left.RawValue <= right.RawValue;
  public static bool operator >=(UQ32_32 left, UQ32_32 right) => left.RawValue >= right.RawValue;

  // Arithmetic operators
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator +(UQ32_32 left, UQ32_32 right) => new(left.RawValue + right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator -(UQ32_32 left, UQ32_32 right) => new(left.RawValue - right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator *(UQ32_32 left, UQ32_32 right) {
    // Use 128-bit multiplication to avoid overflow
    var aLo = (uint)left.RawValue;
    var aHi = (uint)(left.RawValue >> 32);
    var bLo = (uint)right.RawValue;
    var bHi = (uint)(right.RawValue >> 32);

    // Compute partial products
    ulong loLo = (ulong)aLo * bLo;
    ulong loHi = (ulong)aLo * bHi;
    ulong hiLo = (ulong)aHi * bLo;
    ulong hiHi = (ulong)aHi * bHi;

    // Combine (we need bits 32-95 of the 128-bit result, i.e., shift right by 32)
    ulong mid = (loLo >> 32) + (uint)loHi + (uint)hiLo;
    ulong high = hiHi + (loHi >> 32) + (hiLo >> 32) + (mid >> 32);
    ulong result = (high << 32) | (mid & 0xFFFFFFFF);

    return new(result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator /(UQ32_32 left, UQ32_32 right) {
    // For division, we need to shift left by 32 bits before dividing
    // Use double as intermediate for simplicity (some precision loss)
    var result = (left.RawValue / (double)right.RawValue) * Scale;
    return new((ulong)result);
  }

  // Unary plus
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator +(UQ32_32 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator %(UQ32_32 left, UQ32_32 right) => new(left.RawValue % right.RawValue);

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator ++(UQ32_32 value) => new(value.RawValue + 1);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator --(UQ32_32 value) => new(value.RawValue - 1);

  // Mixed-type arithmetic with integers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator *(UQ32_32 left, long right) => new(left.RawValue * (ulong)right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator *(long left, UQ32_32 right) => new((ulong)left * right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator /(UQ32_32 left, long right) => new(left.RawValue / (ulong)right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator +(UQ32_32 left, long right) => new(left.RawValue + ((ulong)right << FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator +(long left, UQ32_32 right) => new(((ulong)left << FractionalBits) + right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator -(UQ32_32 left, long right) => new(left.RawValue - ((ulong)right << FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 operator -(long left, UQ32_32 right) => new(((ulong)left << FractionalBits) - right.RawValue);

  // Conversions from integers (implicit - safe within range)
  public static implicit operator UQ32_32(byte value) => FromUInt32(value);
  public static implicit operator UQ32_32(ushort value) => FromUInt32(value);
  public static implicit operator UQ32_32(uint value) => FromUInt32(value);

  // Conversions to integers (explicit - truncation)
  public static explicit operator byte(UQ32_32 value) => (byte)value.ToUInt32();
  public static explicit operator ushort(UQ32_32 value) => (ushort)value.ToUInt32();
  public static explicit operator uint(UQ32_32 value) => value.ToUInt32();
  public static explicit operator ulong(UQ32_32 value) => value.ToUInt64();

  // Conversions from floating point (explicit - precision loss possible)
  public static explicit operator UQ32_32(float value) => FromSingle(value);
  public static explicit operator UQ32_32(double value) => FromDouble(value);

  // Conversions to floating point (explicit - representation change)
  public static explicit operator float(UQ32_32 value) => value.ToSingle();
  public static explicit operator double(UQ32_32 value) => value.ToDouble();

  // Raw value conversion (explicit)
  public static explicit operator UQ32_32(ulong raw) => FromRaw(raw);

  // Narrowing to smaller fixed-point (explicit - precision loss)
  public static explicit operator UQ8_8(UQ32_32 value) => UQ8_8.FromRaw((ushort)(value.RawValue >> 24));
  public static explicit operator UQ16_16(UQ32_32 value) => UQ16_16.FromRaw((uint)(value.RawValue >> 16));

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 Min(UQ32_32 left, UQ32_32 right) => left.RawValue <= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 Max(UQ32_32 left, UQ32_32 right) => left.RawValue >= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ32_32 Clamp(UQ32_32 value, UQ32_32 min, UQ32_32 max) =>
    value.RawValue < min.RawValue ? min : value.RawValue > max.RawValue ? max : value;

  // Parsing
  public static UQ32_32 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static UQ32_32 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static UQ32_32 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    return FromDouble(value);
  }

  public static bool TryParse(string? s, out UQ32_32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out UQ32_32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out UQ32_32 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
