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
/// Projects LinearRgbF (sRGB linear) to DisplayP3F.
/// </summary>
/// <remarks>
/// Converts sRGB linear to Display P3 linear via XYZ (D65).
/// Display P3 uses DCI-P3 primaries with D65 white point.
/// </remarks>
public readonly struct LinearRgbFToDisplayP3F : IProject<LinearRgbF, DisplayP3F> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public DisplayP3F Project(in LinearRgbF work) {
    // sRGB linear to XYZ
    var x = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    // XYZ to Display P3 linear
    return new(
      ColorMatrices.XyzToP3_RX * x + ColorMatrices.XyzToP3_RY * y + ColorMatrices.XyzToP3_RZ * z,
      ColorMatrices.XyzToP3_GX * x + ColorMatrices.XyzToP3_GY * y + ColorMatrices.XyzToP3_GZ * z,
      ColorMatrices.XyzToP3_BX * x + ColorMatrices.XyzToP3_BY * y + ColorMatrices.XyzToP3_BZ * z
    );
  }
}

/// <summary>
/// Projects LinearRgbaF (sRGB linear) to DisplayP3F.
/// </summary>
public readonly struct LinearRgbaFToDisplayP3F : IProject<LinearRgbaF, DisplayP3F> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public DisplayP3F Project(in LinearRgbaF work) {
    var x = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    return new(
      ColorMatrices.XyzToP3_RX * x + ColorMatrices.XyzToP3_RY * y + ColorMatrices.XyzToP3_RZ * z,
      ColorMatrices.XyzToP3_GX * x + ColorMatrices.XyzToP3_GY * y + ColorMatrices.XyzToP3_GZ * z,
      ColorMatrices.XyzToP3_BX * x + ColorMatrices.XyzToP3_BY * y + ColorMatrices.XyzToP3_BZ * z
    );
  }
}

/// <summary>
/// Projects DisplayP3F back to LinearRgbF (sRGB linear).
/// </summary>
public readonly struct DisplayP3FToLinearRgbF : IProject<DisplayP3F, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in DisplayP3F p3) {
    // Display P3 linear to XYZ
    var x = ColorMatrices.P3ToXyz_XR * p3.R + ColorMatrices.P3ToXyz_XG * p3.G + ColorMatrices.P3ToXyz_XB * p3.B;
    var y = ColorMatrices.P3ToXyz_YR * p3.R + ColorMatrices.P3ToXyz_YG * p3.G + ColorMatrices.P3ToXyz_YB * p3.B;
    var z = ColorMatrices.P3ToXyz_ZR * p3.R + ColorMatrices.P3ToXyz_ZG * p3.G + ColorMatrices.P3ToXyz_ZB * p3.B;

    // XYZ to sRGB linear
    return new(
      ColorMatrices.XyzToRgb_RX * x + ColorMatrices.XyzToRgb_RY * y + ColorMatrices.XyzToRgb_RZ * z,
      ColorMatrices.XyzToRgb_GX * x + ColorMatrices.XyzToRgb_GY * y + ColorMatrices.XyzToRgb_GZ * z,
      ColorMatrices.XyzToRgb_BX * x + ColorMatrices.XyzToRgb_BY * y + ColorMatrices.XyzToRgb_BZ * z
    );
  }
}
