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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.ColorSpaces.Distances;

/// <summary>
/// Calculates Chebyshev (maximum) distance between colors in the specified color space.
/// </summary>
/// <typeparam name="TColorSpace">The color space to perform the distance calculation in.</typeparam>
/// <remarks>
/// The distance is calculated as: max(|c1₁-c2₁|, |c1₂-c2₂|, |c1₃-c2₃|)
/// </remarks>
public readonly struct ChebyshevDistance<TColorSpace> : IColorDistanceCalculator
  where TColorSpace : struct, IThreeComponentColor {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) {
    var (c1a, c1b, c1c, _) = ColorSpaceFactory<TColorSpace>.FromColor(color1);
    var (c2a, c2b, c2c, _) = ColorSpaceFactory<TColorSpace>.FromColor(color2);

    var d1 = Math.Abs(c1a - c2a);
    var d2 = Math.Abs(c1b - c2b);
    var d3 = Math.Abs(c1c - c2c);

    return d1 > d2 ? (d1 > d3 ? d1 : d3) : (d2 > d3 ? d2 : d3);
  }
}

/// <summary>
/// Calculates Chebyshev distance between colors in a four-component color space.
/// </summary>
public readonly struct ChebyshevDistance4<TColorSpace> : IColorDistanceCalculator
  where TColorSpace : struct, IFourComponentColor {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) {
    var (c1a, c1b, c1c, c1d, _) = ColorSpaceFactory<TColorSpace>.FromColor(color1);
    var (c2a, c2b, c2c, c2d, _) = ColorSpaceFactory<TColorSpace>.FromColor(color2);

    var d1 = Math.Abs(c1a - c2a);
    var d2 = Math.Abs(c1b - c2b);
    var d3 = Math.Abs(c1c - c2c);
    var d4 = Math.Abs(c1d - c2d);

    var max12 = d1 > d2 ? d1 : d2;
    var max34 = d3 > d4 ? d3 : d4;
    return max12 > max34 ? max12 : max34;
  }
}
