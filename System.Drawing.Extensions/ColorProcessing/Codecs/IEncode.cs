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
/// Encodes a working color space to a storage pixel format.
/// </summary>
/// <typeparam name="TWork">The working color type (e.g., LinearRgbaF).</typeparam>
/// <typeparam name="TPixel">The storage pixel type (e.g., Rgba32).</typeparam>
/// <remarks>
/// Implementations are stateless structs for zero-cost abstraction via generic dispatch.
/// Example: <c>LinearRgbaFToSrgb32</c> encodes linear float to sRGB bytes with gamma compression.
/// </remarks>
public interface IEncode<TWork, TPixel>
  where TWork : unmanaged
  where TPixel : unmanaged {

  /// <summary>
  /// Encodes a working space color to storage pixel.
  /// </summary>
  /// <param name="color">The working space color to encode.</param>
  /// <returns>The storage pixel.</returns>
  TPixel Encode(in TWork color);
}
