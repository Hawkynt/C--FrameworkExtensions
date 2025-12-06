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

using Guard;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Represents a contiguous region of memory.
/// </summary>
/// <typeparam name="T">The type of items in the memory.</typeparam>
[DebuggerDisplay("{ToString(),raw}")]
public readonly struct Memory<T> : IEquatable<Memory<T>> {

  private readonly object _object;
  private readonly int _index;
  private readonly int _length;

  /// <summary>
  /// Creates a new memory over the entirety of the target array.
  /// </summary>
  /// <param name="array">The target array.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Memory(T[] array) {
    if (array == null) {
      this = default;
      return;
    }

    this._object = array;
    this._index = 0;
    this._length = array.Length;
  }

  /// <summary>
  /// Creates a new memory over the portion of the target array beginning at 'start' index and ending at 'end' index (exclusive).
  /// </summary>
  /// <param name="array">The target array.</param>
  /// <param name="start">The index at which to begin the memory.</param>
  /// <param name="length">The number of items in the memory.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Memory(T[] array, int start, int length) {
    if (array == null) {
      if (start != 0 || length != 0)
        AlwaysThrow.ArgumentOutOfRangeException(nameof(start));
      this = default;
      return;
    }

    if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));

    this._object = array;
    this._index = start;
    this._length = length;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal Memory(object obj, int start, int length) {
    this._object = obj;
    this._index = start;
    this._length = length;
  }

  /// <summary>
  /// Returns an empty <see cref="Memory{T}"/>.
  /// </summary>
  public static Memory<T> Empty => default;

  /// <summary>
  /// The number of items in the memory.
  /// </summary>
  public int Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._length;
  }

  /// <summary>
  /// Returns true if Length is 0.
  /// </summary>
  public bool IsEmpty {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._length == 0;
  }

  /// <summary>
  /// Returns a span from the memory.
  /// </summary>
  public Span<T> Span {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (this._object == null)
        return default;

      if (this._object is T[] array)
        return new Span<T>(array, this._index, this._length);

      if (this._object is MemoryManager<T> manager)
        return manager.GetSpan().Slice(this._index, this._length);

      return default;
    }
  }

  /// <summary>
  /// Forms a slice out of the given memory, beginning at 'start'.
  /// </summary>
  /// <param name="start">The index at which to begin this slice.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Memory<T> Slice(int start) {
    if ((uint)start > (uint)this._length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(start));

    return new(this._object, this._index + start, this._length - start);
  }

  /// <summary>
  /// Forms a slice out of the given memory, beginning at 'start', of given length.
  /// </summary>
  /// <param name="start">The index at which to begin this slice.</param>
  /// <param name="length">The desired length for the slice.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Memory<T> Slice(int start, int length) {
    if ((uint)start > (uint)this._length || (uint)length > (uint)(this._length - start))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));

    return new(this._object, this._index + start, length);
  }

  /// <summary>
  /// Copies the contents of the memory into the destination. If the source and destination overlap, this method behaves as if the original values are in a temporary location before the destination is overwritten.
  /// </summary>
  /// <param name="destination">The Memory to copy items into.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyTo(Memory<T> destination) => this.Span.CopyTo(destination.Span);

  /// <summary>
  /// Copies the contents of the memory into the destination. If the source and destination overlap, this method behaves as if the original values are in a temporary location before the destination is overwritten.
  /// </summary>
  /// <param name="destination">The Memory to copy items into.</param>
  /// <returns>If the destination is shorter than the source, this method returns false and no data is written to the destination.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryCopyTo(Memory<T> destination) => this.Span.TryCopyTo(destination.Span);

  /// <summary>
  /// Creates a handle for the memory.
  /// </summary>
  /// <returns>A handle for the memory.</returns>
  public unsafe MemoryHandle Pin() {
    if (this._object is T[] array) {
      if (array.Length == 0)
        return default;
      var handle = System.Runtime.InteropServices.GCHandle.Alloc(array, System.Runtime.InteropServices.GCHandleType.Pinned);
      var ptr = (void*)((byte*)handle.AddrOfPinnedObject() + this._index * Unsafe.SizeOf<T>());
      return new(ptr, handle, null);
    }

    if (this._object is MemoryManager<T> manager)
      return manager.Pin(this._index);

    return default;
  }

  /// <summary>
  /// Copies the contents from the memory into a new array.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T[] ToArray() => this.Span.ToArray();

  /// <inheritdoc />
  public override bool Equals(object obj) =>
    obj is Memory<T> other && this.Equals(other) ||
    obj is ReadOnlyMemory<T> readOnly && readOnly.Equals(this);

  /// <inheritdoc />
  public bool Equals(Memory<T> other) =>
    this._object == other._object && this._index == other._index && this._length == other._length;

  /// <inheritdoc />
  public override int GetHashCode() {
    if (this._object == null)
      return 0;
    unchecked {
      var hash = this._object.GetHashCode();
      hash = (hash * 397) ^ this._index;
      hash = (hash * 397) ^ this._length;
      return hash;
    }
  }

  /// <inheritdoc />
  public override string ToString() {
    if (typeof(T) == typeof(char) && this._object is T[] array)
      return new string((char[])(object)array, this._index, this._length);

    return $"System.Memory<{typeof(T).Name}>[{this._length}]";
  }

  /// <summary>
  /// Defines an implicit conversion of an array to a <see cref="Memory{T}"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Memory<T>(T[] array) => new(array);

  /// <summary>
  /// Defines an implicit conversion of an ArraySegment to a <see cref="Memory{T}"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Memory<T>(ArraySegment<T> segment) => new(segment.Array, segment.Offset, segment.Count);

  /// <summary>
  /// Defines an implicit conversion of a <see cref="Memory{T}"/> to a <see cref="ReadOnlyMemory{T}"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator ReadOnlyMemory<T>(Memory<T> memory) => new(memory._object, memory._index, memory._length);

  public static bool operator ==(Memory<T> left, Memory<T> right) => left.Equals(right);
  public static bool operator !=(Memory<T> left, Memory<T> right) => !left.Equals(right);
}

#endif
