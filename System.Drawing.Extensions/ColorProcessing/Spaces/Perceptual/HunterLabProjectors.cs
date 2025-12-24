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
using Hawkynt.ColorProcessing.Spaces.Hdr;
using Hawkynt.ColorProcessing.Working;
using SysMath = System.Math;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

/// <summary>
/// Projects XyzF to HunterLabF.
/// </summary>
public readonly struct XyzFToHunterLabF : IProject<XyzF, HunterLabF> {

  // Hunter Lab constants
  private const float Ka = 17.5f;
  private const float Kb = 7.0f;
  private const float K1 = 1.02f;
  private const float K2 = 0.847f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HunterLabF Project(in XyzF xyz) {
    // L = 10 * sqrt(Y)
    var sqrtY = (float)SysMath.Sqrt(xyz.Y);
    var l = 10f * sqrtY;

    if (sqrtY < 1e-6f)
      return new(l, 0f, 0f);

    // a = Ka * (1.02X - Y) / sqrt(Y)
    var a = Ka * (K1 * xyz.X - xyz.Y) / sqrtY;

    // b = Kb * (Y - 0.847Z) / sqrt(Y)
    var b = Kb * (xyz.Y - K2 * xyz.Z) / sqrtY;

    return new(l, a, b);
  }
}

/// <summary>
/// Projects LinearRgbF to HunterLabF via XYZ.
/// </summary>
public readonly struct LinearRgbFToHunterLabF : IProject<LinearRgbF, HunterLabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HunterLabF Project(in LinearRgbF work) {
    var toXyz = new LinearRgbFToXyzF();
    var xyz = toXyz.Project(work);
    var toHunterLab = new XyzFToHunterLabF();
    return toHunterLab.Project(xyz);
  }
}

/// <summary>
/// Projects LinearRgbaF to HunterLabF via XYZ.
/// </summary>
public readonly struct LinearRgbaFToHunterLabF : IProject<LinearRgbaF, HunterLabF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HunterLabF Project(in LinearRgbaF work) {
    var toXyz = new LinearRgbaFToXyzF();
    var xyz = toXyz.Project(work);
    var toHunterLab = new XyzFToHunterLabF();
    return toHunterLab.Project(xyz);
  }
}

/// <summary>
/// Projects HunterLabF back to XyzF.
/// </summary>
public readonly struct HunterLabFToXyzF : IProject<HunterLabF, XyzF> {

  private const float Ka = 17.5f;
  private const float Kb = 7.0f;
  private const float K1 = 1.02f;
  private const float K2 = 0.847f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XyzF Project(in HunterLabF hunterLab) {
    // Y = (L / 10)^2
    var y = (hunterLab.L / 10f) * (hunterLab.L / 10f);

    if (y < 1e-6f)
      return new(0f, 0f, 0f);

    var sqrtY = (float)SysMath.Sqrt(y);

    // X = (a * sqrt(Y) / Ka + Y) / 1.02
    var x = (hunterLab.A * sqrtY / Ka + y) / K1;

    // Z = (Y - b * sqrt(Y) / Kb) / 0.847
    var z = (y - hunterLab.B * sqrtY / Kb) / K2;

    return new(x, y, z);
  }
}

/// <summary>
/// Projects HunterLabF back to LinearRgbF via XYZ.
/// </summary>
public readonly struct HunterLabFToLinearRgbF : IProject<HunterLabF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in HunterLabF hunterLab) {
    var toXyz = new HunterLabFToXyzF();
    var xyz = toXyz.Project(hunterLab);
    var toRgb = new XyzFToLinearRgbF();
    return toRgb.Project(xyz);
  }
}
