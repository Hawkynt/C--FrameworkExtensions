#nullable enable

namespace System;

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

/// <summary>
/// Represents an 8-bit Gray code value.
/// </summary>
/// <remarks>
/// Gray code (also known as reflected binary code) is a binary numeral system where
/// two successive values differ in only one bit. This property makes Gray codes useful
/// for rotary encoders, error correction, and other applications where minimizing
/// bit transitions is important.
/// </remarks>
public readonly struct Gray8 : IComparable, IComparable<Gray8>, IEquatable<Gray8>, IFormattable, ISpanFormattable, IParsable<Gray8>, ISpanParsable<Gray8> {
  /// <summary>The Gray code value zero.</summary>
  public static readonly Gray8 Zero = new(0);

  /// <summary>The maximum Gray code value (255 in binary, which encodes to 128 in Gray).</summary>
  public static readonly Gray8 MaxValue = new(byte.MaxValue);

  /// <summary>The minimum Gray code value (0).</summary>
  public static readonly Gray8 MinValue = new(0);

  private Gray8(byte grayValue) => this.GrayValue = grayValue;

  /// <summary>Gets the raw Gray code value.</summary>
  public byte GrayValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get;
  }

  /// <summary>Gets the decoded binary value.</summary>
  public byte BinaryValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _GrayToBinary(this.GrayValue);
  }

  /// <summary>Creates a Gray8 from a binary value by encoding it.</summary>
  /// <param name="binary">The binary value to encode.</param>
  /// <returns>A new Gray8 with the encoded value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray8 FromBinary(byte binary) => new(_BinaryToGray(binary));

  /// <summary>Creates a Gray8 from a raw Gray code value.</summary>
  /// <param name="gray">The raw Gray code value.</param>
  /// <returns>A new Gray8 with the given Gray value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray8 FromGray(byte gray) => new(gray);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _BinaryToGray(byte binary) => (byte)(binary ^ (binary >> 1));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _GrayToBinary(byte gray) {
    gray ^= (byte)(gray >> 4);
    gray ^= (byte)(gray >> 2);
    gray ^= (byte)(gray >> 1);
    return gray;
  }

  #region Equality and comparison

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Gray8 other) => this.GrayValue == other.GrayValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj) => obj is Gray8 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.GrayValue.GetHashCode();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(Gray8 other) => this.BinaryValue.CompareTo(other.BinaryValue);

  public int CompareTo(object? obj) => obj switch {
    null => 1,
    Gray8 other => this.CompareTo(other),
    _ => throw new ArgumentException("Object must be of type Gray8.")
  };

  #endregion

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Gray8 left, Gray8 right) => left.GrayValue == right.GrayValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Gray8 left, Gray8 right) => left.GrayValue != right.GrayValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <(Gray8 left, Gray8 right) => left.BinaryValue < right.BinaryValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >(Gray8 left, Gray8 right) => left.BinaryValue > right.BinaryValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <=(Gray8 left, Gray8 right) => left.BinaryValue <= right.BinaryValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >=(Gray8 left, Gray8 right) => left.BinaryValue >= right.BinaryValue;

  /// <summary>Increments by one in binary space.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray8 operator ++(Gray8 value) => FromBinary((byte)(value.BinaryValue + 1));

  /// <summary>Decrements by one in binary space.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray8 operator --(Gray8 value) => FromBinary((byte)(value.BinaryValue - 1));

  #endregion

  #region Conversions

  /// <summary>Implicit conversion from byte (binary) to Gray8.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Gray8(byte binary) => FromBinary(binary);

  /// <summary>Explicit conversion from Gray8 to byte (binary).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator byte(Gray8 gray) => gray.BinaryValue;

  /// <summary>Implicit widening conversion to Gray16.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Gray16(Gray8 value) => Gray16.FromBinary(value.BinaryValue);

  /// <summary>Implicit widening conversion to Gray32.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Gray32(Gray8 value) => Gray32.FromBinary(value.BinaryValue);

  /// <summary>Implicit widening conversion to Gray64.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Gray64(Gray8 value) => Gray64.FromBinary(value.BinaryValue);

  /// <summary>Implicit widening conversion to short.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator short(Gray8 value) => value.BinaryValue;

  /// <summary>Implicit widening conversion to ushort.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator ushort(Gray8 value) => value.BinaryValue;

  /// <summary>Implicit widening conversion to int.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator int(Gray8 value) => value.BinaryValue;

  /// <summary>Implicit widening conversion to uint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator uint(Gray8 value) => value.BinaryValue;

  /// <summary>Implicit widening conversion to long.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator long(Gray8 value) => value.BinaryValue;

  /// <summary>Implicit widening conversion to ulong.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator ulong(Gray8 value) => value.BinaryValue;

  /// <summary>Implicit widening conversion to Int96.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Int96(Gray8 value) => new(0, value.BinaryValue);

  /// <summary>Implicit widening conversion to UInt96.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator UInt96(Gray8 value) => new(0, value.BinaryValue);

  /// <summary>Implicit widening conversion to Int128.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Int128(Gray8 value) => new(0, value.BinaryValue);

  /// <summary>Implicit widening conversion to UInt128.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator UInt128(Gray8 value) => new(0, value.BinaryValue);

  #endregion

  #region Formatting

  public override string ToString() => this.BinaryValue.ToString();

  public string ToString(string? format) => this.BinaryValue.ToString(format);

  public string ToString(string? format, IFormatProvider? formatProvider) => this.BinaryValue.ToString(format, formatProvider);

  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
    var str = format.IsEmpty
      ? this.BinaryValue.ToString(provider)
      : this.BinaryValue.ToString(format.ToString(), provider);
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }
    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  #endregion

  #region Parsing

  public static Gray8 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static Gray8 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static Gray8 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = byte.Parse(s, style, provider);
    return FromBinary(value);
  }

  public static bool TryParse(string? s, out Gray8 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Gray8 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Gray8 result) {
    if (byte.TryParse(s, style, provider, out var value)) {
      result = FromBinary(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static Gray8 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
    var value = byte.Parse(s, NumberStyles.Integer, provider);
    return FromBinary(value);
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Gray8 result) {
    if (byte.TryParse(s, NumberStyles.Integer, provider, out var value)) {
      result = FromBinary(value);
      return true;
    }
    result = Zero;
    return false;
  }

  #endregion
}
