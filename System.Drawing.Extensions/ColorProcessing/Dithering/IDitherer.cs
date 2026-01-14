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

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Interface for dithering algorithms that map colors to a palette.
/// </summary>
/// <remarks>
/// <para>
/// Ditherers work with a fixed palette, finding the nearest color for each pixel
/// and optionally diffusing the quantization error to neighboring pixels.
/// </para>
/// <para>
/// The working color space <typeparamref name="TWork"/> is used for:
/// <list type="bullet">
/// <item><description>Error accumulation during error diffusion</description></item>
/// <item><description>Distance calculation via the metric</description></item>
/// <item><description>Palette representation</description></item>
/// </list>
/// </para>
/// <para>
/// For fast mode, use linear RGB (e.g., <c>LinearRgbaF</c>) with Euclidean distance.
/// For high-quality mode, use perceptual distance metrics (e.g., <c>OklabDistanceF</c>).
/// </para>
/// </remarks>
public interface IDitherer {

  /// <summary>
  /// Gets whether this ditherer requires sequential pixel processing.
  /// </summary>
  /// <remarks>
  /// Error diffusion ditherers require sequential processing as each pixel's output
  /// depends on errors propagated from previously processed pixels.
  /// Ordered ditherers can process pixels in parallel.
  /// </remarks>
  bool RequiresSequentialProcessing { get; }

  /// <summary>
  /// Invokes a callback with the concrete ditherer kernel type.
  /// </summary>
  /// <typeparam name="TWork">The working color type for error accumulation and palette matching.</typeparam>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TDecode">The decoder type (TPixel -> TWork).</typeparam>
  /// <typeparam name="TEncode">The encoder type (TWork -> TPixel).</typeparam>
  /// <typeparam name="TMetric">The color distance metric type operating on TWork.</typeparam>
  /// <typeparam name="TResult">The return type of the callback.</typeparam>
  /// <param name="callback">The callback to invoke with the concrete kernel.</param>
  /// <param name="width">Width of the image.</param>
  /// <param name="height">Height of the image.</param>
  /// <returns>The result from the callback.</returns>
  TResult InvokeKernel<TWork, TPixel, TDecode, TEncode, TMetric, TResult>(
    IDithererCallback<TWork, TPixel, TDecode, TEncode, TMetric, TResult> callback,
    int width,
    int height)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TMetric : struct, IColorMetric<TWork>;
}

/// <summary>
/// Represents a dithering kernel that processes pixels against a palette.
/// </summary>
/// <typeparam name="TWork">The working color type for error accumulation and palette matching.</typeparam>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TDecode">The decoder type (TPixel -> TWork).</typeparam>
/// <typeparam name="TEncode">The encoder type (TWork -> TPixel).</typeparam>
/// <typeparam name="TMetric">The color distance metric type.</typeparam>
/// <remarks>
/// <para>
/// The kernel provides two processing modes:
/// <list type="bullet">
/// <item><description><see cref="ProcessOrdered"/>: For ordered dithering (can be parallelized)</description></item>
/// <item><description><see cref="ProcessErrorDiffusion"/>: For error diffusion (sequential processing)</description></item>
/// </list>
/// </para>
/// <para>
/// The working color space <typeparamref name="TWork"/> is used for both palette representation
/// and distance calculation. For perceptual matching, use perceptual metrics like <c>OklabDistanceF</c>.
/// </para>
/// </remarks>
public interface IDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TPixel : unmanaged, IStorageSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TMetric : struct, IColorMetric<TWork> {

  /// <summary>
  /// Gets the image width.
  /// </summary>
  int Width { get; }

  /// <summary>
  /// Gets the image height.
  /// </summary>
  int Height { get; }

  /// <summary>
  /// Gets whether this kernel requires sequential processing.
  /// </summary>
  bool RequiresSequentialProcessing { get; }

  /// <summary>
  /// Processes a single pixel with ordered dithering against a palette.
  /// </summary>
  /// <param name="source">Pointer to source pixel data.</param>
  /// <param name="sourceStride">Source image stride in pixels.</param>
  /// <param name="x">X coordinate of the pixel.</param>
  /// <param name="y">Y coordinate of the pixel.</param>
  /// <param name="dest">Pointer to destination pixel data.</param>
  /// <param name="destStride">Destination image stride in pixels.</param>
  /// <param name="decoder">The pixel decoder (TPixel -> TWork).</param>
  /// <param name="encoder">The pixel encoder (TWork -> TPixel).</param>
  /// <param name="metric">The color distance metric.</param>
  /// <param name="palette">The palette colors in TWork space.</param>
  unsafe void ProcessOrdered(
    TPixel* source,
    int sourceStride,
    int x, int y,
    TPixel* dest,
    int destStride,
    in TDecode decoder,
    in TEncode encoder,
    in TMetric metric,
    TWork[] palette);

  /// <summary>
  /// Processes the entire image with error diffusion against a palette.
  /// </summary>
  /// <param name="source">Pointer to source pixel data.</param>
  /// <param name="sourceStride">Source image stride in pixels.</param>
  /// <param name="dest">Pointer to destination pixel data.</param>
  /// <param name="destStride">Destination image stride in pixels.</param>
  /// <param name="decoder">The pixel decoder (TPixel -> TWork).</param>
  /// <param name="encoder">The pixel encoder (TWork -> TPixel).</param>
  /// <param name="metric">The color distance metric.</param>
  /// <param name="palette">The palette colors in TWork space.</param>
  unsafe void ProcessErrorDiffusion(
    TPixel* source,
    int sourceStride,
    TPixel* dest,
    int destStride,
    in TDecode decoder,
    in TEncode encoder,
    in TMetric metric,
    TWork[] palette);
}

/// <summary>
/// Callback interface for dithering operations.
/// </summary>
/// <typeparam name="TWork">The working color type.</typeparam>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TDecode">The decoder type (TPixel -> TWork).</typeparam>
/// <typeparam name="TEncode">The encoder type (TWork -> TPixel).</typeparam>
/// <typeparam name="TMetric">The color distance metric type.</typeparam>
/// <typeparam name="TResult">The return type of the callback.</typeparam>
public interface IDithererCallback<TWork, TPixel, TDecode, TEncode, TMetric, out TResult>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TPixel : unmanaged, IStorageSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TMetric : struct, IColorMetric<TWork> {

  /// <summary>
  /// Invokes the callback with a concrete ditherer kernel type.
  /// </summary>
  /// <typeparam name="TKernel">The concrete kernel type.</typeparam>
  /// <param name="kernel">The kernel instance.</param>
  /// <returns>The result of the operation.</returns>
  TResult Invoke<TKernel>(TKernel kernel)
    where TKernel : struct, IDithererKernel<TWork, TPixel, TDecode, TEncode, TMetric>;
}
