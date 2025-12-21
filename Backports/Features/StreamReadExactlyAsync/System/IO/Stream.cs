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

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class StreamPolyfills {

  extension(Stream @this) {

    /// <summary>
    /// Asynchronously reads bytes from the current stream, advances the position within the stream until the <paramref name="buffer"/> is filled, and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The buffer to write the data into.</param>
    /// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.</param>
    /// <param name="count">The number of bytes to be read from the current stream.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value task that represents the asynchronous read operation.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before filling the buffer.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask ReadExactlyAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(buffer);
      ArgumentOutOfRangeException.ThrowIfNegative(offset);
      ArgumentOutOfRangeException.ThrowIfNegative(count);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + count, buffer.Length);

      var totalRead = 0;
      while (totalRead < count) {
        var read = await @this.ReadAsync(buffer, offset + totalRead, count - totalRead, cancellationToken).ConfigureAwait(false);
        if (read == 0)
          throw new EndOfStreamException();
        totalRead += read;
      }
    }

    /// <summary>
    /// Asynchronously reads at least a minimum number of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The buffer to write the data into.</param>
    /// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.</param>
    /// <param name="minimumBytes">The minimum number of bytes to read into the buffer.</param>
    /// <param name="throwOnEndOfStream">
    /// <see langword="true"/> to throw an <see cref="EndOfStreamException"/> when the end of the stream is reached before reading <paramref name="minimumBytes"/>;
    /// <see langword="false"/> to return without throwing.
    /// </param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value task that represents the asynchronous read operation. The value contains the total number of bytes read into the buffer.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before reading <paramref name="minimumBytes"/> bytes and <paramref name="throwOnEndOfStream"/> is <see langword="true"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<int> ReadAtLeastAsync(byte[] buffer, int offset, int minimumBytes, bool throwOnEndOfStream = true, CancellationToken cancellationToken = default) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(buffer);
      ArgumentOutOfRangeException.ThrowIfNegative(offset);
      ArgumentOutOfRangeException.ThrowIfNegative(minimumBytes);

      var maxCount = buffer.Length - offset;
      ArgumentOutOfRangeException.ThrowIfGreaterThan(minimumBytes, maxCount);

      var totalRead = 0;
      while (totalRead < minimumBytes) {
        var read = await @this.ReadAsync(buffer, offset + totalRead, maxCount - totalRead, cancellationToken).ConfigureAwait(false);
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
    /// Asynchronously reads bytes from the current stream, advances the position within the stream until the <paramref name="buffer"/> is filled, and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The region of memory to write the data into.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value task that represents the asynchronous read operation.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before filling the buffer.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask ReadExactlyAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
      Against.ThisIsNull(@this);

      var totalRead = 0;
      while (totalRead < buffer.Length) {
        var read = await @this.ReadAsync(buffer[totalRead..], cancellationToken).ConfigureAwait(false);
        if (read == 0)
          throw new EndOfStreamException();
        totalRead += read;
      }
    }

    /// <summary>
    /// Asynchronously reads at least a minimum number of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The region of memory to write the data into.</param>
    /// <param name="minimumBytes">The minimum number of bytes to read into the buffer.</param>
    /// <param name="throwOnEndOfStream">
    /// <see langword="true"/> to throw an <see cref="EndOfStreamException"/> when the end of the stream is reached before reading <paramref name="minimumBytes"/>;
    /// <see langword="false"/> to return without throwing.
    /// </param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value task that represents the asynchronous read operation. The value contains the total number of bytes read into the buffer.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before reading <paramref name="minimumBytes"/> bytes and <paramref name="throwOnEndOfStream"/> is <see langword="true"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<int> ReadAtLeastAsync(Memory<byte> buffer, int minimumBytes, bool throwOnEndOfStream = true, CancellationToken cancellationToken = default) {
      Against.ThisIsNull(@this);
      ArgumentOutOfRangeException.ThrowIfNegative(minimumBytes);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(minimumBytes, buffer.Length);

      var totalRead = 0;
      while (totalRead < minimumBytes) {
        var read = await @this.ReadAsync(buffer[totalRead..], cancellationToken).ConfigureAwait(false);
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
