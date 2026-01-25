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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Metrics.Rgb;
using Hawkynt.ColorProcessing.Resizing;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using Hawkynt.Drawing.Lockers;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Marker type for disambiguating <see cref="IResampler"/> overloads from <see cref="IEdgeAwareResampler"/> overloads.
/// </summary>
/// <typeparam name="T">The resampler type (constraint enforced by compiler).</typeparam>
public struct __ResamplerTag<T> where T : struct, IResampler;

/// <summary>
/// Marker type for disambiguating <see cref="IEdgeAwareResampler"/> overloads from <see cref="IResampler"/> overloads.
/// </summary>
/// <typeparam name="T">The edge-aware resampler type (constraint enforced by compiler).</typeparam>
public struct __EdgeAwareResamplerTag<T> where T : struct, IEdgeAwareResampler;

/// <summary>
/// Marker type for disambiguating <see cref="IContentAwareResampler"/> overloads.
/// </summary>
/// <typeparam name="T">The content-aware resampler type (constraint enforced by compiler).</typeparam>
public struct __ContentAwareResamplerTag<T> where T : struct, IContentAwareResampler;

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
      where TScaler : struct, IPixelScaler
      => _UpscaleGeneric(@this, scaler, quality);

    /// <summary>
    /// Upscales a bitmap using a scaling algorithm with default configuration.
    /// </summary>
    /// <typeparam name="TScaler">The scaler type.</typeparam>
    /// <param name="quality">The quality mode for color operations.</param>
    /// <returns>A new bitmap scaled according to the scaler's factors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap Upscale<TScaler>(ScalerQuality quality = ScalerQuality.Fast)
      where TScaler : struct, IPixelScaler
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
    ) where TScaler : struct, IPixelScaler
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
    ) where TScaler : struct, IPixelScaler
      => _UpscaleToTarget(@this, default(TScaler), targetWidth, targetHeight, quality);

    /// <summary>
    /// Upscales a bitmap with full control over color pipeline and comparison types.
    /// </summary>
    /// <typeparam name="TScaler">The scaler type.</typeparam>
    /// <typeparam name="TWork">The working color space for interpolation.</typeparam>
    /// <typeparam name="TKey">The key color space for pattern matching.</typeparam>
    /// <typeparam name="TDecode">The decoder type (Bgra8888 → TWork).</typeparam>
    /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
    /// <typeparam name="TEncode">The encoder type (TWork → Bgra8888).</typeparam>
    /// <typeparam name="TMetric">The color distance metric type.</typeparam>
    /// <typeparam name="TEquality">The color equality comparer type.</typeparam>
    /// <typeparam name="TLerp">The color interpolation type.</typeparam>
    /// <param name="scaler">The scaler instance.</param>
    /// <param name="equality">The equality comparer instance.</param>
    /// <param name="lerp">The interpolation instance.</param>
    /// <returns>A new bitmap scaled according to the scaler's factors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap Upscale<TScaler, TWork, TKey, TDecode, TProject, TEncode, TMetric, TEquality, TLerp>(
      TScaler scaler,
      TEquality equality = default,
      TLerp lerp = default)
      where TScaler : struct, IPixelScaler
      where TWork : unmanaged, IColorSpace
      where TKey : unmanaged, IColorSpace
      where TDecode : struct, IDecode<Bgra8888, TWork>
      where TProject : struct, IProject<TWork, TKey>
      where TEncode : struct, IEncode<TWork, Bgra8888>
      where TMetric : struct, IColorMetric<TKey>, INormalizedMetric
      where TEquality : struct, IColorEquality<TKey>
      where TLerp : struct, ILerp<TWork> {
      var callback = new UpscaleCallback<TWork, TKey, TDecode, TProject, TEncode>(@this);
      return scaler.InvokeKernel<TWork, TKey, Bgra8888, TMetric, TEquality, TLerp, TEncode, Bitmap>(
        callback, equality, lerp);
    }

    /// <summary>
    /// Upscales a bitmap with a custom metric, using default equality/lerp.
    /// Uses Bgra8888 identity pipeline (no color space conversion).
    /// </summary>
    /// <typeparam name="TScaler">The scaler type.</typeparam>
    /// <typeparam name="TMetric">The color distance metric type for Bgra8888.</typeparam>
    /// <param name="scaler">The scaler instance.</param>
    /// <returns>A new bitmap scaled according to the scaler's factors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap UpscaleWithMetric<TScaler, TMetric>(
      TScaler scaler)
      where TScaler : struct, IPixelScaler
      where TMetric : struct, IColorMetric<Bgra8888>, INormalizedMetric
      => _UpscaleWithMetric<TScaler, TMetric>(@this, scaler);

    /// <summary>
    /// Upscales a bitmap with a custom equality comparer, using default metric/lerp.
    /// Uses Bgra8888 identity pipeline (no color space conversion).
    /// </summary>
    /// <typeparam name="TScaler">The scaler type.</typeparam>
    /// <typeparam name="TEquality">The color equality comparer type for Bgra8888.</typeparam>
    /// <param name="scaler">The scaler instance.</param>
    /// <param name="equality">The equality comparer instance.</param>
    /// <returns>A new bitmap scaled according to the scaler's factors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap UpscaleWithEquality<TScaler, TEquality>(
      TScaler scaler,
      TEquality equality)
      where TScaler : struct, IPixelScaler
      where TEquality : struct, IColorEquality<Bgra8888>
      => _UpscaleWithEquality<TScaler, TEquality>(@this, scaler, equality);

    /// <summary>
    /// Upscales a bitmap with a custom lerp, using default metric/equality.
    /// Uses Bgra8888 identity pipeline (no color space conversion).
    /// </summary>
    /// <typeparam name="TScaler">The scaler type.</typeparam>
    /// <typeparam name="TLerp">The color interpolation type for Bgra8888.</typeparam>
    /// <param name="scaler">The scaler instance.</param>
    /// <param name="lerp">The interpolation instance.</param>
    /// <returns>A new bitmap scaled according to the scaler's factors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap UpscaleWithLerp<TScaler, TLerp>(
      TScaler scaler,
      TLerp lerp)
      where TScaler : struct, IPixelScaler
      where TLerp : struct, ILerp<Bgra8888>
      => _UpscaleWithLerp<TScaler, TLerp>(@this, scaler, lerp);
  }

  /// <summary>
  /// Generic scaling path - dispatches to the appropriate quality pipeline.
  /// JIT devirtualizes the call for struct-constrained generics.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _UpscaleGeneric<TScaler>(Bitmap source, TScaler scaler, ScalerQuality quality)
    where TScaler : struct, IPixelScaler
    => _ApplyScalerOnce(source, scaler, quality);

  /// <summary>
  /// Applies a single scaling pass using the InvokeKernel callback pattern.
  /// Interface dispatch occurs once per bitmap; concrete kernel flows to hot path.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _ApplyScalerOnce<TScaler>(Bitmap source, TScaler scaler, ScalerQuality quality)
    where TScaler : struct, IPixelScaler
    => quality switch {
      ScalerQuality.Fast => _UpscaleFast(source, scaler),
      ScalerQuality.HighQuality => _UpscaleHighQuality(source, scaler),
      _ => throw new NotSupportedException($"Quality {quality} is not supported.")
    };

  /// <summary>
  /// Fast quality path using identity codecs (Bgra8888 throughout).
  /// Uses 8-bit integer-only lerp and squared distance metric (no sqrt) for maximum performance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _UpscaleFast<TScaler>(Bitmap source, TScaler scaler)
    where TScaler : struct, IPixelScaler {
    var callback = new UpscaleCallback<
      Bgra8888, Bgra8888,
      IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>>(source);
    return scaler.InvokeKernel<
      Bgra8888, Bgra8888, Bgra8888,
      CompuPhaseSquared4<Bgra8888>, ExactEquality<Bgra8888>,
      Color4BLerpInt<Bgra8888>, IdentityEncode<Bgra8888>, Bitmap>(callback);
  }

  /// <summary>
  /// High quality path using linear RGB working space and Oklab perceptual space.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _UpscaleHighQuality<TScaler>(Bitmap source, TScaler scaler)
    where TScaler : struct, IPixelScaler {
    var callback = new UpscaleCallback<
      LinearRgbaF, OklabF,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(source);
    return scaler.InvokeKernel<
      LinearRgbaF, OklabF, Bgra8888,
      Euclidean3F<OklabF>, ThresholdEquality3<OklabF>,
      Color4FLerp<LinearRgbaF>, LinearRgbaFToSrgb32, Bitmap>(callback, new(0.02f, 0.04f, 0.04f));
  }

  #region Convenience Overload Helpers

  /// <summary>
  /// Metric-only path: uses Bgra8888 identity codecs, ExactEquality, Color4BLerp.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _UpscaleWithMetric<TScaler, TMetric>(Bitmap source, TScaler scaler)
    where TScaler : struct, IPixelScaler
    where TMetric : struct, IColorMetric<Bgra8888>, INormalizedMetric {
    var callback = new UpscaleCallback<
      Bgra8888, Bgra8888,
      IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>>(source);
    return scaler.InvokeKernel<
      Bgra8888, Bgra8888, Bgra8888,
      TMetric, ExactEquality<Bgra8888>,
      Color4BLerpInt<Bgra8888>, IdentityEncode<Bgra8888>, Bitmap>(callback);
  }

  /// <summary>
  /// Equality-only path: uses Bgra8888 identity codecs, CompuPhaseSquared4, Color4BLerp.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _UpscaleWithEquality<TScaler, TEquality>(Bitmap source, TScaler scaler, TEquality equality)
    where TScaler : struct, IPixelScaler
    where TEquality : struct, IColorEquality<Bgra8888> {
    var callback = new UpscaleCallback<
      Bgra8888, Bgra8888,
      IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>>(source);
    return scaler.InvokeKernel<
      Bgra8888, Bgra8888, Bgra8888,
      CompuPhaseSquared4<Bgra8888>, TEquality,
      Color4BLerpInt<Bgra8888>, IdentityEncode<Bgra8888>, Bitmap>(callback, equality);
  }

  /// <summary>
  /// Lerp-only path: uses Bgra8888 identity codecs, CompuPhaseSquared4, ExactEquality.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _UpscaleWithLerp<TScaler, TLerp>(Bitmap source, TScaler scaler, TLerp lerp)
    where TScaler : struct, IPixelScaler
    where TLerp : struct, ILerp<Bgra8888> {
    var callback = new UpscaleCallback<
      Bgra8888, Bgra8888,
      IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>>(source);
    return scaler.InvokeKernel<
      Bgra8888, Bgra8888, Bgra8888,
      CompuPhaseSquared4<Bgra8888>, ExactEquality<Bgra8888>,
      TLerp, IdentityEncode<Bgra8888>, Bitmap>(callback, default, lerp);
  }

  #endregion

  /// <summary>
  /// Callback that receives a concrete kernel type and executes the upscale pipeline.
  /// Enables struct-constrained dispatch without per-pixel virtual calls.
  /// </summary>
  private sealed class UpscaleCallback<TWork, TKey, TDecode, TProject, TEncode>(Bitmap source)
    : IKernelCallback<TWork, TKey, Bgra8888, TEncode, Bitmap>
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888> {

    public Bitmap Invoke<TKernel>(TKernel kernel)
      where TKernel : struct, IScaler<TWork, TKey, Bgra8888, TEncode>
      => Upscale<TWork, TKey, TDecode, TProject, TEncode, TKernel>(source, kernel);
  }

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
  ) where TScaler : struct, IPixelScaler {
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

    fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels)
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
  /// Uses IColorSpace4F for internal accumulation.
  /// </para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe Bitmap Downscale<TWork, TKey, TDecode, TProject, TEncode, TKernel>(Bitmap source, TKernel kernel)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TKernel : struct, IDownscaleKernel<TWork, TKey, Bgra8888, TEncode> {
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

    fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels)
      ScalerPipeline.ExecuteDownscaleParallel<
        Bgra8888, TWork, TKey,
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
  /// Resamples a bitmap using a kernel with arbitrary pixel access and target dimensions.
  /// </summary>
  /// <typeparam name="TWork">The working color type (for accumulation).</typeparam>
  /// <typeparam name="TKey">The key color type (for NeighborFrame compatibility).</typeparam>
  /// <typeparam name="TDecode">The decoder type (Bgra8888 → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → Bgra8888).</typeparam>
  /// <typeparam name="TKernel">The resampling kernel type.</typeparam>
  /// <param name="source">The source bitmap.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <param name="kernel">The resampling kernel instance.</param>
  /// <returns>A new bitmap scaled to the target dimensions.</returns>
  /// <remarks>
  /// <para>
  /// Unlike pixel-art scalers that use the fixed 5x5 NeighborWindow,
  /// resamplers use NeighborFrame's indexer for arbitrary radius access.
  /// This enables Lanczos, Bicubic, and other filter-based algorithms.
  /// </para>
  /// <para>
  /// Uses IColorSpace4F for internal accumulation.
  /// </para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe Bitmap Resample<TWork, TKey, TDecode, TProject, TEncode, TKernel>(
    Bitmap source,
    int targetWidth,
    int targetHeight,
    TKernel kernel)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TKernel : struct, IResampleKernel<Bgra8888, TWork, TKey, TDecode, TProject, TEncode> {
    var result = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);

    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels)
      ScalerPipeline.ExecuteResampleParallel<
        Bgra8888, TWork, TKey,
        TDecode, TProject, TEncode,
        TKernel
      >(
        srcPtr, srcFrame.Width, srcFrame.Height, srcFrame.Stride,
        dstPtr, targetWidth, targetHeight, dstFrame.Stride,
        kernel
      );

    return result;
  }

  /// <summary>
  /// Callback that receives a concrete resampler kernel and executes the resample pipeline.
  /// </summary>
  private sealed class ResampleCallback<TWork, TKey, TDecode, TProject, TEncode>(Bitmap source, int targetWidth, int targetHeight)
    : IResampleKernelCallback<TWork, TKey, Bgra8888, TDecode, TProject, TEncode, Bitmap>
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888> {

    public Bitmap Invoke<TKernel>(TKernel kernel)
      where TKernel : struct, IResampleKernel<Bgra8888, TWork, TKey, TDecode, TProject, TEncode>
      => Resample<TWork, TKey, TDecode, TProject, TEncode, TKernel>(source, targetWidth, targetHeight, kernel);
  }

  /// <summary>
  /// Resamples a bitmap to target dimensions using a resampler with default configuration.
  /// </summary>
  /// <typeparam name="TResampler">The resampler type (e.g., Lanczos3, Bicubic).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <returns>A new bitmap scaled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, int targetWidth, int targetHeight, __ResamplerTag<TResampler> _ = default)
    where TResampler : struct, IResampler
    => source.Resample(default(TResampler), targetWidth, targetHeight);

  /// <summary>
  /// Resamples a bitmap to target dimensions using a resampler with default configuration.
  /// </summary>
  /// <typeparam name="TResampler">The resampler type (e.g., Lanczos3, Bicubic).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="targetSize">Target dimensions.</param>
  /// <returns>A new bitmap scaled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, Size targetSize, __ResamplerTag<TResampler> _ = default)
    where TResampler : struct, IResampler
    => source.Resample<TResampler>(targetSize.Width, targetSize.Height);

  /// <summary>
  /// Resamples a bitmap to target dimensions using a configured resampler instance.
  /// </summary>
  /// <typeparam name="TResampler">The resampler type (e.g., Lanczos3, Bicubic).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="resampler">The resampler instance with custom configuration.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <returns>A new bitmap scaled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, TResampler resampler, int targetWidth, int targetHeight, __ResamplerTag<TResampler> _ = default)
    where TResampler : struct, IResampler {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var callback = new ResampleCallback<
      LinearRgbaF, OklabF,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(source, targetWidth, targetHeight);
    return resampler.InvokeKernel<
      LinearRgbaF, OklabF, Bgra8888,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32, Bitmap>(
      callback, source.Width, source.Height, targetWidth, targetHeight);
  }

  /// <summary>
  /// Resamples a bitmap to target dimensions using a configured resampler instance.
  /// </summary>
  /// <typeparam name="TResampler">The resampler type (e.g., Lanczos3, Bicubic).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="resampler">The resampler instance with custom configuration.</param>
  /// <param name="targetSize">Target dimensions.</param>
  /// <returns>A new bitmap scaled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, TResampler resampler, Size targetSize, __ResamplerTag<TResampler> _ = default)
    where TResampler : struct, IResampler
    => source.Resample(resampler, targetSize.Width, targetSize.Height);

  #region Edge-Aware Resampling

  /// <summary>
  /// Callback that receives a concrete edge-aware kernel type and executes the resample pipeline.
  /// </summary>
  private sealed class EdgeAwareResampleCallback<TWork, TKey, TDecode, TProject, TEncode, TEquality>(Bitmap source, int targetWidth, int targetHeight)
    : IEdgeAwareResampleKernelCallback<TWork, TKey, Bgra8888, TDecode, TProject, TEncode, TEquality, Bitmap>
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TEquality : struct, IColorEquality<TKey> {

    public Bitmap Invoke<TKernel>(TKernel kernel)
      where TKernel : struct, IEdgeAwareResampleKernel<Bgra8888, TWork, TKey, TDecode, TProject, TEncode, TEquality>
      => Resample<TWork, TKey, TDecode, TProject, TEncode, TEquality, TKernel>(source, targetWidth, targetHeight, kernel);
  }

  /// <summary>
  /// Internal edge-aware resampling method that applies a kernel to a bitmap.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe Bitmap Resample<TWork, TKey, TDecode, TProject, TEncode, TEquality, TKernel>(
    Bitmap source,
    int targetWidth,
    int targetHeight,
    TKernel kernel)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TEquality : struct, IColorEquality<TKey>
    where TKernel : struct, IEdgeAwareResampleKernel<Bgra8888, TWork, TKey, TDecode, TProject, TEncode, TEquality> {
    var result = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);

    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels)
      ScalerPipeline.ExecuteEdgeAwareResampleParallel<
        Bgra8888, TWork, TKey,
        TDecode, TProject, TEncode,
        TEquality, TKernel
      >(
        srcPtr, srcFrame.Width, srcFrame.Height, srcFrame.Stride,
        dstPtr, targetWidth, targetHeight, dstFrame.Stride,
        kernel
      );

    return result;
  }

  /// <summary>
  /// Resamples a bitmap using an edge-aware algorithm with default configuration.
  /// </summary>
  /// <typeparam name="TResampler">The edge-aware resampler type (e.g., KopfLischinski).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <returns>A new bitmap scaled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, int targetWidth, int targetHeight, __EdgeAwareResamplerTag<TResampler> _ = default)
    where TResampler : struct, IEdgeAwareResampler
    => source.Resample(default(TResampler), targetWidth, targetHeight);

  /// <summary>
  /// Resamples a bitmap using an edge-aware algorithm with default configuration.
  /// </summary>
  /// <typeparam name="TResampler">The edge-aware resampler type (e.g., KopfLischinski).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="targetSize">Target dimensions.</param>
  /// <returns>A new bitmap scaled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, Size targetSize, __EdgeAwareResamplerTag<TResampler> _ = default)
    where TResampler : struct, IEdgeAwareResampler
    => source.Resample<TResampler>(targetSize.Width, targetSize.Height);

  /// <summary>
  /// Resamples a bitmap using a configured edge-aware resampler instance.
  /// </summary>
  /// <typeparam name="TResampler">The edge-aware resampler type (e.g., KopfLischinski).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="resampler">The edge-aware resampler instance.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <returns>A new bitmap scaled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, TResampler resampler, int targetWidth, int targetHeight, __EdgeAwareResamplerTag<TResampler> _ = default)
    where TResampler : struct, IEdgeAwareResampler {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var callback = new EdgeAwareResampleCallback<
      LinearRgbaF, OklabF,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
      ThresholdEquality3<OklabF>>(source, targetWidth, targetHeight);
    return resampler.InvokeKernel<
      LinearRgbaF, OklabF, Bgra8888,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
      ThresholdEquality3<OklabF>, Bitmap>(
      callback, source.Width, source.Height, targetWidth, targetHeight, new(0.02f, 0.04f, 0.04f));
  }

  /// <summary>
  /// Resamples a bitmap using a configured edge-aware resampler instance.
  /// </summary>
  /// <typeparam name="TResampler">The edge-aware resampler type (e.g., KopfLischinski).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="resampler">The edge-aware resampler instance.</param>
  /// <param name="targetSize">Target dimensions.</param>
  /// <returns>A new bitmap scaled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, TResampler resampler, Size targetSize, __EdgeAwareResamplerTag<TResampler> _ = default)
    where TResampler : struct, IEdgeAwareResampler
    => source.Resample(resampler, targetSize.Width, targetSize.Height);

  /// <summary>
  /// Resamples a bitmap using an edge-aware algorithm with custom equality threshold.
  /// </summary>
  /// <typeparam name="TResampler">The edge-aware resampler type.</typeparam>
  /// <typeparam name="TEquality">The color equality comparer type.</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="resampler">The edge-aware resampler instance.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <param name="equality">The equality comparer for similarity detection.</param>
  /// <returns>A new bitmap scaled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler, TEquality>(
    this Bitmap source,
    TResampler resampler,
    int targetWidth,
    int targetHeight,
    TEquality equality,
    __EdgeAwareResamplerTag<TResampler> _ = default)
    where TResampler : struct, IEdgeAwareResampler
    where TEquality : struct, IColorEquality<OklabF> {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var callback = new EdgeAwareResampleCallback<
      LinearRgbaF, OklabF,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
      TEquality>(source, targetWidth, targetHeight);
    return resampler.InvokeKernel<
      LinearRgbaF, OklabF, Bgra8888,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
      TEquality, Bitmap>(
      callback, source.Width, source.Height, targetWidth, targetHeight, equality);
  }

  #endregion

  #region Content-Aware Resampling

  /// <summary>
  /// Callback that receives a concrete content-aware kernel and executes the resize pipeline.
  /// </summary>
  private sealed class ContentAwareResampleCallback<TWork, TKey, TDecode, TProject, TEncode, TMetric, TLerp>(Bitmap source, int targetWidth, int targetHeight)
    : IContentAwareKernelCallback<TWork, TKey, Bgra8888, TDecode, TProject, TEncode, TMetric, TLerp, Bitmap>
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TMetric : struct, IColorMetric<TKey>
    where TLerp : struct, ILerp<TWork> {

    public Bitmap Invoke<TKernel>(TKernel kernel)
      where TKernel : struct, IContentAwareKernel<TWork, TKey, Bgra8888, TDecode, TProject, TEncode, TMetric, TLerp>
      => ResampleContentAware<TWork, TKey, TDecode, TProject, TEncode, TMetric, TLerp, TKernel>(source, targetWidth, targetHeight, kernel);
  }

  /// <summary>
  /// Internal content-aware resize implementation.
  /// </summary>
  internal static unsafe Bitmap ResampleContentAware<TWork, TKey, TDecode, TProject, TEncode, TMetric, TLerp, TKernel>(
    Bitmap source,
    int targetWidth,
    int targetHeight,
    TKernel kernel)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888>
    where TMetric : struct, IColorMetric<TKey>
    where TLerp : struct, ILerp<TWork>
    where TKernel : struct, IContentAwareKernel<TWork, TKey, Bgra8888, TDecode, TProject, TEncode, TMetric, TLerp> {
    var result = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);

    using var srcLocker = new Argb8888BitmapLocker(source, ImageLockMode.ReadOnly);
    using var dstLocker = new Argb8888BitmapLocker(result, ImageLockMode.WriteOnly);

    var srcFrame = srcLocker.AsFrame();
    var dstFrame = dstLocker.AsFrame();

    TDecode decoder = default;
    TProject projector = default;
    TEncode encoder = default;
    TMetric metric = default;
    TLerp lerp = default;

    fixed (Bgra8888* srcPtr = srcFrame.ReadOnlyPixels)
    fixed (Bgra8888* dstPtr = dstFrame.Pixels)
      kernel.Resize(srcPtr, srcFrame.Stride, dstPtr, dstFrame.Stride,
        in decoder, in projector, in encoder, in metric, in lerp);

    return result;
  }

  /// <summary>
  /// Resamples a bitmap using content-aware algorithm with default configuration.
  /// </summary>
  /// <typeparam name="TResampler">The content-aware resampler type (e.g., SeamCarving).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <returns>A new bitmap resampled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, int targetWidth, int targetHeight, __ContentAwareResamplerTag<TResampler> _ = default)
    where TResampler : struct, IContentAwareResampler
    => source.Resample(default(TResampler), targetWidth, targetHeight);

  /// <summary>
  /// Resamples a bitmap using content-aware algorithm with default configuration.
  /// </summary>
  /// <typeparam name="TResampler">The content-aware resampler type (e.g., SeamCarving).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="targetSize">Target dimensions.</param>
  /// <returns>A new bitmap resampled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, Size targetSize, __ContentAwareResamplerTag<TResampler> _ = default)
    where TResampler : struct, IContentAwareResampler
    => source.Resample<TResampler>(targetSize.Width, targetSize.Height);

  /// <summary>
  /// Resamples a bitmap using a configured content-aware resampler instance.
  /// </summary>
  /// <typeparam name="TResampler">The content-aware resampler type (e.g., SeamCarving).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="resampler">The resampler instance with custom configuration.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <returns>A new bitmap resampled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, TResampler resampler, int targetWidth, int targetHeight, __ContentAwareResamplerTag<TResampler> _ = default)
    where TResampler : struct, IContentAwareResampler {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var callback = new ContentAwareResampleCallback<
      LinearRgbaF, OklabF,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
      Euclidean3F<OklabF>, Color4FLerp<LinearRgbaF>>(source, targetWidth, targetHeight);
    return resampler.InvokeKernel<
      LinearRgbaF, OklabF, Bgra8888,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
      Euclidean3F<OklabF>, Color4FLerp<LinearRgbaF>, Bitmap>(
      callback, source.Width, source.Height, targetWidth, targetHeight);
  }

  /// <summary>
  /// Resamples a bitmap using a configured content-aware resampler instance.
  /// </summary>
  /// <typeparam name="TResampler">The content-aware resampler type (e.g., SeamCarving).</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="resampler">The resampler instance with custom configuration.</param>
  /// <param name="targetSize">Target dimensions.</param>
  /// <returns>A new bitmap resampled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler>(this Bitmap source, TResampler resampler, Size targetSize, __ContentAwareResamplerTag<TResampler> _ = default)
    where TResampler : struct, IContentAwareResampler
    => source.Resample(resampler, targetSize.Width, targetSize.Height);

  /// <summary>
  /// Resamples a bitmap using content-aware algorithm with a custom metric.
  /// </summary>
  /// <typeparam name="TResampler">The content-aware resampler type.</typeparam>
  /// <typeparam name="TMetric">The color metric type for energy computation.</typeparam>
  /// <param name="source">Source bitmap.</param>
  /// <param name="resampler">The resampler instance.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <returns>A new bitmap resampled to the target dimensions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap Resample<TResampler, TMetric>(
    this Bitmap source,
    TResampler resampler,
    int targetWidth,
    int targetHeight,
    __ContentAwareResamplerTag<TResampler> _ = default)
    where TResampler : struct, IContentAwareResampler
    where TMetric : struct, IColorMetric<OklabF> {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var callback = new ContentAwareResampleCallback<
      LinearRgbaF, OklabF,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
      TMetric, Color4FLerp<LinearRgbaF>>(source, targetWidth, targetHeight);
    return resampler.InvokeKernel<
      LinearRgbaF, OklabF, Bgra8888,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
      TMetric, Color4FLerp<LinearRgbaF>, Bitmap>(
      callback, source.Width, source.Height, targetWidth, targetHeight);
  }

  #endregion

}
