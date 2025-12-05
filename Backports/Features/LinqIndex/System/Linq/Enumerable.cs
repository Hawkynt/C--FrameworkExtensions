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

#if !SUPPORTS_ENUMERABLE_INDEX

#if SUPPORTS_LINQ && (SUPPORTS_VALUE_TUPLE || OFFICIAL_VALUETUPLE)

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {
  /// <summary>
  /// Returns an enumerable that incorporates the element's index into a tuple.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
  /// <param name="source">The source enumerable providing the elements.</param>
  /// <returns>An enumerable of tuples containing the zero-based index and the element.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
  public static IEnumerable<(int Index, TSource Item)> Index<TSource>(this IEnumerable<TSource> source) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));

    return _IndexIterator(source);
  }

  private static IEnumerable<(int Index, TSource Item)> _IndexIterator<TSource>(IEnumerable<TSource> source) {
    var index = 0;
    foreach (var item in source)
      yield return (index++, item);
  }

  /// <summary>
  /// Counts the elements of a sequence by key.
  /// </summary>
  /// <typeparam name="TSource">The type of elements of <paramref name="source"/>.</typeparam>
  /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
  /// <param name="source">A sequence that contains elements to be counted.</param>
  /// <param name="keySelector">A function to extract the key from each element.</param>
  /// <returns>An enumerable containing the count of elements for each key.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<KeyValuePair<TKey, int>> CountBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    => CountBy(source, keySelector, null);

  /// <summary>
  /// Counts the elements of a sequence by key.
  /// </summary>
  /// <typeparam name="TSource">The type of elements of <paramref name="source"/>.</typeparam>
  /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
  /// <param name="source">A sequence that contains elements to be counted.</param>
  /// <param name="keySelector">A function to extract the key from each element.</param>
  /// <param name="keyComparer">An equality comparer to compare keys.</param>
  /// <returns>An enumerable containing the count of elements for each key.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
  public static IEnumerable<KeyValuePair<TKey, int>> CountBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));
    if (keySelector == null)
      AlwaysThrow.ArgumentNullException(nameof(keySelector));

    return _CountByIterator(source, keySelector, keyComparer);
  }

  private static IEnumerable<KeyValuePair<TKey, int>> _CountByIterator<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer) {
    var countByKey = new Dictionary<TKey, int>(keyComparer);
    foreach (var item in source) {
      var key = keySelector(item);
      if (countByKey.TryGetValue(key, out var count))
        countByKey[key] = count + 1;
      else
        countByKey[key] = 1;
    }

    foreach (var kvp in countByKey)
      yield return kvp;
  }

  /// <summary>
  /// Aggregates elements of a sequence by key.
  /// </summary>
  /// <typeparam name="TSource">The type of elements of <paramref name="source"/>.</typeparam>
  /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
  /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
  /// <param name="source">A sequence that contains elements to be aggregated.</param>
  /// <param name="keySelector">A function to extract the key from each element.</param>
  /// <param name="seed">A function to create the initial accumulator value for a key.</param>
  /// <param name="func">An accumulator function to be invoked on each element.</param>
  /// <returns>An enumerable containing the aggregate value for each key.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TSource, TKey, TAccumulate>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    Func<TKey, TAccumulate> seed,
    Func<TAccumulate, TSource, TAccumulate> func
  ) => AggregateBy(source, keySelector, seed, func, null);

  /// <summary>
  /// Aggregates elements of a sequence by key.
  /// </summary>
  /// <typeparam name="TSource">The type of elements of <paramref name="source"/>.</typeparam>
  /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
  /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
  /// <param name="source">A sequence that contains elements to be aggregated.</param>
  /// <param name="keySelector">A function to extract the key from each element.</param>
  /// <param name="seed">A function to create the initial accumulator value for a key.</param>
  /// <param name="func">An accumulator function to be invoked on each element.</param>
  /// <param name="keyComparer">An equality comparer to compare keys.</param>
  /// <returns>An enumerable containing the aggregate value for each key.</returns>
  public static IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TSource, TKey, TAccumulate>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    Func<TKey, TAccumulate> seed,
    Func<TAccumulate, TSource, TAccumulate> func,
    IEqualityComparer<TKey> keyComparer
  ) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));
    if (keySelector == null)
      AlwaysThrow.ArgumentNullException(nameof(keySelector));
    if (seed == null)
      AlwaysThrow.ArgumentNullException(nameof(seed));
    if (func == null)
      AlwaysThrow.ArgumentNullException(nameof(func));

    return _AggregateByIterator(source, keySelector, seed, func, keyComparer);
  }

  private static IEnumerable<KeyValuePair<TKey, TAccumulate>> _AggregateByIterator<TSource, TKey, TAccumulate>(
    IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    Func<TKey, TAccumulate> seed,
    Func<TAccumulate, TSource, TAccumulate> func,
    IEqualityComparer<TKey> keyComparer
  ) {
    var dict = new Dictionary<TKey, TAccumulate>(keyComparer);
    foreach (var item in source) {
      var key = keySelector(item);
      if (!dict.TryGetValue(key, out var accumulate))
        accumulate = seed(key);

      dict[key] = func(accumulate, item);
    }

    foreach (var kvp in dict)
      yield return kvp;
  }

  /// <summary>
  /// Aggregates elements of a sequence by key.
  /// </summary>
  /// <typeparam name="TSource">The type of elements of <paramref name="source"/>.</typeparam>
  /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
  /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
  /// <param name="source">A sequence that contains elements to be aggregated.</param>
  /// <param name="keySelector">A function to extract the key from each element.</param>
  /// <param name="seed">The initial accumulator value for all keys.</param>
  /// <param name="func">An accumulator function to be invoked on each element.</param>
  /// <returns>An enumerable containing the aggregate value for each key.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TSource, TKey, TAccumulate>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    TAccumulate seed,
    Func<TAccumulate, TSource, TAccumulate> func
  ) => AggregateBy(source, keySelector, seed, func, null);

  /// <summary>
  /// Aggregates elements of a sequence by key.
  /// </summary>
  /// <typeparam name="TSource">The type of elements of <paramref name="source"/>.</typeparam>
  /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
  /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
  /// <param name="source">A sequence that contains elements to be aggregated.</param>
  /// <param name="keySelector">A function to extract the key from each element.</param>
  /// <param name="seed">The initial accumulator value for all keys.</param>
  /// <param name="func">An accumulator function to be invoked on each element.</param>
  /// <param name="keyComparer">An equality comparer to compare keys.</param>
  /// <returns>An enumerable containing the aggregate value for each key.</returns>
  public static IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TSource, TKey, TAccumulate>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    TAccumulate seed,
    Func<TAccumulate, TSource, TAccumulate> func,
    IEqualityComparer<TKey> keyComparer
  ) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));
    if (keySelector == null)
      AlwaysThrow.ArgumentNullException(nameof(keySelector));
    if (func == null)
      AlwaysThrow.ArgumentNullException(nameof(func));

    return _AggregateByWithSeedValueIterator(source, keySelector, seed, func, keyComparer);
  }

  private static IEnumerable<KeyValuePair<TKey, TAccumulate>> _AggregateByWithSeedValueIterator<TSource, TKey, TAccumulate>(
    IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    TAccumulate seed,
    Func<TAccumulate, TSource, TAccumulate> func,
    IEqualityComparer<TKey> keyComparer
  ) {
    var dict = new Dictionary<TKey, TAccumulate>(keyComparer);
    foreach (var item in source) {
      var key = keySelector(item);
      if (!dict.TryGetValue(key, out var accumulate))
        accumulate = seed;

      dict[key] = func(accumulate, item);
    }

    foreach (var kvp in dict)
      yield return kvp;
  }
}

#endif

#endif
