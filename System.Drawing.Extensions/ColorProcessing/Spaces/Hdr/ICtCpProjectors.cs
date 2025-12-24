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
using Hawkynt.ColorProcessing.Working;
using SysMath = System.Math;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Hdr;

/// <summary>
/// Projects LinearRgbF to ICtCpF.
/// </summary>
/// <remarks>
/// Implements the ICtCp color space conversion using PQ transfer function.
/// Input is assumed to be linear sRGB which is first converted to BT.2020.
/// Reference: https://professional.dolby.com/siteassets/pdfs/ictcp_dolbywhitepaper_v071.pdf
/// </remarks>
public readonly struct LinearRgbFToICtCpF : IProject<LinearRgbF, ICtCpF> {

  // PQ constants (SMPTE ST 2084)
  private const float C1 = 0.8359375f;         // 3424/4096
  private const float C2 = 18.8515625f;        // 2413/128
  private const float C3 = 18.6875f;           // 2392/128
  private const float M1 = 0.1593017578125f;   // 2610/16384
  private const float M2 = 78.84375f;          // 2523/32

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ICtCpF Project(in LinearRgbF work) {
    // sRGB linear to BT.2020 linear (approximate, assumes D65)
    var r2020 = 0.6274039f * work.R + 0.3292830f * work.G + 0.0433131f * work.B;
    var g2020 = 0.0690973f * work.R + 0.9195404f * work.G + 0.0113623f * work.B;
    var b2020 = 0.0163914f * work.R + 0.0880133f * work.G + 0.8955953f * work.B;

    // Scale for SDR (assume 100 cd/m2 peak, PQ uses 10000 cd/m2 reference)
    r2020 *= 0.01f;
    g2020 *= 0.01f;
    b2020 *= 0.01f;

    // BT.2020 to LMS
    var l = 0.412109375f * r2020 + 0.52392578125f * g2020 + 0.06396484375f * b2020;
    var m = 0.166748046875f * r2020 + 0.720458984375f * g2020 + 0.11279296875f * b2020;
    var s = 0.024169921875f * r2020 + 0.075439453125f * g2020 + 0.900390625f * b2020;

    // Apply PQ transfer function
    var lp = _Pq(l);
    var mp = _Pq(m);
    var sp = _Pq(s);

    // LMS' to ICtCp
    return new(
      0.5f * lp + 0.5f * mp,
      1.613769531f * lp - 3.323486328f * mp + 1.709716797f * sp,
      4.378173828f * lp - 4.245605469f * mp - 0.132568359f * sp
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Pq(float x) {
    if (x <= 0) return 0;
    var xm1 = (float)SysMath.Pow(x, M1);
    return (float)SysMath.Pow((C1 + C2 * xm1) / (1f + C3 * xm1), M2);
  }
}

/// <summary>
/// Projects LinearRgbaF to ICtCpF.
/// </summary>
public readonly struct LinearRgbaFToICtCpF : IProject<LinearRgbaF, ICtCpF> {

  private const float C1 = 0.8359375f;
  private const float C2 = 18.8515625f;
  private const float C3 = 18.6875f;
  private const float M1 = 0.1593017578125f;
  private const float M2 = 78.84375f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ICtCpF Project(in LinearRgbaF work) {
    var r2020 = 0.6274039f * work.R + 0.3292830f * work.G + 0.0433131f * work.B;
    var g2020 = 0.0690973f * work.R + 0.9195404f * work.G + 0.0113623f * work.B;
    var b2020 = 0.0163914f * work.R + 0.0880133f * work.G + 0.8955953f * work.B;

    r2020 *= 0.01f;
    g2020 *= 0.01f;
    b2020 *= 0.01f;

    var l = 0.412109375f * r2020 + 0.52392578125f * g2020 + 0.06396484375f * b2020;
    var m = 0.166748046875f * r2020 + 0.720458984375f * g2020 + 0.11279296875f * b2020;
    var s = 0.024169921875f * r2020 + 0.075439453125f * g2020 + 0.900390625f * b2020;

    var lp = _Pq(l);
    var mp = _Pq(m);
    var sp = _Pq(s);

    return new(
      0.5f * lp + 0.5f * mp,
      1.613769531f * lp - 3.323486328f * mp + 1.709716797f * sp,
      4.378173828f * lp - 4.245605469f * mp - 0.132568359f * sp
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Pq(float x) {
    if (x <= 0) return 0;
    var xm1 = (float)SysMath.Pow(x, M1);
    return (float)SysMath.Pow((C1 + C2 * xm1) / (1f + C3 * xm1), M2);
  }
}

/// <summary>
/// Projects ICtCpF back to LinearRgbF.
/// </summary>
public readonly struct ICtCpFToLinearRgbF : IProject<ICtCpF, LinearRgbF> {

  private const float C1 = 0.8359375f;
  private const float C2 = 18.8515625f;
  private const float C3 = 18.6875f;
  private const float M1 = 0.1593017578125f;
  private const float M2 = 78.84375f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in ICtCpF ictcp) {
    // ICtCp to LMS'
    var lp = ictcp.I + 0.00860904f * ictcp.Ct + 0.11103f * ictcp.Cp;
    var mp = ictcp.I - 0.00860904f * ictcp.Ct - 0.11103f * ictcp.Cp;
    var sp = ictcp.I + 0.56003134f * ictcp.Ct - 0.32062717f * ictcp.Cp;

    // Inverse PQ
    var l = _InvPq(lp);
    var m = _InvPq(mp);
    var s = _InvPq(sp);

    // LMS to BT.2020
    var r2020 = 3.4366066943f * l - 2.5064521187f * m + 0.0698454243f * s;
    var g2020 = -0.7913295556f * l + 1.9836004518f * m - 0.1922708962f * s;
    var b2020 = -0.0259498997f * l - 0.0989137147f * m + 1.1248636144f * s;

    // Scale back from SDR normalization
    r2020 *= 100f;
    g2020 *= 100f;
    b2020 *= 100f;

    // BT.2020 to sRGB linear
    return new(
      1.6604910021f * r2020 - 0.5876411388f * g2020 - 0.0728498633f * b2020,
      -0.1245504745f * r2020 + 1.1328998971f * g2020 - 0.0083494226f * b2020,
      -0.0181507634f * r2020 - 0.1005788980f * g2020 + 1.1187296614f * b2020
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _InvPq(float x) {
    if (x <= 0) return 0;
    var xp = (float)SysMath.Pow(x, 1f / M2);
    var num = xp - C1;
    if (num < 0) num = 0;
    var den = C2 - C3 * xp;
    if (den <= 0) return 0;
    return (float)SysMath.Pow(num / den, 1f / M1);
  }
}
