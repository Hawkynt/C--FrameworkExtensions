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
/// Represents a 64-bit packed BCD value storing 16 decimal digits (0-9999999999999999).
/// Each nibble contains one decimal digit (0-9).
/// </summary>
public readonly struct PackedBCD64 : IComparable, IComparable<PackedBCD64>, IEquatable<PackedBCD64>, IFormattable, IParsable<PackedBCD64> {

  private const long MaxDecimalValue = 9999999999999999L;

  /// <summary>
  /// Gets the raw BCD representation.
  /// </summary>
  public ulong RawValue { get; }

  private PackedBCD64(ulong raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a PackedBCD64 from the raw BCD representation.
  /// </summary>
  /// <exception cref="ArgumentException">Thrown if any nibble contains an invalid BCD digit (>9).</exception>
  public static PackedBCD64 FromRaw(ulong raw) {
    _ValidateRaw(raw);
    return new(raw);
  }

  /// <summary>
  /// Creates a PackedBCD64 from a decimal value (0-9999999999999999).
  /// </summary>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if value is not in range 0-9999999999999999.</exception>
  public static PackedBCD64 FromValue(long value) {
    if (value is < 0 or > MaxDecimalValue)
      throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0 and 9999999999999999.");
    return new(_Encode(value));
  }

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static PackedBCD64 Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static PackedBCD64 One => new(1);

  /// <summary>
  /// Gets the maximum value (9999999999999999).
  /// </summary>
  public static PackedBCD64 MaxValue => new(0x9999999999999999UL);

  /// <summary>
  /// Gets the minimum value (0).
  /// </summary>
  public static PackedBCD64 MinValue => new(0);

  /// <summary>
  /// Gets the decimal value represented by this BCD.
  /// </summary>
  public long Value => _Decode(this.RawValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ValidateRaw(ulong raw) {
    for (var i = 0; i < 16; ++i) {
      if (((raw >> (i * 4)) & 0x0FUL) > 9)
        throw new ArgumentException("Invalid BCD: nibble values must be 0-9.", nameof(raw));
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _Encode(long value) {
    var result = 0UL;
    for (var i = 0; i < 16; ++i) {
      result |= (ulong)(value % 10) << (i * 4);
      value /= 10;
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static long _Decode(ulong raw) {
    var result = 0L;
    var multiplier = 1L;
    for (var i = 0; i < 16; ++i) {
      result += (long)((raw >> (i * 4)) & 0x0FUL) * multiplier;
      multiplier *= 10;
    }
    return result;
  }

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(PackedBCD64 other) => this.Value.CompareTo(other.Value);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not PackedBCD64 other)
      throw new ArgumentException("Object must be of type PackedBCD64.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(PackedBCD64 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is PackedBCD64 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.Value.ToString("D16");

  public string ToString(IFormatProvider? provider) => this.Value.ToString(provider);

  public string ToString(string? format) => this.Value.ToString(format);

  public string ToString(string? format, IFormatProvider? provider) => this.Value.ToString(format, provider);

  // Comparison operators
  public static bool operator ==(PackedBCD64 left, PackedBCD64 right) => left.Equals(right);
  public static bool operator !=(PackedBCD64 left, PackedBCD64 right) => !left.Equals(right);
  public static bool operator <(PackedBCD64 left, PackedBCD64 right) => left.Value < right.Value;
  public static bool operator >(PackedBCD64 left, PackedBCD64 right) => left.Value > right.Value;
  public static bool operator <=(PackedBCD64 left, PackedBCD64 right) => left.Value <= right.Value;
  public static bool operator >=(PackedBCD64 left, PackedBCD64 right) => left.Value >= right.Value;

  // Arithmetic operators
  public static PackedBCD64 operator +(PackedBCD64 left, PackedBCD64 right) {
    var result = left.Value + right.Value;
    if (result > MaxDecimalValue)
      throw new OverflowException("BCD addition overflow.");
    return new(_Encode(result));
  }

  public static PackedBCD64 operator -(PackedBCD64 left, PackedBCD64 right) {
    var result = left.Value - right.Value;
    if (result < 0)
      throw new OverflowException("BCD subtraction underflow.");
    return new(_Encode(result));
  }

  public static PackedBCD64 operator *(PackedBCD64 left, PackedBCD64 right) {
    var leftVal = left.Value;
    var rightVal = right.Value;

    // Check for overflow before multiplication
    if (leftVal != 0 && rightVal > MaxDecimalValue / leftVal)
      throw new OverflowException("BCD multiplication overflow.");

    var result = leftVal * rightVal;
    if (result > MaxDecimalValue)
      throw new OverflowException("BCD multiplication overflow.");
    return new(_Encode(result));
  }

  public static PackedBCD64 operator /(PackedBCD64 left, PackedBCD64 right) {
    if (right.Value == 0)
      throw new DivideByZeroException();
    return new(_Encode(left.Value / right.Value));
  }

  public static PackedBCD64 operator %(PackedBCD64 left, PackedBCD64 right) {
    if (right.Value == 0)
      throw new DivideByZeroException();
    return new(_Encode(left.Value % right.Value));
  }

  public static PackedBCD64 operator ++(PackedBCD64 value) {
    if (value.Value >= MaxDecimalValue)
      throw new OverflowException("BCD increment overflow.");
    return new(_Encode(value.Value + 1));
  }

  public static PackedBCD64 operator --(PackedBCD64 value) {
    if (value.Value <= 0)
      throw new OverflowException("BCD decrement underflow.");
    return new(_Encode(value.Value - 1));
  }

  // Conversions
  public static implicit operator PackedBCD64(long value) => FromValue(value);
  public static explicit operator long(PackedBCD64 value) => value.Value;

  // Narrowing to smaller BCD (explicit - may overflow)
  public static explicit operator PackedBCD8(PackedBCD64 value) => PackedBCD8.FromValue((int)value.Value);
  public static explicit operator PackedBCD16(PackedBCD64 value) => PackedBCD16.FromValue((int)value.Value);
  public static explicit operator PackedBCD32(PackedBCD64 value) => PackedBCD32.FromValue((int)value.Value);

  // Widening from smaller BCD (implicit)
  public static implicit operator PackedBCD64(PackedBCD8 value) => FromValue(value.Value);
  public static implicit operator PackedBCD64(PackedBCD16 value) => FromValue(value.Value);
  public static implicit operator PackedBCD64(PackedBCD32 value) => FromValue(value.Value);

  // Parsing
  public static PackedBCD64 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static PackedBCD64 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static PackedBCD64 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = long.Parse(s, style, provider);
    return FromValue(value);
  }

  public static bool TryParse(string? s, out PackedBCD64 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out PackedBCD64 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out PackedBCD64 result) {
    if (long.TryParse(s, style, provider, out var value) && value >= 0 && value <= MaxDecimalValue) {
      result = FromValue(value);
      return true;
    }
    result = Zero;
    return false;
  }

  // Helper methods
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static PackedBCD64 Min(PackedBCD64 left, PackedBCD64 right) => left.Value <= right.Value ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static PackedBCD64 Max(PackedBCD64 left, PackedBCD64 right) => left.Value >= right.Value ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static PackedBCD64 Clamp(PackedBCD64 value, PackedBCD64 min, PackedBCD64 max) =>
    value.Value < min.Value ? min : value.Value > max.Value ? max : value;

}
