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
  }
}
