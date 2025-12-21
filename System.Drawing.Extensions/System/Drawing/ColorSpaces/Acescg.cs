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
/// Represents a color in the ACEScg (Academy Color Encoding System for Computer Graphics) color space.
/// </summary>
/// <remarks>
/// <para>ACEScg is a scene-linear color space designed for VFX and animation workflows.</para>
/// <para>Uses AP1 primaries (wider than sRGB) with D60 white point.</para>
/// <para>Scene-linear (no gamma encoding) for physically accurate light calculations.</para>
/// <para>L: Lightness (scene-linear, unbounded)</para>
/// <para>Cg1: First chromatic component</para>
/// <para>Cg2: Second chromatic component</para>
/// <para>Primaries (xy): Red(0.713, 0.293), Green(0.165, 0.830), Blue(0.128, 0.044)</para>
/// <para>White point: D60 (0.32168, 0.33767)</para>
/// </remarks>
[ColorSpace(3, ["L", "Cg1", "Cg2"], ColorSpaceType = ColorSpaceType.Additive, DisplayName = "ACEScg", WhitePoint = "D60")]
public record struct Acescg(float L, float Cg1, float Cg2, float Alpha = 1f) : IThreeComponentFloatColor {

  // XYZ D65 to XYZ D60 chromatic adaptation (Bradford)
  private const float D65_TO_D60_XX = 1.01303f;
  private const float D65_TO_D60_XY = 0.00610f;
  private const float D65_TO_D60_XZ = -0.01497f;
  private const float D65_TO_D60_YX = 0.00769f;
  private const float D65_TO_D60_YY = 0.99816f;
  private const float D65_TO_D60_YZ = -0.00503f;
  private const float D65_TO_D60_ZX = 0.00000f;
  private const float D65_TO_D60_ZY = 0.00000f;
  private const float D65_TO_D60_ZZ = 0.91822f;

  // XYZ D60 to XYZ D65 chromatic adaptation (inverse)
  private const float D60_TO_D65_XX = 0.98722f;
  private const float D60_TO_D65_XY = -0.00611f;
  private const float D60_TO_D65_XZ = 0.01596f;
  private const float D60_TO_D65_YX = -0.00759f;
  private const float D60_TO_D65_YY = 1.00186f;
  private const float D60_TO_D65_YZ = 0.00533f;
  private const float D60_TO_D65_ZX = 0.00000f;
  private const float D60_TO_D65_ZY = 0.00000f;
  private const float D60_TO_D65_ZZ = 1.08909f;

  // ACEScg (AP1) to XYZ D60
  private const float AP1_TO_XYZ_XX = 0.66245418f;
  private const float AP1_TO_XYZ_XY = 0.13400421f;
  private const float AP1_TO_XYZ_XZ = 0.15618769f;
  private const float AP1_TO_XYZ_YX = 0.27222872f;
  private const float AP1_TO_XYZ_YY = 0.67408177f;
  private const float AP1_TO_XYZ_YZ = 0.05368952f;
  private const float AP1_TO_XYZ_ZX = -0.00557465f;
  private const float AP1_TO_XYZ_ZY = 0.00406073f;
  private const float AP1_TO_XYZ_ZZ = 1.01033914f;

  // XYZ D60 to ACEScg (AP1)
  private const float XYZ_TO_AP1_XX = 1.64102338f;
  private const float XYZ_TO_AP1_XY = -0.32480329f;
  private const float XYZ_TO_AP1_XZ = -0.23642469f;
  private const float XYZ_TO_AP1_YX = -0.66366286f;
  private const float XYZ_TO_AP1_YY = 1.61533159f;
  private const float XYZ_TO_AP1_YZ = 0.01675635f;
  private const float XYZ_TO_AP1_ZX = 0.01172189f;
  private const float XYZ_TO_AP1_ZY = -0.00828444f;
  private const float XYZ_TO_AP1_ZZ = 0.98839486f;

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
    // ACEScg is scene-linear, so L, Cg1, Cg2 are the linear RGB values
    var rLinear = this.L;
    var gLinear = this.Cg1;
    var bLinear = this.Cg2;

    // Convert ACEScg (AP1) to XYZ D60
    var xd60 = AP1_TO_XYZ_XX * rLinear + AP1_TO_XYZ_XY * gLinear + AP1_TO_XYZ_XZ * bLinear;
    var yd60 = AP1_TO_XYZ_YX * rLinear + AP1_TO_XYZ_YY * gLinear + AP1_TO_XYZ_YZ * bLinear;
    var zd60 = AP1_TO_XYZ_ZX * rLinear + AP1_TO_XYZ_ZY * gLinear + AP1_TO_XYZ_ZZ * bLinear;

    // Chromatic adaptation from D60 to D65
    var xd65 = D60_TO_D65_XX * xd60 + D60_TO_D65_XY * yd60 + D60_TO_D65_XZ * zd60;
    var yd65 = D60_TO_D65_YX * xd60 + D60_TO_D65_YY * yd60 + D60_TO_D65_YZ * zd60;
    var zd65 = D60_TO_D65_ZX * xd60 + D60_TO_D65_ZY * yd60 + D60_TO_D65_ZZ * zd60;

    // Convert XYZ D65 to linear sRGB
    var rSrgbLinear = XYZ_TO_SRGB_XX * xd65 + XYZ_TO_SRGB_XY * yd65 + XYZ_TO_SRGB_XZ * zd65;
    var gSrgbLinear = XYZ_TO_SRGB_YX * xd65 + XYZ_TO_SRGB_YY * yd65 + XYZ_TO_SRGB_YZ * zd65;
    var bSrgbLinear = XYZ_TO_SRGB_ZX * xd65 + XYZ_TO_SRGB_ZY * yd65 + XYZ_TO_SRGB_ZZ * zd65;

    // Apply sRGB gamma
    var rSrgb = _LinearToSrgb(rSrgbLinear);
    var gSrgb = _LinearToSrgb(gSrgbLinear);
    var bSrgb = _LinearToSrgb(bSrgbLinear);

    return Color.FromArgb(
      _FloatToByte(this.Alpha),
      _FloatToByte(rSrgb),
      _FloatToByte(gSrgb),
      _FloatToByte(bSrgb)
    );
  }

  public static IColorSpace FromColor(Color color) {
    // Convert sRGB to linear sRGB
    var rSrgbLinear = _SrgbToLinear(color.R * Rgba32.ByteToNormalized);
    var gSrgbLinear = _SrgbToLinear(color.G * Rgba32.ByteToNormalized);
    var bSrgbLinear = _SrgbToLinear(color.B * Rgba32.ByteToNormalized);

    // Convert linear sRGB to XYZ D65
    var xd65 = SRGB_TO_XYZ_XX * rSrgbLinear + SRGB_TO_XYZ_XY * gSrgbLinear + SRGB_TO_XYZ_XZ * bSrgbLinear;
    var yd65 = SRGB_TO_XYZ_YX * rSrgbLinear + SRGB_TO_XYZ_YY * gSrgbLinear + SRGB_TO_XYZ_YZ * bSrgbLinear;
    var zd65 = SRGB_TO_XYZ_ZX * rSrgbLinear + SRGB_TO_XYZ_ZY * gSrgbLinear + SRGB_TO_XYZ_ZZ * bSrgbLinear;

    // Chromatic adaptation from D65 to D60
    var xd60 = D65_TO_D60_XX * xd65 + D65_TO_D60_XY * yd65 + D65_TO_D60_XZ * zd65;
    var yd60 = D65_TO_D60_YX * xd65 + D65_TO_D60_YY * yd65 + D65_TO_D60_YZ * zd65;
    var zd60 = D65_TO_D60_ZX * xd65 + D65_TO_D60_ZY * yd65 + D65_TO_D60_ZZ * zd65;

    // Convert XYZ D60 to ACEScg (AP1)
    var rLinear = XYZ_TO_AP1_XX * xd60 + XYZ_TO_AP1_XY * yd60 + XYZ_TO_AP1_XZ * zd60;
    var gLinear = XYZ_TO_AP1_YX * xd60 + XYZ_TO_AP1_YY * yd60 + XYZ_TO_AP1_YZ * zd60;
    var bLinear = XYZ_TO_AP1_ZX * xd60 + XYZ_TO_AP1_ZY * yd60 + XYZ_TO_AP1_ZZ * zd60;

    return new Acescg(rLinear, gLinear, bLinear, color.A * Rgba32.ByteToNormalized);
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new Acescg(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(Acescg)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Acescg)
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
