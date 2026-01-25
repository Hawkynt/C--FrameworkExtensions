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
/// Tests for quantizer wrappers (KMeansRefinementWrapper, AcoRefinementWrapper, BitReductionWrapper).
/// These wrappers modify quantizer behavior by preprocessing input or postprocessing output.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Quantization")]
[Category("Wrappers")]
public class QuantizerWrapperTests {

  #region Test Data

  private static Bitmap CreateColoredBitmap() => TestUtilities.CreateTestPattern(32, 32);

  private static Bitmap CreateGradientBitmap() => TestUtilities.CreateGradientBitmap(64, 64, Color.Red, Color.Blue);

  private static Bitmap CreateSolidColorsBitmap(params Color[] colors) {
    var size = (int)Math.Ceiling(Math.Sqrt(colors.Length));
    var bitmap = new Bitmap(size, size);
    using var locker = bitmap.Lock();
    for (var i = 0; i < size * size; ++i)
      locker[i % size, i / size] = colors[i % colors.Length];
    return bitmap;
  }

  #endregion

  #region KMeansRefinementWrapper Tests

  [Test]
  public void KMeansRefinementWrapper_WithOctreeQuantizer_ProducesValidPalette() {
    using var bitmap = CreateColoredBitmap();

    var quantizer = new KMeansRefinementWrapper<OctreeQuantizer>(new OctreeQuantizer(), iterations: 5);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Palette.Entries.Length, Is.GreaterThanOrEqualTo(1));
  }

  [Test]
  public void KMeansRefinementWrapper_PreservesMainColors() {
    var colors = new[] { Color.Red, Color.Green, Color.Blue };
    using var bitmap = CreateSolidColorsBitmap(colors);

    var quantizer = new KMeansRefinementWrapper<MedianCutQuantizer>(new MedianCutQuantizer(), iterations: 10);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 4);

    var palette = result.Palette.Entries.Take(4).ToArray();
    foreach (var inputColor in colors) {
      var found = palette.Any(p => ColorDistance(p, inputColor) < 50);
      Assert.That(found, Is.True, $"Expected {inputColor} in palette");
    }
  }

  [TestCase(1)]
  [TestCase(5)]
  [TestCase(10)]
  public void KMeansRefinementWrapper_DifferentIterations_ProducesValidResults(int iterations) {
    using var bitmap = CreateGradientBitmap();

    var quantizer = new KMeansRefinementWrapper<OctreeQuantizer>(new OctreeQuantizer(), iterations);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
    var uniqueColors = result.Palette.Entries.Take(8).Where(c => c.A > 0).Distinct().Count();
    Assert.That(uniqueColors, Is.GreaterThanOrEqualTo(2));
  }

  [Test]
  public void KMeansRefinementWrapper_Reproducible_SameInputSameOutput() {
    using var bitmap1 = CreateColoredBitmap();
    using var bitmap2 = CreateColoredBitmap();

    var quantizer = new KMeansRefinementWrapper<MedianCutQuantizer>(new MedianCutQuantizer(), iterations: 5);

    using var result1 = bitmap1.ReduceColors(quantizer, NoDithering.Instance, 8);
    using var result2 = bitmap2.ReduceColors(quantizer, NoDithering.Instance, 8);

    var palette1 = result1.Palette.Entries.Take(8).OrderBy(c => c.ToArgb()).ToArray();
    var palette2 = result2.Palette.Entries.Take(8).OrderBy(c => c.ToArgb()).ToArray();

    for (var i = 0; i < 8; ++i)
      Assert.That(ColorDistance(palette1[i], palette2[i]), Is.LessThan(5));
  }

  #endregion

  #region AcoRefinementWrapper Tests

  [Test]
  public void AcoRefinementWrapper_WithOctreeQuantizer_ProducesValidPalette() {
    using var bitmap = CreateColoredBitmap();

    var quantizer = new AcoRefinementWrapper<OctreeQuantizer>(
      new OctreeQuantizer(),
      antCount: 10,
      iterations: 20,
      evaporationRate: 0.1,
      seed: 42
    );

    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Palette.Entries.Length, Is.GreaterThanOrEqualTo(1));
  }

  [Test]
  public void AcoRefinementWrapper_WithSeed_IsReproducible() {
    using var bitmap1 = CreateGradientBitmap();
    using var bitmap2 = CreateGradientBitmap();

    var quantizer1 = new AcoRefinementWrapper<MedianCutQuantizer>(
      new MedianCutQuantizer(),
      antCount: 10,
      iterations: 20,
      seed: 12345
    );

    var quantizer2 = new AcoRefinementWrapper<MedianCutQuantizer>(
      new MedianCutQuantizer(),
      antCount: 10,
      iterations: 20,
      seed: 12345
    );

    using var result1 = bitmap1.ReduceColors(quantizer1, NoDithering.Instance, 8);
    using var result2 = bitmap2.ReduceColors(quantizer2, NoDithering.Instance, 8);

    var palette1 = result1.Palette.Entries.Take(8).OrderBy(c => c.ToArgb()).ToArray();
    var palette2 = result2.Palette.Entries.Take(8).OrderBy(c => c.ToArgb()).ToArray();

    for (var i = 0; i < 8; ++i)
      Assert.That(ColorDistance(palette1[i], palette2[i]), Is.LessThan(5),
        $"Palette color {i} should be reproducible with same seed");
  }

  [Test]
  public void AcoRefinementWrapper_PreservesMainColors() {
    var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Yellow };
    using var bitmap = CreateSolidColorsBitmap(colors);

    var quantizer = new AcoRefinementWrapper<OctreeQuantizer>(
      new OctreeQuantizer(),
      antCount: 15,
      iterations: 30,
      seed: 42
    );

    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 4);

    var palette = result.Palette.Entries.Take(4).ToArray();
    var foundCount = colors.Count(inputColor => palette.Any(p => ColorDistance(p, inputColor) < 60));
    Assert.That(foundCount, Is.GreaterThanOrEqualTo(3), "Should preserve most input colors");
  }

  #endregion

  #region BitReductionWrapper Tests

  [Test]
  public void BitReductionWrapper_WithOctreeQuantizer_ProducesValidPalette() {
    using var bitmap = CreateColoredBitmap();

    var quantizer = new BitReductionWrapper<OctreeQuantizer>(new OctreeQuantizer(), bitsToRemove: 2);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Palette.Entries.Length, Is.GreaterThanOrEqualTo(1));
  }

  [TestCase(1)]
  [TestCase(2)]
  [TestCase(3)]
  [TestCase(4)]
  public void BitReductionWrapper_DifferentBitReductions_ProducesValidResults(int bitsToRemove) {
    using var bitmap = CreateGradientBitmap();

    var quantizer = new BitReductionWrapper<OctreeQuantizer>(new OctreeQuantizer(), bitsToRemove);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
    var uniqueColors = result.Palette.Entries.Take(8).Where(c => c.A > 0).Distinct().Count();
    Assert.That(uniqueColors, Is.GreaterThanOrEqualTo(1));
  }

  [Test]
  public void BitReductionWrapper_ReducesPaletteGranularity() {
    using var bitmap = CreateGradientBitmap();

    var noReduction = new OctreeQuantizer();
    var with4BitReduction = new BitReductionWrapper<OctreeQuantizer>(new OctreeQuantizer(), bitsToRemove: 4);

    using var resultNoReduction = bitmap.ReduceColors(noReduction, NoDithering.Instance, 16);
    using var resultWithReduction = bitmap.ReduceColors(with4BitReduction, NoDithering.Instance, 16);

    var paletteNoReduction = resultNoReduction.Palette.Entries.Take(16).ToArray();
    var paletteWithReduction = resultWithReduction.Palette.Entries.Take(16).ToArray();

    var uniqueNoReduction = paletteNoReduction.Select(c => (c.R >> 4, c.G >> 4, c.B >> 4)).Distinct().Count();
    var uniqueWithReduction = paletteWithReduction.Select(c => (c.R >> 4, c.G >> 4, c.B >> 4)).Distinct().Count();

    Assert.That(uniqueWithReduction, Is.LessThanOrEqualTo(uniqueNoReduction),
      "Bit reduction should produce coarser palette");
  }

  [Test]
  public void BitReductionWrapper_Reproducible_SameInputSameOutput() {
    using var bitmap1 = CreateColoredBitmap();
    using var bitmap2 = CreateColoredBitmap();

    var quantizer = new BitReductionWrapper<MedianCutQuantizer>(new MedianCutQuantizer(), bitsToRemove: 2);

    using var result1 = bitmap1.ReduceColors(quantizer, NoDithering.Instance, 8);
    using var result2 = bitmap2.ReduceColors(quantizer, NoDithering.Instance, 8);

    var palette1 = result1.Palette.Entries.Take(8).OrderBy(c => c.ToArgb()).ToArray();
    var palette2 = result2.Palette.Entries.Take(8).OrderBy(c => c.ToArgb()).ToArray();

    for (var i = 0; i < 8; ++i)
      Assert.That(palette1[i].ToArgb(), Is.EqualTo(palette2[i].ToArgb()));
  }

  #endregion

  #region Wrapper Chaining Tests

  [Test]
  public void WrapperChaining_BitReductionThenKMeans_ProducesValidPalette() {
    using var bitmap = CreateColoredBitmap();

    var quantizer = new BitReductionWrapper<KMeansRefinementWrapper<OctreeQuantizer>>(
      new KMeansRefinementWrapper<OctreeQuantizer>(new OctreeQuantizer(), iterations: 5),
      bitsToRemove: 2
    );

    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Palette.Entries.Length, Is.GreaterThanOrEqualTo(1));
  }

  [Test]
  public void WrapperChaining_KMeansThenAco_ProducesValidPalette() {
    using var bitmap = CreateColoredBitmap();

    var quantizer = new AcoRefinementWrapper<KMeansRefinementWrapper<MedianCutQuantizer>>(
      new KMeansRefinementWrapper<MedianCutQuantizer>(new MedianCutQuantizer(), iterations: 3),
      antCount: 10,
      iterations: 15,
      seed: 42
    );

    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Palette.Entries.Length, Is.GreaterThanOrEqualTo(1));
  }

  [Test]
  public void WrapperChaining_AllThreeWrappers_ProducesValidPalette() {
    using var bitmap = CreateColoredBitmap();

    var quantizer = new BitReductionWrapper<KMeansRefinementWrapper<AcoRefinementWrapper<OctreeQuantizer>>>(
      new KMeansRefinementWrapper<AcoRefinementWrapper<OctreeQuantizer>>(
        new AcoRefinementWrapper<OctreeQuantizer>(new OctreeQuantizer(), antCount: 5, iterations: 10, seed: 42),
        iterations: 3),
      bitsToRemove: 1
    );

    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Palette.Entries.Length, Is.GreaterThanOrEqualTo(1));
  }

  #endregion

  #region Edge Cases

  [Test]
  public void KMeansRefinementWrapper_SinglePixelImage_DoesNotCrash() {
    using var bitmap = TestUtilities.CreateSolidBitmap(1, 1, Color.Red);

    var quantizer = new KMeansRefinementWrapper<OctreeQuantizer>(new OctreeQuantizer(), iterations: 5);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
  }

  [Test]
  public void AcoRefinementWrapper_SinglePixelImage_DoesNotCrash() {
    using var bitmap = TestUtilities.CreateSolidBitmap(1, 1, Color.Red);

    var quantizer = new AcoRefinementWrapper<OctreeQuantizer>(new OctreeQuantizer(), antCount: 5, iterations: 10, seed: 42);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
  }

  [Test]
  public void BitReductionWrapper_SinglePixelImage_DoesNotCrash() {
    using var bitmap = TestUtilities.CreateSolidBitmap(1, 1, Color.Red);

    var quantizer = new BitReductionWrapper<OctreeQuantizer>(new OctreeQuantizer(), bitsToRemove: 3);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 8);

    Assert.That(result, Is.Not.Null);
  }

  [Test]
  public void KMeansWrapper_SingleColorRequest_ProducesValidPalette() {
    using var bitmap = CreateColoredBitmap();

    var quantizer = new KMeansRefinementWrapper<OctreeQuantizer>(new OctreeQuantizer(), iterations: 3);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 1);

    Assert.That(result, Is.Not.Null, "Should handle 1-color request");
    var firstColor = result.Palette.Entries[0];
    Assert.That(firstColor.A, Is.GreaterThan(0), "Single color should not be transparent");
  }

  [Test]
  public void AcoWrapper_SingleColorRequest_ProducesValidPalette() {
    using var bitmap = CreateColoredBitmap();

    var quantizer = new AcoRefinementWrapper<OctreeQuantizer>(new OctreeQuantizer(), antCount: 5, iterations: 10, seed: 42);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 1);

    Assert.That(result, Is.Not.Null, "Should handle 1-color request");
    var firstColor = result.Palette.Entries[0];
    Assert.That(firstColor.A, Is.GreaterThan(0), "Single color should not be transparent");
  }

  [Test]
  public void BitReductionWrapper_SingleColorRequest_ProducesValidPalette() {
    using var bitmap = CreateColoredBitmap();

    var quantizer = new BitReductionWrapper<OctreeQuantizer>(new OctreeQuantizer(), bitsToRemove: 2);
    using var result = bitmap.ReduceColors(quantizer, NoDithering.Instance, 1);

    Assert.That(result, Is.Not.Null, "Should handle 1-color request");
    var firstColor = result.Palette.Entries[0];
    Assert.That(firstColor.A, Is.GreaterThan(0), "Single color should not be transparent");
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
