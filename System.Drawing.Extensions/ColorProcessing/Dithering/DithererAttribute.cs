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

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Defines the type/category of a dithering algorithm.
/// </summary>
public enum DitheringType {
  /// <summary>No dithering - nearest neighbor quantization only.</summary>
  None,
  /// <summary>Ordered dithering using threshold matrices (e.g., Bayer patterns).</summary>
  Ordered,
  /// <summary>Error diffusion dithering (e.g., Floyd-Steinberg, Atkinson).</summary>
  ErrorDiffusion,
  /// <summary>Blue noise dithering using void-and-cluster patterns.</summary>
  Noise,
  /// <summary>Random/noise-based dithering.</summary>
  Random,
  Custom,
}

/// <summary>
/// Attribute to provide metadata for dithering algorithm implementations.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class DithererAttribute : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="DithererAttribute"/> class.
  /// </summary>
  /// <param name="name">The display name of the dithering algorithm.</param>
  public DithererAttribute(string name) => this.Name = name;

  /// <summary>
  /// Gets the display name of the dithering algorithm.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets or sets the description of the algorithm.
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Gets or sets the type of dithering algorithm.
  /// </summary>
  public DitheringType Type { get; set; }

  /// <summary>
  /// Gets or sets the algorithm author.
  /// </summary>
  public string? Author { get; set; }

  /// <summary>
  /// Gets or sets the year the algorithm was published.
  /// </summary>
  public int Year { get; set; }

}
