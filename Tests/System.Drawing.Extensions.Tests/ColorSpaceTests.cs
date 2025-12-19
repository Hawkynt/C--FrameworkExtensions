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

namespace System.Drawing.Tests;

[TestFixture]
[Category("Unit")]
[Category("System.Drawing")]
[Category("ColorSpace")]
public class ColorSpaceTests {

  private const float FloatTolerance = 0.01f;
  private const int ByteTolerance = 2;

  #region RGB Tests

  [Test]
  [Category("HappyPath")]
  public void Rgb_FromColor_ReturnsCorrectComponents() {
    var color = Color.FromArgb(128, 64, 192);
    var rgb = color.Rgb;

    Assert.That(rgb.R, Is.EqualTo(128));
    Assert.That(rgb.G, Is.EqualTo(64));
    Assert.That(rgb.B, Is.EqualTo(192));
  }

  [Test]
  [Category("HappyPath")]
  public void RgbNormalized_FromColor_ReturnsNormalizedComponents() {
    var color = Color.FromArgb(255, 128, 0);
    var rgb = color.RgbNormalized;

    Assert.That(rgb.R, Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(rgb.G, Is.EqualTo(0.5f).Within(FloatTolerance));
    Assert.That(rgb.B, Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Rgb_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(100, 150, 200);
    var rgb = original.Rgb;
    var result = rgb.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Rgb_ToNormalized_ConvertsCorrectly() {
    var rgb = new Rgb(255, 128, 0);
    var normalized = rgb.ToNormalized();

    Assert.That(normalized.R, Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(normalized.G, Is.EqualTo(0.5f).Within(FloatTolerance));
    Assert.That(normalized.B, Is.EqualTo(0f).Within(FloatTolerance));
  }

  #endregion

  #region HSL Tests

  [Test]
  [Category("HappyPath")]
  public void Hsl_Red_HasCorrectHue() {
    var red = Color.Red;
    var hsl = red.HslNormalized;

    Assert.That(hsl.H, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(hsl.S, Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(hsl.L, Is.EqualTo(0.5f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Hsl_Green_HasCorrectHue() {
    var green = Color.FromArgb(0, 255, 0);
    var hsl = green.HslNormalized;

    Assert.That(hsl.H, Is.EqualTo(1f / 3f).Within(FloatTolerance));
    Assert.That(hsl.S, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Hsl_Blue_HasCorrectHue() {
    var blue = Color.Blue;
    var hsl = blue.HslNormalized;

    Assert.That(hsl.H, Is.EqualTo(2f / 3f).Within(FloatTolerance));
    Assert.That(hsl.S, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Hsl_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(180, 90, 45);
    var hsl = original.HslNormalized;
    var result = hsl.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hsl_Gray_HasZeroSaturation() {
    var gray = Color.FromArgb(128, 128, 128);
    var hsl = gray.HslNormalized;

    Assert.That(hsl.S, Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hsl_Black_HasZeroLightness() {
    var black = Color.Black;
    var hsl = black.HslNormalized;

    Assert.That(hsl.L, Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hsl_White_HasFullLightness() {
    var white = Color.White;
    var hsl = white.HslNormalized;

    Assert.That(hsl.L, Is.EqualTo(1f).Within(FloatTolerance));
  }

  #endregion

  #region HSV Tests

  [Test]
  [Category("HappyPath")]
  public void Hsv_Red_HasCorrectValues() {
    var red = Color.Red;
    var hsv = red.HsvNormalized;

    Assert.That(hsv.H, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(hsv.S, Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(hsv.V, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Hsv_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(200, 100, 50);
    var hsv = original.HsvNormalized;
    var result = hsv.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hsv_Black_HasZeroValue() {
    var black = Color.Black;
    var hsv = black.HsvNormalized;

    Assert.That(hsv.V, Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hsv_White_HasZeroSaturation() {
    var white = Color.White;
    var hsv = white.HsvNormalized;

    Assert.That(hsv.S, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(hsv.V, Is.EqualTo(1f).Within(FloatTolerance));
  }

  #endregion

  #region CMYK Tests

  [Test]
  [Category("HappyPath")]
  public void Cmyk_Red_HasCorrectComponents() {
    var red = Color.Red;
    var cmyk = red.CmykNormalized;

    Assert.That(cmyk.C, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(cmyk.M, Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(cmyk.Y, Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(cmyk.K, Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Cmyk_Cyan_HasCorrectComponents() {
    var cyan = Color.Cyan;
    var cmyk = cyan.CmykNormalized;

    Assert.That(cmyk.C, Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(cmyk.M, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(cmyk.Y, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(cmyk.K, Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Cmyk_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(150, 100, 200);
    var cmyk = original.CmykNormalized;
    var result = cmyk.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Cmyk_Black_HasFullKey() {
    var black = Color.Black;
    var cmyk = black.CmykNormalized;

    Assert.That(cmyk.K, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Cmyk_White_HasNoInk() {
    var white = Color.White;
    var cmyk = white.CmykNormalized;

    Assert.That(cmyk.C, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(cmyk.M, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(cmyk.Y, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(cmyk.K, Is.EqualTo(0f).Within(FloatTolerance));
  }

  #endregion

  #region YCbCr Tests

  [Test]
  [Category("HappyPath")]
  public void YCbCr_White_HasMaxLuminance() {
    var white = Color.White;
    var ycbcr = white.YCbCrNormalized;

    Assert.That(ycbcr.Y, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void YCbCr_Black_HasZeroLuminance() {
    var black = Color.Black;
    var ycbcr = black.YCbCrNormalized;

    Assert.That(ycbcr.Y, Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void YCbCr_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(120, 180, 60);
    var ycbcr = original.YCbCrNormalized;
    var result = ycbcr.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void YCbCr_Gray_HasNeutralChrominance() {
    var gray = Color.FromArgb(128, 128, 128);
    var ycbcr = gray.YCbCrNormalized;

    Assert.That(ycbcr.Cb, Is.EqualTo(0.5f).Within(FloatTolerance));
    Assert.That(ycbcr.Cr, Is.EqualTo(0.5f).Within(FloatTolerance));
  }

  #endregion

  #region HWB Tests

  [Test]
  [Category("HappyPath")]
  public void Hwb_Red_HasCorrectValues() {
    var red = Color.Red;
    var hwb = red.HwbNormalized;

    Assert.That(hwb.H, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(hwb.W, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(hwb.B, Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Hwb_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(100, 150, 200);
    var hwb = original.HwbNormalized;
    var result = hwb.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hwb_White_HasFullWhiteness() {
    var white = Color.White;
    var hwb = white.HwbNormalized;

    Assert.That(hwb.W, Is.EqualTo(1f).Within(FloatTolerance));
    Assert.That(hwb.B, Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hwb_Black_HasFullBlackness() {
    var black = Color.Black;
    var hwb = black.HwbNormalized;

    Assert.That(hwb.W, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(hwb.B, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hwb_Gray_HasEqualWhitenessAndBlackness() {
    var gray = Color.FromArgb(128, 128, 128);
    var hwb = gray.HwbNormalized;

    Assert.That(hwb.W + hwb.B, Is.EqualTo(1f).Within(FloatTolerance));
  }

  #endregion

  #region XYZ Tests

  [Test]
  [Category("HappyPath")]
  public void Xyz_White_HasExpectedLuminance() {
    var white = Color.White;
    var xyz = white.XyzNormalized;

    Assert.That(xyz.Y, Is.EqualTo(1f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Xyz_Black_HasZeroComponents() {
    var black = Color.Black;
    var xyz = black.XyzNormalized;

    Assert.That(xyz.X, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(xyz.Y, Is.EqualTo(0f).Within(FloatTolerance));
    Assert.That(xyz.Z, Is.EqualTo(0f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Xyz_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(128, 64, 192);
    var xyz = original.XyzNormalized;
    var result = xyz.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance + 1));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance + 1));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance + 1));
  }

  #endregion

  #region Lab Tests

  [Test]
  [Category("HappyPath")]
  public void Lab_White_HasMaxLightness() {
    var white = Color.White;
    var lab = white.LabNormalized;

    Assert.That(lab.L, Is.EqualTo(100f).Within(1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Lab_Black_HasZeroLightness() {
    var black = Color.Black;
    var lab = black.LabNormalized;

    Assert.That(lab.L, Is.EqualTo(0f).Within(1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Lab_Gray_HasNeutralAB() {
    var gray = Color.FromArgb(128, 128, 128);
    var lab = gray.LabNormalized;

    Assert.That(lab.A, Is.EqualTo(0f).Within(1f));
    Assert.That(lab.B, Is.EqualTo(0f).Within(1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Lab_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(180, 90, 120);
    var lab = original.LabNormalized;
    var result = lab.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance + 1));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance + 1));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance + 1));
  }

  [Test]
  [Category("HappyPath")]
  public void Lab_Red_HasPositiveA() {
    var red = Color.Red;
    var lab = red.LabNormalized;

    Assert.That(lab.A, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Lab_Blue_HasNegativeB() {
    var blue = Color.Blue;
    var lab = blue.LabNormalized;

    Assert.That(lab.B, Is.LessThan(0f));
  }

  #endregion

  #region Deconstruct Tests

  [Test]
  [Category("HappyPath")]
  public void Rgb_Deconstruct_Works() {
    var rgb = new Rgb(100, 150, 200, 255);
    var (r, g, b, a) = rgb;

    Assert.That(r, Is.EqualTo(100));
    Assert.That(g, Is.EqualTo(150));
    Assert.That(b, Is.EqualTo(200));
    Assert.That(a, Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void HslNormalized_Deconstruct_Works() {
    var hsl = new HslNormalized(0.5f, 0.75f, 0.25f, 1.0f);
    var (h, s, l, a) = hsl;

    Assert.That(h, Is.EqualTo(0.5f));
    Assert.That(s, Is.EqualTo(0.75f));
    Assert.That(l, Is.EqualTo(0.25f));
    Assert.That(a, Is.EqualTo(1.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Cmyk_DeconstructWithAlpha_Works() {
    var cmyk = new CmykNormalized(0.1f, 0.2f, 0.3f, 0.4f, 1.0f);
    var (c, m, y, k, a) = cmyk;

    Assert.That(c, Is.EqualTo(0.1f));
    Assert.That(m, Is.EqualTo(0.2f));
    Assert.That(y, Is.EqualTo(0.3f));
    Assert.That(k, Is.EqualTo(0.4f));
    Assert.That(a, Is.EqualTo(1.0f));
  }

  #endregion

  #region Cross-Conversion Tests

  [Test]
  [Category("HappyPath")]
  public void AllColorSpaces_PrimaryColors_Roundtrip() {
    var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Cyan, Color.Magenta };

    foreach (var original in colors) {
      var fromHsl = original.HslNormalized.ToColor();
      var fromHsv = original.HsvNormalized.ToColor();
      var fromCmyk = original.CmykNormalized.ToColor();
      var fromYcbcr = original.YCbCrNormalized.ToColor();

      Assert.That(fromHsl.R, Is.EqualTo(original.R).Within(ByteTolerance), $"HSL failed for {original}");
      Assert.That(fromHsv.R, Is.EqualTo(original.R).Within(ByteTolerance), $"HSV failed for {original}");
      Assert.That(fromCmyk.R, Is.EqualTo(original.R).Within(ByteTolerance), $"CMYK failed for {original}");
      Assert.That(fromYcbcr.R, Is.EqualTo(original.R).Within(ByteTolerance), $"YCbCr failed for {original}");
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ByteToNormalized_Consistency() {
    var color = Color.FromArgb(128, 64, 192);

    var hslByte = color.Hsl;
    var hslNorm = color.HslNormalized;
    var hslFromByte = hslByte.ToNormalized();

    Assert.That(hslFromByte.H, Is.EqualTo(hslNorm.H).Within(FloatTolerance));
    Assert.That(hslFromByte.S, Is.EqualTo(hslNorm.S).Within(FloatTolerance));
    Assert.That(hslFromByte.L, Is.EqualTo(hslNorm.L).Within(FloatTolerance));
  }

  #endregion

  #region Alpha Channel Tests

  [Test]
  [Category("HappyPath")]
  public void Rgb_PreservesAlpha() {
    var color = Color.FromArgb(128, 255, 128, 64);
    var rgb = color.Rgb;

    Assert.That(rgb.A, Is.EqualTo(128));
  }

  [Test]
  [Category("HappyPath")]
  public void HslNormalized_PreservesAlpha() {
    var color = Color.FromArgb(64, 200, 100, 50);
    var hsl = color.HslNormalized;
    var result = hsl.ToColor();

    Assert.That(result.A, Is.EqualTo(64).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void LabNormalized_PreservesAlpha() {
    var color = Color.FromArgb(200, 180, 90, 120);
    var lab = color.LabNormalized;
    var result = lab.ToColor();

    Assert.That(result.A, Is.EqualTo(200).Within(ByteTolerance));
  }

  #endregion

  #region Byte-Based Color Space Tests

  [Test]
  [Category("HappyPath")]
  public void HslByte_FromColor_ReturnsCorrectComponents() {
    var red = Color.Red;
    var hsl = red.Hsl;

    Assert.That(hsl.H, Is.EqualTo(0));
    Assert.That(hsl.S, Is.EqualTo(255));
    Assert.That(hsl.L, Is.EqualTo(128).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void HslByte_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(180, 90, 45);
    var hsl = original.Hsl;
    var result = hsl.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void HslByte_ToNormalized_IsConsistent() {
    var color = Color.FromArgb(128, 64, 192);
    var hslByte = color.Hsl;
    var hslNorm = color.HslNormalized;
    var converted = hslByte.ToNormalized();

    Assert.That(converted.H, Is.EqualTo(hslNorm.H).Within(FloatTolerance));
    Assert.That(converted.S, Is.EqualTo(hslNorm.S).Within(FloatTolerance));
    Assert.That(converted.L, Is.EqualTo(hslNorm.L).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void HsvByte_FromColor_ReturnsCorrectComponents() {
    var red = Color.Red;
    var hsv = red.Hsv;

    Assert.That(hsv.H, Is.EqualTo(0));
    Assert.That(hsv.S, Is.EqualTo(255));
    Assert.That(hsv.V, Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void HsvByte_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(200, 100, 50);
    var hsv = original.Hsv;
    var result = hsv.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void HsvByte_ToNormalized_IsConsistent() {
    var color = Color.FromArgb(128, 64, 192);
    var hsvByte = color.Hsv;
    var hsvNorm = color.HsvNormalized;
    var converted = hsvByte.ToNormalized();

    Assert.That(converted.H, Is.EqualTo(hsvNorm.H).Within(FloatTolerance));
    Assert.That(converted.S, Is.EqualTo(hsvNorm.S).Within(FloatTolerance));
    Assert.That(converted.V, Is.EqualTo(hsvNorm.V).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void HwbByte_FromColor_ReturnsCorrectComponents() {
    var red = Color.Red;
    var hwb = red.Hwb;

    Assert.That(hwb.H, Is.EqualTo(0));
    Assert.That(hwb.W, Is.EqualTo(0));
    Assert.That(hwb.B, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void HwbByte_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(100, 150, 200);
    var hwb = original.Hwb;
    var result = hwb.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void HwbByte_ToNormalized_IsConsistent() {
    var color = Color.FromArgb(128, 64, 192);
    var hwbByte = color.Hwb;
    var hwbNorm = color.HwbNormalized;
    var converted = hwbByte.ToNormalized();

    Assert.That(converted.H, Is.EqualTo(hwbNorm.H).Within(FloatTolerance));
    Assert.That(converted.W, Is.EqualTo(hwbNorm.W).Within(FloatTolerance));
    Assert.That(converted.B, Is.EqualTo(hwbNorm.B).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CmykByte_FromColor_ReturnsCorrectComponents() {
    var red = Color.Red;
    var cmyk = red.Cmyk;

    Assert.That(cmyk.C, Is.EqualTo(0));
    Assert.That(cmyk.M, Is.EqualTo(255));
    Assert.That(cmyk.Y, Is.EqualTo(255));
    Assert.That(cmyk.K, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void CmykByte_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(150, 100, 200);
    var cmyk = original.Cmyk;
    var result = cmyk.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void CmykByte_ToNormalized_IsConsistent() {
    var color = Color.FromArgb(128, 64, 192);
    var cmykByte = color.Cmyk;
    var cmykNorm = color.CmykNormalized;
    var converted = cmykByte.ToNormalized();

    Assert.That(converted.C, Is.EqualTo(cmykNorm.C).Within(FloatTolerance));
    Assert.That(converted.M, Is.EqualTo(cmykNorm.M).Within(FloatTolerance));
    Assert.That(converted.Y, Is.EqualTo(cmykNorm.Y).Within(FloatTolerance));
    Assert.That(converted.K, Is.EqualTo(cmykNorm.K).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void YCbCrByte_FromColor_ReturnsCorrectComponents() {
    var white = Color.White;
    var ycbcr = white.YCbCr;

    Assert.That(ycbcr.Y, Is.EqualTo(255));
    Assert.That(ycbcr.Cb, Is.EqualTo(128).Within(ByteTolerance));
    Assert.That(ycbcr.Cr, Is.EqualTo(128).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void YCbCrByte_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(120, 180, 60);
    var ycbcr = original.YCbCr;
    var result = ycbcr.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void YCbCrByte_ToNormalized_IsConsistent() {
    var color = Color.FromArgb(128, 64, 192);
    var ycbcrByte = color.YCbCr;
    var ycbcrNorm = color.YCbCrNormalized;
    var converted = ycbcrByte.ToNormalized();

    Assert.That(converted.Y, Is.EqualTo(ycbcrNorm.Y).Within(FloatTolerance));
    Assert.That(converted.Cb, Is.EqualTo(ycbcrNorm.Cb).Within(FloatTolerance));
    Assert.That(converted.Cr, Is.EqualTo(ycbcrNorm.Cr).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void YuvByte_FromColor_ReturnsCorrectComponents() {
    var white = Color.White;
    var yuv = white.Yuv;

    Assert.That(yuv.Y, Is.EqualTo(255));
    Assert.That(yuv.U, Is.EqualTo(128).Within(ByteTolerance));
    Assert.That(yuv.V, Is.EqualTo(128).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void YuvByte_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(120, 180, 60);
    var yuv = original.Yuv;
    var result = yuv.ToColor();

    Assert.That(result.R, Is.EqualTo(original.R).Within(ByteTolerance));
    Assert.That(result.G, Is.EqualTo(original.G).Within(ByteTolerance));
    Assert.That(result.B, Is.EqualTo(original.B).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void YuvByte_ToNormalized_IsConsistent() {
    var color = Color.FromArgb(128, 64, 192);
    var yuvByte = color.Yuv;
    var yuvNorm = color.YuvNormalized;
    var converted = yuvByte.ToNormalized();

    Assert.That(converted.Y, Is.EqualTo(yuvNorm.Y).Within(FloatTolerance));
    Assert.That(converted.U, Is.EqualTo(yuvNorm.U).Within(FloatTolerance));
    Assert.That(converted.V, Is.EqualTo(yuvNorm.V).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void XyzByte_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(128, 64, 192);
    var xyz = original.Xyz;
    var result = xyz.ToColor();

    // XYZ byte conversion has higher loss due to clamping and scaling
    Assert.That(result.R, Is.EqualTo(original.R).Within(15));
    Assert.That(result.G, Is.EqualTo(original.G).Within(15));
    Assert.That(result.B, Is.EqualTo(original.B).Within(15));
  }

  [Test]
  [Category("HappyPath")]
  public void XyzByte_ToNormalized_IsConsistent() {
    var color = Color.FromArgb(128, 64, 192);
    var xyzByte = color.Xyz;
    var converted = xyzByte.ToNormalized();

    // Byte to normalized should be consistent with the byte values (divided by 255)
    Assert.That(converted.X, Is.EqualTo(xyzByte.X / 255f).Within(FloatTolerance));
    Assert.That(converted.Y, Is.EqualTo(xyzByte.Y / 255f).Within(FloatTolerance));
    Assert.That(converted.Z, Is.EqualTo(xyzByte.Z / 255f).Within(FloatTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void LabByte_Roundtrip_PreservesColor() {
    var original = Color.FromArgb(180, 90, 120);
    var lab = original.Lab;
    var result = lab.ToColor();

    // Lab byte conversion has some loss due to a/b channel offset encoding
    Assert.That(result.R, Is.EqualTo(original.R).Within(5));
    Assert.That(result.G, Is.EqualTo(original.G).Within(5));
    Assert.That(result.B, Is.EqualTo(original.B).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void LabByte_ToNormalized_IsConsistent() {
    var color = Color.FromArgb(128, 64, 192);
    var labByte = color.Lab;
    var labNorm = color.LabNormalized;
    var converted = labByte.ToNormalized();

    Assert.That(converted.L, Is.EqualTo(labNorm.L).Within(1f));
    Assert.That(converted.A, Is.EqualTo(labNorm.A).Within(2f));
    Assert.That(converted.B, Is.EqualTo(labNorm.B).Within(2f));
  }

  #endregion

  #region Byte-Based Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void HslByte_Gray_HasZeroSaturation() {
    var gray = Color.FromArgb(128, 128, 128);
    var hsl = gray.Hsl;

    Assert.That(hsl.S, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void HslByte_Black_HasZeroLightness() {
    var black = Color.Black;
    var hsl = black.Hsl;

    Assert.That(hsl.L, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void HslByte_White_HasMaxLightness() {
    var white = Color.White;
    var hsl = white.Hsl;

    Assert.That(hsl.L, Is.EqualTo(255));
  }

  [Test]
  [Category("EdgeCase")]
  public void HsvByte_Black_HasZeroValue() {
    var black = Color.Black;
    var hsv = black.Hsv;

    Assert.That(hsv.V, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void HsvByte_White_HasMaxValue() {
    var white = Color.White;
    var hsv = white.Hsv;

    Assert.That(hsv.V, Is.EqualTo(255));
  }

  [Test]
  [Category("EdgeCase")]
  public void CmykByte_Black_HasMaxKey() {
    var black = Color.Black;
    var cmyk = black.Cmyk;

    Assert.That(cmyk.K, Is.EqualTo(255));
  }

  [Test]
  [Category("EdgeCase")]
  public void CmykByte_White_HasNoInk() {
    var white = Color.White;
    var cmyk = white.Cmyk;

    Assert.That(cmyk.C, Is.EqualTo(0));
    Assert.That(cmyk.M, Is.EqualTo(0));
    Assert.That(cmyk.Y, Is.EqualTo(0));
    Assert.That(cmyk.K, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void HwbByte_White_HasMaxWhiteness() {
    var white = Color.White;
    var hwb = white.Hwb;

    Assert.That(hwb.W, Is.EqualTo(255));
    Assert.That(hwb.B, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void HwbByte_Black_HasMaxBlackness() {
    var black = Color.Black;
    var hwb = black.Hwb;

    Assert.That(hwb.W, Is.EqualTo(0));
    Assert.That(hwb.B, Is.EqualTo(255));
  }

  [Test]
  [Category("EdgeCase")]
  public void YCbCrByte_Black_HasZeroLuminance() {
    var black = Color.Black;
    var ycbcr = black.YCbCr;

    Assert.That(ycbcr.Y, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void YuvByte_Black_HasZeroLuminance() {
    var black = Color.Black;
    var yuv = black.Yuv;

    Assert.That(yuv.Y, Is.EqualTo(0));
  }

  #endregion

  #region Byte-Based Alpha Tests

  [Test]
  [Category("HappyPath")]
  public void HslByte_PreservesAlpha() {
    var color = Color.FromArgb(128, 200, 100, 50);
    var hsl = color.Hsl;
    var result = hsl.ToColor();

    Assert.That(result.A, Is.EqualTo(128));
  }

  [Test]
  [Category("HappyPath")]
  public void HsvByte_PreservesAlpha() {
    var color = Color.FromArgb(64, 200, 100, 50);
    var hsv = color.Hsv;
    var result = hsv.ToColor();

    Assert.That(result.A, Is.EqualTo(64));
  }

  [Test]
  [Category("HappyPath")]
  public void HwbByte_PreservesAlpha() {
    var color = Color.FromArgb(192, 200, 100, 50);
    var hwb = color.Hwb;
    var result = hwb.ToColor();

    Assert.That(result.A, Is.EqualTo(192));
  }

  [Test]
  [Category("HappyPath")]
  public void CmykByte_PreservesAlpha() {
    var color = Color.FromArgb(100, 200, 100, 50);
    var cmyk = color.Cmyk;
    var result = cmyk.ToColor();

    Assert.That(result.A, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void YCbCrByte_PreservesAlpha() {
    var color = Color.FromArgb(150, 200, 100, 50);
    var ycbcr = color.YCbCr;
    var result = ycbcr.ToColor();

    Assert.That(result.A, Is.EqualTo(150));
  }

  [Test]
  [Category("HappyPath")]
  public void YuvByte_PreservesAlpha() {
    var color = Color.FromArgb(200, 200, 100, 50);
    var yuv = color.Yuv;
    var result = yuv.ToColor();

    Assert.That(result.A, Is.EqualTo(200));
  }

  [Test]
  [Category("HappyPath")]
  public void LabByte_PreservesAlpha() {
    var color = Color.FromArgb(80, 200, 100, 50);
    var lab = color.Lab;
    var result = lab.ToColor();

    Assert.That(result.A, Is.EqualTo(80).Within(ByteTolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void XyzByte_PreservesAlpha() {
    var color = Color.FromArgb(220, 200, 100, 50);
    var xyz = color.Xyz;
    var result = xyz.ToColor();

    Assert.That(result.A, Is.EqualTo(220).Within(ByteTolerance));
  }

  #endregion

  #region Byte-Based Primary Color Tests

  [Test]
  [Category("HappyPath")]
  public void AllByteColorSpaces_PrimaryColors_Roundtrip() {
    var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Cyan, Color.Magenta };

    foreach (var original in colors) {
      var fromHsl = original.Hsl.ToColor();
      var fromHsv = original.Hsv.ToColor();
      var fromHwb = original.Hwb.ToColor();
      var fromCmyk = original.Cmyk.ToColor();
      var fromYcbcr = original.YCbCr.ToColor();

      Assert.That(fromHsl.R, Is.EqualTo(original.R).Within(ByteTolerance), $"HSL failed for {original}");
      Assert.That(fromHsl.G, Is.EqualTo(original.G).Within(ByteTolerance), $"HSL failed for {original}");
      Assert.That(fromHsl.B, Is.EqualTo(original.B).Within(ByteTolerance), $"HSL failed for {original}");

      Assert.That(fromHsv.R, Is.EqualTo(original.R).Within(ByteTolerance), $"HSV failed for {original}");
      Assert.That(fromHsv.G, Is.EqualTo(original.G).Within(ByteTolerance), $"HSV failed for {original}");
      Assert.That(fromHsv.B, Is.EqualTo(original.B).Within(ByteTolerance), $"HSV failed for {original}");

      Assert.That(fromHwb.R, Is.EqualTo(original.R).Within(ByteTolerance), $"HWB failed for {original}");
      Assert.That(fromHwb.G, Is.EqualTo(original.G).Within(ByteTolerance), $"HWB failed for {original}");
      Assert.That(fromHwb.B, Is.EqualTo(original.B).Within(ByteTolerance), $"HWB failed for {original}");

      Assert.That(fromCmyk.R, Is.EqualTo(original.R).Within(ByteTolerance), $"CMYK failed for {original}");
      Assert.That(fromCmyk.G, Is.EqualTo(original.G).Within(ByteTolerance), $"CMYK failed for {original}");
      Assert.That(fromCmyk.B, Is.EqualTo(original.B).Within(ByteTolerance), $"CMYK failed for {original}");

      Assert.That(fromYcbcr.R, Is.EqualTo(original.R).Within(ByteTolerance), $"YCbCr failed for {original}");
      Assert.That(fromYcbcr.G, Is.EqualTo(original.G).Within(ByteTolerance), $"YCbCr failed for {original}");
      Assert.That(fromYcbcr.B, Is.EqualTo(original.B).Within(ByteTolerance), $"YCbCr failed for {original}");
    }
  }

  [Test]
  [Category("HappyPath")]
  public void YuvByte_MixedColors_Roundtrip() {
    // YUV is designed for video compression and has significant loss for saturated primary colors
    // Test with more realistic mixed colors instead
    var colors = new[] {
      Color.FromArgb(200, 150, 100),
      Color.FromArgb(100, 150, 200),
      Color.FromArgb(150, 100, 150),
      Color.FromArgb(180, 180, 180)
    };

    foreach (var original in colors) {
      var fromYuv = original.Yuv.ToColor();

      Assert.That(fromYuv.R, Is.EqualTo(original.R).Within(ByteTolerance), $"YUV failed for {original}");
      Assert.That(fromYuv.G, Is.EqualTo(original.G).Within(ByteTolerance), $"YUV failed for {original}");
      Assert.That(fromYuv.B, Is.EqualTo(original.B).Within(ByteTolerance), $"YUV failed for {original}");
    }
  }

  #endregion

}

