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

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.Extensions.ColorProcessing.Resizing;

/// <summary>
/// Provides RAII-style temporary pinning for array access via pointers.
/// </summary>
/// <typeparam name="T">The element type of the array.</typeparam>
/// <remarks>
/// <para>
/// This type enables safe, scoped pinning of arrays for pointer access.
/// The array is pinned on construction and unpinned on disposal.
/// </para>
/// <para>
/// Unlike <see cref="GC.AllocateArray{T}"/> with pinned=true which permanently
/// pins arrays on the POH, this type only pins for the duration of the scope,
/// allowing the GC to compact memory when the scope ends.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// using var pinned = pooledFrame.Pin();
/// TPixel* ptr = pinned.Pointer;
/// // Work with pointer safely within this scope
/// </code>
/// </para>
/// </remarks>
public readonly ref struct PinnedScope<T> where T : unmanaged {
  private readonly GCHandle _handle;
  private readonly int _length;

  /// <summary>
  /// Gets an unsafe pointer to the pinned array data.
  /// </summary>
  public unsafe T* Pointer {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (T*)this._handle.AddrOfPinnedObject();
  }

  /// <summary>
  /// Gets the logical length of the pinned data.
  /// </summary>
  public int Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._length;
  }

  /// <summary>
  /// Creates a new pinned scope for the specified array.
  /// </summary>
  /// <param name="array">The array to pin.</param>
  /// <param name="length">The logical length of valid data.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal PinnedScope(T[] array, int length) {
    this._handle = GCHandle.Alloc(array, GCHandleType.Pinned);
    this._length = length;
  }

  /// <summary>
  /// Unpins the array, allowing the GC to relocate it.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Dispose() {
    if (this._handle.IsAllocated)
      this._handle.Free();
  }
}
