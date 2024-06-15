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

using Guard;

namespace System.Collections.Generic;

public static partial class QueueExtensions {
  /// <summary>
  ///   Adds all given items to the queue.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Queue.</param>
  /// <param name="items">The items to enqeue.</param>
  public static void AddRange<TItem>(this Queue<TItem> @this, IEnumerable<TItem> items) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(items);

    foreach (var item in items)
      @this.Enqueue(item);
  }

  /// <summary>
  ///   Adds a given item to the queue.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Queue.</param>
  /// <param name="item">The item to enqeue.</param>
  public static void Add<TItem>(this Queue<TItem> @this, TItem item) {
    Against.ThisIsNull(@this);

    @this.Enqueue(item);
  }

  /// <summary>
  ///   Fetches one item.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Queue.</param>
  /// <returns>The first item.</returns>
  public static TItem Fetch<TItem>(this Queue<TItem> @this) {
    Against.ThisIsNull(@this);

    return @this.Dequeue();
  }

  /// <summary>
  ///   Tries to dequeue an item from the queue.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Queue.</param>
  /// <param name="result">The result.</param>
  /// <returns><c>true</c> if an item could be dequeued; otherwise, <c>false</c>.</returns>
  public static bool TryDequeue<TItem>(this Queue<TItem> @this, out TItem result) {
    Against.ThisIsNull(@this);

    if (@this.Count < 1) {
      result = default;
      return false;
    }

    result = @this.Dequeue();
    return true;
  }
}
