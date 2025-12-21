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
/// Calculates color distance using the PNGQuant algorithm.
/// </summary>
/// <param name="whitePoint">
/// White point color for channel weighting. Higher component values = less important,
/// lower values = more important. Default Color.White gives equal weighting.
/// Example: Color.FromArgb(255, 255, 128, 255) makes green 2x more important.
/// </param>
/// <remarks>
/// <para>
/// This algorithm considers how colors appear when blended on both black and white backgrounds,
/// making it particularly effective for semi-transparent colors. It uses the maximum of the
/// distance on black background and distance on white background for each channel.
/// </para>
/// <para>
/// The white point parameter allows adjusting the importance of each color channel.
/// Lower white point values for a channel increase its weight in the distance calculation.
/// </para>
/// <para>
/// Reference: https://github.com/pornel/pngquant/blob/cc39b47799a7ff2ef17b529f9415ff6e6b213b8f/lib/pam.h#L148
/// </para>
/// </remarks>
public readonly struct PngQuantDistance(Color whitePoint) : IColorDistanceCalculator {

  /// <summary>
  /// Default instance with equal channel weighting (Color.White).
  /// </summary>
  public static readonly PngQuantDistance Default = new(Color.White);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(PngQuantDistanceSquared._Calculate(color1, color2, whitePoint));

}

/// <summary>
/// Calculates squared color distance using the PNGQuant algorithm.
/// Faster than <see cref="PngQuantDistance"/> when only comparing distances.
/// </summary>
/// <param name="whitePoint">
/// White point color for channel weighting. Higher component values = less important,
/// lower values = more important. Default Color.White gives equal weighting.
/// Example: Color.FromArgb(255, 255, 128, 255) makes green 2x more important.
/// </param>
/// <remarks>
/// <para>
/// This algorithm considers how colors appear when blended on both black and white backgrounds,
/// making it particularly effective for semi-transparent colors.
/// </para>
/// <para>
/// Reference: https://github.com/pornel/pngquant/blob/cc39b47799a7ff2ef17b529f9415ff6e6b213b8f/lib/pam.h#L148
/// </para>
/// </remarks>
public readonly struct PngQuantDistanceSquared(Color whitePoint) : IColorDistanceCalculator {

  /// <summary>
  /// Default instance with equal channel weighting (Color.White).
  /// </summary>
  public static readonly PngQuantDistanceSquared Default = new(Color.White);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2, whitePoint);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _Calculate(Color color1, Color color2, Color whitePoint) {
    var wp = new Rgba32(whitePoint);
    var c1 = new Rgba32(color1);
    var c2 = new Rgba32(color2);

    // Calculate white point weights with 16-bit precision: (255 << 16) / whitePoint
    var wpR = wp.R > 0 ? (255 << 16) / wp.R : 0;
    var wpG = wp.G > 0 ? (255 << 16) / wp.G : 0;
    var wpB = wp.B > 0 ? (255 << 16) / wp.B : 0;
    var wpA = wp.A > 0 ? (255 << 16) / wp.A : 0;

    var r1 = c1.R;
    var g1 = c1.G;
    var b1 = c1.B;
    var a1 = c1.A;

    var r2 = c2.R;
    var g2 = c2.G;
    var b2 = c2.B;
    var a2 = c2.A;

    // Alpha difference scaled by white point (keep high precision)
    var alphas = (a2 - a1) * wpA;

    var rDiff = _ColorDifferenceCh((r1 * wpR) >> 16, (r2 * wpR) >> 16, alphas);
    var gDiff = _ColorDifferenceCh((g1 * wpG) >> 16, (g2 * wpG) >> 16, alphas);
    var bDiff = _ColorDifferenceCh((b1 * wpB) >> 16, (b2 * wpB) >> 16, alphas);

    return rDiff + gDiff + bDiff;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _ColorDifferenceCh(int x, int y, int alphaDiff) {
    // Maximum of channel blended on white, and blended on black
    var black = x - y;
    var white = black + (alphaDiff >> 16); // Scale alpha back down for blending
    return black * black + white * white;
  }

}
