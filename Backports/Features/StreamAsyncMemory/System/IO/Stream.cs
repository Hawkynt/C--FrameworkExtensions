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

#if !SUPPORTS_STREAM_READ_SPAN

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class StreamPolyfills {

  extension(Stream @this) {

    /// <summary>
    /// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The region of memory to write the data into.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value task that represents the asynchronous read operation. The value of the TResult parameter contains the total number of bytes read into the buffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
      if (buffer.IsEmpty)
        return 0;

      var array = new byte[buffer.Length];
      var bytesRead = await @this.ReadAsync(array, 0, array.Length, cancellationToken).ConfigureAwait(false);
      if (bytesRead > 0)
        new ReadOnlySpan<byte>(array, 0, bytesRead).CopyTo(buffer.Span);
      return bytesRead;
    }

    /// <summary>
    /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The region of memory to write data from.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value task that represents the asynchronous write operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) {
      if (buffer.IsEmpty)
        return;

      var array = buffer.ToArray();
      await @this.WriteAsync(array, 0, array.Length, cancellationToken).ConfigureAwait(false);
    }
  }
}

#endif
