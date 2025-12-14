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
public class UInt128Tests {

  [Test]
  public void UInt128_Zero_IsCorrect() {
    Assert.That(UInt128.Zero, Is.EqualTo((UInt128)0));
  }

  [Test]
  public void UInt128_One_IsCorrect() {
    Assert.That(UInt128.One, Is.EqualTo((UInt128)1));
  }

  [Test]
  public void UInt128_MinValue_IsZero() {
    Assert.That(UInt128.MinValue, Is.EqualTo(UInt128.Zero));
  }

  [Test]
  public void UInt128_Addition_Works() {
    UInt128 a = 100;
    UInt128 b = 50;
    Assert.That(a + b, Is.EqualTo((UInt128)150));
  }

  [Test]
  public void UInt128_Subtraction_Works() {
    UInt128 a = 100;
    UInt128 b = 50;
    Assert.That(a - b, Is.EqualTo((UInt128)50));
  }

  [Test]
  public void UInt128_Multiplication_Works() {
    UInt128 a = 10;
    UInt128 b = 20;
    Assert.That(a * b, Is.EqualTo((UInt128)200));
  }

  [Test]
  public void UInt128_Division_Works() {
    UInt128 a = 100;
    UInt128 b = 10;
    Assert.That(a / b, Is.EqualTo((UInt128)10));
  }

  [Test]
  public void UInt128_Modulus_Works() {
    UInt128 a = 17;
    UInt128 b = 5;
    Assert.That(a % b, Is.EqualTo((UInt128)2));
  }

  [Test]
  public void UInt128_Comparison_Works() {
    UInt128 a = 100;
    UInt128 b = 200;
    Assert.That(a < b, Is.True);
    Assert.That(b > a, Is.True);
    Assert.That(a <= b, Is.True);
    Assert.That(b >= a, Is.True);
    Assert.That(a != b, Is.True);
  }

  [Test]
  public void UInt128_Equality_Works() {
    UInt128 a = 12345;
    UInt128 b = 12345;
    Assert.That(a == b, Is.True);
    Assert.That(a.Equals(b), Is.True);
  }

  [Test]
  public void UInt128_IsEvenInteger_Works() {
    Assert.That(UInt128.IsEvenInteger((UInt128)2), Is.True);
    Assert.That(UInt128.IsEvenInteger((UInt128)3), Is.False);
    Assert.That(UInt128.IsEvenInteger(UInt128.Zero), Is.True);
  }

  [Test]
  public void UInt128_IsOddInteger_Works() {
    Assert.That(UInt128.IsOddInteger((UInt128)3), Is.True);
    Assert.That(UInt128.IsOddInteger((UInt128)2), Is.False);
    Assert.That(UInt128.IsOddInteger(UInt128.Zero), Is.False);
  }

  [Test]
  public void UInt128_IsPow2_Works() {
    Assert.That(UInt128.IsPow2((UInt128)1), Is.True);
    Assert.That(UInt128.IsPow2((UInt128)2), Is.True);
    Assert.That(UInt128.IsPow2((UInt128)4), Is.True);
    Assert.That(UInt128.IsPow2((UInt128)3), Is.False);
    Assert.That(UInt128.IsPow2(UInt128.Zero), Is.False);
  }

  [Test]
  public void UInt128_Sign_Works() {
    Assert.That(UInt128.Sign((UInt128)100), Is.EqualTo(1));
    Assert.That(UInt128.Sign(UInt128.Zero), Is.EqualTo(0));
  }

  [Test]
  public void UInt128_Max_Works() {
    Assert.That(UInt128.Max((UInt128)100, (UInt128)200), Is.EqualTo((UInt128)200));
    Assert.That(UInt128.Max((UInt128)200, (UInt128)100), Is.EqualTo((UInt128)200));
  }

  [Test]
  public void UInt128_Min_Works() {
    Assert.That(UInt128.Min((UInt128)100, (UInt128)200), Is.EqualTo((UInt128)100));
    Assert.That(UInt128.Min((UInt128)200, (UInt128)100), Is.EqualTo((UInt128)100));
  }

  [Test]
  public void UInt128_Clamp_Works() {
    Assert.That(UInt128.Clamp((UInt128)50, (UInt128)0, (UInt128)100), Is.EqualTo((UInt128)50));
    Assert.That(UInt128.Clamp((UInt128)0, (UInt128)10, (UInt128)100), Is.EqualTo((UInt128)10));
    Assert.That(UInt128.Clamp((UInt128)150, (UInt128)0, (UInt128)100), Is.EqualTo((UInt128)100));
  }

  [Test]
  public void UInt128_DivRem_Works() {
    var (quotient, remainder) = UInt128.DivRem((UInt128)17, (UInt128)5);
    Assert.That(quotient, Is.EqualTo((UInt128)3));
    Assert.That(remainder, Is.EqualTo((UInt128)2));
  }

  [Test]
  public void UInt128_BitwiseAnd_Works() {
    UInt128 a = 0b1010;
    UInt128 b = 0b1100;
    Assert.That(a & b, Is.EqualTo((UInt128)0b1000));
  }

  [Test]
  public void UInt128_BitwiseOr_Works() {
    UInt128 a = 0b1010;
    UInt128 b = 0b1100;
    Assert.That(a | b, Is.EqualTo((UInt128)0b1110));
  }

  [Test]
  public void UInt128_BitwiseXor_Works() {
    UInt128 a = 0b1010;
    UInt128 b = 0b1100;
    Assert.That(a ^ b, Is.EqualTo((UInt128)0b0110));
  }

  [Test]
  public void UInt128_LeftShift_Works() {
    UInt128 a = 1;
    Assert.That(a << 4, Is.EqualTo((UInt128)16));
  }

  [Test]
  public void UInt128_RightShift_Works() {
    UInt128 a = 16;
    Assert.That(a >> 2, Is.EqualTo((UInt128)4));
  }

  [Test]
  public void UInt128_ToString_Works() {
    UInt128 a = 12345;
    Assert.That(a.ToString(), Is.EqualTo("12345"));
  }

  [Test]
  public void UInt128_Parse_Works() {
    var result = UInt128.Parse("12345");
    Assert.That(result, Is.EqualTo((UInt128)12345));
  }

  [Test]
  public void UInt128_TryParse_ValidInput_ReturnsTrue() {
    var success = UInt128.TryParse("12345", out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo((UInt128)12345));
  }

  [Test]
  public void UInt128_TryParse_InvalidInput_ReturnsFalse() {
    var success = UInt128.TryParse("not a number", out _);
    Assert.That(success, Is.False);
  }

  [Test]
  public void UInt128_CompareTo_Works() {
    UInt128 a = 100;
    UInt128 b = 200;
    Assert.That(a.CompareTo(b), Is.LessThan(0));
    Assert.That(b.CompareTo(a), Is.GreaterThan(0));
    Assert.That(a.CompareTo(a), Is.EqualTo(0));
  }

  [Test]
  public void UInt128_GetHashCode_ConsistentForEqualValues() {
    UInt128 a = 12345;
    UInt128 b = 12345;
    Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
  }

  [Test]
  public void UInt128_ImplicitConversion_FromUInt_Works() {
    UInt128 value = 42u;
    Assert.That((ulong)value, Is.EqualTo(42UL));
  }

  [Test]
  public void UInt128_ExplicitConversion_ToUInt_Works() {
    UInt128 value = 42;
    Assert.That((uint)value, Is.EqualTo(42u));
  }

  [Test]
  public void UInt128_LargeValue_Multiplication() {
    UInt128 a = ulong.MaxValue;
    UInt128 b = 2;
    var result = a * b;
    Assert.That(result > a, Is.True);
  }

}
