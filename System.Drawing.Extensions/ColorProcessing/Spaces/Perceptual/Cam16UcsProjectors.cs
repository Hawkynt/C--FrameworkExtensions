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

using System;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

// CAM16 / CAM16-UCS forward + inverse under default sRGB / D65 / dim-surround viewing
// (background Yb = 20, adapting luminance La = 64 cd/m² · π · 0.2 / π = 4 cd/m², surround =
// "average": c = 0.69, Nc = 1, F = 1). All viewing-condition derived quantities are baked
// in as `_VC` constants below — the user-facing projector just runs the forward/inverse
// pipeline. Reference: Li, Luo et al. 2017, "Comprehensive color solutions: CAM16, CAT16,
// and CAM16-UCS", Color Research & Application.

internal static class _Cam16ViewingConditions {
  // sRGB D65 reference white, scaled to Y = 100.
  internal const float Xw = 95.047f;
  internal const float Yw = 100.0f;
  internal const float Zw = 108.883f;

  // CAT16 forward matrix M16 (D65). From the 2017 paper, Table 1.
  internal const float M_R_X = 0.401288f, M_R_Y = 0.650173f, M_R_Z = -0.051461f;
  internal const float M_G_X = -0.250268f, M_G_Y = 1.204414f, M_G_Z = 0.045854f;
  internal const float M_B_X = -0.002079f, M_B_Y = 0.048952f, M_B_Z = 0.953127f;

  // CAT16 inverse matrix M16^-1 (numerical inverse of M16).
  internal const float Mi_R_X = 1.86206786f, Mi_R_Y = -1.01125463f, Mi_R_Z = 0.14918677f;
  internal const float Mi_G_X = 0.38752654f, Mi_G_Y = 0.62144744f, Mi_G_Z = -0.00897398f;
  internal const float Mi_B_X = -0.01584150f, Mi_B_Y = -0.03412294f, Mi_B_Z = 1.04996444f;

  // Cone responses for the reference white (precomputed: M16 · Xw,Yw,Zw).
  internal const float Rw = M_R_X * Xw + M_R_Y * Yw + M_R_Z * Zw;
  internal const float Gw = M_G_X * Xw + M_G_Y * Yw + M_G_Z * Zw;
  internal const float Bw = M_B_X * Xw + M_B_Y * Yw + M_B_Z * Zw;

  // Surround = "average".
  internal const float F = 1.0f;
  internal const float Nc = 1.0f;
  internal const float C = 0.69f;

  internal const float La = 4.0f; // Adapting luminance ≈ 64 cd/m² · 0.2 / π × π.
  internal const float Yb = 20.0f;

  // Degree of adaptation D = F·(1 − exp(−(La+42)/92)/3.6).
  internal static readonly float D = F * (1f - MathF.Exp(-(La + 42f) / 92f) / 3.6f);
  // Clamp D to [0, 1].
  internal static readonly float Dc = D < 0f ? 0f : (D > 1f ? 1f : D);

  // FL = 0.2·k⁴·(5·La) + 0.1·(1−k⁴)²·(5·La)^(1/3) ; k = 1/(5·La+1).
  private static readonly float K = 1f / (5f * La + 1f);
  private static readonly float K4 = K * K * K * K;
  internal static readonly float FL = 0.2f * K4 * 5f * La + 0.1f * (1f - K4) * (1f - K4) * MathF.Pow(5f * La, 1f / 3f);
  internal static readonly float FL025 = MathF.Pow(FL, 0.25f);

  internal static readonly float N = Yb / Yw;
  internal static readonly float Z = 1.48f + MathF.Sqrt(N);
  internal static readonly float Nbb = 0.725f * MathF.Pow(1f / N, 0.2f);
  internal static readonly float Ncb = Nbb;

  // Adapted cone responses for the reference white. The CAT16 step scales each cone by
  // (D · Yw / Cone_w + (1 − D)) — for the white the result is just the original cone value
  // when D = 1 (full adaptation); we precompute the actual adapted-cone values needed for
  // the achromatic-response anchor Aw.
  private static readonly float Drw = (Dc * Yw / Rw + (1f - Dc));
  private static readonly float Dgw = (Dc * Yw / Gw + (1f - Dc));
  private static readonly float Dbw = (Dc * Yw / Bw + (1f - Dc));

  internal static readonly float DRw = Drw;
  internal static readonly float DGw = Dgw;
  internal static readonly float DBw = Dbw;

  // Post-adaptation nonlinear compression on the white-point cones.
  private static readonly float Rcw = Drw * Rw;
  private static readonly float Gcw = Dgw * Gw;
  private static readonly float Bcw = Dbw * Bw;
  private static readonly float Raw = _Compress(Rcw);
  private static readonly float Gaw = _Compress(Gcw);
  private static readonly float Baw = _Compress(Bcw);

  // Achromatic response of the reference white.
  internal static readonly float Aw = (2f * Raw + Gaw + Baw / 20f - 0.305f) * Nbb;

  // CAM16-UCS coefficients.
  internal const float Ucs_C1 = 0.007f;
  internal const float Ucs_C2 = 0.0228f;

  // Hyperbolic compression: 400·(FL·R/100)^0.42 / (27.13 + (FL·R/100)^0.42) + 0.1, sign-preserved.
  internal static float _Compress(float v) {
    var a = MathF.Abs(FL * v / 100f);
    var p = MathF.Pow(a, 0.42f);
    var sign = v < 0f ? -1f : 1f;
    return sign * 400f * p / (27.13f + p) + 0.1f;
  }

  // Inverse of _Compress.
  internal static float _Decompress(float a) {
    var s = a - 0.1f;
    var sign = s < 0f ? -1f : 1f;
    var u = MathF.Abs(s);
    // u = 400·p / (27.13 + p) → p = 27.13·u / (400 − u).
    var p = 27.13f * u / MathF.Max(400f - u, 1e-6f);
    return sign * 100f / FL * MathF.Pow(p, 1f / 0.42f);
  }
}

/// <summary>
/// Projects <see cref="LinearRgbF"/> (sRGB primaries, D65) to <see cref="Cam16UcsF"/>.
/// </summary>
public readonly struct LinearRgbFToCam16UcsF : IProject<LinearRgbF, Cam16UcsF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Cam16UcsF Project(in LinearRgbF work) => _Forward(work.R, work.G, work.B);

  internal static Cam16UcsF _Forward(float r, float g, float b) {
    // sRGB → XYZ (D65), scale Y to 100 (the CAM16 paper convention).
    var x = (0.4124564f * r + 0.3575761f * g + 0.1804375f * b) * 100f;
    var y = (0.2126729f * r + 0.7151522f * g + 0.0721750f * b) * 100f;
    var z = (0.0193339f * r + 0.1191920f * g + 0.9503041f * b) * 100f;

    // CAT16 cone responses.
    var rc = _Cam16ViewingConditions.M_R_X * x + _Cam16ViewingConditions.M_R_Y * y + _Cam16ViewingConditions.M_R_Z * z;
    var gc = _Cam16ViewingConditions.M_G_X * x + _Cam16ViewingConditions.M_G_Y * y + _Cam16ViewingConditions.M_G_Z * z;
    var bc = _Cam16ViewingConditions.M_B_X * x + _Cam16ViewingConditions.M_B_Y * y + _Cam16ViewingConditions.M_B_Z * z;

    // Adaptation.
    var rd = _Cam16ViewingConditions.DRw * rc;
    var gd = _Cam16ViewingConditions.DGw * gc;
    var bd = _Cam16ViewingConditions.DBw * bc;

    // Post-adaptation nonlinear compression.
    var ra = _Cam16ViewingConditions._Compress(rd);
    var ga = _Cam16ViewingConditions._Compress(gd);
    var ba = _Cam16ViewingConditions._Compress(bd);

    // Opponent signals.
    var aOp = ra - 12f * ga / 11f + ba / 11f;
    var bOp = (ra + ga - 2f * ba) / 9f;

    // Achromatic response, lightness J.
    var aResp = (2f * ra + ga + ba / 20f - 0.305f) * _Cam16ViewingConditions.Nbb;
    var jLin = aResp / _Cam16ViewingConditions.Aw;
    if (jLin < 0f) jLin = 0f;
    var j = 100f * MathF.Pow(jLin, _Cam16ViewingConditions.C * _Cam16ViewingConditions.Z);

    // Hue angle h, eccentricity et.
    var h = MathF.Atan2(bOp, aOp); // radians
    var hDeg = h * (180f / MathF.PI);
    if (hDeg < 0f) hDeg += 360f;
    var hRad = hDeg * (MathF.PI / 180f);
    var et = 0.25f * (MathF.Cos(hRad + 2f) + 3.8f);

    // Chroma C, colorfulness M.
    var sqrtAB = MathF.Sqrt(aOp * aOp + bOp * bOp);
    var t = (50000f / 13f * _Cam16ViewingConditions.Nc * _Cam16ViewingConditions.Ncb * et * sqrtAB)
           / MathF.Max(ra + ga + 21f * ba / 20f, 1e-9f);
    var alpha = MathF.Pow(MathF.Max(t, 0f), 0.9f) * MathF.Pow(1.64f - MathF.Pow(0.29f, _Cam16ViewingConditions.N), 0.73f);
    var cChroma = alpha * MathF.Sqrt(j / 100f);
    var m = cChroma * _Cam16ViewingConditions.FL025;

    // CAM16-UCS coordinates.
    var jPrime = (1f + 100f * _Cam16ViewingConditions.Ucs_C1) * j / (1f + _Cam16ViewingConditions.Ucs_C1 * j);
    var mPrime = MathF.Log(1f + _Cam16ViewingConditions.Ucs_C2 * m) / _Cam16ViewingConditions.Ucs_C2;
    var aPrime = mPrime * MathF.Cos(hRad);
    var bPrime = mPrime * MathF.Sin(hRad);

    return new Cam16UcsF(jPrime, aPrime, bPrime);
  }
}

/// <summary>
/// Projects <see cref="LinearRgbaF"/> to <see cref="Cam16UcsF"/> (alpha is dropped).
/// </summary>
public readonly struct LinearRgbaFToCam16UcsF : IProject<LinearRgbaF, Cam16UcsF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Cam16UcsF Project(in LinearRgbaF work) => LinearRgbFToCam16UcsF._Forward(work.R, work.G, work.B);
}

/// <summary>
/// Projects <see cref="Cam16UcsF"/> back to <see cref="LinearRgbF"/>.
/// </summary>
public readonly struct Cam16UcsFToLinearRgbF : IProject<Cam16UcsF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in Cam16UcsF cam) {
    // UCS → CAM16 (J, M, h).
    var jPrime = cam.J;
    var aPrime = cam.A;
    var bPrime = cam.B;
    var mPrime = MathF.Sqrt(aPrime * aPrime + bPrime * bPrime);
    var m = (MathF.Exp(_Cam16ViewingConditions.Ucs_C2 * mPrime) - 1f) / _Cam16ViewingConditions.Ucs_C2;
    var j = jPrime / (1f + _Cam16ViewingConditions.Ucs_C1 * (100f - jPrime));
    var h = MathF.Atan2(bPrime, aPrime); // radians

    // Lightness recovery.
    var jLin01 = MathF.Max(j / 100f, 0f);
    var aResp = _Cam16ViewingConditions.Aw * MathF.Pow(jLin01, 1f / (_Cam16ViewingConditions.C * _Cam16ViewingConditions.Z));
    var p2 = aResp / _Cam16ViewingConditions.Nbb + 0.305f;

    // Opponent recovery. Canonical Li/Luo CAM16 inverse:
    //   t = (α / [(1.64 − 0.29ⁿ)^0.73])^(1/0.9), where α = C/√(J/100)
    //   p1 = (50000/13)·Nc·Ncb·et / t
    //   hr = h (radians)
    //   if |sin hr| ≥ |cos hr|: p4 = p1/sin hr; b = p2·(2+21/20)·460/1403 / (p4 + (2+21/20)·(220/1403)·(cos hr/sin hr) − (27/1403) − (6300/1403)); a = b·(cos hr/sin hr)
    //   else:                   p5 = p1/cos hr; a = p2·(2+21/20)·460/1403 / (p5 + (2+21/20)·(220/1403) − (27/1403)·(sin hr/cos hr) − (6300/1403)·(sin hr/cos hr)); b = a·(sin hr/cos hr)
    // The earlier code dropped the p4/p5 terms from the denominators — that's the bug.
    // Achromatic short-circuit: when chroma → 0 the canonical formulas degenerate
    // (t → 0, α → 0); detect upfront via mPrime and skip the opponent recovery.
    float aOp, bOp;
    if (mPrime < 1e-4f) {
      aOp = 0f;
      bOp = 0f;
    } else {
      var cChroma = m / _Cam16ViewingConditions.FL025;
      var sqrtJ = MathF.Sqrt(jLin01);
      var alpha = sqrtJ > 1e-6f ? cChroma / sqrtJ : 0f;
      var et = 0.25f * (MathF.Cos(h + 2f) + 3.8f);
      var k = MathF.Pow(1.64f - MathF.Pow(0.29f, _Cam16ViewingConditions.N), 0.73f);
      var t = MathF.Pow(alpha / k, 1f / 0.9f);
      var sinH = MathF.Sin(h);
      var cosH = MathF.Cos(h);

      if (t < 1e-6f) {
        aOp = 0f;
        bOp = 0f;
      } else {
        var p1 = 50000f / 13f * _Cam16ViewingConditions.Nc * _Cam16ViewingConditions.Ncb * et / t;
        const float c1 = 460f / 1403f;
        const float c2 = 220f / 1403f;
        const float c3 = 27f / 1403f;
        const float c4 = 6300f / 1403f;
        const float c5 = 2f + 21f / 20f;
        if (MathF.Abs(sinH) >= MathF.Abs(cosH)) {
          var p4 = p1 / sinH;
          bOp = p2 * c5 * c1 / (p4 + c5 * c2 * (cosH / sinH) - c3 - c4);
          aOp = bOp * (cosH / sinH);
        } else {
          var p5 = p1 / cosH;
          aOp = p2 * c5 * c1 / (p5 + c5 * c2 - c3 * (sinH / cosH) - c4 * (sinH / cosH));
          bOp = aOp * (sinH / cosH);
        }
      }
    }

    // Recover adapted cone responses.
    var ra = (460f * p2 + 451f * aOp + 288f * bOp) / 1403f;
    var ga = (460f * p2 - 891f * aOp - 261f * bOp) / 1403f;
    var ba = (460f * p2 - 220f * aOp - 6300f * bOp) / 1403f;

    // Inverse compression.
    var rd = _Cam16ViewingConditions._Decompress(ra);
    var gd = _Cam16ViewingConditions._Decompress(ga);
    var bd = _Cam16ViewingConditions._Decompress(ba);

    // Inverse adaptation.
    var rc = rd / _Cam16ViewingConditions.DRw;
    var gc = gd / _Cam16ViewingConditions.DGw;
    var bc = bd / _Cam16ViewingConditions.DBw;

    // CAT16 inverse → XYZ.
    var x = _Cam16ViewingConditions.Mi_R_X * rc + _Cam16ViewingConditions.Mi_R_Y * gc + _Cam16ViewingConditions.Mi_R_Z * bc;
    var y = _Cam16ViewingConditions.Mi_G_X * rc + _Cam16ViewingConditions.Mi_G_Y * gc + _Cam16ViewingConditions.Mi_G_Z * bc;
    var z = _Cam16ViewingConditions.Mi_B_X * rc + _Cam16ViewingConditions.Mi_B_Y * gc + _Cam16ViewingConditions.Mi_B_Z * bc;

    // XYZ (Y = 100) → linear sRGB.
    x *= 0.01f;
    y *= 0.01f;
    z *= 0.01f;
    return new LinearRgbF(
      3.2404542f * x - 1.5371385f * y - 0.4985314f * z,
      -0.9692660f * x + 1.8760108f * y + 0.0415560f * z,
      0.0556434f * x - 0.2040259f * y + 1.0572252f * z
    );
  }
}
