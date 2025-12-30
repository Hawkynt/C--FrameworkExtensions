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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.Extensions.ColorProcessing.Resizing;

/// <summary>
/// Provides pipeline operations for color processing.
/// </summary>
public static class WorkPipeline {

  /// <summary>
  /// Decodes a pixel frame to a work frame.
  /// </summary>
  /// <typeparam name="TPixel">Storage pixel type.</typeparam>
  /// <typeparam name="TWork">Working color type.</typeparam>
  /// <typeparam name="TDecode">Decoder strategy.</typeparam>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static WorkFrame<TWork> Decode<TPixel, TWork, TDecode>(
    in PixelFrame<TPixel> source,
    TDecode decoder = default
  )
    where TPixel : unmanaged, IStorageSpace
    where TWork : unmanaged, IColorSpace
    where TDecode : struct, IDecode<TPixel, TWork> {
    var result = WorkFrame<TWork>.Rent(source.Width, source.Height);
    var destSpan = result.Span;
    var srcSpan = source.Pixels;

    for (var y = 0; y < source.Height; ++y) {
      var srcRow = srcSpan.Slice(y * source.Stride, source.Width);
      var dstRow = destSpan.Slice(y * source.Width, source.Width);
      for (var x = 0; x < source.Width; ++x)
        dstRow[x] = decoder.Decode(srcRow[x]);
    }

    return result;
  }

  /// <summary>
  /// Encodes a work frame to a pixel frame.
  /// </summary>
  /// <typeparam name="TWork">Working color type.</typeparam>
  /// <typeparam name="TPixel">Storage pixel type.</typeparam>
  /// <typeparam name="TEncode">Encoder strategy.</typeparam>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Encode<TWork, TPixel, TEncode>(
    in WorkFrame<TWork> source,
    in PixelFrame<TPixel> dest,
    TEncode encoder = default
  )
    where TWork : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TEncode : struct, IEncode<TWork, TPixel> {
    var srcSpan = source.Span;
    var dstSpan = dest.Pixels;

    for (var y = 0; y < source.Height; ++y) {
      var srcRow = srcSpan.Slice(y * source.Width, source.Width);
      var dstRow = dstSpan.Slice(y * dest.Stride, dest.Width);
      for (var x = 0; x < source.Width; ++x)
        dstRow[x] = encoder.Encode(srcRow[x]);
    }
  }

  /// <summary>
  /// Projects work colors to key colors for equality/distance comparisons.
  /// </summary>
  /// <typeparam name="TWork">Working color type.</typeparam>
  /// <typeparam name="TKey">Key color type.</typeparam>
  /// <typeparam name="TProject">Projector strategy.</typeparam>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static WorkFrame<TKey> Project<TWork, TKey, TProject>(
    in WorkFrame<TWork> source,
    TProject projector = default
  )
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TProject : struct, IProject<TWork, TKey> {
    var result = WorkFrame<TKey>.Rent(source.Width, source.Height);
    var srcSpan = source.Span;
    var dstSpan = result.Span;

    for (var i = 0; i < srcSpan.Length; ++i)
      dstSpan[i] = projector.Project(srcSpan[i]);

    return result;
  }

  /// <summary>
  /// Applies a transformation to each pixel in the work frame.
  /// </summary>
  /// <typeparam name="TWork">Working color type.</typeparam>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Apply<TWork>(
    ref WorkFrame<TWork> frame,
    Func<TWork, TWork> transform
  ) where TWork : unmanaged, IColorSpace {
    var span = frame.Span;
    for (var i = 0; i < span.Length; ++i)
      span[i] = transform(span[i]);
  }

  /// <summary>
  /// Applies a transformation with coordinates to each pixel.
  /// </summary>
  /// <typeparam name="TWork">Working color type.</typeparam>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ApplyWithCoords<TWork>(
    ref WorkFrame<TWork> frame,
    Func<TWork, int, int, TWork> transform
  ) where TWork : unmanaged, IColorSpace {
    for (var y = 0; y < frame.Height; ++y) {
      var row = frame.GetRow(y);
      for (var x = 0; x < frame.Width; ++x)
        row[x] = transform(row[x], x, y);
    }
  }

  /// <summary>
  /// Creates a sliding neighbor window frame for pixel-art scaling algorithms.
  /// </summary>
  /// <typeparam name="TPixel">Storage pixel type.</typeparam>
  /// <typeparam name="TWork">Working color type (for interpolation).</typeparam>
  /// <typeparam name="TKey">Key color type (for pattern matching).</typeparam>
  /// <typeparam name="TDecode">Decoder strategy (TPixel → TWork).</typeparam>
  /// <typeparam name="TProject">Projector strategy (TWork → TKey).</typeparam>
  /// <param name="sourcePtr">Pointer to the source pixel data (must remain valid for the lifetime of the frame).</param>
  /// <param name="width">Width of the source image.</param>
  /// <param name="height">Height of the source image.</param>
  /// <param name="stride">Stride of the source image in pixels.</param>
  /// <param name="horizontalMode">How to handle horizontal out-of-bounds access.</param>
  /// <param name="verticalMode">How to handle vertical out-of-bounds access.</param>
  /// <param name="startY">The starting Y row (for parallel processing).</param>
  /// <param name="decoder">The decoder instance (default if not specified).</param>
  /// <param name="projector">The projector instance (default if not specified).</param>
  /// <returns>A disposable neighbor frame for sliding window access.</returns>
  /// <remarks>
  /// <para>
  /// The returned frame provides O(1) access to a 5x5 pixel neighborhood.
  /// Each pixel includes both Work and Key representations for efficient scaler algorithms.
  /// </para>
  /// <para>
  /// For parallel processing, create one NeighborFrame per partition using different startY values.
  /// Each frame independently manages its own 5-row buffer.
  /// </para>
  /// <para>
  /// The caller must ensure the source data remains pinned and valid for the lifetime of the returned frame.
  /// </para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> CreateNeighborFrame<TPixel, TWork, TKey, TDecode, TProject>(
    TPixel* sourcePtr,
    int width,
    int height,
    int stride,
    OutOfBoundsMode horizontalMode = OutOfBoundsMode.Const,
    OutOfBoundsMode verticalMode = OutOfBoundsMode.Const,
    int startY = 0,
    TDecode decoder = default,
    TProject projector = default
  )
    where TPixel : unmanaged, IStorageSpace
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    => new(sourcePtr, width, height, stride, decoder, projector, horizontalMode, verticalMode, startY);
}
