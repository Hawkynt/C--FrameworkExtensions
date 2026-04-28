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
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Gamut;

/// <summary>
/// Per-channel hard clip into [0, 1] — the simplest gamut-mapping operator.
/// </summary>
/// <remarks>
/// <para>Used as the final-stage fallback in nearly every gamut-mapping pipeline
/// (and as a baseline for comparison in published research). Fast, deterministic,
/// hue-distorting in saturated regions: a Rec.2020 pure red will lose chroma when
/// clipped to Rec.709 because both red and green channels saturate at different
/// "exit points" along the wide-gamut ray.</para>
/// <para>Use this when the colour pipeline already does perceptual mapping upstream
/// (e.g. an ICC profile with rendering intent) and you only need the final
/// numerical safety net before quantisation.</para>
/// <para>Reference: Morovic, "Color Gamut Mapping" (2008), §3.1 (clipping).</para>
/// </remarks>
public readonly struct GamutClip : IGamutMap {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Map(in LinearRgbF color) => new(
    Clamp01(color.R),
    Clamp01(color.G),
    Clamp01(color.B)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Clamp01(float v) {
    if (v < 0f) return 0f;
    if (v > 1f) return 1f;
    return v;
  }
}
