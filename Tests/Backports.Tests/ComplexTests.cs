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

using System;
using System.Numerics;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Complex")]
public class ComplexTests {

  private const double Tolerance = 1e-10;

  #region Static Properties Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_Zero_HasZeroRealAndImaginary() {
    Assert.That(Complex.Zero.Real, Is.EqualTo(0.0));
    Assert.That(Complex.Zero.Imaginary, Is.EqualTo(0.0));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_One_HasRealOneAndImaginaryZero() {
    Assert.That(Complex.One.Real, Is.EqualTo(1.0));
    Assert.That(Complex.One.Imaginary, Is.EqualTo(0.0));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_ImaginaryOne_HasRealZeroAndImaginaryOne() {
    Assert.That(Complex.ImaginaryOne.Real, Is.EqualTo(0.0));
    Assert.That(Complex.ImaginaryOne.Imaginary, Is.EqualTo(1.0));
  }

  #endregion

  #region Constructor Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_Constructor_SetsRealAndImaginary() {
    var c = new Complex(3.0, 4.0);
    Assert.That(c.Real, Is.EqualTo(3.0));
    Assert.That(c.Imaginary, Is.EqualTo(4.0));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Constructor_SupportsNegativeValues() {
    var c = new Complex(-3.0, -4.0);
    Assert.That(c.Real, Is.EqualTo(-3.0));
    Assert.That(c.Imaginary, Is.EqualTo(-4.0));
  }

  #endregion

  #region Property Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_Magnitude_ReturnsCorrectValue() {
    var c = new Complex(3.0, 4.0);
    Assert.That(c.Magnitude, Is.EqualTo(5.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Phase_ReturnsCorrectAngle() {
    var c = new Complex(1.0, 1.0);
    Assert.That(c.Phase, Is.EqualTo(Math.PI / 4).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Complex_Phase_PureReal_ReturnsZero() {
    var c = new Complex(5.0, 0.0);
    Assert.That(c.Phase, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Complex_Phase_PureImaginary_ReturnsPiOver2() {
    var c = new Complex(0.0, 5.0);
    Assert.That(c.Phase, Is.EqualTo(Math.PI / 2).Within(Tolerance));
  }

  #endregion

  #region FromPolarCoordinates Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_FromPolarCoordinates_ReturnsCorrectComplex() {
    var c = Complex.FromPolarCoordinates(5.0, Math.PI / 4);
    Assert.That(c.Real, Is.EqualTo(5.0 * Math.Cos(Math.PI / 4)).Within(Tolerance));
    Assert.That(c.Imaginary, Is.EqualTo(5.0 * Math.Sin(Math.PI / 4)).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Complex_FromPolarCoordinates_ZeroMagnitude_ReturnsZero() {
    var c = Complex.FromPolarCoordinates(0.0, Math.PI);
    Assert.That(c.Real, Is.EqualTo(0.0).Within(Tolerance));
    Assert.That(c.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  #endregion

  #region Arithmetic Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_Add_ReturnsCorrectSum() {
    var a = new Complex(1.0, 2.0);
    var b = new Complex(3.0, 4.0);
    var result = a + b;
    Assert.That(result.Real, Is.EqualTo(4.0));
    Assert.That(result.Imaginary, Is.EqualTo(6.0));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Subtract_ReturnsCorrectDifference() {
    var a = new Complex(5.0, 7.0);
    var b = new Complex(2.0, 3.0);
    var result = a - b;
    Assert.That(result.Real, Is.EqualTo(3.0));
    Assert.That(result.Imaginary, Is.EqualTo(4.0));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Multiply_ReturnsCorrectProduct() {
    // (1+2i)(3+4i) = 3+4i+6i+8i² = 3+10i-8 = -5+10i
    var a = new Complex(1.0, 2.0);
    var b = new Complex(3.0, 4.0);
    var result = a * b;
    Assert.That(result.Real, Is.EqualTo(-5.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(10.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Divide_ReturnsCorrectQuotient() {
    // (2+2i)/(1+i) = (2+2i)(1-i)/((1+i)(1-i)) = (2-2i+2i-2i²)/(1-i²) = (2+2)/2 = 2
    var a = new Complex(2.0, 2.0);
    var b = new Complex(1.0, 1.0);
    var result = a / b;
    Assert.That(result.Real, Is.EqualTo(2.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Negate_ReturnsNegated() {
    var c = new Complex(3.0, 4.0);
    var result = -c;
    Assert.That(result.Real, Is.EqualTo(-3.0));
    Assert.That(result.Imaginary, Is.EqualTo(-4.0));
  }

  #endregion

  #region Mixed Operators with double Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_AddDouble_ReturnsCorrectSum() {
    var c = new Complex(1.0, 2.0);
    var result = c + 5.0;
    Assert.That(result.Real, Is.EqualTo(6.0));
    Assert.That(result.Imaginary, Is.EqualTo(2.0));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_MultiplyByDouble_ReturnsScaledComplex() {
    var c = new Complex(2.0, 3.0);
    var result = c * 2.0;
    Assert.That(result.Real, Is.EqualTo(4.0));
    Assert.That(result.Imaginary, Is.EqualTo(6.0));
  }

  #endregion

  #region Mathematical Functions Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_Abs_ReturnsMagnitude() {
    var c = new Complex(3.0, 4.0);
    Assert.That(Complex.Abs(c), Is.EqualTo(5.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Conjugate_ReturnsConjugate() {
    var c = new Complex(3.0, 4.0);
    var result = Complex.Conjugate(c);
    Assert.That(result.Real, Is.EqualTo(3.0));
    Assert.That(result.Imaginary, Is.EqualTo(-4.0));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Reciprocal_ReturnsReciprocal() {
    var c = new Complex(2.0, 0.0);
    var result = Complex.Reciprocal(c);
    Assert.That(result.Real, Is.EqualTo(0.5).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Sqrt_ReturnsCorrectRoot() {
    var c = new Complex(4.0, 0.0);
    var result = Complex.Sqrt(c);
    Assert.That(result.Real, Is.EqualTo(2.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Sqrt_NegativeReal_ReturnsImaginaryResult() {
    var c = new Complex(-4.0, 0.0);
    var result = Complex.Sqrt(c);
    Assert.That(result.Real, Is.EqualTo(0.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(2.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Exp_ReturnsCorrectValue() {
    // e^(0) = 1
    var result = Complex.Exp(Complex.Zero);
    Assert.That(result.Real, Is.EqualTo(1.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Exp_PureImaginary_ReturnsEulerFormula() {
    // e^(i*pi) = -1
    var c = new Complex(0.0, Math.PI);
    var result = Complex.Exp(c);
    Assert.That(result.Real, Is.EqualTo(-1.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Log_ReturnsCorrectValue() {
    // log(e) = 1
    var c = new Complex(Math.E, 0.0);
    var result = Complex.Log(c);
    Assert.That(result.Real, Is.EqualTo(1.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Log10_ReturnsCorrectValue() {
    var c = new Complex(100.0, 0.0);
    var result = Complex.Log10(c);
    Assert.That(result.Real, Is.EqualTo(2.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Pow_ReturnsCorrectPower() {
    // (2+0i)^3 = 8
    var @base = new Complex(2.0, 0.0);
    var result = Complex.Pow(@base, 3.0);
    Assert.That(result.Real, Is.EqualTo(8.0).Within(Tolerance));
    Assert.That(Math.Abs(result.Imaginary), Is.LessThan(Tolerance));
  }

  #endregion

  #region Trigonometric Functions Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_Sin_ReturnsCorrectValue() {
    // sin(0) = 0
    var result = Complex.Sin(Complex.Zero);
    Assert.That(result.Real, Is.EqualTo(0.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Cos_ReturnsCorrectValue() {
    // cos(0) = 1
    var result = Complex.Cos(Complex.Zero);
    Assert.That(result.Real, Is.EqualTo(1.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Tan_ReturnsCorrectValue() {
    // tan(0) = 0
    var result = Complex.Tan(Complex.Zero);
    Assert.That(result.Real, Is.EqualTo(0.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Sinh_ReturnsCorrectValue() {
    // sinh(0) = 0
    var result = Complex.Sinh(Complex.Zero);
    Assert.That(result.Real, Is.EqualTo(0.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Cosh_ReturnsCorrectValue() {
    // cosh(0) = 1
    var result = Complex.Cosh(Complex.Zero);
    Assert.That(result.Real, Is.EqualTo(1.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Tanh_ReturnsCorrectValue() {
    // tanh(0) = 0
    var result = Complex.Tanh(Complex.Zero);
    Assert.That(result.Real, Is.EqualTo(0.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Asin_ReturnsCorrectValue() {
    // asin(0) = 0
    var result = Complex.Asin(Complex.Zero);
    Assert.That(result.Real, Is.EqualTo(0.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Acos_ReturnsCorrectValue() {
    // acos(1) = 0
    var result = Complex.Acos(Complex.One);
    Assert.That(result.Real, Is.EqualTo(0.0).Within(Tolerance));
    Assert.That(Math.Abs(result.Imaginary), Is.LessThan(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Atan_ReturnsCorrectValue() {
    // atan(0) = 0
    var result = Complex.Atan(Complex.Zero);
    Assert.That(result.Real, Is.EqualTo(0.0).Within(Tolerance));
    Assert.That(result.Imaginary, Is.EqualTo(0.0).Within(Tolerance));
  }

  #endregion

  #region Equality Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_Equals_ReturnsTrue_ForEqualValues() {
    var a = new Complex(3.0, 4.0);
    var b = new Complex(3.0, 4.0);
    Assert.That(a.Equals(b), Is.True);
    Assert.That(a == b, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Equals_ReturnsFalse_ForDifferentValues() {
    var a = new Complex(3.0, 4.0);
    var b = new Complex(3.0, 5.0);
    Assert.That(a.Equals(b), Is.False);
    Assert.That(a != b, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_GetHashCode_SameForEqualValues() {
    var a = new Complex(3.0, 4.0);
    var b = new Complex(3.0, 4.0);
    Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
  }

  #endregion

  #region ToString Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_ToString_ReturnsFormattedString() {
    var c = new Complex(3.0, 4.0);
    var result = c.ToString();
    Assert.That(result, Does.Contain("3"));
    Assert.That(result, Does.Contain("4"));
  }

  #endregion

  #region Implicit Conversion Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_ImplicitFromDouble_Works() {
    Complex c = 5.0;
    Assert.That(c.Real, Is.EqualTo(5.0));
    Assert.That(c.Imaginary, Is.EqualTo(0.0));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_ImplicitFromInt_Works() {
    Complex c = 42;
    Assert.That(c.Real, Is.EqualTo(42.0));
    Assert.That(c.Imaginary, Is.EqualTo(0.0));
  }

  #endregion

  #region IsFinite/IsInfinity/IsNaN Tests

#if NET7_0_OR_GREATER || !SUPPORTS_COMPLEX

  [Test]
  [Category("HappyPath")]
  public void Complex_IsFinite_ReturnsTrueForFiniteComplex() {
    var c = new Complex(3.0, 4.0);
    Assert.That(Complex.IsFinite(c), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_IsFinite_ReturnsFalseForInfinity() {
    Assert.That(Complex.IsFinite(Complex.Infinity), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_IsInfinity_ReturnsTrueForInfiniteComplex() {
    var c = new Complex(double.PositiveInfinity, 0.0);
    Assert.That(Complex.IsInfinity(c), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_IsNaN_ReturnsTrueForNaN() {
    Assert.That(Complex.IsNaN(Complex.NaN), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_IsNaN_ReturnsFalseForFiniteComplex() {
    var c = new Complex(3.0, 4.0);
    Assert.That(Complex.IsNaN(c), Is.False);
  }

#endif

  #endregion

}
