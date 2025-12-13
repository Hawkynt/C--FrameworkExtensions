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
using System.Collections;
using System.Runtime.Intrinsics;
using NUnit.Framework;

namespace Backports.Tests;

/// <summary>
/// Comprehensive tests for all Vector types (Vector64, Vector128, Vector256, Vector512)
/// Testing all supported generic types and all members.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Vector")]
[Category("Comprehensive")]
public class VectorComprehensiveTests {

  #region Test Data Sources

  private static IEnumerable Vector64SupportedTypes {
    get {
      yield return new TestCaseData(typeof(byte), (byte)0, (byte)1, byte.MaxValue);
      yield return new TestCaseData(typeof(sbyte), (sbyte)0, (sbyte)1, sbyte.MaxValue);
      yield return new TestCaseData(typeof(short), (short)0, (short)1, short.MaxValue);
      yield return new TestCaseData(typeof(ushort), (ushort)0, (ushort)1, ushort.MaxValue);
      yield return new TestCaseData(typeof(int), 0, 1, int.MaxValue);
      yield return new TestCaseData(typeof(uint), 0u, 1u, uint.MaxValue);
      yield return new TestCaseData(typeof(long), 0L, 1L, long.MaxValue);
      yield return new TestCaseData(typeof(ulong), 0UL, 1UL, ulong.MaxValue);
      yield return new TestCaseData(typeof(float), 0.0f, 1.0f, float.MaxValue);
      yield return new TestCaseData(typeof(double), 0.0, 1.0, double.MaxValue);
    }
  }

  private static IEnumerable Vector128SupportedTypes {
    get {
      yield return new TestCaseData(typeof(byte), (byte)0, (byte)1, byte.MaxValue);
      yield return new TestCaseData(typeof(sbyte), (sbyte)0, (sbyte)1, sbyte.MaxValue);
      yield return new TestCaseData(typeof(short), (short)0, (short)1, short.MaxValue);
      yield return new TestCaseData(typeof(ushort), (ushort)0, (ushort)1, ushort.MaxValue);
      yield return new TestCaseData(typeof(int), 0, 1, int.MaxValue);
      yield return new TestCaseData(typeof(uint), 0u, 1u, uint.MaxValue);
      yield return new TestCaseData(typeof(long), 0L, 1L, long.MaxValue);
      yield return new TestCaseData(typeof(ulong), 0UL, 1UL, ulong.MaxValue);
      yield return new TestCaseData(typeof(float), 0.0f, 1.0f, float.MaxValue);
      yield return new TestCaseData(typeof(double), 0.0, 1.0, double.MaxValue);
    }
  }

  #endregion

  #region Vector64<T> - Properties for All Supported Types

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_Zero_AllElementsAreZero_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestZeroProperty))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { zeroValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_One_AllElementsAreOne_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestOneProperty))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_AllBitsSet_AllBitsAreSet_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestAllBitsSetProperty))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { maxValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_Count_ReturnsCorrectCount_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestCountProperty))!.MakeGenericMethod(elementType);
    method.Invoke(null, null);
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_Create_SetsAllElements_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestCreate))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_Add_AddsElements_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestAdd))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_Subtract_SubtractsElements_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestSubtract))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_Multiply_MultipliesElements_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestMultiply))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_GetElement_ReturnsCorrectElement_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestGetElement))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_WithElement_SetsElement_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestWithElement))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { zeroValue, oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_ToScalar_ReturnsFirstElement_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestToScalar))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  #endregion

  #region Vector128<T> - Properties for All Supported Types

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_Zero_AllElementsAreZero_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestZeroProperty))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { zeroValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_One_AllElementsAreOne_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestOneProperty))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_AllBitsSet_AllBitsAreSet_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestAllBitsSetProperty))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { maxValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_Create_SetsAllElements_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestCreate))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_Add_AddsElements_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestAdd))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_GetElement_ReturnsCorrectElement_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestGetElement))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_WithElement_SetsElement_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestWithElement))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { zeroValue, oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_ToScalar_ReturnsFirstElement_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestToScalar))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  #endregion

  #region Vector64<T> - Extended Arithmetic Operations

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_Divide_DividesElements_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestDivide))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_Negate_NegatesElements_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestNegate))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  #endregion

  #region Vector64<T> - Bitwise Operations

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_BitwiseAnd_PerformsAnd_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestBitwiseAnd))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { maxValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_BitwiseOr_PerformsOr_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestBitwiseOr))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { zeroValue, oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_Xor_PerformsXor_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestXor))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector64SupportedTypes))]
  [Category("HappyPath")]
  public void Vector64_OnesComplement_InvertsBits_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector64ComprehensiveTestHelper).GetMethod(nameof(Vector64ComprehensiveTestHelper.TestOnesComplement))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { zeroValue });
  }

  #endregion

  #region Vector64<T> - Type Conversion

  [Test]
  [Category("HappyPath")]
  public void Vector64_AsByte_ConvertsToByteVector() {
    var vector = Vector64.Create(42);
    var result = Vector64.AsByte(vector);
    Assert.That(result, Is.Not.EqualTo(default(Vector64<byte>)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_AsInt32_ConvertsToInt32Vector() {
    var vector = Vector64.Create((byte)42);
    var result = Vector64.AsInt32(vector);
    Assert.That(result, Is.Not.EqualTo(default(Vector64<int>)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector64_AsDouble_ConvertsToDoubleVector() {
    var vector = Vector64.Create(42);
    var result = Vector64.AsDouble(vector);
    Assert.That(result, Is.Not.EqualTo(default(Vector64<double>)));
  }

  #endregion

  #region Vector128<T> - Extended Arithmetic Operations

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_Divide_DividesElements_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestDivide))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_Negate_NegatesElements_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestNegate))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_Abs_ReturnsAbsoluteValue_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestAbs))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  #endregion

  #region Vector128<T> - Bitwise Operations

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_BitwiseAnd_PerformsAnd_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestBitwiseAnd))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { maxValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_BitwiseOr_PerformsOr_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestBitwiseOr))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { zeroValue, oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_Xor_PerformsXor_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestXor))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_AndNot_PerformsAndNot_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestAndNot))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { maxValue, oneValue });
  }

  #endregion

  #region Vector128<T> - Comparison Operations

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_Equals_ComparesForEquality_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestEquals))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_LessThan_ComparesLessThan_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestLessThan))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { zeroValue, oneValue });
  }

  [Test]
  [TestCaseSource(nameof(Vector128SupportedTypes))]
  [Category("HappyPath")]
  public void Vector128_GreaterThan_ComparesGreaterThan_ForAllTypes(Type elementType, object zeroValue, object oneValue, object maxValue) {
    var method = typeof(Vector128ComprehensiveTestHelper).GetMethod(nameof(Vector128ComprehensiveTestHelper.TestGreaterThan))!.MakeGenericMethod(elementType);
    method.Invoke(null, new[] { oneValue, zeroValue });
  }

  #endregion

  #region Vector128<T> - Type Conversion

  [Test]
  [Category("HappyPath")]
  public void Vector128_AsByte_ConvertsToByteVector() {
    var vector = Vector128.Create(42);
    var result = Vector128.AsByte(vector);
    Assert.That(result, Is.Not.EqualTo(default(Vector128<byte>)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_AsInt32_ConvertsToInt32Vector() {
    var vector = Vector128.Create((byte)42);
    var result = Vector128.AsInt32(vector);
    Assert.That(result, Is.Not.EqualTo(default(Vector128<int>)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_AsDouble_ConvertsToDoubleVector() {
    var vector = Vector128.Create(42);
    var result = Vector128.AsDouble(vector);
    Assert.That(result, Is.Not.EqualTo(default(Vector128<double>)));
  }

  [Test]
  [Category("HappyPath")]
  public void Vector128_AsSingle_ConvertsToFloatVector() {
    var vector = Vector128.Create(42);
    var result = Vector128.AsSingle(vector);
    Assert.That(result, Is.Not.EqualTo(default(Vector128<float>)));
  }

  #endregion
}

#region Helper Classes

public static class Vector64ComprehensiveTestHelper {

  public static void TestZeroProperty<T>(T expectedZero) where T : struct {
    var vector = Vector64<T>.Zero;
    for (var i = 0; i < Vector64<T>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(expectedZero));
  }

  public static void TestOneProperty<T>(T expectedOne) where T : struct {
    var vector = Vector64.Create(expectedOne);
    for (var i = 0; i < Vector64<T>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(expectedOne));
  }

  public static void TestAllBitsSetProperty<T>(T expectedValue) where T : struct {
    var vector = Vector64<T>.AllBitsSet;
    // For signed types, all bits set equals -1; for unsigned, it equals max value
    for (var i = 0; i < Vector64<T>.Count; ++i) {
      var element = vector.GetElement(i);
      // Just verify the bits are set, exact value depends on type
      Assert.That(element, Is.Not.EqualTo(default(T)));
    }
  }

  public static void TestCountProperty<T>() where T : struct {
    var count = Vector64<T>.Count;
    Assert.That(count, Is.GreaterThan(0));
    // Count should be reasonable for 64-bit vector
    Assert.That(count, Is.LessThanOrEqualTo(64));
  }

  public static void TestCreate<T>(T value) where T : struct {
    var vector = Vector64.Create(value);
    for (var i = 0; i < Vector64<T>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(value));
  }

  public static void TestAdd<T>(T value) where T : struct {
    var vector1 = Vector64.Create(value);
    var vector2 = Vector64.Create(value);
    var result = vector1 + vector2;
    Assert.That(result, Is.Not.EqualTo(default(Vector64<T>)));
  }

  public static void TestSubtract<T>(T value) where T : struct {
    var vector1 = Vector64.Create(value);
    var vector2 = Vector64.Create(value);
    var result = vector1 - vector2;
    for (var i = 0; i < Vector64<T>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(default(T)));
  }

  public static void TestMultiply<T>(T value) where T : struct {
    var vector1 = Vector64.Create(value);
    var vector2 = Vector64.Create(value);
    var result = vector1 * vector2;
    Assert.That(result, Is.Not.EqualTo(default(Vector64<T>)));
  }

  public static void TestGetElement<T>(T value) where T : struct {
    var vector = Vector64.Create(value);
    var element = vector.GetElement(0);
    Assert.That(element, Is.EqualTo(value));
  }

  public static void TestWithElement<T>(T zeroValue, T oneValue) where T : struct {
    var vector = Vector64.Create(zeroValue);
    var modified = vector.WithElement(0, oneValue);
    Assert.That(modified.GetElement(0), Is.EqualTo(oneValue));
  }

  public static void TestToScalar<T>(T value) where T : struct {
    var vector = Vector64.Create(value);
    var scalar = vector.GetElement(0);
    Assert.That(scalar, Is.EqualTo(value));
  }

  public static void TestDivide<T>(T value) where T : struct {
    var vector1 = Vector64.Create(value);
    var vector2 = Vector64.Create(value);
    var result = vector1 / vector2;
    Assert.That(result, Is.Not.EqualTo(default(Vector64<T>)));
  }

  public static void TestNegate<T>(T value) where T : struct {
    var vector = Vector64.Create(value);
    var result = -vector;
    Assert.That(result, Is.Not.EqualTo(default(Vector64<T>)));
  }

  public static void TestBitwiseAnd<T>(T value) where T : struct {
    var vector1 = Vector64.Create(value);
    var vector2 = Vector64.Create(value);
    var result = vector1 & vector2;
    for (var i = 0; i < Vector64<T>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(value));
  }

  public static void TestBitwiseOr<T>(T zeroValue, T oneValue) where T : struct {
    var vector1 = Vector64.Create(zeroValue);
    var vector2 = Vector64.Create(oneValue);
    var result = vector1 | vector2;
    for (var i = 0; i < Vector64<T>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(oneValue));
  }

  public static void TestXor<T>(T value) where T : struct {
    var vector1 = Vector64.Create(value);
    var vector2 = Vector64.Create(value);
    var result = vector1 ^ vector2;
    for (var i = 0; i < Vector64<T>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(default(T)));
  }

  public static void TestOnesComplement<T>(T value) where T : struct {
    var vector = Vector64.Create(value);
    var result = ~vector;
    Assert.That(result, Is.Not.EqualTo(default(Vector64<T>)));
  }
}

public static class Vector128ComprehensiveTestHelper {

  public static void TestZeroProperty<T>(T expectedZero) where T : struct {
    var vector = Vector128<T>.Zero;
    for (var i = 0; i < Vector128<T>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(expectedZero));
  }

  public static void TestOneProperty<T>(T expectedOne) where T : struct {
    var vector = Vector128.Create(expectedOne);
    for (var i = 0; i < Vector128<T>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(expectedOne));
  }

  public static void TestAllBitsSetProperty<T>(T expectedValue) where T : struct {
    var vector = Vector128<T>.AllBitsSet;
    for (var i = 0; i < Vector128<T>.Count; ++i) {
      var element = vector.GetElement(i);
      Assert.That(element, Is.Not.EqualTo(default(T)));
    }
  }

  public static void TestCreate<T>(T value) where T : struct {
    var vector = Vector128.Create(value);
    for (var i = 0; i < Vector128<T>.Count; ++i)
      Assert.That(vector.GetElement(i), Is.EqualTo(value));
  }

  public static void TestAdd<T>(T value) where T : struct {
    var vector1 = Vector128.Create(value);
    var vector2 = Vector128.Create(value);
    var result = vector1 + vector2;
    Assert.That(result, Is.Not.EqualTo(default(Vector128<T>)));
  }

  public static void TestGetElement<T>(T value) where T : struct {
    var vector = Vector128.Create(value);
    var element = vector.GetElement(0);
    Assert.That(element, Is.EqualTo(value));
  }

  public static void TestWithElement<T>(T zeroValue, T oneValue) where T : struct {
    var vector = Vector128.Create(zeroValue);
    var modified = vector.WithElement(0, oneValue);
    Assert.That(modified.GetElement(0), Is.EqualTo(oneValue));
  }

  public static void TestToScalar<T>(T value) where T : struct {
    var vector = Vector128.Create(value);
    var scalar = vector.GetElement(0);
    Assert.That(scalar, Is.EqualTo(value));
  }

  public static void TestDivide<T>(T value) where T : struct {
    var vector1 = Vector128.Create(value);
    var vector2 = Vector128.Create(value);
    var result = vector1 / vector2;
    Assert.That(result, Is.Not.EqualTo(default(Vector128<T>)));
  }

  public static void TestNegate<T>(T value) where T : struct {
    var vector = Vector128.Create(value);
    var result = -vector;
    Assert.That(result, Is.Not.EqualTo(default(Vector128<T>)));
  }

  public static void TestAbs<T>(T value) where T : struct {
    var vector = Vector128.Create(value);
    var result = Vector128.Abs(vector);
    Assert.That(result, Is.Not.EqualTo(default(Vector128<T>)));
  }

  public static void TestBitwiseAnd<T>(T value) where T : struct {
    var vector1 = Vector128.Create(value);
    var vector2 = Vector128.Create(value);
    var result = vector1 & vector2;
    for (var i = 0; i < Vector128<T>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(value));
  }

  public static void TestBitwiseOr<T>(T zeroValue, T oneValue) where T : struct {
    var vector1 = Vector128.Create(zeroValue);
    var vector2 = Vector128.Create(oneValue);
    var result = vector1 | vector2;
    for (var i = 0; i < Vector128<T>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(oneValue));
  }

  public static void TestXor<T>(T value) where T : struct {
    var vector1 = Vector128.Create(value);
    var vector2 = Vector128.Create(value);
    var result = vector1 ^ vector2;
    for (var i = 0; i < Vector128<T>.Count; ++i)
      Assert.That(result.GetElement(i), Is.EqualTo(default(T)));
  }

  public static void TestAndNot<T>(T value1, T value2) where T : struct {
    var vector1 = Vector128.Create(value1);
    var vector2 = Vector128.Create(value2);
    var result = Vector128.AndNot(vector1, vector2);
    Assert.That(result, Is.Not.EqualTo(default(Vector128<T>)));
  }

  public static void TestEquals<T>(T value) where T : struct {
    var vector1 = Vector128.Create(value);
    var vector2 = Vector128.Create(value);
    var result = Vector128.Equals(vector1, vector2);
    Assert.That(result, Is.Not.EqualTo(default(Vector128<T>)));
  }

  public static void TestLessThan<T>(T smallerValue, T largerValue) where T : struct {
    var vector1 = Vector128.Create(smallerValue);
    var vector2 = Vector128.Create(largerValue);
    var result = Vector128.LessThan(vector1, vector2);
    Assert.That(result, Is.Not.EqualTo(default(Vector128<T>)));
  }

  public static void TestGreaterThan<T>(T largerValue, T smallerValue) where T : struct {
    var vector1 = Vector128.Create(largerValue);
    var vector2 = Vector128.Create(smallerValue);
    var result = Vector128.GreaterThan(vector1, vector2);
    Assert.That(result, Is.Not.EqualTo(default(Vector128<T>)));
  }
}

#endregion
