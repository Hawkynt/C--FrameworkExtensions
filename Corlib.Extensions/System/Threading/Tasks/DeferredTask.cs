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

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
using Guard;

namespace System.Threading.Tasks;

/// <summary>
///   This class allows us to defer actions by a certain time and possibly overwrite the passed values within that
///   timespan.
/// </summary>
/// <typeparam name="TValue">The type of item to pass for execution.</typeparam>
public class DeferredTask<TValue> {
  /// <summary>
  ///   Stores the scheduled values, alongside their storage date.
  /// </summary>
  private sealed class Item {
    public readonly DateTime createDate;
    public readonly TValue value;

    private Item(TValue value, DateTime createDate) {
      this.value = value;
      this.createDate = createDate;
    }

    public static Item Now(TValue value) => new(value, DateTime.MinValue);
    public static Item Schedule(TValue value) => new(value, DateTime.UtcNow);
  }

  private const int _DEFAULT_WAIT_TIME_IN_MSECS = 500;

  private readonly Action<TValue> _action;
  private readonly TimeSpan _waitTime;
  private readonly bool _allowTaskOverlapping;
  private readonly bool _autoAbortOnSchedule;

  private Item _currentValue;
  private Thread _currentThread;
  private int _threadCount;

  public ManualResetEventSlim WaitHandle { get; } = new(true);

  public DeferredTask(Action<TValue> action, TimeSpan? waitTime = null, bool allowTaskOverlapping = true, bool autoAbortOnSchedule = false) {
    Against.ArgumentIsNull(action);

    this._action = action;
    this._waitTime = waitTime ?? TimeSpan.FromMilliseconds(_DEFAULT_WAIT_TIME_IN_MSECS);
    this._allowTaskOverlapping = allowTaskOverlapping;
    this._autoAbortOnSchedule = autoAbortOnSchedule;
  }

  /// <summary>
  ///   Schedules the specified value for later execution.
  /// </summary>
  /// <param name="value">The value.</param>
  public void Schedule(TValue value) {
    if (this._autoAbortOnSchedule)
      this.Abort();

    Interlocked.Exchange(ref this._currentValue, Item.Schedule(value));

    this._RunThreadIfNeeded();
  }

  /// <summary>
  ///   Executes immediately with the given value.
  /// </summary>
  /// <param name="value">The value.</param>
  public void Now(TValue value) {
    Interlocked.Exchange(ref this._currentValue, Item.Now(value));
    if (this._autoAbortOnSchedule)
      this.Abort();

    this._RunThreadIfNeeded();
  }

  /// <summary>
  ///   Aborts a currently running waiting or worker thread.
  /// </summary>
  public void Abort() {
    var thread = this._currentThread;
    if (thread == null || Interlocked.CompareExchange(ref this._currentThread, null, thread) != thread)
      return;

#if !NETCOREAPP && !NETSTANDARD && !NET5_0_OR_GREATER
    thread.Abort();
#else
      // TODO: signal the running thread to abort somehow ... maybe a reset event, maybe a weak-dictionary IDK
#endif
  }

  /// <summary>
  ///   Starts a new worker thread if values are available.
  /// </summary>
  private void _RunThreadIfNeeded() {
    if (this._currentValue == null)
      return;

    Thread thread = new(Invoke) {
      IsBackground = true,
      Name = $"Deferred Task #{this.GetHashCode()}"
    };

    // if not somebody else started a thread already
    if (Interlocked.CompareExchange(ref this._currentThread, thread, null) == null)
      thread.Start(thread);

    return;

    void Invoke(object state) {
      var localThread = (Thread)state;
      try {
        //if this is the first thread reset the WaitHandle
        if (Interlocked.Increment(ref this._threadCount) == 1)
          this.WaitHandle.Reset();

        for (;;) {
          Item item;

          // wait as long as items are inserted
          var sleepTime = TimeSpan.Zero;
          do {
            Thread.Sleep(sleepTime);
            item = this._currentValue;

            // no more items scheduled, exit thread
            if (item == null)
              return;

            // time to wait before executing action/re-checking
            sleepTime = this._waitTime - (DateTime.UtcNow - item.createDate);
          } while (sleepTime > TimeSpan.Zero);

          // remove item if it did not change, if it did - process in next round
          Interlocked.CompareExchange(ref this._currentValue, null, item);

          if (this._allowTaskOverlapping)
            // immediately release thread, new thread will start with null value if needed
            Interlocked.CompareExchange(ref this._currentThread, null, localThread);

          // execute action
          this._action(item.value);
        }
      } finally {
        // release this thread
        Interlocked.CompareExchange(ref this._currentThread, null, localThread);

        //only set the WaitHandle if no other threads are active
        if (Interlocked.Decrement(ref this._threadCount) == 0)
          this.WaitHandle.Set();
      }
    }
  }
}
