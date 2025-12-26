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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Storage;

/// <summary>
/// 16-bit RGB pixel format (5-6-5).
/// </summary>
/// <remarks>
/// Bit layout: [R:5][G:6][B:5] - green has extra bit for human eye sensitivity.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Size = 2)]
public readonly struct Rgb565 : IColorSpace3B<Rgb565>, IStorageSpace, IEquatable<Rgb565> {

  private readonly ushort _packed;

  private const int RShift = 11;
  private const int GShift = 5;
  private const int BShift = 0;
  private const int Mask5 = 0x1F;
  private const int Mask6 = 0x3F;

  #region IColorSpace3B Implementation

  byte IColorSpace3B<Rgb565>.C1 => this.R;
  byte IColorSpace3B<Rgb565>.C2 => this.G;
  byte IColorSpace3B<Rgb565>.C3 => this.B;
  public static Rgb565 Create(byte c1, byte c2, byte c3) => new(c1, c2, c3);

  #endregion

  /// <summary>Gets the packed 16-bit value.</summary>
  public ushort Packed {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._packed;
  }

  /// <summary>Gets the red component (0-31 scaled to 0-255).</summary>
  public byte R {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var r5 = (this._packed >> RShift) & Mask5;
      return (byte)((r5 << 3) | (r5 >> 2));
    }
  }

  /// <summary>Gets the green component (0-63 scaled to 0-255).</summary>
  public byte G {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var g6 = (this._packed >> GShift) & Mask6;
      return (byte)((g6 << 2) | (g6 >> 4));
    }
  }

  /// <summary>Gets the blue component (0-31 scaled to 0-255).</summary>
  public byte B {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var b5 = (this._packed >> BShift) & Mask5;
      return (byte)((b5 << 3) | (b5 >> 2));
    }
  }

  /// <summary>
  /// Constructs an Rgb16 from a packed 16-bit value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Rgb565(ushort packed) => this._packed = packed;

  /// <summary>
  /// Constructs an Rgb16 from 8-bit components.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Rgb565(byte r, byte g, byte b) => this._packed = (ushort)(
    ((r >> 3) << RShift) |
    ((g >> 2) << GShift) |
    ((b >> 3) << BShift)
  );

  /// <summary>
  /// Linearly interpolates between two colors (50/50 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rgb565 Lerp(Rgb565 c1, Rgb565 c2) => new(
    (byte)((c1.R + c2.R) >> 1),
    (byte)((c1.G + c2.G) >> 1),
    (byte)((c1.B + c2.B) >> 1)
  );

  #region IEquatable<Rgb565> Implementation

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Rgb565 other) => this._packed == other._packed;

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is Rgb565 other && this.Equals(other);

  /// <inheritdoc />
  public override int GetHashCode() => this._packed;

  public static bool operator ==(Rgb565 left, Rgb565 right) => left.Equals(right);
  public static bool operator !=(Rgb565 left, Rgb565 right) => !left.Equals(right);

  #endregion
}
