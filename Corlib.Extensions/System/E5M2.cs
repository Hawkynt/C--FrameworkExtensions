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
/// Represents an 8-bit E5M2 floating-point format (1 sign + 5 exponent + 2 mantissa bits).
/// Commonly used in machine learning. Bias: 15.
/// E5M2 follows IEEE 754 conventions: exp=31 with mantissa=0 is infinity, mantissa≠0 is NaN.
/// </summary>
public readonly struct E5M2 : IComparable, IComparable<E5M2>, IEquatable<E5M2>, IFormattable, IParsable<E5M2> {

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

  private E5M2(byte raw) => this.RawValue = raw;

  /// <summary>
  /// Creates an E5M2 from the raw bit representation.
  /// </summary>
  public static E5M2 FromRaw(byte raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static E5M2 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0.
  /// </summary>
  public static E5M2 One => new((ExponentBias << MantissaBits) & 0xFF);

  /// <summary>
  /// Gets the smallest positive value (subnormal).
  /// </summary>
  public static E5M2 Epsilon => new(1);

  /// <summary>
  /// Gets positive infinity.
  /// </summary>
  public static E5M2 PositiveInfinity => new((ExponentMask << MantissaBits) & 0xFF);

  /// <summary>
  /// Gets negative infinity.
  /// </summary>
  public static E5M2 NegativeInfinity => new((SignMask | (ExponentMask << MantissaBits)) & 0xFF);

  /// <summary>
  /// Gets a NaN value.
  /// </summary>
  public static E5M2 NaN => new(((ExponentMask << MantissaBits) | 1) & 0xFF);

  /// <summary>
  /// Gets the maximum finite positive value.
  /// </summary>
  public static E5M2 MaxValue => new((((ExponentMask - 1) << MantissaBits) | MantissaMask) & 0xFF);

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static E5M2 MinValue => new((SignMask | ((ExponentMask - 1) << MantissaBits) | MantissaMask) & 0xFF);

  // Component extraction
  private int Sign => (this.RawValue >> (ExponentBits + MantissaBits)) & 1;
  private int Exponent => (this.RawValue >> MantissaBits) & ExponentMask;
  private int Mantissa => this.RawValue & MantissaMask;

  /// <summary>
  /// Returns true if this value is NaN.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(E5M2 value) => value.Exponent == ExponentMask && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is positive or negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsInfinity(E5M2 value) => value.Exponent == ExponentMask && value.Mantissa == 0;

  /// <summary>
  /// Returns true if this value is positive infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPositiveInfinity(E5M2 value) => value.RawValue == PositiveInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegativeInfinity(E5M2 value) => value.RawValue == NegativeInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is finite (not NaN or infinity).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(E5M2 value) => value.Exponent != ExponentMask;

  /// <summary>
  /// Returns true if this value is a normal number.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNormal(E5M2 value) {
    var exp = value.Exponent;
    return exp != 0 && exp != ExponentMask;
  }

  /// <summary>
  /// Returns true if this value is subnormal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSubnormal(E5M2 value) => value.Exponent == 0 && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(E5M2 value) => value.Sign != 0;

  /// <summary>
  /// Converts this E5M2 to a single-precision float.
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
      // Subnormal
      var value = mant / (float)(1 << MantissaBits) * MathF.Pow(2, 1 - ExponentBias);
      return sign == 0 ? value : -value;
    }

    // Normal number
    var mantissa = 1f + mant / (float)(1 << MantissaBits);
    var result = mantissa * MathF.Pow(2, exp - ExponentBias);
    return sign == 0 ? result : -result;
  }

  /// <summary>
  /// Converts this E5M2 to a double-precision float.
  /// </summary>
  public double ToDouble() => this.ToSingle();

  /// <summary>
  /// Creates an E5M2 from a single-precision float.
  /// </summary>
  public static E5M2 FromSingle(float value) {
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
  /// Creates an E5M2 from a double-precision float.
  /// </summary>
  public static E5M2 FromDouble(double value) => FromSingle((float)value);

  // Comparison
  public int CompareTo(E5M2 other) {
    if (IsNaN(this) || IsNaN(other))
      return IsNaN(this) && IsNaN(other) ? 0 : IsNaN(this) ? 1 : -1;
    return this.ToSingle().CompareTo(other.ToSingle());
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not E5M2 other)
      throw new ArgumentException("Object must be of type E5M2.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(E5M2 other) => this.RawValue == other.RawValue || (IsNaN(this) && IsNaN(other));

  public override bool Equals(object? obj) => obj is E5M2 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToSingle().ToString(CultureInfo.InvariantCulture);

  public string ToString(IFormatProvider? provider) => this.ToSingle().ToString(provider);

  public string ToString(string? format) => this.ToSingle().ToString(format, CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? provider) => this.ToSingle().ToString(format, provider);

  // Operators
  public static bool operator ==(E5M2 left, E5M2 right) => left.Equals(right);
  public static bool operator !=(E5M2 left, E5M2 right) => !left.Equals(right);
  public static bool operator <(E5M2 left, E5M2 right) => left.CompareTo(right) < 0;
  public static bool operator >(E5M2 left, E5M2 right) => left.CompareTo(right) > 0;
  public static bool operator <=(E5M2 left, E5M2 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(E5M2 left, E5M2 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via float conversion)
  public static E5M2 operator +(E5M2 left, E5M2 right) => FromSingle(left.ToSingle() + right.ToSingle());
  public static E5M2 operator -(E5M2 left, E5M2 right) => FromSingle(left.ToSingle() - right.ToSingle());
  public static E5M2 operator *(E5M2 left, E5M2 right) => FromSingle(left.ToSingle() * right.ToSingle());
  public static E5M2 operator /(E5M2 left, E5M2 right) => FromSingle(left.ToSingle() / right.ToSingle());
  public static E5M2 operator -(E5M2 value) => new((byte)(value.RawValue ^ SignMask));
  public static E5M2 operator +(E5M2 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator %(E5M2 left, E5M2 right) => FromSingle(left.ToSingle() % right.ToSingle());

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator ++(E5M2 value) => FromSingle(value.ToSingle() + 1f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator --(E5M2 value) => FromSingle(value.ToSingle() - 1f);

  // Mixed-type arithmetic with float
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator *(E5M2 left, float right) => FromSingle(left.ToSingle() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator *(float left, E5M2 right) => FromSingle(left * right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator /(E5M2 left, float right) => FromSingle(left.ToSingle() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator /(float left, E5M2 right) => FromSingle(left / right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator +(E5M2 left, float right) => FromSingle(left.ToSingle() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator +(float left, E5M2 right) => FromSingle(left + right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator -(E5M2 left, float right) => FromSingle(left.ToSingle() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator -(float left, E5M2 right) => FromSingle(left - right.ToSingle());

  // Mixed-type arithmetic with int
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator *(E5M2 left, int right) => FromSingle(left.ToSingle() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator *(int left, E5M2 right) => FromSingle(left * right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator /(E5M2 left, int right) => FromSingle(left.ToSingle() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator /(int left, E5M2 right) => FromSingle(left / right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator +(E5M2 left, int right) => FromSingle(left.ToSingle() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator +(int left, E5M2 right) => FromSingle(left + right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator -(E5M2 left, int right) => FromSingle(left.ToSingle() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 operator -(int left, E5M2 right) => FromSingle(left - right.ToSingle());

  // Conversions
  public static explicit operator E5M2(float value) => FromSingle(value);
  public static explicit operator E5M2(double value) => FromDouble(value);
  public static explicit operator float(E5M2 value) => value.ToSingle();
  public static explicit operator double(E5M2 value) => value.ToDouble();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 Abs(E5M2 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 Min(E5M2 left, E5M2 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToSingle() <= right.ToSingle() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 Max(E5M2 left, E5M2 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToSingle() >= right.ToSingle() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E5M2 CopySign(E5M2 value, E5M2 sign) {
    var valueBits = (byte)(value.RawValue & ~SignMask);
    var signBit = (byte)(sign.RawValue & SignMask);
    return new((byte)(valueBits | signBit));
  }

  // Parsing
  public static E5M2 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static E5M2 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static E5M2 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = float.Parse(s, style, provider);
    return FromSingle(value);
  }

  public static bool TryParse(string? s, out E5M2 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out E5M2 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out E5M2 result) {
    if (float.TryParse(s, style, provider, out var value)) {
      result = FromSingle(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
