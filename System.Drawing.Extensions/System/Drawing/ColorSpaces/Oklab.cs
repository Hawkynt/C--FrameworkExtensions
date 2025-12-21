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
/// Represents a color in the Oklab perceptual color space.
/// </summary>
/// <remarks>
/// <para>Oklab is a perceptually uniform color space designed by Björn Ottosson in 2020.</para>
/// <para>It provides excellent results for color interpolation, gradients, and color manipulation.</para>
/// <para>L: Perceived lightness (0 = black, 1 = white)</para>
/// <para>a: Green-red axis (-1 = green, +1 = red)</para>
/// <para>b: Blue-yellow axis (-1 = blue, +1 = yellow)</para>
/// <para>Reference: https://bottosson.github.io/posts/oklab/</para>
/// </remarks>
[ColorSpace(3, ["L", "a", "b"], ColorSpaceType = ColorSpaceType.Perceptual, IsPerceptuallyUniform = true, DisplayName = "Oklab")]
public record struct Oklab(float L, float A, float B, float Alpha = 1f) : IThreeComponentFloatColor {

  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Convert Oklab to linear sRGB
    var l_ = this.L + 0.3963377774f * this.A + 0.2158037573f * this.B;
    var m_ = this.L - 0.1055613458f * this.A - 0.0638541728f * this.B;
    var s_ = this.L - 0.0894841775f * this.A - 1.2914855480f * this.B;

    var l = l_ * l_ * l_;
    var m = m_ * m_ * m_;
    var s = s_ * s_ * s_;

    var r = +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
    var g = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
    var b = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

    // Convert linear sRGB to sRGB
    r = _LinearToSrgb(r);
    g = _LinearToSrgb(g);
    b = _LinearToSrgb(b);

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

    // Convert linear sRGB to Oklab
    var l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
    var m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
    var s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;

    var l_ = _Cbrt(l);
    var m_ = _Cbrt(m);
    var s_ = _Cbrt(s);

    return new Oklab(
      0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_,
      1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_,
      0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_,
      color.A * Rgba32.ByteToNormalized
    );
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new Oklab(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(Oklab)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Oklab)
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Cbrt(float x) => x >= 0 ? MathF.Pow(x, 1f / 3f) : -MathF.Pow(-x, 1f / 3f);
}

/// <summary>
/// Represents a color in the Oklch cylindrical color space (Oklab in polar form).
/// </summary>
/// <remarks>
/// <para>Oklch is the cylindrical representation of Oklab.</para>
/// <para>L: Perceived lightness (0 = black, 1 = white)</para>
/// <para>C: Chroma (colorfulness, 0 = gray)</para>
/// <para>h: Hue angle in degrees (0-360)</para>
/// <para>Reference: https://bottosson.github.io/posts/oklab/</para>
/// </remarks>
[ColorSpace(3, ["L", "C", "h"], ColorSpaceType = ColorSpaceType.Cylindrical, IsPerceptuallyUniform = true, DisplayName = "Oklch")]
public record struct Oklch(float L, float C, float H, float Alpha = 1f) : IThreeComponentFloatColor {

  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    var hRad = this.H * MathF.PI / 180f;
    var oklab = new Oklab(
      this.L,
      this.C * MathF.Cos(hRad),
      this.C * MathF.Sin(hRad),
      this.Alpha
    );
    return oklab.ToColor();
  }

  public static IColorSpace FromColor(Color color) {
    var oklab = (Oklab)Oklab.FromColor(color);
    var c = MathF.Sqrt(oklab.A * oklab.A + oklab.B * oklab.B);
    var h = MathF.Atan2(oklab.B, oklab.A) * 180f / MathF.PI;
    if (h < 0)
      h += 360f;

    return new Oklch(oklab.L, c, h, oklab.Alpha);
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new Oklch(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(Oklch)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Oklch)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());
}
