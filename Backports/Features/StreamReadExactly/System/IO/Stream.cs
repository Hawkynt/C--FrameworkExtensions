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

#if !SUPPORTS_STREAM_READEXACTLY

using Guard;

namespace System.IO;

public static partial class StreamPolyfills {

  extension(Stream @this) {

    /// <summary>
    /// Reads bytes from the current stream and advances the position within the stream until the <paramref name="buffer"/> is filled.
    /// </summary>
    /// <param name="buffer">A region of memory. When this method returns, the contents of this region are replaced by the bytes read from the current stream.</param>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before filling the <paramref name="buffer"/>.</exception>
    public void ReadExactly(byte[] buffer, int offset, int count) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(buffer);
      ArgumentOutOfRangeException.ThrowIfNegative(offset);
      ArgumentOutOfRangeException.ThrowIfNegative(count);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + count, buffer.Length);

      var totalRead = 0;
      while (totalRead < count) {
        var read = @this.Read(buffer, offset + totalRead, count - totalRead);
        if (read == 0)
          throw new EndOfStreamException();
        totalRead += read;
      }
    }

    /// <summary>
    /// Reads at least a minimum number of bytes from the current stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <param name="buffer">A region of memory. When this method returns, the contents of this region are replaced by the bytes read from the current stream.</param>
    /// <param name="minimumBytes">The minimum number of bytes to read into the buffer.</param>
    /// <returns>The total number of bytes read into the buffer. This is guaranteed to be at least <paramref name="minimumBytes"/>.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before reading <paramref name="minimumBytes"/> bytes.</exception>
    public int ReadAtLeast(byte[] buffer, int offset, int minimumBytes, bool throwOnEndOfStream = true) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(buffer);
      ArgumentOutOfRangeException.ThrowIfNegative(offset);
      ArgumentOutOfRangeException.ThrowIfNegative(minimumBytes);

      var maxCount = buffer.Length - offset;
      ArgumentOutOfRangeException.ThrowIfGreaterThan(minimumBytes, maxCount);

      var totalRead = 0;
      while (totalRead < minimumBytes) {
        var read = @this.Read(buffer, offset + totalRead, maxCount - totalRead);
        if (read == 0) {
          if (throwOnEndOfStream)
            throw new EndOfStreamException();
          break;
        }
        totalRead += read;
      }

      return totalRead;
    }

    /// <summary>
    /// Reads bytes from the current stream and advances the position within the stream until the <paramref name="buffer"/> is filled.
    /// </summary>
    /// <param name="buffer">A region of memory. When this method returns, the contents of this region are replaced by the bytes read from the current stream.</param>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before filling the <paramref name="buffer"/>.</exception>
    public void ReadExactly(Span<byte> buffer) {
      Against.ThisIsNull(@this);

      var totalRead = 0;
      while (totalRead < buffer.Length) {
        var read = @this.Read(buffer[totalRead..]);
        if (read == 0)
          throw new EndOfStreamException();
        totalRead += read;
      }
    }

    /// <summary>
    /// Reads at least a minimum number of bytes from the current stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <param name="buffer">A region of memory. When this method returns, the contents of this region are replaced by the bytes read from the current stream.</param>
    /// <param name="minimumBytes">The minimum number of bytes to read into the buffer.</param>
    /// <param name="throwOnEndOfStream">
    /// <see langword="true"/> to throw an <see cref="EndOfStreamException"/> when the end of the stream is reached before reading <paramref name="minimumBytes"/>;
    /// <see langword="false"/> to return without throwing.
    /// </param>
    /// <returns>The total number of bytes read into the buffer. This is guaranteed to be at least <paramref name="minimumBytes"/> unless <paramref name="throwOnEndOfStream"/> is <see langword="false"/>.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before reading <paramref name="minimumBytes"/> bytes and <paramref name="throwOnEndOfStream"/> is <see langword="true"/>.</exception>
    public int ReadAtLeast(Span<byte> buffer, int minimumBytes, bool throwOnEndOfStream = true) {
      Against.ThisIsNull(@this);
      ArgumentOutOfRangeException.ThrowIfNegative(minimumBytes);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(minimumBytes, buffer.Length);

      var totalRead = 0;
      while (totalRead < minimumBytes) {
        var read = @this.Read(buffer[totalRead..]);
        if (read == 0) {
          if (throwOnEndOfStream)
            throw new EndOfStreamException();
          break;
        }
        totalRead += read;
      }

      return totalRead;
    }

  }

}

#endif
