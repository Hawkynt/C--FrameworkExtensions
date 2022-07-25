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

#if NET20_OR_GREATER && !NET40_OR_GREATER

using System.Collections.Generic;

namespace System.Collections.Concurrent {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class ConcurrentBag<T> {
    private readonly List<T> _items=new List<T>();

    public bool Any() {
      lock(this._items)
        return this._items.Count>0;
    }

    public void Clear() {
      lock (this._items)
        this._items.Clear();
    }

    public void Add(T item) {
      lock(this._items)
        this._items.Add(item);
    }

    public bool TryTake(out T item) {
      lock (this._items)
        if (this._items.Count > 0) {
          item = this._items[0];
          this._items.RemoveAt(0);
          return true;
        }else {
          item = default;
          return false;
        }
    }
    
  }

}

#endif
