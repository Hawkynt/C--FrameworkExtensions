#nullable enable

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
//

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Represents an 8-bit ITU-T G.711 µ-law (mu-law) companded audio sample. Decoding expands the logarithmic
/// code to a 16-bit linear PCM sample; encoding compresses a 16-bit sample back to µ-law.
/// </summary>
/// <remarks>
/// Layout: 1 sign bit, 3 chord (segment) bits, 4 step bits, stored inverted (one's complement) per the standard.
/// Decoding is convention-independent. Encoding defaults to the <see cref="SunG711"/> (Reese/Campbell) convention;
/// use <see cref="FromPcm16{TConvention}"/> with <see cref="ItuG711"/> for the ITU-T G.191 quantizer (they differ
/// near full scale). Round-trips are lossy (companding).
/// </remarks>
public readonly struct MuLaw : IEquatable<MuLaw>, IComparable<MuLaw>, IComparable {

  private const int Bias = 0x84;

  /// <summary>Gets the raw 8-bit µ-law code.</summary>
  public byte RawValue { get; }

  private MuLaw(byte raw) => this.RawValue = raw;

  /// <summary>Creates a µ-law value from its raw code.</summary>
  public static MuLaw FromRaw(byte raw) => new(raw);

  /// <summary>
  /// Decodes this µ-law code to a 16-bit linear PCM sample (identical for all conventions).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public short ToPcm16() {
    int u = (byte)~this.RawValue;
    var t = ((u & 0x0F) << 3) + Bias;
    t <<= (u & 0x70) >> 4;
    return (short)((u & 0x80) != 0 ? Bias - t : t - Bias);
  }

  /// <summary>
  /// Encodes a 16-bit linear PCM sample to the nearest µ-law code using the default <see cref="SunG711"/> convention.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static MuLaw FromPcm16(short pcm) => FromPcm16<SunG711>(pcm);

  /// <summary>
  /// Encodes a 16-bit linear PCM sample to the nearest µ-law code using the given companding convention.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static MuLaw FromPcm16<TConvention>(short pcm) where TConvention : struct, IG711Convention
    => new(default(TConvention).EncodeMuLaw(pcm));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(MuLaw other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is MuLaw other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public int CompareTo(MuLaw other) => this.ToPcm16().CompareTo(other.ToPcm16());

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not MuLaw other)
      throw new ArgumentException("Object must be of type MuLaw.", nameof(obj));
    return this.CompareTo(other);
  }

  public override string ToString() => $"µ-law 0x{this.RawValue:X2} (PCM {this.ToPcm16()})";

  public static bool operator ==(MuLaw left, MuLaw right) => left.Equals(right);
  public static bool operator !=(MuLaw left, MuLaw right) => !left.Equals(right);

  public static explicit operator MuLaw(short pcm) => FromPcm16(pcm);
  public static implicit operator short(MuLaw value) => value.ToPcm16();
}
