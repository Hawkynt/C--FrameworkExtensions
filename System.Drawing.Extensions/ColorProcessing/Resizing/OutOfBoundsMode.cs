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

namespace System.Drawing.Extensions.ColorProcessing.Resizing;

/// <summary>
/// Specifies how to handle pixel access beyond image boundaries.
/// </summary>
public enum OutOfBoundsMode {
  /// <summary>
  /// Extends edge pixels: aaa|abcde|eee
  /// </summary>
  ConstantExtension,

  /// <summary>
  /// Mirrors at half-pixel positions: cba|abcde|edc
  /// </summary>
  HalfSampleSymmetric,

  /// <summary>
  /// Mirrors at pixel centers: dcb|abcde|dcb
  /// </summary>
  WholeSampleSymmetric,

  /// <summary>
  /// Wraps around (tileable): cde|abcde|abc
  /// </summary>
  WrapAround,

  /// <summary>
  /// Returns a user-supplied flat canvas colour (typically transparent): 000|abcde|000
  /// </summary>
  FlatColor
}
