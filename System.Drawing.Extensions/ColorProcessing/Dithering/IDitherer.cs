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
/// Interface for dithering algorithms that output palette indices.
/// </summary>
/// <remarks>
/// <para>
/// Ditherers map source pixels to palette indices, enabling direct creation of
/// indexed bitmap formats (8bpp, 4bpp, 1bpp) without intermediate 32bpp conversion.
/// </para>
/// <para>
/// The working color space <typeparamref name="TWork"/> uses normalized UNorm32 components
/// for integer-only operations when possible.
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
  /// Dithers source pixels to palette indices.
  /// </summary>
  /// <typeparam name="TWork">The working color type for error accumulation and palette matching.</typeparam>
  /// <typeparam name="TPixel">The storage pixel type.</typeparam>
  /// <typeparam name="TDecode">The decoder type (TPixel -> TWork).</typeparam>
  /// <typeparam name="TMetric">The color distance metric type operating on TWork.</typeparam>
  /// <param name="source">Pointer to source pixel data.</param>
  /// <param name="width">Width of the image in pixels.</param>
  /// <param name="height">Height of the image in pixels.</param>
  /// <param name="stride">Source image stride in pixels (not bytes).</param>
  /// <param name="decoder">The pixel decoder (TPixel -> TWork).</param>
  /// <param name="metric">The color distance metric.</param>
  /// <param name="palette">The palette colors in TWork space.</param>
  /// <returns>An array of palette indices, one per pixel (row-major order).</returns>
  unsafe byte[] Dither<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    int width,
    int height,
    int stride,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork>;
}
