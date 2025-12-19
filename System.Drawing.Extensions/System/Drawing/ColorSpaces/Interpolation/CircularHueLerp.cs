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

namespace System.Drawing.ColorSpaces.Interpolation;

/// <summary>
/// Generic circular hue interpolation for hue-based color spaces (HSL, HSV, HWB).
/// The first component is treated as hue and interpolated via the shortest path around the color wheel.
/// </summary>
/// <typeparam name="TColorSpace">The hue-based color space to interpolate in.</typeparam>
/// <remarks>
/// <para>
/// Use this for smooth color transitions that don't go through muddy intermediate colors.
/// For example, red to yellow goes through orange (short path), not through purple/blue (long path).
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var sunset = CircularHueLerp&lt;Hsv&gt;.Blend(red, yellow, t);
/// var rainbow = CircularHueLerp&lt;Hsl&gt;.Blend(red, magenta, t);
/// </code>
/// </para>
/// </remarks>
public readonly struct CircularHueLerp<TColorSpace> : IColorLerp
  where TColorSpace : struct, IThreeComponentColor {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color Lerp(Color color1, Color color2, byte t) {
    var (h1, c1b, c1c, a1) = ColorSpaceFactory<TColorSpace>.FromColor(color1);
    var (h2, c2b, c2c, a2) = ColorSpaceFactory<TColorSpace>.FromColor(color2);

    // Circular hue interpolation - take shortest path around color wheel
    var hDiff = (int)h2 - h1;

    switch (hDiff) {
      // Wrap around if going the long way
      case > 127:
        hDiff -= 256;
        break;
      case < -128:
        hDiff += 256;
        break;
    }

    var hue = h1 + (hDiff * t + 127) / 255;

    switch (hue) {
      // Normalize hue to 0-255 range
      case < 0:
        hue += 256;
        break;
      case >= 256:
        hue -= 256;
        break;
    }

    var invT = 255 - t;

    return ColorSpaceConstructor<TColorSpace>.Create(
      (byte)hue,
      (byte)((c1b * invT + c2b * t + 127) / 255),
      (byte)((c1c * invT + c2c * t + 127) / 255),
      (byte)((a1 * invT + a2 * t + 127) / 255)
    ).ToColor();
  }

  /// <summary>
  /// Static shortcut for direct calls without instantiation.
  /// </summary>
  /// <param name="color1">The starting color (returned when t=0).</param>
  /// <param name="color2">The ending color (returned when t=255).</param>
  /// <param name="t">Interpolation factor (0-255).</param>
  /// <returns>The interpolated color with circular hue blending.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color Blend(Color color1, Color color2, byte t) => default(CircularHueLerp<TColorSpace>).Lerp(color1, color2, t);
}
