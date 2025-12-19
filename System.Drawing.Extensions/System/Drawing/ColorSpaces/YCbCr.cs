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

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Represents a color in the YCbCr color space using byte values (0-255).
/// Uses ITU-R BT.601 conversion standard.
/// </summary>
/// <param name="Y">Luminance component (0-255).</param>
/// <param name="Cb">Chrominance blue component (0-255).</param>
/// <param name="Cr">Chrominance red component (0-255).</param>
/// <param name="A">Alpha component (0-255). Defaults to 255 (fully opaque).</param>
public record struct YCbCr(byte Y, byte Cb, byte Cr, byte A = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this YCbCr color to an RGB <see cref="Color"/>.
  /// </summary>
  /// <returns>The RGB color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Use fixed-point math for direct byte-to-byte conversion
    // Unshift Cb and Cr from 0-255 range to centered range (-128 to 127)
    var cb = this.Cb - 128;
    var cr = this.Cr - 128;

    // Convert to RGB using ITU-R BT.601 with fixed-point constants
    // R = Y + 1.402 * Cr
    // G = Y - 0.344136 * Cb - 0.714136 * Cr
    // B = Y + 1.772 * Cb
    // Using 16.16 constants but input is already in byte scale, so shift by 16
    var r = this.Y + ((FixedPointMath.YCbCr_R_Cr * cr + 32768) >> 16);
    var g = this.Y + ((FixedPointMath.YCbCr_G_Cb * cb + FixedPointMath.YCbCr_G_Cr * cr + 32768) >> 16);
    var b = this.Y + ((FixedPointMath.YCbCr_B_Cb * cb + 32768) >> 16);

    // Clamp to 0-255
    return Color.FromArgb(
      this.A,
      r < 0 ? 0 : r > 255 ? 255 : r,
      g < 0 ? 0 : g > 255 ? 255 : g,
      b < 0 ? 0 : b > 255 ? 255 : b
    );
  }

  /// <summary>
  /// Converts this byte-based YCbCr color to a normalized (0.0-1.0) YCbCr color.
  /// </summary>
  /// <returns>The normalized YCbCr color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YCbCrNormalized ToNormalized() => new(
    this.Y * Rgba32.ByteToNormalized,
    this.Cb * Rgba32.ByteToNormalized,
    this.Cr * Rgba32.ByteToNormalized,
    this.A * Rgba32.ByteToNormalized
  );

  /// <summary>
  /// Creates a YCbCr color from an RGB <see cref="Color"/>.
  /// </summary>
  /// <param name="color">The RGB color.</param>
  /// <returns>The YCbCr color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var c = new Rgba32(color);
    int r = c.R, g = c.G, b = c.B;

    // Y = 0.299*R + 0.587*G + 0.114*B (result scaled by 65536, then shift back by 16)
    var y = (FixedPointMath.YCbCr_Y_R * r + FixedPointMath.YCbCr_Y_G * g + FixedPointMath.YCbCr_Y_B * b + 32768) >> 16;

    // Cb = -0.168736*R - 0.331264*G + 0.5*B + 128
    var cb = ((FixedPointMath.YCbCr_Cb_R * r + FixedPointMath.YCbCr_Cb_G * g + FixedPointMath.YCbCr_Cb_B * b + 32768) >> 16) + 128;

    // Cr = 0.5*R - 0.418688*G - 0.081312*B + 128
    var cr = ((FixedPointMath.YCbCr_Cr_R * r + FixedPointMath.YCbCr_Cr_G * g + FixedPointMath.YCbCr_Cr_B * b + 32768) >> 16) + 128;

    return new YCbCr(
      (byte)(y),
      (byte)(cb > 255 ? 255 : cb),
      (byte)(cr > 255 ? 255 : cr),
      c.A
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new YCbCr(c1, c2, c3, a);

}

/// <summary>
/// Represents a color in the YCbCr color space using normalized float values (0.0-1.0).
/// Uses ITU-R BT.601 conversion standard.
/// </summary>
/// <param name="Y">Luminance component (0.0-1.0).</param>
/// <param name="Cb">Chrominance blue component (0.0-1.0).</param>
/// <param name="Cr">Chrominance red component (0.0-1.0).</param>
/// <param name="A">Alpha component (0.0-1.0). Defaults to 1.0 (fully opaque).</param>
public record struct YCbCrNormalized(float Y, float Cb, float Cr, float A = 1f) : IThreeComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this normalized YCbCr color to an RGB <see cref="Color"/>.
  /// </summary>
  /// <returns>The RGB color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToByte().ToColor();

  /// <summary>
  /// Converts this normalized YCbCr color to a byte-based YCbCr color.
  /// </summary>
  /// <returns>The byte-based YCbCr color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YCbCr ToByte() => new(
    (byte)(this.Y * 255f + 0.5f),
    (byte)(this.Cb * 255f + 0.5f),
    (byte)(this.Cr * 255f + 0.5f),
    (byte)(this.A * 255f + 0.5f)
  );

  /// <summary>
  /// Creates a normalized YCbCr color from an RGB <see cref="Color"/>.
  /// </summary>
  /// <param name="color">The RGB color.</param>
  /// <returns>The normalized YCbCr color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((YCbCr)YCbCr.FromColor(color)).ToNormalized();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new YCbCrNormalized(c1, c2, c3, a);

}
