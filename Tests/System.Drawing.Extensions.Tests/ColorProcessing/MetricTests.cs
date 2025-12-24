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

using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Metrics.Lab;
using Hawkynt.ColorProcessing.Metrics.Rgb;
using Hawkynt.ColorProcessing.Spaces.Lab;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Working;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Metrics")]
public class MetricTests {

  private const float Tolerance = 0.001f;

  #region Test Colors

  private static readonly LinearRgbF White = new(1f, 1f, 1f);
  private static readonly LinearRgbF Black = new(0f, 0f, 0f);
  private static readonly LinearRgbF Red = new(1f, 0f, 0f);
  private static readonly LinearRgbF Green = new(0f, 1f, 0f);
  private static readonly LinearRgbF Blue = new(0f, 0f, 1f);
  private static readonly LinearRgbF Gray = new(0.5f, 0.5f, 0.5f);
  private static readonly LinearRgbF MixedColor1 = new(0.7f, 0.3f, 0.5f);
  private static readonly LinearRgbF MixedColor2 = new(0.2f, 0.8f, 0.4f);

  private static readonly LabF LabWhite = new(100f, 0f, 0f);
  private static readonly LabF LabBlack = new(0f, 0f, 0f);
  private static readonly LabF LabRed = new(53.23f, 80.11f, 67.22f);
  private static readonly LabF LabGreen = new(87.74f, -86.18f, 83.18f);
  private static readonly LabF LabBlue = new(32.30f, 79.20f, -107.86f);
  private static readonly LabF LabGray = new(53.39f, 0f, 0f);

  // DIN99 test colors (approximate)
  private static readonly Din99F Din99White = new(100f, 0f, 0f);
  private static readonly Din99F Din99Black = new(0f, 0f, 0f);
  private static readonly Din99F Din99Red = new(55f, 30f, 25f);
  private static readonly Din99F Din99Green = new(85f, -35f, 30f);
  private static readonly Din99F Din99Gray = new(55f, 0f, 0f);

  #endregion

  #region Euclidean RGB Tests

  [Test]
  [Category("HappyPath")]
  public void EuclideanRgb_SameColor_ReturnsZero() {
    var metric = new EuclideanRgb();
    var distance = metric.Distance(Red, Red);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void EuclideanRgb_BlackToWhite_ReturnsMaxDistance() {
    var metric = new EuclideanRgb();
    var distance = metric.Distance(Black, White);
    var expected = (float)Math.Sqrt(3f); // sqrt(1^2 + 1^2 + 1^2)
    Assert.That(distance, Is.EqualTo(expected).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void EuclideanRgb_IsSymmetric() {
    var metric = new EuclideanRgb();
    var distance1 = metric.Distance(MixedColor1, MixedColor2);
    var distance2 = metric.Distance(MixedColor2, MixedColor1);
    Assert.That(distance1, Is.EqualTo(distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void EuclideanRgb_TriangleInequality() {
    var metric = new EuclideanRgb();
    var distanceAB = metric.Distance(Black, Gray);
    var distanceBC = metric.Distance(Gray, White);
    var distanceAC = metric.Distance(Black, White);
    Assert.That(distanceAC, Is.LessThanOrEqualTo(distanceAB + distanceBC + Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void EuclideanRgb_PrimaryColors_KnownDistance() {
    var metric = new EuclideanRgb();
    var distance = metric.Distance(Red, Green);
    var expected = (float)Math.Sqrt(2f); // sqrt(1^2 + 1^2)
    Assert.That(distance, Is.EqualTo(expected).Within(Tolerance));
  }

  #endregion

  #region Euclidean3 Generic Tests

  [Test]
  [Category("HappyPath")]
  public void Euclidean3_LinearRgbF_SameColor_ReturnsZero() {
    var metric = new Euclidean3<LinearRgbF>();
    var distance = metric.Distance(Red, Red);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Euclidean3_LinearRgbF_IsSymmetric() {
    var metric = new Euclidean3<LinearRgbF>();
    var distance1 = metric.Distance(MixedColor1, MixedColor2);
    var distance2 = metric.Distance(MixedColor2, MixedColor1);
    Assert.That(distance1, Is.EqualTo(distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Euclidean3_LabF_SameColor_ReturnsZero() {
    var metric = new Euclidean3<LabF>();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Euclidean3_LabF_IsSymmetric() {
    var metric = new Euclidean3<LabF>();
    var distance1 = metric.Distance(LabRed, LabGreen);
    var distance2 = metric.Distance(LabGreen, LabRed);
    Assert.That(distance1, Is.EqualTo(distance2).Within(Tolerance));
  }

  #endregion

  #region Manhattan3 Tests

  [Test]
  [Category("HappyPath")]
  public void Manhattan3_LinearRgbF_SameColor_ReturnsZero() {
    var metric = new Manhattan3<LinearRgbF>();
    var distance = metric.Distance(Red, Red);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Manhattan3_LinearRgbF_BlackToWhite_ReturnsSum() {
    var metric = new Manhattan3<LinearRgbF>();
    var distance = metric.Distance(Black, White);
    Assert.That(distance, Is.EqualTo(3f).Within(Tolerance)); // |1| + |1| + |1|
  }

  [Test]
  [Category("HappyPath")]
  public void Manhattan3_LinearRgbF_IsSymmetric() {
    var metric = new Manhattan3<LinearRgbF>();
    var distance1 = metric.Distance(MixedColor1, MixedColor2);
    var distance2 = metric.Distance(MixedColor2, MixedColor1);
    Assert.That(distance1, Is.EqualTo(distance2).Within(Tolerance));
  }

  #endregion

  #region Chebyshev3 Tests

  [Test]
  [Category("HappyPath")]
  public void Chebyshev3_LinearRgbF_SameColor_ReturnsZero() {
    var metric = new Chebyshev3<LinearRgbF>();
    var distance = metric.Distance(Red, Red);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Chebyshev3_LinearRgbF_BlackToWhite_ReturnsMax() {
    var metric = new Chebyshev3<LinearRgbF>();
    var distance = metric.Distance(Black, White);
    Assert.That(distance, Is.EqualTo(1f).Within(Tolerance)); // max(|1|, |1|, |1|)
  }

  [Test]
  [Category("HappyPath")]
  public void Chebyshev3_LinearRgbF_IsSymmetric() {
    var metric = new Chebyshev3<LinearRgbF>();
    var distance1 = metric.Distance(MixedColor1, MixedColor2);
    var distance2 = metric.Distance(MixedColor2, MixedColor1);
    Assert.That(distance1, Is.EqualTo(distance2).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Chebyshev3_LinearRgbF_ReturnsMaxDifference() {
    var metric = new Chebyshev3<LinearRgbF>();
    var a = new LinearRgbF(0.5f, 0.2f, 0.8f);
    var b = new LinearRgbF(0.3f, 0.9f, 0.6f);
    var distance = metric.Distance(a, b);
    // Max of |0.5-0.3|=0.2, |0.2-0.9|=0.7, |0.8-0.6|=0.2 is 0.7
    Assert.That(distance, Is.EqualTo(0.7f).Within(Tolerance));
  }

  #endregion

  #region CompuPhase Tests

  [Test]
  [Category("HappyPath")]
  public void CompuPhase_SameColor_ReturnsZero() {
    var metric = new CompuPhase();
    var distance = metric.Distance(Red, Red);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhase_BlackToWhite_ReturnsPositive() {
    var metric = new CompuPhase();
    var distance = metric.Distance(Black, White);
    Assert.That(distance, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhase_IsSymmetric() {
    var metric = new CompuPhase();
    var distance1 = metric.Distance(MixedColor1, MixedColor2);
    var distance2 = metric.Distance(MixedColor2, MixedColor1);
    Assert.That(distance1, Is.EqualTo(distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhase_TriangleInequality() {
    var metric = new CompuPhase();
    var distanceAB = metric.Distance(Black, Gray);
    var distanceBC = metric.Distance(Gray, White);
    var distanceAC = metric.Distance(Black, White);
    Assert.That(distanceAC, Is.LessThanOrEqualTo(distanceAB + distanceBC + Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhaseSquared_SameColor_ReturnsZero() {
    var metric = new CompuPhaseSquared();
    var distance = metric.Distance(Red, Red);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  #endregion

  #region CIE76 Tests

  [Test]
  [Category("HappyPath")]
  public void CIE76_SameColor_ReturnsZero() {
    var metric = new CIE76();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIE76_BlackToWhite_ReturnsLightnessDifference() {
    var metric = new CIE76();
    var distance = metric.Distance(LabBlack, LabWhite);
    Assert.That(distance, Is.EqualTo(100f).Within(Tolerance)); // L* from 0 to 100
  }

  [Test]
  [Category("HappyPath")]
  public void CIE76_IsSymmetric() {
    var metric = new CIE76();
    var distance1 = metric.Distance(LabRed, LabGreen);
    var distance2 = metric.Distance(LabGreen, LabRed);
    Assert.That(distance1, Is.EqualTo(distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIE76_GrayColors_OnlyLightnessDiffers() {
    var metric = new CIE76();
    var lab1 = new LabF(30f, 0f, 0f);
    var lab2 = new LabF(80f, 0f, 0f);
    var distance = metric.Distance(lab1, lab2);
    Assert.That(distance, Is.EqualTo(50f).Within(Tolerance));
  }

  #endregion

  #region CIEDE2000 Tests

  [Test]
  [Category("HappyPath")]
  public void CIEDE2000_SameColor_ReturnsZero() {
    var metric = new CIEDE2000();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIEDE2000_IsSymmetric() {
    var metric = new CIEDE2000();
    var distance1 = metric.Distance(LabRed, LabGreen);
    var distance2 = metric.Distance(LabGreen, LabRed);
    Assert.That(distance1, Is.EqualTo(distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIEDE2000_BlackToWhite_ReturnsPositive() {
    var metric = new CIEDE2000();
    var distance = metric.Distance(LabBlack, LabWhite);
    Assert.That(distance, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void CIEDE2000_GrayColors_ReturnsReasonableDistance() {
    var metric = new CIEDE2000();
    var lab1 = new LabF(30f, 0f, 0f);
    var lab2 = new LabF(80f, 0f, 0f);
    var distance = metric.Distance(lab1, lab2);
    Assert.That(distance, Is.GreaterThan(0f));
    Assert.That(distance, Is.LessThan(100f)); // Should be less than max L* difference due to weighting
  }

  [Test]
  [Category("HappyPath")]
  public void CIEDE2000Squared_SameColor_ReturnsZero() {
    var metric = new CIEDE2000Squared();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  #endregion

  #region CIE94 Tests

  [Test]
  [Category("HappyPath")]
  public void CIE94_SameColor_ReturnsZero() {
    var metric = new CIE94();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIE94_DifferentColors_ReturnsPositive() {
    var metric = new CIE94();
    var distance = metric.Distance(LabRed, LabGreen);
    Assert.That(distance, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void CIE94_BlackToWhite_ReturnsPositive() {
    var metric = new CIE94();
    var distance = metric.Distance(LabBlack, LabWhite);
    Assert.That(distance, Is.GreaterThan(0f));
  }

  #endregion

  #region CMC Tests

  [Test]
  [Category("HappyPath")]
  public void CMC_SameColor_ReturnsZero() {
    var metric = new CMC();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CMC_DifferentColors_ReturnsPositive() {
    var metric = new CMC();
    var distance = metric.Distance(LabRed, LabGreen);
    Assert.That(distance, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void CMC_BlackToWhite_ReturnsPositive() {
    var metric = new CMC();
    var distance = metric.Distance(LabBlack, LabWhite);
    Assert.That(distance, Is.GreaterThan(0f));
  }

  #endregion

  #region DIN99 Distance Tests

  [Test]
  [Category("HappyPath")]
  public void DIN99Distance_SameColor_ReturnsZero() {
    var metric = new DIN99Distance();
    var distance = metric.Distance(Din99Red, Din99Red);
    Assert.That(distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void DIN99Distance_IsSymmetric() {
    var metric = new DIN99Distance();
    var distance1 = metric.Distance(Din99Red, Din99Green);
    var distance2 = metric.Distance(Din99Green, Din99Red);
    Assert.That(distance1, Is.EqualTo(distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void DIN99Distance_BlackToWhite_ReturnsPositive() {
    var metric = new DIN99Distance();
    var distance = metric.Distance(Din99Black, Din99White);
    Assert.That(distance, Is.GreaterThan(0f));
  }

  #endregion

  #region Comparison Tests

  [Test]
  [Category("HappyPath")]
  public void AllMetrics_SameColor_ReturnZero() {
    var color = MixedColor1;

    Assert.That(new EuclideanRgb().Distance(color, color), Is.EqualTo(0f).Within(Tolerance));
    Assert.That(new Euclidean3<LinearRgbF>().Distance(color, color), Is.EqualTo(0f).Within(Tolerance));
    Assert.That(new Manhattan3<LinearRgbF>().Distance(color, color), Is.EqualTo(0f).Within(Tolerance));
    Assert.That(new Chebyshev3<LinearRgbF>().Distance(color, color), Is.EqualTo(0f).Within(Tolerance));
    Assert.That(new CompuPhase().Distance(color, color), Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void AllLabMetrics_SameColor_ReturnZero() {
    var labColor = LabGray;
    var din99Color = Din99Gray;

    Assert.That(new CIE76().Distance(labColor, labColor), Is.EqualTo(0f).Within(Tolerance));
    Assert.That(new CIE94().Distance(labColor, labColor), Is.EqualTo(0f).Within(Tolerance));
    Assert.That(new CIEDE2000().Distance(labColor, labColor), Is.EqualTo(0f).Within(Tolerance));
    Assert.That(new CMC().Distance(labColor, labColor), Is.EqualTo(0f).Within(Tolerance));
    Assert.That(new DIN99Distance().Distance(din99Color, din99Color), Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MetricOrdering_CloserColorsHaveSmallerDistance() {
    var reference = Gray;
    var closer = new LinearRgbF(0.45f, 0.45f, 0.45f);
    var farther = new LinearRgbF(0.1f, 0.9f, 0.2f);

    var euclidean = new EuclideanRgb();
    var compuPhase = new CompuPhase();
    var manhattan = new Manhattan3<LinearRgbF>();
    var chebyshev = new Chebyshev3<LinearRgbF>();

    Assert.That(euclidean.Distance(reference, closer), Is.LessThan(euclidean.Distance(reference, farther)));
    Assert.That(compuPhase.Distance(reference, closer), Is.LessThan(compuPhase.Distance(reference, farther)));
    Assert.That(manhattan.Distance(reference, closer), Is.LessThan(manhattan.Distance(reference, farther)));
    Assert.That(chebyshev.Distance(reference, closer), Is.LessThan(chebyshev.Distance(reference, farther)));
  }

  #endregion

}
