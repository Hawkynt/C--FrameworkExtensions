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
/// Represents a 32-bit Brain Float (1 sign + 11 exponent + 20 mantissa bits).
/// BFloat32 is the upper 32 bits of an IEEE 754 double-precision float,
/// providing the same dynamic range as double with reduced precision.
/// </summary>
public readonly struct BFloat32 : IComparable, IComparable<BFloat32>, IEquatable<BFloat32>, IFormattable, ISpanFormattable, IParsable<BFloat32>, ISpanParsable<BFloat32> {

  private const int SignBits = 1;
  private const int ExponentBits = 11;
  private const int MantissaBits = 20;
  private const int ExponentBias = 1023;
  private const int ExponentMask = (1 << ExponentBits) - 1; // 0x7FF
  private const int MantissaMask = (1 << MantissaBits) - 1; // 0xFFFFF
  private const uint SignMask = 1u << (ExponentBits + MantissaBits); // 0x80000000

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public uint RawValue { get; }

  private BFloat32(uint raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a BFloat32 from the raw bit representation.
  /// </summary>
  public static BFloat32 FromRaw(uint raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static BFloat32 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0.
  /// </summary>
  public static BFloat32 One => new((uint)ExponentBias << MantissaBits);

  /// <summary>
  /// Gets the smallest positive subnormal value.
  /// </summary>
  public static BFloat32 Epsilon => new(1);

  /// <summary>
  /// Gets positive infinity.
  /// </summary>
  public static BFloat32 PositiveInfinity => new((uint)ExponentMask << MantissaBits);

  /// <summary>
  /// Gets negative infinity.
  /// </summary>
  public static BFloat32 NegativeInfinity => new(SignMask | ((uint)ExponentMask << MantissaBits));

  /// <summary>
  /// Gets a NaN value.
  /// </summary>
  public static BFloat32 NaN => new(((uint)ExponentMask << MantissaBits) | 1);

  /// <summary>
  /// Gets the maximum finite positive value.
  /// </summary>
  public static BFloat32 MaxValue => new((((uint)ExponentMask - 1) << MantissaBits) | (uint)MantissaMask);

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static BFloat32 MinValue => new(SignMask | (((uint)ExponentMask - 1) << MantissaBits) | (uint)MantissaMask);

  // Component extraction
  private int Sign => (int)((this.RawValue >> (ExponentBits + MantissaBits)) & 1);
  private int Exponent => (int)((this.RawValue >> MantissaBits) & ExponentMask);
  private int Mantissa => (int)(this.RawValue & MantissaMask);

  /// <summary>
  /// Returns true if this value is NaN.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(BFloat32 value) => value.Exponent == ExponentMask && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is positive or negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsInfinity(BFloat32 value) => value.Exponent == ExponentMask && value.Mantissa == 0;

  /// <summary>
  /// Returns true if this value is positive infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPositiveInfinity(BFloat32 value) => value.RawValue == PositiveInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegativeInfinity(BFloat32 value) => value.RawValue == NegativeInfinity.RawValue;

  /// <summary>
  /// Returns true if this value is finite.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(BFloat32 value) => value.Exponent != ExponentMask;

  /// <summary>
  /// Returns true if this value is a normal number.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNormal(BFloat32 value) {
    var exp = value.Exponent;
    return exp != 0 && exp != ExponentMask;
  }

  /// <summary>
  /// Returns true if this value is subnormal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSubnormal(BFloat32 value) => value.Exponent == 0 && value.Mantissa != 0;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(BFloat32 value) => value.Sign != 0;

  /// <summary>
  /// Converts this BFloat32 to a double-precision float.
  /// BFloat32 is the upper 32 bits of double, so conversion expands the mantissa.
  /// </summary>
  public unsafe double ToDouble() {
    // BFloat32 is the upper 32 bits of double
    var bits = (ulong)this.RawValue << 32;
    return *(double*)&bits;
  }

  /// <summary>
  /// Converts this BFloat32 to a single-precision float.
  /// </summary>
  public float ToSingle() => (float)this.ToDouble();

  /// <summary>
  /// Creates a BFloat32 from a double-precision float by truncating the lower 32 mantissa bits.
  /// </summary>
  public static unsafe BFloat32 FromDouble(double value) {
    var bits = *(ulong*)&value;
    // Take upper 32 bits (truncation, not rounding)
    return new((uint)(bits >> 32));
  }

  /// <summary>
  /// Creates a BFloat32 from a single-precision float.
  /// </summary>
  public static BFloat32 FromSingle(float value) => FromDouble(value);

  // Comparison
  public int CompareTo(BFloat32 other) {
    if (IsNaN(this) || IsNaN(other))
      return IsNaN(this) && IsNaN(other) ? 0 : IsNaN(this) ? 1 : -1;
    return this.ToDouble().CompareTo(other.ToDouble());
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not BFloat32 other)
      throw new ArgumentException("Object must be of type BFloat32.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(BFloat32 other) => this.RawValue == other.RawValue || (IsNaN(this) && IsNaN(other));

  public override bool Equals(object? obj) => obj is BFloat32 other && this.Equals(other);

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
  public static bool operator ==(BFloat32 left, BFloat32 right) => left.Equals(right);
  public static bool operator !=(BFloat32 left, BFloat32 right) => !left.Equals(right);
  public static bool operator <(BFloat32 left, BFloat32 right) => left.CompareTo(right) < 0;
  public static bool operator >(BFloat32 left, BFloat32 right) => left.CompareTo(right) > 0;
  public static bool operator <=(BFloat32 left, BFloat32 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(BFloat32 left, BFloat32 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via double conversion)
  public static BFloat32 operator +(BFloat32 left, BFloat32 right) => FromDouble(left.ToDouble() + right.ToDouble());
  public static BFloat32 operator -(BFloat32 left, BFloat32 right) => FromDouble(left.ToDouble() - right.ToDouble());
  public static BFloat32 operator *(BFloat32 left, BFloat32 right) => FromDouble(left.ToDouble() * right.ToDouble());
  public static BFloat32 operator /(BFloat32 left, BFloat32 right) => FromDouble(left.ToDouble() / right.ToDouble());
  public static BFloat32 operator -(BFloat32 value) => new(value.RawValue ^ SignMask);
  public static BFloat32 operator +(BFloat32 value) => value;

  // Modulo
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator %(BFloat32 left, BFloat32 right) => FromDouble(left.ToDouble() % right.ToDouble());

  // Increment/Decrement
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator ++(BFloat32 value) => FromDouble(value.ToDouble() + 1.0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator --(BFloat32 value) => FromDouble(value.ToDouble() - 1.0);

  // Mixed-type arithmetic with double
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator *(BFloat32 left, double right) => FromDouble(left.ToDouble() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator *(double left, BFloat32 right) => FromDouble(left * right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator /(BFloat32 left, double right) => FromDouble(left.ToDouble() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator /(double left, BFloat32 right) => FromDouble(left / right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator +(BFloat32 left, double right) => FromDouble(left.ToDouble() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator +(double left, BFloat32 right) => FromDouble(left + right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator -(BFloat32 left, double right) => FromDouble(left.ToDouble() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator -(double left, BFloat32 right) => FromDouble(left - right.ToDouble());

  // Mixed-type arithmetic with int
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator *(BFloat32 left, int right) => FromDouble(left.ToDouble() * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator *(int left, BFloat32 right) => FromDouble(left * right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator /(BFloat32 left, int right) => FromDouble(left.ToDouble() / right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator /(int left, BFloat32 right) => FromDouble(left / right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator +(BFloat32 left, int right) => FromDouble(left.ToDouble() + right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator +(int left, BFloat32 right) => FromDouble(left + right.ToDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator -(BFloat32 left, int right) => FromDouble(left.ToDouble() - right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 operator -(int left, BFloat32 right) => FromDouble(left - right.ToDouble());

  // Conversions
  public static explicit operator BFloat32(float value) => FromSingle(value);
  public static explicit operator BFloat32(double value) => FromDouble(value);
  public static explicit operator float(BFloat32 value) => value.ToSingle();
  public static implicit operator double(BFloat32 value) => value.ToDouble();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 Abs(BFloat32 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 Min(BFloat32 left, BFloat32 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToDouble() <= right.ToDouble() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 Max(BFloat32 left, BFloat32 right) {
    if (IsNaN(left) || IsNaN(right))
      return NaN;
    return left.ToDouble() >= right.ToDouble() ? left : right;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BFloat32 CopySign(BFloat32 value, BFloat32 sign) {
    var valueBits = value.RawValue & ~SignMask;
    var signBit = sign.RawValue & SignMask;
    return new(valueBits | signBit);
  }

  // Parsing
  public static BFloat32 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static BFloat32 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static BFloat32 Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    return FromDouble(value);
  }

  public static bool TryParse(string? s, out BFloat32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out BFloat32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out BFloat32 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

  public static BFloat32 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
    var value = double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
    return FromDouble(value);
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BFloat32 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }
    result = Zero;
    return false;
  }

}
