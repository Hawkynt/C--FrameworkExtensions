using System.Diagnostics;
using NUnit.Framework;

namespace System.MathExtensionsTests;

/// <summary>
///   Tests for arithmetic math operations
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region Basic Arithmetic Tests

  [Test]
  [TestCase(5, 3, 8)]
  [TestCase(0, 0, 0)]
  [TestCase(-5, 5, 0)]
  [TestCase(int.MaxValue, 0, int.MaxValue)]
  [Category("HappyPath")]
  [Description("Validates Add operation for various inputs")]
  public void Add_VariousInputs_ReturnsCorrectSum(int a, int b, int expected) {
    var result = a.Add(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(10, 3, 7)]
  [TestCase(0, 0, 0)]
  [TestCase(-5, -5, 0)]
  [TestCase(int.MinValue, -1, int.MinValue + 1)]
  [Category("HappyPath")]
  [Description("Validates Subtract operation for various inputs")]
  public void Subtract_VariousInputs_ReturnsCorrectDifference(int a, int b, int expected) {
    var result = a.Subtract(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Division Tests

  [Test]
  [TestCase(10, 2, 5)]
  [TestCase(15, 3, 5)]
  [TestCase(7, 2, 3)] // Integer division
  [TestCase(-10, 2, -5)]
  [Category("HappyPath")]
  [Description("Validates integer division")]
  public void Divide_Integers_ReturnsQuotient(int dividend, int divisor, int expected) {
    var result = dividend.DividedBy(divisor);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("Exception")]
  [Description("Validates division by zero throws")]
  public void Divide_ByZero_ThrowsDivideByZeroException() {
    var dividend = 10;
    var divisor = 0;
    Assert.Throws<DivideByZeroException>(() => dividend.DividedBy(divisor));
  }

  [Test]
  [TestCase(10.0, 3.0, 3.333333)]
  [TestCase(1.0, 3.0, 0.333333)]
  [TestCase(-10.0, 4.0, -2.5)]
  [Category("HappyPath")]
  [Description("Validates floating point division")]
  public void Divide_FloatingPoint_ReturnsAccurateQuotient(double dividend, double divisor, double expected) {
    var result = dividend.DividedBy(divisor);
    Assert.That(result, Is.EqualTo(expected).Within(0.000001));
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  [Description("Validates arithmetic operations performance")]
  public void ArithmeticOperations_ManyIterations_CompletesQuickly() {
    var sw = Stopwatch.StartNew();
    var sum = 0;
    for (var i = 0; i < 1_000_000; ++i) {
      sum = sum.Add(1);
      sum = sum.Subtract(1);
      sum = sum.Add(i.DividedBy(10));
      sum = sum.Subtract(i.MultipliedWith(10));
    }

    sw.Stop();
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(100),
      $"1M arithmetic operations took {sw.ElapsedMilliseconds}ms"
    );
  }

  #endregion
}
