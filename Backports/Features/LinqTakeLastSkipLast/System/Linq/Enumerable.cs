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

#if !SUPPORTS_ENUMERABLE_TAKELAST_SKIPLAST

#if SUPPORTS_LINQ

using System.Collections.Generic;

namespace System.Linq;

public static partial class EnumerablePolyfills {

  extension<TSource>(IEnumerable<TSource> @this) {

    /// <summary>
    /// Returns a specified number of contiguous elements from the end of a sequence.
    /// </summary>
    /// <param name="count">The number of elements to return.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the specified number of elements from the end of the input sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public IEnumerable<TSource> TakeLast(int count) {
      ArgumentNullException.ThrowIfNull(@this);
      return count <= 0 ? [] : Invoke(@this, count);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, int count) {
        // Optimize for arrays (fastest - no virtual dispatch)
        if (source is TSource[] array) {
          var start = Math.Max(0, array.Length - count);
          for (var i = start; i < array.Length; ++i)
            yield return array[i];

          yield break;
        }

        // Optimize for IList<T> with direct indexing
        if (source is IList<TSource> list) {
          var start = Math.Max(0, list.Count - count);
          for (var i = start; i < list.Count; ++i)
            yield return list[i];

          yield break;
        }

        // Use a ring buffer for general enumerable
        var queue = new Queue<TSource>(count);
        foreach (var item in source) {
          if (queue.Count == count)
            queue.Dequeue();
          queue.Enqueue(item);
        }

        foreach (var item in queue)
          yield return item;
      }
    }

    /// <summary>
    /// Returns a new enumerable collection that contains the elements from source
    /// with the last <paramref name="count"/> elements of the source collection omitted.
    /// </summary>
    /// <param name="count">The number of elements to omit from the end of the collection.</param>
    /// <returns>A new enumerable collection that contains the elements from source minus <paramref name="count"/> elements from the end of the collection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public IEnumerable<TSource> SkipLast(int count) {
      ArgumentNullException.ThrowIfNull(@this);
      return count <= 0 ? @this : Invoke(@this, count);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, int count) {
        // Optimize for arrays (fastest - no virtual dispatch)
        if (source is TSource[] array) {
          var end = Math.Max(0, array.Length - count);
          for (var i = 0; i < end; ++i)
            yield return array[i];

          yield break;
        }

        // Optimize for IList<T> with direct indexing
        if (source is IList<TSource> list) {
          var end = Math.Max(0, list.Count - count);
          for (var i = 0; i < end; ++i)
            yield return list[i];

          yield break;
        }

        // Use a ring buffer for general enumerable
        var queue = new Queue<TSource>(count);
        foreach (var item in source) {
          if (queue.Count == count)
            yield return queue.Dequeue();

          queue.Enqueue(item);
        }
      }
    }

  }

}

#endif

#endif
