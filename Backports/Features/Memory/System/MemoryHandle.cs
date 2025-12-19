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

#if !SUPPORTS_MEMORY && !OFFICIAL_MEMORY

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Provides a memory handle for a block of memory.
/// </summary>
public unsafe struct MemoryHandle : IDisposable {

  private void* _pointer;
  private GCHandle _handle;
  private IPinnable? _pinnable;

  /// <summary>
  /// Creates a new memory handle for the given array.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public MemoryHandle(void* pointer, GCHandle handle = default, IPinnable? pinnable = null) {
    this._pointer = pointer;
    this._handle = handle;
    this._pinnable = pinnable;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static MemoryHandle Create<T>(T[] array, int index) where T : unmanaged {
    if (array == null || array.Length == 0)
      return default;

    var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
    var ptr = (T*)handle.AddrOfPinnedObject() + index;
    return new((void*)ptr, handle, null);
  }

  /// <summary>
  /// Returns the pointer to the pinned memory.
  /// </summary>
  public void* Pointer {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._pointer;
  }

  /// <summary>
  /// Frees the pinned handle and releases the memory.
  /// </summary>
  public void Dispose() {
    if (this._handle.IsAllocated)
      this._handle.Free();

    this._pinnable?.Unpin();
    this._pinnable = null;
    this._pointer = null;
  }
}

/// <summary>
/// Provides a mechanism for pinning and unpinning objects.
/// </summary>
public interface IPinnable {
  /// <summary>
  /// Pins the object.
  /// </summary>
  MemoryHandle Pin(int elementIndex);

  /// <summary>
  /// Unpins the object.
  /// </summary>
  void Unpin();
}

#endif
