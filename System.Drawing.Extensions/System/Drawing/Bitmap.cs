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

  public static Bitmap Resize(this Bitmap @this, int width, int height, InterpolationMode mode = InterpolationMode.Bicubic) {
    var result = new Bitmap(width, height, @this.PixelFormat);
    using var graphics = Graphics.FromImage(result);
    graphics.CompositingMode = CompositingMode.SourceCopy;
    graphics.InterpolationMode = mode;
    graphics.DrawImage(@this, new Rectangle(Point.Empty, result.Size), new(Point.Empty, @this.Size), GraphicsUnit.Pixel);

    return result;
  }


}
