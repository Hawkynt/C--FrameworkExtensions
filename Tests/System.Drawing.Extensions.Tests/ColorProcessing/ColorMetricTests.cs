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
using Hawkynt.Drawing.ColorDomain;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("ColorDomain")]
public class ColorMetricTests {

  [TestCase(ColorMetric.Euclidean)]
  [TestCase(ColorMetric.EuclideanRgbOnly)]
  [TestCase(ColorMetric.EuclideanBT709)]
  [TestCase(ColorMetric.EuclideanNommyde)]
  [TestCase(ColorMetric.WeightedEuclideanLowRed)]
  [TestCase(ColorMetric.WeightedEuclideanHighRed)]
  [TestCase(ColorMetric.Manhattan)]
  [TestCase(ColorMetric.ManhattanRgbOnly)]
  [TestCase(ColorMetric.ManhattanBT709)]
  [TestCase(ColorMetric.ManhattanNommyde)]
  [TestCase(ColorMetric.WeightedManhattanLowRed)]
  [TestCase(ColorMetric.WeightedManhattanHighRed)]
  [TestCase(ColorMetric.CompuPhase)]
  [TestCase(ColorMetric.PngQuant)]
  [TestCase(ColorMetric.WeightedYuv)]
  [TestCase(ColorMetric.WeightedYCbCr)]
  [TestCase(ColorMetric.Cie94Textiles)]
  [TestCase(ColorMetric.Cie94GraphicArts)]
  [TestCase(ColorMetric.CieDe2000)]
  public void Metric_IdenticalColors_DistanceIsZero(ColorMetric metric) {
    var c = Color.FromArgb(255, 128, 64, 200);
    Assert.That(metric.Calculate(c, c), Is.EqualTo(0));
  }

  [TestCase(ColorMetric.Euclidean)]
  [TestCase(ColorMetric.Manhattan)]
  [TestCase(ColorMetric.CompuPhase)]
  [TestCase(ColorMetric.PngQuant)]
  [TestCase(ColorMetric.WeightedYuv)]
  [TestCase(ColorMetric.WeightedYCbCr)]
  [TestCase(ColorMetric.Cie94GraphicArts)]
  [TestCase(ColorMetric.CieDe2000)]
  public void Metric_BlackVsWhite_IsLargest(ColorMetric metric) {
    var black = Color.FromArgb(255, 0, 0, 0);
    var white = Color.FromArgb(255, 255, 255, 255);
    var midGray = Color.FromArgb(255, 128, 128, 128);
    var distBlackWhite = metric.Calculate(black, white);
    var distBlackMid = metric.Calculate(black, midGray);
    Assert.That(distBlackWhite, Is.GreaterThan(distBlackMid),
      "Black-vs-white distance should exceed black-vs-mid-gray");
  }

  [Test]
  public void Metric_AsFunc_MatchesCalculate() {
    var a = Color.FromArgb(255, 200, 100, 50);
    var b = Color.FromArgb(255, 50, 200, 150);
    foreach (ColorMetric m in Enum.GetValues(typeof(ColorMetric))) {
      var viaFunc = m.AsFunc()(a, b);
      var viaDirect = m.Calculate(a, b);
      Assert.That(viaFunc, Is.EqualTo(viaDirect), $"{m}: AsFunc and Calculate disagree");
    }
  }

  [Test]
  public void Euclidean_RGBOnly_IgnoresAlpha() {
    var opaque = Color.FromArgb(255, 100, 50, 200);
    var translucent = Color.FromArgb(64, 100, 50, 200);
    Assert.That(ColorMetric.EuclideanRgbOnly.Calculate(opaque, translucent), Is.EqualTo(0));
  }

  [Test]
  public void Manhattan_RGBOnly_IgnoresAlpha() {
    var opaque = Color.FromArgb(255, 50, 50, 50);
    var translucent = Color.FromArgb(64, 50, 50, 50);
    Assert.That(ColorMetric.ManhattanRgbOnly.Calculate(opaque, translucent), Is.EqualTo(0));
  }

  [Test]
  public void Euclidean_HasNonZeroDistanceForSlightDifference() {
    var a = Color.FromArgb(255, 100, 100, 100);
    var b = Color.FromArgb(255, 101, 100, 100);
    Assert.That(ColorMetric.Euclidean.Calculate(a, b), Is.EqualTo(1));
  }

  [Test]
  public void Manhattan_HasExpectedSimpleResult() {
    var a = Color.FromArgb(255, 0, 0, 0);
    var b = Color.FromArgb(255, 10, 20, 30);
    Assert.That(ColorMetric.Manhattan.Calculate(a, b), Is.EqualTo(60));
  }

  [Test]
  public void AsFunc_UnknownMetric_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ((ColorMetric)999).AsFunc());
  }
}
