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

#if !SUPPORTS_ENUMERABLE_CHUNK

#if SUPPORTS_LINQ

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {
  /// <summary>
  /// Splits the elements of a sequence into chunks of size at most <paramref name="size"/>.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of source.</typeparam>
  /// <param name="source">An <see cref="IEnumerable{T}"/> whose elements to chunk.</param>
  /// <param name="size">Maximum size of each chunk.</param>
  /// <returns>An <see cref="IEnumerable{T}"/> that contains the elements the input sequence split into chunks of size <paramref name="size"/>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is below 1.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));
    if (size < 1)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(size), "Size must be at least 1.");

    return _ChunkIterator(source, size);
  }

  private static IEnumerable<TSource[]> _ChunkIterator<TSource>(IEnumerable<TSource> source, int size) {
    using var enumerator = source.GetEnumerator();
    while (enumerator.MoveNext()) {
      var chunk = new TSource[size];
      chunk[0] = enumerator.Current;

      var i = 1;
      for (; i < size && enumerator.MoveNext(); ++i)
        chunk[i] = enumerator.Current;

      if (i < size)
        Array.Resize(ref chunk, i);

      yield return chunk;
    }
  }
}

#endif

#endif
