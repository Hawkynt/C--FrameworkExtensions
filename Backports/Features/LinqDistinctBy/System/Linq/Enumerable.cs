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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {

  extension<TSource>(IEnumerable<TSource> @this) {

    /// <summary>
    /// Returns distinct elements from a sequence according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TKey">The type of key to distinguish elements by.</typeparam>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains distinct elements from the source sequence.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> DistinctBy<TKey>(Func<TSource, TKey> keySelector)
      => @this.DistinctBy(keySelector, null);

    /// <summary>
    /// Returns distinct elements from a sequence according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TKey">The type of key to distinguish elements by.</typeparam>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare keys.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains distinct elements from the source sequence.</returns>
    public IEnumerable<TSource> DistinctBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);

      return Invoke(@this, keySelector, comparer);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
        var set = new HashSet<TKey>(comparer);
        foreach (var element in source)
          if (set.Add(keySelector(element)))
            yield return element;
      }
    }

    /// <summary>
    /// Produces the set difference of two sequences according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
    /// <param name="second">An <see cref="IEnumerable{T}"/> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> ExceptBy<TKey>(IEnumerable<TKey> second, Func<TSource, TKey> keySelector)
      => @this.ExceptBy(second, keySelector, null);

    /// <summary>
    /// Produces the set difference of two sequences according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
    /// <param name="second">An <see cref="IEnumerable{T}"/> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to compare values.</param>
    /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
    public IEnumerable<TSource> ExceptBy<TKey>(IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(second);
      ArgumentNullException.ThrowIfNull(keySelector);

      return Invoke(@this, second, keySelector, comparer);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
        var set = new HashSet<TKey>(second, comparer);
        foreach (var element in first)
          if (set.Add(keySelector(element)))
            yield return element;
      }
    }

    /// <summary>
    /// Produces the set intersection of two sequences according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
    /// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in the first sequence will be returned.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> IntersectBy<TKey>(IEnumerable<TKey> second, Func<TSource, TKey> keySelector)
      => @this.IntersectBy(second, keySelector, null);

    /// <summary>
    /// Produces the set intersection of two sequences according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
    /// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in the first sequence will be returned.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to compare values.</param>
    /// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
    public IEnumerable<TSource> IntersectBy<TKey>(IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(second);
      ArgumentNullException.ThrowIfNull(keySelector);

      return Invoke(@this, second, keySelector, comparer);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
        var set = new HashSet<TKey>(second, comparer);
        foreach (var element in first)
          if (set.Remove(keySelector(element)))
            yield return element;
      }
    }

    /// <summary>
    /// Produces the set union of two sequences according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
    /// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements form the second set for the union.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the elements from both input sequences, excluding duplicates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> UnionBy<TKey>(IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
      => @this.UnionBy(second, keySelector, null);

    /// <summary>
    /// Produces the set union of two sequences according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TKey">The type of key to identify elements by.</typeparam>
    /// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements form the second set for the union.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to compare values.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the elements from both input sequences, excluding duplicates.</returns>
    public IEnumerable<TSource> UnionBy<TKey>(IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(second);
      ArgumentNullException.ThrowIfNull(keySelector);

      return Invoke(@this, second, keySelector, comparer);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
        var set = new HashSet<TKey>(comparer);
        foreach (var element in first)
          if (set.Add(keySelector(element)))
            yield return element;

        foreach (var element in second)
          if (set.Add(keySelector(element)))
            yield return element;
      }
    }

  }

}

#endif

#endif
