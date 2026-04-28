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
/// Projects OklabF to OkhslF (Ottosson 2020 colour-picker space).
/// </summary>
/// <remarks>
/// Reference: Björn Ottosson, "Okhsv and Okhsl" (2020) —
/// <see href="https://bottosson.github.io/posts/colorpicker/"/>.
/// </remarks>
public readonly struct OklabFToOkhslF : IProject<OklabF, OkhslF> {

  private const float TwoPi = 6.2831853071795864769f;
  private const float InvTwoPi = 1f / TwoPi;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OkhslF Project(in OklabF lab) {
    var c = MathF.Sqrt(lab.A * lab.A + lab.B * lab.B);
    if (c < 1e-9f) {
      // Achromatic — toe-mapped lightness only.
      return new(0f, 0f, OkhslOkhsvHelpers.Toe(lab.L));
    }

    var aNorm = lab.A / c;
    var bNorm = lab.B / c;
    var (c0, cMid, cMax) = OkhslOkhsvHelpers.GetCs(lab.L, aNorm, bNorm);

    float s;
    if (c < cMid) {
      var k1 = 0.8f * c0;
      var k2 = 1f - k1 / cMid;
      var t = c / (k1 + k2 * c);
      s = t * 0.8f;
    } else {
      var k1 = 0.2f * cMid * cMid * 1.25f / c0;
      var k2 = 1f - k1 / (cMax - cMid);
      var t = (c - cMid) / (k1 + k2 * (c - cMid));
      s = 0.8f + 0.2f * t;
    }

    var l = OkhslOkhsvHelpers.Toe(lab.L);
    var hRad = MathF.Atan2(lab.B, lab.A);
    if (hRad < 0f) hRad += TwoPi;
    var h = hRad * InvTwoPi;
    return new(h, s, l);
  }
}

/// <summary>
/// Projects OkhslF back to OklabF.
/// </summary>
public readonly struct OkhslFToOklabF : IProject<OkhslF, OklabF> {

  private const float TwoPi = 6.2831853071795864769f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklabF Project(in OkhslF hsl) {
    if (hsl.L >= 1f - 1e-7f) return new(1f, 0f, 0f);
    if (hsl.L <= 1e-7f) return new(0f, 0f, 0f);
    if (hsl.S <= 1e-7f) return new(OkhslOkhsvHelpers.ToeInv(hsl.L), 0f, 0f);

    var l = OkhslOkhsvHelpers.ToeInv(hsl.L);
    var hRad = hsl.H * TwoPi;
    var aNorm = MathF.Cos(hRad);
    var bNorm = MathF.Sin(hRad);

    var (c0, cMid, cMax) = OkhslOkhsvHelpers.GetCs(l, aNorm, bNorm);

    float c;
    if (hsl.S < 0.8f) {
      var t = 1.25f * hsl.S;
      var k1 = 0.8f * c0;
      var k2 = 1f - k1 / cMid;
      c = t * k1 / (1f - k2 * t);
    } else {
      var t = 5f * (hsl.S - 0.8f);
      var k1 = 0.2f * cMid * cMid * 1.25f / c0;
      var k2 = 1f - k1 / (cMax - cMid);
      c = cMid + t * k1 / (1f - k2 * t);
    }

    return new(l, c * aNorm, c * bNorm);
  }
}

/// <summary>
/// Projects LinearRgbF to OkhslF via Oklab.
/// </summary>
public readonly struct LinearRgbFToOkhslF : IProject<LinearRgbF, OkhslF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OkhslF Project(in LinearRgbF work) {
    var oklab = new LinearRgbFToOklabF().Project(work);
    return new OklabFToOkhslF().Project(oklab);
  }
}

/// <summary>
/// Projects LinearRgbaF to OkhslF via Oklab.
/// </summary>
public readonly struct LinearRgbaFToOkhslF : IProject<LinearRgbaF, OkhslF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OkhslF Project(in LinearRgbaF work) {
    var oklab = new LinearRgbaFToOklabF().Project(work);
    return new OklabFToOkhslF().Project(oklab);
  }
}

/// <summary>
/// Projects OkhslF back to LinearRgbF via Oklab.
/// </summary>
public readonly struct OkhslFToLinearRgbF : IProject<OkhslF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in OkhslF hsl) {
    var oklab = new OkhslFToOklabF().Project(hsl);
    return new OklabFToLinearRgbF().Project(oklab);
  }
}
