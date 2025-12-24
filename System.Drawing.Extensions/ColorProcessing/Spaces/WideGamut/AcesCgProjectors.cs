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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.WideGamut;

/// <summary>
/// Projects LinearRgbF (sRGB linear) to AcesCgF.
/// </summary>
/// <remarks>
/// Converts sRGB linear to ACEScg via XYZ.
/// Includes Bradford chromatic adaptation from D65 to ACES white (~D60).
/// </remarks>
public readonly struct LinearRgbFToAcesCgF : IProject<LinearRgbF, AcesCgF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AcesCgF Project(in LinearRgbF work) {
    // sRGB linear to XYZ (D65)
    var x65 = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y65 = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z65 = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    // Bradford adaptation D65 to ACES white
    var xa = ColorMatrices.Brad65ToAces_XX * x65 + ColorMatrices.Brad65ToAces_XY * y65 + ColorMatrices.Brad65ToAces_XZ * z65;
    var ya = ColorMatrices.Brad65ToAces_YX * x65 + ColorMatrices.Brad65ToAces_YY * y65 + ColorMatrices.Brad65ToAces_YZ * z65;
    var za = ColorMatrices.Brad65ToAces_ZX * x65 + ColorMatrices.Brad65ToAces_ZY * y65 + ColorMatrices.Brad65ToAces_ZZ * z65;

    // XYZ to ACEScg
    return new(
      ColorMatrices.XyzToAcesCg_RX * xa + ColorMatrices.XyzToAcesCg_RY * ya + ColorMatrices.XyzToAcesCg_RZ * za,
      ColorMatrices.XyzToAcesCg_GX * xa + ColorMatrices.XyzToAcesCg_GY * ya + ColorMatrices.XyzToAcesCg_GZ * za,
      ColorMatrices.XyzToAcesCg_BX * xa + ColorMatrices.XyzToAcesCg_BY * ya + ColorMatrices.XyzToAcesCg_BZ * za
    );
  }
}

/// <summary>
/// Projects LinearRgbaF (sRGB linear) to AcesCgF.
/// </summary>
public readonly struct LinearRgbaFToAcesCgF : IProject<LinearRgbaF, AcesCgF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AcesCgF Project(in LinearRgbaF work) {
    var x65 = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y65 = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z65 = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    var xa = ColorMatrices.Brad65ToAces_XX * x65 + ColorMatrices.Brad65ToAces_XY * y65 + ColorMatrices.Brad65ToAces_XZ * z65;
    var ya = ColorMatrices.Brad65ToAces_YX * x65 + ColorMatrices.Brad65ToAces_YY * y65 + ColorMatrices.Brad65ToAces_YZ * z65;
    var za = ColorMatrices.Brad65ToAces_ZX * x65 + ColorMatrices.Brad65ToAces_ZY * y65 + ColorMatrices.Brad65ToAces_ZZ * z65;

    return new(
      ColorMatrices.XyzToAcesCg_RX * xa + ColorMatrices.XyzToAcesCg_RY * ya + ColorMatrices.XyzToAcesCg_RZ * za,
      ColorMatrices.XyzToAcesCg_GX * xa + ColorMatrices.XyzToAcesCg_GY * ya + ColorMatrices.XyzToAcesCg_GZ * za,
      ColorMatrices.XyzToAcesCg_BX * xa + ColorMatrices.XyzToAcesCg_BY * ya + ColorMatrices.XyzToAcesCg_BZ * za
    );
  }
}

/// <summary>
/// Projects AcesCgF back to LinearRgbF (sRGB linear).
/// </summary>
public readonly struct AcesCgFToLinearRgbF : IProject<AcesCgF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in AcesCgF aces) {
    // ACEScg to XYZ (ACES white)
    var xa = ColorMatrices.AcesCgToXyz_XR * aces.R + ColorMatrices.AcesCgToXyz_XG * aces.G + ColorMatrices.AcesCgToXyz_XB * aces.B;
    var ya = ColorMatrices.AcesCgToXyz_YR * aces.R + ColorMatrices.AcesCgToXyz_YG * aces.G + ColorMatrices.AcesCgToXyz_YB * aces.B;
    var za = ColorMatrices.AcesCgToXyz_ZR * aces.R + ColorMatrices.AcesCgToXyz_ZG * aces.G + ColorMatrices.AcesCgToXyz_ZB * aces.B;

    // Bradford adaptation ACES white to D65
    var x65 = ColorMatrices.BradAcesTo65_XX * xa + ColorMatrices.BradAcesTo65_XY * ya + ColorMatrices.BradAcesTo65_XZ * za;
    var y65 = ColorMatrices.BradAcesTo65_YX * xa + ColorMatrices.BradAcesTo65_YY * ya + ColorMatrices.BradAcesTo65_YZ * za;
    var z65 = ColorMatrices.BradAcesTo65_ZX * xa + ColorMatrices.BradAcesTo65_ZY * ya + ColorMatrices.BradAcesTo65_ZZ * za;

    // XYZ (D65) to sRGB linear
    return new(
      ColorMatrices.XyzToRgb_RX * x65 + ColorMatrices.XyzToRgb_RY * y65 + ColorMatrices.XyzToRgb_RZ * z65,
      ColorMatrices.XyzToRgb_GX * x65 + ColorMatrices.XyzToRgb_GY * y65 + ColorMatrices.XyzToRgb_GZ * z65,
      ColorMatrices.XyzToRgb_BX * x65 + ColorMatrices.XyzToRgb_BY * y65 + ColorMatrices.XyzToRgb_BZ * z65
    );
  }
}
