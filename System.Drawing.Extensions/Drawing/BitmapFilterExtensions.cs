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
