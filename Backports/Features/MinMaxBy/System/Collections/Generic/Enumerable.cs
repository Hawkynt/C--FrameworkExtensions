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

#if !SUPPORTS_MINMAX_BY

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;
using Guard;

namespace System.Collections.Generic;

public static partial class EnumerablePolyfills {

  extension<TItem>(IEnumerable<TItem> @this) {

    /// <summary>Returns the maximum value in a generic sequence according to a specified key selector function.</summary>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="@this" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    ///   No key extracted from <paramref name="@this" /> implements the
    ///   <see cref="T:System.IComparable" /> or <see cref="T:System.IComparable{T}" /> interface.
    /// </exception>
    /// <returns>The value with the maximum key in the sequence.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TItem MaxBy<TKey>(Func<TItem, TKey> keySelector)
      => @this.MaxBy(keySelector, null);

    /// <summary>Returns the maximum value in a generic sequence according to a specified key selector function.</summary>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> to compare keys.</param>
    /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="@this" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    ///   No key extracted from <paramref name="@this" /> implements the
    ///   <see cref="T:System.IComparable" /> or <see cref="T:System.IComparable{T}" /> interface.
    /// </exception>
    /// <returns>The value with the maximum key in the sequence.</returns>
    public TItem MaxBy<TKey>(Func<TItem, TKey> keySelector, IComparer<TKey> comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);

      comparer ??= Comparer<TKey>.Default;
      using var enumerator = @this.GetEnumerator();
      if (!enumerator.MoveNext()) {
        if (default(TItem) == null)
          return default;

        AlwaysThrow.InvalidOperationException("The sequence contains no elements.");
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
      } else if (ReferenceEquals(comparer, Comparer<TKey>.Default))
        while (enumerator.MoveNext()) {
          var current = enumerator.Current;
          var x = keySelector(current);
          if (Comparer<TKey>.Default.Compare(x, y) <= 0)
            continue;

          y = x;
          source1 = current;
        }
      else
        while (enumerator.MoveNext()) {
          var current = enumerator.Current;
          var x = keySelector(current);
          if (comparer.Compare(x, y) <= 0)
            continue;

          y = x;
          source1 = current;
        }

      return source1;
    }

    /// <summary>Returns the minimum value in a generic sequence according to a specified key selector function.</summary>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="@this" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    ///   No key extracted from <paramref name="@this" /> implements the
    ///   <see cref="T:System.IComparable" /> or <see cref="T:System.IComparable{T}" /> interface.
    /// </exception>
    /// <returns>The value with the minimum key in the sequence.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TItem MinBy<TKey>(Func<TItem, TKey> keySelector)
      => @this.MinBy(keySelector, null);

    /// <summary>Returns the minimum value in a generic sequence according to a specified key selector function.</summary>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> to compare keys.</param>
    /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="@this" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    ///   No key extracted from <paramref name="@this" /> implements the
    ///   <see cref="T:System.IComparable" /> or <see cref="T:System.IComparable{T}" /> interface.
    /// </exception>
    /// <returns>The value with the minimum key in the sequence.</returns>
    public TItem MinBy<TKey>(Func<TItem, TKey> keySelector, IComparer<TKey> comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);

      comparer ??= Comparer<TKey>.Default;
      using var enumerator = @this.GetEnumerator();
      if (!enumerator.MoveNext()) {
        if (default(TItem) == null)
          return default;

        AlwaysThrow.InvalidOperationException("The sequence contains no elements.");
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
      } else if (ReferenceEquals(comparer, Comparer<TKey>.Default))
        while (enumerator.MoveNext()) {
          var current = enumerator.Current;
          var x = keySelector(current);
          if (Comparer<TKey>.Default.Compare(x, y) >= 0)
            continue;

          y = x;
          source1 = current;
        }
      else
        while (enumerator.MoveNext()) {
          var current = enumerator.Current;
          var x = keySelector(current);
          if (comparer.Compare(x, y) >= 0)
            continue;

          y = x;
          source1 = current;
        }

      return source1;
    }

  }

}

#endif
