using System.Globalization;
using NUnit.Framework;

namespace System;

/// <summary>
/// Spec behavior of the IEEE-754 decimal64 (BID) type: exact base-10 arithmetic (16 digits) and BID layout.
/// </summary>
[TestFixture]
public class Decimal64SpecTests {

  private static Decimal64 D(string s) => Decimal64.Parse(s, CultureInfo.InvariantCulture);

  [Test]
  public void DecimalFractions_AddExactly() {
    Assert.AreEqual("0.3", (D("0.1") + D("0.2")).ToString());
    Assert.AreEqual("0.0", (D("0.1") + D("0.2") - D("0.3")).ToString());
  }

  [Test]
  public void SixteenDigitsAreExact() {
    Assert.AreEqual("12345.67", (D("12340.00") + D("5.67")).ToString());
    Assert.AreEqual("0.3333333333333333", (D("1") / D("3")).ToString()); // 16 significant digits
    Assert.AreEqual("2.5", (D("5") / D("2")).ToString());
  }

  [Test]
  public void Bid_Encoding_Anchor_One() {
    // 1 = coefficient 1, exponent 0 -> biased exponent 398, small-coefficient form (E << 53 | 1).
    Assert.AreEqual((398UL << 53) | 1UL, Decimal64.One.RawValue);
  }

  [Test]
  public void Bid_RoundTrips() {
    foreach (var s in new[] { "0", "1", "-1", "0.1", "9999999999999999", "1E+300", "9.999999999999999E-380", "-0.00000001234" }) {
      var d = D(s);
      Assert.AreEqual(d.RawValue, Decimal64.FromRaw(d.RawValue).RawValue, s);
    }
  }

  [Test]
  public void Specials() {
    Assert.IsTrue(Decimal64.IsInfinity(D("1") / D("0")));
    Assert.IsTrue(Decimal64.IsNaN(D("0") / D("0")));
    Assert.IsTrue(Decimal64.IsInfinity(Decimal64.MaxValue * D("100")));
    Assert.AreEqual("-Infinity", Decimal64.NegativeInfinity.ToString());
  }

  [Test]
  public void MonetarySum_IsExact() {
    var total = Decimal64.Zero;
    for (var i = 0; i < 10; ++i)
      total += D("0.1");
    Assert.AreEqual("1.0", total.ToString()); // 10 * 0.1 == 1.0 exactly (binary double would drift)
  }
}
