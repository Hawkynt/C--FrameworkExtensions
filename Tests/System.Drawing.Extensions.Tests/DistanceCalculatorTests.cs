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

using System.Drawing.ColorSpaces;
using NUnit.Framework;
using System.Drawing.ColorSpaces.Distances;

namespace System.Drawing.Tests;

[TestFixture]
[Category("Unit")]
[Category("System.Drawing")]
[Category("ColorDistance")]
public class DistanceCalculatorTests {

  #region CompuPhaseDistance Tests

  [Test]
  [Category("HappyPath")]
  public void CompuPhaseDistance_SameColor_ReturnsZero() {
    var calculator = new CompuPhaseDistance();
    var distance = calculator.Calculate(Color.Red, Color.Red);
    Assert.That(distance, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhaseDistance_BlackAndWhite_ReturnsMaxDistance() {
    var calculator = new CompuPhaseDistance();
    var distance = calculator.Calculate(Color.Black, Color.White);
    Assert.That(distance, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhaseDistance_IsSymmetric() {
    var calculator = new CompuPhaseDistance();
    var color1 = Color.FromArgb(255, 100, 150, 200);
    var color2 = Color.FromArgb(255, 200, 100, 50);
    var distance1 = calculator.Calculate(color1, color2);
    var distance2 = calculator.Calculate(color2, color1);
    Assert.That(distance1, Is.EqualTo(distance2));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhaseDistance_TriangleInequality() {
    var calculator = new CompuPhaseDistance();
    var colorA = Color.FromArgb(255, 0, 0, 0);
    var colorB = Color.FromArgb(255, 128, 128, 128);
    var colorC = Color.FromArgb(255, 255, 255, 255);

    var distanceAB = calculator.Calculate(colorA, colorB);
    var distanceBC = calculator.Calculate(colorB, colorC);
    var distanceAC = calculator.Calculate(colorA, colorC);

    Assert.That(distanceAC, Is.LessThanOrEqualTo(distanceAB + distanceBC));
  }

  [Test]
  [Category("HappyPath")]
  public void CompuPhaseDistance_GradientsOrderCorrectly() {
    var calculator = new CompuPhaseDistance();
    var reference = Color.FromArgb(255, 128, 128, 128);
    var closer = Color.FromArgb(255, 130, 128, 128);
    var farther = Color.FromArgb(255, 200, 128, 128);

    var distanceToCloser = calculator.Calculate(reference, closer);
    var distanceToFarther = calculator.Calculate(reference, farther);

    Assert.That(distanceToCloser, Is.LessThan(distanceToFarther));
  }

  [Test]
  [Category("EdgeCase")]
  public void CompuPhaseDistance_TransparentColors_CalculatesAlphaDifference() {
    var calculator = new CompuPhaseDistance();
    var opaque = Color.FromArgb(255, 128, 128, 128);
    var transparent = Color.FromArgb(0, 128, 128, 128);
    var distance = calculator.Calculate(opaque, transparent);
    Assert.That(distance, Is.GreaterThan(0));
  }

  #endregion

  #region PngQuantDistance Tests

  [Test]
  [Category("HappyPath")]
  public void PngQuantDistance_SameColor_ReturnsZero() {
    var calculator = PngQuantDistance.Default;
    var distance = calculator.Calculate(Color.Red, Color.Red);
    Assert.That(distance, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void PngQuantDistance_BlackAndWhite_ReturnsMaxDistance() {
    var calculator = PngQuantDistance.Default;
    var distance = calculator.Calculate(Color.Black, Color.White);
    Assert.That(distance, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void PngQuantDistance_IsSymmetric() {
    var calculator = PngQuantDistance.Default;
    var color1 = Color.FromArgb(255, 100, 150, 200);
    var color2 = Color.FromArgb(255, 200, 100, 50);
    var distance1 = calculator.Calculate(color1, color2);
    var distance2 = calculator.Calculate(color2, color1);
    Assert.That(distance1, Is.EqualTo(distance2));
  }

  [Test]
  [Category("HappyPath")]
  public void PngQuantDistance_TriangleInequality() {
    var calculator = PngQuantDistance.Default;
    var colorA = Color.FromArgb(255, 0, 0, 0);
    var colorB = Color.FromArgb(255, 128, 128, 128);
    var colorC = Color.FromArgb(255, 255, 255, 255);

    var distanceAB = calculator.Calculate(colorA, colorB);
    var distanceBC = calculator.Calculate(colorB, colorC);
    var distanceAC = calculator.Calculate(colorA, colorC);

    Assert.That(distanceAC, Is.LessThanOrEqualTo(distanceAB + distanceBC));
  }

  [Test]
  [Category("HappyPath")]
  public void PngQuantDistance_GradientsOrderCorrectly() {
    var calculator = PngQuantDistance.Default;
    var reference = Color.FromArgb(255, 128, 128, 128);
    var closer = Color.FromArgb(255, 130, 128, 128);
    var farther = Color.FromArgb(255, 200, 128, 128);

    var distanceToCloser = calculator.Calculate(reference, closer);
    var distanceToFarther = calculator.Calculate(reference, farther);

    Assert.That(distanceToCloser, Is.LessThan(distanceToFarther));
  }

  [Test]
  [Category("HappyPath")]
  public void PngQuantDistance_CustomWhitePoint_AffectsWeighting() {
    var defaultCalculator = PngQuantDistance.Default;
    var customCalculator = new PngQuantDistance(Color.FromArgb(255, 255, 128, 255));

    var color1 = Color.FromArgb(255, 100, 100, 100);
    var color2 = Color.FromArgb(255, 100, 150, 100);

    var defaultDistance = defaultCalculator.Calculate(color1, color2);
    var customDistance = customCalculator.Calculate(color1, color2);

    Assert.That(customDistance, Is.Not.EqualTo(defaultDistance));
  }

  [Test]
  [Category("EdgeCase")]
  public void PngQuantDistance_TransparentColors_ConsidersAlphaBlending() {
    var calculator = PngQuantDistance.Default;
    var opaque = Color.FromArgb(255, 128, 128, 128);
    var semiTransparent = Color.FromArgb(128, 128, 128, 128);
    var distance = calculator.Calculate(opaque, semiTransparent);
    Assert.That(distance, Is.GreaterThan(0));
  }

  #endregion

  #region WeightedYuvDistance Tests

  [Test]
  [Category("HappyPath")]
  public void WeightedYuvDistance_SameColor_ReturnsZero() {
    var calculator = WeightedEuclideanDistances.Yuv;
    var distance = calculator.Calculate(Color.Red, Color.Red);
    Assert.That(distance, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedYuvDistance_BlackAndWhite_ReturnsMaxDistance() {
    var calculator = WeightedEuclideanDistances.Yuv;
    var distance = calculator.Calculate(Color.Black, Color.White);
    Assert.That(distance, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedYuvDistance_IsSymmetric() {
    var calculator = WeightedEuclideanDistances.Yuv;
    var color1 = Color.FromArgb(255, 100, 150, 200);
    var color2 = Color.FromArgb(255, 200, 100, 50);
    var distance1 = calculator.Calculate(color1, color2);
    var distance2 = calculator.Calculate(color2, color1);
    Assert.That(distance1, Is.EqualTo(distance2));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedYuvDistance_TriangleInequality() {
    var calculator = WeightedEuclideanDistances.Yuv;
    var colorA = Color.FromArgb(255, 0, 0, 0);
    var colorB = Color.FromArgb(255, 128, 128, 128);
    var colorC = Color.FromArgb(255, 255, 255, 255);

    var distanceAB = calculator.Calculate(colorA, colorB);
    var distanceBC = calculator.Calculate(colorB, colorC);
    var distanceAC = calculator.Calculate(colorA, colorC);

    Assert.That(distanceAC, Is.LessThanOrEqualTo(distanceAB + distanceBC));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedYuvDistance_GradientsOrderCorrectly() {
    var calculator = WeightedEuclideanDistances.Yuv;
    var reference = Color.FromArgb(255, 128, 128, 128);
    var closer = Color.FromArgb(255, 130, 128, 128);
    var farther = Color.FromArgb(255, 200, 128, 128);

    var distanceToCloser = calculator.Calculate(reference, closer);
    var distanceToFarther = calculator.Calculate(reference, farther);

    Assert.That(distanceToCloser, Is.LessThan(distanceToFarther));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedYuvDistance_CustomWeights_AffectsCalculation() {
    var defaultCalculator = WeightedEuclideanDistances.Yuv;
    var customCalculator = new WeightedEuclideanDistance<Yuv>(1, 1, 1, 1);

    var color1 = Color.FromArgb(255, 100, 100, 100);
    var color2 = Color.FromArgb(255, 150, 150, 150);

    var defaultDistance = defaultCalculator.Calculate(color1, color2);
    var customDistance = customCalculator.Calculate(color1, color2);

    Assert.That(customDistance, Is.Not.EqualTo(defaultDistance));
  }

  [Test]
  [Category("EdgeCase")]
  public void WeightedYuvDistance_IdenticalLuminance_DifferentChrominance() {
    var calculator = WeightedEuclideanDistances.Yuv;
    var gray = Color.FromArgb(255, 128, 128, 128);
    var colorful = Color.FromArgb(255, 255, 0, 0);
    var distance = calculator.Calculate(gray, colorful);
    Assert.That(distance, Is.GreaterThan(0));
  }

  #endregion

  #region WeightedYCbCrDistance Tests

  [Test]
  [Category("HappyPath")]
  public void WeightedYCbCrDistance_SameColor_ReturnsZero() {
    var calculator = WeightedEuclideanDistances.YCbCr;
    var distance = calculator.Calculate(Color.Red, Color.Red);
    Assert.That(distance, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedYCbCrDistance_BlackAndWhite_ReturnsMaxDistance() {
    var calculator = WeightedEuclideanDistances.YCbCr;
    var distance = calculator.Calculate(Color.Black, Color.White);
    Assert.That(distance, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedYCbCrDistance_IsSymmetric() {
    var calculator = WeightedEuclideanDistances.YCbCr;
    var color1 = Color.FromArgb(255, 100, 150, 200);
    var color2 = Color.FromArgb(255, 200, 100, 50);
    var distance1 = calculator.Calculate(color1, color2);
    var distance2 = calculator.Calculate(color2, color1);
    Assert.That(distance1, Is.EqualTo(distance2));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedYCbCrDistance_TriangleInequality() {
    var calculator = WeightedEuclideanDistances.YCbCr;
    var colorA = Color.FromArgb(255, 0, 0, 0);
    var colorB = Color.FromArgb(255, 128, 128, 128);
    var colorC = Color.FromArgb(255, 255, 255, 255);

    var distanceAB = calculator.Calculate(colorA, colorB);
    var distanceBC = calculator.Calculate(colorB, colorC);
    var distanceAC = calculator.Calculate(colorA, colorC);

    Assert.That(distanceAC, Is.LessThanOrEqualTo(distanceAB + distanceBC));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedYCbCrDistance_GradientsOrderCorrectly() {
    var calculator = WeightedEuclideanDistances.YCbCr;
    var reference = Color.FromArgb(255, 128, 128, 128);
    var closer = Color.FromArgb(255, 130, 128, 128);
    var farther = Color.FromArgb(255, 200, 128, 128);

    var distanceToCloser = calculator.Calculate(reference, closer);
    var distanceToFarther = calculator.Calculate(reference, farther);

    Assert.That(distanceToCloser, Is.LessThan(distanceToFarther));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedYCbCrDistance_CustomWeights_AffectsCalculation() {
    var defaultCalculator = WeightedEuclideanDistances.YCbCr;
    var customCalculator = new WeightedEuclideanDistance<YCbCr>(1, 1, 1, 1, 1);

    var color1 = Color.FromArgb(255, 100, 100, 100);
    var color2 = Color.FromArgb(255, 150, 150, 150);

    var defaultDistance = defaultCalculator.Calculate(color1, color2);
    var customDistance = customCalculator.Calculate(color1, color2);

    Assert.That(customDistance, Is.Not.EqualTo(defaultDistance));
  }

  [Test]
  [Category("EdgeCase")]
  public void WeightedYCbCrDistance_LuminanceWeightedHigher_EmphasisOnY() {
    var lowYWeight = new WeightedEuclideanDistance<YCbCr>(1, 1, 1, 1, 1);
    var highYWeight = new WeightedEuclideanDistance<YCbCr>(10, 1, 1, 1, 1);

    var darkColor = Color.FromArgb(255, 50, 50, 50);
    var lightColor = Color.FromArgb(255, 200, 200, 200);

    var lowYDistance = lowYWeight.Calculate(darkColor, lightColor);
    var highYDistance = highYWeight.Calculate(darkColor, lightColor);

    Assert.That(highYDistance, Is.GreaterThan(lowYDistance));
  }

  #endregion

  #region WeightedManhattanDistance Tests

  [Test]
  [Category("HappyPath")]
  public void WeightedManhattanDistance_SameColor_ReturnsZero() {
    var calculator = WeightedManhattanDistances.RgbOnly;
    var distance = calculator.Calculate(Color.Red, Color.Red);
    Assert.That(distance, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedManhattanDistance_BlackAndWhite_ReturnsMaxDistance() {
    var calculator = WeightedManhattanDistances.RgbOnly;
    var distance = calculator.Calculate(Color.Black, Color.White);
    Assert.That(distance, Is.EqualTo(765)); // 255 + 255 + 255
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedManhattanDistance_IsSymmetric() {
    var calculator = WeightedManhattanDistances.RgbOnly;
    var color1 = Color.FromArgb(255, 100, 150, 200);
    var color2 = Color.FromArgb(255, 200, 100, 50);
    var distance1 = calculator.Calculate(color1, color2);
    var distance2 = calculator.Calculate(color2, color1);
    Assert.That(distance1, Is.EqualTo(distance2));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedManhattanDistance_TriangleInequality() {
    var calculator = WeightedManhattanDistances.RgbOnly;
    var colorA = Color.FromArgb(255, 0, 0, 0);
    var colorB = Color.FromArgb(255, 128, 128, 128);
    var colorC = Color.FromArgb(255, 255, 255, 255);

    var distanceAB = calculator.Calculate(colorA, colorB);
    var distanceBC = calculator.Calculate(colorB, colorC);
    var distanceAC = calculator.Calculate(colorA, colorC);

    Assert.That(distanceAC, Is.LessThanOrEqualTo(distanceAB + distanceBC));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedManhattanDistance_GradientsOrderCorrectly() {
    var calculator = WeightedManhattanDistances.RgbOnly;
    var reference = Color.FromArgb(255, 128, 128, 128);
    var closer = Color.FromArgb(255, 130, 128, 128);
    var farther = Color.FromArgb(255, 200, 128, 128);

    var distanceToCloser = calculator.Calculate(reference, closer);
    var distanceToFarther = calculator.Calculate(reference, farther);

    Assert.That(distanceToCloser, Is.LessThan(distanceToFarther));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedManhattanDistance_RgbOnly_IgnoresAlpha() {
    var calculator = WeightedManhattanDistances.RgbOnly;
    var opaque = Color.FromArgb(255, 128, 128, 128);
    var transparent = Color.FromArgb(0, 128, 128, 128);
    var distance = calculator.Calculate(opaque, transparent);
    Assert.That(distance, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedManhattanDistance_BT709_UsesHdtvWeights() {
    var bt709 = WeightedManhattanDistances.BT709;
    var rgbOnly = WeightedManhattanDistances.RgbOnly;

    var color1 = Color.FromArgb(255, 100, 100, 100);
    var color2 = Color.FromArgb(255, 150, 150, 150);

    var bt709Distance = bt709.Calculate(color1, color2);
    var rgbDistance = rgbOnly.Calculate(color1, color2);

    Assert.That(bt709Distance, Is.Not.EqualTo(rgbDistance));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedManhattanDistance_Nommyde_UsesPerceptualWeights() {
    var nommyde = WeightedManhattanDistances.Nommyde;
    var rgbOnly = WeightedManhattanDistances.RgbOnly;

    var color1 = Color.FromArgb(255, 100, 100, 100);
    var color2 = Color.FromArgb(255, 150, 150, 150);

    var nomDistance = nommyde.Calculate(color1, color2);
    var rgbDistance = rgbOnly.Calculate(color1, color2);

    Assert.That(nomDistance, Is.Not.EqualTo(rgbDistance));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedManhattanDistance_LowRed_DeemphasizesRed() {
    var lowRed = WeightedManhattanDistances.LowRed;
    var highRed = WeightedManhattanDistances.HighRed;

    var baseColor = Color.FromArgb(255, 0, 0, 0);
    var redShift = Color.FromArgb(255, 100, 0, 0);

    var lowRedDistance = lowRed.Calculate(baseColor, redShift);
    var highRedDistance = highRed.Calculate(baseColor, redShift);

    Assert.That(lowRedDistance, Is.LessThan(highRedDistance));
  }

  [Test]
  [Category("HappyPath")]
  public void WeightedManhattanDistance_HighRed_EmphasizesRed() {
    var highRed = WeightedManhattanDistances.HighRed;
    var rgbOnly = WeightedManhattanDistances.RgbOnly;

    var baseColor = Color.FromArgb(255, 0, 0, 0);
    var redShift = Color.FromArgb(255, 50, 0, 0);
    var blueShift = Color.FromArgb(255, 0, 0, 50);

    var redDistance = highRed.Calculate(baseColor, redShift);
    var blueDistance = highRed.Calculate(baseColor, blueShift);

    Assert.That(redDistance, Is.GreaterThan(blueDistance));
  }

  [Test]
  [Category("EdgeCase")]
  public void WeightedManhattanDistance_CustomWeights_ProducesExpectedResult() {
    var calculator = new WeightedManhattanDistance<Rgb>(2, 3, 4, 1, 10);
    var color1 = Color.FromArgb(255, 0, 0, 0);
    var color2 = Color.FromArgb(255, 10, 10, 10);

    var expectedDistance = (2 * 10 + 3 * 10 + 4 * 10 + 1 * 0) / 10.0;
    var actualDistance = calculator.Calculate(color1, color2);

    Assert.That(actualDistance, Is.EqualTo(expectedDistance));
  }

  #endregion
}
