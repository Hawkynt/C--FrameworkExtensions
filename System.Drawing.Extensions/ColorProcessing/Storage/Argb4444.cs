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
/// 16-bit RGBA pixel format (4-4-4-4).
/// </summary>
/// <remarks>
/// Bit layout: [A:4][R:4][G:4][B:4].
/// </remarks>
[StructLayout(LayoutKind.Sequential, Size = 2)]
public readonly struct Argb4444 : IColorSpace4B<Argb4444>, IStorageSpace, IEquatable<Argb4444> {

  private readonly ushort _packed;

  private const int AShift = 12;
  private const int RShift = 8;
  private const int GShift = 4;
  private const int BShift = 0;
  private const int Mask4 = 0x0F;

  #region IColorSpace4B Implementation

  byte IColorSpace4B<Argb4444>.C1 => this.R;
  byte IColorSpace4B<Argb4444>.C2 => this.G;
  byte IColorSpace4B<Argb4444>.C3 => this.B;
  byte IColorSpace4B<Argb4444>.A => this.A;
  public static Argb4444 Create(byte c1, byte c2, byte c3, byte a) => new(c1, c2, c3, a);

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
  public static Argb4444 FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a) => new(
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

  /// <summary>Gets the red component (0-15 scaled to 0-255).</summary>
  public byte R {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var r4 = (this._packed >> RShift) & Mask4;
      return (byte)((r4 << 4) | r4);
    }
  }

  /// <summary>Gets the green component (0-15 scaled to 0-255).</summary>
  public byte G {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var g4 = (this._packed >> GShift) & Mask4;
      return (byte)((g4 << 4) | g4);
    }
  }

  /// <summary>Gets the blue component (0-15 scaled to 0-255).</summary>
  public byte B {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var b4 = (this._packed >> BShift) & Mask4;
      return (byte)((b4 << 4) | b4);
    }
  }

  /// <summary>Gets the alpha component (0-15 scaled to 0-255).</summary>
  public byte A {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var a4 = (this._packed >> AShift) & Mask4;
      return (byte)((a4 << 4) | a4);
    }
  }

  /// <summary>
  /// Constructs an Rgba16 from a packed 16-bit value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Argb4444(ushort packed) => this._packed = packed;

  /// <summary>
  /// Constructs an Rgba16 from 8-bit components (truncated to 4 bits each).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Argb4444(byte r, byte g, byte b, byte a = 255) => this._packed = (ushort)(
    ((a >> 4) << AShift) |
    ((r >> 4) << RShift) |
    ((g >> 4) << GShift) |
    ((b >> 4) << BShift)
  );

  /// <summary>
  /// Linearly interpolates between two colors (50/50 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Argb4444 Lerp(Argb4444 c1, Argb4444 c2) => new(
    (byte)((c1.R + c2.R) >> 1),
    (byte)((c1.G + c2.G) >> 1),
    (byte)((c1.B + c2.B) >> 1),
    (byte)((c1.A + c2.A) >> 1)
  );

  #region IEquatable<Argb4444> Implementation

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Argb4444 other) => this._packed == other._packed;

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is Argb4444 other && this.Equals(other);

  /// <inheritdoc />
  public override int GetHashCode() => this._packed;

  public static bool operator ==(Argb4444 left, Argb4444 right) => left.Equals(right);
  public static bool operator !=(Argb4444 left, Argb4444 right) => !left.Equals(right);

  #endregion
}
