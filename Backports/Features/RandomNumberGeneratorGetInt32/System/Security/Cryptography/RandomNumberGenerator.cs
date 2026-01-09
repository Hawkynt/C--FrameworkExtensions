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

// RandomNumberGenerator.GetInt32 was added in .NET 6.0
#if !SUPPORTS_RNG_GETINT32

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Security.Cryptography;

public static partial class RandomNumberGeneratorPolyfills {

  extension(RandomNumberGenerator) {

    /// <summary>
    /// Generates a random integer between 0 (inclusive) and a specified exclusive upper bound using a cryptographically strong random number generator.
    /// </summary>
    /// <param name="toExclusive">The exclusive upper bound of the random range.</param>
    /// <returns>A random integer that is greater than or equal to 0 and less than <paramref name="toExclusive"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="toExclusive"/> is less than or equal to 0.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetInt32(int toExclusive) {
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(toExclusive);
      return GetInt32(0, toExclusive);
    }

    /// <summary>
    /// Generates a random integer between a specified inclusive lower bound and a specified exclusive upper bound using a cryptographically strong random number generator.
    /// </summary>
    /// <param name="fromInclusive">The inclusive lower bound of the random range.</param>
    /// <param name="toExclusive">The exclusive upper bound of the random range.</param>
    /// <returns>A random integer that is greater than or equal to <paramref name="fromInclusive"/> and less than <paramref name="toExclusive"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="toExclusive"/> is less than or equal to <paramref name="fromInclusive"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetInt32(int fromInclusive, int toExclusive) {
      ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(toExclusive,fromInclusive);
      
      // Calculate the range
      var range = (uint)(toExclusive - fromInclusive);

      // For ranges that are powers of two, we can use simple masking
      if ((range & (range - 1)) == 0) {
        var randomBytes = new byte[4];
        _FillRandomBytes(randomBytes);
        var randomValue = BitConverter.ToUInt32(randomBytes, 0);
        return fromInclusive + (int)(randomValue & (range - 1));
      }

      // Use rejection sampling to avoid modulo bias
      // Calculate the largest multiple of range that fits in uint.MaxValue
      var limit = uint.MaxValue - (uint.MaxValue % range);

      uint result;
      do {
        var randomBytes = new byte[4];
        _FillRandomBytes(randomBytes);
        result = BitConverter.ToUInt32(randomBytes, 0);
      } while (result >= limit);

      return fromInclusive + (int)(result % range);
    }

    /// <summary>
    /// Fills a byte array with a cryptographically strong random sequence of values.
    /// </summary>
    /// <param name="data">The array to fill with cryptographically strong random bytes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetBytes(byte[] data) {
      ArgumentNullException.ThrowIfNull(data);
      _FillRandomBytes(data);
    }

    /// <summary>
    /// Fills a byte array with a cryptographically strong random sequence of values, starting at a specified offset for a specified number of bytes.
    /// </summary>
    /// <param name="data">The array to fill with cryptographically strong random bytes.</param>
    /// <param name="offset">The index of the array to start the fill operation.</param>
    /// <param name="count">The number of bytes to fill.</param>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is less than 0, <paramref name="count"/> is less than 0, or their sum exceeds the length of <paramref name="data"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetBytes(byte[] data, int offset, int count) {
      ArgumentNullException.ThrowIfNull(data);
      ArgumentOutOfRangeException.ThrowIfNegative(offset);
      ArgumentOutOfRangeException.ThrowIfNegative(count);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + count, data.Length);

      if (count == 0)
        return;

      var tempBuffer = new byte[count];
      _FillRandomBytes(tempBuffer);
      Array.Copy(tempBuffer, 0, data, offset, count);
    }

  }

  private static void _FillRandomBytes(byte[] data) {
#if NEEDS_RNG_DISPOSE
    using var rng = new RNGCryptoServiceProvider();
    rng.GetBytes(data);
#else
    new RNGCryptoServiceProvider().GetBytes(data);
#endif
  }

}

#endif
