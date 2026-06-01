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
/// Represents a signed Q3.4 fixed-point number (8-bit: 1 sign + 3 integer + 4 fractional bits).
/// Range: -8 to approximately 7.9375 with resolution of 1/16.
/// </summary>
public readonly struct Q3_4 : IComparable, IComparable<Q3_4>, IEquatable<Q3_4>, IFormattable, ISpanFormattable, IParsable<Q3_4>, ISpanParsable<Q3_4> {

  private const int FractionalBits = 4;
  private const int Scale = 1 << FractionalBits; // 16

  /// <summary>
  /// Gets the raw underlying fixed-point representation.
  /// </summary>
  public sbyte RawValue { get; }

  private Q3_4(sbyte raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a Q3_4 from the raw fixed-point representation.
  /// </summary>
  public static Q3_4 FromRaw(sbyte raw) => new(raw);

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static Q3_4 Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static Q3_4 One => new(Scale);

  /// <summary>
  /// Gets the smallest positive value (1/16).
  /// </summary>
  public static Q3_4 Epsilon => new(1);

  /// <summary>
  /// Gets the maximum value (~7.9375).
  /// </summary>
  public static Q3_4 MaxValue => new(sbyte.MaxValue);

  /// <summary>
  /// Gets the minimum value (-8).
  /// </summary>
  public static Q3_4 MinValue => new(sbyte.MinValue);

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(Q3_4 other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Q3_4 other)
      throw new ArgumentException("Object must be of type Q3_4.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Q3_4 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is Q3_4 other && this.Equals(other);

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
  public static Q3_4 FromSingle(float value) => new((sbyte)(value * Scale));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 FromDouble(double value) => new((sbyte)(value * Scale));

  // Factory method from integer
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 FromInt32(int value) => new((sbyte)(value << FractionalBits));

  // Integer part extraction
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int ToInt32() => this.RawValue >> FractionalBits;

  // Comparison operators
  public static bool operator ==(Q3_4 left, Q3_4 right) => left.Equals(right);
  public static bool operator !=(Q3_4 left, Q3_4 right) => !left.Equals(right);
  public static bool operator <(Q3_4 left, Q3_4 right) => left.RawValue < right.RawValue;
  public static bool operator >(Q3_4 left, Q3_4 right) => left.RawValue > right.RawValue;
  public static bool operator <=(Q3_4 left, Q3_4 right) => left.RawValue <= right.RawValue;
  public static bool operator >=(Q3_4 left, Q3_4 right) => left.RawValue >= right.RawValue;

  // Arithmetic operators
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator +(Q3_4 left, Q3_4 right) => new((sbyte)(left.RawValue + right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator -(Q3_4 left, Q3_4 right) => new((sbyte)(left.RawValue - right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator -(Q3_4 value) => new((sbyte)(-value.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator *(Q3_4 left, Q3_4 right) => new((sbyte)((left.RawValue * right.RawValue) >> FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator /(Q3_4 left, Q3_4 right) => new((sbyte)((left.RawValue << FractionalBits) / right.RawValue));

  // Unary plus
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator +(Q3_4 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator %(Q3_4 left, Q3_4 right) => new((sbyte)(left.RawValue % right.RawValue));

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator ++(Q3_4 value) => new((sbyte)(value.RawValue + 1));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator --(Q3_4 value) => new((sbyte)(value.RawValue - 1));

  // Mixed-type arithmetic with integers (scales the integer to fixed-point)
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator *(Q3_4 left, int right) => new((sbyte)(left.RawValue * right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator *(int left, Q3_4 right) => new((sbyte)(left * right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator /(Q3_4 left, int right) => new((sbyte)(left.RawValue / right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator +(Q3_4 left, int right) => new((sbyte)(left.RawValue + (right << FractionalBits)));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator +(int left, Q3_4 right) => new((sbyte)((left << FractionalBits) + right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator -(Q3_4 left, int right) => new((sbyte)(left.RawValue - (right << FractionalBits)));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 operator -(int left, Q3_4 right) => new((sbyte)((left << FractionalBits) - right.RawValue));

  // Conversions from integers (implicit - safe)
  public static implicit operator Q3_4(sbyte value) => FromInt32(value);

  // Conversions to integers (explicit - truncation)
  public static explicit operator sbyte(Q3_4 value) => (sbyte)value.ToInt32();
  public static explicit operator short(Q3_4 value) => (short)value.ToInt32();
  public static explicit operator int(Q3_4 value) => value.ToInt32();

  // Conversions from floating point (explicit - precision loss possible)
  public static explicit operator Q3_4(float value) => FromSingle(value);
  public static explicit operator Q3_4(double value) => FromDouble(value);

  // Conversions to floating point (implicit - widening, no precision loss)
  public static implicit operator float(Q3_4 value) => value.ToSingle();
  public static implicit operator double(Q3_4 value) => value.ToDouble();

  // Raw value conversion (explicit)
  public static explicit operator Q3_4(byte raw) => FromRaw((sbyte)raw);

  // Widening to larger fixed-point types (implicit - safe)
  public static implicit operator Q7_8(Q3_4 value) => Q7_8.FromRaw((short)((short)value.RawValue << 4));
  public static implicit operator Q15_16(Q3_4 value) => Q15_16.FromRaw((int)value.RawValue << 12);
  public static implicit operator Q31_32(Q3_4 value) => Q31_32.FromRaw((long)value.RawValue << 28);

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 Abs(Q3_4 value) => value.RawValue >= 0 ? value : new((sbyte)(-value.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 Min(Q3_4 left, Q3_4 right) => left.RawValue <= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 Max(Q3_4 left, Q3_4 right) => left.RawValue >= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Q3_4 Clamp(Q3_4 value, Q3_4 min, Q3_4 max) =>
    value.RawValue < min.RawValue ? min : value.RawValue > max.RawValue ? max : value;

  // Parsing
  public static Q3_4 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static Q3_4 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static Q3_4 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    return FromDouble(value);
  }

  public static bool TryParse(string? s, out Q3_4 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Q3_4 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Q3_4 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static Q3_4 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
    var value = double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
    return FromDouble(value);
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Q3_4 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
