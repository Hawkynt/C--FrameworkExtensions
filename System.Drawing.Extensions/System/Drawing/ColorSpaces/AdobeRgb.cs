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
/// Represents a color in the Adobe RGB (1998) color space.
/// </summary>
/// <remarks>
/// <para>Adobe RGB is a wide-gamut RGB color space developed by Adobe Systems in 1998.</para>
/// <para>It provides approximately 50% larger gamut than sRGB, especially in cyan-green colors.</para>
/// <para>Uses simple gamma 2.2 transfer function (unlike sRGB's piecewise function).</para>
/// <para>R: Red component (0-1)</para>
/// <para>G: Green component (0-1)</para>
/// <para>B: Blue component (0-1)</para>
/// <para>Primaries (xy): Red(0.6400, 0.3300), Green(0.2100, 0.7100), Blue(0.1500, 0.0600)</para>
/// <para>White point: D65 (0.3127, 0.3290)</para>
/// </remarks>
[ColorSpace(3, ["R", "G", "B"], ColorSpaceType = ColorSpaceType.Additive, DisplayName = "Adobe RGB", WhitePoint = "D65")]
public record struct AdobeRgb(float R, float G, float B, float Alpha = 1f) : IThreeComponentFloatColor {

  private const float Gamma = 2.2f;
  private const float InverseGamma = 1.0f / 2.2f;

  // Adobe RGB to XYZ D65 matrix
  // Calculated from Adobe RGB primaries:
  // R: (0.6400, 0.3300), G: (0.2100, 0.7100), B: (0.1500, 0.0600)
  // White point: D65
  private const float ADOBE_TO_XYZ_XX = 0.5767309f;
  private const float ADOBE_TO_XYZ_XY = 0.1855540f;
  private const float ADOBE_TO_XYZ_XZ = 0.1881852f;
  private const float ADOBE_TO_XYZ_YX = 0.2973769f;
  private const float ADOBE_TO_XYZ_YY = 0.6273491f;
  private const float ADOBE_TO_XYZ_YZ = 0.0752741f;
  private const float ADOBE_TO_XYZ_ZX = 0.0270343f;
  private const float ADOBE_TO_XYZ_ZY = 0.0706872f;
  private const float ADOBE_TO_XYZ_ZZ = 0.9911085f;

  // XYZ D65 to Adobe RGB matrix (inverse)
  private const float XYZ_TO_ADOBE_XX = 2.0413690f;
  private const float XYZ_TO_ADOBE_XY = -0.5649464f;
  private const float XYZ_TO_ADOBE_XZ = -0.3446944f;
  private const float XYZ_TO_ADOBE_YX = -0.9692660f;
  private const float XYZ_TO_ADOBE_YY = 1.8760108f;
  private const float XYZ_TO_ADOBE_YZ = 0.0415560f;
  private const float XYZ_TO_ADOBE_ZX = 0.0134474f;
  private const float XYZ_TO_ADOBE_ZY = -0.1183897f;
  private const float XYZ_TO_ADOBE_ZZ = 1.0154096f;

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
    // Apply inverse Adobe RGB gamma (simple power function)
    var rLinear = _AdobeToLinear(this.R);
    var gLinear = _AdobeToLinear(this.G);
    var bLinear = _AdobeToLinear(this.B);

    // Convert Adobe RGB linear to XYZ D65
    var x = ADOBE_TO_XYZ_XX * rLinear + ADOBE_TO_XYZ_XY * gLinear + ADOBE_TO_XYZ_XZ * bLinear;
    var y = ADOBE_TO_XYZ_YX * rLinear + ADOBE_TO_XYZ_YY * gLinear + ADOBE_TO_XYZ_YZ * bLinear;
    var z = ADOBE_TO_XYZ_ZX * rLinear + ADOBE_TO_XYZ_ZY * gLinear + ADOBE_TO_XYZ_ZZ * bLinear;

    // Convert XYZ D65 to linear sRGB
    var rSrgbLinear = XYZ_TO_SRGB_XX * x + XYZ_TO_SRGB_XY * y + XYZ_TO_SRGB_XZ * z;
    var gSrgbLinear = XYZ_TO_SRGB_YX * x + XYZ_TO_SRGB_YY * y + XYZ_TO_SRGB_YZ * z;
    var bSrgbLinear = XYZ_TO_SRGB_ZX * x + XYZ_TO_SRGB_ZY * y + XYZ_TO_SRGB_ZZ * z;

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
    var x = SRGB_TO_XYZ_XX * rSrgbLinear + SRGB_TO_XYZ_XY * gSrgbLinear + SRGB_TO_XYZ_XZ * bSrgbLinear;
    var y = SRGB_TO_XYZ_YX * rSrgbLinear + SRGB_TO_XYZ_YY * gSrgbLinear + SRGB_TO_XYZ_YZ * bSrgbLinear;
    var z = SRGB_TO_XYZ_ZX * rSrgbLinear + SRGB_TO_XYZ_ZY * gSrgbLinear + SRGB_TO_XYZ_ZZ * bSrgbLinear;

    // Convert XYZ D65 to Adobe RGB linear
    var rLinear = XYZ_TO_ADOBE_XX * x + XYZ_TO_ADOBE_XY * y + XYZ_TO_ADOBE_XZ * z;
    var gLinear = XYZ_TO_ADOBE_YX * x + XYZ_TO_ADOBE_YY * y + XYZ_TO_ADOBE_YZ * z;
    var bLinear = XYZ_TO_ADOBE_ZX * x + XYZ_TO_ADOBE_ZY * y + XYZ_TO_ADOBE_ZZ * z;

    // Apply Adobe RGB gamma (simple power function)
    var rAdobe = _LinearToAdobe(rLinear);
    var gAdobe = _LinearToAdobe(gLinear);
    var bAdobe = _LinearToAdobe(bLinear);

    return new AdobeRgb(rAdobe, gAdobe, bAdobe, color.A * Rgba32.ByteToNormalized);
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new AdobeRgb(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(AdobeRgb)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(AdobeRgb)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _SrgbToLinear(float x)
    => x <= 0.04045f ? x / 12.92f : MathF.Pow((x + 0.055f) / 1.055f, 2.4f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _LinearToSrgb(float x)
    => x <= 0.0031308f ? 12.92f * x : 1.055f * MathF.Pow(x, 1f / 2.4f) - 0.055f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _AdobeToLinear(float x) => MathF.Pow(x, Gamma);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _LinearToAdobe(float x) => MathF.Pow(Math.Max(0f, x), InverseGamma);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _FloatToByte(float value) => (byte)Math.Round(Math.Max(0f, Math.Min(1f, value)) * 255f);
}
