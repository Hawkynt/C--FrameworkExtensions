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
/// Represents a VAX F_floating value (32-bit, PDP-11 word order).
/// Within the 32-bit value: bit 15 = sign; bits 7-14 = 8-bit exponent (excess-128);
/// bits 0-6 = high 7 fraction bits; bits 16-31 = low 16 fraction bits.
/// The 23-bit fraction is ((RawValue &amp; 0x7F) &lt;&lt; 16) | ((RawValue &gt;&gt; 16) &amp; 0xFFFF) with a hidden leading 1.
/// Value = (-1)^sign * (1 + fraction/2^23) * 2^(exponent - 129).
/// </summary>
/// <remarks>
/// An exponent of 0 with sign 0 is true zero; an exponent of 0 with sign 1 is a reserved operand,
/// treated here as zero. Conversions prioritize a correct round-trip with round-to-nearest.
/// The canonical encoding of 1.0 uses exponent 129 (0x81) and fraction 0.
/// </remarks>
public readonly struct VaxFloat : IComparable, IComparable<VaxFloat>, IEquatable<VaxFloat>, IFormattable, ISpanFormattable, IParsable<VaxFloat>, ISpanParsable<VaxFloat> {

  private const int ExponentBias = 129;
  private const int ExponentMask = 0xFF; // 8 bits
  private const int FractionBits = 23;
  private const double FractionScale = 8388608.0; // 2^23

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public uint RawValue { get; }

  private VaxFloat(uint raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="VaxFloat"/> from the raw bit representation.
  /// </summary>
  public static VaxFloat FromRaw(uint raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static VaxFloat Zero => new(0);

  /// <summary>
  /// Gets the value 1.0 (exponent 0x81, fraction 0).
  /// </summary>
  public static VaxFloat One => new((uint)ExponentBias << 7);

  // Component extraction
  private int Sign => (int)((this.RawValue >> 15) & 1);
  private int Exponent => (int)((this.RawValue >> 7) & ExponentMask);
  private uint Fraction => ((this.RawValue & 0x7F) << 16) | ((this.RawValue >> 16) & 0xFFFF);

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(VaxFloat value) => value.Sign != 0 && value.Exponent != 0;

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
      return 0d; // true zero (sign 0) or reserved operand (sign 1) -> treated as 0

    var sign = this.Sign;
    var mantissa = 1d + this.Fraction / FractionScale;
    var result = mantissa * Math.Pow(2d, exp - ExponentBias);
    return sign == 0 ? result : -result;
  }

  /// <summary>
  /// Creates a <see cref="VaxFloat"/> from a single-precision float.
  /// </summary>
  public static VaxFloat FromSingle(float value) => FromDouble(value);

  /// <summary>
  /// Creates a <see cref="VaxFloat"/> from a double-precision float.
  /// </summary>
  public static VaxFloat FromDouble(double value) {
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

    var frac = (uint)fraction;
    var hi7 = (frac >> 16) & 0x7F;
    var lo16 = frac & 0xFFFF;
    var raw = (sign << 15) | ((uint)biasedExp << 7) | hi7 | (lo16 << 16);
    return new(raw);
  }

  // Comparison
  public int CompareTo(VaxFloat other) => this.ToDouble().CompareTo(other.ToDouble());

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not VaxFloat other)
      throw new ArgumentException("Object must be of type VaxFloat.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(VaxFloat other) => this.ToDouble() == other.ToDouble();

  public override bool Equals(object? obj) => obj is VaxFloat other && this.Equals(other);

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
  public static bool operator ==(VaxFloat left, VaxFloat right) => left.Equals(right);
  public static bool operator !=(VaxFloat left, VaxFloat right) => !left.Equals(right);
  public static bool operator <(VaxFloat left, VaxFloat right) => left.CompareTo(right) < 0;
  public static bool operator >(VaxFloat left, VaxFloat right) => left.CompareTo(right) > 0;
  public static bool operator <=(VaxFloat left, VaxFloat right) => left.CompareTo(right) <= 0;
  public static bool operator >=(VaxFloat left, VaxFloat right) => left.CompareTo(right) >= 0;

  // Arithmetic (via float conversion)
  public static VaxFloat operator +(VaxFloat left, VaxFloat right) => FromSingle(left.ToSingle() + right.ToSingle());
  public static VaxFloat operator -(VaxFloat left, VaxFloat right) => FromSingle(left.ToSingle() - right.ToSingle());
  public static VaxFloat operator *(VaxFloat left, VaxFloat right) => FromSingle(left.ToSingle() * right.ToSingle());
  public static VaxFloat operator /(VaxFloat left, VaxFloat right) => FromSingle(left.ToSingle() / right.ToSingle());

  // Conversions
  public static explicit operator VaxFloat(float value) => FromSingle(value);
  public static explicit operator VaxFloat(double value) => FromDouble(value);
  public static implicit operator float(VaxFloat value) => value.ToSingle();
  public static implicit operator double(VaxFloat value) => value.ToDouble();

  // Parsing
  public static VaxFloat Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static VaxFloat Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static VaxFloat Parse(string s, NumberStyles style, IFormatProvider? provider) => FromDouble(double.Parse(s, style, provider));

  public static bool TryParse(string? s, out VaxFloat result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out VaxFloat result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out VaxFloat result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static VaxFloat Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromDouble(double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out VaxFloat result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
