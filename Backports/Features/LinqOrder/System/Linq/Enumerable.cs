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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {

  extension<T>(IEnumerable<T> @this) {

    /// <summary>
    /// Sorts the elements of a sequence in ascending order.
    /// </summary>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="@this"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOrderedEnumerable<T> Order() {
      ArgumentNullException.ThrowIfNull(@this);

      return @this.OrderBy(x => x);
    }

    /// <summary>
    /// Sorts the elements of a sequence in ascending order.
    /// </summary>
    /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="@this"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOrderedEnumerable<T> Order(IComparer<T> comparer) {
      ArgumentNullException.ThrowIfNull(@this);

      return @this.OrderBy(x => x, comparer);
    }

    /// <summary>
    /// Sorts the elements of a sequence in descending order.
    /// </summary>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted in descending order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="@this"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOrderedEnumerable<T> OrderDescending() {
      ArgumentNullException.ThrowIfNull(@this);

      return @this.OrderByDescending(x => x);
    }

    /// <summary>
    /// Sorts the elements of a sequence in descending order.
    /// </summary>
    /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted in descending order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="@this"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOrderedEnumerable<T> OrderDescending(IComparer<T> comparer) {
      ArgumentNullException.ThrowIfNull(@this);

      return @this.OrderByDescending(x => x, comparer);
    }

  }

}

#endif

#endif
