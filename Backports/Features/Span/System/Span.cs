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

#if !SUPPORTS_SPAN && !OFFICIAL_SPAN

using Guard;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

[DebuggerDisplay("{ToString(),raw}")]
public readonly ref struct Span<T> : IEnumerable<T> {
  internal readonly SpanHelper.MemoryHandlerBase<T> memoryHandler;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private Span(SpanHelper.MemoryHandlerBase<T> handler, int length) {
    this.memoryHandler = handler;
    this.Length = length;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Span(T[] array) : this(new SpanHelper.ManagedArrayHandler<T>(array,0),array.Length) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Span(T[] array, int start, int length) : this(new SpanHelper.ManagedArrayHandler<T>(array, start), length) {
    if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));
  }

#pragma warning disable CS8500
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe Span(void* pointer, int length) : this(new SpanHelper.UnmanagedPointerMemoryHandler<T>((T*)pointer), length) { }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe Span(ref T pointer, int length) : this(new SpanHelper.UnmanagedPointerMemoryHandler<T>((T*)Unsafe.AsPointer(ref pointer)), length) { }
#pragma warning restore CS8500
  
  public int Length { get; }

  public bool IsEmpty => this.Length == 0;

  public static Span<T> Empty {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      unsafe {
        return new(new SpanHelper.UnmanagedPointerMemoryHandler<T>(Unsafe.NullPtr<T>()), 0);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ref T GetPinnableReference() => ref this.memoryHandler.GetRef(0);

  public ref T this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if ((uint)index >= (uint)this.Length)
        AlwaysThrow.IndexOutOfRangeException();

      return ref this.memoryHandler.GetRef(index);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Span<T> Slice(int start) {
    ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)start, (uint)this.Length);

    return new(this.memoryHandler.SliceFrom(start), this.Length - start);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Span<T> Slice(int start, int length) {
    if ((uint)start > (uint)this.Length || (uint)length > (uint)(this.Length - start))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));

    return new(this.memoryHandler.SliceFrom(start), length);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T[] ToArray() {
    var array = new T[this.Length];
    this.memoryHandler.CopyTo(array, this.Length);

    return array;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(other));

    this.memoryHandler.CopyTo(other.memoryHandler, this.Length);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Clear() {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    fixed (T* pointer = this)
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
      Unsafe.InitBlockUnaligned(pointer, 0, (uint)(Unsafe.SizeOf<T>() * this.Length));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Fill(T value) {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    if (sizeof(T) == 1)
      fixed (T* pointer = this)
        Unsafe.InitBlockUnaligned(pointer, *(byte*)&value, (uint)this.Length);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    else
      for (var i = 0; i < this.Length; ++i)
        this.memoryHandler.GetRef(i) = value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryCopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      return false;

    this.memoryHandler.CopyTo(other.memoryHandler, this.Length);
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IEnumerator<T> GetEnumerator() => new SpanHelper.Enumerator<T>(this.memoryHandler, this.Length);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => typeof(T) == typeof(char) ? new((char[])(object)this.ToArray()) : $"System.Span<{typeof(T).Name}>[{this.Length}]";

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator ReadOnlySpan<T>(Span<T> @this) => new(@this.memoryHandler, @this.Length);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Span<T>(T[] @this) => new(@this);

}

#endif
