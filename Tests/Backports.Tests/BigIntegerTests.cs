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
    Assert.Throws<System.ArgumentNullException>(() => new BigInteger(null!));
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

}
