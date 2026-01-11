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

#if !SUPPORTS_LINQ_ELEMENTAT_INDEX

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {

  extension<TSource>(IEnumerable<TSource> @this) {

    /// <summary>
    /// Returns the element at a specified index in a sequence.
    /// </summary>
    /// <param name="index">The index of the element to retrieve, which can be from the end of the sequence.</param>
    /// <returns>The element at the specified position in the source sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="this"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the bounds of the sequence.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSource ElementAt(Index index) {
      ArgumentNullException.ThrowIfNull(@this);

      if (!index.IsFromEnd)
        return @this.ElementAt(index.Value);

      if (@this is not IList<TSource> list)
        return ElementAtFromEnd(@this, index.Value);

      var actualIndex = list.Count - index.Value;
      ArgumentOutOfRangeException.ThrowIfNegative(actualIndex);
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(actualIndex, list.Count);

      return list[actualIndex];

      static TSource ElementAtFromEnd(IEnumerable<TSource> source, int offset) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(offset);

        var queue = new Queue<TSource>(offset);
        foreach (var item in source) {
          if (queue.Count == offset)
            queue.Dequeue();
          queue.Enqueue(item);
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(offset, queue.Count);
        return queue.Dequeue();
      }
    }

    /// <summary>
    /// Returns the element at a specified index in a sequence or a default value if the index is out of range.
    /// </summary>
    /// <param name="index">The index of the element to retrieve, which can be from the end of the sequence.</param>
    /// <returns>
    /// The element at the specified position in the source sequence, or <see langword="default"/> if the index is out of range.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="this"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSource? ElementAtOrDefault(Index index) {
      ArgumentNullException.ThrowIfNull(@this);

      if (!index.IsFromEnd)
        return @this.ElementAtOrDefault(index.Value);

      if (@this is IList<TSource> list) {
        var actualIndex = list.Count - index.Value;
        if (actualIndex < 0 || actualIndex >= list.Count)
          return default;
        return list[actualIndex];
      }

      return ElementAtOrDefaultFromEnd(@this, index.Value);

      static TSource? ElementAtOrDefaultFromEnd(IEnumerable<TSource> source, int offset) {
        if (offset <= 0)
          return default;

        var queue = new Queue<TSource>(offset);
        foreach (var item in source) {
          if (queue.Count == offset)
            queue.Dequeue();
          queue.Enqueue(item);
        }

        if (queue.Count < offset)
          return default;

        return queue.Dequeue();
      }
    }

  }

}

#endif
