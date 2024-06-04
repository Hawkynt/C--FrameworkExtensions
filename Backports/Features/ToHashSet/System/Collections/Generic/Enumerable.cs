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

#if !SUPPORTS_TO_HASHSET

using System.Diagnostics;

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

namespace System.Collections.Generic;

// ReSharper disable UnusedMember.Global
// ReSharper disable once PartialTypeWithSinglePart
public static partial class EnumerablePolyfills {
  
  /// <summary>
  /// Creates a hash set from the given enumeration.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <returns>A hashset</returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static HashSet<TItem> ToHashSet<TItem>(this IEnumerable<TItem> @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    return new(@this);
  }

  /// <summary>
  /// Creates a hash set from the given enumeration.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="comparer">The comparer.</param>
  /// <returns>
  /// A hashset
  /// </returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static HashSet<TItem> ToHashSet<TItem>(this IEnumerable<TItem> @this, IEqualityComparer<TItem> comparer) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    return new(@this, comparer);
  }

}

#endif
