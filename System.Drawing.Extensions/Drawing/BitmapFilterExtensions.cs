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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Filtering;
using Hawkynt.ColorProcessing.Filtering.Filters;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Metrics.Rgb;
using Hawkynt.ColorProcessing.Resizing;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Provides extension methods for applying 1:1 pixel filters to Bitmaps.
/// </summary>
public static class BitmapFilterExtensions {

  /// <param name="this">The source bitmap.</param>
  extension(Bitmap @this) {
    /// <summary>
    /// Applies a pixel filter to a bitmap using the specified quality mode.
    /// </summary>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    /// <param name="filter">The filter instance.</param>
    /// <param name="quality">The quality mode for color operations.</param>
    /// <returns>A new bitmap with the filter applied (same dimensions as source).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ApplyFilter<TFilter>(TFilter filter, ScalerQuality quality = ScalerQuality.Fast)
      where TFilter : struct, IPixelFilter {
      // Special-case dispatch: filters whose canonical algorithm is whole-image rather
      // than per-pixel kernel-driven. Each branch implements the published algorithm via
      // a static helper (e.g., FFT-HQS for L0Smoothing); the user sees the same
      // ApplyFilter API.
      if (filter is L0Smoothing l0)
        return L0SmoothingFftHqs.Apply(@this, l0.Lambda);

      // Frame-access path for filters with large neighborhoods (uses resampler pipeline)
      if (filter is IFrameFilter { UsesFrameAccess: true } ff)
        return _ApplyViaFrame(@this, ff);

      // Standard kernel path (single or multi-pass via 5×5 NeighborWindow)
      var passCount = filter is IMultiPassFilter mp ? Math.Max(1, mp.PassCount) : 1;
      var current = _ApplySingle(@this, filter, quality);
      for (var i = 1; i < passCount; ++i) {
        var next = _ApplySingle(current, filter, quality);
        current.Dispose();
        current = next;
      }

      return current;
    }

    /// <summary>
    /// Applies a pixel filter to a bitmap using default configuration.
    /// </summary>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    /// <param name="quality">The quality mode for color operations.</param>
    /// <returns>A new bitmap with the filter applied (same dimensions as source).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ApplyFilter<TFilter>(ScalerQuality quality = ScalerQuality.Fast)
      where TFilter : struct, IPixelFilter
      => @this.ApplyFilter(new TFilter(), quality);

    /// <summary>
    /// Applies a pixel filter with FULLY-CONFIGURABLE working/key/storage color spaces,
    /// metric, equality predicate, and interpolator. The user picks every type parameter;
    /// the JIT specialises the entire pipeline at the call site (zero per-pixel virtual
    /// dispatch). Use this when you need to tune e.g. the equality threshold or work
    /// in a non-default color space (Oklab vs Lab vs YCbCr vs ...).
    /// </summary>
    /// <remarks>
    /// <para>For convenience the lib provides preset combinations via the simpler
    /// <see cref="ApplyFilter{TFilter}(TFilter, ScalerQuality)"/> overload. Use this
    /// fully-generic form when those presets don't fit (e.g., custom thresholds,
    /// custom working space).</para>
    /// </remarks>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    /// <typeparam name="TWork">The working color type (for interpolation).</typeparam>
    /// <typeparam name="TKey">The key color type (for similarity / equality).</typeparam>
    /// <typeparam name="TDecode">The decoder (Bgra8888 → TWork).</typeparam>
    /// <typeparam name="TProject">The projector (TWork → TKey).</typeparam>
    /// <typeparam name="TEncode">The encoder (TWork → Bgra8888).</typeparam>
    /// <typeparam name="TMetric">The color distance metric.</typeparam>
    /// <typeparam name="TEquality">The color equality predicate.</typeparam>
    /// <typeparam name="TLerp">The color interpolation type.</typeparam>
    /// <param name="filter">The filter instance.</param>
    /// <param name="equality">The equality predicate instance.</param>
    /// <param name="lerp">The interpolation instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ApplyFilter<TFilter, TWork, TKey, TDecode, TProject, TEncode, TMetric, TEquality, TLerp>(
      TFilter filter,
      TEquality equality = default,
      TLerp lerp = default)
      where TFilter : struct, IPixelFilter
      where TWork : unmanaged, IColorSpace
      where TKey : unmanaged, IColorSpace
      where TDecode : struct, IDecode<Bgra8888, TWork>
      where TProject : struct, IProject<TWork, TKey>
      where TEncode : struct, IEncode<TWork, Bgra8888>
      where TMetric : struct, IColorMetric<TKey>, INormalizedMetric
      where TEquality : struct, IColorEquality<TKey>
      where TLerp : struct, ILerp<TWork> {
      var callback = new FilterCallback<TWork, TKey, TDecode, TProject, TEncode>(@this);
      return filter.InvokeKernel<TWork, TKey, Bgra8888, TMetric, TEquality, TLerp, TEncode, Bitmap>(
        callback, equality, lerp);
    }

    /// <summary>
    /// Applies a filter with a custom equality predicate, keeping default Bgra8888
    /// identity pipeline (no color-space conversion). Convenience for the common case
    /// of "same algorithm, different equality threshold".
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ApplyFilterWithEquality<TFilter, TEquality>(
      TFilter filter,
      TEquality equality)
      where TFilter : struct, IPixelFilter
      where TEquality : struct, IColorEquality<Bgra8888> {
      var callback = new FilterCallback<
        Bgra8888, Bgra8888,
        IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>>(@this);
      return filter.InvokeKernel<
        Bgra8888, Bgra8888, Bgra8888,
        CompuPhaseSquared4<Bgra8888>, TEquality,
        Color4BLerpInt<Bgra8888>, IdentityEncode<Bgra8888>, Bitmap>(callback, equality);
    }

    /// <summary>
    /// Applies a filter with a custom distance metric (used by filters that compute
    /// per-pixel similarity weights, e.g. bilateral / NLM / guided-filter). Keeps
    /// default Bgra8888 identity pipeline.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ApplyFilterWithMetric<TFilter, TMetric>(TFilter filter)
      where TFilter : struct, IPixelFilter
      where TMetric : struct, IColorMetric<Bgra8888>, INormalizedMetric {
      var callback = new FilterCallback<
        Bgra8888, Bgra8888,
        IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>>(@this);
      return filter.InvokeKernel<
        Bgra8888, Bgra8888, Bgra8888,
        TMetric, ExactEquality<Bgra8888>,
        Color4BLerpInt<Bgra8888>, IdentityEncode<Bgra8888>, Bitmap>(callback);
    }

    /// <summary>
    /// Applies a filter with a custom interpolator (used by filters that lerp adjacent
    /// pixel values, e.g. blur / unsharp / sharpen). Keeps default Bgra8888 identity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ApplyFilterWithLerp<TFilter, TLerp>(
      TFilter filter,
      TLerp lerp)
      where TFilter : struct, IPixelFilter
      where TLerp : struct, ILerp<Bgra8888> {
      var callback = new FilterCallback<
        Bgra8888, Bgra8888,
        IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>>(@this);
      return filter.InvokeKernel<
        Bgra8888, Bgra8888, Bgra8888,
        CompuPhaseSquared4<Bgra8888>, ExactEquality<Bgra8888>,
        TLerp, IdentityEncode<Bgra8888>, Bitmap>(callback, default, lerp);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _ApplySingle<TFilter>(Bitmap source, TFilter filter, ScalerQuality quality)
    where TFilter : struct, IPixelFilter
    => quality switch {
      ScalerQuality.Fast => _ApplyFast(source, filter),
      ScalerQuality.HighQuality => _ApplyHighQuality(source, filter),
      _ => throw new NotSupportedException($"Quality {quality} is not supported.")
    };

  /// <summary>
  /// Applies a filter using the resampler pipeline for arbitrary-radius frame access.
  /// </summary>
  private static Bitmap _ApplyViaFrame(Bitmap source, IFrameFilter filter) {
    var callback = new FrameFilterCallback<
      LinearRgbaF, OklabF,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(source);
    return filter.InvokeFrameKernel<
      LinearRgbaF, OklabF, Bgra8888,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32, Bitmap>(
      callback, source.Width, source.Height);
  }

  /// <summary>
  /// Fast quality path using identity codecs (Bgra8888 throughout).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _ApplyFast<TFilter>(Bitmap source, TFilter filter)
    where TFilter : struct, IPixelFilter {
    var callback = new FilterCallback<
      Bgra8888, Bgra8888,
      IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>>(source);
    return filter.InvokeKernel<
      Bgra8888, Bgra8888, Bgra8888,
      CompuPhaseSquared4<Bgra8888>, ExactEquality<Bgra8888>,
      Color4BLerpInt<Bgra8888>, IdentityEncode<Bgra8888>, Bitmap>(callback);
  }

  /// <summary>
  /// High quality path using linear RGB working space and Oklab perceptual space.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Bitmap _ApplyHighQuality<TFilter>(Bitmap source, TFilter filter)
    where TFilter : struct, IPixelFilter {
    var callback = new FilterCallback<
      LinearRgbaF, OklabF,
      Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(source);
    return filter.InvokeKernel<
      LinearRgbaF, OklabF, Bgra8888,
      Euclidean3F<OklabF>, ThresholdEquality3<OklabF>,
      Color4FLerp<LinearRgbaF>, LinearRgbaFToSrgb32, Bitmap>(callback, new(0.02f, 0.04f, 0.04f));
  }

  /// <summary>
  /// Callback that receives a concrete kernel type and executes the filter pipeline.
  /// Reuses the scaler infrastructure since filter kernels are IScaler with ScaleX=1, ScaleY=1.
  /// </summary>
  private sealed class FilterCallback<TWork, TKey, TDecode, TProject, TEncode>(Bitmap source)
    : IKernelCallback<TWork, TKey, Bgra8888, TEncode, Bitmap>
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888> {

    public Bitmap Invoke<TKernel>(TKernel kernel)
      where TKernel : struct, IScaler<TWork, TKey, Bgra8888, TEncode>
      => BitmapScalerExtensions.Upscale<TWork, TKey, TDecode, TProject, TEncode, TKernel>(source, kernel);
  }

  /// <summary>
  /// Callback that receives a concrete resample kernel and routes through the resampler pipeline.
  /// Used for filters that need arbitrary pixel access beyond the 5×5 neighborhood.
  /// </summary>
  private sealed class FrameFilterCallback<TWork, TKey, TDecode, TProject, TEncode>(Bitmap source)
    : IResampleKernelCallback<TWork, TKey, Bgra8888, TDecode, TProject, TEncode, Bitmap>
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TDecode : struct, IDecode<Bgra8888, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, Bgra8888> {

    public Bitmap Invoke<TKernel>(TKernel kernel)
      where TKernel : struct, IResampleKernel<Bgra8888, TWork, TKey, TDecode, TProject, TEncode>
      => BitmapScalerExtensions.Resample<TWork, TKey, TDecode, TProject, TEncode, TKernel>(
        source, source.Width, source.Height, kernel);
  }
}
