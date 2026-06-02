using System.Globalization;
using NUnit.Framework;

namespace System;

/// <summary>
/// Spec behavior of the IEEE-754 decimal32 (BID) type: exact base-10 arithmetic and the BID bit layout.
/// </summary>
[TestFixture]
public class Decimal32SpecTests {

  private static Decimal32 D(string s) => Decimal32.Parse(s, CultureInfo.InvariantCulture);

  [Test]
  public void DecimalFractions_AddExactly() {
    // The whole point of decimal: this is exactly 0.3, unlike binary float where 0.1 + 0.2 == 0.30000000000000004.
    Assert.AreEqual("0.3", (D("0.1") + D("0.2")).ToString());
    Assert.AreEqual("0.0", (D("0.1") + D("0.2") - D("0.3")).ToString());
  }

  [Test]
  public void Multiply_And_Divide_AreExactDecimal() {
    Assert.AreEqual("1.21", (D("1.1") * D("1.1")).ToString());
    Assert.AreEqual("0.3333333", (D("1") / D("3")).ToString());   // rounded to 7 significant digits
    Assert.AreEqual("2.5", (D("5") / D("2")).ToString());
  }

  [Test]
  public void RoundsToSevenSignificantDigits_HalfEven() {
    Assert.AreEqual("1234.568", D("1234.5675").ToString());  // 7th digit 7 is odd -> rounds up to ...568
    Assert.AreEqual("1234.566", D("1234.5665").ToString());  // 7th digit 6 is even -> stays ...566
  }

  [Test]
  public void Bid_Encoding_Anchor_One() {
    // 1 = coefficient 1, exponent 0 -> biased exponent 101 (0x65) in the small-coefficient form.
    Assert.AreEqual(0x32800001u, Decimal32.One.RawValue);
  }

  [Test]
  public void Bid_RoundTrips_AcrossRepresentableValues() {
    foreach (var s in new[] { "0", "1", "-1", "0.1", "9.999999", "1234567", "-0.0001234", "1E+90", "9.999999E-95" }) {
      var d = D(s);
      Assert.AreEqual(d.RawValue, Decimal32.FromRaw(d.RawValue).RawValue, s);
    }
  }

  [Test]
  public void Specials() {
    Assert.IsTrue(Decimal32.IsInfinity(D("1") / D("0")));
    Assert.IsTrue(Decimal32.IsNaN(D("0") / D("0")));
    Assert.IsTrue(Decimal32.IsNaN(Decimal32.PositiveInfinity - Decimal32.PositiveInfinity));
    Assert.AreEqual("Infinity", Decimal32.PositiveInfinity.ToString());
  }

  [Test]
  public void Overflow_GoesToInfinity() => Assert.IsTrue(Decimal32.IsInfinity(Decimal32.MaxValue * D("10")));

  [Test]
  public void NotSameAsBinaryFloat_ForDecimalFractions() {
    // binary double can't hold 0.1+0.2 exactly; decimal32 can.
    Assert.AreNotEqual(0.3, 0.1 + 0.2);          // 0.30000000000000004
    Assert.AreEqual("0.3", (D("0.1") + D("0.2")).ToString());
  }
}
