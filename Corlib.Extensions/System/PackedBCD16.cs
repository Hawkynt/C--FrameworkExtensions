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
/// Represents a 16-bit packed BCD value storing 4 decimal digits (0-9999).
/// Each nibble contains one decimal digit (0-9).
/// </summary>
public readonly struct PackedBCD16 : IComparable, IComparable<PackedBCD16>, IEquatable<PackedBCD16>, IFormattable, IParsable<PackedBCD16> {
  /// <summary>
  /// Gets the raw BCD representation.
  /// </summary>
  public ushort RawValue { get; }

  private PackedBCD16(ushort raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a PackedBCD16 from the raw BCD representation.
  /// </summary>
  /// <exception cref="ArgumentException">Thrown if any nibble contains an invalid BCD digit (>9).</exception>
  public static PackedBCD16 FromRaw(ushort raw) {
    _ValidateRaw(raw);
    return new(raw);
  }

  /// <summary>
  /// Creates a PackedBCD16 from a decimal value (0-9999).
  /// </summary>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if value is not in range 0-9999.</exception>
  public static PackedBCD16 FromValue(int value) {
    if (value is < 0 or > 9999)
      throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0 and 9999.");
    return new(_Encode(value));
  }

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static PackedBCD16 Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static PackedBCD16 One => new(1);

  /// <summary>
  /// Gets the maximum value (9999).
  /// </summary>
  public static PackedBCD16 MaxValue => new(0x9999);

  /// <summary>
  /// Gets the minimum value (0).
  /// </summary>
  public static PackedBCD16 MinValue => new(0);

  /// <summary>
  /// Gets the decimal value represented by this BCD.
  /// </summary>
  public int Value => _Decode(this.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ValidateRaw(ushort raw) {
    for (var i = 0; i < 4; ++i) {
      if (((raw >> (i * 4)) & 0x0F) > 9)
        throw new ArgumentException("Invalid BCD: nibble values must be 0-9.", nameof(raw));
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ushort _Encode(int value) {
    ushort result = 0;
    for (var i = 0; i < 4; ++i) {
      result |= (ushort)((value % 10) << (i * 4));
      value /= 10;
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _Decode(ushort raw) {
    var result = 0;
    var multiplier = 1;
    for (var i = 0; i < 4; ++i) {
      result += ((raw >> (i * 4)) & 0x0F) * multiplier;
      multiplier *= 10;
    }
    return result;
  }

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(PackedBCD16 other) => this.Value.CompareTo(other.Value);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not PackedBCD16 other)
      throw new ArgumentException("Object must be of type PackedBCD16.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(PackedBCD16 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is PackedBCD16 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.Value.ToString("D4");

  public string ToString(IFormatProvider? provider) => this.Value.ToString(provider);

  public string ToString(string? format) => this.Value.ToString(format);

  public string ToString(string? format, IFormatProvider? provider) => this.Value.ToString(format, provider);

  // Comparison operators
  public static bool operator ==(PackedBCD16 left, PackedBCD16 right) => left.Equals(right);
  public static bool operator !=(PackedBCD16 left, PackedBCD16 right) => !left.Equals(right);
  public static bool operator <(PackedBCD16 left, PackedBCD16 right) => left.Value < right.Value;
  public static bool operator >(PackedBCD16 left, PackedBCD16 right) => left.Value > right.Value;
  public static bool operator <=(PackedBCD16 left, PackedBCD16 right) => left.Value <= right.Value;
  public static bool operator >=(PackedBCD16 left, PackedBCD16 right) => left.Value >= right.Value;

  // Arithmetic operators
  public static PackedBCD16 operator +(PackedBCD16 left, PackedBCD16 right) {
    var result = left.Value + right.Value;
    if (result > 9999)
      throw new OverflowException("BCD addition overflow.");
    return new(_Encode(result));
  }

  public static PackedBCD16 operator -(PackedBCD16 left, PackedBCD16 right) {
    var result = left.Value - right.Value;
    if (result < 0)
      throw new OverflowException("BCD subtraction underflow.");
    return new(_Encode(result));
  }

  public static PackedBCD16 operator *(PackedBCD16 left, PackedBCD16 right) {
    var result = left.Value * right.Value;
    if (result > 9999)
      throw new OverflowException("BCD multiplication overflow.");
    return new(_Encode(result));
  }

  public static PackedBCD16 operator /(PackedBCD16 left, PackedBCD16 right) {
    if (right.Value == 0)
      throw new DivideByZeroException();
    return new(_Encode(left.Value / right.Value));
  }

  public static PackedBCD16 operator %(PackedBCD16 left, PackedBCD16 right) {
    if (right.Value == 0)
      throw new DivideByZeroException();
    return new(_Encode(left.Value % right.Value));
  }

  public static PackedBCD16 operator ++(PackedBCD16 value) {
    if (value.Value >= 9999)
      throw new OverflowException("BCD increment overflow.");
    return new(_Encode(value.Value + 1));
  }

  public static PackedBCD16 operator --(PackedBCD16 value) {
    if (value.Value <= 0)
      throw new OverflowException("BCD decrement underflow.");
    return new(_Encode(value.Value - 1));
  }

  // Conversions
  public static implicit operator PackedBCD16(ushort value) => FromValue(value);
  public static explicit operator ushort(PackedBCD16 value) => (ushort)value.Value;
  public static explicit operator int(PackedBCD16 value) => value.Value;

  // Narrowing to smaller BCD (explicit - may overflow)
  public static explicit operator PackedBCD8(PackedBCD16 value) => PackedBCD8.FromValue(value.Value);

  // Widening to larger BCD type (implicit)
  public static implicit operator PackedBCD32(PackedBCD16 value) => PackedBCD32.FromValue(value.Value);

  // Parsing
  public static PackedBCD16 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static PackedBCD16 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static PackedBCD16 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = int.Parse(s, style, provider);
    return FromValue(value);
  }

  public static bool TryParse(string? s, out PackedBCD16 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out PackedBCD16 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out PackedBCD16 result) {
    if (int.TryParse(s, style, provider, out var value) && value is >= 0 and <= 9999) {
      result = FromValue(value);
      return true;
    }
    result = Zero;
    return false;
  }

  // Helper methods
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static PackedBCD16 Min(PackedBCD16 left, PackedBCD16 right) => left.Value <= right.Value ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static PackedBCD16 Max(PackedBCD16 left, PackedBCD16 right) => left.Value >= right.Value ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static PackedBCD16 Clamp(PackedBCD16 value, PackedBCD16 min, PackedBCD16 max) =>
    value.Value < min.Value ? min : value.Value > max.Value ? max : value;

}
