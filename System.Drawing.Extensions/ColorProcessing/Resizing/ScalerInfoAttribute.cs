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

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Categorizes the type of scaling algorithm.
/// </summary>
public enum ScalerCategory {

  /// <summary>
  /// Pixel-art specific scalers that preserve hard edges and use discrete scale factors.
  /// </summary>
  PixelArt,

  /// <summary>
  /// General-purpose resamplers that support continuous scale factors.
  /// </summary>
  Resampler,

  /// <summary>
  /// Neural network or AI-based upscaling algorithms.
  /// </summary>
  Neural
}

/// <summary>
/// Provides metadata about a scaling algorithm.
/// </summary>
/// <remarks>
/// Apply this attribute to scaler structs to provide display name,
/// author information, and reference URLs.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class ScalerInfoAttribute : Attribute {

  /// <summary>
  /// Gets the display name of the scaler.
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
  /// Gets or sets the category of the scaler.
  /// </summary>
  public ScalerCategory Category { get; init; } = ScalerCategory.PixelArt;

  /// <summary>
  /// Initializes a new instance of the <see cref="ScalerInfoAttribute"/> class.
  /// </summary>
  /// <param name="name">The display name of the scaler.</param>
  public ScalerInfoAttribute(string name) => this.Name = name;
}
