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
/// Represents an 8-bit packed BCD value storing 2 decimal digits (0-99).
/// Each nibble contains one decimal digit (0-9).
/// </summary>
public readonly struct PackedBCD8 : IComparable, IComparable<PackedBCD8>, IEquatable<PackedBCD8>, IFormattable, ISpanFormattable, IParsable<PackedBCD8>, ISpanParsable<PackedBCD8> {
  /// <summary>
  /// Gets the raw BCD representation.
  /// </summary>
  public byte RawValue { get; }

  private PackedBCD8(byte raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a PackedBCD8 from the raw BCD representation.
  /// </summary>
  /// <exception cref="ArgumentException">Thrown if any nibble contains an invalid BCD digit (>9).</exception>
  public static PackedBCD8 FromRaw(byte raw) {
    _ValidateRaw(raw);
    return new(raw);
  }

  /// <summary>
  /// Creates a PackedBCD8 from a decimal value (0-99).
  /// </summary>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if value is not in range 0-99.</exception>
  public static PackedBCD8 FromValue(int value) {
    if (value is < 0 or > 99)
      throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0 and 99.");
    return new(_Encode(value));
  }

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static PackedBCD8 Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static PackedBCD8 One => new(1);

  /// <summary>
  /// Gets the maximum value (99).
  /// </summary>
  public static PackedBCD8 MaxValue => new(0x99);

  /// <summary>
  /// Gets the minimum value (0).
  /// </summary>
  public static PackedBCD8 MinValue => new(0);

  /// <summary>
  /// Gets the decimal value represented by this BCD.
  /// </summary>
  public int Value => _Decode(this.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ValidateRaw(byte raw) {
    if ((raw & 0x0F) > 9 || ((raw >> 4) & 0x0F) > 9)
      throw new ArgumentException("Invalid BCD: nibble values must be 0-9.", nameof(raw));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _Encode(int value) {
    var tens = value / 10;
    var ones = value % 10;
    return (byte)((tens << 4) | ones);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _Decode(byte raw) => ((raw >> 4) & 0x0F) * 10 + (raw & 0x0F);

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(PackedBCD8 other) => this.Value.CompareTo(other.Value);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not PackedBCD8 other)
      throw new ArgumentException("Object must be of type PackedBCD8.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(PackedBCD8 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is PackedBCD8 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.Value.ToString("D2");

  public string ToString(IFormatProvider? provider) => this.Value.ToString(provider);

  public string ToString(string? format) => this.Value.ToString(format);

  public string ToString(string? format, IFormatProvider? provider) => this.Value.ToString(format, provider);

  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
    var str = format.IsEmpty
      ? this.Value.ToString(provider)
      : this.Value.ToString(format.ToString(), provider);
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }
    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  // Comparison operators
  public static bool operator ==(PackedBCD8 left, PackedBCD8 right) => left.Equals(right);
  public static bool operator !=(PackedBCD8 left, PackedBCD8 right) => !left.Equals(right);
  public static bool operator <(PackedBCD8 left, PackedBCD8 right) => left.Value < right.Value;
  public static bool operator >(PackedBCD8 left, PackedBCD8 right) => left.Value > right.Value;
  public static bool operator <=(PackedBCD8 left, PackedBCD8 right) => left.Value <= right.Value;
  public static bool operator >=(PackedBCD8 left, PackedBCD8 right) => left.Value >= right.Value;

  // Arithmetic operators (result clamped to valid BCD range)
  public static PackedBCD8 operator +(PackedBCD8 left, PackedBCD8 right) {
    var result = left.Value + right.Value;
    if (result > 99)
      throw new OverflowException("BCD addition overflow.");
    return new(_Encode(result));
  }

  public static PackedBCD8 operator -(PackedBCD8 left, PackedBCD8 right) {
    var result = left.Value - right.Value;
    if (result < 0)
      throw new OverflowException("BCD subtraction underflow.");
    return new(_Encode(result));
  }

  public static PackedBCD8 operator *(PackedBCD8 left, PackedBCD8 right) {
    var result = left.Value * right.Value;
    if (result > 99)
      throw new OverflowException("BCD multiplication overflow.");
    return new(_Encode(result));
  }

  public static PackedBCD8 operator /(PackedBCD8 left, PackedBCD8 right) {
    if (right.Value == 0)
      throw new DivideByZeroException();
    return new(_Encode(left.Value / right.Value));
  }

  public static PackedBCD8 operator %(PackedBCD8 left, PackedBCD8 right) {
    if (right.Value == 0)
      throw new DivideByZeroException();
    return new(_Encode(left.Value % right.Value));
  }

  public static PackedBCD8 operator ++(PackedBCD8 value) {
    if (value.Value >= 99)
      throw new OverflowException("BCD increment overflow.");
    return new(_Encode(value.Value + 1));
  }

  public static PackedBCD8 operator --(PackedBCD8 value) {
    if (value.Value <= 0)
      throw new OverflowException("BCD decrement underflow.");
    return new(_Encode(value.Value - 1));
  }

  // Conversions
  public static implicit operator PackedBCD8(byte value) => FromValue(value);
  public static explicit operator byte(PackedBCD8 value) => (byte)value.Value;

  // Implicit widening to larger integer types (value range 0-99 fits in all)
  public static implicit operator short(PackedBCD8 value) => (short)value.Value;
  public static implicit operator ushort(PackedBCD8 value) => (ushort)value.Value;
  public static implicit operator int(PackedBCD8 value) => value.Value;
  public static implicit operator uint(PackedBCD8 value) => (uint)value.Value;
  public static implicit operator long(PackedBCD8 value) => value.Value;
  public static implicit operator ulong(PackedBCD8 value) => (ulong)value.Value;

  // Widening to larger BCD types (implicit)
  public static implicit operator PackedBCD16(PackedBCD8 value) => PackedBCD16.FromValue(value.Value);
  public static implicit operator PackedBCD32(PackedBCD8 value) => PackedBCD32.FromValue(value.Value);
  public static implicit operator PackedBCD64(PackedBCD8 value) => PackedBCD64.FromValue(value.Value);

  // Implicit widening to extended integer types (value range 0-99 fits in all)
  public static implicit operator Int96(PackedBCD8 value) => new(0, (ulong)value.Value);
  public static implicit operator UInt96(PackedBCD8 value) => new(0, (ulong)value.Value);
  public static implicit operator Int128(PackedBCD8 value) => new(0, (ulong)value.Value);
  public static implicit operator UInt128(PackedBCD8 value) => new(0, (ulong)value.Value);

  // Parsing
  public static PackedBCD8 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static PackedBCD8 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static PackedBCD8 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = int.Parse(s, style, provider);
    return FromValue(value);
  }

  public static bool TryParse(string? s, out PackedBCD8 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out PackedBCD8 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out PackedBCD8 result) {
    if (int.TryParse(s, style, provider, out var value) && value is >= 0 and <= 99) {
      result = FromValue(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static PackedBCD8 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
    var value = int.Parse(s, NumberStyles.Integer, provider);
    return FromValue(value);
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out PackedBCD8 result) {
    if (int.TryParse(s, NumberStyles.Integer, provider, out var value) && value is >= 0 and <= 99) {
      result = FromValue(value);
      return true;
    }
    result = Zero;
    return false;
  }

  // Helper methods
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static PackedBCD8 Min(PackedBCD8 left, PackedBCD8 right) => left.Value <= right.Value ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static PackedBCD8 Max(PackedBCD8 left, PackedBCD8 right) => left.Value >= right.Value ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static PackedBCD8 Clamp(PackedBCD8 value, PackedBCD8 min, PackedBCD8 max) =>
    value.Value < min.Value ? min : value.Value > max.Value ? max : value;

}
