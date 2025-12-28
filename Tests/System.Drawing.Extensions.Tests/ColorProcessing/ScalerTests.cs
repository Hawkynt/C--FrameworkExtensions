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
using System.Drawing.Imaging;
using Hawkynt.ColorProcessing.Scalers;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Scalers")]
public class ScalerTests {

  #region Scale2x Tests

  [Test]
  [Category("HappyPath")]
  public void Scale2x_Fast_OutputDimensionsAreDoubled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.Upscale(Scale.X2, ScalerQuality.Fast);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Scale2x_HighQuality_OutputDimensionsAreDoubled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.Upscale(Scale.X2, ScalerQuality.HighQuality);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Scale2x_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.Upscale(Scale.X2);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.B, Is.EqualTo(Color.Blue.B));
    Assert.That(centerColor.R, Is.EqualTo(Color.Blue.R));
    Assert.That(centerColor.G, Is.EqualTo(Color.Blue.G));
  }

  [Test]
  [Category("HappyPath")]
  public void Scale2x_Checkerboard_ProducesResult() {
    using var source = TestUtilities.CreateCheckerboard(8, 8, 2, Color.Black, Color.White);
    using var result = source.Upscale(Scale.X2);

    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Scale2x_TestPattern_ScalesCorrectly() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.Upscale(Scale.X2);

    Assert.That(result.Width, Is.EqualTo(40));
    Assert.That(result.Height, Is.EqualTo(40));

    using var locker = result.Lock();
    var topLeft = locker[5, 5];
    var bottomRight = locker[35, 35];

    Assert.That(topLeft.R, Is.GreaterThan(200), "Top-left quadrant should be red");
    Assert.That(bottomRight.R, Is.GreaterThan(200), "Bottom-right quadrant should be yellow (high red)");
    Assert.That(bottomRight.G, Is.GreaterThan(200), "Bottom-right quadrant should be yellow (high green)");
  }

  #endregion

  #region Scale3x Tests

  [Test]
  [Category("HappyPath")]
  public void Scale3x_Fast_OutputDimensionsAreTripled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.Upscale(Scale.X3, ScalerQuality.Fast);

    Assert.That(result.Width, Is.EqualTo(30));
    Assert.That(result.Height, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void Scale3x_HighQuality_OutputDimensionsAreTripled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.Upscale(Scale.X3, ScalerQuality.HighQuality);

    Assert.That(result.Width, Is.EqualTo(30));
    Assert.That(result.Height, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void Scale3x_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Cyan);
    using var result = source.Upscale(Scale.X3);

    using var locker = result.Lock();
    var centerColor = locker[15, 15];

    Assert.That(centerColor.R, Is.EqualTo(Color.Cyan.R));
    Assert.That(centerColor.G, Is.EqualTo(Color.Cyan.G));
    Assert.That(centerColor.B, Is.EqualTo(Color.Cyan.B));
  }

  [Test]
  [Category("HappyPath")]
  public void Scale3x_TestPattern_ScalesCorrectly() {
    using var source = TestUtilities.CreateTestPattern(10, 10);
    using var result = source.Upscale(Scale.X3);

    Assert.That(result.Width, Is.EqualTo(30));
    Assert.That(result.Height, Is.EqualTo(30));
  }

  #endregion

  #region EPX Tests

  [Test]
  [Category("HappyPath")]
  public void Epx_Fast_OutputDimensionsAreDoubled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Magenta);
    using var result = source.Upscale<Epx>(ScalerQuality.Fast);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Epx_HighQuality_OutputDimensionsAreDoubled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Magenta);
    using var result = source.Upscale(Epx.Default, ScalerQuality.HighQuality);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Epx_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Orange);
    using var result = source.Upscale(Epx.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Orange.R).Within(1));
    Assert.That(centerColor.G, Is.EqualTo(Color.Orange.G).Within(1));
    Assert.That(centerColor.B, Is.EqualTo(Color.Orange.B).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Epx_Checkerboard_ProducesResult() {
    using var source = TestUtilities.CreateCheckerboard(8, 8, 1, Color.Black, Color.White);
    using var result = source.Upscale(Epx.Default);

    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Epx_TestPattern_ScalesCorrectly() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.Upscale(Epx.Default);

    Assert.That(result.Width, Is.EqualTo(40));
    Assert.That(result.Height, Is.EqualTo(40));

    using var locker = result.Lock();
    var topLeft = locker[5, 5];
    var topRight = locker[30, 5];

    Assert.That(topLeft.R, Is.GreaterThan(200), "Top-left should be red");
    Assert.That(topRight.G, Is.GreaterThan(100), "Top-right should have green component");
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void Scale2x_1x1Bitmap_ScalesTo2x2() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Red);
    using var result = source.Upscale(Scale.X2);

    Assert.That(result.Width, Is.EqualTo(2));
    Assert.That(result.Height, Is.EqualTo(2));

    using var locker = result.Lock();
    Assert.That(locker[0, 0].R, Is.EqualTo(Color.Red.R));
    Assert.That(locker[1, 1].R, Is.EqualTo(Color.Red.R));
  }

  [Test]
  [Category("EdgeCase")]
  public void Scale3x_1x1Bitmap_ScalesTo3x3() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.Upscale(Scale.X3);

    Assert.That(result.Width, Is.EqualTo(3));
    Assert.That(result.Height, Is.EqualTo(3));

    using var locker = result.Lock();
    Assert.That(locker[1, 1].B, Is.EqualTo(Color.Blue.B));
  }

  [Test]
  [Category("EdgeCase")]
  public void Epx_1x1Bitmap_ScalesTo2x2() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Green);
    using var result = source.Upscale(Epx.Default);

    Assert.That(result.Width, Is.EqualTo(2));
    Assert.That(result.Height, Is.EqualTo(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void Scale2x_NonSquareBitmap_ScalesCorrectly() {
    using var source = TestUtilities.CreateSolidBitmap(20, 10, Color.Yellow);
    using var result = source.Upscale(Scale.X2);

    Assert.That(result.Width, Is.EqualTo(40));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("EdgeCase")]
  public void Scale3x_NonSquareBitmap_ScalesCorrectly() {
    using var source = TestUtilities.CreateSolidBitmap(10, 20, Color.Purple);
    using var result = source.Upscale(Scale.X3);

    Assert.That(result.Width, Is.EqualTo(30));
    Assert.That(result.Height, Is.EqualTo(60));
  }

  [Test]
  [Category("EdgeCase")]
  public void Epx_NonSquareBitmap_ScalesCorrectly() {
    using var source = TestUtilities.CreateSolidBitmap(15, 5, Color.Teal);
    using var result = source.Upscale(Epx.Default);

    Assert.That(result.Width, Is.EqualTo(30));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region Quality Mode Tests

  [Test]
  [Category("HappyPath")]
  public void Scale2x_FastAndHighQuality_ProduceSameColorForSolidInput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fast = source.Upscale(Scale.X2, ScalerQuality.Fast);
    using var hq = source.Upscale(Scale.X2, ScalerQuality.HighQuality);

    using var fastLocker = fast.Lock();
    using var hqLocker = hq.Lock();

    var fastCenter = fastLocker[10, 10];
    var hqCenter = hqLocker[10, 10];

    Assert.That(fastCenter.R, Is.EqualTo(hqCenter.R).Within(1));
    Assert.That(fastCenter.G, Is.EqualTo(hqCenter.G).Within(1));
    Assert.That(fastCenter.B, Is.EqualTo(hqCenter.B).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Epx_DefaultQuality_IsFast() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);

    using var defaultResult = source.Upscale(Epx.Default);
    using var fastResult = source.Upscale(Epx.Default, ScalerQuality.Fast);

    Assert.That(defaultResult.Width, Is.EqualTo(fastResult.Width));
    Assert.That(defaultResult.Height, Is.EqualTo(fastResult.Height));
  }

  #endregion

  #region Output Format Tests

  [Test]
  [Category("HappyPath")]
  public void Scale2x_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.Upscale(Scale.X2);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Scale3x_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.Upscale(Scale.X3);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Epx_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.Upscale(Epx.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Scale2x_24bppInput_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Yellow, PixelFormat.Format24bppRgb);
    using var result = source.Upscale(Scale.X2);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  #endregion

  #region Alpha Channel Tests

  [Test]
  [Category("HappyPath")]
  public void Scale2x_TransparentPixels_PreservesTransparency() {
    using var source = new Bitmap(4, 4, PixelFormat.Format32bppArgb);
    using (var locker = source.Lock()) {
      locker.Clear(Color.Transparent);
      locker[1, 1] = Color.Red;
      locker[2, 1] = Color.Red;
      locker[1, 2] = Color.Red;
      locker[2, 2] = Color.Red;
    }

    using var result = source.Upscale(Scale.X2);

    using var resultLocker = result.Lock();
    var corner = resultLocker[0, 0];
    var center = resultLocker[4, 4];

    Assert.That(corner.A, Is.EqualTo(0), "Corner should be transparent");
    Assert.That(center.A, Is.EqualTo(255), "Center should be opaque");
  }

  [Test]
  [Category("HappyPath")]
  public void Epx_TransparentPixels_PreservesTransparency() {
    using var source = new Bitmap(4, 4, PixelFormat.Format32bppArgb);
    using (var locker = source.Lock()) {
      locker.Clear(Color.Transparent);
      locker[1, 1] = Color.Blue;
      locker[2, 2] = Color.Blue;
    }

    using var result = source.Upscale(Epx.Default);

    using var resultLocker = result.Lock();
    var transparent = resultLocker[0, 0];

    Assert.That(transparent.A, Is.EqualTo(0), "Transparent pixels should remain transparent");
  }

  #endregion

  #region Eagle Tests

  [Test]
  [Category("HappyPath")]
  public void Eagle_Fast_OutputDimensionsAreDoubled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.Upscale(Eagle.Default, ScalerQuality.Fast);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Eagle_HighQuality_OutputDimensionsAreDoubled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.Upscale(Eagle.Default, ScalerQuality.HighQuality);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Eagle_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Purple);
    using var result = source.Upscale(Eagle.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Purple.R));
    Assert.That(centerColor.G, Is.EqualTo(Color.Purple.G));
    Assert.That(centerColor.B, Is.EqualTo(Color.Purple.B));
  }

  [Test]
  [Category("HappyPath")]
  public void Eagle_TestPattern_ScalesCorrectly() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.Upscale(Eagle.Default);

    Assert.That(result.Width, Is.EqualTo(40));
    Assert.That(result.Height, Is.EqualTo(40));
    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Eagle_1x1Bitmap_ScalesTo2x2() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Cyan);
    using var result = source.Upscale(Eagle.Default);

    Assert.That(result.Width, Is.EqualTo(2));
    Assert.That(result.Height, Is.EqualTo(2));
  }

  #endregion
  
  #region EPX-C Tests

  [Test]
  [Category("HappyPath")]
  public void EpxC_Fast_OutputDimensionsAreDoubled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Maroon);
    using var result = source.Upscale(EpxC.Default, ScalerQuality.Fast);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void EpxC_HighQuality_OutputDimensionsAreDoubled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Maroon);
    using var result = source.Upscale(EpxC.Default, ScalerQuality.HighQuality);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void EpxC_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Teal);
    using var result = source.Upscale(EpxC.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Teal.R).Within(1));
    Assert.That(centerColor.G, Is.EqualTo(Color.Teal.G).Within(1));
    Assert.That(centerColor.B, Is.EqualTo(Color.Teal.B).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void EpxC_TestPattern_ScalesCorrectly() {
    using var source = TestUtilities.CreateTestPattern(20, 20);
    using var result = source.Upscale(EpxC.Default);

    Assert.That(result.Width, Is.EqualTo(40));
    Assert.That(result.Height, Is.EqualTo(40));
  }

  [Test]
  [Category("EdgeCase")]
  public void EpxC_1x1Bitmap_ScalesTo2x2() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Olive);
    using var result = source.Upscale(EpxC.Default);

    Assert.That(result.Width, Is.EqualTo(2));
    Assert.That(result.Height, Is.EqualTo(2));
  }

  #endregion

  #region 2xSaI Tests

  [Test]
  [Category("HappyPath")]
  public void Sai2x_Fast_OutputDimensionsAreDoubled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Coral);
    using var result = source.Upscale(Sai2x.Default, ScalerQuality.Fast);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Sai2x_HighQuality_OutputDimensionsAreDoubled() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Coral);
    using var result = source.Upscale(Sai2x.Default, ScalerQuality.HighQuality);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Sai2x_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Gold);
    using var result = source.Upscale(Sai2x.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Gold.R).Within(1));
    Assert.That(centerColor.G, Is.EqualTo(Color.Gold.G).Within(1));
    Assert.That(centerColor.B, Is.EqualTo(Color.Gold.B).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Sai2x_Checkerboard_ProducesResult() {
    using var source = TestUtilities.CreateCheckerboard(8, 8, 2, Color.Black, Color.White);
    using var result = source.Upscale(Sai2x.Default);

    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Sai2x_1x1Bitmap_ScalesTo2x2() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Salmon);
    using var result = source.Upscale(Sai2x.Default);

    Assert.That(result.Width, Is.EqualTo(2));
    Assert.That(result.Height, Is.EqualTo(2));
  }

  #endregion
  
}
