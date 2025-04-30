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

namespace System;

[DebuggerDisplay("{ToString(),raw}")]
public readonly ref struct Span<T> : IEnumerable<T> {
  internal readonly SpanHelper.MemoryHandlerBase<T> memoryHandler;

  private Span(SpanHelper.MemoryHandlerBase<T> handler, int length) {
    this.memoryHandler = handler;
    this.Length = length;
  }

  public Span(T[] array) : this(new SpanHelper.ManagedArrayHandler<T>(array,0),array.Length) { }

  public Span(T[] array, int start, int length) : this(new SpanHelper.ManagedArrayHandler<T>(array, start), length) {
    if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));
  }

#pragma warning disable CS8500
  public unsafe Span(void* pointer, int length) : this(new SpanHelper.UnmanagedPointerMemoryHandler<T>((T*)pointer), length) { }
#pragma warning restore CS8500

  public int Length { get; }

  public bool IsEmpty => this.Length == 0;

  public static Span<T> Empty {
    get {
      unsafe {
        return new(new SpanHelper.UnmanagedPointerMemoryHandler<T>(Unsafe.NullPtr<T>()), 0);
      }
    }
  }

  public ref T this[int index] {
    get {
      if ((uint)index >= (uint)this.Length)
        AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

      return ref this.memoryHandler.GetRef(index);
    }
  }

  public Span<T> Slice(int start, int length) {
    if ((uint)start > (uint)this.Length || (uint)length > (uint)(this.Length - start))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));

    return new(this.memoryHandler.SliceFrom(start), length);
  }

  public T[] ToArray() {
    var array = new T[this.Length];
    this.memoryHandler.CopyTo(array, this.Length);

    return array;
  }

  public void CopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(other));

    this.memoryHandler.CopyTo(other.memoryHandler, this.Length);
  }

  public bool TryCopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      return false;

    this.memoryHandler.CopyTo(other.memoryHandler, this.Length);
    return true;
  }

  public IEnumerator<T> GetEnumerator() => new SpanHelper.Enumerator<T>(this.memoryHandler, this.Length);
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  /// <inheritdoc />
  public override string ToString() => typeof(T) == typeof(char) ? new((char[])(object)this.ToArray()) : $"System.Span<{typeof(T).Name}>[{this.Length}]";

  public static implicit operator ReadOnlySpan<T>(Span<T> @this) => new(@this.memoryHandler, @this.Length);
  
}

#endif
