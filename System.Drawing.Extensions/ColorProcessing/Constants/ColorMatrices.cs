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

  #region BT.601/709 Luminance Coefficients

  /// <summary>ITU-R BT.601 (SDTV) red luminance coefficient.</summary>
  public const float BT601_R = 0.299f;

  /// <summary>ITU-R BT.601 (SDTV) green luminance coefficient.</summary>
  public const float BT601_G = 0.587f;

  /// <summary>ITU-R BT.601 (SDTV) blue luminance coefficient.</summary>
  public const float BT601_B = 0.114f;

  /// <summary>ITU-R BT.709 (HDTV) red luminance coefficient.</summary>
  public const float BT709_R = 0.2126f;

  /// <summary>ITU-R BT.709 (HDTV) green luminance coefficient.</summary>
  public const float BT709_G = 0.7152f;

  /// <summary>ITU-R BT.709 (HDTV) blue luminance coefficient.</summary>
  public const float BT709_B = 0.0722f;

  #endregion

  #region D65 White Point

  /// <summary>D65 reference white X component.</summary>
  public const float D65_Xn = 0.95047f;

  /// <summary>D65 reference white Y component.</summary>
  public const float D65_Yn = 1.00000f;

  /// <summary>D65 reference white Z component.</summary>
  public const float D65_Zn = 1.08883f;

  /// <summary>1/D65_Xn for faster division.</summary>
  public const float Inv_D65_Xn = 1f / D65_Xn;

  /// <summary>1/D65_Yn for faster division (always 1.0).</summary>
  public const float Inv_D65_Yn = 1f / D65_Yn;

  /// <summary>1/D65_Zn for faster division.</summary>
  public const float Inv_D65_Zn = 1f / D65_Zn;

  #endregion

  #region Lab Constants

  /// <summary>Lab epsilon (6/29)^3 = 216/24389.</summary>
  public const float Lab_Epsilon = 216f / 24389f;

  /// <summary>Lab kappa (29/3)^3 = 24389/27.</summary>
  public const float Lab_Kappa = 24389f / 27f;

  /// <summary>Lab delta 6/29.</summary>
  public const float Lab_Delta = 6f / 29f;

  #endregion

  #region Luv Constants

  /// <summary>Pre-calculated u'n for D65: 4 * Xn / (Xn + 15 * Yn + 3 * Zn).</summary>
  public const float Luv_Un = 0.19783691f;

  /// <summary>Pre-calculated v'n for D65: 9 * Yn / (Xn + 15 * Yn + 3 * Zn).</summary>
  public const float Luv_Vn = 0.46831999f;

  #endregion

  #region Oklab Transformation Coefficients

  // RGB to LMS matrix row for L
  /// <summary>Oklab RGB→LMS L-row red coefficient.</summary>
  public const float Oklab_L_R = 0.4122214708f;

  /// <summary>Oklab RGB→LMS L-row green coefficient.</summary>
  public const float Oklab_L_G = 0.5363325363f;

  /// <summary>Oklab RGB→LMS L-row blue coefficient.</summary>
  public const float Oklab_L_B = 0.0514459929f;

  // RGB to LMS matrix row for M
  /// <summary>Oklab RGB→LMS M-row red coefficient.</summary>
  public const float Oklab_M_R = 0.2119034982f;

  /// <summary>Oklab RGB→LMS M-row green coefficient.</summary>
  public const float Oklab_M_G = 0.6806995451f;

  /// <summary>Oklab RGB→LMS M-row blue coefficient.</summary>
  public const float Oklab_M_B = 0.1073969566f;

  // RGB to LMS matrix row for S
  /// <summary>Oklab RGB→LMS S-row red coefficient.</summary>
  public const float Oklab_S_R = 0.0883024619f;

  /// <summary>Oklab RGB→LMS S-row green coefficient.</summary>
  public const float Oklab_S_G = 0.2817188376f;

  /// <summary>Oklab RGB→LMS S-row blue coefficient.</summary>
  public const float Oklab_S_B = 0.6299787005f;

  // LMS' to Oklab transformation matrix
  /// <summary>Oklab LMS'→L L coefficient.</summary>
  public const float Oklab_ToL_L = 0.2104542553f;

  /// <summary>Oklab LMS'→L M coefficient.</summary>
  public const float Oklab_ToL_M = 0.7936177850f;

  /// <summary>Oklab LMS'→L S coefficient.</summary>
  public const float Oklab_ToL_S = -0.0040720468f;

  /// <summary>Oklab LMS'→a L coefficient.</summary>
  public const float Oklab_ToA_L = 1.9779984951f;

  /// <summary>Oklab LMS'→a M coefficient.</summary>
  public const float Oklab_ToA_M = -2.4285922050f;

  /// <summary>Oklab LMS'→a S coefficient.</summary>
  public const float Oklab_ToA_S = 0.4505937099f;

  /// <summary>Oklab LMS'→b L coefficient.</summary>
  public const float Oklab_ToB_L = 0.0259040371f;

  /// <summary>Oklab LMS'→b M coefficient.</summary>
  public const float Oklab_ToB_M = 0.7827717662f;

  /// <summary>Oklab LMS'→b S coefficient.</summary>
  public const float Oklab_ToB_S = -0.8086757660f;

  // Oklab to LMS' inverse transformation matrix
  /// <summary>Oklab Lab→LMS' L-row L coefficient.</summary>
  public const float Oklab_FromL_L = 1f;

  /// <summary>Oklab Lab→LMS' L-row a coefficient.</summary>
  public const float Oklab_FromL_A = 0.3963377774f;

  /// <summary>Oklab Lab→LMS' L-row b coefficient.</summary>
  public const float Oklab_FromL_B = 0.2158037573f;

  /// <summary>Oklab Lab→LMS' M-row L coefficient.</summary>
  public const float Oklab_FromM_L = 1f;

  /// <summary>Oklab Lab→LMS' M-row a coefficient.</summary>
  public const float Oklab_FromM_A = -0.1055613458f;

  /// <summary>Oklab Lab→LMS' M-row b coefficient.</summary>
  public const float Oklab_FromM_B = -0.0638541728f;

  /// <summary>Oklab Lab→LMS' S-row L coefficient.</summary>
  public const float Oklab_FromS_L = 1f;

  /// <summary>Oklab Lab→LMS' S-row a coefficient.</summary>
  public const float Oklab_FromS_A = -0.0894841775f;

  /// <summary>Oklab Lab→LMS' S-row b coefficient.</summary>
  public const float Oklab_FromS_B = -1.2914855480f;

  // LMS to Linear RGB inverse transformation matrix
  /// <summary>Oklab LMS→RGB R-row L coefficient.</summary>
  public const float Oklab_ToR_L = 4.0767416621f;

  /// <summary>Oklab LMS→RGB R-row M coefficient.</summary>
  public const float Oklab_ToR_M = -3.3077115913f;

  /// <summary>Oklab LMS→RGB R-row S coefficient.</summary>
  public const float Oklab_ToR_S = 0.2309699292f;

  /// <summary>Oklab LMS→RGB G-row L coefficient.</summary>
  public const float Oklab_ToG_L = -1.2684380046f;

  /// <summary>Oklab LMS→RGB G-row M coefficient.</summary>
  public const float Oklab_ToG_M = 2.6097574011f;

  /// <summary>Oklab LMS→RGB G-row S coefficient.</summary>
  public const float Oklab_ToG_S = -0.3413193965f;

  /// <summary>Oklab LMS→RGB B-row L coefficient.</summary>
  public const float Oklab_ToB_L_Inv = -0.0041960863f;

  /// <summary>Oklab LMS→RGB B-row M coefficient.</summary>
  public const float Oklab_ToB_M_Inv = -0.7034186147f;

  /// <summary>Oklab LMS→RGB B-row S coefficient.</summary>
  public const float Oklab_ToB_S_Inv = 1.7076147010f;

  #endregion

  #region Reciprocal Constants

  /// <summary>1/3 for averaging three components.</summary>
  public const float Inv3 = 1f / 3f;

  /// <summary>1/6 for hue normalization (360°/60° = 6 sectors).</summary>
  public const float Inv6 = 1f / 6f;

  /// <summary>1/100 for L* normalization.</summary>
  public const float Inv100 = 0.01f;

  /// <summary>1/116 for Lab f-function.</summary>
  public const float Inv116 = 1f / 116f;

  /// <summary>1/200 for Lab B component.</summary>
  public const float Inv200 = 1f / 200f;

  /// <summary>1/255 for byte normalization.</summary>
  public const float Inv255 = 1f / 255f;

  /// <summary>1/360 for hue degree normalization.</summary>
  public const float Inv360 = 1f / 360f;

  /// <summary>1/500 for Lab A component.</summary>
  public const float Inv500 = 1f / 500f;

  #endregion
}
