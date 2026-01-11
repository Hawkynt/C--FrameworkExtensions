using NUnit.Framework;

namespace System;

[TestFixture]
public class FixedPointTests {

  private const double Tolerance = 0.01;

  // Q7_8 tests
  [Test]
  public void Q7_8_Constants_AreCorrect() {
    Assert.AreEqual(0, (double)Q7_8.Zero, Tolerance);
    Assert.AreEqual(1, (double)Q7_8.One, Tolerance);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(0.5)]
  [TestCase(-0.5)]
  [TestCase(127.0)]
  [TestCase(-128.0)]
  public void Q7_8_FromDouble_RoundTrips(double value) {
    var q = (Q7_8)(double)value;
    Assert.AreEqual(value, (double)q, Tolerance);
  }

  [Test]
  public void Q7_8_Arithmetic_Works() {
    var a = (Q7_8)2.5;
    var b = (Q7_8)1.5;
    Assert.AreEqual(4.0, (double)(a + b), Tolerance);
    Assert.AreEqual(1.0, (double)(a - b), Tolerance);
    Assert.AreEqual(3.75, (double)(a * b), Tolerance);
    Assert.AreEqual(1.666, (double)(a / b), 0.1);
  }

  [Test]
  public void Q7_8_Comparison_Works() {
    var a = (Q7_8)2.5;
    var b = (Q7_8)1.5;
    var c = (Q7_8)2.5;
    Assert.IsTrue(a > b);
    Assert.IsTrue(b < a);
    Assert.IsTrue(a == c);
    Assert.IsTrue(a >= c);
    Assert.IsTrue(a <= c);
  }

  // Q15_16 tests
  [Test]
  public void Q15_16_Constants_AreCorrect() {
    Assert.AreEqual(0, (double)Q15_16.Zero, Tolerance);
    Assert.AreEqual(1, (double)Q15_16.One, Tolerance);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(0.5)]
  [TestCase(1000.25)]
  [TestCase(-1000.25)]
  public void Q15_16_FromDouble_RoundTrips(double value) {
    var q = (Q15_16)(double)value;
    Assert.AreEqual(value, (double)q, Tolerance);
  }

  [Test]
  public void Q15_16_Arithmetic_Works() {
    var a = (Q15_16)100.5;
    var b = (Q15_16)50.25;
    Assert.AreEqual(150.75, (double)(a + b), Tolerance);
    Assert.AreEqual(50.25, (double)(a - b), Tolerance);
    Assert.AreEqual(5050.125, (double)(a * b), 1.0);
    Assert.AreEqual(2.0, (double)(a / b), Tolerance);
  }

  // Q31_32 tests
  [Test]
  public void Q31_32_Constants_AreCorrect() {
    Assert.AreEqual(0, (double)Q31_32.Zero, Tolerance);
    Assert.AreEqual(1, (double)Q31_32.One, Tolerance);
  }

  [Test]
  public void Q31_32_LargeValues_Work() {
    var a = (Q31_32)1000000.0;
    var b = (Q31_32)2.0;
    Assert.AreEqual(2000000.0, (double)(a * b), 1.0);
  }

  // UQ8_8 tests
  [Test]
  public void UQ8_8_Constants_AreCorrect() {
    Assert.AreEqual(0, (double)UQ8_8.Zero, Tolerance);
    Assert.AreEqual(1, (double)UQ8_8.One, Tolerance);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(0.5)]
  [TestCase(255.0)]
  public void UQ8_8_FromDouble_RoundTrips(double value) {
    var q = (UQ8_8)(double)value;
    Assert.AreEqual(value, (double)q, Tolerance);
  }

  // UQ16_16 tests
  [Test]
  public void UQ16_16_Constants_AreCorrect() {
    Assert.AreEqual(0, (double)UQ16_16.Zero, Tolerance);
    Assert.AreEqual(1, (double)UQ16_16.One, Tolerance);
  }

  [Test]
  public void UQ16_16_Arithmetic_Works() {
    var a = (UQ16_16)100.5;
    var b = (UQ16_16)50.25;
    Assert.AreEqual(150.75, (double)(a + b), Tolerance);
    Assert.AreEqual(50.25, (double)(a - b), Tolerance);
  }

  // UQ32_32 tests
  [Test]
  public void UQ32_32_Constants_AreCorrect() {
    Assert.AreEqual(0, (double)UQ32_32.Zero, Tolerance);
    Assert.AreEqual(1, (double)UQ32_32.One, Tolerance);
  }

  // Widening conversion tests
  [Test]
  public void Q7_8_ToQ15_16_Widening_Works() {
    Q7_8 small = (Q7_8)12.5;
    Q15_16 wide = small;
    Assert.AreEqual(12.5, (double)wide, Tolerance);
  }

  [Test]
  public void UQ8_8_ToUQ16_16_Widening_Works() {
    UQ8_8 small = (UQ8_8)50.25;
    UQ16_16 wide = small;
    Assert.AreEqual(50.25, (double)wide, Tolerance);
  }

}
