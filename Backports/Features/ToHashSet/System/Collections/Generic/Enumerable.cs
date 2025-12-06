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

using System.Diagnostics;
using MethodImplOptions = Utilities.MethodImplOptions;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

public static partial class EnumerablePolyfills {

  extension<TItem>(IEnumerable<TItem> @this) {

    /// <summary>
    ///   Creates a hash set from the given enumeration.
    /// </summary>
    /// <returns>A hashset</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashSet<TItem> ToHashSet() {
      ArgumentNullException.ThrowIfNull(@this);

      // ReSharper disable once UseCollectionExpression
      return new(@this);
    }

    /// <summary>
    ///   Creates a hash set from the given enumeration.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    /// <returns>
    ///   A hashset
    /// </returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashSet<TItem> ToHashSet(IEqualityComparer<TItem> comparer) {
      ArgumentNullException.ThrowIfNull(@this);

      return new(@this, comparer);
    }

  }

}

#endif
