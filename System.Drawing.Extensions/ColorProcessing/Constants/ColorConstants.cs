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
}
