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
/// Vivid Light blend mode — combines Color Burn and Color Dodge.
/// </summary>
[BlendModeInfo("VividLight",
  Description = "Combines Color Burn and Color Dodge based on foreground",
  Category = BlendModeCategory.Contrast)]
public readonly struct VividLight : IBlendMode {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Blend(float bg, float fg)
    => fg <= 0f
      ? 0f
      : fg < 0.5f
        ? Math.Max(0f, 1f - (1f - bg) / (2f * fg))
        : fg >= 1f
          ? 1f
          : Math.Min(1f, bg / (2f * (1f - fg)));
}
