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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.ColorSpaces;

/// <summary>CIE XYZ color space with byte components (scaled 0-255)</summary>
/// <param name="X">X: 0-255 (scaled from ~0-0.95)</param>
/// <param name="Y">Y: 0-255 (scaled from 0-1)</param>
/// <param name="Z">Z: 0-255 (scaled from ~0-1.09)</param>
/// <param name="A">Alpha: 0-255</param>
public record struct Xyz(byte X, byte Y, byte Z, byte A = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToNormalized().ToColor();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XyzNormalized ToNormalized() => new(this.X * Rgba32.ByteToNormalized, this.Y * Rgba32.ByteToNormalized, this.Z * Rgba32.ByteToNormalized, this.A * Rgba32.ByteToNormalized);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((XyzNormalized)XyzNormalized.FromColor(color)).ToByte();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Xyz(c1, c2, c3, a);

}

/// <summary>CIE 1931 XYZ color space with normalized components (sRGB, D65 illuminant)</summary>
/// <param name="X">X: typically 0.0-0.95047</param>
/// <param name="Y">Y: 0.0-1.0 (luminance)</param>
/// <param name="Z">Z: typically 0.0-1.08883</param>
/// <param name="A">Alpha: 0.0-1.0</param>
public record struct XyzNormalized(float X, float Y, float Z, float A = 1f) : IThreeComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  // D65 reference white point
  private const float Xn = 0.95047f;
  private const float Yn = 1.00000f;
  private const float Zn = 1.08883f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // XYZ to linear RGB using 16.16 fixed-point matrix multiplication
    // Convert XYZ to 16.16 format
    var x = (int)(this.X * 65536f);
    var y = (int)(this.Y * 65536f);
    var z = (int)(this.Z * 65536f);

    // Matrix multiplication in fixed-point
    var rl = (int)(((long)FixedPointMath.Rgb_RX * x + (long)FixedPointMath.Rgb_RY * y + (long)FixedPointMath.Rgb_RZ * z) >> 16);
    var gl = (int)(((long)FixedPointMath.Rgb_GX * x + (long)FixedPointMath.Rgb_GY * y + (long)FixedPointMath.Rgb_GZ * z) >> 16);
    var bl = (int)(((long)FixedPointMath.Rgb_BX * x + (long)FixedPointMath.Rgb_BY * y + (long)FixedPointMath.Rgb_BZ * z) >> 16);

    // Apply gamma compression using LUT
    // First clamp to 0-65536 range, then use LUT
    var r = FixedPointMath.GammaCompress(rl);
    var g = FixedPointMath.GammaCompress(gl);
    var b = FixedPointMath.GammaCompress(bl);

    return Color.FromArgb((byte)(this.A * 255f + 0.5f), r, g, b);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Xyz ToByte() => new(
    (byte)(Math.Min(this.X / Xn, 1f) * 255f + 0.5f),
    (byte)(Math.Min(this.Y / Yn, 1f) * 255f + 0.5f),
    (byte)(Math.Min(this.Z / Zn, 1f) * 255f + 0.5f),
    (byte)(this.A * 255f + 0.5f)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var c = new Rgba32(color);

    // Apply gamma expansion using LUT (sRGB to linear)
    var r = FixedPointMath.GammaExpand(c.R);
    var g = FixedPointMath.GammaExpand(c.G);
    var b = FixedPointMath.GammaExpand(c.B);

    // Linear RGB to XYZ using 16.16 fixed-point matrix multiplication
    var x = (int)(((long)FixedPointMath.Xyz_XR * r + (long)FixedPointMath.Xyz_XG * g + (long)FixedPointMath.Xyz_XB * b) >> 16);
    var y = (int)(((long)FixedPointMath.Xyz_YR * r + (long)FixedPointMath.Xyz_YG * g + (long)FixedPointMath.Xyz_YB * b) >> 16);
    var z = (int)(((long)FixedPointMath.Xyz_ZR * r + (long)FixedPointMath.Xyz_ZG * g + (long)FixedPointMath.Xyz_ZB * b) >> 16);

    // Convert from 16.16 to float
    return new XyzNormalized(x / 65536f, y / 65536f, z / 65536f, c.ANormalized);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new XyzNormalized(c1, c2, c3, a);

}
