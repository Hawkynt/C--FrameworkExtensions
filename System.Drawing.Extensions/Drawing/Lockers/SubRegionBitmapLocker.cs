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
/// Provides a sub-region view over an existing bitmap locker.
/// Coordinate [0,0] maps to the specified offset in the underlying locker.
/// </summary>
internal sealed class SubRegionBitmapLocker(IBitmapLocker inner, Rectangle region) : IBitmapLocker {
  private readonly int _offsetX = region.X;
  private readonly int _offsetY = region.Y;

  public int Width { get; } = region.Width;

  public int Height { get; } = region.Height;

  public BitmapData BitmapData => inner.BitmapData;

  /// <summary>Gets or sets the color at the specified coordinates (relative to sub-region).</summary>
  public Color this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => inner[x + this._offsetX, y + this._offsetY];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => inner[x + this._offsetX, y + this._offsetY] = value;
  }

  /// <summary>Gets or sets the color at the specified point (relative to sub-region).</summary>
  public Color this[Point p] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this[p.X, p.Y];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this[p.X, p.Y] = value;
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Bgra8888 GetPixelBgra8888(int x, int y) => inner.GetPixelBgra8888(x + this._offsetX, y + this._offsetY);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetPixelBgra8888(int x, int y, Bgra8888 color) => inner.SetPixelBgra8888(x + this._offsetX, y + this._offsetY, color);

  public void Dispose() => inner.Dispose();

  #region Drawing Methods (TODO: Delegate to inner with offset so we don't have duplicate code here and utilize fastpaths in inner if available)

  public void Clear(Color color) {
    for (var y = 0; y < this.Height; ++y)
    for (var x = 0; x < this.Width; ++x)
      this[x, y] = color;
  }

  public void DrawHorizontalLine(int x, int y, int length, Color color) {
    if (y < 0 || y >= this.Height)
      return;

    var startX = Math.Max(0, x);
    var endX = Math.Min(this.Width, x + length);
    for (var px = startX; px < endX; ++px)
      this[px, y] = color;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawHorizontalLine(Point start, int length, Color color)
    => this.DrawHorizontalLine(start.X, start.Y, length, color);

  public void DrawHorizontalLineUnchecked(int x, int y, int length, Color color) {
    for (var px = x; px < x + length; ++px)
      this[px, y] = color;
  }

  public void DrawVerticalLine(int x, int y, int length, Color color) {
    if (x < 0 || x >= this.Width)
      return;

    var startY = Math.Max(0, y);
    var endY = Math.Min(this.Height, y + length);
    for (var py = startY; py < endY; ++py)
      this[x, py] = color;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawVerticalLine(Point start, int length, Color color)
    => this.DrawVerticalLine(start.X, start.Y, length, color);

  public void DrawVerticalLineUnchecked(int x, int y, int length, Color color) {
    for (var py = y; py < y + length; ++py)
      this[x, py] = color;
  }

  public void DrawLine(int x1, int y1, int x2, int y2, Color color) {
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawLine(Point start, Point end, Color color)
    => this.DrawLine(start.X, start.Y, end.X, end.Y, color);

  public void DrawRectangle(int x, int y, int width, int height, Color color) {
    this.DrawHorizontalLine(x, y, width, color);
    this.DrawHorizontalLine(x, y + height - 1, width, color);
    this.DrawVerticalLine(x, y + 1, height - 2, color);
    this.DrawVerticalLine(x + width - 1, y + 1, height - 2, color);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangle(Rectangle rect, Color color)
    => this.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height, color);

  public void FillRectangle(int x, int y, int width, int height, Color color) {
    var startX = Math.Max(0, x);
    var startY = Math.Max(0, y);
    var endX = Math.Min(this.Width, x + width);
    var endY = Math.Min(this.Height, y + height);

    for (var py = startY; py < endY; ++py)
    for (var px = startX; px < endX; ++px)
      this[px, py] = color;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangle(Rectangle rect, Color color)
    => this.FillRectangle(rect.X, rect.Y, rect.Width, rect.Height, color);

  public void FillRectangleChecked(int x, int y, int width, int height, Color color) {
    if (x < 0 || y < 0 || x + width > this.Width || y + height > this.Height)
      throw new ArgumentOutOfRangeException(
        null,
        $"Rectangle ({x}, {y}, {width}, {height}) is out of bounds for sub-region ({this.Width}, {this.Height})."
      );

    for (var py = y; py < y + height; ++py)
    for (var px = x; px < x + width; ++px)
      this[px, py] = color;
  }

  public void DrawCircle(int cx, int cy, int radius, Color color) {
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

  public void FillCircle(int cx, int cy, int radius, Color color) {
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

  public void DrawEllipse(int cx, int cy, int rx, int ry, Color color) {
    var rx2 = rx * rx;
    var ry2 = ry * ry;
    var twoRx2 = 2 * rx2;
    var twoRy2 = 2 * ry2;
    var x = 0;
    var y = ry;
    var px = 0;
    var py = twoRx2 * y;

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

  public void FillEllipse(int cx, int cy, int rx, int ry, Color color) {
    var rx2 = rx * rx;
    var ry2 = ry * ry;
    var twoRx2 = 2 * rx2;
    var twoRy2 = 2 * ry2;
    var x = 0;
    var y = ry;
    var px = 0;
    var py = twoRx2 * y;

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

  public void CopyFrom(IBitmapLocker source) {
    var width = Math.Min(this.Width, source.Width);
    var height = Math.Min(this.Height, source.Height);

    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x)
      this[x, y] = source[x, y];
  }

  public void CopyFrom(IBitmapLocker source, int srcX, int srcY, int width, int height, int destX, int destY) {
    var maxWidth = Math.Min(width, Math.Min(source.Width - srcX, this.Width - destX));
    var maxHeight = Math.Min(height, Math.Min(source.Height - srcY, this.Height - destY));

    for (var y = 0; y < maxHeight; ++y)
    for (var x = 0; x < maxWidth; ++x)
      this[destX + x, destY + y] = source[srcX + x, srcY + y];
  }

  #region Rectangle Overloads

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangle(Point location, Size size, Color color)
    => this.DrawRectangle(location.X, location.Y, size.Width, size.Height, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangle(Point location, Size size, Color color)
    => this.FillRectangle(location.X, location.Y, size.Width, size.Height, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangleChecked(Point location, Size size, Color color)
    => this.FillRectangleChecked(location.X, location.Y, size.Width, size.Height, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangleChecked(Rectangle rect, Color color)
    => this.FillRectangleChecked(rect.X, rect.Y, rect.Width, rect.Height, color);

  public void FillRectangleUnchecked(int x, int y, int width, int height, Color color) {
    for (var py = y; py < y + height; ++py)
    for (var px = x; px < x + width; ++px)
      this[px, py] = color;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangleUnchecked(Point location, Size size, Color color)
    => this.FillRectangleUnchecked(location.X, location.Y, size.Width, size.Height, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillRectangleUnchecked(Rectangle rect, Color color)
    => this.FillRectangleUnchecked(rect.X, rect.Y, rect.Width, rect.Height, color);

  public void DrawRectangleChecked(int x, int y, int width, int height, Color color) {
    if (x < 0 || y < 0 || x + width > this.Width || y + height > this.Height)
      throw new ArgumentOutOfRangeException(
        null,
        $"Rectangle ({x}, {y}, {width}, {height}) is out of bounds for sub-region ({this.Width}, {this.Height})."
      );

    this.DrawRectangle(x, y, width, height, color);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangleChecked(Point location, Size size, Color color)
    => this.DrawRectangleChecked(location.X, location.Y, size.Width, size.Height, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangleChecked(Rectangle rect, Color color)
    => this.DrawRectangleChecked(rect.X, rect.Y, rect.Width, rect.Height, color);

  public void DrawRectangleUnchecked(int x, int y, int width, int height, Color color) {
    for (var px = x; px < x + width; ++px) {
      this[px, y] = color;
      this[px, y + height - 1] = color;
    }
    for (var py = y + 1; py < y + height - 1; ++py) {
      this[x, py] = color;
      this[x + width - 1, py] = color;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangleUnchecked(Point location, Size size, Color color)
    => this.DrawRectangleUnchecked(location.X, location.Y, size.Width, size.Height, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawRectangleUnchecked(Rectangle rect, Color color)
    => this.DrawRectangleUnchecked(rect.X, rect.Y, rect.Width, rect.Height, color);

  #endregion

  #region Circle/Ellipse Overloads

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawCircle(Point center, int radius, Color color)
    => this.DrawCircle(center.X, center.Y, radius, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillCircle(Point center, int radius, Color color)
    => this.FillCircle(center.X, center.Y, radius, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawEllipse(Point center, Size radii, Color color)
    => this.DrawEllipse(center.X, center.Y, radii.Width, radii.Height, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DrawEllipse(Rectangle bounds, Color color)
    => this.DrawEllipse(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2, bounds.Width / 2, bounds.Height / 2, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillEllipse(Point center, Size radii, Color color)
    => this.FillEllipse(center.X, center.Y, radii.Width, radii.Height, color);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillEllipse(Rectangle bounds, Color color)
    => this.FillEllipse(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2, bounds.Width / 2, bounds.Height / 2, color);

  #endregion

  #region DrawCross Methods

  public void DrawCross(Point a1, Point b1, Point a2, Point b2, int thickness, Color color) {
    if (thickness <= 1) {
      this.DrawLine(a1, b1, color);
      this.DrawLine(a2, b2, color);
    } else {
      var halfThickness = thickness / 2;
      for (var i = -halfThickness; i <= halfThickness; ++i) {
        this.DrawLine(a1.X + i, a1.Y, b1.X + i, b1.Y, color);
        this.DrawLine(a2.X, a2.Y + i, b2.X, b2.Y + i, color);
      }
    }
  }

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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IBitmapLocker source, Point destLocation)
    => this.CopyFrom(source, 0, 0, source.Width, source.Height, destLocation.X, destLocation.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IBitmapLocker source, Point srcLocation, Size size)
    => this.CopyFrom(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, 0, 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IBitmapLocker source, Point srcLocation, Size size, Point destLocation)
    => this.CopyFrom(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, destLocation.X, destLocation.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IBitmapLocker source, Rectangle srcRect)
    => this.CopyFrom(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 0, 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IBitmapLocker source, Rectangle srcRect, Point destLocation)
    => this.CopyFrom(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destLocation.X, destLocation.Y);

  #endregion

  #region CopyFromChecked Methods

  public void CopyFromChecked(IBitmapLocker source, int srcX, int srcY, int width, int height, int destX, int destY) {
    if (srcX < 0 || srcY < 0 || srcX + width > source.Width || srcY + height > source.Height)
      throw new ArgumentOutOfRangeException(
        nameof(source),
        $"Source rectangle ({srcX}, {srcY}, {width}, {height}) is out of bounds for source ({source.Width}, {source.Height})."
      );

    if (destX < 0 || destY < 0 || destX + width > this.Width || destY + height > this.Height)
      throw new ArgumentOutOfRangeException(
        null,
        $"Destination rectangle ({destX}, {destY}, {width}, {height}) is out of bounds for sub-region ({this.Width}, {this.Height})."
      );

    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x)
      this[destX + x, destY + y] = source[srcX + x, srcY + y];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source)
    => this.CopyFromChecked(source, 0, 0, source.Width, source.Height, 0, 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source, Point destLocation)
    => this.CopyFromChecked(source, 0, 0, source.Width, source.Height, destLocation.X, destLocation.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source, Point srcLocation, Size size)
    => this.CopyFromChecked(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, 0, 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source, Point srcLocation, Size size, Point destLocation)
    => this.CopyFromChecked(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, destLocation.X, destLocation.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source, Rectangle srcRect)
    => this.CopyFromChecked(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 0, 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromChecked(IBitmapLocker source, Rectangle srcRect, Point destLocation)
    => this.CopyFromChecked(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destLocation.X, destLocation.Y);

  #endregion

  #region CopyFromUnchecked Methods

  public void CopyFromUnchecked(IBitmapLocker source, int srcX, int srcY, int width, int height, int destX, int destY) {
    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x)
      this[destX + x, destY + y] = source[srcX + x, srcY + y];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source)
    => this.CopyFromUnchecked(source, 0, 0, source.Width, source.Height, 0, 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source, Point destLocation)
    => this.CopyFromUnchecked(source, 0, 0, source.Width, source.Height, destLocation.X, destLocation.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source, Point srcLocation, Size size)
    => this.CopyFromUnchecked(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, 0, 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source, Point srcLocation, Size size, Point destLocation)
    => this.CopyFromUnchecked(source, srcLocation.X, srcLocation.Y, size.Width, size.Height, destLocation.X, destLocation.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source, Rectangle srcRect)
    => this.CopyFromUnchecked(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 0, 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromUnchecked(IBitmapLocker source, Rectangle srcRect, Point destLocation)
    => this.CopyFromUnchecked(source, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destLocation.X, destLocation.Y);

  #endregion

  #region CopyFromGrid Methods

  public void CopyFromGrid(
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize)
    => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance)
    => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset)
    => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height, offset.Width, offset.Height);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset, Point target)
    => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height, offset.Width, offset.Height, target.X, target.Y);

  #endregion

  public void BlendWith(IBitmapLocker source) {
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

  public bool IsFlatColor {
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

  #endregion
}
