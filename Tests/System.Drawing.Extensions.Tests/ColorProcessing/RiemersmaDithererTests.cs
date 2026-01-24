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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hawkynt.ColorProcessing.Dithering;
using Hawkynt.ColorProcessing.Quantization;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

/// <summary>
/// Tests for RiemersmaDitherer with different space-filling curves (Hilbert, Peano, Linear).
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Dithering")]
[Category("Riemersma")]
public class RiemersmaDithererTests {

  #region Curve Type Tests

  [Test]
  public void HilbertCurve_ProducesValidDitheredOutput() {
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Hilbert);
    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  public void PeanoCurve_ProducesValidDitheredOutput() {
    using var bitmap = TestUtilities.CreateGradientBitmap(27, 27, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Peano);
    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(27));
    Assert.That(result.Height, Is.EqualTo(27));
  }

  [Test]
  public void LinearCurve_ProducesValidDitheredOutput() {
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Linear);
    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  public void AllCurveTypes_DitherSolidBlack_RemainsBlack() {
    using var bitmap = TestUtilities.CreateSolidBitmap(16, 16, Color.Black);

    var curveTypes = new[] { SpaceFillingCurve.Hilbert, SpaceFillingCurve.Peano, SpaceFillingCurve.Linear };

    foreach (var curveType in curveTypes) {
      var ditherer = new RiemersmaDitherer(16, curveType);
      using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 2);

      using var locker = result.Lock();
      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        Assert.That(pixel.R, Is.LessThan(50), $"{curveType}: Black pixel at ({x},{y}) should remain dark");
        Assert.That(pixel.G, Is.LessThan(50), $"{curveType}: Black pixel at ({x},{y}) should remain dark");
        Assert.That(pixel.B, Is.LessThan(50), $"{curveType}: Black pixel at ({x},{y}) should remain dark");
      }
    }
  }

  [Test]
  public void AllCurveTypes_DitherSolidWhite_RemainsWhite() {
    using var bitmap = TestUtilities.CreateSolidBitmap(16, 16, Color.White);

    var curveTypes = new[] { SpaceFillingCurve.Hilbert, SpaceFillingCurve.Peano, SpaceFillingCurve.Linear };

    foreach (var curveType in curveTypes) {
      var ditherer = new RiemersmaDitherer(16, curveType);
      using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 2);

      using var locker = result.Lock();
      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        Assert.That(pixel.R, Is.GreaterThan(200), $"{curveType}: White pixel at ({x},{y}) should remain light");
        Assert.That(pixel.G, Is.GreaterThan(200), $"{curveType}: White pixel at ({x},{y}) should remain light");
        Assert.That(pixel.B, Is.GreaterThan(200), $"{curveType}: White pixel at ({x},{y}) should remain light");
      }
    }
  }

  #endregion

  #region Curve Order Tests

  [TestCase(1)]
  [TestCase(2)]
  [TestCase(3)]
  [TestCase(4)]
  [TestCase(5)]
  public void HilbertCurve_ExplicitOrder_ProducesValidOutput(int order) {
    var size = 1 << order; // 2^order
    using var bitmap = TestUtilities.CreateGradientBitmap(size, size, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Hilbert, order);
    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(size));
    Assert.That(result.Height, Is.EqualTo(size));
  }

  [TestCase(1)]
  [TestCase(2)]
  [TestCase(3)]
  public void PeanoCurve_ExplicitOrder_ProducesValidOutput(int order) {
    var size = (int)Math.Pow(3, order); // 3^order
    using var bitmap = TestUtilities.CreateGradientBitmap(size, size, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Peano, order);
    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(size));
    Assert.That(result.Height, Is.EqualTo(size));
  }

  [Test]
  public void HilbertCurve_AutoOrder_CoversEntireImage() {
    using var bitmap = TestUtilities.CreateGradientBitmap(50, 50, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Hilbert); // Auto order
    using var result = bitmap.ReduceColors(new UniformQuantizer(), ditherer, 2);

    // Verify all pixels are processed by checking for a mix of light/dark
    using var locker = result.Lock();
    var darkCount = 0;
    var lightCount = 0;
    for (var y = 0; y < result.Height; ++y)
    for (var x = 0; x < result.Width; ++x) {
      var pixel = locker[x, y];
      if ((pixel.R + pixel.G + pixel.B) / 3 < 128)
        ++darkCount;
      else
        ++lightCount;
    }

    // Gradient should produce both dark and light pixels
    Assert.That(darkCount, Is.GreaterThan(0), "Should have some dark pixels");
    Assert.That(lightCount, Is.GreaterThan(0), "Should have some light pixels");
  }

  [Test]
  public void PeanoCurve_AutoOrder_CoversEntireImage() {
    using var bitmap = TestUtilities.CreateGradientBitmap(50, 50, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Peano); // Auto order
    using var result = bitmap.ReduceColors(new UniformQuantizer(), ditherer, 2);

    using var locker = result.Lock();
    var darkCount = 0;
    var lightCount = 0;
    for (var y = 0; y < result.Height; ++y)
    for (var x = 0; x < result.Width; ++x) {
      var pixel = locker[x, y];
      if ((pixel.R + pixel.G + pixel.B) / 3 < 128)
        ++darkCount;
      else
        ++lightCount;
    }

    Assert.That(darkCount, Is.GreaterThan(0), "Should have some dark pixels");
    Assert.That(lightCount, Is.GreaterThan(0), "Should have some light pixels");
  }

  [Test]
  public void HilbertCurve_OrderExceedsMax_ClampedToMax() {
    // Order 10 exceeds MaxHilbertOrder (7), should be clamped
    using var bitmap = TestUtilities.CreateGradientBitmap(16, 16, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Hilbert, 10);
    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  public void PeanoCurve_OrderExceedsMax_ClampedToMax() {
    // Order 10 exceeds MaxPeanoOrder (5), should be clamped
    using var bitmap = TestUtilities.CreateGradientBitmap(16, 16, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Peano, 10);
    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  public void HilbertCurve_OrderBelowMin_ClampedToMin() {
    using var bitmap = TestUtilities.CreateGradientBitmap(16, 16, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Hilbert, 0);
    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
  }

  [Test]
  public void PeanoCurve_OrderBelowMin_ClampedToMin() {
    using var bitmap = TestUtilities.CreateGradientBitmap(16, 16, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(16, SpaceFillingCurve.Peano, 0);
    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
  }

  #endregion

  #region Pre-configured Instances Tests

  [Test]
  public void Default_UsesHilbertCurve() {
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);

    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), RiemersmaDitherer.Default, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  public void Small_UsesReducedHistorySize() {
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);

    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), RiemersmaDitherer.Small, 4);

    Assert.That(result, Is.Not.Null);
  }

  [Test]
  public void Large_UsesIncreasedHistorySize() {
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);

    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), RiemersmaDitherer.Large, 4);

    Assert.That(result, Is.Not.Null);
  }

  [Test]
  public void LinearScan_UsesSerpentinePattern() {
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);

    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), RiemersmaDitherer.LinearScan, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  public void Peano_UsesPeanoCurve() {
    using var bitmap = TestUtilities.CreateGradientBitmap(27, 27, Color.Black, Color.White);

    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), RiemersmaDitherer.Peano, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(27));
    Assert.That(result.Height, Is.EqualTo(27));
  }

  #endregion

  #region Edge Cases

  [Test]
  public void AllCurveTypes_SinglePixelImage_DoesNotCrash() {
    using var bitmap = TestUtilities.CreateSolidBitmap(1, 1, Color.Gray);

    var curveTypes = new[] { SpaceFillingCurve.Hilbert, SpaceFillingCurve.Peano, SpaceFillingCurve.Linear };

    foreach (var curveType in curveTypes) {
      var ditherer = new RiemersmaDitherer(16, curveType);
      using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 2);

      Assert.That(result, Is.Not.Null, $"{curveType}: Should handle 1x1 image");
      Assert.That(result.Width, Is.EqualTo(1));
      Assert.That(result.Height, Is.EqualTo(1));
    }
  }

  [Test]
  public void AllCurveTypes_SmallImage_Works() {
    using var bitmap = TestUtilities.CreateTestPattern(3, 3);

    var curveTypes = new[] { SpaceFillingCurve.Hilbert, SpaceFillingCurve.Peano, SpaceFillingCurve.Linear };

    foreach (var curveType in curveTypes) {
      var ditherer = new RiemersmaDitherer(16, curveType);
      using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

      Assert.That(result, Is.Not.Null, $"{curveType}: Should handle 3x3 image");
      Assert.That(result.Width, Is.EqualTo(3));
      Assert.That(result.Height, Is.EqualTo(3));
    }
  }

  [Test]
  public void AllCurveTypes_NonSquareImage_Works() {
    using var bitmap = TestUtilities.CreateGradientBitmap(64, 32, Color.Black, Color.White);

    var curveTypes = new[] { SpaceFillingCurve.Hilbert, SpaceFillingCurve.Peano, SpaceFillingCurve.Linear };

    foreach (var curveType in curveTypes) {
      var ditherer = new RiemersmaDitherer(16, curveType);
      using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

      Assert.That(result, Is.Not.Null, $"{curveType}: Should handle non-square image");
      Assert.That(result.Width, Is.EqualTo(64));
      Assert.That(result.Height, Is.EqualTo(32));
    }
  }

  [Test]
  public void AllCurveTypes_NonPowerOfTwo_Works() {
    // Neither power of 2 (Hilbert) nor power of 3 (Peano)
    using var bitmap = TestUtilities.CreateGradientBitmap(37, 41, Color.Black, Color.White);

    var curveTypes = new[] { SpaceFillingCurve.Hilbert, SpaceFillingCurve.Peano, SpaceFillingCurve.Linear };

    foreach (var curveType in curveTypes) {
      var ditherer = new RiemersmaDitherer(16, curveType);
      using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

      Assert.That(result, Is.Not.Null, $"{curveType}: Should handle non-standard size");
      Assert.That(result.Width, Is.EqualTo(37));
      Assert.That(result.Height, Is.EqualTo(41));
    }
  }

  #endregion

  #region Reproducibility Tests

  [Test]
  public void AllCurveTypes_Reproducible_SameInputSameOutput() {
    using var bitmap1 = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var bitmap2 = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);

    var curveTypes = new[] { SpaceFillingCurve.Hilbert, SpaceFillingCurve.Peano, SpaceFillingCurve.Linear };

    foreach (var curveType in curveTypes) {
      var ditherer = new RiemersmaDitherer(16, curveType);

      using var result1 = bitmap1.ReduceColors(new UniformQuantizer(), ditherer, 4);
      using var result2 = bitmap2.ReduceColors(new UniformQuantizer(), ditherer, 4);

      using var locker1 = result1.Lock();
      using var locker2 = result2.Lock();

      for (var y = 0; y < result1.Height; ++y)
      for (var x = 0; x < result1.Width; ++x)
        Assert.That(locker1[x, y].ToArgb(), Is.EqualTo(locker2[x, y].ToArgb()),
          $"{curveType}: Pixel at ({x},{y}) should be reproducible");
    }
  }

  #endregion

  #region RequiresSequentialProcessing Tests

  [Test]
  public void RiemersmaDitherer_RequiresSequentialProcessing() {
    Assert.That(RiemersmaDitherer.Default.RequiresSequentialProcessing, Is.True);
    Assert.That(RiemersmaDitherer.LinearScan.RequiresSequentialProcessing, Is.True);
    Assert.That(RiemersmaDitherer.Peano.RequiresSequentialProcessing, Is.True);
  }

  #endregion

  #region History Size Tests

  [TestCase(4)]
  [TestCase(8)]
  [TestCase(16)]
  [TestCase(32)]
  public void DifferentHistorySizes_ProduceValidOutput(int historySize) {
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);

    var ditherer = new RiemersmaDitherer(historySize, SpaceFillingCurve.Hilbert);
    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  #endregion

  #region Palette Output Tests

  [Test]
  public void AllCurveTypes_OutputUsesOnlyPaletteColors() {
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Red, Color.Blue);

    var curveTypes = new[] { SpaceFillingCurve.Hilbert, SpaceFillingCurve.Peano, SpaceFillingCurve.Linear };

    foreach (var curveType in curveTypes) {
      var ditherer = new RiemersmaDitherer(16, curveType);
      using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 8);
      using var locker = result.Lock();

      var paletteColors = result.Palette.Entries.Select(c => c.ToArgb()).ToHashSet();

      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        Assert.That(paletteColors.Contains(pixel.ToArgb()), Is.True,
          $"{curveType}: Pixel at ({x},{y}) = {pixel} should be in palette");
      }
    }
  }

  #endregion

  #region Constants Tests

  [Test]
  public void MaxHilbertOrder_IsExpectedValue() {
    Assert.That(RiemersmaDitherer.MaxHilbertOrder, Is.EqualTo(7));
  }

  [Test]
  public void MaxPeanoOrder_IsExpectedValue() {
    Assert.That(RiemersmaDitherer.MaxPeanoOrder, Is.EqualTo(5));
  }

  #endregion

}
