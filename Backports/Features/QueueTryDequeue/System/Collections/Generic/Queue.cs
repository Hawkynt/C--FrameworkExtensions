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

#if !SUPPORTS_QUEUE_TRYDEQUEUE

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class QueuePolyfills {

  extension<T>(Queue<T> @this) {

    /// <summary>
    /// Removes the object at the beginning of the <see cref="Queue{T}"/>, and copies it to the <paramref name="result"/> parameter.
    /// </summary>
    /// <param name="result">The removed object.</param>
    /// <returns>
    /// <see langword="true"/> if the object was successfully removed;
    /// <see langword="false"/> if the <see cref="Queue{T}"/> is empty.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryDequeue([MaybeNullWhen(false)] out T result) {
      Against.ThisIsNull(@this);

      if (@this.Count < 1) {
        result = default;
        return false;
      }

      result = @this.Dequeue();
      return true;
    }

    /// <summary>
    /// Returns a value that indicates whether there is an object at the beginning of the <see cref="Queue{T}"/>,
    /// and if one is present, copies it to the <paramref name="result"/> parameter.
    /// The object is not removed from the <see cref="Queue{T}"/>.
    /// </summary>
    /// <param name="result">The object at the beginning of the <see cref="Queue{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if there is an object at the beginning of the <see cref="Queue{T}"/>;
    /// <see langword="false"/> if the <see cref="Queue{T}"/> is empty.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek([MaybeNullWhen(false)] out T result) {
      Against.ThisIsNull(@this);

      if (@this.Count < 1) {
        result = default;
        return false;
      }

      result = @this.Peek();
      return true;
    }

  }

}

#endif
