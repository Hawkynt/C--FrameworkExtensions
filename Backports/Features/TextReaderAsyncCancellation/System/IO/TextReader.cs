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

#if !SUPPORTS_TEXTREADER_ASYNC_CANCELLATION

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class TextReaderPolyfills {

  extension(TextReader @this) {

    /// <summary>
    /// Reads a line of characters asynchronously and returns the data as a string.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value task that represents the asynchronous read operation. The value of the TResult parameter contains the next line from the text reader, or is null if all of the characters have been read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken) {
      Against.ThisIsNull(@this);
      if (cancellationToken.IsCancellationRequested)
        throw new TaskCanceledException();
      
      // ReSharper disable once MethodSupportsCancellation
      return await @this.ReadLineAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Reads all characters from the current position to the end of the text reader asynchronously and returns them as one string.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value task that represents the asynchronous read operation. The value of the TResult parameter contains a string with the characters from the current position to the end of the text reader.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<string> ReadToEndAsync(CancellationToken cancellationToken) {
      Against.ThisIsNull(@this);
      if (cancellationToken.IsCancellationRequested)
        throw new TaskCanceledException();
      
      // ReSharper disable once MethodSupportsCancellation
      return await @this.ReadToEndAsync().ConfigureAwait(false);
    }

  }

}

#endif
