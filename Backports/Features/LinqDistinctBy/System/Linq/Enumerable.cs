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

#if !SUPPORTS_ENUMERABLE_DISTINCTBY

#if SUPPORTS_LINQ

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {
  /// <summary>
  /// Returns distinct elements from a sequence according to a specified key selector function.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
  /// <typeparam name="TKey">The type of key to distinguish elements by.</typeparam>
  /// <param name="source">The sequence to remove duplicate elements from.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <returns>An <see cref="IEnumerable{T}"/> that contains distinct elements from the source sequence.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    => DistinctBy(source, keySelector, null);

  /// <summary>
  /// Returns distinct elements from a sequence according to a specified key selector function.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
  /// <typeparam name="TKey">The type of key to distinguish elements by.</typeparam>
  /// <param name="source">The sequence to remove duplicate elements from.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare keys.</param>
  /// <returns>An <see cref="IEnumerable{T}"/> that contains distinct elements from the source sequence.</returns>
  public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));
    if (keySelector == null)
      AlwaysThrow.ArgumentNullException(nameof(keySelector));

    return _DistinctByIterator(source, keySelector, comparer);
  }

  private static IEnumerable<TSource> _DistinctByIterator<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
    var set = new HashSet<TKey>(comparer);
    foreach (var element in source)
      if (set.Add(keySelector(element)))
        yield return element;
  }

  /// <summary>
  /// Produces the set difference of two sequences according to a specified key selector function.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
  /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
  /// <param name="first">An <see cref="IEnumerable{T}"/> whose elements that are not also in <paramref name="second"/> will be returned.</param>
  /// <param name="second">An <see cref="IEnumerable{T}"/> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector)
    => ExceptBy(first, second, keySelector, null);

  /// <summary>
  /// Produces the set difference of two sequences according to a specified key selector function.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
  /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
  /// <param name="first">An <see cref="IEnumerable{T}"/> whose elements that are not also in <paramref name="second"/> will be returned.</param>
  /// <param name="second">An <see cref="IEnumerable{T}"/> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to compare values.</param>
  /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
  public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
    if (first == null)
      AlwaysThrow.ArgumentNullException(nameof(first));
    if (second == null)
      AlwaysThrow.ArgumentNullException(nameof(second));
    if (keySelector == null)
      AlwaysThrow.ArgumentNullException(nameof(keySelector));

    return _ExceptByIterator(first, second, keySelector, comparer);
  }

  private static IEnumerable<TSource> _ExceptByIterator<TSource, TKey>(IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
    var set = new HashSet<TKey>(second, comparer);
    foreach (var element in first)
      if (set.Add(keySelector(element)))
        yield return element;
  }

  /// <summary>
  /// Produces the set intersection of two sequences according to a specified key selector function.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
  /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
  /// <param name="first">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in <paramref name="second"/> will be returned.</param>
  /// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in the first sequence will be returned.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector)
    => IntersectBy(first, second, keySelector, null);

  /// <summary>
  /// Produces the set intersection of two sequences according to a specified key selector function.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
  /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
  /// <param name="first">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in <paramref name="second"/> will be returned.</param>
  /// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in the first sequence will be returned.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to compare values.</param>
  /// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
  public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
    if (first == null)
      AlwaysThrow.ArgumentNullException(nameof(first));
    if (second == null)
      AlwaysThrow.ArgumentNullException(nameof(second));
    if (keySelector == null)
      AlwaysThrow.ArgumentNullException(nameof(keySelector));

    return _IntersectByIterator(first, second, keySelector, comparer);
  }

  private static IEnumerable<TSource> _IntersectByIterator<TSource, TKey>(IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
    var set = new HashSet<TKey>(second, comparer);
    foreach (var element in first)
      if (set.Remove(keySelector(element)))
        yield return element;
  }

  /// <summary>
  /// Produces the set union of two sequences according to a specified key selector function.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
  /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
  /// <param name="first">An <see cref="IEnumerable{T}"/> whose distinct elements form the first set for the union.</param>
  /// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements form the second set for the union.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <returns>An <see cref="IEnumerable{T}"/> that contains the elements from both input sequences, excluding duplicates.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource> UnionBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
    => UnionBy(first, second, keySelector, null);

  /// <summary>
  /// Produces the set union of two sequences according to a specified key selector function.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
  /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
  /// <param name="first">An <see cref="IEnumerable{T}"/> whose distinct elements form the first set for the union.</param>
  /// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements form the second set for the union.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to compare values.</param>
  /// <returns>An <see cref="IEnumerable{T}"/> that contains the elements from both input sequences, excluding duplicates.</returns>
  public static IEnumerable<TSource> UnionBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
    if (first == null)
      AlwaysThrow.ArgumentNullException(nameof(first));
    if (second == null)
      AlwaysThrow.ArgumentNullException(nameof(second));
    if (keySelector == null)
      AlwaysThrow.ArgumentNullException(nameof(keySelector));

    return _UnionByIterator(first, second, keySelector, comparer);
  }

  private static IEnumerable<TSource> _UnionByIterator<TSource, TKey>(IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
    var set = new HashSet<TKey>(comparer);
    foreach (var element in first)
      if (set.Add(keySelector(element)))
        yield return element;

    foreach (var element in second)
      if (set.Add(keySelector(element)))
        yield return element;
  }
}

#endif

#endif
