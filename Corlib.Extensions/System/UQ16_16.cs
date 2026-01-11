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
/// Represents an unsigned UQ16.16 fixed-point number (32-bit: 16 integer + 16 fractional bits).
/// Range: 0 to approximately 65535.99998 with resolution of 1/65536.
/// </summary>
public readonly struct UQ16_16 : IComparable, IComparable<UQ16_16>, IEquatable<UQ16_16>, IFormattable, IParsable<UQ16_16> {

  private const int FractionalBits = 16;
  private const uint Scale = 1u << FractionalBits; // 65536

  /// <summary>
  /// Gets the raw underlying fixed-point representation.
  /// </summary>
  public uint RawValue { get; }

  private UQ16_16(uint raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a UQ16_16 from the raw fixed-point representation.
  /// </summary>
  public static UQ16_16 FromRaw(uint raw) => new(raw);

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static UQ16_16 Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static UQ16_16 One => new(Scale);

  /// <summary>
  /// Gets the smallest positive value (1/65536).
  /// </summary>
  public static UQ16_16 Epsilon => new(1);

  /// <summary>
  /// Gets the maximum value (~65535.99998).
  /// </summary>
  public static UQ16_16 MaxValue => new(uint.MaxValue);

  /// <summary>
  /// Gets the minimum value (0).
  /// </summary>
  public static UQ16_16 MinValue => new(0);

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(UQ16_16 other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not UQ16_16 other)
      throw new ArgumentException("Object must be of type UQ16_16.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(UQ16_16 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is UQ16_16 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDouble().ToString(CultureInfo.InvariantCulture);

  public string ToString(IFormatProvider? provider) => this.ToDouble().ToString(provider);

  public string ToString(string? format) => this.ToDouble().ToString(format, CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? provider) => this.ToDouble().ToString(format, provider);

  // Conversion to floating point
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ToSingle() => (float)this.RawValue / Scale;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double ToDouble() => (double)this.RawValue / Scale;

  // Factory methods from floating point
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 FromSingle(float value) => new((uint)(value * Scale));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 FromDouble(double value) => new((uint)(value * Scale));

  // Factory method from integer
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 FromUInt32(uint value) => new(value << FractionalBits);

  // Integer part extraction
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public uint ToUInt32() => this.RawValue >> FractionalBits;

  // Comparison operators
  public static bool operator ==(UQ16_16 left, UQ16_16 right) => left.Equals(right);
  public static bool operator !=(UQ16_16 left, UQ16_16 right) => !left.Equals(right);
  public static bool operator <(UQ16_16 left, UQ16_16 right) => left.RawValue < right.RawValue;
  public static bool operator >(UQ16_16 left, UQ16_16 right) => left.RawValue > right.RawValue;
  public static bool operator <=(UQ16_16 left, UQ16_16 right) => left.RawValue <= right.RawValue;
  public static bool operator >=(UQ16_16 left, UQ16_16 right) => left.RawValue >= right.RawValue;

  // Arithmetic operators
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator +(UQ16_16 left, UQ16_16 right) => new(left.RawValue + right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator -(UQ16_16 left, UQ16_16 right) => new(left.RawValue - right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator *(UQ16_16 left, UQ16_16 right) => new((uint)(((ulong)left.RawValue * right.RawValue) >> FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator /(UQ16_16 left, UQ16_16 right) => new((uint)(((ulong)left.RawValue << FractionalBits) / right.RawValue));

  // Unary plus
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator +(UQ16_16 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator %(UQ16_16 left, UQ16_16 right) => new(left.RawValue % right.RawValue);

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator ++(UQ16_16 value) => new(value.RawValue + 1);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator --(UQ16_16 value) => new(value.RawValue - 1);

  // Mixed-type arithmetic with integers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator *(UQ16_16 left, int right) => new((uint)(left.RawValue * right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator *(int left, UQ16_16 right) => new((uint)(left * right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator /(UQ16_16 left, int right) => new(left.RawValue / (uint)right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator +(UQ16_16 left, int right) => new((uint)(left.RawValue + (right << FractionalBits)));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator +(int left, UQ16_16 right) => new((uint)((left << FractionalBits) + right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator -(UQ16_16 left, int right) => new((uint)(left.RawValue - (right << FractionalBits)));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 operator -(int left, UQ16_16 right) => new((uint)((left << FractionalBits) - right.RawValue));

  // Conversions from integers (implicit - safe within range)
  public static implicit operator UQ16_16(byte value) => FromUInt32(value);
  public static implicit operator UQ16_16(ushort value) => FromUInt32(value);

  // Conversions to integers (explicit - truncation)
  public static explicit operator byte(UQ16_16 value) => (byte)value.ToUInt32();
  public static explicit operator ushort(UQ16_16 value) => (ushort)value.ToUInt32();
  public static explicit operator uint(UQ16_16 value) => value.ToUInt32();

  // Conversions from floating point (explicit - precision loss possible)
  public static explicit operator UQ16_16(float value) => FromSingle(value);
  public static explicit operator UQ16_16(double value) => FromDouble(value);

  // Conversions to floating point (explicit - representation change)
  public static explicit operator float(UQ16_16 value) => value.ToSingle();
  public static explicit operator double(UQ16_16 value) => value.ToDouble();

  // Raw value conversion (explicit)
  public static explicit operator UQ16_16(uint raw) => FromRaw(raw);

  // Narrowing from larger fixed-point (explicit - precision loss)
  public static explicit operator UQ16_16(UQ32_32 value) => new((uint)(value.RawValue >> 16));

  // Narrowing from smaller fixed-point (explicit - handled by UQ8_8's implicit)
  public static explicit operator UQ8_8(UQ16_16 value) => UQ8_8.FromRaw((ushort)(value.RawValue >> 8));

  // Widening to larger fixed-point types (implicit - safe)
  public static implicit operator UQ32_32(UQ16_16 value) => UQ32_32.FromRaw((ulong)value.RawValue << 16);

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 Min(UQ16_16 left, UQ16_16 right) => left.RawValue <= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 Max(UQ16_16 left, UQ16_16 right) => left.RawValue >= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ16_16 Clamp(UQ16_16 value, UQ16_16 min, UQ16_16 max) =>
    value.RawValue < min.RawValue ? min : value.RawValue > max.RawValue ? max : value;

  // Parsing
  public static UQ16_16 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static UQ16_16 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static UQ16_16 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    return FromDouble(value);
  }

  public static bool TryParse(string? s, out UQ16_16 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out UQ16_16 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out UQ16_16 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
