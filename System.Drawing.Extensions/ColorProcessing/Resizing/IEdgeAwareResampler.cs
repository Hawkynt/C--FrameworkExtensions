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
using Hawkynt.ColorProcessing.Metrics;
using PrefilterInfo = System.Drawing.Extensions.ColorProcessing.Resizing.PrefilterInfo;

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Interface for edge-aware resamplers that use color equality for similarity detection.
/// </summary>
/// <remarks>
/// <para>
/// Edge-aware resamplers like Kopf-Lischinski build similarity graphs between pixels
/// to preserve edges and create smooth vector-like output. Unlike standard resamplers
/// that only interpolate, these algorithms analyze pixel relationships using an
/// equality predicate to determine which pixels should blend together.
/// </para>
/// <para>
/// The <typeparamref name="TEquality"/> parameter allows customizing the similarity
/// threshold, enabling perceptual color matching with configurable tolerance.
/// </para>
/// </remarks>
public interface IEdgeAwareResampler : IScalerInfo {

  /// <summary>
  /// Gets the kernel radius for neighborhood sampling.
  /// </summary>
  int Radius { get; }

  /// <summary>
  /// Gets the prefilter parameters for this resampler, if any.
  /// </summary>
  PrefilterInfo? Prefilter { get; }

  /// <summary>
  /// Invokes a callback with the concrete kernel type, enabling struct-constrained dispatch.
  /// </summary>
  /// <typeparam name="TWork">The working color type (for interpolation).</typeparam>
  /// <typeparam name="TKey">The key color type (for similarity detection).</typeparam>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
  /// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
  /// <typeparam name="TEquality">The color equality type for similarity detection.</typeparam>
  /// <typeparam name="TResult">The return type of the callback.</typeparam>
  /// <param name="callback">The callback to invoke with the concrete kernel.</param>
  /// <param name="sourceWidth">Width of the source image.</param>
  /// <param name="sourceHeight">Height of the source image.</param>
  /// <param name="targetWidth">Width of the target image.</param>
  /// <param name="targetHeight">Height of the target image.</param>
  /// <param name="equality">The equality comparer for similarity detection.</param>
  /// <param name="useCenteredGrid">If true, use centered grid sampling; otherwise top-left aligned.</param>
  /// <returns>The result from the callback.</returns>
  TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TEquality, TResult>(
    IEdgeAwareResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TEquality, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight,
    TEquality equality = default,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TEquality : struct, IColorEquality<TKey>;
}
