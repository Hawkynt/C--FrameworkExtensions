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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

[DebuggerDisplay("{ToString(),raw}")]
public readonly ref struct ReadOnlySpan<T> : IEnumerable<T> {
  internal readonly SpanHelper.MemoryHandlerBase<T> memoryHandler;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal ReadOnlySpan(SpanHelper.MemoryHandlerBase<T> handler, int length) {
    this.memoryHandler = handler;
    this.Length = length;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan(T[] array) : this(new SpanHelper.ManagedArrayHandler<T>(array, 0), array.Length) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal ReadOnlySpan(string text) : this(new SpanHelper.StringHandler<T>(text,0),text.Length) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan(T[] array, int start, int length) : this(new SpanHelper.ManagedArrayHandler<T>(array, start), length) {
    if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal ReadOnlySpan(string text, int start, int length) : this(new SpanHelper.StringHandler<T>(text, start), length) {
    if ((uint)start > (uint)text.Length || (uint)length > (uint)(text.Length - start))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));
  }

#pragma warning disable CS8500
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe ReadOnlySpan(void* pointer, int length) : this(new SpanHelper.UnmanagedPointerMemoryHandler<T>((T*)pointer), length) { }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe ReadOnlySpan(ref T pointer, int length) : this(new SpanHelper.UnmanagedPointerMemoryHandler<T>((T*)Unsafe.AsPointer(ref pointer)), length) { }
#pragma warning restore CS8500

  public int Length { get; }

  public bool IsEmpty => this.Length == 0;

  public static ReadOnlySpan<T> Empty {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      unsafe {
        return new (new SpanHelper.UnmanagedPointerMemoryHandler<T>(Unsafe.NullPtr<T>()), 0);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ref T GetPinnableReference() => ref this.memoryHandler.GetRef(0);

  public ref readonly T this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if ((uint)index >= (uint)this.Length)
        AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

      return ref this.memoryHandler.GetRef(index);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<T> Slice(int start, int length) {
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator ReadOnlySpan<T>(T[] array) => new(array);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => this.memoryHandler is SpanHelper.StringHandler<T> sh ? sh.ToString(this.Length) : typeof(T) == typeof(char) ? new((char[])(object)this.ToArray()) : $"System.ReadOnlySpan<{typeof(T).Name}>[{this.Length}]";

}

#endif
