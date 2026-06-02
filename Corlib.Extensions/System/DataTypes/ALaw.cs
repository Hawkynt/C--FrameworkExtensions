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
/// Represents an 8-bit ITU-T G.711 A-law companded audio sample. Decoding expands the logarithmic code to a
/// 16-bit linear PCM sample; encoding compresses a 16-bit sample back to A-law.
/// </summary>
/// <remarks>
/// Layout: 1 sign bit, 3 chord (segment) bits, 4 step bits, with the standard 0x55 even-bit inversion applied.
/// Decoding is convention-independent. Encoding defaults to the <see cref="SunG711"/> (Reese/Campbell) convention;
/// use <see cref="FromPcm16{TConvention}"/> with <see cref="ItuG711"/> for the ITU-T G.191 quantizer. Round-trips
/// are lossy (companding).
/// </remarks>
public readonly struct ALaw : IEquatable<ALaw>, IComparable<ALaw>, IComparable {

  private const int SignBit = 0x80;
  private const int QuantMask = 0x0F;
  private const int SegMask = 0x70;
  private const int SegShift = 4;

  /// <summary>Gets the raw 8-bit A-law code.</summary>
  public byte RawValue { get; }

  private ALaw(byte raw) => this.RawValue = raw;

  /// <summary>Creates an A-law value from its raw code.</summary>
  public static ALaw FromRaw(byte raw) => new(raw);

  /// <summary>
  /// Decodes this A-law code to a 16-bit linear PCM sample (identical for all conventions).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public short ToPcm16() {
    var a = this.RawValue ^ 0x55;
    var t = (a & QuantMask) << 4;
    var seg = (a & SegMask) >> SegShift;
    switch (seg) {
      case 0: t += 8; break;
      case 1: t += 0x108; break;
      default:
        t += 0x108;
        t <<= seg - 1;
        break;
    }

    return (short)((a & SignBit) != 0 ? t : -t);
  }

  /// <summary>
  /// Encodes a 16-bit linear PCM sample to the nearest A-law code using the default <see cref="SunG711"/> convention.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ALaw FromPcm16(short pcm) => FromPcm16<SunG711>(pcm);

  /// <summary>
  /// Encodes a 16-bit linear PCM sample to the nearest A-law code using the given companding convention.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ALaw FromPcm16<TConvention>(short pcm) where TConvention : struct, IG711Convention
    => new(default(TConvention).EncodeALaw(pcm));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(ALaw other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is ALaw other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public int CompareTo(ALaw other) => this.ToPcm16().CompareTo(other.ToPcm16());

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not ALaw other)
      throw new ArgumentException("Object must be of type ALaw.", nameof(obj));
    return this.CompareTo(other);
  }

  public override string ToString() => $"A-law 0x{this.RawValue:X2} (PCM {this.ToPcm16()})";

  public static bool operator ==(ALaw left, ALaw right) => left.Equals(right);
  public static bool operator !=(ALaw left, ALaw right) => !left.Equals(right);

  public static explicit operator ALaw(short pcm) => FromPcm16(pcm);
  public static implicit operator short(ALaw value) => value.ToPcm16();
}
