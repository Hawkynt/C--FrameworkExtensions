#region (c)2010-2042 Hawkynt

/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

#if !SUPPORTS_MINMAX_BY

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

namespace System.Collections.Generic;

// ReSharper disable UnusedMember.Global
// ReSharper disable once PartialTypeWithSinglePart
public static partial class EnumerablePolyfills {

  /// <summary>Returns the maximum value in a generic sequence according to a specified key selector function.</summary>
  /// <param name="this">A sequence of values to determine the maximum value of.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <typeparam name="TItem">The type of the elements of <paramref name="this" />.</typeparam>
  /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">No key extracted from <paramref name="this" /> implements the <see cref="T:System.IComparable" /> or <see cref="T:System.IComparable{T}" /> interface.</exception>
  /// <returns>The value with the maximum key in the sequence.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem MaxBy<TItem, TKey>(this IEnumerable<TItem> @this, Func<TItem, TKey> keySelector)
    => MaxBy(@this, keySelector, null)
    ;

  /// <summary>Returns the maximum value in a generic sequence according to a specified key selector function.</summary>
  /// <param name="this">A sequence of values to determine the maximum value of.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> to compare keys.</param>
  /// <typeparam name="TItem">The type of the elements of <paramref name="this" />.</typeparam>
  /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">No key extracted from <paramref name="this" /> implements the <see cref="T:System.IComparable" /> or <see cref="T:System.IComparable{T}" /> interface.</exception>
  /// <returns>The value with the maximum key in the sequence.</returns>
  public static TItem MaxBy<TItem, TKey>(this IEnumerable<TItem> @this, Func<TItem, TKey> keySelector, IComparer<TKey> comparer) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (keySelector == null)
      throw new ArgumentNullException(nameof(keySelector));
  
    comparer ??= Comparer<TKey>.Default;
    using var enumerator = @this.GetEnumerator();
    if (!enumerator.MoveNext()) {
      if (default(TItem) == null)
        return default;

      throw new InvalidOperationException("The sequence contains no elements.");
    }

    var source1 = enumerator.Current;
    var y = keySelector(source1);
    if (default(TKey) == null) {
      for (; y == null; y = keySelector(source1)) {
        if (!enumerator.MoveNext())
          return source1;

        source1 = enumerator.Current;
      }

      while (enumerator.MoveNext()) {
        var current = enumerator.Current;
        var x = keySelector(current);
        if (x == null || comparer.Compare(x, y) <= 0)
          continue;

        y = x;
        source1 = current;
      }

    } else if (ReferenceEquals(comparer, Comparer<TKey>.Default)) {
      while (enumerator.MoveNext()) {
        var current = enumerator.Current;
        var x = keySelector(current);
        if (Comparer<TKey>.Default.Compare(x, y) <= 0)
          continue;

        y = x;
        source1 = current;
      }

    } else {
      while (enumerator.MoveNext()) {
        var current = enumerator.Current;
        var x = keySelector(current);
        if (comparer.Compare(x, y) <= 0)
          continue;

        y = x;
        source1 = current;
      }
    }

    return source1;
  }

  /// <summary>Returns the minimum value in a generic sequence according to a specified key selector function.</summary>
  /// <param name="this">A sequence of values to determine the minimum value of.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <typeparam name="TItem">The type of the elements of <paramref name="this" />.</typeparam>
  /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">No key extracted from <paramref name="this" /> implements the <see cref="T:System.IComparable" /> or <see cref="T:System.IComparable{T}" /> interface.</exception>
  /// <returns>The value with the minimum key in the sequence.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem MinBy<TItem, TKey>(this IEnumerable<TItem> @this, Func<TItem, TKey> keySelector)
    => MinBy(@this, keySelector, null)
    ;

  /// <summary>Returns the minimum value in a generic sequence according to a specified key selector function.</summary>
  /// <param name="this">A sequence of values to determine the minimum value of.</param>
  /// <param name="keySelector">A function to extract the key for each element.</param>
  /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> to compare keys.</param>
  /// <typeparam name="TItem">The type of the elements of <paramref name="this" />.</typeparam>
  /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">No key extracted from <paramref name="this" /> implements the <see cref="T:System.IComparable" /> or <see cref="T:System.IComparable{T}" /> interface.</exception>
  /// <returns>The value with the minimum key in the sequence.</returns>
  public static TItem MinBy<TItem, TKey>(this IEnumerable<TItem> @this, Func<TItem, TKey> keySelector, IComparer<TKey> comparer) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (keySelector == null)
      throw new ArgumentNullException(nameof(keySelector));

    comparer ??= Comparer<TKey>.Default;
    using var enumerator = @this.GetEnumerator();
    if (!enumerator.MoveNext()) {
      if (default(TItem) == null)
        return default;

      throw new InvalidOperationException("The sequence contains no elements.");
    }

    var source1 = enumerator.Current;
    var y = keySelector(source1);
    if (default(TKey) == null) {
      for (; y == null; y = keySelector(source1)) {
        if (!enumerator.MoveNext())
          return source1;

        source1 = enumerator.Current;
      }

      while (enumerator.MoveNext()) {
        var current = enumerator.Current;
        var x = keySelector(current);
        if (x == null || comparer.Compare(x, y) >= 0)
          continue;

        y = x;
        source1 = current;
      }

    } else if (ReferenceEquals(comparer, Comparer<TKey>.Default)) {
      while (enumerator.MoveNext()) {
        var current = enumerator.Current;
        var x = keySelector(current);
        if (Comparer<TKey>.Default.Compare(x, y) >= 0)
          continue;

        y = x;
        source1 = current;
      }

    } else {
      while (enumerator.MoveNext()) {
        var current = enumerator.Current;
        var x = keySelector(current);
        if (comparer.Compare(x, y) >= 0)
          continue;

        y = x;
        source1 = current;
      }
    }

    return source1;
  }

}

#endif
