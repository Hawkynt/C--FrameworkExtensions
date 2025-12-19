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
/// Internal structure for fast color component access.
/// Uses explicit field layout to allow direct byte access without unsafe code.
/// </summary>
/// <remarks>
/// Memory layout (little-endian): [B, G, R, A] matching Color.ToArgb() bit layout.
/// </remarks>
[StructLayout(LayoutKind.Explicit, Size = 4)]
internal readonly struct Rgba32 {

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
}
