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
/// Represents an 8-bit Posit (unum type III) number with nbits=8 and es=0.
/// Posits have tapered accuracy: values near 1.0 carry the most precision.
/// There is no infinity; the bit pattern 0x80 (sign bit only) represents NaR (Not-a-Real), mapped to <see cref="double.NaN"/>.
/// </summary>
public readonly struct Posit8 : IComparable, IComparable<Posit8>, IEquatable<Posit8>, IFormattable, ISpanFormattable, IParsable<Posit8>, ISpanParsable<Posit8> {

  private const int NBits = 8;
  private const int Es = 0;
  private const uint Mask = (1u << NBits) - 1; // 0xFF
  private const byte NaRBits = 1 << (NBits - 1); // 0x80

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public byte RawValue { get; }

  private Posit8(byte raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="Posit8"/> from the raw bit representation.
  /// </summary>
  public static Posit8 FromRaw(byte raw) => new(raw);

  /// <summary>
  /// Gets zero.
  /// </summary>
  public static Posit8 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0.
  /// </summary>
  public static Posit8 One => FromDouble(1.0);

  /// <summary>
  /// Gets the NaR (Not-a-Real) value.
  /// </summary>
  public static Posit8 NaR => new(NaRBits);

  /// <summary>
  /// Gets the maximum finite positive value.
  /// </summary>
  public static Posit8 MaxValue => new(NaRBits - 1); // 0x7F

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static Posit8 MinValue => new(NaRBits + 1); // 0x81

  /// <summary>
  /// Returns true if this value is NaR (Not-a-Real).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaR(Posit8 value) => value.RawValue == NaRBits;

  /// <summary>
  /// Returns true if this value is finite (i.e. not NaR).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(Posit8 value) => value.RawValue != NaRBits;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(Posit8 value) => (value.RawValue & NaRBits) != 0 && value.RawValue != NaRBits && value.RawValue != 0;

  /// <summary>
  /// Converts this Posit to a double-precision float.
  /// </summary>
  public double ToDouble() => PositCodec.Decode(this.RawValue, NBits, Es);

  /// <summary>
  /// Converts this Posit to a single-precision float.
  /// </summary>
  public float ToSingle() => (float)this.ToDouble();

  /// <summary>
  /// Creates a <see cref="Posit8"/> from a double-precision float.
  /// </summary>
  public static Posit8 FromDouble(double value) => new((byte)PositCodec.Encode(value, NBits, Es));

  /// <summary>
  /// Creates a <see cref="Posit8"/> from a single-precision float.
  /// </summary>
  public static Posit8 FromSingle(float value) => FromDouble(value);

  // Comparison
  public int CompareTo(Posit8 other) {
    if (IsNaR(this) || IsNaR(other))
      return IsNaR(this) && IsNaR(other) ? 0 : IsNaR(this) ? 1 : -1;
    return this.ToDouble().CompareTo(other.ToDouble());
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Posit8 other)
      throw new ArgumentException("Object must be of type Posit8.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Posit8 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is Posit8 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDouble().ToString(CultureInfo.InvariantCulture);

  public string ToString(IFormatProvider? provider) => this.ToDouble().ToString(provider);

  public string ToString(string? format) => this.ToDouble().ToString(format, CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? provider) => this.ToDouble().ToString(format, provider);

  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
    var str = format.IsEmpty
      ? this.ToDouble().ToString(provider)
      : this.ToDouble().ToString(format.ToString(), provider);
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }
    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  // Operators
  public static bool operator ==(Posit8 left, Posit8 right) => left.Equals(right);
  public static bool operator !=(Posit8 left, Posit8 right) => !left.Equals(right);
  public static bool operator <(Posit8 left, Posit8 right) => left.CompareTo(right) < 0;
  public static bool operator >(Posit8 left, Posit8 right) => left.CompareTo(right) > 0;
  public static bool operator <=(Posit8 left, Posit8 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(Posit8 left, Posit8 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via double conversion)
  public static Posit8 operator +(Posit8 left, Posit8 right) => FromDouble(left.ToDouble() + right.ToDouble());
  public static Posit8 operator -(Posit8 left, Posit8 right) => FromDouble(left.ToDouble() - right.ToDouble());
  public static Posit8 operator *(Posit8 left, Posit8 right) => FromDouble(left.ToDouble() * right.ToDouble());
  public static Posit8 operator /(Posit8 left, Posit8 right) => FromDouble(left.ToDouble() / right.ToDouble());
  public static Posit8 operator -(Posit8 value) => IsNaR(value) ? NaR : new((byte)((~value.RawValue + 1) & Mask));
  public static Posit8 operator +(Posit8 value) => value;

  // Conversions
  public static explicit operator Posit8(float value) => FromSingle(value);
  public static explicit operator Posit8(double value) => FromDouble(value);
  public static implicit operator float(Posit8 value) => value.ToSingle();
  public static implicit operator double(Posit8 value) => value.ToDouble();

  // Parsing
  public static Posit8 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static Posit8 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static Posit8 Parse(string s, NumberStyles style, IFormatProvider? provider) => FromDouble(double.Parse(s, style, provider));

  public static bool TryParse(string? s, out Posit8 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Posit8 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Posit8 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static Posit8 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromDouble(double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Posit8 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
