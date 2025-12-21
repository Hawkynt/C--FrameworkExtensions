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

/// <summary>CIE L*a*b* color space with byte components</summary>
[ColorSpace(3, ["L", "a", "b"], ColorSpaceType = ColorSpaceType.Perceptual, IsPerceptuallyUniform = true)]
public record struct Lab(byte L, byte A, byte B, byte Alpha = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToNormalized().ToColor();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LabNormalized ToNormalized() => new(
    this.L * (100f * Rgba32.ByteToNormalized),  // L: 0-255 -> 0-100
    this.A - 128f,                     // a: 0-255 -> -128 to 127
    this.B - 128f,                     // b: 0-255 -> -128 to 127
    this.Alpha * Rgba32.ByteToNormalized
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((LabNormalized)LabNormalized.FromColor(color)).ToByte();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Lab(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentColor
    => typeof(T) == typeof(Lab)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Lab)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}

/// <summary>CIE L*a*b* color space with normalized components (D65 illuminant)</summary>
[ColorSpace(3, ["L", "a", "b"], ColorSpaceType = ColorSpaceType.Perceptual, IsPerceptuallyUniform = true, WhitePoint = "D65")]
public record struct LabNormalized(float L, float A, float B, float Alpha = 1f) : IThreeComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  // D65 reference white point
  private const float Xn = 0.95047f;
  private const float Yn = 1.00000f;
  private const float Zn = 1.08883f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Lab to XYZ using fixed-point LUTs
    // fy = (L + 16) / 116, fx = a / 500 + fy, fz = fy - b / 200
    var fy = (this.L + 16f) / 116f;
    var fx = this.A / 500f + fy;
    var fz = fy - this.B / 200f;

    // Convert f values to 16.16 fixed-point (0-1 range maps to 0-65536)
    var fxFixed = (int)(fx * 65536f);
    var fyFixed = (int)(fy * 65536f);
    var fzFixed = (int)(fz * 65536f);

    // Apply Lab F inverse using LUT
    var xRatio = FixedPointMath.LabFInverse(fxFixed);
    var yRatio = FixedPointMath.LabFInverse(fyFixed);
    var zRatio = FixedPointMath.LabFInverse(fzFixed);

    // Scale by reference white point
    var x = (int)((long)xRatio * FixedPointMath.D65_Xn >> 16);
    var y = (int)((long)yRatio * FixedPointMath.D65_Yn >> 16);
    var z = (int)((long)zRatio * FixedPointMath.D65_Zn >> 16);

    // XYZ to linear RGB using fixed-point matrix
    var rl = (int)(((long)FixedPointMath.Rgb_RX * x + (long)FixedPointMath.Rgb_RY * y + (long)FixedPointMath.Rgb_RZ * z) >> 16);
    var gl = (int)(((long)FixedPointMath.Rgb_GX * x + (long)FixedPointMath.Rgb_GY * y + (long)FixedPointMath.Rgb_GZ * z) >> 16);
    var bl = (int)(((long)FixedPointMath.Rgb_BX * x + (long)FixedPointMath.Rgb_BY * y + (long)FixedPointMath.Rgb_BZ * z) >> 16);

    // Apply gamma compression using LUT
    var r = FixedPointMath.GammaCompress(rl);
    var g = FixedPointMath.GammaCompress(gl);
    var b = FixedPointMath.GammaCompress(bl);

    return Color.FromArgb((byte)(this.Alpha * Rgba32.NormalizedToByte + 0.5f), r, g, b);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Lab ToByte() => new(
    (byte)(Math.Min(Math.Max(this.L / 100f, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(Math.Min(Math.Max((this.A + 128f) * Rgba32.ByteToNormalized, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(Math.Min(Math.Max((this.B + 128f) * Rgba32.ByteToNormalized, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(this.Alpha * Rgba32.NormalizedToByte + 0.5f)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var c = new Rgba32(color);

    // Apply gamma expansion using LUT (sRGB to linear)
    var r = FixedPointMath.GammaExpand(c.R);
    var g = FixedPointMath.GammaExpand(c.G);
    var b = FixedPointMath.GammaExpand(c.B);

    // Linear RGB to XYZ using 16.16 fixed-point matrix
    var x = (int)(((long)FixedPointMath.Xyz_XR * r + (long)FixedPointMath.Xyz_XG * g + (long)FixedPointMath.Xyz_XB * b) >> 16);
    var y = (int)(((long)FixedPointMath.Xyz_YR * r + (long)FixedPointMath.Xyz_YG * g + (long)FixedPointMath.Xyz_YB * b) >> 16);
    var z = (int)(((long)FixedPointMath.Xyz_ZR * r + (long)FixedPointMath.Xyz_ZG * g + (long)FixedPointMath.Xyz_ZB * b) >> 16);

    // Normalize by reference white point (result in 16.16)
    // x/Xn, y/Yn, z/Zn - need to divide by D65 constants
    var xRatio = (int)(((long)x << 16) / FixedPointMath.D65_Xn);
    var yRatio = (int)(((long)y << 16) / FixedPointMath.D65_Yn);
    var zRatio = (int)(((long)z << 16) / FixedPointMath.D65_Zn);

    // Apply Lab F function using LUT
    var fx = FixedPointMath.LabF(xRatio);
    var fy = FixedPointMath.LabF(yRatio);
    var fz = FixedPointMath.LabF(zRatio);

    // Calculate Lab values
    // L = 116 * fy - 16
    // a = 500 * (fx - fy)
    // b = 200 * (fy - fz)
    var lVal = 116f * (fy / 65536f) - 16f;
    var aVal = 500f * ((fx - fy) / 65536f);
    var bVal = 200f * ((fy - fz) / 65536f);

    return new LabNormalized(lVal, aVal, bVal, c.ANormalized);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new LabNormalized(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(LabNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(LabNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}
