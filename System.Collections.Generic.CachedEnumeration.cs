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

using System.Diagnostics.Contracts;

namespace System.Collections.Generic {
  /// <summary>
  /// This class allows cached lazy access to an enumeration's items.
  /// Items will be pulled out of the enumeration as late as possible, at least on first access, but never more than once, because they'll get cached.
  /// </summary>
  /// <typeparam name="TItem">The type of the underlying enumerations' items.</typeparam>
  internal class CachedEnumeration<TItem> : IEnumerable<TItem> {

    private IEnumerator<TItem> _enumerator;
    private bool _enumerationEnded;
    private readonly List<TItem> _cachedItems = new List<TItem>();

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedEnumeration&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="enumeration">The enumeration.</param>
    internal CachedEnumeration(IEnumerable<TItem> enumeration) {
      if (enumeration == null) {
        this._enumerationEnded = true;
      } else {
        this._enumerator = enumeration.GetEnumerator();
      }
    }

    /// <summary>
    /// Gets the cached item at the given position and returns a new one from the enumeration if possible.
    /// </summary>
    /// <param name="cachePosition">The cache position.</param>
    /// <param name="item">The item to be returned.</param>
    /// <returns><c>true</c> if we could get an item somehow; otherwise, <c>false</c>.</returns>
    private bool _GetItemAtPosition(int cachePosition, out TItem item) {
      Contract.Requires(cachePosition >= 0);
      var cachedItems = this._cachedItems;
      if (cachePosition < cachedItems.Count) {
        item = cachedItems[cachePosition];
        return (true);
      }

      lock (cachedItems) {

        // cache could have been changed by another thread already
        if (cachePosition < cachedItems.Count) {
          item = cachedItems[cachePosition];
          return (true);
        }

        // get next item from underlying enumeration
        return (this._GetNextItem(out item));
      }

    }

    /// <summary>
    /// Tries to get the next item from the enumeration and stores it on the cache.
    /// </summary>
    /// <param name="item">The item that got pulled from the enumeration.</param>
    /// <returns><c>true</c> if there was an item; otherwise, <c>false</c>.</returns>
    private bool _GetNextItem(out TItem item) {

      // if enumeration is already at end, there is nothing to pull from
      if (this._enumerationEnded) {
        item = default(TItem);
        return (false);
      }

      // ask for new items
      var enumerator = this._enumerator;
      if (enumerator.MoveNext()) {
        item = enumerator.Current;
        this._cachedItems.Add(item);
        return (true);
      }

      // no new items available
      enumerator.Dispose();
      this._enumerator = null;
      this._enumerationEnded = true;
      item = default(TItem);
      return (false);
    }

    #region Implementation of IEnumerable

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<TItem> GetEnumerator() {

      // if enumeration has already ended, just return an enumerator to the cache, otherwise use the more complex cached enumerator
      return (this._enumerationEnded ? ((IEnumerable<TItem>)this._cachedItems).GetEnumerator() : new CachedEnumerator(this));
    }

    /// <summary>
    /// Gibt einen Enumerator zur�ck, der eine Auflistung durchl�uft.
    /// </summary>
    /// <returns>
    /// Ein <see cref="T:System.Collections.IEnumerator"/>-Objekt, das zum Durchlaufen der Auflistung verwendet werden kann.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    #endregion

    #region enumerator
    /// <summary>
    /// Enumerates through the cached enumeration.
    /// </summary>
    private class CachedEnumerator : IEnumerator<TItem> {
      private readonly CachedEnumeration<TItem> _cache;
      private int _currentIndex = 0;
      private TItem _current;
      public CachedEnumerator(CachedEnumeration<TItem> cache) {
        this._cache = cache;
      }

      #region Implementation of IDisposable
      public void Dispose() { }
      #endregion

      #region Implementation of IEnumerator

      public bool MoveNext() {
        return (this._cache._GetItemAtPosition(this._currentIndex++, out this._current));
      }

      public void Reset() {
        this._currentIndex = 0;
        this._current = default(TItem);
      }

      public TItem Current {
        get { return (this._current); }
      }

      object IEnumerator.Current {
        get { return Current; }
      }

      #endregion
    }
    #endregion

  }

  /// <summary>
  /// Extensions for the generic enumerables.
  /// </summary>
  internal static partial class EnumerableExtensions {
    /// <summary>
    /// Creates a cached version of the given enumeration.
    /// </summary>
    /// <typeparam name="TItem">Type of the elements.</typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <returns>A cached version which will never been enumerated more than once.</returns>
    public static CachedEnumeration<TItem> ToCache<TItem>(this IEnumerable<TItem> This) {
      return (new CachedEnumeration<TItem>(This));
    }
  }
}