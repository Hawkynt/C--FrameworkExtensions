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

#if !SUPPORTS_XXHASH128 && !OFFICIAL_XXHASH128

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO.Hashing;

/// <summary>
/// Provides an implementation of the XXH128 (XXH3 128-bit) hash algorithm.
/// </summary>
/// <remarks>
/// This is a polyfill implementation for .NET versions prior to .NET 10.0.
/// XXH128 is an extremely fast non-cryptographic 128-bit hash algorithm from the XXH3 family.
/// Note: This class uses unchecked arithmetic for performance and correctness (hash algorithms rely on overflow behavior).
/// </remarks>
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
public sealed class XxHash128 {

  private readonly struct Hash128(ulong low, ulong high) : IEquatable<Hash128> {
    public ulong Low { get; } = low;
    public ulong High { get; } = high;

    public bool Equals(Hash128 other) => this.Low == other.Low && this.High == other.High;
    public override bool Equals(object? obj) => obj is Hash128 other && this.Equals(other);
    public override int GetHashCode() => this.Low.GetHashCode() ^ this.High.GetHashCode();
    public static bool operator ==(Hash128 left, Hash128 right) => left.Equals(right);
    public static bool operator !=(Hash128 left, Hash128 right) => !left.Equals(right);

    public byte[] ToByteArray() {
      var result = new byte[16];
      _WriteUInt64(result, 0, this.Low);
      _WriteUInt64(result, 8, this.High);
      return result;
    }

    private static void _WriteUInt64(byte[] dest, int offset, ulong value) {
      dest[offset] = (byte)value;
      dest[offset + 1] = (byte)(value >> 8);
      dest[offset + 2] = (byte)(value >> 16);
      dest[offset + 3] = (byte)(value >> 24);
      dest[offset + 4] = (byte)(value >> 32);
      dest[offset + 5] = (byte)(value >> 40);
      dest[offset + 6] = (byte)(value >> 48);
      dest[offset + 7] = (byte)(value >> 56);
    }
  }
  private const ulong _PRIME64_1 = 0x9E3779B185EBCA87UL;
  private const ulong _PRIME64_2 = 0xC2B2AE3D27D4EB4FUL;
  // Precomputed sum to avoid compile-time overflow in checked context
  private static readonly ulong _PRIME64_1_PLUS_2 = unchecked(_PRIME64_1 + _PRIME64_2);
  private const ulong _PRIME64_3 = 0x165667B19E3779F9UL;
  private const ulong _PRIME64_4 = 0x85EBCA77C2B2AE63UL;
  private const ulong _PRIME64_5 = 0x27D4EB2F165667C5UL;

  private const ulong _PRIME_MX1 = 0x165667919E3779F9UL;
  private const ulong _PRIME_MX2 = 0x9FB21C651E98DF25UL;

  private const int _STRIPE_LEN = 64;
  private const int _SECRET_CONSUME_RATE = 8;
  private const int _ACC_NB = 8;

  // Default secret (first 192 bytes)
  private static readonly byte[] _defaultSecret = {
    0xb8, 0xfe, 0x6c, 0x39, 0x23, 0xa4, 0x4b, 0xbe, 0x7c, 0x01, 0x81, 0x2c, 0xf7, 0x21, 0xad, 0x1c,
    0xde, 0xd4, 0x6d, 0xe9, 0x83, 0x90, 0x97, 0xdb, 0x72, 0x40, 0xa4, 0xa4, 0xb7, 0xb3, 0x67, 0x1f,
    0xcb, 0x79, 0xe6, 0x4e, 0xcc, 0xc0, 0xe5, 0x78, 0x82, 0x5a, 0xd0, 0x7d, 0xcc, 0xff, 0x72, 0x21,
    0xb8, 0x08, 0x46, 0x74, 0xf7, 0x43, 0x24, 0x8e, 0xe0, 0x35, 0x90, 0xe6, 0x81, 0x3a, 0x26, 0x4c,
    0x3c, 0x28, 0x52, 0xbb, 0x91, 0xc3, 0x00, 0xcb, 0x88, 0xd0, 0x65, 0x8b, 0x1b, 0x53, 0x2e, 0xa3,
    0x71, 0x64, 0x48, 0x97, 0xa2, 0x0d, 0xf9, 0x4e, 0x38, 0x19, 0xef, 0x46, 0xa9, 0xde, 0xac, 0xd8,
    0xa8, 0xfa, 0x76, 0x3f, 0xe3, 0x9c, 0x34, 0x3f, 0xf9, 0xdc, 0xbb, 0xc7, 0xc7, 0x0b, 0x4f, 0x1d,
    0x8a, 0x51, 0xe0, 0x4b, 0xcd, 0xb4, 0x59, 0x31, 0xc8, 0x9f, 0x7e, 0xc9, 0xd9, 0x78, 0x73, 0x64,
    0xea, 0xc5, 0xac, 0x83, 0x34, 0xd3, 0xeb, 0xc3, 0xc5, 0x81, 0xa0, 0xff, 0xfa, 0x13, 0x63, 0xeb,
    0x17, 0x0d, 0xdd, 0x51, 0xb7, 0xf0, 0xda, 0x49, 0xd3, 0x16, 0xca, 0xbb, 0x3c, 0x52, 0xd3, 0x74,
    0x23, 0x97, 0x98, 0x28, 0x4e, 0x3d, 0xe9, 0xc0, 0xfe, 0x6e, 0xde, 0x4b, 0x00, 0x60, 0xbe, 0x63,
    0x3c, 0x7a, 0x7b, 0x60, 0x0c, 0x16, 0x82, 0xd0, 0xc3, 0xad, 0x24, 0xff, 0xa6, 0x60, 0x38, 0xc0
  };

  private readonly long _seed;
  private readonly ulong[] _acc = new ulong[_ACC_NB];
  private readonly byte[] _buffer = new byte[256];
  private int _bufferLength;
  private ulong _totalLength;
  private int _nbStripesSoFar;

  /// <summary>
  /// Initializes a new instance of the <see cref="XxHash128"/> class with seed 0.
  /// </summary>
  public XxHash128() : this(0) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="XxHash128"/> class with the specified seed.
  /// </summary>
  /// <param name="seed">The seed value for the hash computation.</param>
  public XxHash128(long seed) {
    this._seed = seed;
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
    if (source.Length == 0)
      return;

    this._totalLength += (ulong)source.Length;

    if (this._bufferLength + source.Length <= 256) {
      source.CopyTo(this._buffer.AsSpan(this._bufferLength));
      this._bufferLength += source.Length;
      return;
    }

    var offset = 0;

    // Fill buffer if partially filled
    if (this._bufferLength > 0) {
      var fillLen = 256 - this._bufferLength;
      source.Slice(0, fillLen).CopyTo(this._buffer.AsSpan(this._bufferLength));
      this._ConsumeStripes(this._buffer.AsSpan(), 4, ref this._nbStripesSoFar);
      this._bufferLength = 0;
      offset = fillLen;
    }

    // Process full 64-byte stripes
    var remaining = source.Length - offset;
    while (remaining >= _STRIPE_LEN) {
      var stripesToProcess = Math.Min(remaining / _STRIPE_LEN, 4);
      this._ConsumeStripes(source.Slice(offset), stripesToProcess, ref this._nbStripesSoFar);
      offset += stripesToProcess * _STRIPE_LEN;
      remaining = source.Length - offset;
    }

    // Buffer remaining
    if (remaining > 0) {
      source.Slice(offset).CopyTo(this._buffer.AsSpan());
      this._bufferLength = remaining;
    }
  }

  /// <summary>
  /// Resets the hash computation to its initial state.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Reset() {
    var seed = (ulong)this._seed;
    this._acc[0] = _PRIME64_1_PLUS_2;
    this._acc[1] = _PRIME64_2;
    this._acc[2] = 0;
    this._acc[3] = seed;
    this._acc[4] = _PRIME64_1;
    this._acc[5] = unchecked(seed ^ _PRIME64_1_PLUS_2);
    this._acc[6] = 0;
    this._acc[7] = seed;
    this._bufferLength = 0;
    this._totalLength = 0;
    this._nbStripesSoFar = 0;
  }

  /// <summary>
  /// Gets the current computed hash value as a byte array.
  /// </summary>
  /// <returns>The hash value as a 16-byte array.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte[] GetCurrentHash() => this._FinalizeHash().ToByteArray();

  /// <summary>
  /// Computes the XXH128 hash for the input data.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(byte[] source) => Hash(source, 0);

  /// <summary>
  /// Computes the XXH128 hash for the input data with the specified seed.
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
  /// Computes the XXH128 hash for the input data.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(ReadOnlySpan<byte> source) => Hash(source, 0);

  /// <summary>
  /// Computes the XXH128 hash for the input data with the specified seed.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <param name="seed">The seed value.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(ReadOnlySpan<byte> source, long seed)
    => _ComputeHash(source, seed).ToByteArray();

  private static Hash128 _ComputeHash(ReadOnlySpan<byte> source, long seed) {
    var length = source.Length;
    var useed = (ulong)seed;

    if (length <= 16)
      return _Len0To16(source, useed);

    if (length <= 128)
      return _Len17To128(source, useed);

    if (length <= 240)
      return _Len129To240(source, useed);

    return _HashLong(source, useed);
  }

  private void _ConsumeStripes(ReadOnlySpan<byte> data, int nbStripes, ref int nbStripesSoFar) {
    for (var i = 0; i < nbStripes; ++i) {
      var stripeOffset = i * _STRIPE_LEN;
      var secretOffset = nbStripesSoFar * _SECRET_CONSUME_RATE;
      this._Accumulate(data.Slice(stripeOffset, _STRIPE_LEN), secretOffset);
      ++nbStripesSoFar;
      if (nbStripesSoFar == 16) {
        this._ScrambleAcc();
        nbStripesSoFar = 0;
      }
    }
  }

  private void _Accumulate(ReadOnlySpan<byte> stripe, int secretOffset) {
    for (var i = 0; i < _ACC_NB; ++i) {
      var dataVal = _ReadUInt64(stripe, i * 8);
      var dataKey = dataVal ^ _ReadUInt64(_defaultSecret.AsSpan(), secretOffset + i * 8);
      this._acc[i ^ 1] += dataVal;
      this._acc[i] += _Mult32To64((uint)dataKey, (uint)(dataKey >> 32));
    }
  }

  private void _ScrambleAcc() {
    for (var i = 0; i < _ACC_NB; ++i) {
      var key = _ReadUInt64(_defaultSecret.AsSpan(), 128 + i * 8);
      this._acc[i] = (this._acc[i] ^ (this._acc[i] >> 47) ^ key) * _PRIME64_1;
    }
  }

  private Hash128 _FinalizeHash() {
    if (this._totalLength <= 240)
      return this._FinalizeShort();

    return this._FinalizeLong();
  }

  private Hash128 _FinalizeShort() {
    // For short inputs, compute directly from buffer
    if (this._totalLength <= 16)
      return _Len0To16(this._buffer.AsSpan(0, this._bufferLength), (ulong)this._seed);

    if (this._totalLength <= 128)
      return _Len17To128(this._buffer.AsSpan(0, this._bufferLength), (ulong)this._seed);

    return _Len129To240(this._buffer.AsSpan(0, this._bufferLength), (ulong)this._seed);
  }

  private Hash128 _FinalizeLong() {
    // Process remaining stripes in buffer
    var nbStripes = this._bufferLength / _STRIPE_LEN;
    var nbStripesSoFar = this._nbStripesSoFar;

    for (var i = 0; i < nbStripes; ++i) {
      var secretOffset = nbStripesSoFar * _SECRET_CONSUME_RATE;
      this._Accumulate(this._buffer.AsSpan(i * _STRIPE_LEN, _STRIPE_LEN), secretOffset);
      ++nbStripesSoFar;
      if (nbStripesSoFar == 16) {
        this._ScrambleAcc();
        nbStripesSoFar = 0;
      }
    }

    // Process last stripe (may overlap)
    if (this._bufferLength > 0) {
      var lastStripeOffset = this._bufferLength >= _STRIPE_LEN ? this._bufferLength - _STRIPE_LEN : 0;
      this._Accumulate(this._buffer.AsSpan(lastStripeOffset, _STRIPE_LEN), 121);
    }

    // Final merge
    var low = _MergeAccs(this._acc, 11) + _MixTwo(this._totalLength * _PRIME64_1, this._totalLength * _PRIME64_4);
    var high = _MergeAccs(this._acc, 103) + _MixTwo(this._totalLength * _PRIME64_2, this._totalLength * _PRIME64_3);

    low = _Xxh3Avalanche(low);
    high = _Xxh3Avalanche(high);

    return new(low, high);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _MergeAccs(ulong[] acc, int secretOffset) {
    var result = (ulong)acc.Length * _PRIME64_1;
    for (var i = 0; i < acc.Length; i += 2) {
      result += _MixTwo(
        acc[i] ^ _ReadUInt64(_defaultSecret.AsSpan(), secretOffset + i * 8),
        acc[i + 1] ^ _ReadUInt64(_defaultSecret.AsSpan(), secretOffset + i * 8 + 8)
      );
    }

    return _Xxh3Avalanche(result);
  }

  private static Hash128 _Len0To16(ReadOnlySpan<byte> input, ulong seed) {
    if (input.Length > 8)
      return _Len9To16(input, seed);

    if (input.Length >= 4)
      return _Len4To8(input, seed);

    if (input.Length > 0)
      return _Len1To3(input, seed);

    return new(seed ^ _ReadUInt64(_defaultSecret.AsSpan(), 64) ^ _ReadUInt64(_defaultSecret.AsSpan(), 72),
               seed ^ _ReadUInt64(_defaultSecret.AsSpan(), 80) ^ _ReadUInt64(_defaultSecret.AsSpan(), 88));
  }

  private static Hash128 _Len1To3(ReadOnlySpan<byte> input, ulong seed) {
    var c1 = input[0];
    var c2 = input[input.Length >> 1];
    var c3 = input[^1];
    var combined = ((uint)c1 << 16) | ((uint)c2 << 24) | c3 | ((uint)input.Length << 8);

    var bitflipLow = (_ReadUInt32(_defaultSecret.AsSpan(), 0) ^ _ReadUInt32(_defaultSecret.AsSpan(), 4)) + seed;
    var bitflipHigh = (_ReadUInt32(_defaultSecret.AsSpan(), 8) ^ _ReadUInt32(_defaultSecret.AsSpan(), 12)) - seed;

    var keyedLo = combined ^ bitflipLow;
    var keyedHi = combined ^ bitflipHigh;

    var low = _Xxh64Avalanche(keyedLo);
    var high = _Xxh64Avalanche(keyedHi);

    return new(low, high);
  }

  private static Hash128 _Len4To8(ReadOnlySpan<byte> input, ulong seed) {
    seed ^= (ulong)_Swap32((uint)seed) << 32;
    var inputLo = _ReadUInt32(input, 0);
    var inputHi = _ReadUInt32(input, input.Length - 4);
    var input64 = inputLo + ((ulong)inputHi << 32);

    var bitflip = (_ReadUInt64(_defaultSecret.AsSpan(), 16) ^ _ReadUInt64(_defaultSecret.AsSpan(), 24)) + seed;
    var keyed = input64 ^ bitflip;

    var m128 = _Mult64To128(keyed, _PRIME64_1 + ((ulong)input.Length << 2));
    var low = m128.Low + (m128.High << 1);
    var high = m128.High + (m128.Low << 1);

    low = _Xxh3Avalanche(low);
    high = _Xxh3Avalanche(high);

    return new(low, high);
  }

  private static Hash128 _Len9To16(ReadOnlySpan<byte> input, ulong seed) {
    var bitflipLow = (_ReadUInt64(_defaultSecret.AsSpan(), 32) ^ _ReadUInt64(_defaultSecret.AsSpan(), 40)) - seed;
    var bitflipHigh = (_ReadUInt64(_defaultSecret.AsSpan(), 48) ^ _ReadUInt64(_defaultSecret.AsSpan(), 56)) + seed;
    var inputLo = _ReadUInt64(input, 0);
    var inputHi = _ReadUInt64(input, input.Length - 8);

    var m128 = _Mult64To128(inputLo ^ inputHi ^ bitflipLow, _PRIME64_1);
    var low = m128.Low + (ulong)(input.Length - 1) * _PRIME64_2;
    var high = m128.High + ((ulong)input.Length - 1) * _PRIME64_3;

    low += inputHi ^ bitflipHigh;
    high += inputLo ^ bitflipLow;

    low = _Xxh3Avalanche(low);
    high = _Xxh3Avalanche(high);

    return new(low, high);
  }

  private static Hash128 _Len17To128(ReadOnlySpan<byte> input, ulong seed) {
    var accLow = (ulong)input.Length * _PRIME64_1;
    var accHigh = 0uL;

    if (input.Length > 32) {
      if (input.Length > 64) {
        if (input.Length > 96) {
          var low = _ReadUInt64(input, 48);
          var high = _ReadUInt64(input, 56);
          var lowEnd = _ReadUInt64(input, input.Length - 64);
          var highEnd = _ReadUInt64(input, input.Length - 56);
          accLow += _MixTwo(low ^ (_ReadUInt64(_defaultSecret.AsSpan(), 96) + seed), high ^ (_ReadUInt64(_defaultSecret.AsSpan(), 104) - seed));
          accLow ^= lowEnd + highEnd;
          accHigh += _MixTwo(lowEnd ^ (_ReadUInt64(_defaultSecret.AsSpan(), 112) + seed), highEnd ^ (_ReadUInt64(_defaultSecret.AsSpan(), 120) - seed));
          accHigh ^= low + high;
        }

        {
          var low = _ReadUInt64(input, 32);
          var high = _ReadUInt64(input, 40);
          var lowEnd = _ReadUInt64(input, input.Length - 48);
          var highEnd = _ReadUInt64(input, input.Length - 40);
          accLow += _MixTwo(low ^ (_ReadUInt64(_defaultSecret.AsSpan(), 64) + seed), high ^ (_ReadUInt64(_defaultSecret.AsSpan(), 72) - seed));
          accLow ^= lowEnd + highEnd;
          accHigh += _MixTwo(lowEnd ^ (_ReadUInt64(_defaultSecret.AsSpan(), 80) + seed), highEnd ^ (_ReadUInt64(_defaultSecret.AsSpan(), 88) - seed));
          accHigh ^= low + high;
        }
      }

      {
        var low = _ReadUInt64(input, 16);
        var high = _ReadUInt64(input, 24);
        var lowEnd = _ReadUInt64(input, input.Length - 32);
        var highEnd = _ReadUInt64(input, input.Length - 24);
        accLow += _MixTwo(low ^ (_ReadUInt64(_defaultSecret.AsSpan(), 32) + seed), high ^ (_ReadUInt64(_defaultSecret.AsSpan(), 40) - seed));
        accLow ^= lowEnd + highEnd;
        accHigh += _MixTwo(lowEnd ^ (_ReadUInt64(_defaultSecret.AsSpan(), 48) + seed), highEnd ^ (_ReadUInt64(_defaultSecret.AsSpan(), 56) - seed));
        accHigh ^= low + high;
      }
    }

    {
      var low = _ReadUInt64(input, 0);
      var high = _ReadUInt64(input, 8);
      var lowEnd = _ReadUInt64(input, input.Length - 16);
      var highEnd = _ReadUInt64(input, input.Length - 8);
      accLow += _MixTwo(low ^ (_ReadUInt64(_defaultSecret.AsSpan(), 0) + seed), high ^ (_ReadUInt64(_defaultSecret.AsSpan(), 8) - seed));
      accLow ^= lowEnd + highEnd;
      accHigh += _MixTwo(lowEnd ^ (_ReadUInt64(_defaultSecret.AsSpan(), 16) + seed), highEnd ^ (_ReadUInt64(_defaultSecret.AsSpan(), 24) - seed));
      accHigh ^= low + high;
    }

    var low128 = accLow + accHigh;
    var high128 = accLow * _PRIME64_1 + accHigh * _PRIME64_4 + unchecked((ulong)input.Length - seed) * _PRIME64_2;

    low128 = _Xxh3Avalanche(low128);
    high128 = 0 - _Xxh3Avalanche(high128);

    return new(low128, high128);
  }

  private static Hash128 _Len129To240(ReadOnlySpan<byte> input, ulong seed) {
    var accLow = (ulong)input.Length * _PRIME64_1;
    var accHigh = 0uL;
    var nbRounds = input.Length / 32;

    for (var i = 0; i < 4; ++i) {
      accLow += _MixTwo(
        _ReadUInt64(input, 32 * i) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 32 * i) + seed),
        _ReadUInt64(input, 32 * i + 8) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 32 * i + 8) - seed));
      accLow ^= _ReadUInt64(input, 32 * i + 16) + _ReadUInt64(input, 32 * i + 24);
      accHigh += _MixTwo(
        _ReadUInt64(input, 32 * i + 16) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 32 * i + 16) + seed),
        _ReadUInt64(input, 32 * i + 24) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 32 * i + 24) - seed));
      accHigh ^= _ReadUInt64(input, 32 * i) + _ReadUInt64(input, 32 * i + 8);
    }

    accLow = _Xxh3Avalanche(accLow);
    accHigh = _Xxh3Avalanche(accHigh);

    for (var i = 4; i < nbRounds; ++i) {
      accLow += _MixTwo(
        _ReadUInt64(input, 32 * i) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 32 * (i - 4) + 3) + seed),
        _ReadUInt64(input, 32 * i + 8) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 32 * (i - 4) + 11) - seed));
      accLow ^= _ReadUInt64(input, 32 * i + 16) + _ReadUInt64(input, 32 * i + 24);
      accHigh += _MixTwo(
        _ReadUInt64(input, 32 * i + 16) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 32 * (i - 4) + 19) + seed),
        _ReadUInt64(input, 32 * i + 24) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 32 * (i - 4) + 27) - seed));
      accHigh ^= _ReadUInt64(input, 32 * i) + _ReadUInt64(input, 32 * i + 8);
    }

    // last 32 bytes
    accLow += _MixTwo(
      _ReadUInt64(input, input.Length - 16) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 103) + seed),
      _ReadUInt64(input, input.Length - 8) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 111) - seed));
    accLow ^= _ReadUInt64(input, input.Length - 32) + _ReadUInt64(input, input.Length - 24);
    accHigh += _MixTwo(
      _ReadUInt64(input, input.Length - 32) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 119) + seed),
      _ReadUInt64(input, input.Length - 24) ^ (_ReadUInt64(_defaultSecret.AsSpan(), 127) - seed));
    accHigh ^= _ReadUInt64(input, input.Length - 16) + _ReadUInt64(input, input.Length - 8);

    var low = accLow + accHigh;
    var high = accLow * _PRIME64_1 + accHigh * _PRIME64_4 + unchecked((ulong)input.Length - seed) * _PRIME64_2;

    low = _Xxh3Avalanche(low);
    high = 0 - _Xxh3Avalanche(high);

    return new(low, high);
  }

  private static Hash128 _HashLong(ReadOnlySpan<byte> input, ulong seed) {
    const int stripesPerBlock = 16;
    const int blockLen = stripesPerBlock * _STRIPE_LEN; // 1024 bytes

    var acc = new ulong[_ACC_NB];
    acc[0] = _PRIME64_1_PLUS_2;
    acc[1] = _PRIME64_2;
    acc[2] = 0;
    acc[3] = seed;
    acc[4] = _PRIME64_1;
    acc[5] = seed ^ (_PRIME64_1_PLUS_2);
    acc[6] = 0;
    acc[7] = seed;

    var nbBlocks = (input.Length - 1) / blockLen;

    for (var b = 0; b < nbBlocks; ++b) {
      for (var s = 0; s < stripesPerBlock; ++s) {
        var stripeOffset = b * blockLen + s * _STRIPE_LEN;
        var secretOffset = s * _SECRET_CONSUME_RATE;
        for (var i = 0; i < _ACC_NB; ++i) {
          var dataVal = _ReadUInt64(input, stripeOffset + i * 8);
          var dataKey = dataVal ^ _ReadUInt64(_defaultSecret.AsSpan(), secretOffset + i * 8);
          acc[i ^ 1] += dataVal;
          acc[i] += _Mult32To64((uint)dataKey, (uint)(dataKey >> 32));
        }
      }

      // Scramble
      for (var i = 0; i < _ACC_NB; ++i) {
        var key = _ReadUInt64(_defaultSecret.AsSpan(), 128 + i * 8);
        acc[i] = (acc[i] ^ (acc[i] >> 47) ^ key) * _PRIME64_1;
      }
    }

    // Last partial block
    var nbStripes = ((input.Length - 1) - (nbBlocks * blockLen)) / _STRIPE_LEN;
    var nbStripesSoFar = 0;

    for (var s = 0; s < nbStripes; ++s) {
      var stripeOffset = nbBlocks * blockLen + s * _STRIPE_LEN;
      var secretOffset = nbStripesSoFar * _SECRET_CONSUME_RATE;
      for (var i = 0; i < _ACC_NB; ++i) {
        var dataVal = _ReadUInt64(input, stripeOffset + i * 8);
        var dataKey = dataVal ^ _ReadUInt64(_defaultSecret.AsSpan(), secretOffset + i * 8);
        acc[i ^ 1] += dataVal;
        acc[i] += _Mult32To64((uint)dataKey, (uint)(dataKey >> 32));
      }

      ++nbStripesSoFar;
    }

    // Last stripe
    var lastStripeOffset = input.Length - _STRIPE_LEN;
    for (var i = 0; i < _ACC_NB; ++i) {
      var dataVal = _ReadUInt64(input, lastStripeOffset + i * 8);
      var dataKey = dataVal ^ _ReadUInt64(_defaultSecret.AsSpan(), 121 + i * 8);
      acc[i ^ 1] += dataVal;
      acc[i] += _Mult32To64((uint)dataKey, (uint)(dataKey >> 32));
    }

    // Final merge
    var low = _MergeAccs(acc, 11) + _MixTwo((ulong)input.Length * _PRIME64_1, (ulong)input.Length * _PRIME64_4);
    var high = _MergeAccs(acc, 103) + _MixTwo((ulong)input.Length * _PRIME64_2, (ulong)input.Length * _PRIME64_3);

    low = _Xxh3Avalanche(low);
    high = _Xxh3Avalanche(high);

    return new(low, high);
  }

  // Utility functions
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _Xxh64Avalanche(ulong hash) {
    hash ^= hash >> 33;
    hash *= _PRIME64_2;
    hash ^= hash >> 29;
    hash *= _PRIME64_3;
    hash ^= hash >> 32;
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _Xxh3Avalanche(ulong hash) {
    hash ^= hash >> 37;
    hash *= _PRIME_MX1;
    hash ^= hash >> 32;
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _MixTwo(ulong lhs, ulong rhs) {
    var result = _Mult64To128(lhs, rhs);
    return result.Low ^ result.High;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Hash128 _Mult64To128(ulong lhs, ulong rhs) {
    var lhsHigh = lhs >> 32;
    var lhsLow = (uint)lhs;
    var rhsHigh = rhs >> 32;
    var rhsLow = (uint)rhs;

    var high = lhsHigh * rhsHigh;
    var mid1 = lhsHigh * rhsLow;
    var mid2 = lhsLow * rhsHigh;
    var low = (ulong)lhsLow * rhsLow;

    var mid = mid1 + mid2;
    if (mid < mid1)
      high += 1UL << 32;

    high += mid >> 32;
    var midLow = mid << 32;
    low += midLow;
    if (low < midLow)
      ++high;

    return new(low, high);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _Mult32To64(uint lhs, uint rhs) => (ulong)lhs * rhs;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _Swap32(uint x) => ((x << 24) & 0xff000000) | ((x << 8) & 0x00ff0000) | ((x >> 8) & 0x0000ff00) | ((x >> 24) & 0x000000ff);

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
