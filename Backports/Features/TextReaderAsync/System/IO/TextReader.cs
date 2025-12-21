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

#if !SUPPORTS_STREAM_ASYNC

using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class TextReaderPolyfills {

  extension(TextReader @this) {

    /// <summary>
    /// Reads a line of characters asynchronously and returns the data as a string.
    /// </summary>
    /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains the next line from the text reader, or is null if all of the characters have been read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<string?> ReadLineAsync() {
      Against.ThisIsNull(@this);
      return Task.Factory.StartNew<string?>(@this.ReadLine);
    }

    /// <summary>
    /// Reads all characters from the current position to the end of the text reader asynchronously and returns them as one string.
    /// </summary>
    /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains a string with the characters from the current position to the end of the text reader.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<string> ReadToEndAsync() {
      Against.ThisIsNull(@this);
      return Task.Factory.StartNew(@this.ReadToEnd);
    }

    /// <summary>
    /// Reads a specified maximum number of characters from the current text reader asynchronously and writes the data to a buffer, beginning at the specified index.
    /// </summary>
    /// <param name="buffer">The buffer to receive the characters.</param>
    /// <param name="index">The position in buffer at which to begin writing.</param>
    /// <param name="count">The maximum number of characters to read.</param>
    /// <returns>A task that represents the asynchronous read operation. The value contains the number of characters that have been read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> ReadAsync(char[] buffer, int index, int count) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(buffer);
      ArgumentOutOfRangeException.ThrowIfNegative(index);
      ArgumentOutOfRangeException.ThrowIfNegative(count);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index + count, buffer.Length);
      return Task.Factory.StartNew(() => @this.Read(buffer, index, count));
    }

    /// <summary>
    /// Reads a specified maximum number of characters from the current text reader asynchronously and writes the data to a buffer, beginning at the specified index.
    /// </summary>
    /// <param name="buffer">The buffer to receive the characters.</param>
    /// <param name="index">The position in buffer at which to begin writing.</param>
    /// <param name="count">The maximum number of characters to read.</param>
    /// <returns>A task that represents the asynchronous read operation. The value contains the number of characters that have been read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> ReadBlockAsync(char[] buffer, int index, int count) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(buffer);
      ArgumentOutOfRangeException.ThrowIfNegative(index);
      ArgumentOutOfRangeException.ThrowIfNegative(count);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index + count, buffer.Length);
      return Task.Factory.StartNew(() => @this.ReadBlock(buffer, index, count));
    }

  }

}

#endif
