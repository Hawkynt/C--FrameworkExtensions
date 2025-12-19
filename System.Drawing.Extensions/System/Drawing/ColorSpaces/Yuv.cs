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
/// Represents a color in the YUV color space using byte values (0-255).
/// YUV is commonly used in PAL/NTSC analog video systems.
/// </summary>
/// <param name="Y">Luminance component (0-255).</param>
/// <param name="U">Chrominance U component (0-255, centered at 128).</param>
/// <param name="V">Chrominance V component (0-255, centered at 128).</param>
/// <param name="A">Alpha component (0-255). Defaults to 255 (fully opaque).</param>
/// <remarks>
/// <para>
/// YUV differs from YCbCr in coefficients and intended use:
/// - YUV: Analog video (PAL/NTSC) with coefficients Y = 0.299R + 0.587G + 0.114B
/// - YCbCr: Digital video (ITU-R BT.601) with similar but slightly different scaling
/// </para>
/// <para>
/// The U and V components are stored as 0-255 with 128 representing zero chrominance.
/// </para>
/// </remarks>
public record struct Yuv(byte Y, byte U, byte V, byte A = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  // YUV to RGB conversion constants in 16.16 fixed-point
  // R = Y + 1.13983 * V
  // G = Y - 0.39465 * U - 0.58060 * V
  // B = Y + 2.03211 * U
  private const int YUV_R_V = 74711;   // 1.13983 * 65536
  private const int YUV_G_U = -25872;  // -0.39465 * 65536
  private const int YUV_G_V = -38050;  // -0.58060 * 65536
  private const int YUV_B_U = 133176;  // 2.03211 * 65536

  /// <summary>
  /// Converts this YUV color to an RGB <see cref="Color"/>.
  /// </summary>
  /// <returns>The RGB color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Center U and V around 0 (-128 to 127)
    var uCentered = this.U - 128;
    var vCentered = this.V - 128;

    // Apply conversion matrix
    var r = this.Y + ((YUV_R_V * vCentered + 32768) >> 16);
    var g = this.Y + ((YUV_G_U * uCentered + YUV_G_V * vCentered + 32768) >> 16);
    var b = this.Y + ((YUV_B_U * uCentered + 32768) >> 16);

    // Clamp to 0-255
    return Color.FromArgb(
      this.A,
      r < 0 ? 0 : r > 255 ? 255 : r,
      g < 0 ? 0 : g > 255 ? 255 : g,
      b < 0 ? 0 : b > 255 ? 255 : b
    );
  }

  /// <summary>
  /// Converts this byte-based YUV color to a normalized (0.0-1.0) YUV color.
  /// </summary>
  /// <returns>The normalized YUV color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public YuvNormalized ToNormalized() => new(
    this.Y * Rgba32.ByteToNormalized,
    (this.U - 128) * Rgba32.ByteToNormalized,
    (this.V - 128) * Rgba32.ByteToNormalized,
    this.A * Rgba32.ByteToNormalized
  );

  // RGB to YUV conversion constants in 16.16 fixed-point
  // Y = 0.299R + 0.587G + 0.114B
  // U = 0.492 * (B - Y) = -0.14713R - 0.28886G + 0.436B
  // V = 0.877 * (R - Y) = 0.615R - 0.51499G - 0.10001B
  private const int YUV_Y_R = 19595;   // 0.299 * 65536
  private const int YUV_Y_G = 38470;   // 0.587 * 65536
  private const int YUV_Y_B = 7471;    // 0.114 * 65536
  private const int YUV_U_R = -9642;   // -0.14713 * 65536
  private const int YUV_U_G = -18931;  // -0.28886 * 65536
  private const int YUV_U_B = 28573;   // 0.436 * 65536
  private const int YUV_V_R = 40318;   // 0.615 * 65536
  private const int YUV_V_G = -33750;  // -0.51499 * 65536
  private const int YUV_V_B = -6568;   // -0.10001 * 65536

  /// <summary>
  /// Creates a YUV color from an RGB <see cref="Color"/>.
  /// </summary>
  /// <param name="color">The RGB color.</param>
  /// <returns>The YUV color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var c = new Rgba32(color);
    int r = c.R, g = c.G, b = c.B;

    // Calculate Y (luminance)
    var y = (YUV_Y_R * r + YUV_Y_G * g + YUV_Y_B * b + 32768) >> 16;

    // Calculate U (blue chrominance) and add 128 to center
    var u = ((YUV_U_R * r + YUV_U_G * g + YUV_U_B * b + 32768) >> 16) + 128;

    // Calculate V (red chrominance) and add 128 to center
    var v = ((YUV_V_R * r + YUV_V_G * g + YUV_V_B * b + 32768) >> 16) + 128;

    return new Yuv(
      (byte)(y < 0 ? 0 : y > 255 ? 255 : y),
      (byte)(u < 0 ? 0 : u > 255 ? 255 : u),
      (byte)(v < 0 ? 0 : v > 255 ? 255 : v),
      c.A
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Yuv(c1, c2, c3, a);

}

/// <summary>
/// Represents a color in the YUV color space using normalized float values.
/// </summary>
/// <param name="Y">Luminance component (0.0-1.0).</param>
/// <param name="U">Chrominance U component (-0.5 to 0.5, centered at 0).</param>
/// <param name="V">Chrominance V component (-0.5 to 0.5, centered at 0).</param>
/// <param name="A">Alpha component (0.0-1.0). Defaults to 1.0 (fully opaque).</param>
public record struct YuvNormalized(float Y, float U, float V, float A = 1f) : IThreeComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this normalized YUV color to an RGB <see cref="Color"/>.
  /// </summary>
  /// <returns>The RGB color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToByte().ToColor();

  /// <summary>
  /// Converts this normalized YUV color to a byte-based YUV color.
  /// </summary>
  /// <returns>The byte-based YUV color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Yuv ToByte() => new(
    (byte)(this.Y * 255f + 0.5f),
    (byte)((this.U + 0.5f) * 255f + 0.5f),
    (byte)((this.V + 0.5f) * 255f + 0.5f),
    (byte)(this.A * 255f + 0.5f)
  );

  /// <summary>
  /// Creates a normalized YUV color from an RGB <see cref="Color"/>.
  /// </summary>
  /// <param name="color">The RGB color.</param>
  /// <returns>The normalized YUV color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((Yuv)Yuv.FromColor(color)).ToNormalized();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new YuvNormalized(c1, c2, c3, a);

}
