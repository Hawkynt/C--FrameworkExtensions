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

#if !SUPPORTS_BINARYREADER_READEXACTLY

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

/// <summary>
/// Provides extension methods for <see cref="BinaryReader"/> ReadExactly methods added in .NET 9.0.
/// </summary>
public static class BinaryReaderPolyfills {
  /// <param name="this">The <see cref="BinaryReader"/> instance.</param>
  extension(BinaryReader @this) {
    /// <summary>
    /// Reads the specified number of bytes from the current stream into a byte array.
    /// </summary>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>A byte array containing the requested bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before reading the requested number of bytes.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadExactly(int count) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentOutOfRangeException.ThrowIfNegative(count);

      var buffer = new byte[count];
      var totalRead = 0;
      while (totalRead < count) {
        var bytesRead = @this.Read(buffer, totalRead, count - totalRead);
        if (bytesRead == 0)
          throw new EndOfStreamException();

        totalRead += bytesRead;
      }

      return buffer;
    }

    /// <summary>
    /// Reads bytes from the current stream into the provided span until the span is filled.
    /// </summary>
    /// <param name="buffer">The span to read bytes into.</param>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before filling the span.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadExactly(Span<byte> buffer) {
      ArgumentNullException.ThrowIfNull(@this);

      var totalRead = 0;
      while (totalRead < buffer.Length) {
        // Use the array-based Read method for compatibility
        var remaining = buffer.Length - totalRead;
        var tempBuffer = new byte[Math.Min(remaining, 4096)];
        var bytesRead = @this.Read(tempBuffer, 0, tempBuffer.Length);
        if (bytesRead == 0)
          throw new EndOfStreamException();

        tempBuffer.AsSpan(0, bytesRead).CopyTo(buffer[totalRead..]);
        totalRead += bytesRead;
      }
    }
  }
}

#endif
