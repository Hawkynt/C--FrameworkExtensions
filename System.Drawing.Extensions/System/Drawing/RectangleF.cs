﻿#region (c)2010-2042 Hawkynt

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

public static partial class RectangleExtensions {
  /// <summary>
  ///   Scales the given rectangle by a given factor.
  /// </summary>
  /// <param name="This">This Rectangle.</param>
  /// <param name="factor">The factor.</param>
  /// <returns>A new rectangle</returns>
  public static RectangleF MultiplyBy(this RectangleF This, int factor) => new(This.X * factor, This.Y * factor, This.Width * factor, This.Height * factor);

  /// <summary>
  ///   Scales the given rectangle by a given factor.
  /// </summary>
  /// <param name="This">This Rectangle.</param>
  /// <param name="factor">The factor.</param>
  /// <returns>A new rectangle</returns>
  public static RectangleF MultiplyBy(this RectangleF This, float factor) => new(This.X * factor, This.Y * factor, This.Width * factor, This.Height * factor);

  /// <summary>
  ///   Scales the given rectangle by a given factors.
  /// </summary>
  /// <param name="This">The this.</param>
  /// <param name="xfactor">The x-factor.</param>
  /// <param name="yfactor">The y-factor.</param>
  /// <returns>A new rectangle</returns>
  public static RectangleF MultiplyBy(this RectangleF This, int xfactor, int yfactor) => new(This.X * xfactor, This.Y * yfactor, This.Width * xfactor, This.Height * yfactor);

  /// <summary>
  ///   Scales the given rectangle by a given factors.
  /// </summary>
  /// <param name="This">The this.</param>
  /// <param name="xfactor">The x-factor.</param>
  /// <param name="yfactor">The y-factor.</param>
  /// <returns>A new rectangle</returns>
  public static RectangleF MultiplyBy(this RectangleF This, float xfactor, float yfactor) => new(This.X * xfactor, This.Y * yfactor, This.Width * xfactor, This.Height * yfactor);

  /// <summary>
  ///   Returns the center point of a rectangle.
  /// </summary>
  /// <param name="this">The rectangle</param>
  /// <returns>The center point</returns>
  public static PointF Center(this RectangleF @this) {
    var x = @this.X + @this.Width / 2;
    var y = @this.Y + @this.Height / 2;
    return new(x, y);
  }

  public static bool CollidesWith(this RectangleF @this, Rectangle other) {
    var top = other.Top;
    var bottom = other.Bottom;
    var left = other.Left;
    var right = other.Right;
    return @this.CollidesWith(left, top)
           || @this.CollidesWith(left, bottom)
           || @this.CollidesWith(right, top)
           || @this.CollidesWith(right, bottom)
      ;
  }

  public static bool CollidesWith(this RectangleF @this, RectangleF other) {
    var top = other.Top;
    var bottom = other.Bottom;
    var left = other.Left;
    var right = other.Right;
    return @this.CollidesWith(left, top)
           || @this.CollidesWith(left, bottom)
           || @this.CollidesWith(right, top)
           || @this.CollidesWith(right, bottom);
  }

  public static bool CollidesWith(this RectangleF @this, Point other) => CollidesWith(@this, other.X, other.Y);

  public static bool CollidesWith(this RectangleF @this, int x, int y) => x >= @this.Left && x <= @this.Right && y >= @this.Top && y <= @this.Bottom;

  public static bool CollidesWith(this RectangleF @this, PointF other) => CollidesWith(@this, other.X, other.Y);

  public static bool CollidesWith(this RectangleF @this, float x, float y) => x >= @this.Left && x <= @this.Right && y >= @this.Top && y <= @this.Bottom;

  public static Rectangle Round(this RectangleF @this) => Rectangle.FromLTRB((int)Math.Round(@this.Left), (int)Math.Round(@this.Top), (int)Math.Round(@this.Right), (int)Math.Round(@this.Bottom));

  public static Rectangle Ceiling(this RectangleF @this) => Rectangle.FromLTRB((int)Math.Ceiling(@this.Left), (int)Math.Ceiling(@this.Top), (int)Math.Ceiling(@this.Right), (int)Math.Ceiling(@this.Bottom));

  public static Rectangle Floor(this RectangleF @this) => Rectangle.FromLTRB((int)Math.Floor(@this.Left), (int)Math.Floor(@this.Top), (int)Math.Floor(@this.Right), (int)Math.Floor(@this.Bottom));
}
