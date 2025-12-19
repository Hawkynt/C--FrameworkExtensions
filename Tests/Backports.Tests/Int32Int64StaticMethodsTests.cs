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
[Category("NumericStatic")]
public class Int32Int64StaticMethodsTests {

  #region Int32.Abs

  [Test]
  [Category("HappyPath")]
  public void Int32_Abs_PositiveValue_ReturnsSame() {
    var result = int.Abs(5);
    Assert.That(result, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_Abs_NegativeValue_ReturnsPositive() {
    var result = int.Abs(-5);
    Assert.That(result, Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_Abs_Zero_ReturnsZero() {
    var result = int.Abs(0);
    Assert.That(result, Is.EqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void Int32_Abs_MinValue_ThrowsOverflow() {
    Assert.Throws<OverflowException>(() => int.Abs(int.MinValue));
  }

  #endregion

  #region Int32.IsEvenInteger / IsOddInteger

  [Test]
  [Category("HappyPath")]
  public void Int32_IsEvenInteger_EvenValue_ReturnsTrue() {
    Assert.That(int.IsEvenInteger(4), Is.True);
    Assert.That(int.IsEvenInteger(0), Is.True);
    Assert.That(int.IsEvenInteger(-2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_IsEvenInteger_OddValue_ReturnsFalse() {
    Assert.That(int.IsEvenInteger(3), Is.False);
    Assert.That(int.IsEvenInteger(-1), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_IsOddInteger_OddValue_ReturnsTrue() {
    Assert.That(int.IsOddInteger(3), Is.True);
    Assert.That(int.IsOddInteger(-1), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_IsOddInteger_EvenValue_ReturnsFalse() {
    Assert.That(int.IsOddInteger(4), Is.False);
    Assert.That(int.IsOddInteger(0), Is.False);
  }

  #endregion

  #region Int32.IsNegative / IsPositive

  [Test]
  [Category("HappyPath")]
  public void Int32_IsNegative_NegativeValue_ReturnsTrue() {
    Assert.That(int.IsNegative(-1), Is.True);
    Assert.That(int.IsNegative(int.MinValue), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_IsNegative_NonNegativeValue_ReturnsFalse() {
    Assert.That(int.IsNegative(0), Is.False);
    Assert.That(int.IsNegative(1), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_IsPositive_PositiveValue_ReturnsTrue() {
    Assert.That(int.IsPositive(1), Is.True);
    Assert.That(int.IsPositive(int.MaxValue), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_IsPositive_ZeroOrPositive_ReturnsTrue() {
    Assert.That(int.IsPositive(0), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_IsPositive_NegativeValue_ReturnsFalse() {
    Assert.That(int.IsPositive(-1), Is.False);
    Assert.That(int.IsPositive(int.MinValue), Is.False);
  }

  #endregion

  #region Int32.DivRem

  [Test]
  [Category("HappyPath")]
  public void Int32_DivRem_ReturnsQuotientAndRemainder() {
    var (q, r) = int.DivRem(17, 5);
    Assert.That(q, Is.EqualTo(3));
    Assert.That(r, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_DivRem_ExactDivision_ZeroRemainder() {
    var (q, r) = int.DivRem(20, 5);
    Assert.That(q, Is.EqualTo(4));
    Assert.That(r, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_DivRem_NegativeDividend_CorrectResult() {
    var (q, r) = int.DivRem(-17, 5);
    Assert.That(q, Is.EqualTo(-3));
    Assert.That(r, Is.EqualTo(-2));
  }

  #endregion

  #region Int32.Min / Max

  [Test]
  [Category("HappyPath")]
  public void Int32_Max_ReturnsGreater() {
    Assert.That(int.Max(5, 10), Is.EqualTo(10));
    Assert.That(int.Max(10, 5), Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_Min_ReturnsLesser() {
    Assert.That(int.Min(5, 10), Is.EqualTo(5));
    Assert.That(int.Min(10, 5), Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_Max_EqualValues_ReturnsValue() {
    Assert.That(int.Max(5, 5), Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_Min_EqualValues_ReturnsValue() {
    Assert.That(int.Min(5, 5), Is.EqualTo(5));
  }

  #endregion

  #region Int32.MaxMagnitude / MinMagnitude

  [Test]
  [Category("HappyPath")]
  public void Int32_MaxMagnitude_ReturnsLargerAbsolute() {
    Assert.That(int.MaxMagnitude(5, -10), Is.EqualTo(-10));
    Assert.That(int.MaxMagnitude(-5, 10), Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_MinMagnitude_ReturnsSmallerAbsolute() {
    Assert.That(int.MinMagnitude(5, -10), Is.EqualTo(5));
    Assert.That(int.MinMagnitude(-5, 10), Is.EqualTo(-5));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_MaxMagnitude_EqualMagnitude_ReturnsGreater() {
    Assert.That(int.MaxMagnitude(-5, 5), Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_MinMagnitude_EqualMagnitude_ReturnsLesser() {
    Assert.That(int.MinMagnitude(-5, 5), Is.EqualTo(-5));
  }

  #endregion

  #region Int32.Sign

  [Test]
  [Category("HappyPath")]
  public void Int32_Sign_PositiveValue_ReturnsOne() {
    Assert.That(int.Sign(5), Is.EqualTo(1));
    Assert.That(int.Sign(int.MaxValue), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_Sign_NegativeValue_ReturnsNegativeOne() {
    Assert.That(int.Sign(-5), Is.EqualTo(-1));
    Assert.That(int.Sign(int.MinValue), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_Sign_Zero_ReturnsZero() {
    Assert.That(int.Sign(0), Is.EqualTo(0));
  }

  #endregion

  #region Int32.LeadingZeroCount / PopCount

  [Test]
  [Category("HappyPath")]
  public void Int32_LeadingZeroCount_PowerOfTwo_ReturnsCorrect() {
    Assert.That(int.LeadingZeroCount(1), Is.EqualTo(31));
    Assert.That(int.LeadingZeroCount(2), Is.EqualTo(30));
    Assert.That(int.LeadingZeroCount(4), Is.EqualTo(29));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_LeadingZeroCount_Zero_Returns32() {
    Assert.That(int.LeadingZeroCount(0), Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_PopCount_ReturnsSetBitCount() {
    Assert.That(int.PopCount(0), Is.EqualTo(0));
    Assert.That(int.PopCount(1), Is.EqualTo(1));
    Assert.That(int.PopCount(3), Is.EqualTo(2));
    Assert.That(int.PopCount(7), Is.EqualTo(3));
    Assert.That(int.PopCount(255), Is.EqualTo(8));
  }

  #endregion

  #region Int32.RotateLeft / RotateRight

  [Test]
  [Category("HappyPath")]
  public void Int32_RotateLeft_RotatesCorrectly() {
    var result = int.RotateLeft(1, 1);
    Assert.That(result, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_RotateRight_RotatesCorrectly() {
    var result = int.RotateRight(2, 1);
    Assert.That(result, Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_RotateLeft_WrapAround_RotatesCorrectly() {
    var result = int.RotateLeft(unchecked((int)0x80000000), 1);
    Assert.That(result, Is.EqualTo(1));
  }

  #endregion

  #region Int32.Clamp / CopySign

  [Test]
  [Category("HappyPath")]
  public void Int32_Clamp_ValueInRange_ReturnsValue() {
    Assert.That(int.Clamp(5, 0, 10), Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_Clamp_ValueBelowMin_ReturnsMin() {
    Assert.That(int.Clamp(-5, 0, 10), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_Clamp_ValueAboveMax_ReturnsMax() {
    Assert.That(int.Clamp(15, 0, 10), Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_CopySign_CopiesSign() {
    Assert.That(int.CopySign(5, -1), Is.EqualTo(-5));
    Assert.That(int.CopySign(-5, 1), Is.EqualTo(5));
    Assert.That(int.CopySign(5, 1), Is.EqualTo(5));
    Assert.That(int.CopySign(-5, -1), Is.EqualTo(-5));
  }

  #endregion

  #region Int64.Abs

  [Test]
  [Category("HappyPath")]
  public void Int64_Abs_PositiveValue_ReturnsSame() {
    var result = long.Abs(5L);
    Assert.That(result, Is.EqualTo(5L));
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_Abs_NegativeValue_ReturnsPositive() {
    var result = long.Abs(-5L);
    Assert.That(result, Is.EqualTo(5L));
  }

  [Test]
  [Category("Exception")]
  public void Int64_Abs_MinValue_ThrowsOverflow() {
    Assert.Throws<OverflowException>(() => long.Abs(long.MinValue));
  }

  #endregion

  #region Int64.IsEvenInteger / IsOddInteger / IsNegative / IsPositive

  [Test]
  [Category("HappyPath")]
  public void Int64_IsEvenInteger_EvenValue_ReturnsTrue() {
    Assert.That(long.IsEvenInteger(4L), Is.True);
    Assert.That(long.IsEvenInteger(0L), Is.True);
    Assert.That(long.IsEvenInteger(-2L), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_IsOddInteger_OddValue_ReturnsTrue() {
    Assert.That(long.IsOddInteger(3L), Is.True);
    Assert.That(long.IsOddInteger(-1L), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_IsNegative_NegativeValue_ReturnsTrue() {
    Assert.That(long.IsNegative(-1L), Is.True);
    Assert.That(long.IsNegative(long.MinValue), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_IsPositive_PositiveValue_ReturnsTrue() {
    Assert.That(long.IsPositive(1L), Is.True);
    Assert.That(long.IsPositive(long.MaxValue), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_IsPositive_ZeroOrPositive_ReturnsTrue() {
    Assert.That(long.IsPositive(0L), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_IsPositive_NegativeValue_ReturnsFalse() {
    Assert.That(long.IsPositive(-1L), Is.False);
    Assert.That(long.IsPositive(long.MinValue), Is.False);
  }

  #endregion

  #region Int64.DivRem

  [Test]
  [Category("HappyPath")]
  public void Int64_DivRem_ReturnsQuotientAndRemainder() {
    var (q, r) = long.DivRem(17L, 5L);
    Assert.That(q, Is.EqualTo(3L));
    Assert.That(r, Is.EqualTo(2L));
  }

  #endregion

  #region Int64.Min / Max / Sign

  [Test]
  [Category("HappyPath")]
  public void Int64_Max_ReturnsGreater() {
    Assert.That(long.Max(5L, 10L), Is.EqualTo(10L));
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_Min_ReturnsLesser() {
    Assert.That(long.Min(5L, 10L), Is.EqualTo(5L));
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_Sign_ReturnsCorrectSign() {
    Assert.That(long.Sign(5L), Is.EqualTo(1));
    Assert.That(long.Sign(-5L), Is.EqualTo(-1));
    Assert.That(long.Sign(0L), Is.EqualTo(0));
  }

  #endregion

  #region Int64.LeadingZeroCount / PopCount

  [Test]
  [Category("HappyPath")]
  public void Int64_LeadingZeroCount_PowerOfTwo_ReturnsCorrect() {
    Assert.That(long.LeadingZeroCount(1L), Is.EqualTo(63));
    Assert.That(long.LeadingZeroCount(2L), Is.EqualTo(62));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int64_LeadingZeroCount_Zero_Returns64() {
    Assert.That(long.LeadingZeroCount(0L), Is.EqualTo(64));
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_PopCount_ReturnsSetBitCount() {
    Assert.That(long.PopCount(0L), Is.EqualTo(0));
    Assert.That(long.PopCount(1L), Is.EqualTo(1));
    Assert.That(long.PopCount(255L), Is.EqualTo(8));
  }

  #endregion

}
