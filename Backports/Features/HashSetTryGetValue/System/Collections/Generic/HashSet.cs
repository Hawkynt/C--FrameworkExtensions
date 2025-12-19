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

#if !SUPPORTS_HASHSET_TRYGETVALUE

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class HashSetPolyfills {

  extension<T>(HashSet<T> @this) {

    /// <summary>
    /// Searches the set for a given value and returns the equal value it finds, if any.
    /// </summary>
    /// <param name="equalValue">The value to search for.</param>
    /// <param name="actualValue">
    /// The value from the set that the search found, or the default value of <typeparamref name="T"/> when the search yielded no match.
    /// </param>
    /// <returns>A value indicating whether the search was successful.</returns>
    /// <remarks>
    /// This can be useful when you want to reuse a previously stored instance instead of a newly constructed one
    /// (so that more sharing of references can occur) or to look up a value that has more complete data than the value you currently have.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue) {
      Against.ThisIsNull(@this);

      foreach (var item in @this) {
        if (!@this.Comparer.Equals(item, equalValue))
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
