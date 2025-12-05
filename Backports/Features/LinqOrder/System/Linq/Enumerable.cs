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

#if !SUPPORTS_ENUMERABLE_ORDER

#if SUPPORTS_LINQ

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {
  /// <summary>
  /// Sorts the elements of a sequence in ascending order.
  /// </summary>
  /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
  /// <param name="source">A sequence of values to order.</param>
  /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));

    return source.OrderBy(x => x);
  }

  /// <summary>
  /// Sorts the elements of a sequence in ascending order.
  /// </summary>
  /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
  /// <param name="source">A sequence of values to order.</param>
  /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
  /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source, IComparer<T> comparer) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));

    return source.OrderBy(x => x, comparer);
  }

  /// <summary>
  /// Sorts the elements of a sequence in descending order.
  /// </summary>
  /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
  /// <param name="source">A sequence of values to order.</param>
  /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted in descending order.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IOrderedEnumerable<T> OrderDescending<T>(this IEnumerable<T> source) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));

    return source.OrderByDescending(x => x);
  }

  /// <summary>
  /// Sorts the elements of a sequence in descending order.
  /// </summary>
  /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
  /// <param name="source">A sequence of values to order.</param>
  /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
  /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted in descending order.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IOrderedEnumerable<T> OrderDescending<T>(this IEnumerable<T> source, IComparer<T> comparer) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));

    return source.OrderByDescending(x => x, comparer);
  }
}

#endif

#endif
