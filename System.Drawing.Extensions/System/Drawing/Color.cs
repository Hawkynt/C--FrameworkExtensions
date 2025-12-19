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

using System.Collections.Generic;
using System.Drawing.ColorSpaces;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing;

public static partial class ColorPolyfills {

  extension(Color @this) {

    [Obsolete("Use Yuv property instead")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetLuminance() => @this.Yuv.Y;

    [Obsolete("Use Yuv property instead")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetChrominanceU() => @this.Yuv.U;

    [Obsolete("Use Yuv property instead")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetChrominanceV() => @this.Yuv.V;

    #region Color Comparison

    public bool IsLike(Color other, byte luminanceDelta = 24, byte chromaUDelta = 7, byte chromaVDelta = 6) {
      if (@this == other)
        return true;

      var yuv1 = @this.Yuv;
      var yuv2 = other.Yuv;

      if (Math.Abs(yuv1.Y - yuv2.Y) > luminanceDelta)
        return false;

      if (Math.Abs(yuv1.U - yuv2.U) > chromaUDelta)
        return false;

      return Math.Abs(yuv1.V - yuv2.V) <= chromaVDelta;
    }

    public bool IsLikeNaive(Color other, int tolerance = 2) {
      if (@this == other)
        return true;

      var c1 = new Rgba32(@this);
      var c2 = new Rgba32(other);

      if (Math.Abs(c1.R - c2.R) > tolerance)
        return false;

      if (Math.Abs(c1.B - c2.B) > tolerance)
        return false;

      return Math.Abs(c1.G - c2.G) > tolerance;
    }

    #endregion

    #region Blending

    public Color BlendWith(Color other, float current, float max) {
      var c1 = new Rgba32(@this);
      var c2 = new Rgba32(other);
      var f = current / max;
      var a = c1.A + (c2.A - c1.A) * f;
      var r = c1.R + (c2.R - c1.R) * f;
      var g = c1.G + (c2.G - c1.G) * f;
      var b = c1.B + (c2.B - c1.B) * f;

      return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
    }

    public Color InterpolateWith(Color other, float factor = 1) {
      var c1 = new Rgba32(@this);
      var c2 = new Rgba32(other);
      var f = 1 + factor;
      var a = (c1.A + factor * c2.A) / f;
      var r = (c1.R + factor * c2.R) / f;
      var g = (c1.G + factor * c2.G) / f;
      var b = (c1.B + factor * c2.B) / f;

      return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
    }

    #endregion

    #region Lighten/Darken

    /// <summary>Lightens the given color.</summary>
    /// <param name="amount">The amount of lightning to add.</param>
    /// <returns>A new color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color Lighten(byte amount) => @this.Add(amount);

    /// <summary>Darkens the given color.</summary>
    /// <param name="amount">The amount of darkness to add.</param>
    /// <returns>A new color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color Darken(byte amount) => @this.Add(-amount);

    /// <summary>Adds a value to the RGB components of a given color.</summary>
    /// <param name="value">The value to add.</param>
    /// <returns>A new color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color Add(int value) => @this.Add(value, value, value);

    /// <summary>Multiplies the RGB components of a given color by a given value.</summary>
    /// <param name="value">The value to multiply with.</param>
    /// <returns>A new color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color Multiply(double value) => @this.Multiply(value, value, value);

    /// <summary>Adds values to the RGB components of a given color.</summary>
    /// <param name="r">The value to add to red.</param>
    /// <param name="g">The value to add to green.</param>
    /// <param name="b">The value to add to blue.</param>
    /// <returns>A new color.</returns>
    public Color Add(int r, int g, int b) {
      var c = new Rgba32(@this);
      r += c.R;
      g += c.G;
      b += c.B;

      return Color.FromArgb(c.A, _ClipToByte(r), _ClipToByte(g), _ClipToByte(b));
    }

    /// <summary>Multiplies values with the RGB components of a given color.</summary>
    /// <param name="r">The value to multiply with red.</param>
    /// <param name="g">The value to multiply with green.</param>
    /// <param name="b">The value to multiply with blue.</param>
    /// <returns>A new color.</returns>
    public Color Multiply(double r, double g, double b) {
      var c = new Rgba32(@this);
      r *= c.R;
      g *= c.G;
      b *= c.B;

      return Color.FromArgb(c.A, _ClipToByte(r), _ClipToByte(g), _ClipToByte(b));
    }

    /// <summary>Gets the complementary color.</summary>
    public Color ComplementaryColor {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var c = new Rgba32(@this);
        return Color.FromArgb(c.A, byte.MaxValue - c.R, byte.MaxValue - c.G, byte.MaxValue - c.B);
      }
    }

    /// <summary>Gets the complementary color.</summary>
    /// <returns>A new color.</returns>
    [Obsolete("Use ComplementaryColor property instead")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color GetComplementaryColor() => @this.ComplementaryColor;

    #endregion

    #region Name and Hex

    /// <summary>
    /// Gets the colors name.
    /// Note: Fixes the issue with colors that were generated instead of chosen directly by looking up the ARGB value.
    /// </summary>
    /// <returns>The name of the color or <c>null</c>.</returns>
    public string GetName() {
      if (!string.IsNullOrWhiteSpace(@this.Name))
        return @this.Name;

      if (!@this.IsNamedColor)
        return null;

      return _ColorLookupTable.TryGetValue(@this.ToArgb(), out var color) ? color.Name : null;
    }

    /// <summary>Converts this color to its corresponding hex-string.</summary>
    /// <returns>The hex-string.</returns>
    public string ToHex() {
      var c = new Rgba32(@this);
      return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }

    #endregion

    #region Color Space Conversions

    /// <summary>Converts this <see cref="Color"/> to RGB color space (byte components 0-255).</summary>
    public Rgb Rgb => (Rgb)Rgb.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to RGB color space (normalized 0.0-1.0 components).</summary>
    public RgbNormalized RgbNormalized => (RgbNormalized)RgbNormalized.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to HSL color space (byte components 0-255).</summary>
    public Hsl Hsl => (Hsl)Hsl.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to HSL color space (normalized 0.0-1.0 components).</summary>
    public HslNormalized HslNormalized => (HslNormalized)HslNormalized.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to HSV color space (byte components 0-255).</summary>
    public Hsv Hsv => (Hsv)Hsv.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to HSV color space (normalized 0.0-1.0 components).</summary>
    public HsvNormalized HsvNormalized => (HsvNormalized)HsvNormalized.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to CMYK color space (byte components 0-255).</summary>
    public Cmyk Cmyk => (Cmyk)Cmyk.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to CMYK color space (normalized 0.0-1.0 components).</summary>
    public CmykNormalized CmykNormalized => (CmykNormalized)CmykNormalized.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to YCbCr color space (byte components 0-255).</summary>
    public YCbCr YCbCr => (YCbCr)YCbCr.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to YCbCr color space (normalized 0.0-1.0 components).</summary>
    public YCbCrNormalized YCbCrNormalized => (YCbCrNormalized)YCbCrNormalized.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to HWB color space (byte components 0-255).</summary>
    public Hwb Hwb => (Hwb)Hwb.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to HWB color space (normalized 0.0-1.0 components).</summary>
    public HwbNormalized HwbNormalized => (HwbNormalized)HwbNormalized.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to CIE XYZ color space (byte components 0-255).</summary>
    public Xyz Xyz => (Xyz)Xyz.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to CIE XYZ color space (normalized components).</summary>
    public XyzNormalized XyzNormalized => (XyzNormalized)XyzNormalized.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to CIE L*a*b* color space (byte components 0-255).</summary>
    public Lab Lab => (Lab)Lab.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to CIE L*a*b* color space (normalized components).</summary>
    public LabNormalized LabNormalized => (LabNormalized)LabNormalized.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to DIN99 color space (byte components 0-255).</summary>
    public Din99 Din99 => (Din99)Din99.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to DIN99 color space (normalized components).</summary>
    public Din99Normalized Din99Normalized => (Din99Normalized)Din99Normalized.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to YUV color space (byte components 0-255).</summary>
    public Yuv Yuv => (Yuv)Yuv.FromColor(@this);

    /// <summary>Converts this <see cref="Color"/> to YUV color space (normalized components).</summary>
    public YuvNormalized YuvNormalized => (YuvNormalized)YuvNormalized.FromColor(@this);

    #endregion

  }

  #region Helper Fields and Methods

  private static Dictionary<int, Color> _ColorLookupTable {
    get {
      if (field != null)
        return field;

      var result = typeof(Color)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Select(f => (Color)f.GetValue(null, null))
        .Where(c => c.IsNamedColor)
        .ToDictionary(c => c.ToArgb(), c => c);

      return field = result;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _ClipToByte(int value) => (byte)Math.Min(byte.MaxValue, Math.Max(byte.MinValue, value));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _ClipToByte(double value) => _ClipToByte((int)value);

  #endregion

}
