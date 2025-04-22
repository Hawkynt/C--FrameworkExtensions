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

namespace System.Collections.Concurrent;

public class ConcurrentStack<T> {
  private readonly Generic.Stack<T> _stack = new();

  public bool IsEmpty => !this.Any();
  public bool Any() => this.Count > 0;

  public void Clear() {
    lock (this._stack)
      this._stack.Clear();
  }

  public int Count {
    get {
      lock (this._stack)
        return this._stack.Count;
    }
  }

  public T[] ToArray() {
    lock (this._stack)
      return this._stack.ToArray();
  }

  public void Push(T item) {
    lock (this._stack)
      this._stack.Push(item);
  }

  public bool TryPop(out T item) {
    lock (this._stack) {
      if (this._stack.Count > 0) {
        item = this._stack.Pop();
        return true;
      }

      item = default;
      return false;
    }
  }
}

#endif
