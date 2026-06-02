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

namespace System.Collections;

/// <summary>
/// Strategy describing how the bits of a code are laid out within the packed byte stream.
/// Implemented by zero-size value types (<see cref="LsbFirst"/>, <see cref="MsbFirst"/>) so the JIT can
/// inline the read/write logic when used as the <c>TBitOrder</c> type argument of
/// <see cref="PackedBitBuffer{TBitOrder}"/>, eliminating any per-access order branch.
/// </summary>
public interface IBitOrder {
  /// <summary>
  /// Reads <paramref name="bits"/> bits starting at <paramref name="bitOffset"/> and returns them as a code.
  /// </summary>
  ulong Read(byte[] data, long bitOffset, int bits);

  /// <summary>
  /// Writes the low <paramref name="bits"/> bits of <paramref name="code"/> starting at <paramref name="bitOffset"/>.
  /// </summary>
  void Write(byte[] data, long bitOffset, int bits, ulong code);
}

/// <summary>
/// Least-significant-bit-first layout: the low bit of a code occupies the lowest stream bit position;
/// within each byte, bit 0 is the least-significant bit. Matches the other packing helpers in this library.
/// </summary>
public readonly struct LsbFirst : IBitOrder {
  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ulong Read(byte[] data, long bitOffset, int bits) {
    var by = (int)(bitOffset >> 3);
    var bit = (int)(bitOffset & 7);
    ulong result = 0;
    var produced = 0;
    while (produced < bits) {
      var take = Math.Min(8 - bit, bits - produced);
      var chunk = (ulong)((data[by] >> bit) & ((1 << take) - 1));
      result |= chunk << produced;
      produced += take;
      bit = 0;
      ++by;
    }
    return result;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Write(byte[] data, long bitOffset, int bits, ulong code) {
    var by = (int)(bitOffset >> 3);
    var bit = (int)(bitOffset & 7);
    var done = 0;
    while (done < bits) {
      var put = Math.Min(8 - bit, bits - done);
      var fieldMask = (1 << put) - 1;
      var clear = fieldMask << bit;
      var value = (int)((code >> done) & (ulong)fieldMask) << bit;
      data[by] = (byte)((data[by] & ~clear) | value);
      done += put;
      bit = 0;
      ++by;
    }
  }
}

/// <summary>
/// Most-significant-bit-first layout: the high bit of a code occupies the lowest stream bit position;
/// within each byte, bit 0 is the most-significant bit (0x80). Useful for externally-defined wire formats.
/// </summary>
public readonly struct MsbFirst : IBitOrder {
  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ulong Read(byte[] data, long bitOffset, int bits) {
    var by = (int)(bitOffset >> 3);
    var bit = (int)(bitOffset & 7);
    ulong result = 0;
    var produced = 0;
    while (produced < bits) {
      var avail = 8 - bit;
      var take = Math.Min(avail, bits - produced);
      var shift = avail - take;
      var chunk = (ulong)((data[by] >> shift) & ((1 << take) - 1));
      result = (result << take) | chunk;
      produced += take;
      bit = 0;
      ++by;
    }
    return result;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Write(byte[] data, long bitOffset, int bits, ulong code) {
    var by = (int)(bitOffset >> 3);
    var bit = (int)(bitOffset & 7);
    var done = 0;
    while (done < bits) {
      var avail = 8 - bit;
      var put = Math.Min(avail, bits - done);
      var shift = avail - put;
      var fieldMask = (1 << put) - 1;
      var clear = fieldMask << shift;
      // most-significant `put` bits of the not-yet-written portion
      var value = (int)((code >> (bits - done - put)) & (ulong)fieldMask) << shift;
      data[by] = (byte)((data[by] & ~clear) | value);
      done += put;
      bit = 0;
      ++by;
    }
  }
}

/// <summary>
/// Order-independent helpers for packed bit buffers.
/// </summary>
public static class PackedBitBuffer {
  /// <summary>
  /// Gets the number of bytes required to store <paramref name="count"/> codes of
  /// <paramref name="bitsPerElement"/> bits each.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GetPackedByteCount(int count, int bitsPerElement) {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    ArgumentOutOfRangeException.ThrowIfLessThan(bitsPerElement, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(bitsPerElement, 64);
    return (int)(((long)count * bitsPerElement + 7) >> 3);
  }
}

/// <summary>
/// Dense storage for <see cref="Count"/> unsigned codes of <see cref="BitsPerElement"/> bits each (1..64),
/// packed with no per-element padding. Codes may straddle byte boundaries. The bit layout is fixed by the
/// <typeparamref name="TBitOrder"/> type argument, so there is no per-access order branch.
/// </summary>
/// <typeparam name="TBitOrder">The bit layout strategy (<see cref="LsbFirst"/> or <see cref="MsbFirst"/>).</typeparam>
/// <remarks>
/// This type is intentionally meaning-agnostic: it stores and retrieves raw codes only. Pair it with an
/// <see cref="IBitCodec{T}"/> via <see cref="PackedBuffer{T,TBitOrder}"/> to obtain a typed view.
/// <para>
/// There is deliberately no <c>Span&lt;element&gt;</c>: the CLR requires every span/ref element to be at
/// least one byte and individually addressable, which a sub-byte code is not. Use the indexer for single
/// codes and <see cref="Unpack"/>/<see cref="Pack"/> for bulk transfer.
/// </para>
/// </remarks>
public sealed class PackedBitBuffer<TBitOrder> where TBitOrder : struct, IBitOrder {
  private readonly byte[] _data;

  /// <summary>
  /// Gets the number of codes stored.
  /// </summary>
  public int Count { get; }

  /// <summary>
  /// Gets the number of bits each code occupies (1..64).
  /// </summary>
  public int BitsPerElement { get; }

  /// <summary>
  /// Creates an all-zero buffer for <paramref name="count"/> codes.
  /// </summary>
  public PackedBitBuffer(int count, int bitsPerElement)
    : this(new byte[PackedBitBuffer.GetPackedByteCount(count, bitsPerElement)], count, bitsPerElement) { }

  private PackedBitBuffer(byte[] data, int count, int bitsPerElement) {
    this._data = data;
    this.Count = count;
    this.BitsPerElement = bitsPerElement;
  }

  /// <summary>
  /// Wraps an existing packed byte buffer without copying.
  /// </summary>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="packed"/> is too small.</exception>
  public static PackedBitBuffer<TBitOrder> FromPacked(byte[] packed, int count, int bitsPerElement) {
    ArgumentNullException.ThrowIfNull(packed);
    var required = PackedBitBuffer.GetPackedByteCount(count, bitsPerElement);
    ArgumentOutOfRangeException.ThrowIfLessThan(packed.Length, required, nameof(packed));
    return new(packed, count, bitsPerElement);
  }

  /// <summary>
  /// Gets a read-only view of the packed bytes for interop or serialization.
  /// </summary>
  public ReadOnlySpan<byte> PackedData => this._data;

  /// <summary>
  /// Gets the raw code at the given index. Only the low <see cref="BitsPerElement"/> bits are significant.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ulong GetBits(int index) {
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)this.Count, nameof(index));
    return default(TBitOrder).Read(this._data, (long)index * this.BitsPerElement, this.BitsPerElement);
  }

  /// <summary>
  /// Sets the raw code at the given index. Only the low <see cref="BitsPerElement"/> bits are stored.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetBits(int index, ulong code) {
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)this.Count, nameof(index));
    default(TBitOrder).Write(this._data, (long)index * this.BitsPerElement, this.BitsPerElement, code);
  }

  /// <summary>
  /// Copies all codes into <paramref name="destination"/>.
  /// </summary>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="destination"/> is too small.</exception>
  public void Unpack(Span<ulong> destination) {
    ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, this.Count, nameof(destination));
    for (var i = 0; i < this.Count; ++i)
      destination[i] = this.GetBits(i);
  }

  /// <summary>
  /// Stores codes from <paramref name="codes"/> (up to <see cref="Count"/>) into the buffer.
  /// </summary>
  public void Pack(ReadOnlySpan<ulong> codes) {
    var n = Math.Min(codes.Length, this.Count);
    for (var i = 0; i < n; ++i)
      this.SetBits(i, codes[i]);
  }
}
