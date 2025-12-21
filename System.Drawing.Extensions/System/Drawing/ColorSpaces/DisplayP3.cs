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
/// Represents a color in the Display P3 color space.
/// </summary>
/// <remarks>
/// <para>Display P3 is Apple's wide-gamut display color space.</para>
/// <para>Uses DCI-P3 primaries with D65 white point and sRGB transfer function.</para>
/// <para>Provides approximately 25% more colors than sRGB.</para>
/// <para>R: Red component (0-1)</para>
/// <para>G: Green component (0-1)</para>
/// <para>B: Blue component (0-1)</para>
/// <para>Primaries (xy): Red(0.680, 0.320), Green(0.265, 0.690), Blue(0.150, 0.060)</para>
/// <para>White point: D65 (0.3127, 0.3290)</para>
/// </remarks>
[ColorSpace(3, ["R", "G", "B"], ColorSpaceType = ColorSpaceType.Additive, DisplayName = "Display P3", WhitePoint = "D65")]
public record struct DisplayP3(float R, float G, float B, float Alpha = 1f) : IThreeComponentFloatColor {

  // Display P3 to sRGB matrix (both use D65, so no chromatic adaptation needed)
  private const float P2S_RR = 1.2249f;
  private const float P2S_RG = -0.2247f;
  private const float P2S_RB = 0.0000f;
  private const float P2S_GR = -0.0420f;
  private const float P2S_GG = 1.0419f;
  private const float P2S_GB = 0.0000f;
  private const float P2S_BR = -0.0197f;
  private const float P2S_BG = -0.0786f;
  private const float P2S_BB = 1.0979f;

  // sRGB to Display P3 matrix (inverse of above)
  private const float S2P_RR = 0.8225f;
  private const float S2P_RG = 0.1774f;
  private const float S2P_RB = 0.0001f;
  private const float S2P_GR = 0.0332f;
  private const float S2P_GG = 0.9669f;
  private const float S2P_GB = 0.0000f;
  private const float S2P_BR = 0.0171f;
  private const float S2P_BG = 0.0724f;
  private const float S2P_BB = 0.9105f;

  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Apply inverse sRGB transfer function (P3 uses sRGB gamma)
    var rLinear = _SrgbToLinear(this.R);
    var gLinear = _SrgbToLinear(this.G);
    var bLinear = _SrgbToLinear(this.B);

    // Convert P3 linear to sRGB linear using matrix
    var rSrgbLinear = P2S_RR * rLinear + P2S_RG * gLinear + P2S_RB * bLinear;
    var gSrgbLinear = P2S_GR * rLinear + P2S_GG * gLinear + P2S_GB * bLinear;
    var bSrgbLinear = P2S_BR * rLinear + P2S_BG * gLinear + P2S_BB * bLinear;

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

    // Convert sRGB linear to P3 linear
    var rLinear = S2P_RR * rSrgbLinear + S2P_RG * gSrgbLinear + S2P_RB * bSrgbLinear;
    var gLinear = S2P_GR * rSrgbLinear + S2P_GG * gSrgbLinear + S2P_GB * bSrgbLinear;
    var bLinear = S2P_BR * rSrgbLinear + S2P_BG * gSrgbLinear + S2P_BB * bSrgbLinear;

    // Apply sRGB transfer function (P3 uses sRGB gamma)
    var rP3 = _LinearToSrgb(rLinear);
    var gP3 = _LinearToSrgb(gLinear);
    var bP3 = _LinearToSrgb(bLinear);

    return new DisplayP3(rP3, gP3, bP3, color.A * Rgba32.ByteToNormalized);
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new DisplayP3(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(DisplayP3)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(DisplayP3)
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
