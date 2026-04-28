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
/// 3×3 homography (projective) bitmap warp.
/// </summary>
/// <remarks>
/// <para>
/// A perspective transform applied to a 2-D image is a tetrahedral
/// 4-corner mapping: every quadrilateral in the source can be mapped to
/// every quadrilateral in the destination, modelling a planar surface
/// observed from an arbitrary camera pose.
/// </para>
/// <para>
/// The forward map is
/// <code>
/// [ x' ]    [ h11 h12 h13 ] [ x ]
/// [ y' ] ~  [ h21 h22 h23 ] [ y ]
/// [ w  ]    [ h31 h32 h33 ] [ 1 ]
/// </code>
/// followed by the perspective divide <c>(x'/w, y'/w)</c>.
/// </para>
/// <para>
/// Use <see cref="PerspectiveMatrix.FromCorners"/> for the common case of
/// "warp these four points onto those four points" (e.g. document
/// rectification, billboard mapping). The constructor solves the linear
/// 8-DOF system that maps the four <c>src</c> corners onto the four
/// <c>dst</c> corners.
/// </para>
/// </remarks>
public static class PerspectiveTransform {

  /// <summary>Applies a homography into a destination of fixed dimensions.</summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="matrix">The forward homography.</param>
  /// <param name="destWidth">Destination width.</param>
  /// <param name="destHeight">Destination height.</param>
  /// <param name="interpolation">Interpolation kernel.</param>
  /// <param name="fill">Boundary fill specification.</param>
  /// <returns>A new ARGB bitmap of the requested size.</returns>
  public static Bitmap Apply(Bitmap source, PerspectiveMatrix matrix, int destWidth, int destHeight, GeometricInterpolation interpolation = GeometricInterpolation.Bilinear, FillSpec fill = default) {
    Against.ArgumentIsNull(source);
    Against.CountBelowOrEqualZero(destWidth);
    Against.CountBelowOrEqualZero(destHeight);
    if (fill.Mode == default && fill.Color.ToArgb() == 0)
      fill = FillSpec.Transparent;

    var inv = matrix.Inverse();
    var result = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb);
    using var srcLock = source.Lock(ImageLockMode.ReadOnly);
    using var dstLock = result.Lock(ImageLockMode.WriteOnly);

    for (var y = 0; y < destHeight; ++y) {
      for (var x = 0; x < destWidth; ++x) {
        if (!inv.TryMap(x, y, out var srcX, out var srcY)) {
          dstLock[x, y] = fill.Mode == GeometricFillMode.Constant ? fill.Color : Color.FromArgb(0, 0, 0, 0);
          continue;
        }
        dstLock[x, y] = GeometricSampler.Sample(srcLock, srcX, srcY, interpolation, fill);
      }
    }

    return result;
  }
}

/// <summary>
/// 3×3 homography matrix (projective transform).
/// </summary>
/// <remarks>
/// Stored row-major as <see cref="H11"/>..<see cref="H33"/>.
/// Apply via <see cref="TryMap"/>, which performs the perspective divide.
/// </remarks>
public readonly struct PerspectiveMatrix {

  /// <summary>Row-major matrix entry.</summary>
  public double H11 { get; }

  /// <summary>Row-major matrix entry.</summary>
  public double H12 { get; }

  /// <summary>Row-major matrix entry.</summary>
  public double H13 { get; }

  /// <summary>Row-major matrix entry.</summary>
  public double H21 { get; }

  /// <summary>Row-major matrix entry.</summary>
  public double H22 { get; }

  /// <summary>Row-major matrix entry.</summary>
  public double H23 { get; }

  /// <summary>Row-major matrix entry.</summary>
  public double H31 { get; }

  /// <summary>Row-major matrix entry.</summary>
  public double H32 { get; }

  /// <summary>Row-major matrix entry.</summary>
  public double H33 { get; }

  /// <summary>Constructs a homography from its nine entries.</summary>
  public PerspectiveMatrix(double h11, double h12, double h13, double h21, double h22, double h23, double h31, double h32, double h33) {
    this.H11 = h11; this.H12 = h12; this.H13 = h13;
    this.H21 = h21; this.H22 = h22; this.H23 = h23;
    this.H31 = h31; this.H32 = h32; this.H33 = h33;
  }

  /// <summary>The identity homography.</summary>
  public static PerspectiveMatrix Identity => new(1, 0, 0, 0, 1, 0, 0, 0, 1);

  /// <summary>
  /// Maps <paramref name="x"/>, <paramref name="y"/> through the homography,
  /// performing the perspective divide.
  /// </summary>
  /// <returns><c>true</c> on success; <c>false</c> if the homogeneous denominator is zero (the point is at infinity).</returns>
  public bool TryMap(double x, double y, out double outX, out double outY) {
    var w = this.H31 * x + this.H32 * y + this.H33;
    if (Math.Abs(w) < 1e-12) {
      outX = 0; outY = 0;
      return false;
    }
    outX = (this.H11 * x + this.H12 * y + this.H13) / w;
    outY = (this.H21 * x + this.H22 * y + this.H23) / w;
    return true;
  }

  /// <summary>Constructs the inverse homography (3×3 cofactor inverse).</summary>
  /// <exception cref="InvalidOperationException">The homography is singular.</exception>
  public PerspectiveMatrix Inverse() {
    // 3×3 cofactor inverse.
    var a = this.H22 * this.H33 - this.H23 * this.H32;
    var b = this.H23 * this.H31 - this.H21 * this.H33;
    var c = this.H21 * this.H32 - this.H22 * this.H31;
    var d = this.H13 * this.H32 - this.H12 * this.H33;
    var e = this.H11 * this.H33 - this.H13 * this.H31;
    var f = this.H12 * this.H31 - this.H11 * this.H32;
    var g = this.H12 * this.H23 - this.H13 * this.H22;
    var h = this.H13 * this.H21 - this.H11 * this.H23;
    var i = this.H11 * this.H22 - this.H12 * this.H21;
    var det = this.H11 * a + this.H12 * b + this.H13 * c;
    if (Math.Abs(det) < 1e-15)
      throw new InvalidOperationException("Perspective matrix is singular.");
    var inv = 1.0 / det;
    return new PerspectiveMatrix(
      a * inv, d * inv, g * inv,
      b * inv, e * inv, h * inv,
      c * inv, f * inv, i * inv);
  }

  /// <summary>
  /// Builds the homography that maps <paramref name="srcCorners"/> onto
  /// <paramref name="dstCorners"/>.
  /// </summary>
  /// <param name="srcCorners">Four source quadrilateral corners.</param>
  /// <param name="dstCorners">Four destination quadrilateral corners (matching order).</param>
  /// <returns>The forward 3×3 homography.</returns>
  /// <remarks>
  /// <para>
  /// Solves the standard 8-equation DLT (Direct Linear Transform) system
  /// for the eight unknowns of the homography (with <c>h33 = 1</c>) using
  /// straightforward Gaussian elimination — fine for this 8×8 system, no
  /// external linear-algebra dependency.
  /// </para>
  /// <para>
  /// Corner ordering must be consistent (e.g. all clockwise or all
  /// counter-clockwise). For document rectification: pass the four detected
  /// corners as <c>src</c> and <c>{(0,0), (W,0), (W,H), (0,H)}</c> as <c>dst</c>.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">Either array does not have exactly four points or the system is degenerate.</exception>
  public static PerspectiveMatrix FromCorners(PointF[] srcCorners, PointF[] dstCorners) {
    Against.ArgumentIsNull(srcCorners);
    Against.ArgumentIsNull(dstCorners);
    if (srcCorners.Length != 4) throw new ArgumentException("Need exactly 4 corners.", nameof(srcCorners));
    if (dstCorners.Length != 4) throw new ArgumentException("Need exactly 4 corners.", nameof(dstCorners));

    // Build 8×9 system:  A·h = 0, normalise h33 = 1 ⇒ 8×8 with RHS.
    // For each correspondence (x, y) → (X, Y):
    //   X = (h11·x + h12·y + h13) / (h31·x + h32·y + 1)
    //   Y = (h21·x + h22·y + h23) / (h31·x + h32·y + 1)
    // Rearranging:
    //   x·h11 + y·h12 + h13 - X·x·h31 - X·y·h32 = X
    //   x·h21 + y·h22 + h23 - Y·x·h31 - Y·y·h32 = Y
    var A = new double[8, 8];
    var rhs = new double[8];
    for (var i = 0; i < 4; ++i) {
      double x = srcCorners[i].X, y = srcCorners[i].Y;
      double X = dstCorners[i].X, Y = dstCorners[i].Y;
      var r1 = i * 2;
      A[r1, 0] = x; A[r1, 1] = y; A[r1, 2] = 1;
      A[r1, 3] = 0; A[r1, 4] = 0; A[r1, 5] = 0;
      A[r1, 6] = -X * x; A[r1, 7] = -X * y;
      rhs[r1] = X;

      var r2 = i * 2 + 1;
      A[r2, 0] = 0; A[r2, 1] = 0; A[r2, 2] = 0;
      A[r2, 3] = x; A[r2, 4] = y; A[r2, 5] = 1;
      A[r2, 6] = -Y * x; A[r2, 7] = -Y * y;
      rhs[r2] = Y;
    }

    var h = _SolveGauss(A, rhs);
    return new PerspectiveMatrix(
      h[0], h[1], h[2],
      h[3], h[4], h[5],
      h[6], h[7], 1);
  }

  // Straightforward Gaussian elimination with partial pivoting on an 8×8 system.
  // Tiny enough to be inlined here; avoids dragging in System.Numerics.LinearAlgebra.
  private static double[] _SolveGauss(double[,] A, double[] b) {
    var n = b.Length;
    // Forward elimination with partial pivoting.
    for (var k = 0; k < n; ++k) {
      // Find pivot.
      var pivot = k;
      var pivotMag = Math.Abs(A[k, k]);
      for (var r = k + 1; r < n; ++r) {
        var mag = Math.Abs(A[r, k]);
        if (mag > pivotMag) {
          pivot = r;
          pivotMag = mag;
        }
      }
      if (pivotMag < 1e-15)
        throw new ArgumentException("Degenerate corner configuration (collinear or coincident).");
      if (pivot != k) {
        for (var c = 0; c < n; ++c) {
          var tmp = A[k, c]; A[k, c] = A[pivot, c]; A[pivot, c] = tmp;
        }
        var tb = b[k]; b[k] = b[pivot]; b[pivot] = tb;
      }
      // Eliminate.
      for (var r = k + 1; r < n; ++r) {
        var factor = A[r, k] / A[k, k];
        if (factor == 0) continue;
        for (var c = k; c < n; ++c)
          A[r, c] -= factor * A[k, c];
        b[r] -= factor * b[k];
      }
    }
    // Back-substitute.
    var x = new double[n];
    for (var r = n - 1; r >= 0; --r) {
      var s = b[r];
      for (var c = r + 1; c < n; ++c)
        s -= A[r, c] * x[c];
      x[r] = s / A[r, r];
    }
    return x;
  }
}
