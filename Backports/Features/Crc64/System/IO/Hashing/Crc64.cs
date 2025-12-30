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

#if !SUPPORTS_CRC64 && !OFFICIAL_CRC64

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO.Hashing;

/// <summary>
/// Provides an implementation of the CRC-64 algorithm as used by ECMA-182.
/// </summary>
/// <remarks>
/// This is a polyfill implementation for .NET versions prior to .NET 8.0.
/// Uses the ECMA-182 polynomial (0xC96C5795D7870F42 in reflected form).
/// </remarks>
public sealed class Crc64 {
  private const ulong _POLYNOMIAL = 0xC96C5795D7870F42uL;
  private static readonly ulong[] _table = GenerateTable();
  private ulong _hash;

  /// <summary>
  /// Initializes a new instance of the <see cref="Crc64"/> class.
  /// </summary>
  public Crc64() { }

  /// <summary>
  /// Appends the contents of the source to the data already processed.
  /// </summary>
  /// <param name="source">The data to process.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Append(byte[] source) {
    ArgumentNullException.ThrowIfNull(source);
    this._hash = AppendCore(this._hash, source, 0, source.Length);
  }

  /// <summary>
  /// Appends the contents of the source to the data already processed.
  /// </summary>
  /// <param name="source">The data to process.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Append(ReadOnlySpan<byte> source) => this._hash = AppendCore(this._hash, source);

  /// <summary>
  /// Resets the hash computation to its initial state.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Reset() => this._hash = 0uL;

  /// <summary>
  /// Gets the current computed hash value as a byte array.
  /// </summary>
  /// <returns>The hash value as a byte array.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte[] GetCurrentHash() => BitConverter.GetBytes(this._hash);

  /// <summary>
  /// Computes the CRC-64 hash for the input data.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(byte[] source) {
    ArgumentNullException.ThrowIfNull(source);
    return BitConverter.GetBytes(AppendCore(0uL, source, 0, source.Length));
  }

  /// <summary>
  /// Computes the CRC-64 hash for the input data.
  /// </summary>
  /// <param name="source">The data to compute the hash for.</param>
  /// <returns>The computed hash.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] Hash(ReadOnlySpan<byte> source) => BitConverter.GetBytes(AppendCore(0uL, source));

  private static ulong[] GenerateTable() {
    var table = new ulong[256];
    for (ulong i = 0; i < 256; ++i) {
      var crc = i;
      for (var j = 0; j < 8; ++j)
        crc = (crc & 1) != 0 ? (crc >> 1) ^ _POLYNOMIAL : crc >> 1;

      table[i] = crc;
    }

    return table;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong AppendCore(ulong hash, ReadOnlySpan<byte> source) {
    foreach (var b in source)
      hash = _table[(byte)(hash ^ b)] ^ (hash >> 8);

    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong AppendCore(ulong hash, byte[] source, int offset, int count) {
    var end = offset + count;
    for (var i = offset; i < end; ++i)
      hash = _table[(byte)(hash ^ source[i])] ^ (hash >> 8);

    return hash;
  }
}

#endif
