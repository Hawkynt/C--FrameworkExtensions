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
using System.Runtime.Intrinsics;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Storage;

/// <summary>
/// 16-bit ARGB pixel format (1-5-5-5).
/// </summary>
/// <remarks>
/// Bit layout: [A:1][R:5][G:5][B:5] where A is 0 (transparent) or 1 (opaque).
/// </remarks>
[StructLayout(LayoutKind.Sequential, Size = 2)]
public readonly struct Argb1555 : IColorSpace4B<Argb1555>, IStorageSpace, IEquatable<Argb1555> {

  private readonly ushort _packed;

  private const int AShift = 15;
  private const int RShift = 10;
  private const int GShift = 5;
  private const int BShift = 0;
  private const int Mask5 = 0x1F;
  private const int MaskA = 0x01;

  #region IColorSpace4B Implementation

  byte IColorSpace4B<Argb1555>.C1 => this.R;
  byte IColorSpace4B<Argb1555>.C2 => this.G;
  byte IColorSpace4B<Argb1555>.C3 => this.B;
  byte IColorSpace4B<Argb1555>.A => this.A;
  public static Argb1555 Create(byte c1, byte c2, byte c3, byte a) => new(c1, c2, c3, a);

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
  public static Argb1555 FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a) => new(
    c1.ToByte(),
    c2.ToByte(),
    c3.ToByte(),
    a.ToByte()
  );

  #endregion

  /// <summary>Gets the packed 16-bit value.</summary>
  public ushort Packed {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._packed;
  }

  /// <summary>Gets the alpha component (0 or 255).</summary>
  public byte A {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => ((this._packed >> AShift) & MaskA) != 0 ? (byte)255 : (byte)0;
  }

  /// <summary>Gets the red component (0-31 scaled to 0-255).</summary>
  public byte R {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var r5 = (this._packed >> RShift) & Mask5;
      return (byte)((r5 << 3) | (r5 >> 2));
    }
  }

  /// <summary>Gets the green component (0-31 scaled to 0-255).</summary>
  public byte G {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var g5 = (this._packed >> GShift) & Mask5;
      return (byte)((g5 << 3) | (g5 >> 2));
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
  /// Constructs an Argb1555 from a packed 16-bit value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Argb1555(ushort packed) => this._packed = packed;

  /// <summary>
  /// Constructs an Argb1555 from 8-bit components (truncated to 5 bits each, alpha to 1 bit).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Argb1555(byte r, byte g, byte b, byte a = 255) => this._packed = (ushort)(
    ((a >= 128 ? 1 : 0) << AShift) |
    ((r >> 3) << RShift) |
    ((g >> 3) << GShift) |
    ((b >> 3) << BShift)
  );

  /// <summary>
  /// Linearly interpolates between two colors (50/50 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Argb1555 Lerp(Argb1555 c1, Argb1555 c2) => new(
    (byte)((c1.R + c2.R) >> 1),
    (byte)((c1.G + c2.G) >> 1),
    (byte)((c1.B + c2.B) >> 1),
    (byte)((c1.A + c2.A) >> 1)
  );

  #region IEquatable<Argb1555> Implementation

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Argb1555 other) => this._packed == other._packed;

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is Argb1555 other && this.Equals(other);

  /// <inheritdoc />
  public override int GetHashCode() => this._packed;

  public static bool operator ==(Argb1555 left, Argb1555 right) => left.Equals(right);
  public static bool operator !=(Argb1555 left, Argb1555 right) => !left.Equals(right);

  #endregion
}
