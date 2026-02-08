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

namespace Hawkynt.ColorProcessing.Filtering;

/// <summary>
/// Categorizes the type of pixel filter.
/// </summary>
public enum FilterCategory {

  /// <summary>
  /// Color correction filters that adjust white balance, color temperature, etc.
  /// </summary>
  ColorCorrection,

  /// <summary>
  /// Enhancement filters that improve image quality (sharpen, blur, denoise).
  /// </summary>
  Enhancement,

  /// <summary>
  /// Artistic filters that apply stylistic transformations.
  /// </summary>
  Artistic,

  /// <summary>
  /// Analysis filters that extract information from images (threshold, channel extraction).
  /// </summary>
  Analysis
}

/// <summary>
/// Categorizes the color channel for extraction.
/// </summary>
public enum ColorChannel {
  Red,
  Green,
  Blue,
  Alpha
}

/// <summary>
/// Provides metadata about a pixel filter algorithm.
/// </summary>
/// <remarks>
/// Apply this attribute to filter structs to provide display name,
/// author information, and reference URLs.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class FilterInfoAttribute : Attribute {

  /// <summary>
  /// Gets the display name of the filter.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets or sets the author of the algorithm.
  /// </summary>
  public string? Author { get; init; }

  /// <summary>
  /// Gets or sets the reference URL for the algorithm.
  /// </summary>
  public string? Url { get; init; }

  /// <summary>
  /// Gets or sets the year the algorithm was created.
  /// </summary>
  public int Year { get; init; }

  /// <summary>
  /// Gets or sets a description of the algorithm.
  /// </summary>
  public string? Description { get; init; }

  /// <summary>
  /// Gets or sets the category of the filter.
  /// </summary>
  public FilterCategory Category { get; init; } = FilterCategory.Enhancement;

  /// <summary>
  /// Initializes a new instance of the <see cref="FilterInfoAttribute"/> class.
  /// </summary>
  /// <param name="name">The display name of the filter.</param>
  public FilterInfoAttribute(string name) => this.Name = name;
}
