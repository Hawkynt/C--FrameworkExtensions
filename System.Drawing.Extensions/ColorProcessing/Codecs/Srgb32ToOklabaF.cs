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
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Codecs;

/// <summary>
/// Decodes sRGB Bgra8888 directly to OkLab color space.
/// </summary>
/// <remarks>
/// <para>Combines gamma expansion (sRGB → linear) and OkLab conversion in one step.</para>
/// <para>Uses LUT-based gamma expansion and fast cube root for OkLab.</para>
/// </remarks>
public readonly struct Srgb32ToOklabaF : IDecode<Bgra8888, OklabaF> {

  private const float FixedToFloat = 1f / 65536f;

  /// <summary>
  /// Decodes sRGB pixel to OkLab working space.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklabaF Decode(in Bgra8888 pixel) {
    // sRGB to linear RGB using LUT
    var r = FixedPointMath.GammaExpansionLut[pixel.R] * FixedToFloat;
    var g = FixedPointMath.GammaExpansionLut[pixel.G] * FixedToFloat;
    var b = FixedPointMath.GammaExpansionLut[pixel.B] * FixedToFloat;

    // Linear RGB to LMS
    var l = ColorMatrices.Oklab_L_R * r + ColorMatrices.Oklab_L_G * g + ColorMatrices.Oklab_L_B * b;
    var m = ColorMatrices.Oklab_M_R * r + ColorMatrices.Oklab_M_G * g + ColorMatrices.Oklab_M_B * b;
    var s = ColorMatrices.Oklab_S_R * r + ColorMatrices.Oklab_S_G * g + ColorMatrices.Oklab_S_B * b;

    // Cube root (LMS → LMS')
    var l_ = FixedPointMath.FastCbrt(l);
    var m_ = FixedPointMath.FastCbrt(m);
    var s_ = FixedPointMath.FastCbrt(s);

    // LMS' to Oklab
    return new(
      ColorMatrices.Oklab_ToL_L * l_ + ColorMatrices.Oklab_ToL_M * m_ + ColorMatrices.Oklab_ToL_S * s_,
      ColorMatrices.Oklab_ToA_L * l_ + ColorMatrices.Oklab_ToA_M * m_ + ColorMatrices.Oklab_ToA_S * s_,
      ColorMatrices.Oklab_ToB_L * l_ + ColorMatrices.Oklab_ToB_M * m_ + ColorMatrices.Oklab_ToB_S * s_,
      pixel.A * Bgra8888.ByteToNormalized
    );
  }
}
