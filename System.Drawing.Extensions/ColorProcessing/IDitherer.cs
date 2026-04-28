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

using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing;

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
/// <para>
/// As of the refactor, the source pixels are always pre-decoded into the working colour
/// space before being handed to the ditherer. The pipeline dispatcher
/// (<see cref="Hawkynt.ColorProcessing.Internal.QuantizationPipeline"/>) is responsible for
/// performing the decode once (using <c>IBatchDecode</c> when available, scalar fallback
/// otherwise). This eliminates per-implementation decode boilerplate and ensures that the
/// per-pixel decode cost (gamma LUT, struct construction, alpha unpacking) is paid only
/// once per source pixel, not once per palette-lookup attempt.
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
  /// Dithers pre-decoded source pixels to palette indices.
  /// </summary>
  /// <typeparam name="TWork">The working color type for error accumulation and palette matching.</typeparam>
  /// <typeparam name="TMetric">The color distance metric type operating on TWork.</typeparam>
  /// <param name="source">
  /// Pointer to the start of the pre-decoded source buffer (row 0, col 0). The buffer is
  /// always tightly packed; <c>source[y * sourceStride + x]</c> is the working-space pixel
  /// at <c>(x, y)</c>.
  /// </param>
  /// <param name="indices">Pointer to caller-allocated index buffer (width * height bytes).</param>
  /// <param name="width">Width of the image in pixels.</param>
  /// <param name="height">Number of rows to process.</param>
  /// <param name="sourceStride">
  /// Source buffer stride in <typeparamref name="TWork"/> elements (not bytes). For the
  /// pipeline-allocated tight buffer this equals <paramref name="width"/>.
  /// </param>
  /// <param name="targetStride">Target index buffer stride in pixels.</param>
  /// <param name="startY">Starting row index for processing (enables row partitioning).</param>
  /// <param name="metric">The color distance metric.</param>
  /// <param name="palette">The palette colors in TWork space.</param>
  /// <remarks>
  /// The caller is responsible for allocating both the indices buffer and the pre-decoded
  /// source buffer. When <see cref="RequiresSequentialProcessing"/> is <c>false</c>, the
  /// caller may partition the image into row ranges and process them in parallel; each
  /// partition still receives the full-image source pointer plus its own
  /// <paramref name="startY"/> and <paramref name="height"/>.
  /// </remarks>
  unsafe void Dither<TWork, TMetric>(
    TWork* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork>;
}
