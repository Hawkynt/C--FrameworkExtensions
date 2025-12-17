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

#if !SUPPORTS_MEMORYEXTENSIONS_COMMONPREFIXLENGTH

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MemoryExtensionsPolyfills {

  extension<T>(Span<T> @this) {

    /// <summary>
    /// Finds the length of any common prefix shared between <paramref name="@this"/> and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other span to compare.</param>
    /// <returns>The length of the common prefix shared by the two spans. If there's no shared prefix, returns 0.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CommonPrefixLength(ReadOnlySpan<T> other)
      => ((ReadOnlySpan<T>)@this).CommonPrefixLength(other);

    /// <summary>
    /// Finds the length of any common prefix shared between <paramref name="@this"/> and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other span to compare.</param>
    /// <param name="comparer">The comparer to use to compare elements.</param>
    /// <returns>The length of the common prefix shared by the two spans. If there's no shared prefix, returns 0.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CommonPrefixLength(ReadOnlySpan<T> other, IEqualityComparer<T> comparer)
      => ((ReadOnlySpan<T>)@this).CommonPrefixLength(other, comparer);

  }

  extension<T>(ReadOnlySpan<T> @this) {

    /// <summary>
    /// Finds the length of any common prefix shared between <paramref name="@this"/> and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other span to compare.</param>
    /// <returns>The length of the common prefix shared by the two spans. If there's no shared prefix, returns 0.</returns>
    public int CommonPrefixLength(ReadOnlySpan<T> other) {
      var minLength = Math.Min(@this.Length, other.Length);
      var comparer = EqualityComparer<T>.Default;

      // TODO: using TypeCodeCache to switch on primitive types for performance?
      for (var i = 0; i < minLength; ++i)
        if (!comparer.Equals(@this[i], other[i]))
          return i;

      return minLength;
    }

    /// <summary>
    /// Finds the length of any common prefix shared between <paramref name="@this"/> and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other span to compare.</param>
    /// <param name="comparer">The comparer to use to compare elements.</param>
    /// <returns>The length of the common prefix shared by the two spans. If there's no shared prefix, returns 0.</returns>
    public int CommonPrefixLength(ReadOnlySpan<T> other, IEqualityComparer<T> comparer) {
      comparer ??= EqualityComparer<T>.Default;
      var minLength = Math.Min(@this.Length, other.Length);

      for (var i = 0; i < minLength; ++i)
        if (!comparer.Equals(@this[i], other[i]))
          return i;

      return minLength;
    }

  }

}

#endif
