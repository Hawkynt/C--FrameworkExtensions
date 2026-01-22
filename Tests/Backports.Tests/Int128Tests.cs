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
public class Int128Tests {

  [Test]
  public void Int128_Zero_IsCorrect() {
    Assert.That(Int128.Zero, Is.EqualTo((Int128)0));
  }

  [Test]
  public void Int128_One_IsCorrect() {
    Assert.That(Int128.One, Is.EqualTo((Int128)1));
  }

  [Test]
  public void Int128_NegativeOne_IsCorrect() {
    Assert.That(Int128.NegativeOne, Is.EqualTo((Int128)(-1)));
  }

  [Test]
  public void Int128_Addition_Works() {
    Int128 a = 100;
    Int128 b = 50;
    Assert.That(a + b, Is.EqualTo((Int128)150));
  }

  [Test]
  public void Int128_Subtraction_Works() {
    Int128 a = 100;
    Int128 b = 50;
    Assert.That(a - b, Is.EqualTo((Int128)50));
  }

  [Test]
  public void Int128_Multiplication_Works() {
    Int128 a = 10;
    Int128 b = 20;
    Assert.That(a * b, Is.EqualTo((Int128)200));
  }

  [Test]
  public void Int128_Division_Works() {
    Int128 a = 100;
    Int128 b = 10;
    Assert.That(a / b, Is.EqualTo((Int128)10));
  }

  [Test]
  public void Int128_Modulus_Works() {
    Int128 a = 17;
    Int128 b = 5;
    Assert.That(a % b, Is.EqualTo((Int128)2));
  }

  [Test]
  public void Int128_Negation_Works() {
    Int128 a = 100;
    Assert.That(-a, Is.EqualTo((Int128)(-100)));
  }

  [Test]
  public void Int128_Comparison_Works() {
    Int128 a = 100;
    Int128 b = 200;
    Assert.That(a < b, Is.True);
    Assert.That(b > a, Is.True);
    Assert.That(a <= b, Is.True);
    Assert.That(b >= a, Is.True);
    Assert.That(a != b, Is.True);
  }

  [Test]
  public void Int128_Equality_Works() {
    Int128 a = 12345;
    Int128 b = 12345;
    Assert.That(a == b, Is.True);
    Assert.That(a.Equals(b), Is.True);
  }

  [Test]
  public void Int128_IsNegative_Works() {
    Assert.That(Int128.IsNegative((Int128)(-100)), Is.True);
    Assert.That(Int128.IsNegative((Int128)100), Is.False);
    Assert.That(Int128.IsNegative(Int128.Zero), Is.False);
  }

  [Test]
  public void Int128_IsPositive_Works() {
    Assert.That(Int128.IsPositive((Int128)100), Is.True);
    Assert.That(Int128.IsPositive((Int128)(-100)), Is.False);
    Assert.That(Int128.IsPositive(Int128.Zero), Is.True);
  }

  [Test]
  public void Int128_IsEvenInteger_Works() {
    Assert.That(Int128.IsEvenInteger((Int128)2), Is.True);
    Assert.That(Int128.IsEvenInteger((Int128)3), Is.False);
    Assert.That(Int128.IsEvenInteger(Int128.Zero), Is.True);
  }

  [Test]
  public void Int128_IsOddInteger_Works() {
    Assert.That(Int128.IsOddInteger((Int128)3), Is.True);
    Assert.That(Int128.IsOddInteger((Int128)2), Is.False);
    Assert.That(Int128.IsOddInteger(Int128.Zero), Is.False);
  }

  [Test]
  public void Int128_IsPow2_Works() {
    Assert.That(Int128.IsPow2((Int128)1), Is.True);
    Assert.That(Int128.IsPow2((Int128)2), Is.True);
    Assert.That(Int128.IsPow2((Int128)4), Is.True);
    Assert.That(Int128.IsPow2((Int128)3), Is.False);
    Assert.That(Int128.IsPow2(Int128.Zero), Is.False);
    Assert.That(Int128.IsPow2((Int128)(-2)), Is.False);
  }

  [Test]
  public void Int128_Abs_Works() {
    Assert.That(Int128.Abs((Int128)(-100)), Is.EqualTo((Int128)100));
    Assert.That(Int128.Abs((Int128)100), Is.EqualTo((Int128)100));
    Assert.That(Int128.Abs(Int128.Zero), Is.EqualTo(Int128.Zero));
  }

  [Test]
  public void Int128_Sign_Works() {
    Assert.That(Int128.Sign((Int128)100), Is.EqualTo(1));
    Assert.That(Int128.Sign((Int128)(-100)), Is.EqualTo(-1));
    Assert.That(Int128.Sign(Int128.Zero), Is.EqualTo(0));
  }

  [Test]
  public void Int128_Max_Works() {
    Assert.That(Int128.Max((Int128)100, (Int128)200), Is.EqualTo((Int128)200));
    Assert.That(Int128.Max((Int128)200, (Int128)100), Is.EqualTo((Int128)200));
  }

  [Test]
  public void Int128_Min_Works() {
    Assert.That(Int128.Min((Int128)100, (Int128)200), Is.EqualTo((Int128)100));
    Assert.That(Int128.Min((Int128)200, (Int128)100), Is.EqualTo((Int128)100));
  }

  [Test]
  public void Int128_Clamp_Works() {
    Assert.That(Int128.Clamp((Int128)50, (Int128)0, (Int128)100), Is.EqualTo((Int128)50));
    Assert.That(Int128.Clamp((Int128)(-10), (Int128)0, (Int128)100), Is.EqualTo((Int128)0));
    Assert.That(Int128.Clamp((Int128)150, (Int128)0, (Int128)100), Is.EqualTo((Int128)100));
  }

  [Test]
  public void Int128_DivRem_Works() {
    var (quotient, remainder) = Int128.DivRem((Int128)17, (Int128)5);
    Assert.That(quotient, Is.EqualTo((Int128)3));
    Assert.That(remainder, Is.EqualTo((Int128)2));
  }

  [Test]
  public void Int128_BitwiseAnd_Works() {
    Int128 a = 0b1010;
    Int128 b = 0b1100;
    Assert.That(a & b, Is.EqualTo((Int128)0b1000));
  }

  [Test]
  public void Int128_BitwiseOr_Works() {
    Int128 a = 0b1010;
    Int128 b = 0b1100;
    Assert.That(a | b, Is.EqualTo((Int128)0b1110));
  }

  [Test]
  public void Int128_BitwiseXor_Works() {
    Int128 a = 0b1010;
    Int128 b = 0b1100;
    Assert.That(a ^ b, Is.EqualTo((Int128)0b0110));
  }

  [Test]
  public void Int128_LeftShift_Works() {
    Int128 a = 1;
    Assert.That(a << 4, Is.EqualTo((Int128)16));
  }

  [Test]
  public void Int128_RightShift_Works() {
    Int128 a = 16;
    Assert.That(a >> 2, Is.EqualTo((Int128)4));
  }

  [Test]
  public void Int128_RightShift_Negative_SignExtends() {
    Int128 a = Int128.MinValue;
    var result = a >> 1;
    Assert.That(Int128.IsNegative(result), Is.True);
  }

  [Test]
  public void Int128_UnsignedRightShift_Works() {
    Int128 a = 16;
    Assert.That(a >>> 2, Is.EqualTo((Int128)4));
  }

  [Test]
  public void Int128_UnsignedRightShift_Negative_ZeroFills() {
    Int128 a = Int128.MinValue;
    var result = a >>> 1;
    Assert.That(Int128.IsNegative(result), Is.False);
    Assert.That(Int128.IsPositive(result), Is.True);
  }

  [Test]
  public void Int128_UnsignedRightShift_DiffersFromRightShift_ForNegative() {
    Int128 a = -1;
    Assert.That(a >> 1, Is.EqualTo((Int128)(-1)));
    Assert.That(a >>> 1, Is.Not.EqualTo(a >> 1));
    Assert.That(Int128.IsPositive(a >>> 1), Is.True);
  }

  [Test]
  public void Int128_ToString_Works() {
    Int128 a = 12345;
    Assert.That(a.ToString(), Is.EqualTo("12345"));
  }

  [Test]
  public void Int128_ToString_Negative_Works() {
    Int128 a = -12345;
    Assert.That(a.ToString(), Is.EqualTo("-12345"));
  }

  [Test]
  public void Int128_Parse_Works() {
    var result = Int128.Parse("12345");
    Assert.That(result, Is.EqualTo((Int128)12345));
  }

  [Test]
  public void Int128_Parse_Negative_Works() {
    var result = Int128.Parse("-12345");
    Assert.That(result, Is.EqualTo((Int128)(-12345)));
  }

  [Test]
  public void Int128_TryParse_ValidInput_ReturnsTrue() {
    var success = Int128.TryParse("12345", out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo((Int128)12345));
  }

  [Test]
  public void Int128_TryParse_InvalidInput_ReturnsFalse() {
    var success = Int128.TryParse("not a number", out _);
    Assert.That(success, Is.False);
  }

  [Test]
  public void Int128_CompareTo_Works() {
    Int128 a = 100;
    Int128 b = 200;
    Assert.That(a.CompareTo(b), Is.LessThan(0));
    Assert.That(b.CompareTo(a), Is.GreaterThan(0));
    Assert.That(a.CompareTo(a), Is.EqualTo(0));
  }

  [Test]
  public void Int128_GetHashCode_ConsistentForEqualValues() {
    Int128 a = 12345;
    Int128 b = 12345;
    Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
  }

  [Test]
  public void Int128_ImplicitConversion_FromInt_Works() {
    Int128 value = 42;
    Assert.That((long)value, Is.EqualTo(42));
  }

  [Test]
  public void Int128_ExplicitConversion_ToInt_Works() {
    Int128 value = 42;
    Assert.That((int)value, Is.EqualTo(42));
  }

  [Test]
  public void Int128_LargeValue_Multiplication() {
    Int128 a = long.MaxValue;
    Int128 b = 2;
    var result = a * b;
    Assert.That(result > a, Is.True);
  }

}
