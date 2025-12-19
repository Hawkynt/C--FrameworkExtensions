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
/// Calculates Manhattan (taxicab) distance between colors in the specified color space.
/// </summary>
/// <typeparam name="TColorSpace">The color space to perform the distance calculation in.</typeparam>
/// <remarks>
/// The distance is calculated as: |c1₁-c2₁| + |c1₂-c2₂| + |c1₃-c2₃|
/// </remarks>
public readonly struct ManhattanDistance<TColorSpace> : IColorDistanceCalculator
  where TColorSpace : struct, IThreeComponentColor {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) {
    var (c1a, c1b, c1c, _) = ColorSpaceFactory<TColorSpace>.FromColor(color1);
    var (c2a, c2b, c2c, _) = ColorSpaceFactory<TColorSpace>.FromColor(color2);

    return Math.Abs(c1a - c2a)
         + Math.Abs(c1b - c2b)
         + Math.Abs(c1c - c2c);
  }
}

/// <summary>
/// Calculates Manhattan distance between colors in a four-component color space.
/// </summary>
public readonly struct ManhattanDistance4<TColorSpace> : IColorDistanceCalculator
  where TColorSpace : struct, IFourComponentColor {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) {
    var (c1a, c1b, c1c, c1d, _) = ColorSpaceFactory<TColorSpace>.FromColor(color1);
    var (c2a, c2b, c2c, c2d, _) = ColorSpaceFactory<TColorSpace>.FromColor(color2);

    return Math.Abs(c1a - c2a)
         + Math.Abs(c1b - c2b)
         + Math.Abs(c1c - c2c)
         + Math.Abs(c1d - c2d);
  }
}
