#region (c)2010-2042 Hawkynt
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

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class RectangleExtensions {
    /// <summary>
    /// Scales the given rectangle by a given factor.
    /// </summary>
    /// <param name="This">This Rectangle.</param>
    /// <param name="factor">The factor.</param>
    /// <returns>A new rectangle</returns>
    public static Rectangle MultiplyBy(this Rectangle This, int factor) {
      return (new Rectangle(This.X * factor, This.Y * factor, This.Width * factor, This.Height * factor));
    }

    /// <summary>
    /// Scales the given rectangle by a given factors.
    /// </summary>
    /// <param name="This">The this.</param>
    /// <param name="xfactor">The x-factor.</param>
    /// <param name="yfactor">The y-factor.</param>
    /// <returns>A new rectangle</returns>
    public static Rectangle MultiplyBy(this Rectangle This, int xfactor, int yfactor) {
      return (new Rectangle(This.X * xfactor, This.Y * yfactor, This.Width * xfactor, This.Height * yfactor));
    }

    public static bool CollidesWith(this Rectangle @this, Rectangle other) {
      var top = other.Top;
      var bottom = other.Bottom;
      var left = other.Left;
      var right = other.Right;
      return @this.CollidesWith(left, top) || @this.CollidesWith(left, bottom) || @this.CollidesWith(right, top) || @this.CollidesWith(right, bottom);
    }

    public static bool CollidesWith(this Rectangle @this, RectangleF other) {
      var top = other.Top;
      var bottom = other.Bottom;
      var left = other.Left;
      var right = other.Right;
      return @this.CollidesWith(left, top) || @this.CollidesWith(left, bottom) || @this.CollidesWith(right, top) || @this.CollidesWith(right, bottom);
    }

    public static bool CollidesWith(this Rectangle @this, Point other) => CollidesWith(@this, other.X, other.Y);
    public static bool CollidesWith(this Rectangle @this, int x, int y) => x >= @this.Left && x <= @this.Right && y >= @this.Top && y <= @this.Bottom;
    public static bool CollidesWith(this Rectangle @this, PointF other) => CollidesWith(@this, other.X, other.Y);
    public static bool CollidesWith(this Rectangle @this, float x, float y) => x >= @this.Left && x <= @this.Right && y >= @this.Top && y <= @this.Bottom;
    public static Point Center(this Rectangle @this) => new Point(@this.X + (@this.Width >> 1), @this.Y + (@this.Height >> 1));

    public static Rectangle SetLeft(this Rectangle @this,int left)=>Rectangle.FromLTRB(left,@this.Top,@this.Right,@this.Bottom);
    public static Rectangle SetRight(this Rectangle @this, int right) => Rectangle.FromLTRB(@this.Left, @this.Top, right, @this.Bottom);
    public static Rectangle SetTop(this Rectangle @this, int top) => Rectangle.FromLTRB(@this.Left, top, @this.Right, @this.Bottom);
    public static Rectangle SetBottom(this Rectangle @this, int bottom) => Rectangle.FromLTRB(@this.Left, @this.Top, @this.Right, bottom);
    

  }
}
