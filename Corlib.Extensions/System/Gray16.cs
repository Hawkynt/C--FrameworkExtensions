#nullable enable

namespace System;

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

/// <summary>
/// Represents a 16-bit Gray code value.
/// </summary>
/// <remarks>
/// Gray code (also known as reflected binary code) is a binary numeral system where
/// two successive values differ in only one bit. This property makes Gray codes useful
/// for rotary encoders, error correction, and other applications where minimizing
/// bit transitions is important.
/// </remarks>
public readonly struct Gray16 : IComparable, IComparable<Gray16>, IEquatable<Gray16>, IFormattable, IParsable<Gray16> {
  /// <summary>The Gray code value zero.</summary>
  public static readonly Gray16 Zero = new(0);

  /// <summary>The maximum Gray code value.</summary>
  public static readonly Gray16 MaxValue = new(ushort.MaxValue);

  /// <summary>The minimum Gray code value (0).</summary>
  public static readonly Gray16 MinValue = new(0);

  private Gray16(ushort grayValue) => this.GrayValue = grayValue;

  /// <summary>Gets the raw Gray code value.</summary>
  public ushort GrayValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get;
  }

  /// <summary>Gets the decoded binary value.</summary>
  public ushort BinaryValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _GrayToBinary(this.GrayValue);
  }

  /// <summary>Creates a Gray16 from a binary value by encoding it.</summary>
  /// <param name="binary">The binary value to encode.</param>
  /// <returns>A new Gray16 with the encoded value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray16 FromBinary(ushort binary) => new(_BinaryToGray(binary));

  /// <summary>Creates a Gray16 from a raw Gray code value.</summary>
  /// <param name="gray">The raw Gray code value.</param>
  /// <returns>A new Gray16 with the given Gray value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray16 FromGray(ushort gray) => new(gray);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ushort _BinaryToGray(ushort binary) => (ushort)(binary ^ (binary >> 1));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ushort _GrayToBinary(ushort gray) {
    gray ^= (ushort)(gray >> 8);
    gray ^= (ushort)(gray >> 4);
    gray ^= (ushort)(gray >> 2);
    gray ^= (ushort)(gray >> 1);
    return gray;
  }

  #region Equality and comparison

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Gray16 other) => this.GrayValue == other.GrayValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj) => obj is Gray16 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.GrayValue.GetHashCode();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(Gray16 other) => this.BinaryValue.CompareTo(other.BinaryValue);

  public int CompareTo(object? obj) => obj switch {
    null => 1,
    Gray16 other => this.CompareTo(other),
    _ => throw new ArgumentException("Object must be of type Gray16.")
  };

  #endregion

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Gray16 left, Gray16 right) => left.GrayValue == right.GrayValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Gray16 left, Gray16 right) => left.GrayValue != right.GrayValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <(Gray16 left, Gray16 right) => left.BinaryValue < right.BinaryValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >(Gray16 left, Gray16 right) => left.BinaryValue > right.BinaryValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <=(Gray16 left, Gray16 right) => left.BinaryValue <= right.BinaryValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >=(Gray16 left, Gray16 right) => left.BinaryValue >= right.BinaryValue;

  /// <summary>Increments by one in binary space.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray16 operator ++(Gray16 value) => FromBinary((ushort)(value.BinaryValue + 1));

  /// <summary>Decrements by one in binary space.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray16 operator --(Gray16 value) => FromBinary((ushort)(value.BinaryValue - 1));

  #endregion

  #region Conversions

  /// <summary>Implicit conversion from ushort (binary) to Gray16.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Gray16(ushort binary) => FromBinary(binary);

  /// <summary>Explicit conversion from Gray16 to ushort (binary).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator ushort(Gray16 gray) => gray.BinaryValue;

  /// <summary>Explicit narrowing conversion from Gray16 to Gray8.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator Gray8(Gray16 value) => Gray8.FromBinary((byte)value.BinaryValue);

  /// <summary>Implicit widening conversion to Gray32.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Gray32(Gray16 value) => Gray32.FromBinary(value.BinaryValue);

  /// <summary>Implicit widening conversion to Gray64.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Gray64(Gray16 value) => Gray64.FromBinary(value.BinaryValue);

  #endregion

  #region Formatting

  public override string ToString() => this.BinaryValue.ToString();

  public string ToString(string? format) => this.BinaryValue.ToString(format);

  public string ToString(string? format, IFormatProvider? formatProvider) => this.BinaryValue.ToString(format, formatProvider);

  #endregion

  #region Parsing

  public static Gray16 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static Gray16 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static Gray16 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = ushort.Parse(s, style, provider);
    return FromBinary(value);
  }

  public static bool TryParse(string? s, out Gray16 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Gray16 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Gray16 result) {
    if (ushort.TryParse(s, style, provider, out var value)) {
      result = FromBinary(value);
      return true;
    }
    result = Zero;
    return false;
  }

  #endregion
}
