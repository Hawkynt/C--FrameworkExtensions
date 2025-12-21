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

/// <summary>CIE L*u*v* color space with byte components</summary>
public record struct Luv(byte L, byte U, byte V, byte A = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToNormalized().ToColor();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LuvNormalized ToNormalized() => new(
    this.L * (100f * Rgba32.ByteToNormalized),  // L: 0-255 -> 0-100
    this.U * (354f * Rgba32.ByteToNormalized) - 134f,  // u: 0-255 -> -134 to 220
    this.V * (262f * Rgba32.ByteToNormalized) - 140f,  // v: 0-255 -> -140 to 122
    this.A * Rgba32.ByteToNormalized
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((LuvNormalized)LuvNormalized.FromColor(color)).ToByte();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Luv(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentColor
    => typeof(T) == typeof(Luv)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Luv)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}

/// <summary>CIE L*u*v* color space with normalized components (D65 illuminant)</summary>
public record struct LuvNormalized(float L, float U, float V, float A = 1f) : IThreeComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  // D65 reference white point
  private const float Xn = 0.95047f;
  private const float Yn = 1.00000f;
  private const float Zn = 1.08883f;

  // Pre-calculated u'n and v'n for D65
  private const float Un = 4f * Xn / (Xn + 15f * Yn + 3f * Zn);  // ≈ 0.19783691
  private const float Vn = 9f * Yn / (Xn + 15f * Yn + 3f * Zn);  // ≈ 0.46831999

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Luv to XYZ conversion
    // If L = 0, color is black
    if (this.L <= 0f)
      return Color.FromArgb((byte)(this.A * Rgba32.NormalizedToByte + 0.5f), 0, 0, 0);

    // Y = Yn * ((L + 16) / 116)^3 if L > 8, else Y = Yn * L * (3/29)^3
    float y;
    if (this.L > 8f) {
      var yRatio = (this.L + 16f) / 116f;
      y = Yn * yRatio * yRatio * yRatio;
    } else
      y = Yn * this.L * 0.00110705646f;  // (3/29)^3

    // Calculate u' and v' from u*, v*
    // u' = u / (13L) + un, v' = v / (13L) + vn
    var u = this.U / (13f * this.L) + Un;
    var v = this.V / (13f * this.L) + Vn;

    // Calculate X and Z from Y, u', v'
    // X = Y * (9u') / (4v'), Z = Y * (12 - 3u' - 20v') / (4v')
    var x = y * (9f * u) / (4f * v);
    var z = y * (12f - 3f * u - 20f * v) / (4f * v);

    // Convert to 16.16 fixed-point
    var xFixed = (int)(x * FixedPointMath.One);
    var yFixed = (int)(y * FixedPointMath.One);
    var zFixed = (int)(z * FixedPointMath.One);

    // XYZ to linear RGB using fixed-point matrix
    var rl = (int)(((long)FixedPointMath.Rgb_RX * xFixed + (long)FixedPointMath.Rgb_RY * yFixed + (long)FixedPointMath.Rgb_RZ * zFixed) >> 16);
    var gl = (int)(((long)FixedPointMath.Rgb_GX * xFixed + (long)FixedPointMath.Rgb_GY * yFixed + (long)FixedPointMath.Rgb_GZ * zFixed) >> 16);
    var bl = (int)(((long)FixedPointMath.Rgb_BX * xFixed + (long)FixedPointMath.Rgb_BY * yFixed + (long)FixedPointMath.Rgb_BZ * zFixed) >> 16);

    // Apply gamma compression using LUT
    var r = FixedPointMath.GammaCompress(rl);
    var g = FixedPointMath.GammaCompress(gl);
    var b = FixedPointMath.GammaCompress(bl);

    return Color.FromArgb((byte)(this.A * Rgba32.NormalizedToByte + 0.5f), r, g, b);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Luv ToByte() => new(
    (byte)(Math.Min(Math.Max(this.L / 100f, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(Math.Min(Math.Max((this.U + 134f) / 354f, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(Math.Min(Math.Max((this.V + 140f) / 262f, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(this.A * Rgba32.NormalizedToByte + 0.5f)
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

    // Convert to float
    var xVal = x / 65536f;
    var yVal = y / 65536f;
    var zVal = z / 65536f;

    // Calculate denominator for u' and v'
    var denom = xVal + 15f * yVal + 3f * zVal;

    // Handle black (all zeros)
    if (denom < 1e-10f)
      return new LuvNormalized(0f, 0f, 0f, c.ANormalized);

    // Calculate u' and v'
    var u = (4f * xVal) / denom;
    var v = (9f * yVal) / denom;

    // Calculate L*
    var yRatio = yVal / Yn;
    float lVal;
    if (yRatio > 0.008856f)  // (6/29)^3
      lVal = 116f * (float)Math.Pow(yRatio, 1.0 / 3.0) - 16f;
    else
      lVal = 903.3f * yRatio;

    // Calculate u* and v*
    // u* = 13L*(u' - u'n), v* = 13L*(v' - v'n)
    var uVal = 13f * lVal * (u - Un);
    var vVal = 13f * lVal * (v - Vn);

    return new LuvNormalized(lVal, uVal, vVal, c.ANormalized);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new LuvNormalized(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(LuvNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(LuvNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}
