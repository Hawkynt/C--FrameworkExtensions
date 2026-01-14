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
/// Represents a zigzag-encoded 16-bit value (stores encoded ushort, represents short range).
/// Zigzag encoding maps signed integers to unsigned integers so that numbers with small
/// absolute values have small encoded values, making them efficient for variable-length encoding.
/// </summary>
public readonly struct ZigZag16 : IComparable, IComparable<ZigZag16>, IEquatable<ZigZag16>, IFormattable, IParsable<ZigZag16> {
  /// <summary>
  /// Gets the raw zigzag-encoded value.
  /// </summary>
  public ushort EncodedValue { get; }

  /// <summary>
  /// Gets the decoded signed value.
  /// </summary>
  public short DecodedValue => _Decode(this.EncodedValue);

  private ZigZag16(ushort encoded) => this.EncodedValue = encoded;

  /// <summary>
  /// Creates a ZigZag16 from an encoded ushort value.
  /// </summary>
  public static ZigZag16 FromEncoded(ushort encoded) => new(encoded);

  /// <summary>
  /// Creates a ZigZag16 from a signed value by encoding it.
  /// </summary>
  public static ZigZag16 FromDecoded(short value) => new(_Encode(value));

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static ZigZag16 Zero => new(0);

  /// <summary>
  /// Gets the maximum decodable value (short.MaxValue = 32767, encoded as 65534).
  /// </summary>
  public static ZigZag16 MaxValue => new(65534);

  /// <summary>
  /// Gets the minimum decodable value (short.MinValue = -32768, encoded as 65535).
  /// </summary>
  public static ZigZag16 MinValue => new(65535);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ushort _Encode(short value) => (ushort)((value << 1) ^ (value >> 15));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static short _Decode(ushort encoded) => (short)((encoded >> 1) ^ -(encoded & 1));

  // Comparison (based on decoded values for semantic ordering)
  public int CompareTo(ZigZag16 other) => this.DecodedValue.CompareTo(other.DecodedValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not ZigZag16 other)
      throw new ArgumentException("Object must be of type ZigZag16.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(ZigZag16 other) => this.EncodedValue == other.EncodedValue;

  public override bool Equals(object? obj) => obj is ZigZag16 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.EncodedValue.GetHashCode();

  public override string ToString() => this.DecodedValue.ToString();

  public string ToString(IFormatProvider? provider) => this.DecodedValue.ToString(provider);

  public string ToString(string? format) => this.DecodedValue.ToString(format);

  public string ToString(string? format, IFormatProvider? provider) => this.DecodedValue.ToString(format, provider);

  // Operators
  public static bool operator ==(ZigZag16 left, ZigZag16 right) => left.Equals(right);
  public static bool operator !=(ZigZag16 left, ZigZag16 right) => !left.Equals(right);

  public static bool operator <(ZigZag16 left, ZigZag16 right) => left.CompareTo(right) < 0;
  public static bool operator >(ZigZag16 left, ZigZag16 right) => left.CompareTo(right) > 0;
  public static bool operator <=(ZigZag16 left, ZigZag16 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(ZigZag16 left, ZigZag16 right) => left.CompareTo(right) >= 0;

  // Implicit conversion from signed type (encode)
  public static implicit operator ZigZag16(short value) => FromDecoded(value);

  // Explicit conversion to signed type (decode)
  public static explicit operator short(ZigZag16 value) => value.DecodedValue;

  // Explicit conversion to/from encoded ushort
  public static explicit operator ushort(ZigZag16 value) => value.EncodedValue;
  public static explicit operator ZigZag16(ushort encoded) => FromEncoded(encoded);

  // Widening conversions from smaller ZigZag
  public static implicit operator ZigZag16(ZigZag8 value) => FromDecoded(value.DecodedValue);

  // Parsing (parses as decoded signed value)
  public static ZigZag16 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static ZigZag16 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static ZigZag16 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = short.Parse(s, style, provider);
    return value;
  }

  public static bool TryParse(string? s, out ZigZag16 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out ZigZag16 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out ZigZag16 result) {
    if (short.TryParse(s, style, provider, out var value)) {
      result = value;
      return true;
    }
    result = Zero;
    return false;
  }

}
