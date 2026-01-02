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
    var metric = new Euclidean3F<LinearRgbF>();
    var distance = metric.Distance(Red, Red);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void EuclideanRgb_BlackToWhite_ReturnsMaxDistance() {
    var metric = new Euclidean3F<LinearRgbF>();
    var distance = metric.Distance(Black, White);
    // Normalized: sqrt(3)/sqrt(3) = 1.0
    Assert.That((float)distance, Is.EqualTo(1f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void EuclideanRgb_IsSymmetric() {
    var metric = new Euclidean3F<LinearRgbF>();
    var distance1 = metric.Distance(MixedColor1, MixedColor2);
    var distance2 = metric.Distance(MixedColor2, MixedColor1);
    Assert.That((float)distance1, Is.EqualTo((float)distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void EuclideanRgb_TriangleInequality() {
    var metric = new Euclidean3F<LinearRgbF>();
    var distanceAB = metric.Distance(Black, Gray);
    var distanceBC = metric.Distance(Gray, White);
    var distanceAC = metric.Distance(Black, White);
    Assert.That((float)distanceAC, Is.LessThanOrEqualTo((float)distanceAB + (float)distanceBC + Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void EuclideanRgb_PrimaryColors_KnownDistance() {
    var metric = new Euclidean3F<LinearRgbF>();
    var distance = metric.Distance(Red, Green);
    // Normalized: sqrt(2)/sqrt(3) ≈ 0.8165
    var expected = (float)Math.Sqrt(2f / 3f);
    Assert.That((float)distance, Is.EqualTo(expected).Within(Tolerance));
  }

  #endregion

  #region Euclidean3 Generic Tests

  [Test]
  [Category("HappyPath")]
  public void Euclidean3_LinearRgbF_SameColor_ReturnsZero() {
    var metric = new Euclidean3F<LinearRgbF>();
    var distance = metric.Distance(Red, Red);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Euclidean3_LinearRgbF_IsSymmetric() {
    var metric = new Euclidean3F<LinearRgbF>();
    var distance1 = metric.Distance(MixedColor1, MixedColor2);
    var distance2 = metric.Distance(MixedColor2, MixedColor1);
    Assert.That((float)distance1, Is.EqualTo((float)distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Euclidean3_LabF_SameColor_ReturnsZero() {
    var metric = new Euclidean3F<LabF>();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Euclidean3_LabF_IsSymmetric() {
    var metric = new Euclidean3F<LabF>();
    var distance1 = metric.Distance(LabRed, LabGreen);
    var distance2 = metric.Distance(LabGreen, LabRed);
    Assert.That((float)distance1, Is.EqualTo((float)distance2).Within(Tolerance));
  }

  #endregion

  #region Manhattan3 Tests

  [Test]
  [Category("HappyPath")]
  public void Manhattan3_LinearRgbF_SameColor_ReturnsZero() {
    var metric = new Manhattan3F<LinearRgbF>();
    var distance = metric.Distance(Red, Red);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Manhattan3_LinearRgbF_BlackToWhite_ReturnsSum() {
    var metric = new Manhattan3F<LinearRgbF>();
    var distance = metric.Distance(Black, White);
    // Normalized: 3/3 = 1.0
    Assert.That((float)distance, Is.EqualTo(1f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Manhattan3_LinearRgbF_IsSymmetric() {
    var metric = new Manhattan3F<LinearRgbF>();
    var distance1 = metric.Distance(MixedColor1, MixedColor2);
    var distance2 = metric.Distance(MixedColor2, MixedColor1);
    Assert.That((float)distance1, Is.EqualTo((float)distance2).Within(Tolerance));
  }

  #endregion

  #region Chebyshev3 Tests

  [Test]
  [Category("HappyPath")]
  public void Chebyshev3_LinearRgbF_SameColor_ReturnsZero() {
    var metric = new Chebyshev3F<LinearRgbF>();
    var distance = metric.Distance(Red, Red);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Chebyshev3_LinearRgbF_BlackToWhite_ReturnsMax() {
    var metric = new Chebyshev3F<LinearRgbF>();
    var distance = metric.Distance(Black, White);
    // Chebyshev max for [0,1] range is already 1.0
    Assert.That((float)distance, Is.EqualTo(1f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Chebyshev3_LinearRgbF_IsSymmetric() {
    var metric = new Chebyshev3F<LinearRgbF>();
    var distance1 = metric.Distance(MixedColor1, MixedColor2);
    var distance2 = metric.Distance(MixedColor2, MixedColor1);
    Assert.That((float)distance1, Is.EqualTo((float)distance2).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Chebyshev3_LinearRgbF_ReturnsMaxDifference() {
    var metric = new Chebyshev3F<LinearRgbF>();
    var a = new LinearRgbF(0.5f, 0.2f, 0.8f);
    var b = new LinearRgbF(0.3f, 0.9f, 0.6f);
    var distance = metric.Distance(a, b);
    // Max of |0.5-0.3|=0.2, |0.2-0.9|=0.7, |0.8-0.6|=0.2 is 0.7
    Assert.That((float)distance, Is.EqualTo(0.7f).Within(Tolerance));
  }

  #endregion

  #region CompuPhase Tests

  [Test]
  [Category("HappyPath")]
  public void CompuPhase_SameColor_ReturnsZero() {
    var metric = new CompuPhase();
    var distance = metric.Distance(Red, Red);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhase_BlackToWhite_ReturnsPositive() {
    var metric = new CompuPhase();
    var distance = metric.Distance(Black, White);
    Assert.That((float)distance, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhase_IsSymmetric() {
    var metric = new CompuPhase();
    var distance1 = metric.Distance(MixedColor1, MixedColor2);
    var distance2 = metric.Distance(MixedColor2, MixedColor1);
    Assert.That((float)distance1, Is.EqualTo((float)distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhase_TriangleInequality() {
    var metric = new CompuPhase();
    var distanceAB = metric.Distance(Black, Gray);
    var distanceBC = metric.Distance(Gray, White);
    var distanceAC = metric.Distance(Black, White);
    Assert.That((float)distanceAC, Is.LessThanOrEqualTo((float)distanceAB + (float)distanceBC + Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhaseSquared_SameColor_ReturnsZero() {
    var metric = new CompuPhaseSquared();
    var distance = metric.Distance(Red, Red);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  #endregion

  #region CIE76 Tests

  [Test]
  [Category("HappyPath")]
  public void CIE76_SameColor_ReturnsZero() {
    var metric = new CIE76();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIE76_BlackToWhite_ReturnsLightnessDifference() {
    var metric = new CIE76();
    var distance = metric.Distance(LabBlack, LabWhite);
    // Normalized: 100/100 = 1.0
    Assert.That((float)distance, Is.EqualTo(1f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIE76_IsSymmetric() {
    var metric = new CIE76();
    var distance1 = metric.Distance(LabRed, LabGreen);
    var distance2 = metric.Distance(LabGreen, LabRed);
    Assert.That((float)distance1, Is.EqualTo((float)distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIE76_GrayColors_OnlyLightnessDiffers() {
    var metric = new CIE76();
    var lab1 = new LabF(30f, 0f, 0f);
    var lab2 = new LabF(80f, 0f, 0f);
    var distance = metric.Distance(lab1, lab2);
    // Normalized: 50/100 = 0.5
    Assert.That((float)distance, Is.EqualTo(0.5f).Within(Tolerance));
  }

  #endregion

  #region CIEDE2000 Tests

  [Test]
  [Category("HappyPath")]
  public void CIEDE2000_SameColor_ReturnsZero() {
    var metric = new CIEDE2000();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIEDE2000_IsSymmetric() {
    var metric = new CIEDE2000();
    var distance1 = metric.Distance(LabRed, LabGreen);
    var distance2 = metric.Distance(LabGreen, LabRed);
    Assert.That((float)distance1, Is.EqualTo((float)distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIEDE2000_BlackToWhite_ReturnsPositive() {
    var metric = new CIEDE2000();
    var distance = metric.Distance(LabBlack, LabWhite);
    Assert.That((float)distance, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void CIEDE2000_GrayColors_ReturnsReasonableDistance() {
    var metric = new CIEDE2000();
    var lab1 = new LabF(30f, 0f, 0f);
    var lab2 = new LabF(80f, 0f, 0f);
    var distance = metric.Distance(lab1, lab2);
    Assert.That((float)distance, Is.GreaterThan(0f));
    // Normalized to [0,1]: distance < 1.0
    Assert.That((float)distance, Is.LessThan(1f));
  }

  [Test]
  [Category("HappyPath")]
  public void CIEDE2000Squared_SameColor_ReturnsZero() {
    var metric = new CIEDE2000Squared();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  #endregion

  #region CIE94 Tests

  [Test]
  [Category("HappyPath")]
  public void CIE94_SameColor_ReturnsZero() {
    var metric = new CIE94();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CIE94_DifferentColors_ReturnsPositive() {
    var metric = new CIE94();
    var distance = metric.Distance(LabRed, LabGreen);
    Assert.That((float)distance, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void CIE94_BlackToWhite_ReturnsPositive() {
    var metric = new CIE94();
    var distance = metric.Distance(LabBlack, LabWhite);
    Assert.That((float)distance, Is.GreaterThan(0f));
  }

  #endregion

  #region CMC Tests

  [Test]
  [Category("HappyPath")]
  public void CMC_SameColor_ReturnsZero() {
    var metric = new CMC();
    var distance = metric.Distance(LabRed, LabRed);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CMC_DifferentColors_ReturnsPositive() {
    var metric = new CMC();
    var distance = metric.Distance(LabRed, LabGreen);
    Assert.That((float)distance, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void CMC_BlackToWhite_ReturnsPositive() {
    var metric = new CMC();
    var distance = metric.Distance(LabBlack, LabWhite);
    Assert.That((float)distance, Is.GreaterThan(0f));
  }

  #endregion

  #region DIN99 Distance Tests

  [Test]
  [Category("HappyPath")]
  public void DIN99Distance_SameColor_ReturnsZero() {
    var metric = new DIN99Distance();
    var distance = metric.Distance(Din99Red, Din99Red);
    Assert.That((float)distance, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void DIN99Distance_IsSymmetric() {
    var metric = new DIN99Distance();
    var distance1 = metric.Distance(Din99Red, Din99Green);
    var distance2 = metric.Distance(Din99Green, Din99Red);
    Assert.That((float)distance1, Is.EqualTo((float)distance2).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void DIN99Distance_BlackToWhite_ReturnsPositive() {
    var metric = new DIN99Distance();
    var distance = metric.Distance(Din99Black, Din99White);
    Assert.That((float)distance, Is.GreaterThan(0f));
  }

  #endregion

  #region Comparison Tests

  [Test]
  [Category("HappyPath")]
  public void AllMetrics_SameColor_ReturnZero() {
    var color = MixedColor1;

    Assert.That((float)new Euclidean3F<LinearRgbF>().Distance(color, color), Is.EqualTo(0f).Within(Tolerance));
    Assert.That((float)new Euclidean3F<LinearRgbF>().Distance(color, color), Is.EqualTo(0f).Within(Tolerance));
    Assert.That((float)new Manhattan3F<LinearRgbF>().Distance(color, color), Is.EqualTo(0f).Within(Tolerance));
    Assert.That((float)new Chebyshev3F<LinearRgbF>().Distance(color, color), Is.EqualTo(0f).Within(Tolerance));
    Assert.That((float)new CompuPhase().Distance(color, color), Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void AllLabMetrics_SameColor_ReturnZero() {
    var labColor = LabGray;
    var din99Color = Din99Gray;

    Assert.That((float)new CIE76().Distance(labColor, labColor), Is.EqualTo(0f).Within(Tolerance));
    Assert.That((float)new CIE94().Distance(labColor, labColor), Is.EqualTo(0f).Within(Tolerance));
    Assert.That((float)new CIEDE2000().Distance(labColor, labColor), Is.EqualTo(0f).Within(Tolerance));
    Assert.That((float)new CMC().Distance(labColor, labColor), Is.EqualTo(0f).Within(Tolerance));
    Assert.That((float)new DIN99Distance().Distance(din99Color, din99Color), Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MetricOrdering_CloserColorsHaveSmallerDistance() {
    var reference = Gray;
    var closer = new LinearRgbF(0.45f, 0.45f, 0.45f);
    var farther = new LinearRgbF(0.1f, 0.9f, 0.2f);

    var euclidean = new Euclidean3F<LinearRgbF>();
    var compuPhase = new CompuPhase();
    var manhattan = new Manhattan3F<LinearRgbF>();
    var chebyshev = new Chebyshev3F<LinearRgbF>();

    Assert.That((float)euclidean.Distance(reference, closer), Is.LessThan((float)euclidean.Distance(reference, farther)));
    Assert.That((float)compuPhase.Distance(reference, closer), Is.LessThan((float)compuPhase.Distance(reference, farther)));
    Assert.That((float)manhattan.Distance(reference, closer), Is.LessThan((float)manhattan.Distance(reference, farther)));
    Assert.That((float)chebyshev.Distance(reference, closer), Is.LessThan((float)chebyshev.Distance(reference, farther)));
  }

  #endregion

}
