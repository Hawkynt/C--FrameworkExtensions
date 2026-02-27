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
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;

namespace Hawkynt.ColorProcessing.Filtering;

/// <summary>
/// Interface for 1:1 pixel filters that transform each pixel without changing dimensions.
/// </summary>
/// <remarks>
/// <para>
/// Filters operate on the same pixel grid as the source image (scale factor 1x1).
/// They use the same <see cref="IScaler{TWork,TKey,TPixel,TEncode}"/> kernel infrastructure
/// and <see cref="ScalerPipeline"/> execution engine as pixel scalers, but produce
/// same-size output instead of scaled output.
/// </para>
/// <para>
/// Examples include color correction (VonKries), enhancement (Sharpen, Blur),
/// and analysis (Threshold, Grayscale, Channel Extraction).
/// </para>
/// </remarks>
public interface IPixelFilter {

  /// <summary>
  /// Invokes a callback with the concrete kernel type, enabling struct-constrained dispatch.
  /// </summary>
  /// <typeparam name="TWork">The working color type (for interpolation).</typeparam>
  /// <typeparam name="TKey">The key color type (for pattern matching).</typeparam>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TDistance">The color distance metric type.</typeparam>
  /// <typeparam name="TEquality">The color equality comparer type.</typeparam>
  /// <typeparam name="TLerp">The color interpolation type.</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
  /// <typeparam name="TResult">The return type of the callback.</typeparam>
  /// <param name="callback">The callback to invoke with the concrete kernel.</param>
  /// <param name="equality">The equality comparer instance.</param>
  /// <param name="lerp">The lerp instance.</param>
  /// <returns>The result from the callback.</returns>
  TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>;
}
