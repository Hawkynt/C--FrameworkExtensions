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

/// <summary>CIE LCh (cylindrical Lab) color space with byte components</summary>
public record struct Lch(byte L, byte C, byte H, byte A = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToNormalized().ToColor();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LchNormalized ToNormalized() => new(
    this.L * (100f * Rgba32.ByteToNormalized),  // L: 0-255 -> 0-100
    this.C * (128f * Rgba32.ByteToNormalized),  // C: 0-255 -> 0-128 (typical max chroma)
    this.H * (360f * Rgba32.ByteToNormalized),  // H: 0-255 -> 0-360
    this.A * Rgba32.ByteToNormalized
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((LchNormalized)LchNormalized.FromColor(color)).ToByte();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Lch(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentColor
    => typeof(T) == typeof(Lch)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Lch)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}

/// <summary>CIE LCh (cylindrical Lab) color space with normalized components</summary>
public record struct LchNormalized(float L, float C, float H, float A = 1f) : IThreeComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Convert LCh to Lab
    // a = C * cos(h), b = C * sin(h)
    var hRad = this.H * (float)Math.PI / 180f;
    var aVal = this.C * (float)Math.Cos(hRad);
    var bVal = this.C * (float)Math.Sin(hRad);

    // Use Lab conversion
    var lab = new LabNormalized(this.L, aVal, bVal, this.A);
    return lab.ToColor();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Lch ToByte() => new(
    (byte)(Math.Min(Math.Max(this.L / 100f, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(Math.Min(Math.Max(this.C / 128f, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(Math.Min(Math.Max(this.H / 360f, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(this.A * Rgba32.NormalizedToByte + 0.5f)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    // Convert to Lab first
    var lab = (LabNormalized)LabNormalized.FromColor(color);

    // Convert Lab to LCh
    // C = sqrt(a^2 + b^2)
    var cVal = (float)Math.Sqrt(lab.A * lab.A + lab.B * lab.B);

    // h = atan2(b, a) in degrees
    var hVal = (float)(Math.Atan2(lab.B, lab.A) * 180.0 / Math.PI);

    // Normalize hue to 0-360 range
    if (hVal < 0f)
      hVal += 360f;

    return new LchNormalized(lab.L, cVal, hVal, lab.Alpha);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new LchNormalized(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(LchNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(LchNormalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}
