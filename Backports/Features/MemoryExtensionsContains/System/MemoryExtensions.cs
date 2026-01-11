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

#if !SUPPORTS_MEMORYEXTENSIONS_CONTAINS

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MemoryExtensionsPolyfills {

  extension<T>(ReadOnlySpan<T> @this) where T : IEquatable<T> {

    /// <summary>
    /// Indicates whether a specified value is found in a read-only span.
    /// </summary>
    /// <param name="value">The value to search for.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is found; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value) {
      for (var i = 0; i < @this.Length; ++i)
        if (@this[i].Equals(value))
          return true;
      return false;
    }

  }

  extension<T>(Span<T> @this) where T : IEquatable<T> {

    /// <summary>
    /// Indicates whether a specified value is found in a span.
    /// </summary>
    /// <param name="value">The value to search for.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is found; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value) {
      for (var i = 0; i < @this.Length; ++i)
        if (@this[i].Equals(value))
          return true;
      return false;
    }

  }

}

#endif
