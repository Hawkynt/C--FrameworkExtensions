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
/// Represents a color in the ProPhoto RGB color space (ROMM RGB).
/// </summary>
/// <remarks>
/// <para>ProPhoto RGB (Reference Output Medium Metric RGB) is a wide-gamut color space developed by Kodak.</para>
/// <para>Designed for photographic applications, it encompasses approximately 90% of possible surface colors in CIE L*a*b*.</para>
/// <para>Uses D50 white point and a gamma 1.8 transfer function.</para>
/// <para>R: Red component (0-1)</para>
/// <para>G: Green component (0-1)</para>
/// <para>B: Blue component (0-1)</para>
/// <para>Primaries (xy): Red(0.7347, 0.2653), Green(0.1596, 0.8404), Blue(0.0366, 0.0001)</para>
/// <para>White point: D50 (0.3457, 0.3585)</para>
/// </remarks>
[ColorSpace(3, ["R", "G", "B"], ColorSpaceType = ColorSpaceType.Additive, DisplayName = "ProPhoto RGB", WhitePoint = "D50")]
public record struct ProPhotoRgb(float R, float G, float B, float Alpha = 1f) : IThreeComponentFloatColor {

  private const float Et = 1f / 512f;
  private const float Gamma = 1.8f;

  // Bradford chromatic adaptation matrix from D50 to D65
  private const float D50_TO_D65_RR = 0.9555766f;
  private const float D50_TO_D65_RG = -0.0230393f;
  private const float D50_TO_D65_RB = 0.0631636f;
  private const float D50_TO_D65_GR = -0.0282895f;
  private const float D50_TO_D65_GG = 1.0099416f;
  private const float D50_TO_D65_GB = 0.0210077f;
  private const float D50_TO_D65_BR = 0.0122982f;
  private const float D50_TO_D65_BG = -0.0204830f;
  private const float D50_TO_D65_BB = 1.3299098f;

  // Bradford chromatic adaptation matrix from D65 to D50
  private const float D65_TO_D50_RR = 1.0478112f;
  private const float D65_TO_D50_RG = 0.0228866f;
  private const float D65_TO_D50_RB = -0.0501270f;
  private const float D65_TO_D50_GR = 0.0295424f;
  private const float D65_TO_D50_GG = 0.9904844f;
  private const float D65_TO_D50_GB = -0.0170491f;
  private const float D65_TO_D50_BR = -0.0092345f;
  private const float D65_TO_D50_BG = 0.0150436f;
  private const float D65_TO_D50_BB = 0.7521316f;

  // ProPhoto RGB to XYZ (D50) matrix
  private const float P2X_RR = 0.7976749f;
  private const float P2X_RG = 0.1351917f;
  private const float P2X_RB = 0.0313534f;
  private const float P2X_GR = 0.2880402f;
  private const float P2X_GG = 0.7118741f;
  private const float P2X_GB = 0.0000857f;
  private const float P2X_BR = 0.0000000f;
  private const float P2X_BG = 0.0000000f;
  private const float P2X_BB = 0.8252100f;

  // XYZ (D50) to ProPhoto RGB matrix
  private const float X2P_RR = 1.3459433f;
  private const float X2P_RG = -0.2556075f;
  private const float X2P_RB = -0.0511118f;
  private const float X2P_GR = -0.5445989f;
  private const float X2P_GG = 1.5081673f;
  private const float X2P_GB = 0.0205351f;
  private const float X2P_BR = 0.0000000f;
  private const float X2P_BG = 0.0000000f;
  private const float X2P_BB = 1.2118128f;

  // sRGB to XYZ (D65) matrix
  private const float S2X_RR = 0.4124564f;
  private const float S2X_RG = 0.3575761f;
  private const float S2X_RB = 0.1804375f;
  private const float S2X_GR = 0.2126729f;
  private const float S2X_GG = 0.7151522f;
  private const float S2X_GB = 0.0721750f;
  private const float S2X_BR = 0.0193339f;
  private const float S2X_BG = 0.1191920f;
  private const float S2X_BB = 0.9503041f;

  // XYZ (D65) to sRGB matrix
  private const float X2S_RR = 3.2404542f;
  private const float X2S_RG = -1.5371385f;
  private const float X2S_RB = -0.4985314f;
  private const float X2S_GR = -0.9692660f;
  private const float X2S_GG = 1.8760108f;
  private const float X2S_GB = 0.0415560f;
  private const float X2S_BR = 0.0556434f;
  private const float X2S_BG = -0.2040259f;
  private const float X2S_BB = 1.0572252f;

  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Apply inverse ProPhoto RGB transfer function
    var rLinear = _ProPhotoToLinear(this.R);
    var gLinear = _ProPhotoToLinear(this.G);
    var bLinear = _ProPhotoToLinear(this.B);

    // Convert ProPhoto RGB linear to XYZ (D50)
    var x = P2X_RR * rLinear + P2X_RG * gLinear + P2X_RB * bLinear;
    var y = P2X_GR * rLinear + P2X_GG * gLinear + P2X_GB * bLinear;
    var z = P2X_BR * rLinear + P2X_BG * gLinear + P2X_BB * bLinear;

    // Chromatic adaptation from D50 to D65
    var xD65 = D50_TO_D65_RR * x + D50_TO_D65_RG * y + D50_TO_D65_RB * z;
    var yD65 = D50_TO_D65_GR * x + D50_TO_D65_GG * y + D50_TO_D65_GB * z;
    var zD65 = D50_TO_D65_BR * x + D50_TO_D65_BG * y + D50_TO_D65_BB * z;

    // Convert XYZ (D65) to sRGB linear
    var rSrgbLinear = X2S_RR * xD65 + X2S_RG * yD65 + X2S_RB * zD65;
    var gSrgbLinear = X2S_GR * xD65 + X2S_GG * yD65 + X2S_GB * zD65;
    var bSrgbLinear = X2S_BR * xD65 + X2S_BG * yD65 + X2S_BB * zD65;

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

    // Convert sRGB linear to XYZ (D65)
    var xD65 = S2X_RR * rSrgbLinear + S2X_RG * gSrgbLinear + S2X_RB * bSrgbLinear;
    var yD65 = S2X_GR * rSrgbLinear + S2X_GG * gSrgbLinear + S2X_GB * bSrgbLinear;
    var zD65 = S2X_BR * rSrgbLinear + S2X_BG * gSrgbLinear + S2X_BB * bSrgbLinear;

    // Chromatic adaptation from D65 to D50
    var x = D65_TO_D50_RR * xD65 + D65_TO_D50_RG * yD65 + D65_TO_D50_RB * zD65;
    var y = D65_TO_D50_GR * xD65 + D65_TO_D50_GG * yD65 + D65_TO_D50_GB * zD65;
    var z = D65_TO_D50_BR * xD65 + D65_TO_D50_BG * yD65 + D65_TO_D50_BB * zD65;

    // Convert XYZ (D50) to ProPhoto RGB linear
    var rLinear = X2P_RR * x + X2P_RG * y + X2P_RB * z;
    var gLinear = X2P_GR * x + X2P_GG * y + X2P_GB * z;
    var bLinear = X2P_BR * x + X2P_BG * y + X2P_BB * z;

    // Apply ProPhoto RGB transfer function
    var rProPhoto = _LinearToProPhoto(rLinear);
    var gProPhoto = _LinearToProPhoto(gLinear);
    var bProPhoto = _LinearToProPhoto(bLinear);

    return new ProPhotoRgb(rProPhoto, gProPhoto, bProPhoto, color.A * Rgba32.ByteToNormalized);
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new ProPhotoRgb(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(ProPhotoRgb)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(ProPhotoRgb)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _LinearToProPhoto(float x)
    => x >= Et ? MathF.Pow(x, 1f / Gamma) : 16f * x;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _ProPhotoToLinear(float x)
    => x >= 16f * Et ? MathF.Pow(x, Gamma) : x / 16f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _SrgbToLinear(float x)
    => x <= 0.04045f ? x / 12.92f : MathF.Pow((x + 0.055f) / 1.055f, 2.4f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _LinearToSrgb(float x)
    => x <= 0.0031308f ? 12.92f * x : 1.055f * MathF.Pow(x, 1f / 2.4f) - 0.055f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _FloatToByte(float value) => (byte)Math.Round(Math.Max(0f, Math.Min(1f, value)) * 255f);
}
