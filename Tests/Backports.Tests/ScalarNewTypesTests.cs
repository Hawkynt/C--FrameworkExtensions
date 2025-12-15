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

// These tests require the polyfill Vector types which include Half, UInt128, Int128 support.
// Native Vector types (FEATURE_VECTOR*_WAVE1 defined) don't include these types in IsSupported.
#if !FEATURE_VECTOR128_WAVE1

using System;
using System.Runtime.Intrinsics;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Scalar")]
public class ScalarNewTypesTests {

  #region Half - Vector64 Support

  [Test]
  [Category("HappyPath")]
  public void Vector64_Half_IsSupported() => Assert.That(Vector64<Half>.IsSupported, Is.True);

  [Test]
  [Category("HappyPath")]
  public void Vector64_Half_Count_Returns4() => Assert.That(Vector64<Half>.Count, Is.EqualTo(4));

  [Test]
  [Category("HappyPath")]
  public void Vector64_Half_Zero_AllElementsZero() {
    var vector = Vector64<Half>.Zero;

    for (var i = 0; i < Vector64<Half>.Count; ++i)
      Assert.That((float)vector.GetElement(i), Is.EqualTo(0.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Half_One_AllElementsOne() {
    var vector = Vector64<Half>.One;

    for (var i = 0; i < Vector64<Half>.Count; ++i)
      Assert.That((float)vector.GetElement(i), Is.EqualTo(1.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Half_Create_SetsAllElements() {
    var value = (Half)3.5f;
    var vector = Vector64.Create(value);

    for (var i = 0; i < Vector64<Half>.Count; ++i)
      Assert.That((float)vector.GetElement(i), Is.EqualTo(3.5f).Within(0.01f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Half_Add_AddsElements() {
    var left = Vector64.Create((Half)2.0f);
    var right = Vector64.Create((Half)3.0f);
    var result = Vector64.Add(left, right);

    for (var i = 0; i < Vector64<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(5.0f).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Half_Subtract_SubtractsElements() {
    var left = Vector64.Create((Half)5.0f);
    var right = Vector64.Create((Half)2.0f);
    var result = Vector64.Subtract(left, right);

    for (var i = 0; i < Vector64<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(3.0f).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Half_Multiply_MultipliesElements() {
    var left = Vector64.Create((Half)2.0f);
    var right = Vector64.Create((Half)3.0f);
    var result = Vector64.Multiply(left, right);

    for (var i = 0; i < Vector64<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(6.0f).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Half_Divide_DividesElements() {
    var left = Vector64.Create((Half)6.0f);
    var right = Vector64.Create((Half)2.0f);
    var result = Vector64.Divide(left, right);

    for (var i = 0; i < Vector64<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(3.0f).Within(0.1f));
  }

  #endregion

  #region Half - Vector128 Support

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_IsSupported() => Assert.That(Vector128<Half>.IsSupported, Is.True);

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Count_Returns8() => Assert.That(Vector128<Half>.Count, Is.EqualTo(8));

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Zero_AllElementsZero() {
    var vector = Vector128<Half>.Zero;

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)vector.GetElement(i), Is.EqualTo(0.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_One_AllElementsOne() {
    var vector = Vector128<Half>.One;

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)vector.GetElement(i), Is.EqualTo(1.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Create_SetsAllElements() {
    var value = (Half)2.5f;
    var vector = Vector128.Create(value);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)vector.GetElement(i), Is.EqualTo(2.5f).Within(0.01f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Add_AddsElements() {
    var left = Vector128.Create((Half)1.5f);
    var right = Vector128.Create((Half)2.5f);
    var result = Vector128.Add(left, right);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(4.0f).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Subtract_SubtractsElements() {
    var left = Vector128.Create((Half)10.0f);
    var right = Vector128.Create((Half)3.0f);
    var result = Vector128.Subtract(left, right);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(7.0f).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Multiply_MultipliesElements() {
    var left = Vector128.Create((Half)4.0f);
    var right = Vector128.Create((Half)2.5f);
    var result = Vector128.Multiply(left, right);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(10.0f).Within(0.2f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Divide_DividesElements() {
    var left = Vector128.Create((Half)12.0f);
    var right = Vector128.Create((Half)4.0f);
    var result = Vector128.Divide(left, right);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(3.0f).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Sqrt_ReturnsSqrt() {
    var vector = Vector128.Create((Half)16.0f);
    var result = Vector128.Sqrt(vector);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(4.0f).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Abs_ReturnsAbsoluteValues() {
    var vector = Vector128.Create((Half)(-5.0f));
    var result = Vector128.Abs(vector);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(5.0f).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Negate_NegatesElements() {
    var vector = Vector128.Create((Half)3.0f);
    var result = Vector128.Negate(vector);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(-3.0f).Within(0.1f));
  }

  #endregion

  #region UInt128 - Vector128 Support

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_IsSupported() => Assert.That(Vector128<UInt128>.IsSupported, Is.True);

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Count_Returns1() => Assert.That(Vector128<UInt128>.Count, Is.EqualTo(1));

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Zero_AllElementsZero() {
    var vector = Vector128<UInt128>.Zero;
    Assert.That(vector.GetElement(0), Is.EqualTo(UInt128.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_One_AllElementsOne() {
    var vector = Vector128<UInt128>.One;
    Assert.That(vector.GetElement(0), Is.EqualTo(UInt128.One));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Create_SetsElement() {
    UInt128 value = 12345;
    var vector = Vector128.Create(value);
    Assert.That(vector.GetElement(0), Is.EqualTo(value));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Add_AddsElements() {
    UInt128 left = 100;
    UInt128 right = 200;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Add(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)300));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Subtract_SubtractsElements() {
    UInt128 left = 500;
    UInt128 right = 200;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Subtract(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)300));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Multiply_MultipliesElements() {
    UInt128 left = 10;
    UInt128 right = 20;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Multiply(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)200));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Divide_DividesElements() {
    UInt128 left = 100;
    UInt128 right = 5;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Divide(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)20));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_LargeValue_Addition() {
    UInt128 large = (UInt128)ulong.MaxValue + 1;
    UInt128 small = 100;
    var leftVector = Vector128.Create(large);
    var rightVector = Vector128.Create(small);
    var result = Vector128.Add(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo(large + small));
  }

  #endregion

  #region Int128 - Vector128 Support

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_IsSupported() => Assert.That(Vector128<Int128>.IsSupported, Is.True);

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Count_Returns1() => Assert.That(Vector128<Int128>.Count, Is.EqualTo(1));

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Zero_AllElementsZero() {
    var vector = Vector128<Int128>.Zero;
    Assert.That(vector.GetElement(0), Is.EqualTo(Int128.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_One_AllElementsOne() {
    var vector = Vector128<Int128>.One;
    Assert.That(vector.GetElement(0), Is.EqualTo(Int128.One));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Create_SetsElement() {
    Int128 value = 54321;
    var vector = Vector128.Create(value);
    Assert.That(vector.GetElement(0), Is.EqualTo(value));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Create_NegativeValue_SetsElement() {
    Int128 value = -54321;
    var vector = Vector128.Create(value);
    Assert.That(vector.GetElement(0), Is.EqualTo(value));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Add_AddsElements() {
    Int128 left = 100;
    Int128 right = 200;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Add(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)300));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Add_NegativeNumbers() {
    Int128 left = -100;
    Int128 right = -200;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Add(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)(-300)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Subtract_SubtractsElements() {
    Int128 left = 500;
    Int128 right = 200;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Subtract(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)300));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Subtract_ResultsInNegative() {
    Int128 left = 100;
    Int128 right = 200;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Subtract(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)(-100)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Multiply_MultipliesElements() {
    Int128 left = 10;
    Int128 right = 20;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Multiply(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)200));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Multiply_NegativeByPositive() {
    Int128 left = -10;
    Int128 right = 20;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Multiply(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)(-200)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Divide_DividesElements() {
    Int128 left = 100;
    Int128 right = 5;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Divide(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)20));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Negate_NegatesElements() {
    Int128 value = 100;
    var vector = Vector128.Create(value);
    var result = Vector128.Negate(vector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)(-100)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Negate_NegativeValue_BecomesPositive() {
    Int128 value = -100;
    var vector = Vector128.Create(value);
    var result = Vector128.Negate(vector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)100));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Abs_ReturnsAbsoluteValue() {
    Int128 value = -100;
    var vector = Vector128.Create(value);
    var result = Vector128.Abs(vector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)100));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Abs_PositiveValue_RemainsPositive() {
    Int128 value = 100;
    var vector = Vector128.Create(value);
    var result = Vector128.Abs(vector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)100));
  }

  #endregion

  #region Half - Vector256 Support

  [Test]
  [Category("HappyPath")]
  public void Vector256_Half_IsSupported() => Assert.That(Vector256<Half>.IsSupported, Is.True);

  [Test]
  [Category("HappyPath")]
  public void Vector256_Half_Count_Returns16() => Assert.That(Vector256<Half>.Count, Is.EqualTo(16));

  [Test]
  [Category("HappyPath")]
  public void Vector256_Half_Zero_AllElementsZero() {
    var vector = Vector256<Half>.Zero;

    for (var i = 0; i < Vector256<Half>.Count; ++i)
      Assert.That((float)vector.GetElement(i), Is.EqualTo(0.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Half_One_AllElementsOne() {
    var vector = Vector256<Half>.One;

    for (var i = 0; i < Vector256<Half>.Count; ++i)
      Assert.That((float)vector.GetElement(i), Is.EqualTo(1.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Half_Add_AddsElements() {
    var left = Vector256.Create((Half)2.0f);
    var right = Vector256.Create((Half)3.0f);
    var result = Vector256.Add(left, right);

    for (var i = 0; i < Vector256<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(5.0f).Within(0.1f));
  }

  #endregion

  #region UInt128 - Vector256 Support

  [Test]
  [Category("HappyPath")]
  public void Vector256_UInt128_IsSupported() => Assert.That(Vector256<UInt128>.IsSupported, Is.True);

  [Test]
  [Category("HappyPath")]
  public void Vector256_UInt128_Count_Returns2() => Assert.That(Vector256<UInt128>.Count, Is.EqualTo(2));

  [Test]
  [Category("HappyPath")]
  public void Vector256_UInt128_Zero_AllElementsZero() {
    var vector = Vector256<UInt128>.Zero;

    for (var i = 0; i < Vector256<UInt128>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(UInt128.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_UInt128_One_AllElementsOne() {
    var vector = Vector256<UInt128>.One;

    for (var i = 0; i < Vector256<UInt128>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(UInt128.One));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_UInt128_Add_AddsElements() {
    UInt128 left = 100;
    UInt128 right = 200;
    var leftVector = Vector256.Create(left);
    var rightVector = Vector256.Create(right);
    var result = Vector256.Add(leftVector, rightVector);

    for (var i = 0; i < Vector256<UInt128>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo((UInt128)300));
  }

  #endregion

  #region Int128 - Vector256 Support

  [Test]
  [Category("HappyPath")]
  public void Vector256_Int128_IsSupported() => Assert.That(Vector256<Int128>.IsSupported, Is.True);

  [Test]
  [Category("HappyPath")]
  public void Vector256_Int128_Count_Returns2() => Assert.That(Vector256<Int128>.Count, Is.EqualTo(2));

  [Test]
  [Category("HappyPath")]
  public void Vector256_Int128_Zero_AllElementsZero() {
    var vector = Vector256<Int128>.Zero;

    for (var i = 0; i < Vector256<Int128>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(Int128.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Int128_One_AllElementsOne() {
    var vector = Vector256<Int128>.One;

    for (var i = 0; i < Vector256<Int128>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(Int128.One));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Int128_Add_AddsElements() {
    Int128 left = 100;
    Int128 right = 200;
    var leftVector = Vector256.Create(left);
    var rightVector = Vector256.Create(right);
    var result = Vector256.Add(leftVector, rightVector);

    for (var i = 0; i < Vector256<Int128>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo((Int128)300));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Int128_Negate_NegatesElements() {
    Int128 value = 100;
    var vector = Vector256.Create(value);
    var result = Vector256.Negate(vector);

    for (var i = 0; i < Vector256<Int128>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo((Int128)(-100)));
  }

  #endregion

  #region Half - Vector512 Support

  [Test]
  [Category("HappyPath")]
  public void Vector512_Half_IsSupported() => Assert.That(Vector512<Half>.IsSupported, Is.True);

  [Test]
  [Category("HappyPath")]
  public void Vector512_Half_Count_Returns32() => Assert.That(Vector512<Half>.Count, Is.EqualTo(32));

  [Test]
  [Category("HappyPath")]
  public void Vector512_Half_Zero_AllElementsZero() {
    var vector = Vector512<Half>.Zero;

    for (var i = 0; i < Vector512<Half>.Count; ++i)
      Assert.That((float)vector.GetElement(i), Is.EqualTo(0.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Half_One_AllElementsOne() {
    var vector = Vector512<Half>.One;

    for (var i = 0; i < Vector512<Half>.Count; ++i)
      Assert.That((float)vector.GetElement(i), Is.EqualTo(1.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Half_Add_AddsElements() {
    var left = Vector512.Create((Half)2.0f);
    var right = Vector512.Create((Half)3.0f);
    var result = Vector512.Add(left, right);

    for (var i = 0; i < Vector512<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(5.0f).Within(0.1f));
  }

  #endregion

  #region UInt128 - Vector512 Support

  [Test]
  [Category("HappyPath")]
  public void Vector512_UInt128_IsSupported() => Assert.That(Vector512<UInt128>.IsSupported, Is.True);

  [Test]
  [Category("HappyPath")]
  public void Vector512_UInt128_Count_Returns4() => Assert.That(Vector512<UInt128>.Count, Is.EqualTo(4));

  [Test]
  [Category("HappyPath")]
  public void Vector512_UInt128_Zero_AllElementsZero() {
    var vector = Vector512<UInt128>.Zero;

    for (var i = 0; i < Vector512<UInt128>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(UInt128.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_UInt128_One_AllElementsOne() {
    var vector = Vector512<UInt128>.One;

    for (var i = 0; i < Vector512<UInt128>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(UInt128.One));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_UInt128_Add_AddsElements() {
    UInt128 left = 100;
    UInt128 right = 200;
    var leftVector = Vector512.Create(left);
    var rightVector = Vector512.Create(right);
    var result = Vector512.Add(leftVector, rightVector);

    for (var i = 0; i < Vector512<UInt128>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo((UInt128)300));
  }

  #endregion

  #region Int128 - Vector512 Support

  [Test]
  [Category("HappyPath")]
  public void Vector512_Int128_IsSupported() => Assert.That(Vector512<Int128>.IsSupported, Is.True);

  [Test]
  [Category("HappyPath")]
  public void Vector512_Int128_Count_Returns4() => Assert.That(Vector512<Int128>.Count, Is.EqualTo(4));

  [Test]
  [Category("HappyPath")]
  public void Vector512_Int128_Zero_AllElementsZero() {
    var vector = Vector512<Int128>.Zero;

    for (var i = 0; i < Vector512<Int128>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(Int128.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Int128_One_AllElementsOne() {
    var vector = Vector512<Int128>.One;

    for (var i = 0; i < Vector512<Int128>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(Int128.One));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Int128_Add_AddsElements() {
    Int128 left = 100;
    Int128 right = 200;
    var leftVector = Vector512.Create(left);
    var rightVector = Vector512.Create(right);
    var result = Vector512.Add(leftVector, rightVector);

    for (var i = 0; i < Vector512<Int128>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo((Int128)300));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Int128_Negate_NegatesElements() {
    Int128 value = 100;
    var vector = Vector512.Create(value);
    var result = Vector512.Negate(vector);

    for (var i = 0; i < Vector512<Int128>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo((Int128)(-100)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Int128_Abs_ReturnsAbsoluteValue() {
    Int128 value = -100;
    var vector = Vector512.Create(value);
    var result = Vector512.Abs(vector);

    for (var i = 0; i < Vector512<Int128>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo((Int128)100));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void Vector128_Half_NearZero_Works() {
    var small = (Half)0.001f;
    var vector = Vector128.Create(small);
    var result = Vector128.Add(vector, vector);

    Assert.That((float)result.GetElement(0), Is.EqualTo(0.002f).Within(0.001f));
  }

  [Test]
  [Category("EdgeCase")]
  public void Vector128_Half_MaxValue_Works() {
    var maxHalf = Half.MaxValue;
    var vector = Vector128.Create(maxHalf);

    Assert.That(vector.GetElement(0), Is.EqualTo(maxHalf));
  }

  [Test]
  [Category("EdgeCase")]
  public void Vector128_UInt128_MaxValue_Works() {
    var maxValue = UInt128.MaxValue;
    var vector = Vector128.Create(maxValue);

    Assert.That(vector.GetElement(0), Is.EqualTo(maxValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void Vector128_Int128_MinValue_Works() {
    var minValue = Int128.MinValue;
    var vector = Vector128.Create(minValue);

    Assert.That(vector.GetElement(0), Is.EqualTo(minValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void Vector128_Int128_MaxValue_Works() {
    var maxValue = Int128.MaxValue;
    var vector = Vector128.Create(maxValue);

    Assert.That(vector.GetElement(0), Is.EqualTo(maxValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void Vector128_UInt128_LargeMultiplication() {
    UInt128 large = (UInt128)ulong.MaxValue;
    UInt128 small = 2;
    var leftVector = Vector128.Create(large);
    var rightVector = Vector128.Create(small);
    var result = Vector128.Multiply(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo(large * small));
  }

  [Test]
  [Category("EdgeCase")]
  public void Vector128_Int128_NegativeMultiplication() {
    Int128 negative = -1000000000000;
    Int128 positive = 2;
    var leftVector = Vector128.Create(negative);
    var rightVector = Vector128.Create(positive);
    var result = Vector128.Multiply(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)(-2000000000000)));
  }

  #endregion

  #region Comparison Operations

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_GreaterThan_ReturnsCorrectMask() {
    var left = Vector128.Create((Half)10.0f);
    var right = Vector128.Create((Half)5.0f);
    var result = Vector128.GreaterThan(left, right);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That(result.GetElement(i), Is.Not.EqualTo((Half)0.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_LessThan_ReturnsCorrectMask() {
    var left = Vector128.Create((Half)3.0f);
    var right = Vector128.Create((Half)7.0f);
    var result = Vector128.LessThan(left, right);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That(result.GetElement(i), Is.Not.EqualTo((Half)0.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_GreaterThan_ReturnsCorrectMask() {
    UInt128 left = 1000;
    UInt128 right = 500;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.GreaterThan(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.Not.EqualTo(UInt128.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_GreaterThan_ReturnsCorrectMask() {
    Int128 left = 1000;
    Int128 right = -500;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.GreaterThan(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.Not.EqualTo(Int128.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_LessThan_ReturnsCorrectMask() {
    Int128 left = -500;
    Int128 right = 1000;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.LessThan(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.Not.EqualTo(Int128.Zero));
  }

  #endregion

  #region Min/Max Operations

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Min_ReturnsMinElements() {
    var left = Vector128.Create((Half)5.0f);
    var right = Vector128.Create((Half)10.0f);
    var result = Vector128.Min(left, right);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(5.0f).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Max_ReturnsMaxElements() {
    var left = Vector128.Create((Half)5.0f);
    var right = Vector128.Create((Half)10.0f);
    var result = Vector128.Max(left, right);

    for (var i = 0; i < Vector128<Half>.Count; ++i)
      Assert.That((float)result.GetElement(i), Is.EqualTo(10.0f).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Min_ReturnsMinElements() {
    UInt128 left = 500;
    UInt128 right = 1000;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Min(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)500));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Max_ReturnsMaxElements() {
    UInt128 left = 500;
    UInt128 right = 1000;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Max(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)1000));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Min_ReturnsMinElements() {
    Int128 left = -500;
    Int128 right = 1000;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Min(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)(-500)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Max_ReturnsMaxElements() {
    Int128 left = -500;
    Int128 right = 1000;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Max(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)1000));
  }

  #endregion

  #region Bitwise Operations

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_BitwiseAnd_Works() {
    UInt128 left = 0b1111;
    UInt128 right = 0b1010;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.BitwiseAnd(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)0b1010));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_BitwiseOr_Works() {
    UInt128 left = 0b1100;
    UInt128 right = 0b1010;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.BitwiseOr(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)0b1110));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Xor_Works() {
    UInt128 left = 0b1111;
    UInt128 right = 0b1010;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.Xor(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)0b0101));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_BitwiseAnd_Works() {
    Int128 left = 0b1111;
    Int128 right = 0b1010;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.BitwiseAnd(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)0b1010));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_BitwiseOr_Works() {
    Int128 left = 0b1100;
    Int128 right = 0b1010;
    var leftVector = Vector128.Create(left);
    var rightVector = Vector128.Create(right);
    var result = Vector128.BitwiseOr(leftVector, rightVector);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)0b1110));
  }

  #endregion

  #region Shift Operations

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_ShiftLeft_Works() {
    UInt128 value = 1;
    var vector = Vector128.Create(value);
    var result = Vector128.ShiftLeft(vector, 4);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)16));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_ShiftRight_Works() {
    UInt128 value = 64;
    var vector = Vector128.Create(value);
    var result = Vector128.ShiftRightArithmetic(vector, 2);

    Assert.That(result.GetElement(0), Is.EqualTo((UInt128)16));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_ShiftLeft_Works() {
    Int128 value = 1;
    var vector = Vector128.Create(value);
    var result = Vector128.ShiftLeft(vector, 4);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)16));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_ShiftRightArithmetic_Works() {
    Int128 value = 64;
    var vector = Vector128.Create(value);
    var result = Vector128.ShiftRightArithmetic(vector, 2);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)16));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_ShiftRightArithmetic_Negative_PreservesSign() {
    Int128 value = -64;
    var vector = Vector128.Create(value);
    var result = Vector128.ShiftRightArithmetic(vector, 2);

    Assert.That(result.GetElement(0), Is.EqualTo((Int128)(-16)));
  }

  #endregion

  #region Sum and Dot Operations

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Sum_SumsAllElements() {
    var vector = Vector128.Create((Half)2.0f);
    var sum = Vector128.Sum(vector);

    Assert.That((float)sum, Is.EqualTo(16.0f).Within(0.5f)); // 2.0 * 8 = 16.0
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Half_Dot_CalculatesDotProduct() {
    var left = Vector128.Create((Half)2.0f);
    var right = Vector128.Create((Half)3.0f);
    var dot = Vector128.Dot(left, right);

    Assert.That((float)dot, Is.EqualTo(48.0f).Within(1.0f)); // (2*3) * 8 = 48
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_UInt128_Sum_SumsAllElements() {
    UInt128 value = 50;
    var vector = Vector128.Create(value);
    var sum = Vector128.Sum(vector);

    Assert.That(sum, Is.EqualTo((UInt128)50)); // 50 * 1 = 50
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Int128_Sum_SumsAllElements() {
    Int128 value = 50;
    var vector = Vector128.Create(value);
    var sum = Vector128.Sum(vector);

    Assert.That(sum, Is.EqualTo((Int128)50)); // 50 * 1 = 50
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_UInt128_Sum_SumsAllElements() {
    UInt128 value = 50;
    var vector = Vector256.Create(value);
    var sum = Vector256.Sum(vector);

    Assert.That(sum, Is.EqualTo((UInt128)100)); // 50 * 2 = 100
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Int128_Sum_SumsAllElements() {
    Int128 value = 50;
    var vector = Vector256.Create(value);
    var sum = Vector256.Sum(vector);

    Assert.That(sum, Is.EqualTo((Int128)100)); // 50 * 2 = 100
  }

  #endregion

}

#endif
