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
/// Represents a color in the HSV (Hue, Saturation, Value) color space using byte values (0-255).
/// </summary>
/// <param name="H">Hue component (0-255, maps to 0-360 degrees).</param>
/// <param name="S">Saturation component (0-255).</param>
/// <param name="V">Value component (0-255).</param>
/// <param name="A">Alpha component (0-255). Defaults to 255 (fully opaque).</param>
public record struct Hsv(byte H, byte S, byte V, byte A = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this HSV color to an RGB <see cref="Color"/>.
  /// </summary>
  /// <returns>The RGB color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    if (this.S == 0)
      // Achromatic (gray)
      return Color.FromArgb(this.A, this.V, this.V, this.V);

    // h scaled to 0-1530 (6 * 255) for sector selection
    var h = this.H * 6;
    var sector = h / 255;  // 0-5
    var f = h - sector * 255;  // fractional part 0-254

    switch (f) {
      // Snap to sector boundaries when very close (compensates for 255 not being divisible by 6)
      case < 4:
        f = 0;
        break;
      case > 251:
        f = 0;
        sector = (sector + 1) % 6;
        break;
    }

    // p = V * (1 - S) = V * (255 - S) / 255
    var p = (this.V * (255 - this.S) + 127) / 255;
    // q = V * (1 - f * S) = V * (255 - f * S / 255) / 255
    var q = (this.V * (255 * 255 - f * this.S) + 32512) / 65025;
    // t = V * (1 - (1 - f) * S) = V * (255 - (255 - f) * S / 255) / 255
    var t = (this.V * (255 * 255 - (255 - f) * this.S) + 32512) / 65025;

    int r, g, b;
    switch (sector % 6) {
      case 0: r = this.V; g = t; b = p; break;
      case 1: r = q; g = this.V; b = p; break;
      case 2: r = p; g = this.V; b = t; break;
      case 3: r = p; g = q; b = this.V; break;
      case 4: r = t; g = p; b = this.V; break;
      default: r = this.V; g = p; b = q; break;
    }

    return Color.FromArgb(this.A, r, g, b);
  }

  /// <summary>
  /// Converts this byte-based HSV color to a normalized (0.0-1.0) HSV color.
  /// </summary>
  /// <returns>The normalized HSV color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HsvNormalized ToNormalized() => new(
    this.H * Rgba32.ByteToNormalized,
    this.S * Rgba32.ByteToNormalized,
    this.V * Rgba32.ByteToNormalized,
    this.A * Rgba32.ByteToNormalized
  );

  /// <summary>
  /// Creates an HSV color from an RGB <see cref="Color"/>.
  /// </summary>
  /// <param name="color">The RGB color.</param>
  /// <returns>The HSV color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var c = new Rgba32(color);
    int r = c.R, g = c.G, b = c.B;

    var max = r > g ? (r > b ? r : b) : (g > b ? g : b);
    var min = r < g ? (r < b ? r : b) : (g < b ? g : b);
    var d = max - min;
    var v = max;

    if (max == 0)
      return new Hsv(0, 0, (byte)v, color.A);

    // s = d / max * 255
    var s = (d * 255 + max / 2) / max;

    if (d == 0)
      return new Hsv(0, (byte)s, (byte)v, color.A);

    // Hue calculation
    int h;
    if (max == r)
      // h = (g - b) / d (+ 6 if negative)
      h = g >= b ? ((g - b) * 255 + d / 2) / d : ((g - b) * 255 - d / 2) / d + 1530;
    else if (max == g)
      // h = (b - r) / d + 2
      h = ((b - r) * 255 + d / 2) / d + 510;
    else
      // h = (r - g) / d + 4
      h = ((r - g) * 255 + d / 2) / d + 1020;

    // Scale from 0-1530 to 0-255
    h = (h * 255 + 765) / 1530;

    return new Hsv(
      (byte)(h < 0 ? 0 : h > 255 ? 255 : h),
      (byte)s,
      (byte)v,
      c.A
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Hsv(c1, c2, c3, a);

}

/// <summary>
/// Represents a color in the HSV (Hue, Saturation, Value) color space using normalized float values (0.0-1.0).
/// </summary>
/// <param name="H">Hue component (0.0-1.0, represents 0-360 degrees).</param>
/// <param name="S">Saturation component (0.0-1.0).</param>
/// <param name="V">Value component (0.0-1.0).</param>
/// <param name="A">Alpha component (0.0-1.0). Defaults to 1.0 (fully opaque).</param>
public record struct HsvNormalized(float H, float S, float V, float A = 1f) : IThreeComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this normalized HSV color to an RGB <see cref="Color"/>.
  /// </summary>
  /// <returns>The RGB color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToByte().ToColor();

  /// <summary>
  /// Converts this normalized HSV color to a byte-based HSV color.
  /// </summary>
  /// <returns>The byte-based HSV color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Hsv ToByte() => new(
    (byte)(this.H * 255f + 0.5f),
    (byte)(this.S * 255f + 0.5f),
    (byte)(this.V * 255f + 0.5f),
    (byte)(this.A * 255f + 0.5f)
  );

  /// <summary>
  /// Creates a normalized HSV color from an RGB <see cref="Color"/>.
  /// </summary>
  /// <param name="color">The RGB color.</param>
  /// <returns>The normalized HSV color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((Hsv)Hsv.FromColor(color)).ToNormalized();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new HsvNormalized(c1, c2, c3, a);

}
