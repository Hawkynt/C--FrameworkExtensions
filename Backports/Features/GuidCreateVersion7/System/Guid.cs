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

// Guid.CreateVersion7 was added in .NET 9.0
#if !SUPPORTS_GUID_CREATEVERSION7

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class GuidPolyfills {

  extension(Guid) {

    /// <summary>
    /// Creates a new <see cref="Guid"/> according to RFC 9562, following the Version 7 format.
    /// </summary>
    /// <returns>A new <see cref="Guid"/> according to RFC 9562, following the Version 7 format.</returns>
    /// <remarks>
    /// <para>
    /// Version 7 GUIDs are generated using a Unix timestamp with millisecond precision
    /// in the first 48 bits, followed by random data. This makes them suitable for database
    /// primary keys as they are roughly time-ordered.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid CreateVersion7() => CreateVersion7(DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a new <see cref="Guid"/> according to RFC 9562, following the Version 7 format.
    /// </summary>
    /// <param name="timestamp">The date time offset used to determine the Unix Epoch timestamp.</param>
    /// <returns>A new <see cref="Guid"/> according to RFC 9562, following the Version 7 format.</returns>
    /// <remarks>
    /// <para>
    /// Version 7 GUIDs embed a Unix timestamp with millisecond precision in the first 48 bits.
    /// The remaining bits contain random data for uniqueness within the same millisecond.
    /// </para>
    /// </remarks>
    public static Guid CreateVersion7(DateTimeOffset timestamp) {
      // UUID v7 layout (RFC 9562):
      // 0                   1                   2                   3
      // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
      // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      // |                         unix_ts_ms (32 bits)                 |
      // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      // |          unix_ts_ms (16 bits) |  ver  |   rand_a (12 bits)   |
      // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      // |var|                       rand_b (62 bits)                   |
      // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      // |                       rand_b (continued)                     |
      // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

      // Get Unix timestamp in milliseconds
      var unixMs = timestamp.ToUnixTimeMilliseconds();

      // Create a 16-byte array for the GUID
      var bytes = new byte[16];

      // Fill with random bytes first
      RandomNumberGenerator.Fill(bytes);

      // Set the timestamp (first 48 bits = 6 bytes)
      // Stored in big-endian format
      bytes[0] = (byte)(unixMs >> 40);
      bytes[1] = (byte)(unixMs >> 32);
      bytes[2] = (byte)(unixMs >> 24);
      bytes[3] = (byte)(unixMs >> 16);
      bytes[4] = (byte)(unixMs >> 8);
      bytes[5] = (byte)unixMs;

      // Set version to 7 (0111 in high nibble of byte 6)
      bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);

      // Set variant to RFC 4122 (10xx in high bits of byte 8)
      bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

      // The Guid constructor expects bytes in a specific order for little-endian systems
      // We need to swap bytes for the first three fields (a, b, c)
      return new Guid(
        (int)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]),
        (short)((bytes[4] << 8) | bytes[5]),
        (short)((bytes[6] << 8) | bytes[7]),
        bytes[8],
        bytes[9],
        bytes[10],
        bytes[11],
        bytes[12],
        bytes[13],
        bytes[14],
        bytes[15]
      );
    }

  }

}

#endif
