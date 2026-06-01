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
/// Represents a 64-bit Brain Float (1 sign + 15 exponent + 48 mantissa bits).
/// BFloat64 uses the exponent width of IEEE 754 quad-precision (binary128)
/// with a truncated 48-bit mantissa, providing extended dynamic range.
/// </summary>
public readonly struct BFloat64 : IComparable, IComparable<BFloat64>, IEquatable<BFloat64>, IFormattable, ISpanFormattable, IParsable<BFloat64>, ISpanParsable<BFloat64> {

  private const int SignBits = 1;
  private const int ExponentBits = 15;
  private const int MantissaBits = 48;
  private const int ExponentBias = 16383;
  private const int ExponentMask = (1 << ExponentBits) - 1; // 0x7FFF
  private const long MantissaMask = (1L << MantissaBits) - 1; // 0xFFFFFFFFFFFF
  private const ulong SignMask = 1UL << 63; // 0x8000000000000000

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public ulong RawValue { get; }

  private BFloat64(ulong raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a BFloat64 from the raw bit representation.
  /// </summary>
  public static BFloat64 FromRaw(ulong raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static BFloat64 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0.
  /// </summary>
  public static BFloat64 One => new((ulong)ExponentBias << MantissaBits);

  /// <summary>
  /// Gets the smallest positive subnormal value.
  /// </summary>
  public static BFloat64 Epsilon => new(1);

  /// <summary>
  /// Gets positive infinity.
  /// </summary>
  public static BFloat64 PositiveInfinity => new((ulong)ExponentMask << MantissaBits);

  /// <summary>
  /// Gets negative infinity.
  /// </summary>
  public static BFloat64 NegativeInfinity => new(SignMask | ((ulong)ExponentMask << MantissaBits));

  /// <summary>
  /// Gets a NaN value.
  /// </summary>
  public static BFloat64 NaN => new(((ulong)ExponentMask << MantissaBits) | 1);

  /// <summary>
  /// Gets the maximum finite positive value.
  /// </summary>
  public static BFloat64 MaxValue => new((((ulong)ExponentMask - 1) << MantissaBits) | (ulong)MantissaMask);

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static BFloat64 MinValue => new(SignMask | (((ulong)ExponentMask - 1) << MantissaBits) | (ulong)MantissaMask);

  // Component extraction
  private int Sign => (int)((this.RawValue >> 63) & 1);
  private int Exponent => (int)((this.RawValue >> MantissaBits) & ExponentMask);
  private long Mantissa => (long)(this.RawValue & (ulong)MantissaMask);

  /// <summary>
  /// Returns true if this value is NaN.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(BFloat64 value) => value.Exponent == ExponentMask && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is positive or negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsInfinity(BFloat64 value) => value.Exponent == ExponentMask && value.Mantissa == 0;

  /// <summary>
  /// Returns true if this value is positive infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPositiveInfinity(BFloat64 value) => value.RawValue == PositiveInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegativeInfinity(BFloat64 value) => value.RawValue == NegativeInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is finite.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(BFloat64 value) => value.Exponent != ExponentMask;

  /// <summary>
  /// Returns true if this value is a normal number.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNormal(BFloat64 value) {
    var exp = value.Exponent;
    return exp != 0 && exp != ExponentMask;
  }

  /// <summary>
  /// Returns true if this value is subnormal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSubnormal(BFloat64 value) => value.Exponent == 0 && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(BFloat64 value) => value.Sign != 0;

  /// <summary>
  /// Converts this BFloat64 to a double-precision float.
  /// Note: This conversion may lose precision or overflow/underflow due to
  /// BFloat64's larger exponent range (15 bits vs double's 11 bits).
  /// </summary>
  public double ToDouble() {
    var sign = this.Sign;
    var exp = this.Exponent;
    var mant = this.Mantissa;

    if (exp == ExponentMask) {
      // Infinity or NaN
      if (mant == 0)
        return sign == 0 ? double.PositiveInfinity : double.NegativeInfinity;
      return double.NaN;
    }

    if (exp == 0) {
      // Zero or subnormal
      if (mant == 0)
        return sign == 0 ? 0.0 : -0.0;
      // Subnormal: denormalize
      var value = mant / (double)(1L << MantissaBits) * Math.Pow(2, 1 - ExponentBias);
      return sign == 0 ? value : -value;
    }

    // Normal number
    // Adjust exponent from BFloat64 bias (16383) to double bias (1023)
    var doubleExp = exp - ExponentBias + 1023;

    if (doubleExp >= 2047) {
      // Overflow to infinity
      return sign == 0 ? double.PositiveInfinity : double.NegativeInfinity;
    }

    if (doubleExp <= 0) {
      // Underflow to subnormal or zero
      if (doubleExp < -52)
        return sign == 0 ? 0.0 : -0.0;
      // Subnormal double
      var mantissa = (1.0 + mant / (double)(1L << MantissaBits)) * Math.Pow(2, doubleExp - 1);
      return sign == 0 ? mantissa : -mantissa;
    }

    // Normal double
    // Take upper 52 bits of our 48-bit mantissa (pad with zeros)
    var doubleMant = (ulong)mant << 4; // 48 bits -> 52 bits position
    var doubleBits = ((ulong)sign << 63) | ((ulong)doubleExp << 52) | doubleMant;
    return BitConverter.Int64BitsToDouble((long)doubleBits);
  }

  /// <summary>
  /// Converts this BFloat64 to a single-precision float.
  /// </summary>
  public float ToSingle() => (float)this.ToDouble();

  /// <summary>
  /// Creates a BFloat64 from a double-precision float.
  /// </summary>
  public static BFloat64 FromDouble(double value) {
    if (double.IsNaN(value))
      return NaN;
    if (double.IsPositiveInfinity(value))
      return PositiveInfinity;
    if (double.IsNegativeInfinity(value))
      return NegativeInfinity;

    var bits = BitConverter.DoubleToInt64Bits(value);
    var sign = (bits >> 63) & 1;
    var doubleExp = (int)((bits >> 52) & 0x7FF);
    var doubleMant = bits & 0xFFFFFFFFFFFFF; // 52 bits

    if (doubleExp == 0) {
      // Zero or subnormal double
      if (doubleMant == 0)
        return new((ulong)sign << 63);
      // Subnormal double -> likely subnormal or underflow in BFloat64
      // For simplicity, treat as zero (subnormal doubles are very small)
      return new((ulong)sign << 63);
    }

    // Convert exponent from double bias (1023) to BFloat64 bias (16383)
    var bfloatExp = doubleExp - 1023 + ExponentBias;

    if (bfloatExp <= 0) {
      // Underflow to zero or subnormal
      return new((ulong)sign << 63);
    }

    if (bfloatExp >= ExponentMask) {
      // Overflow to infinity (shouldn't happen for normal doubles)
      return sign == 0 ? PositiveInfinity : NegativeInfinity;
    }

    // Convert mantissa: double has 52 bits, we want 48 bits (truncate 4 bits)
    var bfloatMant = (ulong)(doubleMant >> 4);

    return new(((ulong)sign << 63) | ((ulong)bfloatExp << MantissaBits) | bfloatMant);
  }

  /// <summary>
  /// Creates a BFloat64 from a single-precision float.
  /// </summary>
  public static BFloat64 FromSingle(float value) => FromDouble(value);

  // Comparison
  public int CompareTo(BFloat64 other) {
    if (IsNaN(this) || IsNaN(other))
      return IsNaN(this) && IsNaN(other) ? 0 : IsNaN(this) ? 1 : -1;
    return this.ToDouble().CompareTo(other.ToDouble());
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not BFloat64 other)
      throw new ArgumentException("Object must be of type BFloat64.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(BFloat64 other) => this.RawValue == other.RawValue || (IsNaN(this) && IsNaN(other));

  public override bool Equals(object? obj) => obj is BFloat64 other && this.Equals(other);

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
  public static bool operator ==(BFloat64 left, BFloat64 right) => left.Equals(right);
  public static bool operator !=(BFloat64 left, BFloat64 right) => !left.Equals(right);
  public static bool operator <(BFloat64 left, BFloat64 right) => left.CompareTo(right) < 0;
  public static bool operator >(BFloat64 left, BFloat64 right) => left.CompareTo(right) > 0;
  public static bool operator <=(BFloat64 left, BFloat64 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(BFloat64 left, BFloat64 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via double conversion - note: may lose extended range)
  public static BFloat64 operator +(BFloat64 left, BFloat64 right) => FromDouble(left.ToDouble() + right.ToDouble());
  public static BFloat64 operator -(BFloat64 left, BFloat64 right) => FromDouble(left.ToDouble() - right.ToDouble());
  public static BFloat64 operator *(BFloat64 left, BFloat64 right) => FromDouble(left.ToDouble() * right.ToDouble());
  public static BFloat64 operator /(BFloat64 left, BFloat64 right) => FromDouble(left.ToDouble() / right.ToDouble());
  public static BFloat64 operator -(BFloat64 value) => new(value.RawValue ^ SignMask);
  public static BFloat64 operator +(BFloat64 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator %(BFloat64 left, BFloat64 right) => FromDouble(left.ToDouble() % right.ToDouble());

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator ++(BFloat64 value) => FromDouble(value.ToDouble() + 1.0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator --(BFloat64 value) => FromDouble(value.ToDouble() - 1.0);

  // Mixed-type arithmetic with double
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator *(BFloat64 left, double right) => FromDouble(left.ToDouble() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator *(double left, BFloat64 right) => FromDouble(left * right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator /(BFloat64 left, double right) => FromDouble(left.ToDouble() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator /(double left, BFloat64 right) => FromDouble(left / right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator +(BFloat64 left, double right) => FromDouble(left.ToDouble() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator +(double left, BFloat64 right) => FromDouble(left + right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator -(BFloat64 left, double right) => FromDouble(left.ToDouble() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator -(double left, BFloat64 right) => FromDouble(left - right.ToDouble());

  // Mixed-type arithmetic with int
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator *(BFloat64 left, int right) => FromDouble(left.ToDouble() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator *(int left, BFloat64 right) => FromDouble(left * right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator /(BFloat64 left, int right) => FromDouble(left.ToDouble() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator /(int left, BFloat64 right) => FromDouble(left / right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator +(BFloat64 left, int right) => FromDouble(left.ToDouble() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator +(int left, BFloat64 right) => FromDouble(left + right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator -(BFloat64 left, int right) => FromDouble(left.ToDouble() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 operator -(int left, BFloat64 right) => FromDouble(left - right.ToDouble());

  // Conversions
  public static explicit operator BFloat64(float value) => FromSingle(value);
  public static explicit operator BFloat64(double value) => FromDouble(value);
  public static explicit operator float(BFloat64 value) => value.ToSingle();
  public static explicit operator double(BFloat64 value) => value.ToDouble();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 Abs(BFloat64 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 Min(BFloat64 left, BFloat64 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToDouble() <= right.ToDouble() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 Max(BFloat64 left, BFloat64 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToDouble() >= right.ToDouble() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat64 CopySign(BFloat64 value, BFloat64 sign) {
    var valueBits = value.RawValue & ~SignMask;
    var signBit = sign.RawValue & SignMask;
    return new(valueBits | signBit);
  }

  // Parsing
  public static BFloat64 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static BFloat64 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static BFloat64 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    return FromDouble(value);
  }

  public static bool TryParse(string? s, out BFloat64 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out BFloat64 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out BFloat64 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static BFloat64 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
    var value = double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
    return FromDouble(value);
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BFloat64 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
