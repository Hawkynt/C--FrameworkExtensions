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

#if !SUPPORTS_MEMORYEXTENSIONS_COUNT

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MemoryExtensionsPolyfills {

  extension<T>(Span<T> @this) {

    /// <summary>
    /// Counts the number of times the specified value occurs in the span.
    /// </summary>
    /// <param name="value">The value to count.</param>
    /// <returns>The number of times <paramref name="value"/> was found in the span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Count(T value) => ((ReadOnlySpan<T>)@this).Count(value);

  }

  extension<T>(ReadOnlySpan<T> @this) {

    /// <summary>
    /// Counts the number of times the specified value occurs in the span.
    /// </summary>
    /// <param name="value">The value to count.</param>
    /// <returns>The number of times <paramref name="value"/> was found in the span.</returns>
    public int Count(T value) {
      var count = 0;
      var comparer = EqualityComparer<T>.Default;
      foreach (var item in @this)
        if (comparer.Equals(item, value))
          ++count;

      return count;
    }

    /// <summary>
    /// Counts the number of times the specified sequence occurs in the span.
    /// </summary>
    /// <param name="value">The sequence to count.</param>
    /// <returns>The number of times <paramref name="value"/> was found in the span.</returns>
    public int Count(ReadOnlySpan<T> value) {
      if (value.IsEmpty)
        return 0;

      var count = 0;
      var comparer = EqualityComparer<T>.Default;
      for (var i = 0; i <= @this.Length - value.Length; ++i) {
        var found = true;
        for (var j = 0; j < value.Length; ++j)
          if (!comparer.Equals(@this[i + j], value[j])) {
            found = false;
            break;
          }

        if (found)
          ++count;
      }
      return count;
    }

  }

}

#endif
