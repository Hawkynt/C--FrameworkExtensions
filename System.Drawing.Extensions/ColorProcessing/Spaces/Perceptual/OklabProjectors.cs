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
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

/// <summary>
/// Projects LinearRgbF to OklabF.
/// </summary>
/// <remarks>
/// Uses the Oklab transformation matrices.
/// Reference: https://bottosson.github.io/posts/oklab/
/// </remarks>
public readonly struct LinearRgbFToOklabF : IProject<LinearRgbF, OklabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklabF Project(in LinearRgbF work) {
    // Linear sRGB to LMS
    var l = ColorMatrices.Oklab_L_R * work.R + ColorMatrices.Oklab_L_G * work.G + ColorMatrices.Oklab_L_B * work.B;
    var m = ColorMatrices.Oklab_M_R * work.R + ColorMatrices.Oklab_M_G * work.G + ColorMatrices.Oklab_M_B * work.B;
    var s = ColorMatrices.Oklab_S_R * work.R + ColorMatrices.Oklab_S_G * work.G + ColorMatrices.Oklab_S_B * work.B;

    // Cube root (using LUT for performance)
    var l_ = FixedPointMath.FastCbrt(l);
    var m_ = FixedPointMath.FastCbrt(m);
    var s_ = FixedPointMath.FastCbrt(s);

    // LMS to Oklab
    return new(
      0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_,
      1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_,
      0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_
    );
  }
}

/// <summary>
/// Projects LinearRgbaF to OklabF.
/// </summary>
public readonly struct LinearRgbaFToOklabF : IProject<LinearRgbaF, OklabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklabF Project(in LinearRgbaF work) {
    var l = ColorMatrices.Oklab_L_R * work.R + ColorMatrices.Oklab_L_G * work.G + ColorMatrices.Oklab_L_B * work.B;
    var m = ColorMatrices.Oklab_M_R * work.R + ColorMatrices.Oklab_M_G * work.G + ColorMatrices.Oklab_M_B * work.B;
    var s = ColorMatrices.Oklab_S_R * work.R + ColorMatrices.Oklab_S_G * work.G + ColorMatrices.Oklab_S_B * work.B;

    var l_ = FixedPointMath.FastCbrt(l);
    var m_ = FixedPointMath.FastCbrt(m);
    var s_ = FixedPointMath.FastCbrt(s);

    return new(
      0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_,
      1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_,
      0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_
    );
  }
}

/// <summary>
/// Projects OklabF back to LinearRgbF.
/// </summary>
public readonly struct OklabFToLinearRgbF : IProject<OklabF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in OklabF oklab) {
    // Oklab to LMS'
    var l_ = oklab.L + 0.3963377774f * oklab.A + 0.2158037573f * oklab.B;
    var m_ = oklab.L - 0.1055613458f * oklab.A - 0.0638541728f * oklab.B;
    var s_ = oklab.L - 0.0894841775f * oklab.A - 1.2914855480f * oklab.B;

    // Cube
    var l = l_ * l_ * l_;
    var m = m_ * m_ * m_;
    var s = s_ * s_ * s_;

    // LMS to linear sRGB
    return new(
      +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s,
      -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s,
      -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s
    );
  }
}
