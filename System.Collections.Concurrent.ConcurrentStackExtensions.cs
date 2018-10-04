#region (c)2010-2020 Hawkynt
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
using System.Threading;

namespace System.Collections.Concurrent {
  internal static partial class ConcurrentStackExtensions {
    /// <summary>
    /// Pops an item from the stack.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This ConcurrentStack.</param>
    /// <returns>The item that was popped.</returns>
    public static TItem Pop<TItem>(this ConcurrentStack<TItem> @this) {
      if (ReferenceEquals(null, @this))
        throw new NullReferenceException();

      TItem result;
      while (!@this.TryPop(out result)) { Thread.Sleep(0); }
      return (result);
    }

    /// <summary>
    /// Pushes the all given items to the stack.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This ConcurrentStack.</param>
    /// <param name="items">The items.</param>
    public static void PushRange<TItem>(this ConcurrentStack<TItem> @this, IEnumerable<TItem> items) {
      if (ReferenceEquals(null, @this))
        throw new NullReferenceException();

      foreach (var item in items)
        @this.Push(item);
    }
  }
}
