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
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("MathF")]
public class MathFTests {

  private const float Tolerance = 0.0001f;

  #region Constants

  [Test]
  [Category("HappyPath")]
  public void MathF_E_ReturnsCorrectValue() {
    Assert.That(MathF.E, Is.EqualTo(2.71828183f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_PI_ReturnsCorrectValue() {
    Assert.That(MathF.PI, Is.EqualTo(3.14159265f).Within(Tolerance));
  }

  #endregion

  #region Basic Operations

  [Test]
  [Category("HappyPath")]
  public void MathF_Abs_PositiveValue_ReturnsSame() {
    Assert.That(MathF.Abs(5.5f), Is.EqualTo(5.5f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Abs_NegativeValue_ReturnsPositive() {
    Assert.That(MathF.Abs(-5.5f), Is.EqualTo(5.5f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Abs_Zero_ReturnsZero() {
    Assert.That(MathF.Abs(0f), Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Sign_Positive_ReturnsOne() {
    Assert.That(MathF.Sign(5.5f), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Sign_Negative_ReturnsMinusOne() {
    Assert.That(MathF.Sign(-5.5f), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Sign_Zero_ReturnsZero() {
    Assert.That(MathF.Sign(0f), Is.EqualTo(0));
  }

  #endregion

  #region Min/Max

  [Test]
  [Category("HappyPath")]
  public void MathF_Min_ReturnsSmaller() {
    Assert.That(MathF.Min(3.0f, 5.0f), Is.EqualTo(3.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Max_ReturnsLarger() {
    Assert.That(MathF.Max(3.0f, 5.0f), Is.EqualTo(5.0f).Within(Tolerance));
  }

  #endregion

  #region Rounding

  [Test]
  [Category("HappyPath")]
  public void MathF_Ceiling_RoundsUp() {
    Assert.That(MathF.Ceiling(3.2f), Is.EqualTo(4.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Floor_RoundsDown() {
    Assert.That(MathF.Floor(3.8f), Is.EqualTo(3.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Round_RoundsToNearest() {
    Assert.That(MathF.Round(3.5f), Is.EqualTo(4.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Truncate_RemovesFractional() {
    Assert.That(MathF.Truncate(3.9f), Is.EqualTo(3.0f).Within(Tolerance));
    Assert.That(MathF.Truncate(-3.9f), Is.EqualTo(-3.0f).Within(Tolerance));
  }

  #endregion

  #region Trigonometric Functions

  [Test]
  [Category("HappyPath")]
  public void MathF_Sin_ReturnsCorrectValue() {
    Assert.That(MathF.Sin(MathF.PI / 2), Is.EqualTo(1.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Cos_ReturnsCorrectValue() {
    Assert.That(MathF.Cos(0f), Is.EqualTo(1.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Tan_ReturnsCorrectValue() {
    Assert.That(MathF.Tan(MathF.PI / 4), Is.EqualTo(1.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Asin_ReturnsCorrectValue() {
    Assert.That(MathF.Asin(1.0f), Is.EqualTo(MathF.PI / 2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Acos_ReturnsCorrectValue() {
    Assert.That(MathF.Acos(1.0f), Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Atan_ReturnsCorrectValue() {
    Assert.That(MathF.Atan(1.0f), Is.EqualTo(MathF.PI / 4).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Atan2_ReturnsCorrectValue() {
    Assert.That(MathF.Atan2(1.0f, 1.0f), Is.EqualTo(MathF.PI / 4).Within(Tolerance));
  }

  #endregion

  #region Hyperbolic Functions

  [Test]
  [Category("HappyPath")]
  public void MathF_Sinh_ReturnsCorrectValue() {
    Assert.That(MathF.Sinh(0f), Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Cosh_ReturnsCorrectValue() {
    Assert.That(MathF.Cosh(0f), Is.EqualTo(1.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Tanh_ReturnsCorrectValue() {
    Assert.That(MathF.Tanh(0f), Is.EqualTo(0f).Within(Tolerance));
  }

  #endregion

  #region Exponential and Logarithmic

  [Test]
  [Category("HappyPath")]
  public void MathF_Exp_ReturnsCorrectValue() {
    Assert.That(MathF.Exp(1.0f), Is.EqualTo(MathF.E).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Log_ReturnsCorrectValue() {
    Assert.That(MathF.Log(MathF.E), Is.EqualTo(1.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Log10_ReturnsCorrectValue() {
    Assert.That(MathF.Log10(100f), Is.EqualTo(2.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_LogWithBase_ReturnsCorrectValue() {
    Assert.That(MathF.Log(8f, 2f), Is.EqualTo(3.0f).Within(Tolerance));
  }

  #endregion

  #region Power and Root

  [Test]
  [Category("HappyPath")]
  public void MathF_Pow_ReturnsCorrectValue() {
    Assert.That(MathF.Pow(2.0f, 3.0f), Is.EqualTo(8.0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_Sqrt_ReturnsCorrectValue() {
    Assert.That(MathF.Sqrt(16.0f), Is.EqualTo(4.0f).Within(Tolerance));
  }

  #endregion

  #region IEEERemainder

  [Test]
  [Category("HappyPath")]
  public void MathF_IEEERemainder_ReturnsCorrectValue() {
    var result = MathF.IEEERemainder(10.0f, 3.0f);
    Assert.That(result, Is.EqualTo(1.0f).Within(Tolerance));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void MathF_Sqrt_NegativeValue_ReturnsNaN() {
    Assert.That(float.IsNaN(MathF.Sqrt(-1.0f)), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_Log_Zero_ReturnsNegativeInfinity() {
    Assert.That(float.IsNegativeInfinity(MathF.Log(0f)), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_Log_NegativeValue_ReturnsNaN() {
    Assert.That(float.IsNaN(MathF.Log(-1.0f)), Is.True);
  }

  #endregion

}
