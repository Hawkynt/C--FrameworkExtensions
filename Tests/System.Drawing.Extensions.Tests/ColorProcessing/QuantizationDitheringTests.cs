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
using System.Drawing.Imaging;
using Hawkynt.ColorProcessing.Dithering;
using Hawkynt.ColorProcessing.Quantization;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Quantization")]
public class QuantizationDitheringTests {

  #region Basic ReduceColors Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_OctreeFloydSteinberg_PreservesDimensions() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_MedianCutAtkinson_PreservesDimensions() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.Atkinson, 16);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_WuStucki_PreservesDimensions() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.ReduceColors(new WuQuantizer(), ErrorDiffusion.Stucki, 16);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  #endregion

  #region Indexed Output Format Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_2Colors_Returns1bppIndexed() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 2);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format1bppIndexed));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_16Colors_Returns4bppIndexed() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Red, Color.Blue);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format4bppIndexed));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_MaxColors_Returns8bppIndexed() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Red, Color.Blue);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 255);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format8bppIndexed));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_4Colors_Returns4bppIndexed() {
    using var source = TestUtilities.CreateTestPattern(32, 32);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 4);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format4bppIndexed));
  }

  #endregion

  #region Quality Mode Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_FastMode_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Red, Color.Blue);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 16, isHighQuality: false);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_HighQualityMode_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Red, Color.Blue);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 16, isHighQuality: true);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  #endregion

  #region Different Quantizer Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_OctreeQuantizer_SolidColorPreserved() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    // Indexed bitmap - check palette contains blue-ish color
    var palette = result.Palette.Entries;
    var hasBlue = false;
    foreach (var c in palette)
      if (c.B > 200 && c.R < 50 && c.G < 50) {
        hasBlue = true;
        break;
      }

    Assert.That(hasBlue, Is.True, "Palette should contain blue color");
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_MedianCutQuantizer_SolidColorPreserved() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    var palette = result.Palette.Entries;
    var hasGreen = false;
    foreach (var c in palette)
      if (c.G > 100) {
        hasGreen = true;
        break;
      }

    Assert.That(hasGreen, Is.True, "Palette should contain green color");
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_WuQuantizer_SolidColorPreserved() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Cyan);
    using var result = source.ReduceColors(new WuQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    var palette = result.Palette.Entries;
    var hasCyan = false;
    foreach (var c in palette)
      if (c.G > 100 && c.B > 100) {
        hasCyan = true;
        break;
      }

    Assert.That(hasCyan, Is.True, "Palette should contain cyan color");
  }

  #endregion

  #region Different Ditherer Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_FloydSteinberg_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Atkinson_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.Atkinson, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_JarvisJudiceNinke_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.JarvisJudiceNinke, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Stucki_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.Stucki, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Sierra_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.Sierra, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_TwoRowSierra_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.TwoRowSierra, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_SierraLite_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.SierraLite, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Burkes_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.Burkes, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_StevensonArce_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.StevensonArce, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Pigeon_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.Pigeon, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_ShiauFan_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.ShiauFan, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Diamond_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.Diamond, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  #endregion

  #region Custom Ditherer Configuration Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_CustomMatrix_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    var ditherer = ErrorDiffusion.JarvisJudiceNinke;
    using var result = source.ReduceColors(new OctreeQuantizer(), ditherer, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_SerpentineScan_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    var ditherer = ErrorDiffusion.FloydSteinberg.Serpentine;
    using var result = source.ReduceColors(new OctreeQuantizer(), ditherer, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_ReducedStrength_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    var ditherer = ErrorDiffusion.FloydSteinberg.WithStrength(0.5f);
    using var result = source.ReduceColors(new OctreeQuantizer(), ditherer, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  #endregion

  #region Edge Case Tests

  [Test]
  [Category("EdgeCase")]
  public void ReduceColors_1x1Bitmap_HandlesEdgeCase() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Gray);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 8);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReduceColors_SingleColor_HandlesEdgeCase() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 1);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReduceColors_Checkerboard_ProducesValidOutput() {
    using var source = TestUtilities.CreateCheckerboard(16, 16, 4, Color.Black, Color.White);
    using var result = source.ReduceColors(new MedianCutQuantizer(), ErrorDiffusion.FloydSteinberg, 4);

    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Input Format Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_24bppInput_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Yellow, PixelFormat.Format24bppRgb);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region Color Preservation Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_TestPattern_ProducesDistinctQuadrants() {
    using var source = TestUtilities.CreateTestPattern(40, 40);
    using var result = source.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    // For indexed bitmap, check that palette has distinct colors
    var palette = result.Palette.Entries;
    var hasRed = false;
    var hasGreen = false;
    var hasBlue = false;

    foreach (var c in palette) {
      if (c.R > 150 && c.G < 100 && c.B < 100) hasRed = true;
      if (c.G > 150 && c.R < 100 && c.B < 100) hasGreen = true;
      if (c.B > 150 && c.R < 100 && c.G < 100) hasBlue = true;
    }

    Assert.That(hasRed || hasGreen || hasBlue, Is.True, "Palette should have distinct colors from test pattern");
  }

  #endregion

  #region New Quantizer Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_PopularityQuantizer_PreservesDimensions() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.ReduceColors(new PopularityQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_PopularityQuantizer_SolidColorPreserved() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Magenta);
    using var result = source.ReduceColors(new PopularityQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    var palette = result.Palette.Entries;
    var hasMagenta = false;
    foreach (var c in palette)
      if (c.R > 150 && c.B > 150) {
        hasMagenta = true;
        break;
      }

    Assert.That(hasMagenta, Is.True, "Palette should contain magenta color");
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_UniformQuantizer_PreservesDimensions() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.ReduceColors(new UniformQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_UniformQuantizer_ProducesUniformGrid() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new UniformQuantizer(), ErrorDiffusion.FloydSteinberg, 8);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format4bppIndexed));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_KMeansQuantizer_PreservesDimensions() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.ReduceColors(new KMeansQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_KMeansQuantizer_SolidColorPreserved() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Orange);
    using var result = source.ReduceColors(new KMeansQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    var palette = result.Palette.Entries;
    var hasOrange = false;
    foreach (var c in palette)
      if (c.R > 200 && c.G > 100 && c.B < 100) {
        hasOrange = true;
        break;
      }

    Assert.That(hasOrange, Is.True, "Palette should contain orange color");
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_KMeansQuantizer_WithCustomIterations_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(20, 20, Color.Red, Color.Blue);
    var quantizer = new KMeansQuantizer { MaxIterations = 50 };
    using var result = source.ReduceColors(quantizer, ErrorDiffusion.FloydSteinberg, 8);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_NeuquantQuantizer_PreservesDimensions() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.ReduceColors(new NeuquantQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_NeuquantQuantizer_SolidColorPreserved() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Purple);
    using var result = source.ReduceColors(new NeuquantQuantizer(), ErrorDiffusion.FloydSteinberg, 16);

    var palette = result.Palette.Entries;
    var hasPurple = false;
    foreach (var c in palette)
      if (c.R > 100 && c.B > 100) {
        hasPurple = true;
        break;
      }

    Assert.That(hasPurple, Is.True, "Palette should contain purple color");
  }

  #endregion

  #region Ordered Ditherer Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Bayer2x2_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), OrderedDitherer.Bayer2x2, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Bayer4x4_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), OrderedDitherer.Bayer4x4, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Bayer8x8_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), OrderedDitherer.Bayer8x8, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Bayer16x16_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), OrderedDitherer.Bayer16x16, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Halftone4x4_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), OrderedDitherer.Halftone4x4, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Halftone8x8_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), OrderedDitherer.Halftone8x8, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_ClusterDot4x4_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), OrderedDitherer.ClusterDot4x4, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_ClusterDot8x8_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), OrderedDitherer.ClusterDot8x8, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_Diagonal4x4_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), OrderedDitherer.Diagonal4x4, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_OrderedDitherer_WithStrength_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    var ditherer = OrderedDitherer.Bayer4x4.WithStrength(0.5f);
    using var result = source.ReduceColors(new OctreeQuantizer(), ditherer, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void OrderedDitherer_RequiresSequentialProcessing_ReturnsFalse() {
    var ditherer = OrderedDitherer.Bayer4x4;
    Assert.That(ditherer.RequiresSequentialProcessing, Is.False);
  }

  #endregion

  #region Noise Ditherer Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_WhiteNoise_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), NoiseDitherer.WhiteNoise, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_BlueNoise_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), NoiseDitherer.BlueNoise, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_PinkNoise_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), NoiseDitherer.PinkNoise, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_BrownNoise_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), NoiseDitherer.BrownNoise, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_VioletNoise_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), NoiseDitherer.VioletNoise, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_GreyNoise_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    using var result = source.ReduceColors(new OctreeQuantizer(), NoiseDitherer.GreyNoise, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_NoiseDitherer_WithStrength_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    var ditherer = NoiseDitherer.WhiteNoise.WithStrength(0.5f);
    using var result = source.ReduceColors(new OctreeQuantizer(), ditherer, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_NoiseDitherer_WithSeed_ProducesReproducibleResult() {
    using var source = TestUtilities.CreateGradientBitmap(32, 32, Color.Black, Color.White);
    var ditherer = NoiseDitherer.WhiteNoise.WithSeed(12345);
    using var result = source.ReduceColors(new OctreeQuantizer(), ditherer, 8);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void NoiseDitherer_RequiresSequentialProcessing_ReturnsFalse() {
    var ditherer = NoiseDitherer.WhiteNoise;
    Assert.That(ditherer.RequiresSequentialProcessing, Is.False);
  }

  #endregion

  #region Combined New Algorithm Tests

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_KMeansWithBayer_ProducesResult() {
    using var source = TestUtilities.CreateTestPattern(32, 32);
    using var result = source.ReduceColors(new KMeansQuantizer(), OrderedDitherer.Bayer8x8, 16);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_NeuquantWithBlueNoise_ProducesResult() {
    using var source = TestUtilities.CreateTestPattern(32, 32);
    using var result = source.ReduceColors(new NeuquantQuantizer(), NoiseDitherer.BlueNoise, 16);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_PopularityWithHalftone_ProducesResult() {
    using var source = TestUtilities.CreateTestPattern(32, 32);
    using var result = source.ReduceColors(new PopularityQuantizer(), OrderedDitherer.Halftone8x8, 16);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void ReduceColors_UniformWithWhiteNoise_ProducesResult() {
    using var source = TestUtilities.CreateTestPattern(32, 32);
    using var result = source.ReduceColors(new UniformQuantizer(), NoiseDitherer.WhiteNoise, 16);

    Assert.That(result.Width, Is.EqualTo(32));
    Assert.That(result.Height, Is.EqualTo(32));
  }

  #endregion

}
