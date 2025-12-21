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
/// Represents a color in the JzAzBz perceptual color space.
/// </summary>
/// <remarks>
/// <para>JzAzBz is a perceptually uniform color space designed for HDR and wide color gamut content.</para>
/// <para>It provides excellent perceptual uniformity for both SDR and HDR content.</para>
/// <para>Jz: Perceived lightness</para>
/// <para>Az: Red-green chromatic component</para>
/// <para>Bz: Yellow-blue chromatic component</para>
/// <para>Reference: M. Safdar, G. Cui, Y.J. Kim, M.R. Luo (2017) "Perceptually uniform color space for image signals including high dynamic range and wide gamut"</para>
/// </remarks>
[ColorSpace(3, ["Jz", "Az", "Bz"], ColorSpaceType = ColorSpaceType.Perceptual, IsPerceptuallyUniform = true, DisplayName = "JzAzBz")]
public record struct JzAzBz(float Jz, float Az, float Bz, float Alpha = 1f) : IThreeComponentFloatColor {

  private const float B = 1.15f;
  private const float G = 0.66f;
  private const float C1 = 0.8359375f;
  private const float C2 = 18.8515625f;
  private const float C3 = 18.6875f;
  private const float N = 0.15930175781f;
  private const float P = 134.034375f;
  private const float D = -0.56f;
  private const float D0 = 1.6295499532821566e-11f;

  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // JzAzBz to Iz, Az, Bz
    var iz = this.Jz + D0;
    iz = iz / (1 + D - D * iz);

    // Iz, Az, Bz to LMS'
    var lp = iz + 0.1386050432715393f * this.Az + 0.05804731615611886f * this.Bz;
    var mp = iz - 0.1386050432715393f * this.Az - 0.05804731615611886f * this.Bz;
    var sp = iz - 0.09601924202631895f * this.Az - 0.8118918960560388f * this.Bz;

    // Inverse PQ
    var l = _InversePq(lp);
    var m = _InversePq(mp);
    var s = _InversePq(sp);

    // LMS to XYZ
    var x = 1.9242264357876067f * l - 1.0047923125953657f * m + 0.037651404030618f * s;
    var y = 0.35031676209499907f * l + 0.7264811939316552f * m - 0.06538442294808501f * s;
    var z = -0.09098281098284752f * l - 0.3127282905230739f * m + 1.5227665613052603f * s;

    // Undo pre-adaptation
    x = (x + (B - 1) * z) / B;
    y = (y + (G - 1) * x) / G;

    // XYZ to linear sRGB
    var r = 3.2404542f * x - 1.5371385f * y - 0.4985314f * z;
    var g = -0.9692660f * x + 1.8760108f * y + 0.0415560f * z;
    var b = 0.0556434f * x - 0.2040259f * y + 1.0572252f * z;

    // Linear to sRGB gamma
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
    // sRGB to linear
    var r = _SrgbToLinear(color.R * Rgba32.ByteToNormalized);
    var g = _SrgbToLinear(color.G * Rgba32.ByteToNormalized);
    var b = _SrgbToLinear(color.B * Rgba32.ByteToNormalized);

    // Linear sRGB to XYZ (D65)
    var x = 0.4124564f * r + 0.3575761f * g + 0.1804375f * b;
    var y = 0.2126729f * r + 0.7151522f * g + 0.0721750f * b;
    var z = 0.0193339f * r + 0.1191920f * g + 0.9503041f * b;

    // Pre-adaptation
    var xp = B * x - (B - 1) * z;
    var yp = G * y - (G - 1) * x;

    // XYZ to LMS
    var l = 0.41478972f * xp + 0.579999f * yp + 0.0146480f * z;
    var m = -0.2015100f * xp + 1.120649f * yp + 0.0531008f * z;
    var s = -0.0166008f * xp + 0.264800f * yp + 0.6684799f * z;

    // Apply PQ transfer
    var lp = _Pq(l);
    var mp = _Pq(m);
    var sp = _Pq(s);

    // LMS' to Iz, Az, Bz
    var iz = 0.5f * lp + 0.5f * mp;
    var az = 3.524000f * lp - 4.066708f * mp + 0.542708f * sp;
    var bz = 0.199076f * lp + 1.096799f * mp - 1.295875f * sp;

    // Iz to Jz
    var jz = ((1 + D) * iz) / (1 + D * iz) - D0;

    return new JzAzBz(jz, az, bz, color.A * Rgba32.ByteToNormalized);
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new JzAzBz(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(JzAzBz)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(JzAzBz)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Pq(float x) {
    if (x <= 0)
      return 0;
    var xn = MathF.Pow(x / 10000f, N);
    return MathF.Pow((C1 + C2 * xn) / (1 + C3 * xn), P);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _InversePq(float x) {
    if (x <= 0)
      return 0;
    var xp = MathF.Pow(x, 1f / P);
    return 10000f * MathF.Pow(Math.Max(0, xp - C1) / (C2 - C3 * xp), 1f / N);
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

/// <summary>
/// Represents a color in the JzCzhz cylindrical color space (JzAzBz in polar form).
/// </summary>
[ColorSpace(3, ["Jz", "Cz", "hz"], ColorSpaceType = ColorSpaceType.Cylindrical, IsPerceptuallyUniform = true, DisplayName = "JzCzhz")]
public record struct JzCzhz(float Jz, float Cz, float Hz, float Alpha = 1f) : IThreeComponentFloatColor {

  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    var hRad = this.Hz * MathF.PI / 180f;
    var jzazbz = new JzAzBz(
      this.Jz,
      this.Cz * MathF.Cos(hRad),
      this.Cz * MathF.Sin(hRad),
      this.Alpha
    );
    return jzazbz.ToColor();
  }

  public static IColorSpace FromColor(Color color) {
    var jzazbz = (JzAzBz)JzAzBz.FromColor(color);
    var cz = MathF.Sqrt(jzazbz.Az * jzazbz.Az + jzazbz.Bz * jzazbz.Bz);
    var hz = MathF.Atan2(jzazbz.Bz, jzazbz.Az) * 180f / MathF.PI;
    if (hz < 0)
      hz += 360f;

    return new JzCzhz(jzazbz.Jz, cz, hz, jzazbz.Alpha);
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new JzCzhz(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(JzCzhz)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(JzCzhz)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());
}
