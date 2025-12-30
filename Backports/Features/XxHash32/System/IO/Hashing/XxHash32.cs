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

#if !SUPPORTS_XXHASH32 && !OFFICIAL_XXHASH32

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO.Hashing;

/// <summary>
/// Provides an implementation of the XXH32 hash algorithm.
/// </summary>
/// <remarks>
/// This is a polyfill implementation for .NET versions prior to .NET 10.0.
/// XXH32 is an extremely fast non-cryptographic hash algorithm.
/// </remarks>
public sealed class XxHash32 {
  private const uint _PRIME32_1 = 0x9E3779B1u;
  private const uint _PRIME32_2 = 0x85EBCA77u;
  private const uint _PRIME32_3 = 0xC2B2AE3Du;
  private const uint _PRIME32_4 = 0x27D4EB2Fu;
  private const uint _PRIME32_5 = 0x165667B1u;

  private readonly uint _seed;
  private uint _acc1;
  private uint _acc2;
  private uint _acc3;
  private uint _acc4;
  private byte[] _buffer = new byte[16];
  private int _bufferLength;
  private uint _totalLength;

  /// <summary>
  /// Initializes a new instance of the <see cref="XxHash32"/> class with seed 0.
  /// </summary>
  public XxHash32() : this(0) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="XxHash32"/> class with the specified seed.
  /// </summary>
  /// <param name="seed">The seed value for the hash computation.</param>
  public XxHash32(int seed) {
    this._seed = (uint)seed;
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
    this._totalLength += (uint)length;

    // If we have buffered data, try to complete a 16-byte block
    if (this._bufferLength > 0) {
      var toCopy = Math.Min(16 - this._bufferLength, length);
      source.Slice(offset, toCopy).CopyTo(this._buffer.AsSpan(this._bufferLength));
      this._bufferLength += toCopy;
      offset += toCopy;
      length -= toCopy;

      if (this._bufferLength == 16) {
        this._ProcessStripe(this._buffer.AsSpan());
        this._bufferLength = 0;
      }
    }

    // Process full 16-byte blocks
    while (length >= 16) {
      this._ProcessStripe(source.Slice(offset, 16));
      offset += 16;
      length -= 16;
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
    this._acc1 = this._seed + _PRIME32_1 + _PRIME32_2;
    this._acc2 = this._seed + _PRIME32_2;
    this._acc3 = this._seed;
    this._acc4 = this._seed - _PRIME32_1;
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
  /// Computes the XXH32 hash for the input data.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(byte[] source) => Hash(source, 0);

  /// <summary>
  /// Computes the XXH32 hash for the input data with the specified seed.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <param name="seed">The seed value.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(byte[] source, int seed) {
    ArgumentNullException.ThrowIfNull(source);
    return Hash(source.AsSpan(), seed);
  }

  /// <summary>
  /// Computes the XXH32 hash for the input data.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(ReadOnlySpan<byte> source) => Hash(source, 0);

  /// <summary>
  /// Computes the XXH32 hash for the input data with the specified seed.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <param name="seed">The seed value.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(ReadOnlySpan<byte> source, int seed)
    => BitConverter.GetBytes(_ComputeHash(source, seed));

  private static uint _ComputeHash(ReadOnlySpan<byte> source, int seed) {
    var length = source.Length;
    var useed = (uint)seed;
    uint hash;

    if (length >= 16) {
      var acc1 = useed + _PRIME32_1 + _PRIME32_2;
      var acc2 = useed + _PRIME32_2;
      var acc3 = useed;
      var acc4 = useed - _PRIME32_1;

      var offset = 0;
      while (offset + 16 <= length) {
        acc1 = _Round(acc1, _ReadUInt32(source, offset));
        acc2 = _Round(acc2, _ReadUInt32(source, offset + 4));
        acc3 = _Round(acc3, _ReadUInt32(source, offset + 8));
        acc4 = _Round(acc4, _ReadUInt32(source, offset + 12));
        offset += 16;
      }

      hash = _RotateLeft(acc1, 1) + _RotateLeft(acc2, 7) + _RotateLeft(acc3, 12) + _RotateLeft(acc4, 18);
      source = source.Slice(offset);
    } else
      hash = useed + _PRIME32_5;

    hash += (uint)length;

    // Process remaining bytes
    var remaining = source.Length;
    var pos = 0;

    while (pos + 4 <= remaining) {
      hash += _ReadUInt32(source, pos) * _PRIME32_3;
      hash = _RotateLeft(hash, 17) * _PRIME32_4;
      pos += 4;
    }

    while (pos < remaining) {
      hash += source[pos] * _PRIME32_5;
      hash = _RotateLeft(hash, 11) * _PRIME32_1;
      ++pos;
    }

    return _Avalanche(hash);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _ProcessStripe(ReadOnlySpan<byte> stripe) {
    this._acc1 = _Round(this._acc1, _ReadUInt32(stripe, 0));
    this._acc2 = _Round(this._acc2, _ReadUInt32(stripe, 4));
    this._acc3 = _Round(this._acc3, _ReadUInt32(stripe, 8));
    this._acc4 = _Round(this._acc4, _ReadUInt32(stripe, 12));
  }

  private uint _FinalizeHash() {
    uint hash;

    if (this._totalLength >= 16)
      hash = _RotateLeft(this._acc1, 1) + _RotateLeft(this._acc2, 7) + _RotateLeft(this._acc3, 12) + _RotateLeft(this._acc4, 18);
    else
      hash = this._seed + _PRIME32_5;

    hash += this._totalLength;

    // Process buffered data
    var offset = 0;
    var remaining = this._bufferLength;

    while (remaining >= 4) {
      hash += _ReadUInt32(this._buffer.AsSpan(), offset) * _PRIME32_3;
      hash = _RotateLeft(hash, 17) * _PRIME32_4;
      offset += 4;
      remaining -= 4;
    }

    while (remaining > 0) {
      hash += this._buffer[offset] * _PRIME32_5;
      hash = _RotateLeft(hash, 11) * _PRIME32_1;
      ++offset;
      --remaining;
    }

    return _Avalanche(hash);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _Round(uint acc, uint input)
    => _RotateLeft(acc + input * _PRIME32_2, 13) * _PRIME32_1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _Avalanche(uint hash) {
    hash ^= hash >> 15;
    hash *= _PRIME32_2;
    hash ^= hash >> 13;
    hash *= _PRIME32_3;
    hash ^= hash >> 16;
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _RotateLeft(uint value, int offset) => (value << offset) | (value >> (32 - offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _ReadUInt32(ReadOnlySpan<byte> source, int offset)
    => (uint)source[offset]
       | ((uint)source[offset + 1] << 8)
       | ((uint)source[offset + 2] << 16)
       | ((uint)source[offset + 3] << 24);
}

#endif
