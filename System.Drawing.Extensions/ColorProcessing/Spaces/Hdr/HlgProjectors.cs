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

using System;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Hdr;

/// <summary>
/// Projects LinearRgbF (linear scene light, 0..1) to HlgF (HLG-encoded).
/// </summary>
/// <remarks>
/// Applies the ITU-R BT.2100 Annex 2 HLG OETF per channel. The input is
/// interpreted as scene-referred linear light normalised so that diffuse
/// reference white = 1.0; the HLG specification treats E = 1.0 as the upper
/// nominal anchor (encoded value E' = 1.0).
/// </remarks>
public readonly struct LinearRgbFToHlgF : IProject<LinearRgbF, HlgF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HlgF Project(in LinearRgbF work) => new(
    HlgTransfer.Oetf(work.R),
    HlgTransfer.Oetf(work.G),
    HlgTransfer.Oetf(work.B)
  );
}

/// <summary>
/// Projects LinearRgbaF to HlgF (drops alpha; HlgF is 3-component).
/// </summary>
public readonly struct LinearRgbaFToHlgF : IProject<LinearRgbaF, HlgF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HlgF Project(in LinearRgbaF work) => new(
    HlgTransfer.Oetf(work.R),
    HlgTransfer.Oetf(work.G),
    HlgTransfer.Oetf(work.B)
  );
}

/// <summary>
/// Projects HlgF (HLG-encoded) back to LinearRgbF (linear scene light).
/// </summary>
public readonly struct HlgFToLinearRgbF : IProject<HlgF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in HlgF hlg) => new(
    HlgTransfer.OetfInverse(hlg.R),
    HlgTransfer.OetfInverse(hlg.G),
    HlgTransfer.OetfInverse(hlg.B)
  );
}

/// <summary>
/// HLG OETF / inverse-OETF per ITU-R BT.2100 Annex 2.
/// </summary>
internal static class HlgTransfer {

  // Constants from BT.2100 Annex 2 Table 5.
  private const float A = 0.17883277f;
  private const float B = 0.28466892f;
  private const float C = 0.55991073f;

  /// <summary>
  /// HLG OETF (linear scene light → encoded signal).
  /// Two-branch piecewise; branches meet at E = 1/12 → E' = 0.5 with C¹ continuity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Oetf(float e) {
    if (e <= 0f) return 0f;
    return e <= 1f / 12f
      ? MathF.Sqrt(3f * e)
      : A * MathF.Log(12f * e - B) + C;
  }

  /// <summary>
  /// HLG OETF inverse (encoded signal → linear scene light).
  /// Two-branch piecewise; branches meet at E' = 0.5 → E = 1/12.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float OetfInverse(float ePrime) {
    if (ePrime <= 0f) return 0f;
    return ePrime <= 0.5f
      ? ePrime * ePrime / 3f
      : (MathF.Exp((ePrime - C) / A) + B) / 12f;
  }
}
