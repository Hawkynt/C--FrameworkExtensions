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

using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Storage;

/// <summary>
/// 32-bit RGBA pixel format with 8 bits per channel.
/// </summary>
/// <remarks>
/// Memory layout (little-endian): [B, G, R, A] matching ARGB packed format.
/// </remarks>
[StructLayout(LayoutKind.Explicit, Size = 4)]
public readonly struct Bgra8888 : IColorSpace4B<Bgra8888>, IStorageSpace, IEquatable<Bgra8888> {

  /// <summary>Reciprocal of 255 for fast byte-to-normalized-float conversion.</summary>
  public const float ByteToNormalized = ColorConstants.ByteToFloat;

  /// <summary>Multiplier for normalized-float-to-byte conversion.</summary>
  public const float NormalizedToByte = ColorConstants.FloatToByte;

  #region Common Colors

  /// <summary>Fully transparent color (0, 0, 0, 0).</summary>
  public static Bgra8888 Transparent => new(0, 0, 0, 0);

  /// <summary>Opaque black color (0, 0, 0, 255).</summary>
  public static Bgra8888 Black => new(0, 0, 0);

  /// <summary>Opaque white color (255, 255, 255, 255).</summary>
  public static Bgra8888 White => new(255, 255, 255);

  /// <summary>Creates an opaque color from RGB components.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgra8888 FromRgb(byte r, byte g, byte b) => new(r, g, b);

  #endregion

  [FieldOffset(0)] private readonly uint _packed;
  [FieldOffset(0)] public readonly byte B;
  [FieldOffset(1)] public readonly byte G;
  [FieldOffset(2)] public readonly byte R;
  [FieldOffset(3)] public readonly byte A;

  #region IColorSpace4B Implementation

  byte IColorSpace4B<Bgra8888>.C1 => this.R;
  byte IColorSpace4B<Bgra8888>.C2 => this.G;
  byte IColorSpace4B<Bgra8888>.C3 => this.B;
  byte IColorSpace4B<Bgra8888>.A => this.A;
  public static Bgra8888 Create(byte c1, byte c2, byte c3, byte a) => new(c1, c2, c3, a);

  #endregion

  #region Normalized Conversion

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3, UNorm32 A) ToNormalized() => (
    UNorm32.FromByte(this.R),
    UNorm32.FromByte(this.G),
    UNorm32.FromByte(this.B),
    UNorm32.FromByte(this.A)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgra8888 FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a) => new(
    c1.ToByte(),
    c2.ToByte(),
    c3.ToByte(),
    a.ToByte()
  );

  #endregion

  /// <summary>Packed ARGB value.</summary>
  public uint Packed {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._packed;
  }

  /// <summary>
  /// Constructs an Rgba32 from a packed ARGB value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Bgra8888(uint packed) {
    this.B = this.G = this.R = this.A = 0;
    this._packed = packed;
  }

  /// <summary>
  /// Constructs an Rgba32 from individual byte components.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Bgra8888(byte r, byte g, byte b, byte a = 255) {
    this._packed = 0;
    this.B = b;
    this.G = g;
    this.R = r;
    this.A = a;
  }

  /// <summary>
  /// Constructs an Rgba32 from a System.Drawing.Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Bgra8888(Color color) : this((uint)color.ToArgb()) { }

  /// <summary>
  /// Converts this color to a System.Drawing.Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color ToColor() => Color.FromArgb((int)this._packed);

  /// <summary>Gets the red component normalized to 0.0-1.0 range.</summary>
  public float RNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.R * ByteToNormalized;
  }

  /// <summary>Gets the green component normalized to 0.0-1.0 range.</summary>
  public float GNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.G * ByteToNormalized;
  }

  /// <summary>Gets the blue component normalized to 0.0-1.0 range.</summary>
  public float BNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.B * ByteToNormalized;
  }

  /// <summary>Gets the alpha component normalized to 0.0-1.0 range.</summary>
  public float ANormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.A * ByteToNormalized;
  }

  /// <summary>Gets luminance using BT.709 coefficients (0.2126R + 0.7152G + 0.0722B).</summary>
  public float Luminance {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 0.2126f * this.R + 0.7152f * this.G + 0.0722f * this.B;
  }

  #region Interpolation Methods

  /// <summary>
  /// Linearly interpolates between two colors (50/50 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgra8888 Lerp(Bgra8888 c1, Bgra8888 c2) => new(
    (byte)((c1.R + c2.R) >> 1),
    (byte)((c1.G + c2.G) >> 1),
    (byte)((c1.B + c2.B) >> 1),
    (byte)((c1.A + c2.A) >> 1)
  );

  /// <summary>
  /// Linearly interpolates between two colors with weights.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgra8888 Lerp(Bgra8888 c1, Bgra8888 c2, int w1, int w2) {
    var total = w1 + w2;
    return new(
      (byte)((c1.R * w1 + c2.R * w2) / total),
      (byte)((c1.G * w1 + c2.G * w2) / total),
      (byte)((c1.B * w1 + c2.B * w2) / total),
      (byte)((c1.A * w1 + c2.A * w2) / total)
    );
  }

  /// <summary>
  /// Averages three colors (33/33/33 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgra8888 Lerp(Bgra8888 c1, Bgra8888 c2, Bgra8888 c3) => new(
    (byte)((c1.R + c2.R + c3.R) / 3),
    (byte)((c1.G + c2.G + c3.G) / 3),
    (byte)((c1.B + c2.B + c3.B) / 3),
    (byte)((c1.A + c2.A + c3.A) / 3)
  );

  /// <summary>
  /// Linearly interpolates between three colors with weights.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgra8888 Lerp(Bgra8888 c1, Bgra8888 c2, Bgra8888 c3, int w1, int w2, int w3) {
    var total = w1 + w2 + w3;
    return new(
      (byte)((c1.R * w1 + c2.R * w2 + c3.R * w3) / total),
      (byte)((c1.G * w1 + c2.G * w2 + c3.G * w3) / total),
      (byte)((c1.B * w1 + c2.B * w2 + c3.B * w3) / total),
      (byte)((c1.A * w1 + c2.A * w2 + c3.A * w3) / total)
    );
  }

  /// <summary>
  /// Averages four colors (25/25/25/25 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgra8888 Lerp(Bgra8888 c1, Bgra8888 c2, Bgra8888 c3, Bgra8888 c4) => new(
    (byte)((c1.R + c2.R + c3.R + c4.R) >> 2),
    (byte)((c1.G + c2.G + c3.G + c4.G) >> 2),
    (byte)((c1.B + c2.B + c3.B + c4.B) >> 2),
    (byte)((c1.A + c2.A + c3.A + c4.A) >> 2)
  );

  /// <summary>
  /// Linearly interpolates between four colors with weights.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgra8888 Lerp(Bgra8888 c1, Bgra8888 c2, Bgra8888 c3, Bgra8888 c4, int w1, int w2, int w3, int w4) {
    var total = w1 + w2 + w3 + w4;
    return new(
      (byte)((c1.R * w1 + c2.R * w2 + c3.R * w3 + c4.R * w4) / total),
      (byte)((c1.G * w1 + c2.G * w2 + c3.G * w3 + c4.G * w4) / total),
      (byte)((c1.B * w1 + c2.B * w2 + c3.B * w3 + c4.B * w4) / total),
      (byte)((c1.A * w1 + c2.A * w2 + c3.A * w3 + c4.A * w4) / total)
    );
  }

  /// <summary>
  /// Linearly interpolates between two colors using a normalized factor (0.0-1.0).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgra8888 Lerp(Bgra8888 c1, Bgra8888 c2, float factor) {
    var invFactor = 1f - factor;
    return new(
      (byte)(c1.R * invFactor + c2.R * factor + 0.5f),
      (byte)(c1.G * invFactor + c2.G * factor + 0.5f),
      (byte)(c1.B * invFactor + c2.B * factor + 0.5f),
      (byte)(c1.A * invFactor + c2.A * factor + 0.5f)
    );
  }

  /// <summary>
  /// Bilinear interpolation between four colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgra8888 Blerp(Bgra8888 c00, Bgra8888 c10, Bgra8888 c01, Bgra8888 c11, float fx, float fy) {
    var fx1 = 1f - fx;
    var fy1 = 1f - fy;
    return new(
      (byte)(c00.R * fx1 * fy1 + c10.R * fx * fy1 + c01.R * fx1 * fy + c11.R * fx * fy + 0.5f),
      (byte)(c00.G * fx1 * fy1 + c10.G * fx * fy1 + c01.G * fx1 * fy + c11.G * fx * fy + 0.5f),
      (byte)(c00.B * fx1 * fy1 + c10.B * fx * fy1 + c01.B * fx1 * fy + c11.B * fx * fy + 0.5f),
      (byte)(c00.A * fx1 * fy1 + c10.A * fx * fy1 + c01.A * fx1 * fy + c11.A * fx * fy + 0.5f)
    );
  }

  #endregion

  #region IEquatable<Bgra8888> Implementation

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Bgra8888 other) => this._packed == other._packed;

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is Bgra8888 other && this.Equals(other);

  /// <inheritdoc />
  public override int GetHashCode() => (int)this._packed;

  public static bool operator ==(Bgra8888 left, Bgra8888 right) => left.Equals(right);
  public static bool operator !=(Bgra8888 left, Bgra8888 right) => !left.Equals(right);

  #endregion

  #region Clamping Utilities

  /// <summary>
  /// Clamps a floating-point value to byte range [0-255] with rounding.
  /// </summary>
  /// <param name="value">The value to clamp (typically 0-255 range).</param>
  /// <returns>The clamped byte value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ClampToByte(float value) => (byte)Math.Max(0, Math.Min(255, (int)(value + 0.5f)));

  /// <summary>
  /// Clamps an integer value to byte range [0-255].
  /// </summary>
  /// <param name="value">The value to clamp.</param>
  /// <returns>The clamped byte value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ClampToByte(int value) => value < 0 ? (byte)0 : value > 255 ? (byte)255 : (byte)value;

  #endregion
}
