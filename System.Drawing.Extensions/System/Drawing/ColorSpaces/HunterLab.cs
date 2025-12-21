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

/// <summary>
/// Represents a color in the Hunter Lab color space.
/// </summary>
/// <remarks>
/// <para>Hunter Lab is a color space developed by Richard S. Hunter in 1948.</para>
/// <para>It was the predecessor to the CIELAB color space, using simpler formulas.</para>
/// <para>The space is designed to approximate human color perception.</para>
/// <para>L: Lightness (0 = black, 100 = white)</para>
/// <para>a: Red-green axis (negative = green, positive = red)</para>
/// <para>b: Yellow-blue axis (negative = blue, positive = yellow)</para>
/// <para>Uses D65 illuminant and assumes Illuminant C constants.</para>
/// </remarks>
[ColorSpace(3, ["L", "a", "b"], ColorSpaceType = ColorSpaceType.Perceptual, DisplayName = "Hunter Lab")]
public record struct HunterLab(float L, float A, float B, float Alpha = 1f) : IThreeComponentFloatColor {

  // Hunter Lab constants (for Illuminant C)
  private const float Ka = 17.5f;
  private const float Kb = 7.0f;

  // Reference white point for Illuminant C (approximation)
  // Using D65 values since sRGB uses D65
  private const float Xn = 0.95047f;
  private const float Yn = 1.00000f;
  private const float Zn = 1.08883f;

  // Adjustment factors for conversion
  private const float K1 = 1.02f;
  private const float K2 = 0.847f;

  // sRGB to XYZ D65 (standard matrix)
  private const float SRGB_TO_XYZ_XX = 0.4124564f;
  private const float SRGB_TO_XYZ_XY = 0.3575761f;
  private const float SRGB_TO_XYZ_XZ = 0.1804375f;
  private const float SRGB_TO_XYZ_YX = 0.2126729f;
  private const float SRGB_TO_XYZ_YY = 0.7151522f;
  private const float SRGB_TO_XYZ_YZ = 0.0721750f;
  private const float SRGB_TO_XYZ_ZX = 0.0193339f;
  private const float SRGB_TO_XYZ_ZY = 0.1191920f;
  private const float SRGB_TO_XYZ_ZZ = 0.9503041f;

  // XYZ D65 to sRGB (inverse)
  private const float XYZ_TO_SRGB_XX = 3.2404542f;
  private const float XYZ_TO_SRGB_XY = -1.5371385f;
  private const float XYZ_TO_SRGB_XZ = -0.4985314f;
  private const float XYZ_TO_SRGB_YX = -0.9692660f;
  private const float XYZ_TO_SRGB_YY = 1.8760108f;
  private const float XYZ_TO_SRGB_YZ = 0.0415560f;
  private const float XYZ_TO_SRGB_ZX = 0.0556434f;
  private const float XYZ_TO_SRGB_ZY = -0.2040259f;
  private const float XYZ_TO_SRGB_ZZ = 1.0572252f;

  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Hunter Lab to XYZ
    // L = 10 * sqrt(Y)  =>  Y = (L / 10)^2
    var y = (this.L / 10.0f) * (this.L / 10.0f);

    // Handle case where Y is very small to avoid division by zero
    if (y < 1e-6f)
      y = 1e-6f;

    var sqrtY = MathF.Sqrt(y);

    // a = Ka * (1.02X - Y) / sqrt(Y)  =>  X = (a * sqrt(Y) / Ka + Y) / 1.02
    var x = (this.A * sqrtY / Ka + y) / K1;

    // b = Kb * (Y - 0.847Z) / sqrt(Y)  =>  Z = (Y - b * sqrt(Y) / Kb) / 0.847
    var z = (y - this.B * sqrtY / Kb) / K2;

    // Convert XYZ to linear sRGB
    var rLinear = XYZ_TO_SRGB_XX * x + XYZ_TO_SRGB_XY * y + XYZ_TO_SRGB_XZ * z;
    var gLinear = XYZ_TO_SRGB_YX * x + XYZ_TO_SRGB_YY * y + XYZ_TO_SRGB_YZ * z;
    var bLinear = XYZ_TO_SRGB_ZX * x + XYZ_TO_SRGB_ZY * y + XYZ_TO_SRGB_ZZ * z;

    // Apply sRGB gamma
    var rSrgb = _LinearToSrgb(rLinear);
    var gSrgb = _LinearToSrgb(gLinear);
    var bSrgb = _LinearToSrgb(bLinear);

    return Color.FromArgb(
      _FloatToByte(this.Alpha),
      _FloatToByte(rSrgb),
      _FloatToByte(gSrgb),
      _FloatToByte(bSrgb)
    );
  }

  public static IColorSpace FromColor(Color color) {
    // Convert sRGB to linear sRGB
    var rLinear = _SrgbToLinear(color.R * Rgba32.ByteToNormalized);
    var gLinear = _SrgbToLinear(color.G * Rgba32.ByteToNormalized);
    var bLinear = _SrgbToLinear(color.B * Rgba32.ByteToNormalized);

    // Convert linear sRGB to XYZ
    var x = SRGB_TO_XYZ_XX * rLinear + SRGB_TO_XYZ_XY * gLinear + SRGB_TO_XYZ_XZ * bLinear;
    var y = SRGB_TO_XYZ_YX * rLinear + SRGB_TO_XYZ_YY * gLinear + SRGB_TO_XYZ_YZ * bLinear;
    var z = SRGB_TO_XYZ_ZX * rLinear + SRGB_TO_XYZ_ZY * gLinear + SRGB_TO_XYZ_ZZ * bLinear;

    // Handle case where Y is very small to avoid division by zero
    if (y < 1e-6f)
      y = 1e-6f;

    var sqrtY = MathF.Sqrt(y);

    // Convert XYZ to Hunter Lab
    // L = 10 * sqrt(Y)
    var l = 10.0f * sqrtY;

    // a = Ka * (1.02X - Y) / sqrt(Y)
    var a = Ka * (K1 * x - y) / sqrtY;

    // b = Kb * (Y - 0.847Z) / sqrt(Y)
    var b = Kb * (y - K2 * z) / sqrtY;

    return new HunterLab(l, a, b, color.A * Rgba32.ByteToNormalized);
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new HunterLab(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(HunterLab)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(HunterLab)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _SrgbToLinear(float x)
    => x <= 0.04045f ? x / 12.92f : MathF.Pow((x + 0.055f) / 1.055f, 2.4f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _LinearToSrgb(float x)
    => x <= 0.0031308f ? 12.92f * x : 1.055f * MathF.Pow(x, 1f / 2.4f) - 0.055f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _FloatToByte(float value) => (byte)Math.Round(Math.Max(0f, Math.Min(1f, value)) * 255f);
}
