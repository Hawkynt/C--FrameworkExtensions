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
/// Horizontal and vertical bitmap shearing.
/// </summary>
/// <remarks>
/// <para>
/// The shear is parameterised as raw factors (slopes), <em>not</em> angles —
/// avoid the <c>tan</c>-of-90° singularity and let the caller pre-compute
/// angle-derived slopes if they need angle semantics.
/// </para>
/// <para>
/// Forward map: <c>(x, y) → (x + factorX·y, y + factorY·x)</c>.
/// </para>
/// <para>
/// The destination size is the bounding box of the sheared parallelogram.
/// Sampling uses <see cref="GeometricSampler"/> with the chosen interpolation
/// and the given <see cref="FillSpec"/>; out-of-parallelogram pixels are filled
/// per the spec.
/// </para>
/// </remarks>
public static class Shear {

  /// <summary>Shears <paramref name="source"/> by the given slope factors.</summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="factorX">Horizontal shear slope (Δx per unit y). 0 = no horizontal shear.</param>
  /// <param name="factorY">Vertical shear slope (Δy per unit x). 0 = no vertical shear.</param>
  /// <param name="interpolation">Interpolation kernel.</param>
  /// <param name="fill">Boundary fill specification.</param>
  /// <returns>A new ARGB bitmap whose dimensions match the bounding box of the sheared image.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
  /// <exception cref="ArgumentException">
  /// The forward map is singular (<c>1 - factorX·factorY = 0</c>); the inverse
  /// is undefined.
  /// </exception>
  public static Bitmap Apply(Bitmap source, double factorX, double factorY, GeometricInterpolation interpolation = GeometricInterpolation.Bilinear, FillSpec fill = default) {
    Against.ArgumentIsNull(source);
    if (fill.Mode == default && fill.Color.ToArgb() == 0)
      fill = FillSpec.Transparent;

    var w = source.Width;
    var h = source.Height;

    // Bounding box of the four sheared corners.
    var x0 = 0.0;
    var y0 = 0.0;
    var x1 = w + factorX * 0;
    var y1 = factorY * w;
    var x2 = factorX * h;
    var y2 = h + 0;
    var x3 = w + factorX * h;
    var y3 = h + factorY * w;

    var minX = Math.Min(Math.Min(x0, x1), Math.Min(x2, x3));
    var maxX = Math.Max(Math.Max(x0, x1), Math.Max(x2, x3));
    var minY = Math.Min(Math.Min(y0, y1), Math.Min(y2, y3));
    var maxY = Math.Max(Math.Max(y0, y1), Math.Max(y2, y3));

    var newW = Math.Max(1, (int)Math.Ceiling(maxX - minX));
    var newH = Math.Max(1, (int)Math.Ceiling(maxY - minY));

    // Inverse of the 2×2 shear matrix [[1, fx], [fy, 1]].
    var det = 1.0 - factorX * factorY;
    if (Math.Abs(det) < 1e-12)
      throw new ArgumentException("Singular shear (factorX * factorY ≈ 1).");
    var invDet = 1.0 / det;

    var result = new Bitmap(newW, newH, PixelFormat.Format32bppArgb);
    using var srcLock = source.Lock(ImageLockMode.ReadOnly);
    using var dstLock = result.Lock(ImageLockMode.WriteOnly);

    for (var y = 0; y < newH; ++y) {
      var dy = y + minY;
      for (var x = 0; x < newW; ++x) {
        var dx = x + minX;
        // Inverse shear: src = [[1,-fx],[-fy,1]] * dest * (1/det)
        var srcX = (dx - factorX * dy) * invDet;
        var srcY = (dy - factorY * dx) * invDet;
        dstLock[x, y] = GeometricSampler.Sample(srcLock, srcX, srcY, interpolation, fill);
      }
    }

    return result;
  }
}
