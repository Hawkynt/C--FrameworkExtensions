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

namespace Hawkynt.ColorProcessing.Spaces.Hdr;

/// <summary>
/// Projects JzAzBzF to JzCzhzF (rectangular to polar).
/// </summary>
public readonly struct JzAzBzFToJzCzhzF : IProject<JzAzBzF, JzCzhzF> {

  private const float TwoPi = 6.2831853071795864769f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public JzCzhzF Project(in JzAzBzF jzazbz) {
    var cz = (float)SysMath.Sqrt(jzazbz.Az * jzazbz.Az + jzazbz.Bz * jzazbz.Bz);
    var hz = (float)SysMath.Atan2(jzazbz.Bz, jzazbz.Az);
    if (hz < 0) hz += TwoPi;
    return new(jzazbz.Jz, cz, hz / TwoPi);
  }
}

/// <summary>
/// Projects JzCzhzF to JzAzBzF (polar to rectangular).
/// </summary>
public readonly struct JzCzhzFToJzAzBzF : IProject<JzCzhzF, JzAzBzF> {

  private const float TwoPi = 6.2831853071795864769f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public JzAzBzF Project(in JzCzhzF jzczhz) {
    var hRad = jzczhz.Hz * TwoPi;
    var az = jzczhz.Cz * (float)SysMath.Cos(hRad);
    var bz = jzczhz.Cz * (float)SysMath.Sin(hRad);
    return new(jzczhz.Jz, az, bz);
  }
}

/// <summary>
/// Projects LinearRgbF to JzCzhzF.
/// </summary>
public readonly struct LinearRgbFToJzCzhzF : IProject<LinearRgbF, JzCzhzF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public JzCzhzF Project(in LinearRgbF work) {
    var jzazbz = new LinearRgbFToJzAzBzF().Project(work);
    return new JzAzBzFToJzCzhzF().Project(jzazbz);
  }
}

/// <summary>
/// Projects LinearRgbaF to JzCzhzF.
/// </summary>
public readonly struct LinearRgbaFToJzCzhzF : IProject<LinearRgbaF, JzCzhzF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public JzCzhzF Project(in LinearRgbaF work) {
    var jzazbz = new LinearRgbaFToJzAzBzF().Project(work);
    return new JzAzBzFToJzCzhzF().Project(jzazbz);
  }
}

/// <summary>
/// Projects JzCzhzF back to LinearRgbF.
/// </summary>
public readonly struct JzCzhzFToLinearRgbF : IProject<JzCzhzF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in JzCzhzF jzczhz) {
    var jzazbz = new JzCzhzFToJzAzBzF().Project(jzczhz);
    return new JzAzBzFToLinearRgbF().Project(jzazbz);
  }
}
