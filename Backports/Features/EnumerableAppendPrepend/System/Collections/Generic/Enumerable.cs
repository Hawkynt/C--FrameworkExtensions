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

#if !SUPPORTS_ENUMERABLE_APPENDPREPEND

using System.Collections.Generic;
namespace System.Linq;

public static partial class EnumerablePolyfills {
  /// <summary>
  ///   Appends a single item to the beginning of the <see cref="IEnumerable{T}" />.
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}" /></param>
  /// <param name="item">The item to append</param>
  /// <returns>A new <see cref="IEnumerable{T}" /> with the added item</returns>
  /// <exception cref="ArgumentNullException">
  ///   When the given <see cref="IEnumerable{T}" /> is <see langword="null" />
  /// </exception>
  public static IEnumerable<TItem> Prepend<TItem>(this IEnumerable<TItem> @this, TItem item) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    return Invoke(@this, item);

    static IEnumerable<TItem> Invoke(IEnumerable<TItem> @this, TItem item) {
      yield return item;

      foreach (var i in @this)
        yield return i;
    }
  }

  /// <summary>
  ///   Appends a single item to the end of the <see cref="IEnumerable{T}" />.
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}" /></param>
  /// <param name="item">The item to append</param>
  /// <returns>A new <see cref="IEnumerable{T}" /> with the added item</returns>
  /// <exception cref="ArgumentNullException">
  ///   When the given <see cref="IEnumerable{T}" /> is <see langword="null" />
  /// </exception>
  public static IEnumerable<TItem> Append<TItem>(this IEnumerable<TItem> @this, TItem item) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    return Invoke(@this, item);

    static IEnumerable<TItem> Invoke(IEnumerable<TItem> @this, TItem item) {
      foreach (var i in @this)
        yield return i;

      yield return item;
    }
  }
}

#endif
