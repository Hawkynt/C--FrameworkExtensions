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

using Guard;
using System.Runtime.CompilerServices;

namespace System;

partial class SpanHelper {
  /// <summary>
  ///   Defines a mechanism for handling memory buffers that contain elements of type <typeparamref name="T" />.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the memory buffer.</typeparam>
  /// <remarks>
  ///   This interface provides methods and properties to access and manipulate memory buffers in a type-safe manner.
  ///   It allows slicing memory segments, and copying contents to other memory handlers or arrays.
  /// </remarks>
  public abstract class MemoryHandlerBase<T> {

    /// <summary>
    ///   Gets the element at the specified index in the memory buffer.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>A reference to the element at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    public abstract ref T GetRef(int index);

    /// <summary>
    ///   Gets the element at the specified index in the memory buffer.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    public abstract T GetValue(int index);

    /// <summary>
    ///   Sets the element at the specified index in the memory buffer.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <param name="value">The value to set</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    public abstract void SetValue(int index, T value);

    /// <summary>
    /// Gets a pointer to the start of the buffer
    /// </summary>
    /// <returns></returns>
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    public abstract unsafe T* Pointer { get; }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

    /// <summary>
    ///   Creates a slice of the current memory buffer starting at the specified offset.
    /// </summary>
    /// <param name="offset">The zero-based starting position of the slice.</param>
    /// <returns>A new <see cref="MemoryHandlerBase{T}" /> representing the slice of the original memory buffer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the offset is out of range.</exception>
    public abstract MemoryHandlerBase<T> SliceFrom(int offset);

    /// <summary>
    ///   Copies a specified number of elements to another <see cref="MemoryHandlerBase{T}" /> starting from the beginning.
    /// </summary>
    /// <param name="other">The target <see cref="MemoryHandlerBase{T}" /> to which elements are copied.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="other" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the length is greater than the size of either memory buffer.</exception>
    public unsafe void CopyTo(MemoryHandlerBase<T> other, int count) {
      switch (count) {
        case < 0:
          AlwaysThrow.ArgumentOutOfRangeException(nameof(count));
          return;
        case 0:
          return;
      }

      const int BYTE_COPY_THRESHOLD_IN_ITEMS = 16;
      if (count < BYTE_COPY_THRESHOLD_IN_ITEMS)
        CopyElements(other, count);
      else if (!IsValueType<T>())
        switch (this) {
          case ManagedArrayHandler<T> mahS when other is ManagedArrayHandler<T> mahT: {
            fixed (T* source = mahS.source)
            fixed (T* target = mahT.source) {
              var sourcePtr = source + mahS.start;
              var targetPtr = target + mahT.start;
              CopyPointerElements(sourcePtr, targetPtr, count);
            }
            break;
          }
          default:
            CopyElements(other, count);
            break;
        }
      else
        Utilities.MemoryCopy.CopyWithoutChecks((byte*)this.Pointer, (byte*)other.Pointer, (uint)(Unsafe.SizeOf<T>() * count));

      return;

      static void CopyPointerElements(T* source, T* target, int elements) {
        // Calculate iterations for chunks of 8 with bit trick (length / 8)
        var iterations = elements >> 3;
        
        // Check remainder using bit trick (length % 8)
        switch (elements & 7) {
          case 0:
            goto Copy0or8;
          case 7:
            goto Copy7;
          case 6:
            goto Copy6;
          case 5:
            goto Copy5;
          case 4:
            goto Copy4;
          case 3:
            goto Copy3;
          case 2:
            goto Copy2;
          case 1:
            goto Copy1;
          default:
            goto CopyDone; // Avoid compiler warning and trigger optimization - Never gonna get here
        }

        Copy0or8:
        if (iterations-- <= 0)
          goto CopyDone;

        *target++ = *source++;
        Copy7: *target++ = *source++;
        Copy6: *target++ = *source++;
        Copy5: *target++ = *source++;
        Copy4: *target++ = *source++;
        Copy3: *target++ = *source++;
        Copy2: *target++ = *source++;
        Copy1: *target++ = *source++;
        goto Copy0or8;
        CopyDone:
        ;
      }

      void CopyElements(MemoryHandlerBase<T> target, int elements) {

        // Call overhead would ruin any nice tricks we could pull
        for (var i = 0; i < elements; ++i)
          target.SetValue(i, this.GetValue(i));
      }

    }

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
  public void CopyTo(T[] target, int count) => this.CopyTo(new ManagedArrayHandler<T>(target,0),count);

   }

}

#endif
