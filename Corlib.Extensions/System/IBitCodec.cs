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
/// Maps a fixed-width raw bit code to and from a value of type <typeparamref name="T"/>.
/// Used together with <see cref="System.Collections.PackedBitBuffer"/> /
/// <see cref="System.Collections.PackedBuffer{T}"/> to give meaning to densely packed sub-byte codes.
/// </summary>
/// <typeparam name="T">The logical value type the code represents.</typeparam>
/// <remarks>
/// Implement this as a <see langword="struct"/> to allow the JIT to inline
/// <see cref="Decode"/>/<see cref="Encode"/> when used as the <c>TCodec</c> type argument of
/// <see cref="System.Collections.PackedBuffer{T,TCodec}"/> (zero-cost abstraction on modern runtimes).
/// </remarks>
public interface IBitCodec<T> {
  /// <summary>
  /// Gets the number of bits each code occupies (1..64).
  /// </summary>
  int BitWidth { get; }

  /// <summary>
  /// Decodes a raw code (only the low <see cref="BitWidth"/> bits are significant) into a value.
  /// </summary>
  T Decode(ulong code);

  /// <summary>
  /// Encodes a value into a raw code. Only the low <see cref="BitWidth"/> bits are stored.
  /// </summary>
  ulong Encode(T value);
}

/// <summary>
/// Raw unsigned N-bit codec: the value is the code itself, truncated to <see cref="BitWidth"/> bits.
/// </summary>
public readonly struct UnsignedBitCodec : IBitCodec<ulong> {
  private readonly ulong _mask;

  /// <summary>
  /// Creates an unsigned codec for the given bit width.
  /// </summary>
  /// <param name="bitWidth">Number of bits per code (1..64).</param>
  public UnsignedBitCodec(int bitWidth) {
    ArgumentOutOfRangeException.ThrowIfLessThan(bitWidth, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(bitWidth, 64);
    this.BitWidth = bitWidth;
    this._mask = bitWidth == 64 ? ulong.MaxValue : (1UL << bitWidth) - 1;
  }

  /// <inheritdoc />
  public int BitWidth { get; }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ulong Decode(ulong code) => code & this._mask;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ulong Encode(ulong value) => value & this._mask;
}

/// <summary>
/// Signed N-bit two's-complement codec. Decoding sign-extends the top bit; encoding truncates.
/// </summary>
public readonly struct SignedBitCodec : IBitCodec<long> {
  private readonly int _bitWidth;
  private readonly ulong _mask;
  private readonly ulong _signBit;

  /// <summary>
  /// Creates a signed codec for the given bit width.
  /// </summary>
  /// <param name="bitWidth">Number of bits per code (1..64).</param>
  public SignedBitCodec(int bitWidth) {
    ArgumentOutOfRangeException.ThrowIfLessThan(bitWidth, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(bitWidth, 64);
    this._bitWidth = bitWidth;
    this._mask = bitWidth == 64 ? ulong.MaxValue : (1UL << bitWidth) - 1;
    this._signBit = 1UL << (bitWidth - 1);
  }

  /// <inheritdoc />
  public int BitWidth => this._bitWidth;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public long Decode(ulong code) {
    code &= this._mask;
    // sign-extend: if the sign bit is set, fill the high bits with ones
    return (code & this._signBit) != 0 ? (long)(code | ~this._mask) : (long)code;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ulong Encode(long value) => (ulong)value & this._mask;
}
