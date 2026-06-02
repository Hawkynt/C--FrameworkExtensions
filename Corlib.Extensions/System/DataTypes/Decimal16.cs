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
/// A 16-bit decimal floating-point value of the form value = (-1)^sign * coefficient * 10^(exponent - bias).
/// Layout: bit15 = sign; bits10-14 = 5-bit biased exponent (excess-16); bits0-9 = 10-bit coefficient (0-1023).
/// </summary>
/// <remarks>
/// This is NOT a standard IEEE-754 decimal format (IEEE defines none below 32 bits) — it is a compact custom
/// layout. Arithmetic is nonetheless exact in base 10 (via the shared <see cref="DecimalFloatMath"/> engine),
/// then refit into the 10-bit coefficient field, so e.g. 0.1 + 0.2 == 0.3. Bias = 16, coefficient 0..1023,
/// unbiased exponent -16..15. There is no infinity/NaN encoding; overflow saturates.
/// </remarks>
public readonly struct Decimal16 : IComparable, IComparable<Decimal16>, IEquatable<Decimal16>, IFormattable, ISpanFormattable, IParsable<Decimal16>, ISpanParsable<Decimal16> {

  private const int ExponentBits = 5;
  private const int CoefficientBits = 10;
  private const int ExponentBias = 16;
  private const int CoefficientMask = (1 << CoefficientBits) - 1; // 0x3FF
  private const int ExponentMask = (1 << ExponentBits) - 1; // 0x1F
  private const int SignMask = 1 << (ExponentBits + CoefficientBits); // 0x8000
  private const int MaxCoefficient = CoefficientMask; // 1023
  private const int MinUnbiasedExponent = -ExponentBias; // -16
  private const int MaxUnbiasedExponent = ExponentMask - ExponentBias; // 15

  private static readonly DecimalFormat _arith = DecimalFormat.D32;

  /// <summary>Gets the raw bit representation.</summary>
  public ushort RawValue { get; }

  private Decimal16(ushort raw) => this.RawValue = raw;

  /// <summary>Creates a <see cref="Decimal16"/> from the raw bit representation.</summary>
  public static Decimal16 FromRaw(ushort raw) => new(raw);

  private int SignField => (this.RawValue >> (ExponentBits + CoefficientBits)) & 1;
  private int ExponentField => (this.RawValue >> CoefficientBits) & ExponentMask;
  private int CoefficientField => this.RawValue & CoefficientMask;

  private DecimalFloatMath.Value Value => new(this.SignField, this.CoefficientField, this.ExponentField - ExponentBias, DecimalKind.Finite);

  private static Decimal16 Encode(int sign, int coefficient, int unbiasedExponent) {
    var biasedExp = unbiasedExponent + ExponentBias;
    return new((ushort)(((sign & 1) << (ExponentBits + CoefficientBits)) | ((biasedExp & ExponentMask) << CoefficientBits) | (coefficient & CoefficientMask)));
  }

  private static Decimal16 FromValue(DecimalFloatMath.Value v) {
    if (v.Kind == DecimalKind.NaN)
      return Zero;
    if (v.Kind == DecimalKind.Infinity)
      return v.Sign == 0 ? MaxValue : MinValue; // no infinity encoding: saturate
    DecimalFloatMath.FitToField(v, MaxCoefficient, MinUnbiasedExponent, MaxUnbiasedExponent, out var sign, out var coeff, out var exp);
    return Encode(sign, (int)coeff, exp);
  }

  public static Decimal16 Zero => new(0);
  public static Decimal16 One => Encode(0, 1, 0);
  public static Decimal16 MaxValue => Encode(0, MaxCoefficient, MaxUnbiasedExponent);
  public static Decimal16 MinValue => Encode(1, MaxCoefficient, MaxUnbiasedExponent);

  /// <summary>Gets the integer coefficient (0..1023).</summary>
  public int Coefficient => this.CoefficientField;

  /// <summary>Gets the unbiased decimal exponent.</summary>
  public int Exponent => this.ExponentField - ExponentBias;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(Decimal16 value) => value.SignField != 0 && !value.Value.IsZero;

  public double ToDouble() => DecimalFloatMath.ToDouble(this.Value);
  public float ToSingle() => (float)this.ToDouble();

  public static Decimal16 FromDouble(double value) {
    if (double.IsNaN(value))
      return Zero;
    if (double.IsPositiveInfinity(value))
      return MaxValue;
    if (double.IsNegativeInfinity(value))
      return MinValue;
    return FromValue(DecimalFloatMath.Parse(value.ToString("R", CultureInfo.InvariantCulture), _arith));
  }

  public static Decimal16 FromSingle(float value) => FromDouble(value);

  public int CompareTo(Decimal16 other) => DecimalFloatMath.Compare(this.Value, other.Value, _arith);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Decimal16 other)
      throw new ArgumentException("Object must be of type Decimal16.", nameof(obj));
    return this.CompareTo(other);
  }

  public bool Equals(Decimal16 other) => this.CompareTo(other) == 0;

  public override bool Equals(object? obj) => obj is Decimal16 other && this.Equals(other);

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

  public static bool operator ==(Decimal16 left, Decimal16 right) => left.Equals(right);
  public static bool operator !=(Decimal16 left, Decimal16 right) => !left.Equals(right);
  public static bool operator <(Decimal16 left, Decimal16 right) => left.CompareTo(right) < 0;
  public static bool operator >(Decimal16 left, Decimal16 right) => left.CompareTo(right) > 0;
  public static bool operator <=(Decimal16 left, Decimal16 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(Decimal16 left, Decimal16 right) => left.CompareTo(right) >= 0;

  public static Decimal16 operator +(Decimal16 a, Decimal16 b) => FromValue(DecimalFloatMath.Add(a.Value, b.Value, _arith));
  public static Decimal16 operator -(Decimal16 a, Decimal16 b) => FromValue(DecimalFloatMath.Subtract(a.Value, b.Value, _arith));
  public static Decimal16 operator *(Decimal16 a, Decimal16 b) => FromValue(DecimalFloatMath.Multiply(a.Value, b.Value, _arith));
  public static Decimal16 operator /(Decimal16 a, Decimal16 b) => FromValue(DecimalFloatMath.Divide(a.Value, b.Value, _arith));
  public static Decimal16 operator -(Decimal16 value) => new((ushort)(value.RawValue ^ SignMask));
  public static Decimal16 operator +(Decimal16 value) => value;
  public static Decimal16 operator ++(Decimal16 value) => value + One;
  public static Decimal16 operator --(Decimal16 value) => value - One;

  public static explicit operator Decimal16(float value) => FromSingle(value);
  public static explicit operator Decimal16(double value) => FromDouble(value);
  public static implicit operator float(Decimal16 value) => value.ToSingle();
  public static implicit operator double(Decimal16 value) => value.ToDouble();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal16 Abs(Decimal16 value) => IsNegative(value) ? -value : value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal16 Min(Decimal16 left, Decimal16 right) => left <= right ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Decimal16 Max(Decimal16 left, Decimal16 right) => left >= right ? left : right;

  public static Decimal16 Parse(string s) => FromValue(DecimalFloatMath.Parse(s, _arith));
  public static Decimal16 Parse(string s, IFormatProvider? provider) => Parse(s);
  public static Decimal16 Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s);
  public static Decimal16 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s.ToString());

  public static bool TryParse(string? s, out Decimal16 result) => TryParse(s, null, out result);
  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Decimal16 result) => TryParse(s, provider, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out Decimal16 result) {
    try {
      result = s == null ? Zero : Parse(s);
      return s != null;
    } catch (FormatException) {
      result = Zero;
      return false;
    }
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Decimal16 result) => TryParse(s.ToString(), provider, out result);
}
