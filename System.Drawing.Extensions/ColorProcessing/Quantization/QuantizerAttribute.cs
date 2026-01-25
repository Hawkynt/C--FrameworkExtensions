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

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Defines the type/category of a color quantization algorithm.
/// </summary>
public enum QuantizationType {
  /// <summary>Tree-based quantization (e.g., Octree).</summary>
  Tree,
  /// <summary>Clustering-based quantization (e.g., K-Means).</summary>
  Clustering,
  /// <summary>Splitting-based quantization (e.g., Median Cut, Wu).</summary>
  Splitting,
  /// <summary>Fixed palette quantization.</summary>
  Fixed,
  /// <summary>Variance-based quantization.</summary>
  Variance,
  /// <summary>Neural network-based quantization (e.g., NeuQuant).</summary>
  Neural,
  /// <summary>Preprocessing wrapper that modifies input before passing to inner quantizer.</summary>
  Preprocessing,
  /// <summary>Postprocessing wrapper that refines output from inner quantizer.</summary>
  Postprocessing
}

/// <summary>
/// Attribute to provide metadata for color quantization algorithm implementations.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class QuantizerAttribute : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="QuantizerAttribute"/> class.
  /// </summary>
  /// <param name="quantizationType">The type of quantization algorithm.</param>
  public QuantizerAttribute(QuantizationType quantizationType) => this.QuantizationType = quantizationType;

  /// <summary>
  /// Gets the type of quantization algorithm.
  /// </summary>
  public QuantizationType QuantizationType { get; }

  /// <summary>
  /// Gets or sets the display name for UI purposes.
  /// </summary>
  public string? DisplayName { get; set; }

  /// <summary>
  /// Gets or sets the algorithm author.
  /// </summary>
  public string? Author { get; set; }

  /// <summary>
  /// Gets or sets the year the algorithm was published.
  /// </summary>
  public int Year { get; set; }

  /// <summary>
  /// Gets or sets a quality rating (1-10, higher is better quality but potentially slower).
  /// </summary>
  public int QualityRating { get; set; }

}
