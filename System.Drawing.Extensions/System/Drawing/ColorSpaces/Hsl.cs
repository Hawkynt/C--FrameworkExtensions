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
/// Represents a color in the HSL (Hue, Saturation, Lightness) color space using byte values (0-255).
/// </summary>
/// <param name="H">Hue component (0-255, maps to 0-360 degrees).</param>
/// <param name="S">Saturation component (0-255).</param>
/// <param name="L">Lightness component (0-255).</param>
/// <param name="A">Alpha component (0-255). Defaults to 255 (fully opaque).</param>
[ColorSpace(3, ["H", "S", "L"], ColorSpaceType = ColorSpaceType.Cylindrical)]
public record struct Hsl(byte H, byte S, byte L, byte A = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this HSL color to an RGB <see cref="Color"/>.
  /// </summary>
  /// <returns>The RGB color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Fixed-point HSL to RGB conversion
    // All values in 0-255 byte range
    if (this.S == 0)
      // Achromatic (gray)
      return Color.FromArgb(this.A, this.L, this.L, this.L);

    // Using 16.8 fixed-point for intermediate calculations
    // q = L < 0.5 ? L * (1 + S) : L + S - L * S
    // In byte scale: q = L < 128 ? L * (255 + S) / 255 : L + S - L * S / 255
    int q;
    if (this.L < 128)
      q = (this.L * (255 + this.S) + 127) / 255;
    else
      q = this.L + this.S - (this.L * this.S + 127) / 255;

    // p = 2 * L - q
    var p = 2 * this.L - q;

    // Convert hue (0-255) to position in color wheel (0-1530 = 6 * 255)
    var h6 = this.H * 6;

    // Snap to sector boundaries when very close (compensates for 255 not being divisible by 6)
    var hMod = h6 % 255;
    switch (hMod) {
      case < 4:
        h6 -= hMod;
        break;
      case > 251:
        h6 += 255 - hMod;
        break;
    }

    // Calculate r, g, b using hue offset
    var r = _Hue2Rgb(p, q, h6 + 510);  // h + 1/3 = h + 255*2
    var g = _Hue2Rgb(p, q, h6);
    var b = _Hue2Rgb(p, q, h6 - 510);  // h - 1/3 = h - 255*2

    return Color.FromArgb(this.A, r, g, b);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _Hue2Rgb(int p, int q, int t) {
    switch (t) {
      // Wrap t to 0-1530 range (6 * 255)
      case < 0:
        t += 1530;
        break;
      case >= 1530:
        t -= 1530;
        break;
    }

    return t switch {
      // t < 1/6 (255)
      < 255 => (byte)(p + ((q - p) * t + 127) / 255),
      // t < 1/2 (765)
      < 765 => (byte)q,
      // t < 2/3 (1020)
      < 1020 => (byte)(p + ((q - p) * (1020 - t) + 127) / 255),
      _ => (byte)p
    };
  }

  /// <summary>
  /// Converts this byte-based HSL color to a normalized (0.0-1.0) HSL color.
  /// </summary>
  /// <returns>The normalized HSL color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HslNormalized ToNormalized() => new(
    this.H * Rgba32.ByteToNormalized,
    this.S * Rgba32.ByteToNormalized,
    this.L * Rgba32.ByteToNormalized,
    this.A * Rgba32.ByteToNormalized
  );

  /// <summary>
  /// Creates an HSL color from an RGB <see cref="Color"/>.
  /// </summary>
  /// <param name="color">The RGB color.</param>
  /// <returns>The HSL color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var c = new Rgba32(color);
    int r = c.R, g = c.G, b = c.B;

    var max = r > g ? (r > b ? r : b) : (g > b ? g : b);
    var min = r < g ? (r < b ? r : b) : (g < b ? g : b);
    var l = (max + min + 1) / 2;  // Lightness with rounding

    if (max == min)
      // Achromatic (gray)
      return new Hsl(0, 0, (byte)l, color.A);

    var d = max - min;

    // Saturation: s = l > 0.5 ? d / (2 - max - min) : d / (max + min)
    // In byte scale: s = l > 127 ? d * 255 / (510 - max - min) : d * 255 / (max + min)
    int s;
    if (l > 127)
      s = (d * 255 + (510 - max - min) / 2) / (510 - max - min);
    else
      s = (d * 255 + (max + min) / 2) / (max + min);

    // Hue calculation: result in 0-1530 range (6 sectors of 255 each)
    int h;
    if (max == r)
      // h = (g - b) / d + (g < b ? 6 : 0)
      h = g >= b ? ((g - b) * 255 + d / 2) / d : ((g - b) * 255 - d / 2) / d + 1530;
    else if (max == g)
      // h = (b - r) / d + 2
      h = ((b - r) * 255 + d / 2) / d + 510;
    else
      // h = (r - g) / d + 4
      h = ((r - g) * 255 + d / 2) / d + 1020;

    // Scale from 0-1530 to 0-255
    h = (h * 255 + 765) / 1530;

    return new Hsl(
      (byte)(h < 0 ? 0 : h > 255 ? 255 : h),
      (byte)(s < 0 ? 0 : s > 255 ? 255 : s),
      (byte)l,
      c.A
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Hsl(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentColor
    => typeof(T) == typeof(Hsl)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Hsl)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}

/// <summary>
/// Represents a color in the HSL (Hue, Saturation, Lightness) color space using normalized float values (0.0-1.0).
/// </summary>
/// <param name="H">Hue component (0.0-1.0, represents 0-360 degrees).</param>
/// <param name="S">Saturation component (0.0-1.0).</param>
/// <param name="L">Lightness component (0.0-1.0).</param>
/// <param name="A">Alpha component (0.0-1.0). Defaults to 1.0 (fully opaque).</param>
[ColorSpace(3, ["H", "S", "L"], ColorSpaceType = ColorSpaceType.Cylindrical)]
public record struct HslNormalized(float H, float S, float L, float A = 1f) : IThreeComponentFloatColor {
  
  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this normalized HSL color to an RGB <see cref="Color"/>.
  /// </summary>
  /// <returns>The RGB color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToByte().ToColor();

  /// <summary>
  /// Converts this normalized HSL color to a byte-based HSL color.
  /// </summary>
  /// <returns>The byte-based HSL color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Hsl ToByte() => new(
    (byte)(this.H * 255f + 0.5f),
    (byte)(this.S * 255f + 0.5f),
    (byte)(this.L * 255f + 0.5f),
    (byte)(this.A * 255f + 0.5f)
  );

  /// <summary>
  /// Creates a normalized HSL color from an RGB <see cref="Color"/>.
  /// </summary>
  /// <param name="color">The RGB color.</param>
  /// <returns>The normalized HSL color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((Hsl)Hsl.FromColor(color)).ToNormalized();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new HslNormalized(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(HslNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(HslNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}
