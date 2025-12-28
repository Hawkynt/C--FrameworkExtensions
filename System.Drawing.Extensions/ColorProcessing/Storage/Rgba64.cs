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
using Hawkynt.ColorProcessing.Constants;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Storage;

/// <summary>
/// 64-bit RGBA pixel format with 16 bits per channel.
/// </summary>
/// <remarks>
/// Memory layout: [R:16][G:16][B:16][A:16] in native endianness.
/// Used for high dynamic range and professional imaging with alpha.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 2, Size = 8)]
public readonly struct Rgba64 : IColorSpace4B<Rgba64>, IStorageSpace, IEquatable<Rgba64> {

  /// <summary>Reciprocal of 65535 for fast ushort-to-normalized-float conversion.</summary>
  public const float UShortToNormalized = ColorConstants.UShortToFloat;

  public readonly ushort RValue;
  public readonly ushort GValue;
  public readonly ushort BValue;
  public readonly ushort AValue;

  #region IColorSpace4B Implementation

  byte IColorSpace4B<Rgba64>.C1 => this.R;
  byte IColorSpace4B<Rgba64>.C2 => this.G;
  byte IColorSpace4B<Rgba64>.C3 => this.B;
  byte IColorSpace4B<Rgba64>.A => this.A;
  public static Rgba64 Create(byte c1, byte c2, byte c3, byte a) => new(c1, c2, c3, a);

  #endregion

  /// <summary>Gets the red component scaled to 0-255.</summary>
  public byte R {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (byte)(this.RValue >> 8);
  }

  /// <summary>Gets the green component scaled to 0-255.</summary>
  public byte G {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (byte)(this.GValue >> 8);
  }

  /// <summary>Gets the blue component scaled to 0-255.</summary>
  public byte B {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (byte)(this.BValue >> 8);
  }

  /// <summary>Gets the alpha component scaled to 0-255.</summary>
  public byte A {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (byte)(this.AValue >> 8);
  }

  /// <summary>Gets the red component normalized to 0.0-1.0 range.</summary>
  public float RNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.RValue * UShortToNormalized;
  }

  /// <summary>Gets the green component normalized to 0.0-1.0 range.</summary>
  public float GNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.GValue * UShortToNormalized;
  }

  /// <summary>Gets the blue component normalized to 0.0-1.0 range.</summary>
  public float BNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.BValue * UShortToNormalized;
  }

  /// <summary>Gets the alpha component normalized to 0.0-1.0 range.</summary>
  public float ANormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.AValue * UShortToNormalized;
  }

  /// <summary>
  /// Constructs an Rgba64 from 16-bit components.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Rgba64(ushort r, ushort g, ushort b, ushort a = ushort.MaxValue) {
    this.RValue = r;
    this.GValue = g;
    this.BValue = b;
    this.AValue = a;
  }

  /// <summary>
  /// Constructs an Rgba64 from 8-bit components (scaled to 16-bit).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Rgba64(byte r, byte g, byte b, byte a = 255) {
    this.RValue = (ushort)((r << 8) | r);
    this.GValue = (ushort)((g << 8) | g);
    this.BValue = (ushort)((b << 8) | b);
    this.AValue = (ushort)((a << 8) | a);
  }

  /// <summary>
  /// Linearly interpolates between two colors (50/50 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rgba64 Lerp(Rgba64 c1, Rgba64 c2) => new(
    (ushort)((c1.RValue + c2.RValue) >> 1),
    (ushort)((c1.GValue + c2.GValue) >> 1),
    (ushort)((c1.BValue + c2.BValue) >> 1),
    (ushort)((c1.AValue + c2.AValue) >> 1)
  );

  #region IEquatable<Rgba64> Implementation

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Rgba64 other)
    => this.RValue == other.RValue && this.GValue == other.GValue && this.BValue == other.BValue && this.AValue == other.AValue;

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is Rgba64 other && this.Equals(other);

  /// <inheritdoc />
  public override int GetHashCode() {
    unchecked {
      var hash = 17;
      hash = hash * 31 + this.RValue;
      hash = hash * 31 + this.GValue;
      hash = hash * 31 + this.BValue;
      hash = hash * 31 + this.AValue;
      return hash;
    }
  }

  public static bool operator ==(Rgba64 left, Rgba64 right) => left.Equals(right);
  public static bool operator !=(Rgba64 left, Rgba64 right) => !left.Equals(right);

  #endregion
}
