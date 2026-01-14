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
/// Encodes OkLab color space directly to sRGB Bgra8888.
/// </summary>
/// <remarks>
/// <para>Combines OkLab to linear RGB conversion and gamma compression in one step.</para>
/// <para>Uses LUT-based gamma compression for sRGB.</para>
/// </remarks>
public readonly struct OklabaFToSrgb32 : IEncode<OklabaF, Bgra8888> {

  /// <summary>
  /// Encodes OkLab color to sRGB pixel.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Bgra8888 Encode(in OklabaF color) {
    // Oklab to LMS'
    var l_ = ColorMatrices.Oklab_FromL_L * color.L + ColorMatrices.Oklab_FromL_A * color.A + ColorMatrices.Oklab_FromL_B * color.B;
    var m_ = ColorMatrices.Oklab_FromM_L * color.L + ColorMatrices.Oklab_FromM_A * color.A + ColorMatrices.Oklab_FromM_B * color.B;
    var s_ = ColorMatrices.Oklab_FromS_L * color.L + ColorMatrices.Oklab_FromS_A * color.A + ColorMatrices.Oklab_FromS_B * color.B;

    // LMS' to LMS (cube)
    var l = l_ * l_ * l_;
    var m = m_ * m_ * m_;
    var s = s_ * s_ * s_;

    // LMS to Linear RGB
    var r = ColorMatrices.Oklab_ToR_L * l + ColorMatrices.Oklab_ToR_M * m + ColorMatrices.Oklab_ToR_S * s;
    var g = ColorMatrices.Oklab_ToG_L * l + ColorMatrices.Oklab_ToG_M * m + ColorMatrices.Oklab_ToG_S * s;
    var b = ColorMatrices.Oklab_ToB_L_Inv * l + ColorMatrices.Oklab_ToB_M_Inv * m + ColorMatrices.Oklab_ToB_S_Inv * s;

    // Linear RGB to sRGB using LUT-based gamma compression
    return new(
      FixedPointMath.GammaCompress((int)(r * 65536f + 0.5f)),
      FixedPointMath.GammaCompress((int)(g * 65536f + 0.5f)),
      FixedPointMath.GammaCompress((int)(b * 65536f + 0.5f)),
      (byte)(color.Alpha * 255f + 0.5f)
    );
  }
}
