using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace System;

[TestFixture]
public partial class MathTests {

  [Test]
  public void MinMax_Int_ReturnsCorrectValues() {
    var result = System.Math.Min(5, 3);
    Assert.AreEqual(3, result);
    
    result = System.Math.Max(5, 3);
    Assert.AreEqual(5, result);
  }

  private static IEnumerable<TestCaseData> _MinMaxGenerator() {
    yield return new(new byte[] { 0 }, 0, 0);
    yield return new(new byte[] { 0, 0 }, 0, 0);
    yield return new(new byte[] { 1, 0 }, 0, 1);
    yield return new(new byte[] { 0, 1 }, 0, 1);
  }

  [Test]
  [TestCaseSource(nameof(_MinMaxGenerator))]
  public void MinMax(byte[] values, int minValue, int maxValue) {
    Assert.AreEqual(minValue, values.Min());
    Assert.AreEqual(maxValue, values.Max());
  }

}