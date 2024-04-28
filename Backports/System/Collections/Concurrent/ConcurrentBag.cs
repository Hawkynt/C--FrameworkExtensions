#if !SUPPORTS_CONCURRENT_COLLECTIONS

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

namespace System.Collections.Concurrent;

public class ConcurrentBag<T> {
  private readonly List<T> _items = new();

  public bool Any() {
    lock (this._items)
      return this._items.Count > 0;
  }

  public void Clear() {
    lock (this._items)
      this._items.Clear();
  }

  public void Add(T item) {
    lock (this._items)
      this._items.Add(item);
  }

  public bool TryTake(out T item) {
    lock (this._items) {
      if (this._items.Count > 0) {
        item = this._items[0];
        this._items.RemoveAt(0);
        return true;
      }

      item = default;
      return false;
    }
  }

  public T[] ToArray() {
    lock (this._items)
      return this._items.ToArray();
  }
}

#endif
