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

using System.Numerics;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("BigInteger")]
public class BigIntegerTests {

  #region Static Properties Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Zero_IsZero() {
    Assert.That(BigInteger.Zero.IsZero, Is.True);
    Assert.That(BigInteger.Zero.Sign, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_One_IsOne() {
    Assert.That(BigInteger.One.IsOne, Is.True);
    Assert.That(BigInteger.One.Sign, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_MinusOne_HasNegativeSign() {
    Assert.That(BigInteger.MinusOne.Sign, Is.EqualTo(-1));
  }

  #endregion

  #region Constructor Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_FromInt32_CreatesCorrectValue() {
    var value = new BigInteger(42);
    Assert.That((int)value, Is.EqualTo(42));
    Assert.That(value.Sign, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_FromNegativeInt32_CreatesCorrectValue() {
    var value = new BigInteger(-42);
    Assert.That((int)value, Is.EqualTo(-42));
    Assert.That(value.Sign, Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_FromZero_CreatesZero() {
    var value = new BigInteger(0);
    Assert.That(value.IsZero, Is.True);
    Assert.That(value.Sign, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_FromInt64_CreatesCorrectValue() {
    var value = new BigInteger(9876543210L);
    Assert.That((long)value, Is.EqualTo(9876543210L));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_FromIntMinValue_CreatesCorrectValue() {
    var value = new BigInteger(int.MinValue);
    Assert.That((int)value, Is.EqualTo(int.MinValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_FromIntMaxValue_CreatesCorrectValue() {
    var value = new BigInteger(int.MaxValue);
    Assert.That((int)value, Is.EqualTo(int.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_FromLongMinValue_CreatesCorrectValue() {
    var value = new BigInteger(long.MinValue);
    Assert.That((long)value, Is.EqualTo(long.MinValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_FromLongMaxValue_CreatesCorrectValue() {
    var value = new BigInteger(long.MaxValue);
    Assert.That((long)value, Is.EqualTo(long.MaxValue));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_FromByteArray_CreatesCorrectValue() {
    var bytes = new byte[] { 42, 0 }; // Little-endian: 42
    var value = new BigInteger(bytes);
    Assert.That((int)value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_FromNegativeByteArray_CreatesCorrectValue() {
    // -1 in two's complement: all 1s
    var bytes = new byte[] { 0xFF };
    var value = new BigInteger(bytes);
    Assert.That((int)value, Is.EqualTo(-1));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_FromEmptyByteArray_CreatesZero() {
    var bytes = new byte[] { };
    var value = new BigInteger(bytes);
    Assert.That(value.IsZero, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void BigInteger_FromNullByteArray_ThrowsArgumentNullException() {
    byte[] nullArray = null;
    Assert.Throws<System.ArgumentNullException>(() => new BigInteger(nullArray));
  }

  #endregion

  #region Property Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_IsEven_ReturnsTrueForEvenNumber() {
    var value = new BigInteger(42);
    Assert.That(value.IsEven, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_IsEven_ReturnsFalseForOddNumber() {
    var value = new BigInteger(43);
    Assert.That(value.IsEven, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_IsEven_ReturnsTrueForZero() {
    Assert.That(BigInteger.Zero.IsEven, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_IsPowerOfTwo_ReturnsTrueForPowersOfTwo() {
    Assert.That(new BigInteger(1).IsPowerOfTwo, Is.True);
    Assert.That(new BigInteger(2).IsPowerOfTwo, Is.True);
    Assert.That(new BigInteger(4).IsPowerOfTwo, Is.True);
    Assert.That(new BigInteger(8).IsPowerOfTwo, Is.True);
    Assert.That(new BigInteger(1024).IsPowerOfTwo, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_IsPowerOfTwo_ReturnsFalseForNonPowersOfTwo() {
    Assert.That(new BigInteger(3).IsPowerOfTwo, Is.False);
    Assert.That(new BigInteger(5).IsPowerOfTwo, Is.False);
    Assert.That(new BigInteger(6).IsPowerOfTwo, Is.False);
    Assert.That(new BigInteger(1000).IsPowerOfTwo, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_IsPowerOfTwo_ReturnsFalseForZero() {
    Assert.That(BigInteger.Zero.IsPowerOfTwo, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_IsPowerOfTwo_ReturnsFalseForNegative() {
    Assert.That(new BigInteger(-4).IsPowerOfTwo, Is.False);
  }

  #endregion

  #region Arithmetic Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Add_ReturnsCorrectSum() {
    var a = new BigInteger(100);
    var b = new BigInteger(200);
    Assert.That((int)(a + b), Is.EqualTo(300));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Add_WithNegative_ReturnsCorrectSum() {
    var a = new BigInteger(100);
    var b = new BigInteger(-50);
    Assert.That((int)(a + b), Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Subtract_ReturnsCorrectDifference() {
    var a = new BigInteger(100);
    var b = new BigInteger(30);
    Assert.That((int)(a - b), Is.EqualTo(70));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Subtract_NegativeResult_ReturnsCorrectDifference() {
    var a = new BigInteger(30);
    var b = new BigInteger(100);
    Assert.That((int)(a - b), Is.EqualTo(-70));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Multiply_ReturnsCorrectProduct() {
    var a = new BigInteger(12);
    var b = new BigInteger(11);
    Assert.That((int)(a * b), Is.EqualTo(132));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Multiply_WithNegative_ReturnsCorrectProduct() {
    var a = new BigInteger(-12);
    var b = new BigInteger(11);
    Assert.That((int)(a * b), Is.EqualTo(-132));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Multiply_BothNegative_ReturnsPositive() {
    var a = new BigInteger(-12);
    var b = new BigInteger(-11);
    Assert.That((int)(a * b), Is.EqualTo(132));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Divide_ReturnsCorrectQuotient() {
    var a = new BigInteger(100);
    var b = new BigInteger(10);
    Assert.That((int)(a / b), Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Divide_TruncatesTowardsZero() {
    var a = new BigInteger(17);
    var b = new BigInteger(5);
    Assert.That((int)(a / b), Is.EqualTo(3));
  }

  [Test]
  [Category("Exception")]
  public void BigInteger_Divide_ByZero_ThrowsDivideByZeroException() {
    var a = new BigInteger(100);
    Assert.Throws<System.DivideByZeroException>(() => { var _ = a / BigInteger.Zero; });
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Remainder_ReturnsCorrectRemainder() {
    var a = new BigInteger(17);
    var b = new BigInteger(5);
    Assert.That((int)(a % b), Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Negate_ReturnsNegated() {
    var value = new BigInteger(42);
    Assert.That((int)(-value), Is.EqualTo(-42));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Negate_NegativeValue_ReturnsPositive() {
    var value = new BigInteger(-42);
    Assert.That((int)(-value), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Abs_ReturnsAbsoluteValue() {
    Assert.That((int)BigInteger.Abs(new BigInteger(-42)), Is.EqualTo(42));
    Assert.That((int)BigInteger.Abs(new BigInteger(42)), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Pow_ReturnsCorrectPower() {
    var @base = new BigInteger(2);
    Assert.That((int)BigInteger.Pow(@base, 10), Is.EqualTo(1024));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_Pow_ZeroExponent_ReturnsOne() {
    var @base = new BigInteger(12345);
    Assert.That((int)BigInteger.Pow(@base, 0), Is.EqualTo(1));
  }

  [Test]
  [Category("Exception")]
  public void BigInteger_Pow_NegativeExponent_ThrowsArgumentOutOfRangeException() {
    var @base = new BigInteger(2);
    Assert.Throws<System.ArgumentOutOfRangeException>(() => BigInteger.Pow(@base, -1));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_GCD_ReturnsGreatestCommonDivisor() {
    Assert.That((int)BigInteger.GreatestCommonDivisor(new BigInteger(48), new BigInteger(18)), Is.EqualTo(6));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_GCD_WithZero_ReturnsOtherValue() {
    Assert.That((int)BigInteger.GreatestCommonDivisor(new BigInteger(0), new BigInteger(5)), Is.EqualTo(5));
    Assert.That((int)BigInteger.GreatestCommonDivisor(new BigInteger(5), new BigInteger(0)), Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Max_ReturnsLarger() {
    var a = new BigInteger(100);
    var b = new BigInteger(200);
    Assert.That(BigInteger.Max(a, b), Is.EqualTo(b));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Min_ReturnsSmaller() {
    var a = new BigInteger(100);
    var b = new BigInteger(200);
    Assert.That(BigInteger.Min(a, b), Is.EqualTo(a));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Increment_ReturnsValuePlusOne() {
    var value = new BigInteger(41);
    Assert.That((int)(++value), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Decrement_ReturnsValueMinusOne() {
    var value = new BigInteger(43);
    Assert.That((int)(--value), Is.EqualTo(42));
  }

  #endregion

  #region Comparison Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_CompareTo_ReturnsCorrectComparison() {
    var a = new BigInteger(10);
    var b = new BigInteger(20);
    var c = new BigInteger(10);

    Assert.That(a.CompareTo(b), Is.LessThan(0));
    Assert.That(b.CompareTo(a), Is.GreaterThan(0));
    Assert.That(a.CompareTo(c), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Equals_ReturnsTrue_ForEqualValues() {
    var a = new BigInteger(42);
    var b = new BigInteger(42);
    Assert.That(a.Equals(b), Is.True);
    Assert.That(a == b, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Equals_ReturnsFalse_ForDifferentValues() {
    var a = new BigInteger(42);
    var b = new BigInteger(43);
    Assert.That(a.Equals(b), Is.False);
    Assert.That(a != b, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_LessThan_ReturnsCorrectResult() {
    var a = new BigInteger(10);
    var b = new BigInteger(20);
    Assert.That(a < b, Is.True);
    Assert.That(b < a, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_GreaterThan_ReturnsCorrectResult() {
    var a = new BigInteger(10);
    var b = new BigInteger(20);
    Assert.That(b > a, Is.True);
    Assert.That(a > b, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_LessThanOrEqual_ReturnsCorrectResult() {
    var a = new BigInteger(10);
    var b = new BigInteger(20);
    var c = new BigInteger(10);
    Assert.That(a <= b, Is.True);
    Assert.That(a <= c, Is.True);
    Assert.That(b <= a, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_GreaterThanOrEqual_ReturnsCorrectResult() {
    var a = new BigInteger(10);
    var b = new BigInteger(20);
    var c = new BigInteger(10);
    Assert.That(b >= a, Is.True);
    Assert.That(a >= c, Is.True);
    Assert.That(a >= b, Is.False);
  }

  #endregion

  #region Bitwise Operation Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_LeftShift_ReturnsCorrectResult() {
    var value = new BigInteger(1);
    Assert.That((int)(value << 4), Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_RightShift_ReturnsCorrectResult() {
    var value = new BigInteger(16);
    Assert.That((int)(value >> 2), Is.EqualTo(4));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_LeftShift_ByZero_ReturnsSameValue() {
    var value = new BigInteger(42);
    Assert.That((int)(value << 0), Is.EqualTo(42));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_RightShift_ByZero_ReturnsSameValue() {
    var value = new BigInteger(42);
    Assert.That((int)(value >> 0), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseAnd_ReturnsCorrectResult() {
    var a = new BigInteger(0b1100);
    var b = new BigInteger(0b1010);
    Assert.That((int)(a & b), Is.EqualTo(0b1000));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseAnd_WithZero_ReturnsZero() {
    var a = new BigInteger(0xFF);
    Assert.That((int)(a & BigInteger.Zero), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseAnd_WithLong_ReturnsCorrectResult() {
    var a = new BigInteger(0b11111111);
    Assert.That((int)(a & 0b00001111L), Is.EqualTo(0b00001111));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseOr_ReturnsCorrectResult() {
    var a = new BigInteger(0b1100);
    var b = new BigInteger(0b1010);
    Assert.That((int)(a | b), Is.EqualTo(0b1110));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseOr_WithZero_ReturnsSameValue() {
    var a = new BigInteger(0xFF);
    Assert.That((int)(a | BigInteger.Zero), Is.EqualTo(0xFF));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseOr_WithInt_ReturnsCorrectResult() {
    var a = new BigInteger(0b11110000);
    Assert.That((int)(a | 0b00001111), Is.EqualTo(0b11111111));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseXor_ReturnsCorrectResult() {
    var a = new BigInteger(0b1100);
    var b = new BigInteger(0b1010);
    Assert.That((int)(a ^ b), Is.EqualTo(0b0110));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseXor_WithSelf_ReturnsZero() {
    var a = new BigInteger(0xFF);
    Assert.That((int)(a ^ a), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseNot_ReturnsCorrectResult() {
    var a = new BigInteger(0);
    Assert.That((int)(~a), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseNot_OfNegativeOne_ReturnsZero() {
    var a = new BigInteger(-1);
    Assert.That((int)(~a), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseNot_OfPositive_ReturnsNegative() {
    var a = new BigInteger(5);
    Assert.That((int)(~a), Is.EqualTo(-6));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_BitwiseAnd_LargeValues_ReturnsCorrectResult() {
    var a = new BigInteger(long.MaxValue);
    var b = new BigInteger(0x00FF00FF00FF00FFL);
    var result = a & b;
    Assert.That((long)result, Is.EqualTo(0x00FF00FF00FF00FFL));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_BitwiseOr_LargeValues_ReturnsCorrectResult() {
    var a = new BigInteger(0x0F0F0F0F0F0F0F0FL);
    var b = new BigInteger(unchecked((long)0xF0F0F0F0F0F0F0F0L));
    var result = a | b;
    Assert.That(result, Is.EqualTo(BigInteger.MinusOne));
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_BitwiseXor_LargeValues_ReturnsCorrectResult() {
    var a = new BigInteger(0x0F0F0F0F0F0F0F0FL);
    var b = new BigInteger(-1);
    var result = a ^ b;
    Assert.That(result, Is.EqualTo(new BigInteger(unchecked((long)0xF0F0F0F0F0F0F0F0L))));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseAnd_NegativeValues_ReturnsCorrectResult() {
    var a = new BigInteger(-1);
    var b = new BigInteger(0xFF);
    Assert.That((int)(a & b), Is.EqualTo(0xFF));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_BitwiseOr_NegativeValues_ReturnsCorrectResult() {
    var a = new BigInteger(-256);
    var b = new BigInteger(0xFF);
    Assert.That((int)(a | b), Is.EqualTo(-1));
  }

  #endregion

  #region Parsing Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Parse_ReturnsCorrectValue() {
    var value = BigInteger.Parse("12345");
    Assert.That((int)value, Is.EqualTo(12345));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Parse_NegativeNumber_ReturnsCorrectValue() {
    var value = BigInteger.Parse("-12345");
    Assert.That((int)value, Is.EqualTo(-12345));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_Parse_WithPositiveSign_ReturnsCorrectValue() {
    var value = BigInteger.Parse("+12345");
    Assert.That((int)value, Is.EqualTo(12345));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_TryParse_ReturnsTrue_ForValidInput() {
    var success = BigInteger.TryParse("12345", out var result);
    Assert.That(success, Is.True);
    Assert.That((int)result, Is.EqualTo(12345));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_TryParse_ReturnsFalse_ForInvalidInput() {
    var success = BigInteger.TryParse("abc", out _);
    Assert.That(success, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_TryParse_EmptyString_ReturnsFalse() {
    var success = BigInteger.TryParse("", out _);
    Assert.That(success, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void BigInteger_TryParse_NullString_ReturnsFalse() {
    var success = BigInteger.TryParse(null, out _);
    Assert.That(success, Is.False);
  }

  [Test]
  [Category("Exception")]
  public void BigInteger_Parse_InvalidInput_ThrowsFormatException() {
    Assert.Throws<System.FormatException>(() => BigInteger.Parse("abc"));
  }

  #endregion

  #region ToString Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ToString_ReturnsCorrectDecimalString() {
    var value = new BigInteger(12345);
    Assert.That(value.ToString(), Is.EqualTo("12345"));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ToString_Negative_ReturnsCorrectString() {
    var value = new BigInteger(-12345);
    Assert.That(value.ToString(), Is.EqualTo("-12345"));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ToString_Zero_ReturnsZero() {
    Assert.That(BigInteger.Zero.ToString(), Is.EqualTo("0"));
  }

  #endregion

  #region ToByteArray Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ToByteArray_ReturnsCorrectBytes() {
    var value = new BigInteger(255);
    var bytes = value.ToByteArray();
    // 255 needs a sign byte to distinguish from -1
    Assert.That(bytes[0], Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ToByteArray_Zero_ReturnsZeroByte() {
    var bytes = BigInteger.Zero.ToByteArray();
    Assert.That(bytes.Length, Is.EqualTo(1));
    Assert.That(bytes[0], Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ToByteArray_Roundtrip_PreservesValue() {
    var original = new BigInteger(123456789);
    var bytes = original.ToByteArray();
    var reconstructed = new BigInteger(bytes);
    Assert.That(reconstructed, Is.EqualTo(original));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ToByteArray_NegativeRoundtrip_PreservesValue() {
    var original = new BigInteger(-123456789);
    var bytes = original.ToByteArray();
    var reconstructed = new BigInteger(bytes);
    Assert.That(reconstructed, Is.EqualTo(original));
  }

  #endregion

  #region Implicit/Explicit Conversion Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ImplicitFromInt_Works() {
    BigInteger value = 42;
    Assert.That((int)value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ImplicitFromLong_Works() {
    BigInteger value = 9876543210L;
    Assert.That((long)value, Is.EqualTo(9876543210L));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ExplicitToInt_Works() {
    var value = new BigInteger(42);
    Assert.That((int)value, Is.EqualTo(42));
  }

  [Test]
  [Category("Exception")]
  public void BigInteger_ExplicitToInt_Overflow_ThrowsOverflowException() {
    var value = new BigInteger(long.MaxValue);
    Assert.Throws<System.OverflowException>(() => { var _ = (int)value; });
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_ExplicitToDouble_Works() {
    var value = new BigInteger(12345);
    Assert.That((double)value, Is.EqualTo(12345.0).Within(0.001));
  }

  #endregion

  #region Large Number Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_LargeMultiplication_Works() {
    var a = new BigInteger(long.MaxValue);
    var b = new BigInteger(2);
    var result = a * b;
    Assert.That(result > a, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_LargeAddition_Works() {
    var a = new BigInteger(long.MaxValue);
    var result = a + a;
    Assert.That(result > a, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_VeryLargePow_Works() {
    var @base = new BigInteger(2);
    var result = BigInteger.Pow(@base, 100);
    // 2^100 is a large number, just verify it's positive and large
    Assert.That(result.Sign, Is.EqualTo(1));
    Assert.That(result > new BigInteger(long.MaxValue), Is.True);
  }

  #endregion

  #region GetHashCode Tests

  [Test]
  [Category("HappyPath")]
  public void BigInteger_GetHashCode_SameValuesSameHash() {
    var a = new BigInteger(12345);
    var b = new BigInteger(12345);
    Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
  }

  [Test]
  [Category("HappyPath")]
  public void BigInteger_GetHashCode_DifferentValuesDifferentHash() {
    var a = new BigInteger(12345);
    var b = new BigInteger(12346);
    // Note: hash collisions are possible, but unlikely for adjacent values
    Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
  }

  #endregion

  #region ConfigurableFloatingPoint-like Operations (Sign Bit Handling)

  /// <summary>
  /// Tests OR operation with sign mask - mimics ConfigurableFloatingPoint's sign bit setting.
  /// This is the exact pattern used: raw |= signMask where signMask = 1 << 7 for 8-bit.
  /// </summary>
  [Test]
  [Category("EdgeCase")]
  public void BigInteger_OrWithSignMask_8bit_PreservesSignBit() {
    // Simulate ConfigurableFloatingPoint behavior for 8-bit signed type
    var signMask = BigInteger.One << 7; // 0x80 = 128
    var exponentAndMantissa = new BigInteger(0x4F); // Some value without sign bit

    // This is what ConfigurableFloatingPoint does: raw |= signMask
    var result = exponentAndMantissa | signMask;

    // Expected: 0x4F | 0x80 = 0xCF = 207
    Assert.That((int)result, Is.EqualTo(0xCF), "OR with sign mask should set bit 7");
    Assert.That(result.Sign, Is.EqualTo(1), "Result should be positive BigInteger");
  }

  /// <summary>
  /// Tests the full cycle: set sign bit, mask to range, convert to byte, read back sign.
  /// </summary>
  [Test]
  [Category("EdgeCase")]
  public void BigInteger_SignMaskCycle_8bit_PreservesSignBit() {
    // ConfigurableFloatingPoint constants for 8-bit signed
    var signMask = BigInteger.One << 7; // 0x80
    var maxRawValue = (BigInteger.One << 8) - 1; // 0xFF = 255
    var exponentAndMantissa = new BigInteger(0x4F);

    // Step 1: Set sign bit (what FromDouble does for negative values)
    var raw = exponentAndMantissa | signMask;
    Assert.That((int)raw, Is.EqualTo(0xCF), "Step 1: OR should give 0xCF");

    // Step 2: Mask to range (constructor does this)
    var masked = raw & maxRawValue;
    Assert.That((int)masked, Is.EqualTo(0xCF), "Step 2: AND with 0xFF should preserve 0xCF");

    // Step 3: Convert to byte
    var byteValue = (byte)masked;
    Assert.That(byteValue, Is.EqualTo(0xCF), "Step 3: Conversion to byte should give 0xCF");

    // Step 4: Convert back to BigInteger
    BigInteger backToBigInt = byteValue;
    Assert.That((int)backToBigInt, Is.EqualTo(0xCF), "Step 4: Back to BigInteger should be 0xCF");

    // Step 5: Extract sign bit (what IsNegative does)
    var signBit = (backToBigInt >> 7) & 1;
    Assert.That((int)signBit, Is.EqualTo(1), "Step 5: Sign bit should be 1");
  }

  /// <summary>
  /// Tests ToByteArray for value 128 (0x80) - the sign mask value.
  /// This value needs special handling because 0x80 alone would be negative.
  /// </summary>
  [Test]
  [Category("EdgeCase")]
  public void BigInteger_ToByteArray_128_HasCorrectFormat() {
    var value = new BigInteger(128);
    var bytes = value.ToByteArray();

    // 128 = 0x80 needs two bytes: [0x80, 0x00] to be positive
    // If it were just [0x80], it would be interpreted as -128
    Assert.That(bytes.Length, Is.EqualTo(2), "128 needs 2 bytes to be positive");
    Assert.That(bytes[0], Is.EqualTo(0x80), "First byte should be 0x80");
    Assert.That(bytes[1], Is.EqualTo(0x00), "Second byte should be 0x00 (sign extension)");

    // Verify roundtrip
    var reconstructed = new BigInteger(bytes);
    Assert.That(reconstructed, Is.EqualTo(value), "Roundtrip should preserve value");
    Assert.That(reconstructed.Sign, Is.EqualTo(1), "Reconstructed value should be positive");
  }

  /// <summary>
  /// Tests OR operation where result has high bit set.
  /// The result should remain a positive BigInteger.
  /// </summary>
  [Test]
  [Category("EdgeCase")]
  public void BigInteger_Or_PositiveWithHighBitResult_RemainsPositive() {
    var a = new BigInteger(0x40); // 64
    var b = new BigInteger(0x80); // 128

    var result = a | b;

    Assert.That(result.Sign, Is.EqualTo(1), "Result of OR should be positive");
    Assert.That((int)result, Is.EqualTo(0xC0), "0x40 | 0x80 = 0xC0");
  }

  /// <summary>
  /// Tests AND operation with 0xFF mask.
  /// </summary>
  [Test]
  [Category("EdgeCase")]
  public void BigInteger_And_With0xFF_ReturnsCorrectByte() {
    var value = new BigInteger(0xCF);
    var mask = new BigInteger(0xFF);

    var result = value & mask;

    Assert.That((int)result, Is.EqualTo(0xCF), "0xCF & 0xFF = 0xCF");
    Assert.That(result.Sign, Is.EqualTo(1), "Result should be positive");
  }

  /// <summary>
  /// Tests explicit conversion to byte for values with high bit set.
  /// </summary>
  [Test]
  [Category("EdgeCase")]
  public void BigInteger_ExplicitToByte_HighBitSet_Works() {
    var value = new BigInteger(207); // 0xCF

    var byteVal = (byte)value;

    Assert.That(byteVal, Is.EqualTo(207), "Should convert to 207");
    Assert.That(byteVal, Is.EqualTo(0xCF), "Should be 0xCF");
  }

  /// <summary>
  /// Tests right shift to extract high bit.
  /// </summary>
  [Test]
  [Category("EdgeCase")]
  public void BigInteger_RightShift_ExtractsHighBit() {
    var value = new BigInteger(0xCF); // 11001111 in binary

    var highBit = (value >> 7) & 1;

    Assert.That((int)highBit, Is.EqualTo(1), "Bit 7 of 0xCF should be 1");
  }

  /// <summary>
  /// Tests the complete sign bit cycle that ConfigurableFloatingPoint uses.
  /// This test verifies OR result stays positive even when high bit is set.
  /// </summary>
  [Test]
  [Category("EdgeCase")]
  public void BigInteger_Or_BothPositive_ResultIsPositive() {
    // Test various combinations that produce high bit in result
    var testCases = new[] {
      (0x01, 0x80, 0x81),
      (0x40, 0x80, 0xC0),
      (0x7F, 0x80, 0xFF),
      (0x00, 0x80, 0x80),
      (0x4F, 0x80, 0xCF),
    };

    foreach (var (aVal, bVal, expected) in testCases) {
      var a = new BigInteger(aVal);
      var b = new BigInteger(bVal);
      var result = a | b;

      Assert.That(result.Sign, Is.EqualTo(1),
        $"OR of {aVal:X2} and {bVal:X2} should be positive");
      Assert.That((int)result, Is.EqualTo(expected),
        $"0x{aVal:X2} | 0x{bVal:X2} should equal 0x{expected:X2}");
    }
  }

  /// <summary>
  /// Tests AND operation between two positive values with high bits.
  /// </summary>
  [Test]
  [Category("EdgeCase")]
  public void BigInteger_And_BothPositive_ResultIsPositive() {
    var testCases = new[] {
      (0xCF, 0xFF, 0xCF),
      (0xFF, 0xFF, 0xFF),
      (0x80, 0xFF, 0x80),
      (0xC0, 0xF0, 0xC0),
    };

    foreach (var (aVal, bVal, expected) in testCases) {
      var a = new BigInteger(aVal);
      var b = new BigInteger(bVal);
      var result = a & b;

      Assert.That(result.Sign, Is.EqualTo(1),
        $"AND of {aVal:X2} and {bVal:X2} should be positive");
      Assert.That((int)result, Is.EqualTo(expected),
        $"0x{aVal:X2} & 0x{bVal:X2} should equal 0x{expected:X2}");
    }
  }

  /// <summary>
  /// Full integration test mimicking ConfigurableFloatingPoint<sbyte>.FromDouble(-0.5).
  /// </summary>
  [Test]
  [Category("Integration")]
  public void BigInteger_ConfigurableFloatingPoint_NegativeValue_FullCycle() {
    // Simulate a negative floating point value storage
    // For sbyte storage with 1 sign bit, 4 exponent bits, 3 mantissa bits

    // These would be the computed exponent and mantissa for some negative value
    var exponent = new BigInteger(6); // example biased exponent
    var mantissa = new BigInteger(3); // example mantissa
    var mantissaBits = 3;
    var signMask = BigInteger.One << 7; // sign bit position for 8-bit
    var maxRawValue = (BigInteger.One << 8) - 1; // 0xFF

    // Build the raw value (without sign)
    var raw = (exponent << mantissaBits) | mantissa;
    Assert.That((int)raw, Is.EqualTo((6 << 3) | 3), "Raw value before sign");
    Assert.That((int)raw, Is.EqualTo(51), "Raw = 0x33 = 51");

    // Add sign bit for negative value
    raw |= signMask;
    Assert.That((int)raw, Is.EqualTo(51 | 128), "Raw value after sign bit");
    Assert.That((int)raw, Is.EqualTo(179), "Raw = 0xB3 = 179");
    Assert.That(raw.Sign, Is.EqualTo(1), "BigInteger should still be positive");

    // Mask to storage range
    var masked = raw & maxRawValue;
    Assert.That((int)masked, Is.EqualTo(179), "Masked value should be 179");

    // Convert to storage type
    var stored = (byte)masked;
    Assert.That(stored, Is.EqualTo(179), "Stored byte should be 179");

    // Read back
    BigInteger readBack = stored;
    Assert.That((int)readBack, Is.EqualTo(179), "Read back should be 179");

    // Extract sign bit
    var signBit = (readBack >> 7) & 1;
    Assert.That((int)signBit, Is.EqualTo(1), "Sign bit should be 1 (negative)");

    // Extract exponent
    var readExponent = (readBack >> mantissaBits) & ((BigInteger.One << 4) - 1);
    Assert.That((int)readExponent, Is.EqualTo(6), "Exponent should be recovered");

    // Extract mantissa
    var readMantissa = readBack & ((BigInteger.One << mantissaBits) - 1);
    Assert.That((int)readMantissa, Is.EqualTo(3), "Mantissa should be recovered");
  }

  #endregion

}
