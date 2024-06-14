#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

#if !SUPPORTS_CONCURRENT_COLLECTIONS

namespace System.Collections.Concurrent;

public class ConcurrentQueue<T> {
  private readonly Generic.Queue<T> _queue = new();

  public bool IsEmpty => !this.Any();
  public bool Any() => this.Count > 0;

  public void Clear() {
    lock (this._queue)
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

#endif
