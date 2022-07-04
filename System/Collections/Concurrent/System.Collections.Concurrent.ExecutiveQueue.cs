#region (c)2010-2030 Hawkynt
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

using System.Diagnostics;
using System.Threading;

namespace System.Collections.Concurrent {
  /// <summary>
  /// A thread-safe queue which executes a certain callback whenever an element is added
  /// </summary>
  /// <typeparam name="TItem">The type of items contained in this queue.</typeparam>
  internal class ExecutiveQueue<TItem>{

    #region consts
    private const int _IDLE = 0;
    private const int _PROCESSING = 1;
    #endregion

    #region fields
    private readonly Action<TItem> _callback;
    private readonly Action<TItem, Exception> _exceptionCallback;
    private readonly bool _isAsync;
    private readonly int _maxItems;
    private readonly TimeSpan? _executionDelay;
    private readonly TimeSpan _dequeueThrottle;
    private readonly TimeSpan _overflowThrottle;
    private int _processing = _IDLE;
    #endregion

    /// <summary>
    /// Gets the items.
    /// </summary>
    private ConcurrentQueue<TItem> _Items { get; } = new ConcurrentQueue<TItem>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutiveQueue&lt;T&gt;" /> class.
    /// </summary>
    /// <param name="callback">The callback that will get executed.</param>
    /// <param name="isAsync">if set to <c>true</c> the callback is executed asnychronousely.</param>
    /// <param name="exceptionCallback">The exception callback.</param>
    /// <param name="maxItems">The max items; if more items are enqueued, the queue will be block.</param>
    /// <param name="executionDelay">The execution delay timespan</param>
    public ExecutiveQueue(Action<TItem> callback, bool isAsync = true, Action<TItem, Exception> exceptionCallback = null, int maxItems = int.MaxValue, TimeSpan? executionDelay = null,TimeSpan? dequeueThrottle=default,TimeSpan? overflowThrottle=default) {
      Debug.Assert(callback != null);
      Debug.Assert(maxItems > 0);

      this._isAsync = isAsync;
      this._callback = callback;
      this._exceptionCallback = exceptionCallback;
      this._maxItems = maxItems;
      this._executionDelay = executionDelay;
      this._dequeueThrottle = dequeueThrottle ?? TimeSpan.FromMilliseconds(5);
      this._overflowThrottle=overflowThrottle?? TimeSpan.FromMilliseconds(5);
    }

    #region iQueue<T> Member
    /// <summary>
    /// Clears this queue.
    /// </summary>
    public void Clear() {
      while (this._Items.TryDequeue(out _))
        ;
    }

    /// <summary>
    /// Dequeues an item or blocks until there is one.
    /// </summary>
    /// <returns>the item</returns>
    public TItem Dequeue() {
      TItem result;
      while (!this._Items.TryDequeue(out result))
        Thread.Sleep(this._dequeueThrottle);
      
      return result;
    }

    /// <summary>
    /// Enqueues the specified item and starts executing the callback.
    /// If a callback is already running, this one gets queued up and is executed immediately after the previous call is completed.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Enqueue(TItem item) {
      var queue = this._Items;
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

      //overflow protection
      while (queue.Count >= this._maxItems)
        Thread.Sleep(_overflowThrottle);

      queue.Enqueue(item);

      // if already running just return
      if (Thread.VolatileRead(ref this._processing) != _IDLE)
        return;

      try {
        Action call = () => _Worker(queue, callback, this._executionDelay, this._exceptionCallback, ref this._processing);
        call.BeginInvoke(call.EndInvoke, null);
      } catch {
        // in case we're crashing
        Interlocked.CompareExchange(ref this._processing, _IDLE, _PROCESSING);
        throw;
      }
    }


    /// <summary>
    /// The worker thread.
    /// </summary>
    /// <param name="queue">The queue.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="executionDelay">The execution delay.</param>
    /// <param name="exceptionCallback">The exception callback, if any.</param>
    /// <param name="isRunning">The reference to the isRunning flag.</param>
    private static void _Worker(ConcurrentQueue<TItem> queue, Action<TItem> callback, TimeSpan? executionDelay, Action<TItem, Exception> exceptionCallback, ref int isRunning) {
      Debug.Assert(queue != null);
      Debug.Assert(callback != null);

      while(queue.Count>0){ // just be save in case of races
        
        // in case the value has alread changed
        if (Interlocked.CompareExchange(ref isRunning, _PROCESSING, _IDLE) != _IDLE)
          return;

        try {
          _DeqeueAndExecute(queue, callback, executionDelay, exceptionCallback);
        } finally {

          // reset flag when done
          Interlocked.CompareExchange(ref isRunning, _IDLE, _PROCESSING);
        }
      }
    }

    /// <summary>
    /// Dequeues and executes items
    /// </summary>
    /// <param name="queue">Our queue</param>
    /// <param name="callback">The item executor</param>
    /// <param name="executionDelay">The delay to use when executing items</param>
    /// <param name="exceptionCallback">Called on exceptions</param>
    private static void _DeqeueAndExecute(ConcurrentQueue<TItem> queue, Action<TItem> callback, TimeSpan? executionDelay, Action<TItem, Exception> exceptionCallback) {
      var useDelay = executionDelay != null;
      
      while (queue.TryDequeue(out var current)) {
        if (useDelay)
          Thread.Sleep(executionDelay.Value);

        try {
          callback(current);
        } catch (Exception e) {
          if (exceptionCallback == null)
            throw;
          exceptionCallback(current, e);
        }
      }
    }

    /// <summary>
    /// Tries to dequeue.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public bool TryDequeue(out TItem item) => this._Items.TryDequeue(out item);

    /// <summary>
    /// Gets the number of contained items.
    /// </summary>
    /// <value>The number of items.</value>
    public int Count => this._Items.Count;

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
    public bool IsEmpty => this._Items.IsEmpty;

    /// <summary>
    /// Gets the current queue content as an array.
    /// </summary>
    /// <returns>An array containig the queues elements.</returns>
    public TItem[] ToArray() => this._Items.ToArray();

    #endregion
  }
}