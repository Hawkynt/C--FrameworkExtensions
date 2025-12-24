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
using Hawkynt.ColorProcessing.Working;
using SysMath = System.Math;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

/// <summary>
/// Projects OklabF to OklchF (rectangular to polar).
/// </summary>
public readonly struct OklabFToOklchF : IProject<OklabF, OklchF> {

  private const float TwoPi = 6.2831853071795864769f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklchF Project(in OklabF oklab) {
    var c = (float)SysMath.Sqrt(oklab.A * oklab.A + oklab.B * oklab.B);
    var h = (float)SysMath.Atan2(oklab.B, oklab.A);
    if (h < 0) h += TwoPi;
    return new(oklab.L, c, h / TwoPi);
  }
}

/// <summary>
/// Projects OklchF to OklabF (polar to rectangular).
/// </summary>
public readonly struct OklchFToOklabF : IProject<OklchF, OklabF> {

  private const float TwoPi = 6.2831853071795864769f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklabF Project(in OklchF oklch) {
    var hRad = oklch.H * TwoPi;
    var a = oklch.C * (float)SysMath.Cos(hRad);
    var b = oklch.C * (float)SysMath.Sin(hRad);
    return new(oklch.L, a, b);
  }
}

/// <summary>
/// Projects LinearRgbF to OklchF.
/// </summary>
public readonly struct LinearRgbFToOklchF : IProject<LinearRgbF, OklchF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklchF Project(in LinearRgbF work) {
    var oklab = new LinearRgbFToOklabF().Project(work);
    return new OklabFToOklchF().Project(oklab);
  }
}

/// <summary>
/// Projects LinearRgbaF to OklchF.
/// </summary>
public readonly struct LinearRgbaFToOklchF : IProject<LinearRgbaF, OklchF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklchF Project(in LinearRgbaF work) {
    var oklab = new LinearRgbaFToOklabF().Project(work);
    return new OklabFToOklchF().Project(oklab);
  }
}

/// <summary>
/// Projects OklchF back to LinearRgbF.
/// </summary>
public readonly struct OklchFToLinearRgbF : IProject<OklchF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in OklchF oklch) {
    var oklab = new OklchFToOklabF().Project(oklch);
    return new OklabFToLinearRgbF().Project(oklab);
  }
}
