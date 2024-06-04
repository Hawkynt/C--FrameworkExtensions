#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#if !SUPPORTS_SPAN

namespace System;

using Collections;
using Collections.Generic;

public readonly struct ReadOnlySpan<T> : IEnumerable<T> {
  private readonly SpanHelper.IMemoryHandler<T> _pointerMemoryHandler;

  private ReadOnlySpan(SpanHelper.IMemoryHandler<T> handler, int length) {
    this._pointerMemoryHandler = handler;
    this.Length = length;
  }

  public ReadOnlySpan(T[] array) : this(array, 0, array?.Length ?? 0) { }
  
  public ReadOnlySpan(T[] array, int start, int length) : this(SpanHelper.PinnedArrayMemoryHandler<T>.FromManagedArray(array, start), length) { }

#pragma warning disable CS8500
  public unsafe ReadOnlySpan(void* pointer, int length):this(new SpanHelper.UnmanagedPointerMemoryHandler<T>((T*)pointer),length) { }
#pragma warning restore CS8500

  public int Length { get; }

  public bool IsEmpty => this.Length == 0;

  public ref readonly T this[int index] {
    get {
      if ((uint)index >= (uint)this.Length)
        throw new ArgumentOutOfRangeException();
      
      return ref this._pointerMemoryHandler[index];
    }
  }

  public ReadOnlySpan<T> Slice(int start, int length) {
    if (start < 0 || length < 0 || start + length > this.Length)
      throw new ArgumentOutOfRangeException();

    return new(this._pointerMemoryHandler.SliceFrom(start), length);
  }

  public T[] ToArray() {
    var array = new T[this.Length];
    this._pointerMemoryHandler.CopyTo(array, this.Length);
    
    return array;
  }

  public void CopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      throw new ArgumentOutOfRangeException();

    this._pointerMemoryHandler.CopyTo(other.pointerMemoryHandler, this.Length);
  }

  public bool TryCopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      return false;

    this._pointerMemoryHandler.CopyTo(other.pointerMemoryHandler, this.Length);
    return true;
  }

  public IEnumerator<T> GetEnumerator() => new SpanHelper.Enumerator<T>(this._pointerMemoryHandler, this.Length);
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public static implicit operator ReadOnlySpan<T>(Span<T> @this) => new(@this.pointerMemoryHandler, @this.Length);
  public static implicit operator ReadOnlySpan<T>(T[] array) => new(array);
  
}

#endif
