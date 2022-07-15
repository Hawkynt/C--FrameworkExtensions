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
#if NET40_OR_GREATER
using System.Diagnostics.Contracts;
#endif

namespace System.Collections.Generic {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class QueueExtensions {

    /// <summary>
    /// Adds all given items to the queue.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This Queue.</param>
    /// <param name="items">The items to enqeue.</param>
    public static void AddRange<TItem>(this Queue<TItem> This, IEnumerable<TItem> items) {
#if NET40_OR_GREATER
      Contract.Requires(This != null);
      Contract.Requires(items != null);
#endif
      foreach (var item in items)
        This.Enqueue(item);
    }

    /// <summary>
    /// Adds a given item to the queue.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This Queue.</param>
    /// <param name="item">The item to enqeue.</param>
    public static void Add<TItem>(this Queue<TItem> @this, TItem item) => @this.Enqueue(item);

    /// <summary>
    /// Fetches one item.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This Queue.</param>
    /// <returns>The first item.</returns>
    public static TItem Fetch<TItem>(this Queue<TItem> @this) => @this.Dequeue();

    /// <summary>
    /// Tries to dequeue an item from the queue.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This Queue.</param>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if an item could be dequeued; otherwise, <c>false</c>.</returns>
    public static bool TryDequeue<TItem>(this Queue<TItem> @this, out TItem result) {
#if NET40_OR_GREATER
      Contract.Requires(@this != null);
#endif
      if (@this.Count < 1) {
        result = default(TItem);
        return false;
      }
      result = @this.Dequeue();
      return true;
    }
  }
}