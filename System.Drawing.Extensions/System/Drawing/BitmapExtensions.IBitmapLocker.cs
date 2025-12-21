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

using System.Drawing.ColorSpaces;
using System.Drawing.Imaging;

namespace System.Drawing;

public static partial class BitmapExtensions {
  public interface IBitmapLocker : IDisposable {
    BitmapData BitmapData { get; }
    Color this[int x, int y] { get; set; }
    Color this[Point p] { get; set; }
    internal Rgba32 GetPixelRgba32(int x, int y);
    internal void SetPixelRgba32(int x, int y, Rgba32 color);

    void Clear(Color color);
    void DrawHorizontalLine(int x, int y, int count, Color color);
    void DrawHorizontalLine(Point p, int count, Color color);
    void DrawVerticalLine(int x, int y, int count, Color color);
    void DrawVerticalLine(Point p, int count, Color color);
    void DrawLine(int x0, int y0, int x1, int y1, Color color);
    void DrawLine(Point a, Point b, Color color);
    void DrawCross(Point a1, Point b1, Point a2, Point b2, int thickness, Color color);
    void DrawCross(Rectangle rect, int thickness, Color color);

    void DrawRectangle(int x, int y, int width, int height, Color color);
    void DrawRectangle(Point p, Size size, Color color);
    void DrawRectangleChecked(Rectangle rect, Color color, int lineWidth);
    void DrawRectangle(Rectangle rect, Color color);
    void DrawRectangle(int x, int y, int width, int height, Color color, int lineWidth);
    void DrawRectangleChecked(int x, int y, int width, int height, Color color);
    void DrawRectangleChecked(Point p, Size size, Color color);
    void DrawRectangleChecked(Rectangle rect, Color color);
    void DrawRectangleUnchecked(int x, int y, int width, int height, Color color);
    void DrawRectangleUnchecked(Point p, Size size, Color color);
    void DrawRectangleUnchecked(Rectangle rect, Color color);
    void FillRectangle(int x, int y, int width, int height, Color color);
    void FillRectangle(Point p, Size size, Color color);
    void FillRectangle(Rectangle rect, Color color);
    void FillRectangleChecked(int x, int y, int width, int height, Color color);
    void FillRectangleChecked(Point p, Size size, Color color);
    void FillRectangleChecked(Rectangle rect, Color color);
    void FillRectangleUnchecked(int x, int y, int width, int height, Color color);
    void FillRectangleUnchecked(Point p, Size size, Color color);
    void FillRectangleUnchecked(Rectangle rect, Color color);

    void CopyFrom(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void CopyFrom(IBitmapLocker other);
    void CopyFrom(IBitmapLocker other, Point target);
    void CopyFrom(IBitmapLocker other, Point source, Size size);
    void CopyFrom(IBitmapLocker other, Point source, Size size, Point target);
    void CopyFrom(IBitmapLocker other, Rectangle source);
    void CopyFrom(IBitmapLocker other, Rectangle source, Point target);
    void CopyFromChecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void CopyFromChecked(IBitmapLocker other);
    void CopyFromChecked(IBitmapLocker other, Point target);
    void CopyFromChecked(IBitmapLocker other, Point source, Size size);
    void CopyFromChecked(IBitmapLocker other, Point source, Size size, Point target);
    void CopyFromChecked(IBitmapLocker other, Rectangle source);
    void CopyFromChecked(IBitmapLocker other, Rectangle source, Point target);
    void CopyFromUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void CopyFromUnchecked(IBitmapLocker other);
    void CopyFromUnchecked(IBitmapLocker other, Point target);
    void CopyFromUnchecked(IBitmapLocker other, Point source, Size size);
    void CopyFromUnchecked(IBitmapLocker other, Point source, Size size, Point target);
    void CopyFromUnchecked(IBitmapLocker other, Rectangle source);
    void CopyFromUnchecked(IBitmapLocker other, Rectangle source, Point target);

    void BlendWith(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void BlendWith(IBitmapLocker other);
    void BlendWith(IBitmapLocker other, Point target);
    void BlendWith(IBitmapLocker other, Point source, Size size);
    void BlendWith(IBitmapLocker other, Point source, Size size, Point target);
    void BlendWith(IBitmapLocker other, Rectangle source);
    void BlendWith(IBitmapLocker other, Rectangle source, Point target);
    void BlendWithChecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void BlendWithChecked(IBitmapLocker other);
    void BlendWithChecked(IBitmapLocker other, Point target);
    void BlendWithChecked(IBitmapLocker other, Point source, Size size);
    void BlendWithChecked(IBitmapLocker other, Point source, Size size, Point target);
    void BlendWithChecked(IBitmapLocker other, Rectangle source);
    void BlendWithChecked(IBitmapLocker other, Rectangle source, Point target);
    void BlendWithUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void BlendWithUnchecked(IBitmapLocker other);
    void BlendWithUnchecked(IBitmapLocker other, Point target);
    void BlendWithUnchecked(IBitmapLocker other, Point source, Size size);
    void BlendWithUnchecked(IBitmapLocker other, Point source, Size size, Point target);
    void BlendWithUnchecked(IBitmapLocker other, Rectangle source);
    void BlendWithUnchecked(IBitmapLocker other, Rectangle source, Point target);

    void CopyFromGrid(
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
    );

    void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize);
    void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance);
    void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset);
    void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset, Point target);

    Bitmap CopyFromGrid(int column, int row, int width, int height, int dx = 0, int dy = 0, int offsetX = 0, int offsetY = 0);

    Bitmap CopyFromGrid(Point tile, Size tileSize);
    Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance);
    Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance, Size offset);
    Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance, Size offset, Point target);

    /// <summary>
    /// Gets a value indicating whether the entire bitmap region contains a uniform color.
    /// </summary>
    /// <value>
    /// <c>true</c> if all pixels in the locked region share the same color; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property can be used for fast-path optimizations where a single-color fill is detected,
    /// potentially reducing the need for per-pixel operations in image processing routines.
    /// </remarks>
    /// <example>
    /// <code>
    /// using var bitmap = new Bitmap(100, 100);
    /// using var locker = bitmap.Lock();
    /// if (locker.IsFlatColor) {
    ///   Console.WriteLine("Bitmap has a uniform color.");
    /// } else {
    ///   Console.WriteLine("Bitmap contains multiple colors.");
    /// }
    /// </code>
    /// </example>
    bool IsFlatColor { get; }

    int Width { get; }
    int Height { get; }
  }
}
