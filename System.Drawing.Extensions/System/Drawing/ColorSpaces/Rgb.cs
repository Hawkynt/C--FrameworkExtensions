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
/// Represents a color in the RGB color space using byte values (0-255).
/// </summary>
[ColorSpace(3, ["R", "G", "B"], ColorSpaceType = ColorSpaceType.Additive)]
public record struct Rgb(byte R, byte G, byte B, byte A = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this RGB color to a System.Drawing.Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => Color.FromArgb(this.A, this.R, this.G, this.B);

  /// <summary>
  /// Converts this RGB color to normalized float values (0.0-1.0).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public RgbNormalized ToNormalized() => new(this.R * Rgba32.ByteToNormalized, this.G * Rgba32.ByteToNormalized, this.B * Rgba32.ByteToNormalized, this.A * Rgba32.ByteToNormalized);

  /// <summary>
  /// Creates an RGB color from a System.Drawing.Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var c = new Rgba32(color);
    return new Rgb(c.R, c.G, c.B, c.A);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Rgb(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentColor
    => typeof(T) == typeof(Rgb)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Rgb)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}

/// <summary>
/// Represents a color in the RGB color space using normalized float values (0.0-1.0).
/// </summary>
[ColorSpace(3, ["R", "G", "B"], ColorSpaceType = ColorSpaceType.Additive)]
public record struct RgbNormalized(float R, float G, float B, float A = 1f) : IThreeComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  /// <summary>
  /// Converts this normalized RGB color to a System.Drawing.Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => Color.FromArgb(
    _FloatToByte(this.A),
    _FloatToByte(this.R),
    _FloatToByte(this.G),
    _FloatToByte(this.B)
  );

  /// <summary>
  /// Converts this normalized RGB color to byte values (0-255).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Rgb ToByte() => new(
    _FloatToByte(this.R),
    _FloatToByte(this.G),
    _FloatToByte(this.B),
    _FloatToByte(this.A)
  );

  /// <summary>
  /// Creates a normalized RGB color from a System.Drawing.Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var c = new Rgba32(color);
    return new RgbNormalized(c.RNormalized, c.GNormalized, c.BNormalized, c.ANormalized);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new RgbNormalized(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(RgbNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(RgbNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _FloatToByte(float value) => (byte)Math.Round(Math.Max(0f, Math.Min(1f, value)) * 255f);

}
