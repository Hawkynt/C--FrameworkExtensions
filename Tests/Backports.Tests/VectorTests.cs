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

using System.Runtime.Intrinsics;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Vector")]
public class VectorTests {

  #region Vector64<T> - Basic Properties

  [Test]
  [Category("HappyPath")]
  public void Vector64_Count_Byte_Returns8() => Assert.That(Vector64<byte>.Count, Is.EqualTo(8));

  [Test]
  [Category("HappyPath")]
  public void Vector64_Count_Int_Returns2() => Assert.That(Vector64<int>.Count, Is.EqualTo(2));

  [Test]
  [Category("HappyPath")]
  public void Vector64_Count_Long_Returns1() => Assert.That(Vector64<long>.Count, Is.EqualTo(1));

  [Test]
  [Category("HappyPath")]
  public void Vector64_Zero_AllElementsZero() {
    var vector = Vector64<int>.Zero;

    Assert.That(vector.GetElement(0), Is.EqualTo(0));
    Assert.That(vector.GetElement(1), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_One_AllElementsOne() {
    var vector = Vector64.Create(1);

    Assert.That(vector.GetElement(0), Is.EqualTo(1));
    Assert.That(vector.GetElement(1), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_AllBitsSet_AllBitsAreSet() {
    var vector = Vector64<int>.AllBitsSet;

    Assert.That(vector.GetElement(0), Is.EqualTo(-1));
    Assert.That(vector.GetElement(1), Is.EqualTo(-1));
  }

  #endregion

  #region Vector64<T> - Construction

  [Test]
  [Category("HappyPath")]
  public void Vector64_Create_SetsAllElements() {
    var vector = Vector64.Create(42);

    Assert.That(vector.GetElement(0), Is.EqualTo(42));
    Assert.That(vector.GetElement(1), Is.EqualTo(42));
  }

  #endregion

  #region Vector64<T> - Arithmetic Operations

  [Test]
  [Category("HappyPath")]
  public void Vector64_Add_AddsElements() {
    var left = Vector64.Create(10);
    var right = Vector64.Create(5);
    var result = Vector64.Add<int>(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(15));
    Assert.That(result.GetElement(1), Is.EqualTo(15));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Subtract_SubtractsElements() {
    var left = Vector64.Create(10);
    var right = Vector64.Create(3);
    var result = Vector64.Subtract(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(7));
    Assert.That(result.GetElement(1), Is.EqualTo(7));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Multiply_MultipliesElements() {
    var left = Vector64.Create(4);
    var right = Vector64.Create(3);
    var result = Vector64.Multiply(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(12));
    Assert.That(result.GetElement(1), Is.EqualTo(12));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Divide_DividesElements() {
    var left = Vector64.Create(12.0f);
    var right = Vector64.Create(3.0f);
    var result = Vector64.Divide(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(4.0f));
    Assert.That(result.GetElement(1), Is.EqualTo(4.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Negate_NegatesElements() {
    var vector = Vector64.Create(5);
    var result = Vector64.Negate(vector);

    Assert.That(result.GetElement(0), Is.EqualTo(-5));
    Assert.That(result.GetElement(1), Is.EqualTo(-5));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Abs_ReturnsAbsoluteValues() {
    var vector = Vector64.Create(-5);
    var result = Vector64.Abs(vector);

    Assert.That(result.GetElement(0), Is.EqualTo(5));
    Assert.That(result.GetElement(1), Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Sqrt_ReturnsSqrt() {
    var vector = Vector64.Create(16.0f);
    var result = Vector64.Sqrt(vector);

    Assert.That(result.GetElement(0), Is.EqualTo(4.0f));
    Assert.That(result.GetElement(1), Is.EqualTo(4.0f));
  }

  #endregion

  #region Vector64<T> - Aggregate Operations

  [Test]
  [Category("HappyPath")]
  public void Vector64_Sum_SumsAllElements() {
    var vector = Vector64.Create(10);
    var sum = Vector64.Sum(vector);

    Assert.That(sum, Is.EqualTo(20)); // 10 + 10 = 20
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Dot_CalculatesDotProduct() {
    var left = Vector64.Create(2);
    var right = Vector64.Create(3);
    var dot = Vector64.Dot(left, right);

    Assert.That(dot, Is.EqualTo(12)); // (2*3) * 2 = 12
  }

  #endregion

  #region Vector64<T> - Bitwise Operations

  [Test]
  [Category("HappyPath")]
  public void Vector64_BitwiseAnd_CorrectResult() {
    var left = Vector64.Create(0b1111);
    var right = Vector64.Create(0b1010);
    var result = Vector64.BitwiseAnd(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(0b1010));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_BitwiseOr_CorrectResult() {
    var left = Vector64.Create(0b1100);
    var right = Vector64.Create(0b1010);
    var result = Vector64.BitwiseOr(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(0b1110));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Xor_CorrectResult() {
    var left = Vector64.Create(0b1111);
    var right = Vector64.Create(0b1010);
    var result = Vector64.Xor(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(0b0101));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_OnesComplement_CorrectResult() {
    var vector = Vector64.Create(0);
    var result = Vector64.OnesComplement(vector);

    Assert.That(result.GetElement(0), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_AndNot_CorrectResult() {
    var left = Vector64.Create(0b1111);
    var right = Vector64.Create(0b1010);
    var result = Vector64.AndNot(left, right);

    // AndNot = left & ~right = 0b1111 & ~0b1010 = 0b1111 & 0b0101 = 0b0101
    Assert.That(result.GetElement(0), Is.EqualTo(0b0101));
  }

  #endregion

  #region Vector64<T> - Comparison Operations

  [Test]
  [Category("HappyPath")]
  public void Vector64_Equals_SameValues_ReturnsAllBitsSet() {
    var left = Vector64.Create(5);
    var right = Vector64.Create(5);
    var result = Vector64.Equals<int>(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_GreaterThan_ReturnsCorrectMask() {
    var left = Vector64.Create(10);
    var right = Vector64.Create(5);
    var result = Vector64.GreaterThan(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_LessThan_ReturnsCorrectMask() {
    var left = Vector64.Create(5);
    var right = Vector64.Create(10);
    var result = Vector64.LessThan(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Min_ReturnsMinElements() {
    var left = Vector64.Create(5);
    var right = Vector64.Create(10);
    var result = Vector64.Min(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_Max_ReturnsMaxElements() {
    var left = Vector64.Create(5);
    var right = Vector64.Create(10);
    var result = Vector64.Max(left, right);

    Assert.That(result.GetElement(0), Is.EqualTo(10));
  }

  #endregion

  #region Vector64<T> - Element Access

  [Test]
  [Category("HappyPath")]
  public void Vector64_GetElement_ReturnsCorrectElement() {
    var vector = Vector64.Create(42);

    Assert.That(vector.GetElement(0), Is.EqualTo(42));
    Assert.That(vector.GetElement(1), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_WithElement_SetsElement() {
    var vector = Vector64.Create(0);
    var modified = vector.WithElement(1, 42);

    Assert.That(modified.GetElement(0), Is.EqualTo(0));
    Assert.That(modified.GetElement(1), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_ToScalar_ReturnsFirstElement() {
    var vector = Vector64.Create(42);

    Assert.That(vector.ToScalar(), Is.EqualTo(42));
  }

  #endregion

  #region Vector128<T> - Basic Properties

  [Test]
  [Category("HappyPath")]
  public void Vector128_Count_Byte_Returns16() => Assert.That(Vector128<byte>.Count, Is.EqualTo(16));

  [Test]
  [Category("HappyPath")]
  public void Vector128_Count_Int_Returns4() => Assert.That(Vector128<int>.Count, Is.EqualTo(4));

  [Test]
  [Category("HappyPath")]
  public void Vector128_Count_Long_Returns2() => Assert.That(Vector128<long>.Count, Is.EqualTo(2));

  [Test]
  [Category("HappyPath")]
  public void Vector128_Count_Float_Returns4() => Assert.That(Vector128<float>.Count, Is.EqualTo(4));

  [Test]
  [Category("HappyPath")]
  public void Vector128_Count_Double_Returns2() => Assert.That(Vector128<double>.Count, Is.EqualTo(2));

  [Test]
  [Category("HappyPath")]
  public void Vector128_Zero_AllElementsZero() {
    var vector = Vector128<int>.Zero;

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_One_AllElementsOne() {
    var vector = Vector128<int>.One;

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_AllBitsSet_AllBitsAreSet() {
    var vector = Vector128<int>.AllBitsSet;

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(-1));
  }

  #endregion

  #region Vector128<T> - Construction

  [Test]
  [Category("HappyPath")]
  public void Vector128_Create_Int_SetsAllElements() {
    var vector = Vector128.Create(42);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Create_Byte_SetsAllElements() {
    var vector = Vector128.Create((byte)255);

    for (var i = 0; i < Vector128<byte>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Create_Float_SetsAllElements() {
    var vector = Vector128.Create(3.14f);

    for (var i = 0; i < Vector128<float>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(3.14f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Create_FromVector64s_CombinesCorrectly() {
    var lower = Vector64.Create(1);
    var upper = Vector64.Create(2);
    var result = Vector128.Create(lower, upper);

    Assert.That(result.GetElement(0), Is.EqualTo(1));
    Assert.That(result.GetElement(1), Is.EqualTo(1));
    Assert.That(result.GetElement(2), Is.EqualTo(2));
    Assert.That(result.GetElement(3), Is.EqualTo(2));
  }

  #endregion

  #region Vector128<T> - Arithmetic Operations

  [Test]
  [Category("HappyPath")]
  public void Vector128_Add_AddsElements() {
    var left = Vector128.Create(10);
    var right = Vector128.Create(5);
    var result = Vector128.Add(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(15));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Subtract_SubtractsElements() {
    var left = Vector128.Create(10);
    var right = Vector128.Create(3);
    var result = Vector128.Subtract(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(7));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Multiply_MultipliesElements() {
    var left = Vector128.Create(4);
    var right = Vector128.Create(3);
    var result = Vector128.Multiply(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(12));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Divide_DividesElements() {
    var left = Vector128.Create(12.0f);
    var right = Vector128.Create(3.0f);
    var result = Vector128.Divide(left, right);

    for (var i = 0; i < Vector128<float>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(4.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Negate_NegatesElements() {
    var vector = Vector128.Create(5);
    var result = Vector128.Negate(vector);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-5));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Abs_ReturnsAbsoluteValues() {
    var vector = Vector128.Create(-5);
    var result = Vector128.Abs(vector);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Sqrt_ReturnsSqrt() {
    var vector = Vector128.Create(16.0f);
    var result = Vector128.Sqrt(vector);

    for (var i = 0; i < Vector128<float>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(4.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Floor_ReturnsFloor() {
    var vector = Vector128.Create(3.7f);
    var result = Vector128.Floor(vector);

    for (var i = 0; i < Vector128<float>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(3.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Ceiling_ReturnsCeiling() {
    var vector = Vector128.Create(3.2f);
    var result = Vector128.Ceiling(vector);

    for (var i = 0; i < Vector128<float>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(4.0f));
  }

  #endregion

  #region Vector128<T> - Bitwise Operations

  [Test]
  [Category("HappyPath")]
  public void Vector128_BitwiseAnd_CorrectResult() {
    var left = Vector128.Create(0b1111);
    var right = Vector128.Create(0b1010);
    var result = Vector128.BitwiseAnd(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(0b1010));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_BitwiseOr_CorrectResult() {
    var left = Vector128.Create(0b1100);
    var right = Vector128.Create(0b1010);
    var result = Vector128.BitwiseOr(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(0b1110));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Xor_CorrectResult() {
    var left = Vector128.Create(0b1111);
    var right = Vector128.Create(0b1010);
    var result = Vector128.Xor(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(0b0101));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_OnesComplement_CorrectResult() {
    var vector = Vector128.Create(0);
    var result = Vector128.OnesComplement(vector);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_AndNot_CorrectResult() {
    var left = Vector128.Create(0b1111);
    var right = Vector128.Create(0b1010);
    var result = Vector128.AndNot(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(0b0101));
  }

  #endregion

  #region Vector128<T> - Aggregate Operations

  [Test]
  [Category("HappyPath")]
  public void Vector128_Sum_SumsAllElements() {
    var vector = Vector128.Create(10);
    var sum = Vector128.Sum(vector);

    Assert.That(sum, Is.EqualTo(40)); // 10 * 4 = 40
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Dot_CalculatesDotProduct() {
    var left = Vector128.Create(2);
    var right = Vector128.Create(3);
    var dot = Vector128.Dot(left, right);

    Assert.That(dot, Is.EqualTo(24)); // (2*3) * 4 = 24
  }

  #endregion

  #region Vector128<T> - Comparison Operations

  [Test]
  [Category("HappyPath")]
  public void Vector128_Equals_SameValues_ReturnsAllBitsSet() {
    var left = Vector128.Create(5);
    var right = Vector128.Create(5);
    var result = Vector128.Equals<int>(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Equals_DifferentValues_ReturnsZero() {
    var left = Vector128.Create(5);
    var right = Vector128.Create(10);
    var result = Vector128.Equals<int>(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_GreaterThan_ReturnsCorrectMask() {
    var left = Vector128.Create(10);
    var right = Vector128.Create(5);
    var result = Vector128.GreaterThan(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_LessThan_ReturnsCorrectMask() {
    var left = Vector128.Create(5);
    var right = Vector128.Create(10);
    var result = Vector128.LessThan(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Max_ReturnsMaxElements() {
    var left = Vector128.Create(5);
    var right = Vector128.Create(10);
    var result = Vector128.Max(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_Min_ReturnsMinElements() {
    var left = Vector128.Create(5);
    var right = Vector128.Create(10);
    var result = Vector128.Min(left, right);

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(5));
  }

  #endregion

  #region Vector128<T> - Element Access

  [Test]
  [Category("HappyPath")]
  public void Vector128_GetElement_ReturnsCorrectElement() {
    var vector = Vector128.Create(42);

    Assert.That(vector.GetElement(0), Is.EqualTo(42));
    Assert.That(vector.GetElement(3), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_WithElement_SetsElement() {
    var vector = Vector128.Create(0);
    var modified = vector.WithElement(1, 42);

    Assert.That(modified.GetElement(0), Is.EqualTo(0));
    Assert.That(modified.GetElement(1), Is.EqualTo(42));
    Assert.That(modified.GetElement(2), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_ToScalar_ReturnsFirstElement() {
    var vector = Vector128.Create(42);

    Assert.That(vector.ToScalar(), Is.EqualTo(42));
  }

  #endregion

  #region Vector128<T> - Type Conversion

  [Test]
  [Category("HappyPath")]
  public void Vector128_AsByte_ReinterpretsAsBytes() {
    var vector = Vector128.Create(0x01020304);
    var bytes = vector.AsByte();

    Assert.That(Vector128<byte>.Count, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_AsInt32_ReinterpretsAsInts() {
    var vector = Vector128.Create((byte)255);
    var ints = vector.AsInt32();

    Assert.That(Vector128<int>.Count, Is.EqualTo(4));
    Assert.That(ints.GetElement(0), Is.EqualTo(-1)); // 0xFFFFFFFF as signed int
  }

  #endregion

  #region Vector128<T> - GetLower/GetUpper

  [Test]
  [Category("HappyPath")]
  public void Vector128_GetLower_ReturnsLowerHalf() {
    var vector = Vector128.Create(42);
    var lower = vector.GetLower();

    Assert.That(lower.GetElement(0), Is.EqualTo(42));
    Assert.That(lower.GetElement(1), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_GetUpper_ReturnsUpperHalf() {
    var vector = Vector128.Create(42);
    var upper = vector.GetUpper();

    Assert.That(upper.GetElement(0), Is.EqualTo(42));
    Assert.That(upper.GetElement(1), Is.EqualTo(42));
  }

  #endregion

  #region Vector256<T> - Basic Properties

  [Test]
  [Category("HappyPath")]
  public void Vector256_Count_Byte_Returns32() => Assert.That(Vector256<byte>.Count, Is.EqualTo(32));

  [Test]
  [Category("HappyPath")]
  public void Vector256_Count_Int_Returns8() => Assert.That(Vector256<int>.Count, Is.EqualTo(8));

  [Test]
  [Category("HappyPath")]
  public void Vector256_Count_Long_Returns4() => Assert.That(Vector256<long>.Count, Is.EqualTo(4));

  [Test]
  [Category("HappyPath")]
  public void Vector256_Zero_AllElementsZero() {
    var vector = Vector256<int>.Zero;

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_One_AllElementsOne() {
    var vector = Vector256<int>.One;

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_AllBitsSet_AllBitsAreSet() {
    var vector = Vector256<int>.AllBitsSet;

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(-1));
  }

  #endregion

  #region Vector256<T> - Construction

  [Test]
  [Category("HappyPath")]
  public void Vector256_Create_SetsAllElements() {
    var vector = Vector256.Create(42);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Create_FromVector128s_CombinesCorrectly() {
    var lower = Vector128.Create(1);
    var upper = Vector128.Create(2);
    var result = Vector256.Create(lower, upper);

    for (var i = 0; i < 4; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(1));
    for (var i = 4; i < 8; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(2));
  }

  #endregion

  #region Vector256<T> - Arithmetic Operations

  [Test]
  [Category("HappyPath")]
  public void Vector256_Add_AddsElements() {
    var left = Vector256.Create(10);
    var right = Vector256.Create(5);
    var result = Vector256.Add(left, right);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(15));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Subtract_SubtractsElements() {
    var left = Vector256.Create(10);
    var right = Vector256.Create(3);
    var result = Vector256.Subtract(left, right);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(7));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Multiply_MultipliesElements() {
    var left = Vector256.Create(4);
    var right = Vector256.Create(3);
    var result = Vector256.Multiply(left, right);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(12));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Divide_DividesElements() {
    var left = Vector256.Create(12.0f);
    var right = Vector256.Create(3.0f);
    var result = Vector256.Divide(left, right);

    for (var i = 0; i < Vector256<float>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(4.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Negate_NegatesElements() {
    var vector = Vector256.Create(5);
    var result = Vector256.Negate(vector);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-5));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Abs_ReturnsAbsoluteValues() {
    var vector = Vector256.Create(-5);
    var result = Vector256.Abs(vector);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Sqrt_ReturnsSqrt() {
    var vector = Vector256.Create(16.0f);
    var result = Vector256.Sqrt(vector);

    for (var i = 0; i < Vector256<float>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(4.0f));
  }

  #endregion

  #region Vector256<T> - Aggregate Operations

  [Test]
  [Category("HappyPath")]
  public void Vector256_Sum_SumsAllElements() {
    var vector = Vector256.Create(10);
    var sum = Vector256.Sum(vector);

    Assert.That(sum, Is.EqualTo(80)); // 10 * 8 = 80
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Dot_CalculatesDotProduct() {
    var left = Vector256.Create(2);
    var right = Vector256.Create(3);
    var dot = Vector256.Dot(left, right);

    Assert.That(dot, Is.EqualTo(48)); // (2*3) * 8 = 48
  }

  #endregion

  #region Vector256<T> - Comparison Operations

  [Test]
  [Category("HappyPath")]
  public void Vector256_Equals_SameValues_ReturnsAllBitsSet() {
    var left = Vector256.Create(5);
    var right = Vector256.Create(5);
    var result = Vector256.Equals<int>(left, right);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_GreaterThan_ReturnsCorrectMask() {
    var left = Vector256.Create(10);
    var right = Vector256.Create(5);
    var result = Vector256.GreaterThan(left, right);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_LessThan_ReturnsCorrectMask() {
    var left = Vector256.Create(5);
    var right = Vector256.Create(10);
    var result = Vector256.LessThan(left, right);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Max_ReturnsMaxElements() {
    var left = Vector256.Create(5);
    var right = Vector256.Create(10);
    var result = Vector256.Max(left, right);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_Min_ReturnsMinElements() {
    var left = Vector256.Create(5);
    var right = Vector256.Create(10);
    var result = Vector256.Min(left, right);

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(5));
  }

  #endregion

  #region Vector256<T> - GetLower/GetUpper

  [Test]
  [Category("HappyPath")]
  public void Vector256_GetLower_ReturnsLowerHalf() {
    var vector = Vector256.Create(42);
    var lower = vector.GetLower();

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(lower.GetElement(i), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector256_GetUpper_ReturnsUpperHalf() {
    var vector = Vector256.Create(42);
    var upper = vector.GetUpper();

    for (var i = 0; i < Vector128<int>.Count; ++i)
      Assert.That(upper.GetElement(i), Is.EqualTo(42));
  }

  #endregion

  #region Vector512<T> - Basic Properties

  [Test]
  [Category("HappyPath")]
  public void Vector512_Count_Byte_Returns64() => Assert.That(Vector512<byte>.Count, Is.EqualTo(64));

  [Test]
  [Category("HappyPath")]
  public void Vector512_Count_Int_Returns16() => Assert.That(Vector512<int>.Count, Is.EqualTo(16));

  [Test]
  [Category("HappyPath")]
  public void Vector512_Count_Long_Returns8() => Assert.That(Vector512<long>.Count, Is.EqualTo(8));

  [Test]
  [Category("HappyPath")]
  public void Vector512_Zero_AllElementsZero() {
    var vector = Vector512<int>.Zero;

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_One_AllElementsOne() {
    var vector = Vector512<int>.One;

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_AllBitsSet_AllBitsAreSet() {
    var vector = Vector512<int>.AllBitsSet;

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(-1));
  }

  #endregion

  #region Vector512<T> - Construction

  [Test]
  [Category("HappyPath")]
  public void Vector512_Create_SetsAllElements() {
    var vector = Vector512.Create(42);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Create_FromVector256s_CombinesCorrectly() {
    var lower = Vector256.Create(1);
    var upper = Vector256.Create(2);
    var result = Vector512.Create(lower, upper);

    for (var i = 0; i < 8; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(1));
    for (var i = 8; i < 16; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(2));
  }

  #endregion

  #region Vector512<T> - Arithmetic Operations

  [Test]
  [Category("HappyPath")]
  public void Vector512_Add_AddsElements() {
    var left = Vector512.Create(10);
    var right = Vector512.Create(5);
    var result = Vector512.Add(left, right);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(15));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Subtract_SubtractsElements() {
    var left = Vector512.Create(10);
    var right = Vector512.Create(3);
    var result = Vector512.Subtract(left, right);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(7));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Multiply_MultipliesElements() {
    var left = Vector512.Create(4);
    var right = Vector512.Create(3);
    var result = Vector512.Multiply(left, right);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(12));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Divide_DividesElements() {
    var left = Vector512.Create(12.0f);
    var right = Vector512.Create(3.0f);
    var result = Vector512.Divide(left, right);

    for (var i = 0; i < Vector512<float>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(4.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Negate_NegatesElements() {
    var vector = Vector512.Create(5);
    var result = Vector512.Negate(vector);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-5));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Abs_ReturnsAbsoluteValues() {
    var vector = Vector512.Create(-5);
    var result = Vector512.Abs(vector);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Sqrt_ReturnsSqrt() {
    var vector = Vector512.Create(16.0f);
    var result = Vector512.Sqrt(vector);

    for (var i = 0; i < Vector512<float>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(4.0f));
  }

  #endregion

  #region Vector512<T> - Aggregate Operations

  [Test]
  [Category("HappyPath")]
  public void Vector512_Sum_SumsAllElements() {
    var vector = Vector512.Create(10);
    var sum = Vector512.Sum(vector);

    Assert.That(sum, Is.EqualTo(160)); // 10 * 16 = 160
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Dot_CalculatesDotProduct() {
    var left = Vector512.Create(2);
    var right = Vector512.Create(3);
    var dot = Vector512.Dot(left, right);

    Assert.That(dot, Is.EqualTo(96)); // (2*3) * 16 = 96
  }

  #endregion

  #region Vector512<T> - Comparison Operations

  [Test]
  [Category("HappyPath")]
  public void Vector512_Equals_SameValues_ReturnsAllBitsSet() {
    var left = Vector512.Create(5);
    var right = Vector512.Create(5);
    var result = Vector512.Equals(left, right);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_GreaterThan_ReturnsCorrectMask() {
    var left = Vector512.Create(10);
    var right = Vector512.Create(5);
    var result = Vector512.GreaterThan(left, right);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_LessThan_ReturnsCorrectMask() {
    var left = Vector512.Create(5);
    var right = Vector512.Create(10);
    var result = Vector512.LessThan(left, right);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Max_ReturnsMaxElements() {
    var left = Vector512.Create(5);
    var right = Vector512.Create(10);
    var result = Vector512.Max(left, right);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_Min_ReturnsMinElements() {
    var left = Vector512.Create(5);
    var right = Vector512.Create(10);
    var result = Vector512.Min(left, right);

    for (var i = 0; i < Vector512<int>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(5));
  }

  #endregion

  #region Vector512<T> - GetLower/GetUpper

  [Test]
  [Category("HappyPath")]
  public void Vector512_GetLower_ReturnsLowerHalf() {
    var vector = Vector512.Create(42);
    var lower = vector.GetLower();

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(lower.GetElement(i), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector512_GetUpper_ReturnsUpperHalf() {
    var vector = Vector512.Create(42);
    var upper = vector.GetUpper();

    for (var i = 0; i < Vector256<int>.Count; ++i)
      Assert.That(upper.GetElement(i), Is.EqualTo(42));
  }

  #endregion

}
