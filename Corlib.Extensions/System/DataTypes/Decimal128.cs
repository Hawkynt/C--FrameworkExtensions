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
/// IEEE-754 decimal128: a 128-bit decimal floating-point number (34 significant decimal digits, exponent
/// 10^-6143..10^6144) in the BID (Binary Integer Decimal) encoding. Arithmetic is exact in base 10
/// (0.1 + 0.2 == 0.3). With 34 digits it covers the most demanding monetary/decimal needs.
/// </summary>
/// <remarks>
/// The 128-bit raw value is exposed as two 64-bit halves (<see cref="High"/>, <see cref="Low"/>) for
/// portability across target frameworks that lack a native 128-bit integer.
/// </remarks>
public readonly struct Decimal128 : IComparable, IComparable<Decimal128>, IEquatable<Decimal128>, IFormattable {

  private static readonly DecimalFormat _fmt = DecimalFormat.D128;
  private static readonly BigInteger _mask64 = (BigInteger.One << 64) - 1;
  private static readonly BigInteger _mask128 = (BigInteger.One << 128) - 1;
  private static readonly BigInteger _maxCoeff = BigInteger.Pow(10, 34) - 1;

  /// <summary>Gets the high 64 bits of the raw BID encoding.</summary>
  public ulong High { get; }

  /// <summary>Gets the low 64 bits of the raw BID encoding.</summary>
  public ulong Low { get; }

  private Decimal128(ulong high, ulong low) {
    this.High = high;
    this.Low = low;
  }

  private Decimal128(BigInteger raw) {
    raw &= _mask128;
    this.High = (ulong)(raw >> 64);
    this.Low = (ulong)(raw & _mask64);
  }

  private Decimal128(DecimalFloatMath.Value v) : this(DecimalFloatMath.Encode(v, _fmt)) { }

  private BigInteger Raw => ((BigInteger)this.High << 64) | this.Low;

  private DecimalFloatMath.Value Value => DecimalFloatMath.Decode(this.Raw, _fmt);

  /// <summary>Creates a Decimal128 from the two halves of its raw BID encoding.</summary>
  public static Decimal128 FromRaw(ulong high, ulong low) => new(high, low);

  public static Decimal128 Zero => new(new DecimalFloatMath.Value(0, BigInteger.Zero, 0, DecimalKind.Finite));
  public static Decimal128 One => new(new DecimalFloatMath.Value(0, BigInteger.One, 0, DecimalKind.Finite));
  public static Decimal128 MaxValue => new(new DecimalFloatMath.Value(0, _maxCoeff, 6111, DecimalKind.Finite));
  public static Decimal128 MinValue => new(new DecimalFloatMath.Value(1, _maxCoeff, 6111, DecimalKind.Finite));
  public static Decimal128 PositiveInfinity => new(new DecimalFloatMath.Value(0, BigInteger.Zero, 0, DecimalKind.Infinity));
  public static Decimal128 NegativeInfinity => new(new DecimalFloatMath.Value(1, BigInteger.Zero, 0, DecimalKind.Infinity));
  public static Decimal128 NaN => new(new DecimalFloatMath.Value(0, BigInteger.Zero, 0, DecimalKind.NaN));

  /// <summary>Gets the integer coefficient (significand) of the decoded value.</summary>
  public BigInteger Coefficient => this.Value.Coeff;

  /// <summary>Gets the base-10 exponent of the decoded value.</summary>
  public int Exponent => this.Value.Q;

  public static bool IsNaN(Decimal128 d) => d.Value.Kind == DecimalKind.NaN;
  public static bool IsInfinity(Decimal128 d) => d.Value.Kind == DecimalKind.Infinity;
  public static bool IsFinite(Decimal128 d) => d.Value.Kind == DecimalKind.Finite;
  public static bool IsNegative(Decimal128 d) => d.Value.Sign != 0 && d.Value.Kind != DecimalKind.NaN;

  public double ToDouble() => DecimalFloatMath.ToDouble(this.Value);
  public float ToSingle() => (float)this.ToDouble();

  public static Decimal128 FromDouble(double value) {
    if (double.IsNaN(value))
      return NaN;
    if (double.IsInfinity(value))
      return double.IsNegative(value) ? NegativeInfinity : PositiveInfinity;
    return new(DecimalFloatMath.Parse(value.ToString("R", CultureInfo.InvariantCulture), _fmt));
  }

  public static Decimal128 FromSingle(float value) => FromDouble(value);

  public static Decimal128 operator +(Decimal128 a, Decimal128 b) => new(DecimalFloatMath.Add(a.Value, b.Value, _fmt));
  public static Decimal128 operator -(Decimal128 a, Decimal128 b) => new(DecimalFloatMath.Subtract(a.Value, b.Value, _fmt));
  public static Decimal128 operator *(Decimal128 a, Decimal128 b) => new(DecimalFloatMath.Multiply(a.Value, b.Value, _fmt));
  public static Decimal128 operator /(Decimal128 a, Decimal128 b) => new(DecimalFloatMath.Divide(a.Value, b.Value, _fmt));
  public static Decimal128 operator -(Decimal128 a) => new(DecimalFloatMath.Negate(a.Value));
  public static Decimal128 operator +(Decimal128 a) => a;

  public int CompareTo(Decimal128 other) {
    DecimalFloatMath.Value a = this.Value, b = other.Value;
    if (a.Kind == DecimalKind.NaN || b.Kind == DecimalKind.NaN)
      return a.Kind == DecimalKind.NaN && b.Kind == DecimalKind.NaN ? 0 : a.Kind == DecimalKind.NaN ? 1 : -1;
    return DecimalFloatMath.Compare(a, b, _fmt);
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Decimal128 other)
      throw new ArgumentException("Object must be of type Decimal128.", nameof(obj));
    return this.CompareTo(other);
  }

  public bool Equals(Decimal128 other) => this.CompareTo(other) == 0 && !IsNaN(this);

  public override bool Equals(object? obj) => obj is Decimal128 other && this.Equals(other);

  public override int GetHashCode() => IsNaN(this) ? 0x7FC00000 : this.ToDouble().GetHashCode();

  public override string ToString() => DecimalFloatMath.ToString(this.Value);
  public string ToString(string? format, IFormatProvider? formatProvider) => DecimalFloatMath.ToString(this.Value);

  public static bool operator ==(Decimal128 a, Decimal128 b) => a.Equals(b);
  public static bool operator !=(Decimal128 a, Decimal128 b) => !a.Equals(b);
  public static bool operator <(Decimal128 a, Decimal128 b) => a.CompareTo(b) < 0;
  public static bool operator >(Decimal128 a, Decimal128 b) => a.CompareTo(b) > 0;
  public static bool operator <=(Decimal128 a, Decimal128 b) => a.CompareTo(b) <= 0;
  public static bool operator >=(Decimal128 a, Decimal128 b) => a.CompareTo(b) >= 0;

  public static explicit operator Decimal128(double value) => FromDouble(value);
  public static implicit operator double(Decimal128 value) => value.ToDouble();

  public static Decimal128 Parse(string s) => new(DecimalFloatMath.Parse(s, _fmt));
  public static Decimal128 Parse(string s, IFormatProvider? provider) => Parse(s);

  public static bool TryParse(string? s, out Decimal128 result) {
    try {
      result = s == null ? Zero : new(DecimalFloatMath.Parse(s, _fmt));
      return s != null;
    } catch (FormatException) {
      result = Zero;
      return false;
    }
  }

  public static bool TryParse(string? s, IFormatProvider? provider, out Decimal128 result) => TryParse(s, out result);
}
