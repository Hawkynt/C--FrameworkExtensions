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
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Spaces.Hdr;
using Hawkynt.ColorProcessing.Spaces.Lab;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

/// <summary>
/// Projects <see cref="XyzF"/> (D65) to <see cref="MunsellF"/> via the ASTM D1535 renotation table.
/// </summary>
/// <remarks>
/// <para>Inputs are interpreted as XYZ relative to D65; a Bradford CAT D65 -&gt; C is applied so
/// that the renotation lookup operates in its native illuminant. The forward steps:</para>
/// <list type="number">
///   <item><description>Adapt XYZ from D65 to illuminant C (Bradford).</description></item>
///   <item><description>Invert the ASTM D1535-08 Y(V) polynomial to recover Munsell Value.</description></item>
///   <item><description>Convert XYZ -&gt; xy chromaticity at the recovered Value.</description></item>
///   <item><description>Refine (Hue, Chroma) by 2D Newton against the table's bilinear (H, C) map.</description></item>
/// </list>
/// <para>Output is normalised to the <see cref="MunsellF"/> 0..1 component range:
/// H = (idx + 39) mod 40 / 40 (so H=0 is 5R), V = munsellValue / 10, C = chroma / 30.</para>
/// </remarks>
public readonly struct XyzFToMunsellF : IProject<XyzF, MunsellF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public MunsellF Project(in XyzF xyzD65) {
    // Adapt D65 -> C.
    var x = ColorMatrices.Brad65ToC_XX * xyzD65.X + ColorMatrices.Brad65ToC_XY * xyzD65.Y + ColorMatrices.Brad65ToC_XZ * xyzD65.Z;
    var y = ColorMatrices.Brad65ToC_YX * xyzD65.X + ColorMatrices.Brad65ToC_YY * xyzD65.Y + ColorMatrices.Brad65ToC_YZ * xyzD65.Z;
    var z = ColorMatrices.Brad65ToC_ZX * xyzD65.X + ColorMatrices.Brad65ToC_ZY * xyzD65.Y + ColorMatrices.Brad65ToC_ZZ * xyzD65.Z;

    var sum = x + y + z;
    if (sum < 1e-9f || y < 1e-9f) return new(0f, 0f, 0f);

    var cx = x / sum;
    var cy = y / sum;

    // Recover Munsell Value from Y. Y here is in [0..1.0257] illuminant-C-relative.
    var vIndexF = MunsellRenotationTable.InvertValueByY(y);
    var munsellValue = MunsellRenotationTable.LevelIndexToValue(vIndexF);

    MunsellRenotationTable.Inverse(vIndexF, cx, cy, out var hueF, out var chroma);

    // Convert hue index 0..40 (where 0 = 2.5R) to API H 0..1 (where 0 = 5R).
    // 5R is hueF = 1, so apiH = (hueF - 1) / 40 mod 1.
    var apiH = (hueF - 1f) / MunsellRenotationTable.HueSlotCount;
    while (apiH < 0f) apiH += 1f;
    while (apiH >= 1f) apiH -= 1f;

    return new(apiH, munsellValue * 0.1f, chroma * (1f / 30f));
  }
}

/// <summary>
/// Projects <see cref="MunsellF"/> back to <see cref="XyzF"/> (D65) via the ASTM D1535 renotation
/// table.
/// </summary>
/// <remarks>
/// <para>Trilinear (Hue, Value, Chroma) lookup yields xy under illuminant C; the recovered
/// Value sets Y via the ASTM polynomial. Result is composed back to XYZ and adapted C -&gt; D65
/// (Bradford) so the rest of the library's D65-based pipeline sees consistent values.</para>
/// </remarks>
public readonly struct MunsellFToXyzF : IProject<MunsellF, XyzF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XyzF Project(in MunsellF m) {
    // Convert API H (0=5R) to renotation hue index (0=2.5R, 1=5R, ...).
    var hueF = m.H * MunsellRenotationTable.HueSlotCount + 1f;
    var munsellValue = m.V * 10f;
    var chroma = m.C * 30f;

    var vIndexF = MunsellValueToLevelIndex(munsellValue);

    MunsellRenotationTable.Forward(vIndexF, hueF, chroma, out var cx, out var cy, out var bigY);

    if (bigY < 1e-9f || cy < 1e-9f) return new(0f, 0f, 0f);
    var X_C = bigY * cx / cy;
    var Y_C = bigY;
    var Z_C = bigY * (1f - cx - cy) / cy;

    // Adapt C -> D65.
    return new(
      ColorMatrices.BradCTo65_XX * X_C + ColorMatrices.BradCTo65_XY * Y_C + ColorMatrices.BradCTo65_XZ * Z_C,
      ColorMatrices.BradCTo65_YX * X_C + ColorMatrices.BradCTo65_YY * Y_C + ColorMatrices.BradCTo65_YZ * Z_C,
      ColorMatrices.BradCTo65_ZX * X_C + ColorMatrices.BradCTo65_ZY * Y_C + ColorMatrices.BradCTo65_ZZ * Z_C
    );
  }

  // Munsell V (0..10) -> level index (0..13). Inverse of MunsellRenotationTable.LevelIndexToValue.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float MunsellValueToLevelIndex(float v) {
    var levels = MunsellRenotationTable.ValueLevels;
    if (v <= levels[0]) return 0f;
    if (v >= levels[MunsellRenotationTable.ValueLevelCount - 1]) return MunsellRenotationTable.ValueLevelCount - 1;
    for (var i = 0; i < MunsellRenotationTable.ValueLevelCount - 1; ++i) {
      var lo = levels[i];
      var hi = levels[i + 1];
      if (v <= hi) return i + (v - lo) / (hi - lo);
    }
    return MunsellRenotationTable.ValueLevelCount - 1;
  }
}

/// <summary>
/// Projects <see cref="LinearRgbF"/> (assumed sRGB-linear, D65) to <see cref="MunsellF"/> via XYZ.
/// </summary>
public readonly struct LinearRgbFToMunsellF : IProject<LinearRgbF, MunsellF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public MunsellF Project(in LinearRgbF work) {
    var xyz = new LinearRgbFToXyzF().Project(work);
    return new XyzFToMunsellF().Project(xyz);
  }
}

/// <summary>
/// Projects <see cref="LinearRgbaF"/> (assumed sRGB-linear, D65) to <see cref="MunsellF"/> via XYZ.
/// </summary>
public readonly struct LinearRgbaFToMunsellF : IProject<LinearRgbaF, MunsellF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public MunsellF Project(in LinearRgbaF work) {
    var xyz = new LinearRgbaFToXyzF().Project(work);
    return new XyzFToMunsellF().Project(xyz);
  }
}

/// <summary>
/// Projects <see cref="MunsellF"/> back to <see cref="LinearRgbF"/> (sRGB-linear, D65) via XYZ.
/// </summary>
public readonly struct MunsellFToLinearRgbF : IProject<MunsellF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in MunsellF m) {
    var xyz = new MunsellFToXyzF().Project(m);
    return new XyzFToLinearRgbF().Project(xyz);
  }
}

/// <summary>
/// Projects <see cref="LabF"/> (D65) to <see cref="MunsellF"/> via XYZ.
/// </summary>
/// <remarks>
/// <para>Composed projector kept for backwards source-compatibility with the previous analytic
/// implementation: applications that already projected through <c>LabFToMunsellF</c> continue
/// to work, but the path is now table-backed so accuracy improves dramatically.</para>
/// </remarks>
public readonly struct LabFToMunsellF : IProject<LabF, MunsellF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public MunsellF Project(in LabF lab) {
    // Lab -> XYZ (D65)
    var fy = (lab.L + 16f) * ColorMatrices.Inv116;
    var fx = lab.A * ColorMatrices.Inv500 + fy;
    var fz = fy - lab.B * ColorMatrices.Inv200;

    var x = fx > ColorMatrices.Lab_Delta ? fx * fx * fx : (fx - 16f / 116f) * 3f * ColorMatrices.Lab_Delta * ColorMatrices.Lab_Delta;
    var y = lab.L > 8f ? fy * fy * fy : lab.L / ColorMatrices.Lab_Kappa;
    var z = fz > ColorMatrices.Lab_Delta ? fz * fz * fz : (fz - 16f / 116f) * 3f * ColorMatrices.Lab_Delta * ColorMatrices.Lab_Delta;

    var X = x * ColorMatrices.D65_Xn;
    var Y = y * ColorMatrices.D65_Yn;
    var Z = z * ColorMatrices.D65_Zn;

    return new XyzFToMunsellF().Project(new XyzF(X, Y, Z));
  }
}

/// <summary>
/// Projects <see cref="MunsellF"/> back to <see cref="LabF"/> (D65) via XYZ.
/// </summary>
public readonly struct MunsellFToLabF : IProject<MunsellF, LabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LabF Project(in MunsellF m) {
    var xyz = new MunsellFToXyzF().Project(m);
    var x = xyz.X * ColorMatrices.Inv_D65_Xn;
    var y = xyz.Y * ColorMatrices.Inv_D65_Yn;
    var z = xyz.Z * ColorMatrices.Inv_D65_Zn;

    var fx = x > ColorMatrices.Lab_Epsilon ? MathF.Cbrt(x) : (ColorMatrices.Lab_Kappa * x + 16f) / 116f;
    var fy = y > ColorMatrices.Lab_Epsilon ? MathF.Cbrt(y) : (ColorMatrices.Lab_Kappa * y + 16f) / 116f;
    var fz = z > ColorMatrices.Lab_Epsilon ? MathF.Cbrt(z) : (ColorMatrices.Lab_Kappa * z + 16f) / 116f;

    return new(116f * fy - 16f, 500f * (fx - fy), 200f * (fy - fz));
  }
}
