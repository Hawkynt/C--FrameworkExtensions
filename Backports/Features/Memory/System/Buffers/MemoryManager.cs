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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Buffers;

/// <summary>
/// An abstract base class that is used to replace the implementation of <see cref="Memory{T}"/>.
/// </summary>
/// <typeparam name="T">The type of items in the memory buffer managed by this memory manager.</typeparam>
public abstract class MemoryManager<T> : IMemoryOwner<T>, IPinnable {

  /// <summary>
  /// Gets the memory block handled by this <see cref="MemoryManager{T}"/>.
  /// </summary>
  public virtual Memory<T> Memory {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(this, 0, this.GetSpan().Length);
  }

  /// <summary>
  /// Returns a <see cref="Span{T}"/> wrapping the underlying memory.
  /// </summary>
  public abstract Span<T> GetSpan();

  /// <summary>
  /// Returns a handle to the memory that has been pinned and whose address can be taken.
  /// </summary>
  /// <param name="elementIndex">The offset to the element in the memory at which the returned <see cref="MemoryHandle"/> points.</param>
  public abstract MemoryHandle Pin(int elementIndex = 0);

  /// <summary>
  /// Unpins pinned memory.
  /// </summary>
  public abstract void Unpin();

  /// <summary>
  /// Returns the memory as a <see cref="Memory{T}"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected Memory<T> CreateMemory(int length) => new(this, 0, length);

  /// <summary>
  /// Returns the memory as a <see cref="Memory{T}"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected Memory<T> CreateMemory(int start, int length) => new(this, start, length);

  /// <summary>
  /// Attempts to retrieve the underlying data source.
  /// </summary>
  protected internal virtual bool TryGetArray(out ArraySegment<T> segment) {
    segment = default;
    return false;
  }

  /// <summary>
  /// Releases all resources used by the <see cref="MemoryManager{T}"/>.
  /// </summary>
  void IDisposable.Dispose() {
    this.Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Releases the unmanaged resources used by the <see cref="MemoryManager{T}"/> and optionally releases the managed resources.
  /// </summary>
  /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
  protected abstract void Dispose(bool disposing);
}

#endif
