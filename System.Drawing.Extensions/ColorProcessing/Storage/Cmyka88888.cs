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
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Storage;

/// <summary>
/// 40-bit CMYKA pixel format with 8 bits per channel.
/// </summary>
/// <remarks>
/// Memory layout: [C][M][Y][K][A] - 5 bytes for CMYK + Alpha.
/// Used for print-oriented color workflows.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 5)]
public readonly struct Cmyka88888 : IColorSpace5B<Cmyka88888>, IStorageSpace, IEquatable<Cmyka88888> {

  /// <summary>Reciprocal of 255 for fast byte-to-normalized-float conversion.</summary>
  public const float ByteToNormalized = ColorConstants.ByteToFloat;

  public readonly byte C;
  public readonly byte M;
  public readonly byte Y;
  public readonly byte K;
  public readonly byte A;

  #region IColorSpace5B Implementation

  byte IColorSpace5B<Cmyka88888>.C1 => this.C;
  byte IColorSpace5B<Cmyka88888>.C2 => this.M;
  byte IColorSpace5B<Cmyka88888>.C3 => this.Y;
  byte IColorSpace5B<Cmyka88888>.C4 => this.K;
  byte IColorSpace5B<Cmyka88888>.A => this.A;
  public static Cmyka88888 Create(byte c1, byte c2, byte c3, byte c4, byte a) => new(c1, c2, c3, c4, a);

  #endregion

  #region Normalized Conversion

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3, UNorm32 C4, UNorm32 A) ToNormalized() => (
    UNorm32.FromByte(this.C),
    UNorm32.FromByte(this.M),
    UNorm32.FromByte(this.Y),
    UNorm32.FromByte(this.K),
    UNorm32.FromByte(this.A)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Cmyka88888 FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 c4, UNorm32 a) => new(
    c1.ToByte(),
    c2.ToByte(),
    c3.ToByte(),
    c4.ToByte(),
    a.ToByte()
  );

  #endregion

  /// <summary>Gets the cyan component normalized to 0.0-1.0 range.</summary>
  public float CNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.C * ByteToNormalized;
  }

  /// <summary>Gets the magenta component normalized to 0.0-1.0 range.</summary>
  public float MNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.M * ByteToNormalized;
  }

  /// <summary>Gets the yellow component normalized to 0.0-1.0 range.</summary>
  public float YNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Y * ByteToNormalized;
  }

  /// <summary>Gets the key (black) component normalized to 0.0-1.0 range.</summary>
  public float KNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.K * ByteToNormalized;
  }

  /// <summary>Gets the alpha component normalized to 0.0-1.0 range.</summary>
  public float ANormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.A * ByteToNormalized;
  }

  /// <summary>
  /// Constructs a Cmyka40 from individual byte components.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Cmyka88888(byte c, byte m, byte y, byte k, byte a = 255) {
    this.C = c;
    this.M = m;
    this.Y = y;
    this.K = k;
    this.A = a;
  }

  /// <summary>
  /// Linearly interpolates between two colors (50/50 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Cmyka88888 Lerp(Cmyka88888 c1, Cmyka88888 c2) => new(
    (byte)((c1.C + c2.C) >> 1),
    (byte)((c1.M + c2.M) >> 1),
    (byte)((c1.Y + c2.Y) >> 1),
    (byte)((c1.K + c2.K) >> 1),
    (byte)((c1.A + c2.A) >> 1)
  );

  /// <summary>
  /// Linearly interpolates between two colors using a normalized factor (0.0-1.0).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Cmyka88888 Lerp(Cmyka88888 c1, Cmyka88888 c2, float factor) {
    var invFactor = 1f - factor;
    return new(
      (byte)(c1.C * invFactor + c2.C * factor + 0.5f),
      (byte)(c1.M * invFactor + c2.M * factor + 0.5f),
      (byte)(c1.Y * invFactor + c2.Y * factor + 0.5f),
      (byte)(c1.K * invFactor + c2.K * factor + 0.5f),
      (byte)(c1.A * invFactor + c2.A * factor + 0.5f)
    );
  }

  #region IEquatable<Cmyka88888> Implementation

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Cmyka88888 other)
    => this.C == other.C && this.M == other.M && this.Y == other.Y && this.K == other.K && this.A == other.A;

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is Cmyka88888 other && this.Equals(other);

  /// <inheritdoc />
  public override int GetHashCode() {
    unchecked {
      var hash = 17;
      hash = hash * 31 + this.C;
      hash = hash * 31 + this.M;
      hash = hash * 31 + this.Y;
      hash = hash * 31 + this.K;
      hash = hash * 31 + this.A;
      return hash;
    }
  }

  public static bool operator ==(Cmyka88888 left, Cmyka88888 right) => left.Equals(right);
  public static bool operator !=(Cmyka88888 left, Cmyka88888 right) => !left.Equals(right);

  #endregion
}
