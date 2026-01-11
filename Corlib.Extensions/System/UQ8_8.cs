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
/// Represents an unsigned UQ8.8 fixed-point number (16-bit: 8 integer + 8 fractional bits).
/// Range: 0 to approximately 255.996 with resolution of 1/256.
/// </summary>
public readonly struct UQ8_8 : IComparable, IComparable<UQ8_8>, IEquatable<UQ8_8>, IFormattable, IParsable<UQ8_8> {

  private const int FractionalBits = 8;
  private const int Scale = 1 << FractionalBits; // 256

  /// <summary>
  /// Gets the raw underlying fixed-point representation.
  /// </summary>
  public ushort RawValue { get; }

  private UQ8_8(ushort raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a UQ8_8 from the raw fixed-point representation.
  /// </summary>
  public static UQ8_8 FromRaw(ushort raw) => new(raw);

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static UQ8_8 Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static UQ8_8 One => new(Scale);

  /// <summary>
  /// Gets the smallest positive value (1/256).
  /// </summary>
  public static UQ8_8 Epsilon => new(1);

  /// <summary>
  /// Gets the maximum value (~255.996).
  /// </summary>
  public static UQ8_8 MaxValue => new(ushort.MaxValue);

  /// <summary>
  /// Gets the minimum value (0).
  /// </summary>
  public static UQ8_8 MinValue => new(0);

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(UQ8_8 other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not UQ8_8 other)
      throw new ArgumentException("Object must be of type UQ8_8.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(UQ8_8 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is UQ8_8 other && this.Equals(other);

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
  public static UQ8_8 FromSingle(float value) => new((ushort)(value * Scale));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 FromDouble(double value) => new((ushort)(value * Scale));

  // Factory method from integer
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 FromUInt32(uint value) => new((ushort)(value << FractionalBits));

  // Integer part extraction
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public uint ToUInt32() => (uint)(this.RawValue >> FractionalBits);

  // Comparison operators
  public static bool operator ==(UQ8_8 left, UQ8_8 right) => left.Equals(right);
  public static bool operator !=(UQ8_8 left, UQ8_8 right) => !left.Equals(right);
  public static bool operator <(UQ8_8 left, UQ8_8 right) => left.RawValue < right.RawValue;
  public static bool operator >(UQ8_8 left, UQ8_8 right) => left.RawValue > right.RawValue;
  public static bool operator <=(UQ8_8 left, UQ8_8 right) => left.RawValue <= right.RawValue;
  public static bool operator >=(UQ8_8 left, UQ8_8 right) => left.RawValue >= right.RawValue;

  // Arithmetic operators
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator +(UQ8_8 left, UQ8_8 right) => new((ushort)(left.RawValue + right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator -(UQ8_8 left, UQ8_8 right) => new((ushort)(left.RawValue - right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator *(UQ8_8 left, UQ8_8 right) => new((ushort)((left.RawValue * right.RawValue) >> FractionalBits));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator /(UQ8_8 left, UQ8_8 right) => new((ushort)((left.RawValue << FractionalBits) / right.RawValue));

  // Unary plus
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator +(UQ8_8 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator %(UQ8_8 left, UQ8_8 right) => new((ushort)(left.RawValue % right.RawValue));

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator ++(UQ8_8 value) => new((ushort)(value.RawValue + 1));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator --(UQ8_8 value) => new((ushort)(value.RawValue - 1));

  // Mixed-type arithmetic with integers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator *(UQ8_8 left, int right) => new((ushort)(left.RawValue * right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator *(int left, UQ8_8 right) => new((ushort)(left * right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator /(UQ8_8 left, int right) => new((ushort)(left.RawValue / right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator +(UQ8_8 left, int right) => new((ushort)(left.RawValue + (right << FractionalBits)));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator +(int left, UQ8_8 right) => new((ushort)((left << FractionalBits) + right.RawValue));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator -(UQ8_8 left, int right) => new((ushort)(left.RawValue - (right << FractionalBits)));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 operator -(int left, UQ8_8 right) => new((ushort)((left << FractionalBits) - right.RawValue));

  // Conversions from integers (implicit - safe)
  public static implicit operator UQ8_8(byte value) => FromUInt32(value);

  // Conversions to integers (explicit - truncation)
  public static explicit operator byte(UQ8_8 value) => (byte)value.ToUInt32();
  public static explicit operator ushort(UQ8_8 value) => (ushort)value.ToUInt32();
  public static explicit operator uint(UQ8_8 value) => value.ToUInt32();

  // Conversions from floating point (explicit - precision loss possible)
  public static explicit operator UQ8_8(float value) => FromSingle(value);
  public static explicit operator UQ8_8(double value) => FromDouble(value);

  // Conversions to floating point (explicit - representation change)
  public static explicit operator float(UQ8_8 value) => value.ToSingle();
  public static explicit operator double(UQ8_8 value) => value.ToDouble();

  // Raw value conversion (explicit)
  public static explicit operator UQ8_8(ushort raw) => FromRaw(raw);

  // Widening to larger fixed-point types (implicit - safe)
  public static implicit operator UQ16_16(UQ8_8 value) => UQ16_16.FromRaw((uint)value.RawValue << 8);
  public static implicit operator UQ32_32(UQ8_8 value) => UQ32_32.FromRaw((ulong)value.RawValue << 24);

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 Min(UQ8_8 left, UQ8_8 right) => left.RawValue <= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 Max(UQ8_8 left, UQ8_8 right) => left.RawValue >= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UQ8_8 Clamp(UQ8_8 value, UQ8_8 min, UQ8_8 max) =>
    value.RawValue < min.RawValue ? min : value.RawValue > max.RawValue ? max : value;

  // Parsing
  public static UQ8_8 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static UQ8_8 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static UQ8_8 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    return FromDouble(value);
  }

  public static bool TryParse(string? s, out UQ8_8 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out UQ8_8 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out UQ8_8 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
