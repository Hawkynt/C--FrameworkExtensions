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
using System.Runtime.CompilerServices;
using System.Threading;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

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

  /// <summary>
  /// Transfers elements from the source <see cref="ConcurrentStack{T}"/> into the specified <see cref="Span{T}"/>,
  /// starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentStack{T}"/> instance from which elements are popped.</param>
  /// <param name="target">
  /// The destination span to fill with elements from the stack.  
  /// If the span is empty or the stack has no elements, the operation completes without copying any data.
  /// </param>
  /// <returns>
  /// A <see cref="Span{T}"/> representing the portion of <paramref name="target"/> that was filled with popped elements.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new ConcurrentStack&lt;int&gt;();
  /// stack.Push(1);
  /// stack.Push(2);
  /// stack.Push(3);
  ///
  /// Span&lt;int&gt; buffer = stackalloc int[2];
  /// var result = stack.PullTo(buffer);
  /// // result[0] == 3, result[1] == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<T> PullTo<T>(this ConcurrentStack<T> @this, Span<T> target) {
    Against.ThisIsNull(@this);
    return target.IsEmpty ? target : _PullCore(@this, target);
  }

  /// <summary>
  /// Transfers elements from the source <see cref="ConcurrentStack{T}"/> into the specified target array,
  /// starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentStack{T}"/> instance from which elements are popped.</param>
  /// <param name="target">The destination array to fill with popped elements.</param>
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
  /// var stack = new ConcurrentStack&lt;string&gt;();
  /// stack.Push("third");
  /// stack.Push("second");
  /// stack.Push("first");
  ///
  /// var buffer = new string[2];
  /// int copied = stack.PullTo(buffer);
  /// // buffer[0] == "first", buffer[1] == "second", copied == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this ConcurrentStack<T> @this, T[] target) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    return PullTo(@this, target.AsSpan()).Length;
  }

  /// <summary>
  /// Transfers elements from the source <see cref="ConcurrentStack{T}"/> into the specified target array,
  /// beginning at the given <paramref name="offset"/> and starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentStack{T}"/> instance from which elements are popped.</param>
  /// <param name="target">The destination array to fill with popped elements.</param>
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
  /// Thrown if <paramref name="offset"/> is outside the bounds of <paramref name="target"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new ConcurrentStack&lt;int&gt;();
  /// stack.Push(10);
  /// stack.Push(20);
  /// stack.Push(30);
  ///
  /// int[] buffer = new int[5];
  /// int copied = stack.PullTo(buffer, 1);
  /// // buffer[1] == 30, buffer[2] == 20, buffer[3] == 10, copied == 3
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this ConcurrentStack<T> @this, T[] target, int offset) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    Against.IndexOutOfRange(offset, target.Length - 1);
    return PullTo(@this, target.AsSpan(offset)).Length;
  }

  /// <summary>
  /// Transfers up to <paramref name="maxCount"/> elements from the source <see cref="ConcurrentStack{T}"/> into the specified target array,
  /// starting at the given <paramref name="offset"/> and popping from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentStack{T}"/> instance from which elements are popped.</param>
  /// <param name="target">The destination array to fill with popped elements.</param>
  /// <param name="offset">The index in <paramref name="target"/> at which to begin writing.</param>
  /// <param name="maxCount">
  /// The maximum number of elements to copy.  
  /// Must be greater than zero and must not exceed the remaining space in <paramref name="target"/> starting at <paramref name="offset"/>.
  /// </param>
  /// <returns>
  /// The number of elements actually copied into <paramref name="target"/>.  
  /// This will be the lesser of <paramref name="maxCount"/> and the number of items in the stack.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown if <paramref name="target"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="IndexOutOfRangeException">
  /// Thrown if <paramref name="offset"/> is outside the bounds of <paramref name="target"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if <paramref name="maxCount"/> is less than or equal to zero, or exceeds the space from <paramref name="offset"/> to the end of <paramref name="target"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new ConcurrentStack&lt;string&gt;();
  /// stack.Push("C");
  /// stack.Push("B");
  /// stack.Push("A");
  ///
  /// string[] buffer = new string[5];
  /// int copied = stack.PullTo(buffer, 1, 2);
  /// // buffer[1] == "A", buffer[2] == "B", copied == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this ConcurrentStack<T> @this, T[] target, int offset, int maxCount) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    Against.IndexOutOfRange(offset, target.Length - 1);
    Against.CountOutOfRange(maxCount, target.Length - offset);
    return PullTo(@this, target.AsSpan(offset, maxCount)).Length;
  }

  /// <summary>
  /// Copies all elements from the source <see cref="ConcurrentStack{T}"/> into a new array,
  /// starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentStack{T}"/> instance.</param>
  /// <returns>
  /// A new array containing all elements from the stack, in the order they would be popped (LIFO).
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new ConcurrentStack&lt;int&gt;();
  /// stack.Push(1);
  /// stack.Push(2);
  ///
  /// int[] result = stack.PullAll();
  /// // result[0] == 2, result[1] == 1
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T[] PullAll<T>(this ConcurrentStack<T> @this) {
    Against.ThisIsNull(@this);
    var result = new List<T>();
    while (@this.TryPop(out var item))
      result.Add(item);

    return result.ToArray();
  }

  /// <summary>
  /// Copies up to <paramref name="maxCount"/> elements from the source <see cref="ConcurrentStack{T}"/> into a new array,
  /// starting from the top of the stack.
  /// </summary>
  /// <typeparam name="T">The element type contained in the stack.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentStack{T}"/> instance.</param>
  /// <param name="maxCount">
  /// The maximum number of elements to copy.  
  /// Must be greater than zero.
  /// </param>
  /// <returns>
  /// A new array containing up to <paramref name="maxCount"/> elements from the stack,  
  /// in the order they would be popped (LIFO).
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if <paramref name="maxCount"/> is less than or equal to zero.
  /// </exception>
  /// <example>
  /// <code>
  /// var stack = new ConcurrentStack&lt;string&gt;();
  /// stack.Push("third");
  /// stack.Push("second");
  /// stack.Push("first");
  ///
  /// string[] result = stack.Pull(2);
  /// // result[0] == "first", result[1] == "second"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T[] Pull<T>(this ConcurrentStack<T> @this, int maxCount) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(maxCount);

    var result = new T[maxCount];
    var span = result.AsSpan();
    var actual = PullTo(@this, span).Length;

    return actual == result.Length ? result : result.Splice(0, actual);
  }

  private static Span<T> _PullCore<T>(ConcurrentStack<T> queue, Span<T> target) {
    for (var i = 0; i < target.Length; ++i)
      if (!queue.TryPop(out target[i]))
        return i == 0 ? Span<T>.Empty : target[..i];

    return target;
  }

}
