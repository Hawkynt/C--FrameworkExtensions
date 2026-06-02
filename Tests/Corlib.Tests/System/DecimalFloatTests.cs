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

  // ---------------------------------------------------------------------------
  // Decimal32 (coefficient 0..8388607, unbiased exponent -128..127, bias 128)
  // ---------------------------------------------------------------------------

  [Test]
  public void Decimal32_Zero_IsZero() {
    Assert.AreEqual(0d, Decimal32.Zero.ToDouble(), 0d);
  }

  [TestCase(1d)]
  [TestCase(10d)]
  [TestCase(100d)]
  [TestCase(12d)]
  [TestCase(1.2d)]
  [TestCase(120d)]
  [TestCase(-3.4d)]
  [TestCase(123456d)]
  [TestCase(0.000123d)]
  public void Decimal32_RoundTrip_RepresentableValues(double value) {
    var decoded = Decimal32.FromDouble(value).ToDouble();
    var tolerance = Math.Abs(value) * 1e-9 + 1e-12;
    Assert.AreEqual(value, decoded, tolerance);
  }

  [Test]
  public void Decimal32_FromSingle_RoundTrips() {
    var encoded = Decimal32.FromSingle(1.2f);
    Assert.AreEqual(1.2f, encoded.ToSingle(), 1e-4f);
  }

  [Test]
  public void Decimal32_Overflow_Saturates() {
    var encoded = Decimal32.FromDouble(1e300);
    Assert.AreEqual(Decimal32.MaxValue.ToDouble(), encoded.ToDouble(), 0d);
  }

  [Test]
  public void Decimal32_NegativeOverflow_SaturatesToMin() {
    var encoded = Decimal32.FromDouble(-1e300);
    Assert.AreEqual(Decimal32.MinValue.ToDouble(), encoded.ToDouble(), 0d);
  }

  [Test]
  public void Decimal32_Sign_IsPreserved() {
    var encoded = Decimal32.FromDouble(-3.4d);
    Assert.IsTrue(Decimal32.IsNegative(encoded));
    Assert.AreEqual(-3.4d, encoded.ToDouble(), 1e-9);
  }

  [Test]
  public void Decimal32_Arithmetic_Works() {
    var a = Decimal32.FromDouble(2d);
    var b = Decimal32.FromDouble(3d);
    Assert.AreEqual(5d, (a + b).ToDouble(), 1e-9);
    Assert.AreEqual(-1d, (a - b).ToDouble(), 1e-9);
    Assert.AreEqual(6d, (a * b).ToDouble(), 1e-9);
    Assert.AreEqual(2d / 3d, (a / b).ToDouble(), 1e-6);
  }

  [Test]
  public void Decimal32_Parse_RoundTrips() {
    Assert.IsTrue(Decimal32.TryParse("12.5", CultureInfo.InvariantCulture, out var parsed));
    Assert.AreEqual(12.5d, parsed.ToDouble(), 1e-9);
  }

  // ---------------------------------------------------------------------------
  // Decimal64 (coefficient 0..2^53-1, unbiased exponent -512..511, bias 512)
  // ---------------------------------------------------------------------------

  [Test]
  public void Decimal64_Zero_IsZero() {
    Assert.AreEqual(0d, Decimal64.Zero.ToDouble(), 0d);
  }

  [TestCase(1d)]
  [TestCase(10d)]
  [TestCase(100d)]
  [TestCase(12d)]
  [TestCase(1.2d)]
  [TestCase(-3.4d)]
  [TestCase(123456789d)]
  [TestCase(0.0000001d)]
  [TestCase(9999999999d)]
  public void Decimal64_RoundTrip_RepresentableValues(double value) {
    var decoded = Decimal64.FromDouble(value).ToDouble();
    var tolerance = Math.Abs(value) * 1e-12 + 1e-15;
    Assert.AreEqual(value, decoded, tolerance);
  }

  [Test]
  public void Decimal64_Overflow_Saturates() {
    var encoded = Decimal64.FromDouble(1e308 * 10);
    Assert.AreEqual(Decimal64.MaxValue.ToDouble(), encoded.ToDouble(), 0d);
  }

  [Test]
  public void Decimal64_Sign_IsPreserved() {
    Assert.IsTrue(Decimal64.IsNegative(Decimal64.FromDouble(-3.4d)));
    Assert.IsFalse(Decimal64.IsNegative(Decimal64.FromDouble(3.4d)));
  }

  [Test]
  public void Decimal64_CoefficientAndExponent_Accessible() {
    var encoded = Decimal64.FromDouble(120d);
    // 120 == 12 * 10^1 or 120 * 10^0 depending on most-precise fit; reconstruct value.
    var reconstructed = encoded.Coefficient * Math.Pow(10, encoded.Exponent);
    Assert.AreEqual(120d, reconstructed, 1e-6);
  }

}
