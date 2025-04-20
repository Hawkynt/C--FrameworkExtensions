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

using System.Runtime.InteropServices;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace System;

unsafe partial class SpanHelper {
  /// <summary>
  ///   Provides a mechanism to pin a managed array in memory, allowing for safe and direct access to its elements via an
  ///   unmanaged pointer.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the array.</typeparam>
  /// <remarks>
  ///   This class uses <see cref="GCHandle" /> to pin an array in memory, preventing the garbage collector from moving it,
  ///   which is crucial for interoperability with unmanaged code that requires direct memory access.
  /// </remarks>
  public sealed class ManagedArrayPin<T>(GCHandle handle, T* pointer) {
    /// <summary>
    ///   Gets the pointer to the first element of the pinned array.
    /// </summary>
    /// <value>
    ///   The pointer to the first element of the pinned array.
    /// </value>
    public T* Pointer { get; } = pointer;

    /// <summary>
    ///   Finalizes an instance of the <see cref="ManagedArrayPin{T}" /> class, unpinning the array, so the GC can freely move
    ///   it again in memory.
    /// </summary>
    ~ManagedArrayPin() {
      if (handle.IsAllocated)
        handle.Free();
    }

    /// <summary>
    ///   Attempts to pin the specified array in memory and provides access to it via a <see cref="ManagedArrayPin{T}" />
    ///   instance.
    /// </summary>
    /// <param name="array">The array to pin in memory.</param>
    /// <param name="instance">
    ///   When this method returns, contains the <see cref="ManagedArrayPin{T}" /> instance representing
    ///   the pinned array, if the pinning succeeded; otherwise, null.
    /// </param>
    /// <returns><see langword="true" /> if the array was successfully pinned; otherwise, <see langword="false" />.</returns>
    /// <example>
    ///   <code>
    /// int[] numbers = { 1, 2, 3, 4, 5 };
    /// if (ManagedArrayPin&lt;int&gt;.TryPin(numbers, out var pin))
    /// {
    ///     int* ptr = pin.Pointer;
    ///     for (int i = 0; i &lt; numbers.Length; i++)
    ///     {
    ///         Console.WriteLine(ptr[i]); // Outputs each number in the array
    ///     }
    /// }
    /// </code>
    ///   This example demonstrates how to pin an array and access its elements through a pointer.
    /// </example>
    /// <remarks>
    ///   Pinning should be done with care, as excessive pinning can lead to memory fragmentation and performance issues.
    ///   Always ensure to release pinned objects as soon as possible.
    /// </remarks>
    public static bool TryPin(T[] array, out ManagedArrayPin<T> instance) {
      try {
        instance = new(GCHandle.Alloc(array, GCHandleType.Pinned), (T*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0));
        return true;
      } catch (Exception) {
        instance = null;
        return false;
      }
    }
  }
}

#endif
