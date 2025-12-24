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

namespace Hawkynt.ColorProcessing.Codecs;

/// <summary>
/// Projects a working color to a key space for distance/equality calculations.
/// </summary>
/// <typeparam name="TWork">The working color type (e.g., LinearRgbaF).</typeparam>
/// <typeparam name="TKey">The key color type (e.g., YuvF, LabF).</typeparam>
/// <remarks>
/// Implementations are stateless structs for zero-cost abstraction via generic dispatch.
/// Key space is used for perceptual distance calculations in quantization and dithering.
/// Example: <c>LinearRgbaFToLabF</c> projects linear RGB to CIELAB for perceptual metrics.
/// </remarks>
public interface IProject<TWork, TKey>
  where TWork : unmanaged
  where TKey : unmanaged {

  /// <summary>
  /// Projects a working space color to key space.
  /// </summary>
  /// <param name="color">The working space color to project.</param>
  /// <returns>The key space color.</returns>
  TKey Project(in TWork color);
}
