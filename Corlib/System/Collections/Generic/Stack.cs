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
using System.Diagnostics.Contracts;

namespace System.Collections.Generic {
  internal static partial class StackExtensions {
    /// <summary>
    /// Inverts the specified stack.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">The stack to invert.</param>
    public static void Invert<TItem>(this Stack<TItem> This) {
      Contract.Requires(This != null);
      var helpStack = new Queue<TItem>(This.Count);
      while (This.Count > 0)
        helpStack.Enqueue(This.Pop());
      while (helpStack.Count > 0)
        This.Push(helpStack.Dequeue());
    }

    /// <summary>
    /// Adds all given items to the stack.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This Stack.</param>
    /// <param name="items">The items to push on top of the stack.</param>
    public static void AddRange<TItem>(this Stack<TItem> This, IEnumerable<TItem> items) {
      Contract.Requires(This != null);
      Contract.Requires(items != null);
      foreach (var item in items)
        This.Push(item);
    }

    /// <summary>
    /// Adds a given item to the stack.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This Stack.</param>
    /// <param name="item">The item to push on top of the stack.</param>
    public static void Add<TItem>(this Stack<TItem> This, TItem item) {
      Contract.Requires(This != null);
      This.Push(item);
    }

    /// <summary>
    /// Fetches one item.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This Stack.</param>
    /// <returns>The top-most item.</returns>
    public static TItem Fetch<TItem>(this Stack<TItem> This) {
      Contract.Requires(This != null);
      return (This.Pop());
    }
  }
}