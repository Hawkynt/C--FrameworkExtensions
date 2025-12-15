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

#if !SUPPORTS_ENUMERABLE_WAVE9

using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if !SUPPORTS_GENERIC_MATH
using System.Runtime.Intrinsics;
#else
using System.Numerics;
#endif
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {

  extension<TSource>(IEnumerable<TSource> @this) {

    /// <summary>
    /// Returns a new sequence with the elements in random order.
    /// </summary>
    /// <returns>A sequence with the elements in random order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="@this"/> is <see langword="null"/>.</exception>
    public IEnumerable<TSource> Shuffle() {
      ArgumentNullException.ThrowIfNull(@this);

      var list = @this.ToList();
      var random = Random.Shared;
      var n = list.Count;
      while (n > 1) {
        --n;
        var k = random.Next(n + 1);
        (list[k], list[n]) = (list[n], list[k]);
      }

      return list;
    }

    /// <summary>
    /// Performs a left outer join on two sequences based on matching keys.
    /// </summary>
    /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="inner">The sequence to join to the first sequence.</param>
    /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
    /// <returns>An enumerable that has elements of type TResult obtained by performing a left outer join on two sequences.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TResult> LeftJoin<TInner, TKey, TResult>(
      IEnumerable<TInner> inner,
      Func<TSource, TKey> outerKeySelector,
      Func<TInner, TKey> innerKeySelector,
      Func<TSource, TInner, TResult> resultSelector
    ) => @this.LeftJoin(inner, outerKeySelector, innerKeySelector, resultSelector, null);

    /// <summary>
    /// Performs a left outer join on two sequences based on matching keys.
    /// </summary>
    /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="inner">The sequence to join to the first sequence.</param>
    /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
    /// <param name="comparer">An equality comparer to compare keys.</param>
    /// <returns>An enumerable that has elements of type TResult obtained by performing a left outer join on two sequences.</returns>
    public IEnumerable<TResult> LeftJoin<TInner, TKey, TResult>(
      IEnumerable<TInner> inner,
      Func<TSource, TKey> outerKeySelector,
      Func<TInner, TKey> innerKeySelector,
      Func<TSource, TInner, TResult> resultSelector,
      IEqualityComparer<TKey> comparer
    ) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(inner);
      ArgumentNullException.ThrowIfNull(outerKeySelector);
      ArgumentNullException.ThrowIfNull(innerKeySelector);
      ArgumentNullException.ThrowIfNull(resultSelector);

      return Invoke(@this, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

      static IEnumerable<TResult> Invoke(
        IEnumerable<TSource> outer,
        IEnumerable<TInner> inner,
        Func<TSource, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TSource, TInner, TResult> resultSelector,
        IEqualityComparer<TKey> comparer
      ) {
        var lookup = inner.ToLookup(innerKeySelector, comparer);
        foreach (var outerItem in outer) {
          var key = outerKeySelector(outerItem);
          var innerItems = lookup[key];
          if (innerItems.Any())
            foreach (var innerItem in innerItems)
              yield return resultSelector(outerItem, innerItem);
          else
            yield return resultSelector(outerItem, default);
        }
      }
    }

    /// <summary>
    /// Performs a right outer join on two sequences based on matching keys.
    /// </summary>
    /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="inner">The sequence to join to the first sequence.</param>
    /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
    /// <returns>An enumerable that has elements of type TResult obtained by performing a right outer join on two sequences.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TResult> RightJoin<TInner, TKey, TResult>(
      IEnumerable<TInner> inner,
      Func<TSource, TKey> outerKeySelector,
      Func<TInner, TKey> innerKeySelector,
      Func<TSource, TInner, TResult> resultSelector
    ) => @this.RightJoin(inner, outerKeySelector, innerKeySelector, resultSelector, null);

    /// <summary>
    /// Performs a right outer join on two sequences based on matching keys.
    /// </summary>
    /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="inner">The sequence to join to the first sequence.</param>
    /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
    /// <param name="comparer">An equality comparer to compare keys.</param>
    /// <returns>An enumerable that has elements of type TResult obtained by performing a right outer join on two sequences.</returns>
    public IEnumerable<TResult> RightJoin<TInner, TKey, TResult>(
      IEnumerable<TInner> inner,
      Func<TSource, TKey> outerKeySelector,
      Func<TInner, TKey> innerKeySelector,
      Func<TSource, TInner, TResult> resultSelector,
      IEqualityComparer<TKey> comparer
    ) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(inner);
      ArgumentNullException.ThrowIfNull(outerKeySelector);
      ArgumentNullException.ThrowIfNull(innerKeySelector);
      ArgumentNullException.ThrowIfNull(resultSelector);

      return Invoke(@this, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

      static IEnumerable<TResult> Invoke(
        IEnumerable<TSource> outer,
        IEnumerable<TInner> inner,
        Func<TSource, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TSource, TInner, TResult> resultSelector,
        IEqualityComparer<TKey> comparer
      ) {
        var lookup = outer.ToLookup(outerKeySelector, comparer);
        foreach (var innerItem in inner) {
          var key = innerKeySelector(innerItem);
          var outerItems = lookup[key];
          if (outerItems.Any())
            foreach (var outerItem in outerItems)
              yield return resultSelector(outerItem, innerItem);
          else
            yield return resultSelector(default, innerItem);
        }
      }
    }

  }

  extension<TSource>(TSource[] @this) {

    /// <summary>
    /// Inverts the order of the elements in a sequence.
    /// </summary>
    /// <returns>A sequence whose elements correspond to those of the input sequence in reverse order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="@this"/> is <see langword="null"/>.</exception>
    public IEnumerable<TSource> Reverse() {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this);

      static IEnumerable<TSource> Invoke(TSource[] source) {
        for (var i = source.Length - 1; i >= 0; --i)
          yield return source[i];
      }
    }

  }

#if SUPPORTS_GENERIC_MATH

  extension<TSource>(IEnumerable<TSource>) where TSource : IAdditionOperators<TSource, TSource, TSource> {

    /// <summary>
    /// Generates an infinite sequence of values starting at a specified value with a specified step.
    /// </summary>
    /// <typeparam name="T">The type of the values to generate.</typeparam>
    /// <param name="start">The value of the first element in the sequence.</param>
    /// <param name="step">The step to add to each subsequent element.</param>
    /// <returns>An infinite enumerable of values.</returns>
    public static IEnumerable<TSource> InfiniteSequence(TSource start, TSource step) {
      return Invoke(start, step);

      static IEnumerable<TSource> Invoke(TSource start, TSource step) {
        var current = start;
        while (true) {
          yield return current;
          current += step;
        }
      }
    }

  }
  
  extension<TSource>(IEnumerable<TSource>) where TSource : IAdditionOperators<TSource, TSource, TSource>, IComparisonOperators<TSource, TSource, bool> {

    /// <summary>
    /// Generates a finite sequence of values from a start value to an end value (inclusive) with a specified step.
    /// </summary>
    /// <typeparam name="T">The type of the values to generate.</typeparam>
    /// <param name="start">The value of the first element in the sequence.</param>
    /// <param name="endInclusive">The maximum value of elements in the sequence (inclusive).</param>
    /// <param name="step">The step to add to each subsequent element.</param>
    /// <returns>An enumerable of values from start to end.</returns>
    public static IEnumerable<TSource> Sequence(TSource start, TSource endInclusive, TSource step) {
      return Invoke(start, endInclusive, step);

      static IEnumerable<TSource> Invoke(TSource start, TSource endInclusive, TSource step) {
        for (var current = start; current <= endInclusive; current += step)
          yield return current;
      }
    }
  }

#else

  extension<TSource>(IEnumerable<TSource>) {

    /// <summary>
    /// Generates an infinite sequence of values starting at a specified value with a specified step.
    /// </summary>
    /// <typeparam name="T">The type of the values to generate. Must be a standard numeric type.</typeparam>
    /// <param name="start">The value of the first element in the sequence.</param>
    /// <param name="step">The step to add to each subsequent element.</param>
    /// <returns>An infinite enumerable of values.</returns>
    public static IEnumerable<TSource> InfiniteSequence(TSource start, TSource step) {
      return Invoke(start, step);

      static IEnumerable<TSource> Invoke(TSource start, TSource step) {
        var current = start;
        while (true) {
          yield return current;
          current = Scalar<TSource>.Add(current, step);
        }
      }
    }

    /// <summary>
    /// Generates a finite sequence of values from a start value to an end value (inclusive) with a specified step.
    /// </summary>
    /// <typeparam name="T">The type of the values to generate. Must be a standard numeric type.</typeparam>
    /// <param name="start">The value of the first element in the sequence.</param>
    /// <param name="endInclusive">The maximum value of elements in the sequence (inclusive).</param>
    /// <param name="step">The step to add to each subsequent element.</param>
    /// <returns>An enumerable of values from start to end.</returns>
    public static IEnumerable<TSource> Sequence(TSource start, TSource endInclusive, TSource step) {
      return Invoke(start, endInclusive, step);

      static IEnumerable<TSource> Invoke(TSource start, TSource endInclusive, TSource step) {
        for (var current = start; Scalar<TSource>.LessThanOrEqual(current, endInclusive); current = Scalar<TSource>.Add(current, step))
          yield return current;
      }
    }
  }

#endif

}

#endif
