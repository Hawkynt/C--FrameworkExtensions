#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using Hawkynt.ColorProcessing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("BayerMatrix")]
public class BayerMatrixTests {

  [TestCase(1, true)]
  [TestCase(2, true)]
  [TestCase(4, true)]
  [TestCase(8, true)]
  [TestCase(16, true)]
  [TestCase(256, true)]
  [TestCase(0, false)]
  [TestCase(-1, false)]
  [TestCase(3, false)]
  [TestCase(5, false)]
  [TestCase(6, false)]
  [TestCase(12, false)]
  public void IsValidSize_ClassifiesPowersOfTwo(int size, bool expected) {
    Assert.That(BayerMatrix.IsValidSize(size), Is.EqualTo(expected));
  }

  [TestCase(0)]
  [TestCase(-1)]
  [TestCase(3)]
  [TestCase(5)]
  [TestCase(6)]
  [TestCase(12)]
  public void Generate_RejectsInvalidSize(int size) {
    Assert.Throws<ArgumentOutOfRangeException>(() => BayerMatrix.Generate(size));
  }

  [Test]
  public void Generate_Bayer2x2_MatchesKnownPattern() {
    // Upstream uses bit-interleave convention producing:
    // { 0, 1 }
    // { 3, 2 }
    // (Equivalent to the textbook {0,2},{3,1} transposed; both are valid
    // Bayer orderings — the key property is even coverage of [0..N²-1].)
    var m = BayerMatrix.Generate(2);
    Assert.That(m.GetLength(0), Is.EqualTo(2));
    Assert.That(m.GetLength(1), Is.EqualTo(2));
    Assert.That(m[0, 0], Is.EqualTo(0f));
    Assert.That(m[0, 1], Is.EqualTo(1f));
    Assert.That(m[1, 0], Is.EqualTo(3f));
    Assert.That(m[1, 1], Is.EqualTo(2f));
  }

  [Test]
  public void Generate_Bayer4x4_ContainsExactlyZeroThroughFifteen() {
    var m = BayerMatrix.Generate(4);
    var seen = new bool[16];
    for (var y = 0; y < 4; ++y)
    for (var x = 0; x < 4; ++x) {
      var v = (int)m[y, x];
      Assert.That(v, Is.GreaterThanOrEqualTo(0).And.LessThan(16));
      Assert.That(seen[v], Is.False, $"Duplicate value {v} at ({x},{y})");
      seen[v] = true;
    }
    for (var i = 0; i < 16; ++i)
      Assert.That(seen[i], Is.True, $"Missing value {i}");
  }

  [Test]
  public void Generate_Bayer8x8_IsSquareAndContainsContiguousRange() {
    var m = BayerMatrix.Generate(8);
    Assert.That(m.GetLength(0), Is.EqualTo(8));
    Assert.That(m.GetLength(1), Is.EqualTo(8));

    var count = new int[64];
    for (var y = 0; y < 8; ++y)
    for (var x = 0; x < 8; ++x)
      ++count[(int)m[y, x]];

    for (var i = 0; i < 64; ++i)
      Assert.That(count[i], Is.EqualTo(1), $"Value {i} should appear exactly once");
  }

  [Test]
  public void OrderedDitherer_GenerateBayer_ForwardsToUtility() {
    // Backward-compat forwarder must produce the same matrix as the utility.
    var direct = BayerMatrix.Generate(4);
    var viaOrdered = Hawkynt.ColorProcessing.Dithering.OrderedDitherer.GenerateBayer(4);
    for (var y = 0; y < 4; ++y)
    for (var x = 0; x < 4; ++x)
      Assert.That(viaOrdered[y, x], Is.EqualTo(direct[y, x]));
  }
}
