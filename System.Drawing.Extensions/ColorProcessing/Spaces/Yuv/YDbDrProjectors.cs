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
/// Projects LinearRgbF to YDbDrF (SECAM analogue-TV chroma encoding).
/// </summary>
/// <remarks>
/// <para>Y = 0.299R + 0.587G + 0.114B
/// Db = −0.450R − 0.883G + 1.333B
/// Dr = −1.333R + 1.116G + 0.217B</para>
/// <para>Reference: ITU-R BT.470-7 SECAM-B/G/D/K/L (FCC equivalent).</para>
/// </remarks>
public readonly struct LinearRgbFToYDbDrF : IProject<LinearRgbF, YDbDrF> {

  private const float YR = 0.299f;
  private const float YG = 0.587f;
  private const float YB = 0.114f;
  private const float DbR = -0.450f;
  private const float DbG = -0.883f;
  private const float DbB = 1.333f;
  private const float DrR = -1.333f;
  private const float DrG = 1.116f;
  private const float DrB = 0.217f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YDbDrF Project(in LinearRgbF work) => new(
    YR * work.R + YG * work.G + YB * work.B,
    DbR * work.R + DbG * work.G + DbB * work.B,
    DrR * work.R + DrG * work.G + DrB * work.B
  );
}

/// <summary>
/// Projects LinearRgbaF to YDbDrF.
/// </summary>
public readonly struct LinearRgbaFToYDbDrF : IProject<LinearRgbaF, YDbDrF> {

  private const float YR = 0.299f;
  private const float YG = 0.587f;
  private const float YB = 0.114f;
  private const float DbR = -0.450f;
  private const float DbG = -0.883f;
  private const float DbB = 1.333f;
  private const float DrR = -1.333f;
  private const float DrG = 1.116f;
  private const float DrB = 0.217f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YDbDrF Project(in LinearRgbaF work) => new(
    YR * work.R + YG * work.G + YB * work.B,
    DbR * work.R + DbG * work.G + DbB * work.B,
    DrR * work.R + DrG * work.G + DrB * work.B
  );
}

/// <summary>
/// Projects YDbDrF back to LinearRgbF (SECAM inverse).
/// </summary>
public readonly struct YDbDrFToLinearRgbF : IProject<YDbDrF, LinearRgbF> {

  // Reference inverse: R = Y + 0.000Db − 0.526Dr; G = Y − 0.129Db + 0.268Dr;
  // B = Y + 0.665Db + 0.000Dr.
  private const float RDb = 0f;
  private const float RDr = -0.526f;
  private const float GDb = -0.129f;
  private const float GDr = 0.268f;
  private const float BDb = 0.665f;
  private const float BDr = 0f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in YDbDrF y) => new(
    y.Y + RDb * y.Db + RDr * y.Dr,
    y.Y + GDb * y.Db + GDr * y.Dr,
    y.Y + BDb * y.Db + BDr * y.Dr
  );
}
