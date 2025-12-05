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

#if !SUPPORTS_ARRAY_FILL

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class ArrayExtensions {
  extension(Array) {
  /// <summary>
  /// Assigns the given value of type <typeparamref name="T"/> to each element of the specified array.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the array.</typeparam>
  /// <param name="array">The array to be filled.</param>
  /// <param name="value">The value to assign to each array element.</param>
  /// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Fill<T>(T[] array, T value) {
    if (array == null)
      AlwaysThrow.ArgumentNullException(nameof(array));

    for (var i = 0; i < array.Length; ++i)
      array[i] = value;
  }

  /// <summary>
  /// Assigns the given value of type <typeparamref name="T"/> to the elements of the specified array
  /// which are within the range of <paramref name="startIndex"/> (inclusive) and the next
  /// <paramref name="count"/> number of indices.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the array.</typeparam>
  /// <param name="array">The array to be filled.</param>
  /// <param name="value">The value to assign to each array element.</param>
  /// <param name="startIndex">A 32-bit integer that represents the index in the array at which filling begins.</param>
  /// <param name="count">The number of elements to fill.</param>
  /// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="startIndex"/> is less than zero, or <paramref name="count"/> is less than zero,
  /// or <paramref name="startIndex"/> + <paramref name="count"/> is greater than the length of the array.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Fill<T>(T[] array, T value, int startIndex, int count) {
    if (array == null)
      AlwaysThrow.ArgumentNullException(nameof(array));
    if (startIndex < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex), "Non-negative number required.");
    if (count < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(count), "Non-negative number required.");
    if (startIndex + count > array.Length)
      AlwaysThrow.ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.", nameof(count));

    var end = startIndex + count;
    for (var i = startIndex; i < end; ++i)
      array[i] = value;
  }
  }
}

#endif
