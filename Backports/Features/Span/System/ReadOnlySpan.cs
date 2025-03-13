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

using System.Collections;
using System.Collections.Generic;

namespace System;

public readonly ref struct ReadOnlySpan<T> : IEnumerable<T> {
  internal readonly SpanHelper.IMemoryHandler<T> pointerMemoryHandler;

  private ReadOnlySpan(SpanHelper.IMemoryHandler<T> handler, int length) {
    this.pointerMemoryHandler = handler;
    this.Length = length;
  }

  public ReadOnlySpan(T[] array) : this(array, 0, array?.Length ?? 0) { }
  internal ReadOnlySpan(string text) : this(text, 0, text?.Length ?? 0) { }

  public ReadOnlySpan(T[] array, int start, int length) : this(SpanHelper.PinnedArrayMemoryHandler<T>.FromManagedArray(array, start), length) { }
  internal ReadOnlySpan(string text, int start, int length) : this((SpanHelper.IMemoryHandler<T>)new SpanHelper.StringHandler(text, start), length) { }

#pragma warning disable CS8500
  public unsafe ReadOnlySpan(void* pointer, int length) : this(new SpanHelper.UnmanagedPointerMemoryHandler<T>((T*)pointer), length) { }
#pragma warning restore CS8500

  public int Length { get; }

  public bool IsEmpty => this.Length == 0;

  public ref readonly T this[int index] {
    get {
      if ((uint)index >= (uint)this.Length)
        throw new ArgumentOutOfRangeException();

      return ref this.pointerMemoryHandler[index];
    }
  }

  public ReadOnlySpan<T> Slice(int start, int length) {
    if (start < 0 || length < 0 || start + length > this.Length)
      throw new ArgumentOutOfRangeException();

    return new(this.pointerMemoryHandler.SliceFrom(start), length);
  }

  public T[] ToArray() {
    var array = new T[this.Length];
    this.pointerMemoryHandler.CopyTo(array, this.Length);

    return array;
  }

  public void CopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      throw new ArgumentOutOfRangeException();

    this.pointerMemoryHandler.CopyTo(other.pointerMemoryHandler, this.Length);
  }

  public bool TryCopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      return false;

    this.pointerMemoryHandler.CopyTo(other.pointerMemoryHandler, this.Length);
    return true;
  }

  public IEnumerator<T> GetEnumerator() => new SpanHelper.Enumerator<T>(this.pointerMemoryHandler, this.Length);
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public static implicit operator ReadOnlySpan<T>(Span<T> @this) => new(@this.pointerMemoryHandler, @this.Length);
  public static implicit operator ReadOnlySpan<T>(T[] array) => new(array);

  /// <inheritdoc />
  public override string ToString() => this.pointerMemoryHandler is SpanHelper.StringHandler sh ? sh.ToString(this.Length) : typeof(T) == typeof(char) ? new((char[])(object)this.ToArray()) : $"System.ReadOnlySpan<{typeof(T).Name}>[{this.Length}]";

}

#endif
