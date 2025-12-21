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
/// Represents a color in the CMYK (Cyan, Magenta, Yellow, Key/Black) color space using byte values (0-255).
/// CMYK is a subtractive color model used primarily in color printing.
/// </summary>
/// <param name="C">Cyan component (0-255).</param>
/// <param name="M">Magenta component (0-255).</param>
/// <param name="Y">Yellow component (0-255).</param>
/// <param name="K">Key (Black) component (0-255).</param>
/// <param name="A">Alpha component (0-255). Defaults to 255 (fully opaque).</param>
[ColorSpace(4, ["C", "M", "Y", "K"], ColorSpaceType = ColorSpaceType.Subtractive)]
public record struct Cmyk(byte C, byte M, byte Y, byte K, byte A = 255) : IFourComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this CMYK color to an RGB <see cref="Color"/>.
  /// </summary>
  /// <returns>The RGB color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // R = (1-C)(1-K) * 255, G = (1-M)(1-K) * 255, B = (1-Y)(1-K) * 255
    // Using fixed-point: (255-C) * (255-K) / 255
    var oneMinusK = 255 - this.K;
    var r = ((255 - this.C) * oneMinusK + 127) / 255;
    var g = ((255 - this.M) * oneMinusK + 127) / 255;
    var b = ((255 - this.Y) * oneMinusK + 127) / 255;

    return Color.FromArgb(this.A, r, g, b);
  }

  /// <summary>
  /// Converts this byte-based CMYK color to a normalized (0.0-1.0) CMYK color.
  /// </summary>
  /// <returns>The normalized CMYK color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public CmykNormalized ToNormalized() => new(
    this.C * Rgba32.ByteToNormalized,
    this.M * Rgba32.ByteToNormalized,
    this.Y * Rgba32.ByteToNormalized,
    this.K * Rgba32.ByteToNormalized,
    this.A * Rgba32.ByteToNormalized
  );

  /// <summary>
  /// Creates a CMYK color from an RGB <see cref="Color"/>.
  /// </summary>
  /// <param name="color">The RGB color.</param>
  /// <returns>The CMYK color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var rgba = new Rgba32(color);
    int r = rgba.R, g = rgba.G, b = rgba.B;

    // K = 1 - max(R, G, B) = (255 - max) / 255 in byte form: k = 255 - max
    var max = r > g ? (r > b ? r : b) : (g > b ? g : b);
    var k = 255 - max;

    // If k == 255 (pure black), C = M = Y = 0
    if (k >= 255)
      return new Cmyk(0, 0, 0, 255, rgba.A);

    // C = (1 - R/255 - K) / (1 - K) = (max/255 - R/255) / (max/255) = (max - R) / max
    // In byte scale: c = (max - r) * 255 / max
    var c = ((max - r) * 255 + max / 2) / max;
    var m = ((max - g) * 255 + max / 2) / max;
    var y = ((max - b) * 255 + max / 2) / max;

    return new Cmyk(
      (byte)(c > 255 ? 255 : c),
      (byte)(m > 255 ? 255 : m),
      (byte)(y > 255 ? 255 : y),
      (byte)k,
      rgba.A
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFourComponentColor Create(byte c1, byte c2, byte c3, byte c4, byte a) => new Cmyk(c1, c2, c3, c4, a);

  public T ConvertTo<T>() where T : struct, IFourComponentColor
    => typeof(T) == typeof(Cmyk)
      ?  (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Cmyk)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}

/// <summary>
/// Represents a color in the CMYK (Cyan, Magenta, Yellow, Key/Black) color space using normalized float values (0.0-1.0).
/// CMYK is a subtractive color model used primarily in color printing.
/// </summary>
/// <param name="C">Cyan component (0.0-1.0).</param>
/// <param name="M">Magenta component (0.0-1.0).</param>
/// <param name="Y">Yellow component (0.0-1.0).</param>
/// <param name="K">Key (Black) component (0.0-1.0).</param>
/// <param name="A">Alpha component (0.0-1.0). Defaults to 1.0 (fully opaque).</param>
[ColorSpace(4, ["C", "M", "Y", "K"], ColorSpaceType = ColorSpaceType.Subtractive)]
public record struct CmykNormalized(float C, float M, float Y, float K, float A = 1f) : IFourComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this normalized CMYK color to an RGB <see cref="Color"/>.
  /// </summary>
  /// <returns>The RGB color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToByte().ToColor();

  /// <summary>
  /// Converts this normalized CMYK color to a byte-based CMYK color.
  /// </summary>
  /// <returns>The byte-based CMYK color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Cmyk ToByte() => new(
    (byte)(this.C * 255f + 0.5f),
    (byte)(this.M * 255f + 0.5f),
    (byte)(this.Y * 255f + 0.5f),
    (byte)(this.K * 255f + 0.5f),
    (byte)(this.A * 255f + 0.5f)
  );

  /// <summary>
  /// Creates a normalized CMYK color from an RGB <see cref="Color"/>.
  /// </summary>
  /// <param name="color">The RGB color.</param>
  /// <returns>The normalized CMYK color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((Cmyk)Cmyk.FromColor(color)).ToNormalized();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFourComponentFloatColor Create(float c1, float c2, float c3, float c4, float a) => new CmykNormalized(c1, c2, c3, c4, a);

  public T ConvertTo<T>() where T : struct, IFourComponentFloatColor
    => typeof(T) == typeof(CmykNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(CmykNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}
