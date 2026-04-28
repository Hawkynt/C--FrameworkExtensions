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
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Cylindrical;

/// <summary>
/// Projects LuvF to LchUvF (rectangular Luv to cylindrical LCh(uv)).
/// </summary>
public readonly struct LuvFToLchUvF : IProject<LuvF, LchUvF> {

  private const float TwoPi = 2f * MathF.PI;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LchUvF Project(in LuvF luv) {
    var c = MathF.Sqrt(luv.U * luv.U + luv.V * luv.V);
    var h = MathF.Atan2(luv.V, luv.U);
    if (h < 0f) h += TwoPi;
    return new(luv.L, c, h / TwoPi);
  }
}

/// <summary>
/// Projects LchUvF back to LuvF.
/// </summary>
public readonly struct LchUvFToLuvF : IProject<LchUvF, LuvF> {

  private const float TwoPi = 2f * MathF.PI;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LuvF Project(in LchUvF lch) {
    var hRad = lch.H * TwoPi;
    return new(lch.L, lch.C * MathF.Cos(hRad), lch.C * MathF.Sin(hRad));
  }
}

/// <summary>
/// Projects LinearRgbF to LchUvF via Luv.
/// </summary>
public readonly struct LinearRgbFToLchUvF : IProject<LinearRgbF, LchUvF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LchUvF Project(in LinearRgbF work) {
    var luv = new LinearRgbFToLuvF().Project(work);
    return new LuvFToLchUvF().Project(luv);
  }
}

/// <summary>
/// Projects LinearRgbaF to LchUvF via Luv.
/// </summary>
public readonly struct LinearRgbaFToLchUvF : IProject<LinearRgbaF, LchUvF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LchUvF Project(in LinearRgbaF work) {
    var luv = new LinearRgbaFToLuvF().Project(work);
    return new LuvFToLchUvF().Project(luv);
  }
}

/// <summary>
/// Projects LchUvF back to LinearRgbF via Luv.
/// </summary>
public readonly struct LchUvFToLinearRgbF : IProject<LchUvF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in LchUvF lch) {
    var luv = new LchUvFToLuvF().Project(lch);
    return new LuvFToLinearRgbF().Project(luv);
  }
}
