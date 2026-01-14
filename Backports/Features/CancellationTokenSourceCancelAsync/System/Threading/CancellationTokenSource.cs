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

// CancellationTokenSource.CancelAsync was added in .NET 8.0
#if !SUPPORTS_CANCELLATIONTOKENSOURCE_CANCELASYNC

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

public static partial class CancellationTokenSourcePolyfills {

  extension(CancellationTokenSource @this) {

    /// <summary>
    /// Communicates a request for cancellation asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous cancel operation.</returns>
    /// <exception cref="ObjectDisposedException">The token source has been disposed.</exception>
    /// <remarks>
    /// <para>
    /// The associated <see cref="CancellationToken"/> will be notified of the cancellation
    /// and will transition to a state where <see cref="CancellationToken.IsCancellationRequested"/>
    /// returns <see langword="true"/>.
    /// </para>
    /// <para>
    /// Any callbacks or cancelable operations registered with the <see cref="CancellationToken"/>
    /// will be executed.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task CancelAsync() {
      if (@this is null)
        throw new NullReferenceException();

      // In older .NET versions, we simply call Cancel() synchronously
      // and return a completed task. The actual cancellation callbacks
      // are executed synchronously on the calling thread.
      @this.Cancel();

#if SUPPORTS_TASK_COMPLETEDTASK
      return Task.CompletedTask;
#else
      return _completedTask;
#endif
    }

  }

#if !SUPPORTS_TASK_COMPLETEDTASK
  private static readonly Task _completedTask = _CreateCompletedTask();

  private static Task _CreateCompletedTask() {
    var tcs = new TaskCompletionSource<object?>();
    tcs.SetResult(null);
    return tcs.Task;
  }
#endif

}

#endif
