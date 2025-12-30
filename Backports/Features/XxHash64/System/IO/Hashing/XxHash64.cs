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

#if !SUPPORTS_XXHASH64 && !OFFICIAL_XXHASH64

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO.Hashing;

/// <summary>
/// Provides an implementation of the XXH64 hash algorithm.
/// </summary>
/// <remarks>
/// This is a polyfill implementation for .NET versions prior to .NET 10.0.
/// XXH64 is an extremely fast non-cryptographic hash algorithm.
/// </remarks>
public sealed class XxHash64 {
  private const ulong _PRIME64_1 = 0x9E3779B185EBCA87UL;
  private const ulong _PRIME64_2 = 0xC2B2AE3D27D4EB4FUL;
  private const ulong _PRIME64_3 = 0x165667B19E3779F9UL;
  private const ulong _PRIME64_4 = 0x85EBCA77C2B2AE63UL;
  private const ulong _PRIME64_5 = 0x27D4EB2F165667C5UL;

  private readonly ulong _seed;
  private ulong _acc1;
  private ulong _acc2;
  private ulong _acc3;
  private ulong _acc4;
  private byte[] _buffer = new byte[32];
  private int _bufferLength;
  private ulong _totalLength;

  /// <summary>
  /// Initializes a new instance of the <see cref="XxHash64"/> class with seed 0.
  /// </summary>
  public XxHash64() : this(0) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="XxHash64"/> class with the specified seed.
  /// </summary>
  /// <param name="seed">The seed value for the hash computation.</param>
  public XxHash64(long seed) {
    this._seed = (ulong)seed;
    this.Reset();
  }

  /// <summary>
  /// Appends the contents of the source to the data already processed.
  /// </summary>
  /// <param name="source">The data to process.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Append(byte[] source) {
    ArgumentNullException.ThrowIfNull(source);
    this.Append(source.AsSpan());
  }

  /// <summary>
  /// Appends the contents of the source to the data already processed.
  /// </summary>
  /// <param name="source">The data to process.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Append(ReadOnlySpan<byte> source) {
    var offset = 0;
    var length = source.Length;
    this._totalLength += (ulong)length;

    // If we have buffered data, try to complete a 32-byte block
    if (this._bufferLength > 0) {
      var toCopy = Math.Min(32 - this._bufferLength, length);
      source.Slice(offset, toCopy).CopyTo(this._buffer.AsSpan(this._bufferLength));
      this._bufferLength += toCopy;
      offset += toCopy;
      length -= toCopy;

      if (this._bufferLength == 32) {
        this._ProcessStripe(this._buffer.AsSpan());
        this._bufferLength = 0;
      }
    }

    // Process full 32-byte blocks
    while (length >= 32) {
      this._ProcessStripe(source.Slice(offset, 32));
      offset += 32;
      length -= 32;
    }

    // Buffer remaining bytes
    if (length > 0) {
      source.Slice(offset, length).CopyTo(this._buffer.AsSpan());
      this._bufferLength = length;
    }
  }

  /// <summary>
  /// Resets the hash computation to its initial state.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Reset() {
    this._acc1 = this._seed + _PRIME64_1 + _PRIME64_2;
    this._acc2 = this._seed + _PRIME64_2;
    this._acc3 = this._seed;
    this._acc4 = this._seed - _PRIME64_1;
    this._bufferLength = 0;
    this._totalLength = 0;
  }

  /// <summary>
  /// Gets the current computed hash value as a byte array.
  /// </summary>
  /// <returns>The hash value as a byte array.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte[] GetCurrentHash() => BitConverter.GetBytes(this._FinalizeHash());

  /// <summary>
  /// Computes the XXH64 hash for the input data.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(byte[] source) => Hash(source, 0);

  /// <summary>
  /// Computes the XXH64 hash for the input data with the specified seed.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <param name="seed">The seed value.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(byte[] source, long seed) {
    ArgumentNullException.ThrowIfNull(source);
    return Hash(source.AsSpan(), seed);
  }

  /// <summary>
  /// Computes the XXH64 hash for the input data.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(ReadOnlySpan<byte> source) => Hash(source, 0);

  /// <summary>
  /// Computes the XXH64 hash for the input data with the specified seed.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <param name="seed">The seed value.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(ReadOnlySpan<byte> source, long seed)
    => BitConverter.GetBytes(_ComputeHash(source, seed));

  private static ulong _ComputeHash(ReadOnlySpan<byte> source, long seed) {
    var length = source.Length;
    var useed = (ulong)seed;
    ulong hash;

    if (length >= 32) {
      var acc1 = useed + _PRIME64_1 + _PRIME64_2;
      var acc2 = useed + _PRIME64_2;
      var acc3 = useed;
      var acc4 = useed - _PRIME64_1;

      var offset = 0;
      while (offset + 32 <= length) {
        acc1 = _Round(acc1, _ReadUInt64(source, offset));
        acc2 = _Round(acc2, _ReadUInt64(source, offset + 8));
        acc3 = _Round(acc3, _ReadUInt64(source, offset + 16));
        acc4 = _Round(acc4, _ReadUInt64(source, offset + 24));
        offset += 32;
      }

      hash = _RotateLeft(acc1, 1) + _RotateLeft(acc2, 7) + _RotateLeft(acc3, 12) + _RotateLeft(acc4, 18);
      hash = _MergeAccumulator(hash, acc1);
      hash = _MergeAccumulator(hash, acc2);
      hash = _MergeAccumulator(hash, acc3);
      hash = _MergeAccumulator(hash, acc4);

      source = source.Slice(offset);
    } else
      hash = useed + _PRIME64_5;

    hash += (ulong)length;

    // Process remaining bytes
    var remaining = source.Length;
    var pos = 0;

    while (pos + 8 <= remaining) {
      hash ^= _Round(0, _ReadUInt64(source, pos));
      hash = _RotateLeft(hash, 27) * _PRIME64_1 + _PRIME64_4;
      pos += 8;
    }

    while (pos + 4 <= remaining) {
      hash ^= _ReadUInt32(source, pos) * _PRIME64_1;
      hash = _RotateLeft(hash, 23) * _PRIME64_2 + _PRIME64_3;
      pos += 4;
    }

    while (pos < remaining) {
      hash ^= source[pos] * _PRIME64_5;
      hash = _RotateLeft(hash, 11) * _PRIME64_1;
      ++pos;
    }

    return _Avalanche(hash);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _ProcessStripe(ReadOnlySpan<byte> stripe) {
    this._acc1 = _Round(this._acc1, _ReadUInt64(stripe, 0));
    this._acc2 = _Round(this._acc2, _ReadUInt64(stripe, 8));
    this._acc3 = _Round(this._acc3, _ReadUInt64(stripe, 16));
    this._acc4 = _Round(this._acc4, _ReadUInt64(stripe, 24));
  }

  private ulong _FinalizeHash() {
    ulong hash;

    if (this._totalLength >= 32) {
      hash = _RotateLeft(this._acc1, 1) + _RotateLeft(this._acc2, 7) + _RotateLeft(this._acc3, 12) + _RotateLeft(this._acc4, 18);
      hash = _MergeAccumulator(hash, this._acc1);
      hash = _MergeAccumulator(hash, this._acc2);
      hash = _MergeAccumulator(hash, this._acc3);
      hash = _MergeAccumulator(hash, this._acc4);
    } else
      hash = this._seed + _PRIME64_5;

    hash += this._totalLength;

    // Process buffered data
    var offset = 0;
    var remaining = this._bufferLength;

    while (remaining >= 8) {
      hash ^= _Round(0, _ReadUInt64(this._buffer.AsSpan(), offset));
      hash = _RotateLeft(hash, 27) * _PRIME64_1 + _PRIME64_4;
      offset += 8;
      remaining -= 8;
    }

    while (remaining >= 4) {
      hash ^= _ReadUInt32(this._buffer.AsSpan(), offset) * _PRIME64_1;
      hash = _RotateLeft(hash, 23) * _PRIME64_2 + _PRIME64_3;
      offset += 4;
      remaining -= 4;
    }

    while (remaining > 0) {
      hash ^= this._buffer[offset] * _PRIME64_5;
      hash = _RotateLeft(hash, 11) * _PRIME64_1;
      ++offset;
      --remaining;
    }

    return _Avalanche(hash);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _Round(ulong acc, ulong input)
    => _RotateLeft(acc + input * _PRIME64_2, 31) * _PRIME64_1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _MergeAccumulator(ulong acc, ulong val) {
    val = _Round(0, val);
    acc ^= val;
    return acc * _PRIME64_1 + _PRIME64_4;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _Avalanche(ulong hash) {
    hash ^= hash >> 33;
    hash *= _PRIME64_2;
    hash ^= hash >> 29;
    hash *= _PRIME64_3;
    hash ^= hash >> 32;
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _RotateLeft(ulong value, int offset) => (value << offset) | (value >> (64 - offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _ReadUInt64(ReadOnlySpan<byte> source, int offset)
    => (ulong)source[offset]
       | ((ulong)source[offset + 1] << 8)
       | ((ulong)source[offset + 2] << 16)
       | ((ulong)source[offset + 3] << 24)
       | ((ulong)source[offset + 4] << 32)
       | ((ulong)source[offset + 5] << 40)
       | ((ulong)source[offset + 6] << 48)
       | ((ulong)source[offset + 7] << 56);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _ReadUInt32(ReadOnlySpan<byte> source, int offset)
    => (uint)source[offset]
       | ((uint)source[offset + 1] << 8)
       | ((uint)source[offset + 2] << 16)
       | ((uint)source[offset + 3] << 24);
}

#endif
