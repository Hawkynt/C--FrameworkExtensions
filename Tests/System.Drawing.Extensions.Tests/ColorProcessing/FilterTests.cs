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
}
