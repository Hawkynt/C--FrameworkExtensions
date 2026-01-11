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
/// Represents a zigzag-encoded 32-bit value (stores encoded uint, represents int range).
/// Zigzag encoding maps signed integers to unsigned integers so that numbers with small
/// absolute values have small encoded values, making them efficient for variable-length encoding.
/// </summary>
public readonly struct ZigZag32 : IComparable, IComparable<ZigZag32>, IEquatable<ZigZag32>, IFormattable, IParsable<ZigZag32> {
  /// <summary>
  /// Gets the raw zigzag-encoded value.
  /// </summary>
  public uint EncodedValue { get; }

  /// <summary>
  /// Gets the decoded signed value.
  /// </summary>
  public int DecodedValue => _Decode(this.EncodedValue);

  private ZigZag32(uint encoded) => this.EncodedValue = encoded;

  /// <summary>
  /// Creates a ZigZag32 from an encoded uint value.
  /// </summary>
  public static ZigZag32 FromEncoded(uint encoded) => new(encoded);

  /// <summary>
  /// Creates a ZigZag32 from a signed value by encoding it.
  /// </summary>
  public static ZigZag32 FromDecoded(int value) => new(_Encode(value));

  /// <summary>
  /// Gets the value 0.
  /// </summary>
  public static ZigZag32 Zero => new(0);

  /// <summary>
  /// Gets the maximum decodable value (int.MaxValue, encoded as uint.MaxValue - 1).
  /// </summary>
  public static ZigZag32 MaxValue => new(uint.MaxValue - 1);

  /// <summary>
  /// Gets the minimum decodable value (int.MinValue, encoded as uint.MaxValue).
  /// </summary>
  public static ZigZag32 MinValue => new(uint.MaxValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _Encode(int value) => (uint)((value << 1) ^ (value >> 31));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _Decode(uint encoded) => (int)((encoded >> 1) ^ -(int)(encoded & 1));

  // Comparison (based on decoded values for semantic ordering)
  public int CompareTo(ZigZag32 other) => this.DecodedValue.CompareTo(other.DecodedValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not ZigZag32 other)
      throw new ArgumentException("Object must be of type ZigZag32.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(ZigZag32 other) => this.EncodedValue == other.EncodedValue;

  public override bool Equals(object? obj) => obj is ZigZag32 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.EncodedValue.GetHashCode();

  public override string ToString() => this.DecodedValue.ToString();

  public string ToString(IFormatProvider? provider) => this.DecodedValue.ToString(provider);

  public string ToString(string? format) => this.DecodedValue.ToString(format);

  public string ToString(string? format, IFormatProvider? provider) => this.DecodedValue.ToString(format, provider);

  // Operators
  public static bool operator ==(ZigZag32 left, ZigZag32 right) => left.Equals(right);
  public static bool operator !=(ZigZag32 left, ZigZag32 right) => !left.Equals(right);

  public static bool operator <(ZigZag32 left, ZigZag32 right) => left.CompareTo(right) < 0;
  public static bool operator >(ZigZag32 left, ZigZag32 right) => left.CompareTo(right) > 0;
  public static bool operator <=(ZigZag32 left, ZigZag32 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(ZigZag32 left, ZigZag32 right) => left.CompareTo(right) >= 0;

  // Implicit conversion from signed type (encode)
  public static implicit operator ZigZag32(int value) => FromDecoded(value);

  // Explicit conversion to signed type (decode)
  public static explicit operator int(ZigZag32 value) => value.DecodedValue;

  // Explicit conversion to/from encoded uint
  public static explicit operator uint(ZigZag32 value) => value.EncodedValue;
  public static explicit operator ZigZag32(uint encoded) => FromEncoded(encoded);

  // Widening conversions from smaller ZigZag
  public static implicit operator ZigZag32(ZigZag8 value) => FromDecoded(value.DecodedValue);
  public static implicit operator ZigZag32(ZigZag16 value) => FromDecoded(value.DecodedValue);

  // Parsing (parses as decoded signed value)
  public static ZigZag32 Parse(string s) => Parse(s, NumberStyles.Integer, null);

  public static ZigZag32 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);

  public static ZigZag32 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = int.Parse(s, style, provider);
    return value;
  }

  public static bool TryParse(string? s, out ZigZag32 result) => TryParse(s, NumberStyles.Integer, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out ZigZag32 result) => TryParse(s, NumberStyles.Integer, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out ZigZag32 result) {
    if (int.TryParse(s, style, provider, out var value)) {
      result = value;
      return true;
    }
    result = Zero;
    return false;
  }

}
