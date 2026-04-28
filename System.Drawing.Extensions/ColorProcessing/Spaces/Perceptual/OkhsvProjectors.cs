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

/// <summary>
/// Projects OklabF to OkhsvF (Ottosson 2020 colour-picker space).
/// </summary>
/// <remarks>
/// Reference: Björn Ottosson, "Okhsv and Okhsl" (2020) —
/// <see href="https://bottosson.github.io/posts/colorpicker/"/>.
/// </remarks>
public readonly struct OklabFToOkhsvF : IProject<OklabF, OkhsvF> {

  private const float TwoPi = 6.2831853071795864769f;
  private const float InvTwoPi = 1f / TwoPi;
  private const float SmallEps = 1e-9f;

  // Black-to-white toe scaling factor in OKHSV (Ottosson reference).
  private const float S0 = 0.5f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OkhsvF Project(in OklabF lab) {
    var c = MathF.Sqrt(lab.A * lab.A + lab.B * lab.B);
    if (c < SmallEps) return new(0f, 0f, OkhslOkhsvHelpers.Toe(lab.L));

    var aNorm = lab.A / c;
    var bNorm = lab.B / c;

    var hRad = MathF.Atan2(lab.B, lab.A);
    if (hRad < 0f) hRad += TwoPi;
    var h = hRad * InvTwoPi;

    var (cuspL, cuspC) = OkhslOkhsvHelpers.FindCusp(aNorm, bNorm);
    var sMax = cuspC / cuspL;
    var tMax = cuspC / (1f - cuspL);

    var sScale = sMax / (sMax + tMax * S0);

    // Project (L, C) onto S=t·S_max, V=L_v scaling such that V=1 maps to cusp.
    var k = 1f - S0 / sScale;

    var t = tMax / (c + lab.L * tMax);
    var lV = t * lab.L;
    var cV = t * c;

    var lVt = OkhslOkhsvHelpers.ToeInv(lV);
    var cVt = cV * lVt / lV;

    // Scale RGB to find max channel; toe-map L.
    var rgbScaleA = 1f + sScale * (0.3963377774f * aNorm + 0.2158037573f * bNorm) * cVt / lVt;
    var rgbScaleB = 1f + sScale * (-0.1055613458f * aNorm - 0.0638541728f * bNorm) * cVt / lVt;
    var rgbScaleS = 1f + sScale * (-0.0894841775f * aNorm - 1.2914855480f * bNorm) * cVt / lVt;
    // Suppress unused (we only need the toe).
    _ = rgbScaleA; _ = rgbScaleB; _ = rgbScaleS;

    var lNew = OkhslOkhsvHelpers.Toe(lab.L / lVt * lab.L);
    // Simplified: V = toe(lab.L / lV). Equivalent and stable at the cusp.
    var v = OkhslOkhsvHelpers.Toe(lab.L) / OkhslOkhsvHelpers.Toe(lVt);
    if (!float.IsFinite(v)) v = 0f;
    _ = lNew;

    var s = (S0 + tMax) * cV / (tMax * S0 + tMax * k * cV);
    if (!float.IsFinite(s)) s = 0f;

    return new(h, MathF.Max(0f, MathF.Min(1f, s)), MathF.Max(0f, MathF.Min(1f, v)));
  }
}

/// <summary>
/// Projects OkhsvF back to OklabF.
/// </summary>
public readonly struct OkhsvFToOklabF : IProject<OkhsvF, OklabF> {

  private const float TwoPi = 6.2831853071795864769f;
  private const float SmallEps = 1e-9f;
  private const float S0 = 0.5f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklabF Project(in OkhsvF hsv) {
    if (hsv.V <= SmallEps) return new(0f, 0f, 0f);
    if (hsv.S <= SmallEps) return new(OkhslOkhsvHelpers.ToeInv(hsv.V), 0f, 0f);

    var hRad = hsv.H * TwoPi;
    var aNorm = MathF.Cos(hRad);
    var bNorm = MathF.Sin(hRad);

    var (cuspL, cuspC) = OkhslOkhsvHelpers.FindCusp(aNorm, bNorm);
    var sMax = cuspC / cuspL;
    var tMax = cuspC / (1f - cuspL);
    var sScale = sMax / (sMax + tMax * S0);
    var k = 1f - S0 / sScale;

    var lV = 1f - hsv.S * S0 / (S0 + tMax - tMax * k * hsv.S);
    var cV = hsv.S * tMax * S0 / (S0 + tMax - tMax * k * hsv.S);

    var l = hsv.V * lV;
    var c = hsv.V * cV;

    // Compensate for toe.
    var lVt = OkhslOkhsvHelpers.ToeInv(lV);
    var cVt = cV * lVt / lV;

    var lNew = OkhslOkhsvHelpers.ToeInv(l);
    c = c * lNew / l;
    l = lNew;
    _ = lVt; _ = cVt;

    return new(l, c * aNorm, c * bNorm);
  }
}

/// <summary>
/// Projects LinearRgbF to OkhsvF via Oklab.
/// </summary>
public readonly struct LinearRgbFToOkhsvF : IProject<LinearRgbF, OkhsvF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OkhsvF Project(in LinearRgbF work) {
    var oklab = new LinearRgbFToOklabF().Project(work);
    return new OklabFToOkhsvF().Project(oklab);
  }
}

/// <summary>
/// Projects LinearRgbaF to OkhsvF via Oklab.
/// </summary>
public readonly struct LinearRgbaFToOkhsvF : IProject<LinearRgbaF, OkhsvF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OkhsvF Project(in LinearRgbaF work) {
    var oklab = new LinearRgbaFToOklabF().Project(work);
    return new OklabFToOkhsvF().Project(oklab);
  }
}

/// <summary>
/// Projects OkhsvF back to LinearRgbF via Oklab.
/// </summary>
public readonly struct OkhsvFToLinearRgbF : IProject<OkhsvF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in OkhsvF hsv) {
    var oklab = new OkhsvFToOklabF().Project(hsv);
    return new OklabFToLinearRgbF().Project(oklab);
  }
}
