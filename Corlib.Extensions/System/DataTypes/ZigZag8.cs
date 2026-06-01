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
/// Represents a zigzag-encoded 8-bit value (stores encoded byte, represents sbyte range).
/// Zigzag encoding maps signed integers to unsigned integers so that numbers with small
/// absolute values have small encoded values, making them efficient for variable-length encoding.
/// </summary>
public readonly struct ZigZag8 : IComparable, IComparable<ZigZag8>, IEquatable<ZigZag8>, IFormattable, ISpanFormattable, IParsable<ZigZag8>, ISpanParsable<ZigZag8> {
  /// <summary>
  /// Gets the raw zigzag-encoded value.
  /// </summary>
  public byte EncodedValue { get; }

  /// <summary>
  /// Gets the decoded signed value.
  /// </summary>
  public sbyte DecodedValue => _Decode(this.EncodedValue);

  private ZigZag8(byte encoded) => this.EncodedValue = encoded;

  /// <summary>
  /// Creates a ZigZag8 from an encoded byte value.
  /// </summary>
  public static ZigZag8 FromEncoded(byte encoded) => new(encoded);

  /// <summary>
  /// Creates a ZigZag8 from a signed value by encoding it.
  /// </summary>
  public static ZigZag8 FromDecoded(sbyte value) => new(_Encode(value));

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static ZigZag8 Zero => new(0);

  /// <summary>
  /// Gets the maximum decodable value (sbyte.MaxValue = 127, encoded as 254).
  /// </summary>
  public static ZigZag8 MaxValue => new(254);

  /// <summary>
  /// Gets the minimum decodable value (sbyte.MinValue = -128, encoded as 255).
  /// </summary>
  public static ZigZag8 MinValue => new(255);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _Encode(sbyte value) => (byte)((value << 1) ^ (value >> 7));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static sbyte _Decode(byte encoded) => (sbyte)((encoded >> 1) ^ -(encoded & 1));

  // Comparison (based on decoded values for semantic ordering)
  public int CompareTo(ZigZag8 other) => this.DecodedValue.CompareTo(other.DecodedValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not ZigZag8 other)
      throw new ArgumentException("Object must be of type ZigZag8.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(ZigZag8 other) => this.EncodedValue == other.EncodedValue;

  public override bool Equals(object? obj) => obj is ZigZag8 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.EncodedValue.GetHashCode();

  public override string ToString() => this.DecodedValue.ToString();

  public string ToString(IFormatProvider? provider) => this.DecodedValue.ToString(provider);

  public string ToString(string? format) => this.DecodedValue.ToString(format);

  public string ToString(string? format, IFormatProvider? provider) => this.DecodedValue.ToString(format, provider);

  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
    var str = format.IsEmpty
      ? this.DecodedValue.ToString(provider)
      : this.DecodedValue.ToString(format.ToString(), provider);
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }
    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  // Operators
  public static bool operator ==(ZigZag8 left, ZigZag8 right) => left.Equals(right);
  public static bool operator !=(ZigZag8 left, ZigZag8 right) => !left.Equals(right);

  public static bool operator <(ZigZag8 left, ZigZag8 right) => left.CompareTo(right) < 0;
  public static bool operator >(ZigZag8 left, ZigZag8 right) => left.CompareTo(right) > 0;
  public static bool operator <=(ZigZag8 left, ZigZag8 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(ZigZag8 left, ZigZag8 right) => left.CompareTo(right) >= 0;

  // Implicit conversion from signed type (encode)
  public static implicit operator ZigZag8(sbyte value) => FromDecoded(value);

  // Explicit conversion to signed type (decode)
  public static explicit operator sbyte(ZigZag8 value) => value.DecodedValue;

  // Explicit conversion to/from encoded byte
  public static explicit operator byte(ZigZag8 value) => value.EncodedValue;
  public static explicit operator ZigZag8(byte encoded) => FromEncoded(encoded);

  // Implicit widening to larger signed types
  public static implicit operator short(ZigZag8 value) => value.DecodedValue;
  public static implicit operator int(ZigZag8 value) => value.DecodedValue;
  public static implicit operator long(ZigZag8 value) => value.DecodedValue;

  // Implicit widening to larger ZigZag types
  public static implicit operator ZigZag16(ZigZag8 value) => ZigZag16.FromDecoded(value.DecodedValue);
  public static implicit operator ZigZag32(ZigZag8 value) => ZigZag32.FromDecoded(value.DecodedValue);
  public static implicit operator ZigZag64(ZigZag8 value) => ZigZag64.FromDecoded(value.DecodedValue);

  // Implicit widening to extended signed types
  public static implicit operator Int96(ZigZag8 value) => new(value.DecodedValue < 0 ? uint.MaxValue : 0u, (ulong)(long)value.DecodedValue);
  public static implicit operator Int128(ZigZag8 value) => new(value.DecodedValue < 0 ? ulong.MaxValue : 0ul, (ulong)(long)value.DecodedValue);

  // Parsing (parses as decoded signed value)
  public static ZigZag8 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static ZigZag8 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static ZigZag8 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = sbyte.Parse(s, style, provider);
    return value;
  }

  public static bool TryParse(string? s, out ZigZag8 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out ZigZag8 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out ZigZag8 result) {
    if (sbyte.TryParse(s, style, provider, out var value)) {
      result = value;
      return true;
    }
    result = Zero;
    return false;
  }

  public static ZigZag8 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
    var value = sbyte.Parse(s, NumberStyles.Integer, provider);
    return value;
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ZigZag8 result) {
    if (sbyte.TryParse(s, NumberStyles.Integer, provider, out var value)) {
      result = value;
      return true;
    }
    result = Zero;
    return false;
  }

}
