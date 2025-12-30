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

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Interface for pixel-art scalers with discrete scale factors.
/// </summary>
/// <remarks>
/// <para>
/// Implemented by public scaler structs (Scale2x, Scale3x, Epx, Hq, etc.).
/// Used for generic dispatch in Upscale methods.
/// </para>
/// <para>
/// Pixel-art scalers operate on discrete scale factors.
/// Each concrete scaler type should provide static members:
/// <list type="bullet">
/// <item><c>SupportedScales</c> - Array of supported scale factors</item>
/// <item><c>SupportsScale(ScaleFactor)</c> - Check if a scale is supported</item>
/// <item><c>GetPossibleTargets(int, int)</c> - Enumerate valid target dimensions</item>
/// </list>
/// </para>
/// </remarks>
public interface IPixelScaler : IScalerInfo {

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
  /// <remarks>
  /// <para>
  /// This method enables zero-overhead dispatch by passing the concrete kernel type
  /// to the callback, which can then use struct-constrained generic methods.
  /// Interface dispatch occurs once per call, not per pixel.
  /// </para>
  /// </remarks>
  TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>;
}
