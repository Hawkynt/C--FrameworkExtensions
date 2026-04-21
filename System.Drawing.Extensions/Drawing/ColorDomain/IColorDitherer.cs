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
using Hawkynt.Drawing.Lockers;

namespace Hawkynt.Drawing.ColorDomain;

/// <summary>
/// Color-domain ditherer contract. Operates directly on <see cref="Color"/>-based
/// palettes locked through <see cref="IBitmapLocker"/>, so callers don't have to learn
/// the generic <c>TWork</c>/<c>TPixel</c>/<c>TDecode</c>/<c>TMetric</c> machinery of
/// <see cref="Hawkynt.ColorProcessing.IDitherer"/>.
/// </summary>
/// <remarks>
/// Built for tools that pick algorithms by name at runtime (e.g. CLIs).
/// Get instances from <see cref="ColorDithererRegistry"/> or wrap an extension
/// ditherer directly via <see cref="ColorDithererAdapter"/>.
/// </remarks>
public interface IColorDitherer {

  /// <summary>
  /// Dithers <paramref name="source"/> into the indexed <paramref name="target"/>
  /// against <paramref name="palette"/>.
  /// </summary>
  /// <param name="source">Locked source bitmap; any pixel format is allowed.</param>
  /// <param name="target">Locked target bitmap (typically <see cref="PixelFormat.Format8bppIndexed"/>).</param>
  /// <param name="palette">Palette colors, indexed; entry 0 is written when the palette is empty.</param>
  /// <param name="colorDistanceMetric">
  /// Optional runtime distance function. <see langword="null"/> means use the adapter's
  /// default (typically Euclidean over Bgra8888). Most algorithms internally convert
  /// to a normalized work space, so this metric only applies where the algorithm itself
  /// performs the closest-color lookup.
  /// </param>
  void Dither(
    IBitmapLocker source,
    BitmapData target,
    Color[] palette,
    Func<Color, Color, int>? colorDistanceMetric = null);
}
