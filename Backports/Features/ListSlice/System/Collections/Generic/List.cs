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

#if !SUPPORTS_LIST_SLICE

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class ListPolyfills {

  extension<T>(List<T> @this) {

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="List{T}"/>.
    /// </summary>
    /// <param name="start">The zero-based <see cref="List{T}"/> index at which the range starts.</param>
    /// <param name="length">The number of elements in the range.</param>
    /// <returns>A shallow copy of a range of elements in the source <see cref="List{T}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="this"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than 0.
    /// -or-
    /// <paramref name="length"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="start"/> and <paramref name="length"/> do not denote a valid range of elements in the <see cref="List{T}"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> Slice(int start, int length) {
      Against.ThisIsNull(@this);
      ArgumentOutOfRangeException.ThrowIfNegative(start);
      ArgumentOutOfRangeException.ThrowIfNegative(length);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(start + length, @this.Count);

      return @this.GetRange(start, length);
    }

  }

}

#endif
