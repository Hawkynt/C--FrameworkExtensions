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
/// Represents a color in the ICtCp perceptual color space.
/// </summary>
/// <remarks>
/// <para>ICtCp is a color space developed by Dolby for HDR video content.</para>
/// <para>It provides better perceptual uniformity than other spaces for HDR content.</para>
/// <para>I: Intensity (luminance)</para>
/// <para>Ct: Chroma tritan (blue-yellow)</para>
/// <para>Cp: Chroma protan (red-green)</para>
/// <para>Reference: ITU-R BT.2100</para>
/// </remarks>
[ColorSpace(3, ["I", "Ct", "Cp"], ColorSpaceType = ColorSpaceType.Perceptual, IsPerceptuallyUniform = true, DisplayName = "ICtCp")]
public record struct ICtCp(float I, float Ct, float Cp, float Alpha = 1f) : IThreeComponentFloatColor {

  private const float M1 = 0.1593017578125f;
  private const float M2 = 78.84375f;
  private const float C1 = 0.8359375f;
  private const float C2 = 18.8515625f;
  private const float C3 = 18.6875f;

  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // ICtCp to LMS'
    var lp = this.I + 0.00860904f * this.Ct + 0.11103f * this.Cp;
    var mp = this.I - 0.00860904f * this.Ct - 0.11103f * this.Cp;
    var sp = this.I + 0.56003f * this.Ct - 0.32068f * this.Cp;

    // Inverse PQ EOTF
    var l = _InversePq(lp);
    var m = _InversePq(mp);
    var s = _InversePq(sp);

    // LMS to linear RGB (BT.2020)
    var r = 3.4366066943f * l - 2.5064521187f * m + 0.0698454243f * s;
    var g = -0.7913295556f * l + 1.9836004518f * m - 0.1922708962f * s;
    var b = -0.0259498997f * l - 0.0989137147f * m + 1.1248636144f * s;

    // Linear to sRGB gamma (approximation for SDR display)
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
    var r = _SrgbToLinear(color.R / 255f);
    var g = _SrgbToLinear(color.G / 255f);
    var b = _SrgbToLinear(color.B / 255f);

    // Linear RGB to LMS (using BT.2020 primaries approximation)
    var l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
    var m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
    var s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;

    // Apply PQ EOTF
    var lp = _Pq(l);
    var mp = _Pq(m);
    var sp = _Pq(s);

    // LMS' to ICtCp
    return new ICtCp(
      0.5f * lp + 0.5f * mp,
      1.61376953125f * lp - 3.323486328125f * mp + 1.709716796875f * sp,
      4.378173828125f * lp - 4.24560546875f * mp - 0.132568359375f * sp,
      color.A / 255f
    );
  }

  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new ICtCp(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(ICtCp)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(ICtCp)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Pq(float x) {
    if (x <= 0)
      return 0;
    var xm1 = MathF.Pow(x, M1);
    return MathF.Pow((C1 + C2 * xm1) / (1 + C3 * xm1), M2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _InversePq(float x) {
    if (x <= 0)
      return 0;
    var xm2 = MathF.Pow(x, 1f / M2);
    return MathF.Pow(Math.Max(0, xm2 - C1) / (C2 - C3 * xm2), 1f / M1);
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
