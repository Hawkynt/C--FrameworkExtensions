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
/// Represents a color in the DIN99 color space using byte values (0-255).
/// DIN99 is a German standard (DIN 6176) optimized for small color differences.
/// </summary>
/// <param name="L">Lightness component (0-255, maps to 0-105.509 in normalized form).</param>
/// <param name="A">a99 component (0-255, centered at 128).</param>
/// <param name="B">b99 component (0-255, centered at 128).</param>
/// <param name="Alpha">Alpha component (0-255). Defaults to 255 (fully opaque).</param>
/// <remarks>
/// <para>
/// DIN99 transforms Lab coordinates into a more perceptually uniform space.
/// It was designed to be computationally simpler than CIEDE2000 while providing
/// good perceptual uniformity for small color differences.
/// </para>
/// </remarks>
[ColorSpace(3, ["L", "a", "b"], ColorSpaceType = ColorSpaceType.Perceptual, IsPerceptuallyUniform = true)]
public record struct Din99(byte L, byte A, byte B, byte Alpha = 255) : IThreeComponentColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => this.ToNormalized().ToColor();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Din99Normalized ToNormalized() => new(
    this.L * (Din99Normalized.MaxL99 * Rgba32.ByteToNormalized),  // L: 0-255 -> 0-105.509
    (this.A - 128f) * Din99Normalized.ComponentScale,             // a: 0-255 -> approx -40 to 40
    (this.B - 128f) * Din99Normalized.ComponentScale,             // b: 0-255 -> approx -40 to 40
    this.Alpha * Rgba32.ByteToNormalized
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) => ((Din99Normalized)Din99Normalized.FromColor(color)).ToByte();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Din99(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentColor
    => typeof(T) == typeof(Din99)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Din99)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}

/// <summary>
/// Represents a color in the DIN99 color space using normalized float values.
/// </summary>
/// <param name="L">Lightness component (0 to ~105.509).</param>
/// <param name="A">a99 component (approximately -40 to +40).</param>
/// <param name="B">b99 component (approximately -40 to +40).</param>
/// <param name="Alpha">Alpha component (0.0-1.0). Defaults to 1.0 (fully opaque).</param>
[ColorSpace(3, ["L", "a", "b"], ColorSpaceType = ColorSpaceType.Perceptual, IsPerceptuallyUniform = true)]
public record struct Din99Normalized(float L, float A, float B, float Alpha = 1f) : IThreeComponentFloatColor {

  /// <inheritdoc />
  public bool Equals(Color other) => this.ToColor().Equals(other);

  // DIN99 transformation constants
  private const double Cos16 = 0.9612616959383189;  // cos(16°)
  private const double Sin16 = 0.27563735581699916; // sin(16°)
  private const double KE = 1.0;   // Reference white adjustment
  private const double KCH = 1.0;  // Chroma/hue adjustment

  /// <summary>Maximum L99 value (when Lab L = 100)</summary>
  internal const float MaxL99 = 105.509f;

  /// <summary>Scale factor for a99/b99 byte conversion (128 maps to 0, range ~80 total)</summary>
  internal const float ComponentScale = 40f / 128f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() {
    // Convert DIN99 back to Lab
    var lab = this.ToLab();
    return lab.ToColor();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LabNormalized ToLab() {
    // Inverse DIN99 transformation
    // L99 = 105.509 * ln(1 + 0.0158 * L * kE)
    // L = (exp(L99 / 105.509) - 1) / (0.0158 * kE)
    var l = (Math.Exp(this.L / 105.509) - 1.0) / (0.0158 * KE);

    // c99 and h99 from a99, b99
    var c99 = Math.Sqrt(this.A * this.A + this.B * this.B);
    var h99 = Math.Atan2(this.B, this.A);

    // Inverse chroma: c99 = ln(1 + 0.045 * g * kCH * kE) / 0.045
    // g = (exp(c99 * 0.045) - 1) / (0.045 * kCH * kE)
    var g = c99 > 0 ? (Math.Exp(c99 * 0.045) - 1.0) / (0.045 * KCH * KE) : 0;

    // e, f from g and h99
    var e = g * Math.Cos(h99);
    var f = g * Math.Sin(h99);

    // Inverse rotation: e = a*cos16 + b*sin16, f = 0.7*(-a*sin16 + b*cos16)
    // Solving: a = e*cos16 - (f/0.7)*sin16, b = e*sin16 + (f/0.7)*cos16
    var fScaled = f / 0.7;
    var a = e * Cos16 - fScaled * Sin16;
    var b = e * Sin16 + fScaled * Cos16;

    return new((float)l, (float)a, (float)b, this.Alpha);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Din99 ToByte() => new(
    (byte)(Math.Min(Math.Max(this.L / MaxL99, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(Math.Min(Math.Max((this.A / ComponentScale + 128f) * Rgba32.ByteToNormalized, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(Math.Min(Math.Max((this.B / ComponentScale + 128f) * Rgba32.ByteToNormalized, 0f), 1f) * Rgba32.NormalizedToByte + 0.5f),
    (byte)(this.Alpha * Rgba32.NormalizedToByte + 0.5f)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IColorSpace FromColor(Color color) {
    var lab = (LabNormalized)LabNormalized.FromColor(color);
    return FromLab(lab);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Din99Normalized FromLab(LabNormalized lab) {
    double l = lab.L, a = lab.A, b = lab.B;

    // L99 = 105.509 * ln(1 + 0.0158 * L * kE)
    var l99 = 105.509 * Math.Log(1.0 + 0.0158 * l * KE);

    // Rotate a,b by 16 degrees
    var e = a * Cos16 + b * Sin16;
    var f = 0.7 * (-a * Sin16 + b * Cos16);

    // Chroma
    var g = Math.Sqrt(e * e + f * f);
    var c99 = g > 0 ? Math.Log(1.0 + 0.045 * g * KCH * KE) / 0.045 : 0;

    // Hue angle
    var h99 = Math.Atan2(f, e);
    var a99 = c99 * Math.Cos(h99);
    var b99 = c99 * Math.Sin(h99);

    return new((float)l99, (float)a99, (float)b99, lab.Alpha);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IThreeComponentFloatColor Create(float c1, float c2, float c3, float a) => new Din99Normalized(c1, c2, c3, a);

  public T ConvertTo<T>() where T : struct, IThreeComponentFloatColor
    => typeof(T) == typeof(Din99Normalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

  public T ToColor<T>() where T : struct, IColorSpace
    => typeof(T) == typeof(Din99Normalized)
      ? (T)(object)this
      : ColorSpaceFactory<T>.FromColor(this.ToColor());

}
