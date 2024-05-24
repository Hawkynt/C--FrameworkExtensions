#region (c)2010-2042 Hawkynt

/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

#if !SUPPORTS_STREAM_ASYNC && SUPPORTS_ASYNC
using System.Threading.Tasks;
using System.Threading;
#endif

namespace System.IO;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
public static partial class StreamPolyfills {

#if !SUPPORTS_STREAM_COPY

  /// <summary>
  /// Copies all contents from this <see cref="Stream"/> to another <see cref="Stream"/>.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="target">Target <see cref="Stream"/>.</param>
  public static void CopyTo(this Stream @this, Stream target) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    if (!@this.CanRead)
      throw new ArgumentException("Can not read",nameof(@this));
    if(!target.CanWrite)
      throw new ArgumentException("Can not write", nameof(target));
    
    var buffer = new byte[65536];
    int count;
    while ((count = @this.Read(buffer, 0, buffer.Length)) != 0)
      target.Write(buffer, 0, count);
  }

  /// <summary>
  /// Flushes the <see cref="Stream"/>.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="_">Dummy, ignored</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Flush(this Stream @this, bool _) => @this.Flush();

#endif

#if !SUPPORTS_STREAM_ASYNC && SUPPORTS_ASYNC

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
        var nextReadTask = source.ReadAsync(nextBuffer, 0, bufferSize, token);
        await target.WriteAsync(currentBuffer, 0, bytesRead, token).ConfigureAwait(false);
        bytesRead = await nextReadTask.ConfigureAwait(false);

        (currentBuffer, nextBuffer) = (nextBuffer, currentBuffer);
      }
    }
  }

#endif

}
