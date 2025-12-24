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

using Hawkynt.ColorProcessing.Working;
using Hawkynt.ColorProcessing.Spaces.Hdr;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Spaces.WideGamut;
using Hawkynt.ColorProcessing.Spaces.Yuv;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Projectors")]
public class ProjectorTests {

  private const float Tolerance = 0.05f;

  #region Test Colors

  private static readonly LinearRgbF White = new(1f, 1f, 1f);
  private static readonly LinearRgbF Black = new(0f, 0f, 0f);
  private static readonly LinearRgbF Red = new(1f, 0f, 0f);
  private static readonly LinearRgbF Green = new(0f, 1f, 0f);
  private static readonly LinearRgbF Blue = new(0f, 0f, 1f);
  private static readonly LinearRgbF Gray = new(0.5f, 0.5f, 0.5f);
  private static readonly LinearRgbF MixedColor = new(0.7f, 0.3f, 0.5f);

  #endregion

  #region OkLCh Tests

  [Test]
  [Category("HappyPath")]
  public void OklabToOklch_White_HasZeroChroma() {
    var oklab = new LinearRgbFToOklabF().Project(White);
    var oklch = new OklabFToOklchF().Project(oklab);

    Assert.That(oklch.L, Is.EqualTo(1f).Within(Tolerance));
    Assert.That(oklch.C, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void OklabToOklch_Roundtrip_PreservesValues() {
    var oklab = new LinearRgbFToOklabF().Project(MixedColor);
    var oklch = new OklabFToOklchF().Project(oklab);
    var backToOklab = new OklchFToOklabF().Project(oklch);

    Assert.That(backToOklab.L, Is.EqualTo(oklab.L).Within(Tolerance));
    Assert.That(backToOklab.A, Is.EqualTo(oklab.A).Within(Tolerance));
    Assert.That(backToOklab.B, Is.EqualTo(oklab.B).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void OklchToLinearRgb_Roundtrip_PreservesColor() {
    var oklch = new LinearRgbFToOklchF().Project(MixedColor);
    var backToRgb = new OklchFToLinearRgbF().Project(oklch);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void OklchF_Create_WorksCorrectly() {
    var oklch = OklchF.Create(0.7f, 0.15f, 0.5f);

    Assert.That(oklch.L, Is.EqualTo(0.7f));
    Assert.That(oklch.C, Is.EqualTo(0.15f));
    Assert.That(oklch.H, Is.EqualTo(0.5f));
  }

  #endregion

  #region JzAzBz Tests

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToJzAzBz_White_HasHighJz() {
    var jzazbz = new LinearRgbFToJzAzBzF().Project(White);

    Assert.That(jzazbz.Jz, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToJzAzBz_Black_HasZeroJz() {
    var jzazbz = new LinearRgbFToJzAzBzF().Project(Black);

    Assert.That(jzazbz.Jz, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void JzAzBzToLinearRgb_Roundtrip_PreservesColor() {
    var jzazbz = new LinearRgbFToJzAzBzF().Project(MixedColor);
    var backToRgb = new JzAzBzFToLinearRgbF().Project(jzazbz);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void JzAzBzF_Create_WorksCorrectly() {
    var jzazbz = JzAzBzF.Create(0.5f, 0.1f, -0.1f);

    Assert.That(jzazbz.Jz, Is.EqualTo(0.5f));
    Assert.That(jzazbz.Az, Is.EqualTo(0.1f));
    Assert.That(jzazbz.Bz, Is.EqualTo(-0.1f));
  }

  #endregion

  #region JzCzhz Tests

  [Test]
  [Category("HappyPath")]
  public void JzAzBzToJzCzhz_Roundtrip_PreservesValues() {
    var jzazbz = new LinearRgbFToJzAzBzF().Project(MixedColor);
    var jzczhz = new JzAzBzFToJzCzhzF().Project(jzazbz);
    var backToJzAzBz = new JzCzhzFToJzAzBzF().Project(jzczhz);

    Assert.That(backToJzAzBz.Jz, Is.EqualTo(jzazbz.Jz).Within(Tolerance));
    Assert.That(backToJzAzBz.Az, Is.EqualTo(jzazbz.Az).Within(Tolerance));
    Assert.That(backToJzAzBz.Bz, Is.EqualTo(jzazbz.Bz).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void JzCzhzToLinearRgb_Roundtrip_PreservesColor() {
    var jzczhz = new LinearRgbFToJzCzhzF().Project(MixedColor);
    var backToRgb = new JzCzhzFToLinearRgbF().Project(jzczhz);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void JzCzhzF_Create_WorksCorrectly() {
    var jzczhz = JzCzhzF.Create(0.6f, 0.2f, 0.75f);

    Assert.That(jzczhz.Jz, Is.EqualTo(0.6f));
    Assert.That(jzczhz.Cz, Is.EqualTo(0.2f));
    Assert.That(jzczhz.Hz, Is.EqualTo(0.75f));
  }

  #endregion

  #region ICtCp Tests

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToICtCp_White_HasHighIntensity() {
    var ictcp = new LinearRgbFToICtCpF().Project(White);

    Assert.That(ictcp.I, Is.GreaterThan(0f));
  }

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToICtCp_Gray_HasNeutralChroma() {
    var ictcp = new LinearRgbFToICtCpF().Project(Gray);

    Assert.That(ictcp.Ct, Is.EqualTo(0f).Within(Tolerance));
    Assert.That(ictcp.Cp, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void ICtCpToLinearRgb_Roundtrip_PreservesColor() {
    var ictcp = new LinearRgbFToICtCpF().Project(MixedColor);
    var backToRgb = new ICtCpFToLinearRgbF().Project(ictcp);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void ICtCpF_Create_WorksCorrectly() {
    var ictcp = ICtCpF.Create(0.8f, 0.1f, -0.05f);

    Assert.That(ictcp.I, Is.EqualTo(0.8f));
    Assert.That(ictcp.Ct, Is.EqualTo(0.1f));
    Assert.That(ictcp.Cp, Is.EqualTo(-0.05f));
  }

  #endregion

  #region Display P3 Tests

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToDisplayP3_White_RemainsWhite() {
    var p3 = new LinearRgbFToDisplayP3F().Project(White);

    Assert.That(p3.R, Is.EqualTo(1f).Within(Tolerance));
    Assert.That(p3.G, Is.EqualTo(1f).Within(Tolerance));
    Assert.That(p3.B, Is.EqualTo(1f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void DisplayP3ToLinearRgb_Roundtrip_PreservesColor() {
    var p3 = new LinearRgbFToDisplayP3F().Project(MixedColor);
    var backToRgb = new DisplayP3FToLinearRgbF().Project(p3);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void DisplayP3F_Create_WorksCorrectly() {
    var p3 = DisplayP3F.Create(0.6f, 0.4f, 0.2f);

    Assert.That(p3.R, Is.EqualTo(0.6f));
    Assert.That(p3.G, Is.EqualTo(0.4f));
    Assert.That(p3.B, Is.EqualTo(0.2f));
  }

  #endregion

  #region ProPhoto RGB Tests

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToProPhotoRgb_White_RemainsWhite() {
    var pp = new LinearRgbFToProPhotoRgbF().Project(White);

    Assert.That(pp.R, Is.EqualTo(1f).Within(Tolerance));
    Assert.That(pp.G, Is.EqualTo(1f).Within(Tolerance));
    Assert.That(pp.B, Is.EqualTo(1f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void ProPhotoRgbToLinearRgb_Roundtrip_PreservesColor() {
    var pp = new LinearRgbFToProPhotoRgbF().Project(MixedColor);
    var backToRgb = new ProPhotoRgbFToLinearRgbF().Project(pp);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void ProPhotoRgbF_Create_WorksCorrectly() {
    var pp = ProPhotoRgbF.Create(0.5f, 0.7f, 0.3f);

    Assert.That(pp.R, Is.EqualTo(0.5f));
    Assert.That(pp.G, Is.EqualTo(0.7f));
    Assert.That(pp.B, Is.EqualTo(0.3f));
  }

  #endregion

  #region ACEScg Tests

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToAcesCg_White_RemainsWhite() {
    var aces = new LinearRgbFToAcesCgF().Project(White);

    Assert.That(aces.R, Is.EqualTo(1f).Within(Tolerance));
    Assert.That(aces.G, Is.EqualTo(1f).Within(Tolerance));
    Assert.That(aces.B, Is.EqualTo(1f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void AcesCgToLinearRgb_Roundtrip_PreservesColor() {
    var aces = new LinearRgbFToAcesCgF().Project(MixedColor);
    var backToRgb = new AcesCgFToLinearRgbF().Project(aces);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void AcesCgF_Create_WorksCorrectly() {
    var aces = AcesCgF.Create(0.8f, 0.5f, 0.2f);

    Assert.That(aces.R, Is.EqualTo(0.8f));
    Assert.That(aces.G, Is.EqualTo(0.5f));
    Assert.That(aces.B, Is.EqualTo(0.2f));
  }

  #endregion

  #region Adobe RGB Tests

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToAdobeRgb_White_RemainsWhite() {
    var adobe = new LinearRgbFToAdobeRgbF().Project(White);

    Assert.That(adobe.R, Is.EqualTo(1f).Within(Tolerance));
    Assert.That(adobe.G, Is.EqualTo(1f).Within(Tolerance));
    Assert.That(adobe.B, Is.EqualTo(1f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void AdobeRgbToLinearRgb_Roundtrip_PreservesColor() {
    var adobe = new LinearRgbFToAdobeRgbF().Project(MixedColor);
    var backToRgb = new AdobeRgbFToLinearRgbF().Project(adobe);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void AdobeRgbF_Create_WorksCorrectly() {
    var adobe = AdobeRgbF.Create(0.4f, 0.6f, 0.8f);

    Assert.That(adobe.R, Is.EqualTo(0.4f));
    Assert.That(adobe.G, Is.EqualTo(0.6f));
    Assert.That(adobe.B, Is.EqualTo(0.8f));
  }

  #endregion

  #region YCbCr Tests

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToYCbCrBt601_White_HasMaxLuma() {
    var ycbcr = new LinearRgbFToYCbCrBt601F().Project(White);

    Assert.That(ycbcr.Y, Is.EqualTo(1f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToYCbCrBt601_Black_HasZeroLuma() {
    var ycbcr = new LinearRgbFToYCbCrBt601F().Project(Black);

    Assert.That(ycbcr.Y, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void YCbCrBt601ToLinearRgb_Roundtrip_PreservesColor() {
    var ycbcr = new LinearRgbFToYCbCrBt601F().Project(MixedColor);
    var backToRgb = new YCbCrBt601FToLinearRgbF().Project(ycbcr);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToYCbCrBt709_Roundtrip_PreservesColor() {
    var ycbcr = new LinearRgbFToYCbCrBt709F().Project(MixedColor);
    var backToRgb = new YCbCrBt709FToLinearRgbF().Project(ycbcr);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToYCbCrBt2020_Roundtrip_PreservesColor() {
    var ycbcr = new LinearRgbFToYCbCrBt2020F().Project(MixedColor);
    var backToRgb = new YCbCrBt2020FToLinearRgbF().Project(ycbcr);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void YCbCrF_Create_WorksCorrectly() {
    var ycbcr = YCbCrF.Create(0.5f, 0.1f, -0.2f);

    Assert.That(ycbcr.Y, Is.EqualTo(0.5f));
    Assert.That(ycbcr.Cb, Is.EqualTo(0.1f));
    Assert.That(ycbcr.Cr, Is.EqualTo(-0.2f));
  }

  [Test]
  [Category("HappyPath")]
  public void YCbCrF_ToStudioRange_ReturnsCorrectRange() {
    var ycbcr = new YCbCrF(0.5f, 0f, 0f);
    var (y, cb, cr) = ycbcr.ToStudioRange();

    Assert.That(y, Is.InRange((byte)16, (byte)235));
    Assert.That(cb, Is.EqualTo(128).Within(2));
    Assert.That(cr, Is.EqualTo(128).Within(2));
  }

  #endregion

  #region XYZ Tests

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToXyz_White_HasExpectedY() {
    var xyz = new LinearRgbFToXyzF().Project(White);

    Assert.That(xyz.Y, Is.EqualTo(1f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void LinearRgbToXyz_Black_HasZeroComponents() {
    var xyz = new LinearRgbFToXyzF().Project(Black);

    Assert.That(xyz.X, Is.EqualTo(0f).Within(Tolerance));
    Assert.That(xyz.Y, Is.EqualTo(0f).Within(Tolerance));
    Assert.That(xyz.Z, Is.EqualTo(0f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void XyzToLinearRgb_Roundtrip_PreservesColor() {
    var xyz = new LinearRgbFToXyzF().Project(MixedColor);
    var backToRgb = new XyzFToLinearRgbF().Project(xyz);

    Assert.That(backToRgb.R, Is.EqualTo(MixedColor.R).Within(Tolerance));
    Assert.That(backToRgb.G, Is.EqualTo(MixedColor.G).Within(Tolerance));
    Assert.That(backToRgb.B, Is.EqualTo(MixedColor.B).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void XyzF_Create_WorksCorrectly() {
    var xyz = XyzF.Create(0.4f, 0.5f, 0.6f);

    Assert.That(xyz.X, Is.EqualTo(0.4f));
    Assert.That(xyz.Y, Is.EqualTo(0.5f));
    Assert.That(xyz.Z, Is.EqualTo(0.6f));
  }

  #endregion

  #region Primary Color Tests

  [Test]
  [Category("HappyPath")]
  public void AllProjectors_PrimaryColors_Roundtrip() {
    var colors = new[] { Red, Green, Blue, White, Black, Gray };

    foreach (var original in colors) {
      // OkLCh
      var fromOklch = new OklchFToLinearRgbF().Project(new LinearRgbFToOklchF().Project(original));
      Assert.That(fromOklch.R, Is.EqualTo(original.R).Within(Tolerance), $"OkLCh failed for {original}");

      // JzAzBz
      var fromJzAzBz = new JzAzBzFToLinearRgbF().Project(new LinearRgbFToJzAzBzF().Project(original));
      Assert.That(fromJzAzBz.R, Is.EqualTo(original.R).Within(Tolerance), $"JzAzBz failed for {original}");

      // ICtCp
      var fromICtCp = new ICtCpFToLinearRgbF().Project(new LinearRgbFToICtCpF().Project(original));
      Assert.That(fromICtCp.R, Is.EqualTo(original.R).Within(Tolerance), $"ICtCp failed for {original}");

      // Display P3
      var fromP3 = new DisplayP3FToLinearRgbF().Project(new LinearRgbFToDisplayP3F().Project(original));
      Assert.That(fromP3.R, Is.EqualTo(original.R).Within(Tolerance), $"Display P3 failed for {original}");

      // Adobe RGB
      var fromAdobe = new AdobeRgbFToLinearRgbF().Project(new LinearRgbFToAdobeRgbF().Project(original));
      Assert.That(fromAdobe.R, Is.EqualTo(original.R).Within(Tolerance), $"Adobe RGB failed for {original}");
    }
  }

  #endregion

}
