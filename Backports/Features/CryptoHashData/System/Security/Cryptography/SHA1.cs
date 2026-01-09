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

// SHA1.HashData was added in .NET 5.0
#if !SUPPORTS_SHA_HASHDATA

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Security.Cryptography;

public static partial class SHA1Polyfills {

  /// <summary>
  /// The hash size produced by the SHA1 algorithm, in bytes.
  /// </summary>
  private const int _HASH_SIZE_IN_BYTES = 20;

  extension(SHA1) {

    /// <summary>
    /// Computes the hash of data using the SHA1 algorithm.
    /// </summary>
    /// <param name="source">The data to hash.</param>
    /// <returns>The hash of the data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] HashData(byte[] source) {
      ArgumentNullException.ThrowIfNull(source);
      using var sha1 = SHA1.Create();
      return sha1.ComputeHash(source);
    }

    /// <summary>
    /// Computes the hash of data using the SHA1 algorithm.
    /// </summary>
    /// <param name="source">The data to hash.</param>
    /// <returns>The hash of the data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] HashData(ReadOnlySpan<byte> source) {
      using var sha1 = SHA1.Create();
      return sha1.ComputeHash(source.ToArray());
    }

    /// <summary>
    /// Computes the hash of data using the SHA1 algorithm.
    /// </summary>
    /// <param name="source">The data to hash.</param>
    /// <param name="destination">The buffer to receive the hash value.</param>
    /// <returns>The total number of bytes written to <paramref name="destination"/>.</returns>
    /// <exception cref="ArgumentException">The buffer in <paramref name="destination"/> is too small to hold the calculated hash size.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int HashData(ReadOnlySpan<byte> source, Span<byte> destination) {
      if (destination.Length < _HASH_SIZE_IN_BYTES)
        throw new ArgumentException("Destination is too short.", nameof(destination));

      var hash = HashData(source);
      hash.AsSpan().CopyTo(destination);
      return _HASH_SIZE_IN_BYTES;
    }

    /// <summary>
    /// Attempts to compute the hash of data using the SHA1 algorithm.
    /// </summary>
    /// <param name="source">The data to hash.</param>
    /// <param name="destination">The buffer to receive the hash value.</param>
    /// <param name="bytesWritten">When this method returns, the total number of bytes written into <paramref name="destination"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="destination"/> is long enough to receive the hash value; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryHashData(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten) {
      if (destination.Length < _HASH_SIZE_IN_BYTES) {
        bytesWritten = 0;
        return false;
      }

      var hash = HashData(source);
      hash.AsSpan().CopyTo(destination);
      bytesWritten = _HASH_SIZE_IN_BYTES;
      return true;
    }

  }

}

#endif
