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

// Array.FindLast was added in .NET Framework 2.0 but some older runtimes may not have it
#if !SUPPORTS_ARRAY_FINDLAST

namespace System;

public static partial class ArrayPolyfills {

  extension(Array) {

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="Array"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the array.</typeparam>
    /// <param name="array">The one-dimensional, zero-based <see cref="Array"/> to search.</param>
    /// <param name="match">The <see cref="Predicate{T}"/> that defines the conditions of the element to search for.</param>
    /// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>. -or- <paramref name="match"/> is <see langword="null"/>.</exception>
    public static T? FindLast<T>(T[] array, Predicate<T> match) {
      ArgumentNullException.ThrowIfNull(array);
      ArgumentNullException.ThrowIfNull(match);

      for (var i = array.Length - 1; i >= 0; --i)
        if (match(array[i]))
          return array[i];

      return default;
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="Array"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the array.</typeparam>
    /// <param name="array">The one-dimensional, zero-based <see cref="Array"/> to search.</param>
    /// <param name="match">The <see cref="Predicate{T}"/> that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match"/>, if found; otherwise, -1.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>. -or- <paramref name="match"/> is <see langword="null"/>.</exception>
    public static int FindLastIndex<T>(T[] array, Predicate<T> match) {
      ArgumentNullException.ThrowIfNull(array);
      ArgumentNullException.ThrowIfNull(match);

      for (var i = array.Length - 1; i >= 0; --i)
        if (match(array[i]))
          return i;

      return -1;
    }

  }

}

#endif
