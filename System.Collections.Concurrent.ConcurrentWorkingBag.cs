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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace System.Collections.Concurrent {
  /// <summary>
  /// An item bag on which work can be executed in an atomic way.
  /// Order of inserted items is not guaranteed.
  /// </summary>
  /// <typeparam name="T">Type of items contained</typeparam>
  public class ConcurrentWorkingBag<T> : IEnumerable<T> {
    private readonly List<T> _items = new List<T>();
    private readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

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
        Parallel.For(0, this._items.Count, index => {
          var item = this._items[index];

          // does not match, skip it
          if (!selector(item))
            return;

          this._items[index] = call(item);
          Interlocked.Increment(ref isFound);
        });

        // already there
        if (isFound != 0)
          return (true);

        // create new item
        var newItem = factory();
        if (selector(newItem))
          newItem = call(newItem);
        else {
          // the new element does not match the selector
        }

        this._items.Add(newItem);
        return (false);

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
        var result = this._items.AsParallel().Where(predicate).Select(item => {
          call(item);
          return (byte.MinValue);
        }).Any();

        // already there
        if (result)
          return (true);

        var newItem = factory();
        this._items.Add(newItem);
        if (predicate(newItem))
          call(newItem);
        else {
          // the new element does not match the selector
        }
        return (false);

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
      var result = new ConcurrentBag<T>();
      this._readerWriterLockSlim.EnterWriteLock();
      try {
        var matchingItems = this._items.AsParallel().Where(selector).Select(varItem => {
          result.Add(varItem);
          return (varItem);
        });
        foreach (var item in matchingItems)
          this._items.Remove(item);

      } finally {
        this._readerWriterLockSlim.ExitWriteLock();
      }
      removed = result.ToArray();
      return (removed.Length > 0);
    }
    /// <summary>
    /// Gets the number of elements in this bag.
    /// </summary>
    /// <value>The number.</value>
    public int Count {
      get {
        this._readerWriterLockSlim.EnterReadLock();
        try {
          return (this._items.Count());
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
        return (this._items.ToArray());
      } finally {
        this._readerWriterLockSlim.ExitReadLock();
      }
    }

    #region IEnumerable<T> Member
    public IEnumerator<T> GetEnumerator() {
      return (this.ToArray().ToList().GetEnumerator());
    }
    #endregion

    #region IEnumerable Member
    IEnumerator IEnumerable.GetEnumerator() {
      return (this.ToArray().GetEnumerator());
    }
    #endregion
  } // end class
} // end namespace
