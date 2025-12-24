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

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Hawkynt.Drawing.Lockers;

/// <summary>
/// Provides basic bitmap locking with Color-based pixel access.
/// </summary>
public interface IBitmapLocker : IDisposable {
  /// <summary>Width of the bitmap in pixels.</summary>
  int Width { get; }

  /// <summary>Height of the bitmap in pixels.</summary>
  int Height { get; }

  /// <summary>Gets or sets the color at the specified coordinates.</summary>
  Color this[int x, int y] { get; set; }

  /// <summary>Gets or sets the color at the specified point.</summary>
  Color this[Point p] { get; set; }

  /// <summary>Gets the underlying BitmapData for low-level access.</summary>
  BitmapData BitmapData { get; }

  #region Drawing Methods

  /// <summary>Clears the bitmap with the specified color.</summary>
  /// <param name="color">The color to fill with.</param>
  void Clear(Color color);

  /// <summary>Draws a horizontal line.</summary>
  /// <param name="x">Starting X coordinate.</param>
  /// <param name="y">Y coordinate.</param>
  /// <param name="length">Length in pixels.</param>
  /// <param name="color">Line color.</param>
  void DrawHorizontalLine(int x, int y, int length, Color color);

  /// <summary>Draws a horizontal line.</summary>
  /// <param name="start">Starting point.</param>
  /// <param name="length">Length in pixels.</param>
  /// <param name="color">Line color.</param>
  void DrawHorizontalLine(Point start, int length, Color color);

  /// <summary>Draws a horizontal line without bounds checking. May write out of bounds.</summary>
  /// <param name="x">Starting X coordinate.</param>
  /// <param name="y">Y coordinate.</param>
  /// <param name="length">Length in pixels.</param>
  /// <param name="color">Line color.</param>
  void DrawHorizontalLineUnchecked(int x, int y, int length, Color color);

  /// <summary>Draws a vertical line.</summary>
  /// <param name="x">X coordinate.</param>
  /// <param name="y">Starting Y coordinate.</param>
  /// <param name="length">Length in pixels.</param>
  /// <param name="color">Line color.</param>
  void DrawVerticalLine(int x, int y, int length, Color color);

  /// <summary>Draws a vertical line.</summary>
  /// <param name="start">Starting point.</param>
  /// <param name="length">Length in pixels.</param>
  /// <param name="color">Line color.</param>
  void DrawVerticalLine(Point start, int length, Color color);

  /// <summary>Draws a vertical line without bounds checking. May write out of bounds.</summary>
  /// <param name="x">X coordinate.</param>
  /// <param name="y">Starting Y coordinate.</param>
  /// <param name="length">Length in pixels.</param>
  /// <param name="color">Line color.</param>
  void DrawVerticalLineUnchecked(int x, int y, int length, Color color);

  /// <summary>Draws a line between two points using Bresenham's algorithm.</summary>
  /// <param name="x1">Start X.</param>
  /// <param name="y1">Start Y.</param>
  /// <param name="x2">End X.</param>
  /// <param name="y2">End Y.</param>
  /// <param name="color">Line color.</param>
  void DrawLine(int x1, int y1, int x2, int y2, Color color);

  /// <summary>Draws a line between two points using Bresenham's algorithm.</summary>
  /// <param name="start">Start point.</param>
  /// <param name="end">End point.</param>
  /// <param name="color">Line color.</param>
  void DrawLine(Point start, Point end, Color color);

  /// <summary>Draws a rectangle outline.</summary>
  /// <param name="x">Top-left X.</param>
  /// <param name="y">Top-left Y.</param>
  /// <param name="width">Width.</param>
  /// <param name="height">Height.</param>
  /// <param name="color">Line color.</param>
  void DrawRectangle(int x, int y, int width, int height, Color color);

  /// <summary>Draws a rectangle outline.</summary>
  /// <param name="rect">The rectangle to draw.</param>
  /// <param name="color">Line color.</param>
  void DrawRectangle(Rectangle rect, Color color);

  /// <summary>Draws a rectangle outline.</summary>
  /// <param name="location">Top-left corner.</param>
  /// <param name="size">Width and height.</param>
  /// <param name="color">Line color.</param>
  void DrawRectangle(Point location, Size size, Color color);

  /// <summary>Draws a rectangle outline with bounds checking.</summary>
  /// <param name="x">Top-left X.</param>
  /// <param name="y">Top-left Y.</param>
  /// <param name="width">Width.</param>
  /// <param name="height">Height.</param>
  /// <param name="color">Line color.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void DrawRectangleChecked(int x, int y, int width, int height, Color color);

  /// <summary>Draws a rectangle outline with bounds checking.</summary>
  /// <param name="location">Top-left corner.</param>
  /// <param name="size">Width and height.</param>
  /// <param name="color">Line color.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void DrawRectangleChecked(Point location, Size size, Color color);

  /// <summary>Draws a rectangle outline with bounds checking.</summary>
  /// <param name="rect">The rectangle to draw.</param>
  /// <param name="color">Line color.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void DrawRectangleChecked(Rectangle rect, Color color);

  /// <summary>Draws a rectangle outline without bounds checking. May write out of bounds.</summary>
  /// <param name="x">Top-left X.</param>
  /// <param name="y">Top-left Y.</param>
  /// <param name="width">Width.</param>
  /// <param name="height">Height.</param>
  /// <param name="color">Line color.</param>
  void DrawRectangleUnchecked(int x, int y, int width, int height, Color color);

  /// <summary>Draws a rectangle outline without bounds checking. May write out of bounds.</summary>
  /// <param name="location">Top-left corner.</param>
  /// <param name="size">Width and height.</param>
  /// <param name="color">Line color.</param>
  void DrawRectangleUnchecked(Point location, Size size, Color color);

  /// <summary>Draws a rectangle outline without bounds checking. May write out of bounds.</summary>
  /// <param name="rect">The rectangle to draw.</param>
  /// <param name="color">Line color.</param>
  void DrawRectangleUnchecked(Rectangle rect, Color color);

  /// <summary>Fills a rectangle with a solid color.</summary>
  /// <param name="x">Top-left X.</param>
  /// <param name="y">Top-left Y.</param>
  /// <param name="width">Width.</param>
  /// <param name="height">Height.</param>
  /// <param name="color">Fill color.</param>
  void FillRectangle(int x, int y, int width, int height, Color color);

  /// <summary>Fills a rectangle with a solid color.</summary>
  /// <param name="rect">The rectangle to fill.</param>
  /// <param name="color">Fill color.</param>
  void FillRectangle(Rectangle rect, Color color);

  /// <summary>Fills a rectangle with a solid color.</summary>
  /// <param name="location">Top-left corner.</param>
  /// <param name="size">Width and height.</param>
  /// <param name="color">Fill color.</param>
  void FillRectangle(Point location, Size size, Color color);

  /// <summary>Fills a rectangle with a solid color, with bounds checking.</summary>
  /// <param name="x">Top-left X.</param>
  /// <param name="y">Top-left Y.</param>
  /// <param name="width">Width.</param>
  /// <param name="height">Height.</param>
  /// <param name="color">Fill color.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void FillRectangleChecked(int x, int y, int width, int height, Color color);

  /// <summary>Fills a rectangle with a solid color, with bounds checking.</summary>
  /// <param name="location">Top-left corner.</param>
  /// <param name="size">Width and height.</param>
  /// <param name="color">Fill color.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void FillRectangleChecked(Point location, Size size, Color color);

  /// <summary>Fills a rectangle with a solid color, with bounds checking.</summary>
  /// <param name="rect">The rectangle to fill.</param>
  /// <param name="color">Fill color.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void FillRectangleChecked(Rectangle rect, Color color);

  /// <summary>Fills a rectangle with a solid color without bounds checking. May write out of bounds.</summary>
  /// <param name="x">Top-left X.</param>
  /// <param name="y">Top-left Y.</param>
  /// <param name="width">Width.</param>
  /// <param name="height">Height.</param>
  /// <param name="color">Fill color.</param>
  void FillRectangleUnchecked(int x, int y, int width, int height, Color color);

  /// <summary>Fills a rectangle with a solid color without bounds checking. May write out of bounds.</summary>
  /// <param name="location">Top-left corner.</param>
  /// <param name="size">Width and height.</param>
  /// <param name="color">Fill color.</param>
  void FillRectangleUnchecked(Point location, Size size, Color color);

  /// <summary>Fills a rectangle with a solid color without bounds checking. May write out of bounds.</summary>
  /// <param name="rect">The rectangle to fill.</param>
  /// <param name="color">Fill color.</param>
  void FillRectangleUnchecked(Rectangle rect, Color color);

  /// <summary>Draws a circle outline using midpoint algorithm.</summary>
  /// <param name="cx">Center X.</param>
  /// <param name="cy">Center Y.</param>
  /// <param name="radius">Radius in pixels.</param>
  /// <param name="color">Line color.</param>
  void DrawCircle(int cx, int cy, int radius, Color color);

  /// <summary>Draws a circle outline using midpoint algorithm.</summary>
  /// <param name="center">Center point.</param>
  /// <param name="radius">Radius in pixels.</param>
  /// <param name="color">Line color.</param>
  void DrawCircle(Point center, int radius, Color color);

  /// <summary>Fills a circle with a solid color.</summary>
  /// <param name="cx">Center X.</param>
  /// <param name="cy">Center Y.</param>
  /// <param name="radius">Radius in pixels.</param>
  /// <param name="color">Fill color.</param>
  void FillCircle(int cx, int cy, int radius, Color color);

  /// <summary>Fills a circle with a solid color.</summary>
  /// <param name="center">Center point.</param>
  /// <param name="radius">Radius in pixels.</param>
  /// <param name="color">Fill color.</param>
  void FillCircle(Point center, int radius, Color color);

  /// <summary>Draws an ellipse outline.</summary>
  /// <param name="cx">Center X.</param>
  /// <param name="cy">Center Y.</param>
  /// <param name="rx">X radius.</param>
  /// <param name="ry">Y radius.</param>
  /// <param name="color">Line color.</param>
  void DrawEllipse(int cx, int cy, int rx, int ry, Color color);

  /// <summary>Draws an ellipse outline.</summary>
  /// <param name="center">Center point.</param>
  /// <param name="radii">X and Y radii.</param>
  /// <param name="color">Line color.</param>
  void DrawEllipse(Point center, Size radii, Color color);

  /// <summary>Draws an ellipse within a bounding rectangle.</summary>
  /// <param name="bounds">Bounding rectangle.</param>
  /// <param name="color">Line color.</param>
  void DrawEllipse(Rectangle bounds, Color color);

  /// <summary>Fills an ellipse with a solid color.</summary>
  /// <param name="cx">Center X.</param>
  /// <param name="cy">Center Y.</param>
  /// <param name="rx">X radius.</param>
  /// <param name="ry">Y radius.</param>
  /// <param name="color">Fill color.</param>
  void FillEllipse(int cx, int cy, int rx, int ry, Color color);

  /// <summary>Fills an ellipse with a solid color.</summary>
  /// <param name="center">Center point.</param>
  /// <param name="radii">X and Y radii.</param>
  /// <param name="color">Fill color.</param>
  void FillEllipse(Point center, Size radii, Color color);

  /// <summary>Fills an ellipse within a bounding rectangle.</summary>
  /// <param name="bounds">Bounding rectangle.</param>
  /// <param name="color">Fill color.</param>
  void FillEllipse(Rectangle bounds, Color color);

  /// <summary>Draws a cross between two line segments.</summary>
  /// <param name="a1">First line start.</param>
  /// <param name="b1">First line end.</param>
  /// <param name="a2">Second line start.</param>
  /// <param name="b2">Second line end.</param>
  /// <param name="thickness">Line thickness.</param>
  /// <param name="color">Line color.</param>
  void DrawCross(Point a1, Point b1, Point a2, Point b2, int thickness, Color color);

  /// <summary>Draws a cross within a bounding rectangle.</summary>
  /// <param name="rect">Bounding rectangle.</param>
  /// <param name="thickness">Line thickness.</param>
  /// <param name="color">Line color.</param>
  void DrawCross(Rectangle rect, int thickness, Color color);

  /// <summary>Copies all pixels from another locker.</summary>
  /// <param name="source">Source bitmap locker.</param>
  void CopyFrom(IBitmapLocker source);

  /// <summary>Copies all pixels from another locker to the specified destination.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="destLocation">Destination offset.</param>
  void CopyFrom(IBitmapLocker source, Point destLocation);

  /// <summary>Copies a region of pixels from another locker.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcLocation">Source offset.</param>
  /// <param name="size">Size of region.</param>
  void CopyFrom(IBitmapLocker source, Point srcLocation, Size size);

  /// <summary>Copies a region of pixels from another locker.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcLocation">Source offset.</param>
  /// <param name="size">Size of region.</param>
  /// <param name="destLocation">Destination offset.</param>
  void CopyFrom(IBitmapLocker source, Point srcLocation, Size size, Point destLocation);

  /// <summary>Copies a region of pixels from another locker.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcRect">Source rectangle.</param>
  void CopyFrom(IBitmapLocker source, Rectangle srcRect);

  /// <summary>Copies a region of pixels from another locker.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcRect">Source rectangle.</param>
  /// <param name="destLocation">Destination offset.</param>
  void CopyFrom(IBitmapLocker source, Rectangle srcRect, Point destLocation);

  /// <summary>Copies a region of pixels from another locker.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcX">Source X offset.</param>
  /// <param name="srcY">Source Y offset.</param>
  /// <param name="width">Width of region.</param>
  /// <param name="height">Height of region.</param>
  /// <param name="destX">Destination X offset.</param>
  /// <param name="destY">Destination Y offset.</param>
  void CopyFrom(IBitmapLocker source, int srcX, int srcY, int width, int height, int destX, int destY);

  /// <summary>Copies all pixels from another locker with bounds checking.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if dimensions don't match.</exception>
  void CopyFromChecked(IBitmapLocker source);

  /// <summary>Copies all pixels from another locker with bounds checking.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="destLocation">Destination offset.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void CopyFromChecked(IBitmapLocker source, Point destLocation);

  /// <summary>Copies a region of pixels from another locker with bounds checking.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcX">Source X offset.</param>
  /// <param name="srcY">Source Y offset.</param>
  /// <param name="width">Width of region.</param>
  /// <param name="height">Height of region.</param>
  /// <param name="destX">Destination X offset.</param>
  /// <param name="destY">Destination Y offset.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void CopyFromChecked(IBitmapLocker source, int srcX, int srcY, int width, int height, int destX, int destY);

  /// <summary>Copies a region of pixels from another locker with bounds checking.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcLocation">Source offset.</param>
  /// <param name="size">Size of region.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void CopyFromChecked(IBitmapLocker source, Point srcLocation, Size size);

  /// <summary>Copies a region of pixels from another locker with bounds checking.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcLocation">Source offset.</param>
  /// <param name="size">Size of region.</param>
  /// <param name="destLocation">Destination offset.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void CopyFromChecked(IBitmapLocker source, Point srcLocation, Size size, Point destLocation);

  /// <summary>Copies a region of pixels from another locker with bounds checking.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcRect">Source rectangle.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void CopyFromChecked(IBitmapLocker source, Rectangle srcRect);

  /// <summary>Copies a region of pixels from another locker with bounds checking.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcRect">Source rectangle.</param>
  /// <param name="destLocation">Destination offset.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if any coordinate is out of bounds.</exception>
  void CopyFromChecked(IBitmapLocker source, Rectangle srcRect, Point destLocation);

  /// <summary>Copies all pixels from another locker without bounds checking. May read/write out of bounds.</summary>
  /// <param name="source">Source bitmap locker.</param>
  void CopyFromUnchecked(IBitmapLocker source);

  /// <summary>Copies all pixels from another locker without bounds checking. May read/write out of bounds.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="destLocation">Destination offset.</param>
  void CopyFromUnchecked(IBitmapLocker source, Point destLocation);

  /// <summary>Copies a region of pixels from another locker without bounds checking. May read/write out of bounds.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcX">Source X offset.</param>
  /// <param name="srcY">Source Y offset.</param>
  /// <param name="width">Width of region.</param>
  /// <param name="height">Height of region.</param>
  /// <param name="destX">Destination X offset.</param>
  /// <param name="destY">Destination Y offset.</param>
  void CopyFromUnchecked(IBitmapLocker source, int srcX, int srcY, int width, int height, int destX, int destY);

  /// <summary>Copies a region of pixels from another locker without bounds checking. May read/write out of bounds.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcLocation">Source offset.</param>
  /// <param name="size">Size of region.</param>
  void CopyFromUnchecked(IBitmapLocker source, Point srcLocation, Size size);

  /// <summary>Copies a region of pixels from another locker without bounds checking. May read/write out of bounds.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcLocation">Source offset.</param>
  /// <param name="size">Size of region.</param>
  /// <param name="destLocation">Destination offset.</param>
  void CopyFromUnchecked(IBitmapLocker source, Point srcLocation, Size size, Point destLocation);

  /// <summary>Copies a region of pixels from another locker without bounds checking. May read/write out of bounds.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcRect">Source rectangle.</param>
  void CopyFromUnchecked(IBitmapLocker source, Rectangle srcRect);

  /// <summary>Copies a region of pixels from another locker without bounds checking. May read/write out of bounds.</summary>
  /// <param name="source">Source bitmap locker.</param>
  /// <param name="srcRect">Source rectangle.</param>
  /// <param name="destLocation">Destination offset.</param>
  void CopyFromUnchecked(IBitmapLocker source, Rectangle srcRect, Point destLocation);

  /// <summary>Blends source bitmap over this bitmap using alpha.</summary>
  /// <param name="source">Source bitmap locker.</param>
  void BlendWith(IBitmapLocker source);

  /// <summary>Gets a value indicating whether all pixels have the same color.</summary>
  bool IsFlatColor { get; }

  #endregion
}
