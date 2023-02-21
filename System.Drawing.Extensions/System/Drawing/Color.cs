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

using System.Collections.Generic;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Reflection;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Drawing {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class ColorExtensions {

    public static byte GetLuminance(this Color @this) => _CalculateLuminance(@this);
    public static byte GetChrominanceU(this Color @this) => _CalculateChrominanceU(@this);
    public static byte GetChrominanceV(this Color @this) => _CalculateChrominanceV(@this);

    public static bool IsLike(this Color @this, Color other, byte luminanceDelta = 24, byte chromaUDelta = 7, byte chromaVDelta = 6) {
      if (@this == other)
        return true;

      if (Math.Abs(@this.GetLuminance() - other.GetLuminance()) > luminanceDelta)
        return false;

      if (Math.Abs(@this.GetChrominanceU() - other.GetChrominanceU()) > chromaUDelta)
        return false;

      return Math.Abs(@this.GetChrominanceV() - other.GetChrominanceV()) <= chromaVDelta;
    }

    public static bool IsLikeNaive(this Color @this, Color other, int tolerance = 2) {
      if (@this == other)
        return true;

      var thisColor = @this.ToArgb();
      var otherColor = other.ToArgb();

      if (Math.Abs(@this.R - other.R) > tolerance)
        return false;

      if (Math.Abs(@this.B - other.B) > tolerance)
        return false;

      return Math.Abs(@this.G - other.G) > tolerance;
    }

    private static byte _CalculateLuminance(Color @this)
      => _TopClamp((@this.R * 299 + @this.G * 587 + @this.B * 114) / 1000)
    ;

    private static byte _CalculateChrominanceU(Color @this)
      => _FullClamp((127500000 + @this.R * 500000 - @this.G * 418688 - @this.B * 081312) / 1000000)
    ;

    private static byte _CalculateChrominanceV(Color @this)
      => _FullClamp((127500000 - @this.R * 168736 - @this.G * 331264 + @this.B * 500000) / 1000000)
    ;

    private static byte _FullClamp(int value) => value > byte.MaxValue ? byte.MaxValue : value < byte.MinValue ? byte.MinValue : (byte)value;
    private static byte _TopClamp(int value) => value > byte.MaxValue ? byte.MaxValue : (byte)value;

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
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Color Lighten(this Color This, byte amount) => This.Add(amount);

    /// <summary>
    /// Darkens the given color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="amount">The amount of darkness to add.</param>
    /// <returns>A new color.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Color Darken(this Color This, byte amount) => This.Add(-amount);

    /// <summary>
    /// Adds a value to the RGB components of a given color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>A new color.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Color Add(this Color This, int value) => This.Add(value, value, value);

    /// <summary>
    /// Multiplies the RGB components of a given color by a given value.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <param name="value">The value to multiply with.</param>
    /// <returns>A new color.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

      return Color.FromArgb(This.A, _ClipToByte(r), _ClipToByte(g), _ClipToByte(b));
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

      return Color.FromArgb(This.A, _ClipToByte(r), _ClipToByte(g), _ClipToByte(b));
    }

    /// <summary>
    /// Gets the complementary color.
    /// </summary>
    /// <param name="This">This Color.</param>
    /// <returns>A new color.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
          return _colorLookupTable;

        var result = typeof(Color)
               .GetProperties(BindingFlags.Public | BindingFlags.Static)
               .Select(f => (Color)f.GetValue(null, null))
               .Where(c => c.IsNamedColor)
               .ToDictionary(c => c.ToArgb(), c => c);

        return _colorLookupTable = result;
      }
    }

    /// <summary>
    /// Gets the colors name.
    /// Note: Fixes the issue with colors that were generated instead of chosen directly by looking up the ARGB value.
    /// </summary>
    /// <param name="this">This Color.</param>
    /// <returns>The name of the color or <c>null</c>.</returns>
    public static string GetName(this Color @this) {
      if (!string.IsNullOrEmpty(@this.Name) && @this.Name.Trim().Length != 0)
        return @this.Name;

      if (!@this.IsNamedColor)
        return null;

      return _ColorLookupTable.TryGetValue(@this.ToArgb(), out var color) ? color.Name : null;
    }

    /// <summary>
    /// Converts this color to its corresponding hex-string.
    /// </summary>
    /// <param name="this">This Color.</param>
    /// <returns>The hex-string.</returns>
    public static string ToHex(this Color @this) =>
      "#" + @this.R.ToString("X2") + @this.G.ToString("X2") + @this.B.ToString("X2")
    ;

    #region private methods
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static byte _ClipToByte(int value) => (byte)Math.Min(byte.MaxValue, Math.Max(byte.MinValue, value));

#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static byte _ClipToByte(double value) => _ClipToByte((int)value);

    #endregion

  }
}