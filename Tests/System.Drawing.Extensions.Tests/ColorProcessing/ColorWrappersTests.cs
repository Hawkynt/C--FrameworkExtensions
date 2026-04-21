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

using System.Drawing;
using System.Linq;
using Hawkynt.ColorProcessing.Quantization;
using Hawkynt.Drawing.ColorDomain;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("ColorDomain")]
public class ColorWrappersTests {

  private static (Color color, uint count)[] BuildHistogram() => new[] {
    (Color.Red,     100u),
    (Color.Green,   80u),
    (Color.Blue,    60u),
    (Color.Yellow,  40u),
    (Color.Cyan,    30u),
    (Color.Magenta, 20u),
    (Color.Gray,    10u),
  };

  #region PaletteLookup

  [Test]
  public void PaletteLookup_ExactMatch_ReturnsExactIndex() {
    var palette = new[] { Color.Red, Color.Green, Color.Blue };
    var lookup = new PaletteLookup(palette);
    Assert.That(lookup.FindClosestColorIndex(Color.Red), Is.EqualTo(0));
    Assert.That(lookup.FindClosestColorIndex(Color.Green), Is.EqualTo(1));
    Assert.That(lookup.FindClosestColorIndex(Color.Blue), Is.EqualTo(2));
  }

  [Test]
  public void PaletteLookup_EmptyPalette_ReturnsMinusOne() {
    var lookup = new PaletteLookup([]);
    Assert.That(lookup.FindClosestColorIndex(Color.Red), Is.EqualTo(-1));
  }

  [Test]
  public void PaletteLookup_HonorsCustomMetric() {
    var palette = new[] { Color.Red, Color.Black };
    var dimRed = Color.FromArgb(255, 50, 0, 0);

    var euclid = new PaletteLookup(palette, ColorMetric.Euclidean.AsFunc());
    var manhattan = new PaletteLookup(palette, ColorMetric.Manhattan.AsFunc());
    Assert.That(euclid.FindClosestColorIndex(dimRed), Is.EqualTo(manhattan.FindClosestColorIndex(dimRed)),
      "Both metrics should still pick black for a very dim red");
  }

  [Test]
  public void PaletteLookup_RepeatedQuery_HitsCache() {
    // Use a metric that has identifiable side-effects on a counter.
    var calls = 0;
    var lookup = new PaletteLookup(new[] { Color.Red, Color.Blue }, (a, b) => { ++calls; return Math.Abs(a.R - b.R); });
    var c = Color.FromArgb(255, 100, 0, 0);
    lookup.FindClosestColorIndex(c);
    var afterFirst = calls;
    lookup.FindClosestColorIndex(c);
    Assert.That(calls, Is.EqualTo(afterFirst), "Repeated query for same color must hit the cache");
  }

  [Test]
  public void PaletteLookup_CountAndIndexer() {
    var palette = new[] { Color.Red, Color.Blue, Color.Green };
    var lookup = new PaletteLookup(palette);
    Assert.That(lookup.Count, Is.EqualTo(3));
    Assert.That(lookup[1].ToArgb(), Is.EqualTo(Color.Blue.ToArgb()));
  }

  #endregion

  #region KMeansColorRefinementWrapper

  [Test]
  public void KMeansRefinement_ReturnsRequestedSize() {
    var inner = new ColorQuantizerAdapter(new OctreeQuantizer());
    var wrapper = new KMeansColorRefinementWrapper(inner, iterations: 5, ColorMetric.Euclidean.AsFunc());
    var palette = wrapper.ReduceColorsTo(4, BuildHistogram());
    Assert.That(palette.Length, Is.EqualTo(4));
  }

  [Test]
  public void KMeansRefinement_ZeroIterations_PassesInnerOutputThrough() {
    var inner = new ColorQuantizerAdapter(new OctreeQuantizer());
    var wrapper = new KMeansColorRefinementWrapper(inner, iterations: 0, ColorMetric.Euclidean.AsFunc());

    var direct = inner.ReduceColorsTo(4, BuildHistogram());
    var refined = wrapper.ReduceColorsTo(4, BuildHistogram());

    CollectionAssert.AreEqual(direct.Select(c => c.ToArgb()).ToArray(),
                              refined.Select(c => c.ToArgb()).ToArray());
  }

  [Test]
  public void KMeansRefinement_NullInner_Throws() {
    Assert.Throws<ArgumentNullException>(() =>
      _ = new KMeansColorRefinementWrapper(null!, 5, ColorMetric.Euclidean.AsFunc()));
  }

  [Test]
  public void KMeansRefinement_NegativeIterations_Throws() {
    var inner = new ColorQuantizerAdapter(new OctreeQuantizer());
    Assert.Throws<ArgumentOutOfRangeException>(() =>
      _ = new KMeansColorRefinementWrapper(inner, -1, ColorMetric.Euclidean.AsFunc()));
  }

  #endregion

  #region PcaColorQuantizerWrapper

  [Test]
  public void PcaWrapper_ReturnsRequestedSize() {
    var inner = new ColorQuantizerAdapter(new OctreeQuantizer());
    var wrapper = new PcaColorQuantizerWrapper(inner);
    var palette = wrapper.ReduceColorsTo(4, BuildHistogram());
    Assert.That(palette.Length, Is.EqualTo(4));
  }

  [Test]
  public void PcaWrapper_EmptyHistogram_ReturnsEmpty() {
    var inner = new ColorQuantizerAdapter(new OctreeQuantizer());
    var wrapper = new PcaColorQuantizerWrapper(inner);
    Assert.That(wrapper.ReduceColorsTo(4, Array.Empty<Color>()), Is.Empty);
  }

  [Test]
  public void PcaWrapper_NullInner_Throws() {
    Assert.Throws<ArgumentNullException>(() => _ = new PcaColorQuantizerWrapper(null!));
  }

  #endregion
}
