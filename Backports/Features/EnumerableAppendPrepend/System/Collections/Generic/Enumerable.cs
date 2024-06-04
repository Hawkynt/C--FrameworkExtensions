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

#if !SUPPORTS_ENUMERABLE_APPENDPREPEND

namespace System.Collections.Generic;

// ReSharper disable UnusedMember.Global
// ReSharper disable once PartialTypeWithSinglePart
public static partial class EnumerablePolyfills {
  
  /// <summary>
  /// Appends a single item to the beginning of the <see cref="IEnumerable{T}"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="item">The item to append</param>
  /// <returns>A new <see cref="IEnumerable{T}"/> with the added item</returns>
  /// <exception cref="ArgumentNullException">When the given <see cref="IEnumerable{T}"/> is <see langword="null"/></exception>
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
  /// Appends a single item to the end of the <see cref="IEnumerable{T}"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="item">The item to append</param>
  /// <returns>A new <see cref="IEnumerable{T}"/> with the added item</returns>
  /// <exception cref="ArgumentNullException">When the given <see cref="IEnumerable{T}"/> is <see langword="null"/></exception>
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
