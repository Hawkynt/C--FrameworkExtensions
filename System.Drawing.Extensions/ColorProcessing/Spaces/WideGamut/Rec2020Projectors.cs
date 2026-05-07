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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.WideGamut;

/// <summary>
/// Projects LinearRgbF (linear sRGB, D65) to Rec2020F (linear BT.2020, D65).
/// </summary>
/// <remarks>
/// Direct primary-change matrix (no chromatic adaptation needed since both spaces share
/// D65 white). Coefficients per ITU-R BT.2087 / Lindbloom's RGB-XYZ matrix tables for
/// the BT.2020 chromaticities R=(0.708, 0.292), G=(0.170, 0.797), B=(0.131, 0.046).
/// </remarks>
public readonly struct LinearRgbFToRec2020F : IProject<LinearRgbF, Rec2020F> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Rec2020F Project(in LinearRgbF work) => new(
    Rec2020Matrix.M00 * work.R + Rec2020Matrix.M01 * work.G + Rec2020Matrix.M02 * work.B,
    Rec2020Matrix.M10 * work.R + Rec2020Matrix.M11 * work.G + Rec2020Matrix.M12 * work.B,
    Rec2020Matrix.M20 * work.R + Rec2020Matrix.M21 * work.G + Rec2020Matrix.M22 * work.B
  );
}

/// <summary>
/// Projects LinearRgbaF to Rec2020F (drops alpha; Rec2020F is 3-component).
/// </summary>
public readonly struct LinearRgbaFToRec2020F : IProject<LinearRgbaF, Rec2020F> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Rec2020F Project(in LinearRgbaF work) => new(
    Rec2020Matrix.M00 * work.R + Rec2020Matrix.M01 * work.G + Rec2020Matrix.M02 * work.B,
    Rec2020Matrix.M10 * work.R + Rec2020Matrix.M11 * work.G + Rec2020Matrix.M12 * work.B,
    Rec2020Matrix.M20 * work.R + Rec2020Matrix.M21 * work.G + Rec2020Matrix.M22 * work.B
  );
}

/// <summary>
/// Projects Rec2020F (linear BT.2020) back to LinearRgbF (linear sRGB).
/// </summary>
/// <remarks>
/// Inverse of the BT.2020 primary matrix. Out-of-sRGB-gamut input colours produce
/// negative components — caller is responsible for gamut clamping if rendering to sRGB.
/// </remarks>
public readonly struct Rec2020FToLinearRgbF : IProject<Rec2020F, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in Rec2020F rec2020) => new(
    Rec2020Matrix.I00 * rec2020.R + Rec2020Matrix.I01 * rec2020.G + Rec2020Matrix.I02 * rec2020.B,
    Rec2020Matrix.I10 * rec2020.R + Rec2020Matrix.I11 * rec2020.G + Rec2020Matrix.I12 * rec2020.B,
    Rec2020Matrix.I20 * rec2020.R + Rec2020Matrix.I21 * rec2020.G + Rec2020Matrix.I22 * rec2020.B
  );
}

/// <summary>
/// sRGB ↔ BT.2020 primary-change matrix coefficients (D65 → D65).
/// </summary>
internal static class Rec2020Matrix {

  // sRGB linear → BT.2020 linear (D65 → D65, no chromatic adaptation).
  // Source: ITU-R BT.2087 Table 1; matches Lindbloom's calculator output for the
  // BT.2020 primary chromaticities.
  public const float M00 = 0.627404078712f;
  public const float M01 = 0.329282836970f;
  public const float M02 = 0.043313084318f;
  public const float M10 = 0.069097291817f;
  public const float M11 = 0.919541039598f;
  public const float M12 = 0.011361668585f;
  public const float M20 = 0.016391438857f;
  public const float M21 = 0.088013321855f;
  public const float M22 = 0.895595239288f;

  // BT.2020 linear → sRGB linear (inverse of the above).
  public const float I00 =  1.660491006601f;
  public const float I01 = -0.587641148949f;
  public const float I02 = -0.072849857651f;
  public const float I10 = -0.124550473953f;
  public const float I11 =  1.132898332135f;
  public const float I12 = -0.008347858182f;
  public const float I20 = -0.018150767009f;
  public const float I21 = -0.100578891323f;
  public const float I22 =  1.118729658332f;
}
