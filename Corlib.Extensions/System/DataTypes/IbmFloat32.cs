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
/// Represents an IBM System/360 hexadecimal floating-point single value (HFP, 32-bit).
/// Layout: bit 31 = sign; bits 24-30 = 7-bit exponent (excess-64, radix 16);
/// bits 0-23 = 24-bit fraction with NO hidden bit.
/// Value = (-1)^sign * (fraction / 2^24) * 16^(exponent - 64).
/// </summary>
/// <remarks>
/// The fraction is normalized so the most significant hex digit is non-zero (fraction in [1/16, 1)).
/// Because this format has no hidden bit and uses a radix-16 exponent, conversions to and from
/// IEEE-754 <see cref="float"/> are not bit-exact in general; this implementation prioritizes a
/// correct round-trip of representable values using round-to-nearest. The canonical encoding of
/// 1.0 is 0x41100000.
/// </remarks>
public readonly struct IbmFloat32 : IComparable, IComparable<IbmFloat32>, IEquatable<IbmFloat32>, IFormattable, ISpanFormattable, IParsable<IbmFloat32>, ISpanParsable<IbmFloat32> {

  private const uint SignMask = 0x80000000u;
  private const int ExponentShift = 24;
  private const int ExponentMask = 0x7F; // 7 bits
  private const int ExponentBias = 64;
  private const uint FractionMask = 0x00FFFFFFu; // 24 bits
  private const double FractionScale = 16777216.0; // 2^24

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public uint RawValue { get; }

  private IbmFloat32(uint raw) => this.RawValue = raw;

  /// <summary>
  /// Creates an <see cref="IbmFloat32"/> from the raw bit representation.
  /// </summary>
  public static IbmFloat32 FromRaw(uint raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static IbmFloat32 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0 (0x41100000).
  /// </summary>
  public static IbmFloat32 One => new(0x41100000u);

  // Component extraction
  private int Sign => (int)((this.RawValue & SignMask) >> 31);
  private int Exponent => (int)((this.RawValue >> ExponentShift) & ExponentMask);
  private uint Fraction => this.RawValue & FractionMask;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(IbmFloat32 value) => value.Sign != 0 && value.Fraction != 0;

  /// <summary>
  /// Converts this value to a single-precision float.
  /// </summary>
  public float ToSingle() => (float)this.ToDouble();

  /// <summary>
  /// Converts this value to a double-precision float.
  /// </summary>
  public double ToDouble() {
    var fraction = this.Fraction;
    if (fraction == 0)
      return 0d;

    var sign = this.Sign;
    var exp = this.Exponent;
    var mantissa = fraction / FractionScale; // in [1/16, 1)
    var result = mantissa * Math.Pow(16d, exp - ExponentBias);
    return sign == 0 ? result : -result;
  }

  /// <summary>
  /// Creates an <see cref="IbmFloat32"/> from a single-precision float.
  /// </summary>
  public static IbmFloat32 FromSingle(float value) => FromDouble(value);

  /// <summary>
  /// Creates an <see cref="IbmFloat32"/> from a double-precision float.
  /// </summary>
  public static IbmFloat32 FromDouble(double value) {
    if (double.IsNaN(value) || double.IsInfinity(value) || value == 0)
      return Zero;

    var sign = value < 0 ? 1u : 0u;
    var v = Math.Abs(value);

    // Find hex exponent e (excess-64) such that mantissa = v / 16^(e-64) is in [1/16, 1).
    // => 16^(e-64) is in (v, 16*v]  =>  e-64 = ceil(log16(v))
    var hexExp = (int)Math.Floor(Math.Log(v) / Math.Log(16d)) + 1;
    var biasedExp = hexExp + ExponentBias;

    var mantissa = v / Math.Pow(16d, hexExp); // in [1/16, 1)
    var fraction = (long)Math.Round(mantissa * FractionScale);

    // Rounding may push fraction to 2^24 (mantissa rounded up to 1.0); renormalize.
    if (fraction >= (long)FractionScale) {
      fraction = (long)(FractionScale / 16d); // 0x100000 -> mantissa 1/16
      ++biasedExp;
    }

    // Clamp on overflow / underflow of the 7-bit exponent field.
    if (biasedExp > ExponentMask)
      biasedExp = ExponentMask;
    if (biasedExp < 0)
      return Zero;

    if (fraction == 0)
      return Zero;

    var raw = (sign << 31) | ((uint)biasedExp << ExponentShift) | ((uint)fraction & FractionMask);
    return new(raw);
  }

  // Comparison
  public int CompareTo(IbmFloat32 other) => this.ToDouble().CompareTo(other.ToDouble());

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not IbmFloat32 other)
      throw new ArgumentException("Object must be of type IbmFloat32.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(IbmFloat32 other) => this.ToDouble() == other.ToDouble();

  public override bool Equals(object? obj) => obj is IbmFloat32 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.ToDouble().GetHashCode();

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
  public static bool operator ==(IbmFloat32 left, IbmFloat32 right) => left.Equals(right);
  public static bool operator !=(IbmFloat32 left, IbmFloat32 right) => !left.Equals(right);
  public static bool operator <(IbmFloat32 left, IbmFloat32 right) => left.CompareTo(right) < 0;
  public static bool operator >(IbmFloat32 left, IbmFloat32 right) => left.CompareTo(right) > 0;
  public static bool operator <=(IbmFloat32 left, IbmFloat32 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(IbmFloat32 left, IbmFloat32 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via float conversion)
  public static IbmFloat32 operator +(IbmFloat32 left, IbmFloat32 right) => FromSingle(left.ToSingle() + right.ToSingle());
  public static IbmFloat32 operator -(IbmFloat32 left, IbmFloat32 right) => FromSingle(left.ToSingle() - right.ToSingle());
  public static IbmFloat32 operator *(IbmFloat32 left, IbmFloat32 right) => FromSingle(left.ToSingle() * right.ToSingle());
  public static IbmFloat32 operator /(IbmFloat32 left, IbmFloat32 right) => FromSingle(left.ToSingle() / right.ToSingle());

  // Conversions
  public static explicit operator IbmFloat32(float value) => FromSingle(value);
  public static explicit operator IbmFloat32(double value) => FromDouble(value);
  public static implicit operator float(IbmFloat32 value) => value.ToSingle();
  public static implicit operator double(IbmFloat32 value) => value.ToDouble();

  // Parsing
  public static IbmFloat32 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static IbmFloat32 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static IbmFloat32 Parse(string s, NumberStyles style, IFormatProvider? provider) => FromDouble(double.Parse(s, style, provider));

  public static bool TryParse(string? s, out IbmFloat32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out IbmFloat32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out IbmFloat32 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static IbmFloat32 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromDouble(double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out IbmFloat32 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
