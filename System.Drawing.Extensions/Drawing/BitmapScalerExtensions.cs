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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Pipeline;
using Hawkynt.ColorProcessing.Scalers;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using Hawkynt.Drawing.Lockers;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Provides extension methods for pixel-art scaling of Bitmaps.
/// </summary>
public static class BitmapScalerExtensions {

  /// <param name="this">Source bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Upscales a bitmap using a pixel-art scaling algorithm.
    /// </summary>
    /// <typeparam name="TScaler">The scaler type (Scale2x, Scale3x, or Epx).</typeparam>
    /// <param name="scaler">The scaler instance.</param>
    /// <param name="quality">The quality mode for color operations.</param>
    /// <returns>A new bitmap scaled according to the scaler's factors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap Upscale<TScaler>(TScaler scaler, ScalerQuality quality = ScalerQuality.Fast)
      where TScaler : struct, IScalerInfo
      => _UpscaleGeneric(@this, scaler, quality);

    /// <summary>
    /// Upscales a bitmap using a scaling algorithm with default configuration.
    /// </summary>
    /// <typeparam name="TScaler">The scaler type.</typeparam>
    /// <param name="quality">The quality mode for color operations.</param>
    /// <returns>A new bitmap scaled according to the scaler's factors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap Upscale<TScaler>(ScalerQuality quality = ScalerQuality.Fast)
      where TScaler : struct, IScalerInfo
      => _UpscaleGeneric(@this, default(TScaler), quality);

    /// <summary>
    /// Upscales a bitmap to a target resolution using repeated scaler applications.
    /// </summary>
    /// <typeparam name="TScaler">The scaler type.</typeparam>
    /// <param name="targetWidth">The desired target width.</param>
    /// <param name="targetHeight">The desired target height.</param>
    /// <param name="scaler">The scaler instance.</param>
    /// <param name="quality">The quality mode for scaling operations.</param>
    /// <returns>A new bitmap at the target resolution.</returns>
    /// <remarks>
    /// <para>
    /// The scaler is applied repeatedly until the result meets or exceeds the target dimensions.
    /// If the result exceeds the target, it is downsampled using the specified quality mode.
    /// </para>
    /// <para>
    /// For <see cref="ScalerQuality.Fast"/>, downsampling uses <see cref="DownsampleQuality.NearestNeighbor"/>.
    /// For <see cref="ScalerQuality.HighQuality"/>, downsampling uses <see cref="DownsampleQuality.Lanczos"/>.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap UpscaleTo<TScaler>(
      int targetWidth,
      int targetHeight,
      TScaler scaler = default,
      ScalerQuality quality = ScalerQuality.Fast
    ) where TScaler : struct, IScalerInfo
      => _UpscaleToTarget(@this, scaler, targetWidth, targetHeight, quality);

    /// <summary>
    /// Upscales a bitmap to a target resolution using repeated scaler applications.
    /// </summary>
    /// <typeparam name="TScaler">The scaler type.</typeparam>
    /// <param name="targetWidth">The desired target width.</param>
    /// <param name="targetHeight">The desired target height.</param>
    /// <param name="quality">The quality mode for scaling operations.</param>
    /// <returns>A new bitmap at the target resolution.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap UpscaleTo<TScaler>(
      int targetWidth,
      int targetHeight,
      ScalerQuality quality = ScalerQuality.Fast
    ) where TScaler : struct, IScalerInfo
      => _UpscaleToTarget(@this, default(TScaler), targetWidth, targetHeight, quality);
  }

  /// <summary>
  /// Generic scaling path - dispatches to the scaler's Apply method.
  /// JIT devirtualizes the call for struct-constrained generics.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _UpscaleGeneric<TScaler>(Bitmap source, TScaler scaler, ScalerQuality quality)
    where TScaler : struct, IScalerInfo
    => _ApplyScalerOnce(source, scaler, quality);

  /// <summary>
  /// Applies a single scaling pass by delegating to the scaler's dispatch method.
  /// Uses runtime cast to internal interface to keep public API clean.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _ApplyScalerOnce<TScaler>(Bitmap source, TScaler scaler, ScalerQuality quality)
    where TScaler : struct, IScalerInfo
    => ((IScalerDispatch)(object)scaler).Apply(source, quality);

  /// <summary>
  /// Upscales to a target resolution by repeated scaler application and optional downsampling.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _UpscaleToTarget<TScaler>(
    Bitmap source,
    TScaler scaler,
    int targetWidth,
    int targetHeight,
    ScalerQuality quality
  ) where TScaler : struct, IScalerInfo {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);
    
    var current = source;
    var ownsCurrentBitmap = false;

    try {
      // Apply scaler repeatedly until we reach or exceed target dimensions
      while (current.Width < targetWidth || current.Height < targetHeight) {
        var next = _UpscaleGeneric(current, scaler, quality);
        if (ownsCurrentBitmap)
          current.Dispose();

        current = next;
        ownsCurrentBitmap = true;
      }

      // If exact match, return the result
      if (current.Width == targetWidth && current.Height == targetHeight) {
        ownsCurrentBitmap = false; // Transfer ownership to caller
        return current;
      }

      // Downsample to exact target
      var downsampleQuality = quality == ScalerQuality.Fast
        ? InterpolationMode.NearestNeighbor
        : InterpolationMode.HighQualityBicubic;

      var result = _Downsample(current, targetWidth, targetHeight, downsampleQuality);
      return result;
    } finally {
      if (ownsCurrentBitmap)
        current.Dispose();
    }
  }

  /// <summary>
  /// Downsamples a bitmap to the target dimensions using the specified quality mode.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _Downsample(Bitmap source, int targetWidth, int targetHeight, InterpolationMode quality) {
    var result = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);

    using var g = Graphics.FromImage(result);
    g.InterpolationMode = quality;
    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    g.DrawImage(source, 0, 0, targetWidth, targetHeight);

    return result;
  }

  /// <summary>
  /// Internal scaling method that applies a scaler kernel to a bitmap.
  /// </summary>
  /// <typeparam name="TWork">The working color type (for interpolation).</typeparam>
  /// <typeparam name="TKey">The key color type (for pattern matching).</typeparam>
  /// <typeparam name="TDecode">The decoder type (Bgra8888 → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → Bgra8888).</typeparam>
  /// <typeparam name="TScaler">The scaler kernel type.</typeparam>
  /// <param name="source">The source bitmap.</param>
  /// <param name="scaler">The scaler kernel instance.</param>
  /// <returns>A new bitmap scaled according to the scaler's factors.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe Bitmap Upscale<TWork, TKey, TDecode, TProject, TEncode, TScaler>(Bitmap source, TScaler scaler)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TScaler : struct, IScaler<TWork, TKey, Bgra8888, TEncode> {
    var scaleX = scaler.ScaleX;
    var scaleY = scaler.ScaleY;
    var destWidth = source.Width * scaleX;
    var destHeight = source.Height * scaleY;
    var result = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);

    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    fixed (Bgra8888* srcPtr = srcFrame.Pixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels)
      ScalerPipeline.ExecuteParallel<
        Bgra8888, TWork, TKey,
        TDecode, TProject, TEncode,
        TScaler
      >(
        srcPtr, srcFrame.Width, srcFrame.Height, srcFrame.Stride,
        dstPtr, dstFrame.Stride,
        scaler
      );

    return result;
  }

  /// <summary>
  /// Downscales a bitmap using a downscale kernel.
  /// </summary>
  /// <typeparam name="TWork">The working color type (for averaging calculations).</typeparam>
  /// <typeparam name="TKey">The key color type (for pattern matching compatibility).</typeparam>
  /// <typeparam name="TDecode">The decoder type (Bgra8888 → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → Bgra8888).</typeparam>
  /// <typeparam name="TKernel">The downscale kernel type.</typeparam>
  /// <param name="source">The source bitmap.</param>
  /// <param name="kernel">The downscale kernel instance.</param>
  /// <returns>A new bitmap scaled down according to the kernel's ratios.</returns>
  /// <remarks>
  /// <para>
  /// The kernel uses NeighborWindow for fixed-ratio downscaling (max 5:1).
  /// Uses IAccum&lt;TAccum, TWork&gt; for weighted accumulation.
  /// </para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe Bitmap Downscale<TAccum, TWork, TKey, TDecode, TProject, TEncode, TKernel>(Bitmap source, TKernel kernel)
    where TAccum : unmanaged, IAccum<TAccum, TWork>
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TKernel : struct, IDownscaleKernel<TAccum, TWork, TKey, Bgra8888, TEncode> {
    var ratioX = kernel.RatioX;
    var ratioY = kernel.RatioY;
    var destWidth = source.Width / ratioX;
    var destHeight = source.Height / ratioY;

    if (destWidth < 1 || destHeight < 1)
      throw new ArgumentException($"Source image ({source.Width}x{source.Height}) is too small for {ratioX}x{ratioY} downscaling.");

    var result = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);

    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    fixed (Bgra8888* srcPtr = srcFrame.Pixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels)
      ScalerPipeline.ExecuteDownscaleParallel<
        TAccum, Bgra8888, TWork, TKey,
        TDecode, TProject, TEncode,
        TKernel
      >(
        srcPtr, srcFrame.Width, srcFrame.Height, srcFrame.Stride,
        dstPtr, destWidth, destHeight, dstFrame.Stride,
        kernel
      );

    return result;
  }

  /// <summary>
  /// Decodes a bitmap to a Frame in working color space.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <typeparam name="TDecode">The decoder type (Bgra8888 → TWork).</typeparam>
  /// <param name="source">The source bitmap.</param>
  /// <param name="decoder">The decoder instance.</param>
  /// <returns>A new frame containing the decoded pixel data.</returns>
  /// <remarks>
  /// Use this method to convert a bitmap to working space for multi-pass
  /// scaling operations. The frame should be disposed when no longer needed.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe PooledFrame<TWork> BitmapToFrame<TWork, TDecode>(Bitmap source, TDecode decoder = default)
    where TWork : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork> {
    var result = new PooledFrame<TWork>(source.Width, source.Height);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    var srcFrame = srcLocker.AsFrame();

    fixed (Bgra8888* srcPtr = srcFrame.Pixels) {
      using var dstPinned = result.Pin();
      var dstPtr = dstPinned.Pointer;
      var srcStride = srcFrame.Stride;
      var dstStride = result.Stride;
      var width = srcFrame.Width;
      var height = srcFrame.Height;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcPtr + y * srcStride;
        var dstRow = dstPtr + y * dstStride;

        for (var x = 0; x < width; ++x)
          dstRow[x] = decoder.Decode(srcRow[x]);
      }
    }

    return result;
  }

  /// <summary>
  /// Encodes a Frame from working color space to a bitmap.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → Bgra8888).</typeparam>
  /// <param name="source">The source frame.</param>
  /// <param name="encoder">The encoder instance.</param>
  /// <returns>A new bitmap containing the encoded pixel data.</returns>
  /// <remarks>
  /// Use this method to convert a working-space frame back to a bitmap
  /// after multi-pass scaling operations.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe Bitmap FrameToBitmap<TWork, TEncode>(PooledFrame<TWork> source, TEncode encoder = default)
    where TWork : unmanaged, IColorSpace
    where TEncode : struct, IEncode<TWork, Bgra8888> {
    var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);
    var dstFrame = dstLocker.AsFrame();

    fixed (Bgra8888* dstPtr = dstFrame.Pixels) {
      using var srcPinned = source.Pin();
      var srcPtr = srcPinned.Pointer;
      var srcStride = source.Stride;
      var dstStride = dstFrame.Stride;
      var width = source.Width;
      var height = source.Height;

      // TODO: parallelize, unroll loop, jump-table to tail, etc.
      for (var y = 0; y < height; ++y) {
        var srcRow = srcPtr + y * srcStride;
        var dstRow = dstPtr + y * dstStride;

        for (var x = 0; x < width; ++x)
          dstRow[x] = encoder.Encode(srcRow[x]);
      }
    }

    return result;
  }

  #region PooledFrame Overloads

  /// <summary>
  /// Decodes a bitmap to a PooledFrame in working color space.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <typeparam name="TDecode">The decoder type (Bgra8888 → TWork).</typeparam>
  /// <param name="source">The source bitmap.</param>
  /// <param name="decoder">The decoder instance.</param>
  /// <returns>A new pooled frame containing the decoded pixel data.</returns>
  /// <remarks>
  /// Use this method to convert a bitmap to working space for multi-pass
  /// scaling operations. The pooled frame should be disposed when no longer
  /// needed to return the buffer to the pool.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe PooledFrame<TWork> BitmapToPooledFrame<TWork, TDecode>(Bitmap source, TDecode decoder = default)
    where TWork : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork> {
    var result = new PooledFrame<TWork>(source.Width, source.Height);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    var srcFrame = srcLocker.AsFrame();

    fixed (Bgra8888* srcPtr = srcFrame.Pixels) {
      using var dstPin = result.Pin();
      var dstPtr = dstPin.Pointer;
      var srcStride = srcFrame.Stride;
      var dstStride = result.Stride;
      var width = srcFrame.Width;
      var height = srcFrame.Height;

      // TODO: parallelize, unroll loop, jump-table to tail, etc.
      for (var y = 0; y < height; ++y) {
        var srcRow = srcPtr + y * srcStride;
        var dstRow = dstPtr + y * dstStride;

        for (var x = 0; x < width; ++x)
          dstRow[x] = decoder.Decode(srcRow[x]);
      }
    }

    return result;
  }

  /// <summary>
  /// Encodes a PooledFrame from working color space to a bitmap.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → Bgra8888).</typeparam>
  /// <param name="source">The source pooled frame.</param>
  /// <param name="encoder">The encoder instance.</param>
  /// <returns>A new bitmap containing the encoded pixel data.</returns>
  /// <remarks>
  /// Use this method to convert a working-space pooled frame back to a bitmap
  /// after multi-pass scaling operations.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe Bitmap PooledFrameToBitmap<TWork, TEncode>(ref PooledFrame<TWork> source, TEncode encoder = default)
    where TWork : unmanaged, IColorSpace
    where TEncode : struct, IEncode<TWork, Bgra8888> {
    var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);
    var dstFrame = dstLocker.AsFrame();

    fixed (Bgra8888* dstPtr = dstFrame.Pixels) {
      using var srcPin = source.Pin();
      var srcPtr = srcPin.Pointer;
      var srcStride = source.Stride;
      var dstStride = dstFrame.Stride;
      var width = source.Width;
      var height = source.Height;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcPtr + y * srcStride;
        var dstRow = dstPtr + y * dstStride;

        for (var x = 0; x < width; ++x)
          dstRow[x] = encoder.Encode(srcRow[x]);
      }
    }

    return result;
  }

  /// <summary>
  /// Upscales a bitmap using pooled memory for better memory efficiency.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <typeparam name="TKey">The key color type.</typeparam>
  /// <typeparam name="TDecode">The decoder type.</typeparam>
  /// <typeparam name="TProject">The projector type.</typeparam>
  /// <typeparam name="TEncode">The encoder type.</typeparam>
  /// <typeparam name="TScaler">The scaler kernel type.</typeparam>
  /// <param name="source">The source bitmap.</param>
  /// <param name="scaler">The scaler kernel instance.</param>
  /// <returns>A new bitmap scaled according to the scaler's factors.</returns>
  /// <remarks>
  /// This method uses ArrayPool-backed frames with scoped pinning for better
  /// memory efficiency compared to <see cref="Upscale{TWork,TKey,TDecode,TProject,TEncode,TScaler}"/>.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe Bitmap UpscalePooled<TWork, TKey, TDecode, TProject, TEncode, TScaler>(Bitmap source, TScaler scaler)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TScaler : struct, IScaler<TWork, TKey, Bgra8888, TEncode> {
    var scaleX = scaler.ScaleX;
    var scaleY = scaler.ScaleY;
    var destWidth = source.Width * scaleX;
    var destHeight = source.Height * scaleY;
    var result = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);

    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    // Use the existing pointer-based parallel execution since bitmap data is already pinned by the locker
    fixed (Bgra8888* srcPtr = srcFrame.Pixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels)
      ScalerPipeline.ExecuteParallel<
        Bgra8888, TWork, TKey,
        TDecode, TProject, TEncode,
        TScaler
      >(
        srcPtr, srcFrame.Width, srcFrame.Height, srcFrame.Stride,
        dstPtr, dstFrame.Stride,
        scaler
      );

    return result;
  }

  #endregion

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS

  /// <summary>
  /// Resamples a bitmap using a kernel with arbitrary pixel access.
  /// </summary>
  /// <typeparam name="TAccum">The accumulator type for weighted averaging.</typeparam>
  /// <typeparam name="TWork">The working color type (for accumulation).</typeparam>
  /// <typeparam name="TKey">The key color type (for NeighborFrame compatibility).</typeparam>
  /// <typeparam name="TDecode">The decoder type (Bgra8888 → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → Bgra8888).</typeparam>
  /// <typeparam name="TKernel">The resampling kernel type.</typeparam>
  /// <param name="source">The source bitmap.</param>
  /// <param name="kernel">The resampling kernel instance.</param>
  /// <returns>A new bitmap scaled according to the kernel's factors.</returns>
  /// <remarks>
  /// <para>
  /// Unlike pixel-art scalers that use the fixed 5x5 NeighborWindow,
  /// resamplers use NeighborFrame's indexer for arbitrary radius access.
  /// This enables Lanczos, Bicubic, and other filter-based algorithms.
  /// </para>
  /// <para>
  /// Uses IAccum&lt;TAccum, TWork&gt; for weighted accumulation.
  /// </para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe Bitmap Resample<TAccum, TWork, TKey, TDecode, TProject, TEncode, TKernel>(Bitmap source, TKernel kernel)
    where TAccum : unmanaged, IAccum<TAccum, TWork>
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TKernel : struct, IResampleKernel<TAccum, Bgra8888, TWork, TKey, TDecode, TProject, TEncode> {
    var scaleX = kernel.ScaleX;
    var scaleY = kernel.ScaleY;
    var destWidth = source.Width * scaleX;
    var destHeight = source.Height * scaleY;
    var result = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);

    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    fixed (Bgra8888* srcPtr = srcFrame.Pixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels)
      ScalerPipeline.ExecuteResampleParallel<
        TAccum, Bgra8888, TWork, TKey,
        TDecode, TProject, TEncode,
        TKernel
      >(
        srcPtr, srcFrame.Width, srcFrame.Height, srcFrame.Stride,
        dstPtr, dstFrame.Stride,
        kernel
      );

    return result;
  }

#endif

}
