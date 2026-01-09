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

// NativeMemory was added in .NET 6.0
// Requires nuint/nint and Span which are available in .NET Core 2.1+
#if !SUPPORTS_NATIVE_MEMORY

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.InteropServices;

/// <summary>
/// Provides methods for allocating and freeing native memory.
/// </summary>
public static class NativeMemory {

  /// <summary>
  /// Allocates a block of memory of the specified size, in bytes.
  /// </summary>
  /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
  /// <returns>A pointer to the allocated block of memory.</returns>
  /// <exception cref="OutOfMemoryException">Allocating <paramref name="byteCount"/> of memory failed.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void* Alloc(nuint byteCount) {
    if (byteCount == 0)
      return null;

    var ptr = Marshal.AllocHGlobal((nint)byteCount);
    if (ptr == IntPtr.Zero)
      throw new OutOfMemoryException();

    return (void*)ptr;
  }

  /// <summary>
  /// Allocates a block of memory of the specified size, in bytes, and initializes it to zero.
  /// </summary>
  /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
  /// <returns>A pointer to the allocated and zero-initialized block of memory.</returns>
  /// <exception cref="OutOfMemoryException">Allocating <paramref name="byteCount"/> of memory failed.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void* AllocZeroed(nuint byteCount) {
    var ptr = Alloc(byteCount);
    if (ptr == null)
      return ptr;

    var span = new Span<byte>(ptr, (int)byteCount);
    span.Clear();
    return ptr;
  }

  /// <summary>
  /// Allocates a block of memory of the specified size and alignment, in bytes.
  /// </summary>
  /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
  /// <param name="alignment">The alignment, in bytes, of the block to allocate. This must be a power of 2.</param>
  /// <returns>A pointer to the allocated block of memory.</returns>
  /// <exception cref="OutOfMemoryException">Allocating <paramref name="byteCount"/> of memory failed.</exception>
  /// <exception cref="ArgumentException"><paramref name="alignment"/> is not a power of 2.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void* AlignedAlloc(nuint byteCount, nuint alignment) {
    if (alignment == 0 || (alignment & (alignment - 1)) != 0)
      throw new ArgumentException("Alignment must be a power of 2.", nameof(alignment));

    if (byteCount == 0)
      return null;

    // Allocate extra space for alignment and storing the original pointer
    var totalSize = byteCount + alignment + (nuint)sizeof(void*);
    var originalPtr = Alloc(totalSize);

    // Calculate aligned address
    var address = (nuint)originalPtr + (nuint)sizeof(void*);
    var alignedAddress = (address + alignment - 1) & ~(alignment - 1);

    // Store original pointer just before the aligned address
    *((void**)(alignedAddress - (nuint)sizeof(void*))) = originalPtr;

    return (void*)alignedAddress;
  }

  /// <summary>
  /// Frees a block of memory.
  /// </summary>
  /// <param name="ptr">A pointer to the block of memory to free.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Free(void* ptr) {
    if (ptr != null)
      Marshal.FreeHGlobal((IntPtr)ptr);
  }

  /// <summary>
  /// Frees a block of memory that was allocated with <see cref="AlignedAlloc"/>.
  /// </summary>
  /// <param name="ptr">A pointer to the block of memory to free.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void AlignedFree(void* ptr) {
    if (ptr == null)
      return;

    // Retrieve the original pointer stored just before the aligned address
    var originalPtr = *((void**)((nuint)ptr - (nuint)sizeof(void*)));
    Free(originalPtr);
  }

  /// <summary>
  /// Reallocates a block of memory to have the specified size, in bytes.
  /// </summary>
  /// <param name="ptr">A pointer to the block of memory to reallocate.</param>
  /// <param name="byteCount">The new size, in bytes, of the block of memory.</param>
  /// <returns>A pointer to the reallocated block of memory.</returns>
  /// <exception cref="OutOfMemoryException">Reallocating <paramref name="byteCount"/> of memory failed.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void* Realloc(void* ptr, nuint byteCount) {
    if (ptr == null)
      return Alloc(byteCount);

    if (byteCount == 0) {
      Free(ptr);
      return null;
    }

    var newPtr = Marshal.ReAllocHGlobal((IntPtr)ptr, (IntPtr)(nint)byteCount);
    if (newPtr == IntPtr.Zero)
      throw new OutOfMemoryException();

    return (void*)newPtr;
  }

}

#endif
