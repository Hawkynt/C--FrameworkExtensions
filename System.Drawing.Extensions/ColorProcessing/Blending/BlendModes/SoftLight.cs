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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Blending.BlendModes;

/// <summary>
/// Soft Light blend mode — W3C compositing specification formula.
/// </summary>
[BlendModeInfo("SoftLight",
  Description = "Soft contrast adjustment using W3C formula",
  Category = BlendModeCategory.Contrast)]
public readonly struct SoftLight : IBlendMode {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Blend(float bg, float fg) {
    if (fg <= 0.5f)
      return bg - (1f - 2f * fg) * bg * (1f - bg);

    var d = bg <= 0.25f
      ? ((16f * bg - 12f) * bg + 4f) * bg
      : (float)Math.Sqrt(bg);

    return bg + (2f * fg - 1f) * (d - bg);
  }
}
