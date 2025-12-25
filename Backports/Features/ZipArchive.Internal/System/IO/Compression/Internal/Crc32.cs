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

#if !SUPPORTS_ZIPARCHIVE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO.Compression.Internal;

/// <summary>
/// CRC-32 implementation for ZIP files using ISO 3309 polynomial.
/// </summary>
internal static class Crc32 {
  /// <summary>
  /// The polynomial used for CRC-32 calculation (reflected form).
  /// </summary>
  private const uint Polynomial = 0xEDB88320u;

  /// <summary>
  /// Pre-computed lookup table for fast CRC-32 calculation.
  /// </summary>
  private static readonly uint[] _table = _GenerateTable();

  /// <summary>
  /// Generates the CRC-32 lookup table.
  /// </summary>
  private static uint[] _GenerateTable() {
    var table = new uint[256];
    for (var i = 0; i < 256; ++i) {
      var crc = (uint)i;
      for (var j = 0; j < 8; ++j)
        crc = (crc & 1) != 0 ? (crc >> 1) ^ Polynomial : crc >> 1;

      table[i] = crc;
    }

    return table;
  }

  /// <summary>
  /// Computes the CRC-32 checksum for the specified data.
  /// </summary>
  /// <param name="data">The byte array containing the data.</param>
  /// <param name="offset">The starting offset in the array.</param>
  /// <param name="length">The number of bytes to process.</param>
  /// <returns>The CRC-32 checksum.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Compute(byte[] data, int offset, int length) => Update(0xFFFFFFFFu, data, offset, length) ^ 0xFFFFFFFFu;

  /// <summary>
  /// Computes the CRC-32 checksum for the entire byte array.
  /// </summary>
  /// <param name="data">The byte array containing the data.</param>
  /// <returns>The CRC-32 checksum.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Compute(byte[] data) => Compute(data, 0, data.Length);

  /// <summary>
  /// Updates a running CRC-32 with additional data.
  /// </summary>
  /// <param name="crc">The current CRC value (pre-XORed with 0xFFFFFFFF).</param>
  /// <param name="data">The byte array containing the data.</param>
  /// <param name="offset">The starting offset in the array.</param>
  /// <param name="length">The number of bytes to process.</param>
  /// <returns>The updated CRC value (still pre-XORed).</returns>
  public static uint Update(uint crc, byte[] data, int offset, int length) {
    var end = offset + length;
    for (var i = offset; i < end; ++i)
      crc = _table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);

    return crc;
  }

  /// <summary>
  /// Starts a new CRC-32 calculation.
  /// </summary>
  /// <returns>The initial CRC state.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Start() => 0xFFFFFFFFu;

  /// <summary>
  /// Finalizes a CRC-32 calculation.
  /// </summary>
  /// <param name="crc">The running CRC state.</param>
  /// <returns>The final CRC-32 checksum.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Finish(uint crc) => crc ^ 0xFFFFFFFFu;
}

#endif
