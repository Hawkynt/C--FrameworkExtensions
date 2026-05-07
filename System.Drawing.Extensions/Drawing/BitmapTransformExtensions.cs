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
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Provides geometric transform extension methods for <see cref="Bitmap"/>.
/// </summary>
public static class BitmapTransformExtensions {

  extension(Bitmap @this) {

    /// <summary>
    /// Flips the bitmap horizontally (mirrors left to right).
    /// </summary>
    /// <returns>A new bitmap with horizontally flipped content.</returns>
    public Bitmap FlipHorizontal() {
      ArgumentNullException.ThrowIfNull(@this);
      var w = @this.Width;
      var h = @this.Height;
      var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
      using var srcLock = @this.Lock(ImageLockMode.ReadOnly);
      using var dstLock = result.Lock(ImageLockMode.WriteOnly);
      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x)
        dstLock[w - 1 - x, y] = srcLock[x, y];

      return result;
    }

    /// <summary>
    /// Flips the bitmap vertically (mirrors top to bottom).
    /// </summary>
    /// <returns>A new bitmap with vertically flipped content.</returns>
    public Bitmap FlipVertical() {
      ArgumentNullException.ThrowIfNull(@this);
      var w = @this.Width;
      var h = @this.Height;
      var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
      using var srcLock = @this.Lock(ImageLockMode.ReadOnly);
      using var dstLock = result.Lock(ImageLockMode.WriteOnly);
      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x)
        dstLock[x, h - 1 - y] = srcLock[x, y];

      return result;
    }

    /// <summary>
    /// Mirrors pixels across an arbitrary line defined by two points.
    /// </summary>
    /// <param name="p1">First point defining the mirror axis.</param>
    /// <param name="p2">Second point defining the mirror axis.</param>
    /// <returns>A new bitmap with pixels reflected across the axis.</returns>
    public Bitmap MirrorAlongAxis(PointF p1, PointF p2) {
      ArgumentNullException.ThrowIfNull(@this);
      var w = @this.Width;
      var h = @this.Height;
      var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
      using var srcLock = @this.Lock(ImageLockMode.ReadOnly);
      using var dstLock = result.Lock(ImageLockMode.WriteOnly);

      var dx = p2.X - p1.X;
      var dy = p2.Y - p1.Y;
      var lenSq = dx * dx + dy * dy;
      if (lenSq < 0.0001f) {
        for (var y = 0; y < h; ++y)
        for (var x = 0; x < w; ++x)
          dstLock[x, y] = srcLock[x, y];
        return result;
      }

      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x) {
        var vx = x - p1.X;
        var vy = y - p1.Y;
        var t = (vx * dx + vy * dy) / lenSq;
        var projX = p1.X + t * dx;
        var projY = p1.Y + t * dy;
        var rx = 2f * projX - x;
        var ry = 2f * projY - y;
        dstLock[x, y] = _BilinearSample(srcLock, rx, ry, w, h);
      }

      return result;
    }

    /// <summary>
    /// Zooms into the image centered on a specific point.
    /// </summary>
    /// <param name="center">The center point for zoom in pixel coordinates.</param>
    /// <param name="factor">Zoom factor (values greater than 1 zoom in).</param>
    /// <returns>A new bitmap with the zoomed content.</returns>
    public Bitmap ZoomToPoint(PointF center, float factor) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(factor);
      factor = Math.Max(0.01f, factor);
      var w = @this.Width;
      var h = @this.Height;
      var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
      using var srcLock = @this.Lock(ImageLockMode.ReadOnly);
      using var dstLock = result.Lock(ImageLockMode.WriteOnly);

      var invFactor = 1f / factor;
      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x) {
        var sx = center.X + (x - w * 0.5f) * invFactor;
        var sy = center.Y + (y - h * 0.5f) * invFactor;
        dstLock[x, y] = _BilinearSample(srcLock, sx, sy, w, h);
      }

      return result;
    }

    /// <summary>
    /// Deskews the image by rotating by the given angle and cropping to the largest inscribed axis-aligned rectangle.
    /// </summary>
    /// <param name="angle">The rotation angle in degrees to straighten.</param>
    /// <returns>A new bitmap with the straightened and cropped content.</returns>
    /// <remarks>
    /// <para>The crop dimensions use the canonical "largest axis-aligned rectangle inscribed
    /// in a rotated rectangle" formula (two-case form). For a rectangle <c>w × h</c>
    /// rotated by angle <c>θ</c>, with <c>L = max(w,h)</c>, <c>S = min(w,h)</c>,
    /// <c>c = |cosθ|</c>, <c>s = |sinθ|</c>:</para>
    /// <list type="bullet">
    ///   <item><description><b>Half-constrained</b> (when <c>S ≤ 2·s·c·L</c>): the long
    ///   side limits the crop. Two corners of the inscribed rect lie on the long sides of
    ///   the rotated rectangle and the other two on the perpendicular mid-line:
    ///   <c>x = S/2; cropLong = x/s; cropShort = x/c</c>.</description></item>
    ///   <item><description><b>Fully constrained</b> (otherwise): all four corners of the
    ///   inscribed rect touch all four sides:
    ///   <c>cropW = (w·c - h·s)/cos(2θ); cropH = (h·c - w·s)/cos(2θ)</c>.</description></item>
    /// </list>
    /// <para>Reference: Andri Rost / "Calculate largest inscribed rectangle in a rotated
    /// rectangle" (also commonly attributed to Coffin/Larson). See e.g.
    /// https://stackoverflow.com/a/16778797 — derivation matches B. Larson, "Universal
    /// Image Rotation", and is verifiable by elementary trig on the rotated-rectangle
    /// edge equations.</para>
    /// </remarks>
    public Bitmap Straighten(float angle) {
      ArgumentNullException.ThrowIfNull(@this);
      if (Math.Abs(angle) < 0.001f)
        return (Bitmap)@this.Clone();

      using var rotated = @this.Rotated(angle);

      var w = @this.Width;
      var h = @this.Height;
      var rad = Math.Abs(angle * Math.PI / 180.0);
      // Reduce to first-quadrant equivalent: solutions for ±θ and 180°−θ are identical.
      var sinA = Math.Abs(Math.Sin(rad));
      var cosA = Math.Abs(Math.Cos(rad));

      var widthIsLonger = w >= h;
      double sideLong = widthIsLonger ? w : h;
      double sideShort = widthIsLonger ? h : w;

      double cropWd, cropHd;
      // Half-constrained case: long side dominates → inscribed rect is bounded by the
      // short side; one pair of corners sits on the long edges, the other pair on the
      // mid-line. Also covers the 45° degenerate (sin = cos) where cos(2θ) = 0.
      if (sideShort <= 2.0 * sinA * cosA * sideLong || Math.Abs(sinA - cosA) < 1e-10) {
        var x = 0.5 * sideShort;
        // The crop's long side is x/sinA and short side is x/cosA in the rotated frame.
        if (widthIsLonger) {
          cropWd = x / sinA;
          cropHd = x / cosA;
        } else {
          cropWd = x / cosA;
          cropHd = x / sinA;
        }
      } else {
        // Fully constrained: all four corners of the inscribed rect touch all four
        // sides of the rotated source rectangle.
        var cos2A = cosA * cosA - sinA * sinA; // = cos(2θ)
        cropWd = (w * cosA - h * sinA) / cos2A;
        cropHd = (h * cosA - w * sinA) / cos2A;
      }

      // Clamp to the rotated bitmap; never allow zero/negative on degenerate input.
      var cropW = Math.Max(1, Math.Min(rotated.Width, (int)cropWd));
      var cropH = Math.Max(1, Math.Min(rotated.Height, (int)cropHd));

      var cx = (rotated.Width - cropW) / 2;
      var cy = (rotated.Height - cropH) / 2;
      return rotated.Crop(new Rectangle(cx, cy, cropW, cropH));
    }

    /// <summary>
    /// Applies a shear/skew transformation to the bitmap.
    /// </summary>
    /// <param name="angleX">Horizontal shear angle in degrees.</param>
    /// <param name="angleY">Vertical shear angle in degrees.</param>
    /// <returns>A new bitmap with the skewed content.</returns>
    public Bitmap Skew(float angleX, float angleY) {
      ArgumentNullException.ThrowIfNull(@this);
      var tanX = (float)Math.Tan(angleX * Math.PI / 180.0);
      var tanY = (float)Math.Tan(angleY * Math.PI / 180.0);
      var w = @this.Width;
      var h = @this.Height;

      // Compute new bounds
      float[] xs = [0, w + h * tanX, w * tanY, w + h * tanX + w * tanY];
      float[] ys = [0, h, w * tanY, h + w * tanY];
      var minX = float.MaxValue;
      var minY = float.MaxValue;
      var maxX = float.MinValue;
      var maxY = float.MinValue;
      for (var i = 0; i < 4; ++i) {
        var fx = new[] { 0f, w, 0f, (float)w }[i] + new[] { 0f, 0f, (float)h, (float)h }[i] * tanX;
        var fy = new[] { 0f, 0f, (float)h, (float)h }[i] + new[] { 0f, (float)w, 0f, (float)w }[i] * tanY;
        if (fx < minX) minX = fx;
        if (fy < minY) minY = fy;
        if (fx > maxX) maxX = fx;
        if (fy > maxY) maxY = fy;
      }

      var nw = Math.Max(1, (int)Math.Ceiling(maxX - minX));
      var nh = Math.Max(1, (int)Math.Ceiling(maxY - minY));
      var result = new Bitmap(nw, nh, PixelFormat.Format32bppArgb);
      using var srcLock = @this.Lock(ImageLockMode.ReadOnly);
      using var dstLock = result.Lock(ImageLockMode.WriteOnly);

      // Inverse map: for each dest pixel, find source
      var det = 1f - tanX * tanY;
      if (Math.Abs(det) < 0.0001f)
        det = 0.0001f;

      for (var y = 0; y < nh; ++y)
      for (var x = 0; x < nw; ++x) {
        var dx = x + minX;
        var dy = y + minY;
        var sx = (dx - dy * tanX) / det;
        var sy = (dy - dx * tanY) / det;
        dstLock[x, y] = _BilinearSample(srcLock, sx, sy, w, h);
      }

      return result;
    }

    /// <summary>
    /// Reads the EXIF orientation tag and applies the correct rotation/flip to normalize the image.
    /// </summary>
    /// <returns>A new bitmap with the EXIF orientation applied, or a clone if no EXIF data is present.</returns>
    public Bitmap AutoRotate() {
      ArgumentNullException.ThrowIfNull(@this);
      const int ORIENTATION_TAG = 0x0112;
      int orientation;
      try {
        var propItem = @this.GetPropertyItem(ORIENTATION_TAG);
        if (propItem?.Value == null || propItem.Value.Length < 2)
          return (Bitmap)@this.Clone();
        orientation = BitConverter.ToUInt16(propItem.Value, 0);
      } catch {
        return (Bitmap)@this.Clone();
      }

      var clone = (Bitmap)@this.Clone();
      switch (orientation) {
        case 2:
          clone.RotateFlip(RotateFlipType.RotateNoneFlipX);
          break;
        case 3:
          clone.RotateFlip(RotateFlipType.Rotate180FlipNone);
          break;
        case 4:
          clone.RotateFlip(RotateFlipType.RotateNoneFlipY);
          break;
        case 5:
          clone.RotateFlip(RotateFlipType.Rotate90FlipX);
          break;
        case 6:
          clone.RotateFlip(RotateFlipType.Rotate90FlipNone);
          break;
        case 7:
          clone.RotateFlip(RotateFlipType.Rotate270FlipX);
          break;
        case 8:
          clone.RotateFlip(RotateFlipType.Rotate270FlipNone);
          break;
      }

      return clone;
    }
  }

  /// <summary>
  /// Bilinear sample from <paramref name="src"/> at floating-point coordinate (<paramref name="x"/>, <paramref name="y"/>).
  /// Decodes through sRGB EOTF, interpolates in linear-light, encodes via sRGB OETF —
  /// produces correct edge fringing on bright/saturated boundaries (gamma-naive interpolation
  /// of sRGB bytes produces dark fringes on red↔white edges).
  /// </summary>
  /// <remarks>
  /// Used by <see cref="MirrorAlongAxis"/>, <see cref="ZoomToPoint"/>, and <see cref="Skew"/>.
  /// Reference: ITU-R BT.709-6 §1.2 (sRGB EOTF/OETF).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Color _BilinearSample(Hawkynt.Drawing.Lockers.IBitmapLocker src, float x, float y, int w, int h) {
    var x0 = (int)Math.Floor(x);
    var y0 = (int)Math.Floor(y);
    var x1 = x0 + 1;
    var y1 = y0 + 1;

    x0 = Math.Max(0, Math.Min(w - 1, x0));
    y0 = Math.Max(0, Math.Min(h - 1, y0));
    x1 = Math.Max(0, Math.Min(w - 1, x1));
    y1 = Math.Max(0, Math.Min(h - 1, y1));

    var fx = x - (float)Math.Floor(x);
    var fy = y - (float)Math.Floor(y);

    var c00 = src[x0, y0];
    var c10 = src[x1, y0];
    var c01 = src[x0, y1];
    var c11 = src[x1, y1];

    // Bilinear weights (continuous; arithmetic, no transcendental → cross-TFM deterministic).
    var w00 = (1 - fx) * (1 - fy);
    var w10 = fx * (1 - fy);
    var w01 = (1 - fx) * fy;
    var w11 = fx * fy;

    // Alpha is unaffected by gamma — straight bilinear in byte space.
    var a = (int)(c00.A * w00 + c10.A * w10 + c01.A * w01 + c11.A * w11 + 0.5f);

    // RGB: gamma-decode (sRGB byte → linear 16.16) → bilinear in linear → gamma-encode.
    // GammaExpand returns Q16 linear (0..65536). Float weighting is fine — keeps the existing
    // float arithmetic, just changes the input/output domain to linear.
    var lr = Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c00.R) * w00
           + Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c10.R) * w10
           + Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c01.R) * w01
           + Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c11.R) * w11;
    var lg = Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c00.G) * w00
           + Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c10.G) * w10
           + Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c01.G) * w01
           + Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c11.G) * w11;
    var lb = Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c00.B) * w00
           + Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c10.B) * w10
           + Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c01.B) * w01
           + Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaExpand(c11.B) * w11;

    var r = Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaCompress((int)(lr + 0.5f));
    var g = Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaCompress((int)(lg + 0.5f));
    var b = Hawkynt.ColorProcessing.Internal.FixedPointMath.GammaCompress((int)(lb + 0.5f));

    return Color.FromArgb(Math.Max(0, Math.Min(255, a)), r, g, b);
  }
}
