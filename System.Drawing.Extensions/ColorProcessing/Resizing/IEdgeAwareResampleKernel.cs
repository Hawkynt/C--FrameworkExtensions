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
using Hawkynt.ColorProcessing.Metrics;

namespace System.Drawing.Extensions.ColorProcessing.Resizing;

/// <summary>
/// Represents an edge-aware resampling kernel that uses color equality for similarity detection.
/// </summary>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TWork">The working color type (for interpolation).</typeparam>
/// <typeparam name="TKey">The key color type (for similarity detection).</typeparam>
/// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
/// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
/// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
/// <typeparam name="TEquality">The color equality type for similarity detection.</typeparam>
/// <remarks>
/// <para>
/// Edge-aware kernels build similarity graphs between pixels and use them to
/// preserve edges during resampling. The equality predicate determines which
/// pixels are considered "similar" and should blend together.
/// </para>
/// <para>
/// Implementations may lazily build their similarity graphs on first access
/// using the frame's random access indexer.
/// </para>
/// </remarks>
public interface IEdgeAwareResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TEquality>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TEquality : struct, IColorEquality<TKey> {

  /// <summary>
  /// The kernel radius for neighborhood access.
  /// </summary>
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
  /// Resamples a single destination pixel using edge-aware interpolation.
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
  /// uses the similarity graph to determine edge relationships, and
  /// writes the interpolated result to dest[destY * destStride + destX].
  /// </para>
  /// <para>
  /// Implementations may lazily initialize similarity data on first call.
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
