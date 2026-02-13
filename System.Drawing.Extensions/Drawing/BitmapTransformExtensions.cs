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
    public Bitmap Straighten(float angle) {
      if (Math.Abs(angle) < 0.001f)
        return (Bitmap)@this.Clone();

      using var rotated = @this.Rotated(angle);

      var w = @this.Width;
      var h = @this.Height;
      var rad = Math.Abs(angle * Math.PI / 180.0);
      var sinA = Math.Abs(Math.Sin(rad));
      var cosA = Math.Abs(Math.Cos(rad));

      int cropW, cropH;
      if (w <= h) {
        cropW = (int)(w * cosA - h * sinA);
        cropH = (int)(h * cosA - w * sinA);
      } else {
        cropW = (int)(h * cosA - w * sinA);
        cropH = (int)(w * cosA - h * sinA);
      }

      // Fallback: if the angle is too large for a valid inscribed rect, just use original dimensions scaled down
      if (cropW <= 0 || cropH <= 0) {
        var scale = 1.0 / (sinA + cosA);
        cropW = Math.Max(1, (int)(w * scale));
        cropH = Math.Max(1, (int)(h * scale));
      }

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

    var a = (int)(c00.A * (1 - fx) * (1 - fy) + c10.A * fx * (1 - fy) + c01.A * (1 - fx) * fy + c11.A * fx * fy + 0.5f);
    var r = (int)(c00.R * (1 - fx) * (1 - fy) + c10.R * fx * (1 - fy) + c01.R * (1 - fx) * fy + c11.R * fx * fy + 0.5f);
    var g = (int)(c00.G * (1 - fx) * (1 - fy) + c10.G * fx * (1 - fy) + c01.G * (1 - fx) * fy + c11.G * fx * fy + 0.5f);
    var b = (int)(c00.B * (1 - fx) * (1 - fy) + c10.B * fx * (1 - fy) + c01.B * (1 - fx) * fy + c11.B * fx * fy + 0.5f);

    return Color.FromArgb(
      Math.Max(0, Math.Min(255, a)),
      Math.Max(0, Math.Min(255, r)),
      Math.Max(0, Math.Min(255, g)),
      Math.Max(0, Math.Min(255, b)));
  }
}
