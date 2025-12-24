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
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Lab;

/// <summary>
/// Projects LinearRgbF to LabF using D65 illuminant.
/// </summary>
/// <remarks>
/// Uses LUT-based conversion for performance:
/// 1. Linear RGB to XYZ (matrix multiplication)
/// 2. XYZ to Lab (f-function with LUT)
/// </remarks>
public readonly struct LinearRgbFToLabF : IProject<LinearRgbF, LabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LabF Project(in LinearRgbF work) {
    // Linear RGB to XYZ
    var x = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    // XYZ to Lab using fixed-point LUTs
    var xRatioFixed = (int)((x / ColorConstants.D65_Xn) * 65536f);
    var yRatioFixed = (int)((y / ColorConstants.D65_Yn) * 65536f);
    var zRatioFixed = (int)((z / ColorConstants.D65_Zn) * 65536f);

    var fx = FixedPointMath.LabF(xRatioFixed) / 65536f;
    var fy = FixedPointMath.LabF(yRatioFixed) / 65536f;
    var fz = FixedPointMath.LabF(zRatioFixed) / 65536f;

    return new(
      116f * fy - 16f,
      500f * (fx - fy),
      200f * (fy - fz)
    );
  }
}

/// <summary>
/// Projects LinearRgbaF to LabF using D65 illuminant.
/// </summary>
public readonly struct LinearRgbaFToLabF : IProject<LinearRgbaF, LabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LabF Project(in LinearRgbaF work) {
    var x = ColorMatrices.RgbToXyz_XR * work.R + ColorMatrices.RgbToXyz_XG * work.G + ColorMatrices.RgbToXyz_XB * work.B;
    var y = ColorMatrices.RgbToXyz_YR * work.R + ColorMatrices.RgbToXyz_YG * work.G + ColorMatrices.RgbToXyz_YB * work.B;
    var z = ColorMatrices.RgbToXyz_ZR * work.R + ColorMatrices.RgbToXyz_ZG * work.G + ColorMatrices.RgbToXyz_ZB * work.B;

    var xRatioFixed = (int)((x / ColorConstants.D65_Xn) * 65536f);
    var yRatioFixed = (int)((y / ColorConstants.D65_Yn) * 65536f);
    var zRatioFixed = (int)((z / ColorConstants.D65_Zn) * 65536f);

    var fx = FixedPointMath.LabF(xRatioFixed) / 65536f;
    var fy = FixedPointMath.LabF(yRatioFixed) / 65536f;
    var fz = FixedPointMath.LabF(zRatioFixed) / 65536f;

    return new(
      116f * fy - 16f,
      500f * (fx - fy),
      200f * (fy - fz)
    );
  }
}

/// <summary>
/// Projects LabF back to LinearRgbF using D65 illuminant.
/// </summary>
public readonly struct LabFToLinearRgbF : IProject<LabF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in LabF lab) {
    // Lab to XYZ
    var fy = (lab.L + 16f) / 116f;
    var fx = lab.A / 500f + fy;
    var fz = fy - lab.B / 200f;

    // Inverse f function
    var x = fx > ColorConstants.Lab_Delta ? fx * fx * fx : (fx - 16f / 116f) * 3f * ColorConstants.Lab_Delta * ColorConstants.Lab_Delta;
    var y = lab.L > 8f ? fy * fy * fy : lab.L / ColorConstants.Lab_Kappa;
    var z = fz > ColorConstants.Lab_Delta ? fz * fz * fz : (fz - 16f / 116f) * 3f * ColorConstants.Lab_Delta * ColorConstants.Lab_Delta;

    // Scale by white point
    x *= ColorConstants.D65_Xn;
    y *= ColorConstants.D65_Yn;
    z *= ColorConstants.D65_Zn;

    // XYZ to linear RGB
    return new(
      ColorMatrices.XyzToRgb_RX * x + ColorMatrices.XyzToRgb_RY * y + ColorMatrices.XyzToRgb_RZ * z,
      ColorMatrices.XyzToRgb_GX * x + ColorMatrices.XyzToRgb_GY * y + ColorMatrices.XyzToRgb_GZ * z,
      ColorMatrices.XyzToRgb_BX * x + ColorMatrices.XyzToRgb_BY * y + ColorMatrices.XyzToRgb_BZ * z
    );
  }
}
