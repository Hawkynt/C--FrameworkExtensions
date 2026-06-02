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
/// Represents a 32-bit simplified decimal floating-point value of the form
/// value = (-1)^sign * coefficient * 10^(exponent - bias).
/// Layout: bit31 = sign; bits23-30 = 8-bit biased exponent (excess-128, so unbiased = exp-128); bits0-22 = 23-bit coefficient (0-8388607).
/// </summary>
/// <remarks>
/// This is NOT a standard IEEE-754-2008 decimal interchange format (no BID/DPD declet encoding).
/// It is a simple, self-consistent sign + biased-exponent + integer-coefficient layout intended for
/// round-trip storage of small decimal values. Bias = 128. The coefficient ranges 0..8388607 and the
/// unbiased exponent ranges -128..127.
/// </remarks>
public readonly struct Decimal32 : IComparable, IComparable<Decimal32>, IEquatable<Decimal32>, IFormattable, ISpanFormattable, IParsable<Decimal32>, ISpanParsable<Decimal32> {

  private const int SignBits = 1;
  private const int ExponentBits = 8;
  private const int CoefficientBits = 23;
  private const int ExponentBias = 128;
  private const uint CoefficientMask = (1u << CoefficientBits) - 1; // 0x7FFFFF
  private const int ExponentMask = (1 << ExponentBits) - 1; // 0xFF
  private const uint SignMask = 1u << (ExponentBits + CoefficientBits); // 0x80000000

  private const int MaxCoefficient = (int)CoefficientMask; // 8388607
  private const int MinUnbiasedExponent = -ExponentBias; // -128
  private const int MaxUnbiasedExponent = ExponentMask - ExponentBias; // 127

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public uint RawValue { get; }

  private Decimal32(uint raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="Decimal32"/> from the raw bit representation.
  /// </summary>
  public static Decimal32 FromRaw(uint raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static Decimal32 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0 (coefficient=1, unbiased exponent=0).
  /// </summary>
  public static Decimal32 One => Encode(0, 1, 0);

  /// <summary>
  /// Gets the maximum finite positive value (coefficient=8388607, max exponent).
  /// </summary>
  public static Decimal32 MaxValue => Encode(0, MaxCoefficient, MaxUnbiasedExponent);

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static Decimal32 MinValue => Encode(1, MaxCoefficient, MaxUnbiasedExponent);

  private int SignField => (int)((this.RawValue >> (ExponentBits + CoefficientBits)) & 1);
  private int ExponentField => (int)((this.RawValue >> CoefficientBits) & ExponentMask);
  private int CoefficientField => (int)(this.RawValue & CoefficientMask);

  /// <summary>
  /// Gets the integer coefficient (significand) in the range 0..8388607.
  /// </summary>
  public int Coefficient => this.CoefficientField;

  /// <summary>
  /// Gets the unbiased decimal exponent (power of ten).
  /// </summary>
  public int Exponent => this.ExponentField - ExponentBias;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(Decimal32 value) => value.SignField != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Decimal32 Encode(int sign, int coefficient, int unbiasedExponent) {
    var biasedExp = (uint)(unbiasedExponent + ExponentBias);
    var raw = ((uint)(sign & 1) << (ExponentBits + CoefficientBits)) | ((biasedExp & ExponentMask) << CoefficientBits) | ((uint)coefficient & CoefficientMask);
    return new(raw);
  }

  /// <summary>
  /// Converts this value to a double-precision float.
  /// </summary>
  public double ToDouble() {
    var value = this.CoefficientField * Math.Pow(10, this.ExponentField - ExponentBias);
    return this.SignField == 0 ? value : -value;
  }

  /// <summary>
  /// Converts this value to a single-precision float.
  /// </summary>
  public float ToSingle() => (float)this.ToDouble();

  /// <summary>
  /// Creates a <see cref="Decimal32"/> from a double-precision float, choosing the exponent and integer
  /// coefficient that best represent the value, rounding to nearest and saturating on overflow.
  /// </summary>
  public static Decimal32 FromDouble(double value) {
    if (double.IsNaN(value))
      return Zero;
    if (double.IsPositiveInfinity(value))
      return MaxValue;
    if (double.IsNegativeInfinity(value))
      return MinValue;

    var sign = value < 0 || (value == 0 && 1 / value < 0) ? 1 : 0;
    var magnitude = Math.Abs(value);

    if (magnitude == 0)
      return new((uint)sign << (ExponentBits + CoefficientBits));

    return Decimal8.EncodeMagnitude(sign, magnitude, MaxCoefficient, MinUnbiasedExponent, MaxUnbiasedExponent, Encode);
  }

  /// <summary>
  /// Creates a <see cref="Decimal32"/> from a single-precision float.
  /// </summary>
  public static Decimal32 FromSingle(float value) => FromDouble(value);

  // Comparison
  public int CompareTo(Decimal32 other) => this.ToDouble().CompareTo(other.ToDouble());

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Decimal32 other)
      throw new ArgumentException("Object must be of type Decimal32.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Decimal32 other) => this.ToDouble().Equals(other.ToDouble());

  public override bool Equals(object? obj) => obj is Decimal32 other && this.Equals(other);

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
  public static bool operator ==(Decimal32 left, Decimal32 right) => left.Equals(right);
  public static bool operator !=(Decimal32 left, Decimal32 right) => !left.Equals(right);
  public static bool operator <(Decimal32 left, Decimal32 right) => left.CompareTo(right) < 0;
  public static bool operator >(Decimal32 left, Decimal32 right) => left.CompareTo(right) > 0;
  public static bool operator <=(Decimal32 left, Decimal32 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(Decimal32 left, Decimal32 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via double conversion)
  public static Decimal32 operator +(Decimal32 left, Decimal32 right) => FromDouble(left.ToDouble() + right.ToDouble());
  public static Decimal32 operator -(Decimal32 left, Decimal32 right) => FromDouble(left.ToDouble() - right.ToDouble());
  public static Decimal32 operator *(Decimal32 left, Decimal32 right) => FromDouble(left.ToDouble() * right.ToDouble());
  public static Decimal32 operator /(Decimal32 left, Decimal32 right) => FromDouble(left.ToDouble() / right.ToDouble());
  public static Decimal32 operator %(Decimal32 left, Decimal32 right) => FromDouble(left.ToDouble() % right.ToDouble());
  public static Decimal32 operator -(Decimal32 value) => new(value.RawValue ^ SignMask);
  public static Decimal32 operator +(Decimal32 value) => value;
  public static Decimal32 operator ++(Decimal32 value) => FromDouble(value.ToDouble() + 1d);
  public static Decimal32 operator --(Decimal32 value) => FromDouble(value.ToDouble() - 1d);

  // Conversions
  public static explicit operator Decimal32(float value) => FromSingle(value);
  public static explicit operator Decimal32(double value) => FromDouble(value);
  public static implicit operator float(Decimal32 value) => value.ToSingle();
  public static implicit operator double(Decimal32 value) => value.ToDouble();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal32 Abs(Decimal32 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal32 Min(Decimal32 left, Decimal32 right) => left.ToDouble() <= right.ToDouble() ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal32 Max(Decimal32 left, Decimal32 right) => left.ToDouble() >= right.ToDouble() ? left : right;

  // Parsing
  public static Decimal32 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static Decimal32 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static Decimal32 Parse(string s, NumberStyles style, IFormatProvider? provider) => FromDouble(double.Parse(s, style, provider));

  public static bool TryParse(string? s, out Decimal32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Decimal32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Decimal32 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }

    result = Zero;
    return false;
  }

  public static Decimal32 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromDouble(double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Decimal32 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }

    result = Zero;
    return false;
  }

}
