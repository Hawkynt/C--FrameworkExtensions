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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Metrics.Lab;
using Hawkynt.ColorProcessing.Spaces.Cmyk;
using Hawkynt.ColorProcessing.Spaces.Cylindrical;
using Hawkynt.ColorProcessing.Spaces.Hdr;
using Hawkynt.ColorProcessing.Spaces.Lab;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Spaces.Yuv;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
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

      var c1 = new Bgra8888(@this);
      var c2 = new Bgra8888(other);

      if (Math.Abs(c1.R - c2.R) > tolerance)
        return false;

      if (Math.Abs(c1.B - c2.B) > tolerance)
        return false;

      return Math.Abs(c1.G - c2.G) > tolerance;
    }

    #endregion

    #region Blending

    public Color BlendWith(Color other, float current, float max) {
      var c1 = new Bgra8888(@this);
      var c2 = new Bgra8888(other);
      var f = current / max;
      var a = c1.A + (c2.A - c1.A) * f;
      var r = c1.R + (c2.R - c1.R) * f;
      var g = c1.G + (c2.G - c1.G) * f;
      var b = c1.B + (c2.B - c1.B) * f;

      return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
    }

    public Color InterpolateWith(Color other, float factor = 1) {
      var c1 = new Bgra8888(@this);
      var c2 = new Bgra8888(other);
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
      var c = new Bgra8888(@this);
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
      var c = new Bgra8888(@this);
      r *= c.R;
      g *= c.G;
      b *= c.B;

      return Color.FromArgb(c.A, _ClipToByte(r), _ClipToByte(g), _ClipToByte(b));
    }

    /// <summary>Gets the complementary color.</summary>
    public Color ComplementaryColor {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var c = new Bgra8888(@this);
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
      var c = new Bgra8888(@this);
      return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }

    #endregion

    #region Color Space Conversions

    public (byte R, byte G, byte B) Rgb {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var result = new Bgra8888(@this);
        return (result.R, result.G, result.B);
      }
    }

    /// <summary>Gets RGB components normalized to 0.0-1.0 range.</summary>
    public (float R, float G, float B) RgbNormalized {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var result = new Bgra8888(@this);
        return (result.R * ColorConstants.ByteToFloat, result.G * ColorConstants.ByteToFloat, result.B * ColorConstants.ByteToFloat);
      }
    }

    /// <summary>Gets HSL components as bytes (0-255).</summary>
    public (byte H, byte S, byte L) Hsl {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var hsl = default(LinearRgbaFToHslF).Project(linear);
        return ((byte)(hsl.H * 255f), (byte)(hsl.S * 255f), (byte)(hsl.L * 255f));
      }
    }

    /// <summary>Gets HSL components normalized to 0.0-1.0 range.</summary>
    public (float H, float S, float L) HslNormalized {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var hsl = default(LinearRgbaFToHslF).Project(linear);
        return (hsl.H, hsl.S, hsl.L);
      }
    }

    /// <summary>Gets HSV components as bytes (0-255).</summary>
    public (byte H, byte S, byte V) Hsv {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var hsv = default(LinearRgbaFToHsvF).Project(linear);
        return ((byte)(hsv.H * 255f), (byte)(hsv.S * 255f), (byte)(hsv.V * 255f));
      }
    }

    /// <summary>Gets HSV components normalized to 0.0-1.0 range.</summary>
    public (float H, float S, float V) HsvNormalized {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var hsv = default(LinearRgbaFToHsvF).Project(linear);
        return (hsv.H, hsv.S, hsv.V);
      }
    }

    /// <summary>Gets CMYK components as bytes (0-255).</summary>
    public (byte C, byte M, byte Y, byte K) Cmyk {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var cmyk = default(LinearRgbaFToCmykF).Project(linear);
        return ((byte)(cmyk.C * 255f), (byte)(cmyk.M * 255f), (byte)(cmyk.Y * 255f), (byte)(cmyk.K * 255f));
      }
    }

    /// <summary>Gets CMYK components normalized to 0.0-1.0 range.</summary>
    public (float C, float M, float Y, float K) CmykNormalized {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var cmyk = default(LinearRgbaFToCmykF).Project(linear);
        return (cmyk.C, cmyk.M, cmyk.Y, cmyk.K);
      }
    }

    /// <summary>Gets YCbCr (BT.601) components as bytes (0-255).</summary>
    /// <remarks>Y is 0-255, Cb/Cr are shifted from -0.5..0.5 to 0-255.</remarks>
    public (byte Y, byte Cb, byte Cr) YCbCr {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var ycbcr = default(LinearRgbaFToYCbCrBt601F).Project(linear);
        return ((byte)(ycbcr.Y * 255f), (byte)((ycbcr.Cb + 0.5f) * 255f), (byte)((ycbcr.Cr + 0.5f) * 255f));
      }
    }

    /// <summary>Gets YCbCr (BT.601) components normalized to 0.0-1.0 range.</summary>
    /// <remarks>All components normalized: Y from 0-1, Cb/Cr shifted from -0.5..0.5 to 0-1.</remarks>
    public (float Y, float Cb, float Cr) YCbCrNormalized {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var ycbcr = default(LinearRgbaFToYCbCrBt601F).Project(linear);
        return (ycbcr.Y, ycbcr.Cb + 0.5f, ycbcr.Cr + 0.5f);
      }
    }

    /// <summary>Gets HWB (Hue, Whiteness, Blackness) components as bytes (0-255).</summary>
    public (byte H, byte W, byte B) Hwb {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var hwb = default(LinearRgbaFToHwbF).Project(linear);
        return ((byte)(hwb.H * 255f), (byte)(hwb.W * 255f), (byte)(hwb.B * 255f));
      }
    }

    /// <summary>Gets HWB (Hue, Whiteness, Blackness) components normalized to 0.0-1.0 range.</summary>
    public (float H, float W, float B) HwbNormalized {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var hwb = default(LinearRgbaFToHwbF).Project(linear);
        return (hwb.H, hwb.W, hwb.B);
      }
    }

    /// <summary>Gets XYZ components as bytes (0-255).</summary>
    /// <remarks>Values are scaled assuming typical D65 reference white.</remarks>
    public (byte X, byte Y, byte Z) Xyz {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var xyz = default(LinearRgbaFToXyzF).Project(linear);
        return ((byte)Math.Min(255, xyz.X * 255f), (byte)Math.Min(255, xyz.Y * 255f), (byte)Math.Min(255, xyz.Z * 255f));
      }
    }

    /// <summary>Gets XYZ components normalized to 0.0-1.0 range.</summary>
    /// <remarks>Values are clamped assuming typical D65 reference white.</remarks>
    public (float X, float Y, float Z) XyzNormalized {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var xyz = default(LinearRgbaFToXyzF).Project(linear);
        return (Math.Min(1f, xyz.X), Math.Min(1f, xyz.Y), Math.Min(1f, xyz.Z));
      }
    }

    /// <summary>Gets L*a*b* components as bytes (0-255).</summary>
    /// <remarks>L is scaled from 0-100 to 0-255. A and B are shifted from -128..127 to 0..255.</remarks>
    public (byte L, byte A, byte B) Lab {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var lab = default(LinearRgbaFToLabF).Project(linear);
        return ((byte)(lab.L * 2.55f), (byte)(lab.A + 128f), (byte)(lab.B + 128f));
      }
    }

    /// <summary>Gets L*a*b* components normalized to 0.0-1.0 range.</summary>
    /// <remarks>L is scaled from 0-100 to 0-1. A and B are shifted from -128..127 and scaled to 0-1.</remarks>
    public (float L, float A, float B) LabNormalized {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var lab = default(LinearRgbaFToLabF).Project(linear);
        return (lab.L / 100f, (lab.A + 128f) / 255f, (lab.B + 128f) / 255f);
      }
    }

    /// <summary>Gets DIN99 components as bytes (0-255).</summary>
    /// <remarks>Components are scaled to 0-255 range.</remarks>
    public (byte L, byte A, byte B) Din99 {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var din99 = default(LinearRgbaFToDin99F).Project(linear);
        return ((byte)(din99.L * 2.55f), (byte)(din99.A + 128f), (byte)(din99.B + 128f));
      }
    }

    /// <summary>Gets DIN99 components normalized to 0.0-1.0 range.</summary>
    /// <remarks>All components normalized to 0-1.</remarks>
    public (float L, float A, float B) Din99Normalized {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var din99 = default(LinearRgbaFToDin99F).Project(linear);
        return (din99.L / 100f, (din99.A + 128f) / 255f, (din99.B + 128f) / 255f);
      }
    }

    /// <summary>Gets YUV (BT.601) components as bytes (0-255).</summary>
    /// <remarks>Y is 0-255, U/V are shifted from -0.5..0.5 to 0-255.</remarks>
    public (byte Y, byte U, byte V) Yuv {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var yuv = default(LinearRgbaFToYuvF).Project(linear);
        return ((byte)(yuv.Y * 255f), (byte)((yuv.U + 0.5f) * 255f), (byte)((yuv.V + 0.5f) * 255f));
      }
    }

    /// <summary>Gets YUV (BT.601) components normalized to 0.0-1.0 range.</summary>
    /// <remarks>All components normalized: Y from 0-1, U/V shifted from -0.5..0.5 to 0-1.</remarks>
    public (float Y, float U, float V) YuvNormalized {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var linear = default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this));
        var yuv = default(LinearRgbaFToYuvF).Project(linear);
        return (yuv.Y, yuv.U + 0.5f, yuv.V + 0.5f);
      }
    }
    
    /// <summary>Calculates the CIE76 color distance to another color.</summary>
    /// <param name="other">The other color to compare to.</param>
    /// <returns>The Euclidean distance in L*a*b* space.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float DistanceTo(Color other) {
      var lab1 = default(LinearRgbaFToLabF).Project(default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this)));
      var lab2 = default(LinearRgbaFToLabF).Project(default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(other)));
      return default(CIE76).Distance(lab1, lab2);
    }

    /// <summary>Calculates the CIEDE2000 perceptual color distance to another color.</summary>
    /// <param name="other">The other color to compare to.</param>
    /// <returns>The CIEDE2000 ΔE00 distance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float PerceptualDistanceTo(Color other) {
      var lab1 = default(LinearRgbaFToLabF).Project(default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(@this)));
      var lab2 = default(LinearRgbaFToLabF).Project(default(Srgb32ToLinearRgbaF).Decode(new Bgra8888(other)));
      return default(CIEDE2000).Distance(lab1, lab2);
    }

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

  #region Static Factory Methods

  extension(Color) {
    /// <summary>Creates a color from HSL values (0-1 range).</summary>
    /// <param name="h">Hue (0-1).</param>
    /// <param name="s">Saturation (0-1).</param>
    /// <param name="l">Lightness (0-1).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromHsl(float h, float s, float l) {
      var hsl = new HslF(h, s, l);
      var linear = default(HslFToLinearRgbF).Project(hsl);
      var rgba = default(LinearRgbaFToSrgb32).Encode(new LinearRgbaF(linear.R, linear.G, linear.B, 1f));
      return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
    }

    /// <summary>Creates a color from HSV values (0-1 range).</summary>
    /// <param name="h">Hue (0-1).</param>
    /// <param name="s">Saturation (0-1).</param>
    /// <param name="v">Value (0-1).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromHsv(float h, float s, float v) {
      var hsv = new HsvF(h, s, v);
      var linear = default(HsvFToLinearRgbF).Project(hsv);
      var rgba = default(LinearRgbaFToSrgb32).Encode(new LinearRgbaF(linear.R, linear.G, linear.B, 1f));
      return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
    }

    /// <summary>Creates a color from L*a*b* values (natural range).</summary>
    /// <param name="l">Lightness (0-100).</param>
    /// <param name="a">A component (-128 to 127).</param>
    /// <param name="b">B component (-128 to 127).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromLab(float l, float a, float b) {
      var lab = new LabF(l, a, b);
      var linear = default(LabFToLinearRgbF).Project(lab);
      var rgba = default(LinearRgbaFToSrgb32).Encode(new LinearRgbaF(linear.R, linear.G, linear.B, 1f));
      return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
    }

    /// <summary>Creates a color from XYZ values (natural range).</summary>
    /// <param name="x">X component.</param>
    /// <param name="y">Y component (luminance).</param>
    /// <param name="z">Z component.</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromXyz(float x, float y, float z) {
      var xyz = new XyzF(x, y, z);
      var linear = default(XyzFToLinearRgbF).Project(xyz);
      var rgba = default(LinearRgbaFToSrgb32).Encode(new LinearRgbaF(linear.R, linear.G, linear.B, 1f));
      return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
    }

    /// <summary>Creates a color from HSL bytes (0-255 range).</summary>
    /// <param name="h">Hue (0-255).</param>
    /// <param name="s">Saturation (0-255).</param>
    /// <param name="l">Lightness (0-255).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromHslBytes(byte h, byte s, byte l)
      => FromHsl(h / 255f, s / 255f, l / 255f);

    /// <summary>Creates a color from HSV bytes (0-255 range).</summary>
    /// <param name="h">Hue (0-255).</param>
    /// <param name="s">Saturation (0-255).</param>
    /// <param name="v">Value (0-255).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromHsvBytes(byte h, byte s, byte v)
      => FromHsv(h / 255f, s / 255f, v / 255f);

    /// <summary>Creates a color from L*a*b* bytes (0-255 range).</summary>
    /// <param name="l">Lightness (0-255, maps to 0-100).</param>
    /// <param name="a">A component (0-255, maps to -128..127).</param>
    /// <param name="b">B component (0-255, maps to -128..127).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromLabBytes(byte l, byte a, byte b)
      => FromLab(l / 2.55f, a - 128f, b - 128f);

    /// <summary>Creates a color from XYZ bytes (0-255 range).</summary>
    /// <param name="x">X component (0-255).</param>
    /// <param name="y">Y component (0-255).</param>
    /// <param name="z">Z component (0-255).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromXyzBytes(byte x, byte y, byte z)
      => FromXyz(x / 255f, y / 255f, z / 255f);

    /// <summary>Creates a color from L*a*b* normalized values (0.0-1.0 range).</summary>
    /// <param name="l">Lightness (0-1, maps to 0-100).</param>
    /// <param name="a">A component (0-1, maps to -128..127).</param>
    /// <param name="b">B component (0-1, maps to -128..127).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromLabNormalized(float l, float a, float b)
      => FromLab(l * 100f, a * 255f - 128f, b * 255f - 128f);

    /// <summary>Creates a color from XYZ normalized values (0.0-1.0 range).</summary>
    /// <param name="x">X component (0-1).</param>
    /// <param name="y">Y component (0-1).</param>
    /// <param name="z">Z component (0-1).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromXyzNormalized(float x, float y, float z)
      => FromXyz(x, y, z);

    /// <summary>Creates a color from CMYK values (0-1 range).</summary>
    /// <param name="c">Cyan (0-1).</param>
    /// <param name="m">Magenta (0-1).</param>
    /// <param name="y">Yellow (0-1).</param>
    /// <param name="k">Key/Black (0-1).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromCmyk(float c, float m, float y, float k) {
      var cmyk = new CmykF(c, m, y, k);
      var linear = default(CmykFToLinearRgbF).Project(cmyk);
      var rgba = default(LinearRgbaFToSrgb32).Encode(new LinearRgbaF(linear.R, linear.G, linear.B, 1f));
      return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
    }

    /// <summary>Creates a color from CMYK bytes (0-255 range).</summary>
    /// <param name="c">Cyan (0-255).</param>
    /// <param name="m">Magenta (0-255).</param>
    /// <param name="y">Yellow (0-255).</param>
    /// <param name="k">Key/Black (0-255).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromCmykBytes(byte c, byte m, byte y, byte k)
      => FromCmyk(c / 255f, m / 255f, y / 255f, k / 255f);

    /// <summary>Creates a color from CMYK normalized values (0.0-1.0 range).</summary>
    /// <param name="c">Cyan (0-1).</param>
    /// <param name="m">Magenta (0-1).</param>
    /// <param name="y">Yellow (0-1).</param>
    /// <param name="k">Key/Black (0-1).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromCmykNormalized(float c, float m, float y, float k)
      => FromCmyk(c, m, y, k);

    /// <summary>Creates a color from YCbCr (BT.601) values (natural range).</summary>
    /// <param name="y">Y luma (0-1).</param>
    /// <param name="cb">Cb chrominance (-0.5 to 0.5).</param>
    /// <param name="cr">Cr chrominance (-0.5 to 0.5).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromYCbCr(float y, float cb, float cr) {
      var ycbcr = new YCbCrF(y, cb, cr);
      var linear = default(YCbCrBt601FToLinearRgbF).Project(ycbcr);
      var rgba = default(LinearRgbaFToSrgb32).Encode(new LinearRgbaF(linear.R, linear.G, linear.B, 1f));
      return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
    }

    /// <summary>Creates a color from YCbCr bytes (0-255 range).</summary>
    /// <param name="y">Y luma (0-255).</param>
    /// <param name="cb">Cb chrominance (0-255, maps to -0.5..0.5).</param>
    /// <param name="cr">Cr chrominance (0-255, maps to -0.5..0.5).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromYCbCrBytes(byte y, byte cb, byte cr)
      => FromYCbCr(y / 255f, cb / 255f - 0.5f, cr / 255f - 0.5f);

    /// <summary>Creates a color from YCbCr normalized values (0.0-1.0 range).</summary>
    /// <param name="y">Y luma (0-1).</param>
    /// <param name="cb">Cb chrominance (0-1, maps to -0.5..0.5).</param>
    /// <param name="cr">Cr chrominance (0-1, maps to -0.5..0.5).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromYCbCrNormalized(float y, float cb, float cr)
      => FromYCbCr(y, cb - 0.5f, cr - 0.5f);

    /// <summary>Creates a color from HWB values (0-1 range).</summary>
    /// <param name="h">Hue (0-1).</param>
    /// <param name="w">Whiteness (0-1).</param>
    /// <param name="b">Blackness (0-1).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromHwb(float h, float w, float b) {
      var hwb = new HwbF(h, w, b);
      var linear = default(HwbFToLinearRgbF).Project(hwb);
      var rgba = default(LinearRgbaFToSrgb32).Encode(new LinearRgbaF(linear.R, linear.G, linear.B, 1f));
      return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
    }

    /// <summary>Creates a color from HWB bytes (0-255 range).</summary>
    /// <param name="h">Hue (0-255).</param>
    /// <param name="w">Whiteness (0-255).</param>
    /// <param name="b">Blackness (0-255).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromHwbBytes(byte h, byte w, byte b)
      => FromHwb(h / 255f, w / 255f, b / 255f);

    /// <summary>Creates a color from HWB normalized values (0.0-1.0 range).</summary>
    /// <param name="h">Hue (0-1).</param>
    /// <param name="w">Whiteness (0-1).</param>
    /// <param name="b">Blackness (0-1).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromHwbNormalized(float h, float w, float b)
      => FromHwb(h, w, b);

    /// <summary>Creates a color from DIN99 values (natural range).</summary>
    /// <param name="l">Lightness (0-100).</param>
    /// <param name="a">A component.</param>
    /// <param name="b">B component.</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromDin99(float l, float a, float b) {
      var din99 = new Din99F(l, a, b);
      var linear = default(Din99FToLinearRgbF).Project(din99);
      var rgba = default(LinearRgbaFToSrgb32).Encode(new LinearRgbaF(linear.R, linear.G, linear.B, 1f));
      return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
    }

    /// <summary>Creates a color from DIN99 bytes (0-255 range).</summary>
    /// <param name="l">Lightness (0-255, maps to 0-100).</param>
    /// <param name="a">A component (0-255, maps to -128..127).</param>
    /// <param name="b">B component (0-255, maps to -128..127).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromDin99Bytes(byte l, byte a, byte b)
      => FromDin99(l / 2.55f, a - 128f, b - 128f);

    /// <summary>Creates a color from DIN99 normalized values (0.0-1.0 range).</summary>
    /// <param name="l">Lightness (0-1, maps to 0-100).</param>
    /// <param name="a">A component (0-1, maps to -128..127).</param>
    /// <param name="b">B component (0-1, maps to -128..127).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromDin99Normalized(float l, float a, float b)
      => FromDin99(l * 100f, a * 255f - 128f, b * 255f - 128f);

    /// <summary>Creates a color from YUV (BT.601) values (natural range).</summary>
    /// <param name="y">Y luma (0-1).</param>
    /// <param name="u">U chrominance (-0.5 to 0.5).</param>
    /// <param name="v">V chrominance (-0.5 to 0.5).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromYuv(float y, float u, float v) {
      var yuv = new YuvF(y, u, v);
      var linear = default(YuvFToLinearRgbF).Project(yuv);
      var rgba = default(LinearRgbaFToSrgb32).Encode(new LinearRgbaF(linear.R, linear.G, linear.B, 1f));
      return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
    }

    /// <summary>Creates a color from YUV bytes (0-255 range).</summary>
    /// <param name="y">Y luma (0-255).</param>
    /// <param name="u">U chrominance (0-255, maps to -0.5..0.5).</param>
    /// <param name="v">V chrominance (0-255, maps to -0.5..0.5).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromYuvBytes(byte y, byte u, byte v)
      => FromYuv(y / 255f, u / 255f - 0.5f, v / 255f - 0.5f);

    /// <summary>Creates a color from YUV normalized values (0.0-1.0 range).</summary>
    /// <param name="y">Y luma (0-1).</param>
    /// <param name="u">U chrominance (0-1, maps to -0.5..0.5).</param>
    /// <param name="v">V chrominance (0-1, maps to -0.5..0.5).</param>
    /// <returns>The resulting color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FromYuvNormalized(float y, float u, float v)
      => FromYuv(y, u - 0.5f, v - 0.5f);

  }

  #endregion

}
