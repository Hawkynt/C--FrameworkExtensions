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
using Guard;

namespace Hawkynt.ColorProcessing.Geometry;

/// <summary>
/// Arbitrary-angle bitmap rotation with selectable interpolation kernel and
/// boundary fill mode.
/// </summary>
/// <remarks>
/// <para>
/// Rotates around the source bitmap's centre. The destination bitmap is
/// expanded so the entire rotated image fits — no clipping. The four
/// corners that appear due to the bounding-box expansion are filled
/// according to <see cref="FillSpec"/>.
/// </para>
/// <para>
/// Inverse mapping is used (each destination pixel queries the source);
/// interpolation is performed by the chosen <see cref="GeometricInterpolation"/>
/// kernel via <see cref="GeometricSampler"/>. Output is always
/// <see cref="PixelFormat.Format32bppArgb"/>.
/// </para>
/// </remarks>
public static class Rotate {

  /// <summary>Rotates <paramref name="source"/> by <paramref name="angleDegrees"/>.</summary>
  /// <param name="source">The source bitmap. Not mutated.</param>
  /// <param name="angleDegrees">Rotation angle in degrees. Positive = counter-clockwise.</param>
  /// <param name="interpolation">
  /// Interpolation kernel (default <see cref="GeometricInterpolation.Bilinear"/>).
  /// </param>
  /// <param name="fill">
  /// Fill specification for destination corners that map outside the source
  /// (default <see cref="FillSpec.Transparent"/>).
  /// </param>
  /// <returns>A new <see cref="PixelFormat.Format32bppArgb"/> bitmap containing the rotated image.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
  public static Bitmap Apply(Bitmap source, double angleDegrees, GeometricInterpolation interpolation = GeometricInterpolation.Bilinear, FillSpec fill = default) {
    Against.ArgumentIsNull(source);
    if (fill.Mode == default && fill.Color.ToArgb() == 0)
      fill = FillSpec.Transparent;

    var w = source.Width;
    var h = source.Height;

    var rad = angleDegrees * Math.PI / 180.0;
    var cos = Math.Cos(rad);
    var sin = Math.Sin(rad);

    // Bounding box of the rotated rectangle.
    var absC = Math.Abs(cos);
    var absS = Math.Abs(sin);
    var newW = (int)Math.Ceiling(w * absC + h * absS);
    var newH = (int)Math.Ceiling(w * absS + h * absC);
    if (newW < 1) newW = 1;
    if (newH < 1) newH = 1;

    var result = new Bitmap(newW, newH, PixelFormat.Format32bppArgb);

    // Inverse rotation: dest -> src.
    // We rotate around the centre of each image, so:
    //   srcX = cos*(dx - newW/2) + sin*(dy - newH/2) + w/2
    //   srcY = -sin*(dx - newW/2) + cos*(dy - newH/2) + h/2
    var cxDst = (newW - 1) * 0.5;
    var cyDst = (newH - 1) * 0.5;
    var cxSrc = (w - 1) * 0.5;
    var cySrc = (h - 1) * 0.5;

    using var srcLock = source.Lock(ImageLockMode.ReadOnly);
    using var dstLock = result.Lock(ImageLockMode.WriteOnly);
    for (var y = 0; y < newH; ++y) {
      var dy = y - cyDst;
      for (var x = 0; x < newW; ++x) {
        var dx = x - cxDst;
        var srcX = cos * dx + sin * dy + cxSrc;
        var srcY = -sin * dx + cos * dy + cySrc;
        dstLock[x, y] = GeometricSampler.Sample(srcLock, srcX, srcY, interpolation, fill);
      }
    }

    return result;
  }
}
