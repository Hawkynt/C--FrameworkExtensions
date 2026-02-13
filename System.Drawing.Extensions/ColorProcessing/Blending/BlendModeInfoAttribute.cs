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

using System;

namespace Hawkynt.ColorProcessing.Blending;

/// <summary>
/// Categorizes the type of blend mode.
/// </summary>
public enum BlendModeCategory {

  /// <summary>
  /// Normal compositing modes.
  /// </summary>
  Normal,

  /// <summary>
  /// Darkening blend modes (Multiply, ColorBurn, Darken, LinearBurn).
  /// </summary>
  Darken,

  /// <summary>
  /// Lightening blend modes (Screen, ColorDodge, Lighten, Add, LinearDodge).
  /// </summary>
  Lighten,

  /// <summary>
  /// Contrast blend modes (Overlay, SoftLight, HardLight, VividLight, LinearLight, PinLight, HardMix).
  /// </summary>
  Contrast,

  /// <summary>
  /// Inversion blend modes (Difference, Exclusion, Subtract, Divide).
  /// </summary>
  Inversion,

  /// <summary>
  /// HSL component blend modes (Hue, Saturation, Color, Luminosity).
  /// </summary>
  Component,

  /// <summary>
  /// Other blend modes (GrainExtract, GrainMerge).
  /// </summary>
  Other
}

/// <summary>
/// Provides metadata about a blend mode algorithm.
/// </summary>
/// <remarks>
/// Apply this attribute to blend mode structs to provide display name,
/// description, and category information.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class BlendModeInfoAttribute : Attribute {

  /// <summary>
  /// Gets the display name of the blend mode.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets or sets a description of the blend mode.
  /// </summary>
  public string? Description { get; init; }

  /// <summary>
  /// Gets or sets the category of the blend mode.
  /// </summary>
  public BlendModeCategory Category { get; init; } = BlendModeCategory.Normal;

  /// <summary>
  /// Initializes a new instance of the <see cref="BlendModeInfoAttribute"/> class.
  /// </summary>
  /// <param name="name">The display name of the blend mode.</param>
  public BlendModeInfoAttribute(string name) => this.Name = name;
}
