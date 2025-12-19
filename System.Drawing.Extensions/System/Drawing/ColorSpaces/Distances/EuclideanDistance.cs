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
/// Calculates Euclidean distance between colors in the specified color space.
/// </summary>
/// <typeparam name="TColorSpace">The color space to perform the distance calculation in.</typeparam>
/// <remarks>
/// <para>
/// This is a zero-cost abstraction - the JIT will fully inline all calls when TColorSpace is a struct.
/// The distance is calculated as: sqrt((c1₁-c2₁)² + (c1₂-c2₂)² + (c1₃-c2₃)²)
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var index = PaletteSearch.GetMostSimilarColorIndex&lt;EuclideanDistance&lt;Yuv&gt;&gt;(palette, color);
/// </code>
/// </para>
/// </remarks>
public readonly struct EuclideanDistance<TColorSpace> : IColorDistanceCalculator
  where TColorSpace : struct, IThreeComponentColor {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(EuclideanDistanceSquared<TColorSpace>._Calculate(color1, color2));
  
}

/// <summary>
/// Calculates squared Euclidean distance between colors in the specified color space.
/// Faster than <see cref="EuclideanDistance{TColorSpace}"/> when only comparing distances.
/// </summary>
/// <typeparam name="TColorSpace">The color space to perform the distance calculation in.</typeparam>
public readonly struct EuclideanDistanceSquared<TColorSpace> : IColorDistanceCalculator
  where TColorSpace : struct, IThreeComponentColor {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2);

  internal static double _Calculate(Color color1, Color color2) {
    var (c1a, c1b, c1c, _) = ColorSpaceFactory<TColorSpace>.FromColor(color1);
    var (c2a, c2b, c2c, _) = ColorSpaceFactory<TColorSpace>.FromColor(color2);

    var d1 = c1a - c2a;
    var d2 = c1b - c2b;
    var d3 = c1c - c2c;

    return d1 * d1 + d2 * d2 + d3 * d3;
  }
}
