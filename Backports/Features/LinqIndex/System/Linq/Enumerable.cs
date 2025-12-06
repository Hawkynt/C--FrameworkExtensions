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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {

  extension<TSource>(IEnumerable<TSource> @this) {

    /// <summary>
    /// Returns an enumerable that incorporates the element's index into a tuple.
    /// </summary>
    /// <returns>An enumerable of tuples containing the zero-based index and the element.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="@this"/> is <see langword="null"/>.</exception>
    public IEnumerable<(int Index, TSource Item)> Index() {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this);

      static IEnumerable<(int Index, TSource Item)> Invoke(IEnumerable<TSource> source) {
        var index = 0;
        foreach (var item in source)
          yield return (index++, item);
      }
    }

    /// <summary>
    /// Counts the elements of a sequence by key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <returns>An enumerable containing the count of elements for each key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="@this"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<KeyValuePair<TKey, int>> CountBy<TKey>(Func<TSource, TKey> keySelector)
      => @this.CountBy(keySelector, null);

    /// <summary>
    /// Counts the elements of a sequence by key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <param name="keyComparer">An equality comparer to compare keys.</param>
    /// <returns>An enumerable containing the count of elements for each key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="@this"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public IEnumerable<KeyValuePair<TKey, int>> CountBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);

      return Invoke(@this, keySelector, keyComparer);

      static IEnumerable<KeyValuePair<TKey, int>> Invoke(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer) {
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
    }

    /// <summary>
    /// Aggregates elements of a sequence by key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <param name="seed">A function to create the initial accumulator value for a key.</param>
    /// <param name="func">An accumulator function to be invoked on each element.</param>
    /// <returns>An enumerable containing the aggregate value for each key.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(
      Func<TSource, TKey> keySelector,
      Func<TKey, TAccumulate> seed,
      Func<TAccumulate, TSource, TAccumulate> func
    ) => @this.AggregateBy(keySelector, seed, func, null);

    /// <summary>
    /// Aggregates elements of a sequence by key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <param name="seed">A function to create the initial accumulator value for a key.</param>
    /// <param name="func">An accumulator function to be invoked on each element.</param>
    /// <param name="keyComparer">An equality comparer to compare keys.</param>
    /// <returns>An enumerable containing the aggregate value for each key.</returns>
    public IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(
      Func<TSource, TKey> keySelector,
      Func<TKey, TAccumulate> seed,
      Func<TAccumulate, TSource, TAccumulate> func,
      IEqualityComparer<TKey> keyComparer
    ) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);
      ArgumentNullException.ThrowIfNull(seed);
      ArgumentNullException.ThrowIfNull(func);

      return Invoke(@this, keySelector, seed, func, keyComparer);

      static IEnumerable<KeyValuePair<TKey, TAccumulate>> Invoke(
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
    }

    /// <summary>
    /// Aggregates elements of a sequence by key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <param name="seed">The initial accumulator value for all keys.</param>
    /// <param name="func">An accumulator function to be invoked on each element.</param>
    /// <returns>An enumerable containing the aggregate value for each key.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(
      Func<TSource, TKey> keySelector,
      TAccumulate seed,
      Func<TAccumulate, TSource, TAccumulate> func
    ) => @this.AggregateBy(keySelector, seed, func, null);

    /// <summary>
    /// Aggregates elements of a sequence by key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <param name="seed">The initial accumulator value for all keys.</param>
    /// <param name="func">An accumulator function to be invoked on each element.</param>
    /// <param name="keyComparer">An equality comparer to compare keys.</param>
    /// <returns>An enumerable containing the aggregate value for each key.</returns>
    public IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(
      Func<TSource, TKey> keySelector,
      TAccumulate seed,
      Func<TAccumulate, TSource, TAccumulate> func,
      IEqualityComparer<TKey> keyComparer
    ) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);
      ArgumentNullException.ThrowIfNull(func);

      return Invoke(@this, keySelector, seed, func, keyComparer);

      static IEnumerable<KeyValuePair<TKey, TAccumulate>> Invoke(
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

  }

}

#endif

#endif
