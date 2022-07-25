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

using System.Collections.Generic;
using System.Threading;

namespace System.Collections.Concurrent {
  /// <summary>
  /// An item bag on which work can be executed in an atomic way.
  /// Order of inserted items is not guaranteed.
  /// </summary>
  /// <typeparam name="T">Type of items contained</typeparam>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class ConcurrentWorkingBag<T> : IEnumerable<T> {
    private readonly List<T> _items = new List<T>();
    private readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

    private void _ProcessAll(Action<int> callback) {
      var processedAll = new ManualResetEventSlim(false);
      var count = this._items.Count;

      void CallBack(object index) {
        try {
          callback((int)index);
        } finally {
          if (Interlocked.Decrement(ref count) < 1)
            processedAll.Set();
        }
      }

      for (var index = 0; index < count; ++index) {
        for (; ; ) {
          var couldEnqueue = ThreadPool.QueueUserWorkItem(CallBack, index);
          if (couldEnqueue)
            break;

          Thread.Sleep(5);
        }
      }

      processedAll.Wait();
    }

    /// <summary>
    /// Replaces all matching items or inserts a new one if none exists (which also gets replaced)
    /// </summary>
    /// <param name="selector">The selector.</param>
    /// <param name="call">The replacer.</param>
    /// <param name="factory">The factory.</param>
    /// <returns>false if a new one gets inserted, else true</returns>
    public bool AddOrReplace(Func<T, bool> selector, Func<T, T> call, Func<T> factory) {
      this._readerWriterLockSlim.EnterWriteLock();
      try {
        var isFound = 0;

        this._ProcessAll(index=> {
          var item = this._items[index];
          if (!selector(item))
            return;

          Interlocked.Increment(ref isFound);
          this._items[index] = call(item);
        });
        
        if (isFound != 0)
          return true;

        var newItem = factory();
        if (selector(newItem))
          newItem = call(newItem);
        
        this._items.Add(newItem);
        return false;

      } finally {
        this._readerWriterLockSlim.ExitWriteLock();
      }
    }

    /// <summary>
    /// Executes the function with all matching items or inserts a new one (which also gets executed)
    /// </summary>
    /// <param name="predicate">The selector.</param>
    /// <param name="call">The call.</param>
    /// <param name="factory">The factory.</param>
    /// <returns>false if a new one gets inserted, else true</returns>
    public bool AddOrExecute(Func<T, bool> predicate, Action<T> call, Func<T> factory) {
      this._readerWriterLockSlim.EnterWriteLock();
      try {
        var isFound = 0;

        this._ProcessAll(index => {
          var item = this._items[index];
          if (!predicate(item))
            return;

          Interlocked.Increment(ref isFound);
          call(item);
        });
        
        if (isFound != 0)
          return true;

        var newItem = factory();
        this._items.Add(newItem);

        if (predicate(newItem))
          call(newItem);
        
        return false;

      } finally {
        this._readerWriterLockSlim.ExitWriteLock();
      }
    }

    /// <summary>
    /// Tries the remove all matching items.
    /// </summary>
    /// <param name="selector">The selector.</param>
    /// <param name="removed">The removed items.</param>
    /// <returns>true if something got removed, else false</returns>
    public bool TryRemove(Func<T, bool> selector, out T[] removed) {
      this._readerWriterLockSlim.EnterWriteLock();
      try {

        var matches = new bool[this._items.Count];
        this._ProcessAll(index => matches[index] = selector(this._items[index]));

        var results = new List<T>();
        for (var i = this._items.Count - 1; i >= 0; --i) {
          if (!matches[i])
            continue;

          results.Add(this._items[i]);
          this._items.RemoveAt(i);
        }

        removed = results.ToArray();
      } finally {
        this._readerWriterLockSlim.ExitWriteLock();
      }
      return removed.Length > 0;
    }
    /// <summary>
    /// Gets the number of elements in this bag.
    /// </summary>
    /// <value>The number.</value>
    public int Count {
      get {
        this._readerWriterLockSlim.EnterReadLock();
        try {
          return this._items.Count();
        } finally {
          this._readerWriterLockSlim.ExitReadLock();
        }
      }
    }
    /// <summary>
    /// Returns all contained elements as an array.
    /// </summary>
    /// <returns>The array with elements</returns>
    public T[] ToArray() {
      this._readerWriterLockSlim.EnterReadLock();
      try {
        return this._items.ToArray();
      } finally {
        this._readerWriterLockSlim.ExitReadLock();
      }
    }

    #region IEnumerable<T> Member

    public IEnumerator<T> GetEnumerator() => (IEnumerator<T>)this.ToArray().GetEnumerator();

    #endregion

    #region IEnumerable Member

    IEnumerator IEnumerable.GetEnumerator() => this.ToArray().GetEnumerator();

    #endregion
  } // end class
} // end namespace
