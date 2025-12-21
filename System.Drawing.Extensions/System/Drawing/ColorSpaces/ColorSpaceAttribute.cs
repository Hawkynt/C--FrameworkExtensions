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

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Defines the type/category of a color space.
/// </summary>
public enum ColorSpaceType {
  /// <summary>
  /// Additive color model (e.g., RGB).
  /// </summary>
  Additive,

  /// <summary>
  /// Subtractive color model (e.g., CMYK).
  /// </summary>
  Subtractive,

  /// <summary>
  /// Perceptually uniform color space (e.g., Lab, Luv).
  /// </summary>
  Perceptual,

  /// <summary>
  /// Cylindrical color space (e.g., HSL, HSV, LCh).
  /// </summary>
  Cylindrical,

  /// <summary>
  /// Luminance-chrominance color space (e.g., YUV, YCbCr).
  /// </summary>
  LuminanceChrominance,

  /// <summary>
  /// CIE standard color space (e.g., XYZ).
  /// </summary>
  CieStandard
}

/// <summary>
/// Attribute to provide metadata for color space implementations.
/// </summary>
/// <param name="componentCount">The number of color components (excluding alpha).</param>
/// <param name="componentNames">The names of each component.</param>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class ColorSpaceAttribute(int componentCount, params string[] componentNames) : Attribute {

  /// <summary>
  /// Gets the number of color components (excluding alpha).
  /// </summary>
  public int ComponentCount { get; } = componentCount;

  /// <summary>
  /// Gets the names of each component.
  /// </summary>
  public string[] ComponentNames { get; } = componentNames;

  /// <summary>
  /// Gets or sets the type of color space.
  /// </summary>
  public ColorSpaceType ColorSpaceType { get; set; }

  /// <summary>
  /// Gets or sets the display name for UI purposes.
  /// </summary>
  public string? DisplayName { get; set; }

  /// <summary>
  /// Gets or sets whether this color space is perceptually uniform.
  /// </summary>
  public bool IsPerceptuallyUniform { get; set; }

  /// <summary>
  /// Gets or sets the white point reference (e.g., "D65", "D50").
  /// </summary>
  public string? WhitePoint { get; set; }

}
