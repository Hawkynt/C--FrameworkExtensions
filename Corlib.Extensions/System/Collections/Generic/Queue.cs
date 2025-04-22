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
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class QueueExtensions {
  
  /// <summary>
  /// Transfers elements from the source <see cref="Queue{T}"/> into the specified <see cref="Span{T}"/>,
  /// starting from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="Queue{T}"/> instance from which elements are dequeued.</param>
  /// <param name="target">
  /// The destination span to fill with elements from the queue.  
  /// If the span is empty or the queue has no elements, the operation completes with no data transfer.
  /// </param>
  /// <returns>
  /// A <see cref="Span{T}"/> representing the portion of <paramref name="target"/> that was filled with dequeued elements.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var queue = new Queue&lt;char&gt;();
  /// queue.Enqueue('A');
  /// queue.Enqueue('B');
  /// queue.Enqueue('C');
  ///
  /// Span&lt;char&gt; buffer = stackalloc char[2];
  /// var result = queue.PullTo(buffer);
  /// // result[0] == 'A', result[1] == 'B'
  /// </code>
  /// </example>

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<T> PullTo<T>(this Queue<T> @this, Span<T> target) {
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
  /// Transfers elements from the source <see cref="Queue{T}"/> into the specified target array,
  /// starting from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="Queue{T}"/> instance from which elements are dequeued.</param>
  /// <param name="target">The destination array to fill with dequeued items.</param>
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
  /// var queue = new Queue&lt;string&gt;();
  /// queue.Enqueue("first");
  /// queue.Enqueue("second");
  ///
  /// string[] result = new string[2];
  /// int count = queue.PullTo(result);
  /// // result[0] == "first", result[1] == "second", count == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this Queue<T> @this, T[] target) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    var result = Math.Min(target.Length, @this.Count);
    _PullCore(@this, target.AsSpan()[..result]);
    return result;
  }

  /// <summary>
  /// Transfers elements from the source <see cref="Queue{T}"/> into the specified target array,
  /// beginning at the specified <paramref name="offset"/> index and starting from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="Queue{T}"/> instance from which elements are dequeued.</param>
  /// <param name="target">The destination array to fill with dequeued elements.</param>
  /// <param name="offset">The index in <paramref name="target"/> at which to begin writing.</param>
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
  /// var queue = new Queue&lt;int&gt;();
  /// queue.Enqueue(10);
  /// queue.Enqueue(20);
  ///
  /// int[] buffer = new int[5];
  /// int count = queue.PullTo(buffer, 2);
  /// // buffer[2] == 10, buffer[3] == 20, count == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this Queue<T> @this, T[] target, int offset) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    Against.IndexOutOfRange(offset, target.Length - 1);
    var result = Math.Min(target.Length - offset, @this.Count);
    _PullCore(@this, target.AsSpan(offset, result));
    return result;
  }

  /// <summary>
  /// Transfers up to <paramref name="maxCount"/> elements from the source <see cref="Queue{T}"/> into the specified target array,
  /// starting at the given <paramref name="offset"/> and dequeuing from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="Queue{T}"/> instance from which elements are dequeued.</param>
  /// <param name="target">The destination array to fill with dequeued elements.</param>
  /// <param name="offset">The index in <paramref name="target"/> at which to begin writing.</param>
  /// <param name="maxCount">
  /// The maximum number of elements to copy.  
  /// Must be greater than zero and must not exceed the available space in <paramref name="target"/> starting at <paramref name="offset"/>.
  /// </param>
  /// <returns>
  /// The number of elements actually copied into <paramref name="target"/>.  
  /// This will be the lesser of <paramref name="maxCount"/> and the number of items in the queue.
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
  /// Thrown if <paramref name="maxCount"/> is less than or equal to zero,
  /// or greater than the remaining length of <paramref name="target"/> from <paramref name="offset"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var queue = new Queue&lt;string&gt;();
  /// queue.Enqueue("one");
  /// queue.Enqueue("two");
  /// queue.Enqueue("three");
  ///
  /// var buffer = new string[5];
  /// int count = queue.PullTo(buffer, 1, 2);
  /// // buffer[1] == "one", buffer[2] == "two", count == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this Queue<T> @this, T[] target, int offset, int maxCount) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    Against.IndexOutOfRange(offset, target.Length - 1);
    Against.CountOutOfRange(maxCount, target.Length - offset);
    var result = Math.Min(maxCount, @this.Count);
    _PullCore(@this, target.AsSpan(offset, result));
    return result;
  }

  /// <summary>
  /// Copies all elements from the source <see cref="Queue{T}"/> into a new array,
  /// starting from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="Queue{T}"/> instance.</param>
  /// <returns>
  /// A new array containing all elements from the queue, in the order they were enqueued.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var queue = new Queue&lt;char&gt;();
  /// queue.Enqueue('A');
  /// queue.Enqueue('B');
  ///
  /// char[] result = queue.PullAll();
  /// // result[0] == 'A', result[1] == 'B'
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T[] PullAll<T>(this Queue<T> @this) {
    Against.ThisIsNull(@this);
    var count = @this.Count;
    var result = new T[count];
    _PullCore(@this, result.AsSpan());
    return result;
  }

  /// <summary>
  /// Copies up to <paramref name="maxCount"/> elements from the source <see cref="Queue{T}"/> into a new array,
  /// starting from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="Queue{T}"/> instance.</param>
  /// <param name="maxCount">
  /// The maximum number of elements to copy.  
  /// Must be greater than zero.
  /// </param>
  /// <returns>
  /// A new array containing up to <paramref name="maxCount"/> elements from the queue,
  /// in the order they were enqueued.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if <paramref name="maxCount"/> is less than or equal to zero.
  /// </exception>
  /// <example>
  /// <code>
  /// var queue = new Queue&lt;int&gt;();
  /// queue.Enqueue(1);
  /// queue.Enqueue(2);
  /// queue.Enqueue(3);
  ///
  /// int[] result = queue.Pull(2);
  /// // result[0] == 1, result[1] == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T[] Pull<T>(this Queue<T> @this, int maxCount) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(maxCount);

    var result = new T[Math.Min(maxCount, @this.Count)];
    _PullCore(@this, result.AsSpan());
    return result;
  }

  private static void _PullCore<T>(Queue<T> queue, Span<T> target) {
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
      default: goto PullNone;
    }

    Pull0Or8:
    if (iterations-- <= 0)
      goto PullNone;

    target[offset++] = queue.Dequeue();
    Pull7: target[offset++] = queue.Dequeue();
    Pull6: target[offset++] = queue.Dequeue();
    Pull5: target[offset++] = queue.Dequeue();
    Pull4: target[offset++] = queue.Dequeue();
    Pull3: target[offset++] = queue.Dequeue();
    Pull2: target[offset++] = queue.Dequeue();
    Pull1: target[offset++] = queue.Dequeue();
    goto Pull0Or8;

    PullNone:
    ;
  }

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
