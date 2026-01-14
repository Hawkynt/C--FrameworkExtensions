#nullable enable

namespace System;

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

/// <summary>
/// Represents a 64-bit Gray code value.
/// </summary>
/// <remarks>
/// Gray code (also known as reflected binary code) is a binary numeral system where
/// two successive values differ in only one bit. This property makes Gray codes useful
/// for rotary encoders, error correction, and other applications where minimizing
/// bit transitions is important.
/// </remarks>
public readonly struct Gray64 : IComparable, IComparable<Gray64>, IEquatable<Gray64>, IFormattable, IParsable<Gray64> {
  /// <summary>The Gray code value zero.</summary>
  public static readonly Gray64 Zero = new(0);

  /// <summary>The maximum Gray code value.</summary>
  public static readonly Gray64 MaxValue = new(ulong.MaxValue);

  /// <summary>The minimum Gray code value (0).</summary>
  public static readonly Gray64 MinValue = new(0);

  private Gray64(ulong grayValue) => this.GrayValue = grayValue;

  /// <summary>Gets the raw Gray code value.</summary>
  public ulong GrayValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get;
  }

  /// <summary>Gets the decoded binary value.</summary>
  public ulong BinaryValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _GrayToBinary(this.GrayValue);
  }

  /// <summary>Creates a Gray64 from a binary value by encoding it.</summary>
  /// <param name="binary">The binary value to encode.</param>
  /// <returns>A new Gray64 with the encoded value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray64 FromBinary(ulong binary) => new(_BinaryToGray(binary));

  /// <summary>Creates a Gray64 from a raw Gray code value.</summary>
  /// <param name="gray">The raw Gray code value.</param>
  /// <returns>A new Gray64 with the given Gray value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray64 FromGray(ulong gray) => new(gray);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _BinaryToGray(ulong binary) => binary ^ (binary >> 1);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _GrayToBinary(ulong gray) {
    gray ^= gray >> 32;
    gray ^= gray >> 16;
    gray ^= gray >> 8;
    gray ^= gray >> 4;
    gray ^= gray >> 2;
    gray ^= gray >> 1;
    return gray;
  }

  #region Equality and comparison

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Gray64 other) => this.GrayValue == other.GrayValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj) => obj is Gray64 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.GrayValue.GetHashCode();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(Gray64 other) => this.BinaryValue.CompareTo(other.BinaryValue);

  public int CompareTo(object? obj) => obj switch {
    null => 1,
    Gray64 other => this.CompareTo(other),
    _ => throw new ArgumentException("Object must be of type Gray64.")
  };

  #endregion

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Gray64 left, Gray64 right) => left.GrayValue == right.GrayValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Gray64 left, Gray64 right) => left.GrayValue != right.GrayValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <(Gray64 left, Gray64 right) => left.BinaryValue < right.BinaryValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >(Gray64 left, Gray64 right) => left.BinaryValue > right.BinaryValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <=(Gray64 left, Gray64 right) => left.BinaryValue <= right.BinaryValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >=(Gray64 left, Gray64 right) => left.BinaryValue >= right.BinaryValue;

  /// <summary>Increments by one in binary space.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray64 operator ++(Gray64 value) => FromBinary(value.BinaryValue + 1);

  /// <summary>Decrements by one in binary space.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray64 operator --(Gray64 value) => FromBinary(value.BinaryValue - 1);

  #endregion

  #region Conversions

  /// <summary>Implicit conversion from ulong (binary) to Gray64.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Gray64(ulong binary) => FromBinary(binary);

  /// <summary>Explicit conversion from Gray64 to ulong (binary).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator ulong(Gray64 gray) => gray.BinaryValue;

  /// <summary>Explicit narrowing conversion from Gray64 to Gray8.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator Gray8(Gray64 value) => Gray8.FromBinary((byte)value.BinaryValue);

  /// <summary>Explicit narrowing conversion from Gray64 to Gray16.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator Gray16(Gray64 value) => Gray16.FromBinary((ushort)value.BinaryValue);

  /// <summary>Explicit narrowing conversion from Gray64 to Gray32.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator Gray32(Gray64 value) => Gray32.FromBinary((uint)value.BinaryValue);

  #endregion

  #region Formatting

  public override string ToString() => this.BinaryValue.ToString();

  public string ToString(string? format) => this.BinaryValue.ToString(format);

  public string ToString(string? format, IFormatProvider? formatProvider) => this.BinaryValue.ToString(format, formatProvider);

  #endregion

  #region Parsing

  public static Gray64 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static Gray64 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static Gray64 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = ulong.Parse(s, style, provider);
    return FromBinary(value);
  }

  public static bool TryParse(string? s, out Gray64 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Gray64 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Gray64 result) {
    if (ulong.TryParse(s, style, provider, out var value)) {
      result = FromBinary(value);
      return true;
    }
    result = Zero;
    return false;
  }

  #endregion
}
