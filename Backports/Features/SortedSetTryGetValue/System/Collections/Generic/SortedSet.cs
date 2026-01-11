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

// SortedSet was added in .NET 4.0, guard with SUPPORTS_ISET
#if !SUPPORTS_SORTEDSET_TRYGETVALUE

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class SortedSetPolyfills {
  /// <param name="this">This <see cref="SortedSet{T}" /></param>
  /// <typeparam name="T">The type of the items.</typeparam>
  extension<T>(SortedSet<T> @this) {
    /// <summary>
    /// Searches the set for a given value and returns the equal value it finds, if any.
    /// </summary>
    /// <param name="equalValue">The value to search for.</param>
    /// <param name="actualValue">
    /// The value from the set that the search found, or the default value of <typeparamref name="T"/>
    /// when the search yielded no match.
    /// </param>
    /// <returns>A value indicating whether the search was successful.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue) {
      Against.ThisIsNull(@this);

      var comparer = @this.Comparer;
      foreach (var item in @this) {
        if (comparer.Compare(item, equalValue) != 0)
          continue;
        actualValue = item;
        return true;
      }

      actualValue = default;
      return false;
    }
  }
}

#endif
