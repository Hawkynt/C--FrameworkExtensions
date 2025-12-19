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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Concurrent;

public class ConcurrentBag<T> {
  private readonly List<T> _items = [];

  public bool IsEmpty => !this.Any();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Any() {
    lock (this._items)
      return this._items.Count > 0;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Clear() {
    lock (this._items)
      this._items.Clear();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Add(T item) {
    lock (this._items)
      this._items.Add(item);
  }

  public bool TryTake([MaybeNullWhen(false)] out T item) {
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T[] ToArray() {
    lock (this._items)
      return this._items.ToArray();
  }
}

#endif
