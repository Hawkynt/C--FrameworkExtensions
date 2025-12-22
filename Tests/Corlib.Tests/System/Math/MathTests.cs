using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using static System.MathEx;

namespace System.MathExtensionsTests;

/// <summary>
///   Core math extension tests
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region Min/Max Tests

  private static IEnumerable<TestCaseData> MinMaxTestData() {
    yield return new TestCaseData(new byte[] { 0 }, 0, 0).SetName("MinMax_SingleZero");
    yield return new TestCaseData(new byte[] { 0, 0 }, 0, 0).SetName("MinMax_TwoZeros");
    yield return new TestCaseData(new byte[] { 1, 0 }, 0, 1).SetName("MinMax_ZeroAndOne");
    yield return new TestCaseData(new byte[] { 0, 1 }, 0, 1).SetName("MinMax_OneAndZero");
    yield return new TestCaseData(new byte[] { 5, 2, 8, 1, 9 }, 1, 9).SetName("MinMax_MixedValues");
    yield return new TestCaseData(new byte[] { 255 }, 255, 255).SetName("MinMax_MaxByte");
  }

  [Test]
  [TestCaseSource(nameof(MinMaxTestData))]
  [Category("HappyPath")]
  [Description("Validates Min and Max functions return correct values")]
  public void MinMax_VariousArrays_ReturnsCorrectValues(byte[] values, int expectedMin, int expectedMax) {
    var actualMin = Min(values);
    var actualMax = Max(values);
    Assert.That(actualMin, Is.EqualTo(expectedMin), "Min value incorrect");
    Assert.That(actualMax, Is.EqualTo(expectedMax), "Max value incorrect");
  }

  [Test]
  [Category("Exception")]
  [Description("Validates Min/Max handle null array")]
  public void MinMax_NullArray_ThrowsArgumentNullException() {
    byte[]? nullArray = null;
    Assert.Throws<NullReferenceException>(() => Min(nullArray));
    Assert.Throws<NullReferenceException>(() => Max(nullArray));
  }

  [Test]
  [Category("Performance")]
  [Description("Validates Min/Max performance with large arrays")]
  public void MinMax_LargeArray_CompletesQuickly() {
    var random = new Random(42);
    var largeArray = new byte[1_000_000];
    random.NextBytes(largeArray);

    // Warmup to reduce JIT impact
    _ = Min(largeArray);
    _ = Max(largeArray);

    var sw = Stopwatch.StartNew();
    var min = Min(largeArray);
    var max = Max(largeArray);
    sw.Stop();

    Assert.That(min, Is.LessThanOrEqualTo(max));
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(200),
      $"Min/Max on 1M elements took {sw.ElapsedMilliseconds}ms"
    );
  }

  #endregion
}
