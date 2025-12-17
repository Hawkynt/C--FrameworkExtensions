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

using NUnit.Framework;

namespace System.Drawing.Tests;

[TestFixture]
[Category("Unit")]
[Category("System.Drawing")]
[Category("Color")]
public class ColorExtensionsTests {

  #region Luminance Tests

  [Test]
  [Category("HappyPath")]
  public void GetLuminance_White_Returns255() {
    var luminance = Color.White.GetLuminance();
    Assert.That(luminance, Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void GetLuminance_Black_Returns0() {
    var luminance = Color.Black.GetLuminance();
    Assert.That(luminance, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void GetLuminance_Red_ReturnsApprox77() {
    var luminance = Color.Red.GetLuminance();
    Assert.That(luminance, Is.InRange(75, 78));
  }

  [Test]
  [Category("HappyPath")]
  public void GetLuminance_Green_ReturnsApprox150() {
    var luminance = Color.FromArgb(0, 255, 0).GetLuminance();
    Assert.That(luminance, Is.InRange(148, 152));
  }

  [Test]
  [Category("HappyPath")]
  public void GetLuminance_Blue_ReturnsApprox29() {
    var luminance = Color.Blue.GetLuminance();
    Assert.That(luminance, Is.InRange(27, 31));
  }

  #endregion

  #region Chrominance Tests

  [Test]
  [Category("HappyPath")]
  public void GetChrominanceU_Gray_ReturnsNeutral() {
    var chromaU = Color.Gray.GetChrominanceU();
    Assert.That(chromaU, Is.InRange(125, 130));
  }

  [Test]
  [Category("HappyPath")]
  public void GetChrominanceV_Gray_ReturnsNeutral() {
    var chromaV = Color.Gray.GetChrominanceV();
    Assert.That(chromaV, Is.InRange(125, 130));
  }

  [Test]
  [Category("HappyPath")]
  public void GetChrominanceU_Red_IsHigher() {
    var chromaU = Color.Red.GetChrominanceU();
    Assert.That(chromaU, Is.GreaterThan(127));
  }

  [Test]
  [Category("HappyPath")]
  public void GetChrominanceV_Blue_IsHigher() {
    var chromaV = Color.Blue.GetChrominanceV();
    Assert.That(chromaV, Is.GreaterThan(127));
  }

  #endregion

  #region IsLike Tests

  [Test]
  [Category("HappyPath")]
  public void IsLike_SameColor_ReturnsTrue() {
    Assert.That(Color.Red.IsLike(Color.Red), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsLike_VeryDifferentColors_ReturnsFalse() {
    Assert.That(Color.Red.IsLike(Color.Blue), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IsLike_SimilarColors_ReturnsTrue() {
    var c1 = Color.FromArgb(255, 128, 128, 128);
    var c2 = Color.FromArgb(255, 130, 128, 128);
    Assert.That(c1.IsLike(c2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsLike_WithStrictTolerance_ReturnsFalse() {
    var c1 = Color.FromArgb(255, 128, 128, 128);
    var c2 = Color.FromArgb(255, 140, 128, 128);
    Assert.That(c1.IsLike(c2, luminanceDelta: 1, chromaUDelta: 1, chromaVDelta: 1), Is.False);
  }

  #endregion

  #region BlendWith Tests

  [Test]
  [Category("HappyPath")]
  public void BlendWith_ZeroFactor_ReturnsFirstColor() {
    var result = Color.Red.BlendWith(Color.Blue, 0, 1);
    Assert.That(result.R, Is.EqualTo(Color.Red.R));
    Assert.That(result.G, Is.EqualTo(Color.Red.G));
    Assert.That(result.B, Is.EqualTo(Color.Red.B));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendWith_MaxFactor_ReturnsSecondColor() {
    var result = Color.Red.BlendWith(Color.Blue, 1, 1);
    Assert.That(result.R, Is.EqualTo(Color.Blue.R));
    Assert.That(result.G, Is.EqualTo(Color.Blue.G));
    Assert.That(result.B, Is.EqualTo(Color.Blue.B));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendWith_HalfFactor_ReturnsMidpoint() {
    var result = Color.FromArgb(255, 0, 0, 0).BlendWith(Color.FromArgb(255, 200, 200, 200), 0.5f, 1);
    Assert.That(result.R, Is.EqualTo(100));
    Assert.That(result.G, Is.EqualTo(100));
    Assert.That(result.B, Is.EqualTo(100));
  }

  #endregion

  #region InterpolateWith Tests

  [Test]
  [Category("HappyPath")]
  public void InterpolateWith_ZeroFactor_ReturnsFirstColor() {
    var result = Color.Red.InterpolateWith(Color.Blue, 0);
    Assert.That(result.R, Is.EqualTo(Color.Red.R));
    Assert.That(result.G, Is.EqualTo(Color.Red.G));
    Assert.That(result.B, Is.EqualTo(Color.Red.B));
  }

  [Test]
  [Category("HappyPath")]
  public void InterpolateWith_OneFactor_ReturnsAverage() {
    var result = Color.FromArgb(255, 100, 100, 100).InterpolateWith(Color.FromArgb(255, 200, 200, 200), 1);
    Assert.That(result.R, Is.EqualTo(150));
    Assert.That(result.G, Is.EqualTo(150));
    Assert.That(result.B, Is.EqualTo(150));
  }

  #endregion

  #region Lighten/Darken Tests

  [Test]
  [Category("HappyPath")]
  public void Lighten_IncreasesRgbValues() {
    var original = Color.FromArgb(255, 100, 100, 100);
    var lightened = original.Lighten(50);
    Assert.That(lightened.R, Is.EqualTo(150));
    Assert.That(lightened.G, Is.EqualTo(150));
    Assert.That(lightened.B, Is.EqualTo(150));
  }

  [Test]
  [Category("HappyPath")]
  public void Darken_DecreasesRgbValues() {
    var original = Color.FromArgb(255, 100, 100, 100);
    var darkened = original.Darken(50);
    Assert.That(darkened.R, Is.EqualTo(50));
    Assert.That(darkened.G, Is.EqualTo(50));
    Assert.That(darkened.B, Is.EqualTo(50));
  }

  [Test]
  [Category("EdgeCase")]
  public void Lighten_ClampsAt255() {
    var original = Color.FromArgb(255, 200, 200, 200);
    var lightened = original.Lighten(100);
    Assert.That(lightened.R, Is.EqualTo(255));
    Assert.That(lightened.G, Is.EqualTo(255));
    Assert.That(lightened.B, Is.EqualTo(255));
  }

  [Test]
  [Category("EdgeCase")]
  public void Darken_ClampsAt0() {
    var original = Color.FromArgb(255, 50, 50, 50);
    var darkened = original.Darken(100);
    Assert.That(darkened.R, Is.EqualTo(0));
    Assert.That(darkened.G, Is.EqualTo(0));
    Assert.That(darkened.B, Is.EqualTo(0));
  }

  #endregion

  #region Add Tests

  [Test]
  [Category("HappyPath")]
  public void Add_SingleValue_AddsToAllChannels() {
    var original = Color.FromArgb(255, 100, 100, 100);
    var result = original.Add(25);
    Assert.That(result.R, Is.EqualTo(125));
    Assert.That(result.G, Is.EqualTo(125));
    Assert.That(result.B, Is.EqualTo(125));
  }

  [Test]
  [Category("HappyPath")]
  public void Add_PerChannel_AddsToEachChannel() {
    var original = Color.FromArgb(255, 100, 100, 100);
    var result = original.Add(10, 20, 30);
    Assert.That(result.R, Is.EqualTo(110));
    Assert.That(result.G, Is.EqualTo(120));
    Assert.That(result.B, Is.EqualTo(130));
  }

  [Test]
  [Category("HappyPath")]
  public void Add_PreservesAlpha() {
    var original = Color.FromArgb(128, 100, 100, 100);
    var result = original.Add(25);
    Assert.That(result.A, Is.EqualTo(128));
  }

  #endregion

  #region Multiply Tests

  [Test]
  [Category("HappyPath")]
  public void Multiply_SingleValue_MultipliesAllChannels() {
    var original = Color.FromArgb(255, 100, 100, 100);
    var result = original.Multiply(2.0);
    Assert.That(result.R, Is.EqualTo(200));
    Assert.That(result.G, Is.EqualTo(200));
    Assert.That(result.B, Is.EqualTo(200));
  }

  [Test]
  [Category("HappyPath")]
  public void Multiply_PerChannel_MultipliesEachChannel() {
    var original = Color.FromArgb(255, 100, 100, 100);
    var result = original.Multiply(1.0, 1.5, 2.0);
    Assert.That(result.R, Is.EqualTo(100));
    Assert.That(result.G, Is.EqualTo(150));
    Assert.That(result.B, Is.EqualTo(200));
  }

  [Test]
  [Category("EdgeCase")]
  public void Multiply_ClampsAt255() {
    var original = Color.FromArgb(255, 200, 200, 200);
    var result = original.Multiply(2.0);
    Assert.That(result.R, Is.EqualTo(255));
    Assert.That(result.G, Is.EqualTo(255));
    Assert.That(result.B, Is.EqualTo(255));
  }

  #endregion

  #region GetComplementaryColor Tests

  [Test]
  [Category("HappyPath")]
  public void GetComplementaryColor_White_ReturnsBlack() {
    var result = Color.White.GetComplementaryColor();
    Assert.That(result.R, Is.EqualTo(0));
    Assert.That(result.G, Is.EqualTo(0));
    Assert.That(result.B, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void GetComplementaryColor_Black_ReturnsWhite() {
    var result = Color.Black.GetComplementaryColor();
    Assert.That(result.R, Is.EqualTo(255));
    Assert.That(result.G, Is.EqualTo(255));
    Assert.That(result.B, Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void GetComplementaryColor_PreservesAlpha() {
    var original = Color.FromArgb(128, 100, 150, 200);
    var result = original.GetComplementaryColor();
    Assert.That(result.A, Is.EqualTo(128));
    Assert.That(result.R, Is.EqualTo(155));
    Assert.That(result.G, Is.EqualTo(105));
    Assert.That(result.B, Is.EqualTo(55));
  }

  #endregion

  #region ToHex Tests

  [Test]
  [Category("HappyPath")]
  public void ToHex_Black_Returns000000() {
    Assert.That(Color.Black.ToHex(), Is.EqualTo("#000000"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToHex_White_ReturnsFFFFFF() {
    Assert.That(Color.White.ToHex(), Is.EqualTo("#FFFFFF"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToHex_Red_ReturnsFF0000() {
    Assert.That(Color.Red.ToHex(), Is.EqualTo("#FF0000"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToHex_Green_Returns00FF00() {
    Assert.That(Color.FromArgb(0, 255, 0).ToHex(), Is.EqualTo("#00FF00"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToHex_Blue_Returns0000FF() {
    Assert.That(Color.Blue.ToHex(), Is.EqualTo("#0000FF"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToHex_CustomColor_ReturnsCorrectHex() {
    var color = Color.FromArgb(255, 171, 205, 239);
    Assert.That(color.ToHex(), Is.EqualTo("#ABCDEF"));
  }

  #endregion

  #region GetName Tests

  [Test]
  [Category("HappyPath")]
  public void GetName_NamedColor_ReturnsName() {
    var name = Color.Red.GetName();
    Assert.That(name, Is.EqualTo("Red"));
  }

  [Test]
  [Category("HappyPath")]
  public void GetName_Blue_ReturnsBlue() {
    var name = Color.Blue.GetName();
    Assert.That(name, Is.EqualTo("Blue"));
  }

  #endregion
}
