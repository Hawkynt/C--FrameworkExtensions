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
/// Projects LinearRgbF to YiqF using the FCC 1953 NTSC matrix (SMPTE 170M).
/// </summary>
/// <remarks>
/// <para>Y = 0.299R + 0.587G + 0.114B
/// I = 0.596R − 0.274G − 0.322B
/// Q = 0.211R − 0.523G + 0.312B</para>
/// <para>Reference: FCC 1953 / SMPTE 170M.</para>
/// </remarks>
public readonly struct LinearRgbFToYiqF : IProject<LinearRgbF, YiqF> {

  private const float YR = 0.299f;
  private const float YG = 0.587f;
  private const float YB = 0.114f;
  private const float IR = 0.596f;
  private const float IG = -0.274f;
  private const float IB = -0.322f;
  private const float QR = 0.211f;
  private const float QG = -0.523f;
  private const float QB = 0.312f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YiqF Project(in LinearRgbF work) => new(
    YR * work.R + YG * work.G + YB * work.B,
    IR * work.R + IG * work.G + IB * work.B,
    QR * work.R + QG * work.G + QB * work.B
  );
}

/// <summary>
/// Projects LinearRgbaF to YiqF.
/// </summary>
public readonly struct LinearRgbaFToYiqF : IProject<LinearRgbaF, YiqF> {

  private const float YR = 0.299f;
  private const float YG = 0.587f;
  private const float YB = 0.114f;
  private const float IR = 0.596f;
  private const float IG = -0.274f;
  private const float IB = -0.322f;
  private const float QR = 0.211f;
  private const float QG = -0.523f;
  private const float QB = 0.312f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YiqF Project(in LinearRgbaF work) => new(
    YR * work.R + YG * work.G + YB * work.B,
    IR * work.R + IG * work.G + IB * work.B,
    QR * work.R + QG * work.G + QB * work.B
  );
}

/// <summary>
/// Projects YiqF back to LinearRgbF using the FCC 1953 NTSC inverse matrix.
/// </summary>
public readonly struct YiqFToLinearRgbF : IProject<YiqF, LinearRgbF> {

  private const float RI = 0.956f;
  private const float RQ = 0.621f;
  private const float GI = -0.272f;
  private const float GQ = -0.647f;
  private const float BI = -1.106f;
  private const float BQ = 1.703f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in YiqF yiq) => new(
    yiq.Y + RI * yiq.I + RQ * yiq.Q,
    yiq.Y + GI * yiq.I + GQ * yiq.Q,
    yiq.Y + BI * yiq.I + BQ * yiq.Q
  );
}
