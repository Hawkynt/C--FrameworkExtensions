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

namespace Hawkynt.ColorProcessing.Spaces.Yuv;

/// <summary>
/// Projects LinearRgbF to YuvF using standard BT.601 coefficients.
/// </summary>
/// <remarks>
/// Y = 0.299R + 0.587G + 0.114B
/// U = 0.492(B - Y) = -0.14713R - 0.28886G + 0.436B
/// V = 0.877(R - Y) = 0.615R - 0.51499G - 0.10001B
/// </remarks>
public readonly struct LinearRgbFToYuvF : IProject<LinearRgbF, YuvF> {

  // RGB to YUV coefficients
  private const float YR = 0.299f;
  private const float YG = 0.587f;
  private const float YB = 0.114f;
  private const float UR = -0.14713f;
  private const float UG = -0.28886f;
  private const float UB = 0.436f;
  private const float VR = 0.615f;
  private const float VG = -0.51499f;
  private const float VB = -0.10001f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YuvF Project(in LinearRgbF work) => new(
    YR * work.R + YG * work.G + YB * work.B,
    UR * work.R + UG * work.G + UB * work.B,
    VR * work.R + VG * work.G + VB * work.B
  );
}

/// <summary>
/// Projects LinearRgbaF to YuvF using standard BT.601 coefficients.
/// </summary>
public readonly struct LinearRgbaFToYuvF : IProject<LinearRgbaF, YuvF> {

  private const float YR = 0.299f;
  private const float YG = 0.587f;
  private const float YB = 0.114f;
  private const float UR = -0.14713f;
  private const float UG = -0.28886f;
  private const float UB = 0.436f;
  private const float VR = 0.615f;
  private const float VG = -0.51499f;
  private const float VB = -0.10001f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YuvF Project(in LinearRgbaF work) => new(
    YR * work.R + YG * work.G + YB * work.B,
    UR * work.R + UG * work.G + UB * work.B,
    VR * work.R + VG * work.G + VB * work.B
  );
}

/// <summary>
/// Projects LinearRgbF to YuvF using BT.709 (HDTV) coefficients.
/// </summary>
public readonly struct LinearRgbFToBt709YuvF : IProject<LinearRgbF, YuvF> {

  // BT.709 coefficients
  private const float YR = 0.2126f;
  private const float YG = 0.7152f;
  private const float YB = 0.0722f;
  private const float UR = -0.1146f;
  private const float UG = -0.3854f;
  private const float UB = 0.5f;
  private const float VR = 0.5f;
  private const float VG = -0.4542f;
  private const float VB = -0.0458f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YuvF Project(in LinearRgbF work) => new(
    YR * work.R + YG * work.G + YB * work.B,
    UR * work.R + UG * work.G + UB * work.B,
    VR * work.R + VG * work.G + VB * work.B
  );
}

/// <summary>
/// Projects YuvF back to LinearRgbF using standard BT.601 coefficients.
/// </summary>
/// <remarks>
/// R = Y + 1.140V
/// G = Y - 0.395U - 0.581V
/// B = Y + 2.032U
/// </remarks>
public readonly struct YuvFToLinearRgbF : IProject<YuvF, LinearRgbF> {

  // YUV to RGB coefficients (BT.601 inverse)
  private const float RV = 1.140f;
  private const float GU = -0.395f;
  private const float GV = -0.581f;
  private const float BU = 2.032f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in YuvF yuv) => new(
    yuv.Y + RV * yuv.V,
    yuv.Y + GU * yuv.U + GV * yuv.V,
    yuv.Y + BU * yuv.U
  );
}
