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
using System.Collections.Concurrent;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Provides pipeline operations for executing image scalers.
/// </summary>
public static class ScalerPipeline {

  /// <summary>
  /// Executes a downscaling kernel on source pixel data, using parallel processing for large images.
  /// </summary>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TWork">The working color type (for averaging calculations).</typeparam>
  /// <typeparam name="TKey">The key color type (for pattern matching compatibility).</typeparam>
  /// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
  /// <typeparam name="TKernel">The downscale kernel type.</typeparam>
  /// <param name="sourcePtr">Pointer to source pixel data.</param>
  /// <param name="sourceWidth">Width of source image.</param>
  /// <param name="sourceHeight">Height of source image.</param>
  /// <param name="sourceStride">Stride of source image in pixels.</param>
  /// <param name="destPtr">Pointer to destination pixel data.</param>
  /// <param name="destWidth">Width of destination image.</param>
  /// <param name="destHeight">Height of destination image.</param>
  /// <param name="destStride">Stride of destination image in pixels.</param>
  /// <param name="kernel">The downscale kernel instance.</param>
  /// <param name="decoder">The decoder instance.</param>
  /// <param name="projector">The projector instance.</param>
  /// <param name="encoder">The encoder instance.</param>
  /// <param name="horizontalMode">How to handle horizontal out-of-bounds access.</param>
  /// <param name="verticalMode">How to handle vertical out-of-bounds access.</param>
  /// <remarks>
  /// <para>
  /// Unlike pixel-art upscalers that iterate each source pixel, downscalers iterate
  /// source pixels in steps of RatioX/RatioY, producing one output pixel per step.
  /// </para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void ExecuteDownscaleParallel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TKernel>(
    TPixel* sourcePtr,
    int sourceWidth,
    int sourceHeight,
    int sourceStride,
    TPixel* destPtr,
    int destWidth,
    int destHeight,
    int destStride,
    TKernel kernel,
    TDecode decoder = default,
    TProject projector = default,
    TEncode encoder = default,
    OutOfBoundsMode horizontalMode = OutOfBoundsMode.Const,
    OutOfBoundsMode verticalMode = OutOfBoundsMode.Const
  )
    where TPixel : unmanaged, IStorageSpace
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TKernel : struct, IDownscaleKernel<TWork, TKey, TPixel, TEncode> {
    var ratioX = kernel.RatioX;
    var ratioY = kernel.RatioY;
    var totalPixels = (long)destWidth * destHeight;
    var minRowsForParallel = Math.Max(100, Environment.ProcessorCount * 5);

    // For downscaling, we iterate source pixels in steps of ratioX/ratioY
    if (totalPixels <= 1_000_000 || destHeight < minRowsForParallel) {
      // Sequential path
      using var frame = new NeighborFrame<TPixel, TWork, TKey, TDecode, TProject>(
        sourcePtr,
        sourceWidth,
        sourceHeight,
        sourceStride,
        decoder,
        projector,
        horizontalMode,
        verticalMode,
        startY: 0
      );

      for (var destY = 0; destY < destHeight; ++destY) {
        var window = frame.GetWindow();
        var destRow = destPtr + destY * destStride;

        for (var destX = 0; destX < destWidth; ++destX) {
          destRow[destX] = kernel.Average(window, encoder);

          // Move window to next source block
          if (destX < destWidth - 1)
            window.MoveBy(ratioX);
        }

        // Move to next source row block
        if (destY < destHeight - 1) {
          // For ratio 2, SeekToRow is equivalent to MoveDown twice but more efficient for ratio 3+
          frame.SeekToRow((destY + 1) * ratioY);
        }
      }

      return;
    }

    // Parallel path - batch destination rows
    var rowsPerBatch = Math.Max(8, destHeight / (Environment.ProcessorCount * 4));
    var partitioner = Partitioner.Create(0, destHeight, rowsPerBatch);

    Parallel.ForEach(
      partitioner,
      range => {
        var startRow = range.Item1;
        var endRow = range.Item2;

        // Initialize frame at the first source row for this partition
        using var frame = new NeighborFrame<TPixel, TWork, TKey, TDecode, TProject>(
          sourcePtr,
          sourceWidth,
          sourceHeight,
          sourceStride,
          decoder,
          projector,
          horizontalMode,
          verticalMode,
          startY: startRow * ratioY
        );

        for (var destY = startRow; destY < endRow; ++destY) {
          var window = frame.GetWindow();
          var destRow = destPtr + destY * destStride;

          for (var destX = 0; destX < destWidth; ++destX) {
            destRow[destX] = kernel.Average(window, encoder);

            // Move window to next source block
            if (destX < destWidth - 1)
              window.MoveBy(ratioX);
          }

          // Move to next source row block
          if (destY < endRow - 1)
            frame.SeekToRow((destY + 1) * ratioY);
        }
      }
    );
  }

  /// <summary>
  /// Executes a scaler on source pixel data, using parallel processing for large images.
  /// </summary>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TWork">The working color type (for interpolation).</typeparam>
  /// <typeparam name="TKey">The key color type (for pattern matching).</typeparam>
  /// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
  /// <typeparam name="TScaler">The scaler kernel type.</typeparam>
  /// <param name="sourcePtr">Pointer to source pixel data.</param>
  /// <param name="sourceWidth">Width of source image.</param>
  /// <param name="sourceHeight">Height of source image.</param>
  /// <param name="sourceStride">Stride of source image in pixels.</param>
  /// <param name="destPtr">Pointer to destination pixel data.</param>
  /// <param name="destStride">Stride of destination image in pixels.</param>
  /// <param name="scaler">The scaler kernel instance.</param>
  /// <param name="decoder">The decoder instance.</param>
  /// <param name="projector">The projector instance.</param>
  /// <param name="encoder">The encoder instance.</param>
  /// <param name="horizontalMode">How to handle horizontal out-of-bounds access.</param>
  /// <param name="verticalMode">How to handle vertical out-of-bounds access.</param>
  /// <remarks>
  /// Uses parallel row processing when totalPixels > 1,000,000 AND rows >= max(100, ProcessorCount * 5).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void ExecuteParallel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TScaler>(
    TPixel* sourcePtr,
    int sourceWidth,
    int sourceHeight,
    int sourceStride,
    TPixel* destPtr,
    int destStride,
    TScaler scaler,
    TDecode decoder = default,
    TProject projector = default,
    TEncode encoder = default,
    OutOfBoundsMode horizontalMode = OutOfBoundsMode.Const,
    OutOfBoundsMode verticalMode = OutOfBoundsMode.Const
  )
    where TPixel : unmanaged, IStorageSpace
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TScaler : struct, IScaler<TWork, TKey, TPixel, TEncode> {
    var totalPixels = (long)sourceWidth * sourceHeight;
    var minRowsForParallel = Math.Max(100, Environment.ProcessorCount * 5);

    var scaleX = scaler.ScaleX;
    var scaleY = scaler.ScaleY;
    var destRowStride = destStride * scaleY;

    // Fall back to sequential if image is too small for parallel overhead
    if (totalPixels <= 1_000_000 || sourceHeight < minRowsForParallel) {
      using var frame = new NeighborFrame<TPixel, TWork, TKey, TDecode, TProject>(
        sourcePtr,
        sourceWidth,
        sourceHeight,
        sourceStride,
        decoder,
        projector,
        horizontalMode,
        verticalMode,
        startY: 0
      );

      for (var y = 0; y < sourceHeight; ++y) {
        var window = frame.GetWindow();
        var destRow = destPtr + y * destRowStride;

        for (var x = 0; x < sourceWidth; ++x) {
          // Scale and write directly to destination
          scaler.Scale(window, destRow, destStride, encoder);

          // Advance destination pointer by ScaleX
          destRow += scaleX;

          // Move to next pixel
          if (x < sourceWidth - 1)
            window.MoveRight();
        }

        // Move to next row within this batch
        if (y < sourceHeight - 1)
          frame.MoveDown();
      }

      return;
    }

    // Batch rows for better cache locality - each batch processes adjacent rows together
    var rowsPerBatch = Math.Max(8, sourceHeight / (Environment.ProcessorCount * 4));
    var partitioner = Partitioner.Create(0, sourceHeight, rowsPerBatch);

    Parallel.ForEach(
      partitioner,
      range => {
        var startRow = range.Item1;
        var endRow = range.Item2;

        using var frame = new NeighborFrame<TPixel, TWork, TKey, TDecode, TProject>(
          sourcePtr,
          sourceWidth,
          sourceHeight,
          sourceStride,
          decoder,
          projector,
          horizontalMode,
          verticalMode,
          startY: startRow
        );

        for (var y = startRow; y < endRow; ++y) {
          var window = frame.GetWindow();
          var destRow = destPtr + y * destRowStride;

          for (var x = 0; x < sourceWidth; ++x) {
            // Scale and write directly to destination
            scaler.Scale(window, destRow, destStride, encoder);

            // Advance destination pointer by ScaleX
            destRow += scaleX;

            // Move to next pixel
            if (x < sourceWidth - 1)
              window.MoveRight();
          }

          // Move to next row within this batch
          if (y < endRow - 1)
            frame.MoveDown();
        }
      }
    );
  }

  /// <summary>
  /// Executes a resampling kernel on source pixel data, iterating over destination pixels.
  /// Uses parallel processing for large images.
  /// </summary>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TWork">The working color type (for accumulation).</typeparam>
  /// <typeparam name="TKey">The key color type (for pattern matching compatibility).</typeparam>
  /// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
  /// <typeparam name="TKernel">The resampling kernel type.</typeparam>
  /// <param name="sourcePtr">Pointer to source pixel data.</param>
  /// <param name="sourceWidth">Width of source image.</param>
  /// <param name="sourceHeight">Height of source image.</param>
  /// <param name="sourceStride">Stride of source image in pixels.</param>
  /// <param name="destPtr">Pointer to destination pixel data.</param>
  /// <param name="destWidth">Width of destination image.</param>
  /// <param name="destHeight">Height of destination image.</param>
  /// <param name="destStride">Stride of destination image in pixels.</param>
  /// <param name="kernel">The resampling kernel instance.</param>
  /// <param name="decoder">The decoder instance.</param>
  /// <param name="projector">The projector instance.</param>
  /// <param name="encoder">The encoder instance.</param>
  /// <param name="horizontalMode">How to handle horizontal out-of-bounds access.</param>
  /// <param name="verticalMode">How to handle vertical out-of-bounds access.</param>
  /// <remarks>
  /// <para>
  /// Unlike pixel-art scalers that iterate over source pixels, this method
  /// iterates over destination pixels and maps back to source coordinates.
  /// This enables arbitrary target dimensions.
  /// </para>
  /// <para>
  /// Uses parallel row processing when totalPixels > 1,000,000 AND rows >= max(100, ProcessorCount * 5).
  /// </para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void ExecuteResampleParallel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TKernel>(
    TPixel* sourcePtr,
    int sourceWidth,
    int sourceHeight,
    int sourceStride,
    TPixel* destPtr,
    int destWidth,
    int destHeight,
    int destStride,
    TKernel kernel,
    TDecode decoder = default,
    TProject projector = default,
    TEncode encoder = default,
    OutOfBoundsMode horizontalMode = OutOfBoundsMode.Const,
    OutOfBoundsMode verticalMode = OutOfBoundsMode.Const
  )
    where TPixel : unmanaged, IStorageSpace
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TKernel : struct, IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode> {
    var totalPixels = (long)destWidth * destHeight;
    var minRowsForParallel = Math.Max(100, Environment.ProcessorCount * 5);

    // Fall back to sequential if image is too small for parallel overhead
    if (totalPixels <= 1_000_000 || destHeight < minRowsForParallel) {
      // Use a single frame for random access - no sliding window needed
      using var frame = new NeighborFrame<TPixel, TWork, TKey, TDecode, TProject>(
        sourcePtr,
        sourceWidth,
        sourceHeight,
        sourceStride,
        decoder,
        projector,
        horizontalMode,
        verticalMode,
        startY: 0
      );

      for (var destY = 0; destY < destHeight; ++destY)
      for (var destX = 0; destX < destWidth; ++destX)
        kernel.Resample(frame, destX, destY, destPtr, destStride, encoder);

      return;
    }

    // Batch destination rows for better cache locality
    var rowsPerBatch = Math.Max(8, destHeight / (Environment.ProcessorCount * 4));
    var partitioner = Partitioner.Create(0, destHeight, rowsPerBatch);

    Parallel.ForEach(
      partitioner,
      range => {
        var startRow = range.Item1;
        var endRow = range.Item2;

        // Each partition creates its own frame for thread-safe random access
        using var frame = new NeighborFrame<TPixel, TWork, TKey, TDecode, TProject>(
          sourcePtr,
          sourceWidth,
          sourceHeight,
          sourceStride,
          decoder,
          projector,
          horizontalMode,
          verticalMode,
          startY: 0
        );

        for (var destY = startRow; destY < endRow; ++destY)
        for (var destX = 0; destX < destWidth; ++destX)
          kernel.Resample(frame, destX, destY, destPtr, destStride, encoder);
      }
    );
  }

  /// <summary>
  /// Executes an edge-aware resampling kernel that uses color equality for similarity detection.
  /// Uses parallel processing for large images.
  /// </summary>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TWork">The working color type (for interpolation).</typeparam>
  /// <typeparam name="TKey">The key color type (for similarity detection).</typeparam>
  /// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
  /// <typeparam name="TEquality">The color equality type for similarity detection.</typeparam>
  /// <typeparam name="TKernel">The edge-aware resampling kernel type.</typeparam>
  /// <param name="sourcePtr">Pointer to source pixel data.</param>
  /// <param name="sourceWidth">Width of source image.</param>
  /// <param name="sourceHeight">Height of source image.</param>
  /// <param name="sourceStride">Stride of source image in pixels.</param>
  /// <param name="destPtr">Pointer to destination pixel data.</param>
  /// <param name="destWidth">Width of destination image.</param>
  /// <param name="destHeight">Height of destination image.</param>
  /// <param name="destStride">Stride of destination image in pixels.</param>
  /// <param name="kernel">The edge-aware resampling kernel instance.</param>
  /// <param name="decoder">The decoder instance.</param>
  /// <param name="projector">The projector instance.</param>
  /// <param name="encoder">The encoder instance.</param>
  /// <param name="horizontalMode">How to handle horizontal out-of-bounds access.</param>
  /// <param name="verticalMode">How to handle vertical out-of-bounds access.</param>
  /// <remarks>
  /// <para>
  /// Edge-aware resamplers like Kopf-Lischinski build similarity graphs to preserve
  /// edges during resampling. The kernel may lazily initialize these structures
  /// on first pixel access.
  /// </para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void ExecuteEdgeAwareResampleParallel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TEquality, TKernel>(
    TPixel* sourcePtr,
    int sourceWidth,
    int sourceHeight,
    int sourceStride,
    TPixel* destPtr,
    int destWidth,
    int destHeight,
    int destStride,
    TKernel kernel,
    TDecode decoder = default,
    TProject projector = default,
    TEncode encoder = default,
    OutOfBoundsMode horizontalMode = OutOfBoundsMode.Const,
    OutOfBoundsMode verticalMode = OutOfBoundsMode.Const
  )
    where TPixel : unmanaged, IStorageSpace
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TEquality : struct, IColorEquality<TKey>
    where TKernel : struct, IEdgeAwareResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TEquality> {
    var totalPixels = (long)destWidth * destHeight;
    var minRowsForParallel = Math.Max(100, Environment.ProcessorCount * 5);

    // Fall back to sequential if image is too small for parallel overhead
    if (totalPixels <= 1_000_000 || destHeight < minRowsForParallel) {
      using var frame = new NeighborFrame<TPixel, TWork, TKey, TDecode, TProject>(
        sourcePtr,
        sourceWidth,
        sourceHeight,
        sourceStride,
        decoder,
        projector,
        horizontalMode,
        verticalMode,
        startY: 0
      );

      for (var destY = 0; destY < destHeight; ++destY)
      for (var destX = 0; destX < destWidth; ++destX)
        kernel.Resample(frame, destX, destY, destPtr, destStride, encoder);

      return;
    }

    // Batch destination rows for better cache locality
    var rowsPerBatch = Math.Max(8, destHeight / (Environment.ProcessorCount * 4));
    var partitioner = Partitioner.Create(0, destHeight, rowsPerBatch);

    Parallel.ForEach(
      partitioner,
      range => {
        var startRow = range.Item1;
        var endRow = range.Item2;

        using var frame = new NeighborFrame<TPixel, TWork, TKey, TDecode, TProject>(
          sourcePtr,
          sourceWidth,
          sourceHeight,
          sourceStride,
          decoder,
          projector,
          horizontalMode,
          verticalMode,
          startY: 0
        );

        for (var destY = startRow; destY < endRow; ++destY)
        for (var destX = 0; destX < destWidth; ++destX)
          kernel.Resample(frame, destX, destY, destPtr, destStride, encoder);
      }
    );
  }

}
