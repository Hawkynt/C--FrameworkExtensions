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

namespace System.Drawing;

public static partial class GraphicsExtensions {
  public static void DrawString(this Graphics @this, float x, float y, string text, Font font, Brush brush, ContentAlignment anchor) {
    var size = @this.MeasureString(text, font);

    // correct x
    switch (anchor) {
      case ContentAlignment.TopCenter:
      case ContentAlignment.MiddleCenter:
      case ContentAlignment.BottomCenter: {
        x -= size.Width / 2;
        break;
      }
      case ContentAlignment.TopRight:
      case ContentAlignment.MiddleRight:
      case ContentAlignment.BottomRight: {
        x -= size.Width;
        break;
      }
    }

    // correct y
    switch (anchor) {
      case ContentAlignment.MiddleLeft:
      case ContentAlignment.MiddleCenter:
      case ContentAlignment.MiddleRight: {
        y -= size.Height / 2;
        break;
      }
      case ContentAlignment.BottomLeft:
      case ContentAlignment.BottomCenter:
      case ContentAlignment.BottomRight: {
        y -= size.Height;
        break;
      }
    }

    @this.DrawString(text, font, brush, x, y);
  }

  public static void DrawCross(this Graphics @this, float x, float y, float size, Pen pen) {
    @this.DrawLine(pen, x, y, x - size, y);
    @this.DrawLine(pen, x, y, x + size, y);
    @this.DrawLine(pen, x, y, x, y - size);
    @this.DrawLine(pen, x, y, x, y + size);
  }

  public static void DrawCross(this Graphics @this, int x, int y, int size, Pen pen) {
    @this.DrawLine(pen, x, y, x - size, y);
    @this.DrawLine(pen, x, y, x + size, y);
    @this.DrawLine(pen, x, y, x, y - size);
    @this.DrawLine(pen, x, y, x, y + size);
  }

  public static void DrawCross(this Graphics @this, Point p, int size, Pen pen) => DrawCross(@this, p.X, p.Y, size, pen);

  public static void DrawCross(this Graphics @this, PointF p, int size, Pen pen) => DrawCross(@this, p.X, p.Y, size, pen);
}
