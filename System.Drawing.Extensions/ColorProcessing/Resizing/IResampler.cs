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

using System.Drawing.Extensions.ColorProcessing.Resizing;
using Hawkynt.ColorProcessing.Codecs;

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Interface for continuous-scale resamplers.
/// </summary>
/// <remarks>
/// <para>
/// Resamplers like Lanczos, Bilinear, and Bicubic support arbitrary
/// target dimensions. Unlike <see cref="IPixelScaler"/>, resamplers
/// do not have fixed scale factors - target dimensions are passed
/// at call time.
/// </para>
/// <para>
/// Each concrete resampler provides a <see cref="Radius"/> that determines
/// how many source pixels are sampled in each direction.
/// </para>
/// </remarks>
public interface IResampler : IScalerInfo {

  /// <summary>
  /// Gets the kernel radius (e.g., 2 for Lanczos-2, 3 for Lanczos-3).
  /// </summary>
  /// <remarks>
  /// The kernel samples from -(radius-1) to +radius in each dimension.
  /// </remarks>
  int Radius { get; }

  /// <summary>
  /// Gets the prefilter parameters for this resampler, if any.
  /// </summary>
  /// <remarks>
  /// <para>
  /// B-splines and o-Moms require prefiltering the source image before convolution
  /// to achieve proper interpolation. The prefilter converts discrete samples to
  /// spline coefficients using recursive IIR filtering.
  /// </para>
  /// <para>
  /// Returns <c>null</c> for resamplers that don't require prefiltering.
  /// </para>
  /// </remarks>
  PrefilterInfo? Prefilter { get; }

  /// <summary>
  /// Invokes a callback with the concrete kernel type, enabling struct-constrained dispatch.
  /// </summary>
  /// <typeparam name="TWork">The working color type (for accumulation).</typeparam>
  /// <typeparam name="TKey">The key color type (for pattern matching compatibility).</typeparam>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
  /// <typeparam name="TResult">The return type of the callback.</typeparam>
  /// <param name="callback">The callback to invoke with the concrete kernel.</param>
  /// <param name="sourceWidth">Width of the source image.</param>
  /// <param name="sourceHeight">Height of the source image.</param>
  /// <param name="targetWidth">Width of the target image.</param>
  /// <param name="targetHeight">Height of the target image.</param>
  /// <param name="useCenteredGrid">If true, use centered grid sampling; otherwise top-left aligned.</param>
  /// <returns>The result from the callback.</returns>
  /// <remarks>
  /// <para>
  /// This method enables zero-overhead dispatch by passing the concrete kernel type
  /// to the callback, which can then use struct-constrained generic methods.
  /// Interface dispatch occurs once per call, not per pixel.
  /// </para>
  /// <para>
  /// When <paramref name="useCenteredGrid"/> is true (default), destination pixel centers
  /// are mapped to source coordinates: srcX = (destX + 0.5) * scale - 0.5.
  /// When false, top-left corners are mapped: srcX = destX * scale.
  /// </para>
  /// </remarks>
  TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>;
}
