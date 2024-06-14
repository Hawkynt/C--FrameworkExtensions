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

using Guard;

namespace System.Collections.Generic;

public static partial class StackExtensions {

  /// <summary>
  /// Replaces the item at the top of the stack with the specified item and returns the original top item.
  /// </summary>
  /// <typeparam name="TItem">The type of elements in the stack.</typeparam>
  /// <param name="this">The <see cref="Stack{T}"/> instance on which this extension method is called.</param>
  /// <param name="item">The item to push onto the stack.</param>
  /// <returns>The original top item from the stack.</returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="InvalidOperationException">Thrown if the stack is empty.</exception>
  /// <example>
  /// <code>
  /// Stack&lt;int&gt; stack = new Stack&lt;int&gt;();
  /// stack.Push(1);
  /// stack.Push(2);
  /// stack.Push(3);
  ///
  /// int top = stack.Exchange(4); // top is 3, stack now contains 4, 2, 1
  /// Console.WriteLine($"Replaced top item: {top}");
  /// Console.WriteLine($"New top item: {stack.Peek()}");
  /// </code>
  /// This example demonstrates replacing the top item of a stack and retrieving the original top item.
  /// </example>
  /// <remarks>
  /// This method provides a way to replace the top item of a stack with a new item, returning the original top item for further use.
  /// </remarks>
  public static TItem Exchange<TItem>(this Stack<TItem> @this, TItem item) {
    Against.ThisIsNull(@this);

    var result = @this.Pop();
    @this.Push(item);
    
    return result;
  }

  /// <summary>
  /// Inverts the specified stack.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">The stack to invert.</param>
  public static void Invert<TItem>(this Stack<TItem> @this) {
    Against.ThisIsNull(@this);
    @this.ToArray();

    Queue<TItem> helpStack = new(@this.Count);
    while (@this.Count > 0)
      helpStack.Enqueue(@this.Pop());

    while (helpStack.Count > 0)
      @this.Push(helpStack.Dequeue());
  }

  /// <summary>
  /// Adds all given items to the stack.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Stack.</param>
  /// <param name="items">The items to push on top of the stack.</param>
  public static void AddRange<TItem>(this Stack<TItem> @this, IEnumerable<TItem> items) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(items);

    foreach (var item in items)
      @this.Push(item);
  }

  /// <summary>
  /// Adds a given item to the stack.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Stack.</param>
  /// <param name="item">The item to push on top of the stack.</param>
  public static void Add<TItem>(this Stack<TItem> @this, TItem item) {
    Against.ThisIsNull(@this);

    @this.Push(item);
  }

  /// <summary>
  /// Fetches one item.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Stack.</param>
  /// <returns>The top-most item.</returns>
  public static TItem Fetch<TItem>(this Stack<TItem> @this) {
    Against.ThisIsNull(@this);

    return @this.Pop();
  }
  
}