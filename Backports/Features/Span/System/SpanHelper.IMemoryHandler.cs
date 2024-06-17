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

#if !SUPPORTS_SPAN

namespace System;

internal static partial class SpanHelper {
  /// <summary>
  ///   Defines a mechanism for handling memory buffers that contain elements of type <typeparamref name="T" />.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the memory buffer.</typeparam>
  /// <remarks>
  ///   This interface provides methods and properties to access and manipulate memory buffers in a type-safe manner.
  ///   It allows slicing memory segments, and copying contents to other memory handlers or arrays.
  /// </remarks>
  public interface IMemoryHandler<T> {
    /// <summary>
    ///   Gets the element at the specified index in the memory buffer.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>A reference to the element at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    public ref T this[int index] { get; }

    /// <summary>
    ///   Creates a slice of the current memory buffer starting at the specified offset.
    /// </summary>
    /// <param name="offset">The zero-based starting position of the slice.</param>
    /// <returns>A new <see cref="IMemoryHandler{T}" /> representing the slice of the original memory buffer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the offset is out of range.</exception>
    public IMemoryHandler<T> SliceFrom(int offset);

    /// <summary>
    ///   Copies a specified number of elements to another <see cref="IMemoryHandler{T}" /> starting from the beginning.
    /// </summary>
    /// <param name="other">The target <see cref="IMemoryHandler{T}" /> to which elements are copied.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="other" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the length is greater than the size of either memory buffer.</exception>
    public void CopyTo(IMemoryHandler<T> other, int length);

    /// <summary>
    ///   Copies a specified number of elements from the memory buffer to a target array starting at the array's beginning.
    /// </summary>
    /// <param name="target">The target array to which elements are copied.</param>
    /// <param name="count">The number of elements to copy to the target array.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="target" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   Thrown if the count is greater than the size of the memory buffer or the
    ///   target array.
    /// </exception>
    public void CopyTo(T[] target, int count);
  }
}

#endif
