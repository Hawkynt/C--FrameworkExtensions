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
/// Represents a signed Q7.8 fixed-point number (16-bit: 1 sign + 7 integer + 8 fractional bits).
/// Range: -128 to approximately 127.996 with resolution of 1/256.
/// </summary>
public readonly struct Q7_8 : IComparable, IComparable<Q7_8>, IEquatable<Q7_8>, IFormattable, ISpanFormattable, IParsable<Q7_8>, ISpanParsable<Q7_8> {

  private const int FractionalBits = 8;
  private const int Scale = 1 << FractionalBits; // 256

  /// <summary>
  /// Gets the raw underlying fixed-point representation.
  /// </summary>
  public short RawValue { get; }

  private Q7_8(short raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a Q7_8 from the raw fixed-point representation.
  /// </summary>
  public static Q7_8 FromRaw(short raw) => new(raw);

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static Q7_8 Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static Q7_8 One => new(Scale);

  /// <summary>
  /// Gets the smallest positive value (1/256).
  /// </summary>
  public static Q7_8 Epsilon => new(1);

  /// <summary>
  /// Gets the maximum value (~127.996).
  /// </summary>
  public static Q7_8 MaxValue => new(short.MaxValue);

  /// <summary>
  /// Gets the minimum value (-128).
  /// </summary>
  public static Q7_8 MinValue => new(short.MinValue);

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(Q7_8 other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Q7_8 other)
      throw new ArgumentException("Object must be of type Q7_8.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Q7_8 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is Q7_8 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDouble().ToString(CultureInfo.InvariantCulture);

  public string ToString(IFormatProvider? provider) => this.ToDouble().ToString(provider);

  public string ToString(string? format) => this.ToDouble().ToString(format, CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? provider) => this.ToDouble().ToString(format, provider);

  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
    var str = format.IsEmpty
      ? this.ToDouble().ToString(provider)
      : this.ToDouble().ToString(format.ToString(), provider);
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }
    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  // Conversion to floating point
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ToSingle() => (float)this.RawValue / Scale;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double ToDouble() => (double)this.RawValue / Scale;

  // Factory methods from floating point
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 FromSingle(float value) => new((short)(value * Scale));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 FromDouble(double value) => new((short)(value * Scale));

  // Factory method from integer
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 FromInt32(int value) => new((short)(value << FractionalBits));

  // Integer part extraction
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int ToInt32() => this.RawValue >> FractionalBits;

  // Comparison operators
  public static bool operator ==(Q7_8 left, Q7_8 right) => left.Equals(right);
  public static bool operator !=(Q7_8 left, Q7_8 right) => !left.Equals(right);
  public static bool operator <(Q7_8 left, Q7_8 right) => left.RawValue < right.RawValue;
  public static bool operator >(Q7_8 left, Q7_8 right) => left.RawValue > right.RawValue;
  public static bool operator <=(Q7_8 left, Q7_8 right) => left.RawValue <= right.RawValue;
  public static bool operator >=(Q7_8 left, Q7_8 right) => left.RawValue >= right.RawValue;

  // Arithmetic operators
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator +(Q7_8 left, Q7_8 right) => new((short)(left.RawValue + right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator -(Q7_8 left, Q7_8 right) => new((short)(left.RawValue - right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator -(Q7_8 value) => new((short)(-value.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator *(Q7_8 left, Q7_8 right) => new((short)((left.RawValue * right.RawValue) >> FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator /(Q7_8 left, Q7_8 right) => new((short)((left.RawValue << FractionalBits) / right.RawValue));

  // Unary plus
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator +(Q7_8 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator %(Q7_8 left, Q7_8 right) => new((short)(left.RawValue % right.RawValue));

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator ++(Q7_8 value) => new((short)(value.RawValue + 1));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator --(Q7_8 value) => new((short)(value.RawValue - 1));

  // Mixed-type arithmetic with integers (scales the integer to fixed-point)
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator *(Q7_8 left, int right) => new((short)(left.RawValue * right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator *(int left, Q7_8 right) => new((short)(left * right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator /(Q7_8 left, int right) => new((short)(left.RawValue / right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator +(Q7_8 left, int right) => new((short)(left.RawValue + (right << FractionalBits)));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator +(int left, Q7_8 right) => new((short)((left << FractionalBits) + right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator -(Q7_8 left, int right) => new((short)(left.RawValue - (right << FractionalBits)));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 operator -(int left, Q7_8 right) => new((short)((left << FractionalBits) - right.RawValue));

  // Conversions from integers (implicit - safe)
  public static implicit operator Q7_8(sbyte value) => FromInt32(value);

  // Conversions to integers (explicit - truncation)
  public static explicit operator sbyte(Q7_8 value) => (sbyte)value.ToInt32();
  public static explicit operator short(Q7_8 value) => (short)value.ToInt32();
  public static explicit operator int(Q7_8 value) => value.ToInt32();

  // Conversions from floating point (explicit - precision loss possible)
  public static explicit operator Q7_8(float value) => FromSingle(value);
  public static explicit operator Q7_8(double value) => FromDouble(value);

  // Conversions to floating point (implicit - widening, no precision loss)
  public static implicit operator float(Q7_8 value) => value.ToSingle();
  public static implicit operator double(Q7_8 value) => value.ToDouble();

  // Raw value conversion (explicit)
  public static explicit operator Q7_8(short raw) => FromRaw(raw);

  // Widening to larger fixed-point types (implicit - safe)
  public static implicit operator Q15_16(Q7_8 value) => Q15_16.FromRaw((int)value.RawValue << 8);
  public static implicit operator Q31_32(Q7_8 value) => Q31_32.FromRaw((long)value.RawValue << 24);

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 Abs(Q7_8 value) => value.RawValue >= 0 ? value : new((short)(-value.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 Min(Q7_8 left, Q7_8 right) => left.RawValue <= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 Max(Q7_8 left, Q7_8 right) => left.RawValue >= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q7_8 Clamp(Q7_8 value, Q7_8 min, Q7_8 max) =>
    value.RawValue < min.RawValue ? min : value.RawValue > max.RawValue ? max : value;

  // Parsing
  public static Q7_8 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static Q7_8 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static Q7_8 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    return FromDouble(value);
  }

  public static bool TryParse(string? s, out Q7_8 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Q7_8 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Q7_8 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static Q7_8 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
    var value = double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
    return FromDouble(value);
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Q7_8 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
