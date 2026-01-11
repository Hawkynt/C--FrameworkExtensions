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
/// Represents a signed Q15.16 fixed-point number (32-bit: 1 sign + 15 integer + 16 fractional bits).
/// Range: -32768 to approximately 32767.99998 with resolution of 1/65536.
/// </summary>
public readonly struct Q15_16 : IComparable, IComparable<Q15_16>, IEquatable<Q15_16>, IFormattable, IParsable<Q15_16> {

  private const int FractionalBits = 16;
  private const int Scale = 1 << FractionalBits; // 65536

  /// <summary>
  /// Gets the raw underlying fixed-point representation.
  /// </summary>
  public int RawValue { get; }

  private Q15_16(int raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a Q15_16 from the raw fixed-point representation.
  /// </summary>
  public static Q15_16 FromRaw(int raw) => new(raw);

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static Q15_16 Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static Q15_16 One => new(Scale);

  /// <summary>
  /// Gets the smallest positive value (1/65536).
  /// </summary>
  public static Q15_16 Epsilon => new(1);

  /// <summary>
  /// Gets the maximum value (~32767.99998).
  /// </summary>
  public static Q15_16 MaxValue => new(int.MaxValue);

  /// <summary>
  /// Gets the minimum value (-32768).
  /// </summary>
  public static Q15_16 MinValue => new(int.MinValue);

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(Q15_16 other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Q15_16 other)
      throw new ArgumentException("Object must be of type Q15_16.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Q15_16 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is Q15_16 other && this.Equals(other);

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
  public static Q15_16 FromSingle(float value) => new((int)(value * Scale));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 FromDouble(double value) => new((int)(value * Scale));

  // Factory method from integer
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 FromInt32(int value) => new(value << FractionalBits);

  // Integer part extraction
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int ToInt32() => this.RawValue >> FractionalBits;

  // Comparison operators
  public static bool operator ==(Q15_16 left, Q15_16 right) => left.Equals(right);
  public static bool operator !=(Q15_16 left, Q15_16 right) => !left.Equals(right);
  public static bool operator <(Q15_16 left, Q15_16 right) => left.RawValue < right.RawValue;
  public static bool operator >(Q15_16 left, Q15_16 right) => left.RawValue > right.RawValue;
  public static bool operator <=(Q15_16 left, Q15_16 right) => left.RawValue <= right.RawValue;
  public static bool operator >=(Q15_16 left, Q15_16 right) => left.RawValue >= right.RawValue;

  // Arithmetic operators
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator +(Q15_16 left, Q15_16 right) => new(left.RawValue + right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator -(Q15_16 left, Q15_16 right) => new(left.RawValue - right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator -(Q15_16 value) => new(-value.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator *(Q15_16 left, Q15_16 right) => new((int)(((long)left.RawValue * right.RawValue) >> FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator /(Q15_16 left, Q15_16 right) => new((int)(((long)left.RawValue << FractionalBits) / right.RawValue));

  // Unary plus
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator +(Q15_16 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator %(Q15_16 left, Q15_16 right) => new(left.RawValue % right.RawValue);

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator ++(Q15_16 value) => new(value.RawValue + 1);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator --(Q15_16 value) => new(value.RawValue - 1);

  // Mixed-type arithmetic with integers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator *(Q15_16 left, int right) => new(left.RawValue * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator *(int left, Q15_16 right) => new(left * right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator /(Q15_16 left, int right) => new(left.RawValue / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator +(Q15_16 left, int right) => new(left.RawValue + (right << FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator +(int left, Q15_16 right) => new((left << FractionalBits) + right.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator -(Q15_16 left, int right) => new(left.RawValue - (right << FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 operator -(int left, Q15_16 right) => new((left << FractionalBits) - right.RawValue);

  // Conversions from integers (implicit - safe within range)
  public static implicit operator Q15_16(sbyte value) => FromInt32(value);
  public static implicit operator Q15_16(short value) => FromInt32(value);

  // Conversions to integers (explicit - truncation)
  public static explicit operator sbyte(Q15_16 value) => (sbyte)value.ToInt32();
  public static explicit operator short(Q15_16 value) => (short)value.ToInt32();
  public static explicit operator int(Q15_16 value) => value.ToInt32();

  // Conversions from floating point (explicit - precision loss possible)
  public static explicit operator Q15_16(float value) => FromSingle(value);
  public static explicit operator Q15_16(double value) => FromDouble(value);

  // Conversions to floating point (explicit - representation change)
  public static explicit operator float(Q15_16 value) => value.ToSingle();
  public static explicit operator double(Q15_16 value) => value.ToDouble();

  // Raw value conversion (explicit)
  public static explicit operator Q15_16(int raw) => FromRaw(raw);

  // Narrowing from larger fixed-point (explicit - precision loss)
  public static explicit operator Q15_16(Q31_32 value) => new((int)(value.RawValue >> 16));

  // Narrowing from smaller fixed-point (explicit - extend precision safely handled by Q7_8's implicit)
  public static explicit operator Q7_8(Q15_16 value) => Q7_8.FromRaw((short)(value.RawValue >> 8));

  // Widening to larger fixed-point types (implicit - safe)
  public static implicit operator Q31_32(Q15_16 value) => Q31_32.FromRaw((long)value.RawValue << 16);

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 Abs(Q15_16 value) => value.RawValue >= 0 ? value : new(-value.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 Min(Q15_16 left, Q15_16 right) => left.RawValue <= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 Max(Q15_16 left, Q15_16 right) => left.RawValue >= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q15_16 Clamp(Q15_16 value, Q15_16 min, Q15_16 max) =>
    value.RawValue < min.RawValue ? min : value.RawValue > max.RawValue ? max : value;

  // Parsing
  public static Q15_16 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static Q15_16 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static Q15_16 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    return FromDouble(value);
  }

  public static bool TryParse(string? s, out Q15_16 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Q15_16 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Q15_16 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
