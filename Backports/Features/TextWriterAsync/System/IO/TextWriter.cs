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
using System.Threading.Tasks;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class TextWriterPolyfills {

  extension(TextWriter @this) {

    /// <summary>
    /// Writes a string to the text stream asynchronously.
    /// </summary>
    /// <param name="value">The string to write. If value is null, nothing is written to the text stream.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WriteAsync(string? value) {
      Against.ThisIsNull(@this);
      return Task.Factory.StartNew(() => @this.Write(value));
    }

    /// <summary>
    /// Writes a character to the text stream asynchronously.
    /// </summary>
    /// <param name="value">The character to write to the text stream.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WriteAsync(char value) {
      Against.ThisIsNull(@this);
      return Task.Factory.StartNew(() => @this.Write(value));
    }

    /// <summary>
    /// Writes a subarray of characters to the text stream asynchronously.
    /// </summary>
    /// <param name="buffer">The character array to write data from.</param>
    /// <param name="index">The character position in the buffer at which to start retrieving data.</param>
    /// <param name="count">The number of characters to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WriteAsync(char[] buffer, int index, int count) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(buffer);
      ArgumentOutOfRangeException.ThrowIfNegative(index);
      ArgumentOutOfRangeException.ThrowIfNegative(count);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index + count, buffer.Length);
      return Task.Factory.StartNew(() => @this.Write(buffer, index, count));
    }

    /// <summary>
    /// Writes a line terminator asynchronously to the text stream.
    /// </summary>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WriteLineAsync() {
      Against.ThisIsNull(@this);
      return Task.Factory.StartNew(@this.WriteLine);
    }

    /// <summary>
    /// Writes a string followed by a line terminator asynchronously to the text stream.
    /// </summary>
    /// <param name="value">The string to write. If the value is null, only a line terminator is written.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WriteLineAsync(string? value) {
      Against.ThisIsNull(@this);
      return Task.Factory.StartNew(() => @this.WriteLine(value));
    }

    /// <summary>
    /// Writes a character followed by a line terminator asynchronously to the text stream.
    /// </summary>
    /// <param name="value">The character to write to the text stream.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WriteLineAsync(char value) {
      Against.ThisIsNull(@this);
      return Task.Factory.StartNew(() => @this.WriteLine(value));
    }

    /// <summary>
    /// Writes a subarray of characters followed by a line terminator asynchronously to the text stream.
    /// </summary>
    /// <param name="buffer">The character array to write data from.</param>
    /// <param name="index">The character position in the buffer at which to start retrieving data.</param>
    /// <param name="count">The number of characters to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WriteLineAsync(char[] buffer, int index, int count) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(buffer);
      ArgumentOutOfRangeException.ThrowIfNegative(index);
      ArgumentOutOfRangeException.ThrowIfNegative(count);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index + count, buffer.Length);
      return Task.Factory.StartNew(() => {
        @this.Write(buffer, index, count);
        @this.WriteLine();
      });
    }

    /// <summary>
    /// Asynchronously clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
    /// </summary>
    /// <returns>A task that represents the asynchronous flush operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task FlushAsync() {
      Against.ThisIsNull(@this);
      return Task.Factory.StartNew(@this.Flush);
    }

  }

}

#endif
