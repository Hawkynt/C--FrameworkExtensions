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
/// Represents an 8-bit E4M3 floating-point format (1 sign + 4 exponent + 3 mantissa bits).
/// Commonly used in machine learning. Bias: 7.
/// Note: In E4M3, the value 0x7F (S=0, E=15, M=7) and 0xFF (S=1, E=15, M=7) represent NaN.
/// There is no infinity in E4M3 - the maximum exponent with non-NaN mantissa is a finite value.
/// </summary>
public readonly struct E4M3 : IComparable, IComparable<E4M3>, IEquatable<E4M3>, IFormattable, ISpanFormattable, IParsable<E4M3>, ISpanParsable<E4M3> {

  private const int SignBits = 1;
  private const int ExponentBits = 4;
  private const int MantissaBits = 3;
  private const int ExponentBias = 7;
  private const int ExponentMask = (1 << ExponentBits) - 1; // 0x0F
  private const int MantissaMask = (1 << MantissaBits) - 1; // 0x07
  private const int SignMask = 1 << (ExponentBits + MantissaBits); // 0x80

  // In E4M3, NaN is represented by exp=15, mantissa=7 (0x7F or 0xFF)
  private const byte NaNBits = (ExponentMask << MantissaBits) | MantissaMask; // 0x7F

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public byte RawValue { get; }

  private E4M3(byte raw) => this.RawValue = raw;

  /// <summary>
  /// Creates an E4M3 from the raw bit representation.
  /// </summary>
  public static E4M3 FromRaw(byte raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static E4M3 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0.
  /// </summary>
  public static E4M3 One => new((ExponentBias << MantissaBits) & 0xFF);

  /// <summary>
  /// Gets the smallest positive value (subnormal).
  /// </summary>
  public static E4M3 Epsilon => new(1);

  /// <summary>
  /// Gets a NaN value.
  /// </summary>
  public static E4M3 NaN => new(NaNBits);

  /// <summary>
  /// Gets the maximum finite positive value.
  /// In E4M3: exp=15, mantissa=6 (one less than NaN mantissa).
  /// </summary>
  public static E4M3 MaxValue => new((ExponentMask << MantissaBits) | (MantissaMask - 1));

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static E4M3 MinValue => new((byte)(SignMask | (ExponentMask << MantissaBits) | (MantissaMask - 1)));

  // Component extraction
  private int Sign => (this.RawValue >> (ExponentBits + MantissaBits)) & 1;
  private int Exponent => (this.RawValue >> MantissaBits) & ExponentMask;
  private int Mantissa => this.RawValue & MantissaMask;

  /// <summary>
  /// Returns true if this value is NaN.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(E4M3 value) => value.Exponent == ExponentMask && value.Mantissa == MantissaMask;

  /// <summary>
  /// Returns true if this value is finite.
  /// E4M3 has no infinity, so this is true for all non-NaN values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(E4M3 value) => !IsNaN(value);

  /// <summary>
  /// Returns true if this value is a normal number.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNormal(E4M3 value) {
    var exp = value.Exponent;
    return exp != 0 && !IsNaN(value);
  }

  /// <summary>
  /// Returns true if this value is subnormal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSubnormal(E4M3 value) => value.Exponent == 0 && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(E4M3 value) => value.Sign != 0 && !IsNaN(value);

  /// <summary>
  /// Converts this E4M3 to a single-precision float.
  /// </summary>
  public float ToSingle() {
    var sign = this.Sign;
    var exp = this.Exponent;
    var mant = this.Mantissa;

    // Check for NaN
    if (exp == ExponentMask && mant == MantissaMask)
      return float.NaN;

    if (exp == 0) {
      // Zero or subnormal
      if (mant == 0)
        return sign == 0 ? 0f : -0f;
      // Subnormal
      var value = mant / (float)(1 << MantissaBits) * MathF.Pow(2, 1 - ExponentBias);
      return sign == 0 ? value : -value;
    }

    // Normal number (including exp=15 with mantissa<7)
    var mantissa = 1f + mant / (float)(1 << MantissaBits);
    var result = mantissa * MathF.Pow(2, exp - ExponentBias);
    return sign == 0 ? result : -result;
  }

  /// <summary>
  /// Converts this E4M3 to a double-precision float.
  /// </summary>
  public double ToDouble() => this.ToSingle();

  /// <summary>
  /// Creates an E4M3 from a single-precision float.
  /// </summary>
  public static E4M3 FromSingle(float value) {
    if (float.IsNaN(value))
      return NaN;
    // E4M3 has no infinity - clamp to max value
    if (float.IsPositiveInfinity(value))
      return MaxValue;
    if (float.IsNegativeInfinity(value))
      return MinValue;

    var sign = value < 0 ? 1 : 0;
    value = Math.Abs(value);

    if (value == 0)
      return new((byte)(sign << (ExponentBits + MantissaBits)));

    // Get exponent
    var exp = (int)Math.Floor(Math.Log(value, 2));
    var biasedExp = exp + ExponentBias;

    // Check for overflow (clamp to max, not infinity)
    if (biasedExp >= ExponentMask) {
      // Return max value with appropriate sign
      return sign == 0 ? MaxValue : MinValue;
    }

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
        return sign == 0 ? MaxValue : MinValue;
    }

    // Avoid creating NaN (exp=15, mant=7)
    if (biasedExp == ExponentMask && mant == MantissaMask)
      --mant;

    return new((byte)((sign << (ExponentBits + MantissaBits)) | (biasedExp << MantissaBits) | mant));
  }

  /// <summary>
  /// Creates an E4M3 from a double-precision float.
  /// </summary>
  public static E4M3 FromDouble(double value) => FromSingle((float)value);

  // Comparison
  public int CompareTo(E4M3 other) {
    if (IsNaN(this) || IsNaN(other))
      return IsNaN(this) && IsNaN(other) ? 0 : IsNaN(this) ? 1 : -1;
    return this.ToSingle().CompareTo(other.ToSingle());
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not E4M3 other)
      throw new ArgumentException("Object must be of type E4M3.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(E4M3 other) => this.RawValue == other.RawValue || (IsNaN(this) && IsNaN(other));

  public override bool Equals(object? obj) => obj is E4M3 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToSingle().ToString(CultureInfo.InvariantCulture);

  public string ToString(IFormatProvider? provider) => this.ToSingle().ToString(provider);

  public string ToString(string? format) => this.ToSingle().ToString(format, CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? provider) => this.ToSingle().ToString(format, provider);

  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
    var str = format.IsEmpty
      ? this.ToSingle().ToString(provider)
      : this.ToSingle().ToString(format.ToString(), provider);
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }
    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  // Operators
  public static bool operator ==(E4M3 left, E4M3 right) => left.Equals(right);
  public static bool operator !=(E4M3 left, E4M3 right) => !left.Equals(right);
  public static bool operator <(E4M3 left, E4M3 right) => left.CompareTo(right) < 0;
  public static bool operator >(E4M3 left, E4M3 right) => left.CompareTo(right) > 0;
  public static bool operator <=(E4M3 left, E4M3 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(E4M3 left, E4M3 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via float conversion)
  public static E4M3 operator +(E4M3 left, E4M3 right) => FromSingle(left.ToSingle() + right.ToSingle());
  public static E4M3 operator -(E4M3 left, E4M3 right) => FromSingle(left.ToSingle() - right.ToSingle());
  public static E4M3 operator *(E4M3 left, E4M3 right) => FromSingle(left.ToSingle() * right.ToSingle());
  public static E4M3 operator /(E4M3 left, E4M3 right) => FromSingle(left.ToSingle() / right.ToSingle());
  public static E4M3 operator -(E4M3 value) => IsNaN(value) ? NaN : new((byte)(value.RawValue ^ SignMask));
  public static E4M3 operator +(E4M3 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator %(E4M3 left, E4M3 right) => FromSingle(left.ToSingle() % right.ToSingle());

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator ++(E4M3 value) => FromSingle(value.ToSingle() + 1f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator --(E4M3 value) => FromSingle(value.ToSingle() - 1f);

  // Mixed-type arithmetic with float
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator *(E4M3 left, float right) => FromSingle(left.ToSingle() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator *(float left, E4M3 right) => FromSingle(left * right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator /(E4M3 left, float right) => FromSingle(left.ToSingle() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator /(float left, E4M3 right) => FromSingle(left / right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator +(E4M3 left, float right) => FromSingle(left.ToSingle() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator +(float left, E4M3 right) => FromSingle(left + right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator -(E4M3 left, float right) => FromSingle(left.ToSingle() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator -(float left, E4M3 right) => FromSingle(left - right.ToSingle());

  // Mixed-type arithmetic with int
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator *(E4M3 left, int right) => FromSingle(left.ToSingle() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator *(int left, E4M3 right) => FromSingle(left * right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator /(E4M3 left, int right) => FromSingle(left.ToSingle() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator /(int left, E4M3 right) => FromSingle(left / right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator +(E4M3 left, int right) => FromSingle(left.ToSingle() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator +(int left, E4M3 right) => FromSingle(left + right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator -(E4M3 left, int right) => FromSingle(left.ToSingle() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 operator -(int left, E4M3 right) => FromSingle(left - right.ToSingle());

  // Conversions
  public static explicit operator E4M3(float value) => FromSingle(value);
  public static explicit operator E4M3(double value) => FromDouble(value);
  public static implicit operator float(E4M3 value) => value.ToSingle();
  public static implicit operator double(E4M3 value) => value.ToDouble();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 Abs(E4M3 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 Min(E4M3 left, E4M3 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToSingle() <= right.ToSingle() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 Max(E4M3 left, E4M3 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToSingle() >= right.ToSingle() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E4M3 CopySign(E4M3 value, E4M3 sign) {
    var valueBits = (byte)(value.RawValue & ~SignMask);
    var signBit = (byte)(sign.RawValue & SignMask);
    return new((byte)(valueBits | signBit));
  }

  // Parsing
  public static E4M3 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static E4M3 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static E4M3 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = float.Parse(s, style, provider);
    return FromSingle(value);
  }

  public static bool TryParse(string? s, out E4M3 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out E4M3 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out E4M3 result) {
    if (float.TryParse(s, style, provider, out var value)) {
      result = FromSingle(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static E4M3 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
    var value = float.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
    return FromSingle(value);
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out E4M3 result) {
    if (float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromSingle(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
