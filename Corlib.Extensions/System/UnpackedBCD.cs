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
/// Represents an 8-bit unpacked BCD value storing 1 decimal digit (0-9).
/// The entire byte stores a single digit, with the upper nibble typically zero.
/// </summary>
public readonly struct UnpackedBCD : IComparable, IComparable<UnpackedBCD>, IEquatable<UnpackedBCD>, IFormattable, IParsable<UnpackedBCD> {
  /// <summary>
  /// Gets the raw value (0-9).
  /// </summary>
  public byte RawValue { get; }

  /// <summary>
  /// Gets the decimal digit value (0-9).
  /// </summary>
  public int Value => this.RawValue;

  private UnpackedBCD(byte raw) => this.RawValue = raw;

  /// <summary>
  /// Creates an UnpackedBCD from a digit value (0-9).
  /// </summary>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if value is not in range 0-9.</exception>
  public static UnpackedBCD FromValue(int value) {
    if (value is < 0 or > 9)
      throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0 and 9.");
    return new((byte)value);
  }

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static UnpackedBCD Zero => new(0);

  /// <summary>
  /// Gets the value 1.
  /// </summary>
  public static UnpackedBCD One => new(1);

  /// <summary>
  /// Gets the maximum value (9).
  /// </summary>
  public static UnpackedBCD MaxValue => new(9);

  /// <summary>
  /// Gets the minimum value (0).
  /// </summary>
  public static UnpackedBCD MinValue => new(0);

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(UnpackedBCD other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not UnpackedBCD other)
      throw new ArgumentException("Object must be of type UnpackedBCD.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(UnpackedBCD other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is UnpackedBCD other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.RawValue.ToString();

  public string ToString(IFormatProvider? provider) => this.RawValue.ToString(provider);

  public string ToString(string? format) => this.RawValue.ToString(format);

  public string ToString(string? format, IFormatProvider? provider) => this.RawValue.ToString(format, provider);

  // Comparison operators
  public static bool operator ==(UnpackedBCD left, UnpackedBCD right) => left.Equals(right);
  public static bool operator !=(UnpackedBCD left, UnpackedBCD right) => !left.Equals(right);
  public static bool operator <(UnpackedBCD left, UnpackedBCD right) => left.RawValue < right.RawValue;
  public static bool operator >(UnpackedBCD left, UnpackedBCD right) => left.RawValue > right.RawValue;
  public static bool operator <=(UnpackedBCD left, UnpackedBCD right) => left.RawValue <= right.RawValue;
  public static bool operator >=(UnpackedBCD left, UnpackedBCD right) => left.RawValue >= right.RawValue;

  // Arithmetic operators
  public static UnpackedBCD operator +(UnpackedBCD left, UnpackedBCD right) {
    var result = left.RawValue + right.RawValue;
    if (result > 9)
      throw new OverflowException("BCD addition overflow.");
    return new((byte)result);
  }

  public static UnpackedBCD operator -(UnpackedBCD left, UnpackedBCD right) {
    var result = left.RawValue - right.RawValue;
    if (result < 0)
      throw new OverflowException("BCD subtraction underflow.");
    return new((byte)result);
  }

  public static UnpackedBCD operator *(UnpackedBCD left, UnpackedBCD right) {
    var result = left.RawValue * right.RawValue;
    if (result > 9)
      throw new OverflowException("BCD multiplication overflow.");
    return new((byte)result);
  }

  public static UnpackedBCD operator /(UnpackedBCD left, UnpackedBCD right) {
    if (right.RawValue == 0)
      throw new DivideByZeroException();
    return new((byte)(left.RawValue / right.RawValue));
  }

  public static UnpackedBCD operator %(UnpackedBCD left, UnpackedBCD right) {
    if (right.RawValue == 0)
      throw new DivideByZeroException();
    return new((byte)(left.RawValue % right.RawValue));
  }

  public static UnpackedBCD operator ++(UnpackedBCD value) {
    if (value.RawValue >= 9)
      throw new OverflowException("BCD increment overflow.");
    return new((byte)(value.RawValue + 1));
  }

  public static UnpackedBCD operator --(UnpackedBCD value) {
    if (value.RawValue <= 0)
      throw new OverflowException("BCD decrement underflow.");
    return new((byte)(value.RawValue - 1));
  }

  // Conversions
  public static implicit operator UnpackedBCD(byte value) => FromValue(value);
  public static explicit operator byte(UnpackedBCD value) => value.RawValue;
  public static explicit operator int(UnpackedBCD value) => value.RawValue;

  // Widening to packed BCD types (implicit)
  public static implicit operator PackedBCD8(UnpackedBCD value) => PackedBCD8.FromValue(value.RawValue);
  public static implicit operator PackedBCD16(UnpackedBCD value) => PackedBCD16.FromValue(value.RawValue);
  public static implicit operator PackedBCD32(UnpackedBCD value) => PackedBCD32.FromValue(value.RawValue);

  // Parsing
  public static UnpackedBCD Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static UnpackedBCD Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static UnpackedBCD Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = int.Parse(s, style, provider);
    return FromValue(value);
  }

  public static bool TryParse(string? s, out UnpackedBCD result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out UnpackedBCD result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out UnpackedBCD result) {
    if (int.TryParse(s, style, provider, out var value) && value is >= 0 and <= 9) {
      result = FromValue(value);
      return true;
    }
    result = Zero;
    return false;
  }

  // Helper methods
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UnpackedBCD Min(UnpackedBCD left, UnpackedBCD right) => left.RawValue <= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UnpackedBCD Max(UnpackedBCD left, UnpackedBCD right) => left.RawValue >= right.RawValue ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UnpackedBCD Clamp(UnpackedBCD value, UnpackedBCD min, UnpackedBCD max) =>
    value.RawValue < min.RawValue ? min : value.RawValue > max.RawValue ? max : value;

}
