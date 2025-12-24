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
/// Decodes a storage pixel format to a working color space.
/// </summary>
/// <typeparam name="TPixel">The storage pixel type (e.g., Rgba32).</typeparam>
/// <typeparam name="TWork">The working color type (e.g., LinearRgbaF).</typeparam>
/// <remarks>
/// Implementations are stateless structs for zero-cost abstraction via generic dispatch.
/// Example: <c>Srgb32ToLinearRgbaF</c> decodes sRGB bytes to linear float with gamma expansion.
/// </remarks>
public interface IDecode<TPixel, TWork>
  where TPixel : unmanaged
  where TWork : unmanaged {

  /// <summary>
  /// Decodes a storage pixel to working space.
  /// </summary>
  /// <param name="pixel">The storage pixel to decode.</param>
  /// <returns>The working space color.</returns>
  TWork Decode(in TPixel pixel);
}
