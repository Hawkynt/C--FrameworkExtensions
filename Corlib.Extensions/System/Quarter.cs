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
/// Represents an 8-bit IEEE 754 minifloat (1 sign + 5 exponent + 2 mantissa bits).
/// Bias: 15, range approximately ±57344 to ±0.0000305.
/// </summary>
public readonly struct Quarter : IComparable, IComparable<Quarter>, IEquatable<Quarter>, IFormattable, IParsable<Quarter> {

  private const int SignBits = 1;
  private const int ExponentBits = 5;
  private const int MantissaBits = 2;
  private const int ExponentBias = 15;
  private const int ExponentMask = (1 << ExponentBits) - 1; // 0x1F
  private const int MantissaMask = (1 << MantissaBits) - 1; // 0x03
  private const int SignMask = 1 << (ExponentBits + MantissaBits); // 0x80

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public byte RawValue { get; }

  private Quarter(byte raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a Quarter from the raw bit representation.
  /// </summary>
  public static Quarter FromRaw(byte raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static Quarter Zero => new(0);

  /// <summary>
  /// Gets the value 1.0.
  /// </summary>
  public static Quarter One => new((ExponentBias << MantissaBits) & 0xFF);

  /// <summary>
  /// Gets the smallest positive value (subnormal).
  /// </summary>
  public static Quarter Epsilon => new(1);

  /// <summary>
  /// Gets positive infinity.
  /// </summary>
  public static Quarter PositiveInfinity => new((ExponentMask << MantissaBits) & 0xFF);

  /// <summary>
  /// Gets negative infinity.
  /// </summary>
  public static Quarter NegativeInfinity => new((SignMask | (ExponentMask << MantissaBits)) & 0xFF);

  /// <summary>
  /// Gets a NaN value.
  /// </summary>
  public static Quarter NaN => new(((ExponentMask << MantissaBits) | 1) & 0xFF);

  /// <summary>
  /// Gets the maximum finite positive value.
  /// </summary>
  public static Quarter MaxValue => new((((ExponentMask - 1) << MantissaBits) | MantissaMask) & 0xFF);

  /// <summary>
  /// Gets the minimum finite positive value (smallest positive normal).
  /// </summary>
  public static Quarter MinValue => new((SignMask | ((ExponentMask - 1) << MantissaBits) | MantissaMask) & 0xFF);

  // Component extraction
  private int Sign => (this.RawValue >> (ExponentBits + MantissaBits)) & 1;
  private int Exponent => (this.RawValue >> MantissaBits) & ExponentMask;
  private int Mantissa => this.RawValue & MantissaMask;

  /// <summary>
  /// Returns true if this value is NaN.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(Quarter value) => value.Exponent == ExponentMask && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is positive or negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsInfinity(Quarter value) => value.Exponent == ExponentMask && value.Mantissa == 0;

  /// <summary>
  /// Returns true if this value is positive infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPositiveInfinity(Quarter value) => value.RawValue == PositiveInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegativeInfinity(Quarter value) => value.RawValue == NegativeInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is finite (not NaN or infinity).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(Quarter value) => value.Exponent != ExponentMask;

  /// <summary>
  /// Returns true if this value is a normal number.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNormal(Quarter value) {
    var exp = value.Exponent;
    return exp != 0 && exp != ExponentMask;
  }

  /// <summary>
  /// Returns true if this value is subnormal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSubnormal(Quarter value) => value.Exponent == 0 && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(Quarter value) => value.Sign != 0;

  /// <summary>
  /// Converts this Quarter to a single-precision float.
  /// </summary>
  public float ToSingle() {
    var sign = this.Sign;
    var exp = this.Exponent;
    var mant = this.Mantissa;

    if (exp == ExponentMask) {
      // Infinity or NaN
      if (mant == 0)
        return sign == 0 ? float.PositiveInfinity : float.NegativeInfinity;
      return float.NaN;
    }

    if (exp == 0) {
      // Zero or subnormal
      if (mant == 0)
        return sign == 0 ? 0f : -0f;
      // Subnormal: denormalize
      var value = mant / (float)(1 << MantissaBits) * MathF.Pow(2, 1 - ExponentBias);
      return sign == 0 ? value : -value;
    }

    // Normal number
    var mantissa = 1f + mant / (float)(1 << MantissaBits);
    var result = mantissa * MathF.Pow(2, exp - ExponentBias);
    return sign == 0 ? result : -result;
  }

  /// <summary>
  /// Converts this Quarter to a double-precision float.
  /// </summary>
  public double ToDouble() => this.ToSingle();

  /// <summary>
  /// Creates a Quarter from a single-precision float.
  /// </summary>
  public static Quarter FromSingle(float value) {
    if (float.IsNaN(value))
      return NaN;
    if (float.IsPositiveInfinity(value))
      return PositiveInfinity;
    if (float.IsNegativeInfinity(value))
      return NegativeInfinity;

    var sign = value < 0 ? 1 : 0;
    value = Math.Abs(value);

    if (value == 0)
      return new((byte)(sign << (ExponentBits + MantissaBits)));

    // Get exponent
    var exp = (int)Math.Floor(Math.Log(value, 2));
    var biasedExp = exp + ExponentBias;

    if (biasedExp >= ExponentMask)
      return sign == 0 ? PositiveInfinity : NegativeInfinity;

    if (biasedExp <= 0) {
      // Subnormal
      var mantissa = (int)Math.Round(value / MathF.Pow(2, 1 - ExponentBias) * (1 << MantissaBits));
      if (mantissa == 0)
        return new((byte)(sign << (ExponentBits + MantissaBits)));
      return new((byte)((sign << (ExponentBits + MantissaBits)) | (mantissa & MantissaMask)));
    }

    // Normal
    var mant = (int)Math.Round((value / MathF.Pow(2, exp) - 1) * (1 << MantissaBits));
    if (mant > MantissaMask) {
      mant = 0;
      ++biasedExp;
      if (biasedExp >= ExponentMask)
        return sign == 0 ? PositiveInfinity : NegativeInfinity;
    }

    return new((byte)((sign << (ExponentBits + MantissaBits)) | (biasedExp << MantissaBits) | mant));
  }

  /// <summary>
  /// Creates a Quarter from a double-precision float.
  /// </summary>
  public static Quarter FromDouble(double value) => FromSingle((float)value);

  // Comparison
  public int CompareTo(Quarter other) {
    if (IsNaN(this) || IsNaN(other))
      return IsNaN(this) && IsNaN(other) ? 0 : IsNaN(this) ? 1 : -1;
    return this.ToSingle().CompareTo(other.ToSingle());
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Quarter other)
      throw new ArgumentException("Object must be of type Quarter.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Quarter other) => this.RawValue == other.RawValue || (IsNaN(this) && IsNaN(other));

  public override bool Equals(object? obj) => obj is Quarter other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToSingle().ToString(CultureInfo.InvariantCulture);

  public string ToString(IFormatProvider? provider) => this.ToSingle().ToString(provider);

  public string ToString(string? format) => this.ToSingle().ToString(format, CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? provider) => this.ToSingle().ToString(format, provider);

  // Operators
  public static bool operator ==(Quarter left, Quarter right) => left.Equals(right);
  public static bool operator !=(Quarter left, Quarter right) => !left.Equals(right);
  public static bool operator <(Quarter left, Quarter right) => left.CompareTo(right) < 0;
  public static bool operator >(Quarter left, Quarter right) => left.CompareTo(right) > 0;
  public static bool operator <=(Quarter left, Quarter right) => left.CompareTo(right) <= 0;
  public static bool operator >=(Quarter left, Quarter right) => left.CompareTo(right) >= 0;

  // Arithmetic (via float conversion)
  public static Quarter operator +(Quarter left, Quarter right) => FromSingle(left.ToSingle() + right.ToSingle());
  public static Quarter operator -(Quarter left, Quarter right) => FromSingle(left.ToSingle() - right.ToSingle());
  public static Quarter operator *(Quarter left, Quarter right) => FromSingle(left.ToSingle() * right.ToSingle());
  public static Quarter operator /(Quarter left, Quarter right) => FromSingle(left.ToSingle() / right.ToSingle());
  public static Quarter operator -(Quarter value) => new((byte)(value.RawValue ^ SignMask));
  public static Quarter operator +(Quarter value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator %(Quarter left, Quarter right) => FromSingle(left.ToSingle() % right.ToSingle());

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator ++(Quarter value) => FromSingle(value.ToSingle() + 1f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator --(Quarter value) => FromSingle(value.ToSingle() - 1f);

  // Mixed-type arithmetic with float
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator *(Quarter left, float right) => FromSingle(left.ToSingle() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator *(float left, Quarter right) => FromSingle(left * right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator /(Quarter left, float right) => FromSingle(left.ToSingle() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator /(float left, Quarter right) => FromSingle(left / right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator +(Quarter left, float right) => FromSingle(left.ToSingle() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator +(float left, Quarter right) => FromSingle(left + right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator -(Quarter left, float right) => FromSingle(left.ToSingle() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator -(float left, Quarter right) => FromSingle(left - right.ToSingle());

  // Mixed-type arithmetic with int
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator *(Quarter left, int right) => FromSingle(left.ToSingle() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator *(int left, Quarter right) => FromSingle(left * right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator /(Quarter left, int right) => FromSingle(left.ToSingle() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator /(int left, Quarter right) => FromSingle(left / right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator +(Quarter left, int right) => FromSingle(left.ToSingle() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator +(int left, Quarter right) => FromSingle(left + right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator -(Quarter left, int right) => FromSingle(left.ToSingle() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter operator -(int left, Quarter right) => FromSingle(left - right.ToSingle());

  // Conversions
  public static explicit operator Quarter(float value) => FromSingle(value);
  public static explicit operator Quarter(double value) => FromDouble(value);
  public static explicit operator float(Quarter value) => value.ToSingle();
  public static explicit operator double(Quarter value) => value.ToDouble();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter Abs(Quarter value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter Min(Quarter left, Quarter right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToSingle() <= right.ToSingle() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter Max(Quarter left, Quarter right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToSingle() >= right.ToSingle() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quarter CopySign(Quarter value, Quarter sign) {
    var valueBits = (byte)(value.RawValue & ~SignMask);
    var signBit = (byte)(sign.RawValue & SignMask);
    return new((byte)(valueBits | signBit));
  }

  // Parsing
  public static Quarter Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static Quarter Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static Quarter Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = float.Parse(s, style, provider);
    return FromSingle(value);
  }

  public static bool TryParse(string? s, out Quarter result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Quarter result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Quarter result) {
    if (float.TryParse(s, style, provider, out var value)) {
      result = FromSingle(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
