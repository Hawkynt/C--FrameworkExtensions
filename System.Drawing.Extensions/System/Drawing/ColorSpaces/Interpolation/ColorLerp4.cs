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
/// Generic linear interpolation for 4-component color spaces (e.g., CMYK).
/// Zero-cost abstraction - JIT specializes for each color space type.
/// </summary>
/// <typeparam name="TColorSpace">The 4-component color space to interpolate in.</typeparam>
/// <remarks>
/// <para>
/// Example usage:
/// <code>
/// var print = ColorLerp4&lt;Cmyk&gt;.Blend(color1, color2, 128);
/// </code>
/// </para>
/// </remarks>
public readonly struct ColorLerp4<TColorSpace> : IColorLerp
  where TColorSpace : struct, IFourComponentColor {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color Lerp(Color color1, Color color2, byte t) {
    var (c1a, c1b, c1c, c1d, a1) = ColorSpaceFactory<TColorSpace>.FromColor(color1);
    var (c2a, c2b, c2c, c2d, a2) = ColorSpaceFactory<TColorSpace>.FromColor(color2);

    var invT = 255 - t;

    return ColorSpaceConstructor4<TColorSpace>.Create(
      (byte)((c1a * invT + c2a * t + 127) / 255),
      (byte)((c1b * invT + c2b * t + 127) / 255),
      (byte)((c1c * invT + c2c * t + 127) / 255),
      (byte)((c1d * invT + c2d * t + 127) / 255),
      (byte)((a1 * invT + a2 * t + 127) / 255)
    ).ToColor();
  }

  /// <summary>
  /// Static shortcut for direct calls without instantiation.
  /// </summary>
  /// <param name="color1">The starting color (returned when t=0).</param>
  /// <param name="color2">The ending color (returned when t=255).</param>
  /// <param name="t">Interpolation factor (0-255).</param>
  /// <returns>The interpolated color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color Blend(Color color1, Color color2, byte t) => default(ColorLerp4<TColorSpace>).Lerp(color1, color2, t);
}
