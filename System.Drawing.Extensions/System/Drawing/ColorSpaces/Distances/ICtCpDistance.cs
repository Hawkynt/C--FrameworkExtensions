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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Calculates color distance using the ICtCp color space.
/// </summary>
/// <remarks>
/// <para>ICtCp is designed for HDR video content and provides perceptually uniform color differences.</para>
/// </remarks>
public readonly struct ICtCpDistance : IColorDistanceCalculator {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(ICtCpDistanceSquared._Calculate(color1, color2));
}

/// <summary>
/// Calculates squared color distance using the ICtCp color space.
/// Faster than <see cref="ICtCpDistance"/> when only comparing distances.
/// </summary>
/// <remarks>
/// <para>ICtCp is designed for HDR video content and provides perceptually uniform color differences.</para>
/// <para>Use this when you only need to compare relative distances, as it avoids the expensive square root operation.</para>
/// </remarks>
public readonly struct ICtCpDistanceSquared : IColorDistanceCalculator {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(Color color1, Color color2) {
    var ictcp1 = (ICtCp)ICtCp.FromColor(color1);
    var ictcp2 = (ICtCp)ICtCp.FromColor(color2);

    var dI = ictcp1.I - ictcp2.I;
    var dCt = ictcp1.Ct - ictcp2.Ct;
    var dCp = ictcp1.Cp - ictcp2.Cp;

    return dI * dI + dCt * dCt + dCp * dCp;
  }
}