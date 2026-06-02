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
/// Represents a Microsoft Binary Format value (MBF, 32-bit) as used by early Microsoft BASIC.
/// Layout: bits 24-31 = 8-bit exponent (excess-128); bit 23 = sign;
/// bits 0-22 = 23-bit fraction with a hidden leading 1.
/// An exponent of 0 represents zero; otherwise
/// value = (-1)^sign * (1 + fraction/2^23) * 2^(exponent - 129).
/// </summary>
/// <remarks>
/// MBF has no infinities or NaNs. Conversions prioritize a correct round-trip with round-to-nearest.
/// The canonical encoding of 1.0 uses exponent 0x81, sign 0, and fraction 0.
/// </remarks>
public readonly struct MBF32 : IComparable, IComparable<MBF32>, IEquatable<MBF32>, IFormattable, ISpanFormattable, IParsable<MBF32>, ISpanParsable<MBF32> {

  private const int ExponentBias = 129;
  private const int ExponentMask = 0xFF; // 8 bits
  private const int ExponentShift = 24;
  private const uint SignMask = 1u << 23;
  private const uint FractionMask = 0x007FFFFFu; // 23 bits
  private const double FractionScale = 8388608.0; // 2^23

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public uint RawValue { get; }

  private MBF32(uint raw) => this.RawValue = raw;

  /// <summary>
  /// Creates an <see cref="MBF32"/> from the raw bit representation.
  /// </summary>
  public static MBF32 FromRaw(uint raw) => new(raw);

  /// <summary>
  /// Gets zero.
  /// </summary>
  public static MBF32 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0 (exponent 0x81, sign 0, fraction 0).
  /// </summary>
  public static MBF32 One => new((uint)ExponentBias << ExponentShift);

  // Component extraction
  private int Exponent => (int)((this.RawValue >> ExponentShift) & ExponentMask);
  private int Sign => (int)((this.RawValue & SignMask) >> 23);
  private uint Fraction => this.RawValue & FractionMask;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(MBF32 value) => value.Sign != 0 && value.Exponent != 0;

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
  /// Creates an <see cref="MBF32"/> from a single-precision float.
  /// </summary>
  public static MBF32 FromSingle(float value) => FromDouble(value);

  /// <summary>
  /// Creates an <see cref="MBF32"/> from a double-precision float.
  /// </summary>
  public static MBF32 FromDouble(double value) {
    if (double.IsNaN(value) || double.IsInfinity(value) || value == 0)
      return Zero;

    var sign = value < 0 ? 1u : 0u;
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
      fraction = (long)FractionScale - 1;
    }

    var raw = ((uint)biasedExp << ExponentShift) | (sign << 23) | ((uint)fraction & FractionMask);
    return new(raw);
  }

  // Comparison
  public int CompareTo(MBF32 other) => this.ToDouble().CompareTo(other.ToDouble());

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not MBF32 other)
      throw new ArgumentException("Object must be of type MBF32.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(MBF32 other) => this.ToDouble() == other.ToDouble();

  public override bool Equals(object? obj) => obj is MBF32 other && this.Equals(other);

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
  public static bool operator ==(MBF32 left, MBF32 right) => left.Equals(right);
  public static bool operator !=(MBF32 left, MBF32 right) => !left.Equals(right);
  public static bool operator <(MBF32 left, MBF32 right) => left.CompareTo(right) < 0;
  public static bool operator >(MBF32 left, MBF32 right) => left.CompareTo(right) > 0;
  public static bool operator <=(MBF32 left, MBF32 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(MBF32 left, MBF32 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via float conversion)
  public static MBF32 operator +(MBF32 left, MBF32 right) => FromSingle(left.ToSingle() + right.ToSingle());
  public static MBF32 operator -(MBF32 left, MBF32 right) => FromSingle(left.ToSingle() - right.ToSingle());
  public static MBF32 operator *(MBF32 left, MBF32 right) => FromSingle(left.ToSingle() * right.ToSingle());
  public static MBF32 operator /(MBF32 left, MBF32 right) => FromSingle(left.ToSingle() / right.ToSingle());

  // Conversions
  public static explicit operator MBF32(float value) => FromSingle(value);
  public static explicit operator MBF32(double value) => FromDouble(value);
  public static implicit operator float(MBF32 value) => value.ToSingle();
  public static implicit operator double(MBF32 value) => value.ToDouble();

  // Parsing
  public static MBF32 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static MBF32 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static MBF32 Parse(string s, NumberStyles style, IFormatProvider? provider) => FromDouble(double.Parse(s, style, provider));

  public static bool TryParse(string? s, out MBF32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out MBF32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out MBF32 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static MBF32 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromDouble(double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out MBF32 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
