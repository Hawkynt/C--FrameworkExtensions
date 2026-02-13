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
using Hawkynt.ColorProcessing.Blending;
using Hawkynt.ColorProcessing.Blending.BlendModes;
using Hawkynt.ColorProcessing.Filtering.Filters;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Blending")]
public class BlendTests {

  #region Normal Blend Tests

  [Test]
  [Category("HappyPath")]
  public void Normal_SolidColors_OutputDimensionsAreSame() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<Normal>(fg);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Normal_FullStrength_ReplacesForeground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<Normal>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(1));
    Assert.That(c.G, Is.EqualTo(0).Within(1));
    Assert.That(c.B, Is.EqualTo(255).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Normal_ZeroStrength_ReturnsOriginal() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<Normal>(fg, 0f);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(1));
    Assert.That(c.G, Is.EqualTo(0).Within(1));
    Assert.That(c.B, Is.EqualTo(0).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Normal_HalfStrength_BlendsMidway() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 0, 0, 0));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 255, 255, 255));
    using var result = bg.BlendWith<Normal>(fg, 0.5f);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
    Assert.That(c.G, Is.EqualTo(128).Within(2));
    Assert.That(c.B, Is.EqualTo(128).Within(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void Normal_1x1Bitmap_ProducesOutput() {
    using var bg = TestUtilities.CreateSolidBitmap(1, 1, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(1, 1, Color.Green);
    using var result = bg.BlendWith<Normal>(fg);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Normal_OutputIsFormat32bppArgb() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<Normal>(fg);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Multiply Blend Tests

  [Test]
  [Category("HappyPath")]
  public void Multiply_WhiteForeground_ReturnsBackground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 64, 200));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = bg.BlendWith<Multiply>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
    Assert.That(c.G, Is.EqualTo(64).Within(2));
    Assert.That(c.B, Is.EqualTo(200).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Multiply_BlackForeground_ReturnsBlack() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 200, 150, 100));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = bg.BlendWith<Multiply>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(1));
    Assert.That(c.G, Is.EqualTo(0).Within(1));
    Assert.That(c.B, Is.EqualTo(0).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Multiply_SameColor_SquaresChannels() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var result = bg.BlendWith<Multiply>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    var expected = (byte)(128 * 128 / 255);
    Assert.That(c.R, Is.EqualTo(expected).Within(2));
  }

  #endregion

  #region Screen Blend Tests

  [Test]
  [Category("HappyPath")]
  public void Screen_BlackForeground_ReturnsBackground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 64, 200));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = bg.BlendWith<Screen>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
    Assert.That(c.G, Is.EqualTo(64).Within(2));
    Assert.That(c.B, Is.EqualTo(200).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Screen_WhiteForeground_ReturnsWhite() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 100, 100));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = bg.BlendWith<Screen>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(1));
    Assert.That(c.G, Is.EqualTo(255).Within(1));
    Assert.That(c.B, Is.EqualTo(255).Within(1));
  }

  #endregion

  #region Overlay Blend Tests

  [Test]
  [Category("HappyPath")]
  public void Overlay_MidGrayForeground_PreservesBackground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 150, 200));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var result = bg.BlendWith<Overlay>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(100).Within(10));
    Assert.That(c.G, Is.EqualTo(150).Within(10));
    Assert.That(c.B, Is.EqualTo(200).Within(10));
  }

  #endregion

  #region Darken/Lighten Blend Tests

  [Test]
  [Category("HappyPath")]
  public void Darken_TakesDarkerChannel() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 200, 100, 150));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 200, 150));
    using var result = bg.BlendWith<Darken>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(100).Within(2));
    Assert.That(c.G, Is.EqualTo(100).Within(2));
    Assert.That(c.B, Is.EqualTo(150).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Lighten_TakesLighterChannel() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 200, 100, 150));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 200, 150));
    using var result = bg.BlendWith<Lighten>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(200).Within(2));
    Assert.That(c.G, Is.EqualTo(200).Within(2));
    Assert.That(c.B, Is.EqualTo(150).Within(2));
  }

  #endregion

  #region Difference/Exclusion Tests

  [Test]
  [Category("HappyPath")]
  public void Difference_SameColor_ReturnsBlack() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var result = bg.BlendWith<Difference>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(1));
    Assert.That(c.G, Is.EqualTo(0).Within(1));
    Assert.That(c.B, Is.EqualTo(0).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Difference_BlackAndWhite_ReturnsWhite() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = bg.BlendWith<Difference>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Exclusion_BlackForeground_ReturnsBackground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 150, 200));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = bg.BlendWith<Exclusion>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(100).Within(2));
    Assert.That(c.G, Is.EqualTo(150).Within(2));
    Assert.That(c.B, Is.EqualTo(200).Within(2));
  }

  #endregion

  #region Add/Subtract Tests

  [Test]
  [Category("HappyPath")]
  public void Add_BlackForeground_ReturnsBackground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 150, 200));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = bg.BlendWith<Add>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(100).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Add_ClampsAt255() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 200, 200, 200));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 200, 200, 200));
    using var result = bg.BlendWith<Add>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Subtract_WhiteForeground_ReturnsBlack() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 100, 100));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = bg.BlendWith<Subtract>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(1));
  }

  #endregion

  #region ColorDodge/ColorBurn Tests

  [Test]
  [Category("HappyPath")]
  public void ColorDodge_BlackForeground_ReturnsBackground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = bg.BlendWith<ColorDodge>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorDodge_WhiteForeground_ReturnsWhite() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = bg.BlendWith<ColorDodge>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorBurn_WhiteForeground_ReturnsBackground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = bg.BlendWith<ColorBurn>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorBurn_BlackForeground_ReturnsBlack() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = bg.BlendWith<ColorBurn>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(1));
  }

  #endregion

  #region LinearBurn/LinearDodge Tests

  [Test]
  [Category("HappyPath")]
  public void LinearBurn_WhiteForeground_ReturnsBackground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = bg.BlendWith<LinearBurn>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void LinearDodge_BlackForeground_ReturnsBackground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = bg.BlendWith<LinearDodge>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
  }

  #endregion

  #region HardLight/SoftLight Tests

  [Test]
  [Category("HappyPath")]
  public void HardLight_OutputDimensionsAreSame() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<HardLight>(fg);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void SoftLight_OutputDimensionsAreSame() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<SoftLight>(fg);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region VividLight/LinearLight/PinLight/HardMix Tests

  [Test]
  [Category("HappyPath")]
  public void VividLight_OutputDimensionsAreSame() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var result = bg.BlendWith<VividLight>(fg);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void LinearLight_OutputDimensionsAreSame() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var result = bg.BlendWith<LinearLight>(fg);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void PinLight_OutputDimensionsAreSame() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 64, 64, 64));
    using var result = bg.BlendWith<PinLight>(fg);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void HardMix_BlackAndWhite_ReturnsBlack() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = bg.BlendWith<HardMix>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(0).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void HardMix_WhiteAndWhite_ReturnsWhite() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = bg.BlendWith<HardMix>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(1));
  }

  #endregion

  #region GrainExtract/GrainMerge Tests

  [Test]
  [Category("HappyPath")]
  public void GrainExtract_SameColor_ReturnsMidGray() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var result = bg.BlendWith<GrainExtract>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void GrainMerge_SameColor_ReturnsMidGray() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 64, 64, 64));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 64, 64, 64));
    using var result = bg.BlendWith<GrainMerge>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    var expected = (byte)Math.Max(0, Math.Min(255, 64 + 64 - 128));
    Assert.That(c.R, Is.EqualTo(expected).Within(2));
  }

  #endregion

  #region Divide Tests

  [Test]
  [Category("HappyPath")]
  public void Divide_WhiteForeground_ReturnsBackground() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = bg.BlendWith<Divide>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void Divide_BlackForeground_ReturnsWhite() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = bg.BlendWith<Divide>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(255).Within(1));
  }

  #endregion

  #region HSL Component Blend Tests

  [Test]
  [Category("HappyPath")]
  public void HueBlend_OutputDimensionsAreSame() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<HueBlend>(fg);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void SaturationBlend_OutputDimensionsAreSame() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<SaturationBlend>(fg);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorBlend_OutputDimensionsAreSame() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<ColorBlend>(fg);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void LuminosityBlend_OutputDimensionsAreSame() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<LuminosityBlend>(fg);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void HueBlend_GrayForeground_PreservesBackgroundColor() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    using var result = bg.BlendWith<HueBlend>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void LuminosityBlend_SameColor_PreservesOriginal() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 150, 200));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 150, 200));
    using var result = bg.BlendWith<LuminosityBlend>(fg);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(100).Within(5));
    Assert.That(c.G, Is.EqualTo(150).Within(5));
    Assert.That(c.B, Is.EqualTo(200).Within(5));
  }

  #endregion

  #region Strength Parameter Tests

  [Test]
  [Category("HappyPath")]
  public void Multiply_ZeroStrength_ReturnsOriginal() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 200, 100, 50));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = bg.BlendWith<Multiply>(fg, 0f);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(200).Within(2));
    Assert.That(c.G, Is.EqualTo(100).Within(2));
    Assert.That(c.B, Is.EqualTo(50).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Screen_ZeroStrength_ReturnsOriginal() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 150, 200));
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = bg.BlendWith<Screen>(fg, 0f);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(100).Within(2));
    Assert.That(c.G, Is.EqualTo(150).Within(2));
    Assert.That(c.B, Is.EqualTo(200).Within(2));
  }

  #endregion

  #region Size Mismatch Tests

  [Test]
  [Category("EdgeCase")]
  public void Normal_DifferentSizes_UsesIntersection() {
    using var bg = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = bg.BlendWith<Normal>(fg);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));

    using var locker = result.Lock();
    var blended = locker[5, 5];
    Assert.That(blended.B, Is.EqualTo(255).Within(1));

    var unblended = locker[15, 15];
    Assert.That(unblended.R, Is.EqualTo(255).Within(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Normal_OverlaySmallerThanBackground_BackgroundPreservedOutside() {
    using var bg = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var fg = TestUtilities.CreateSolidBitmap(5, 5, Color.White);
    using var result = bg.BlendWith<Normal>(fg);

    using var locker = result.Lock();
    var outside = locker[10, 10];
    Assert.That(outside.R, Is.EqualTo(0).Within(1));
    Assert.That(outside.G, Is.EqualTo(128).Within(2));
  }

  #endregion

  #region BlendInto (In-Place) Tests

  [Test]
  [Category("HappyPath")]
  public void BlendInto_Normal_ModifiesInPlace() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    bg.BlendInto<Normal>(fg);

    using var locker = bg.Lock();
    var c = locker[5, 5];
    Assert.That(c.B, Is.EqualTo(255).Within(1));
    Assert.That(c.R, Is.EqualTo(0).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendInto_Multiply_ModifiesInPlace() {
    using var bg = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var fg = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 128, 128, 128));
    bg.BlendInto<Multiply>(fg);

    using var locker = bg.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(128).Within(2));
  }

  #endregion

  #region Filter-Blend Overload Tests

  [Test]
  [Category("HappyPath")]
  public void BlendWithFilter_Normal_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.BlendWith(Invert.Default, 0.5f);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendWithFilter_ZeroStrength_ReturnsOriginal() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 200, 100, 50));
    using var result = source.BlendWith(Invert.Default, 0f);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(200).Within(2));
    Assert.That(c.G, Is.EqualTo(100).Within(2));
    Assert.That(c.B, Is.EqualTo(50).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendWithFilter_FullStrength_ReturnsFilterResult() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 200, 100, 50));
    using var result = source.BlendWith(Invert.Default, 1f);

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.R, Is.EqualTo(55).Within(5));
    Assert.That(c.G, Is.EqualTo(155).Within(5));
    Assert.That(c.B, Is.EqualTo(205).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendWithFilter_CustomMode_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.BlendWith<Multiply, Brightness>(new Brightness(0.1f), 0.5f);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion

  #region BlendModeRegistry Tests

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_All_Discovers27Modes() {
    var modes = BlendModeRegistry.All;

    Assert.That(modes.Count, Is.EqualTo(27));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_FindByName_FindsMultiply() {
    var desc = BlendModeRegistry.FindByName("Multiply");

    Assert.That(desc, Is.Not.Null);
    Assert.That(desc.Name, Is.EqualTo("Multiply"));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_FindByName_FindsNormal() {
    var desc = BlendModeRegistry.FindByName("Normal");

    Assert.That(desc, Is.Not.Null);
    Assert.That(desc.Category, Is.EqualTo(BlendModeCategory.Normal));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_GetByCategory_FindsDarken() {
    var modes = BlendModeRegistry.GetByCategory(BlendModeCategory.Darken).ToList();

    Assert.That(modes.Count, Is.GreaterThanOrEqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_GetByCategory_FindsLighten() {
    var modes = BlendModeRegistry.GetByCategory(BlendModeCategory.Lighten).ToList();

    Assert.That(modes.Count, Is.GreaterThanOrEqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_GetByCategory_FindsContrast() {
    var modes = BlendModeRegistry.GetByCategory(BlendModeCategory.Contrast).ToList();

    Assert.That(modes.Count, Is.GreaterThanOrEqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_GetByCategory_FindsInversion() {
    var modes = BlendModeRegistry.GetByCategory(BlendModeCategory.Inversion).ToList();

    Assert.That(modes.Count, Is.GreaterThanOrEqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_GetByCategory_FindsComponent() {
    var modes = BlendModeRegistry.GetByCategory(BlendModeCategory.Component).ToList();

    Assert.That(modes.Count, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_GetByCategory_FindsOther() {
    var modes = BlendModeRegistry.GetByCategory(BlendModeCategory.Other).ToList();

    Assert.That(modes.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_AllModes_CanBeFoundByName() {
    var expectedNames = new[] {
      "Normal", "Multiply", "Screen", "Overlay", "SoftLight", "HardLight",
      "ColorDodge", "ColorBurn", "Darken", "Lighten", "Difference", "Exclusion",
      "Add", "Subtract", "Divide", "LinearBurn", "LinearDodge",
      "LinearLight", "VividLight", "PinLight", "HardMix",
      "GrainExtract", "GrainMerge",
      "Hue", "Saturation", "Color", "Luminosity"
    };

    foreach (var name in expectedNames) {
      var desc = BlendModeRegistry.FindByName(name);
      Assert.That(desc, Is.Not.Null, $"Blend mode '{name}' not found in registry");
    }
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_ComponentModes_AreFullPixel() {
    var componentModes = BlendModeRegistry.GetByCategory(BlendModeCategory.Component).ToList();

    foreach (var mode in componentModes)
      Assert.That(mode.IsFullPixelMode, Is.True, $"Mode '{mode.Name}' should be full-pixel");
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeRegistry_PerChannelModes_AreNotFullPixel() {
    var desc = BlendModeRegistry.FindByName("Multiply");

    Assert.That(desc, Is.Not.Null);
    Assert.That(desc.IsFullPixelMode, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void BlendModeDescriptor_CreateDefault_ReturnsInstance() {
    var desc = BlendModeRegistry.FindByName("Screen");

    Assert.That(desc, Is.Not.Null);
    var instance = desc.CreateDefault();
    Assert.That(instance, Is.Not.Null);
    Assert.That(instance, Is.InstanceOf<IBlendMode>());
  }

  #endregion

  #region All Modes Smoke Tests

  [Test]
  [Category("HappyPath")]
  public void AllPerChannelModes_SolidColors_ProduceValidOutput() {
    using var bg = TestUtilities.CreateSolidBitmap(5, 5, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(5, 5, Color.FromArgb(255, 64, 192, 128));

    AssertBlendProducesValidOutput<Normal>(bg, fg);
    AssertBlendProducesValidOutput<Multiply>(bg, fg);
    AssertBlendProducesValidOutput<Screen>(bg, fg);
    AssertBlendProducesValidOutput<Overlay>(bg, fg);
    AssertBlendProducesValidOutput<SoftLight>(bg, fg);
    AssertBlendProducesValidOutput<HardLight>(bg, fg);
    AssertBlendProducesValidOutput<ColorDodge>(bg, fg);
    AssertBlendProducesValidOutput<ColorBurn>(bg, fg);
    AssertBlendProducesValidOutput<Darken>(bg, fg);
    AssertBlendProducesValidOutput<Lighten>(bg, fg);
    AssertBlendProducesValidOutput<Difference>(bg, fg);
    AssertBlendProducesValidOutput<Exclusion>(bg, fg);
    AssertBlendProducesValidOutput<Add>(bg, fg);
    AssertBlendProducesValidOutput<Subtract>(bg, fg);
    AssertBlendProducesValidOutput<Divide>(bg, fg);
    AssertBlendProducesValidOutput<LinearBurn>(bg, fg);
    AssertBlendProducesValidOutput<LinearDodge>(bg, fg);
    AssertBlendProducesValidOutput<LinearLight>(bg, fg);
    AssertBlendProducesValidOutput<VividLight>(bg, fg);
    AssertBlendProducesValidOutput<PinLight>(bg, fg);
    AssertBlendProducesValidOutput<HardMix>(bg, fg);
    AssertBlendProducesValidOutput<GrainExtract>(bg, fg);
    AssertBlendProducesValidOutput<GrainMerge>(bg, fg);
  }

  [Test]
  [Category("HappyPath")]
  public void AllFullPixelModes_SolidColors_ProduceValidOutput() {
    using var bg = TestUtilities.CreateSolidBitmap(5, 5, Color.FromArgb(255, 128, 128, 128));
    using var fg = TestUtilities.CreateSolidBitmap(5, 5, Color.FromArgb(255, 64, 192, 128));

    AssertBlendProducesValidOutput<HueBlend>(bg, fg);
    AssertBlendProducesValidOutput<SaturationBlend>(bg, fg);
    AssertBlendProducesValidOutput<ColorBlend>(bg, fg);
    AssertBlendProducesValidOutput<LuminosityBlend>(bg, fg);
  }

  private static void AssertBlendProducesValidOutput<TMode>(Bitmap bg, Bitmap fg) where TMode : struct, IBlendMode {
    using var result = bg.BlendWith<TMode>(fg);
    Assert.That(result.Width, Is.EqualTo(bg.Width), $"{typeof(TMode).Name} changed width");
    Assert.That(result.Height, Is.EqualTo(bg.Height), $"{typeof(TMode).Name} changed height");
    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb), $"{typeof(TMode).Name} wrong format");

    using var locker = result.Lock();
    var c = locker[0, 0];
    Assert.That(c.R, Is.InRange(0, 255), $"{typeof(TMode).Name} R out of range");
    Assert.That(c.G, Is.InRange(0, 255), $"{typeof(TMode).Name} G out of range");
    Assert.That(c.B, Is.InRange(0, 255), $"{typeof(TMode).Name} B out of range");
  }

  #endregion
}
