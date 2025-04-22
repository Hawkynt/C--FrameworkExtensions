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

using System.Collections.Generic;
using System.Threading;
using Guard;

namespace System.Collections.Concurrent;

public static partial class ConcurrentStackExtensions {
  /// <summary>
  ///   Pops an item from the stack.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This ConcurrentStack.</param>
  /// <returns>The item that was popped.</returns>
  public static TItem Pop<TItem>(this ConcurrentStack<TItem> @this) {
    Against.ThisIsNull(@this);

    TItem result;
    while (!@this.TryPop(out result))
      Thread.Sleep(0);
    return result;
  }

  /// <summary>
  ///   Pushes the all given items to the stack.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This ConcurrentStack.</param>
  /// <param name="items">The items.</param>
  public static void PushRange<TItem>(this ConcurrentStack<TItem> @this, IEnumerable<TItem> items) {
    Against.ThisIsNull(@this);

    foreach (var item in items)
      @this.Push(item);
  }
}
