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
using Hawkynt.ColorProcessing.Spaces.Lab;
using Hawkynt.ColorProcessing.Working;
using SysMath = System.Math;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Cylindrical;

/// <summary>
/// Projects LabF to LchF (Lab to cylindrical Lab).
/// </summary>
public readonly struct LabFToLchF : IProject<LabF, LchF> {

  private const float TwoPi = 2f * (float)SysMath.PI;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LchF Project(in LabF lab) {
    var c = (float)SysMath.Sqrt(lab.A * lab.A + lab.B * lab.B);
    var h = (float)SysMath.Atan2(lab.B, lab.A);

    // Normalize hue to 0-1 range
    if (h < 0f)
      h += TwoPi;
    h /= TwoPi;

    return new(lab.L, c, h);
  }
}

/// <summary>
/// Projects LchF back to LabF (cylindrical Lab to Lab).
/// </summary>
public readonly struct LchFToLabF : IProject<LchF, LabF> {

  private const float TwoPi = 2f * (float)SysMath.PI;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LabF Project(in LchF lch) {
    var hRad = lch.H * TwoPi;
    return new(
      lch.L,
      lch.C * (float)SysMath.Cos(hRad),
      lch.C * (float)SysMath.Sin(hRad)
    );
  }
}

/// <summary>
/// Projects LinearRgbF to LchF via Lab.
/// </summary>
public readonly struct LinearRgbFToLchF : IProject<LinearRgbF, LchF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LchF Project(in LinearRgbF work) {
    var toLab = new LinearRgbFToLabF();
    var lab = toLab.Project(work);
    var toLch = new LabFToLchF();
    return toLch.Project(lab);
  }
}

/// <summary>
/// Projects LinearRgbaF to LchF via Lab.
/// </summary>
public readonly struct LinearRgbaFToLchF : IProject<LinearRgbaF, LchF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LchF Project(in LinearRgbaF work) {
    var toLab = new LinearRgbaFToLabF();
    var lab = toLab.Project(work);
    var toLch = new LabFToLchF();
    return toLch.Project(lab);
  }
}

/// <summary>
/// Projects LchF back to LinearRgbF via Lab.
/// </summary>
public readonly struct LchFToLinearRgbF : IProject<LchF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in LchF lch) {
    var toLab = new LchFToLabF();
    var lab = toLab.Project(lch);
    var toRgb = new LabFToLinearRgbF();
    return toRgb.Project(lab);
  }
}
