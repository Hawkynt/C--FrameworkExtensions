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
/// Projects LinearRgbF to YCbCrF using ITU-R BT.601 (SDTV) coefficients.
/// </summary>
/// <remarks>
/// BT.601 is the standard for SDTV (480i/576i).
/// Kr = 0.299, Kb = 0.114, Kg = 1 - Kr - Kb = 0.587
/// Y = Kr*R + Kg*G + Kb*B
/// Cb = (B - Y) / (2 * (1 - Kb))
/// Cr = (R - Y) / (2 * (1 - Kr))
/// </remarks>
public readonly struct LinearRgbFToYCbCrBt601F : IProject<LinearRgbF, YCbCrF> {

  // BT.601 coefficients
  private const float Kr = 0.299f;
  private const float Kg = 0.587f;
  private const float Kb = 0.114f;

  // Derived coefficients for Cb = (B - Y) / (2 * (1 - Kb)) = -0.5*Kr/(1-Kb)*R - 0.5*Kg/(1-Kb)*G + 0.5*B
  private const float CbR = -0.168736f;  // -Kr / (2 * (1 - Kb))
  private const float CbG = -0.331264f;  // -Kg / (2 * (1 - Kb))
  private const float CbB = 0.5f;        // 0.5

  // Derived coefficients for Cr = (R - Y) / (2 * (1 - Kr)) = 0.5*R - 0.5*Kg/(1-Kr)*G - 0.5*Kb/(1-Kr)*B
  private const float CrR = 0.5f;        // 0.5
  private const float CrG = -0.418688f;  // -Kg / (2 * (1 - Kr))
  private const float CrB = -0.081312f;  // -Kb / (2 * (1 - Kr))

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YCbCrF Project(in LinearRgbF work) => new(
    Kr * work.R + Kg * work.G + Kb * work.B,
    CbR * work.R + CbG * work.G + CbB * work.B,
    CrR * work.R + CrG * work.G + CrB * work.B
  );
}

/// <summary>
/// Projects LinearRgbaF to YCbCrF using ITU-R BT.601 (SDTV) coefficients.
/// </summary>
public readonly struct LinearRgbaFToYCbCrBt601F : IProject<LinearRgbaF, YCbCrF> {

  private const float Kr = 0.299f;
  private const float Kg = 0.587f;
  private const float Kb = 0.114f;
  private const float CbR = -0.168736f;
  private const float CbG = -0.331264f;
  private const float CbB = 0.5f;
  private const float CrR = 0.5f;
  private const float CrG = -0.418688f;
  private const float CrB = -0.081312f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YCbCrF Project(in LinearRgbaF work) => new(
    Kr * work.R + Kg * work.G + Kb * work.B,
    CbR * work.R + CbG * work.G + CbB * work.B,
    CrR * work.R + CrG * work.G + CrB * work.B
  );
}

/// <summary>
/// Projects YCbCrF back to LinearRgbF using ITU-R BT.601 (SDTV) coefficients.
/// </summary>
public readonly struct YCbCrBt601FToLinearRgbF : IProject<YCbCrF, LinearRgbF> {

  // Inverse coefficients for BT.601
  // R = Y + Cr * (2 * (1 - Kr))
  // G = Y - Cb * (2 * (1 - Kb) * Kb / Kg) - Cr * (2 * (1 - Kr) * Kr / Kg)
  // B = Y + Cb * (2 * (1 - Kb))
  private const float RCr = 1.402f;       // 2 * (1 - 0.299)
  private const float GCb = -0.344136f;   // -2 * (1 - 0.114) * 0.114 / 0.587
  private const float GCr = -0.714136f;   // -2 * (1 - 0.299) * 0.299 / 0.587
  private const float BCb = 1.772f;       // 2 * (1 - 0.114)

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in YCbCrF ycbcr) => new(
    ycbcr.Y + RCr * ycbcr.Cr,
    ycbcr.Y + GCb * ycbcr.Cb + GCr * ycbcr.Cr,
    ycbcr.Y + BCb * ycbcr.Cb
  );
}

/// <summary>
/// Projects LinearRgbF to YCbCrF using ITU-R BT.709 (HDTV) coefficients.
/// </summary>
/// <remarks>
/// BT.709 is the standard for HDTV (720p/1080i/1080p).
/// Kr = 0.2126, Kb = 0.0722, Kg = 0.7152
/// </remarks>
public readonly struct LinearRgbFToYCbCrBt709F : IProject<LinearRgbF, YCbCrF> {

  // BT.709 coefficients
  private const float Kr = 0.2126f;
  private const float Kg = 0.7152f;
  private const float Kb = 0.0722f;

  // Derived coefficients
  private const float CbR = -0.114572f;  // -Kr / (2 * (1 - Kb))
  private const float CbG = -0.385428f;  // -Kg / (2 * (1 - Kb))
  private const float CbB = 0.5f;
  private const float CrR = 0.5f;
  private const float CrG = -0.454153f;  // -Kg / (2 * (1 - Kr))
  private const float CrB = -0.045847f;  // -Kb / (2 * (1 - Kr))

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YCbCrF Project(in LinearRgbF work) => new(
    Kr * work.R + Kg * work.G + Kb * work.B,
    CbR * work.R + CbG * work.G + CbB * work.B,
    CrR * work.R + CrG * work.G + CrB * work.B
  );
}

/// <summary>
/// Projects LinearRgbaF to YCbCrF using ITU-R BT.709 (HDTV) coefficients.
/// </summary>
public readonly struct LinearRgbaFToYCbCrBt709F : IProject<LinearRgbaF, YCbCrF> {

  private const float Kr = 0.2126f;
  private const float Kg = 0.7152f;
  private const float Kb = 0.0722f;
  private const float CbR = -0.114572f;
  private const float CbG = -0.385428f;
  private const float CbB = 0.5f;
  private const float CrR = 0.5f;
  private const float CrG = -0.454153f;
  private const float CrB = -0.045847f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YCbCrF Project(in LinearRgbaF work) => new(
    Kr * work.R + Kg * work.G + Kb * work.B,
    CbR * work.R + CbG * work.G + CbB * work.B,
    CrR * work.R + CrG * work.G + CrB * work.B
  );
}

/// <summary>
/// Projects YCbCrF back to LinearRgbF using ITU-R BT.709 (HDTV) coefficients.
/// </summary>
public readonly struct YCbCrBt709FToLinearRgbF : IProject<YCbCrF, LinearRgbF> {

  // Inverse coefficients for BT.709
  private const float RCr = 1.5748f;      // 2 * (1 - 0.2126)
  private const float GCb = -0.187324f;   // -2 * (1 - 0.0722) * 0.0722 / 0.7152
  private const float GCr = -0.468124f;   // -2 * (1 - 0.2126) * 0.2126 / 0.7152
  private const float BCb = 1.8556f;      // 2 * (1 - 0.0722)

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in YCbCrF ycbcr) => new(
    ycbcr.Y + RCr * ycbcr.Cr,
    ycbcr.Y + GCb * ycbcr.Cb + GCr * ycbcr.Cr,
    ycbcr.Y + BCb * ycbcr.Cb
  );
}

/// <summary>
/// Projects LinearRgbF to YCbCrF using ITU-R BT.2020 (UHDTV) coefficients.
/// </summary>
/// <remarks>
/// BT.2020 is the standard for UHDTV (4K/8K).
/// Kr = 0.2627, Kb = 0.0593, Kg = 0.678
/// </remarks>
public readonly struct LinearRgbFToYCbCrBt2020F : IProject<LinearRgbF, YCbCrF> {

  // BT.2020 coefficients
  private const float Kr = 0.2627f;
  private const float Kg = 0.6780f;
  private const float Kb = 0.0593f;

  // Derived coefficients
  private const float CbR = -0.139630f;  // -Kr / (2 * (1 - Kb))
  private const float CbG = -0.360370f;  // -Kg / (2 * (1 - Kb))
  private const float CbB = 0.5f;
  private const float CrR = 0.5f;
  private const float CrG = -0.459786f;  // -Kg / (2 * (1 - Kr))
  private const float CrB = -0.040214f;  // -Kb / (2 * (1 - Kr))

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YCbCrF Project(in LinearRgbF work) => new(
    Kr * work.R + Kg * work.G + Kb * work.B,
    CbR * work.R + CbG * work.G + CbB * work.B,
    CrR * work.R + CrG * work.G + CrB * work.B
  );
}

/// <summary>
/// Projects LinearRgbaF to YCbCrF using ITU-R BT.2020 (UHDTV) coefficients.
/// </summary>
public readonly struct LinearRgbaFToYCbCrBt2020F : IProject<LinearRgbaF, YCbCrF> {

  private const float Kr = 0.2627f;
  private const float Kg = 0.6780f;
  private const float Kb = 0.0593f;
  private const float CbR = -0.139630f;
  private const float CbG = -0.360370f;
  private const float CbB = 0.5f;
  private const float CrR = 0.5f;
  private const float CrG = -0.459786f;
  private const float CrB = -0.040214f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YCbCrF Project(in LinearRgbaF work) => new(
    Kr * work.R + Kg * work.G + Kb * work.B,
    CbR * work.R + CbG * work.G + CbB * work.B,
    CrR * work.R + CrG * work.G + CrB * work.B
  );
}

/// <summary>
/// Projects YCbCrF back to LinearRgbF using ITU-R BT.2020 (UHDTV) coefficients.
/// </summary>
public readonly struct YCbCrBt2020FToLinearRgbF : IProject<YCbCrF, LinearRgbF> {

  // Inverse coefficients for BT.2020
  private const float RCr = 1.4746f;      // 2 * (1 - 0.2627)
  private const float GCb = -0.164553f;   // -2 * (1 - 0.0593) * 0.0593 / 0.678
  private const float GCr = -0.571353f;   // -2 * (1 - 0.2627) * 0.2627 / 0.678
  private const float BCb = 1.8814f;      // 2 * (1 - 0.0593)

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in YCbCrF ycbcr) => new(
    ycbcr.Y + RCr * ycbcr.Cr,
    ycbcr.Y + GCb * ycbcr.Cb + GCr * ycbcr.Cr,
    ycbcr.Y + BCb * ycbcr.Cb
  );
}
