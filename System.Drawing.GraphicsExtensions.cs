#region (c)2010-2020 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable PartialTypeWithSinglePart

namespace System.Drawing {
  internal static partial class GraphicsExtensions {

    public static void DrawRectangle(this Graphics @this, Color color, Rectangle rectangle) {
      using (var pen = new Pen(color))
        @this.DrawRectangle(pen,rectangle);
    }

    public static void FillRectangle(this Graphics @this, Color color, Rectangle rectangle) {
      using (var brush = new SolidBrush(color))
        @this.FillRectangle(brush, rectangle);
    }

    public static void FillRectangle(this Graphics @this, Brush fill,Pen border, Rectangle rectangle) {
      @this.FillRectangle(fill, rectangle);
      @this.DrawRectangle(border, rectangle);
    }

    public static void FillRectangle(this Graphics @this, Color fill, Color border, Rectangle rectangle) {
      using (var pen = new Pen(border))
      using (var brush = new SolidBrush(fill))
        FillRectangle(@this,brush,pen,rectangle);
    }

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
}


