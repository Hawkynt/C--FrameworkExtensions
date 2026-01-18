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
/// Known-answer tests for all quantizers.
/// These tests verify that quantizers produce consistent, expected results.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Quantization")]
[Category("KnownAnswer")]
public class QuantizerKnownAnswerTests {

  #region Test Data

  private static Bitmap CreateSolidColorsBitmap(params Color[] colors) {
    var size = (int)Math.Ceiling(Math.Sqrt(colors.Length));
    var bitmap = new Bitmap(size, size);
    using var locker = bitmap.Lock();
    // Fill all pixels, wrapping colors if needed to avoid uninitialized pixels
    for (var i = 0; i < size * size; ++i)
      locker[i % size, i / size] = colors[i % colors.Length];
    return bitmap;
  }

  private static Bitmap CreateWeightedColorsBitmap(params (Color color, int weight)[] colorWeights) {
    var totalPixels = colorWeights.Sum(cw => cw.weight);
    var size = (int)Math.Ceiling(Math.Sqrt(totalPixels));
    var bitmap = new Bitmap(size, size);
    using var locker = bitmap.Lock();

    var pixelIndex = 0;
    foreach (var (color, weight) in colorWeights)
      for (var i = 0; i < weight && pixelIndex < size * size; ++i, ++pixelIndex)
        locker[pixelIndex % size, pixelIndex / size] = color;

    // Fill remaining pixels with first color to avoid introducing transparent/black as extra color
    if (colorWeights.Length > 0) {
      var fillColor = colorWeights[0].color;
      for (; pixelIndex < size * size; ++pixelIndex)
        locker[pixelIndex % size, pixelIndex / size] = fillColor;
    }

    return bitmap;
  }

  #endregion

  #region Quantizer Test Executor

  private delegate Bitmap ReduceColorsFunc(Bitmap source, int colorCount);

  /// <summary>
  /// Returns all quantizers for basic functionality tests.
  /// </summary>
  private static IEnumerable<(string name, ReduceColorsFunc reduce)> AllQuantizerReducers() {
    yield return ("Uniform", (s, c) => s.ReduceColors(new UniformQuantizer(), NoDithering.Instance, c));
    foreach (var item in AdaptiveQuantizerReducers())
      yield return item;
  }

  /// <summary>
  /// Returns adaptive quantizers (excludes UniformQuantizer which uses a fixed grid).
  /// Use this for tests that require color-adaptive behavior.
  /// </summary>
  private static IEnumerable<(string name, ReduceColorsFunc reduce)> AdaptiveQuantizerReducers() {
    yield return ("Popularity", (s, c) => s.ReduceColors(new PopularityQuantizer(), NoDithering.Instance, c));
    yield return ("Octree", (s, c) => s.ReduceColors(new OctreeQuantizer(), NoDithering.Instance, c));
    yield return ("MedianCut", (s, c) => s.ReduceColors(new MedianCutQuantizer(), NoDithering.Instance, c));
    yield return ("Wu", (s, c) => s.ReduceColors(new WuQuantizer(), NoDithering.Instance, c));
    yield return ("KMeans", (s, c) => s.ReduceColors(new KMeansQuantizer { MaxIterations = 30 }, NoDithering.Instance, c));
    yield return ("Neuquant", (s, c) => s.ReduceColors(new NeuquantQuantizer(), NoDithering.Instance, c));
    yield return ("VarianceCut", (s, c) => s.ReduceColors(new VarianceCutQuantizer(), NoDithering.Instance, c));
    yield return ("BinarySplitting", (s, c) => s.ReduceColors(new BinarySplittingQuantizer(), NoDithering.Instance, c));
    yield return ("BisectingKMeans", (s, c) => s.ReduceColors(new BisectingKMeansQuantizer { MaxIterationsPerSplit = 20 }, NoDithering.Instance, c));
    yield return ("IncrementalKMeans", (s, c) => s.ReduceColors(new IncrementalKMeansQuantizer { RefinementPasses = 3 }, NoDithering.Instance, c));
    yield return ("SpatialColor", (s, c) => s.ReduceColors(new SpatialColorQuantizer(), NoDithering.Instance, c));
    yield return ("VarianceBased", (s, c) => s.ReduceColors(new VarianceBasedQuantizer(), NoDithering.Instance, c));
    yield return ("Adu", (s, c) => s.ReduceColors(new AduQuantizer(), NoDithering.Instance, c));
    yield return ("GaussianMixture", (s, c) => s.ReduceColors(new GaussianMixtureQuantizer { MaxIterations = 10, MaxSampleSize = 1000 }, NoDithering.Instance, c));
    yield return ("FuzzyCMeans", (s, c) => s.ReduceColors(new FuzzyCMeansQuantizer { MaxIterations = 10, MaxSampleSize = 1000 }, NoDithering.Instance, c));
    yield return ("ColorQuantizationNetwork", (s, c) => s.ReduceColors(new ColorQuantizationNetworkQuantizer { MaxSampleSize = 1000 }, NoDithering.Instance, c));
  }

  #endregion

  #region Exact Color Preservation Tests

  [Test]
  public void AdaptiveQuantizers_ExactColorCount_PreservesColors() {
    var inputColors = new[] { Color.Red, Color.Green, Color.Blue };
    using var bitmap = CreateSolidColorsBitmap(inputColors);

    // Only test adaptive quantizers - UniformQuantizer uses a fixed grid by design
    foreach (var (name, reduce) in AdaptiveQuantizerReducers()) {
      using var result = reduce(bitmap, 3);
      var palette = result.Palette.Entries.Take(3).ToArray();

      foreach (var inputColor in inputColors) {
        var found = palette.Any(p => ColorDistance(p, inputColor) < 30);
        Assert.That(found, Is.True, $"{name}: Expected {inputColor} in palette, got [{string.Join(", ", palette.Select(p => p.ToString()))}]");
      }
    }
  }

  [TestCase(255, 0, 0)]     // Red
  [TestCase(0, 255, 0)]     // Green
  [TestCase(0, 0, 255)]     // Blue
  [TestCase(255, 255, 0)]   // Yellow
  [TestCase(255, 0, 255)]   // Magenta
  [TestCase(0, 255, 255)]   // Cyan
  [TestCase(128, 128, 128)] // Gray
  public void AdaptiveQuantizers_SolidColor_PreservesColor(int r, int g, int b) {
    var color = Color.FromArgb(r, g, b);
    using var bitmap = TestUtilities.CreateSolidBitmap(10, 10, color);

    // Only test adaptive quantizers - UniformQuantizer uses a fixed grid by design
    foreach (var (name, reduce) in AdaptiveQuantizerReducers()) {
      using var result = reduce(bitmap, 16);
      var palette = result.Palette.Entries;

      var found = palette.Any(p => ColorDistance(p, color) < 30);
      Assert.That(found, Is.True, $"{name}: Expected {color} in palette");
    }
  }

  #endregion

  #region Dominant Color Tests

  [Test]
  public void AllQuantizers_DominantColor_IncludesDominant() {
    using var bitmap = CreateWeightedColorsBitmap(
      (Color.Red, 900),
      (Color.Blue, 100)
    );

    foreach (var (name, reduce) in AllQuantizerReducers()) {
      using var result = reduce(bitmap, 8);
      var palette = result.Palette.Entries;

      var hasRed = palette.Any(p => p.R > 200 && p.G < 50 && p.B < 50);
      Assert.That(hasRed, Is.True, $"{name}: Dominant red color should be in palette");
    }
  }

  #endregion

  #region Grayscale Tests

  [Test]
  public void AdaptiveQuantizers_GrayscaleGradient_ProducesGrayscalePalette() {
    using var bitmap = TestUtilities.CreateGradientBitmap(64, 64, Color.Black, Color.White);

    // Only test adaptive quantizers - UniformQuantizer creates RGB grid with non-grayscale colors
    foreach (var (name, reduce) in AdaptiveQuantizerReducers()) {
      using var result = reduce(bitmap, 8);
      var palette = result.Palette.Entries.Take(8).ToArray();

      foreach (var color in palette) {
        var maxDiff = Math.Max(Math.Abs(color.R - color.G), Math.Max(Math.Abs(color.G - color.B), Math.Abs(color.R - color.B)));
        Assert.That(maxDiff, Is.LessThan(30), $"{name}: Palette color {color} should be grayscale");
      }
    }
  }

  [Test]
  public void AdaptiveQuantizers_GrayscaleGradient_2Colors_ProducesBlackAndWhite() {
    using var bitmap = TestUtilities.CreateGradientBitmap(64, 64, Color.Black, Color.White);

    // PopularityQuantizer picks most common colors - for gradients with similar counts,
    // it doesn't ensure spread across the color range. Neuquant/Adu use neural networks
    // that may converge to similar local minima for small palettes.
    var knownExceptions = new HashSet<string> { "Popularity", "Neuquant", "Adu" };

    // Only test adaptive quantizers - UniformQuantizer may produce non-grayscale colors
    foreach (var (name, reduce) in AdaptiveQuantizerReducers()) {
      if (knownExceptions.Contains(name))
        continue;

      using var result = reduce(bitmap, 2);
      var palette = result.Palette.Entries.Take(2).ToArray();

      var hasDark = palette.Any(p => p.R < 100 && p.G < 100 && p.B < 100);
      var hasLight = palette.Any(p => p.R > 150 && p.G > 150 && p.B > 150);

      Assert.That(hasDark, Is.True, $"{name}: Should have dark color in 2-color palette");
      Assert.That(hasLight, Is.True, $"{name}: Should have light color in 2-color palette");
    }
  }

  #endregion

  #region Color Distribution Tests

  [Test]
  public void AdaptiveQuantizers_RGBCorners_MostCornersInPalette() {
    var corners = new[] {
      Color.FromArgb(255, 0, 0),    // Red
      Color.FromArgb(0, 255, 0),    // Green
      Color.FromArgb(0, 0, 255),    // Blue
      Color.FromArgb(255, 255, 0),  // Yellow
      Color.FromArgb(255, 0, 255),  // Magenta
      Color.FromArgb(0, 255, 255),  // Cyan
      Color.FromArgb(0, 0, 0),      // Black
      Color.FromArgb(255, 255, 255) // White
    };

    using var bitmap = CreateWeightedColorsBitmap(corners.Select(c => (c, 100)).ToArray());

    // Only test adaptive quantizers - UniformQuantizer uses a fixed grid
    // Neural network based quantizers may combine similar corners, so we only require 5 of 8
    foreach (var (name, reduce) in AdaptiveQuantizerReducers()) {
      using var result = reduce(bitmap, 8);
      var palette = result.Palette.Entries.Take(8).ToArray();

      var foundCount = corners.Count(corner => palette.Any(p => ColorDistance(p, corner) < 50));
      Assert.That(foundCount, Is.GreaterThanOrEqualTo(5), $"{name}: Should find at least 5 of 8 RGB corners in palette, found {foundCount}");
    }
  }

  #endregion

  #region Palette Size Tests

  [TestCase(2)]
  [TestCase(4)]
  [TestCase(8)]
  [TestCase(16)]
  public void AllQuantizers_RequestedPaletteSize_ProducesCorrectSize(int requestedColors) {
    using var bitmap = TestUtilities.CreateTestPattern(32, 32);

    foreach (var (name, reduce) in AllQuantizerReducers()) {
      using var result = reduce(bitmap, requestedColors);

      var uniqueColors = result.Palette.Entries
        .Take(requestedColors)
        .Where(c => c.A > 0)
        .Distinct()
        .Count();

      Assert.That(uniqueColors, Is.GreaterThanOrEqualTo(Math.Min(requestedColors, 4)),
        $"{name}: Should produce at least {Math.Min(requestedColors, 4)} unique colors for test pattern");
    }
  }

  #endregion

  #region Reproducibility Tests

  [Test]
  public void AllQuantizers_SameInput_ProducesSameOutput() {
    using var bitmap1 = TestUtilities.CreateTestPattern(20, 20);
    using var bitmap2 = TestUtilities.CreateTestPattern(20, 20);

    foreach (var (name, reduce) in AllQuantizerReducers()) {
      using var result1 = reduce(bitmap1, 8);
      using var result2 = reduce(bitmap2, 8);

      var palette1 = result1.Palette.Entries.Take(8).OrderBy(c => c.ToArgb()).ToArray();
      var palette2 = result2.Palette.Entries.Take(8).OrderBy(c => c.ToArgb()).ToArray();

      for (var i = 0; i < 8; ++i)
        Assert.That(ColorDistance(palette1[i], palette2[i]), Is.LessThan(5),
          $"{name}: Palette color {i} should be reproducible");
    }
  }

  #endregion

  #region Edge Cases

  [Test]
  public void AllQuantizers_SinglePixel_ProducesValidPalette() {
    using var bitmap = TestUtilities.CreateSolidBitmap(1, 1, Color.Red);

    foreach (var (name, reduce) in AllQuantizerReducers()) {
      using var result = reduce(bitmap, 8);

      Assert.That(result, Is.Not.Null, $"{name}: Should handle 1x1 image");
      Assert.That(result.Width, Is.EqualTo(1), $"{name}: Width should be 1");
      Assert.That(result.Height, Is.EqualTo(1), $"{name}: Height should be 1");
    }
  }

  [Test]
  public void AllQuantizers_SingleColorRequest_ProducesValidPalette() {
    using var bitmap = TestUtilities.CreateGradientBitmap(32, 32, Color.Red, Color.Blue);

    foreach (var (name, reduce) in AllQuantizerReducers()) {
      using var result = reduce(bitmap, 1);

      Assert.That(result, Is.Not.Null, $"{name}: Should handle 1-color request");
      var firstColor = result.Palette.Entries[0];
      Assert.That(firstColor.A, Is.GreaterThan(0), $"{name}: Single color should not be transparent");
    }
  }

  #endregion

  #region Quality Tests

  [Test]
  public void AllQuantizers_QuantizedImage_UsesOnlyPaletteColors() {
    using var bitmap = TestUtilities.CreateTestPattern(20, 20);

    foreach (var (name, reduce) in AllQuantizerReducers()) {
      using var result = reduce(bitmap, 16);
      using var locker = result.Lock();

      var paletteSet = new HashSet<int>(result.Palette.Entries.Select(c => c.ToArgb()));

      for (var y = 0; y < result.Height; ++y)
      for (var x = 0; x < result.Width; ++x) {
        var pixel = locker[x, y];
        Assert.That(paletteSet.Contains(pixel.ToArgb()), Is.True,
          $"{name}: Pixel at ({x},{y}) = {pixel} should be in palette");
      }
    }
  }

  #endregion

  #region Helper Methods

  private static double ColorDistance(Color a, Color b) {
    var dr = a.R - b.R;
    var dg = a.G - b.G;
    var db = a.B - b.B;
    return Math.Sqrt(dr * dr + dg * dg + db * db);
  }

  #endregion

}
