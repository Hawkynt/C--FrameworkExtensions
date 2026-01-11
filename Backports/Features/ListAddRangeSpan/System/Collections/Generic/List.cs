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

#if !SUPPORTS_LIST_ADDRANGE_SPAN

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class ListPolyfills {
  /// <param name="this">This <see cref="List{T}" /></param>
  /// <typeparam name="T">The type of the items.</typeparam>
  extension<T>(List<T> @this) {

    /// <summary>
    /// Adds the elements of the specified span to the end of the <see cref="List{T}"/>.
    /// </summary>
    /// <param name="source">The span whose elements should be added to the end of the <see cref="List{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(ReadOnlySpan<T> source) {
      Against.ThisIsNull(@this);

      foreach (var item in source)
        @this.Add(item);
    }

    /// <summary>
    /// Inserts the elements of a span into the <see cref="List{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
    /// <param name="source">The span whose elements should be inserted into the <see cref="List{T}"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than <see cref="List{T}.Count"/>.</exception>
    public void InsertRange(int index, ReadOnlySpan<T> source) {
      Against.ThisIsNull(@this);
      ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)index, (uint)@this.Count);

      var insertIndex = index;
      foreach (var item in source)
        @this.Insert(insertIndex++, item);
    }

    /// <summary>
    /// Copies the elements of the <see cref="List{T}"/> to a span.
    /// </summary>
    /// <param name="destination">The span that is the destination of the elements copied from <see cref="List{T}"/>.</param>
    /// <exception cref="ArgumentException">The destination span is shorter than the source <see cref="List{T}"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<T> destination) {
      Against.ThisIsNull(@this);

      if (destination.Length < @this.Count)
        throw new ArgumentException("Destination span is too short.", nameof(destination));

      for (var i = 0; i < @this.Count; ++i)
        destination[i] = @this[i];
    }

  }
}

#endif
