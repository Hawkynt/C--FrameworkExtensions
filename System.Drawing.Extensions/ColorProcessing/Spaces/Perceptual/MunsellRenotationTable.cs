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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

/// <summary>
/// ASTM D1535 / Newhall-Nickerson-Judd 1943 Munsell renotation table, embedded as
/// the <c>MunsellRenotation.dat</c> resource and parsed lazily on first use.
/// </summary>
/// <remarks>
/// <para>The renotation gives, for each Munsell chip (Hue, Value, Chroma) the CIE 1931
/// chromaticity (x, y) and luminance Y under illuminant C with the 1931 2&#176; standard
/// observer. Source data: <c>http://www.rit-mcsl.org/MunsellRenotation/all.dat</c>
/// (4995 chips, including extrapolated chromas; public domain US-government compilation
/// originally published as Newhall, Nickerson &amp; Judd, "Final report of the OSA
/// subcommittee on the spacing of the Munsell colors", J. Opt. Soc. Am. 33 (1943) 385).</para>
/// <para>The table is indexed by:
/// <list type="bullet">
///   <item><description><b>Value level</b> 0..13 covering the discrete V scale
///     0.2, 0.4, 0.6, 0.8, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10. Y depends only on V
///     and is held in <see cref="YByValueLevel"/>.</description></item>
///   <item><description><b>Hue index</b> 0..39 cyclic. Index 0 = 2.5R, 1 = 5R, 2 = 7.5R,
///     3 = 10R, 4 = 2.5YR, ..., 39 = 10RP. The standard 10-family wheel
///     R YR Y GY G BG B PB P RP each split into 2.5/5/7.5/10 steps.</description></item>
///   <item><description><b>Chroma</b> even integers 2..50 (variable max per (V, H)).
///     Stored densely; <see cref="MaxChroma"/>[v, h] gives the highest sampled
///     chroma at each (Value, Hue) corner.</description></item>
/// </list></para>
/// <para>Use <see cref="Forward"/> for trilinear (V, H, C) -&gt; xyY interpolation under
/// illuminant C, and <see cref="Inverse"/> for the inverse map. The inverse first
/// inverts Y -&gt; V via the ASTM D1535-08 polynomial, then refines (H, C) by 2D Newton
/// in xy at the interpolated Value slice.</para>
/// </remarks>
internal static class MunsellRenotationTable {

  /// <summary>The 14 sampled Munsell Value levels in the renotation table.</summary>
  internal static readonly float[] ValueLevels = {
    0.2f, 0.4f, 0.6f, 0.8f, 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f,
  };

  /// <summary>Y/100 (relative to perfect diffuser) at each Value level. Y_renotation = Y_ASTM * 1.0257
  /// where 1.0257 reflects the historical MgO-white normalisation; the values match the table exactly.</summary>
  internal static readonly float[] YByValueLevel = {
    0.00237f, 0.00467f, 0.00699f, 0.00943f, 0.0121f, 0.03126f, 0.0655f, 0.12f,
    0.1977f, 0.3003f, 0.4306f, 0.591f, 0.7866f, 1.0257f,
  };

  internal const int ValueLevelCount = 14;

  /// <summary>40 hue slots: index 0 = 2.5R, 1 = 5R, 2 = 7.5R, 3 = 10R, 4 = 2.5YR, ..., 39 = 10RP.</summary>
  internal const int HueSlotCount = 40;

  /// <summary>Even chromas only; chroma index k stores chroma 2*(k+1).</summary>
  internal const int MaxChromaIndex = 25; // 2..50, 25 slots (2*1..2*25)

  /// <summary>Illuminant C 2&#176; chromaticity (CIE 1931).</summary>
  internal const float IlluminantC_x = 0.31006f;

  /// <summary>Illuminant C 2&#176; chromaticity (CIE 1931).</summary>
  internal const float IlluminantC_y = 0.31616f;

  // Storage layout: x and y stored separately, indexed [v * HueSlotCount * MaxChromaIndex + h * MaxChromaIndex + ci]
  // where ci = chroma/2 - 1 (so chroma 2 -> ci 0). Slots without a chip carry NaN.
  private static readonly Lazy<TableData> _data = new(LoadFromEmbeddedResource);

  private sealed class TableData {
    public float[] X = new float[ValueLevelCount * HueSlotCount * MaxChromaIndex];
    public float[] Y = new float[ValueLevelCount * HueSlotCount * MaxChromaIndex];
    public byte[] MaxChroma = new byte[ValueLevelCount * HueSlotCount]; // chroma in chips, e.g. 14
  }

  /// <summary>Forces table parsing on the calling thread. Useful in tests.</summary>
  internal static void EnsureLoaded() => _ = _data.Value;

  /// <summary>Returns the maximum sampled chroma at (value-level v, hue index h).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int MaxChroma(int v, int h) => _data.Value.MaxChroma[v * HueSlotCount + h];

  /// <summary>Look up a chip directly. <paramref name="ci"/> is chroma/2 - 1. Returns NaN if absent.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void GetChip(int v, int h, int ci, out float x, out float y) {
    var d = _data.Value;
    var idx = v * HueSlotCount * MaxChromaIndex + h * MaxChromaIndex + ci;
    x = d.X[idx];
    y = d.Y[idx];
  }

  /// <summary>
  /// Forward Munsell -&gt; xyY under illuminant C with trilinear (Value, Hue, Chroma)
  /// interpolation. Value uses the discrete level scale; Hue is cyclic on 0..40;
  /// Chroma is in raw Munsell units (so chroma 6 = 6.0f).
  /// </summary>
  /// <param name="vIndexF">Continuous index into <see cref="ValueLevels"/>; e.g. 7.0 = V=4.</param>
  /// <param name="hueF">Continuous hue index 0..40 (cyclic). 0 = 2.5R.</param>
  /// <param name="chroma">Munsell chroma (>= 0). Clamped to per-(V,H) maximum.</param>
  /// <param name="x">Out CIE x.</param>
  /// <param name="y">Out CIE y.</param>
  /// <param name="bigY">Out CIE Y (relative; Y(V=10) = 1.0257).</param>
  internal static void Forward(float vIndexF, float hueF, float chroma, out float x, out float y, out float bigY) {
    if (vIndexF < 0f) vIndexF = 0f;
    else if (vIndexF > ValueLevelCount - 1) vIndexF = ValueLevelCount - 1;
    if (chroma < 0f) chroma = 0f;

    var v0 = (int)vIndexF;
    if (v0 >= ValueLevelCount - 1) v0 = ValueLevelCount - 2;
    var v1 = v0 + 1;
    var tv = vIndexF - v0;

    // Y is fully determined by Value level; interpolate linearly between the two table Ys.
    bigY = YByValueLevel[v0] + (YByValueLevel[v1] - YByValueLevel[v0]) * tv;

    SliceForward(v0, hueF, chroma, out var x0, out var y0);
    SliceForward(v1, hueF, chroma, out var x1, out var y1);
    x = x0 + (x1 - x0) * tv;
    y = y0 + (y1 - y0) * tv;
  }

  /// <summary>
  /// At a single Value slice, evaluate xy from (hue, chroma) by bilinear interpolation
  /// in the (Hue index, Chroma index) grid; falls back to the achromatic point when chroma
  /// approaches 0 or exceeds the local table maximum.
  /// </summary>
  internal static void SliceForward(int v, float hueF, float chroma, out float x, out float y) {
    // Wrap hue to [0, 40)
    var h = hueF;
    while (h < 0f) h += HueSlotCount;
    while (h >= HueSlotCount) h -= HueSlotCount;
    var h0 = (int)h;
    if (h0 >= HueSlotCount) h0 = HueSlotCount - 1;
    var h1 = h0 + 1;
    if (h1 >= HueSlotCount) h1 = 0;
    var th = h - h0;

    // Local maxima for the two bracketing hues. Clamp chroma to the smaller of the two
    // so the interpolation always stays inside both source curves.
    var max0 = MaxChroma(v, h0);
    var max1 = MaxChroma(v, h1);
    var maxLocal = max0 < max1 ? max0 : max1;
    if (maxLocal <= 0) {
      // Achromatic (no chips at this (V, H)). Fall back to illuminant C.
      x = IlluminantC_x;
      y = IlluminantC_y;
      return;
    }

    var c = chroma;
    if (c > maxLocal) c = maxLocal;

    // Map chroma -> floating chroma index (chroma=2 -> 0, chroma=4 -> 1, ...). Below 2,
    // blend with the achromatic point.
    var ciF = c * 0.5f - 1f; // c=2 -> 0, c=0 -> -1
    if (ciF < 0f) {
      // Blend with achromatic axis. tBlend = c/2 in [0,1].
      var tBlend = c * 0.5f;
      EvaluateAtChromaIndex(v, h0, h1, th, 0, out var xC2, out var yC2);
      x = IlluminantC_x + (xC2 - IlluminantC_x) * tBlend;
      y = IlluminantC_y + (yC2 - IlluminantC_y) * tBlend;
      return;
    }

    var ci0 = (int)ciF;
    if (ci0 >= MaxChromaIndex - 1) ci0 = MaxChromaIndex - 2;
    var ci1 = ci0 + 1;
    var tc = ciF - ci0;

    EvaluateAtChromaIndex(v, h0, h1, th, ci0, out var xa, out var ya);
    EvaluateAtChromaIndex(v, h0, h1, th, ci1, out var xb, out var yb);
    x = xa + (xb - xa) * tc;
    y = ya + (yb - ya) * tc;
  }

  /// <summary>
  /// Bilinearly mixes the chip at (v, h0, ci) and (v, h1, ci) by parameter <paramref name="th"/>,
  /// substituting the achromatic point for any missing chip slot so the interpolation never reads NaN.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void EvaluateAtChromaIndex(int v, int h0, int h1, float th, int ci, out float x, out float y) {
    GetChipOrAchromatic(v, h0, ci, out var x0, out var y0);
    GetChipOrAchromatic(v, h1, ci, out var x1, out var y1);
    x = x0 + (x1 - x0) * th;
    y = y0 + (y1 - y0) * th;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void GetChipOrAchromatic(int v, int h, int ci, out float x, out float y) {
    GetChip(v, h, ci, out x, out y);
    if (float.IsNaN(x)) {
      // Slot not in renotation table for this (V, H, C). Fall back along the chroma direction
      // by linearly extrapolating from the nearest two valid lower-chroma chips, or, failing that,
      // pin to the achromatic point.
      ExtrapolateMissingChip(v, h, ci, out x, out y);
    }
  }

  private static void ExtrapolateMissingChip(int v, int h, int ci, out float x, out float y) {
    // Walk down to find two valid chips at lower chroma at this (v, h).
    var loIdx = -1;
    float xLo = 0f, yLo = 0f;
    for (var k = ci - 1; k >= 0; --k) {
      GetChip(v, h, k, out var xi, out var yi);
      if (!float.IsNaN(xi)) {
        loIdx = k;
        xLo = xi; yLo = yi;
        break;
      }
    }

    if (loIdx < 0) {
      // No chips at all on this hue at this Value: fall back to illuminant C.
      x = IlluminantC_x;
      y = IlluminantC_y;
      return;
    }

    // Try to find a second valid chip below loIdx.
    var lo2Idx = -1;
    float xLo2 = 0f, yLo2 = 0f;
    for (var k = loIdx - 1; k >= 0; --k) {
      GetChip(v, h, k, out var xi, out var yi);
      if (!float.IsNaN(xi)) {
        lo2Idx = k;
        xLo2 = xi; yLo2 = yi;
        break;
      }
    }

    if (lo2Idx < 0) {
      // Only one valid chip - extrapolate linearly from achromatic point through that chip.
      var t = (float)(ci + 1) / (loIdx + 1); // chroma is (idx+1)*2; ratio along radial line
      x = IlluminantC_x + (xLo - IlluminantC_x) * t;
      y = IlluminantC_y + (yLo - IlluminantC_y) * t;
      return;
    }

    // Linear extrapolation in chroma between lo2Idx and loIdx out to ci.
    var step = ci - loIdx;
    var span = loIdx - lo2Idx;
    var dxds = (xLo - xLo2) / span;
    var dyds = (yLo - yLo2) / span;
    x = xLo + dxds * step;
    y = yLo + dyds * step;
  }

  /// <summary>
  /// Inverts xy at a fixed Value-level pair (interpolated by tv) to (continuous hue index, chroma).
  /// Uses 2D Newton on the forward bilinear map; converges in ~5 iterations to ~1e-5 in xy.
  /// </summary>
  /// <param name="vIndexF">Continuous Value level index (output of <see cref="InvertValueByY"/>).</param>
  /// <param name="targetX">Target CIE x under illuminant C.</param>
  /// <param name="targetY">Target CIE y under illuminant C.</param>
  /// <param name="hueF">Out continuous hue index 0..40 (cyclic).</param>
  /// <param name="chroma">Out Munsell chroma.</param>
  internal static void Inverse(float vIndexF, float targetX, float targetY, out float hueF, out float chroma) {
    // Initial estimate: polar coordinates relative to illuminant C, then a coarse scan over
    // hue slots at the nearest Value slice to find a good seed.
    var dx = targetX - IlluminantC_x;
    var dy = targetY - IlluminantC_y;
    var rho = MathF.Sqrt(dx * dx + dy * dy);
    if (rho < 1e-6f) {
      hueF = 0f;
      chroma = 0f;
      return;
    }

    // Coarse seed: pick the nearest sampled chip on the bracketing Value slice.
    var v0 = (int)vIndexF;
    if (v0 < 0) v0 = 0;
    if (v0 >= ValueLevelCount - 1) v0 = ValueLevelCount - 2;
    var v1 = v0 + 1;
    var tv = vIndexF - v0;
    var vSeed = tv < 0.5f ? v0 : v1;

    var bestDist = float.MaxValue;
    var bestH = 0;
    var bestCi = 0;
    for (var hh = 0; hh < HueSlotCount; ++hh) {
      // MaxChroma returns chroma in raw Munsell units (e.g. 14); convert to chroma index count.
      var maxCi = MaxChroma(vSeed, hh) >> 1;
      for (var k = 0; k < maxCi; ++k) {
        GetChip(vSeed, hh, k, out var sx, out var sy);
        if (float.IsNaN(sx)) continue;
        var ddx = sx - targetX;
        var ddy = sy - targetY;
        var d2 = ddx * ddx + ddy * ddy;
        if (d2 < bestDist) {
          bestDist = d2;
          bestH = hh;
          bestCi = k;
        }
      }
    }

    var hSeed = (float)bestH;
    var cSeed = (bestCi + 1) * 2f;

    // Newton-style refinement on the forward map at the interpolated Value.
    // Compute Jacobian via central differences in (H, C); update; clamp.
    const int MaxIters = 12;
    const float Eps = 1e-6f;
    var h = hSeed;
    var c = cSeed;

    for (var iter = 0; iter < MaxIters; ++iter) {
      Forward(vIndexF, h, c, out var fx, out var fy, out _);
      var ex = fx - targetX;
      var ey = fy - targetY;
      if (ex * ex + ey * ey < Eps * Eps) break;

      // Numerical Jacobian (central differences on hue, forward diff on chroma to handle C=0).
      const float dh = 0.05f;
      const float dc = 0.1f;
      Forward(vIndexF, h + dh, c, out var fxhp, out var fyhp, out _);
      Forward(vIndexF, h - dh, c, out var fxhm, out var fyhm, out _);
      var dxdH = (fxhp - fxhm) / (2f * dh);
      var dydH = (fyhp - fyhm) / (2f * dh);

      Forward(vIndexF, h, c + dc, out var fxcp, out var fycp, out _);
      Forward(vIndexF, h, c - dc < 0 ? 0 : c - dc, out var fxcm, out var fycm, out _);
      var dcSpan = c - dc < 0 ? c + dc : 2f * dc;
      if (dcSpan < 1e-6f) dcSpan = dc; // chroma collapsed
      var dxdC = (fxcp - fxcm) / dcSpan;
      var dydC = (fycp - fycm) / dcSpan;

      var det = dxdH * dydC - dxdC * dydH;
      if (MathF.Abs(det) < 1e-9f) break; // singular; bail and use current best

      var stepH = (dydC * (-ex) - dxdC * (-ey)) / det;
      var stepC = (-dydH * (-ex) + dxdH * (-ey)) / det;

      // Damping: limit single-step magnitude to avoid overshoots in flat regions.
      const float maxHueStep = 4f;
      const float maxChromaStep = 4f;
      if (stepH > maxHueStep) stepH = maxHueStep;
      else if (stepH < -maxHueStep) stepH = -maxHueStep;
      if (stepC > maxChromaStep) stepC = maxChromaStep;
      else if (stepC < -maxChromaStep) stepC = -maxChromaStep;

      h += stepH;
      c += stepC;

      // Wrap hue cyclically; keep chroma non-negative.
      while (h < 0f) h += HueSlotCount;
      while (h >= HueSlotCount) h -= HueSlotCount;
      if (c < 0f) c = 0f;
    }

    hueF = h;
    chroma = c;
  }

  /// <summary>
  /// Solves the ASTM D1535-08 Y(V) polynomial for V given Y in [0..1.0257].
  /// The polynomial Y_ASTM(V) = 1.1914 V - 0.22533 V^2 + 0.23352 V^3 - 0.020484 V^4 + 0.00081939 V^5
  /// takes V in [0..10] and returns Y/100. The renotation table multiplies by 1.0257 (MgO white).
  /// </summary>
  /// <param name="bigY">Relative luminance in [0..1.0257] under illuminant C.</param>
  /// <returns>Corresponding fractional Value level index (0..13) into <see cref="ValueLevels"/>.</returns>
  internal static float InvertValueByY(float bigY) {
    if (bigY <= 0f) return 0f;
    if (bigY >= YByValueLevel[ValueLevelCount - 1]) return ValueLevelCount - 1;

    // First, recover the actual Munsell V (1..10 scale) by Newton on the polynomial.
    // Y_renotation = (1.1914 V - 0.22533 V^2 + 0.23352 V^3 - 0.020484 V^4 + 0.00081939 V^5) * 1.0257 / 100
    var v = MunsellValueFromY(bigY);

    // Then map V back onto the discrete-level index space used by the table.
    return ValueToLevelIndex(v);
  }

  /// <summary>Computes Munsell Value from Y (relative; Y(V=10) ~= 1.0257).</summary>
  private static float MunsellValueFromY(float bigY) {
    // Convert to ASTM polynomial scale: Y_ASTM = bigY * 100 / 1.0257
    var yAstm = bigY * (100f / 1.0257f);
    if (yAstm <= 0f) return 0f;
    if (yAstm >= 100f) return 10f;

    // Newton iteration on f(V) = poly(V) - yAstm.
    var v = MathF.Pow(yAstm * 0.01f, 0.45f) * 10f; // crude seed
    if (v < 0.01f) v = 0.01f;
    else if (v > 9.99f) v = 9.99f;

    for (var iter = 0; iter < 30; ++iter) {
      // poly(V) = 1.1914 V - 0.22533 V^2 + 0.23352 V^3 - 0.020484 V^4 + 0.00081939 V^5
      var v2 = v * v;
      var v3 = v2 * v;
      var v4 = v3 * v;
      var v5 = v4 * v;
      var f = 1.1914f * v - 0.22533f * v2 + 0.23352f * v3 - 0.020484f * v4 + 0.00081939f * v5 - yAstm;
      var fp = 1.1914f - 0.45066f * v + 0.70056f * v2 - 0.081936f * v3 + 0.00409695f * v4;
      if (MathF.Abs(fp) < 1e-9f) break;
      var step = f / fp;
      v -= step;
      if (v < 0f) v = 0f;
      else if (v > 10f) v = 10f;
      if (MathF.Abs(step) < 1e-6f) break;
    }
    return v;
  }

  /// <summary>Maps a continuous Munsell Value V (0..10) to its index into <see cref="ValueLevels"/>.</summary>
  private static float ValueToLevelIndex(float v) {
    // ValueLevels = {0.2, 0.4, 0.6, 0.8, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
    if (v <= ValueLevels[0]) return 0f;
    for (var i = 0; i < ValueLevelCount - 1; ++i) {
      var lo = ValueLevels[i];
      var hi = ValueLevels[i + 1];
      if (v <= hi) return i + (v - lo) / (hi - lo);
    }
    return ValueLevelCount - 1;
  }

  /// <summary>Inverse of <see cref="ValueToLevelIndex"/>.</summary>
  internal static float LevelIndexToValue(float vIndexF) {
    if (vIndexF <= 0f) return ValueLevels[0];
    if (vIndexF >= ValueLevelCount - 1) return ValueLevels[ValueLevelCount - 1];
    var i = (int)vIndexF;
    var t = vIndexF - i;
    return ValueLevels[i] + (ValueLevels[i + 1] - ValueLevels[i]) * t;
  }

  /// <summary>Computes Y (relative; V=10 -> 1.0257) from a continuous Munsell Value V (0..10).</summary>
  internal static float YFromMunsellValue(float v) {
    if (v <= 0f) return 0f;
    if (v >= 10f) return 1.0257f;
    var v2 = v * v;
    var v3 = v2 * v;
    var v4 = v3 * v;
    var v5 = v4 * v;
    var yAstm = 1.1914f * v - 0.22533f * v2 + 0.23352f * v3 - 0.020484f * v4 + 0.00081939f * v5;
    return yAstm * 1.0257f / 100f;
  }

  // -- Hue parsing --------------------------------------------------------------

  private static readonly string[] HueFamilies = { "R", "YR", "Y", "GY", "G", "BG", "B", "PB", "P", "RP" };

  /// <summary>
  /// Parses a renotation hue string like "2.5R" / "5YR" / "10BG" into a 0..40 cyclic index where
  /// 0 = 2.5R, 1 = 5R, 2 = 7.5R, 3 = 10R, 4 = 2.5YR, ..., 39 = 10RP.
  /// </summary>
  private static bool TryParseHue(string hue, out int index) {
    // Split numeric prefix and family suffix.
    var splitAt = 0;
    while (splitAt < hue.Length && (char.IsDigit(hue[splitAt]) || hue[splitAt] == '.'))
      ++splitAt;
    if (splitAt == 0 || splitAt == hue.Length) {
      index = 0;
      return false;
    }
    var numPart = hue.Substring(0, splitAt);
    var famPart = hue.Substring(splitAt);
    if (!float.TryParse(numPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var prefix)) {
      index = 0;
      return false;
    }
    var family = -1;
    for (var i = 0; i < HueFamilies.Length; ++i)
      if (HueFamilies[i] == famPart) {
        family = i;
        break;
      }
    if (family < 0) {
      index = 0;
      return false;
    }
    // Prefix 2.5 -> 0, 5 -> 1, 7.5 -> 2, 10 -> 3.
    var p = prefix == 2.5f ? 0 : prefix == 5f ? 1 : prefix == 7.5f ? 2 : prefix == 10f ? 3 : -1;
    if (p < 0) {
      index = 0;
      return false;
    }
    index = family * 4 + p;
    return true;
  }

  // -- Resource loader ----------------------------------------------------------

  private static readonly char[] _whitespaceSeparators = { ' ', '\t' };

  private static string[] SplitWhitespace(string line) =>
    line.Split(_whitespaceSeparators, StringSplitOptions.RemoveEmptyEntries);


  private static TableData LoadFromEmbeddedResource() {
    var data = new TableData();
    for (var i = 0; i < data.X.Length; ++i) {
      data.X[i] = float.NaN;
      data.Y[i] = float.NaN;
    }

    var asm = typeof(MunsellRenotationTable).Assembly;
    const string ResourceName = "Hawkynt.ColorProcessing.Spaces.Perceptual.MunsellRenotation.dat";
    using var stream = asm.GetManifestResourceStream(ResourceName);
    if (stream == null)
      throw new InvalidOperationException(
        "Embedded resource '" + ResourceName + "' not found. Check the .csproj <EmbeddedResource> entry.");
    using var reader = new StreamReader(stream);

    var first = true;
    while (true) {
      var line = reader.ReadLine();
      if (line == null) break;
      if (first) {
        first = false;
        // Header row: "H V C x y Y"
        if (line.Contains("H") && line.Contains("V"))
          continue;
      }
      var parts = SplitWhitespace(line);
      if (parts.Length != 6) continue;
      if (!TryParseHue(parts[0], out var hueIdx)) continue;
      if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var vRaw)) continue;
      if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var chromaInt)) continue;
      if (!float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) continue;
      if (!float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) continue;
      // We don't read Y here; it's encoded in YByValueLevel.

      // Match the V to its level index.
      var vLevel = -1;
      for (var i = 0; i < ValueLevelCount; ++i)
        if (MathF.Abs(ValueLevels[i] - vRaw) < 1e-3f) {
          vLevel = i;
          break;
        }
      if (vLevel < 0) continue;
      if (chromaInt < 2 || chromaInt > MaxChromaIndex * 2) continue;
      if ((chromaInt & 1) != 0) continue; // odd chroma not in renotation
      var ci = (chromaInt >> 1) - 1;

      var idx = vLevel * HueSlotCount * MaxChromaIndex + hueIdx * MaxChromaIndex + ci;
      data.X[idx] = x;
      data.Y[idx] = y;

      var mcIdx = vLevel * HueSlotCount + hueIdx;
      if (chromaInt > data.MaxChroma[mcIdx])
        data.MaxChroma[mcIdx] = (byte)chromaInt;
    }

    return data;
  }
}
