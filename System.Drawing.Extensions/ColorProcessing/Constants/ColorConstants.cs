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
}
