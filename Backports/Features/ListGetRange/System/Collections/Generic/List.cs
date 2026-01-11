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

// List<T>.GetRange(Range) was added in .NET 8.0
#if !SUPPORTS_LIST_GETRANGE_RANGE

using Guard;

namespace System.Collections.Generic;

public static partial class ListPolyfills {

  extension<T>(List<T> @this) {

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="List{T}"/>.
    /// </summary>
    /// <param name="range">The range of elements to copy.</param>
    /// <returns>A shallow copy of a range of elements in the source <see cref="List{T}"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="range"/> is outside the bounds of the <see cref="List{T}"/>.</exception>
    public List<T> GetRange(Range range) {
      Against.ThisIsNull(@this);
      var (offset, length) = range.GetOffsetAndLength(@this.Count);
      return @this.GetRange(offset, length);
    }

    /// <summary>
    /// Removes a range of elements from the <see cref="List{T}"/>.
    /// </summary>
    /// <param name="range">The range of elements to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="range"/> is outside the bounds of the <see cref="List{T}"/>.</exception>
    public void RemoveRange(Range range) {
      Against.ThisIsNull(@this);
      var (offset, length) = range.GetOffsetAndLength(@this.Count);
      @this.RemoveRange(offset, length);
    }

  }

}

#endif
