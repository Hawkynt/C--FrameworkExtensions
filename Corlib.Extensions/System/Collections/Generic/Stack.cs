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

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions=Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class StackExtensions {

  /// <summary>
  /// Transfers elements from the source <see cref="Stack{T}"/> into the target <see cref="Span{T}"/>,
  /// starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="Stack{T}"/> instance from which elements are pulled.</param>
  /// <param name="target">
  /// The destination span to fill with items from the stack.  
  /// If the span is empty, no elements are transferred.
  /// </param>
  /// <returns>
  /// A <see cref="Span{T}"/> containing the elements pulled from the stack, in order from top to bottom.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new Stack&lt;int&gt;();
  /// stack.Push(3);
  /// stack.Push(2);
  /// stack.Push(1);
  ///
  /// Span&lt;int&gt; buffer = stackalloc int[2];
  /// var result = stack.PullTo(buffer);
  /// // result[0] == 1, result[1] == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<T> PullTo<T>(this Stack<T> @this, Span<T> target) {
    Against.ThisIsNull(@this);
    if (target.IsEmpty)
      return target;

    var count = Math.Min(@this.Count, target.Length);
    if (count <= 0)
      return Span<T>.Empty;

    var result = target[..count];
    _PullCore(@this, result);
    return result;
  }

  /// <summary>
  /// Transfers elements from the source <see cref="Stack{T}"/> into the specified target array,
  /// starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="Stack{T}"/> instance from which elements are pulled.</param>
  /// <param name="target">The destination array to fill with items from the stack.</param>
  /// <returns>
  /// The number of elements successfully copied into <paramref name="target"/>.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown if <paramref name="target"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new Stack&lt;string&gt;();
  /// stack.Push("C");
  /// stack.Push("B");
  /// stack.Push("A");
  ///
  /// string[] buffer = new string[2];
  /// int copied = stack.PullTo(buffer);
  /// // buffer[0] == "A", buffer[1] == "B", copied == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this Stack<T> @this, T[] target) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    var result = Math.Min(target.Length, @this.Count);
    _PullCore(@this, target.AsSpan()[..result]);
    return result;
  }

  /// <summary>
  /// Transfers elements from the source <see cref="Stack{T}"/> into the specified target array,
  /// beginning at the given offset and starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="Stack{T}"/> instance from which elements are pulled.</param>
  /// <param name="target">The destination array to fill with items from the stack.</param>
  /// <param name="offset">The zero-based index in <paramref name="target"/> at which to begin writing.</param>
  /// <returns>
  /// The number of elements successfully copied into <paramref name="target"/> starting at <paramref name="offset"/>.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown if <paramref name="target"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="IndexOutOfRangeException">
  /// Thrown if <paramref name="offset"/> is outside the valid range of the <paramref name="target"/> array.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new Stack&lt;char&gt;();
  /// stack.Push('Z');
  /// stack.Push('Y');
  /// stack.Push('X');
  ///
  /// char[] buffer = new char[5];
  /// int copied = stack.PullTo(buffer, 2);
  /// // buffer[2] == 'X', buffer[3] == 'Y', buffer[4] == 'Z', copied == 3
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this Stack<T> @this, T[] target, int offset) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    Against.IndexOutOfRange(offset, target.Length - 1);
    var result = Math.Min(target.Length - offset, @this.Count);
    _PullCore(@this, target.AsSpan(offset, result));
    return result;
  }

  /// <summary>
  /// Transfers up to <paramref name="maxCount"/> elements from the source <see cref="Stack{T}"/> into the specified target array,
  /// beginning at the given <paramref name="offset"/> and starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="Stack{T}"/> instance from which elements are pulled.</param>
  /// <param name="target">The destination array to fill with items from the stack.</param>
  /// <param name="offset">The zero-based index in <paramref name="target"/> at which to begin writing.</param>
  /// <param name="maxCount">
  /// The maximum number of elements to copy.  
  /// Must be greater than zero and must not exceed the available space from <paramref name="offset"/> to the end of <paramref name="target"/>.
  /// </param>
  /// <returns>
  /// The actual number of elements copied into <paramref name="target"/>, which will be the lesser of <paramref name="maxCount"/>
  /// and the number of elements available in the stack.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown if <paramref name="target"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="IndexOutOfRangeException">
  /// Thrown if <paramref name="offset"/> is outside the valid range of the <paramref name="target"/> array.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if <paramref name="maxCount"/> is less than or equal to zero, or greater than the remaining capacity
  /// of <paramref name="target"/> starting at <paramref name="offset"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new Stack&lt;int&gt;();
  /// stack.Push(3);
  /// stack.Push(2);
  /// stack.Push(1);
  ///
  /// int[] buffer = new int[5];
  /// int copied = stack.PullTo(buffer, 1, 2);
  /// // buffer[1] == 1, buffer[2] == 2, copied == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this Stack<T> @this, T[] target, int offset, int maxCount) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    Against.IndexOutOfRange(offset, target.Length - 1);
    Against.CountOutOfRange(maxCount, target.Length - offset);
    var result = Math.Min(maxCount, @this.Count);
    _PullCore(@this, target.AsSpan(offset, result));
    return result;
  }

  /// <summary>
  /// Copies all elements from the source <see cref="Stack{T}"/> into a new array,
  /// starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="Stack{T}"/> instance.</param>
  /// <returns>
  /// A new array containing all elements from the stack, in order from top to bottom.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new Stack&lt;string&gt;();
  /// stack.Push("C");
  /// stack.Push("B");
  /// stack.Push("A");
  ///
  /// string[] all = stack.PullAll();
  /// // all[0] == "A", all[1] == "B", all[2] == "C"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T[] PullAll<T>(this Stack<T> @this) {
    Against.ThisIsNull(@this);
    var count = @this.Count;
    var result = new T[count];
    _PullCore(@this, result.AsSpan());
    return result;
  }

  /// <summary>
  /// Copies up to <paramref name="maxCount"/> elements from the source <see cref="Stack{T}"/> into a new array,
  /// starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="Stack{T}"/> instance.</param>
  /// <param name="maxCount">
  /// The maximum number of elements to copy.  
  /// Must be greater than zero.
  /// </param>
  /// <returns>
  /// A new array containing up to <paramref name="maxCount"/> elements from the stack,  
  /// in order from top to bottom.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if <paramref name="maxCount"/> is less than or equal to zero.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new Stack&lt;int&gt;();
  /// stack.Push(3);
  /// stack.Push(2);
  /// stack.Push(1);
  ///
  /// int[] result = stack.Pull(2);
  /// // result[0] == 1, result[1] == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T[] Pull<T>(this Stack<T> @this, int maxCount) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(maxCount);

    var result = new T[Math.Min(maxCount, @this.Count)];
    _PullCore(@this, result.AsSpan());
    return result;
  }

  private static void _PullCore<T>(Stack<T> stack, Span<T> target) {
    var offset = 0;
    var iterations = target.Length >> 3;
    switch (target.Length & 7) {
      case 0: goto Pull0Or8;
      case 1: goto Pull1;
      case 2: goto Pull2;
      case 3: goto Pull3;
      case 4: goto Pull4;
      case 5: goto Pull5;
      case 6: goto Pull6;
      case 7: goto Pull7;
      default:goto PullNone;
    }

    Pull0Or8:
    if(iterations--<=0)
      goto PullNone;

    target[offset++] = stack.Pop();
    Pull7: target[offset++] = stack.Pop();
    Pull6: target[offset++] = stack.Pop();
    Pull5: target[offset++] = stack.Pop();
    Pull4: target[offset++] = stack.Pop();
    Pull3: target[offset++] = stack.Pop();
    Pull2: target[offset++] = stack.Pop();
    Pull1: target[offset++] = stack.Pop();
    goto Pull0Or8;

    PullNone: 
    ;
  }

  /// <summary>
  ///   Replaces the item at the top of the stack with the specified item and returns the original top item.
  /// </summary>
  /// <typeparam name="TItem">The type of elements in the stack.</typeparam>
  /// <param name="this">The <see cref="Stack{T}" /> instance on which this extension method is called.</param>
  /// <param name="item">The item to push onto the stack.</param>
  /// <returns>The original top item from the stack.</returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="InvalidOperationException">Thrown if the stack is empty.</exception>
  /// <example>
  ///   <code>
  /// Stack&lt;int&gt; stack = new Stack&lt;int&gt;();
  /// stack.Push(1);
  /// stack.Push(2);
  /// stack.Push(3);
  /// 
  /// int top = stack.Exchange(4); // top is 3, stack now contains 4, 2, 1
  /// Console.WriteLine($"Replaced top item: {top}");
  /// Console.WriteLine($"New top item: {stack.Peek()}");
  /// </code>
  ///   This example demonstrates replacing the top item of a stack and retrieving the original top item.
  /// </example>
  /// <remarks>
  ///   This method provides a way to replace the top item of a stack with a new item, returning the original top item for
  ///   further use.
  /// </remarks>
  public static TItem Exchange<TItem>(this Stack<TItem> @this, TItem item) {
    Against.ThisIsNull(@this);

    var result = @this.Pop();
    @this.Push(item);

    return result;
  }

  /// <summary>
  ///   Inverts the specified stack.
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
  ///   Adds all given items to the stack.
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
  ///   Adds a given item to the stack.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Stack.</param>
  /// <param name="item">The item to push on top of the stack.</param>
  public static void Add<TItem>(this Stack<TItem> @this, TItem item) {
    Against.ThisIsNull(@this);

    @this.Push(item);
  }

  /// <summary>
  ///   Fetches one item.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Stack.</param>
  /// <returns>The top-most item.</returns>
  public static TItem Fetch<TItem>(this Stack<TItem> @this) {
    Against.ThisIsNull(@this);

    return @this.Pop();
  }
}
