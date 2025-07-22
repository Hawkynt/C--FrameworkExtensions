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

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing;

public static partial class BitmapExtensions {
  #region nested types

  private delegate IBitmapLocker LockerFactory(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format);

  private static readonly Dictionary<PixelFormat, LockerFactory> _LOCKER_TYPES = new() {
    [PixelFormat.Format32bppArgb] = (b, r, f, f2) => new ARGB32BitmapLocker(b, r, f, f2), 
    [PixelFormat.Format32bppRgb] = (b, r, f, f2) => new RGB32BitmapLocker(b, r, f, f2), 
    [PixelFormat.Format24bppRgb] = (b, r, f, f2) => new RGB24BitmapLocker(b, r, f, f2),
    [PixelFormat.Format16bppRgb565] = (b, r, f, f2) => new RGB565BitmapLocker(b, r, f, f2),
    [PixelFormat.Format16bppArgb1555] = (b, r, f, f2) => new ARGB1555BitmapLocker(b, r, f, f2),
    [PixelFormat.Format16bppGrayScale] = (b, r, f, f2) => new Gray16BitmapLocker(b, r, f, f2),
    [PixelFormat.Format16bppRgb555] = (b, r, f, f2) => new RGB555BitmapLocker(b, r, f, f2),
    [PixelFormat.Format8bppIndexed] = (b, r, f, f2) => new Indexed8BitmapLocker(b, r, f, f2),
    [PixelFormat.Format4bppIndexed] = (b, r, f, f2) => new IndexedBitmapLocker(b, r, f, f2),
    [PixelFormat.Format1bppIndexed] = (b, r, f, f2) => new IndexedBitmapLocker(b, r, f, f2),
  };

  #endregion

  #region Lock

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, ImageLockMode flags, PixelFormat format)
    => _LOCKER_TYPES.TryGetValue(format, out var factory)
      ? factory(@this, rect, flags, format)
      : new UnsupportedDrawingBitmapLocker(@this, rect, flags, format);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IBitmapLocker Lock(this Bitmap @this) => Lock(@this, new(Point.Empty, @this.Size), ImageLockMode.ReadWrite, @this.PixelFormat);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect) => Lock(@this, rect, ImageLockMode.ReadWrite, @this.PixelFormat);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IBitmapLocker Lock(this Bitmap @this, ImageLockMode flags) => Lock(@this, new(Point.Empty, @this.Size), flags, @this.PixelFormat);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IBitmapLocker Lock(this Bitmap @this, PixelFormat format) => Lock(@this, new(Point.Empty, @this.Size), ImageLockMode.ReadWrite, format);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, ImageLockMode flags) => Lock(@this, rect, flags, @this.PixelFormat);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, PixelFormat format) => Lock(@this, rect, ImageLockMode.ReadWrite, format);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IBitmapLocker Lock(this Bitmap @this, ImageLockMode flags, PixelFormat format) => Lock(@this, new(Point.Empty, @this.Size), flags, format);

  #endregion

  public static Bitmap ConvertPixelFormat(this Bitmap @this, PixelFormat format) {
    Against.ThisIsNull(@this);
    
    if (@this.PixelFormat == format)
      return (Bitmap)@this.Clone();

    var result = new Bitmap(@this.Width, @this.Height, format);

#if UNSAFE

    var sourceFormat = @this.PixelFormat;

    switch (sourceFormat) {
      case PixelFormat.Format24bppRgb when format == PixelFormat.Format32bppArgb: {
        var rect = new Rectangle(0, 0, @this.Width, @this.Height);
        using var sourceData = Lock(@this, rect, ImageLockMode.ReadOnly, sourceFormat);
        using var targetData = Lock(result, rect, ImageLockMode.WriteOnly, format);
        unsafe {
          var source = (byte*)sourceData.BitmapData.Scan0;
          Debug.Assert(source != null, nameof(source) + " != null");
          var target = (byte*)targetData.BitmapData.Scan0;
          Debug.Assert(target != null, nameof(target) + " != null");

          var sourceStride = sourceData.BitmapData.Stride;
          var targetStride = targetData.BitmapData.Stride;
          for (var y = @this.Height; y > 0; --y) {
            var sourceRow = source;
            var targetRow = target;
            for (var x = @this.Width; x > 0; --x) {
              var bg = *(ushort*)sourceRow;
              var r = sourceRow[2];

              *(ushort*)targetRow = bg;
              targetRow[2] = r;
              targetRow[3] = 0xff;

              sourceRow += 3;
              targetRow += 4;
            }

            source += sourceStride;
            target += targetStride;
          }
        }

        return result;
      }
      case PixelFormat.Format32bppArgb when format == PixelFormat.Format24bppRgb: {
        var rect = new Rectangle(0, 0, @this.Width, @this.Height);
        using var sourceData = Lock(@this, rect, ImageLockMode.ReadOnly, sourceFormat);
        using var targetData = Lock(result, rect, ImageLockMode.WriteOnly, format);
        unsafe {
          var source = (byte*)sourceData.BitmapData.Scan0;
          Debug.Assert(source != null, nameof(source) + " != null");
          var target = (byte*)targetData.BitmapData.Scan0;
          Debug.Assert(target != null, nameof(target) + " != null");

          var sourceStride = sourceData.BitmapData.Stride;
          var targetStride = targetData.BitmapData.Stride;
          for (var y = @this.Height; y > 0; --y) {
            var sourceRow = source;
            var targetRow = target;
            for (var x = @this.Width; x > 0; --x) {
              var bg = *(ushort*)sourceRow;
              var r = sourceRow[2];

              *(ushort*)targetRow = bg;
              targetRow[2] = r;

              sourceRow += 4;
              targetRow += 3;
            }

            source += sourceStride;
            target += targetStride;
          }
        }

        return result;
      }
    }

#endif

    using var g = Graphics.FromImage(result);
    g.CompositingMode = CompositingMode.SourceCopy;
    g.InterpolationMode = InterpolationMode.NearestNeighbor;
    g.DrawImage(@this, Point.Empty);

    return result;
  }

  public static Bitmap Crop(this Bitmap @this, Rectangle rect, PixelFormat format = PixelFormat.DontCare) {
    rect = Rectangle.FromLTRB(rect.Left, rect.Top, Math.Min(rect.Right, @this.Width), Math.Min(rect.Bottom, @this.Height));

    var result = new Bitmap(rect.Width, rect.Height, format == PixelFormat.DontCare ? @this.PixelFormat : format);
    using var g = Graphics.FromImage(result);
    g.CompositingMode = CompositingMode.SourceCopy;
    g.CompositingQuality = CompositingQuality.HighSpeed;
    g.InterpolationMode = InterpolationMode.NearestNeighbor;
    g.DrawImage(@this, new Rectangle(Point.Empty, new(rect.Width, rect.Height)), rect, GraphicsUnit.Pixel);

    return result;
  }

  /// <summary>
  /// Resizes the current <see cref="Bitmap"/> to the specified <paramref name="width"/> and <paramref name="height"/>
  /// using the given <paramref name="mode"/> for interpolation.
  /// </summary>
  /// <param name="this">The source bitmap to resize.</param>
  /// <param name="width">The target width in pixels. Must be a positive integer.</param>
  /// <param name="height">The target height in pixels. Must be a positive integer.</param>
  /// <param name="mode">
  /// The <see cref="InterpolationMode"/> used for resizing. Defaults to <see cref="InterpolationMode.Bicubic"/>.
  /// </param>
  /// <returns>A new <see cref="Bitmap"/> with the resized dimensions and content.</returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <c>null</c>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if <paramref name="width"/> or <paramref name="height"/> is negative or zero.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// Thrown if <paramref name="mode"/> is not a defined value of <see cref="InterpolationMode"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// using var original = new Bitmap("large_image.png");
  /// using var resized = original.Resize(800, 600);
  /// resized.Save("resized_image.png");
  /// </code>
  /// </example>
  public static Bitmap Resize(this Bitmap @this, int width, int height, InterpolationMode mode = InterpolationMode.Bicubic) {
    Against.ThisIsNull(@this);
    Against.NegativeValues(width);
    Against.NegativeValues(height);
    Against.UnknownEnumValues(mode);

    var result = new Bitmap(width, height, @this.PixelFormat);
    using var graphics = Graphics.FromImage(result);
    graphics.CompositingMode = CompositingMode.SourceCopy;
    graphics.InterpolationMode = mode;
    graphics.DrawImage(@this, new Rectangle(Point.Empty, result.Size), new(Point.Empty, @this.Size), GraphicsUnit.Pixel);

    return result;
  }

  /// <summary>
  /// Returns a rotated copy of the specified <see cref="Bitmap"/> by the given <paramref name="angle"/>.
  /// </summary>
  /// <param name="this">The source bitmap to rotate. This bitmap remains unmodified.</param>
  /// <param name="angle">The angle in degrees to rotate the image. Positive values rotate clockwise.</param>
  /// <param name="center">
  /// Optional pivot point around which the rotation is performed. If <c>null</c>, the center of the bitmap is used.
  /// </param>
  /// <returns>
  /// A new <see cref="Bitmap"/> instance with the rotated content. The output dimensions match the original bitmap size,
  /// so rotation may clip parts of the image.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if the <paramref name="this"/> bitmap is <c>null</c>.
  /// </exception>
  /// <example>
  /// <code>
  /// using var original = new Bitmap("input.png");
  /// using var rotated = original.Rotated(90);
  /// rotated.Save("rotated_output.png");
  /// </code>
  /// </example>
  public static Bitmap Rotated(this Bitmap @this, float angle, Point? center = null) {
    Against.ThisIsNull(@this);

    var srcSize = @this.Size;
    var srcCenter = center ?? new(srcSize.Width / 2, srcSize.Height / 2);
    var radians = angle * MathF.PI / 180;

    // corners relative to srcCenter
    PointF[] corners = [
      new(-srcCenter.X, -srcCenter.Y),
      new(srcSize.Width - srcCenter.X, -srcCenter.Y),
      new(-srcCenter.X, srcSize.Height - srcCenter.Y),
      new(srcSize.Width - srcCenter.X, srcSize.Height - srcCenter.Y)
    ];

    // rotate all corners
    var cos = MathF.Cos(radians);
    var sin = MathF.Sin(radians);
    var rotated = new PointF[4];
    for (var i = 0; i < 4; ++i) {
      var x = corners[i].X * cos - corners[i].Y * sin;
      var y = corners[i].X * sin + corners[i].Y * cos;
      rotated[i] = new(x, y);
    }

    // bounding box
    var minX = rotated.Min(p => p.X);
    var minY = rotated.Min(p => p.Y);
    var maxX = rotated.Max(p => p.X);
    var maxY = rotated.Max(p => p.Y);

    var newWidth = (int)MathF.Ceiling(maxX - minX);
    var newHeight = (int)MathF.Ceiling(maxY - minY);
    Bitmap result = new(newWidth, newHeight, @this.PixelFormat);

    // adjust rotation origin by offsetting so srcCenter maps to newCenter
    Point adjustedOrigin = new(
      srcCenter.X - (int)MathF.Floor(minX),
      srcCenter.Y - (int)MathF.Floor(minY)
    );

    @this.RotateTo(result, angle, adjustedOrigin);

    return result;
  }

  /// <summary>
  /// Rotates the given <see cref="Bitmap"/> in-place by the specified <paramref name="angle"/>.
  /// </summary>
  /// <param name="this">The bitmap to rotate. This bitmap will be modified directly.</param>
  /// <param name="angle">The angle in degrees to rotate the image. Positive values rotate clockwise.</param>
  /// <param name="center">
  /// Optional center point for rotation. If <c>null</c>, the center of the bitmap is used as the pivot point.
  /// </param>
  /// <remarks>
  /// The bitmap is cloned internally, and the rotation result is written back into the original bitmap.
  /// The dimensions of the bitmap remain unchanged; content may be clipped depending on the rotation angle and pivot point.
  /// </remarks>
  /// <exception cref="NullReferenceException">
  /// Thrown if the <paramref name="this"/> bitmap is <c>null</c>.
  /// </exception>
  /// <example>
  /// <code>
  /// using var image = new Bitmap("photo.jpg");
  /// image.RotateInplace(45); // Rotates image 45° clockwise
  /// image.Save("photo_rotated.jpg");
  /// </code>
  /// </example>
  public static void RotateInplace(this Bitmap @this, float angle, Point? center = null) {
    Against.ThisIsNull(@this);
    using var source = (Bitmap)@this.Clone();
    RotateTo(source, @this, angle, center);
  }

  /// <summary>
  /// Rotates the source <see cref="Bitmap"/> and draws the result into the specified <paramref name="target"/> bitmap.
  /// </summary>
  /// <param name="this">The source bitmap to rotate.</param>
  /// <param name="target">The target bitmap where the rotated image will be rendered.</param>
  /// <param name="angle">The rotation angle in degrees. Positive values rotate clockwise.</param>
  /// <param name="center">
  /// Optional center point of rotation relative to the source bitmap. If <c>null</c>, the rotation is centered around the midpoint of the source bitmap.
  /// </param>
  /// <remarks>
  /// The method does not resize the target bitmap. It assumes the <paramref name="target"/> is already appropriately sized to receive the rotated result.
  /// Pixels outside the bounds of the rotated source may result in transparent areas.
  /// </remarks>
  /// <exception cref="NullReferenceException">
  /// Thrown if the source bitmap is <c>null</c>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown if the target bitmap is <c>null</c>.
  /// </exception>
  /// <example>
  /// <code>
  /// using var source = new Bitmap("input.png");
  /// using var target = new Bitmap(source.Width, source.Height);
  /// source.RotateTo(target, 90); // Rotate 90° clockwise
  /// target.Save("rotated.png");
  /// </code>
  /// </example>
  public static void RotateTo(this Bitmap @this, Bitmap target, float angle, Point? center = null) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);

    center ??= @this.Size.Center();
    angle *= MathF.PI / 180;

    switch (@this.PixelFormat) {
      case PixelFormat.Format32bppArgb:
      case PixelFormat.Format32bppRgb:
      case PixelFormat.Format24bppRgb:
      case PixelFormat.Format16bppRgb555:
      case PixelFormat.Format16bppRgb565:
      case PixelFormat.Format16bppGrayScale:
        InvokeWithFourPointResampling(target, @this, angle, center.Value);
        break;
      case PixelFormat.Format16bppArgb1555:
        InvokeWithFourPointResamplingAndNoAlphaInterpolation(target, @this, angle, center.Value);
        break;
      case PixelFormat.Format1bppIndexed:
      case PixelFormat.Format4bppIndexed:
      case PixelFormat.Format8bppIndexed:
        InvokeWithoutInterpolation(target, @this, angle, center.Value);
        break;
      default: 
        throw new NotSupportedException($"Pixel format not supported for rotation: {@this.PixelFormat}");
    }

    return;

    void InvokeWithFourPointResampling(Bitmap target, Bitmap source, float radians, Point origin) {
      using var sourceLock = source.Lock();
      using var targetLock = target.Lock();

      var cos = MathF.Cos(-radians); // inverse rotation
      var sin = MathF.Sin(-radians);
      for (var y = 0; y < target.Height; ++y)
      for (var x = 0; x < target.Width; ++x) {
        float dx = x - origin.X;
        float dy = y - origin.Y;
        var sx = dx * cos - dy * sin + origin.X;
        var sy = dx * sin + dy * cos + origin.Y;

        var x0 = (int)MathF.Floor(sx);
        var y0 = (int)MathF.Floor(sy);
        var x1 = x0 + 1;
        var y1 = y0 + 1;

        if (x0 < 0 || y0 < 0 || x1 >= target.Width || y1 >= target.Height) {
          targetLock[x, y] = Color.Transparent;
          continue;
        }

        var fx = sx - x0;
        var fy = sy - y0;
        var fx1 = 1f - fx;
        var fy1 = 1f - fy;

        var c00 = sourceLock[x0, y0];
        var c10 = sourceLock[x1, y0];
        var c01 = sourceLock[x0, y1];
        var c11 = sourceLock[x1, y1];

        var b = (byte)(
          c00.B * fx1 * fy1 +
          c10.B * fx * fy1 +
          c01.B * fx1 * fy +
          c11.B * fx * fy
        );

        var g = (byte)(
          c00.G * fx1 * fy1 +
          c10.G * fx * fy1 +
          c01.G * fx1 * fy +
          c11.G * fx * fy
        );

        var r = (byte)(
          c00.R * fx1 * fy1 +
          c10.R * fx * fy1 +
          c01.R * fx1 * fy +
          c11.R * fx * fy
        );

        var a = (byte)(
          c00.A * fx1 * fy1 +
          c10.A * fx * fy1 +
          c01.A * fx1 * fy +
          c11.A * fx * fy
        );

        targetLock[x, y] = Color.FromArgb(a, r, g, b);
      }
    }

    void InvokeWithFourPointResamplingAndNoAlphaInterpolation(Bitmap target, Bitmap source, float radians, Point origin) {
      using var sourceLock = source.Lock();
      using var targetLock = target.Lock();

      var cos = MathF.Cos(-radians); // inverse rotation
      var sin = MathF.Sin(-radians);
      for (var y = 0; y < target.Height; ++y)
      for (var x = 0; x < target.Width; ++x) {
        float dx = x - origin.X;
        float dy = y - origin.Y;
        var sx = dx * cos - dy * sin + origin.X;
        var sy = dx * sin + dy * cos + origin.Y;

        var x0 = (int)MathF.Floor(sx);
        var y0 = (int)MathF.Floor(sy);
        var x1 = x0 + 1;
        var y1 = y0 + 1;

        if (x0 < 0 || y0 < 0 || x1 >= target.Width || y1 >= target.Height) {
          targetLock[x, y] = Color.Transparent;
          continue;
        }

        var fx = sx - x0;
        var fy = sy - y0;
        var fx1 = 1f - fx;
        var fy1 = 1f - fy;

        var c00 = sourceLock[x0, y0];
        var c10 = sourceLock[x1, y0];
        var c01 = sourceLock[x0, y1];
        var c11 = sourceLock[x1, y1];

        var b = (byte)(
          c00.B * fx1 * fy1 +
          c10.B * fx * fy1 +
          c01.B * fx1 * fy +
          c11.B * fx * fy
        );

        var g = (byte)(
          c00.G * fx1 * fy1 +
          c10.G * fx * fy1 +
          c01.G * fx1 * fy +
          c11.G * fx * fy
        );

        var r = (byte)(
          c00.R * fx1 * fy1 +
          c10.R * fx * fy1 +
          c01.R * fx1 * fy +
          c11.R * fx * fy
        );
          
        targetLock[x, y] = Color.FromArgb(c00.A, r, g, b);
      }
    }

    void InvokeWithoutInterpolation(Bitmap target, Bitmap source, float radians, Point origin) {
      using var sourceLock = source.Lock();
      using var targetLock = target.Lock();

      var cos = MathF.Cos(-radians);
      var sin = MathF.Sin(-radians);
      for (var y = 0; y < target.Height; ++y)
      for (var x = 0; x < target.Width; ++x) {
        float dx = x - origin.X;
        float dy = y - origin.Y;
        var srcX = dx * cos - dy * sin + origin.X;
        var srcY = dx * sin + dy * cos + origin.Y;
        if (srcX >= 0 && srcY >= 0 && srcX < target.Width && srcY < target.Height)
          targetLock[x, y] = sourceLock[(int)srcX, (int)srcY];
        else
          targetLock[x, y] = Color.Transparent;
      }
    }
    
  }

}
