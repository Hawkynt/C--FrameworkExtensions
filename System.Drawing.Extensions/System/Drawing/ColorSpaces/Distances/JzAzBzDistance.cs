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
/// Calculates color distance using the JzAzBz perceptual color space.
/// </summary>
/// <remarks>
/// <para>JzAzBz is optimized for HDR content and provides excellent perceptual uniformity.</para>
/// </remarks>
public readonly struct JzAzBzDistance : IColorDistanceCalculator {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(JzAzBzDistanceSquared._Calculate(color1, color2));
}

/// <summary>
/// Calculates squared color distance using the JzAzBz perceptual color space.
/// Faster than <see cref="JzAzBzDistance"/> when only comparing distances.
/// </summary>
/// <remarks>
/// <para>JzAzBz is optimized for HDR content and provides excellent perceptual uniformity.</para>
/// <para>Use this when you only need to compare relative distances, as it avoids the expensive square root operation.</para>
/// </remarks>
public readonly struct JzAzBzDistanceSquared : IColorDistanceCalculator {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(Color color1, Color color2) {
    var jzazbz1 = (JzAzBz)JzAzBz.FromColor(color1);
    var jzazbz2 = (JzAzBz)JzAzBz.FromColor(color2);

    var dJz = jzazbz1.Jz - jzazbz2.Jz;
    var dAz = jzazbz1.Az - jzazbz2.Az;
    var dBz = jzazbz1.Bz - jzazbz2.Bz;

    return dJz * dJz + dAz * dAz + dBz * dBz;
  }
}
