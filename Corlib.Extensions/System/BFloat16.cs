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
/// Represents a 16-bit Brain Float (1 sign + 8 exponent + 7 mantissa bits).
/// BFloat16 is the upper 16 bits of an IEEE 754 single-precision float,
/// providing the same dynamic range as float32 with reduced precision.
/// </summary>
public readonly struct BFloat16 : IComparable, IComparable<BFloat16>, IEquatable<BFloat16>, IFormattable, IParsable<BFloat16> {

  private const int SignBits = 1;
  private const int ExponentBits = 8;
  private const int MantissaBits = 7;
  private const int ExponentBias = 127;
  private const int ExponentMask = (1 << ExponentBits) - 1; // 0xFF
  private const int MantissaMask = (1 << MantissaBits) - 1; // 0x7F
  private const int SignMask = 1 << (ExponentBits + MantissaBits); // 0x8000

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public ushort RawValue { get; }

  private BFloat16(ushort raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a BFloat16 from the raw bit representation.
  /// </summary>
  public static BFloat16 FromRaw(ushort raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static BFloat16 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0.
  /// </summary>
  public static BFloat16 One => new((ushort)(ExponentBias << MantissaBits));

  /// <summary>
  /// Gets the smallest positive subnormal value.
  /// </summary>
  public static BFloat16 Epsilon => new(1);

  /// <summary>
  /// Gets positive infinity.
  /// </summary>
  public static BFloat16 PositiveInfinity => new((ushort)(ExponentMask << MantissaBits));

  /// <summary>
  /// Gets negative infinity.
  /// </summary>
  public static BFloat16 NegativeInfinity => new((ushort)(SignMask | (ExponentMask << MantissaBits)));

  /// <summary>
  /// Gets a NaN value.
  /// </summary>
  public static BFloat16 NaN => new((ushort)((ExponentMask << MantissaBits) | 1));

  /// <summary>
  /// Gets the maximum finite positive value.
  /// </summary>
  public static BFloat16 MaxValue => new((ushort)(((ExponentMask - 1) << MantissaBits) | MantissaMask));

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static BFloat16 MinValue => new((ushort)(SignMask | ((ExponentMask - 1) << MantissaBits) | MantissaMask));

  // Component extraction
  private int Sign => (this.RawValue >> (ExponentBits + MantissaBits)) & 1;
  private int Exponent => (this.RawValue >> MantissaBits) & ExponentMask;
  private int Mantissa => this.RawValue & MantissaMask;

  /// <summary>
  /// Returns true if this value is NaN.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(BFloat16 value) => value.Exponent == ExponentMask && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is positive or negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsInfinity(BFloat16 value) => value.Exponent == ExponentMask && value.Mantissa == 0;

  /// <summary>
  /// Returns true if this value is positive infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPositiveInfinity(BFloat16 value) => value.RawValue == PositiveInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegativeInfinity(BFloat16 value) => value.RawValue == NegativeInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is finite.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(BFloat16 value) => value.Exponent != ExponentMask;

  /// <summary>
  /// Returns true if this value is a normal number.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNormal(BFloat16 value) {
    var exp = value.Exponent;
    return exp != 0 && exp != ExponentMask;
  }

  /// <summary>
  /// Returns true if this value is subnormal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSubnormal(BFloat16 value) => value.Exponent == 0 && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(BFloat16 value) => value.Sign != 0;

  /// <summary>
  /// Converts this BFloat16 to a single-precision float.
  /// BFloat16 is the upper 16 bits of float32, so conversion is simple.
  /// </summary>
  public unsafe float ToSingle() {
    // BFloat16 is the upper 16 bits of float32
    var bits = (uint)this.RawValue << 16;
    return *(float*)&bits;
  }

  /// <summary>
  /// Converts this BFloat16 to a double-precision float.
  /// </summary>
  public double ToDouble() => this.ToSingle();

  /// <summary>
  /// Creates a BFloat16 from a single-precision float by truncating the lower 16 mantissa bits.
  /// </summary>
  public static unsafe BFloat16 FromSingle(float value) {
    var bits = *(uint*)&value;
    // Take upper 16 bits (truncation, not rounding)
    return new((ushort)(bits >> 16));
  }

  /// <summary>
  /// Creates a BFloat16 from a double-precision float.
  /// </summary>
  public static BFloat16 FromDouble(double value) => FromSingle((float)value);

  // Comparison
  public int CompareTo(BFloat16 other) {
    if (IsNaN(this) || IsNaN(other))
      return IsNaN(this) && IsNaN(other) ? 0 : IsNaN(this) ? 1 : -1;
    return this.ToSingle().CompareTo(other.ToSingle());
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not BFloat16 other)
      throw new ArgumentException("Object must be of type BFloat16.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(BFloat16 other) => this.RawValue == other.RawValue || (IsNaN(this) && IsNaN(other));

  public override bool Equals(object? obj) => obj is BFloat16 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToSingle().ToString(CultureInfo.InvariantCulture);

  public string ToString(IFormatProvider? provider) => this.ToSingle().ToString(provider);

  public string ToString(string? format) => this.ToSingle().ToString(format, CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? provider) => this.ToSingle().ToString(format, provider);

  // Operators
  public static bool operator ==(BFloat16 left, BFloat16 right) => left.Equals(right);
  public static bool operator !=(BFloat16 left, BFloat16 right) => !left.Equals(right);
  public static bool operator <(BFloat16 left, BFloat16 right) => left.CompareTo(right) < 0;
  public static bool operator >(BFloat16 left, BFloat16 right) => left.CompareTo(right) > 0;
  public static bool operator <=(BFloat16 left, BFloat16 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(BFloat16 left, BFloat16 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via float conversion)
  public static BFloat16 operator +(BFloat16 left, BFloat16 right) => FromSingle(left.ToSingle() + right.ToSingle());
  public static BFloat16 operator -(BFloat16 left, BFloat16 right) => FromSingle(left.ToSingle() - right.ToSingle());
  public static BFloat16 operator *(BFloat16 left, BFloat16 right) => FromSingle(left.ToSingle() * right.ToSingle());
  public static BFloat16 operator /(BFloat16 left, BFloat16 right) => FromSingle(left.ToSingle() / right.ToSingle());
  public static BFloat16 operator -(BFloat16 value) => new((ushort)(value.RawValue ^ SignMask));
  public static BFloat16 operator +(BFloat16 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator %(BFloat16 left, BFloat16 right) => FromSingle(left.ToSingle() % right.ToSingle());

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator ++(BFloat16 value) => FromSingle(value.ToSingle() + 1f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator --(BFloat16 value) => FromSingle(value.ToSingle() - 1f);

  // Mixed-type arithmetic with float
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator *(BFloat16 left, float right) => FromSingle(left.ToSingle() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator *(float left, BFloat16 right) => FromSingle(left * right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator /(BFloat16 left, float right) => FromSingle(left.ToSingle() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator /(float left, BFloat16 right) => FromSingle(left / right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator +(BFloat16 left, float right) => FromSingle(left.ToSingle() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator +(float left, BFloat16 right) => FromSingle(left + right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator -(BFloat16 left, float right) => FromSingle(left.ToSingle() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator -(float left, BFloat16 right) => FromSingle(left - right.ToSingle());

  // Mixed-type arithmetic with int
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator *(BFloat16 left, int right) => FromSingle(left.ToSingle() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator *(int left, BFloat16 right) => FromSingle(left * right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator /(BFloat16 left, int right) => FromSingle(left.ToSingle() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator /(int left, BFloat16 right) => FromSingle(left / right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator +(BFloat16 left, int right) => FromSingle(left.ToSingle() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator +(int left, BFloat16 right) => FromSingle(left + right.ToSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator -(BFloat16 left, int right) => FromSingle(left.ToSingle() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 operator -(int left, BFloat16 right) => FromSingle(left - right.ToSingle());

  // Conversions
  public static explicit operator BFloat16(float value) => FromSingle(value);
  public static explicit operator BFloat16(double value) => FromDouble(value);
  public static explicit operator float(BFloat16 value) => value.ToSingle();
  public static explicit operator double(BFloat16 value) => value.ToDouble();

  // Conversion from/to other float types
  public static explicit operator BFloat16(Half value) => FromSingle((float)value);
  public static explicit operator Half(BFloat16 value) => (Half)value.ToSingle();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 Abs(BFloat16 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 Min(BFloat16 left, BFloat16 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToSingle() <= right.ToSingle() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 Max(BFloat16 left, BFloat16 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToSingle() >= right.ToSingle() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat16 CopySign(BFloat16 value, BFloat16 sign) {
    var valueBits = (ushort)(value.RawValue & ~SignMask);
    var signBit = (ushort)(sign.RawValue & SignMask);
    return new((ushort)(valueBits | signBit));
  }

  // Parsing
  public static BFloat16 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static BFloat16 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static BFloat16 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = float.Parse(s, style, provider);
    return FromSingle(value);
  }

  public static bool TryParse(string? s, out BFloat16 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out BFloat16 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out BFloat16 result) {
    if (float.TryParse(s, style, provider, out var value)) {
      result = FromSingle(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
