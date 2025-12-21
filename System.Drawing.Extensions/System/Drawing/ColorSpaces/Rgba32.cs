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
using System.Runtime.InteropServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Structure for fast color component access.
/// Uses explicit field layout to allow direct byte access without unsafe code.
/// </summary>
/// <remarks>
/// Memory layout (little-endian): [B, G, R, A] matching Color.ToArgb() bit layout.
/// </remarks>
[StructLayout(LayoutKind.Explicit, Size = 4)]
public readonly struct Rgba32 {

  /// <summary>
  /// Reciprocal of 255 for fast byte-to-normalized-float conversion.
  /// Use multiplication instead of division: <c>value * ByteToNormalized</c> instead of <c>value / 255f</c>.
  /// </summary>
  public const float ByteToNormalized = 1f / 255f;

  /// <summary>
  /// Multiplier for normalized-float-to-byte conversion.
  /// Use: <c>(byte)(value * NormalizedToByte)</c> instead of <c>(byte)(value * 255f)</c>.
  /// </summary>
  public const float NormalizedToByte = 255f;

  [FieldOffset(0)] private readonly uint _packed;
  [FieldOffset(0)] public readonly byte B;
  [FieldOffset(1)] public readonly byte G;
  [FieldOffset(2)] public readonly byte R;
  [FieldOffset(3)] public readonly byte A;

  /// <summary>Packed ARGB value.</summary>
  public uint Packed {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._packed;
  }

  /// <summary>
  /// Constructs an Rgba32 from a System.Drawing.Color.
  /// </summary>
  /// <param name="color">The color to convert.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Rgba32(Color color) : this((uint)color.ToArgb()) { }

  /// <summary>
  /// Constructs an Rgba32 from a packed ARGB value.
  /// </summary>
  /// <param name="packed">The packed ARGB value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Rgba32(uint packed) {
    this.B = this.G = this.R = this.A = 0; // Required to initialize all fields before setting _packed
    this._packed = packed;
  }

  /// <summary>
  /// Constructs an Rgba32 from individual byte components.
  /// </summary>
  /// <param name="r">Red component (0-255).</param>
  /// <param name="g">Green component (0-255).</param>
  /// <param name="b">Blue component (0-255).</param>
  /// <param name="a">Alpha component (0-255).</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Rgba32(byte r, byte g, byte b, byte a = 255) {
    this._packed = 0; // Required to initialize all fields before setting individual bytes
    this.B = b;
    this.G = g;
    this.R = r;
    this.A = a;
  }

  /// <summary>
  /// Converts this Rgba32 back to a System.Drawing.Color.
  /// </summary>
  /// <returns>The equivalent Color.</returns>
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

  #region Interpolation Methods

  /// <summary>
  /// Linearly interpolates between two colors (50/50 blend).
  /// </summary>
  /// <param name="c1">First color.</param>
  /// <param name="c2">Second color.</param>
  /// <returns>The interpolated color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rgba32 Lerp(Rgba32 c1, Rgba32 c2) => new(
    (byte)((c1.R + c2.R) >> 1),
    (byte)((c1.G + c2.G) >> 1),
    (byte)((c1.B + c2.B) >> 1),
    (byte)((c1.A + c2.A) >> 1)
  );

  /// <summary>
  /// Linearly interpolates between two colors with weights.
  /// </summary>
  /// <param name="c1">First color.</param>
  /// <param name="c2">Second color.</param>
  /// <param name="w1">Weight for first color.</param>
  /// <param name="w2">Weight for second color.</param>
  /// <returns>The interpolated color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rgba32 Lerp(Rgba32 c1, Rgba32 c2, int w1, int w2) {
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
  /// <param name="c1">First color.</param>
  /// <param name="c2">Second color.</param>
  /// <param name="c3">Third color.</param>
  /// <returns>The interpolated color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rgba32 Lerp(Rgba32 c1, Rgba32 c2, Rgba32 c3) => new(
    (byte)((c1.R + c2.R + c3.R) / 3),
    (byte)((c1.G + c2.G + c3.G) / 3),
    (byte)((c1.B + c2.B + c3.B) / 3),
    (byte)((c1.A + c2.A + c3.A) / 3)
  );

  /// <summary>
  /// Linearly interpolates between three colors with weights.
  /// </summary>
  /// <param name="c1">First color.</param>
  /// <param name="c2">Second color.</param>
  /// <param name="c3">Third color.</param>
  /// <param name="w1">Weight for first color.</param>
  /// <param name="w2">Weight for second color.</param>
  /// <param name="w3">Weight for third color.</param>
  /// <returns>The interpolated color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rgba32 Lerp(Rgba32 c1, Rgba32 c2, Rgba32 c3, int w1, int w2, int w3) {
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
  /// <param name="c1">First color.</param>
  /// <param name="c2">Second color.</param>
  /// <param name="c3">Third color.</param>
  /// <param name="c4">Fourth color.</param>
  /// <returns>The interpolated color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rgba32 Lerp(Rgba32 c1, Rgba32 c2, Rgba32 c3, Rgba32 c4) => new(
    (byte)((c1.R + c2.R + c3.R + c4.R) >> 2),
    (byte)((c1.G + c2.G + c3.G + c4.G) >> 2),
    (byte)((c1.B + c2.B + c3.B + c4.B) >> 2),
    (byte)((c1.A + c2.A + c3.A + c4.A) >> 2)
  );

  /// <summary>
  /// Linearly interpolates between four colors with weights.
  /// </summary>
  /// <param name="c1">First color.</param>
  /// <param name="c2">Second color.</param>
  /// <param name="c3">Third color.</param>
  /// <param name="c4">Fourth color.</param>
  /// <param name="w1">Weight for first color.</param>
  /// <param name="w2">Weight for second color.</param>
  /// <param name="w3">Weight for third color.</param>
  /// <param name="w4">Weight for fourth color.</param>
  /// <returns>The interpolated color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rgba32 Lerp(Rgba32 c1, Rgba32 c2, Rgba32 c3, Rgba32 c4, int w1, int w2, int w3, int w4) {
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
  /// <param name="c1">First color (factor = 0).</param>
  /// <param name="c2">Second color (factor = 1).</param>
  /// <param name="factor">Interpolation factor (0.0 to 1.0).</param>
  /// <returns>The interpolated color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rgba32 Lerp(Rgba32 c1, Rgba32 c2, float factor) {
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
  /// <param name="c00">Top-left color.</param>
  /// <param name="c10">Top-right color.</param>
  /// <param name="c01">Bottom-left color.</param>
  /// <param name="c11">Bottom-right color.</param>
  /// <param name="fx">Horizontal interpolation factor (0.0 to 1.0).</param>
  /// <param name="fy">Vertical interpolation factor (0.0 to 1.0).</param>
  /// <returns>The interpolated color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rgba32 Blerp(Rgba32 c00, Rgba32 c10, Rgba32 c01, Rgba32 c11, float fx, float fy) {
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
}
