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
/// Represents a 64-bit simplified decimal floating-point value of the form
/// value = (-1)^sign * coefficient * 10^(exponent - bias).
/// Layout: bit63 = sign; bits53-62 = 10-bit biased exponent (excess-512, so unbiased = exp-512); bits0-52 = 53-bit coefficient (0-9007199254740991).
/// </summary>
/// <remarks>
/// This is NOT a standard IEEE-754-2008 decimal interchange format (no BID/DPD declet encoding).
/// It is a simple, self-consistent sign + biased-exponent + integer-coefficient layout intended for
/// round-trip storage of small decimal values. Bias = 512. The coefficient ranges 0..9007199254740991
/// and the unbiased exponent ranges -512..511. Primary conversions are <see cref="ToDouble"/>/<see cref="FromDouble"/>.
/// </remarks>
public readonly struct Decimal64 : IComparable, IComparable<Decimal64>, IEquatable<Decimal64>, IFormattable, ISpanFormattable, IParsable<Decimal64>, ISpanParsable<Decimal64> {

  private const int SignBits = 1;
  private const int ExponentBits = 10;
  private const int CoefficientBits = 53;
  private const int ExponentBias = 512;
  private const ulong CoefficientMask = (1ul << CoefficientBits) - 1; // 2^53-1
  private const int ExponentMask = (1 << ExponentBits) - 1; // 0x3FF
  private const ulong SignMask = 1ul << (ExponentBits + CoefficientBits); // 0x8000000000000000

  private const long MaxCoefficient = (long)CoefficientMask; // 9007199254740991
  private const int MinUnbiasedExponent = -ExponentBias; // -512
  private const int MaxUnbiasedExponent = ExponentMask - ExponentBias; // 511

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public ulong RawValue { get; }

  private Decimal64(ulong raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="Decimal64"/> from the raw bit representation.
  /// </summary>
  public static Decimal64 FromRaw(ulong raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static Decimal64 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0 (coefficient=1, unbiased exponent=0).
  /// </summary>
  public static Decimal64 One => Encode(0, 1, 0);

  /// <summary>
  /// Gets the maximum finite positive value (coefficient=9007199254740991, max exponent).
  /// </summary>
  public static Decimal64 MaxValue => Encode(0, MaxCoefficient, MaxUnbiasedExponent);

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static Decimal64 MinValue => Encode(1, MaxCoefficient, MaxUnbiasedExponent);

  private int SignField => (int)((this.RawValue >> (ExponentBits + CoefficientBits)) & 1);
  private int ExponentField => (int)((this.RawValue >> CoefficientBits) & ExponentMask);
  private long CoefficientField => (long)(this.RawValue & CoefficientMask);

  /// <summary>
  /// Gets the integer coefficient (significand) in the range 0..9007199254740991.
  /// </summary>
  public long Coefficient => this.CoefficientField;

  /// <summary>
  /// Gets the unbiased decimal exponent (power of ten).
  /// </summary>
  public int Exponent => this.ExponentField - ExponentBias;

  /// <summary>
  /// Returns true if this value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(Decimal64 value) => value.SignField != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Decimal64 Encode(int sign, long coefficient, int unbiasedExponent) {
    var biasedExp = (ulong)(unbiasedExponent + ExponentBias);
    var raw = ((ulong)(sign & 1) << (ExponentBits + CoefficientBits)) | ((biasedExp & ExponentMask) << CoefficientBits) | ((ulong)coefficient & CoefficientMask);
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
  /// Creates a <see cref="Decimal64"/> from a double-precision float, choosing the exponent and integer
  /// coefficient that best represent the value, rounding to nearest and saturating on overflow.
  /// </summary>
  public static Decimal64 FromDouble(double value) {
    if (double.IsNaN(value))
      return Zero;
    if (double.IsPositiveInfinity(value))
      return MaxValue;
    if (double.IsNegativeInfinity(value))
      return MinValue;

    var sign = value < 0 || (value == 0 && 1 / value < 0) ? 1 : 0;
    var magnitude = Math.Abs(value);

    if (magnitude == 0)
      return new((ulong)sign << (ExponentBits + CoefficientBits));

    return Decimal8.EncodeMagnitude64(sign, magnitude, MaxCoefficient, MinUnbiasedExponent, MaxUnbiasedExponent, Encode);
  }

  /// <summary>
  /// Creates a <see cref="Decimal64"/> from a single-precision float.
  /// </summary>
  public static Decimal64 FromSingle(float value) => FromDouble(value);

  // Comparison
  public int CompareTo(Decimal64 other) => this.ToDouble().CompareTo(other.ToDouble());

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Decimal64 other)
      throw new ArgumentException("Object must be of type Decimal64.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Decimal64 other) => this.ToDouble().Equals(other.ToDouble());

  public override bool Equals(object? obj) => obj is Decimal64 other && this.Equals(other);

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
  public static bool operator ==(Decimal64 left, Decimal64 right) => left.Equals(right);
  public static bool operator !=(Decimal64 left, Decimal64 right) => !left.Equals(right);
  public static bool operator <(Decimal64 left, Decimal64 right) => left.CompareTo(right) < 0;
  public static bool operator >(Decimal64 left, Decimal64 right) => left.CompareTo(right) > 0;
  public static bool operator <=(Decimal64 left, Decimal64 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(Decimal64 left, Decimal64 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via double conversion)
  public static Decimal64 operator +(Decimal64 left, Decimal64 right) => FromDouble(left.ToDouble() + right.ToDouble());
  public static Decimal64 operator -(Decimal64 left, Decimal64 right) => FromDouble(left.ToDouble() - right.ToDouble());
  public static Decimal64 operator *(Decimal64 left, Decimal64 right) => FromDouble(left.ToDouble() * right.ToDouble());
  public static Decimal64 operator /(Decimal64 left, Decimal64 right) => FromDouble(left.ToDouble() / right.ToDouble());
  public static Decimal64 operator %(Decimal64 left, Decimal64 right) => FromDouble(left.ToDouble() % right.ToDouble());
  public static Decimal64 operator -(Decimal64 value) => new(value.RawValue ^ SignMask);
  public static Decimal64 operator +(Decimal64 value) => value;
  public static Decimal64 operator ++(Decimal64 value) => FromDouble(value.ToDouble() + 1d);
  public static Decimal64 operator --(Decimal64 value) => FromDouble(value.ToDouble() - 1d);

  // Conversions
  public static explicit operator Decimal64(float value) => FromSingle(value);
  public static explicit operator Decimal64(double value) => FromDouble(value);
  public static implicit operator float(Decimal64 value) => value.ToSingle();
  public static implicit operator double(Decimal64 value) => value.ToDouble();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal64 Abs(Decimal64 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal64 Min(Decimal64 left, Decimal64 right) => left.ToDouble() <= right.ToDouble() ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal64 Max(Decimal64 left, Decimal64 right) => left.ToDouble() >= right.ToDouble() ? left : right;

  // Parsing
  public static Decimal64 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static Decimal64 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static Decimal64 Parse(string s, NumberStyles style, IFormatProvider? provider) => FromDouble(double.Parse(s, style, provider));

  public static bool TryParse(string? s, out Decimal64 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Decimal64 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Decimal64 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }

    result = Zero;
    return false;
  }

  public static Decimal64 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromDouble(double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Decimal64 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }

    result = Zero;
    return false;
  }

}
