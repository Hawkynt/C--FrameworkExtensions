#region (c)2010-2042 Hawkynt

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

#endregion

#if !SUPPORTS_COMPLEX

using System.Globalization;

namespace System.Numerics;

/// <summary>
/// Represents a complex number with double-precision floating-point real and imaginary parts.
/// </summary>
public readonly struct Complex : IEquatable<Complex>, IFormattable {

  #region Fields

  private readonly double _real;
  private readonly double _imaginary;

  #endregion

  #region Constructors

  /// <summary>
  /// Initializes a new instance of the <see cref="Complex"/> structure using the specified real and imaginary values.
  /// </summary>
  /// <param name="real">The real part of the complex number.</param>
  /// <param name="imaginary">The imaginary part of the complex number.</param>
  public Complex(double real, double imaginary) {
    this._real = real;
    this._imaginary = imaginary;
  }

  #endregion

  #region Properties

  /// <summary>
  /// Gets the real component of the current <see cref="Complex"/> object.
  /// </summary>
  public double Real => this._real;

  /// <summary>
  /// Gets the imaginary component of the current <see cref="Complex"/> object.
  /// </summary>
  public double Imaginary => this._imaginary;

  /// <summary>
  /// Gets the magnitude (or absolute value) of a complex number.
  /// </summary>
  public double Magnitude => Abs(this);

  /// <summary>
  /// Gets the phase of a complex number.
  /// </summary>
  public double Phase => Math.Atan2(this._imaginary, this._real);

  #endregion

  #region Static Properties

  /// <summary>
  /// Returns a new <see cref="Complex"/> instance with a real number equal to zero and an imaginary number equal to zero.
  /// </summary>
  public static Complex Zero { get; } = new(0.0, 0.0);

  /// <summary>
  /// Returns a new <see cref="Complex"/> instance with a real number equal to one and an imaginary number equal to zero.
  /// </summary>
  public static Complex One { get; } = new(1.0, 0.0);

  /// <summary>
  /// Returns a new <see cref="Complex"/> instance with a real number equal to zero and an imaginary number equal to one.
  /// </summary>
  public static Complex ImaginaryOne { get; } = new(0.0, 1.0);

  /// <summary>
  /// Gets a value representing positive infinity.
  /// </summary>
  public static Complex Infinity { get; } = new(double.PositiveInfinity, double.PositiveInfinity);

  /// <summary>
  /// Gets a value representing NaN (Not a Number).
  /// </summary>
  public static Complex NaN { get; } = new(double.NaN, double.NaN);

  #endregion

  #region Factory Methods

  /// <summary>
  /// Creates a complex number from a point's polar coordinates.
  /// </summary>
  /// <param name="magnitude">The magnitude, which is the distance from the origin to the number.</param>
  /// <param name="phase">The phase, which is the angle from the positive real axis to the line segment that represents the number.</param>
  /// <returns>A complex number.</returns>
  public static Complex FromPolarCoordinates(double magnitude, double phase)
    => new(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));

  #endregion

  #region Arithmetic Operations

  /// <summary>
  /// Adds two complex numbers and returns the result.
  /// </summary>
  /// <param name="left">The first complex number to add.</param>
  /// <param name="right">The second complex number to add.</param>
  /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
  public static Complex Add(Complex left, Complex right)
    => new(left._real + right._real, left._imaginary + right._imaginary);

  /// <summary>
  /// Subtracts one complex number from another and returns the result.
  /// </summary>
  /// <param name="left">The value to subtract from (the minuend).</param>
  /// <param name="right">The value to subtract (the subtrahend).</param>
  /// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/>.</returns>
  public static Complex Subtract(Complex left, Complex right)
    => new(left._real - right._real, left._imaginary - right._imaginary);

  /// <summary>
  /// Returns the product of two complex numbers.
  /// </summary>
  /// <param name="left">The first complex number to multiply.</param>
  /// <param name="right">The second complex number to multiply.</param>
  /// <returns>The product of the <paramref name="left"/> and <paramref name="right"/> parameters.</returns>
  public static Complex Multiply(Complex left, Complex right)
    => new(
      left._real * right._real - left._imaginary * right._imaginary,
      left._real * right._imaginary + left._imaginary * right._real
    );

  /// <summary>
  /// Divides one complex number by another and returns the result.
  /// </summary>
  /// <param name="dividend">The complex number to be divided.</param>
  /// <param name="divisor">The complex number to divide by.</param>
  /// <returns>The quotient of the division.</returns>
  public static Complex Divide(Complex dividend, Complex divisor) {
    var denominator = divisor._real * divisor._real + divisor._imaginary * divisor._imaginary;
    return new(
      (dividend._real * divisor._real + dividend._imaginary * divisor._imaginary) / denominator,
      (dividend._imaginary * divisor._real - dividend._real * divisor._imaginary) / denominator
    );
  }

  /// <summary>
  /// Returns the additive inverse of a specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The result of the <see cref="Real"/> and <see cref="Imaginary"/> components of the <paramref name="value"/> parameter multiplied by -1.</returns>
  public static Complex Negate(Complex value)
    => new(-value._real, -value._imaginary);

  #endregion

  #region Mathematical Functions

  /// <summary>
  /// Gets the absolute value (or magnitude) of a complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The absolute value of <paramref name="value"/>.</returns>
  public static double Abs(Complex value)
    => Math.Sqrt(value._real * value._real + value._imaginary * value._imaginary);

  /// <summary>
  /// Computes the conjugate of a complex number and returns the result.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The conjugate of <paramref name="value"/>.</returns>
  public static Complex Conjugate(Complex value)
    => new(value._real, -value._imaginary);

  /// <summary>
  /// Returns the multiplicative inverse of a complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The reciprocal of <paramref name="value"/>.</returns>
  public static Complex Reciprocal(Complex value) {
    if (value._real == 0 && value._imaginary == 0)
      return Zero;

    var denominator = value._real * value._real + value._imaginary * value._imaginary;
    return new(value._real / denominator, -value._imaginary / denominator);
  }

  /// <summary>
  /// Returns the square root of a specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The square root of <paramref name="value"/>.</returns>
  public static Complex Sqrt(Complex value) {
    if (value._imaginary == 0) {
      if (value._real >= 0)
        return new(Math.Sqrt(value._real), 0);

      return new(0, Math.Sqrt(-value._real));
    }

    var magnitude = Abs(value);
    return new(
      Math.Sqrt((magnitude + value._real) / 2),
      Math.Sign(value._imaginary) * Math.Sqrt((magnitude - value._real) / 2)
    );
  }

  /// <summary>
  /// Returns a specified complex number raised to a power specified by a complex number.
  /// </summary>
  /// <param name="value">A complex number to be raised to a power.</param>
  /// <param name="power">A complex number that specifies a power.</param>
  /// <returns>The complex number <paramref name="value"/> raised to the power <paramref name="power"/>.</returns>
  public static Complex Pow(Complex value, Complex power)
    => Exp(power * Log(value));

  /// <summary>
  /// Returns a specified complex number raised to a power specified by a double-precision floating-point number.
  /// </summary>
  /// <param name="value">A complex number to be raised to a power.</param>
  /// <param name="power">A double-precision floating-point number that specifies a power.</param>
  /// <returns>The complex number <paramref name="value"/> raised to the power <paramref name="power"/>.</returns>
  public static Complex Pow(Complex value, double power)
    => Pow(value, new Complex(power, 0));

  /// <summary>
  /// Returns e raised to the power specified by a complex number.
  /// </summary>
  /// <param name="value">A complex number that specifies a power.</param>
  /// <returns>The number e raised to the power <paramref name="value"/>.</returns>
  public static Complex Exp(Complex value) {
    var expReal = Math.Exp(value._real);
    return new(expReal * Math.Cos(value._imaginary), expReal * Math.Sin(value._imaginary));
  }

  /// <summary>
  /// Returns the natural (base e) logarithm of a specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The natural logarithm of <paramref name="value"/>.</returns>
  public static Complex Log(Complex value)
    => new(Math.Log(Abs(value)), value.Phase);

  /// <summary>
  /// Returns the logarithm of a specified complex number in a specified base.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <param name="baseValue">The base of the logarithm.</param>
  /// <returns>The logarithm of <paramref name="value"/> in base <paramref name="baseValue"/>.</returns>
  public static Complex Log(Complex value, double baseValue)
    => Log(value) / Math.Log(baseValue);

  /// <summary>
  /// Returns the base-10 logarithm of a specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The base-10 logarithm of <paramref name="value"/>.</returns>
  public static Complex Log10(Complex value)
    => Log(value) / Math.Log(10);

  #endregion

  #region Trigonometric Functions

  /// <summary>
  /// Returns the sine of the specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The sine of <paramref name="value"/>.</returns>
  public static Complex Sin(Complex value)
    => new(
      Math.Sin(value._real) * Math.Cosh(value._imaginary),
      Math.Cos(value._real) * Math.Sinh(value._imaginary)
    );

  /// <summary>
  /// Returns the cosine of the specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The cosine of <paramref name="value"/>.</returns>
  public static Complex Cos(Complex value)
    => new(
      Math.Cos(value._real) * Math.Cosh(value._imaginary),
      -Math.Sin(value._real) * Math.Sinh(value._imaginary)
    );

  /// <summary>
  /// Returns the tangent of the specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The tangent of <paramref name="value"/>.</returns>
  public static Complex Tan(Complex value)
    => Sin(value) / Cos(value);

  /// <summary>
  /// Returns the hyperbolic sine of the specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The hyperbolic sine of <paramref name="value"/>.</returns>
  public static Complex Sinh(Complex value)
    => new(
      Math.Sinh(value._real) * Math.Cos(value._imaginary),
      Math.Cosh(value._real) * Math.Sin(value._imaginary)
    );

  /// <summary>
  /// Returns the hyperbolic cosine of the specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The hyperbolic cosine of <paramref name="value"/>.</returns>
  public static Complex Cosh(Complex value)
    => new(
      Math.Cosh(value._real) * Math.Cos(value._imaginary),
      Math.Sinh(value._real) * Math.Sin(value._imaginary)
    );

  /// <summary>
  /// Returns the hyperbolic tangent of the specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The hyperbolic tangent of <paramref name="value"/>.</returns>
  public static Complex Tanh(Complex value)
    => Sinh(value) / Cosh(value);

  /// <summary>
  /// Returns the angle that is the arc sine of the specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The arc sine of <paramref name="value"/>.</returns>
  public static Complex Asin(Complex value)
    => -ImaginaryOne * Log(ImaginaryOne * value + Sqrt(One - value * value));

  /// <summary>
  /// Returns the angle that is the arc cosine of the specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The arc cosine of <paramref name="value"/>.</returns>
  public static Complex Acos(Complex value)
    => -ImaginaryOne * Log(value + ImaginaryOne * Sqrt(One - value * value));

  /// <summary>
  /// Returns the angle that is the arc tangent of the specified complex number.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns>The arc tangent of <paramref name="value"/>.</returns>
  public static Complex Atan(Complex value) {
    var i = ImaginaryOne;
    return i / 2 * (Log(One - i * value) - Log(One + i * value));
  }

  #endregion

  #region Operators

  /// <summary>
  /// Adds two complex numbers.
  /// </summary>
  public static Complex operator +(Complex left, Complex right) => Add(left, right);

  /// <summary>
  /// Subtracts a complex number from another complex number.
  /// </summary>
  public static Complex operator -(Complex left, Complex right) => Subtract(left, right);

  /// <summary>
  /// Multiplies two complex numbers.
  /// </summary>
  public static Complex operator *(Complex left, Complex right) => Multiply(left, right);

  /// <summary>
  /// Divides a complex number by another complex number.
  /// </summary>
  public static Complex operator /(Complex left, Complex right) => Divide(left, right);

  /// <summary>
  /// Returns the additive inverse of a specified complex number.
  /// </summary>
  public static Complex operator -(Complex value) => Negate(value);

  /// <summary>
  /// Returns a specified complex number unchanged (unary plus).
  /// </summary>
  public static Complex operator +(Complex value) => value;

  /// <summary>
  /// Returns a value that indicates whether two complex numbers are equal.
  /// </summary>
  public static bool operator ==(Complex left, Complex right) => left._real == right._real && left._imaginary == right._imaginary;

  /// <summary>
  /// Returns a value that indicates whether two complex numbers are not equal.
  /// </summary>
  public static bool operator !=(Complex left, Complex right) => !(left == right);

  #endregion

  #region Mixed Operators with double

  /// <summary>
  /// Adds a complex number to a double.
  /// </summary>
  public static Complex operator +(Complex left, double right) => new(left._real + right, left._imaginary);

  /// <summary>
  /// Adds a double to a complex number.
  /// </summary>
  public static Complex operator +(double left, Complex right) => new(left + right._real, right._imaginary);

  /// <summary>
  /// Subtracts a double from a complex number.
  /// </summary>
  public static Complex operator -(Complex left, double right) => new(left._real - right, left._imaginary);

  /// <summary>
  /// Subtracts a complex number from a double.
  /// </summary>
  public static Complex operator -(double left, Complex right) => new(left - right._real, -right._imaginary);

  /// <summary>
  /// Multiplies a complex number by a double.
  /// </summary>
  public static Complex operator *(Complex left, double right) => new(left._real * right, left._imaginary * right);

  /// <summary>
  /// Multiplies a double by a complex number.
  /// </summary>
  public static Complex operator *(double left, Complex right) => new(left * right._real, left * right._imaginary);

  /// <summary>
  /// Divides a complex number by a double.
  /// </summary>
  public static Complex operator /(Complex left, double right) => new(left._real / right, left._imaginary / right);

  /// <summary>
  /// Divides a double by a complex number.
  /// </summary>
  public static Complex operator /(double left, Complex right) => Divide(new Complex(left, 0), right);

  #endregion

  #region Implicit/Explicit Conversions

  /// <summary>
  /// Defines an implicit conversion of a signed byte to a complex number.
  /// </summary>
  public static implicit operator Complex(sbyte value) => new(value, 0);

  /// <summary>
  /// Defines an implicit conversion of a byte to a complex number.
  /// </summary>
  public static implicit operator Complex(byte value) => new(value, 0);

  /// <summary>
  /// Defines an implicit conversion of a 16-bit signed integer to a complex number.
  /// </summary>
  public static implicit operator Complex(short value) => new(value, 0);

  /// <summary>
  /// Defines an implicit conversion of a 16-bit unsigned integer to a complex number.
  /// </summary>
  public static implicit operator Complex(ushort value) => new(value, 0);

  /// <summary>
  /// Defines an implicit conversion of a 32-bit signed integer to a complex number.
  /// </summary>
  public static implicit operator Complex(int value) => new(value, 0);

  /// <summary>
  /// Defines an implicit conversion of a 32-bit unsigned integer to a complex number.
  /// </summary>
  public static implicit operator Complex(uint value) => new(value, 0);

  /// <summary>
  /// Defines an implicit conversion of a 64-bit signed integer to a complex number.
  /// </summary>
  public static implicit operator Complex(long value) => new(value, 0);

  /// <summary>
  /// Defines an implicit conversion of a 64-bit unsigned integer to a complex number.
  /// </summary>
  public static implicit operator Complex(ulong value) => new(value, 0);

  /// <summary>
  /// Defines an implicit conversion of a single-precision floating-point number to a complex number.
  /// </summary>
  public static implicit operator Complex(float value) => new(value, 0);

  /// <summary>
  /// Defines an implicit conversion of a double-precision floating-point number to a complex number.
  /// </summary>
  public static implicit operator Complex(double value) => new(value, 0);

  /// <summary>
  /// Defines an explicit conversion of a decimal to a complex number.
  /// </summary>
  public static explicit operator Complex(decimal value) => new((double)value, 0);

  #endregion

  #region Equality

  /// <summary>
  /// Returns a value that indicates whether the current instance and a specified complex number have the same value.
  /// </summary>
  /// <param name="value">The complex number to compare.</param>
  /// <returns><see langword="true"/> if this complex number and <paramref name="value"/> have the same value; otherwise, <see langword="false"/>.</returns>
  public bool Equals(Complex value) => this._real == value._real && this._imaginary == value._imaginary;

  /// <summary>
  /// Returns a value that indicates whether the current instance and a specified object have the same value.
  /// </summary>
  /// <param name="obj">The object to compare.</param>
  /// <returns><see langword="true"/> if the <paramref name="obj"/> parameter is a <see cref="Complex"/> object or a type capable of implicit conversion to a <see cref="Complex"/> object, and its value equals the current <see cref="Complex"/> object; otherwise, <see langword="false"/>.</returns>
  public override bool Equals(object? obj) => obj is Complex other && this.Equals(other);

  /// <summary>
  /// Returns the hash code for the current <see cref="Complex"/> object.
  /// </summary>
  /// <returns>A 32-bit signed integer hash code.</returns>
  public override int GetHashCode() {
    unchecked {
      return (this._real.GetHashCode() * 397) ^ this._imaginary.GetHashCode();
    }
  }

  #endregion

  #region ToString

  /// <summary>
  /// Converts the value of the current complex number to its equivalent string representation in Cartesian form.
  /// </summary>
  /// <returns>The string representation of the current instance in Cartesian form.</returns>
  public override string ToString() => this.ToString(null, CultureInfo.CurrentCulture);

  /// <summary>
  /// Converts the value of the current complex number to its equivalent string representation in Cartesian form by using the specified format for its real and imaginary parts.
  /// </summary>
  /// <param name="format">A standard or custom numeric format string.</param>
  /// <returns>The string representation of the current instance in Cartesian form.</returns>
  public string ToString(string? format) => this.ToString(format, CultureInfo.CurrentCulture);

  /// <summary>
  /// Converts the value of the current complex number to its equivalent string representation in Cartesian form by using the specified culture-specific format information.
  /// </summary>
  /// <param name="provider">An object that supplies culture-specific formatting information.</param>
  /// <returns>The string representation of the current instance in Cartesian form.</returns>
  public string ToString(IFormatProvider? provider) => this.ToString(null, provider);

  /// <summary>
  /// Converts the value of the current complex number to its equivalent string representation in Cartesian form by using the specified format and culture-specific format information for its real and imaginary parts.
  /// </summary>
  /// <param name="format">A standard or custom numeric format string.</param>
  /// <param name="provider">An object that supplies culture-specific formatting information.</param>
  /// <returns>The string representation of the current instance in Cartesian form.</returns>
  public string ToString(string? format, IFormatProvider? formatProvider) {
    var numberFormatInfo = NumberFormatInfo.GetInstance(formatProvider);
    var separator = numberFormatInfo.NumberDecimalSeparator == "," ? ";" : ",";

    return $"<{this._real.ToString(format, formatProvider)}{separator} {this._imaginary.ToString(format, formatProvider)}>";
  }

  #endregion

  #region IsFinite/IsInfinity/IsNaN

  /// <summary>
  /// Returns a value indicating whether the specified complex number is finite.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns><see langword="true"/> if both components are finite; otherwise, <see langword="false"/>.</returns>
  public static bool IsFinite(Complex value)
    => !double.IsInfinity(value._real) && !double.IsNaN(value._real) &&
       !double.IsInfinity(value._imaginary) && !double.IsNaN(value._imaginary);

  /// <summary>
  /// Returns a value indicating whether the specified complex number is infinite.
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns><see langword="true"/> if either component is infinite; otherwise, <see langword="false"/>.</returns>
  public static bool IsInfinity(Complex value)
    => double.IsInfinity(value._real) || double.IsInfinity(value._imaginary);

  /// <summary>
  /// Returns a value indicating whether the specified complex number is NaN (Not a Number).
  /// </summary>
  /// <param name="value">A complex number.</param>
  /// <returns><see langword="true"/> if either component is NaN; otherwise, <see langword="false"/>.</returns>
  public static bool IsNaN(Complex value)
    => double.IsNaN(value._real) || double.IsNaN(value._imaginary);

  #endregion

}

#endif
