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
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Working;
using SysMath = System.Math;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Hdr;

/// <summary>
/// Projects LinearRgbF to JzAzBzF.
/// </summary>
/// <remarks>
/// Implements the JzAzBz color space conversion using PQ transfer function.
/// Reference: https://observablehq.com/@jrus/jzazbz
/// </remarks>
public readonly struct LinearRgbFToJzAzBzF : IProject<LinearRgbF, JzAzBzF> {

  // JzAzBz constants
  private const float B = 1.15f;
  private const float G = 0.66f;
  private const float C1 = 0.8359375f;         // 3424/4096
  private const float C2 = 18.8515625f;        // 2413/128
  private const float C3 = 18.6875f;           // 2392/128
  private const float N = 0.15930175781f;      // 2610/16384
  private const float P = 134.034375f;         // 1.7*2523/32
  private const float D = -0.56f;
  private const float D0 = 1.6295499532821566e-11f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public JzAzBzF Project(in LinearRgbF work) {
    // Linear sRGB to XYZ D65
    var x = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    // Convert to absolute XYZ (assuming 203 cd/m2 peak luminance for SDR)
    x *= 203f;
    y *= 203f;
    z *= 203f;

    // XYZ to X'Y'Z' (JzAzBz modification)
    var xp = B * x - (B - 1f) * z;
    var yp = G * y - (G - 1f) * x;

    // X'Y'Z' to LMS
    var l = 0.41478972f * xp + 0.579999f * yp + 0.0146480f * z;
    var m = -0.2015100f * xp + 1.120649f * yp + 0.0531008f * z;
    var s = -0.0166008f * xp + 0.264800f * yp + 0.6684799f * z;

    // Normalize for PQ
    l /= 10000f;
    m /= 10000f;
    s /= 10000f;

    // Apply PQ transfer function
    var lp = _Pq(l);
    var mp = _Pq(m);
    var sp = _Pq(s);

    // LMS' to Izazbz
    var iz = 0.5f * (lp + mp);
    var az = 3.524000f * lp - 4.066708f * mp + 0.542708f * sp;
    var bz = 0.199076f * lp + 1.096799f * mp - 1.295875f * sp;

    // Izazbz to Jzazbz
    var jz = ((1f + D) * iz) / (1f + D * iz) - D0;

    return new(jz, az, bz);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Pq(float x) {
    if (x <= 0) return 0;
    var xn = (float)SysMath.Pow(x, N);
    return (float)SysMath.Pow((C1 + C2 * xn) / (1f + C3 * xn), P);
  }
}

/// <summary>
/// Projects LinearRgbaF to JzAzBzF.
/// </summary>
public readonly struct LinearRgbaFToJzAzBzF : IProject<LinearRgbaF, JzAzBzF> {

  private const float B = 1.15f;
  private const float G = 0.66f;
  private const float C1 = 0.8359375f;
  private const float C2 = 18.8515625f;
  private const float C3 = 18.6875f;
  private const float N = 0.15930175781f;
  private const float P = 134.034375f;
  private const float D = -0.56f;
  private const float D0 = 1.6295499532821566e-11f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public JzAzBzF Project(in LinearRgbaF work) {
    var x = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    x *= 203f;
    y *= 203f;
    z *= 203f;

    var xp = B * x - (B - 1f) * z;
    var yp = G * y - (G - 1f) * x;

    var l = 0.41478972f * xp + 0.579999f * yp + 0.0146480f * z;
    var m = -0.2015100f * xp + 1.120649f * yp + 0.0531008f * z;
    var s = -0.0166008f * xp + 0.264800f * yp + 0.6684799f * z;

    l /= 10000f;
    m /= 10000f;
    s /= 10000f;

    var lp = _Pq(l);
    var mp = _Pq(m);
    var sp = _Pq(s);

    var iz = 0.5f * (lp + mp);
    var az = 3.524000f * lp - 4.066708f * mp + 0.542708f * sp;
    var bz = 0.199076f * lp + 1.096799f * mp - 1.295875f * sp;

    var jz = ((1f + D) * iz) / (1f + D * iz) - D0;

    return new(jz, az, bz);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Pq(float x) {
    if (x <= 0) return 0;
    var xn = (float)SysMath.Pow(x, N);
    return (float)SysMath.Pow((C1 + C2 * xn) / (1f + C3 * xn), P);
  }
}

/// <summary>
/// Projects JzAzBzF back to LinearRgbF.
/// </summary>
public readonly struct JzAzBzFToLinearRgbF : IProject<JzAzBzF, LinearRgbF> {

  private const float B = 1.15f;
  private const float G = 0.66f;
  private const float C1 = 0.8359375f;
  private const float C2 = 18.8515625f;
  private const float C3 = 18.6875f;
  private const float N = 0.15930175781f;
  private const float P = 134.034375f;
  private const float D = -0.56f;
  private const float D0 = 1.6295499532821566e-11f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in JzAzBzF jzazbz) {
    // Jzazbz to Izazbz
    var jz = jzazbz.Jz + D0;
    var iz = jz / (1f + D - D * jz);

    // Izazbz to LMS'
    var lp = iz + 0.1386050f * jzazbz.Az + 0.0580473f * jzazbz.Bz;
    var mp = iz - 0.1386050f * jzazbz.Az - 0.0580473f * jzazbz.Bz;
    var sp = iz - 0.0960193f * jzazbz.Az - 0.8118919f * jzazbz.Bz;

    // Inverse PQ
    var l = _InvPq(lp) * 10000f;
    var m = _InvPq(mp) * 10000f;
    var s = _InvPq(sp) * 10000f;

    // LMS to X'Y'Z'
    var xp = 1.9242264358f * l - 1.0047923126f * m + 0.0376514040f * s;
    var yp = 0.3503167621f * l + 0.7264811939f * m - 0.0653844229f * s;
    var z = -0.0909828110f * l - 0.3127282905f * m + 1.5227665613f * s;

    // X'Y'Z' to XYZ
    var x = (xp + (B - 1f) * z) / B;
    var y = (yp + (G - 1f) * x) / G;

    // Convert from absolute XYZ
    x /= 203f;
    y /= 203f;
    z /= 203f;

    // XYZ to linear sRGB
    return new(
      ColorMatrices.XyzToRgb_RX * x + ColorMatrices.XyzToRgb_RY * y + ColorMatrices.XyzToRgb_RZ * z,
      ColorMatrices.XyzToRgb_GX * x + ColorMatrices.XyzToRgb_GY * y + ColorMatrices.XyzToRgb_GZ * z,
      ColorMatrices.XyzToRgb_BX * x + ColorMatrices.XyzToRgb_BY * y + ColorMatrices.XyzToRgb_BZ * z
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _InvPq(float x) {
    if (x <= 0) return 0;
    var xp = (float)SysMath.Pow(x, 1f / P);
    var num = C1 - xp;
    var den = C3 * xp - C2;
    if (den >= 0) return 0;
    return (float)SysMath.Pow(num / den, 1f / N);
  }
}
