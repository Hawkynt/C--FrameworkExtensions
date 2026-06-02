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

namespace System;

/// <summary>
/// IEEE-754 decimal64: a 64-bit decimal floating-point number (16 significant decimal digits, exponent
/// 10^-383..10^384) in the BID (Binary Integer Decimal) encoding. Arithmetic is exact in base 10, so
/// 0.1 + 0.2 == 0.3 exactly. With 16 digits this is the practical choice for monetary/decimal computation.
/// </summary>
public readonly struct Decimal64 : IComparable, IComparable<Decimal64>, IEquatable<Decimal64>, IFormattable {

  private static readonly DecimalFormat _fmt = DecimalFormat.D64;
  private static readonly BigInteger _maxCoeff = BigInteger.Parse("9999999999999999", CultureInfo.InvariantCulture);

  /// <summary>Gets the raw 64-bit BID encoding.</summary>
  public ulong RawValue { get; }

  private Decimal64(ulong raw) => this.RawValue = raw;

  private Decimal64(DecimalFloatMath.Value v) => this.RawValue = (ulong)DecimalFloatMath.Encode(v, _fmt);

  private DecimalFloatMath.Value Value => DecimalFloatMath.Decode(this.RawValue, _fmt);

  /// <summary>Creates a Decimal64 from its raw BID encoding.</summary>
  public static Decimal64 FromRaw(ulong raw) => new(raw);

  public static Decimal64 Zero => new(new DecimalFloatMath.Value(0, BigInteger.Zero, 0, DecimalKind.Finite));
  public static Decimal64 One => new(new DecimalFloatMath.Value(0, BigInteger.One, 0, DecimalKind.Finite));
  public static Decimal64 MaxValue => new(new DecimalFloatMath.Value(0, _maxCoeff, 369, DecimalKind.Finite));
  public static Decimal64 MinValue => new(new DecimalFloatMath.Value(1, _maxCoeff, 369, DecimalKind.Finite));
  public static Decimal64 PositiveInfinity => new(new DecimalFloatMath.Value(0, BigInteger.Zero, 0, DecimalKind.Infinity));
  public static Decimal64 NegativeInfinity => new(new DecimalFloatMath.Value(1, BigInteger.Zero, 0, DecimalKind.Infinity));
  public static Decimal64 NaN => new(new DecimalFloatMath.Value(0, BigInteger.Zero, 0, DecimalKind.NaN));

  /// <summary>Gets the integer coefficient (significand) of the decoded value.</summary>
  public BigInteger Coefficient => this.Value.Coeff;

  /// <summary>Gets the base-10 exponent of the decoded value.</summary>
  public int Exponent => this.Value.Q;

  public static bool IsNaN(Decimal64 d) => d.Value.Kind == DecimalKind.NaN;
  public static bool IsInfinity(Decimal64 d) => d.Value.Kind == DecimalKind.Infinity;
  public static bool IsFinite(Decimal64 d) => d.Value.Kind == DecimalKind.Finite;
  public static bool IsNegative(Decimal64 d) => d.Value.Sign != 0 && d.Value.Kind != DecimalKind.NaN;

  public double ToDouble() => DecimalFloatMath.ToDouble(this.Value);
  public float ToSingle() => (float)this.ToDouble();

  public static Decimal64 FromDouble(double value) {
    if (double.IsNaN(value))
      return NaN;
    if (double.IsInfinity(value))
      return double.IsNegative(value) ? NegativeInfinity : PositiveInfinity;
    return new(DecimalFloatMath.Parse(value.ToString("R", CultureInfo.InvariantCulture), _fmt));
  }

  public static Decimal64 FromSingle(float value) => FromDouble(value);

  public static Decimal64 operator +(Decimal64 a, Decimal64 b) => new(DecimalFloatMath.Add(a.Value, b.Value, _fmt));
  public static Decimal64 operator -(Decimal64 a, Decimal64 b) => new(DecimalFloatMath.Subtract(a.Value, b.Value, _fmt));
  public static Decimal64 operator *(Decimal64 a, Decimal64 b) => new(DecimalFloatMath.Multiply(a.Value, b.Value, _fmt));
  public static Decimal64 operator /(Decimal64 a, Decimal64 b) => new(DecimalFloatMath.Divide(a.Value, b.Value, _fmt));
  public static Decimal64 operator -(Decimal64 a) => new(DecimalFloatMath.Negate(a.Value));
  public static Decimal64 operator +(Decimal64 a) => a;

  public int CompareTo(Decimal64 other) {
    DecimalFloatMath.Value a = this.Value, b = other.Value;
    if (a.Kind == DecimalKind.NaN || b.Kind == DecimalKind.NaN)
      return a.Kind == DecimalKind.NaN && b.Kind == DecimalKind.NaN ? 0 : a.Kind == DecimalKind.NaN ? 1 : -1;
    return DecimalFloatMath.Compare(a, b, _fmt);
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Decimal64 other)
      throw new ArgumentException("Object must be of type Decimal64.", nameof(obj));
    return this.CompareTo(other);
  }

  public bool Equals(Decimal64 other) => this.CompareTo(other) == 0 && !IsNaN(this);

  public override bool Equals(object? obj) => obj is Decimal64 other && this.Equals(other);

  public override int GetHashCode() => IsNaN(this) ? 0x7FC00000 : this.ToDouble().GetHashCode();

  public override string ToString() => DecimalFloatMath.ToString(this.Value);
  public string ToString(string? format, IFormatProvider? formatProvider) => DecimalFloatMath.ToString(this.Value);

  public static bool operator ==(Decimal64 a, Decimal64 b) => a.Equals(b);
  public static bool operator !=(Decimal64 a, Decimal64 b) => !a.Equals(b);
  public static bool operator <(Decimal64 a, Decimal64 b) => a.CompareTo(b) < 0;
  public static bool operator >(Decimal64 a, Decimal64 b) => a.CompareTo(b) > 0;
  public static bool operator <=(Decimal64 a, Decimal64 b) => a.CompareTo(b) <= 0;
  public static bool operator >=(Decimal64 a, Decimal64 b) => a.CompareTo(b) >= 0;

  public static explicit operator Decimal64(double value) => FromDouble(value);
  public static implicit operator double(Decimal64 value) => value.ToDouble();

  public static Decimal64 Parse(string s) => new(DecimalFloatMath.Parse(s, _fmt));
  public static Decimal64 Parse(string s, IFormatProvider? provider) => Parse(s);

  public static bool TryParse(string? s, out Decimal64 result) {
    try {
      result = s == null ? Zero : new(DecimalFloatMath.Parse(s, _fmt));
      return s != null;
    } catch (FormatException) {
      result = Zero;
      return false;
    }
  }

  public static bool TryParse(string? s, IFormatProvider? provider, out Decimal64 result) => TryParse(s, out result);
}
