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

using System.Collections.Generic;

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Public marker interface for color quantizers with configurable parameters.
/// </summary>
/// <remarks>
/// <para>
/// Quantizers implementing this interface are structs with configurable properties
/// that can create internal workers for different color spaces.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var quantizer = new OctreeQuantizer();
/// using var indexed = bitmap.ReduceColors&lt;OctreeQuantizer, FloydSteinberg&gt;(quantizer, default, 16);
/// </code>
/// </example>
public interface IQuantizer {

  /// <summary>
  /// Creates a quantizer worker for the specified color space.
  /// </summary>
  /// <returns>A quantizer worker operating in the specified color space.</returns>
  internal IQuantizer<TWork> CreateKernel<TWork>() where TWork : unmanaged, IColorSpace4<TWork>;

}

/// <summary>
/// Defines the interface for color quantization algorithm workers.
/// </summary>
/// <typeparam name="TWork">The working color space type (e.g., OklabaF, LinearRgbaF).</typeparam>
/// <remarks>
/// <para>
/// Color quantization reduces the number of distinct colors while preserving visual quality.
/// By operating in a chosen color space, quantizers can optimize for perceptual uniformity
/// (OkLab), physical accuracy (LinearRGB), or speed.
/// </para>
/// <para>Common use cases include:</para>
/// <list type="bullet">
/// <item><description>GIF/PNG8 palette generation</description></item>
/// <item><description>Retro game palette reduction</description></item>
/// <item><description>Color analysis and clustering</description></item>
/// </list>
/// </remarks>
internal interface IQuantizer<TWork>
  where TWork : unmanaged, IColorSpace4<TWork> {

  /// <summary>
  /// Generates a color palette of the specified size from a color histogram.
  /// </summary>
  /// <param name="histogram">The color histogram where each entry contains a color in TWork space and its occurrence count.</param>
  /// <param name="colorCount">The desired number of colors in the palette (1-256).</param>
  /// <returns>An array of colors in TWork space representing the optimized palette.</returns>
  /// <remarks>
  /// <para>
  /// The histogram provides weighted importance for each color based on its frequency.
  /// Colors with higher counts will have more influence on the resulting palette.
  /// </para>
  /// <para>
  /// The histogram should be built by deduplicating in storage space (Bgra8888) first,
  /// then decoding each unique color to TWork once for efficiency.
  /// </para>
  /// </remarks>
  TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount);
}
