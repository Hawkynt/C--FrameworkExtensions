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
using System.Numerics;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// An 8-bit decimal floating-point value of the form value = (-1)^sign * coefficient * 10^(exponent - bias).
/// Layout: bit7 = sign; bits4-6 = 3-bit biased exponent (excess-4); bits0-3 = 4-bit coefficient (0-15).
/// </summary>
/// <remarks>
/// This is NOT a standard IEEE-754 decimal format (IEEE defines none below 32 bits) — it is a compact custom
/// layout. Arithmetic is nonetheless exact in base 10 (via the shared <see cref="DecimalFloatMath"/> engine),
/// then refit into the 4-bit coefficient field, so e.g. 0.1 + 0.2 == 0.3. Bias = 4, coefficient 0..15,
/// unbiased exponent -4..3. There is no infinity/NaN encoding; overflow saturates.
/// </remarks>
public readonly struct Decimal8 : IComparable, IComparable<Decimal8>, IEquatable<Decimal8>, IFormattable, ISpanFormattable, IParsable<Decimal8>, ISpanParsable<Decimal8> {

  private const int ExponentBits = 3;
  private const int CoefficientBits = 4;
  private const int ExponentBias = 4;
  private const int CoefficientMask = (1 << CoefficientBits) - 1; // 0x0F
  private const int ExponentMask = (1 << ExponentBits) - 1; // 0x07
  private const int SignMask = 1 << (ExponentBits + CoefficientBits); // 0x80
  private const int MaxCoefficient = CoefficientMask; // 15
  private const int MinUnbiasedExponent = -ExponentBias; // -4
  private const int MaxUnbiasedExponent = ExponentMask - ExponentBias; // 3

  // The element format only constrains the field widths; arithmetic precision/guard digits come from D32.
  private static readonly DecimalFormat _arith = DecimalFormat.D32;

  /// <summary>Gets the raw bit representation.</summary>
  public byte RawValue { get; }

  private Decimal8(byte raw) => this.RawValue = raw;

  /// <summary>Creates a <see cref="Decimal8"/> from the raw bit representation.</summary>
  public static Decimal8 FromRaw(byte raw) => new(raw);

  private int SignField => (this.RawValue >> (ExponentBits + CoefficientBits)) & 1;
  private int ExponentField => (this.RawValue >> CoefficientBits) & ExponentMask;
  private int CoefficientField => this.RawValue & CoefficientMask;

  private DecimalFloatMath.Value Value => new(this.SignField, this.CoefficientField, this.ExponentField - ExponentBias, DecimalKind.Finite);

  private static Decimal8 Encode(int sign, int coefficient, int unbiasedExponent) {
    var biasedExp = unbiasedExponent + ExponentBias;
    return new((byte)(((sign & 1) << (ExponentBits + CoefficientBits)) | ((biasedExp & ExponentMask) << CoefficientBits) | (coefficient & CoefficientMask)));
  }

  private static Decimal8 FromValue(DecimalFloatMath.Value v) {
    if (v.Kind == DecimalKind.NaN)
      return Zero;
    if (v.Kind == DecimalKind.Infinity)
      return v.Sign == 0 ? MaxValue : MinValue; // no infinity encoding: saturate
    DecimalFloatMath.FitToField(v, MaxCoefficient, MinUnbiasedExponent, MaxUnbiasedExponent, out var sign, out var coeff, out var exp);
    return Encode(sign, (int)coeff, exp);
  }

  public static Decimal8 Zero => new(0);
  public static Decimal8 One => Encode(0, 1, 0);
  public static Decimal8 MaxValue => Encode(0, MaxCoefficient, MaxUnbiasedExponent);
  public static Decimal8 MinValue => Encode(1, MaxCoefficient, MaxUnbiasedExponent);

  /// <summary>Gets the integer coefficient (0..15).</summary>
  public int Coefficient => this.CoefficientField;

  /// <summary>Gets the unbiased decimal exponent.</summary>
  public int Exponent => this.ExponentField - ExponentBias;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(Decimal8 value) => value.SignField != 0 && !value.Value.IsZero;

  public double ToDouble() => DecimalFloatMath.ToDouble(this.Value);
  public float ToSingle() => (float)this.ToDouble();

  public static Decimal8 FromDouble(double value) {
    if (double.IsNaN(value))
      return Zero;
    if (double.IsPositiveInfinity(value))
      return MaxValue;
    if (double.IsNegativeInfinity(value))
      return MinValue;
    return FromValue(DecimalFloatMath.Parse(value.ToString("R", CultureInfo.InvariantCulture), _arith));
  }

  public static Decimal8 FromSingle(float value) => FromDouble(value);

  // Comparison (exact, base-10)
  public int CompareTo(Decimal8 other) => DecimalFloatMath.Compare(this.Value, other.Value, _arith);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Decimal8 other)
      throw new ArgumentException("Object must be of type Decimal8.", nameof(obj));
    return this.CompareTo(other);
  }

  public bool Equals(Decimal8 other) => this.CompareTo(other) == 0;

  public override bool Equals(object? obj) => obj is Decimal8 other && this.Equals(other);

  public override int GetHashCode() => this.ToDouble().GetHashCode();

  public override string ToString() => DecimalFloatMath.ToString(this.Value);
  public string ToString(IFormatProvider? provider) => this.ToString();
  public string ToString(string? format) => this.ToString();
  public string ToString(string? format, IFormatProvider? provider) => this.ToString();

  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
    var str = this.ToString();
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }

    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  public static bool operator ==(Decimal8 left, Decimal8 right) => left.Equals(right);
  public static bool operator !=(Decimal8 left, Decimal8 right) => !left.Equals(right);
  public static bool operator <(Decimal8 left, Decimal8 right) => left.CompareTo(right) < 0;
  public static bool operator >(Decimal8 left, Decimal8 right) => left.CompareTo(right) > 0;
  public static bool operator <=(Decimal8 left, Decimal8 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(Decimal8 left, Decimal8 right) => left.CompareTo(right) >= 0;

  // Arithmetic: exact base-10 via the engine, refit into the 4-bit coefficient field.
  public static Decimal8 operator +(Decimal8 a, Decimal8 b) => FromValue(DecimalFloatMath.Add(a.Value, b.Value, _arith));
  public static Decimal8 operator -(Decimal8 a, Decimal8 b) => FromValue(DecimalFloatMath.Subtract(a.Value, b.Value, _arith));
  public static Decimal8 operator *(Decimal8 a, Decimal8 b) => FromValue(DecimalFloatMath.Multiply(a.Value, b.Value, _arith));
  public static Decimal8 operator /(Decimal8 a, Decimal8 b) => FromValue(DecimalFloatMath.Divide(a.Value, b.Value, _arith));
  public static Decimal8 operator -(Decimal8 value) => new((byte)(value.RawValue ^ SignMask));
  public static Decimal8 operator +(Decimal8 value) => value;
  public static Decimal8 operator ++(Decimal8 value) => value + One;
  public static Decimal8 operator --(Decimal8 value) => value - One;

  public static explicit operator Decimal8(float value) => FromSingle(value);
  public static explicit operator Decimal8(double value) => FromDouble(value);
  public static implicit operator float(Decimal8 value) => value.ToSingle();
  public static implicit operator double(Decimal8 value) => value.ToDouble();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal8 Abs(Decimal8 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal8 Min(Decimal8 left, Decimal8 right) => left <= right ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal8 Max(Decimal8 left, Decimal8 right) => left >= right ? left : right;

  // Parsing
  public static Decimal8 Parse(string s) => FromValue(DecimalFloatMath.Parse(s, _arith));
  public static Decimal8 Parse(string s, IFormatProvider? provider) => Parse(s);
  public static Decimal8 Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s);
  public static Decimal8 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s.ToString());

  public static bool TryParse(string? s, out Decimal8 result) => TryParse(s, null, out result);
  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Decimal8 result) => TryParse(s, provider, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Decimal8 result) {
    try {
      result = s == null ? Zero : Parse(s);
      return s != null;
    } catch (FormatException) {
      result = Zero;
      return false;
    }
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Decimal8 result) => TryParse(s.ToString(), provider, out result);
}
