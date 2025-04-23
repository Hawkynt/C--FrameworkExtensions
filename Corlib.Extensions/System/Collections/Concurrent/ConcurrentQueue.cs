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

using System.Buffers;
using Guard;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions=Utilities.MethodImplOptions;

namespace System.Collections.Concurrent;

public static partial class ConcurrentQueueExtensions {

  /// <summary>
  /// Transfers elements from the source <see cref="ConcurrentQueue{T}"/> into the specified <see cref="Span{T}"/>,
  /// starting from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentQueue{T}"/> instance from which elements are dequeued.</param>
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
  /// var queue = new ConcurrentQueue&lt;int&gt;();
  /// queue.Enqueue(10);
  /// queue.Enqueue(20);
  ///
  /// Span&lt;int&gt; buffer = stackalloc int[2];
  /// var result = queue.PullTo(buffer);
  /// // result[0] == 10, result[1] == 20
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<T> PullTo<T>(this ConcurrentQueue<T> @this, Span<T> target) {
    Against.ThisIsNull(@this);
    return target.IsEmpty ? target : _PullCore(@this, target);
  }

  /// <summary>
  /// Transfers elements from the source <see cref="ConcurrentQueue{T}"/> into the specified target array,
  /// starting from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentQueue{T}"/> instance from which elements are dequeued.</param>
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
  /// var queue = new ConcurrentQueue&lt;string&gt;();
  /// queue.Enqueue("alpha");
  /// queue.Enqueue("beta");
  ///
  /// string[] buffer = new string[2];
  /// int copied = queue.PullTo(buffer);
  /// // buffer[0] == "alpha", buffer[1] == "beta", copied == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this ConcurrentQueue<T> @this, T[] target) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    return PullTo(@this, target.AsSpan()).Length;
  }

  /// <summary>
  /// Transfers elements from the source <see cref="ConcurrentQueue{T}"/> into the specified target array,
  /// beginning at the specified <paramref name="offset"/> and starting from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentQueue{T}"/> instance from which elements are dequeued.</param>
  /// <param name="target">The destination array to fill with dequeued elements.</param>
  /// <param name="offset">The zero-based index in <paramref name="target"/> at which to begin writing.</param>
  /// <returns>
  /// The number of elements copied into <paramref name="target"/> starting at <paramref name="offset"/>.
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
  /// var queue = new ConcurrentQueue&lt;int&gt;();
  /// queue.Enqueue(1);
  /// queue.Enqueue(2);
  /// queue.Enqueue(3);
  ///
  /// int[] buffer = new int[5];
  /// int copied = queue.PullTo(buffer, 2);
  /// // buffer[2] == 1, buffer[3] == 2, buffer[4] == 3, copied == 3
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this ConcurrentQueue<T> @this, T[] target, int offset) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    Against.IndexOutOfRange(offset, target.Length - 1);
    return PullTo(@this, target.AsSpan(offset)).Length;
  }

  /// <summary>
  /// Transfers up to <paramref name="maxCount"/> elements from the source <see cref="ConcurrentQueue{T}"/> into the specified target array,
  /// starting at the given <paramref name="offset"/> and dequeuing from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentQueue{T}"/> instance from which elements are dequeued.</param>
  /// <param name="target">The destination array to fill with dequeued elements.</param>
  /// <param name="offset">The index in <paramref name="target"/> at which to begin writing.</param>
  /// <param name="maxCount">
  /// The maximum number of elements to copy.  
  /// Must be greater than zero and must not exceed the space remaining in <paramref name="target"/> from <paramref name="offset"/>.
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
  /// Thrown if <paramref name="offset"/> is outside the bounds of the <paramref name="target"/> array.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown if <paramref name="maxCount"/> is less than or equal to zero,
  /// or greater than the remaining capacity of <paramref name="target"/> from <paramref name="offset"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var queue = new ConcurrentQueue&lt;string&gt;();
  /// queue.Enqueue("X");
  /// queue.Enqueue("Y");
  /// queue.Enqueue("Z");
  ///
  /// var buffer = new string[5];
  /// int copied = queue.PullTo(buffer, 1, 2);
  /// // buffer[1] == "X", buffer[2] == "Y", copied == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PullTo<T>(this ConcurrentQueue<T> @this, T[] target, int offset, int maxCount) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    Against.IndexOutOfRange(offset, target.Length - 1);
    Against.CountOutOfRange(maxCount, target.Length - offset);
    return PullTo(@this, target.AsSpan(offset, maxCount)).Length;
  }

  /// <summary>
  /// Copies all elements from the source <see cref="ConcurrentQueue{T}"/> into a new array,
  /// starting from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentQueue{T}"/> instance.</param>
  /// <returns>
  /// A new array containing all elements from the queue, in the order they were enqueued.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// var queue = new ConcurrentQueue&lt;int&gt;();
  /// queue.Enqueue(1);
  /// queue.Enqueue(2);
  ///
  /// int[] result = queue.PullAll();
  /// // result[0] == 1, result[1] == 2
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T[] PullAll<T>(this ConcurrentQueue<T> @this) {
    Against.ThisIsNull(@this);
    var result = new List<T>();
    while (@this.TryDequeue(out var item))
      result.Add(item);

    return result.ToArray();
  }

  /// <summary>
  /// Copies up to <paramref name="maxCount"/> elements from the source <see cref="ConcurrentQueue{T}"/> into a new array,
  /// starting from the front of the queue.
  /// </summary>
  /// <typeparam name="T">The element type contained in the queue.</typeparam>
  /// <param name="this">The source <see cref="ConcurrentQueue{T}"/> instance.</param>
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
  /// var queue = new ConcurrentQueue&lt;char&gt;();
  /// queue.Enqueue('A');
  /// queue.Enqueue('B');
  /// queue.Enqueue('C');
  ///
  /// char[] result = queue.Pull(2);
  /// // result[0] == 'A', result[1] == 'B'
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T[] Pull<T>(this ConcurrentQueue<T> @this, int maxCount) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(maxCount);

    const int FIRST_BLOCK_SIZE = 64;
    if (maxCount <= FIRST_BLOCK_SIZE) {
      using var array = ArrayPool<T>.Shared.Use(maxCount);
      return _PullCore(@this, array).ToArray();
    }
    
    const int MAX_PARTS = 26; // log2 int.maxValue except the 64 we already use at start, because array can't grow beyond that in C#
    var chunks = ArrayPool<T[]>.Shared.Rent(MAX_PARTS);

    var itemCount = 0;
    var currentChunkIndex = -1;
    var indexInCurrentChunk = 0;

    while (maxCount-- > 0 && @this.TryDequeue(out var item)) {
      
      // no parts yet, allocate the first part
      if (currentChunkIndex < 0) {
        currentChunkIndex = 0;

        var array = ArrayPool<T>.Shared.Rent(FIRST_BLOCK_SIZE);
        chunks[currentChunkIndex] = array;
      }

      // current part is full
      if (indexInCurrentChunk >= chunks[currentChunkIndex].Length) {
        
        // avoid overpooling by calculating how many items we still need to store at most, capping at double the size of the previous segment
        var newSize = Math.Min(maxCount + 1, chunks[currentChunkIndex].Length * 2);
        var array = ArrayPool<T>.Shared.Rent(newSize);
        chunks[++currentChunkIndex] = array;
        indexInCurrentChunk = 0;
      }

      chunks[currentChunkIndex][indexInCurrentChunk++] = item;
      ++itemCount;
    }

    // no items pulled
    if (currentChunkIndex < 0)
      return [];

    // copy to result
    var result = new T[itemCount];
    itemCount = 0;
    
    // copy full parts
    for (var i = 0; i < currentChunkIndex; ++i) {
      var part= chunks[i].AsSpan();
      part.CopyTo(result.AsSpan(itemCount));
      itemCount += part.Length;
    }

    // copy current (partial) part
    chunks[currentChunkIndex].AsSpan(0,indexInCurrentChunk).CopyTo(result.AsSpan(itemCount));

    // release all pooled arrays
    for (var i = 0; i <= currentChunkIndex; ++i) {
      ArrayPool<T>.Shared.Return(chunks[i]);
      chunks[i] = null;
    }

    ArrayPool<T[]>.Shared.Return(chunks);

    return result;
  }

  private static Span<T> _PullCore<T>(ConcurrentQueue<T> queue, Span<T> target) {
    for (var i = 0; i < target.Length; ++i)
      if (!queue.TryDequeue(out target[i]))
        return i == 0 ? Span<T>.Empty : target[..i];

    return target;
  }

}
