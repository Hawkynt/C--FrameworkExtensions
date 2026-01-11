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
/// Represents an 8-bit Brain Float (1 sign + 5 exponent + 2 mantissa bits).
/// BFloat8 is a truncated version of Float16/Half, providing the same dynamic range
/// as Half with reduced precision.
/// </summary>
public readonly struct BFloat8 : IComparable, IComparable<BFloat8>, IEquatable<BFloat8>, IFormattable, IParsable<BFloat8> {

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

  private BFloat8(byte raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a BFloat8 from the raw bit representation.
  /// </summary>
  public static BFloat8 FromRaw(byte raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static BFloat8 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0.
  /// </summary>
  public static BFloat8 One => new((byte)(ExponentBias << MantissaBits));

  /// <summary>
  /// Gets the smallest positive subnormal value.
  /// </summary>
  public static BFloat8 Epsilon => new(1);

  /// <summary>
  /// Gets positive infinity.
  /// </summary>
  public static BFloat8 PositiveInfinity => new((byte)(ExponentMask << MantissaBits));

  /// <summary>
  /// Gets negative infinity.
  /// </summary>
  public static BFloat8 NegativeInfinity => new((byte)(SignMask | (ExponentMask << MantissaBits)));

  /// <summary>
  /// Gets a NaN value.
  /// </summary>
  public static BFloat8 NaN => new((byte)((ExponentMask << MantissaBits) | 1));

  /// <summary>
  /// Gets the maximum finite positive value.
  /// </summary>
  public static BFloat8 MaxValue => new((byte)(((ExponentMask - 1) << MantissaBits) | MantissaMask));

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static BFloat8 MinValue => new((byte)(SignMask | ((ExponentMask - 1) << MantissaBits) | MantissaMask));

  // Component extraction
  private int Sign => (this.RawValue >> (ExponentBits + MantissaBits)) & 1;
  private int Exponent => (this.RawValue >> MantissaBits) & ExponentMask;
  private int Mantissa => this.RawValue & MantissaMask;

  /// <summary>
  /// Returns true if this value is NaN.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(BFloat8 value) => value.Exponent == ExponentMask && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is positive or negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsInfinity(BFloat8 value) => value.Exponent == ExponentMask && value.Mantissa == 0;

  /// <summary>
  /// Returns true if this value is positive infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPositiveInfinity(BFloat8 value) => value.RawValue == PositiveInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegativeInfinity(BFloat8 value) => value.RawValue == NegativeInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is finite.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(BFloat8 value) => value.Exponent != ExponentMask;

  /// <summary>
  /// Returns true if this value is a normal number.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNormal(BFloat8 value) {
    var exp = value.Exponent;
    return exp != 0 && exp != ExponentMask;
  }

  /// <summary>
  /// Returns true if this value is subnormal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSubnormal(BFloat8 value) => value.Exponent == 0 && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(BFloat8 value) => value.Sign != 0;

  /// <summary>
  /// Converts this BFloat8 to a Half by expanding the mantissa.
  /// </summary>
  public Half ToHalf() {
    // BFloat8 layout: S EEEEE MM (1+5+2 = 8 bits)
    // Half layout:    S EEEEE MMMMMMMMMM (1+5+10 = 16 bits)
    // We expand the 2-bit mantissa to 10 bits by padding with zeros
    var sign = (ushort)((this.RawValue >> 7) << 15);
    var exp = (ushort)(((this.RawValue >> 2) & 0x1F) << 10);
    var mant = (ushort)((this.RawValue & 0x03) << 8);
    var bits = (ushort)(sign | exp | mant);
    return BitConverter.ToUInt16(BitConverter.GetBytes(bits), 0).IsHalf();
  }

  /// <summary>
  /// Converts this BFloat8 to a single-precision float.
  /// </summary>
  public float ToSingle() => (float)this.ToHalf();

  /// <summary>
  /// Converts this BFloat8 to a double-precision float.
  /// </summary>
  public double ToDouble() => (double)this.ToHalf();

  /// <summary>
  /// Creates a BFloat8 from a Half by truncating the mantissa.
  /// </summary>
  public static BFloat8 FromHalf(Half value) {
    var bits = value.IsUInt16();
    // Half layout: S EEEEE MMMMMMMMMM
    // BFloat8 layout: S EEEEE MM
    var sign = (byte)((bits >> 15) << 7);
    var exp = (byte)(((bits >> 10) & 0x1F) << 2);
    var mant = (byte)((bits >> 8) & 0x03);
    return new((byte)(sign | exp | mant));
  }

  /// <summary>
  /// Creates a BFloat8 from a single-precision float.
  /// </summary>
  public static BFloat8 FromSingle(float value) => FromHalf((Half)value);

  /// <summary>
  /// Creates a BFloat8 from a double-precision float.
  /// </summary>
  public static BFloat8 FromDouble(double value) => FromHalf((Half)value);

  // Comparison
  public int CompareTo(BFloat8 other) {
    if (IsNaN(this) || IsNaN(other))
      return IsNaN(this) && IsNaN(other) ? 0 : IsNaN(this) ? 1 : -1;
    return this.ToSingle().CompareTo(other.ToSingle());
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not BFloat8 other)
      throw new ArgumentException("Object must be of type BFloat8.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(BFloat8 other) => this.RawValue == other.RawValue || (IsNaN(this) && IsNaN(other));

  public override bool Equals(object? obj) => obj is BFloat8 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToSingle().ToString(CultureInfo.InvariantCulture);

  public string ToString(IFormatProvider? provider) => this.ToSingle().ToString(provider);

  public string ToString(string? format) => this.ToSingle().ToString(format, CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? provider) => this.ToSingle().ToString(format, provider);

  // Operators
  public static bool operator ==(BFloat8 left, BFloat8 right) => left.Equals(right);
  public static bool operator !=(BFloat8 left, BFloat8 right) => !left.Equals(right);
  public static bool operator <(BFloat8 left, BFloat8 right) => left.CompareTo(right) < 0;
  public static bool operator >(BFloat8 left, BFloat8 right) => left.CompareTo(right) > 0;
  public static bool operator <=(BFloat8 left, BFloat8 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(BFloat8 left, BFloat8 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via float conversion)
  public static BFloat8 operator +(BFloat8 left, BFloat8 right) => FromSingle(left.ToSingle() + right.ToSingle());
  public static BFloat8 operator -(BFloat8 left, BFloat8 right) => FromSingle(left.ToSingle() - right.ToSingle());
  public static BFloat8 operator *(BFloat8 left, BFloat8 right) => FromSingle(left.ToSingle() * right.ToSingle());
  public static BFloat8 operator /(BFloat8 left, BFloat8 right) => FromSingle(left.ToSingle() / right.ToSingle());
  public static BFloat8 operator -(BFloat8 value) => new((byte)(value.RawValue ^ SignMask));
  public static BFloat8 operator +(BFloat8 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator %(BFloat8 left, BFloat8 right) => FromSingle(left.ToSingle() % right.ToSingle());

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator ++(BFloat8 value) => FromSingle(value.ToSingle() + 1f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator --(BFloat8 value) => FromSingle(value.ToSingle() - 1f);

  // Mixed-type arithmetic with float
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator *(BFloat8 left, float right) => FromSingle(left.ToSingle() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator *(float left, BFloat8 right) => FromSingle(left * right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator /(BFloat8 left, float right) => FromSingle(left.ToSingle() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator /(float left, BFloat8 right) => FromSingle(left / right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator +(BFloat8 left, float right) => FromSingle(left.ToSingle() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator +(float left, BFloat8 right) => FromSingle(left + right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator -(BFloat8 left, float right) => FromSingle(left.ToSingle() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator -(float left, BFloat8 right) => FromSingle(left - right.ToSingle());

  // Mixed-type arithmetic with int
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator *(BFloat8 left, int right) => FromSingle(left.ToSingle() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator *(int left, BFloat8 right) => FromSingle(left * right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator /(BFloat8 left, int right) => FromSingle(left.ToSingle() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator /(int left, BFloat8 right) => FromSingle(left / right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator +(BFloat8 left, int right) => FromSingle(left.ToSingle() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator +(int left, BFloat8 right) => FromSingle(left + right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator -(BFloat8 left, int right) => FromSingle(left.ToSingle() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 operator -(int left, BFloat8 right) => FromSingle(left - right.ToSingle());

  // Conversions
  public static explicit operator BFloat8(float value) => FromSingle(value);
  public static explicit operator BFloat8(double value) => FromDouble(value);
  public static explicit operator BFloat8(Half value) => FromHalf(value);
  public static explicit operator float(BFloat8 value) => value.ToSingle();
  public static explicit operator double(BFloat8 value) => value.ToDouble();
  public static explicit operator Half(BFloat8 value) => value.ToHalf();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 Abs(BFloat8 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 Min(BFloat8 left, BFloat8 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToSingle() <= right.ToSingle() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 Max(BFloat8 left, BFloat8 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToSingle() >= right.ToSingle() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat8 CopySign(BFloat8 value, BFloat8 sign) {
    var valueBits = (byte)(value.RawValue & ~SignMask);
    var signBit = (byte)(sign.RawValue & SignMask);
    return new((byte)(valueBits | signBit));
  }

  // Parsing
  public static BFloat8 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static BFloat8 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static BFloat8 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = float.Parse(s, style, provider);
    return FromSingle(value);
  }

  public static bool TryParse(string? s, out BFloat8 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out BFloat8 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out BFloat8 result) {
    if (float.TryParse(s, style, provider, out var value)) {
      result = FromSingle(value);
      return true;
    }
    result = Zero;
    return false;
  }

}

// Helper extension for Half bit manipulation
file static class BFloat8HalfExtensions {
  public static ushort IsUInt16(this Half value) {
    var bytes = BitConverter.GetBytes((float)value);
    // Get the Half representation from float (this is a simplification)
    // In reality, we need proper Half bit extraction
    unsafe {
      var halfValue = value;
      return *(ushort*)&halfValue;
    }
  }

  public static Half IsHalf(this ushort bits) {
    unsafe {
      return *(Half*)&bits;
    }
  }
}
