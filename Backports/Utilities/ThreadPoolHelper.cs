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
//

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Utilities;

internal static class ThreadPoolHelper {

  private const int _MAX_RETRIES = 10;
  private const int _INITIAL_BACKOFF_MS = 1;
  private const int _IDLE_TIMEOUT_MS = 5000;

  private static readonly ConcurrentQueue<WorkItem> _workQueue = new();
  private static int _idleWorkerCount;
  private static readonly AutoResetEvent _workAvailable = new(false);

  private readonly struct WorkItem(WaitCallback callback, object? state) {
    public readonly WaitCallback Callback = callback;
    public readonly object? State = state;
  }

  /// <summary>
  /// Queues a work item to the thread pool, with retry logic and fallback to dedicated thread.
  /// On .NET 3.5, QueueUserWorkItem can return false when the pool is saturated.
  /// This method handles that by retrying with exponential backoff and falling back to a
  /// pool of reusable worker threads that stay alive for 5 seconds after becoming idle.
  /// </summary>
  /// <param name="callback">The callback to execute.</param>
  /// <param name="state">The state to pass to the callback.</param>
  public static void QueueUserWorkItem(WaitCallback callback, object? state) {
    ArgumentNullException.ThrowIfNull(callback);

    // Try to queue to ThreadPool with retries
    var backoffMs = _INITIAL_BACKOFF_MS;
    for (var i = 0; i < _MAX_RETRIES; ++i) {
      if (ThreadPool.QueueUserWorkItem(callback, state))
        return;

      // ThreadPool is saturated, wait before retrying
      Thread.Sleep(backoffMs);
      backoffMs = Math.Min(backoffMs * 2, 100); // Cap at 100ms
    }

    // Fallback: use our own worker pool
    _QueueToFallbackPool(callback, state);
  }

  /// <summary>
  /// Queues a work item to the thread pool without state, with retry logic and fallback.
  /// </summary>
  /// <param name="callback">The callback to execute.</param>
  public static void QueueUserWorkItem(WaitCallback callback)
    => QueueUserWorkItem(callback, null);

  private static void _QueueToFallbackPool(WaitCallback callback, object? state) {
    _workQueue.Enqueue(new(callback, state));

    // Check if we need to spawn a new worker
    if (Interlocked.CompareExchange(ref _idleWorkerCount, 0, 0) == 0) {
      // No idle workers, spawn a new one
      var thread = new Thread(_WorkerLoop) {
        IsBackground = true,
        Name = "Backports-FallbackWorker"
      };
      thread.Start();
    } else {
      // Signal an idle worker to wake up
      _workAvailable.Set();
    }
  }

  private static void _WorkerLoop() {
    for (;;) {
      // Try to get work from the queue
      if (_workQueue.TryDequeue(out var item)) {
        try {
          item.Callback(item.State);
        } catch {
          // Swallow exceptions like ThreadPool does
        }
        continue;
      }

      // No work available, mark ourselves as idle and wait
      Interlocked.Increment(ref _idleWorkerCount);

      var signaled = _workAvailable.WaitOne(_IDLE_TIMEOUT_MS);

      Interlocked.Decrement(ref _idleWorkerCount);

      if (!signaled && _workQueue.IsEmpty)
        // Timeout with no work, terminate this worker
        return;

      // Either signaled or there's work in the queue, loop back to process
    }
  }

}
