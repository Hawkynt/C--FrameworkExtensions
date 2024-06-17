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

#if !SUPPORTS_STREAM_ASYNC && SUPPORTS_ASYNC
using System.Threading.Tasks;
using System.Threading;

namespace System.IO;

public static partial class StreamPolyfills {

  public static Task<int> ReadAsync(this Stream @this, byte[] buffer, int offset, int count) => ReadAsync(@this, buffer, offset, count, CancellationToken.None);

  public static Task<int> ReadAsync(this Stream @this, byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
    if (@this == null)
      throw new NullReferenceException();
    if (!@this.CanRead)
      throw new InvalidOperationException("Can not read source");
    if (offset < 0)
      throw new ArgumentOutOfRangeException(nameof(offset));
    if (count < 0 || count > buffer.Length - offset)
      throw new ArgumentOutOfRangeException(nameof(count));

    return Invoke(@this, buffer, offset, count, cancellationToken);

    static Task<int> Invoke(Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
      var tcs = new TaskCompletionSource<int>();
      if (cancellationToken.IsCancellationRequested) {
        tcs.TrySetCanceled();
        return tcs.Task;
      }

      try {
        cancellationToken.Register(OnCancellationRequested);
        stream.BeginRead(buffer, offset, count, OnReadComplete, (stream, tcs));
      } catch (Exception ex) {
        tcs.TrySetException(ex);
      }

      return tcs.Task;

      void OnCancellationRequested() {
        try {
          tcs.TrySetCanceled();
        } catch (Exception ex) {
          tcs.TrySetException(ex);
        }
      }

      static void OnReadComplete(IAsyncResult ar) {
        var (stream, tcs) = ((Stream, TaskCompletionSource<int>))ar.AsyncState;
        try {
          var bytesRead = stream.EndRead(ar);
          tcs.TrySetResult(bytesRead);
        } catch (Exception ex) {
          tcs.TrySetException(ex);
        }
      }

    }
  }

  public static Task WriteAsync(this Stream @this, byte[] buffer, int offset, int count) => WriteAsync(@this, buffer, offset, count, CancellationToken.None);

  public static Task WriteAsync(this Stream @this, byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
    if (@this == null)
      throw new NullReferenceException();
    if (!@this.CanWrite)
      throw new InvalidOperationException("Can not write destination");
    if (offset < 0)
      throw new ArgumentOutOfRangeException(nameof(offset));
    if (count < 0 || count > buffer.Length - offset)
      throw new ArgumentOutOfRangeException(nameof(count));

    return Invoke(@this, buffer, offset, count, cancellationToken);

    static Task<int> Invoke(Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
      var tcs = new TaskCompletionSource<int>();
      if (cancellationToken.IsCancellationRequested) {
        tcs.TrySetCanceled();
        return tcs.Task;
      }

      try {
        cancellationToken.Register(OnCancellationRequested);
        stream.BeginWrite(buffer, offset, count, OnWriteComplete, (stream, tcs));
      } catch (Exception ex) {
        tcs.TrySetException(ex);
      }

      return tcs.Task;

      void OnCancellationRequested() {
        try {
          tcs.TrySetCanceled();
        } catch (Exception ex) {
          tcs.TrySetException(ex);
        }
      }

      static void OnWriteComplete(IAsyncResult ar) {
        var (stream, tcs) = ((Stream, TaskCompletionSource<int>))ar.AsyncState;
        try {
          var bytesRead = stream.EndRead(ar);
          tcs.TrySetResult(bytesRead);
        } catch (Exception ex) {
          tcs.TrySetException(ex);
        }
      }

    }
  }

  public static Task CopyToAsync(this Stream @this, Stream destination, int bufferSize, CancellationToken cancellationToken) {
    if (@this == null) 
      throw new NullReferenceException();
    if (destination == null)
      throw new ArgumentNullException(nameof(destination));
    if (bufferSize <= 0)
      throw new ArgumentOutOfRangeException(nameof(bufferSize));
    if (!@this.CanRead)
      throw new InvalidOperationException("Can not read source");
    if (!destination.CanWrite)
      throw new InvalidOperationException("Can not write destination");

    return Invoke(@this, destination, bufferSize, cancellationToken);

    static async Task Invoke(Stream source, Stream target, int bufferSize, CancellationToken token) {
      var currentBuffer = new byte[bufferSize];
      var nextBuffer = new byte[bufferSize];

      var bytesRead = await source.ReadAsync(currentBuffer, 0, bufferSize, token).ConfigureAwait(false);
      while (bytesRead > 0) {
        var nextReadTask = source.ReadAsync(nextBuffer, 0, bufferSize, token).ConfigureAwait(false);
        await target.WriteAsync(currentBuffer, 0, bytesRead, token).ConfigureAwait(false);
        bytesRead = await nextReadTask;

        (currentBuffer, nextBuffer) = (nextBuffer, currentBuffer);
      }
    }
  }

}

#endif
