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

namespace System.Collections.Concurrent {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class ConcurrentQueue<T> {
    private readonly Queue<T> _queue = new Queue<T>();

    public bool IsEmpty => !this.Any();
    public bool Any() => this.Count > 0;
    public void Clear() {
      lock(this._queue)
        this._queue.Clear();
    }

    public int Count {
      get {
        lock (this._queue) 
          return this._queue.Count;
      }
    }

    public T[] ToArray() {
      lock (this._queue)
        return this._queue.ToArray();
    }

    public void Enqueue(T item) {
      lock (this._queue)
        this._queue.Enqueue(item);
    }

    public bool TryDequeue(out T item) {
      lock (this._queue) {
        if (this._queue.Count > 0) {
          item = this._queue.Dequeue();
          return true;
        }

        item = default;
        return false;
      }
    }

  }

}

#endif
