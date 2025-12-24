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

namespace Hawkynt.ColorProcessing.Constants;

/// <summary>
/// Color transformation matrix coefficients.
/// </summary>
public static class ColorMatrices {

  #region sRGB to XYZ (D65 illuminant)

  /// <summary>sRGB R to XYZ X coefficient.</summary>
  public const float RgbToXyz_XR = 0.4124564f;

  /// <summary>sRGB G to XYZ X coefficient.</summary>
  public const float RgbToXyz_XG = 0.3575761f;

  /// <summary>sRGB B to XYZ X coefficient.</summary>
  public const float RgbToXyz_XB = 0.1804375f;

  /// <summary>sRGB R to XYZ Y coefficient.</summary>
  public const float RgbToXyz_YR = 0.2126729f;

  /// <summary>sRGB G to XYZ Y coefficient.</summary>
  public const float RgbToXyz_YG = 0.7151522f;

  /// <summary>sRGB B to XYZ Y coefficient.</summary>
  public const float RgbToXyz_YB = 0.0721750f;

  /// <summary>sRGB R to XYZ Z coefficient.</summary>
  public const float RgbToXyz_ZR = 0.0193339f;

  /// <summary>sRGB G to XYZ Z coefficient.</summary>
  public const float RgbToXyz_ZG = 0.1191920f;

  /// <summary>sRGB B to XYZ Z coefficient.</summary>
  public const float RgbToXyz_ZB = 0.9503041f;

  #endregion

  #region XYZ to sRGB (D65 illuminant)

  /// <summary>XYZ X to sRGB R coefficient.</summary>
  public const float XyzToRgb_RX = 3.2404542f;

  /// <summary>XYZ Y to sRGB R coefficient.</summary>
  public const float XyzToRgb_RY = -1.5371385f;

  /// <summary>XYZ Z to sRGB R coefficient.</summary>
  public const float XyzToRgb_RZ = -0.4985314f;

  /// <summary>XYZ X to sRGB G coefficient.</summary>
  public const float XyzToRgb_GX = -0.9692660f;

  /// <summary>XYZ Y to sRGB G coefficient.</summary>
  public const float XyzToRgb_GY = 1.8760108f;

  /// <summary>XYZ Z to sRGB G coefficient.</summary>
  public const float XyzToRgb_GZ = 0.0415560f;

  /// <summary>XYZ X to sRGB B coefficient.</summary>
  public const float XyzToRgb_BX = 0.0556434f;

  /// <summary>XYZ Y to sRGB B coefficient.</summary>
  public const float XyzToRgb_BY = -0.2040259f;

  /// <summary>XYZ Z to sRGB B coefficient.</summary>
  public const float XyzToRgb_BZ = 1.0572252f;

  #endregion

  #region Adobe RGB to XYZ (D65 illuminant)

  public const float AdobeToXyz_XR = 0.5767309f;
  public const float AdobeToXyz_XG = 0.1855540f;
  public const float AdobeToXyz_XB = 0.1881852f;
  public const float AdobeToXyz_YR = 0.2973769f;
  public const float AdobeToXyz_YG = 0.6273491f;
  public const float AdobeToXyz_YB = 0.0752741f;
  public const float AdobeToXyz_ZR = 0.0270343f;
  public const float AdobeToXyz_ZG = 0.0706872f;
  public const float AdobeToXyz_ZB = 0.9911085f;

  #endregion

  #region XYZ to Adobe RGB (D65 illuminant)

  public const float XyzToAdobe_RX = 2.0413690f;
  public const float XyzToAdobe_RY = -0.5649464f;
  public const float XyzToAdobe_RZ = -0.3446944f;
  public const float XyzToAdobe_GX = -0.9692660f;
  public const float XyzToAdobe_GY = 1.8760108f;
  public const float XyzToAdobe_GZ = 0.0415560f;
  public const float XyzToAdobe_BX = 0.0134474f;
  public const float XyzToAdobe_BY = -0.1183897f;
  public const float XyzToAdobe_BZ = 1.0154096f;

  #endregion

  #region Display P3 to XYZ (D65 illuminant)

  public const float P3ToXyz_XR = 0.4865709f;
  public const float P3ToXyz_XG = 0.2656677f;
  public const float P3ToXyz_XB = 0.1982173f;
  public const float P3ToXyz_YR = 0.2289746f;
  public const float P3ToXyz_YG = 0.6917385f;
  public const float P3ToXyz_YB = 0.0792869f;
  public const float P3ToXyz_ZR = 0.0000000f;
  public const float P3ToXyz_ZG = 0.0451134f;
  public const float P3ToXyz_ZB = 1.0439444f;

  #endregion

  #region XYZ to Display P3 (D65 illuminant)

  public const float XyzToP3_RX = 2.4934969f;
  public const float XyzToP3_RY = -0.9313836f;
  public const float XyzToP3_RZ = -0.4027108f;
  public const float XyzToP3_GX = -0.8294890f;
  public const float XyzToP3_GY = 1.7626641f;
  public const float XyzToP3_GZ = 0.0236247f;
  public const float XyzToP3_BX = 0.0358458f;
  public const float XyzToP3_BY = -0.0761724f;
  public const float XyzToP3_BZ = 0.9568845f;

  #endregion

  #region ProPhoto RGB to XYZ (D50 illuminant)

  public const float ProPhotoToXyz_XR = 0.7976749f;
  public const float ProPhotoToXyz_XG = 0.1351917f;
  public const float ProPhotoToXyz_XB = 0.0313534f;
  public const float ProPhotoToXyz_YR = 0.2880402f;
  public const float ProPhotoToXyz_YG = 0.7118741f;
  public const float ProPhotoToXyz_YB = 0.0000857f;
  public const float ProPhotoToXyz_ZR = 0.0000000f;
  public const float ProPhotoToXyz_ZG = 0.0000000f;
  public const float ProPhotoToXyz_ZB = 0.8252100f;

  #endregion

  #region XYZ to ProPhoto RGB (D50 illuminant)

  public const float XyzToProPhoto_RX = 1.3459433f;
  public const float XyzToProPhoto_RY = -0.2556075f;
  public const float XyzToProPhoto_RZ = -0.0511118f;
  public const float XyzToProPhoto_GX = -0.5445989f;
  public const float XyzToProPhoto_GY = 1.5081673f;
  public const float XyzToProPhoto_GZ = 0.0205351f;
  public const float XyzToProPhoto_BX = 0.0000000f;
  public const float XyzToProPhoto_BY = 0.0000000f;
  public const float XyzToProPhoto_BZ = 1.2118128f;

  #endregion

  #region ACEScg to XYZ (AP1 primaries)

  public const float AcesCgToXyz_XR = 0.6624542f;
  public const float AcesCgToXyz_XG = 0.1340042f;
  public const float AcesCgToXyz_XB = 0.1561877f;
  public const float AcesCgToXyz_YR = 0.2722287f;
  public const float AcesCgToXyz_YG = 0.6740818f;
  public const float AcesCgToXyz_YB = 0.0536895f;
  public const float AcesCgToXyz_ZR = -0.0055746f;
  public const float AcesCgToXyz_ZG = 0.0040607f;
  public const float AcesCgToXyz_ZB = 1.0103391f;

  #endregion

  #region XYZ to ACEScg (AP1 primaries)

  public const float XyzToAcesCg_RX = 1.6410234f;
  public const float XyzToAcesCg_RY = -0.3248033f;
  public const float XyzToAcesCg_RZ = -0.2364247f;
  public const float XyzToAcesCg_GX = -0.6636629f;
  public const float XyzToAcesCg_GY = 1.6153316f;
  public const float XyzToAcesCg_GZ = 0.0167563f;
  public const float XyzToAcesCg_BX = 0.0117219f;
  public const float XyzToAcesCg_BY = -0.0082844f;
  public const float XyzToAcesCg_BZ = 0.9883949f;

  #endregion

  #region Bradford Chromatic Adaptation D65 to D50

  public const float Brad65To50_XX = 1.0478112f;
  public const float Brad65To50_XY = 0.0228866f;
  public const float Brad65To50_XZ = -0.0501270f;
  public const float Brad65To50_YX = 0.0295424f;
  public const float Brad65To50_YY = 0.9904844f;
  public const float Brad65To50_YZ = -0.0170491f;
  public const float Brad65To50_ZX = -0.0092345f;
  public const float Brad65To50_ZY = 0.0150436f;
  public const float Brad65To50_ZZ = 0.7521316f;

  #endregion

  #region Bradford Chromatic Adaptation D50 to D65

  public const float Brad50To65_XX = 0.9555766f;
  public const float Brad50To65_XY = -0.0230393f;
  public const float Brad50To65_XZ = 0.0631636f;
  public const float Brad50To65_YX = -0.0282895f;
  public const float Brad50To65_YY = 1.0099416f;
  public const float Brad50To65_YZ = 0.0210077f;
  public const float Brad50To65_ZX = 0.0122982f;
  public const float Brad50To65_ZY = -0.0204830f;
  public const float Brad50To65_ZZ = 1.3299098f;

  #endregion

  #region Bradford Chromatic Adaptation D65 to ACES (~D60)

  public const float Brad65ToAces_XX = 1.0163601f;
  public const float Brad65ToAces_XY = 0.0061227f;
  public const float Brad65ToAces_XZ = -0.0149715f;
  public const float Brad65ToAces_YX = 0.0076142f;
  public const float Brad65ToAces_YY = 0.9977354f;
  public const float Brad65ToAces_YZ = -0.0042827f;
  public const float Brad65ToAces_ZX = -0.0002960f;
  public const float Brad65ToAces_ZY = 0.0048125f;
  public const float Brad65ToAces_ZZ = 0.9246714f;

  #endregion

  #region Bradford Chromatic Adaptation ACES (~D60) to D65

  public const float BradAcesTo65_XX = 0.9839836f;
  public const float BradAcesTo65_XY = -0.0059818f;
  public const float BradAcesTo65_XZ = 0.0163152f;
  public const float BradAcesTo65_YX = -0.0075094f;
  public const float BradAcesTo65_YY = 1.0022652f;
  public const float BradAcesTo65_YZ = 0.0046629f;
  public const float BradAcesTo65_ZX = 0.0003140f;
  public const float BradAcesTo65_ZY = -0.0052908f;
  public const float BradAcesTo65_ZZ = 1.0814896f;

  #endregion
}
