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

using System.Drawing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Dithering;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Quantization;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Provides extension methods for color quantization and dithering of Bitmaps.
/// </summary>
public static class BitmapQuantizationExtensions {

  /// <param name="this">Source bitmap.</param>
  extension(Bitmap @this) {
    /// <summary>
    /// Reduces colors in a bitmap using Linear RGB color space for high-quality gamma-correct results.
    /// </summary>
    /// <param name="quantizer">The quantizer to generate the palette.</param>
    /// <param name="ditherer">The ditherer for error diffusion.</param>
    /// <param name="colorCount">The target number of colors (1-256).</param>
    /// <param name="isHighQuality">Whether to use Linear RGB with floats or BGRA with int-only calculations</param>
    /// <returns>
    /// A new indexed bitmap with pixel format based on color count:
    /// 2 colors → 1bpp, ≤16 colors → 4bpp, ≤256 colors → 8bpp.
    /// </returns>
    /// <example>
    /// <code>
    /// using var original = new Bitmap("photo.png");
    /// using var indexed = original.ReduceColors(new OctreeQuantizer(), ErrorDiffusion.FloydSteinberg, 16);
    /// indexed.Save("indexed.gif");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ReduceColors<TQuantizer, TDitherer>(
      TQuantizer quantizer,
      TDitherer ditherer,
      int colorCount = 256,
      bool isHighQuality = false
    )
      where TDitherer : struct, IDitherer
      where TQuantizer : struct, IQuantizer
      => isHighQuality
        ? QuantizationPipeline.Quantize<LinearRgbaF, Srgb32ToLinearRgbaF, LinearRgbaFToSrgb32, Euclidean4F<LinearRgbaF>>(
          @this, quantizer.CreateKernel<LinearRgbaF>(), ditherer, colorCount)
        : QuantizationPipeline.Quantize<Bgra8888, IdentityDecode<Bgra8888>, IdentityEncode<Bgra8888>, EuclideanSquared4B<Bgra8888>>(
          @this, quantizer.CreateKernel<Bgra8888>(), ditherer, colorCount);
    
    /// <summary>
    /// Reduces colors in a bitmap using default quantizer and ditherer instances.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ReduceColors<TQuantizer, TDitherer>(
      int colorCount = 256,
      bool isHighQuality = false
    )
      where TDitherer : struct, IDitherer
      where TQuantizer : struct, IQuantizer
      => @this.ReduceColors(default(TQuantizer), default(TDitherer), colorCount, isHighQuality);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ReduceColors<TQuantizer, TDitherer>(
      TQuantizer quantizer,
      int colorCount = 256,
      bool isHighQuality = false
    )
      where TDitherer : struct, IDitherer
      where TQuantizer : struct, IQuantizer
      => @this.ReduceColors(quantizer, default(TDitherer), colorCount, isHighQuality);

    /// <code>
    /// using var original = new Bitmap("photo.png");
    /// using var indexed = original.ReduceColors&lt;OctreeQuantizer, ErrorDiffusion&gt;(ErrorDiffusion.FloydSteinberg, 16);
    /// indexed.Save("indexed.gif");
    /// </code>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap ReduceColors<TQuantizer, TDitherer>(
      TDitherer ditherer,
      int colorCount = 256,
      bool isHighQuality = false
    )
      where TDitherer : struct, IDitherer
      where TQuantizer : struct, IQuantizer
      => @this.ReduceColors(default(TQuantizer), ditherer, colorCount, isHighQuality);



  }

}
