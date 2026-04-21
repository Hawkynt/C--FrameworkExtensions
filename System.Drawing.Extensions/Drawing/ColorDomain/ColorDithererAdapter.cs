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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.Drawing.Lockers;

namespace Hawkynt.Drawing.ColorDomain;

/// <summary>
/// Wraps any <see cref="Hawkynt.ColorProcessing.IDitherer"/> and exposes it via the
/// Color-domain <see cref="IColorDitherer"/> contract.
/// </summary>
/// <remarks>
/// This adapter is the canonical bridge from <see cref="System.Drawing.Color"/>
/// + <see cref="IBitmapLocker"/> + <see cref="BitmapData"/> to the extension's
/// pointer-based generic dispatch. It always operates in the <see cref="Bgra8888"/>
/// work space using <see cref="IdentityDecode{TPixel}"/> + <see cref="EuclideanSquared4B{TKey}"/>;
/// per-call source pixels are copied into a managed Bgra8888 buffer so the source
/// locker may use any pixel format.
/// </remarks>
public sealed class ColorDithererAdapter : IColorDitherer {

  private readonly Hawkynt.ColorProcessing.IDitherer _inner;

  public ColorDithererAdapter(Hawkynt.ColorProcessing.IDitherer inner)
    => this._inner = inner ?? throw new ArgumentNullException(nameof(inner));

  /// <summary>The underlying extension ditherer; useful for downcasting to call
  /// algorithm-specific helpers (e.g. <c>ErrorDiffusion.Serpentine</c>).</summary>
  public Hawkynt.ColorProcessing.IDitherer Inner => this._inner;

  public unsafe void Dither(
    IBitmapLocker source,
    BitmapData target,
    Color[] palette,
    Func<Color, Color, int>? colorDistanceMetric = null) {
    var width = source.Width;
    var height = source.Height;

    // Empty palette: zero-fill the indexed target. PaletteLookup would crash on a
    // zero-length palette, but the IColorDitherer contract treats this as a no-op
    // (every pixel collapses to index 0).
    if (palette.Length == 0) {
      var idx = (byte*)target.Scan0;
      for (var y = 0; y < height; ++y) {
        var row = idx + y * target.Stride;
        for (var x = 0; x < width; ++x)
          row[x] = 0;
      }
      return;
    }

    var bgraPalette = new Bgra8888[palette.Length];
    for (var i = 0; i < palette.Length; ++i)
      bgraPalette[i] = new Bgra8888(palette[i]);

    var srcBuffer = new Bgra8888[width * height];
    for (var y = 0; y < height; ++y)
      for (var x = 0; x < width; ++x)
        srcBuffer[y * width + x] = new Bgra8888(source[x, y]);

    fixed (Bgra8888* srcPtr = srcBuffer) {
      var idxPtr = (byte*)target.Scan0;
      var decoder = default(IdentityDecode<Bgra8888>);
      var metric = default(EuclideanSquared4B<Bgra8888>);

      this._inner.Dither<Bgra8888, Bgra8888, IdentityDecode<Bgra8888>, EuclideanSquared4B<Bgra8888>>(
        srcPtr, idxPtr, width, height,
        width, target.Stride, 0,
        decoder, metric, bgraPalette);
    }
  }
}
