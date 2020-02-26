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

namespace System.Drawing {
  internal static partial class RectangleExtensions {
    /// <summary>
    /// Scales the given rectangle by a given factor.
    /// </summary>
    /// <param name="This">This Rectangle.</param>
    /// <param name="factor">The factor.</param>
    /// <returns>A new rectangle</returns>
    public static RectangleF MultiplyBy(this RectangleF This, int factor) {
      return (new RectangleF(This.X * factor, This.Y * factor, This.Width * factor, This.Height * factor));
    }

    /// <summary>
    /// Scales the given rectangle by a given factors.
    /// </summary>
    /// <param name="This">The this.</param>
    /// <param name="xfactor">The x-factor.</param>
    /// <param name="yfactor">The y-factor.</param>
    /// <returns>A new rectangle</returns>
    public static RectangleF MultiplyBy(this RectangleF This, int xfactor, int yfactor) {
      return (new RectangleF(This.X * xfactor, This.Y * yfactor, This.Width * xfactor, This.Height * yfactor));
    }

    /// <summary>
    /// Returns the center point of a rectangle.
    /// </summary>
    /// <param name="this">The rectangle</param>
    /// <returns>The center point</returns>
    public static PointF Center(this RectangleF @this) {
      var x = @this.X + @this.Width / 2;
      var y = @this.Y + @this.Height / 2;
      return new PointF(x, y);
    }

    public static bool CollidesWith(this RectangleF @this, Rectangle other) {
      var top = other.Top;
      var bottom = other.Bottom;
      var left = other.Left;
      var right = other.Right;
      return @this.CollidesWith(left, top) || @this.CollidesWith(left, bottom) || @this.CollidesWith(right, top) || @this.CollidesWith(right, bottom);
    }

    public static bool CollidesWith(this RectangleF @this, RectangleF other) {
      var top = other.Top;
      var bottom = other.Bottom;
      var left = other.Left;
      var right = other.Right;
      return @this.CollidesWith(left, top) || @this.CollidesWith(left, bottom) || @this.CollidesWith(right, top) || @this.CollidesWith(right, bottom);
    }

    public static bool CollidesWith(this RectangleF @this, Point other) => CollidesWith(@this, other.X, other.Y);
    public static bool CollidesWith(this RectangleF @this, int x, int y) => x >= @this.Left && x <= @this.Right && y >= @this.Top && y <= @this.Bottom;
    public static bool CollidesWith(this RectangleF @this, PointF other) => CollidesWith(@this, other.X, other.Y);
    public static bool CollidesWith(this RectangleF @this, float x, float y) => x >= @this.Left && x <= @this.Right && y >= @this.Top && y <= @this.Bottom;

    public static Rectangle Round(this RectangleF @this) => Rectangle.FromLTRB((int)Math.Round(@this.Left), (int)Math.Round(@this.Top), (int)Math.Round(@this.Right), (int)Math.Round(@this.Bottom));
    public static Rectangle Ceiling(this RectangleF @this) => Rectangle.FromLTRB((int)Math.Ceiling(@this.Left), (int)Math.Ceiling(@this.Top), (int)Math.Ceiling(@this.Right), (int)Math.Ceiling(@this.Bottom));
    public static Rectangle Floor(this RectangleF @this) => Rectangle.FromLTRB((int)Math.Floor(@this.Left), (int)Math.Floor(@this.Top), (int)Math.Floor(@this.Right), (int)Math.Floor(@this.Bottom));

  }
}
