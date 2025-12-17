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

#if !SUPPORTS_TASK_WAITASYNC

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading.Tasks;

public static partial class TaskPolyfills {

  extension(Task @this) {

    /// <summary>
    /// Gets a <see cref="Task"/> that will complete when this <see cref="Task"/> completes or when the specified timeout expires.
    /// </summary>
    /// <param name="timeout">The timeout after which the <see cref="Task"/> should be faulted with a <see cref="TimeoutException"/> if it hasn't otherwise completed.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous wait.</returns>
    /// <exception cref="TimeoutException">The timeout expired before the task completed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WaitAsync(TimeSpan timeout)
      => @this.WaitAsync(timeout, CancellationToken.None);

    /// <summary>
    /// Gets a <see cref="Task"/> that will complete when this <see cref="Task"/> completes or when the specified <see cref="CancellationToken"/> has cancellation requested.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for a cancellation request.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous wait.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WaitAsync(CancellationToken cancellationToken)
      => @this.WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken);

    /// <summary>
    /// Gets a <see cref="Task"/> that will complete when this <see cref="Task"/> completes, when the specified timeout expires, or when the specified <see cref="CancellationToken"/> has cancellation requested.
    /// </summary>
    /// <param name="timeout">The timeout after which the <see cref="Task"/> should be faulted with a <see cref="TimeoutException"/> if it hasn't otherwise completed.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for a cancellation request.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous wait.</returns>
    /// <exception cref="TimeoutException">The timeout expired before the task completed.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      var delayTask = Task.Delay(timeout, cts.Token);
      var completedTask = await Task.WhenAny(@this, delayTask).ConfigureAwait(false);

      if (completedTask == delayTask) {
        cancellationToken.ThrowIfCancellationRequested();
        throw new TimeoutException("The operation has timed out.");
      }

      cts.Cancel();
      await @this.ConfigureAwait(false);
    }

  }

  extension<TResult>(Task<TResult> @this) {

    /// <summary>
    /// Gets a <see cref="Task{TResult}"/> that will complete when this <see cref="Task{TResult}"/> completes or when the specified timeout expires.
    /// </summary>
    /// <param name="timeout">The timeout after which the <see cref="Task{TResult}"/> should be faulted with a <see cref="TimeoutException"/> if it hasn't otherwise completed.</param>
    /// <returns>The <see cref="Task{TResult}"/> representing the asynchronous wait.</returns>
    /// <exception cref="TimeoutException">The timeout expired before the task completed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResult> WaitAsync(TimeSpan timeout)
      => @this.WaitAsync(timeout, CancellationToken.None);

    /// <summary>
    /// Gets a <see cref="Task{TResult}"/> that will complete when this <see cref="Task{TResult}"/> completes or when the specified <see cref="CancellationToken"/> has cancellation requested.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for a cancellation request.</param>
    /// <returns>The <see cref="Task{TResult}"/> representing the asynchronous wait.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResult> WaitAsync(CancellationToken cancellationToken)
      => @this.WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken);

    /// <summary>
    /// Gets a <see cref="Task{TResult}"/> that will complete when this <see cref="Task{TResult}"/> completes, when the specified timeout expires, or when the specified <see cref="CancellationToken"/> has cancellation requested.
    /// </summary>
    /// <param name="timeout">The timeout after which the <see cref="Task{TResult}"/> should be faulted with a <see cref="TimeoutException"/> if it hasn't otherwise completed.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for a cancellation request.</param>
    /// <returns>The <see cref="Task{TResult}"/> representing the asynchronous wait.</returns>
    /// <exception cref="TimeoutException">The timeout expired before the task completed.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task<TResult> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      var delayTask = Task.Delay(timeout, cts.Token);
      var completedTask = await Task.WhenAny(@this, delayTask).ConfigureAwait(false);

      if (completedTask == delayTask) {
        cancellationToken.ThrowIfCancellationRequested();
        throw new TimeoutException("The operation has timed out.");
      }

      cts.Cancel();
      return await @this.ConfigureAwait(false);
    }

  }

}

#endif
