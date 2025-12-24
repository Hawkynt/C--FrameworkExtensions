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
/// Projects LinearRgbF (sRGB linear) to ProPhotoRgbF.
/// </summary>
/// <remarks>
/// Converts sRGB linear to ProPhoto RGB linear via XYZ.
/// Includes Bradford chromatic adaptation from D65 to D50.
/// </remarks>
public readonly struct LinearRgbFToProPhotoRgbF : IProject<LinearRgbF, ProPhotoRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ProPhotoRgbF Project(in LinearRgbF work) {
    // sRGB linear to XYZ (D65)
    var x65 = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y65 = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z65 = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    // Bradford adaptation D65 to D50
    var x50 = ColorMatrices.Brad65To50_XX * x65 + ColorMatrices.Brad65To50_XY * y65 + ColorMatrices.Brad65To50_XZ * z65;
    var y50 = ColorMatrices.Brad65To50_YX * x65 + ColorMatrices.Brad65To50_YY * y65 + ColorMatrices.Brad65To50_YZ * z65;
    var z50 = ColorMatrices.Brad65To50_ZX * x65 + ColorMatrices.Brad65To50_ZY * y65 + ColorMatrices.Brad65To50_ZZ * z65;

    // XYZ (D50) to ProPhoto RGB
    return new(
      ColorMatrices.XyzToProPhoto_RX * x50 + ColorMatrices.XyzToProPhoto_RY * y50 + ColorMatrices.XyzToProPhoto_RZ * z50,
      ColorMatrices.XyzToProPhoto_GX * x50 + ColorMatrices.XyzToProPhoto_GY * y50 + ColorMatrices.XyzToProPhoto_GZ * z50,
      ColorMatrices.XyzToProPhoto_BX * x50 + ColorMatrices.XyzToProPhoto_BY * y50 + ColorMatrices.XyzToProPhoto_BZ * z50
    );
  }
}

/// <summary>
/// Projects LinearRgbaF (sRGB linear) to ProPhotoRgbF.
/// </summary>
public readonly struct LinearRgbaFToProPhotoRgbF : IProject<LinearRgbaF, ProPhotoRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ProPhotoRgbF Project(in LinearRgbaF work) {
    var x65 = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y65 = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z65 = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    var x50 = ColorMatrices.Brad65To50_XX * x65 + ColorMatrices.Brad65To50_XY * y65 + ColorMatrices.Brad65To50_XZ * z65;
    var y50 = ColorMatrices.Brad65To50_YX * x65 + ColorMatrices.Brad65To50_YY * y65 + ColorMatrices.Brad65To50_YZ * z65;
    var z50 = ColorMatrices.Brad65To50_ZX * x65 + ColorMatrices.Brad65To50_ZY * y65 + ColorMatrices.Brad65To50_ZZ * z65;

    return new(
      ColorMatrices.XyzToProPhoto_RX * x50 + ColorMatrices.XyzToProPhoto_RY * y50 + ColorMatrices.XyzToProPhoto_RZ * z50,
      ColorMatrices.XyzToProPhoto_GX * x50 + ColorMatrices.XyzToProPhoto_GY * y50 + ColorMatrices.XyzToProPhoto_GZ * z50,
      ColorMatrices.XyzToProPhoto_BX * x50 + ColorMatrices.XyzToProPhoto_BY * y50 + ColorMatrices.XyzToProPhoto_BZ * z50
    );
  }
}

/// <summary>
/// Projects ProPhotoRgbF back to LinearRgbF (sRGB linear).
/// </summary>
public readonly struct ProPhotoRgbFToLinearRgbF : IProject<ProPhotoRgbF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in ProPhotoRgbF pp) {
    // ProPhoto RGB to XYZ (D50)
    var x50 = ColorMatrices.ProPhotoToXyz_XR * pp.R + ColorMatrices.ProPhotoToXyz_XG * pp.G + ColorMatrices.ProPhotoToXyz_XB * pp.B;
    var y50 = ColorMatrices.ProPhotoToXyz_YR * pp.R + ColorMatrices.ProPhotoToXyz_YG * pp.G + ColorMatrices.ProPhotoToXyz_YB * pp.B;
    var z50 = ColorMatrices.ProPhotoToXyz_ZR * pp.R + ColorMatrices.ProPhotoToXyz_ZG * pp.G + ColorMatrices.ProPhotoToXyz_ZB * pp.B;

    // Bradford adaptation D50 to D65
    var x65 = ColorMatrices.Brad50To65_XX * x50 + ColorMatrices.Brad50To65_XY * y50 + ColorMatrices.Brad50To65_XZ * z50;
    var y65 = ColorMatrices.Brad50To65_YX * x50 + ColorMatrices.Brad50To65_YY * y50 + ColorMatrices.Brad50To65_YZ * z50;
    var z65 = ColorMatrices.Brad50To65_ZX * x50 + ColorMatrices.Brad50To65_ZY * y50 + ColorMatrices.Brad50To65_ZZ * z50;

    // XYZ (D65) to sRGB linear
    return new(
      ColorMatrices.XyzToRgb_RX * x65 + ColorMatrices.XyzToRgb_RY * y65 + ColorMatrices.XyzToRgb_RZ * z65,
      ColorMatrices.XyzToRgb_GX * x65 + ColorMatrices.XyzToRgb_GY * y65 + ColorMatrices.XyzToRgb_GZ * z65,
      ColorMatrices.XyzToRgb_BX * x65 + ColorMatrices.XyzToRgb_BY * y65 + ColorMatrices.XyzToRgb_BZ * z65
    );
  }
}
