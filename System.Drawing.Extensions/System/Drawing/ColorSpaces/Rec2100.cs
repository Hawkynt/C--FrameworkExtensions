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
/// Represents a color in the Rec.2100 color space with PQ (Perceptual Quantizer) transfer function.
/// </summary>
/// <remarks>
/// <para>Rec.2100 is the ITU-R Recommendation BT.2100 for HDR television.</para>
/// <para>Uses Rec.2020 primaries with D65 white point.</para>
/// <para>PQ (SMPTE ST 2084) transfer function provides perceptually uniform encoding for HDR content up to 10,000 nits.</para>
/// <para>R: Red component (0-1, PQ-encoded)</para>
/// <para>G: Green component (0-1, PQ-encoded)</para>
/// <para>B: Blue component (0-1, PQ-encoded)</para>
/// <para>Reference: ITU-R BT.2100, SMPTE ST 2084</para>
/// </remarks>
[ColorSpace(3, ["R", "G", "B"], ColorSpaceType = ColorSpaceType.Additive, DisplayName = "Rec.2100 PQ", WhitePoint = "D65")]
public record struct Rec2100(float R, float G, float B, float Alpha = 1f) : IThreeComponentFloatColor {

  // PQ (Perceptual Quantizer) constants
  private const float M1 = 0.1593017578125f;           // 2610/16384
  private const float M2 = 78.84375f;                  // 2523/32
  private const float C1 = 0.8359375f;                 // 3424/4096
  private const float C2 = 18.8515625f;                // 2413/128
  private const float C3 = 18.6875f;                   // 2392/128

  // Rec.2020 to sRGB matrix (D65 white point)
  private const float R2S_RR = 1.6605f;
  private const float R2S_RG = -0.5876f;
  private const float R2S_RB = -0.0728f;
  private const float R2S_GR = -0.1246f;
  private const float R2S_GG = 1.1329f;
  private const float R2S_GB = -0.0083f;
  private const float R2S_BR = -0.0182f;
  private const float R2S_BG = -0.1006f;
  private const float R2S_BB = 1.1187f;

  // sRGB to Rec.2020 matrix (inverse of above)
  private const float S2R_RR = 0.6274f;
  private const float S2R_RG = 0.3293f;
  private const float S2R_RB = 0.0433f;
  private const float S2R_GR = 0.0691f;
  private const float S2R_GG = 0.9195f;
  private const float S2R_GB = 0.0114f;
  private const float S2R_BR = 0.0164f;
  private const float S2R_BG = 0.0880f;
  private const float S2R_BB = 0.8956f;

  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Apply inverse PQ transfer function to get linear Rec.2020
    var rLinear = _InversePq(this.R);
    var gLinear = _InversePq(this.G);
    var bLinear = _InversePq(this.B);

    // Convert Rec.2020 linear to sRGB linear
    var rSrgbLinear = R2S_RR * rLinear + R2S_RG * gLinear + R2S_RB * bLinear;
    var gSrgbLinear = R2S_GR * rLinear + R2S_GG * gLinear + R2S_GB * bLinear;
    var bSrgbLinear = R2S_BR * rLinear + R2S_BG * gLinear + R2S_BB * bLinear;

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

    // Convert sRGB linear to Rec.2020 linear
    var rLinear = S2R_RR * rSrgbLinear + S2R_RG * gSrgbLinear + S2R_RB * bSrgbLinear;
    var gLinear = S2R_GR * rSrgbLinear + S2R_GG * gSrgbLinear + S2R_GB * bSrgbLinear;
    var bLinear = S2R_BR * rSrgbLinear + S2R_BG * gSrgbLinear + S2R_BB * bSrgbLinear;

    // Apply PQ transfer function
    var rPq = _Pq(rLinear);
    var gPq = _Pq(gLinear);
    var bPq = _Pq(bLinear);

    return new Rec2100(rPq, gPq, bPq, color.A * Rgba32.ByteToNormalized);
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new Rec2100(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(Rec2100)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Rec2100)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Pq(float y) {
    if (y <= 0)
      return 0;

    var yn = MathF.Pow(y, M1);
    return MathF.Pow((C1 + C2 * yn) / (1 + C3 * yn), M2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _InversePq(float n) {
    if (n <= 0)
      return 0;

    var np = MathF.Pow(n, 1f / M2);
    return MathF.Pow(Math.Max(0, np - C1) / (C2 - C3 * np), 1f / M1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _SrgbToLinear(float x)
    => x <= 0.04045f ? x / 12.92f : MathF.Pow((x + 0.055f) / 1.055f, 2.4f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _LinearToSrgb(float x)
    => x <= 0.0031308f ? 12.92f * x : 1.055f * MathF.Pow(x, 1f / 2.4f) - 0.055f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _FloatToByte(float value) => (byte)Math.Round(Math.Max(0f, Math.Min(1f, value)) * 255f);
}
