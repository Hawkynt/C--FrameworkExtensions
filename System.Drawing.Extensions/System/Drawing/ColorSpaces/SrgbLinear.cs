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
/// Represents a color in the linear sRGB color space (sRGB without gamma correction).
/// </summary>
/// <remarks>
/// <para>Linear sRGB is the sRGB color space in its linear form, removing the gamma correction.</para>
/// <para>This is useful for physically accurate color operations like blending and compositing.</para>
/// <para>R: Red component (0-1, linear)</para>
/// <para>G: Green component (0-1, linear)</para>
/// <para>B: Blue component (0-1, linear)</para>
/// </remarks>
[ColorSpace(3, ["R", "G", "B"], ColorSpaceType = ColorSpaceType.Additive, DisplayName = "sRGB Linear")]
public record struct SrgbLinear(float R, float G, float B, float Alpha = 1f) : IThreeComponentFloatColor {

  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Convert linear sRGB to sRGB gamma
    var r = _LinearToSrgb(this.R);
    var g = _LinearToSrgb(this.G);
    var b = _LinearToSrgb(this.B);

    return Color.FromArgb(
      _FloatToByte(this.Alpha),
      _FloatToByte(r),
      _FloatToByte(g),
      _FloatToByte(b)
    );
  }

  public static IColorSpace FromColor(Color color) {
    // Convert sRGB to linear sRGB
    var r = _SrgbToLinear(color.R * Rgba32.ByteToNormalized);
    var g = _SrgbToLinear(color.G * Rgba32.ByteToNormalized);
    var b = _SrgbToLinear(color.B * Rgba32.ByteToNormalized);

    return new SrgbLinear(r, g, b, color.A * Rgba32.ByteToNormalized);
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new SrgbLinear(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(SrgbLinear)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(SrgbLinear)
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
