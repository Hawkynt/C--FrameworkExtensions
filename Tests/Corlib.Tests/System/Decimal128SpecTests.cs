using System.Globalization;
using NUnit.Framework;

namespace System;

/// <summary>
/// Spec behavior of IEEE-754 decimal128 (BID), whose 128-bit format parameters (precision 34, emax 6144,
/// bias 6176) are derived by the same width formula that produces decimal32/decimal64.
/// </summary>
[TestFixture]
public class Decimal128SpecTests {

  private static Decimal128 D(string s) => Decimal128.Parse(s, CultureInfo.InvariantCulture);

  [Test]
  public void DecimalFractions_AddExactly() {
    Assert.AreEqual("0.3", (D("0.1") + D("0.2")).ToString());
  }

  [Test]
  public void ThirtyFourDigitsAreExact() {
    var s = "1.234567890123456789012345678901234"; // 34 significant digits
    Assert.AreEqual(s, D(s).ToString());
    Assert.AreEqual("0.3333333333333333333333333333333333", (D("1") / D("3")).ToString()); // 34 threes
  }

  [Test]
  public void Bid_Encoding_Anchor_One() {
    // 1 = coefficient 1, exponent 0 -> biased exponent 6176, small-coefficient form: raw = (6176 << 113) | 1.
    Assert.AreEqual(6176UL << 49, Decimal128.One.High);
    Assert.AreEqual(1UL, Decimal128.One.Low);
  }

  [Test]
  public void Bid_RoundTrips() {
    foreach (var s in new[] { "0", "1", "-1", "0.1", "1E+6000", "9.999999999999999999999999999999999E-6100", "-123456789.0123456789" }) {
      var d = D(s);
      var rt = Decimal128.FromRaw(d.High, d.Low);
      Assert.AreEqual(d.High, rt.High, s);
      Assert.AreEqual(d.Low, rt.Low, s);
    }
  }

  [Test]
  public void MaxValue_HasThirtyFourNines() {
    Assert.AreEqual(6111, Decimal128.MaxValue.Exponent);
    Assert.AreEqual(new string('9', 34), Decimal128.MaxValue.Coefficient.ToString(CultureInfo.InvariantCulture));
  }

  [Test]
  public void Specials() {
    Assert.IsTrue(Decimal128.IsInfinity(D("1") / D("0")));
    Assert.IsTrue(Decimal128.IsNaN(D("0") / D("0")));
    Assert.IsTrue(Decimal128.IsInfinity(Decimal128.MaxValue * D("10")));
    Assert.AreEqual("-Infinity", Decimal128.NegativeInfinity.ToString());
  }
}
