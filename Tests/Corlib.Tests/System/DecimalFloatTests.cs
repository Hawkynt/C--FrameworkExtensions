using System.Globalization;
using NUnit.Framework;

namespace System;

[TestFixture]
public class DecimalFloatTests {

  // ---------------------------------------------------------------------------
  // Decimal8 (coefficient 0..15, unbiased exponent -4..3, bias 4)
  // ---------------------------------------------------------------------------

  [Test]
  public void Decimal8_Zero_IsZero() {
    Assert.AreEqual(0d, Decimal8.Zero.ToDouble(), 0d);
  }

  [Test]
  public void Decimal8_One_IsOne() {
    Assert.AreEqual(1d, Decimal8.One.ToDouble(), 1e-9);
    Assert.AreEqual(1, Decimal8.One.Coefficient);
    Assert.AreEqual(0, Decimal8.One.Exponent);
  }

  [TestCase(12d)]
  [TestCase(1.2d)]
  [TestCase(120d)]
  [TestCase(0.012d)]
  [TestCase(-3d)]
  [TestCase(15d)]
  public void Decimal8_RoundTrip_RepresentableValues(double value) {
    var encoded = Decimal8.FromDouble(value);
    var decoded = encoded.ToDouble();
    var tolerance = Math.Abs(value) * 1e-9 + 1e-12;
    Assert.AreEqual(value, decoded, tolerance);
  }

  [Test]
  public void Decimal8_NegativeSign_IsPreserved() {
    var encoded = Decimal8.FromDouble(-3d);
    Assert.IsTrue(Decimal8.IsNegative(encoded));
    Assert.AreEqual(-3d, encoded.ToDouble(), 1e-9);
  }

  [Test]
  public void Decimal8_Overflow_SaturatesNotThrows() {
    var encoded = Decimal8.FromDouble(1e30);
    Assert.AreEqual(Decimal8.MaxValue.ToDouble(), encoded.ToDouble(), 0d);
  }

  [Test]
  public void Decimal8_NegativeOverflow_SaturatesToMin() {
    var encoded = Decimal8.FromDouble(-1e30);
    Assert.AreEqual(Decimal8.MinValue.ToDouble(), encoded.ToDouble(), 0d);
  }

  [Test]
  public void Decimal8_Underflow_GoesToZero() {
    var encoded = Decimal8.FromDouble(1e-30);
    Assert.AreEqual(0d, encoded.ToDouble(), 0d);
  }

  [Test]
  public void Decimal8_Infinity_Saturates() {
    Assert.AreEqual(Decimal8.MaxValue.ToDouble(), Decimal8.FromDouble(double.PositiveInfinity).ToDouble(), 0d);
    Assert.AreEqual(Decimal8.MinValue.ToDouble(), Decimal8.FromDouble(double.NegativeInfinity).ToDouble(), 0d);
  }

  [Test]
  public void Decimal8_RawRoundTrip() {
    var original = Decimal8.FromDouble(12d);
    var roundTripped = Decimal8.FromRaw(original.RawValue);
    Assert.AreEqual(original.ToDouble(), roundTripped.ToDouble(), 0d);
  }

  [Test]
  public void Decimal8_Comparison_Works() {
    var a = Decimal8.FromDouble(1d);
    var b = Decimal8.FromDouble(2d);
    Assert.IsTrue(a < b);
    Assert.IsTrue(b > a);
    Assert.IsTrue(a <= Decimal8.FromDouble(1d));
    Assert.IsTrue(a >= Decimal8.FromDouble(1d));
    Assert.IsTrue(a == Decimal8.FromDouble(1d));
    Assert.IsTrue(a != b);
  }

  // ---------------------------------------------------------------------------
  // Decimal16 (coefficient 0..1023, unbiased exponent -16..15, bias 16)
  // ---------------------------------------------------------------------------

  [Test]
  public void Decimal16_Zero_IsZero() {
    Assert.AreEqual(0d, Decimal16.Zero.ToDouble(), 0d);
  }

  [TestCase(1d)]
  [TestCase(10d)]
  [TestCase(100d)]
  [TestCase(123d)]
  [TestCase(1.23d)]
  [TestCase(-45.6d)]
  [TestCase(0.001d)]
  [TestCase(1023d)]
  public void Decimal16_RoundTrip_RepresentableValues(double value) {
    var decoded = Decimal16.FromDouble(value).ToDouble();
    var tolerance = Math.Abs(value) * 1e-9 + 1e-12;
    Assert.AreEqual(value, decoded, tolerance);
  }

  [Test]
  public void Decimal16_Overflow_Saturates() {
    var encoded = Decimal16.FromDouble(1e300);
    Assert.AreEqual(Decimal16.MaxValue.ToDouble(), encoded.ToDouble(), 0d);
  }

  [Test]
  public void Decimal16_Sign_IsPreserved() {
    Assert.IsTrue(Decimal16.IsNegative(Decimal16.FromDouble(-45.6d)));
    Assert.IsFalse(Decimal16.IsNegative(Decimal16.FromDouble(45.6d)));
  }

  // The custom 8/16-bit formats now arithmetic-exactly in base 10 (via the shared engine), not through double.
  [Test]
  public void Decimal8_ExactDecimalArithmetic() {
    Assert.AreEqual("0.3", (Decimal8.Parse("0.1") + Decimal8.Parse("0.2")).ToString());
  }

  [Test]
  public void Decimal16_ExactDecimalArithmetic() {
    Assert.AreEqual("0.3", (Decimal16.Parse("0.1") + Decimal16.Parse("0.2")).ToString());
    Assert.AreEqual("5.79", (Decimal16.Parse("1.23") + Decimal16.Parse("4.56")).ToString());
  }
}
