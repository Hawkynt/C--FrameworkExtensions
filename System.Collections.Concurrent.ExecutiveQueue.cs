#region (c)2010-2020 Hawkynt
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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
namespace System.Collections.Concurrent {
  /// <summary>
  /// A thread-safe queue which executes a certain callback whenever an element is added
  /// </summary>
  /// <typeparam name="TItem">The type of items contained in this queue.</typeparam>
  internal class ExecutiveQueue<TItem> : IQueue<TItem> {
    private readonly ConcurrentQueue<TItem> _queue = new ConcurrentQueue<TItem>();
    private readonly Action<TItem> _callback;
    private readonly Action<TItem, Exception> _exceptionCallback;
    private readonly bool _isAsync;
    private readonly int _maxItems;
    private int _isRunning;
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutiveQueue&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="callback">The callback that will get executed.</param>
    /// <param name="isAsync">if set to <c>true</c> the callback is executed asnychronousely.</param>
    /// <param name="exceptionCallback">The exception callback.</param>
    /// <param name="maxItems">The max items; if more items are enqueued, the queue will be block.</param>
    public ExecutiveQueue(Action<TItem> callback, bool isAsync = true, Action<TItem, Exception> exceptionCallback = null, int maxItems = 1000000) {
      Contract.Requires(callback != null);
      Contract.Requires(maxItems > 0);
      this._isAsync = isAsync;
      this._callback = callback;
      this._exceptionCallback = exceptionCallback;
      this._maxItems = maxItems;
    }

    #region iQueue<T> Member
    /// <summary>
    /// Clears this queue.
    /// </summary>
    public void Clear() {
      TItem dummy;
      while (this._queue.TryDequeue(out dummy))
        ;
    }

    /// <summary>
    /// Dequeues an item or blocks until there is one.
    /// </summary>
    /// <returns>the item</returns>
    public TItem Dequeue() {
      TItem result;
      while (!this._queue.TryDequeue(out result))
        ;
      return (result);
    }

    /// <summary>
    /// Enqueues the specified item and starts executing the callback.
    /// If a callback is already running, this one gets queued up and is executed immediately after the previous call is completed.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Enqueue(TItem item) {
      var queue = this._queue;
      var callback = this._callback;

      if (!this._isAsync) {
        try {
          callback(item);
        } catch (Exception e) {
          var exceptionCallback = this._exceptionCallback;
          if (exceptionCallback == null)
            throw;
          exceptionCallback(item, e);
        }
        return;
      }

      while (queue.Count >= this._maxItems)
        Thread.Sleep(5);
      queue.Enqueue(item);

      // if already running just return
      if (Thread.VolatileRead(ref this._isRunning) != 0)
        return;

      try {
        Action call = () => _Worker(queue, callback, this._exceptionCallback, ref this._isRunning);
        call.BeginInvoke(call.EndInvoke, null);
      } catch {
        // in case we're crashing
        Interlocked.CompareExchange(ref this._isRunning, 0, 1);
        throw;
      }
    }


    /// <summary>
    /// The worker thread.
    /// </summary>
    /// <param name="queue">The queue.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="exceptionCallback">The exception callback, if any.</param>
    /// <param name="isRunning">The reference to the isRunning flag.</param>
    private static void _Worker(ConcurrentQueue<TItem> queue, Action<TItem> callback, Action<TItem, Exception> exceptionCallback, ref int isRunning) {
      Contract.Requires(queue != null);
      Contract.Requires(callback != null);

      // in case the value has alread changed
      if (Interlocked.CompareExchange(ref isRunning, 1, 0) != 0)
        return;

      try {

        TItem current;
        while (queue.TryDequeue(out current)) {
          try {
            callback(current);
          } catch (Exception e) {
            if (exceptionCallback == null)
              throw;
            exceptionCallback(current, e);
          }
        }

      } finally {
        // reset flag when done
        Interlocked.CompareExchange(ref isRunning, 0, 1);
      }
    }

    /// <summary>
    /// Tries to dequeue.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public bool TryDequeue(out TItem item) {
      return (this._queue.TryDequeue(out item));
    }

    /// <summary>
    /// Gets the number of contained items.
    /// </summary>
    /// <value>The number of items.</value>
    public int Count {
      get {
        return (this._queue.Count);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
    public bool IsEmpty {
      get {
        return (this._queue.IsEmpty);
      }
    }

    /// <summary>
    /// Gets the current queue content as an array.
    /// </summary>
    /// <returns>An array containig the queues elements.</returns>
    public TItem[] ToArray() {
      return (this._queue.ToArray());
    }

    #endregion
  }
}