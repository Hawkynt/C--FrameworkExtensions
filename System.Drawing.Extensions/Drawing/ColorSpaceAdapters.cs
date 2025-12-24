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

using System.Drawing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Spaces.Cylindrical;
using Hawkynt.ColorProcessing.Spaces.Hdr;
using Hawkynt.ColorProcessing.Spaces.Lab;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Spaces.Yuv;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Provides extension methods for converting System.Drawing.Color to various color spaces.
/// </summary>
/// <remarks>
/// All conversions go through LinearRgbaF as the intermediate working space.
/// sRGB gamma is properly applied when encoding/decoding.
/// </remarks>
public static class ColorSpaceAdapters {

  #region Cylindrical Color Spaces

  /// <summary>
  /// Converts a Color to HSL (Hue, Saturation, Lightness).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HslF ToHslF(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToHslF().Project(linear);
  }

  /// <summary>
  /// Converts HSL to Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this HslF hsl) {
    var linear = new HslFToLinearRgbF().Project(hsl);
    return new LinearRgbaF(linear.R, linear.G, linear.B, 1f).ToColor();
  }

  /// <summary>
  /// Converts a Color to HSV (Hue, Saturation, Value).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HsvF ToHsvF(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToHsvF().Project(linear);
  }

  /// <summary>
  /// Converts HSV to Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this HsvF hsv) {
    var linear = new HsvFToLinearRgbF().Project(hsv);
    return new LinearRgbaF(linear.R, linear.G, linear.B, 1f).ToColor();
  }

  /// <summary>
  /// Converts a Color to HWB (Hue, Whiteness, Blackness).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HwbF ToHwbF(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToHwbF().Project(linear);
  }

  /// <summary>
  /// Converts HWB to Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this HwbF hwb) {
    var linear = new HwbFToLinearRgbF().Project(hwb);
    return new LinearRgbaF(linear.R, linear.G, linear.B, 1f).ToColor();
  }

  /// <summary>
  /// Converts a Color to LCh (Lightness, Chroma, Hue) in CIE Lab space.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LchF ToLchF(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToLchF().Project(linear);
  }

  /// <summary>
  /// Converts LCh to Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this LchF lch) {
    var linear = new LchFToLinearRgbF().Project(lch);
    return new LinearRgbaF(linear.R, linear.G, linear.B, 1f).ToColor();
  }

  #endregion

  #region YUV Color Spaces

  /// <summary>
  /// Converts a Color to YUV (BT.601).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static YuvF ToYuvF(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToYuvF().Project(linear);
  }

  #endregion

  #region CIE Color Spaces

  /// <summary>
  /// Converts a Color to CIE XYZ.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static XyzF ToXyzF(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToXyzF().Project(linear);
  }

  /// <summary>
  /// Converts XYZ to Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this XyzF xyz) {
    var linear = new XyzFToLinearRgbF().Project(xyz);
    return new LinearRgbaF(linear.R, linear.G, linear.B, 1f).ToColor();
  }

  /// <summary>
  /// Converts a Color to CIE Lab.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LabF ToLabF(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToLabF().Project(linear);
  }

  /// <summary>
  /// Converts Lab to Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this LabF lab) {
    var linear = new LabFToLinearRgbF().Project(lab);
    return new LinearRgbaF(linear.R, linear.G, linear.B, 1f).ToColor();
  }

  /// <summary>
  /// Converts a Color to CIE Luv.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LuvF ToLuvF(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToLuvF().Project(linear);
  }

  /// <summary>
  /// Converts Luv to Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this LuvF luv) {
    var linear = new LuvFToLinearRgbF().Project(luv);
    return new LinearRgbaF(linear.R, linear.G, linear.B, 1f).ToColor();
  }

  #endregion

  #region Perceptual Color Spaces

  /// <summary>
  /// Converts a Color to Oklab (perceptually uniform).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OklabF ToOklabF(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToOklabF().Project(linear);
  }

  /// <summary>
  /// Converts Oklab to Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this OklabF oklab) {
    var linear = new OklabFToLinearRgbF().Project(oklab);
    return new LinearRgbaF(linear.R, linear.G, linear.B, 1f).ToColor();
  }

  /// <summary>
  /// Converts a Color to DIN99 (perceptually uniform).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Din99F ToDin99F(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToDin99F().Project(linear);
  }

  /// <summary>
  /// Converts DIN99 to Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this Din99F din99) {
    var linear = new Din99FToLinearRgbF().Project(din99);
    return new LinearRgbaF(linear.R, linear.G, linear.B, 1f).ToColor();
  }

  /// <summary>
  /// Converts a Color to Hunter Lab.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HunterLabF ToHunterLabF(this Color color) {
    var linear = color.ToLinearRgbaF();
    return new LinearRgbaFToHunterLabF().Project(linear);
  }

  /// <summary>
  /// Converts Hunter Lab to Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this HunterLabF hunterLab) {
    var linear = new HunterLabFToLinearRgbF().Project(hunterLab);
    return new LinearRgbaF(linear.R, linear.G, linear.B, 1f).ToColor();
  }

  #endregion

  #region Byte-Based Conversions

  /// <summary>
  /// Converts a Color to HSL with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte H, byte S, byte L) ToHslBytes(this Color color) => color.ToHslF().ToBytes();

  /// <summary>
  /// Converts a Color to HSV with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte H, byte S, byte V) ToHsvBytes(this Color color) => color.ToHsvF().ToBytes();

  /// <summary>
  /// Converts a Color to HWB with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte H, byte W, byte B) ToHwbBytes(this Color color) => color.ToHwbF().ToBytes();

  /// <summary>
  /// Converts a Color to LCh with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte L, byte C, byte H) ToLchBytes(this Color color) => color.ToLchF().ToBytes();

  /// <summary>
  /// Converts a Color to YUV with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte Y, byte U, byte V) ToYuvBytes(this Color color) => color.ToYuvF().ToBytes();

  /// <summary>
  /// Converts a Color to CIE XYZ with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte X, byte Y, byte Z) ToXyzBytes(this Color color) => color.ToXyzF().ToBytes();

  /// <summary>
  /// Converts a Color to CIE Lab with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte L, byte A, byte B) ToLabBytes(this Color color) => color.ToLabF().ToBytes();

  /// <summary>
  /// Converts a Color to CIE Luv with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte L, byte U, byte V) ToLuvBytes(this Color color) => color.ToLuvF().ToBytes();

  /// <summary>
  /// Converts a Color to Oklab with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte L, byte A, byte B) ToOklabBytes(this Color color) => color.ToOklabF().ToBytes();

  /// <summary>
  /// Converts a Color to DIN99 with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte L, byte A, byte B) ToDin99Bytes(this Color color) => color.ToDin99F().ToBytes();

  /// <summary>
  /// Converts a Color to Hunter Lab with byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte L, byte A, byte B) ToHunterLabBytes(this Color color) => color.ToHunterLabF().ToBytes();

  #endregion

  #region From Bytes Constructors

  /// <summary>
  /// Creates a Color from HSL byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ColorFromHslBytes(byte h, byte s, byte l)
    => new HslF(h * ColorConstants.ByteToFloat, s * ColorConstants.ByteToFloat, l * ColorConstants.ByteToFloat).ToColor();

  /// <summary>
  /// Creates a Color from HSV byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ColorFromHsvBytes(byte h, byte s, byte v)
    => new HsvF(h * ColorConstants.ByteToFloat, s * ColorConstants.ByteToFloat, v * ColorConstants.ByteToFloat).ToColor();

  /// <summary>
  /// Creates a Color from HWB byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ColorFromHwbBytes(byte h, byte w, byte b)
    => new HwbF(h * ColorConstants.ByteToFloat, w * ColorConstants.ByteToFloat, b * ColorConstants.ByteToFloat).ToColor();

  /// <summary>
  /// Creates a Color from LCh byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ColorFromLchBytes(byte l, byte c, byte h)
    => new LchF(l * ColorConstants.ByteToFloat * 100f, c * ColorConstants.ByteToFloat * 128f, h * ColorConstants.ByteToFloat).ToColor();

  /// <summary>
  /// Creates a Color from CIE Lab byte components (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ColorFromLabBytes(byte l, byte a, byte b)
    => new LabF(l * ColorConstants.ByteToFloat * 100f, a - 128f, b - 128f).ToColor();

  #endregion

}
