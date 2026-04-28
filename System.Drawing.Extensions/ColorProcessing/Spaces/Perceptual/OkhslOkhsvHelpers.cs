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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

/// <summary>
/// Internal numerical helpers shared between OKHSL and OKHSV projectors.
/// </summary>
/// <remarks>
/// <para>Direct port of Björn Ottosson's reference C from
/// <see href="https://bottosson.github.io/posts/colorpicker/"/>
/// (public-domain). The formulas compute, for a given Oklab hue direction
/// <c>(a_, b_)</c> with unit chroma, the maximum chroma C that keeps the
/// resulting linear-sRGB triplet inside [0,1]³.</para>
/// <para>Routines are <see langword="internal"/> on purpose; only the
/// surrounding <c>OkhslF</c> / <c>OkhsvF</c> projectors are intended to
/// use them.</para>
/// </remarks>
internal static class OkhslOkhsvHelpers {

  // Toe / inverse-toe constants from Ottosson's reference implementation.
  private const float K1 = 0.206f;
  private const float K2 = 0.03f;
  private const float K3 = (1f + K1) / (1f + K2);

  /// <summary>Forward toe — maps Oklab L to OKHSL L.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float Toe(float x) {
    var inner = K3 * x - K1;
    return 0.5f * (inner + MathF.Sqrt(inner * inner + 4f * K2 * K3 * x));
  }

  /// <summary>Inverse toe — maps OKHSL L back to Oklab L.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float ToeInv(float x) => (x * x + K1 * x) / (K3 * (x + K2));

  /// <summary>Largest C such that (L,a_·C,b_·C) ↦ inside [0,1]³ linear sRGB.</summary>
  /// <remarks>Iterative Halley refinement of the cusp from Ottosson's blog post.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float ComputeMaxSaturation(float a_, float b_) {
    // Pick the channel that hits the gamut boundary first based on the hue direction.
    float k0, k1, k2, k3, k4, wl, wm, ws;
    if (-1.88170328f * a_ - 0.80936493f * b_ > 1f) {
      // Red component
      k0 = 1.19086277f; k1 = 1.76576728f; k2 = 0.59662641f;
      k3 = 0.75515197f; k4 = 0.56771245f;
      wl = 4.0767416621f; wm = -3.3077115913f; ws = 0.2309699292f;
    } else if (1.81444104f * a_ - 1.19445276f * b_ > 1f) {
      // Green component
      k0 = 0.73956515f; k1 = -0.45954404f; k2 = 0.08285427f;
      k3 = 0.12541070f; k4 = 0.14503204f;
      wl = -1.2684380046f; wm = 2.6097574011f; ws = -0.3413193965f;
    } else {
      // Blue component
      k0 = 1.35733652f; k1 = -0.00915799f; k2 = -1.15130210f;
      k3 = -0.50559606f; k4 = 0.00692167f;
      wl = -0.0041960863f; wm = -0.7034186147f; ws = 1.7076147010f;
    }

    var s = k0 + k1 * a_ + k2 * b_ + k3 * a_ * a_ + k4 * a_ * b_;

    // One Halley step suffices for ~6 decimals (Ottosson's claim, verified).
    var kL = 0.3963377774f * a_ + 0.2158037573f * b_;
    var kM = -0.1055613458f * a_ - 0.0638541728f * b_;
    var kS = -0.0894841775f * a_ - 1.2914855480f * b_;

    var lLms = 1f + s * kL;
    var mLms = 1f + s * kM;
    var sLms = 1f + s * kS;

    var lCubed = lLms * lLms * lLms;
    var mCubed = mLms * mLms * mLms;
    var sCubed = sLms * sLms * sLms;

    var lDs = 3f * kL * lLms * lLms;
    var mDs = 3f * kM * mLms * mLms;
    var sDs = 3f * kS * sLms * sLms;

    var lDs2 = 6f * kL * kL * lLms;
    var mDs2 = 6f * kM * kM * mLms;
    var sDs2 = 6f * kS * kS * sLms;

    var f = wl * lCubed + wm * mCubed + ws * sCubed;
    var f1 = wl * lDs + wm * mDs + ws * sDs;
    var f2 = wl * lDs2 + wm * mDs2 + ws * sDs2;

    s = s - f * f1 / (f1 * f1 - 0.5f * f * f2);
    return s;
  }

  /// <summary>(L,C) of the chroma cusp for hue direction (a_, b_).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static (float L, float C) FindCusp(float a_, float b_) {
    var sCusp = ComputeMaxSaturation(a_, b_);

    // Convert (L=1, C=Sc) to RGB and rescale so the largest channel is 1.
    var lLms_ = 1f + sCusp * (0.3963377774f * a_ + 0.2158037573f * b_);
    var mLms_ = 1f + sCusp * (-0.1055613458f * a_ - 0.0638541728f * b_);
    var sLms_ = 1f + sCusp * (-0.0894841775f * a_ - 1.2914855480f * b_);

    var lLms = lLms_ * lLms_ * lLms_;
    var mLms = mLms_ * mLms_ * mLms_;
    var sLms = sLms_ * sLms_ * sLms_;

    var r = 4.0767416621f * lLms - 3.3077115913f * mLms + 0.2309699292f * sLms;
    var g = -1.2684380046f * lLms + 2.6097574011f * mLms - 0.3413193965f * sLms;
    var b = -0.0041960863f * lLms - 0.7034186147f * mLms + 1.7076147010f * sLms;

    var maxRgb = MathF.Max(MathF.Max(r, g), b);
    if (maxRgb < 1e-9f) return (0f, 0f);

    var lCusp = MathF.Cbrt(1f / maxRgb);
    var cCusp = lCusp * sCusp;
    return (lCusp, cCusp);
  }

  /// <summary>Anchor chromas (Cs, Cmid, Cmax) for a given (L, a_, b_), used by OKHSL.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static (float Cs, float Cmid, float Cmax) GetCs(float L, float a_, float b_) {
    var (cuspL, cuspC) = FindCusp(a_, b_);
    var cMax = FindGamutIntersection(a_, b_, L, 1f, L, cuspL, cuspC);
    var stMax = (S: cuspC / cuspL, T: cuspC / (1f - cuspL));

    // Approximate ST_mid from polynomial fit (Ottosson reference).
    var sMid = 0.11516993f
      + 1f / (
        7.44778970f
        + 4.15901240f * b_
        + a_ * (-2.19557347f + 1.75198401f * b_
          + a_ * (-2.13704948f - 10.02301043f * b_
            + a_ * (-4.24894561f + 5.38770819f * b_ + 4.69891013f * a_)))
      );

    var tMid = 0.11239642f
      + 1f / (
        1.61320320f
        - 0.68124379f * b_
        + a_ * (0.40370612f + 0.90148123f * b_
          + a_ * (-0.27087943f + 0.61223990f * b_
            + a_ * (0.00299215f - 0.45399568f * b_ - 0.14661872f * a_)))
      );

    var k = cMax / MathF.Min(L * stMax.S, (1f - L) * stMax.T);
    var caMid = L * sMid;
    var cbMid = (1f - L) * tMid;
    var cMid = 0.9f * k * MathF.Sqrt(MathF.Sqrt(1f / (1f / (caMid * caMid * caMid * caMid) + 1f / (cbMid * cbMid * cbMid * cbMid))));

    var ca0 = L * 0.4f;
    var cb0 = (1f - L) * 0.8f;
    var c0 = MathF.Sqrt(1f / (1f / (ca0 * ca0) + 1f / (cb0 * cb0)));
    return (c0, cMid, cMax);
  }

  /// <summary>Finds the largest t such that (L0+t·dL, t·dC·a_, t·dC·b_) is inside the sRGB gamut.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float FindGamutIntersection(
    float a_, float b_,
    float L1, float C1,
    float L0,
    float cuspL, float cuspC) {
    if ((L1 - L0) * cuspC - (cuspL - L0) * C1 <= 0f) {
      // Lower half — bound by the line from black to cusp.
      return cuspC * L0 / (C1 * cuspL + cuspC * (L0 - L1));
    }

    // Upper half — Halley iteration on the gamut surface.
    var t = cuspC * (L0 - 1f) / (C1 * (cuspL - 1f) + cuspC * (L0 - L1));

    var dL = L1 - L0;
    var dC = C1;

    var kL = 0.3963377774f * a_ + 0.2158037573f * b_;
    var kM = -0.1055613458f * a_ - 0.0638541728f * b_;
    var kS = -0.0894841775f * a_ - 1.2914855480f * b_;

    var lDt = dL + dC * kL;
    var mDt = dL + dC * kM;
    var sDt = dL + dC * kS;

    {
      var L = L0 * (1f - t) + t * L1;
      var C = t * C1;

      var lLms_ = L + C * kL;
      var mLms_ = L + C * kM;
      var sLms_ = L + C * kS;

      var lLms = lLms_ * lLms_ * lLms_;
      var mLms = mLms_ * mLms_ * mLms_;
      var sLms = sLms_ * sLms_ * sLms_;

      var lDt1 = 3f * lDt * lLms_ * lLms_;
      var mDt1 = 3f * mDt * mLms_ * mLms_;
      var sDt1 = 3f * sDt * sLms_ * sLms_;

      var lDt2 = 6f * lDt * lDt * lLms_;
      var mDt2 = 6f * mDt * mDt * mLms_;
      var sDt2 = 6f * sDt * sDt * sLms_;

      var r = 4.0767416621f * lLms - 3.3077115913f * mLms + 0.2309699292f * sLms - 1f;
      var r1 = 4.0767416621f * lDt1 - 3.3077115913f * mDt1 + 0.2309699292f * sDt1;
      var r2 = 4.0767416621f * lDt2 - 3.3077115913f * mDt2 + 0.2309699292f * sDt2;
      var uR = r1 / (r1 * r1 - 0.5f * r * r2);
      var tR = uR >= 0f ? -r * uR : float.MaxValue;

      var g = -1.2684380046f * lLms + 2.6097574011f * mLms - 0.3413193965f * sLms - 1f;
      var g1 = -1.2684380046f * lDt1 + 2.6097574011f * mDt1 - 0.3413193965f * sDt1;
      var g2 = -1.2684380046f * lDt2 + 2.6097574011f * mDt2 - 0.3413193965f * sDt2;
      var uG = g1 / (g1 * g1 - 0.5f * g * g2);
      var tG = uG >= 0f ? -g * uG : float.MaxValue;

      var bR = -0.0041960863f * lLms - 0.7034186147f * mLms + 1.7076147010f * sLms - 1f;
      var b1 = -0.0041960863f * lDt1 - 0.7034186147f * mDt1 + 1.7076147010f * sDt1;
      var b2 = -0.0041960863f * lDt2 - 0.7034186147f * mDt2 + 1.7076147010f * sDt2;
      var uB = b1 / (b1 * b1 - 0.5f * bR * b2);
      var tB = uB >= 0f ? -bR * uB : float.MaxValue;

      t += MathF.Min(tR, MathF.Min(tG, tB));
    }
    return t;
  }
}
