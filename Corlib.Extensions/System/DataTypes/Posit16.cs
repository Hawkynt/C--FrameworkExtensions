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
/// Represents a 16-bit Posit (unum type III) number with nbits=16 and es=1.
/// Posits have tapered accuracy: values near 1.0 carry the most precision.
/// There is no infinity; the bit pattern 0x8000 (sign bit only) represents NaR (Not-a-Real), mapped to <see cref="double.NaN"/>.
/// </summary>
public readonly struct Posit16 : IComparable, IComparable<Posit16>, IEquatable<Posit16>, IFormattable, ISpanFormattable, IParsable<Posit16>, ISpanParsable<Posit16> {

  private const int NBits = 16;
  private const int Es = 1;
  private const uint Mask = (1u << NBits) - 1; // 0xFFFF
  private const ushort NaRBits = 1 << (NBits - 1); // 0x8000

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public ushort RawValue { get; }

  private Posit16(ushort raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="Posit16"/> from the raw bit representation.
  /// </summary>
  public static Posit16 FromRaw(ushort raw) => new(raw);

  /// <summary>
  /// Gets zero.
  /// </summary>
  public static Posit16 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0.
  /// </summary>
  public static Posit16 One => FromDouble(1.0);

  /// <summary>
  /// Gets the NaR (Not-a-Real) value.
  /// </summary>
  public static Posit16 NaR => new(NaRBits);

  /// <summary>
  /// Gets the maximum finite positive value.
  /// </summary>
  public static Posit16 MaxValue => new(NaRBits - 1); // 0x7FFF

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static Posit16 MinValue => new((ushort)(NaRBits + 1)); // 0x8001

  /// <summary>
  /// Returns true if this value is NaR (Not-a-Real).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaR(Posit16 value) => value.RawValue == NaRBits;

  /// <summary>
  /// Returns true if this value is finite (i.e. not NaR).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(Posit16 value) => value.RawValue != NaRBits;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(Posit16 value) => (value.RawValue & NaRBits) != 0 && value.RawValue != NaRBits && value.RawValue != 0;

  /// <summary>
  /// Converts this Posit to a double-precision float.
  /// </summary>
  public double ToDouble() => PositCodec.Decode(this.RawValue, NBits, Es);

  /// <summary>
  /// Converts this Posit to a single-precision float.
  /// </summary>
  public float ToSingle() => (float)this.ToDouble();

  /// <summary>
  /// Creates a <see cref="Posit16"/> from a double-precision float.
  /// </summary>
  public static Posit16 FromDouble(double value) => new((ushort)PositCodec.Encode(value, NBits, Es));

  /// <summary>
  /// Creates a <see cref="Posit16"/> from a single-precision float.
  /// </summary>
  public static Posit16 FromSingle(float value) => FromDouble(value);

  // Comparison
  public int CompareTo(Posit16 other) {
    if (IsNaR(this) || IsNaR(other))
      return IsNaR(this) && IsNaR(other) ? 0 : IsNaR(this) ? 1 : -1;
    return this.ToDouble().CompareTo(other.ToDouble());
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Posit16 other)
      throw new ArgumentException("Object must be of type Posit16.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Posit16 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is Posit16 other && this.Equals(other);

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
  public static bool operator ==(Posit16 left, Posit16 right) => left.Equals(right);
  public static bool operator !=(Posit16 left, Posit16 right) => !left.Equals(right);
  public static bool operator <(Posit16 left, Posit16 right) => left.CompareTo(right) < 0;
  public static bool operator >(Posit16 left, Posit16 right) => left.CompareTo(right) > 0;
  public static bool operator <=(Posit16 left, Posit16 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(Posit16 left, Posit16 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via double conversion)
  public static Posit16 operator +(Posit16 left, Posit16 right) => FromDouble(left.ToDouble() + right.ToDouble());
  public static Posit16 operator -(Posit16 left, Posit16 right) => FromDouble(left.ToDouble() - right.ToDouble());
  public static Posit16 operator *(Posit16 left, Posit16 right) => FromDouble(left.ToDouble() * right.ToDouble());
  public static Posit16 operator /(Posit16 left, Posit16 right) => FromDouble(left.ToDouble() / right.ToDouble());
  public static Posit16 operator -(Posit16 value) => IsNaR(value) ? NaR : new((ushort)((~value.RawValue + 1) & Mask));
  public static Posit16 operator +(Posit16 value) => value;

  // Conversions
  public static explicit operator Posit16(float value) => FromSingle(value);
  public static explicit operator Posit16(double value) => FromDouble(value);
  public static implicit operator float(Posit16 value) => value.ToSingle();
  public static implicit operator double(Posit16 value) => value.ToDouble();

  // Parsing
  public static Posit16 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static Posit16 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static Posit16 Parse(string s, NumberStyles style, IFormatProvider? provider) => FromDouble(double.Parse(s, style, provider));

  public static bool TryParse(string? s, out Posit16 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Posit16 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Posit16 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static Posit16 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromDouble(double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Posit16 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
