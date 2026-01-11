using NUnit.Framework;

namespace System;

[TestFixture]
public class BFloatTests {

  private const float FloatTolerance = 0.01f;
  private const double DoubleTolerance = 0.01;

  // BFloat16 tests
  [Test]
  public void BFloat16_SpecialValues_AreCorrect() {
    Assert.IsTrue(BFloat16.IsNaN(BFloat16.NaN));
    Assert.IsTrue(BFloat16.IsInfinity(BFloat16.PositiveInfinity));
    Assert.IsTrue(BFloat16.IsInfinity(BFloat16.NegativeInfinity));
    Assert.IsTrue(BFloat16.IsPositiveInfinity(BFloat16.PositiveInfinity));
    Assert.IsTrue(BFloat16.IsNegativeInfinity(BFloat16.NegativeInfinity));
    Assert.IsFalse(BFloat16.IsNaN(BFloat16.Zero));
    Assert.IsTrue(BFloat16.IsFinite(BFloat16.One));
  }

  [Test]
  public void BFloat16_Zero_IsCorrect() {
    Assert.AreEqual(0f, BFloat16.Zero.ToSingle(), FloatTolerance);
  }

  [Test]
  public void BFloat16_One_IsCorrect() {
    Assert.AreEqual(1f, BFloat16.One.ToSingle(), FloatTolerance);
  }

  [Test]
  public void BFloat16_RoundTrip_PreservesValue() {
    var original = 3.14f;
    var bf = (BFloat16)original;
    // BFloat16 truncates, so we can't expect exact match
    Assert.AreEqual(original, bf.ToSingle(), 0.1f);
  }

  [Test]
  public void BFloat16_Arithmetic_Works() {
    var a = (BFloat16)2.0f;
    var b = (BFloat16)1.0f;
    Assert.AreEqual(3.0f, (a + b).ToSingle(), FloatTolerance);
    Assert.AreEqual(1.0f, (a - b).ToSingle(), FloatTolerance);
    Assert.AreEqual(2.0f, (a * b).ToSingle(), FloatTolerance);
    Assert.AreEqual(2.0f, (a / b).ToSingle(), FloatTolerance);
  }

  [Test]
  public void BFloat16_Negation_Works() {
    var positive = (BFloat16)2.0f;
    var negative = -positive;
    Assert.IsTrue(BFloat16.IsNegative(negative));
  }

  // BFloat32 tests
  [Test]
  public void BFloat32_SpecialValues_AreCorrect() {
    Assert.IsTrue(BFloat32.IsNaN(BFloat32.NaN));
    Assert.IsTrue(BFloat32.IsInfinity(BFloat32.PositiveInfinity));
    Assert.IsTrue(BFloat32.IsInfinity(BFloat32.NegativeInfinity));
    Assert.IsFalse(BFloat32.IsNaN(BFloat32.Zero));
    Assert.IsTrue(BFloat32.IsFinite(BFloat32.One));
  }

  [Test]
  public void BFloat32_Zero_IsCorrect() {
    Assert.AreEqual(0.0, BFloat32.Zero.ToDouble(), DoubleTolerance);
  }

  [Test]
  public void BFloat32_One_IsCorrect() {
    Assert.AreEqual(1.0, BFloat32.One.ToDouble(), DoubleTolerance);
  }

  [Test]
  public void BFloat32_RoundTrip_PreservesValue() {
    var original = 3.14159;
    var bf = (BFloat32)original;
    Assert.AreEqual(original, bf.ToDouble(), 0.1);
  }

  [Test]
  public void BFloat32_Arithmetic_Works() {
    var a = (BFloat32)100.0;
    var b = (BFloat32)50.0;
    Assert.AreEqual(150.0, (a + b).ToDouble(), DoubleTolerance);
    Assert.AreEqual(50.0, (a - b).ToDouble(), DoubleTolerance);
  }

  // BFloat64 tests
  [Test]
  public void BFloat64_SpecialValues_AreCorrect() {
    Assert.IsTrue(BFloat64.IsNaN(BFloat64.NaN));
    Assert.IsTrue(BFloat64.IsInfinity(BFloat64.PositiveInfinity));
    Assert.IsTrue(BFloat64.IsInfinity(BFloat64.NegativeInfinity));
    Assert.IsFalse(BFloat64.IsNaN(BFloat64.Zero));
    Assert.IsTrue(BFloat64.IsFinite(BFloat64.One));
  }

  [Test]
  public void BFloat64_Zero_IsCorrect() {
    Assert.AreEqual(0.0, BFloat64.Zero.ToDouble(), DoubleTolerance);
  }

  [Test]
  public void BFloat64_One_IsCorrect() {
    Assert.AreEqual(1.0, BFloat64.One.ToDouble(), DoubleTolerance);
  }

  [Test]
  public void BFloat64_RoundTrip_PreservesValue() {
    var original = 123456.789;
    var bf = (BFloat64)original;
    Assert.AreEqual(original, bf.ToDouble(), 1.0);
  }

  [Test]
  public void BFloat64_Negation_Works() {
    var positive = (BFloat64)100.0;
    var negative = -positive;
    Assert.IsTrue(BFloat64.IsNegative(negative));
  }

  // Comparison tests
  [Test]
  public void BFloat16_Comparison_Works() {
    var a = (BFloat16)1.0f;
    var b = (BFloat16)2.0f;
    var c = (BFloat16)1.0f;

    Assert.IsTrue(a < b);
    Assert.IsTrue(b > a);
    Assert.IsTrue(a == c);
    Assert.IsFalse(a != c);
  }

  [Test]
  public void BFloat_NaN_Equality_Works() {
    Assert.IsTrue(BFloat16.NaN.Equals(BFloat16.NaN));
    Assert.IsTrue(BFloat32.NaN.Equals(BFloat32.NaN));
    Assert.IsTrue(BFloat64.NaN.Equals(BFloat64.NaN));
  }

}
