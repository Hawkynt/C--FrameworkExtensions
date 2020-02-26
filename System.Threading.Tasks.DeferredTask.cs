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

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
#if NET40
using System.Diagnostics.Contracts;
#endif

namespace System.Threading.Tasks {
  /// <summary>
  /// This class allows us to defer actions by a certain time and possibly overwrite the passed values within that timespan.
  /// </summary>
  /// <typeparam name="TValue">The type of item to pass for execution.</typeparam>
  internal class DeferredTask<TValue> {

    /// <summary>
    /// Stores the scheduled values, alongside their storage date.
    /// </summary>
    private class Item {
      public readonly TValue value;
      public readonly DateTime createDate;
      private Item(TValue value, DateTime createDate) {
        this.value = value;
        this.createDate = createDate;
      }

      public static Item Now(TValue value) => new Item(value, DateTime.MinValue);
      public static Item Schedule(TValue value) => new Item(value, DateTime.UtcNow);

    }

    private const int _DEFAULT_WAIT_TIME_IN_MSECS = 500;

    private readonly Action<TValue> _action;
    private readonly TimeSpan _waitTime;
    private readonly bool _allowTaskOverlapping;
    private readonly bool _autoAbortOnSchedule;

    private Item _currentValue;
    private Thread _currentThread;

    public DeferredTask(Action<TValue> action, TimeSpan? waitTime = null, bool allowTaskOverlapping = true, bool autoAbortOnSchedule = false) {
#if NET40
      Contract.Requires(action != null);
#endif
      this._action = action;
      this._waitTime = waitTime ?? TimeSpan.FromMilliseconds(_DEFAULT_WAIT_TIME_IN_MSECS);
      this._allowTaskOverlapping = allowTaskOverlapping;
      this._autoAbortOnSchedule = autoAbortOnSchedule;
    }

    /// <summary>
    /// Schedules the specified value for later execution.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Schedule(TValue value) {
      if (this._autoAbortOnSchedule)
        this.Abort();

      Interlocked.Exchange(ref this._currentValue, Item.Schedule(value));

      this._RunThreadIfNeeded();
    }

    /// <summary>
    /// Executes immediately with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Now(TValue value) {
      Interlocked.Exchange(ref this._currentValue, Item.Now(value));
      if (this._autoAbortOnSchedule)
        this.Abort();

      this._RunThreadIfNeeded();
    }

    /// <summary>
    /// Aborts a currently running waiting or worker thread.
    /// </summary>
    public void Abort() {
      var thread = this._currentThread;
      if (thread == null || Interlocked.CompareExchange(ref this._currentThread, null, thread) != thread)
        return;

      thread.Abort();
    }

    /// <summary>
    /// Starts a new worker thread if values are available.
    /// </summary>
    private void _RunThreadIfNeeded() {
      if (this._currentValue == null)
        return;

      var thread = new Thread(this._thread) {
        IsBackground = true,
        Name = "Deferred Task #" + this.GetHashCode()
      };

      // if not somebody else started a thread already
      if (Interlocked.CompareExchange(ref this._currentThread, thread, null) == null)
        thread.Start(thread);
    }


    private void _thread(object state) {
      var thread = (Thread)state;
      try {
        while (true) {
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

          if (this._allowTaskOverlapping) {

            // immediately release thread, new thread will start with null value if needed
            Interlocked.CompareExchange(ref this._currentThread, null, thread);
          }

          // execute action
          this._action(item.value);
        }
      } finally {

        // release this thread
        Interlocked.CompareExchange(ref this._currentThread, null, thread);
      }
    }
  }
}
