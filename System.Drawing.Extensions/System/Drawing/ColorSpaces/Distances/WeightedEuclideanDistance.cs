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
/// Calculates weighted Euclidean distance in any 3-component color space.
/// </summary>
/// <typeparam name="TColorSpace">The color space to perform the distance calculation in.</typeparam>
/// <param name="w1">Weight for the first component.</param>
/// <param name="w2">Weight for the second component.</param>
/// <param name="w3">Weight for the third component.</param>
/// <param name="wa">Weight for the alpha component.</param>
/// <param name="divisor">Divisor for normalization. Defaults to 1.</param>
/// <remarks>
/// <para>
/// This is a generic weighted Euclidean distance calculator that works with any color space
/// implementing <see cref="IThreeComponentColor"/>. The weighted distance is calculated as:
/// (w1*d1² + w2*d2² + w3*d3² + wa*da²) / divisor
/// </para>
/// <para>
/// Example usage for YUV space with luminance emphasis:
/// <code>
/// var distance = new WeightedEuclideanDistance&lt;Yuv&gt;(6, 2, 2, 10, 20);
/// </code>
/// </para>
/// <para>
/// Example usage for YCbCr space:
/// <code>
/// var distance = new WeightedEuclideanDistance&lt;YCbCr&gt;(2, 1, 1, 1, 5);
/// </code>
/// </para>
/// </remarks>
public readonly struct WeightedEuclideanDistance<TColorSpace>(int w1, int w2, int w3, int wa, int divisor = 1) : IColorDistanceCalculator
  where TColorSpace : struct, IThreeComponentColor {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) {
    var (c1a, c1b, c1c, c1Alpha) = ColorSpaceFactory<TColorSpace>.FromColor(color1);
    var (c2a, c2b, c2c, c2Alpha) = ColorSpaceFactory<TColorSpace>.FromColor(color2);

    var d1 = c1a - c2a;
    var d2 = c1b - c2b;
    var d3 = c1c - c2c;
    var da = c1Alpha - c2Alpha;

    return (w1 * d1 * d1 + w2 * d2 * d2 + w3 * d3 * d3 + wa * da * da) / (double)divisor;
  }
}

public readonly struct WeightedEuclideanDistance4<TColorSpace>(int w1, int w2, int w3, int w4, int wa, int divisor = 1) : IColorDistanceCalculator
  where TColorSpace : struct, IFourComponentColor {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) {
    var (c1a, c1b, c1c, c1d, c1Alpha) = ColorSpaceFactory<TColorSpace>.FromColor(color1);
    var (c2a, c2b, c2c, c2d, c2Alpha) = ColorSpaceFactory<TColorSpace>.FromColor(color2);

    var d1 = c1a - c2a;
    var d2 = c1b - c2b;
    var d3 = c1c - c2c;
    var d4 = c1d - c2d;
    var da = c1Alpha - c2Alpha;

    return (w1 * d1 * d1 + w2 * d2 * d2 + w3 * d3 * d3 + w4 * d4 * d4 + wa * da * da) / (double)divisor;
  }
}

/// <summary>
/// Provides predefined weighted Euclidean distance instances for common color spaces.
/// </summary>
public static class WeightedEuclideanDistances {

  /// <summary>
  /// Weighted YUV distance with default weights: Y=6, U=2, V=2, A=10, divisor=20.
  /// Emphasizes luminance and alpha over chrominance.
  /// </summary>
  public static readonly WeightedEuclideanDistance<Yuv> Yuv = new(6, 2, 2, 10, 20);

  /// <summary>
  /// Weighted YCbCr distance with default weights: Y=2, Cb=1, Cr=1, A=1, divisor=5.
  /// Emphasizes luminance (Y) over chrominance (Cb, Cr).
  /// </summary>
  public static readonly WeightedEuclideanDistance<YCbCr> YCbCr = new(2, 1, 1, 1, 5);

  /// <summary>
  /// Weighted RGB distance with custom weights: R=30, G=59, B=11, A=10, divisor=110.
  /// </summary>
  public static readonly WeightedEuclideanDistance<Rgb> Rgb = new(30, 59, 11, 10, 110);

  /// <summary>
  /// BT.709 (HDTV) weights based on relative luminance formula.
  /// Y = 0.2126R + 0.7152G + 0.0722B
  /// </summary>
  public static readonly WeightedEuclideanDistance<Rgb> BT709 = new(
    DistanceWeights.BT709.Red,
    DistanceWeights.BT709.Green,
    DistanceWeights.BT709.Blue,
    DistanceWeights.BT709.Alpha,
    DistanceWeights.BT709.Divisor
  );

  /// <summary>
  /// Nommyde perceptually optimized weights.
  /// </summary>
  public static readonly WeightedEuclideanDistance<Rgb> Nommyde = new(
    DistanceWeights.Nommyde.Red,
    DistanceWeights.Nommyde.Green,
    DistanceWeights.Nommyde.Blue,
    DistanceWeights.Nommyde.Alpha,
    DistanceWeights.Nommyde.Divisor
  );

  /// <summary>
  /// Low red sensitivity - emphasizes green over red.
  /// </summary>
  public static readonly WeightedEuclideanDistance<Rgb> LowRed = new(
    DistanceWeights.LowRed.Red,
    DistanceWeights.LowRed.Green,
    DistanceWeights.LowRed.Blue,
    DistanceWeights.LowRed.Alpha
  );

  /// <summary>
  /// High red sensitivity - emphasizes red over blue.
  /// </summary>
  public static readonly WeightedEuclideanDistance<Rgb> HighRed = new(
    DistanceWeights.HighRed.Red,
    DistanceWeights.HighRed.Green,
    DistanceWeights.HighRed.Blue,
    DistanceWeights.HighRed.Alpha
  );

}
