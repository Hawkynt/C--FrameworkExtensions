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
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Interface for content-aware resampling algorithms that require whole-image access.
/// </summary>
/// <remarks>
/// <para>
/// Unlike kernel-based <see cref="IResampler"/>, content-aware resamplers need access to the entire
/// image to make global decisions about which pixels to preserve, remove, or duplicate.
/// </para>
/// <para>
/// Examples include seam carving, which uses dynamic programming to find optimal seams across
/// the entire image based on energy maps.
/// </para>
/// </remarks>
public interface IContentAwareResampler : IScalerInfo {

  /// <summary>
  /// Invokes a callback with the concrete kernel type, enabling struct-constrained dispatch.
  /// </summary>
  /// <typeparam name="TWork">The working color type (for energy computation and blending).</typeparam>
  /// <typeparam name="TKey">The key color type (for metric computation).</typeparam>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
  /// <typeparam name="TMetric">The color metric type for energy computation.</typeparam>
  /// <typeparam name="TLerp">The interpolation type for color blending.</typeparam>
  /// <typeparam name="TResult">The return type of the callback.</typeparam>
  /// <param name="callback">The callback to invoke with the concrete kernel.</param>
  /// <param name="sourceWidth">Width of the source image.</param>
  /// <param name="sourceHeight">Height of the source image.</param>
  /// <param name="targetWidth">Width of the target image.</param>
  /// <param name="targetHeight">Height of the target image.</param>
  /// <returns>The result from the callback.</returns>
  TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TMetric, TLerp, TResult>(
    IContentAwareKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TMetric, TLerp, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TMetric : struct, IColorMetric<TKey>
    where TLerp : struct, ILerp<TWork>;
}

/// <summary>
/// Callback interface for content-aware kernel dispatch.
/// </summary>
/// <typeparam name="TWork">The working color type.</typeparam>
/// <typeparam name="TKey">The key color type.</typeparam>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TDecode">The decoder type.</typeparam>
/// <typeparam name="TProject">The projector type.</typeparam>
/// <typeparam name="TEncode">The encoder type.</typeparam>
/// <typeparam name="TMetric">The color metric type.</typeparam>
/// <typeparam name="TLerp">The interpolation type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public interface IContentAwareKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TMetric, TLerp, TResult>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork> {

  /// <summary>
  /// Invokes the content-aware resizing operation with the concrete kernel.
  /// </summary>
  /// <typeparam name="TKernel">The concrete kernel type.</typeparam>
  /// <param name="kernel">The kernel instance.</param>
  /// <returns>The result of the operation.</returns>
  TResult Invoke<TKernel>(TKernel kernel)
    where TKernel : struct, IContentAwareKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TMetric, TLerp>;
}

/// <summary>
/// Kernel interface for content-aware resizing operations.
/// </summary>
/// <typeparam name="TWork">The working color type.</typeparam>
/// <typeparam name="TKey">The key color type.</typeparam>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TDecode">The decoder type.</typeparam>
/// <typeparam name="TProject">The projector type.</typeparam>
/// <typeparam name="TEncode">The encoder type.</typeparam>
/// <typeparam name="TMetric">The color metric type.</typeparam>
/// <typeparam name="TLerp">The interpolation type.</typeparam>
public interface IContentAwareKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TMetric, TLerp>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork> {

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
  /// Performs the content-aware resizing operation.
  /// </summary>
  /// <param name="source">Pointer to the source pixel data.</param>
  /// <param name="sourceStride">Stride of the source buffer in pixels.</param>
  /// <param name="dest">Pointer to the destination pixel data.</param>
  /// <param name="destStride">Stride of the destination buffer in pixels.</param>
  /// <param name="decoder">The decoder for converting TPixel → TWork.</param>
  /// <param name="projector">The projector for converting TWork → TKey.</param>
  /// <param name="encoder">The encoder for converting TWork → TPixel.</param>
  /// <param name="metric">The metric for computing color distances.</param>
  /// <param name="lerp">The interpolation for blending colors.</param>
  unsafe void Resize(
    TPixel* source,
    int sourceStride,
    TPixel* dest,
    int destStride,
    in TDecode decoder,
    in TProject projector,
    in TEncode encoder,
    in TMetric metric,
    in TLerp lerp);
}
