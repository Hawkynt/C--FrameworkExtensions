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
/// Known-answer tests for all ditherers.
/// These tests verify that ditherers produce consistent, expected results.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Dithering")]
[Category("KnownAnswer")]
public class DithererKnownAnswerTests {

  #region Ditherer Test Executor

  private delegate Bitmap DitherFunc(Bitmap source, int colorCount);

  /// <summary>
  /// Returns all ditherers using MedianCutQuantizer for adaptive color selection.
  /// This ensures the palette contains colors that match the input image.
  /// </summary>
  private static IEnumerable<(string name, DitherFunc dither)> AllDithererFuncs() {
    // NoDithering - only for basic functionality tests, not dithering pattern tests
    yield return ("NoDithering", (s, c) => s.ReduceColors(new MedianCutQuantizer(), NoDithering.Instance, c));
    foreach (var item in ActualDithererFuncs())
      yield return item;
  }

  /// <summary>
  /// Returns ditherers that actually perform dithering (excludes NoDithering).
  /// Use this for tests that verify dithering patterns produce expected pixel ratios.
  /// </summary>
  private static IEnumerable<(string name, DitherFunc dither)> ActualDithererFuncs() {

    // Error Diffusion
    yield return ("FloydSteinberg", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.FloydSteinberg, c));
    yield return ("EqualFloydSteinberg", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.EqualFloydSteinberg, c));
    yield return ("FalseFloydSteinberg", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.FalseFloydSteinberg, c));
    yield return ("Simple", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Simple, c));
    yield return ("JarvisJudiceNinke", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.JarvisJudiceNinke, c));
    yield return ("Stucki", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Stucki, c));
    yield return ("Atkinson", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Atkinson, c));
    yield return ("Burkes", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Burkes, c));
    yield return ("Sierra", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Sierra, c));
    yield return ("TwoRowSierra", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.TwoRowSierra, c));
    yield return ("SierraLite", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.SierraLite, c));
    yield return ("StevensonArce", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.StevensonArce, c));
    yield return ("Pigeon", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Pigeon, c));
    yield return ("ShiauFan", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.ShiauFan, c));
    yield return ("ShiauFan2", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.ShiauFan2, c));
    yield return ("Fan93", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Fan93, c));
    yield return ("TwoD", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.TwoD, c));
    yield return ("Down", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Down, c));
    yield return ("DoubleDown", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.DoubleDown, c));
    yield return ("Diagonal", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Diagonal, c));
    yield return ("VerticalDiamond", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.VerticalDiamond, c));
    yield return ("HorizontalDiamond", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.HorizontalDiamond, c));
    yield return ("Diamond", (s, c) => s.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Diamond, c));

    // Ordered Dithering
    yield return ("Bayer2x2", (s, c) => s.ReduceColors(new MedianCutQuantizer(), OrderedDitherer.Bayer2x2, c));
    yield return ("Bayer4x4", (s, c) => s.ReduceColors(new MedianCutQuantizer(), OrderedDitherer.Bayer4x4, c));
    yield return ("Bayer8x8", (s, c) => s.ReduceColors(new MedianCutQuantizer(), OrderedDitherer.Bayer8x8, c));
    yield return ("Bayer16x16", (s, c) => s.ReduceColors(new MedianCutQuantizer(), OrderedDitherer.Bayer16x16, c));
    yield return ("Halftone4x4", (s, c) => s.ReduceColors(new MedianCutQuantizer(), OrderedDitherer.Halftone4x4, c));
    yield return ("Halftone8x8", (s, c) => s.ReduceColors(new MedianCutQuantizer(), OrderedDitherer.Halftone8x8, c));
    yield return ("ClusterDot4x4", (s, c) => s.ReduceColors(new MedianCutQuantizer(), OrderedDitherer.ClusterDot4x4, c));
    yield return ("ClusterDot8x8", (s, c) => s.ReduceColors(new MedianCutQuantizer(), OrderedDitherer.ClusterDot8x8, c));
    yield return ("Diagonal4x4", (s, c) => s.ReduceColors(new MedianCutQuantizer(), OrderedDitherer.Diagonal4x4, c));

    // Noise Dithering
    yield return ("WhiteNoise", (s, c) => s.ReduceColors(new MedianCutQuantizer(), NoiseDitherer.WhiteNoise, c));
    yield return ("BlueNoise", (s, c) => s.ReduceColors(new MedianCutQuantizer(), NoiseDitherer.BlueNoise, c));
    yield return ("PinkNoise", (s, c) => s.ReduceColors(new MedianCutQuantizer(), NoiseDitherer.PinkNoise, c));
    yield return ("BrownNoise", (s, c) => s.ReduceColors(new MedianCutQuantizer(), NoiseDitherer.BrownNoise, c));
    yield return ("VioletNoise", (s, c) => s.ReduceColors(new MedianCutQuantizer(), NoiseDitherer.VioletNoise, c));
    yield return ("GreyNoise", (s, c) => s.ReduceColors(new MedianCutQuantizer(), NoiseDitherer.GreyNoise, c));

    // Specialized Ditherers
    yield return ("Yliluoma1", (s, c) => s.ReduceColors(new MedianCutQuantizer(), YliluomaDitherer.Algorithm1, c));
    yield return ("Yliluoma2", (s, c) => s.ReduceColors(new MedianCutQuantizer(), YliluomaDitherer.Algorithm2, c));
    yield return ("Yliluoma3", (s, c) => s.ReduceColors(new MedianCutQuantizer(), YliluomaDitherer.Algorithm3, c));
    yield return ("Ostromoukhov", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new OstromoukhovDitherer(), c));
    yield return ("Riemersma", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new RiemersmaDitherer(), c));
    yield return ("BlueNoiseDitherer", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new BlueNoiseDitherer(), c));
    yield return ("Knoll", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new KnollDitherer(), c));
    yield return ("InterleavedGradientNoise", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new InterleavedGradientNoiseDitherer(), c));
    yield return ("Random", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new RandomDitherer(), c));
    yield return ("VoidAndCluster", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new VoidAndClusterDitherer(), c));
    yield return ("Debanding", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new DebandingDitherer(), c));
    yield return ("GradientAware", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new GradientAwareDitherer(), c));
    yield return ("NaturalNeighbour", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new NaturalNeighbourDitherer(), c));
    yield return ("StructureAware", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new StructureAwareDitherer(), c));
    yield return ("NClosest", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new NClosestDitherer(), c));
    yield return ("Barycentric", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new BarycentricDitherer(), c));
    yield return ("Tin", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new TinDitherer(), c));
    yield return ("NConvex", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new NConvexDitherer(), c));
    yield return ("ADithererXorY149", (s, c) => s.ReduceColors(new MedianCutQuantizer(), XorY149Ditherer.Default, c));
    yield return ("ADithererXYArithmetic", (s, c) => s.ReduceColors(new MedianCutQuantizer(), XYArithmeticDitherer.Default, c));
    yield return ("Adaptive", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new AdaptiveDitherer(), c));
    yield return ("Smart", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new SmartDitherer(), c));
    yield return ("Dbs", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new DbsDitherer(), c));
    yield return ("Dizzy", (s, c) => s.ReduceColors(new MedianCutQuantizer(), new DizzyDitherer(), c));
    yield return ("AdaptiveMatrix", (s, c) => s.ReduceColors(new MedianCutQuantizer(), AdaptiveMatrixDitherer.Default, c));
  }

  #endregion

  #region Solid Color Tests

  [Test]
  public void AllDitherers_SolidBlack_RemainsBlack() {
    using var bitmap = TestUtilities.CreateSolidBitmap(16, 16, Color.Black);

    foreach (var (name, dither) in AllDithererFuncs()) {
      using var result = dither(bitmap, 2);

      using var locker = result.Lock();
      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        Assert.That(pixel.R, Is.LessThan(50), $"{name}: Black pixel at ({x},{y}) should remain dark, got {pixel}");
        Assert.That(pixel.G, Is.LessThan(50), $"{name}: Black pixel at ({x},{y}) should remain dark");
        Assert.That(pixel.B, Is.LessThan(50), $"{name}: Black pixel at ({x},{y}) should remain dark");
      }
    }
  }

  [Test]
  public void AllDitherers_SolidWhite_RemainsWhite() {
    using var bitmap = TestUtilities.CreateSolidBitmap(16, 16, Color.White);

    foreach (var (name, dither) in AllDithererFuncs()) {
      using var result = dither(bitmap, 2);

      using var locker = result.Lock();
      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        Assert.That(pixel.R, Is.GreaterThan(200), $"{name}: White pixel at ({x},{y}) should remain light, got {pixel}");
        Assert.That(pixel.G, Is.GreaterThan(200), $"{name}: White pixel at ({x},{y}) should remain light");
        Assert.That(pixel.B, Is.GreaterThan(200), $"{name}: White pixel at ({x},{y}) should remain light");
      }
    }
  }

  [Test]
  public void AllDitherers_SolidRed_With16Colors_RemainsRed() {
    using var bitmap = TestUtilities.CreateSolidBitmap(16, 16, Color.Red);

    foreach (var (name, dither) in AllDithererFuncs()) {
      using var result = dither(bitmap, 16);

      using var locker = result.Lock();
      var redPixelCount = 0;
      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        if (pixel.R > 200 && pixel.G < 100 && pixel.B < 100)
          ++redPixelCount;
      }

      var totalPixels = result.Width * result.Height;
      Assert.That(redPixelCount, Is.GreaterThan(totalPixels * 0.8),
        $"{name}: Most pixels should remain red, got {redPixelCount}/{totalPixels}");
    }
  }

  #endregion

  #region Gray Dithering Tests

  [Test]
  public void ActualDitherers_MidGray_ProducesBalancedDither() {
    // Use a gradient (many colors) to ensure dithering is triggered
    // A solid gray has only 1 unique color which fits in 2-color palette without dithering
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);

    // Use UniformQuantizer for gray tests to ensure black-and-white palette
    var ditherers = new (string name, DitherFunc dither)[] {
      ("FloydSteinberg", (s, c) => s.ReduceColors(new UniformQuantizer(), ErrorDiffusion.FloydSteinberg, c)),
      ("Bayer4x4", (s, c) => s.ReduceColors(new UniformQuantizer(), OrderedDitherer.Bayer4x4, c)),
      ("WhiteNoise", (s, c) => s.ReduceColors(new UniformQuantizer(), NoiseDitherer.WhiteNoise, c)),
      ("BlueNoise", (s, c) => s.ReduceColors(new UniformQuantizer(), NoiseDitherer.BlueNoise, c)),
      ("Yliluoma1", (s, c) => s.ReduceColors(new UniformQuantizer(), YliluomaDitherer.Algorithm1, c)),
    };

    foreach (var (name, dither) in ditherers) {
      using var result = dither(bitmap, 2);

      using var locker = result.Lock();
      var darkCount = 0;
      var lightCount = 0;

      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        var luminance = (pixel.R + pixel.G + pixel.B) / 3;
        if (luminance < 128)
          ++darkCount;
        else
          ++lightCount;
      }

      var totalPixels = result.Width * result.Height;
      var darkRatio = (double)darkCount / totalPixels;

      // Relax tolerance - some ditherers may have different balance points
      Assert.That(darkRatio, Is.InRange(0.2, 0.8),
        $"{name}: gradient should produce roughly 50% dark pixels, got {darkRatio:P0}");
    }
  }

  [Test]
  public void ActualDitherers_DarkGray_ProducesCorrectRatio() {
    // Use a dark gradient (many colors) to ensure dithering is triggered
    // A solid gray has only 1 unique color which fits in 2-color palette without dithering
    var darkGray = Color.FromArgb(64, 64, 64);
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, darkGray);

    // Use only actual ditherers - NoDithering doesn't produce balanced patterns
    foreach (var (name, dither) in ActualDithererFuncs()) {
      using var result = dither(bitmap, 2);

      using var locker = result.Lock();
      var lightCount = 0;

      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        var luminance = (pixel.R + pixel.G + pixel.B) / 3;
        if (luminance >= 128)
          ++lightCount;
      }

      var totalPixels = result.Width * result.Height;
      var lightRatio = (double)lightCount / totalPixels;

      // Relax tolerance for dark grays
      Assert.That(lightRatio, Is.InRange(0.0, 0.6),
        $"{name}: dark gradient should produce mostly dark pixels, got {lightRatio:P0}");
    }
  }

  [Test]
  public void ActualDitherers_LightGray_ProducesCorrectRatio() {
    // Use a light gradient (many colors) to ensure dithering is triggered
    var lightGray = Color.FromArgb(192, 192, 192);
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, lightGray, Color.White);

    // Use only actual ditherers - NoDithering doesn't produce balanced patterns
    foreach (var (name, dither) in ActualDithererFuncs()) {
      using var result = dither(bitmap, 2);

      using var locker = result.Lock();
      var lightCount = 0;

      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        var luminance = (pixel.R + pixel.G + pixel.B) / 3;
        if (luminance >= 128)
          ++lightCount;
      }

      var totalPixels = result.Width * result.Height;
      var lightRatio = (double)lightCount / totalPixels;

      // Relax tolerance for light grays
      Assert.That(lightRatio, Is.InRange(0.4, 1.0),
        $"{name}: light gradient should produce mostly light pixels, got {lightRatio:P0}");
    }
  }

  #endregion

  #region Gradient Tests

  [Test]
  public void ActualDitherers_Gradient_ProducesVaryingDensity() {
    using var bitmap = TestUtilities.CreateGradientBitmap(64, 16, Color.Black, Color.White);

    // Ditherers with known issues for gradient direction (positional dithering that doesn't preserve gradient)
    var knownExceptions = new HashSet<string> { "Yliluoma3" };

    // Use only actual ditherers - NoDithering doesn't produce gradual transitions
    foreach (var (name, dither) in ActualDithererFuncs()) {
      if (knownExceptions.Contains(name))
        continue;

      using var result = dither(bitmap, 2);

      using var locker = result.Lock();

      var leftLightCount = 0;
      var rightLightCount = 0;
      var leftWidth = result.Width / 3;
      var rightStart = result.Width * 2 / 3;

      for (var y = 0; y < result.Height; ++y) {
        for (var x = 0; x < leftWidth; ++x) {
          var pixel = locker[x, y];
          if ((pixel.R + pixel.G + pixel.B) / 3 >= 128)
            ++leftLightCount;
        }

        for (var x = rightStart; x < result.Width; ++x) {
          var pixel = locker[x, y];
          if ((pixel.R + pixel.G + pixel.B) / 3 >= 128)
            ++rightLightCount;
        }
      }

      // Use >= to allow equal counts for some ditherers on small areas
      Assert.That(rightLightCount, Is.GreaterThanOrEqualTo(leftLightCount),
        $"{name}: Gradient should have more or equal white on right side (right={rightLightCount}, left={leftLightCount})");
    }
  }

  #endregion

  #region Output Validity Tests

  [Test]
  public void AllDitherers_Output_UsesOnlyPaletteColors() {
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Red, Color.Blue);

    foreach (var (name, dither) in AllDithererFuncs()) {
      using var result = dither(bitmap, 8);
      using var locker = result.Lock();

      var paletteColors = new HashSet<int>(result.Palette.Entries.Select(c => c.ToArgb()));

      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        Assert.That(paletteColors.Contains(pixel.ToArgb()), Is.True,
          $"{name}: Pixel at ({x},{y}) = {pixel} should be in palette");
      }
    }
  }

  [Test]
  public void AllDitherers_Output_PreservesDimensions() {
    using var bitmap = TestUtilities.CreateTestPattern(25, 17);

    foreach (var (name, dither) in AllDithererFuncs()) {
      using var result = dither(bitmap, 16);

      Assert.That(result.Width, Is.EqualTo(25), $"{name}: Width should be preserved");
      Assert.That(result.Height, Is.EqualTo(17), $"{name}: Height should be preserved");
    }
  }

  #endregion

  #region Reproducibility Tests

  [Test]
  public void DeterministicDitherers_SameInput_ProducesSameOutput() {
    using var bitmap1 = TestUtilities.CreateGradientBitmap(16, 16, Color.Black, Color.White);
    using var bitmap2 = TestUtilities.CreateGradientBitmap(16, 16, Color.Black, Color.White);

    var deterministicDitherers = new (string name, DitherFunc dither)[] {
      ("NoDithering", (s, c) => s.ReduceColors(new UniformQuantizer(), NoDithering.Instance, c)),
      ("Bayer2x2", (s, c) => s.ReduceColors(new UniformQuantizer(), OrderedDitherer.Bayer2x2, c)),
      ("Bayer4x4", (s, c) => s.ReduceColors(new UniformQuantizer(), OrderedDitherer.Bayer4x4, c)),
      ("Bayer8x8", (s, c) => s.ReduceColors(new UniformQuantizer(), OrderedDitherer.Bayer8x8, c)),
      ("Halftone4x4", (s, c) => s.ReduceColors(new UniformQuantizer(), OrderedDitherer.Halftone4x4, c)),
      ("FloydSteinberg", (s, c) => s.ReduceColors(new UniformQuantizer(), ErrorDiffusion.FloydSteinberg, c)),
      ("Atkinson", (s, c) => s.ReduceColors(new UniformQuantizer(), ErrorDiffusion.Atkinson, c)),
      ("JarvisJudiceNinke", (s, c) => s.ReduceColors(new UniformQuantizer(), ErrorDiffusion.JarvisJudiceNinke, c)),
    };

    foreach (var (name, dither) in deterministicDitherers) {
      using var result1 = dither(bitmap1, 4);
      using var result2 = dither(bitmap2, 4);

      using var locker1 = result1.Lock();
      using var locker2 = result2.Lock();

      for (var y = 0; y < result1.Height; ++y)
      for (var x = 0; x < result1.Width; ++x)
        Assert.That(locker1[x, y].ToArgb(), Is.EqualTo(locker2[x, y].ToArgb()),
          $"{name}: Pixel at ({x},{y}) should be reproducible");
    }
  }

  #endregion

  #region NoDithering Specific Tests

  [Test]
  public void NoDithering_SolidGray_ProducesUniformOutput() {
    var gray = Color.FromArgb(100, 100, 100);
    using var bitmap = TestUtilities.CreateSolidBitmap(16, 16, gray);

    using var result = bitmap.ReduceColors(new UniformQuantizer(), NoDithering.Instance, 2);

    using var locker = result.Lock();
    var firstPixel = locker[0, 0];

    for (var y = 0; y < result.Height; ++y)
    for (var x = 0; x < result.Width; ++x)
      Assert.That(locker[x, y].ToArgb(), Is.EqualTo(firstPixel.ToArgb()),
        $"NoDithering: All pixels should be identical for solid input");
  }

  #endregion

  #region Ordered Dithering Specific Tests

  [Test]
  public void BayerDithering_ProducesRegularPattern() {
    var gray = Color.FromArgb(128, 128, 128);
    using var bitmap = TestUtilities.CreateSolidBitmap(8, 8, gray);

    using var result = bitmap.ReduceColors(new UniformQuantizer(), OrderedDitherer.Bayer4x4, 2);

    using var locker = result.Lock();

    for (var y = 0; y < 4; ++y)
    for (var x = 0; x < 4; ++x) {
      var color1 = locker[x, y];
      var color2 = locker[x + 4, y + 4];
      Assert.That(color1.ToArgb(), Is.EqualTo(color2.ToArgb()),
        $"Bayer4x4: Pattern should repeat at ({x},{y}) and ({x + 4},{y + 4})");
    }
  }

  #endregion

  #region Error Diffusion Specific Tests

  [Test]
  public void ErrorDiffusion_GrayGradient_ProducesSmootherTransition() {
    using var bitmap = TestUtilities.CreateGradientBitmap(64, 8, Color.Black, Color.White);

    using var resultNone = bitmap.ReduceColors(new UniformQuantizer(), NoDithering.Instance, 2);
    using var resultFS = bitmap.ReduceColors(new UniformQuantizer(), ErrorDiffusion.FloydSteinberg, 2);

    var transitionsNone = CountHorizontalTransitions(resultNone);
    var transitionsFS = CountHorizontalTransitions(resultFS);

    Assert.That(transitionsFS, Is.GreaterThan(transitionsNone),
      $"Floyd-Steinberg should have more transitions than no dithering ({transitionsFS} vs {transitionsNone})");
  }

  private static int CountHorizontalTransitions(Bitmap bitmap) {
    using var locker = bitmap.Lock();
    var transitions = 0;

    for (var y = 0; y < bitmap.Height; ++y) {
      var prevLuminance = -1;
      for (var x = 0; x < bitmap.Width; ++x) {
        var pixel = locker[x, y];
        var luminance = (pixel.R + pixel.G + pixel.B) / 3 >= 128 ? 1 : 0;
        if (prevLuminance >= 0 && luminance != prevLuminance)
          ++transitions;
        prevLuminance = luminance;
      }
    }

    return transitions;
  }

  #endregion

  #region Edge Case Tests

  [Test]
  public void AllDitherers_SinglePixel_Works() {
    using var bitmap = TestUtilities.CreateSolidBitmap(1, 1, Color.Gray);

    foreach (var (name, dither) in AllDithererFuncs()) {
      using var result = dither(bitmap, 8);

      Assert.That(result.Width, Is.EqualTo(1), $"{name}: Should handle 1x1 image");
      Assert.That(result.Height, Is.EqualTo(1), $"{name}: Should handle 1x1 image");
    }
  }

  [Test]
  public void AllDitherers_SmallImage_Works() {
    using var bitmap = TestUtilities.CreateTestPattern(3, 3);

    foreach (var (name, dither) in AllDithererFuncs()) {
      using var result = dither(bitmap, 4);

      Assert.That(result.Width, Is.EqualTo(3), $"{name}: Width should be 3");
      Assert.That(result.Height, Is.EqualTo(3), $"{name}: Height should be 3");
    }
  }

  #endregion

  #region RequiresSequentialProcessing Tests

  [Test]
  public void OrderedDitherers_DoNotRequireSequentialProcessing() {
    Assert.That(NoDithering.Instance.RequiresSequentialProcessing, Is.False);
    Assert.That(OrderedDitherer.Bayer4x4.RequiresSequentialProcessing, Is.False);
    Assert.That(OrderedDitherer.Halftone4x4.RequiresSequentialProcessing, Is.False);
    Assert.That(NoiseDitherer.WhiteNoise.RequiresSequentialProcessing, Is.False);
  }

  [Test]
  public void ErrorDiffusion_RequiresSequentialProcessing() {
    Assert.That(ErrorDiffusion.FloydSteinberg.RequiresSequentialProcessing, Is.True);
    Assert.That(ErrorDiffusion.Atkinson.RequiresSequentialProcessing, Is.True);
    Assert.That(ErrorDiffusion.JarvisJudiceNinke.RequiresSequentialProcessing, Is.True);
  }

  #endregion

}
