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
using Hawkynt.ColorProcessing.Filtering;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Blending.BlendModes;

/// <summary>
/// Saturation blend mode — takes saturation from foreground, hue and lightness from background.
/// </summary>
[BlendModeInfo("Saturation",
  Description = "Takes saturation from foreground, hue and lightness from background",
  Category = BlendModeCategory.Component)]
public readonly struct SaturationBlend : IFullPixelBlendMode {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  float IBlendMode.Blend(float bg, float fg) => fg;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (float R, float G, float B) BlendPixel(float bgR, float bgG, float bgB, float fgR, float fgG, float fgB) {
    var (bgH, _, bgL) = HslMath.RgbToHsl(bgR, bgG, bgB);
    var (_, fgS, _) = HslMath.RgbToHsl(fgR, fgG, fgB);
    return HslMath.HslToRgb(bgH, fgS, bgL);
  }
}
