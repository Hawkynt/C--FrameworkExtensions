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
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

namespace System.Collections.Generic;

public static partial class LinkedListExtensions {

  /// <summary>
  /// Determines whether the <see cref="LinkedList{T}"/> contains any elements.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the linked list.</typeparam>
  /// <param name="this">The <see cref="LinkedList{T}"/> instance to check for elements.</param>
  /// <returns><see langword="true"/> if the linked list contains any elements; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// LinkedList&lt;int&gt; numbers = new LinkedList&lt;int&gt;(new int[] { 1, 2, 3 });
  /// bool hasElements = numbers.Any();
  /// Console.WriteLine($"Does the list have elements? {hasElements}");
  ///
  /// LinkedList&lt;string&gt; strings = new LinkedList&lt;string&gt;();
  /// bool isEmpty = !strings.Any();
  /// Console.WriteLine($"Is the list empty? {isEmpty}");
  /// </code>
  /// This example checks if a list of integers contains any elements and similarly checks if an empty list of strings contains any elements, demonstrating the use of the method.
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool Any<T>(this LinkedList<T> @this) {
    Against.ThisIsNull(@this);

    return @this.Count > 0;
  }

  /// <summary>
  /// Adds a new element to the end of the <see cref="LinkedList{T}"/>, mimicking queue behavior.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the linked list.</typeparam>
  /// <param name="this">The <see cref="LinkedList{T}"/> instance on which this extension method is called.</param>
  /// <param name="value">The value to add to the linked list.</param>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// LinkedList&lt;int&gt; queue = new LinkedList&lt;int&gt;();
  /// queue.Enqueue(1);
  /// queue.Enqueue(2);
  /// queue.Enqueue(3);
  /// 
  /// Console.WriteLine("Queue contents:");
  /// foreach (int item in queue)
  /// {
  ///     Console.WriteLine(item);
  /// }
  /// </code>
  /// This example demonstrates adding elements to a <see cref="LinkedList{T}"/> using the <c>Enqueue</c> method, and then iterating over the list to display its contents.
  /// </example>
  /// <remarks>
  /// This method enhances the <see cref="LinkedList{T}"/> to operate like a queue, where elements are always added at the end. This is useful for scenarios where you need FIFO (First-In, First-Out) behavior with the flexibility of a linked list.
  /// </remarks>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Enqueue<T>(this LinkedList<T> @this, T value) {
    Against.ThisIsNull(@this);

    @this.AddLast(value);
  }

  /// <summary>
  /// Removes and returns the first element of the <see cref="LinkedList{T}"/>, mimicking queue behavior.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the linked list.</typeparam>
  /// <param name="this">The <see cref="LinkedList{T}"/> instance on which this extension method is called.</param>
  /// <returns>The first element of the linked list.</returns>
  /// <exception cref="InvalidOperationException">Thrown if the linked list is empty.</exception>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// LinkedList&lt;int&gt; queue = new LinkedList&lt;int&gt;();
  /// queue.Enqueue(1);
  /// queue.Enqueue(2);
  /// queue.Enqueue(3);
  ///
  /// Console.WriteLine("Dequeued item: " + queue.Dequeue());
  /// Console.WriteLine("Next item in queue: " + queue.First.Value);
  /// </code>
  /// This example demonstrates removing the first element from a <see cref="LinkedList{T}"/> using the <c>Dequeue</c> method, and then displaying the next item in the list.
  /// </example>
  /// <remarks>
  /// This method enhances the <see cref="LinkedList{T}"/> to operate like a queue, where elements are always removed from the beginning. This is useful for scenarios where you need FIFO (First-In, First-Out) behavior with the flexibility of a linked list.
  /// </remarks>
  public static T Dequeue<T>(this LinkedList<T> @this) {
    Against.ThisIsNull(@this);

    var result = @this.First;
    if (result == null)
      AlwaysThrow.InvalidOperationException("Empty Queue");

    @this.RemoveFirst();
    return result.Value;
  }

  /// <summary>
  /// Returns the first element of the <see cref="LinkedList{T}"/> without removing it, mimicking the peek operation of a queue/stack.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the linked list.</typeparam>
  /// <param name="this">The <see cref="LinkedList{T}"/> instance on which this extension method is called.</param>
  /// <returns>The first element of the linked list.</returns>
  /// <exception cref="InvalidOperationException">Thrown if the linked list is empty.</exception>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// LinkedList&lt;int&gt; queue = new LinkedList&lt;int&gt;();
  /// queue.Enqueue(1);
  /// queue.Enqueue(2);
  /// queue.Enqueue(3);
  ///
  /// Console.WriteLine("First item: " + queue.Peek());
  /// Console.WriteLine("Queue still has " + queue.Count + " items.");
  /// </code>
  /// This example demonstrates viewing the first element of a <see cref="LinkedList{T}"/> using the <c>Peek</c> method without removing it, showing that the queue still retains its elements.
  /// </example>
  /// <remarks>
  /// This method is useful in scenarios where you need to view the next item to be processed without actually removing it from the queue.
  /// </remarks>

  public static T Peek<T>(this LinkedList<T> @this) {
    Against.ThisIsNull(@this);

    var result = @this.First;
    if (result == null)
      AlwaysThrow.InvalidOperationException("Empty Queue/Stack");

    return result.Value;
  }

  /// <summary>
  /// Adds an element to the front of the <see cref="LinkedList{T}"/>, mimicking the push operation of a stack.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the linked list.</typeparam>
  /// <param name="this">The <see cref="LinkedList{T}"/> instance on which this extension method is called.</param>
  /// <param name="value">The value to add to the front of the linked list.</param>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// LinkedList&lt;int&gt; stack = new LinkedList&lt;int&gt;();
  /// stack.Push(1);
  /// stack.Push(2);
  /// stack.Push(3);
  ///
  /// Console.WriteLine("Current top of stack: " + stack.First.Value);
  /// </code>
  /// This example demonstrates adding elements to the front of a <see cref="LinkedList{T}"/> using the <c>Push</c> method, mimicking a stack's LIFO (Last-In, First-Out) behavior.
  /// </example>
  /// <remarks>
  /// This method enhances the <see cref="LinkedList{T}"/> to operate like a stack, where elements are always added to the front.
  /// It is useful for scenarios requiring LIFO behavior with the flexibility of a linked list, such as undo functionalities in applications.
  /// </remarks>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Push<T>(this LinkedList<T> @this, T value) {
    Against.ThisIsNull(@this);

    @this.AddFirst(value);
  }

  /// <summary>
  /// Removes and returns the first element from the <see cref="LinkedList{T}"/>, mimicking the pop operation of a stack.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the linked list.</typeparam>
  /// <param name="this">The <see cref="LinkedList{T}"/> instance on which this extension method is called.</param>
  /// <returns>The element at the front of the linked list.</returns>
  /// <exception cref="InvalidOperationException">Thrown if the linked list is empty, indicating there are no elements to pop.</exception>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// LinkedList&lt;int&gt; stack = new LinkedList&lt;int&gt;();
  /// stack.Push(1);
  /// stack.Push(2);
  /// stack.Push(3);
  ///
  /// Console.WriteLine("Popped item: " + stack.Pop());
  /// Console.WriteLine("Current top of stack: " + (stack.Any() ? stack.First.Value.ToString() : "Stack is empty"));
  /// </code>
  /// This example demonstrates removing the top element from a <see cref="LinkedList{T}"/> using the <c>Pop</c> method, and then showing the new top of the stack.
  /// </example>
  /// <remarks>
  /// This method is used to implement stack-like behavior on a <see cref="LinkedList{T}"/>, where elements are removed from the front, following the LIFO (Last-In, First-Out) principle.
  /// </remarks>
  public static T Pop<T>(this LinkedList<T> @this) {
    Against.ThisIsNull(@this);

    var result = @this.First;
    if (result == null)
      AlwaysThrow.InvalidOperationException("Empty Stack");

    @this.RemoveFirst();
    return result.Value;
  }

  /// <summary>
  /// Attempts to remove and return the first element from the <see cref="LinkedList{T}"/>, mimicking the pop operation of a stack with a safe approach.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the linked list.</typeparam>
  /// <param name="this">The <see cref="LinkedList{T}"/> instance on which this extension method is called.</param>
  /// <param name="result">The element at the front of the linked list if the operation is successful; otherwise, the default value for the type <typeparamref name="T"/>.</param>
  /// <returns><see langword="true"/> if the element is successfully removed and returned; otherwise, <see langword="false"/> if the linked list is empty.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// LinkedList&lt;int&gt; stack = new LinkedList&lt;int&gt;();
  /// stack.Push(1);
  /// stack.Push(2);
  /// stack.Push(3);
  ///
  /// if (stack.TryPop(out int item))
  /// {
  ///     Console.WriteLine("Popped item: " + item);
  /// }
  /// else
  /// {
  ///     Console.WriteLine("No item to pop.");
  /// }
  /// Console.WriteLine("Current top of stack: " + (stack.Any() ? stack.First.Value.ToString() : "Stack is empty"));
  /// </code>
  /// This example demonstrates the safe popping of an item from a <see cref="LinkedList{T}"/>, ensuring that the stack is not empty before attempting to pop.
  /// </example>
  /// <remarks>
  /// This method provides a fail-safe mechanism for popping elements from a linked list, returning a boolean value to indicate success or failure, which is useful in scenarios where the list might be empty and exception handling for such cases is undesirable.
  /// </remarks>
  public static bool TryPop<T>(this LinkedList<T> @this, out T result) {

    var node = @this.First;
    if (node == null) {
      result = default;
      return false;
    }

    @this.RemoveFirst();
    result = node.Value;
    return true;
  }

  /// <summary>
  /// Attempts to view the first element of the <see cref="LinkedList{T}"/> without removing it, mimicking the peek operation of a stack/queue with a safe approach.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the linked list.</typeparam>
  /// <param name="this">The <see cref="LinkedList{T}"/> instance on which this extension method is called.</param>
  /// <param name="result">The first element of the linked list if the operation is successful; otherwise, the default value for the type <typeparamref name="T"/>.</param>
  /// <returns><see langword="true"/> if the list is not empty and the first element can be retrieved; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// LinkedList&lt;int&gt; queue = new LinkedList&lt;int&gt;();
  /// queue.Enqueue(1);
  /// queue.Enqueue(2);
  /// queue.Enqueue(3);
  ///
  /// if (queue.TryPeek(out int item))
  /// {
  ///     Console.WriteLine("First item in queue: " + item);
  /// }
  /// else
  /// {
  ///     Console.WriteLine("Queue is empty.");
  /// }
  /// </code>
  /// This example demonstrates attempting to view the first item in a <see cref="LinkedList{T}"/> using the TryPeek method without removing it from the list.
  /// </example>
  /// <remarks>
  /// This method is useful when you need to check the next item to be processed without modifying the queue, especially in concurrency scenarios or when the queue's state should not be altered.
  /// </remarks>
  public static bool TryPeek<T>(this LinkedList<T> @this, out T result) {

    var node = @this.First;
    if (node == null) {
      result = default;
      return false;
    }

    result = node.Value;
    return true;
  }

  /// <summary>
  /// Attempts to remove and return the first element from the <see cref="LinkedList{T}"/>, mimicking the dequeue operation of a queue with a safe approach.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the linked list.</typeparam>
  /// <param name="this">The <see cref="LinkedList{T}"/> instance on which this extension method is called.</param>
  /// <param name="result">The first element of the linked list if the operation is successful; otherwise, the default value for the type <typeparamref name="T"/>.</param>
  /// <returns><see langword="true"/> if the element is successfully removed and returned; otherwise, <see langword="false"/> if the linked list is empty.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// LinkedList&lt;int&gt; queue = new LinkedList&lt;int&gt;();
  /// queue.Enqueue(1);
  /// queue.Enqueue(2);
  /// queue.Enqueue(3);
  ///
  /// if (queue.TryDequeue(out int item))
  /// {
  ///     Console.WriteLine("Dequeued item: " + item);
  /// }
  /// else
  /// {
  ///     Console.WriteLine("No item to dequeue.");
  /// }
  /// Console.WriteLine("Next item in queue: " + (queue.Any() ? queue.First.Value.ToString() : "Queue is empty"));
  /// </code>
  /// This example demonstrates the safe dequeuing of an item from a <see cref="LinkedList{T}"/>, ensuring that the queue is not empty before attempting to dequeue.
  /// </example>
  /// <remarks>
  /// This method provides a fail-safe mechanism for dequeuing elements from a linked list, returning a boolean value to indicate success or failure. This is useful in scenarios where the list might be empty and raising an exception for such cases is undesirable.
  /// </remarks>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool TryDequeue<T>(this LinkedList<T> @this, out T result) => TryPop(@this, out result);

}
