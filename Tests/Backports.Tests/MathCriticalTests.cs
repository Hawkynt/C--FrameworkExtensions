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
[Category("MathCritical")]
public class MathCriticalTests {

  private const float FloatTolerance = 0.0001f;

  #region MathF.BitDecrement

  [Test]
  [Category("HappyPath")]
  public void MathF_BitDecrement_PositiveValue_ReturnsNextSmaller() {
    var result = MathF.BitDecrement(1.0f);
    Assert.That(result, Is.LessThan(1.0f));
    Assert.That(result, Is.GreaterThan(0.99f));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_BitDecrement_NegativeValue_ReturnsNextSmaller() {
    var result = MathF.BitDecrement(-1.0f);
    Assert.That(result, Is.LessThan(-1.0f));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_BitDecrement_Zero_ReturnsNegativeEpsilon() {
    var result = MathF.BitDecrement(0.0f);
    Assert.That(result, Is.EqualTo(-float.Epsilon));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_BitDecrement_PositiveInfinity_ReturnsMaxValue() {
    var result = MathF.BitDecrement(float.PositiveInfinity);
    Assert.That(result, Is.EqualTo(float.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_BitDecrement_NegativeInfinity_ReturnsNegativeInfinity() {
    var result = MathF.BitDecrement(float.NegativeInfinity);
    Assert.That(float.IsNegativeInfinity(result), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_BitDecrement_NaN_ReturnsNaN() {
    var result = MathF.BitDecrement(float.NaN);
    Assert.That(float.IsNaN(result), Is.True);
  }

  #endregion

  #region MathF.BitIncrement

  [Test]
  [Category("HappyPath")]
  public void MathF_BitIncrement_PositiveValue_ReturnsNextLarger() {
    var result = MathF.BitIncrement(1.0f);
    Assert.That(result, Is.GreaterThan(1.0f));
    Assert.That(result, Is.LessThan(1.01f));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_BitIncrement_NegativeValue_ReturnsNextLarger() {
    var result = MathF.BitIncrement(-1.0f);
    Assert.That(result, Is.GreaterThan(-1.0f));
    Assert.That(result, Is.LessThan(-0.99f));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_BitIncrement_NegativeZero_ReturnsEpsilon() {
    var result = MathF.BitIncrement(-0.0f);
    Assert.That(result, Is.EqualTo(float.Epsilon));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_BitIncrement_NegativeInfinity_ReturnsMinValue() {
    var result = MathF.BitIncrement(float.NegativeInfinity);
    Assert.That(result, Is.EqualTo(float.MinValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_BitIncrement_PositiveInfinity_ReturnsPositiveInfinity() {
    var result = MathF.BitIncrement(float.PositiveInfinity);
    Assert.That(float.IsPositiveInfinity(result), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_BitIncrement_NaN_ReturnsNaN() {
    var result = MathF.BitIncrement(float.NaN);
    Assert.That(float.IsNaN(result), Is.True);
  }

  #endregion

  #region MathF.ILogB

  [Test]
  [Category("HappyPath")]
  public void MathF_ILogB_PowerOfTwo_ReturnsExponent() {
    Assert.That(MathF.ILogB(1.0f), Is.EqualTo(0));
    Assert.That(MathF.ILogB(2.0f), Is.EqualTo(1));
    Assert.That(MathF.ILogB(4.0f), Is.EqualTo(2));
    Assert.That(MathF.ILogB(8.0f), Is.EqualTo(3));
    Assert.That(MathF.ILogB(0.5f), Is.EqualTo(-1));
    Assert.That(MathF.ILogB(0.25f), Is.EqualTo(-2));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_ILogB_NonPowerOfTwo_ReturnsFloorLog2() {
    Assert.That(MathF.ILogB(3.0f), Is.EqualTo(1));
    Assert.That(MathF.ILogB(5.0f), Is.EqualTo(2));
    Assert.That(MathF.ILogB(7.0f), Is.EqualTo(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_ILogB_Zero_ReturnsMinValue() {
    var result = MathF.ILogB(0.0f);
    Assert.That(result, Is.EqualTo(int.MinValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_ILogB_NaN_ReturnsMaxValue() {
    var result = MathF.ILogB(float.NaN);
    Assert.That(result, Is.EqualTo(int.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_ILogB_Infinity_ReturnsMaxValue() {
    var result = MathF.ILogB(float.PositiveInfinity);
    Assert.That(result, Is.EqualTo(int.MaxValue));
  }

  #endregion

  #region MathF.ScaleB

  [Test]
  [Category("HappyPath")]
  public void MathF_ScaleB_PositiveExponent_ScalesUp() {
    Assert.That(MathF.ScaleB(1.0f, 2), Is.EqualTo(4.0f).Within(FloatTolerance));
    Assert.That(MathF.ScaleB(1.0f, 3), Is.EqualTo(8.0f).Within(FloatTolerance));
    Assert.That(MathF.ScaleB(2.0f, 3), Is.EqualTo(16.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_ScaleB_NegativeExponent_ScalesDown() {
    Assert.That(MathF.ScaleB(4.0f, -1), Is.EqualTo(2.0f).Within(FloatTolerance));
    Assert.That(MathF.ScaleB(8.0f, -2), Is.EqualTo(2.0f).Within(FloatTolerance));
    Assert.That(MathF.ScaleB(1.0f, -1), Is.EqualTo(0.5f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_ScaleB_ZeroExponent_ReturnsSame() {
    Assert.That(MathF.ScaleB(5.0f, 0), Is.EqualTo(5.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_ScaleB_LargeExponent_ReturnsInfinity() {
    var result = MathF.ScaleB(1.0f, 200);
    Assert.That(float.IsPositiveInfinity(result), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_ScaleB_VeryNegativeExponent_ReturnsZero() {
    var result = MathF.ScaleB(1.0f, -200);
    Assert.That(result, Is.EqualTo(0.0f));
  }

  #endregion

  #region MathF.CopySign

  [Test]
  [Category("HappyPath")]
  public void MathF_CopySign_PositiveToPositive_ReturnsPositive() {
    Assert.That(MathF.CopySign(5.0f, 1.0f), Is.EqualTo(5.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_CopySign_PositiveToNegative_ReturnsNegative() {
    Assert.That(MathF.CopySign(5.0f, -1.0f), Is.EqualTo(-5.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_CopySign_NegativeToPositive_ReturnsPositive() {
    Assert.That(MathF.CopySign(-5.0f, 1.0f), Is.EqualTo(5.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_CopySign_NegativeToNegative_ReturnsNegative() {
    Assert.That(MathF.CopySign(-5.0f, -1.0f), Is.EqualTo(-5.0f).Within(FloatTolerance));
  }

  #endregion

  #region MathF.MaxMagnitude

  [Test]
  [Category("HappyPath")]
  public void MathF_MaxMagnitude_LargerPositive_ReturnsLarger() {
    Assert.That(MathF.MaxMagnitude(3.0f, 5.0f), Is.EqualTo(5.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_MaxMagnitude_LargerNegative_ReturnsNegative() {
    Assert.That(MathF.MaxMagnitude(3.0f, -5.0f), Is.EqualTo(-5.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_MaxMagnitude_EqualMagnitude_ReturnsPositive() {
    Assert.That(MathF.MaxMagnitude(-5.0f, 5.0f), Is.EqualTo(5.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_MaxMagnitude_WithNaN_ReturnsNaN() {
    Assert.That(float.IsNaN(MathF.MaxMagnitude(float.NaN, 5.0f)), Is.True);
  }

  #endregion

  #region MathF.MinMagnitude

  [Test]
  [Category("HappyPath")]
  public void MathF_MinMagnitude_SmallerPositive_ReturnsSmaller() {
    Assert.That(MathF.MinMagnitude(3.0f, 5.0f), Is.EqualTo(3.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_MinMagnitude_SmallerNegative_ReturnsNegative() {
    Assert.That(MathF.MinMagnitude(-3.0f, 5.0f), Is.EqualTo(-3.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_MinMagnitude_EqualMagnitude_ReturnsNegative() {
    Assert.That(MathF.MinMagnitude(-5.0f, 5.0f), Is.EqualTo(-5.0f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void MathF_MinMagnitude_WithNaN_ReturnsNaN() {
    Assert.That(float.IsNaN(MathF.MinMagnitude(float.NaN, 5.0f)), Is.True);
  }

  #endregion

  #region MathF.IEEERemainder Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void MathF_IEEERemainder_WithNaN_ReturnsNaN() {
    Assert.That(float.IsNaN(MathF.IEEERemainder(float.NaN, 3.0f)), Is.True);
    Assert.That(float.IsNaN(MathF.IEEERemainder(10.0f, float.NaN)), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void MathF_IEEERemainder_NegativeValues_WorksCorrectly() {
    var result = MathF.IEEERemainder(-10.0f, 3.0f);
    Assert.That(result, Is.EqualTo(-1.0f).Within(FloatTolerance));
  }

  #endregion

}
