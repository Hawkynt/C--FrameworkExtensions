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

using System.Drawing.Extensions.ColorProcessing.Resizing;
using Hawkynt.ColorProcessing.Codecs;

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Represents a pixel-art scaling algorithm that processes a 5x5 neighborhood
/// and writes scaled pixels directly to the destination.
/// </summary>
/// <typeparam name="TWork">The working color type (for interpolation).</typeparam>
/// <typeparam name="TKey">The key color type (for pattern matching).</typeparam>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
/// <remarks>
/// <para>
/// Implementations are stateless structs for zero-cost abstraction via generic dispatch.
/// The Scale method receives the current window position and writes directly to the destination
/// buffer, encoding TWork values to TPixel format.
/// </para>
/// <para>
/// Pattern matching decisions are made in TKey space, while interpolation operates in TWork space.
/// This separation allows tolerance-based equality without assuming TWork identity.
/// </para>
/// </remarks>
public interface IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <summary>
  /// The horizontal scaling factor.
  /// </summary>
  int ScaleX { get; }

  /// <summary>
  /// The vertical scaling factor.
  /// </summary>
  int ScaleY { get; }

  /// <summary>
  /// Scales a single source pixel and writes directly to the destination.
  /// </summary>
  /// <param name="window">The 5x5 neighborhood centered on the source pixel.</param>
  /// <param name="destTopLeft">Pointer to the top-left pixel of the output block. Caller advances by ScaleX after each call.</param>
  /// <param name="destStride">Stride of the destination buffer in pixels.</param>
  /// <param name="encoder">The encoder to convert TWork to TPixel.</param>
  /// <remarks>
  /// Kernels write to destTopLeft[row * stride + col] for row in [0, ScaleY-1] and col in [0, ScaleX-1].
  /// </remarks>
  unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  );
}
