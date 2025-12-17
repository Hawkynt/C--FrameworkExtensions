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

// Task.Delay and Task.WhenAny were added in .NET 4.5
// This provides polyfills for .NET 4.0 where Task exists but these methods don't
// Also works on .NET 2.0/3.5 with our Task polyfill
#if !SUPPORTS_TASK_RUN

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading.Tasks;

public static partial class TaskPolyfills {

  extension(Task) {

    /// <summary>
    /// Creates a task that completes after a specified time interval.
    /// </summary>
    /// <param name="delay">The time span to wait before completing the returned task.</param>
    /// <returns>A task that represents the time delay.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task Delay(TimeSpan delay)
      => Delay((int)delay.TotalMilliseconds, CancellationToken.None);

    /// <summary>
    /// Creates a task that completes after a specified number of milliseconds.
    /// </summary>
    /// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned task.</param>
    /// <returns>A task that represents the time delay.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task Delay(int millisecondsDelay)
      => Delay(millisecondsDelay, CancellationToken.None);

    /// <summary>
    /// Creates a task that completes after a specified time interval.
    /// </summary>
    /// <param name="delay">The time span to wait before completing the returned task.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the time delay.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task Delay(TimeSpan delay, CancellationToken cancellationToken)
      => Delay((int)delay.TotalMilliseconds, cancellationToken);

    /// <summary>
    /// Creates a task that completes after a specified number of milliseconds.
    /// </summary>
    /// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned task.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the time delay.</returns>
    public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken) {
      ArgumentOutOfRangeException.ThrowIfLessThan(millisecondsDelay, -1);

      if (cancellationToken.IsCancellationRequested)
        return _CreateCanceledTask();

      if (millisecondsDelay == 0)
        return _CompletedTask;

      var tcs = new TaskCompletionSource<object>();
      Timer timer = null;
      CancellationTokenRegistration registration = default;

      timer = new Timer(
        _ => {
          timer?.Dispose();
          registration.Dispose();
          tcs.TrySetResult(null);
        },
        null,
        millisecondsDelay,
        Timeout.Infinite
      );

      if (cancellationToken.CanBeCanceled)
        registration = cancellationToken.Register(
          () => {
            timer?.Dispose();
            tcs.TrySetCanceled();
          }
        );

      return tcs.Task;
    }

    /// <summary>
    /// Creates a task that will complete when any of the supplied tasks have completed.
    /// </summary>
    /// <param name="tasks">The tasks to wait on for completion.</param>
    /// <returns>A task that represents the completion of one of the supplied tasks.</returns>
    public static Task<Task> WhenAny(params Task[] tasks) {
      ArgumentNullException.ThrowIfNull(tasks);
      if (tasks.Length == 0)
        throw new ArgumentException("At least one task must be provided.", nameof(tasks));

      var tcs = new TaskCompletionSource<Task>();

      foreach (var task in tasks)
        task.ContinueWith(
          t => tcs.TrySetResult(t),
          CancellationToken.None,
          TaskContinuationOptions.ExecuteSynchronously,
          TaskScheduler.Default
        );

      return tcs.Task;
    }

    /// <summary>
    /// Creates a task that will complete when all of the supplied tasks have completed.
    /// </summary>
    /// <param name="tasks">The tasks to wait on for completion.</param>
    /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
    public static Task WhenAll(params Task[] tasks) {
      ArgumentNullException.ThrowIfNull(tasks);

      if (tasks.Length == 0)
        return _CompletedTask;

      var tcs = new TaskCompletionSource<object>();
      var remaining = tasks.Length;
      var exceptions = new System.Collections.Generic.List<Exception>();
      var lockObj = new object();

      foreach (var task in tasks)
        task.ContinueWith(
          t => {
            lock (lockObj) {
              if (t.IsFaulted && t.Exception != null)
                exceptions.AddRange(t.Exception.InnerExceptions);

              if (Interlocked.Decrement(ref remaining) == 0)
                if (exceptions.Count > 0)
                  tcs.TrySetException(exceptions);
                else
                  tcs.TrySetResult(null);
            }
          },
          CancellationToken.None,
          TaskContinuationOptions.ExecuteSynchronously,
          TaskScheduler.Default
        );

      return tcs.Task;
    }

    /// <summary>
    /// Creates a task that will complete when all of the supplied tasks have completed.
    /// </summary>
    /// <typeparam name="TResult">The type of the completed task.</typeparam>
    /// <param name="tasks">The tasks to wait on for completion.</param>
    /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
    public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks) {
      ArgumentNullException.ThrowIfNull(tasks);

      if (tasks.Length == 0)
        return FromResult(Array.Empty<TResult>());

      var tcs = new TaskCompletionSource<TResult[]>();
      var results = new TResult[tasks.Length];
      var remaining = tasks.Length;
      var exceptions = new System.Collections.Generic.List<Exception>();
      var lockObj = new object();

      for (var i = 0; i < tasks.Length; ++i) {
        var index = i;
        tasks[i].ContinueWith(
          t => {
            lock (lockObj) {
              if (t.IsFaulted && t.Exception != null)
                exceptions.AddRange(t.Exception.InnerExceptions);
              else if (t.Status == TaskStatus.RanToCompletion)
                results[index] = t.Result;

              if (Interlocked.Decrement(ref remaining) == 0)
                if (exceptions.Count > 0)
                  tcs.TrySetException(exceptions);
                else
                  tcs.TrySetResult(results);
            }
          },
          CancellationToken.None,
          TaskContinuationOptions.ExecuteSynchronously,
          TaskScheduler.Default
        );
      }

      return tcs.Task;
    }

    /// <summary>
    /// Creates a <see cref="Task{TResult}"/> that's completed successfully with the specified result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
    /// <param name="result">The result to store into the completed task.</param>
    /// <returns>The successfully completed task.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> FromResult<TResult>(TResult result) {
      var tcs = new TaskCompletionSource<TResult>();
      tcs.SetResult(result);
      return tcs.Task;
    }

    /// <summary>
    /// Queues the specified work to run on the thread pool and returns a <see cref="Task"/> object that represents that work.
    /// </summary>
    /// <param name="action">The work to execute asynchronously.</param>
    /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
    public static Task Run(Action action)
      => Run(action, CancellationToken.None);

    /// <summary>
    /// Queues the specified work to run on the thread pool and returns a <see cref="Task"/> object that represents that work.
    /// </summary>
    /// <param name="action">The work to execute asynchronously.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
    /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
    public static Task Run(Action action, CancellationToken cancellationToken) {
      ArgumentNullException.ThrowIfNull(action);

      return Task.Factory.StartNew(action, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
    }

    /// <summary>
    /// Queues the specified work to run on the thread pool and returns a <see cref="Task{TResult}"/> object that represents that work.
    /// </summary>
    /// <typeparam name="TResult">The result type of the task.</typeparam>
    /// <param name="function">The work to execute asynchronously.</param>
    /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
    public static Task<TResult> Run<TResult>(Func<TResult> function)
      => Run(function, CancellationToken.None);

    /// <summary>
    /// Queues the specified work to run on the thread pool and returns a <see cref="Task{TResult}"/> object that represents that work.
    /// </summary>
    /// <typeparam name="TResult">The result type of the task.</typeparam>
    /// <param name="function">The work to execute asynchronously.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
    /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
    public static Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken) {
      ArgumentNullException.ThrowIfNull(function);

      return Task.Factory.StartNew(function, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
    }

  }

  private static readonly Task _CompletedTask = _CreateCompletedTask();

  private static Task _CreateCompletedTask() {
    var tcs = new TaskCompletionSource<object>();
    tcs.SetResult(null);
    return tcs.Task;
  }

  private static Task _CreateCanceledTask() {
    var tcs = new TaskCompletionSource<object>();
    tcs.SetCanceled();
    return tcs.Task;
  }

}

#endif
