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
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Spaces.Hdr;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

/// <summary>
/// Projects XyzF to LuvF using D65 illuminant.
/// </summary>
public readonly struct XyzFToLuvF : IProject<XyzF, LuvF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LuvF Project(in XyzF xyz) {
    var yRatio = xyz.Y / ColorMatrices.D65_Yn;

    // Calculate L*
    var l = yRatio > ColorMatrices.Lab_Epsilon
      ? 116f * MathF.Pow(yRatio, 1f / 3f) - 16f
      : ColorMatrices.Lab_Kappa * yRatio;

    // Calculate u' and v'
    var denom = xyz.X + 15f * xyz.Y + 3f * xyz.Z;
    if (denom < 1e-6f)
      return new(l, 0f, 0f);

    var uPrime = 4f * xyz.X / denom;
    var vPrime = 9f * xyz.Y / denom;

    // Calculate u* and v*
    return new(
      l,
      13f * l * (uPrime - ColorMatrices.Luv_Un),
      13f * l * (vPrime - ColorMatrices.Luv_Vn)
    );
  }
}

/// <summary>
/// Projects LinearRgbF to LuvF via XYZ.
/// </summary>
public readonly struct LinearRgbFToLuvF : IProject<LinearRgbF, LuvF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LuvF Project(in LinearRgbF work) {
    var toXyz = new LinearRgbFToXyzF();
    var xyz = toXyz.Project(work);
    var toLuv = new XyzFToLuvF();
    return toLuv.Project(xyz);
  }
}

/// <summary>
/// Projects LinearRgbaF to LuvF via XYZ.
/// </summary>
public readonly struct LinearRgbaFToLuvF : IProject<LinearRgbaF, LuvF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LuvF Project(in LinearRgbaF work) {
    var toXyz = new LinearRgbaFToXyzF();
    var xyz = toXyz.Project(work);
    var toLuv = new XyzFToLuvF();
    return toLuv.Project(xyz);
  }
}

/// <summary>
/// Projects LuvF back to XyzF using D65 illuminant.
/// </summary>
public readonly struct LuvFToXyzF : IProject<LuvF, XyzF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XyzF Project(in LuvF luv) {
    if (luv.L <= 0f)
      return new(0f, 0f, 0f);

    // Calculate Y from L*
    float y;
    if (luv.L > 8f) {
      var t = (luv.L + 16f) * ColorMatrices.Inv116;
      y = ColorMatrices.D65_Yn * t * t * t;
    } else
      y = ColorMatrices.D65_Yn * luv.L / ColorMatrices.Lab_Kappa;

    // Calculate u' and v' from u* and v*
    var uPrime = luv.U / (13f * luv.L) + ColorMatrices.Luv_Un;
    var vPrime = luv.V / (13f * luv.L) + ColorMatrices.Luv_Vn;

    // Calculate X and Z
    var x = y * (9f * uPrime) / (4f * vPrime);
    var z = y * (12f - 3f * uPrime - 20f * vPrime) / (4f * vPrime);

    return new(x, y, z);
  }
}

/// <summary>
/// Projects LuvF back to LinearRgbF via XYZ.
/// </summary>
public readonly struct LuvFToLinearRgbF : IProject<LuvF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in LuvF luv) {
    var toXyz = new LuvFToXyzF();
    var xyz = toXyz.Project(luv);
    var toRgb = new XyzFToLinearRgbF();
    return toRgb.Project(xyz);
  }
}
