// This file is part of Hawkynt's .NET Framework extensions.
//
// Tests for Vector64, Vector128, Vector256, Vector512 polyfills
// These tests verify polyfill correctness across all target frameworks.

using System;
using System.Runtime.Intrinsics;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("HappyPath")]
public class VectorPolyfillTests {

  #region Vector128 Basic Operations

  [Test]
  public void Vector128_Create_Int32_AllElementsSet() {
    var vector = Vector128.Create(42);
    Assert.That(Vector128.GetElement(vector, 0), Is.EqualTo(42));
    Assert.That(Vector128.GetElement(vector, 1), Is.EqualTo(42));
    Assert.That(Vector128.GetElement(vector, 2), Is.EqualTo(42));
    Assert.That(Vector128.GetElement(vector, 3), Is.EqualTo(42));
  }

  [Test]
  public void Vector128_Create_Float_AllElementsSet() {
    var vector = Vector128.Create(3.14f);
    Assert.That(Vector128.GetElement(vector, 0), Is.EqualTo(3.14f));
    Assert.That(Vector128.GetElement(vector, 1), Is.EqualTo(3.14f));
    Assert.That(Vector128.GetElement(vector, 2), Is.EqualTo(3.14f));
    Assert.That(Vector128.GetElement(vector, 3), Is.EqualTo(3.14f));
  }

  [Test]
  public void Vector128_Add_Int32_CorrectResult() {
    var left = Vector128.Create(1, 2, 3, 4);
    var right = Vector128.Create(10, 20, 30, 40);
    var result = Vector128.Add(left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(11));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(22));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(33));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(44));
  }

  [Test]
  public void Vector128_Subtract_Int32_CorrectResult() {
    var left = Vector128.Create(10, 20, 30, 40);
    var right = Vector128.Create(1, 2, 3, 4);
    var result = Vector128.Subtract(left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(9));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(18));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(27));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(36));
  }

  [Test]
  public void Vector128_Multiply_Float_CorrectResult() {
    var left = Vector128.Create(1f, 2f, 3f, 4f);
    var right = Vector128.Create(2f, 3f, 4f, 5f);
    var result = Vector128.Multiply(left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(2f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(6f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(12f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(20f));
  }

  [Test]
  public void Vector128_Divide_Float_CorrectResult() {
    var left = Vector128.Create(10f, 20f, 30f, 40f);
    var right = Vector128.Create(2f, 4f, 5f, 8f);
    var result = Vector128.Divide(left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(5f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(5f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(6f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(5f));
  }

  #endregion

  #region Vector128 Comparison Operations

  [Test]
  public void Vector128_EqualsAll_SameVectors_ReturnsTrue() {
    var left = Vector128.Create(1, 2, 3, 4);
    var right = Vector128.Create(1, 2, 3, 4);
    Assert.That(Vector128.EqualsAll(left, right), Is.True);
  }

  [Test]
  public void Vector128_EqualsAll_DifferentVectors_ReturnsFalse() {
    var left = Vector128.Create(1, 2, 3, 4);
    var right = Vector128.Create(1, 2, 3, 5);
    Assert.That(Vector128.EqualsAll(left, right), Is.False);
  }

  [Test]
  public void Vector128_EqualsAny_OneMatch_ReturnsTrue() {
    var left = Vector128.Create(1, 2, 3, 4);
    var right = Vector128.Create(0, 2, 0, 0);
    Assert.That(Vector128.EqualsAny(left, right), Is.True);
  }

  [Test]
  public void Vector128_EqualsAny_NoMatch_ReturnsFalse() {
    var left = Vector128.Create(1, 2, 3, 4);
    var right = Vector128.Create(5, 6, 7, 8);
    Assert.That(Vector128.EqualsAny(left, right), Is.False);
  }

  [Test]
  public void Vector128_GreaterThanAll_AllGreater_ReturnsTrue() {
    var left = Vector128.Create(5, 6, 7, 8);
    var right = Vector128.Create(1, 2, 3, 4);
    Assert.That(Vector128.GreaterThanAll(left, right), Is.True);
  }

  [Test]
  public void Vector128_GreaterThanAll_NotAllGreater_ReturnsFalse() {
    var left = Vector128.Create(5, 2, 7, 8);
    var right = Vector128.Create(1, 2, 3, 4);
    Assert.That(Vector128.GreaterThanAll(left, right), Is.False);
  }

  [Test]
  public void Vector128_GreaterThanAny_SomeGreater_ReturnsTrue() {
    var left = Vector128.Create(0, 0, 7, 0);
    var right = Vector128.Create(1, 2, 3, 4);
    Assert.That(Vector128.GreaterThanAny(left, right), Is.True);
  }

  [Test]
  public void Vector128_LessThanAll_AllLess_ReturnsTrue() {
    var left = Vector128.Create(1, 2, 3, 4);
    var right = Vector128.Create(5, 6, 7, 8);
    Assert.That(Vector128.LessThanAll(left, right), Is.True);
  }

  [Test]
  public void Vector128_LessThanAny_SomeLess_ReturnsTrue() {
    var left = Vector128.Create(10, 10, 3, 10);
    var right = Vector128.Create(5, 6, 7, 8);
    Assert.That(Vector128.LessThanAny(left, right), Is.True);
  }

  #endregion

  #region Vector128 Clamp

  [Test]
  public void Vector128_Clamp_Int32_ClampsCorrectly() {
    var value = Vector128.Create(-5, 15, 50, 100);
    var min = Vector128.Create(0, 0, 0, 0);
    var max = Vector128.Create(10, 10, 10, 10);
    var result = Vector128.Clamp(value, min, max);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(0));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(10));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(10));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(10));
  }

  [Test]
  public void Vector128_Clamp_Float_ClampsCorrectly() {
    var value = Vector128.Create(-5f, 5f, 15f, 25f);
    var min = Vector128.Create(0f, 0f, 0f, 0f);
    var max = Vector128.Create(10f, 10f, 10f, 10f);
    var result = Vector128.Clamp(value, min, max);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(0f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(5f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(10f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(10f));
  }

  #endregion

  #region Vector128 Rounding

  [Test]
  public void Vector128_Round_Float_RoundsCorrectly() {
    var vector = Vector128.Create(1.4f, 1.5f, 2.5f, 2.6f);
    var result = Vector128.Round(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(2f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(3f));
  }

  [Test]
  public void Vector128_Round_Double_RoundsCorrectly() {
    var vector = Vector128.Create(1.4, 1.6);
    var result = Vector128.Round(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1.0));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2.0));
  }

  [Test]
  public void Vector128_Truncate_Float_TruncatesCorrectly() {
    var vector = Vector128.Create(1.9f, -1.9f, 2.1f, -2.1f);
    var result = Vector128.Truncate(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(-1f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(2f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(-2f));
  }

  [Test]
  public void Vector128_Truncate_Double_TruncatesCorrectly() {
    var vector = Vector128.Create(1.9, -1.9);
    var result = Vector128.Truncate(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1.0));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(-1.0));
  }

  #endregion

  #region Vector128 ToScalar

  [Test]
  public void Vector128_ToScalar_Int32_ReturnsFirstElement() {
    var vector = Vector128.Create(42, 0, 0, 0);
    Assert.That(Vector128.ToScalar(vector), Is.EqualTo(42));
  }

  [Test]
  public void Vector128_ToScalar_Float_ReturnsFirstElement() {
    var vector = Vector128.Create(3.14f, 0f, 0f, 0f);
    Assert.That(Vector128.ToScalar(vector), Is.EqualTo(3.14f));
  }

  #endregion

  #region Vector128 Narrow

  [Test]
  public void Vector128_Narrow_DoublesToFloats_CorrectResult() {
    var lower = Vector128.Create(1.0, 2.0);
    var upper = Vector128.Create(3.0, 4.0);
    var result = Vector128.Narrow(lower, upper);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(3f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(4f));
  }

  [Test]
  public void Vector128_Narrow_LongsToInts_CorrectResult() {
    var lower = Vector128.Create(1L, 2L);
    var upper = Vector128.Create(3L, 4L);
    var result = Vector128.Narrow(lower, upper);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(3));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(4));
  }

  #endregion

  #region Vector128 FusedMultiplyAdd

  [Test]
  public void Vector128_FusedMultiplyAdd_Float_CorrectResult() {
    var a = Vector128.Create(1f, 2f, 3f, 4f);
    var b = Vector128.Create(2f, 2f, 2f, 2f);
    var c = Vector128.Create(1f, 1f, 1f, 1f);
    var result = Vector128.FusedMultiplyAdd(a, b, c);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(3f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(5f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(7f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(9f));
  }

  [Test]
  public void Vector128_FusedMultiplyAdd_Double_CorrectResult() {
    var a = Vector128.Create(1.0, 2.0);
    var b = Vector128.Create(3.0, 3.0);
    var c = Vector128.Create(1.0, 1.0);
    var result = Vector128.FusedMultiplyAdd(a, b, c);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(4.0));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(7.0));
  }

  #endregion

  #region Vector128 ConvertTo

  [Test]
  public void Vector128_ConvertToInt32_CorrectResult() {
    var vector = Vector128.Create(1.5f, 2.7f, -3.2f, 4.9f);
    var result = Vector128.ConvertToInt32(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(-3));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(4));
  }

  [Test]
  public void Vector128_ConvertToSingle_FromInt32_CorrectResult() {
    var vector = Vector128.Create(1, 2, 3, 4);
    var result = Vector128.ConvertToSingle(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(3f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(4f));
  }

  [Test]
  public void Vector128_ConvertToInt64_CorrectResult() {
    var vector = Vector128.Create(1.5, 2.7);
    var result = Vector128.ConvertToInt64(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1L));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2L));
  }

  [Test]
  public void Vector128_ConvertToDouble_FromInt64_CorrectResult() {
    var vector = Vector128.Create(1L, 2L);
    var result = Vector128.ConvertToDouble(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1.0));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2.0));
  }

  #endregion

  #region Vector128 Bitwise Operations

  [Test]
  public void Vector128_BitwiseAnd_CorrectResult() {
    var left = Vector128.Create(0xFF00FF00u, 0xFF00FF00u, 0xFF00FF00u, 0xFF00FF00u);
    var right = Vector128.Create(0xFFFF0000u, 0xFFFF0000u, 0xFFFF0000u, 0xFFFF0000u);
    var result = Vector128.BitwiseAnd(left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(0xFF000000u));
  }

  [Test]
  public void Vector128_BitwiseOr_CorrectResult() {
    var left = Vector128.Create(0x00FF0000u, 0x00FF0000u, 0x00FF0000u, 0x00FF0000u);
    var right = Vector128.Create(0x000000FFu, 0x000000FFu, 0x000000FFu, 0x000000FFu);
    var result = Vector128.BitwiseOr(left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(0x00FF00FFu));
  }

  [Test]
  public void Vector128_Xor_CorrectResult() {
    var left = Vector128.Create(0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu);
    var right = Vector128.Create(0xFF00FF00u, 0xFF00FF00u, 0xFF00FF00u, 0xFF00FF00u);
    var result = Vector128.Xor(left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(0x00FF00FFu));
  }

  #endregion

  #region Vector128 Min/Max

  [Test]
  public void Vector128_Min_Int32_ReturnsMinElements() {
    var left = Vector128.Create(1, 5, 3, 7);
    var right = Vector128.Create(2, 4, 6, 8);
    var result = Vector128.Min(left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(4));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(3));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(7));
  }

  [Test]
  public void Vector128_Max_Int32_ReturnsMaxElements() {
    var left = Vector128.Create(1, 5, 3, 7);
    var right = Vector128.Create(2, 4, 6, 8);
    var result = Vector128.Max(left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(2));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(5));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(6));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(8));
  }

  #endregion

  #region Vector128 Abs/Negate/Sqrt

  [Test]
  public void Vector128_Abs_Int32_ReturnsAbsoluteValues() {
    var vector = Vector128.Create(-1, 2, -3, 4);
    var result = Vector128.Abs(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(3));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(4));
  }

  [Test]
  public void Vector128_Negate_Int32_NegatesElements() {
    var vector = Vector128.Create(1, -2, 3, -4);
    var result = Vector128.Negate(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(-1));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(-3));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(4));
  }

  [Test]
  public void Vector128_Sqrt_Float_ReturnsSqrt() {
    var vector = Vector128.Create(4f, 9f, 16f, 25f);
    var result = Vector128.Sqrt(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(2f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(3f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(4f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(5f));
  }

  #endregion

  #region Vector128 Sum/Dot

  [Test]
  public void Vector128_Sum_Int32_SumsAllElements() {
    var vector = Vector128.Create(1, 2, 3, 4);
    var result = Vector128.Sum(vector);
    Assert.That(result, Is.EqualTo(10));
  }

  [Test]
  public void Vector128_Dot_Int32_CalculatesDotProduct() {
    var left = Vector128.Create(1, 2, 3, 4);
    var right = Vector128.Create(2, 3, 4, 5);
    var result = Vector128.Dot(left, right);
    Assert.That(result, Is.EqualTo(2 + 6 + 12 + 20));
  }

  #endregion

  #region Vector128 Floor/Ceiling

  [Test]
  public void Vector128_Floor_Float_FloorsCorrectly() {
    var vector = Vector128.Create(1.5f, 2.9f, -1.1f, -2.9f);
    var result = Vector128.Floor(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(-2f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(-3f));
  }

  [Test]
  public void Vector128_Ceiling_Float_CeilingsCorrectly() {
    var vector = Vector128.Create(1.1f, 2.9f, -1.1f, -2.9f);
    var result = Vector128.Ceiling(vector);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(2f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(3f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(-1f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(-2f));
  }

  #endregion

  #region Vector128 Shift Operations

  [Test]
  public void Vector128_ShiftLeft_Int32_ShiftsCorrectly() {
    var vector = Vector128.Create(1, 2, 4, 8);
    var result = Vector128.ShiftLeft(vector, 2);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(4));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(8));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(16));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(32));
  }

  [Test]
  public void Vector128_ShiftRightLogical_Int32_ShiftsCorrectly() {
    var vector = Vector128.Create(8, 16, 32, 64);
    var result = Vector128.ShiftRightLogical(vector, 2);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(2));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(4));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(8));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(16));
  }

  #endregion

  #region Vector128 Zero/One/AllBitsSet

  [Test]
  public void Vector128_Zero_AllElementsZero() {
    var vector = Vector128<int>.Zero;
    Assert.That(Vector128.GetElement(vector, 0), Is.EqualTo(0));
    Assert.That(Vector128.GetElement(vector, 1), Is.EqualTo(0));
    Assert.That(Vector128.GetElement(vector, 2), Is.EqualTo(0));
    Assert.That(Vector128.GetElement(vector, 3), Is.EqualTo(0));
  }

  [Test]
  public void Vector128_One_AllElementsOne() {
    var vector = Vector128<int>.One;
    Assert.That(Vector128.GetElement(vector, 0), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(vector, 1), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(vector, 2), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(vector, 3), Is.EqualTo(1));
  }

  [Test]
  public void Vector128_AllBitsSet_AllBitsAreSet() {
    var vector = Vector128<uint>.AllBitsSet;
    Assert.That(Vector128.GetElement(vector, 0), Is.EqualTo(0xFFFFFFFFu));
    Assert.That(Vector128.GetElement(vector, 1), Is.EqualTo(0xFFFFFFFFu));
    Assert.That(Vector128.GetElement(vector, 2), Is.EqualTo(0xFFFFFFFFu));
    Assert.That(Vector128.GetElement(vector, 3), Is.EqualTo(0xFFFFFFFFu));
  }

  #endregion

  #region Vector128 Count

  [Test]
  public void Vector128_Count_Int32_Returns4() {
    Assert.That(Vector128<int>.Count, Is.EqualTo(4));
  }

  [Test]
  public void Vector128_Count_Long_Returns2() {
    Assert.That(Vector128<long>.Count, Is.EqualTo(2));
  }

  [Test]
  public void Vector128_Count_Byte_Returns16() {
    Assert.That(Vector128<byte>.Count, Is.EqualTo(16));
  }

  [Test]
  public void Vector128_Count_Float_Returns4() {
    Assert.That(Vector128<float>.Count, Is.EqualTo(4));
  }

  [Test]
  public void Vector128_Count_Double_Returns2() {
    Assert.That(Vector128<double>.Count, Is.EqualTo(2));
  }

  #endregion

  #region Vector128 GetElement/WithElement

  [Test]
  public void Vector128_GetElement_ReturnsCorrectElement() {
    var vector = Vector128.Create(10, 20, 30, 40);
    Assert.That(Vector128.GetElement(vector, 0), Is.EqualTo(10));
    Assert.That(Vector128.GetElement(vector, 1), Is.EqualTo(20));
    Assert.That(Vector128.GetElement(vector, 2), Is.EqualTo(30));
    Assert.That(Vector128.GetElement(vector, 3), Is.EqualTo(40));
  }

  [Test]
  public void Vector128_WithElement_SetsElement() {
    var vector = Vector128.Create(0, 0, 0, 0);
    vector = Vector128.WithElement(vector, 2, 42);
    Assert.That(Vector128.GetElement(vector, 2), Is.EqualTo(42));
  }

  #endregion

  #region Vector128 As Methods

  [Test]
  public void Vector128_AsByte_ReinterpretsCast() {
    var vector = Vector128.Create(0x01020304u, 0x05060708u, 0x090A0B0Cu, 0x0D0E0F10u);
    var result = vector.AsByte();
    Assert.That(Vector128<byte>.Count, Is.EqualTo(16));
  }

  [Test]
  public void Vector128_AsInt32_ReinterpretsCast() {
    var vector = Vector128.Create(1.0f, 2.0f, 3.0f, 4.0f);
    var result = vector.AsInt32();
    Assert.That(Vector128<int>.Count, Is.EqualTo(4));
  }

  #endregion

  #region Vector128 ConditionalSelect

  [Test]
  public void Vector128_ConditionalSelect_SelectsCorrectly() {
    var condition = Vector128.Create(-1, 0, -1, 0);
    var left = Vector128.Create(1, 2, 3, 4);
    var right = Vector128.Create(10, 20, 30, 40);
    var result = Vector128.ConditionalSelect(condition, left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(20));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(3));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(40));
  }

  #endregion

  #region Vector128 ExtractMostSignificantBits

  [Test]
  public void Vector128_ExtractMostSignificantBits_CorrectResult() {
    var vector = Vector128.Create(-1, 1, -1, 1);
    var result = Vector128.ExtractMostSignificantBits(vector);
    Assert.That(result, Is.EqualTo(0b0101u));
  }

  #endregion
}
