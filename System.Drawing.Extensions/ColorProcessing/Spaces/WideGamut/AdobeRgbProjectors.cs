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
/// Projects LinearRgbF (sRGB linear) to AdobeRgbF.
/// </summary>
/// <remarks>
/// Converts sRGB linear to Adobe RGB linear via XYZ (D65).
/// Both spaces use D65, so no chromatic adaptation is needed.
/// </remarks>
public readonly struct LinearRgbFToAdobeRgbF : IProject<LinearRgbF, AdobeRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AdobeRgbF Project(in LinearRgbF work) {
    // sRGB linear to XYZ
    var x = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    // XYZ to Adobe RGB linear
    return new(
      ColorMatrices.XyzToAdobe_RX * x + ColorMatrices.XyzToAdobe_RY * y + ColorMatrices.XyzToAdobe_RZ * z,
      ColorMatrices.XyzToAdobe_GX * x + ColorMatrices.XyzToAdobe_GY * y + ColorMatrices.XyzToAdobe_GZ * z,
      ColorMatrices.XyzToAdobe_BX * x + ColorMatrices.XyzToAdobe_BY * y + ColorMatrices.XyzToAdobe_BZ * z
    );
  }
}

/// <summary>
/// Projects LinearRgbaF (sRGB linear) to AdobeRgbF.
/// </summary>
public readonly struct LinearRgbaFToAdobeRgbF : IProject<LinearRgbaF, AdobeRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AdobeRgbF Project(in LinearRgbaF work) {
    var x = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    return new(
      ColorMatrices.XyzToAdobe_RX * x + ColorMatrices.XyzToAdobe_RY * y + ColorMatrices.XyzToAdobe_RZ * z,
      ColorMatrices.XyzToAdobe_GX * x + ColorMatrices.XyzToAdobe_GY * y + ColorMatrices.XyzToAdobe_GZ * z,
      ColorMatrices.XyzToAdobe_BX * x + ColorMatrices.XyzToAdobe_BY * y + ColorMatrices.XyzToAdobe_BZ * z
    );
  }
}

/// <summary>
/// Projects AdobeRgbF back to LinearRgbF (sRGB linear).
/// </summary>
public readonly struct AdobeRgbFToLinearRgbF : IProject<AdobeRgbF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in AdobeRgbF adobe) {
    // Adobe RGB linear to XYZ
    var x = ColorMatrices.AdobeToXyz_XR * adobe.R + ColorMatrices.AdobeToXyz_XG * adobe.G + ColorMatrices.AdobeToXyz_XB * adobe.B;
    var y = ColorMatrices.AdobeToXyz_YR * adobe.R + ColorMatrices.AdobeToXyz_YG * adobe.G + ColorMatrices.AdobeToXyz_YB * adobe.B;
    var z = ColorMatrices.AdobeToXyz_ZR * adobe.R + ColorMatrices.AdobeToXyz_ZG * adobe.G + ColorMatrices.AdobeToXyz_ZB * adobe.B;

    // XYZ to sRGB linear
    return new(
      ColorMatrices.XyzToRgb_RX * x + ColorMatrices.XyzToRgb_RY * y + ColorMatrices.XyzToRgb_RZ * z,
      ColorMatrices.XyzToRgb_GX * x + ColorMatrices.XyzToRgb_GY * y + ColorMatrices.XyzToRgb_GZ * z,
      ColorMatrices.XyzToRgb_BX * x + ColorMatrices.XyzToRgb_BY * y + ColorMatrices.XyzToRgb_BZ * z
    );
  }
}
