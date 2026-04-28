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
/// 2×3 affine bitmap warp.
/// </summary>
/// <remarks>
/// <para>
/// Forward map (column vector convention):
/// <code>
/// [ x' ]   [ M11 M12 M13 ] [ x ]
/// [ y' ] = [ M21 M22 M23 ] [ y ]
///                          [ 1 ]
/// </code>
/// </para>
/// <para>
/// Composes uniformly with rotation, non-uniform scale, shear, and translation.
/// Convenience factory methods (<see cref="FromComponents"/>,
/// <see cref="FromTranslation"/>, etc.) cover the common cases.
/// </para>
/// <para>
/// The destination size is the bounding box of the four warped source corners
/// unless overridden with <see cref="Apply(Bitmap, AffineMatrix, int, int, GeometricInterpolation, FillSpec)"/>.
/// </para>
/// </remarks>
public static class AffineTransform {

  /// <summary>Applies an affine warp; output is sized to the bounding box of the warped corners.</summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="matrix">The forward affine transform.</param>
  /// <param name="interpolation">Interpolation kernel.</param>
  /// <param name="fill">Boundary fill specification.</param>
  /// <returns>A new ARGB bitmap.</returns>
  public static Bitmap Apply(Bitmap source, AffineMatrix matrix, GeometricInterpolation interpolation = GeometricInterpolation.Bilinear, FillSpec fill = default) {
    Against.ArgumentIsNull(source);
    var w = source.Width;
    var h = source.Height;

    // Bounding box of the four forward-mapped corners.
    var (x0, y0) = matrix.Map(0, 0);
    var (x1, y1) = matrix.Map(w, 0);
    var (x2, y2) = matrix.Map(0, h);
    var (x3, y3) = matrix.Map(w, h);
    var minX = Math.Min(Math.Min(x0, x1), Math.Min(x2, x3));
    var maxX = Math.Max(Math.Max(x0, x1), Math.Max(x2, x3));
    var minY = Math.Min(Math.Min(y0, y1), Math.Min(y2, y3));
    var maxY = Math.Max(Math.Max(y0, y1), Math.Max(y2, y3));
    var newW = Math.Max(1, (int)Math.Ceiling(maxX - minX));
    var newH = Math.Max(1, (int)Math.Ceiling(maxY - minY));

    return _Apply(source, matrix, newW, newH, minX, minY, interpolation, fill);
  }

  /// <summary>Applies an affine warp into a destination of fixed dimensions, with no centring offset.</summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="matrix">The forward affine transform.</param>
  /// <param name="destWidth">Destination width in pixels.</param>
  /// <param name="destHeight">Destination height in pixels.</param>
  /// <param name="interpolation">Interpolation kernel.</param>
  /// <param name="fill">Boundary fill specification.</param>
  /// <returns>A new ARGB bitmap of the requested size.</returns>
  public static Bitmap Apply(Bitmap source, AffineMatrix matrix, int destWidth, int destHeight, GeometricInterpolation interpolation = GeometricInterpolation.Bilinear, FillSpec fill = default) {
    Against.ArgumentIsNull(source);
    Against.CountBelowOrEqualZero(destWidth);
    Against.CountBelowOrEqualZero(destHeight);
    return _Apply(source, matrix, destWidth, destHeight, 0, 0, interpolation, fill);
  }

  private static Bitmap _Apply(Bitmap source, AffineMatrix matrix, int newW, int newH, double offsetX, double offsetY, GeometricInterpolation interpolation, FillSpec fill) {
    if (fill.Mode == default && fill.Color.ToArgb() == 0)
      fill = FillSpec.Transparent;

    var inv = matrix.Inverse();
    var result = new Bitmap(newW, newH, PixelFormat.Format32bppArgb);
    using var srcLock = source.Lock(ImageLockMode.ReadOnly);
    using var dstLock = result.Lock(ImageLockMode.WriteOnly);

    for (var y = 0; y < newH; ++y) {
      var dy = y + offsetY;
      for (var x = 0; x < newW; ++x) {
        var dx = x + offsetX;
        var (srcX, srcY) = inv.Map(dx, dy);
        dstLock[x, y] = GeometricSampler.Sample(srcLock, srcX, srcY, interpolation, fill);
      }
    }

    return result;
  }
}

/// <summary>
/// 2×3 affine matrix (homogeneous third row implicit <c>[0 0 1]</c>).
/// </summary>
/// <remarks>
/// Components map a source point <c>(x, y)</c> to
/// <c>(M11·x + M12·y + M13, M21·x + M22·y + M23)</c>.
/// </remarks>
public readonly struct AffineMatrix {

  /// <summary>Row-major matrix entries.</summary>
  public double M11 { get; }

  /// <summary>Row-major matrix entries.</summary>
  public double M12 { get; }

  /// <summary>Row-major matrix entries.</summary>
  public double M13 { get; }

  /// <summary>Row-major matrix entries.</summary>
  public double M21 { get; }

  /// <summary>Row-major matrix entries.</summary>
  public double M22 { get; }

  /// <summary>Row-major matrix entries.</summary>
  public double M23 { get; }

  /// <summary>Constructs an affine matrix from its six entries.</summary>
  public AffineMatrix(double m11, double m12, double m13, double m21, double m22, double m23) {
    this.M11 = m11; this.M12 = m12; this.M13 = m13;
    this.M21 = m21; this.M22 = m22; this.M23 = m23;
  }

  /// <summary>The identity transform.</summary>
  public static AffineMatrix Identity => new(1, 0, 0, 0, 1, 0);

  /// <summary>Pure translation.</summary>
  public static AffineMatrix FromTranslation(double tx, double ty) => new(1, 0, tx, 0, 1, ty);

  /// <summary>Pure (non-uniform) scale.</summary>
  public static AffineMatrix FromScale(double sx, double sy) => new(sx, 0, 0, 0, sy, 0);

  /// <summary>Pure rotation around the origin (counter-clockwise, degrees).</summary>
  public static AffineMatrix FromRotation(double angleDegrees) {
    var rad = angleDegrees * Math.PI / 180.0;
    var c = Math.Cos(rad);
    var s = Math.Sin(rad);
    return new AffineMatrix(c, -s, 0, s, c, 0);
  }

  /// <summary>Pure shear.</summary>
  public static AffineMatrix FromShear(double sx, double sy) => new(1, sx, 0, sy, 1, 0);

  /// <summary>
  /// Composes a transform from translation, rotation (degrees), non-uniform scale and shear,
  /// applied in the order Scale → Shear → Rotate → Translate.
  /// </summary>
  public static AffineMatrix FromComponents(double tx, double ty, double angleDegrees, double sx, double sy, double shearX, double shearY) {
    var t = FromTranslation(tx, ty);
    var r = FromRotation(angleDegrees);
    var sh = FromShear(shearX, shearY);
    var sc = FromScale(sx, sy);
    return Multiply(t, Multiply(r, Multiply(sh, sc)));
  }

  /// <summary>Forward-maps a single point.</summary>
  public (double X, double Y) Map(double x, double y)
    => (this.M11 * x + this.M12 * y + this.M13, this.M21 * x + this.M22 * y + this.M23);

  /// <summary>Returns the inverse transform.</summary>
  /// <exception cref="InvalidOperationException">The transform is singular.</exception>
  public AffineMatrix Inverse() {
    var det = this.M11 * this.M22 - this.M12 * this.M21;
    if (Math.Abs(det) < 1e-15)
      throw new InvalidOperationException("Affine matrix is singular.");
    var inv = 1.0 / det;
    var a = this.M22 * inv;
    var b = -this.M12 * inv;
    var d = -this.M21 * inv;
    var e = this.M11 * inv;
    var c = -(a * this.M13 + b * this.M23);
    var f = -(d * this.M13 + e * this.M23);
    return new AffineMatrix(a, b, c, d, e, f);
  }

  /// <summary>Returns the matrix product <c>a · b</c>.</summary>
  public static AffineMatrix Multiply(AffineMatrix a, AffineMatrix b) => new(
    a.M11 * b.M11 + a.M12 * b.M21,
    a.M11 * b.M12 + a.M12 * b.M22,
    a.M11 * b.M13 + a.M12 * b.M23 + a.M13,
    a.M21 * b.M11 + a.M22 * b.M21,
    a.M21 * b.M12 + a.M22 * b.M22,
    a.M21 * b.M13 + a.M22 * b.M23 + a.M23);
}
