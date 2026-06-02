using NUnit.Framework;

namespace System;

[TestFixture]
public class PositTests {

  private const double Tolerance = 1e-9;

  // --- Posit8 (nbits=8, es=0) reference decode values ---

  [Test]
  public void Posit8_DecodeReferenceValues() {
    Assert.AreEqual(1.0, Posit8.FromRaw(0x40).ToDouble(), Tolerance, "0x40 should decode to 1.0");
    Assert.AreEqual(0.0, Posit8.FromRaw(0x00).ToDouble(), Tolerance, "0x00 should decode to 0.0");
  }

  [Test]
  public void Posit8_NaR_DecodesToNaN() {
    Assert.IsTrue(double.IsNaN(Posit8.FromRaw(0x80).ToDouble()), "0x80 should decode to NaR (NaN)");
    Assert.IsTrue(Posit8.IsNaR(Posit8.NaR));
    Assert.IsTrue(double.IsNaN(Posit8.NaR.ToDouble()));
  }

  [Test]
  public void Posit8_Zero_IsCorrect() {
    Assert.AreEqual(0.0, Posit8.Zero.ToDouble(), Tolerance);
    Assert.AreEqual(0x00, Posit8.Zero.RawValue);
  }

  [Test]
  public void Posit8_FromDouble_One_HasExpectedRaw() {
    Assert.AreEqual(0x40, Posit8.FromDouble(1.0).RawValue, "FromDouble(1.0) should encode to 0x40");
  }

  [Test]
  public void Posit8_NonFinite_EncodesToNaR() {
    Assert.IsTrue(Posit8.IsNaR(Posit8.FromDouble(double.NaN)));
    Assert.IsTrue(Posit8.IsNaR(Posit8.FromDouble(double.PositiveInfinity)));
    Assert.IsTrue(Posit8.IsNaR(Posit8.FromDouble(double.NegativeInfinity)));
  }

  [TestCase(1.0)]
  [TestCase(2.0)]
  [TestCase(0.5)]
  [TestCase(4.0)]
  [TestCase(0.25)]
  [TestCase(-1.0)]
  [TestCase(-2.0)]
  public void Posit8_RoundTrip(double value) {
    var p = Posit8.FromDouble(value);
    Assert.AreEqual(value, p.ToDouble(), Tolerance, $"Posit8 round-trip of {value}");
  }

  // --- Posit16 (nbits=16, es=1) ---

  [Test]
  public void Posit16_Zero_And_NaR() {
    Assert.AreEqual(0.0, Posit16.FromRaw(0x0000).ToDouble(), Tolerance);
    Assert.IsTrue(double.IsNaN(Posit16.FromRaw(0x8000).ToDouble()));
    Assert.IsTrue(Posit16.IsNaR(Posit16.NaR));
  }

  [Test]
  public void Posit16_NonFinite_EncodesToNaR() {
    Assert.IsTrue(Posit16.IsNaR(Posit16.FromDouble(double.NaN)));
    Assert.IsTrue(Posit16.IsNaR(Posit16.FromDouble(double.PositiveInfinity)));
    Assert.IsTrue(Posit16.IsNaR(Posit16.FromDouble(double.NegativeInfinity)));
  }

  [TestCase(1.0)]
  [TestCase(2.0)]
  [TestCase(0.5)]
  [TestCase(4.0)]
  [TestCase(0.25)]
  [TestCase(-1.0)]
  [TestCase(-2.0)]
  public void Posit16_RoundTrip(double value) {
    var p = Posit16.FromDouble(value);
    Assert.AreEqual(value, p.ToDouble(), Tolerance, $"Posit16 round-trip of {value}");
  }

  // --- Posit32 (nbits=32, es=2) ---

  [Test]
  public void Posit32_Zero_And_NaR() {
    Assert.AreEqual(0.0, Posit32.FromRaw(0x00000000u).ToDouble(), Tolerance);
    Assert.IsTrue(double.IsNaN(Posit32.FromRaw(0x80000000u).ToDouble()));
    Assert.IsTrue(Posit32.IsNaR(Posit32.NaR));
  }

  [Test]
  public void Posit32_NonFinite_EncodesToNaR() {
    Assert.IsTrue(Posit32.IsNaR(Posit32.FromDouble(double.NaN)));
    Assert.IsTrue(Posit32.IsNaR(Posit32.FromDouble(double.PositiveInfinity)));
    Assert.IsTrue(Posit32.IsNaR(Posit32.FromDouble(double.NegativeInfinity)));
  }

  [TestCase(1.0)]
  [TestCase(2.0)]
  [TestCase(0.5)]
  [TestCase(4.0)]
  [TestCase(0.25)]
  [TestCase(-1.0)]
  [TestCase(-2.0)]
  public void Posit32_RoundTrip(double value) {
    var p = Posit32.FromDouble(value);
    Assert.AreEqual(value, p.ToDouble(), Tolerance, $"Posit32 round-trip of {value}");
  }

  // --- Cross-cutting behavior ---

  [Test]
  public void Posit8_Negation_Works() {
    var one = Posit8.FromDouble(1.0);
    var negOne = -one;
    Assert.AreEqual(-1.0, negOne.ToDouble(), Tolerance);
    Assert.IsTrue(Posit8.IsNegative(negOne));
  }

  [Test]
  public void Posit8_Comparison_Works() {
    var a = Posit8.FromDouble(1.0);
    var b = Posit8.FromDouble(2.0);
    Assert.IsTrue(a < b);
    Assert.IsTrue(b > a);
    Assert.IsTrue(a == Posit8.FromDouble(1.0));
  }

  [Test]
  public void Posit8_Arithmetic_Works() {
    var a = Posit8.FromDouble(2.0);
    var b = Posit8.FromDouble(1.0);
    Assert.AreEqual(3.0, (a + b).ToDouble(), Tolerance);
    Assert.AreEqual(1.0, (a - b).ToDouble(), Tolerance);
    Assert.AreEqual(2.0, (a * b).ToDouble(), Tolerance);
    Assert.AreEqual(2.0, (a / b).ToDouble(), Tolerance);
  }

}
