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

#if !SUPPORTS_TO_HASHSET

using Guard;
using System.Diagnostics;
using MethodImplOptions = Utilities.MethodImplOptions;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

public static partial class EnumerablePolyfills {
  /// <summary>
  ///   Creates a hash set from the given enumeration.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <returns>A hashset</returns>
  [DebuggerStepThrough]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HashSet<TItem> ToHashSet<TItem>(this IEnumerable<TItem> @this) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));

    // ReSharper disable once UseCollectionExpression
    return new(@this);
  }

  /// <summary>
  ///   Creates a hash set from the given enumeration.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="comparer">The comparer.</param>
  /// <returns>
  ///   A hashset
  /// </returns>
  [DebuggerStepThrough]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HashSet<TItem> ToHashSet<TItem>(this IEnumerable<TItem> @this, IEqualityComparer<TItem> comparer) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));

    return new(@this, comparer);
  }
}

#endif
