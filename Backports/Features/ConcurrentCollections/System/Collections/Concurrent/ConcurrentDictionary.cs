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

#if !SUPPORTS_CONCURRENT_COLLECTIONS

using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Concurrent;

public class ConcurrentDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
  private readonly Dictionary<TKey, TValue> _items;

  public ConcurrentDictionary() => this._items = new();
  public ConcurrentDictionary(IEqualityComparer<TKey> comparer) => this._items = new(comparer);

  public TValue GetOrAdd(TKey key, Func<TKey, TValue> creator) {
    lock (this._items) {
      if (this._items.TryGetValue(key, out var value))
        return value;

      value = creator(key);
      this._items.Add(key, value);
      return value;
    }
  }

  public bool TryGetValue(TKey key, out TValue value) {
    lock (this._items)
      return this._items.TryGetValue(key, out value);
  }

  public bool TryAdd(TKey key, TValue value) {
    lock (this._items) {
      if (this._items.ContainsKey(key))
        return false;

      this._items.Add(key, value);
      return true;
    }
  }

  public int Count {
    get {
      lock (this._items)
        return this._items.Count;
    }
  }

  public bool TryRemove(TKey key, out TValue value) {
    lock (this._items) {
      if (!this._items.TryGetValue(key, out value))
        return false;

      this._items.Remove(key);
      return true;
    }
  }

  public IEnumerable<TKey> Keys {
    get {
      lock (this._items)
        return this._items.Keys.ToArray();
    }
  }

  public TValue[] Values {
    get {
      lock (this._items)
        return this._items.Values.ToArray();
    }
  }

  public TValue this[TKey key] {
    get {
      lock (this._items)
        return this._items[key];
    }
    set {
      lock (this._items)
        this._items[key] = value;
    }
  }

#if SUPPORTS_LINQ
  public KeyValuePair<TKey, TValue>[] OrderBy<TSort>(Func<KeyValuePair<TKey, TValue>, TSort> keySelector) {
    lock (this._items)
      return this._items.OrderBy(keySelector).ToArray();
  }

  public KeyValuePair<TKey, TValue>[] OrderByDescending<TSort>(Func<KeyValuePair<TKey, TValue>, TSort> keySelector) {
    lock (this._items)
      return this._items.OrderByDescending(keySelector).ToArray();
  }

#endif

  #region Implementation of IEnumerable

  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
    lock (this._items) {
      var items = this._items.ToArray();
      return (IEnumerator<KeyValuePair<TKey, TValue>>)items.GetEnumerator();
    }
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  #endregion
}


#endif
