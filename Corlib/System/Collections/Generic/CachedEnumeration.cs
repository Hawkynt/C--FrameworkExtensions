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

using System.Diagnostics;
using System.Threading;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Collections.Generic {
  /// <summary>
  /// This class allows cached lazy access to an enumeration's items.
  /// Items will be pulled out of the enumeration as late as possible, at least on first access, but never more than once, because they'll get cached.
  /// </summary>
  /// <typeparam name="TItem">The type of the underlying enumerations' items.</typeparam>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class CachedEnumeration<TItem> : IEnumerable<TItem>, IDisposable {

    private IEnumerator<TItem> _enumerator;
    private bool _enumerationEnded;
    private readonly List<TItem> _cachedItems = new List<TItem>();
    private readonly IEnumerable<TItem> _sourceEnumeration;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedEnumeration&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="enumeration">The enumeration.</param>
    internal CachedEnumeration(IEnumerable<TItem> enumeration) {
      this._sourceEnumeration = enumeration;
      this._StartEnumeratingSource();
    }

    /// <summary>
    /// Starts enumerating the source.
    /// </summary>
    private void _StartEnumeratingSource() {
      lock (this._cachedItems)
        this._cachedItems.Clear();

      if (this._sourceEnumeration == null) {
        this._enumerator = null;
        this._enumerationEnded = true;
      } else {
        this._enumerator = this._sourceEnumeration.GetEnumerator();
        this._enumerationEnded = false;
      }
    }

    /// <summary>
    /// Ends enumerating the source.
    /// </summary>
    private void _EndEnumeratingSource() {
      this._enumerationEnded = true;
      var enumerator = Interlocked.Exchange(ref this._enumerator, null);
      enumerator?.Dispose();
    }

    /// <summary>
    /// Gets the <see cref="TItem"/> at the specified index.
    /// </summary>
    public TItem this[int index] {
      get {
        TItem item;
        if (this._TryGetItemAtPosition(index, out item))
          return item;

        throw new ArgumentOutOfRangeException(nameof(index), "Position out of enumeration");
      }
    }

    /// <summary>
    /// Gets the cached item count.
    /// </summary>
    public int CachedItemCount => this._cachedItems.Count;

    /// <summary>
    /// Gets the cached item at the given position and returns a new one from the enumeration if possible.
    /// </summary>
    /// <param name="cachePosition">The cache position.</param>
    /// <param name="item">The item to be returned.</param>
    /// <returns><c>true</c> if we could get an item somehow; otherwise, <c>false</c>.</returns>
    private bool _TryGetItemAtPosition(int cachePosition, out TItem item) {
      Debug.Assert(cachePosition >= 0);
      var cachedItems = this._cachedItems;
      if (cachePosition < cachedItems.Count) {
        item = cachedItems[cachePosition];
        return true;
      }

      lock (cachedItems) {

        // cache could have been changed by another thread already
        if (cachePosition < cachedItems.Count) {
          item = cachedItems[cachePosition];
          return true;
        }

        // get next items from underlying enumeration
        ++cachePosition;
        while (!this._enumerationEnded) {
          var result = this._GetNextItem(out item);
          if (cachePosition == cachedItems.Count)
            return result;
        }

        // return default value
        item = default(TItem);
        return false;
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
        return false;
      }

      // ask for new items
      var enumerator = this._enumerator;
      if (enumerator.MoveNext()) {
        item = enumerator.Current;
        this._cachedItems.Add(item);
        return true;
      }

      // no new items available
      this._EndEnumeratingSource();
      item = default(TItem);
      return false;
    }

    /// <summary>
    /// Resets this instance, effectively clearing all cached content.
    /// </summary>
    public void Reset() {
      lock (this._cachedItems) {
        this._EndEnumeratingSource();
        this._StartEnumeratingSource();
      }
    }

    #region Implementation of IEnumerable

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// Note: If enumeration has already ended, returns an enumerator to the cache, otherwise uses the more complex cached enumerator.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<TItem> GetEnumerator() => this._enumerationEnded ? ((IEnumerable<TItem>)this._cachedItems).GetEnumerator() : new CachedEnumerator(this);

    /// <summary>
    /// Gibt einen Enumerator zurück, der eine Auflistung durchläuft.
    /// </summary>
    /// <returns>
    /// Ein <see cref="T:System.Collections.IEnumerator"/>-Objekt, das zum Durchlaufen der Auflistung verwendet werden kann.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    #endregion

    #region IDisposable Support

    private bool _isAlreadyDisposed;

    protected virtual void Dispose(bool disposing) {
      if (this._isAlreadyDisposed)
        return;

      this._isAlreadyDisposed = true;

      if (disposing)
        this._EndEnumeratingSource();
    }

    public void Dispose() => this.Dispose(true);

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

      public bool MoveNext() => this._cache._TryGetItemAtPosition(this._currentIndex++, out this._current);

      public void Reset() {
        this._currentIndex = 0;
        this._current = default(TItem);
      }

      public TItem Current => this._current;
      object IEnumerator.Current => this.Current;

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
    public static CachedEnumeration<TItem> ToCache<TItem>(this IEnumerable<TItem> This) => new CachedEnumeration<TItem>(This);
  }
}