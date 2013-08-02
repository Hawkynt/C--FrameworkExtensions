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

using System.Runtime;

namespace System.Drawing {
  internal static partial class ColorExtensions {
    /// <summary>
    /// Lightens the given color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="amount">The amount of lightning to add.</param>
    /// <returns>A new color.</returns>
    public static Color Lighten(this Color This, byte amount) {
      return (This.Add(amount));
    }

    /// <summary>
    /// Darkens the given color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="amount">The amount of darkness to add.</param>
    /// <returns>A new color.</returns>
    public static Color Darken(this Color This, byte amount) {
      return (This.Add(-amount));
    }

    /// <summary>
    /// Adds a value to the RGB components of a given color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>A new color.</returns>
    public static Color Add(this Color This, int value) {
      return (This.Add(value, value, value));
    }

    /// <summary>
    /// Multiplies the RGB components of a given color by a given value.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="value">The value to multiply with.</param>
    /// <returns>A new color.</returns>
    public static Color Multiply(this Color This, double value) {
      return (This.Multiply(value, value, value));
    }

    /// <summary>
    /// Adds values to the RGB components of a given color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="r">The value to add to red.</param>
    /// <param name="g">The value to add to green.</param>
    /// <param name="b">The value to add to blue.</param>
    /// <returns>A new color.</returns>
    public static Color Add(this Color This, int r, int g, int b) {
      r += This.R;
      g += This.G;
      b += This.B;

      return (Color.FromArgb(This.A, _ClipToByte(r), _ClipToByte(g), _ClipToByte(b)));
    }

    /// <summary>
    /// Mutiplies values with the RGB components of a given color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="r">The value to multiply with red.</param>
    /// <param name="g">The value to multiply with green.</param>
    /// <param name="b">The value to multiply with blue.</param>
    /// <returns>A new color.</returns>
    public static Color Multiply(this Color This, double r, double g, double b) {
      r *= This.R;
      g *= This.G;
      b *= This.B;

      return (Color.FromArgb(This.A, _ClipToByte(r), _ClipToByte(g), _ClipToByte(b)));
    }

    /// <summary>
    /// Gets the complementary color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <returns>A new color.</returns>
    public static Color GetComplementaryColor(this Color This) {
      return (Color.FromArgb(This.A, byte.MaxValue - This.R, byte.MaxValue - This.G, byte.MaxValue - This.B));
    }

    #region private methods
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    private static byte _ClipToByte(int value) {
      if (value < byte.MinValue)
        return (byte.MinValue);
      if (value > byte.MaxValue)
        return (byte.MaxValue);
      return ((byte)value);
    }

    [TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
    private static byte _ClipToByte(double value) {
      return (_ClipToByte((int)value));
    }
    #endregion

  }
}