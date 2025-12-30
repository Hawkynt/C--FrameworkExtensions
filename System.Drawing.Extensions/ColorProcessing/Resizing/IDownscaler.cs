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

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Interface for integer-ratio downscalers.
/// </summary>
/// <remarks>
/// <para>
/// Downscalers reduce image size by combining multiple source pixels
/// into single output pixels using averaging or other techniques.
/// </para>
/// <para>
/// Unlike <see cref="IPixelScaler"/> which upscales, downscalers work
/// by reading NxN blocks of source pixels and producing single output pixels.
/// Each concrete downscaler type should provide static members:
/// <list type="bullet">
/// <item><c>SupportedRatios</c> - Array of supported downscale ratios</item>
/// <item><c>SupportsRatio(int)</c> - Check if a ratio is supported</item>
/// </list>
/// </para>
/// </remarks>
public interface IDownscaler : IScalerInfo {

  /// <summary>
  /// Gets the horizontal downscale ratio (2 means source width / 2).
  /// </summary>
  int RatioX { get; }

  /// <summary>
  /// Gets the vertical downscale ratio (2 means source height / 2).
  /// </summary>
  int RatioY { get; }

  /// <summary>
  /// Invokes a callback with the concrete kernel type, enabling struct-constrained dispatch.
  /// </summary>
  /// <typeparam name="TWork">The working color type (for accumulation).</typeparam>
  /// <typeparam name="TKey">The key color type (for pattern matching compatibility).</typeparam>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
  /// <typeparam name="TResult">The return type of the callback.</typeparam>
  /// <param name="callback">The callback to invoke with the concrete kernel.</param>
  /// <returns>The result from the callback.</returns>
  /// <remarks>
  /// <para>
  /// This method enables zero-overhead dispatch by passing the concrete kernel type
  /// to the callback, which can then use struct-constrained generic methods.
  /// Interface dispatch occurs once per call, not per pixel.
  /// </para>
  /// </remarks>
  TResult InvokeKernel<TWork, TKey, TPixel, TEncode, TResult>(
    IDownscaleKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TEncode : struct, IEncode<TWork, TPixel>;
}
