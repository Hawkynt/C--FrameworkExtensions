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

namespace Hawkynt.ColorProcessing.Spaces.Hdr;

/// <summary>
/// Projects LinearRgbF to XyzF using D65 illuminant.
/// </summary>
/// <remarks>
/// Uses the sRGB to XYZ matrix (D65 illuminant).
/// </remarks>
public readonly struct LinearRgbFToXyzF : IProject<LinearRgbF, XyzF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XyzF Project(in LinearRgbF work) => new(
    ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B,
    ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B,
    ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B
  );
}

/// <summary>
/// Projects LinearRgbaF to XyzF using D65 illuminant.
/// </summary>
public readonly struct LinearRgbaFToXyzF : IProject<LinearRgbaF, XyzF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XyzF Project(in LinearRgbaF work) => new(
    ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B,
    ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B,
    ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B
  );
}

/// <summary>
/// Projects XyzF back to LinearRgbF using D65 illuminant.
/// </summary>
public readonly struct XyzFToLinearRgbF : IProject<XyzF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in XyzF xyz) => new(
    ColorMatrices.XyzToRgb_RX * xyz.X + ColorMatrices.XyzToRgb_RY * xyz.Y + ColorMatrices.XyzToRgb_RZ * xyz.Z,
    ColorMatrices.XyzToRgb_GX * xyz.X + ColorMatrices.XyzToRgb_GY * xyz.Y + ColorMatrices.XyzToRgb_GZ * xyz.Z,
    ColorMatrices.XyzToRgb_BX * xyz.X + ColorMatrices.XyzToRgb_BY * xyz.Y + ColorMatrices.XyzToRgb_BZ * xyz.Z
  );
}
