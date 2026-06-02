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
/// Represents a Microsoft Binary Format value (MBF, 64-bit) as used by early Microsoft BASIC.
/// Layout: bits 56-63 = 8-bit exponent (excess-128); bit 55 = sign;
/// bits 0-54 = 55-bit fraction with a hidden leading 1.
/// An exponent of 0 represents zero; otherwise
/// value = (-1)^sign * (1 + fraction/2^55) * 2^(exponent - 129).
/// </summary>
/// <remarks>
/// MBF has no infinities or NaNs. This type's primary conversions are <see cref="ToDouble"/> and
/// <see cref="FromDouble"/>; the single-precision overloads delegate to them. Conversions prioritize
/// a correct round-trip with round-to-nearest. The canonical encoding of 1.0 uses exponent 0x81,
/// sign 0, and fraction 0.
/// </remarks>
public readonly struct MBF64 : IComparable, IComparable<MBF64>, IEquatable<MBF64>, IFormattable, ISpanFormattable, IParsable<MBF64>, ISpanParsable<MBF64> {

  private const int ExponentBias = 129;
  private const int ExponentMask = 0xFF; // 8 bits
  private const int ExponentShift = 56;
  private const ulong SignMask = 1UL << 55;
  private const ulong FractionMask = (1UL << 55) - 1; // 55 bits
  private const double FractionScale = 36028797018963968.0; // 2^55

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public ulong RawValue { get; }

  private MBF64(ulong raw) => this.RawValue = raw;

  /// <summary>
  /// Creates an <see cref="MBF64"/> from the raw bit representation.
  /// </summary>
  public static MBF64 FromRaw(ulong raw) => new(raw);

  /// <summary>
  /// Gets zero.
  /// </summary>
  public static MBF64 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0 (exponent 0x81, sign 0, fraction 0).
  /// </summary>
  public static MBF64 One => new((ulong)ExponentBias << ExponentShift);

  // Component extraction
  private int Exponent => (int)((this.RawValue >> ExponentShift) & ExponentMask);
  private int Sign => (int)((this.RawValue & SignMask) >> 55);
  private ulong Fraction => this.RawValue & FractionMask;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(MBF64 value) => value.Sign != 0 && value.Exponent != 0;

  /// <summary>
  /// Converts this value to a single-precision float.
  /// </summary>
  public float ToSingle() => (float)this.ToDouble();

  /// <summary>
  /// Converts this value to a double-precision float.
  /// </summary>
  public double ToDouble() {
    var exp = this.Exponent;
    if (exp == 0)
      return 0d;

    var sign = this.Sign;
    var mantissa = 1d + this.Fraction / FractionScale;
    var result = mantissa * Math.Pow(2d, exp - ExponentBias);
    return sign == 0 ? result : -result;
  }

  /// <summary>
  /// Creates an <see cref="MBF64"/> from a single-precision float.
  /// </summary>
  public static MBF64 FromSingle(float value) => FromDouble(value);

  /// <summary>
  /// Creates an <see cref="MBF64"/> from a double-precision float.
  /// </summary>
  public static MBF64 FromDouble(double value) {
    if (double.IsNaN(value) || double.IsInfinity(value) || value == 0)
      return Zero;

    var sign = value < 0 ? 1UL : 0UL;
    var v = Math.Abs(value);

    var exp = (int)Math.Floor(Math.Log(v, 2));
    var biasedExp = exp + ExponentBias;

    var fraction = (long)Math.Round((v / Math.Pow(2d, exp) - 1d) * FractionScale);
    if (fraction >= (long)FractionScale) {
      fraction = 0;
      ++biasedExp;
    }

    // Underflow -> zero; overflow -> clamp to maximum magnitude.
    if (biasedExp <= 0)
      return Zero;
    if (biasedExp > ExponentMask) {
      biasedExp = ExponentMask;
      fraction = (long)FractionMask;
    }

    var raw = ((ulong)biasedExp << ExponentShift) | (sign << 55) | ((ulong)fraction & FractionMask);
    return new(raw);
  }

  // Comparison
  public int CompareTo(MBF64 other) => this.ToDouble().CompareTo(other.ToDouble());

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not MBF64 other)
      throw new ArgumentException("Object must be of type MBF64.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(MBF64 other) => this.ToDouble() == other.ToDouble();

  public override bool Equals(object? obj) => obj is MBF64 other && this.Equals(other);

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
  public static bool operator ==(MBF64 left, MBF64 right) => left.Equals(right);
  public static bool operator !=(MBF64 left, MBF64 right) => !left.Equals(right);
  public static bool operator <(MBF64 left, MBF64 right) => left.CompareTo(right) < 0;
  public static bool operator >(MBF64 left, MBF64 right) => left.CompareTo(right) > 0;
  public static bool operator <=(MBF64 left, MBF64 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(MBF64 left, MBF64 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via double conversion)
  public static MBF64 operator +(MBF64 left, MBF64 right) => FromDouble(left.ToDouble() + right.ToDouble());
  public static MBF64 operator -(MBF64 left, MBF64 right) => FromDouble(left.ToDouble() - right.ToDouble());
  public static MBF64 operator *(MBF64 left, MBF64 right) => FromDouble(left.ToDouble() * right.ToDouble());
  public static MBF64 operator /(MBF64 left, MBF64 right) => FromDouble(left.ToDouble() / right.ToDouble());

  // Conversions
  public static explicit operator MBF64(float value) => FromSingle(value);
  public static explicit operator MBF64(double value) => FromDouble(value);
  public static implicit operator float(MBF64 value) => value.ToSingle();
  public static implicit operator double(MBF64 value) => value.ToDouble();

  // Parsing
  public static MBF64 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static MBF64 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static MBF64 Parse(string s, NumberStyles style, IFormatProvider? provider) => FromDouble(double.Parse(s, style, provider));

  public static bool TryParse(string? s, out MBF64 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out MBF64 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out MBF64 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static MBF64 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromDouble(double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out MBF64 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
