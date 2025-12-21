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

/// <summary>HWB color space with byte components (H: 0-255, W: 0-255, B: 0-255)</summary>
[ColorSpace(3, ["H", "W", "B"], ColorSpaceType = ColorSpaceType.Cylindrical)]
public record struct Hwb(byte H, byte W, byte B, byte A = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    var w = this.W;
    var b = this.B;

    // Handle the case where W + B >= 255 (achromatic)
    var sum = w + b;
    if (sum >= 255) {
      var gray = (w * 255 + sum / 2) / sum;
      return Color.FromArgb(this.A, gray, gray, gray);
    }

    // Convert HWB to RGB via HSV
    // V = 1 - B/255 = (255 - B) / 255
    // S = 1 - W / V = 1 - W * 255 / (255 - B) = ((255 - B) - W) / (255 - B)
    var v = 255 - b;
    var s = ((v - w) * 255 + v / 2) / v;

    // Use HSV to RGB conversion with fixed-point
    var h6 = this.H * 6;
    var sector = h6 / 255;
    var f = h6 - sector * 255;

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

    var p = (v * (255 - s) + 127) / 255;
    var q = (v * (255 * 255 - f * s) + 32512) / 65025;
    var t = (v * (255 * 255 - (255 - f) * s) + 32512) / 65025;

    int r, g, bl;
    switch (sector % 6) {
      case 0: r = v; g = t; bl = p; break;
      case 1: r = q; g = v; bl = p; break;
      case 2: r = p; g = v; bl = t; break;
      case 3: r = p; g = q; bl = v; break;
      case 4: r = t; g = p; bl = v; break;
      default: r = v; g = p; bl = q; break;
    }

    return Color.FromArgb(this.A, r, g, bl);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public HwbNormalized ToNormalized() => new(this.H * Rgba32.ByteToNormalized, this.W * Rgba32.ByteToNormalized, this.B * Rgba32.ByteToNormalized, this.A * Rgba32.ByteToNormalized);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var c = new Rgba32(color);
    int r = c.R, g = c.G, b = c.B;

    var max = r > g ? (r > b ? r : b) : (g > b ? g : b);
    var min = r < g ? (r < b ? r : b) : (g < b ? g : b);

    // Whiteness and Blackness
    var w = min;
    var bl = 255 - max;

    // Hue calculation (same as HSV)
    if (max == min)
      return new Hwb(0, (byte)w, (byte)bl, color.A);

    var d = max - min;
    int h;
    if (max == r)
      h = g >= b ? ((g - b) * 255 + d / 2) / d : ((g - b) * 255 - d / 2) / d + 1530;
    else if (max == g)
      h = ((b - r) * 255 + d / 2) / d + 510;
    else
      h = ((r - g) * 255 + d / 2) / d + 1020;

    // Scale from 0-1530 to 0-255
    h = (h * 255 + 765) / 1530;

    return new Hwb(
      (byte)(h < 0 ? 0 : h > 255 ? 255 : h),
      (byte)w,
      (byte)bl,
      c.A
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Hwb(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentColor
    => typeof(T) == typeof(Hwb)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Hwb)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}

/// <summary>HWB color space with normalized components (0.0-1.0)</summary>
[ColorSpace(3, ["H", "W", "B"], ColorSpaceType = ColorSpaceType.Cylindrical)]
public record struct HwbNormalized(float H, float W, float B, float A = 1f) : IThreeComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToByte().ToColor();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Hwb ToByte() => new(
    (byte)(this.H * 255f + 0.5f),
    (byte)(this.W * 255f + 0.5f),
    (byte)(this.B * 255f + 0.5f),
    (byte)(this.A * 255f + 0.5f)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((Hwb)Hwb.FromColor(color)).ToNormalized();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new HwbNormalized(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(HwbNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(HwbNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}
