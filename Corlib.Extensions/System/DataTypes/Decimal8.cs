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
/// Represents an 8-bit simplified decimal floating-point value of the form
/// value = (-1)^sign * coefficient * 10^(exponent - bias).
/// Layout: bit7 = sign; bits4-6 = 3-bit biased exponent (excess-4, so unbiased = exp-4); bits0-3 = 4-bit coefficient (0-15).
/// </summary>
/// <remarks>
/// This is NOT a standard IEEE-754-2008 decimal interchange format (no BID/DPD declet encoding).
/// It is a simple, self-consistent sign + biased-exponent + integer-coefficient layout intended for
/// round-trip storage of small decimal values. Bias = 4. The coefficient ranges 0..15 and the
/// unbiased exponent ranges -4..3.
/// </remarks>
public readonly struct Decimal8 : IComparable, IComparable<Decimal8>, IEquatable<Decimal8>, IFormattable, ISpanFormattable, IParsable<Decimal8>, ISpanParsable<Decimal8> {

  private const int SignBits = 1;
  private const int ExponentBits = 3;
  private const int CoefficientBits = 4;
  private const int ExponentBias = 4;
  private const int CoefficientMask = (1 << CoefficientBits) - 1; // 0x0F
  private const int ExponentMask = (1 << ExponentBits) - 1; // 0x07
  private const int SignMask = 1 << (ExponentBits + CoefficientBits); // 0x80

  private const int MaxCoefficient = CoefficientMask; // 15
  private const int MinUnbiasedExponent = -ExponentBias; // -4
  private const int MaxUnbiasedExponent = ExponentMask - ExponentBias; // 3

  /// <summary>
  /// Gets the raw bit representation.
  /// </summary>
  public byte RawValue { get; }

  private Decimal8(byte raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="Decimal8"/> from the raw bit representation.
  /// </summary>
  public static Decimal8 FromRaw(byte raw) => new(raw);

  /// <summary>
  /// Gets positive zero.
  /// </summary>
  public static Decimal8 Zero => new(0);

  /// <summary>
  /// Gets the value 1.0 (coefficient=1, unbiased exponent=0).
  /// </summary>
  public static Decimal8 One => Encode(0, 1, 0);

  /// <summary>
  /// Gets the maximum finite positive value (coefficient=15, max exponent).
  /// </summary>
  public static Decimal8 MaxValue => Encode(0, MaxCoefficient, MaxUnbiasedExponent);

  /// <summary>
  /// Gets the minimum finite value (most negative).
  /// </summary>
  public static Decimal8 MinValue => Encode(1, MaxCoefficient, MaxUnbiasedExponent);

  private int SignField => (this.RawValue >> (ExponentBits + CoefficientBits)) & 1;
  private int ExponentField => (this.RawValue >> CoefficientBits) & ExponentMask;
  private int CoefficientField => this.RawValue & CoefficientMask;

  /// <summary>
  /// Gets the integer coefficient (significand) in the range 0..15.
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
  public static bool IsNegative(Decimal8 value) => value.SignField != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Decimal8 Encode(int sign, int coefficient, int unbiasedExponent) {
    var biasedExp = unbiasedExponent + ExponentBias;
    var raw = (byte)(((sign & 1) << (ExponentBits + CoefficientBits)) | ((biasedExp & ExponentMask) << CoefficientBits) | (coefficient & CoefficientMask));
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
  /// Creates a <see cref="Decimal8"/> from a double-precision float, choosing the exponent and integer
  /// coefficient that best represent the value, rounding to nearest and saturating on overflow.
  /// </summary>
  public static Decimal8 FromDouble(double value) {
    if (double.IsNaN(value))
      return Zero;
    if (double.IsPositiveInfinity(value))
      return MaxValue;
    if (double.IsNegativeInfinity(value))
      return MinValue;

    var sign = value < 0 || (value == 0 && 1 / value < 0) ? 1 : 0;
    var magnitude = Math.Abs(value);

    if (magnitude == 0)
      return new((byte)(sign << (ExponentBits + CoefficientBits)));

    return EncodeMagnitude(sign, magnitude, MaxCoefficient, MinUnbiasedExponent, MaxUnbiasedExponent, Encode);
  }

  /// <summary>
  /// Creates a <see cref="Decimal8"/> from a single-precision float.
  /// </summary>
  public static Decimal8 FromSingle(float value) => FromDouble(value);

  internal static T EncodeMagnitude<T>(int sign, double magnitude, int maxCoefficient, int minExp, int maxExp, Func<int, int, int, T> encoder) {
    // Walk exponents from smallest (most precision) to largest. Pick the first exponent at which the
    // rounded coefficient fits the field; that yields the most precise representation. If no exponent
    // fits because the magnitude is too large, saturate at the maximum value.
    for (var e = minExp; e <= maxExp; ++e) {
      // Compare as a double BEFORE casting: at very negative exponents the quotient is astronomically
      // large and casting it to long would overflow to a garbage value that spuriously "fits".
      var coeff = Math.Round(magnitude / Math.Pow(10, e), MidpointRounding.AwayFromZero);
      if (coeff <= maxCoefficient)
        return encoder(sign, (int)coeff, e);
    }

    // Magnitude exceeds what the largest exponent can hold -> saturate.
    return encoder(sign, maxCoefficient, maxExp);
  }

  internal static T EncodeMagnitude64<T>(int sign, double magnitude, long maxCoefficient, int minExp, int maxExp, Func<int, long, int, T> encoder) {
    // Same strategy as EncodeMagnitude but with a 64-bit coefficient field.
    for (var e = minExp; e <= maxExp; ++e) {
      var scaled = Math.Round(magnitude / Math.Pow(10, e), MidpointRounding.AwayFromZero);
      if (scaled <= maxCoefficient)
        return encoder(sign, (long)scaled, e);
    }

    return encoder(sign, maxCoefficient, maxExp);
  }

  // Comparison
  public int CompareTo(Decimal8 other) => this.ToDouble().CompareTo(other.ToDouble());

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Decimal8 other)
      throw new ArgumentException("Object must be of type Decimal8.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Decimal8 other) => this.ToDouble().Equals(other.ToDouble());

  public override bool Equals(object? obj) => obj is Decimal8 other && this.Equals(other);

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
  public static bool operator ==(Decimal8 left, Decimal8 right) => left.Equals(right);
  public static bool operator !=(Decimal8 left, Decimal8 right) => !left.Equals(right);
  public static bool operator <(Decimal8 left, Decimal8 right) => left.CompareTo(right) < 0;
  public static bool operator >(Decimal8 left, Decimal8 right) => left.CompareTo(right) > 0;
  public static bool operator <=(Decimal8 left, Decimal8 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(Decimal8 left, Decimal8 right) => left.CompareTo(right) >= 0;

  // Arithmetic (via double conversion)
  public static Decimal8 operator +(Decimal8 left, Decimal8 right) => FromDouble(left.ToDouble() + right.ToDouble());
  public static Decimal8 operator -(Decimal8 left, Decimal8 right) => FromDouble(left.ToDouble() - right.ToDouble());
  public static Decimal8 operator *(Decimal8 left, Decimal8 right) => FromDouble(left.ToDouble() * right.ToDouble());
  public static Decimal8 operator /(Decimal8 left, Decimal8 right) => FromDouble(left.ToDouble() / right.ToDouble());
  public static Decimal8 operator %(Decimal8 left, Decimal8 right) => FromDouble(left.ToDouble() % right.ToDouble());
  public static Decimal8 operator -(Decimal8 value) => new((byte)(value.RawValue ^ SignMask));
  public static Decimal8 operator +(Decimal8 value) => value;
  public static Decimal8 operator ++(Decimal8 value) => FromDouble(value.ToDouble() + 1d);
  public static Decimal8 operator --(Decimal8 value) => FromDouble(value.ToDouble() - 1d);

  // Conversions
  public static explicit operator Decimal8(float value) => FromSingle(value);
  public static explicit operator Decimal8(double value) => FromDouble(value);
  public static implicit operator float(Decimal8 value) => value.ToSingle();
  public static implicit operator double(Decimal8 value) => value.ToDouble();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal8 Abs(Decimal8 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal8 Min(Decimal8 left, Decimal8 right) => left.ToDouble() <= right.ToDouble() ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal8 Max(Decimal8 left, Decimal8 right) => left.ToDouble() >= right.ToDouble() ? left : right;

  // Parsing
  public static Decimal8 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static Decimal8 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static Decimal8 Parse(string s, NumberStyles style, IFormatProvider? provider) => FromDouble(double.Parse(s, style, provider));

  public static bool TryParse(string? s, out Decimal8 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Decimal8 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Decimal8 result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }

    result = Zero;
    return false;
  }

  public static Decimal8 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromDouble(double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Decimal8 result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value);
      return true;
    }

    result = Zero;
    return false;
  }

}
