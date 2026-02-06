using NUnit.Framework;

namespace System;

[TestFixture]
public class Float8Tests {

  private const float Tolerance = 0.5f;

  // Quarter tests
  [Test]
  public void Quarter_SpecialValues_AreCorrect() {
    Assert.IsTrue(Quarter.IsNaN(Quarter.NaN));
    Assert.IsTrue(Quarter.IsInfinity(Quarter.PositiveInfinity));
    Assert.IsTrue(Quarter.IsInfinity(Quarter.NegativeInfinity));
    Assert.IsTrue(Quarter.IsPositiveInfinity(Quarter.PositiveInfinity));
    Assert.IsTrue(Quarter.IsNegativeInfinity(Quarter.NegativeInfinity));
    Assert.IsFalse(Quarter.IsNaN(Quarter.Zero));
    Assert.IsTrue(Quarter.IsFinite(Quarter.One));
  }

  [Test]
  public void Quarter_Zero_IsCorrect() {
    Assert.AreEqual(0f, Quarter.Zero.ToSingle(), 0.001f);
  }

  [Test]
  public void Quarter_One_IsCorrect() {
    Assert.AreEqual(1f, Quarter.One.ToSingle(), Tolerance);
  }

  [Test]
  public void Quarter_Negation_Works() {
    var positive = (Quarter)2.0f;
    var negative = -positive;
    Assert.IsTrue(Quarter.IsNegative(negative));
    Assert.AreEqual(-positive.ToSingle(), negative.ToSingle(), Tolerance);
  }

  [Test]
  public void Quarter_Arithmetic_Works() {
    var a = (Quarter)2.0f;
    var b = (Quarter)1.0f;
    Assert.AreEqual(3.0f, (a + b).ToSingle(), Tolerance);
    Assert.AreEqual(1.0f, (a - b).ToSingle(), Tolerance);
    Assert.AreEqual(2.0f, (a * b).ToSingle(), Tolerance);
    Assert.AreEqual(2.0f, (a / b).ToSingle(), Tolerance);
  }

  // E4M3 tests
  [Test]
  public void E4M3_SpecialValues_AreCorrect() {
    Assert.IsTrue(E4M3.IsNaN(E4M3.NaN));
    Assert.IsFalse(E4M3.IsNaN(E4M3.Zero));
    Assert.IsTrue(E4M3.IsFinite(E4M3.MaxValue));
  }

  [Test]
  public void E4M3_Zero_IsCorrect() {
    Assert.AreEqual(0f, E4M3.Zero.ToSingle(), 0.001f);
  }

  [Test]
  public void E4M3_One_IsCorrect() {
    Assert.AreEqual(1f, E4M3.One.ToSingle(), Tolerance);
  }

  [Test]
  public void E4M3_NoInfinity_ClampToMax() {
    var result = (E4M3)float.PositiveInfinity;
    Assert.IsFalse(E4M3.IsNaN(result));
    Assert.IsTrue(E4M3.IsFinite(result));
  }

  // Comparison tests
  [Test]
  public void Float8_Comparison_Works() {
    var q1 = (Quarter)1.0f;
    var q2 = (Quarter)2.0f;
    var q3 = (Quarter)1.0f;

    Assert.IsTrue(q1 < q2);
    Assert.IsTrue(q2 > q1);
    Assert.IsTrue(q1 == q3);
    Assert.IsTrue(q1 <= q3);
    Assert.IsTrue(q1 >= q3);
  }

  [Test]
  public void Float8_NaN_Comparison_Special() {
    Assert.IsTrue(Quarter.NaN.Equals(Quarter.NaN));
    Assert.IsTrue(E4M3.NaN.Equals(E4M3.NaN));
  }

}
