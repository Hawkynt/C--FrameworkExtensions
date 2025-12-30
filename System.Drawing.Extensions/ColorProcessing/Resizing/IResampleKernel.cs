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

using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;

namespace System.Drawing.Extensions.ColorProcessing.Resizing;

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
/// with configurable kernel sizes (Lanczos-2, Lanczos-3, etc.).
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
  /// The kernel radius (e.g., 2 for Lanczos-2, 3 for Lanczos-3).
  /// </summary>
  /// <remarks>
  /// The kernel samples from -(radius-1) to +radius in each dimension.
  /// </remarks>
  int Radius { get; }

  /// <summary>
  /// Gets the source image width.
  /// </summary>
  int SourceWidth { get; }

  /// <summary>
  /// Gets the source image height.
  /// </summary>
  int SourceHeight { get; }

  /// <summary>
  /// Gets the target image width.
  /// </summary>
  int TargetWidth { get; }

  /// <summary>
  /// Gets the target image height.
  /// </summary>
  int TargetHeight { get; }

  /// <summary>
  /// Resamples a single destination pixel by sampling from the source.
  /// </summary>
  /// <param name="frame">The source frame with random access indexer.</param>
  /// <param name="destX">The destination X coordinate.</param>
  /// <param name="destY">The destination Y coordinate.</param>
  /// <param name="dest">Pointer to the destination buffer.</param>
  /// <param name="destStride">Stride of the destination buffer in pixels.</param>
  /// <param name="encoder">The encoder to convert TWork to TPixel.</param>
  /// <remarks>
  /// <para>
  /// The kernel maps the destination pixel back to source coordinates,
  /// samples source pixels within its radius, and writes the interpolated
  /// result to dest[destY * destStride + destX].
  /// </para>
  /// <para>
  /// Implementations use internal accumulation for weighted averaging.
  /// </para>
  /// </remarks>
  unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX,
    int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder
  );
}
