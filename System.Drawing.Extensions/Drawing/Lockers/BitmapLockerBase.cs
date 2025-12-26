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
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing.Lockers;

/// <summary>
/// Abstract base class for bitmap lockers providing common drawing implementations.
/// </summary>
internal abstract class BitmapLockerBase : IBitmapLocker {
  /// <summary>The bitmap being locked.</summary>
  protected readonly Bitmap _bitmap;

  /// <summary>The locked bitmap data.</summary>
  protected readonly BitmapData _data;

  /// <inheritdoc/>
  public int Width { get; }

  /// <inheritdoc/>
  public int Height { get; }

  /// <summary>Gets the number of bytes per pixel.</summary>
  public int BytesPerPixel { get; }

  /// <summary>Gets the stride in pixels (width including padding).</summary>
  public int Stride { get; }

  /// <inheritdoc/>
  public BitmapData BitmapData => this._data;

  /// <inheritdoc/>
  public abstract Color this[int x, int y] { get; set; }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public virtual Bgra8888 GetPixelBgra8888(int x, int y) => new(this[x, y]);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public virtual void SetPixelBgra8888(int x, int y, Bgra8888 color) => this[x, y] = color.ToColor();

  /// <inheritdoc/>
  public Color this[Point p] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this[p.X, p.Y];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this[p.X, p.Y] = value;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="BitmapLockerBase"/> class.
  /// </summary>
  /// <param name="bitmap">The bitmap to lock.</param>
  /// <param name="lockMode">The lock mode.</param>
  /// <param name="validFormats">The valid pixel formats. If empty, any format is accepted.</param>
  protected BitmapLockerBase(Bitmap bitmap, ImageLockMode lockMode, params PixelFormat[] validFormats)
    : this(bitmap, new(0, 0, bitmap.Width, bitmap.Height), lockMode, 0, validFormats.Length > 0 ? validFormats[0] : bitmap.PixelFormat, validFormats) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="BitmapLockerBase"/> class.
  /// </summary>
  /// <param name="bitmap">The bitmap to lock.</param>
  /// <param name="lockMode">The lock mode.</param>
  /// <param name="bytesPerPixel">The number of bytes per pixel.</param>
  /// <param name="validFormats">The valid pixel formats. If empty, any format is accepted.</param>
  protected BitmapLockerBase(Bitmap bitmap, ImageLockMode lockMode, int bytesPerPixel, params PixelFormat[] validFormats)
    : this(bitmap, new(0, 0, bitmap.Width, bitmap.Height), lockMode, bytesPerPixel, validFormats.Length > 0 ? validFormats[0] : bitmap.PixelFormat, validFormats) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="BitmapLockerBase"/> class with region support.
  /// </summary>
  /// <param name="bitmap">The bitmap to lock.</param>
  /// <param name="rect">The region of the bitmap to lock.</param>
  /// <param name="lockMode">The lock mode.</param>
  /// <param name="bytesPerPixel">The number of bytes per pixel.</param>
  /// <param name="validFormats">The valid pixel formats. If empty, any format is accepted.</param>
  protected BitmapLockerBase(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, int bytesPerPixel, params PixelFormat[] validFormats)
    : this(bitmap, rect, lockMode, bytesPerPixel, validFormats.Length > 0 ? validFormats[0] : bitmap.PixelFormat, validFormats) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="BitmapLockerBase"/> class with region and target format support.
  /// </summary>
  /// <param name="bitmap">The bitmap to lock.</param>
  /// <param name="rect">The region of the bitmap to lock.</param>
  /// <param name="lockMode">The lock mode.</param>
  /// <param name="bytesPerPixel">The number of bytes per pixel.</param>
  /// <param name="targetFormat">The pixel format to lock the bitmap as. GDI+ will convert if different from native format.</param>
  /// <param name="validFormats">The valid pixel formats for this locker. If empty, any format is accepted.</param>
  protected BitmapLockerBase(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, int bytesPerPixel, PixelFormat targetFormat, params PixelFormat[] validFormats) {
    if (validFormats.Length > 0 && Array.IndexOf(validFormats, targetFormat) < 0)
      throw new ArgumentException($"Locker supports: {string.Join(", ", validFormats)}. Got: {targetFormat}", nameof(targetFormat));

    this._bitmap = bitmap;
    this.Width = rect.Width;
    this.Height = rect.Height;
    this._data = bitmap.LockBits(rect, lockMode, targetFormat);
    this.BytesPerPixel = bytesPerPixel > 0 ? bytesPerPixel : Image.GetPixelFormatSize(targetFormat) / 8;
    this.Stride = this._data.Stride / this.BytesPerPixel;
  }

  /// <inheritdoc/>
  public virtual void Dispose() => this._bitmap.UnlockBits(this._data);

  #region Drawing Methods

  /// <inheritdoc/>
  public virtual void Clear(Color color) {
    for (var y = 0; y < this.Height; ++y)
    for (var x = 0; x < this.Width; ++x)
      this[x, y] = color;
  }

  /// <inheritdoc/>
  public virtual void DrawHorizontalLine(int x, int y, int length, Color color) {
    if (y < 0 || y >= this.Height || length <= 0)
      return;

    var startX = Math.Max(0, x);
    var endX = Math.Min(this.Width, x + length);
    var actualLength = endX - startX;
    if (actualLength <= 0)
      return;

    this.DrawHorizontalLineUnchecked(startX, y, actualLength, color);
  }

  /// <inheritdoc/>
  public virtual void DrawHorizontalLineUnchecked(int x, int y, int length, Color color) {
    for (var px = x; px < x + length; ++px)
      this[px, y] = color;
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawHorizontalLine(Point start, int length, Color color)
    => this.DrawHorizontalLine(start.X, start.Y, length, color);

  /// <inheritdoc/>
  public virtual void DrawVerticalLine(int x, int y, int length, Color color) {
    if (x < 0 || x >= this.Width || length <= 0)
      return;

    var startY = Math.Max(0, y);
    var endY = Math.Min(this.Height, y + length);
    var actualLength = endY - startY;
    if (actualLength <= 0)
      return;

    this.DrawVerticalLineUnchecked(x, startY, actualLength, color);
  }

  /// <inheritdoc/>
  public virtual void DrawVerticalLineUnchecked(int x, int y, int length, Color color) {
    for (var py = y; py < y + length; ++py)
      this[x, py] = color;
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawVerticalLine(Point start, int length, Color color)
    => this.DrawVerticalLine(start.X, start.Y, length, color);

  /// <inheritdoc/>
  public virtual void DrawLine(int x1, int y1, int x2, int y2, Color color) {
    // Bresenham's line algorithm
    var dx = Math.Abs(x2 - x1);
    var dy = Math.Abs(y2 - y1);
    var sx = x1 < x2 ? 1 : -1;
    var sy = y1 < y2 ? 1 : -1;
    var err = dx - dy;

    while (true) {
      if (x1 >= 0 && x1 < this.Width && y1 >= 0 && y1 < this.Height)
        this[x1, y1] = color;

      if (x1 == x2 && y1 == y2)
        break;

      var e2 = 2 * err;
      if (e2 > -dy) {
        err -= dy;
        x1 += sx;
      }
      if (e2 < dx) {
        err += dx;
        y1 += sy;
      }
    }
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawLine(Point start, Point end, Color color)
    => this.DrawLine(start.X, start.Y, end.X, end.Y, color);

  /// <inheritdoc/>
  public virtual void DrawRectangle(int x, int y, int width, int height, Color color) {
    this.DrawHorizontalLine(x, y, width, color);
    this.DrawHorizontalLine(x, y + height - 1, width, color);
    this.DrawVerticalLine(x, y + 1, height - 2, color);
    this.DrawVerticalLine(x + width - 1, y + 1, height - 2, color);
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangle(Rectangle rect, Color color)
    => this.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height, color);

  /// <inheritdoc/>
  public virtual void FillRectangle(int x, int y, int width, int height, Color color) {
    var startX = Math.Max(0, x);
    var startY = Math.Max(0, y);
    var endX = Math.Min(this.Width, x + width);
    var endY = Math.Min(this.Height, y + height);
    var actualWidth = endX - startX;
    var actualHeight = endY - startY;
    if (actualWidth <= 0 || actualHeight <= 0)
      return;

    this.FillRectangleUnchecked(startX, startY, actualWidth, actualHeight, color);
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangle(Rectangle rect, Color color)
    => this.FillRectangle(rect.X, rect.Y, rect.Width, rect.Height, color);

  /// <inheritdoc/>
  public virtual void FillRectangleChecked(int x, int y, int width, int height, Color color) {
    if (x < 0 || y < 0 || x + width > this.Width || y + height > this.Height)
      throw new ArgumentOutOfRangeException(
        null,
        $"Rectangle ({x}, {y}, {width}, {height}) is out of bounds for bitmap ({this.Width}, {this.Height})."
      );

    this.FillRectangleUnchecked(x, y, width, height, color);
  }

  /// <inheritdoc/>
  public virtual void DrawCircle(int cx, int cy, int radius, Color color) {
    // Midpoint circle algorithm
    var x = radius;
    var y = 0;
    var err = 0;

    while (x >= y) {
      this._SetPixelIfInBounds(cx + x, cy + y, color);
      this._SetPixelIfInBounds(cx + y, cy + x, color);
      this._SetPixelIfInBounds(cx - y, cy + x, color);
      this._SetPixelIfInBounds(cx - x, cy + y, color);
      this._SetPixelIfInBounds(cx - x, cy - y, color);
      this._SetPixelIfInBounds(cx - y, cy - x, color);
      this._SetPixelIfInBounds(cx + y, cy - x, color);
      this._SetPixelIfInBounds(cx + x, cy - y, color);

      ++y;
      if (err <= 0)
        err += 2 * y + 1;
      if (err > 0) {
        --x;
        err -= 2 * x + 1;
      }
    }
  }

  /// <inheritdoc/>
  public virtual void FillCircle(int cx, int cy, int radius, Color color) {
    var x = radius;
    var y = 0;
    var err = 0;

    while (x >= y) {
      this.DrawHorizontalLine(cx - x, cy + y, 2 * x + 1, color);
      this.DrawHorizontalLine(cx - x, cy - y, 2 * x + 1, color);
      this.DrawHorizontalLine(cx - y, cy + x, 2 * y + 1, color);
      this.DrawHorizontalLine(cx - y, cy - x, 2 * y + 1, color);

      ++y;
      if (err <= 0)
        err += 2 * y + 1;
      if (err > 0) {
        --x;
        err -= 2 * x + 1;
      }
    }
  }

  /// <inheritdoc/>
  public virtual void DrawEllipse(int cx, int cy, int rx, int ry, Color color) {
    // Midpoint ellipse algorithm
    var rx2 = rx * rx;
    var ry2 = ry * ry;
    var twoRx2 = 2 * rx2;
    var twoRy2 = 2 * ry2;
    var x = 0;
    var y = ry;
    var px = 0;
    var py = twoRx2 * y;

    // Region 1
    var p = (int)(ry2 - rx2 * ry + 0.25 * rx2);
    while (px < py) {
      this._SetPixelIfInBounds(cx + x, cy + y, color);
      this._SetPixelIfInBounds(cx - x, cy + y, color);
      this._SetPixelIfInBounds(cx + x, cy - y, color);
      this._SetPixelIfInBounds(cx - x, cy - y, color);

      ++x;
      px += twoRy2;
      if (p < 0)
        p += ry2 + px;
      else {
        --y;
        py -= twoRx2;
        p += ry2 + px - py;
      }
    }

    // Region 2
    p = (int)(ry2 * (x + 0.5) * (x + 0.5) + rx2 * (y - 1) * (y - 1) - rx2 * ry2);
    while (y >= 0) {
      this._SetPixelIfInBounds(cx + x, cy + y, color);
      this._SetPixelIfInBounds(cx - x, cy + y, color);
      this._SetPixelIfInBounds(cx + x, cy - y, color);
      this._SetPixelIfInBounds(cx - x, cy - y, color);

      --y;
      py -= twoRx2;
      if (p > 0)
        p += rx2 - py;
      else {
        ++x;
        px += twoRy2;
        p += rx2 - py + px;
      }
    }
  }

  /// <inheritdoc/>
  public virtual void FillEllipse(int cx, int cy, int rx, int ry, Color color) {
    var rx2 = rx * rx;
    var ry2 = ry * ry;
    var twoRx2 = 2 * rx2;
    var twoRy2 = 2 * ry2;
    var x = 0;
    var y = ry;
    var px = 0;
    var py = twoRx2 * y;

    // Region 1
    var p = (int)(ry2 - rx2 * ry + 0.25 * rx2);
    while (px < py) {
      this.DrawHorizontalLine(cx - x, cy + y, 2 * x + 1, color);
      this.DrawHorizontalLine(cx - x, cy - y, 2 * x + 1, color);

      ++x;
      px += twoRy2;
      if (p < 0)
        p += ry2 + px;
      else {
        --y;
        py -= twoRx2;
        p += ry2 + px - py;
      }
    }

    // Region 2
    p = (int)(ry2 * (x + 0.5) * (x + 0.5) + rx2 * (y - 1) * (y - 1) - rx2 * ry2);
    while (y >= 0) {
      this.DrawHorizontalLine(cx - x, cy + y, 2 * x + 1, color);
      this.DrawHorizontalLine(cx - x, cy - y, 2 * x + 1, color);

      --y;
      py -= twoRx2;
      if (p > 0)
        p += rx2 - py;
      else {
        ++x;
        px += twoRy2;
        p += rx2 - py + px;
      }
    }
  }

  /// <inheritdoc/>
  public virtual void CopyFrom(IBitmapLocker source) {
    var width = Math.Min(this.Width, source.Width);
    var height = Math.Min(this.Height, source.Height);
    if (width <= 0 || height <= 0)
      return;

    this.CopyFromUnchecked(source, 0, 0, width, height, 0, 0);
  }

  /// <inheritdoc/>
  public virtual void CopyFrom(IBitmapLocker source, int srcX, int srcY, int width, int height, int destX, int destY) {
    var maxWidth = Math.Min(width, Math.Min(source.Width - srcX, this.Width - destX));
    var maxHeight = Math.Min(height, Math.Min(source.Height - srcY, this.Height - destY));
    if (maxWidth <= 0 || maxHeight <= 0)
      return;

    this.CopyFromUnchecked(source, srcX, srcY, maxWidth, maxHeight, destX, destY);
  }

  /// <inheritdoc/>
  public virtual void BlendWith(IBitmapLocker source) {
    var width = Math.Min(this.Width, source.Width);
    var height = Math.Min(this.Height, source.Height);

    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var srcColor = source[x, y];
      var srcAlpha = srcColor.A / 255f;
      if (srcAlpha <= 0)
        continue;

      if (srcAlpha >= 1) {
        this[x, y] = srcColor;
        continue;
      }

      var dstColor = this[x, y];
      var dstAlpha = dstColor.A / 255f;
      var outAlpha = srcAlpha + dstAlpha * (1 - srcAlpha);

      if (outAlpha <= 0) {
        this[x, y] = Color.Transparent;
        continue;
      }

      var r = (byte)((srcColor.R * srcAlpha + dstColor.R * dstAlpha * (1 - srcAlpha)) / outAlpha);
      var g = (byte)((srcColor.G * srcAlpha + dstColor.G * dstAlpha * (1 - srcAlpha)) / outAlpha);
      var b = (byte)((srcColor.B * srcAlpha + dstColor.B * dstAlpha * (1 - srcAlpha)) / outAlpha);
      var a = (byte)(outAlpha * 255);

      this[x, y] = Color.FromArgb(a, r, g, b);
    }
  }

  /// <inheritdoc/>
  public virtual bool IsFlatColor {
    get {
      if (this.Width == 0 || this.Height == 0)
        return true;

      var firstColor = this[0, 0];
      for (var y = 0; y < this.Height; ++y)
      for (var x = 0; x < this.Width; ++x)
        if (this[x, y] != firstColor)
          return false;

      return true;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _SetPixelIfInBounds(int x, int y, Color color) {
    if (x >= 0 && x < this.Width && y >= 0 && y < this.Height)
      this[x, y] = color;
  }

  #region Rectangle Overloads

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangle(Point location, Size size, Color color)
    => this.DrawRectangle(location.X, location.Y, size.Width, size.Height, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangle(Point location, Size size, Color color)
    => this.FillRectangle(location.X, location.Y, size.Width, size.Height, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangleChecked(Point location, Size size, Color color)
    => this.FillRectangleChecked(location.X, location.Y, size.Width, size.Height, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangleChecked(Rectangle rect, Color color)
    => this.FillRectangleChecked(rect.X, rect.Y, rect.Width, rect.Height, color);

  /// <inheritdoc/>
  public virtual void FillRectangleUnchecked(int x, int y, int width, int height, Color color) {
    for (var py = y; py < y + height; ++py)
    for (var px = x; px < x + width; ++px)
      this[px, py] = color;
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangleUnchecked(Point location, Size size, Color color)
    => this.FillRectangleUnchecked(location.X, location.Y, size.Width, size.Height, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangleUnchecked(Rectangle rect, Color color)
    => this.FillRectangleUnchecked(rect.X, rect.Y, rect.Width, rect.Height, color);

  /// <inheritdoc/>
  public virtual void DrawRectangleChecked(int x, int y, int width, int height, Color color) {
    if (x < 0 || y < 0 || x + width > this.Width || y + height > this.Height)
      throw new ArgumentOutOfRangeException(
        null,
        $"Rectangle ({x}, {y}, {width}, {height}) is out of bounds for bitmap ({this.Width}, {this.Height})."
      );

    this.DrawRectangle(x, y, width, height, color);
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangleChecked(Point location, Size size, Color color)
    => this.DrawRectangleChecked(location.X, location.Y, size.Width, size.Height, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangleChecked(Rectangle rect, Color color)
    => this.DrawRectangleChecked(rect.X, rect.Y, rect.Width, rect.Height, color);

  /// <inheritdoc/>
  public virtual void DrawRectangleUnchecked(int x, int y, int width, int height, Color color) {
    // Draw top and bottom edges
    for (var px = x; px < x + width; ++px) {
      this[px, y] = color;
      this[px, y + height - 1] = color;
    }
    // Draw left and right edges (excluding corners)
    for (var py = y + 1; py < y + height - 1; ++py) {
      this[x, py] = color;
      this[x + width - 1, py] = color;
    }
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangleUnchecked(Point location, Size size, Color color)
    => this.DrawRectangleUnchecked(location.X, location.Y, size.Width, size.Height, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangleUnchecked(Rectangle rect, Color color)
    => this.DrawRectangleUnchecked(rect.X, rect.Y, rect.Width, rect.Height, color);

  #endregion

  #region Circle/Ellipse Overloads

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawCircle(Point center, int radius, Color color)
    => this.DrawCircle(center.X, center.Y, radius, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillCircle(Point center, int radius, Color color)
    => this.FillCircle(center.X, center.Y, radius, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawEllipse(Point center, Size radii, Color color)
    => this.DrawEllipse(center.X, center.Y, radii.Width, radii.Height, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawEllipse(Rectangle bounds, Color color)
    => this.DrawEllipse(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2, bounds.Width / 2, bounds.Height / 2, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillEllipse(Point center, Size radii, Color color)
    => this.FillEllipse(center.X, center.Y, radii.Width, radii.Height, color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillEllipse(Rectangle bounds, Color color)
    => this.FillEllipse(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2, bounds.Width / 2, bounds.Height / 2, color);

  #endregion

  #region DrawCross Methods

  /// <inheritdoc/>
  public virtual void DrawCross(Point a1, Point b1, Point a2, Point b2, int thickness, Color color) {
    // Draw two lines to form a cross
    if (thickness <= 1) {
      this.DrawLine(a1, b1, color);
      this.DrawLine(a2, b2, color);
    } else {
      // Draw thick lines by drawing multiple parallel lines
      var halfThickness = thickness / 2;
      for (var i = -halfThickness; i <= halfThickness; ++i) {
        this.DrawLine(a1.X + i, a1.Y, b1.X + i, b1.Y, color);
        this.DrawLine(a2.X, a2.Y + i, b2.X, b2.Y + i, color);
      }
    }
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawCross(Rectangle rect, int thickness, Color color)
    => this.DrawCross(
      new(rect.Left, rect.Top),
      new(rect.Right - 1, rect.Bottom - 1),
      new(rect.Right - 1, rect.Top),
      new(rect.Left, rect.Bottom - 1),
      thickness,
      color
    );

  #endregion

  #region CopyFrom Overloads

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IBitmapLocker source, Point destLocation)
    => this.CopyFrom(source, 0, 0, source.Width, source.Height, destLocation.X, destLocation.Y);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IBitmapLocker source, Point srcLocation, Size size)
    => this.CopyFrom(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, 0, 0);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IBitmapLocker source, Point srcLocation, Size size, Point destLocation)
    => this.CopyFrom(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, destLocation.X, destLocation.Y);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IBitmapLocker source, Rectangle srcRect)
    => this.CopyFrom(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 0, 0);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IBitmapLocker source, Rectangle srcRect, Point destLocation)
    => this.CopyFrom(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destLocation.X, destLocation.Y);

  #endregion

  #region CopyFromChecked Methods

  /// <inheritdoc/>
  public virtual void CopyFromChecked(IBitmapLocker source, int srcX, int srcY, int width, int height, int destX, int destY) {
    if (srcX < 0 || srcY < 0 || srcX + width > source.Width || srcY + height > source.Height)
      throw new ArgumentOutOfRangeException(
        nameof(source),
        $"Source rectangle ({srcX}, {srcY}, {width}, {height}) is out of bounds for source bitmap ({source.Width}, {source.Height})."
      );

    if (destX < 0 || destY < 0 || destX + width > this.Width || destY + height > this.Height)
      throw new ArgumentOutOfRangeException(
        null,
        $"Destination rectangle ({destX}, {destY}, {width}, {height}) is out of bounds for bitmap ({this.Width}, {this.Height})."
      );

    this.CopyFromUnchecked(source, srcX, srcY, width, height, destX, destY);
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source)
    => this.CopyFromChecked(source, 0, 0, source.Width, source.Height, 0, 0);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source, Point destLocation)
    => this.CopyFromChecked(source, 0, 0, source.Width, source.Height, destLocation.X, destLocation.Y);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source, Point srcLocation, Size size)
    => this.CopyFromChecked(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, 0, 0);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source, Point srcLocation, Size size, Point destLocation)
    => this.CopyFromChecked(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, destLocation.X, destLocation.Y);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source, Rectangle srcRect)
    => this.CopyFromChecked(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 0, 0);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source, Rectangle srcRect, Point destLocation)
    => this.CopyFromChecked(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destLocation.X, destLocation.Y);

  #endregion

  #region CopyFromUnchecked Methods

  /// <inheritdoc/>
  public virtual void CopyFromUnchecked(IBitmapLocker source, int srcX, int srcY, int width, int height, int destX, int destY) {
    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x)
      this[destX + x, destY + y] = source[srcX + x, srcY + y];
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source)
    => this.CopyFromUnchecked(source, 0, 0, source.Width, source.Height, 0, 0);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source, Point destLocation)
    => this.CopyFromUnchecked(source, 0, 0, source.Width, source.Height, destLocation.X, destLocation.Y);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source, Point srcLocation, Size size)
    => this.CopyFromUnchecked(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, 0, 0);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source, Point srcLocation, Size size, Point destLocation)
    => this.CopyFromUnchecked(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, destLocation.X, destLocation.Y);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source, Rectangle srcRect)
    => this.CopyFromUnchecked(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 0, 0);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source, Rectangle srcRect, Point destLocation)
    => this.CopyFromUnchecked(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destLocation.X, destLocation.Y);

  #endregion

  #region CopyFromGrid Methods

  /// <inheritdoc/>
  public virtual void CopyFromGrid(
    IBitmapLocker other,
    int column,
    int row,
    int width,
    int height,
    int dx = 0,
    int dy = 0,
    int offsetX = 0,
    int offsetY = 0,
    int targetX = 0,
    int targetY = 0
  ) {
    var sourceX = column * (width + dx) + offsetX;
    var sourceY = row * (height + dy) + offsetY;
    this.CopyFromChecked(other, sourceX, sourceY, width, height, targetX, targetY);
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize)
    => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance)
    => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset)
    => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height, offset.Width, offset.Height);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset, Point target)
    => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height, offset.Width, offset.Height, target.X, target.Y);

  #endregion

  #endregion
}
