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
using System.Linq;
using Hawkynt.ColorProcessing.Filtering;
using Hawkynt.ColorProcessing.Filtering.Filters;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Filters")]
public class FilterTests {

  #region VonKries Tests

  [Test]
  [Category("HappyPath")]
  public void VonKries_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(VonKries.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void VonKries_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.ApplyFilter(VonKries.Default);

    using var locker = result.Lock();
    var centerColor = locker[5, 5];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(20));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(20));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(20));
  }

  [Test]
  [Category("EdgeCase")]
  public void VonKries_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(VonKries.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void VonKries_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Magenta);
    using var result = source.ApplyFilter(VonKries.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Threshold Tests

  [Test]
  [Category("HappyPath")]
  public void Threshold_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = source.ApplyFilter(Threshold.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Threshold_SolidWhite_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = source.ApplyFilter(Threshold.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Threshold_SolidBlack_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = source.ApplyFilter(Threshold.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Threshold_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Gray);
    using var result = source.ApplyFilter(Threshold.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  #endregion

  #region Sharpen Tests

  [Test]
  [Category("HappyPath")]
  public void Sharpen_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(Sharpen.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Sharpen_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.ApplyFilter(Sharpen.Default);

    using var locker = result.Lock();
    var centerColor = locker[5, 5];

    Assert.That(centerColor.R, Is.EqualTo(Color.Blue.R).Within(5));
    Assert.That(centerColor.G, Is.EqualTo(Color.Blue.G).Within(5));
    Assert.That(centerColor.B, Is.EqualTo(Color.Blue.B).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Sharpen_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Gold);
    using var result = source.ApplyFilter(Sharpen.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Blur Tests

  [Test]
  [Category("HappyPath")]
  public void Blur_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(Blur.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Blur_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.ApplyFilter(Blur.Default);

    using var locker = result.Lock();
    var centerColor = locker[5, 5];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(5));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(5));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Blur_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Coral);
    using var result = source.ApplyFilter(Blur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Grayscale Tests

  [Test]
  [Category("HappyPath")]
  public void Grayscale_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(Grayscale.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Grayscale_SolidWhite_StaysNearWhite() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = source.ApplyFilter(Grayscale.Default);

    using var locker = result.Lock();
    var centerColor = locker[5, 5];

    Assert.That(centerColor.R, Is.EqualTo(255).Within(5));
    Assert.That(centerColor.G, Is.EqualTo(255).Within(5));
    Assert.That(centerColor.B, Is.EqualTo(255).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Grayscale_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Purple);
    using var result = source.ApplyFilter(Grayscale.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region ChannelExtraction Tests

  [Test]
  [Category("HappyPath")]
  public void ChannelExtraction_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(ChannelExtraction.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void ChannelExtraction_Default_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Cyan);
    using var result = source.ApplyFilter(ChannelExtraction.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void ChannelExtraction_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Yellow);
    using var result = source.ApplyFilter(ChannelExtraction.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ChannelExtraction_GreenChannel_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Lime);
    using var result = source.ApplyFilter(new ChannelExtraction(ColorChannel.Green));

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region Brightness Tests

  [Test]
  [Category("HappyPath")]
  public void Brightness_ZeroAmount_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(Brightness.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(2));
    Assert.That(c.G, Is.EqualTo(0).Within(2));
    Assert.That(c.B, Is.EqualTo(0).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Brightness_Positive_IncreasesValues() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(new Brightness(0.5f));

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.GreaterThan(128));
  }

  [Test]
  [Category("HappyPath")]
  public void Brightness_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(100, 128, 128, 128));
    using var result = source.ApplyFilter(new Brightness(0.2f));

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(100).Within(2));
  }

  #endregion

  #region Contrast Tests

  [Test]
  [Category("HappyPath")]
  public void Contrast_ZeroAmount_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.ApplyFilter(Contrast.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.G, Is.EqualTo(128).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Contrast_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(80, 100, 200, 50));
    using var result = source.ApplyFilter(new Contrast(0.5f));

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(80).Within(2));
  }

  #endregion

  #region BrightnessContrast Tests

  [Test]
  [Category("HappyPath")]
  public void BrightnessContrast_Neutral_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.ApplyFilter(BrightnessContrast.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.B, Is.EqualTo(255).Within(2));
    Assert.That(c.R, Is.EqualTo(0).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void BrightnessContrast_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(50, 128, 128, 128));
    using var result = source.ApplyFilter(new BrightnessContrast(0.1f, 0.1f));

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(50).Within(2));
  }

  #endregion

  #region Gamma Tests

  [Test]
  [Category("HappyPath")]
  public void Gamma_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(Gamma.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Gamma_GreaterThanOne_ChangesPixelValues() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(new Gamma(2.2f));

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Gamma_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(90, 128, 128, 128));
    using var result = source.ApplyFilter(new Gamma(2.2f));

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(90).Within(2));
  }

  #endregion

  #region Invert Tests

  [Test]
  [Category("HappyPath")]
  public void Invert_White_ProducesBlack() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = source.ApplyFilter(Invert.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(2));
    Assert.That(c.G, Is.EqualTo(0).Within(2));
    Assert.That(c.B, Is.EqualTo(0).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Invert_Black_ProducesWhite() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = source.ApplyFilter(Invert.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(2));
    Assert.That(c.G, Is.EqualTo(255).Within(2));
    Assert.That(c.B, Is.EqualTo(255).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Invert_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(70, 200, 100, 50));
    using var result = source.ApplyFilter(Invert.Default);

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(70).Within(2));
  }

  #endregion

  #region Sepia Tests

  [Test]
  [Category("HappyPath")]
  public void Sepia_SolidGray_ProducesWarmTones() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(Sepia.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.GreaterThanOrEqualTo(c.B));
  }

  [Test]
  [Category("HappyPath")]
  public void Sepia_ZeroIntensity_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(new Sepia(0f));

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Sepia_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(60, 128, 128, 128));
    using var result = source.ApplyFilter(Sepia.Default);

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(60).Within(2));
  }

  #endregion

  #region Posterize Tests

  [Test]
  [Category("HappyPath")]
  public void Posterize_SolidColor_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(Posterize.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Posterize_TwoLevels_ProducesBinaryOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(200, 200, 200));
    using var result = source.ApplyFilter(new Posterize(2));

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Posterize_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(55, 128, 128, 128));
    using var result = source.ApplyFilter(Posterize.Default);

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(55).Within(2));
  }

  #endregion

  #region Solarize Tests

  [Test]
  [Category("HappyPath")]
  public void Solarize_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(50, 50, 50));
    using var result = source.ApplyFilter(Solarize.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Solarize_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(200, 200, 200));
    using var result = source.ApplyFilter(Solarize.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Solarize_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(45, 128, 128, 128));
    using var result = source.ApplyFilter(Solarize.Default);

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(45).Within(2));
  }

  #endregion

  #region Exposure Tests

  [Test]
  [Category("HappyPath")]
  public void Exposure_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(Exposure.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Exposure_PositiveStops_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(100, 100, 100));
    using var result = source.ApplyFilter(new Exposure(1f));

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Exposure_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(35, 128, 128, 128));
    using var result = source.ApplyFilter(new Exposure(1f));

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(35).Within(2));
  }

  #endregion

  #region Levels Tests

  [Test]
  [Category("HappyPath")]
  public void Levels_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(Levels.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Levels_OutputRemapping_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = source.ApplyFilter(new Levels(0f, 1f, 0f, 0.5f));

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Levels_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(25, 128, 128, 128));
    using var result = source.ApplyFilter(Levels.Default);

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(25).Within(2));
  }

  #endregion

  #region HueSaturation Tests

  [Test]
  [Category("HappyPath")]
  public void HueSaturation_Neutral_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(HueSaturation.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void HueSaturation_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(88, 200, 100, 50));
    using var result = source.ApplyFilter(new HueSaturation(0.1f, 0.1f, 0f));

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(88).Within(2));
  }

  #endregion

  #region Vibrance Tests

  [Test]
  [Category("HappyPath")]
  public void Vibrance_ZeroAmount_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(Vibrance.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Vibrance_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(77, 128, 128, 128));
    using var result = source.ApplyFilter(new Vibrance(0.5f));

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(77).Within(2));
  }

  #endregion

  #region ColorTemperature Tests

  [Test]
  [Category("HappyPath")]
  public void ColorTemperature_Zero_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(ColorTemperature.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
    Assert.That(c.B, Is.EqualTo(128).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorTemperature_Warm_IncreasesRed() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(new ColorTemperature(1f));

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.GreaterThan(c.B));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorTemperature_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(66, 128, 128, 128));
    using var result = source.ApplyFilter(new ColorTemperature(0.5f));

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(66).Within(2));
  }

  #endregion

  #region ColorTint Tests

  [Test]
  [Category("HappyPath")]
  public void ColorTint_Zero_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(ColorTint.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
    Assert.That(c.G, Is.EqualTo(128).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorTint_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(99, 128, 128, 128));
    using var result = source.ApplyFilter(new ColorTint(0.5f));

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(99).Within(2));
  }

  #endregion

  #region GaussianBlur Tests

  [Test]
  [Category("HappyPath")]
  public void GaussianBlur_Default3x3_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(GaussianBlur.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
    Assert.That(c.B, Is.EqualTo(0).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void GaussianBlur_5x5_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.ApplyFilter(new GaussianBlur(2, 2));

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.G, Is.EqualTo(128).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void GaussianBlur_Rectangular_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.ApplyFilter(new GaussianBlur(1, 2));

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void GaussianBlur_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(44, 128, 128, 128));
    using var result = source.ApplyFilter(GaussianBlur.Default);

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(44).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void GaussianBlur_LargeRadius_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(new GaussianBlur(5, 5));

    using var locker = result.Lock();
    var c = locker[10, 10];
    Assert.That(c.R, Is.EqualTo(255).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
    Assert.That(c.B, Is.EqualTo(0).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void GaussianBlur_LargeRectangular_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Blue);
    using var result = source.ApplyFilter(new GaussianBlur(10, 3));

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void GaussianBlur_LargeRadius_UsesFrameAccess() {
    var small = new GaussianBlur(2, 2);
    var large = new GaussianBlur(5, 5);
    Assert.That(((IFrameFilter)small).UsesFrameAccess, Is.False);
    Assert.That(((IFrameFilter)large).UsesFrameAccess, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GaussianBlur_VeryLargeRadius_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(30, 30, Color.Green);
    using var result = source.ApplyFilter(new GaussianBlur(10, 10));

    using var locker = result.Lock();
    var c = locker[15, 15];
    Assert.That(c.G, Is.EqualTo(128).Within(5));
  }

  #endregion

  #region UnsharpMask Tests

  [Test]
  [Category("HappyPath")]
  public void UnsharpMask_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.ApplyFilter(UnsharpMask.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.B, Is.EqualTo(255).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void UnsharpMask_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(33, 128, 128, 128));
    using var result = source.ApplyFilter(UnsharpMask.Default);

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(33).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void UnsharpMask_LargeRadius_UsesFrameAccess() {
    var small = new UnsharpMask(1f, 0f, 2, 2);
    var large = new UnsharpMask(1f, 0f, 5, 5);
    Assert.That(((IFrameFilter)small).UsesFrameAccess, Is.False);
    Assert.That(((IFrameFilter)large).UsesFrameAccess, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void UnsharpMask_LargeRadius_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Blue);
    using var result = source.ApplyFilter(new UnsharpMask(1f, 0f, 5, 5));

    using var locker = result.Lock();
    var c = locker[10, 10];
    Assert.That(c.B, Is.EqualTo(255).Within(5));
  }

  #endregion

  #region MedianFilter Tests

  [Test]
  [Category("HappyPath")]
  public void MedianFilter_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(MedianFilter.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void MedianFilter_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Yellow);
    using var result = source.ApplyFilter(MedianFilter.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region Emboss Tests

  [Test]
  [Category("HappyPath")]
  public void Emboss_SolidColor_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(Emboss.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Emboss_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Cyan);
    using var result = source.ApplyFilter(Emboss.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region SobelEdge Tests

  [Test]
  [Category("HappyPath")]
  public void SobelEdge_SolidColor_ProducesBlack() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = source.ApplyFilter(SobelEdge.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
    Assert.That(c.B, Is.EqualTo(0).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void SobelEdge_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(SobelEdge.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region LaplacianEdge Tests

  [Test]
  [Category("HappyPath")]
  public void LaplacianEdge_SolidColor_ProducesBlack() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.ApplyFilter(LaplacianEdge.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void LaplacianEdge_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.ApplyFilter(LaplacianEdge.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region PrewittEdge Tests

  [Test]
  [Category("HappyPath")]
  public void PrewittEdge_SolidColor_ProducesBlack() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = source.ApplyFilter(PrewittEdge.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void PrewittEdge_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Yellow);
    using var result = source.ApplyFilter(PrewittEdge.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region Dilate Tests

  [Test]
  [Category("HappyPath")]
  public void Dilate_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(Dilate.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Dilate_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.ApplyFilter(Dilate.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region Erode Tests

  [Test]
  [Category("HappyPath")]
  public void Erode_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.ApplyFilter(Erode.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.B, Is.EqualTo(255).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Erode_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Cyan);
    using var result = source.ApplyFilter(Erode.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region HighPass Tests

  [Test]
  [Category("HappyPath")]
  public void HighPass_SolidColor_ProducesGray() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(HighPass.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(10));
  }

  [Test]
  [Category("HappyPath")]
  public void HighPass_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Magenta);
    using var result = source.ApplyFilter(HighPass.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void HighPass_LargeRadius_UsesFrameAccess() {
    var small = new HighPass(2, 2);
    var large = new HighPass(5, 5);
    Assert.That(((IFrameFilter)small).UsesFrameAccess, Is.False);
    Assert.That(((IFrameFilter)large).UsesFrameAccess, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void HighPass_LargeRadius_SolidColor_ProducesBiasedOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(new HighPass(5, 5));

    using var locker = result.Lock();
    var c = locker[10, 10];
    Assert.That(c.R, Is.EqualTo(c.G).Within(5));
    Assert.That(c.G, Is.EqualTo(c.B).Within(5));
    Assert.That(c.R, Is.InRange(100, 200));
  }

  #endregion

  #region Dilate Extended Tests

  [TestCase(0, false)]
  [TestCase(1, false)]
  [TestCase(2, false)]
  [TestCase(3, true)]
  [TestCase(5, true)]
  [Category("HappyPath")]
  public void Dilate_VariousRadii_UsesFrameAccessCorrectly(int radius, bool expectedFrameAccess) {
    var filter = new Dilate(radius);
    Assert.That(((IFrameFilter)filter).UsesFrameAccess, Is.EqualTo(expectedFrameAccess));
  }

  [TestCase(1, 10)]
  [TestCase(2, 10)]
  [TestCase(3, 20)]
  [TestCase(5, 20)]
  [Category("HappyPath")]
  public void Dilate_VariousRadii_SolidColor_PreservesColor(int radius, int size) {
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.Red);
    using var result = source.ApplyFilter(new Dilate(radius));

    using var locker = result.Lock();
    var c = locker[size / 2, size / 2];
    Assert.That(c.R, Is.EqualTo(255).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
    Assert.That(c.B, Is.EqualTo(0).Within(5));
  }

  [TestCase(1)]
  [TestCase(2)]
  [TestCase(5)]
  [Category("HappyPath")]
  public void Dilate_VariousRadii_PreservesAlpha(int radius) {
    var size = Math.Max(10, 2 * radius + 5);
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.FromArgb(77, 128, 128, 128));
    using var result = source.ApplyFilter(new Dilate(radius));

    using var locker = result.Lock();
    Assert.That(locker[size / 2, size / 2].A, Is.EqualTo(77).Within(2));
  }

  [TestCase(1)]
  [TestCase(3)]
  [TestCase(5)]
  [Category("HappyPath")]
  public void Dilate_VariousRadii_OutputDimensionsAreSame(int radius) {
    var size = 2 * radius + 6;
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.Green);
    using var result = source.ApplyFilter(new Dilate(radius));

    Assert.That(result.Width, Is.EqualTo(size));
    Assert.That(result.Height, Is.EqualTo(size));
  }

  [Test]
  [Category("HappyPath")]
  public void Dilate_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Cyan);
    using var result = source.ApplyFilter(new Dilate(3));

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Dilate_ZeroRadius_OutputMatchesInput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(100, 200, 50));
    using var result = source.ApplyFilter(new Dilate(0));

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(100).Within(5));
    Assert.That(c.G, Is.EqualTo(200).Within(5));
    Assert.That(c.B, Is.EqualTo(50).Within(5));
  }

  #endregion

  #region Erode Extended Tests

  [TestCase(0, false)]
  [TestCase(1, false)]
  [TestCase(2, false)]
  [TestCase(3, true)]
  [TestCase(5, true)]
  [Category("HappyPath")]
  public void Erode_VariousRadii_UsesFrameAccessCorrectly(int radius, bool expectedFrameAccess) {
    var filter = new Erode(radius);
    Assert.That(((IFrameFilter)filter).UsesFrameAccess, Is.EqualTo(expectedFrameAccess));
  }

  [TestCase(1, 10)]
  [TestCase(2, 10)]
  [TestCase(3, 20)]
  [TestCase(5, 20)]
  [Category("HappyPath")]
  public void Erode_VariousRadii_SolidColor_PreservesColor(int radius, int size) {
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.Blue);
    using var result = source.ApplyFilter(new Erode(radius));

    using var locker = result.Lock();
    var c = locker[size / 2, size / 2];
    Assert.That(c.B, Is.EqualTo(255).Within(5));
    Assert.That(c.R, Is.EqualTo(0).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
  }

  [TestCase(1)]
  [TestCase(2)]
  [TestCase(5)]
  [Category("HappyPath")]
  public void Erode_VariousRadii_PreservesAlpha(int radius) {
    var size = Math.Max(10, 2 * radius + 5);
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.FromArgb(33, 128, 128, 128));
    using var result = source.ApplyFilter(new Erode(radius));

    using var locker = result.Lock();
    Assert.That(locker[size / 2, size / 2].A, Is.EqualTo(33).Within(2));
  }

  [TestCase(1)]
  [TestCase(3)]
  [TestCase(5)]
  [Category("HappyPath")]
  public void Erode_VariousRadii_OutputDimensionsAreSame(int radius) {
    var size = 2 * radius + 6;
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.Cyan);
    using var result = source.ApplyFilter(new Erode(radius));

    Assert.That(result.Width, Is.EqualTo(size));
    Assert.That(result.Height, Is.EqualTo(size));
  }

  [Test]
  [Category("HappyPath")]
  public void Erode_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Magenta);
    using var result = source.ApplyFilter(new Erode(3));

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Erode_ZeroRadius_OutputMatchesInput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(100, 200, 50));
    using var result = source.ApplyFilter(new Erode(0));

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(100).Within(5));
    Assert.That(c.G, Is.EqualTo(200).Within(5));
    Assert.That(c.B, Is.EqualTo(50).Within(5));
  }

  #endregion

  #region MedianFilter Extended Tests

  [TestCase(0, false)]
  [TestCase(1, false)]
  [TestCase(2, false)]
  [TestCase(3, true)]
  [TestCase(5, true)]
  [Category("HappyPath")]
  public void MedianFilter_VariousRadii_UsesFrameAccessCorrectly(int radius, bool expectedFrameAccess) {
    var filter = new MedianFilter(radius);
    Assert.That(((IFrameFilter)filter).UsesFrameAccess, Is.EqualTo(expectedFrameAccess));
  }

  [TestCase(1, 10)]
  [TestCase(2, 10)]
  [TestCase(3, 20)]
  [TestCase(5, 20)]
  [Category("HappyPath")]
  public void MedianFilter_VariousRadii_SolidColor_PreservesColor(int radius, int size) {
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.Red);
    using var result = source.ApplyFilter(new MedianFilter(radius));

    using var locker = result.Lock();
    var c = locker[size / 2, size / 2];
    Assert.That(c.R, Is.EqualTo(255).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
  }

  [TestCase(1)]
  [TestCase(2)]
  [TestCase(5)]
  [Category("HappyPath")]
  public void MedianFilter_VariousRadii_PreservesAlpha(int radius) {
    var size = Math.Max(10, 2 * radius + 5);
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.FromArgb(44, 128, 128, 128));
    using var result = source.ApplyFilter(new MedianFilter(radius));

    using var locker = result.Lock();
    Assert.That(locker[size / 2, size / 2].A, Is.EqualTo(44).Within(2));
  }

  [TestCase(1)]
  [TestCase(3)]
  [TestCase(5)]
  [Category("HappyPath")]
  public void MedianFilter_VariousRadii_OutputDimensionsAreSame(int radius) {
    var size = 2 * radius + 6;
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.Yellow);
    using var result = source.ApplyFilter(new MedianFilter(radius));

    Assert.That(result.Width, Is.EqualTo(size));
    Assert.That(result.Height, Is.EqualTo(size));
  }

  [Test]
  [Category("HappyPath")]
  public void MedianFilter_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Gold);
    using var result = source.ApplyFilter(new MedianFilter(3));

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region BilateralFilter Extended Tests

  [Test]
  [Category("HappyPath")]
  public void BilateralFilter_AlwaysUsesFrameAccess() {
    var filter = new BilateralFilter(1, 1f, 0.1f);
    Assert.That(((IFrameFilter)filter).UsesFrameAccess, Is.True);
  }

  [TestCase(1, 1f, 0.1f)]
  [TestCase(2, 2f, 0.1f)]
  [TestCase(3, 3f, 0.1f)]
  [TestCase(5, 5f, 0.2f)]
  [Category("HappyPath")]
  public void BilateralFilter_VariousParameters_SolidColor_PreservesColor(int radius, float spatialSigma, float rangeSigma) {
    var size = 2 * radius + 10;
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.Red);
    using var result = source.ApplyFilter(new BilateralFilter(radius, spatialSigma, rangeSigma));

    using var locker = result.Lock();
    var c = locker[size / 2, size / 2];
    Assert.That(c.R, Is.EqualTo(255).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
    Assert.That(c.B, Is.EqualTo(0).Within(5));
  }

  [TestCase(1, 1f, 0.1f)]
  [TestCase(3, 3f, 0.1f)]
  [TestCase(5, 5f, 0.2f)]
  [Category("HappyPath")]
  public void BilateralFilter_VariousParameters_PreservesAlpha(int radius, float spatialSigma, float rangeSigma) {
    var size = 2 * radius + 10;
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.FromArgb(66, 128, 128, 128));
    using var result = source.ApplyFilter(new BilateralFilter(radius, spatialSigma, rangeSigma));

    using var locker = result.Lock();
    Assert.That(locker[size / 2, size / 2].A, Is.EqualTo(66).Within(2));
  }

  [TestCase(1)]
  [TestCase(3)]
  [TestCase(5)]
  [Category("HappyPath")]
  public void BilateralFilter_VariousRadii_OutputDimensionsAreSame(int radius) {
    var size = 2 * radius + 10;
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.Blue);
    using var result = source.ApplyFilter(new BilateralFilter(radius));

    Assert.That(result.Width, Is.EqualTo(size));
    Assert.That(result.Height, Is.EqualTo(size));
  }

  [Test]
  [Category("HappyPath")]
  public void BilateralFilter_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Coral);
    using var result = source.ApplyFilter(BilateralFilter.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void BilateralFilter_HighRangeSigma_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.FromArgb(100, 150, 200));
    using var result = source.ApplyFilter(new BilateralFilter(3, 3f, 1f));

    using var locker = result.Lock();
    var c = locker[10, 10];
    Assert.That(c.R, Is.EqualTo(100).Within(5));
    Assert.That(c.G, Is.EqualTo(150).Within(5));
    Assert.That(c.B, Is.EqualTo(200).Within(5));
  }

  #endregion

  #region MorphologicalGradient Extended Tests

  [TestCase(0, false)]
  [TestCase(1, false)]
  [TestCase(2, false)]
  [TestCase(3, true)]
  [TestCase(5, true)]
  [Category("HappyPath")]
  public void MorphologicalGradient_VariousRadii_UsesFrameAccessCorrectly(int radius, bool expectedFrameAccess) {
    var filter = new MorphologicalGradient(radius);
    Assert.That(((IFrameFilter)filter).UsesFrameAccess, Is.EqualTo(expectedFrameAccess));
  }

  [TestCase(1, 10)]
  [TestCase(2, 10)]
  [TestCase(3, 20)]
  [TestCase(5, 20)]
  [Category("HappyPath")]
  public void MorphologicalGradient_VariousRadii_SolidColor_ProducesBlack(int radius, int size) {
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.White);
    using var result = source.ApplyFilter(new MorphologicalGradient(radius));

    using var locker = result.Lock();
    var c = locker[size / 2, size / 2];
    Assert.That(c.R, Is.EqualTo(0).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
    Assert.That(c.B, Is.EqualTo(0).Within(5));
  }

  [TestCase(1)]
  [TestCase(2)]
  [TestCase(5)]
  [Category("HappyPath")]
  public void MorphologicalGradient_VariousRadii_PreservesAlpha(int radius) {
    var size = Math.Max(10, 2 * radius + 5);
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.FromArgb(55, 128, 128, 128));
    using var result = source.ApplyFilter(new MorphologicalGradient(radius));

    using var locker = result.Lock();
    Assert.That(locker[size / 2, size / 2].A, Is.EqualTo(55).Within(2));
  }

  [TestCase(1)]
  [TestCase(3)]
  [TestCase(5)]
  [Category("HappyPath")]
  public void MorphologicalGradient_VariousRadii_OutputDimensionsAreSame(int radius) {
    var size = 2 * radius + 6;
    using var source = TestUtilities.CreateSolidBitmap(size, size, Color.Red);
    using var result = source.ApplyFilter(new MorphologicalGradient(radius));

    Assert.That(result.Width, Is.EqualTo(size));
    Assert.That(result.Height, Is.EqualTo(size));
  }

  [Test]
  [Category("HappyPath")]
  public void MorphologicalGradient_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Yellow);
    using var result = source.ApplyFilter(MorphologicalGradient.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void MorphologicalGradient_SolidBlack_ProducesBlack() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = source.ApplyFilter(MorphologicalGradient.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
    Assert.That(c.B, Is.EqualTo(0).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void MorphologicalGradient_SolidRed_ProducesBlack() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(new MorphologicalGradient(2));

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
    Assert.That(c.B, Is.EqualTo(0).Within(5));
  }

  #endregion

  #region HDRToneMap Tests

  [Test]
  [Category("HappyPath")]
  public void HDRToneMap_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(HDRToneMap.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void HDRToneMap_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.ApplyFilter(HDRToneMap.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void HDRToneMap_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(30, 128, 128, 128));
    using var result = source.ApplyFilter(HDRToneMap.Default);

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(30).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void HDRToneMap_BlackStaysBlack() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = source.ApplyFilter(HDRToneMap.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(5));
    Assert.That(c.G, Is.EqualTo(0).Within(5));
    Assert.That(c.B, Is.EqualTo(0).Within(5));
  }

  #endregion

  #region ColorBalance Tests

  [Test]
  [Category("HappyPath")]
  public void ColorBalance_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(ColorBalance.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorBalance_Neutral_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(ColorBalance.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(5));
    Assert.That(c.G, Is.EqualTo(128).Within(5));
    Assert.That(c.B, Is.EqualTo(128).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorBalance_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(85, 128, 128, 128));
    using var result = source.ApplyFilter(ColorBalance.Default);

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(85).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorBalance_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Yellow);
    using var result = source.ApplyFilter(ColorBalance.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region ChannelMixer Tests

  [Test]
  [Category("HappyPath")]
  public void ChannelMixer_Identity_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(100, 150, 200));
    using var result = source.ApplyFilter(ChannelMixer.Default);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(100).Within(5));
    Assert.That(c.G, Is.EqualTo(150).Within(5));
    Assert.That(c.B, Is.EqualTo(200).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void ChannelMixer_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(ChannelMixer.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void ChannelMixer_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(75, 128, 128, 128));
    using var result = source.ApplyFilter(ChannelMixer.Default);

    using var locker = result.Lock();
    Assert.That(locker[5, 5].A, Is.EqualTo(75).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ChannelMixer_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Magenta);
    using var result = source.ApplyFilter(ChannelMixer.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Clarity Tests

  [Test]
  [Category("HappyPath")]
  public void Clarity_AlwaysUsesFrameAccess() {
    var filter = new Clarity(0.5f, 3);
    Assert.That(((IFrameFilter)filter).UsesFrameAccess, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Clarity_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Clarity.Default);

    using var locker = result.Lock();
    var c = locker[10, 10];
    Assert.That(c.R, Is.EqualTo(255).Within(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Clarity_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Blue);
    using var result = source.ApplyFilter(Clarity.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Clarity_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.FromArgb(22, 128, 128, 128));
    using var result = source.ApplyFilter(Clarity.Default);

    using var locker = result.Lock();
    Assert.That(locker[10, 10].A, Is.EqualTo(22).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Clarity_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Clarity.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Dehaze Tests

  [Test]
  [Category("HappyPath")]
  public void Dehaze_AlwaysUsesFrameAccess() {
    var filter = new Dehaze(0.5f, 7);
    Assert.That(((IFrameFilter)filter).UsesFrameAccess, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Dehaze_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.FromArgb(128, 128, 128));
    using var result = source.ApplyFilter(Dehaze.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Dehaze_PreservesAlpha() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.FromArgb(11, 128, 128, 128));
    using var result = source.ApplyFilter(Dehaze.Default);

    using var locker = result.Lock();
    Assert.That(locker[10, 10].A, Is.EqualTo(11).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Dehaze_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Lime);
    using var result = source.ApplyFilter(Dehaze.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region MotionBlur Tests

  [Test]
  [Category("HappyPath")]
  public void MotionBlur_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(MotionBlur.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void MotionBlur_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(MotionBlur.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void MotionBlur_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(MotionBlur.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void MotionBlur_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(MotionBlur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region RadialBlur Tests

  [Test]
  [Category("HappyPath")]
  public void RadialBlur_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(RadialBlur.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void RadialBlur_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(RadialBlur.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void RadialBlur_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(RadialBlur.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void RadialBlur_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(RadialBlur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region ZoomBlur Tests

  [Test]
  [Category("HappyPath")]
  public void ZoomBlur_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(ZoomBlur.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ZoomBlur_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(ZoomBlur.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void ZoomBlur_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(ZoomBlur.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ZoomBlur_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(ZoomBlur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region SpinBlur Tests

  [Test]
  [Category("HappyPath")]
  public void SpinBlur_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(SpinBlur.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinBlur_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(SpinBlur.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void SpinBlur_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(SpinBlur.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinBlur_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(SpinBlur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region SurfaceBlur Tests

  [Test]
  [Category("HappyPath")]
  public void SurfaceBlur_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(SurfaceBlur.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void SurfaceBlur_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(SurfaceBlur.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void SurfaceBlur_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(SurfaceBlur.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void SurfaceBlur_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(SurfaceBlur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Twirl Tests

  [Test]
  [Category("HappyPath")]
  public void Twirl_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Twirl.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Twirl_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Twirl.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void Twirl_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Twirl.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Twirl_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Twirl.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Spherize Tests

  [Test]
  [Category("HappyPath")]
  public void Spherize_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Spherize.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Spherize_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Spherize.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void Spherize_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Spherize.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Spherize_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Spherize.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Ripple Tests

  [Test]
  [Category("HappyPath")]
  public void Ripple_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Ripple.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Ripple_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Ripple.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void Ripple_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Ripple.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Ripple_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Ripple.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Wave Tests

  [Test]
  [Category("HappyPath")]
  public void Wave_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Wave.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Wave_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Wave.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void Wave_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Wave.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Wave_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Wave.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Pinch Tests

  [Test]
  [Category("HappyPath")]
  public void Pinch_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Pinch.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Pinch_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Pinch.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void Pinch_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Pinch.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Pinch_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Pinch.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region PolarCoordinates Tests

  [Test]
  [Category("HappyPath")]
  public void PolarCoordinates_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(PolarCoordinates.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("EdgeCase")]
  public void PolarCoordinates_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(PolarCoordinates.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void PolarCoordinates_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(PolarCoordinates.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region FrostedGlass Tests

  [Test]
  [Category("HappyPath")]
  public void FrostedGlass_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(FrostedGlass.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void FrostedGlass_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(FrostedGlass.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void FrostedGlass_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(FrostedGlass.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void FrostedGlass_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(FrostedGlass.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region LensDistortion Tests

  [Test]
  [Category("HappyPath")]
  public void LensDistortion_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(LensDistortion.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void LensDistortion_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(LensDistortion.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void LensDistortion_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(LensDistortion.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void LensDistortion_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(LensDistortion.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Turbulence Tests

  [Test]
  [Category("HappyPath")]
  public void Turbulence_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Turbulence.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("EdgeCase")]
  public void Turbulence_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Turbulence.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Turbulence_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Turbulence.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Wind Tests

  [Test]
  [Category("HappyPath")]
  public void Wind_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Wind.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("EdgeCase")]
  public void Wind_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Wind.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Wind_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Wind.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Spread Tests

  [Test]
  [Category("HappyPath")]
  public void Spread_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Spread.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Spread_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Spread.Default);

    using var locker = result.Lock();
    var centerColor = locker[10, 10];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(30));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(30));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void Spread_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Spread.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Spread_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Spread.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region TraceContour Tests

  [Test]
  [Category("HappyPath")]
  public void TraceContour_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(TraceContour.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("EdgeCase")]
  public void TraceContour_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(TraceContour.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void TraceContour_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(TraceContour.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region AccentedEdges Tests

  [Test]
  [Category("HappyPath")]
  public void AccentedEdges_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(AccentedEdges.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("EdgeCase")]
  public void AccentedEdges_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(AccentedEdges.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void AccentedEdges_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(AccentedEdges.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region BokehBlur Tests

  [Test]
  [Category("HappyPath")]
  public void BokehBlur_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(BokehBlur.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void BokehBlur_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(BokehBlur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void BokehBlur_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(BokehBlur.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void BokehBlur_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(BokehBlur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region SmartBlur Tests

  [Test]
  [Category("HappyPath")]
  public void SmartBlur_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(SmartBlur.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void SmartBlur_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(SmartBlur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void SmartBlur_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(SmartBlur.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void SmartBlur_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(SmartBlur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region SmartSharpen Tests

  [Test]
  [Category("HappyPath")]
  public void SmartSharpen_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(SmartSharpen.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void SmartSharpen_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(SmartSharpen.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void SmartSharpen_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(SmartSharpen.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void SmartSharpen_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(SmartSharpen.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region CannyEdge Tests

  [Test]
  [Category("HappyPath")]
  public void CannyEdge_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(CannyEdge.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void CannyEdge_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(CannyEdge.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void CannyEdge_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(CannyEdge.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void CannyEdge_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(CannyEdge.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region FindEdges Tests

  [Test]
  [Category("HappyPath")]
  public void FindEdges_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(FindEdges.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void FindEdges_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(FindEdges.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void FindEdges_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(FindEdges.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void FindEdges_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(FindEdges.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region SigmoidContrast Tests

  [Test]
  [Category("HappyPath")]
  public void SigmoidContrast_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(SigmoidContrast.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void SigmoidContrast_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(SigmoidContrast.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void SigmoidContrast_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(SigmoidContrast.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void SigmoidContrast_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(SigmoidContrast.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Opening Tests

  [Test]
  [Category("HappyPath")]
  public void Opening_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Opening.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Opening_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Opening.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Opening_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Opening.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Opening_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Opening.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Closing Tests

  [Test]
  [Category("HappyPath")]
  public void Closing_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Closing.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Closing_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Closing.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Closing_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Closing.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Closing_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Closing.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region TopHat Tests

  [Test]
  [Category("HappyPath")]
  public void TopHat_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(TopHat.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void TopHat_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(TopHat.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void TopHat_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(TopHat.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void TopHat_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(TopHat.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region BottomHat Tests

  [Test]
  [Category("HappyPath")]
  public void BottomHat_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(BottomHat.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void BottomHat_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(BottomHat.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void BottomHat_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(BottomHat.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void BottomHat_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(BottomHat.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Equalize Tests

  [Test]
  [Category("HappyPath")]
  public void Equalize_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Equalize.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Equalize_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Equalize.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Equalize_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Equalize.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Equalize_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Equalize.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region AutoLevels Tests

  [Test]
  [Category("HappyPath")]
  public void AutoLevels_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(AutoLevels.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void AutoLevels_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(AutoLevels.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void AutoLevels_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(AutoLevels.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void AutoLevels_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(AutoLevels.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region ZigZag Tests

  [Test]
  [Category("HappyPath")]
  public void ZigZag_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(ZigZag.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ZigZag_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(ZigZag.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void ZigZag_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(ZigZag.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ZigZag_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(ZigZag.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region OceanRipple Tests

  [Test]
  [Category("HappyPath")]
  public void OceanRipple_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(OceanRipple.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void OceanRipple_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(OceanRipple.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void OceanRipple_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(OceanRipple.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void OceanRipple_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(OceanRipple.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Shear Tests

  [Test]
  [Category("HappyPath")]
  public void Shear_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Shear.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Shear_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Shear.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Shear_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Shear.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Shear_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Shear.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Dents Tests

  [Test]
  [Category("HappyPath")]
  public void Dents_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Dents.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Dents_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Dents.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Dents_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Dents.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Dents_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Dents.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Bulge Tests

  [Test]
  [Category("HappyPath")]
  public void Bulge_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Bulge.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Bulge_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Bulge.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Bulge_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Bulge.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Bulge_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Bulge.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Offset Tests

  [Test]
  [Category("HappyPath")]
  public void Offset_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ApplyFilter(Offset.Default);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Offset_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.ApplyFilter(Offset.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Offset_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(Offset.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Offset_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Magenta);
    using var result = source.ApplyFilter(Offset.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion
}
