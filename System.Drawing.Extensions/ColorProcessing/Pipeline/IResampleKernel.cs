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

using Hawkynt.ColorProcessing.Codecs;

namespace Hawkynt.ColorProcessing.Pipeline;

/// <summary>
/// Represents a resampling kernel that uses arbitrary pixel access via NeighborFrame.
/// </summary>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TWork">The working color type (for accumulation).</typeparam>
/// <typeparam name="TKey">The key color type (for NeighborFrame compatibility).</typeparam>
/// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
/// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
/// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
/// <remarks>
/// <para>
/// Unlike pixel-art scalers that use the fixed 5x5 NeighborWindow,
/// resamplers use NeighborFrame's indexer for arbitrary radius access.
/// This enables Lanczos, Bicubic, and other filter-based algorithms
/// with configurable kernel sizes (Lanczos-2, Lanczos-3, Lanczos-5, etc.).
/// </para>
/// <para>
/// Kernel implementations handle accumulation internally using IAccum with proper precision.
/// </para>
/// </remarks>
public interface IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
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
  /// The kernel radius (e.g., 2 for Lanczos-2, 3 for Lanczos-3).
  /// </summary>
  /// <remarks>
  /// The kernel samples from -(radius-1) to +radius in each dimension.
  /// </remarks>
  int Radius { get; }

  /// <summary>
  /// Resamples a single source pixel and writes scaled output to the destination.
  /// </summary>
  /// <param name="frame">The source frame with random access indexer.</param>
  /// <param name="srcX">The source X coordinate.</param>
  /// <param name="srcY">The source Y coordinate.</param>
  /// <param name="destTopLeft">Pointer to the top-left pixel of the output block.</param>
  /// <param name="destStride">Stride of the destination buffer in pixels.</param>
  /// <param name="encoder">The encoder to convert TWork to TPixel.</param>
  /// <remarks>
  /// <para>
  /// The kernel writes ScaleX × ScaleY output pixels starting at destTopLeft.
  /// For each output pixel, it samples source pixels in a radius-dependent neighborhood
  /// using the frame's indexer: frame[srcX + dx, srcY + dy].
  /// </para>
  /// <para>
  /// Implementations use internal accumulation for weighted averaging.
  /// </para>
  /// </remarks>
  unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int srcX,
    int srcY,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  );
}
