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
/// Common color processing constants.
/// </summary>
public static class ColorConstants {

  #region Byte/UShort Normalization

  /// <summary>Reciprocal of 255 for fast byte-to-normalized-float conversion.</summary>
  public const float ByteToFloat = 1f / 255f;

  /// <summary>Multiplier for normalized-float-to-byte conversion.</summary>
  public const float FloatToByte = 255f;

  /// <summary>Reciprocal of 65535 for fast ushort-to-normalized-float conversion.</summary>
  public const float UShortToFloat = 1f / 65535f;

  /// <summary>Multiplier for normalized-float-to-ushort conversion.</summary>
  public const float FloatToUShort = 65535f;

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

  // LMS' to Oklab L transformation vector
  /// <summary>Oklab LMS'→L L coefficient.</summary>
  public const float Oklab_ToL_L = 0.2104542553f;

  /// <summary>Oklab LMS'→L M coefficient.</summary>
  public const float Oklab_ToL_M = 0.7936177850f;

  /// <summary>Oklab LMS'→L S coefficient.</summary>
  public const float Oklab_ToL_S = -0.0040720468f;

  #endregion
}
