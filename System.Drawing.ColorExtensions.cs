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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Drawing {
  internal static partial class ColorExtensions {

    public static Color BlendWith(this Color @this, Color other, float current, float max) {
      var f = current / max;
      var a = @this.A + (other.A - @this.A) * f;
      var r = @this.R + (other.R - @this.R) * f;
      var g = @this.G + (other.G - @this.G) * f;
      var b = @this.B + (other.B - @this.B) * f;

      return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
    }

    public static Color InterpolateWith(this Color @this, Color other, float factor = 1) {
      var f = 1 + factor;
      var a = (@this.A + factor * other.A) / f;
      var r = (@this.R + factor * other.R) / f;
      var g = (@this.G + factor * other.G) / f;
      var b = (@this.B + factor * other.B) / f;

      return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
    }

    /// <summary>
    /// Lightens the given color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="amount">The amount of lightning to add.</param>
    /// <returns>A new color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Lighten(this Color This, byte amount) => This.Add(amount);

    /// <summary>
    /// Darkens the given color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="amount">The amount of darkness to add.</param>
    /// <returns>A new color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Darken(this Color This, byte amount) => This.Add(-amount);

    /// <summary>
    /// Adds a value to the RGB components of a given color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>A new color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Add(this Color This, int value) => This.Add(value, value, value);

    /// <summary>
    /// Multiplies the RGB components of a given color by a given value.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="value">The value to multiply with.</param>
    /// <returns>A new color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Multiply(this Color This, double value) => This.Multiply(value, value, value);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color GetComplementaryColor(this Color This) => Color.FromArgb(This.A, byte.MaxValue - This.R, byte.MaxValue - This.G, byte.MaxValue - This.B);

    /// <summary>
    /// Cache
    /// </summary>
    private static Dictionary<int, Color> _colorLookupTable;

    /// <summary>
    /// Gets the color lookup table with all known colors in it.
    /// </summary>
    private static Dictionary<int, Color> _ColorLookupTable {
      get {
        if (_colorLookupTable != null)
          return (_colorLookupTable);

        var result = typeof(Color)
               .GetProperties(BindingFlags.Public | BindingFlags.Static)
               .Select(f => (Color)f.GetValue(null, null))
               .Where(c => c.IsNamedColor)
               .ToDictionary(c => c.ToArgb(), c => c);

        return (_colorLookupTable = result);
      }
    }

    /// <summary>
    /// Gets the colors name.
    /// Note: Fixes the issue with colors that were generated instead of chosen directly by looking up the ARGB value.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <returns>The name of the color or <c>null</c>.</returns>
    public static string GetName(this Color This) {
      if (!string.IsNullOrWhiteSpace(This.Name))
        return (This.Name);

      if (!This.IsNamedColor)
        return (null);

      var table = _ColorLookupTable;

      Color color;
      return (table.TryGetValue(This.ToArgb(), out color) ? color.Name : null);
    }

    #region private methods
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    private static byte _ClipToByte(int value) => (byte)(Math.Min(byte.MaxValue, Math.Max(byte.MinValue, value)));

    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    private static byte _ClipToByte(double value) => _ClipToByte((int)value);

    #endregion

  }
}