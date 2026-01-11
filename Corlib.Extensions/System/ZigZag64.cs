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
/// Represents a zigzag-encoded 64-bit value (stores encoded ulong, represents long range).
/// Zigzag encoding maps signed integers to unsigned integers so that numbers with small
/// absolute values have small encoded values, making them efficient for variable-length encoding.
/// </summary>
public readonly struct ZigZag64 : IComparable, IComparable<ZigZag64>, IEquatable<ZigZag64>, IFormattable, IParsable<ZigZag64> {
  /// <summary>
  /// Gets the raw zigzag-encoded value.
  /// </summary>
  public ulong EncodedValue { get; }

  /// <summary>
  /// Gets the decoded signed value.
  /// </summary>
  public long DecodedValue => _Decode(this.EncodedValue);

  private ZigZag64(ulong encoded) => this.EncodedValue = encoded;

  /// <summary>
  /// Creates a ZigZag64 from an encoded ulong value.
  /// </summary>
  public static ZigZag64 FromEncoded(ulong encoded) => new(encoded);

  /// <summary>
  /// Creates a ZigZag64 from a signed value by encoding it.
  /// </summary>
  public static ZigZag64 FromDecoded(long value) => new(_Encode(value));

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static ZigZag64 Zero => new(0);

  /// <summary>
  /// Gets the maximum decodable value (long.MaxValue, encoded as ulong.MaxValue - 1).
  /// </summary>
  public static ZigZag64 MaxValue => new(ulong.MaxValue - 1);

  /// <summary>
  /// Gets the minimum decodable value (long.MinValue, encoded as ulong.MaxValue).
  /// </summary>
  public static ZigZag64 MinValue => new(ulong.MaxValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _Encode(long value) => (ulong)((value << 1) ^ (value >> 63));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static long _Decode(ulong encoded) => (long)(encoded >> 1) ^ -(long)(encoded & 1);

  // Comparison (based on decoded values for semantic ordering)
  public int CompareTo(ZigZag64 other) => this.DecodedValue.CompareTo(other.DecodedValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not ZigZag64 other)
      throw new ArgumentException("Object must be of type ZigZag64.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(ZigZag64 other) => this.EncodedValue == other.EncodedValue;

  public override bool Equals(object? obj) => obj is ZigZag64 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.EncodedValue.GetHashCode();

  public override string ToString() => this.DecodedValue.ToString();

  public string ToString(IFormatProvider? provider) => this.DecodedValue.ToString(provider);

  public string ToString(string? format) => this.DecodedValue.ToString(format);

  public string ToString(string? format, IFormatProvider? provider) => this.DecodedValue.ToString(format, provider);

  // Operators
  public static bool operator ==(ZigZag64 left, ZigZag64 right) => left.Equals(right);
  public static bool operator !=(ZigZag64 left, ZigZag64 right) => !left.Equals(right);

  public static bool operator <(ZigZag64 left, ZigZag64 right) => left.CompareTo(right) < 0;
  public static bool operator >(ZigZag64 left, ZigZag64 right) => left.CompareTo(right) > 0;
  public static bool operator <=(ZigZag64 left, ZigZag64 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(ZigZag64 left, ZigZag64 right) => left.CompareTo(right) >= 0;

  // Implicit conversion from signed type (encode)
  public static implicit operator ZigZag64(long value) => FromDecoded(value);

  // Explicit conversion to signed type (decode)
  public static explicit operator long(ZigZag64 value) => value.DecodedValue;

  // Explicit conversion to/from encoded ulong
  public static explicit operator ulong(ZigZag64 value) => value.EncodedValue;
  public static explicit operator ZigZag64(ulong encoded) => FromEncoded(encoded);

  // Widening conversions from smaller ZigZag
  public static implicit operator ZigZag64(ZigZag8 value) => FromDecoded(value.DecodedValue);
  public static implicit operator ZigZag64(ZigZag16 value) => FromDecoded(value.DecodedValue);
  public static implicit operator ZigZag64(ZigZag32 value) => FromDecoded(value.DecodedValue);

  // Parsing (parses as decoded signed value)
  public static ZigZag64 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static ZigZag64 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static ZigZag64 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = long.Parse(s, style, provider);
    return value;
  }

  public static bool TryParse(string? s, out ZigZag64 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out ZigZag64 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out ZigZag64 result) {
    if (long.TryParse(s, style, provider, out var value)) {
      result = value;
      return true;
    }
    result = Zero;
    return false;
  }

}
