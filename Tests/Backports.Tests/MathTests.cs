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
[Category("Math")]
public class MathTests {

  #region Math.Clamp (Int32)

  [Test]
  [Category("HappyPath")]
  public void Clamp_Int_ValueWithinRange_ReturnsValue() {
    var result = Math.Clamp(5, 0, 10);
    Assert.That(result, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Int_ValueBelowMin_ReturnsMin() {
    var result = Math.Clamp(-5, 0, 10);
    Assert.That(result, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Int_ValueAboveMax_ReturnsMax() {
    var result = Math.Clamp(15, 0, 10);
    Assert.That(result, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void Clamp_Int_ValueEqualsMin_ReturnsMin() {
    var result = Math.Clamp(0, 0, 10);
    Assert.That(result, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Clamp_Int_ValueEqualsMax_ReturnsMax() {
    var result = Math.Clamp(10, 0, 10);
    Assert.That(result, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void Clamp_Int_MinEqualsMax_ReturnsMinMax() {
    var result = Math.Clamp(5, 3, 3);
    Assert.That(result, Is.EqualTo(3));
  }

  #endregion

  #region Math.Clamp (Double)

  [Test]
  [Category("HappyPath")]
  public void Clamp_Double_ValueWithinRange_ReturnsValue() {
    var result = Math.Clamp(5.5, 0.0, 10.0);
    Assert.That(result, Is.EqualTo(5.5));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Double_ValueBelowMin_ReturnsMin() {
    var result = Math.Clamp(-5.5, 0.0, 10.0);
    Assert.That(result, Is.EqualTo(0.0));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Double_ValueAboveMax_ReturnsMax() {
    var result = Math.Clamp(15.5, 0.0, 10.0);
    Assert.That(result, Is.EqualTo(10.0));
  }

  #endregion

  #region Math.Clamp (Single)

  [Test]
  [Category("HappyPath")]
  public void Clamp_Single_ValueWithinRange_ReturnsValue() {
    var result = Math.Clamp(5.5f, 0.0f, 10.0f);
    Assert.That(result, Is.EqualTo(5.5f));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Single_ValueBelowMin_ReturnsMin() {
    var result = Math.Clamp(-5.5f, 0.0f, 10.0f);
    Assert.That(result, Is.EqualTo(0.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Single_ValueAboveMax_ReturnsMax() {
    var result = Math.Clamp(15.5f, 0.0f, 10.0f);
    Assert.That(result, Is.EqualTo(10.0f));
  }

  #endregion

  #region Math.Clamp (Long)

  [Test]
  [Category("HappyPath")]
  public void Clamp_Long_ValueWithinRange_ReturnsValue() {
    var result = Math.Clamp(5L, 0L, 10L);
    Assert.That(result, Is.EqualTo(5L));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Long_ValueBelowMin_ReturnsMin() {
    var result = Math.Clamp(-5L, 0L, 10L);
    Assert.That(result, Is.EqualTo(0L));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Long_ValueAboveMax_ReturnsMax() {
    var result = Math.Clamp(15L, 0L, 10L);
    Assert.That(result, Is.EqualTo(10L));
  }

  #endregion

  #region Math.Clamp (Byte)

  [Test]
  [Category("HappyPath")]
  public void Clamp_Byte_ValueWithinRange_ReturnsValue() {
    var result = Math.Clamp((byte)50, (byte)0, (byte)100);
    Assert.That(result, Is.EqualTo((byte)50));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Byte_ValueBelowMin_ReturnsMin() {
    var result = Math.Clamp((byte)0, (byte)10, (byte)100);
    Assert.That(result, Is.EqualTo((byte)10));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Byte_ValueAboveMax_ReturnsMax() {
    var result = Math.Clamp((byte)150, (byte)0, (byte)100);
    Assert.That(result, Is.EqualTo((byte)100));
  }

  #endregion

  #region Math.CopySign (Double)

  [Test]
  [Category("HappyPath")]
  public void CopySign_Double_PositiveToPositive_ReturnsPositive() {
    var result = Math.CopySign(5.0, 1.0);
    Assert.That(result, Is.EqualTo(5.0));
  }

  [Test]
  [Category("HappyPath")]
  public void CopySign_Double_PositiveToNegative_ReturnsNegative() {
    var result = Math.CopySign(5.0, -1.0);
    Assert.That(result, Is.EqualTo(-5.0));
  }

  [Test]
  [Category("HappyPath")]
  public void CopySign_Double_NegativeToPositive_ReturnsPositive() {
    var result = Math.CopySign(-5.0, 1.0);
    Assert.That(result, Is.EqualTo(5.0));
  }

  [Test]
  [Category("HappyPath")]
  public void CopySign_Double_NegativeToNegative_ReturnsNegative() {
    var result = Math.CopySign(-5.0, -1.0);
    Assert.That(result, Is.EqualTo(-5.0));
  }

  [Test]
  [Category("EdgeCase")]
  public void CopySign_Double_Zero_PreservesSign() {
    var result = Math.CopySign(0.0, -1.0);
    // Check for negative zero by examining the sign bit
    var bits = BitConverter.DoubleToInt64Bits(result);
    Assert.That(bits < 0, Is.True);
  }

  #endregion

  #region Math.CopySign (Single)

  [Test]
  [Category("HappyPath")]
  public void CopySign_Single_PositiveToNegative_ReturnsNegative() {
    var result = Math.CopySign(5.0f, -1.0f);
    Assert.That(result, Is.EqualTo(-5.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void CopySign_Single_NegativeToPositive_ReturnsPositive() {
    var result = Math.CopySign(-5.0f, 1.0f);
    Assert.That(result, Is.EqualTo(5.0f));
  }

  #endregion

  #region Math.ScaleB

  [Test]
  [Category("HappyPath")]
  public void ScaleB_MultiplyByPowerOfTwo_ReturnsCorrectResult() {
    var result = Math.ScaleB(1.0, 3);
    Assert.That(result, Is.EqualTo(8.0));
  }

  [Test]
  [Category("HappyPath")]
  public void ScaleB_DivideByPowerOfTwo_ReturnsCorrectResult() {
    var result = Math.ScaleB(8.0, -3);
    Assert.That(result, Is.EqualTo(1.0));
  }

  [Test]
  [Category("EdgeCase")]
  public void ScaleB_ZeroExponent_ReturnsOriginal() {
    var result = Math.ScaleB(3.14, 0);
    Assert.That(result, Is.EqualTo(3.14));
  }

  [Test]
  [Category("EdgeCase")]
  public void ScaleB_ZeroValue_ReturnsZero() {
    var result = Math.ScaleB(0.0, 10);
    Assert.That(result, Is.EqualTo(0.0));
  }

  #endregion

  #region Math.ILogB

  [Test]
  [Category("HappyPath")]
  public void ILogB_PowerOfTwo_ReturnsExponent() {
    var result = Math.ILogB(8.0);
    Assert.That(result, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ILogB_One_ReturnsZero() {
    var result = Math.ILogB(1.0);
    Assert.That(result, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ILogB_FractionalValue_ReturnsNegative() {
    var result = Math.ILogB(0.5);
    Assert.That(result, Is.EqualTo(-1));
  }

  [Test]
  [Category("EdgeCase")]
  public void ILogB_Zero_ReturnsMinValue() {
    var result = Math.ILogB(0.0);
    Assert.That(result, Is.EqualTo(int.MinValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void ILogB_PositiveInfinity_ReturnsMaxValue() {
    var result = Math.ILogB(double.PositiveInfinity);
    Assert.That(result, Is.EqualTo(int.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void ILogB_NaN_ReturnsExtremeValue() {
    var result = Math.ILogB(double.NaN);
    // Native .NET 7+ returns int.MaxValue, older versions and polyfill return int.MinValue
    Assert.That(result == int.MinValue || result == int.MaxValue, Is.True);
  }

  #endregion

  #region Math.ReciprocalEstimate

  [Test]
  [Category("HappyPath")]
  public void ReciprocalEstimate_PositiveNumber_ReturnsReciprocal() {
    var result = Math.ReciprocalEstimate(4.0);
    Assert.That(result, Is.EqualTo(0.25));
  }

  [Test]
  [Category("HappyPath")]
  public void ReciprocalEstimate_One_ReturnsOne() {
    var result = Math.ReciprocalEstimate(1.0);
    Assert.That(result, Is.EqualTo(1.0));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReciprocalEstimate_Zero_ReturnsInfinity() {
    var result = Math.ReciprocalEstimate(0.0);
    Assert.That(double.IsInfinity(result), Is.True);
  }

  #endregion

  #region Math.ReciprocalSqrtEstimate

  [Test]
  [Category("HappyPath")]
  public void ReciprocalSqrtEstimate_Four_ReturnsHalf() {
    var result = Math.ReciprocalSqrtEstimate(4.0);
    Assert.That(result, Is.EqualTo(0.5));
  }

  [Test]
  [Category("HappyPath")]
  public void ReciprocalSqrtEstimate_One_ReturnsOne() {
    var result = Math.ReciprocalSqrtEstimate(1.0);
    Assert.That(result, Is.EqualTo(1.0));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReciprocalSqrtEstimate_Zero_ReturnsInfinity() {
    var result = Math.ReciprocalSqrtEstimate(0.0);
    Assert.That(double.IsInfinity(result), Is.True);
  }

  #endregion

  #region Math.BigMul (Int128)

  [Test]
  [Category("HappyPath")]
  public void BigMul_Long_PositiveNumbers_ReturnsCorrectResult() {
    var result = Math.BigMul(1000000L, 1000000L);
    Assert.That((long)result, Is.EqualTo(1000000000000L));
  }

  [Test]
  [Category("HappyPath")]
  public void BigMul_Long_WithNegative_ReturnsCorrectResult() {
    var result = Math.BigMul(-100L, 100L);
    Assert.That((long)result, Is.EqualTo(-10000L));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigMul_Long_MaxValues_DoesNotOverflow() {
    var result = Math.BigMul(long.MaxValue, 2L);
    // The result should be larger than what fits in long
    Assert.That(result > long.MaxValue, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void BigMul_Long_Zero_ReturnsZero() {
    var result = Math.BigMul(12345L, 0L);
    Assert.That((long)result, Is.EqualTo(0L));
  }

  [Test]
  [Category("HappyPath")]
  public void BigMul_ULong_PositiveNumbers_ReturnsCorrectResult() {
    var result = Math.BigMul(1000000UL, 1000000UL);
    Assert.That((ulong)result, Is.EqualTo(1000000000000UL));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigMul_ULong_MaxValues_DoesNotOverflow() {
    var result = Math.BigMul(ulong.MaxValue, 2UL);
    // The result should be larger than what fits in ulong
    Assert.That(result > ulong.MaxValue, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void BigMul_Long_WithOutLow_ReturnsHighAndLow() {
    var high = Math.BigMul(0x100000000L, 0x100000000L, out var low);
    // 2^32 * 2^32 = 2^64, so high = 1, low = 0
    Assert.That(high, Is.EqualTo(1L));
    Assert.That(low, Is.EqualTo(0L));
  }

  [Test]
  [Category("HappyPath")]
  public void BigMul_ULong_WithOutLow_ReturnsHighAndLow() {
    var high = Math.BigMul(0x100000000UL, 0x100000000UL, out var low);
    // 2^32 * 2^32 = 2^64, so high = 1, low = 0
    Assert.That(high, Is.EqualTo(1UL));
    Assert.That(low, Is.EqualTo(0UL));
  }

  #endregion

  #region Math.FusedMultiplyAdd

  [Test]
  [Category("HappyPath")]
  public void FusedMultiplyAdd_Double_BasicOperation_ReturnsCorrectResult() {
    var result = Math.FusedMultiplyAdd(2.0, 3.0, 4.0);
    Assert.That(result, Is.EqualTo(10.0));
  }

  [Test]
  [Category("HappyPath")]
  public void FusedMultiplyAdd_Double_WithNegatives_ReturnsCorrectResult() {
    var result = Math.FusedMultiplyAdd(-2.0, 3.0, 10.0);
    Assert.That(result, Is.EqualTo(4.0));
  }

  [Test]
  [Category("EdgeCase")]
  public void FusedMultiplyAdd_Double_ZeroMultiplier_ReturnsAddend() {
    var result = Math.FusedMultiplyAdd(0.0, 100.0, 5.0);
    Assert.That(result, Is.EqualTo(5.0));
  }

  [Test]
  [Category("HappyPath")]
  public void FusedMultiplyAdd_Float_BasicOperation_ReturnsCorrectResult() {
    var result = MathF.FusedMultiplyAdd(2.0f, 3.0f, 4.0f);
    Assert.That(result, Is.EqualTo(10.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void FusedMultiplyAdd_Float_WithNegatives_ReturnsCorrectResult() {
    var result = MathF.FusedMultiplyAdd(-2.0f, 3.0f, 10.0f);
    Assert.That(result, Is.EqualTo(4.0f));
  }

  #endregion

  #region Math.Asinh (Inverse Hyperbolic Sine)

  [Test]
  [Category("HappyPath")]
  public void Asinh_Double_PositiveNumber_ReturnsCorrectResult() {
    var result = Math.Asinh(1.0);
    Assert.That(result, Is.EqualTo(0.881373587).Within(0.0001));
  }

  [Test]
  [Category("HappyPath")]
  public void Asinh_Double_NegativeNumber_ReturnsCorrectResult() {
    var result = Math.Asinh(-1.0);
    Assert.That(result, Is.EqualTo(-0.881373587).Within(0.0001));
  }

  [Test]
  [Category("EdgeCase")]
  public void Asinh_Double_Zero_ReturnsZero() {
    var result = Math.Asinh(0.0);
    Assert.That(result, Is.EqualTo(0.0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Asinh_Double_PositiveInfinity_ReturnsPositiveInfinity() {
    var result = Math.Asinh(double.PositiveInfinity);
    Assert.That(double.IsPositiveInfinity(result), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Asinh_Double_NegativeInfinity_ReturnsNegativeInfinity() {
    var result = Math.Asinh(double.NegativeInfinity);
    Assert.That(double.IsNegativeInfinity(result), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Asinh_Double_NaN_ReturnsNaN() {
    var result = Math.Asinh(double.NaN);
    Assert.That(double.IsNaN(result), Is.True);
  }

  #endregion

  #region Math.Acosh (Inverse Hyperbolic Cosine)

  [Test]
  [Category("HappyPath")]
  public void Acosh_Double_GreaterThanOne_ReturnsCorrectResult() {
    var result = Math.Acosh(2.0);
    Assert.That(result, Is.EqualTo(1.31695789).Within(0.0001));
  }

  [Test]
  [Category("EdgeCase")]
  public void Acosh_Double_One_ReturnsZero() {
    var result = Math.Acosh(1.0);
    Assert.That(result, Is.EqualTo(0.0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Acosh_Double_LessThanOne_ReturnsNaN() {
    var result = Math.Acosh(0.5);
    Assert.That(double.IsNaN(result), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Acosh_Double_PositiveInfinity_ReturnsPositiveInfinity() {
    var result = Math.Acosh(double.PositiveInfinity);
    Assert.That(double.IsPositiveInfinity(result), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Acosh_Double_NaN_ReturnsNaN() {
    var result = Math.Acosh(double.NaN);
    Assert.That(double.IsNaN(result), Is.True);
  }

  #endregion

  #region Math.Atanh (Inverse Hyperbolic Tangent)

  [Test]
  [Category("HappyPath")]
  public void Atanh_Double_ValueInRange_ReturnsCorrectResult() {
    var result = Math.Atanh(0.5);
    Assert.That(result, Is.EqualTo(0.549306144).Within(0.0001));
  }

  [Test]
  [Category("EdgeCase")]
  public void Atanh_Double_Zero_ReturnsZero() {
    var result = Math.Atanh(0.0);
    Assert.That(result, Is.EqualTo(0.0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Atanh_Double_One_ReturnsPositiveInfinity() {
    var result = Math.Atanh(1.0);
    Assert.That(double.IsPositiveInfinity(result), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Atanh_Double_MinusOne_ReturnsNegativeInfinity() {
    var result = Math.Atanh(-1.0);
    Assert.That(double.IsNegativeInfinity(result), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Atanh_Double_OutOfRange_ReturnsNaN() {
    var result = Math.Atanh(2.0);
    Assert.That(double.IsNaN(result), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Atanh_Double_NaN_ReturnsNaN() {
    var result = Math.Atanh(double.NaN);
    Assert.That(double.IsNaN(result), Is.True);
  }

  #endregion

}
